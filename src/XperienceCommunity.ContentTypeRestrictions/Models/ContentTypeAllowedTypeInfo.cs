using System;
using System.Data;
using System.Runtime.Serialization;
using XperienceCommunity.ContentTypeRestrictions.Models;
using CMS;
using CMS.DataEngine;
using CMS.Helpers;

[assembly: RegisterObjectType(typeof(ContentTypeAllowedTypeInfo), ContentTypeAllowedTypeInfo.OBJECT_TYPE)]

namespace XperienceCommunity.ContentTypeRestrictions.Models;

/// <summary>
/// Data container class for <see cref="ContentTypeAllowedTypeInfo"/>.
/// </summary>
[Serializable]
public partial class ContentTypeAllowedTypeInfo : AbstractInfo<ContentTypeAllowedTypeInfo, IInfoProvider<ContentTypeAllowedTypeInfo>>
{
    /// <summary>
    /// Object type.
    /// </summary>
    public const string OBJECT_TYPE = "bqctr.contenttypeallowedtype";
    public const string OBJECT_CLASS_NAME = "BQCTR.ContentTypeAllowedType";
    public const string OBJECT_CLASS_DISPLAYNAME = "Content Type Allowed Type";


    /// <summary>
    /// Type information.
    /// </summary>
    public static readonly ObjectTypeInfo TYPEINFO = new(typeof(IInfoProvider<ContentTypeAllowedTypeInfo>), OBJECT_TYPE, OBJECT_CLASS_NAME, nameof(ContentTypeAllowedTypeId), null, nameof(ContentTypeAllowedTypeGuid), null, null, null, null, null)
    {
        TouchCacheDependencies = true,
        ContinuousIntegrationSettings =
        {
            Enabled = true,
        },
    };


    /// <summary>
    /// Content type allowed type id.
    /// </summary>
    [DatabaseField]
    public virtual int ContentTypeAllowedTypeId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(ContentTypeAllowedTypeId)), 0);
        set => SetValue(nameof(ContentTypeAllowedTypeId), value);
    }


    /// <summary>
    /// Content type allowed type Guid.
    /// </summary>
    [DatabaseField]
    public virtual Guid ContentTypeAllowedTypeGuid
    {
        get => ValidationHelper.GetGuid(GetValue(nameof(ContentTypeAllowedTypeGuid)), default);
        set => SetValue(nameof(ContentTypeAllowedTypeGuid), value);
    }


    /// <summary>
    /// Content type configuration id.
    /// </summary>
    [DatabaseField]
    public virtual int ContentTypeAllowedTypeContentTypeConfigurationId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(ContentTypeAllowedTypeContentTypeConfigurationId)), default);
        set => SetValue(nameof(ContentTypeAllowedTypeContentTypeConfigurationId), value);
    }


    /// <summary>
    /// Content type id.
    /// </summary>
    [DatabaseField]
    public virtual int ContentTypeAllowedTypeContentTypeId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(ContentTypeAllowedTypeContentTypeId)), default);
        set => SetValue(nameof(ContentTypeAllowedTypeContentTypeId), value);
    }


    /// <summary>
    /// Deletes the object using appropriate provider.
    /// </summary>
    protected override void DeleteObject()
    {
        Provider.Delete(this);
    }


    /// <summary>
    /// Updates the object using appropriate provider.
    /// </summary>
    protected override void SetObject()
    {
        Provider.Set(this);
    }


    /// <summary>
    /// Creates an empty instance of the <see cref="ContentTypeAllowedTypeInfo"/> class.
    /// </summary>
    public ContentTypeAllowedTypeInfo()
        : base(TYPEINFO)
    {
    }


    /// <summary>
    /// Creates a new instances of the <see cref="ContentTypeAllowedTypeInfo"/> class from the given <see cref="DataRow"/>.
    /// </summary>
    /// <param name="dr">DataRow with the object data.</param>
    public ContentTypeAllowedTypeInfo(DataRow dr)
        : base(TYPEINFO, dr)
    {
    }
}
