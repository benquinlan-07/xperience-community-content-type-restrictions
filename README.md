> [!WARNING]
> 
> With the release of the Feb 2025 refresh, this functionality of this package has now been replaced with core features available > directly within Xperience by Kentico. To that end, this package will no longer be supported and will instead be intended to be > replaced with the core functionality.
> 
> See https://community.kentico.com/blog/xperience-by-kentico-refresh-february-20-2025#allowed-web-page-content-types-and-scopes for > more information on the feature release.
> 
> To assist you with migrating your data from this plugin, I have included a script below. Note that this script is provided for general use and may need to be modified based on how your specific instance of XbyK is setup.
> 
> ```sql
> insert into CMS_WebPageScope (WebPageScopeWebsiteChannelID, WebPageScopeWebPageItemID, WebPageScopeIncludeChildren, WebPageScopeGUID)
> select		WebsiteChannelID, null, 0, newid()
> from		CMS_WebsiteChannel wc
> left join	CMS_WebPageScope wps on wc.WebsiteChannelID = wps.WebPageScopeWebsiteChannelID
> where		wps.WebPageScopeWebsiteChannelID is null
> 
> insert into CMS_WebPageScopeContentType (WebPageScopeContentTypeWebPageScopeID, WebPageScopeContentTypeContentTypeID)
> select		WebPageScopeID, ContentTypeConfigurationContentTypeId
> from		CMS_WebPageScope
> cross join	BQCTR_ContentTypeConfiguration 
> where		ContentTypeConfigurationAllowAtRoot = 1
> 
> insert into CMS_AllowedChildContentType (AllowedChildContentTypeParentID, AllowedChildContentTypeChildID)
> select		distinct ctc.ContentTypeConfigurationContentTypeId, ctat.ContentTypeAllowedTypeContentTypeId
> from		BQCTR_ContentTypeAllowedType ctat
> inner join	BQCTR_ContentTypeConfiguration ctc on ctat.ContentTypeAllowedTypeContentTypeConfigurationId = ctc.ContentTypeConfigurationId
> ```

# Xperience Community: Content Type Restrictions

## Description

This package provides Xperience by Kentico administrators with an interface to restrict allowed content types within the content trees of a website channel. This package is intended to reduce allow control over what content types are available in order to guide editors towards the correct options when structuring site content.

![Xperience by Content Type Restrictions](https://raw.githubusercontent.com/benquinlan-07/xperience-community-content-type-restrictions/refs/heads/main/images/content-type-restrictions-edit.jpeg)

## Requirements

### Library Version Matrix

| Xperience Version | Library Version |
| ----------------- | --------------- |
| >= 29.5.3         | 1.0.0           |

### Dependencies

- [ASP.NET Core 6.0](https://dotnet.microsoft.com/en-us/download)
- [Xperience by Kentico](https://docs.kentico.com)

## Package Installation

Add the package to your application using the .NET CLI

```
dotnet add package XperienceCommunity.ContentTypeRestrictions
```

or via the Package Manager

```
Install-Package XperienceCommunity.ContentTypeRestrictions
```

## Quick Start

1. Install the NuGet package.

1. Update your Program.cs to register the necessary services.

```csharp
    using XperienceCommunity.ContentTypeRestrictions;

    ...

    builder.Services.AddContentTypeRestrictionsExtensionServices();
```

## Full Instructions

1. Start your Xperience by Kentico website.

1. Log in to the administration site.

1. Edit a content type.

1. Select whether the content type is allowed at the root level and what child types are allowed under this content type.
![Xperience by Content Type Restrictions](https://raw.githubusercontent.com/benquinlan-07/xperience-community-content-type-restrictions/refs/heads/main/images/content-type-restrictions-edit.jpeg)

1. When creating a new page in the website channel, the available content types will be restricted to only the specified allowed types.
![Xperience by Content Type Restrictions](https://raw.githubusercontent.com/benquinlan-07/xperience-community-content-type-restrictions/refs/heads/main/images/content-type-restrictions-add-page.jpeg)
