using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BQ.Xperience.Extensions.ContentTypeRestrictions.Models;
using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
using CMS.Core;
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
        IInfoProvider<ContentTypeAllowedTypeInfo> contentTypeAllowedTypeInfoProvider,
        IEventLogService eventLogService)
    {
        // For request not on expected path, skip
        if (ExpectedRequestPath.Equals(context.Request.Path.Value, StringComparison.InvariantCultureIgnoreCase))
        {
            // Pull out the path parameter and check for match with expected pattern
            var path = context.Request.Form.ContainsKey("path") ? context.Request.Form["path"].ToString() : null;
            var pathMatch = _createPagePathRegex.Match(path ?? "");
            if (pathMatch.Success)
            {
                // Parse out the web page url identifier
                var webPageUrlIdentifierValue = pathMatch.Groups[1].Value;
                var webPageUrlIdentifier = new WebPageUrlIdentifier(webPageUrlIdentifierValue);

                var allowedContentTypeIds = Array.Empty<int>();

                // If attempting to create from the root node, the WebPageItemID will be 0. In this case, we need to get all content types that are allowed at the root.
                if (webPageUrlIdentifier.WebPageItemID == 0)
                {
                    var configurations = contentTypeConfigurationInfoProvider.Get()
                        .WhereEquals(nameof(ContentTypeConfigurationInfo.ContentTypeConfigurationAllowAtRoot), true)
                        .ToArray();

                    allowedContentTypeIds = configurations.Select(x => x.ContentTypeConfigurationContentTypeId).ToArray();
                }
                else
                {
                    // Determine the content type of the web page item
                    var webPage = webPageItemInfoProvider.Get(webPageUrlIdentifier.WebPageItemID);
                    var contentItem = contentItemInfoProvider.Get(webPage.WebPageItemContentItemID);
                    var contentTypeId = contentItem.ContentItemContentTypeID;

                    // Try and see if there is configuration defined
                    var configuration = contentTypeConfigurationInfoProvider.Get()
                        .WhereEquals(nameof(ContentTypeConfigurationInfo.ContentTypeConfigurationContentTypeId), contentTypeId)
                        .TopN(1)
                        .FirstOrDefault();

                    if (configuration != null)
                    {
                        // Pull the list of allowed types from the configuration
                        var allowedTypes = contentTypeAllowedTypeInfoProvider.Get()
                            .WhereEquals(nameof(ContentTypeAllowedTypeInfo.ContentTypeAllowedTypeContentTypeConfigurationId), configuration.ContentTypeConfigurationId)
                            .ToArray();

                        allowedContentTypeIds = allowedTypes.Select(x => x.ContentTypeAllowedTypeContentTypeId).ToArray();
                    }
                }

                // We need to intercept the response and modify the content type options directly from the response due to sealed and internal implementations within XbK
                var originalBody = context.Response.Body;

                // Create a temporary memory stream to store the response that we can access later
                using var memStream = new MemoryStream();
                context.Response.Body = memStream;

                // Run the request
                await _next(context);

                // Parse out the json response from the memory stream
                memStream.Position = 0;
                var responseBody = await new StreamReader(memStream).ReadToEndAsync();

                try
                {
                    // Parse to JObject
                    var toParse = JObject.Parse(responseBody);

                    // Find items property
                    var items = toParse.Property(nameof(CreateWebPageClientProperties.Items).ToLower())?.Value as JArray;
                    if (items != null)
                    {
                        var contentType = items.Children().First(x => x.Value<string>("name") == "ContentType");
                        var contentTypeOptions = contentType.Value<JArray>("items");

                        // Read out current content type options
                        var currentOptions = contentTypeOptions.ToObject<List<TileSelectorItem>>();

                        // Apply allowed content types filter to provided options
                        var newOptions = currentOptions.Where(x => allowedContentTypeIds.Contains(x.Identifier)).ToArray();

                        // Reset property in json
                        contentType["items"] = JArray.FromObject(newOptions, new JsonSerializer() { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                        // Set response body to new json object
                        responseBody = toParse.ToString();

                        // Write the modified response back to the original stream
                        using var newStream = new MemoryStream();
                        using var streamWriter = new StreamWriter(newStream);
                        await streamWriter.WriteAsync(responseBody);
                        await streamWriter.FlushAsync();

                        newStream.Position = 0;
                        await newStream.CopyToAsync(originalBody);
                        
                        return;
                    }
                }
                catch (Exception ex)
                {
                    eventLogService.LogException("ContentTypeRestrictions", "MIDDLEWARE", ex);
                }

                memStream.Position = 0;
                await memStream.CopyToAsync(originalBody);

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