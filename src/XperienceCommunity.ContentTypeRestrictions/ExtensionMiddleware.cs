using System;
using Microsoft.AspNetCore.Builder;

namespace XperienceCommunity.ContentTypeRestrictions;

public static class ExtensionMiddlewareExtensions
{
    [Obsolete("The middleware for this component is no longer required.")]
    public static IApplicationBuilder UseContentTypeRestrictionsExtension(this IApplicationBuilder builder)
    {
        return builder;
    }
}