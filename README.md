# Xperience Community: Content Type Restrictions

## Description

This package provides Xperience by Kentico administrators with an interface to restrict allowed content types within the content trees of a website channel. This package is intended to reduce allow control over what content types are available in order to guide editors towards the correct options when structuring site content.

![Xperience by Content Type Restrictions](https://github.com/benquinlan-07/Xperience.Extensions.ContentTypeRestrictions/blob/main/images/content-type-restrictions-add-page.jpeg?raw=true)

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
![Xperience by Content Type Restrictions](https://github.com/benquinlan-07/Xperience.Extensions.ContentTypeRestrictions/blob/main/images/content-type-restrictions-edit.jpeg?raw=true)

1. When creating a new page in the website channel, the available content types will be restricted to only the specified allowed types.
![Xperience by Content Type Restrictions](https://github.com/benquinlan-07/Xperience.Extensions.ContentTypeRestrictions/blob/main/images/content-type-restrictions-add-page.jpeg?raw=true)
