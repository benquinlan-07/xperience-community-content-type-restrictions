using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BQ.Xperience.Extensions.ContentTypeRestrictions.Models;
using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
using CMS.DataEngine;
using CMS.Websites.Internal;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Admin.Websites.UIPages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Kentico.Xperience.Admin.Websites;


namespace BQ.Xperience.Extensions.ContentTypeRestrictions;

public class ExtensionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Regex _createPagePathRegex;

    private const string ExpectedRequestPath = "/admin/api/page";

    public ExtensionMiddleware(RequestDelegate next)
    {
        _next = next;
        _createPagePathRegex = new Regex("webpages-[0-9]+\\/([a-zA-Z-]+_[0-9]+)\\/create");
    }

    public async Task InvokeAsync(HttpContext context, 
        IInfoProvider<WebPageItemInfo> webPageItemInfoProvider, 
        IInfoProvider<ContentItemInfo> contentItemInfoProvider,
        IInfoProvider<ContentTypeConfigurationInfo> contentTypeConfigurationInfoProvider, 
        IInfoProvider<ContentTypeAllowedTypeInfo> contentTypeAllowedTypeInfoProvider)
    {
        if (ExpectedRequestPath.Equals(context.Request.Path.Value, StringComparison.InvariantCultureIgnoreCase))
        {
            var path = context.Request.Form.ContainsKey("path") ? context.Request.Form["path"].ToString() : null;
            var pathMatch = _createPagePathRegex.Match(path ?? "");
            if (pathMatch.Success)
            {
                var webPageUrlIdentifierValue = pathMatch.Groups[1].Value;
                var webPageUrlIdentifier = new WebPageUrlIdentifier(webPageUrlIdentifierValue);

                var allowedContentTypeIds = Array.Empty<int>();

                if (webPageUrlIdentifier.WebPageItemID == 0)
                {
                    var configurations = contentTypeConfigurationInfoProvider.Get()
                        .WhereEquals(nameof(ContentTypeConfigurationInfo.ContentTypeConfigurationAllowAtRoot), true)
                        .ToArray();

                    allowedContentTypeIds = configurations.Select(x => x.ContentTypeConfigurationContentTypeId).ToArray();
                }
                else
                {
                    var webPage = webPageItemInfoProvider.Get(webPageUrlIdentifier.WebPageItemID);
                    var contentItem = contentItemInfoProvider.Get(webPage.WebPageItemContentItemID);
                    var contentTypeId = contentItem.ContentItemContentTypeID;

                    var configuration = contentTypeConfigurationInfoProvider.Get()
                        .WhereEquals(nameof(ContentTypeConfigurationInfo.ContentTypeConfigurationContentTypeId), contentTypeId)
                        .TopN(1)
                        .FirstOrDefault();

                    if (configuration != null)
                    {
                        var allowedTypes = contentTypeAllowedTypeInfoProvider.Get()
                            .WhereEquals(nameof(ContentTypeAllowedTypeInfo.ContentTypeAllowedTypeContentTypeConfigurationId), configuration.ContentTypeConfigurationId)
                            .ToArray();

                        allowedContentTypeIds = allowedTypes.Select(x => x.ContentTypeAllowedTypeContentTypeId).ToArray();
                    }
                }

                var originalBody = context.Response.Body;

                // Read out the body
                using (var memStream = new MemoryStream())
                {
                    context.Response.Body = memStream;

                    await _next(context);

                    memStream.Position = 0;
                    string responseBody = new StreamReader(memStream).ReadToEnd();

                    var toParse = JObject.Parse(responseBody);

                    var items = toParse.Property(nameof(CreateWebPageClientProperties.Items).ToLower())?.Value as JArray;
                    if (items != null)
                    {
                        var contentType = items.Children().First(x => x.Value<string>("name") == "ContentType");
                        var contentTypeOptions = contentType.Value<JArray>("items");

                        var currentOptions = contentTypeOptions.ToObject<List<TileSelectorItem>>();
                        var newOptions = currentOptions.Where(x => allowedContentTypeIds.Contains(x.Identifier)).ToArray();

                        contentType["items"] = JArray.FromObject(newOptions, new JsonSerializer() { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                        responseBody = toParse.ToString();

                        using (var newStream = new MemoryStream())
                        using (var streamWriter = new StreamWriter(newStream))
                        {
                            streamWriter.Write(responseBody);
                            streamWriter.Flush();

                            newStream.Position = 0;
                            await newStream.CopyToAsync(originalBody);
                        }

                    }
                    else
                    {
                        memStream.Position = 0;
                        await memStream.CopyToAsync(originalBody);
                    }
                }


                return;
            }
        }

        // Call the next delegate/middleware in the pipeline.
        await _next(context);
    }
}
public static class ExtensionMiddlewareExtensions
{
    public static IApplicationBuilder UseContentTypeRestrictionsExtension(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExtensionMiddleware>();
    }
}