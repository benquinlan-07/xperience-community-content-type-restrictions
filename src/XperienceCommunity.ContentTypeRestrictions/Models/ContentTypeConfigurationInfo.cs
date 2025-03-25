using System;
using System.Data;
using System.Runtime.Serialization;
using XperienceCommunity.ContentTypeRestrictions.Models;
using CMS;
using CMS.DataEngine;
using CMS.Helpers;

[assembly: RegisterObjectType(typeof(ContentTypeConfigurationInfo), ContentTypeConfigurationInfo.OBJECT_TYPE)]

namespace XperienceCommunity.ContentTypeRestrictions.Models;

/// <summary>
/// Data container class for <see cref="ContentTypeConfigurationInfo"/>.
/// </summary>
[Serializable]
public partial class ContentTypeConfigurationInfo : AbstractInfo<ContentTypeConfigurationInfo, IInfoProvider<ContentTypeConfigurationInfo>>
{
    /// <summary>
    /// Object type.
    /// </summary>
    public const string OBJECT_TYPE = "bqctr.contenttypeconfiguration";
    public const string OBJECT_CLASS_NAME = "BQCTR.ContentTypeConfiguration";
    public const string OBJECT_CLASS_DISPLAYNAME = "Content Type Configuration";


    /// <summary>
    /// Type information.
    /// </summary>
    public static readonly ObjectTypeInfo TYPEINFO = new(typeof(IInfoProvider<ContentTypeConfigurationInfo>), OBJECT_TYPE, OBJECT_CLASS_NAME, nameof(ContentTypeConfigurationId), null, nameof(ContentTypeConfigurationGuid), null, null, null, null, null)
    {
        TouchCacheDependencies = true,
        ContinuousIntegrationSettings =
        {
            Enabled = true,
        },
    };


    /// <summary>
    /// Content type configuration id.
    /// </summary>
    [DatabaseField]
    public virtual int ContentTypeConfigurationId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(ContentTypeConfigurationId)), 0);
        set => SetValue(nameof(ContentTypeConfigurationId), value);
    }


    /// <summary>
    /// Content type configuration Guid.
    /// </summary>
    [DatabaseField]
    public virtual Guid ContentTypeConfigurationGuid
    {
        get => ValidationHelper.GetGuid(GetValue(nameof(ContentTypeConfigurationGuid)), default);
        set => SetValue(nameof(ContentTypeConfigurationGuid), value);
    }


    /// <summary>
    /// Content type id.
    /// </summary>
    [DatabaseField]
    public virtual int ContentTypeConfigurationContentTypeId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(ContentTypeConfigurationContentTypeId)), default);
        set => SetValue(nameof(ContentTypeConfigurationContentTypeId), value);
    }


    /// <summary>
    /// Allow at root.
    /// </summary>
    [DatabaseField]
    public virtual bool ContentTypeConfigurationAllowAtRoot
    {
        get => ValidationHelper.GetBoolean(GetValue(nameof(ContentTypeConfigurationAllowAtRoot)), default);
        set => SetValue(nameof(ContentTypeConfigurationAllowAtRoot), value);
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
    /// Constructor for de-serialization.
    /// </summary>
    /// <param name="info">Serialization info.</param>
    /// <param name="context">Streaming context.</param>
    protected ContentTypeConfigurationInfo(SerializationInfo info, StreamingContext context)
    {
    }


    /// <summary>
    /// Creates an empty instance of the <see cref="ContentTypeConfigurationInfo"/> class.
    /// </summary>
    public ContentTypeConfigurationInfo()
        : base(TYPEINFO)
    {
    }


    /// <summary>
    /// Creates a new instances of the <see cref="ContentTypeConfigurationInfo"/> class from the given <see cref="DataRow"/>.
    /// </summary>
    /// <param name="dr">DataRow with the object data.</param>
    public ContentTypeConfigurationInfo(DataRow dr)
        : base(TYPEINFO, dr)
    {
    }
}
