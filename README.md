# Xperience.Extensions.ContentTypeRestrictions
## Description
This package provides XbK administrators with an interface to restrict allowed content types within the content trees of a website channel.

When editing a content type, a new menu item will be shown for "Allowed types".

## Installation
This package can be installed from nuget using the command:

    Install-Package BQ.Xperience.Extensions.ContentTypeRestrictions

or using the .NET CLI with the command:

    dotnet add package BQ.Xperience.Extensions.ContentTypeRestrictions


## Setup

Add the following line to your Program.cs to register the necessary services.

    using using BQ.Xperience.Extensions.ContentTypeRestrictions;

    ...

    builder.Services.AddContentTypeRestrictionsExtensionServices();

    ...

    app.UseContentTypeRestrictionsExtension();


Ensure the call to register the middleware is added before `app.UseKentico()` to ensure that the middleware is able to intercept the Kentico response.