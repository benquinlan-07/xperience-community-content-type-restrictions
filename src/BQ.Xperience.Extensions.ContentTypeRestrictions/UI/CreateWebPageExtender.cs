using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BQ.Xperience.Extensions.ContentTypeRestrictions.Models;
using BQ.Xperience.Extensions.ContentTypeRestrictions.UI;
using CMS.ContentEngine.Internal;
using CMS.DataEngine;
using CMS.Websites.Internal;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Admin.Websites;
using Kentico.Xperience.Admin.Websites.UIPages;

[assembly: PageExtender(typeof(CreateWebPageExtender))]

namespace BQ.Xperience.Extensions.ContentTypeRestrictions.UI;

internal class CreateWebPageExtender : PageExtender<CreateWebPage>
{
    private readonly IInfoProvider<WebPageItemInfo> _webPageItemInfoProvider;
    private readonly IInfoProvider<ContentItemInfo> _contentItemInfoProvider;
    private readonly IInfoProvider<ContentTypeConfigurationInfo> _contentTypeConfigurationInfoProvider;
    private readonly IInfoProvider<ContentTypeAllowedTypeInfo> _contentTypeAllowedTypeInfoProvider;

    private const string ContentTypePropertyName = "ContentType";

    public CreateWebPageExtender(IInfoProvider<WebPageItemInfo> webPageItemInfoProvider,
        IInfoProvider<ContentItemInfo> contentItemInfoProvider,
        IInfoProvider<ContentTypeConfigurationInfo> contentTypeConfigurationInfoProvider,
        IInfoProvider<ContentTypeAllowedTypeInfo> contentTypeAllowedTypeInfoProvider)
    {
        _webPageItemInfoProvider = webPageItemInfoProvider;
        _contentItemInfoProvider = contentItemInfoProvider;
        _contentTypeConfigurationInfoProvider = contentTypeConfigurationInfoProvider;
        _contentTypeAllowedTypeInfoProvider = contentTypeAllowedTypeInfoProvider;
    }

    /// <inheritdoc />
    public override async Task<TemplateClientProperties> ConfigureTemplateProperties(TemplateClientProperties properties)
    {
        await base.ConfigureTemplateProperties(properties);

        if (properties is CreateWebPageClientProperties webPageClientProperties)
        {
            var contentTypePropertyEditor = webPageClientProperties.Items
                .OfType<TileSelectorClientProperties>()
                .FirstOrDefault(x => x.Name == ContentTypePropertyName);

            if (contentTypePropertyEditor != null)
            {
                var allowedContentTypeIds = GetAllowedContentTypeIds(Page.ParentWebPageIdentifier);

                contentTypePropertyEditor.Items = contentTypePropertyEditor.Items
                    ?.Where(x => allowedContentTypeIds.Contains(x.Identifier))
                    .ToArray();
            }
        }

        return properties;
    }

    private ICollection<int> GetAllowedContentTypeIds(WebPageUrlIdentifier parentWebPageIdentifier)
    {
        // If attempting to create from the root node, the WebPageItemID will be 0. In this case, we need to get all content types that are allowed at the root.
        if (parentWebPageIdentifier.WebPageItemID == 0)
        {
            var configurations = _contentTypeConfigurationInfoProvider.Get()
                .WhereEquals(nameof(ContentTypeConfigurationInfo.ContentTypeConfigurationAllowAtRoot), true)
                .ToArray();

            return configurations.Select(x => x.ContentTypeConfigurationContentTypeId).ToArray();
        }

        // Determine the content type of the web page item
        var webPage = _webPageItemInfoProvider.Get(parentWebPageIdentifier.WebPageItemID);
        var contentItem = _contentItemInfoProvider.Get(webPage.WebPageItemContentItemID);
        var contentTypeId = contentItem.ContentItemContentTypeID;

        // Try and see if there is configuration defined
        var configuration = _contentTypeConfigurationInfoProvider.Get()
            .WhereEquals(nameof(ContentTypeConfigurationInfo.ContentTypeConfigurationContentTypeId), contentTypeId)
            .TopN(1)
            .FirstOrDefault();

        if (configuration != null)
        {
            // Pull the list of allowed types from the configuration
            var allowedTypes = _contentTypeAllowedTypeInfoProvider.Get()
                .WhereEquals(nameof(ContentTypeAllowedTypeInfo.ContentTypeAllowedTypeContentTypeConfigurationId), configuration.ContentTypeConfigurationId)
                .ToArray();

            return allowedTypes.Select(x => x.ContentTypeAllowedTypeContentTypeId).ToArray();
        }

        return Array.Empty<int>();
    }
}