using XperienceCommunity.ContentTypeRestrictions.Models;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Modules;

namespace XperienceCommunity.ContentTypeRestrictions;

internal class ExtensionModuleInstaller
{
    private readonly IInfoProvider<ResourceInfo> resourceProvider;

    public ExtensionModuleInstaller(IInfoProvider<ResourceInfo> resourceProvider)
    {
        this.resourceProvider = resourceProvider;
    }

    public void Install()
    {
        var resource = resourceProvider.Get(Constants.ResourceName)
                       ?? new ResourceInfo();

        InitializeResource(resource);
        InstallContentTypeConfigurationInfo(resource);
        InstallContentTypeAllowedTypeInfo(resource);
    }

    public ResourceInfo InitializeResource(ResourceInfo resource)
    {
        resource.ResourceDisplayName = Constants.ResourceDisplayName;
        resource.ResourceName = Constants.ResourceName;
        resource.ResourceDescription = Constants.ResourceDescription;
        resource.ResourceIsInDevelopment = false;
        
        if (resource.HasChanged)
            resourceProvider.Set(resource);

        return resource;
    }

    public void InstallContentTypeConfigurationInfo(ResourceInfo resource)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(ContentTypeConfigurationInfo.OBJECT_TYPE) ?? DataClassInfo.New(ContentTypeConfigurationInfo.OBJECT_TYPE);

        info.ClassName = ContentTypeConfigurationInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = ContentTypeConfigurationInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = ContentTypeConfigurationInfo.OBJECT_CLASS_DISPLAYNAME;
        info.ClassType = ClassType.OTHER;
        info.ClassResourceID = resource.ResourceID;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(ContentTypeConfigurationInfo.ContentTypeConfigurationId));

        var formItem = new FormFieldInfo
        {
            Name = nameof(ContentTypeConfigurationInfo.ContentTypeConfigurationGuid),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "guid",
            Enabled = true,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(ContentTypeConfigurationInfo.ContentTypeConfigurationContentTypeId),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "integer",
            ReferenceToObjectType = ContentTypeInfo.OBJECT_TYPE,
            ReferenceType = ObjectDependencyEnum.Required
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(ContentTypeConfigurationInfo.ContentTypeConfigurationAllowAtRoot),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "boolean",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            DataClassInfoProvider.SetDataClassInfo(info);
        }
    }

    public void InstallContentTypeAllowedTypeInfo(ResourceInfo resource)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(ContentTypeAllowedTypeInfo.OBJECT_TYPE) ?? DataClassInfo.New(ContentTypeAllowedTypeInfo.OBJECT_TYPE);

        info.ClassName = ContentTypeAllowedTypeInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = ContentTypeAllowedTypeInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = ContentTypeAllowedTypeInfo.OBJECT_CLASS_DISPLAYNAME;
        info.ClassType = ClassType.OTHER;
        info.ClassResourceID = resource.ResourceID;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(ContentTypeAllowedTypeInfo.ContentTypeAllowedTypeId));

        var formItem = new FormFieldInfo
        {
            Name = nameof(ContentTypeAllowedTypeInfo.ContentTypeAllowedTypeGuid),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "guid",
            Enabled = true,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(ContentTypeAllowedTypeInfo.ContentTypeAllowedTypeContentTypeConfigurationId),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "integer",
            ReferenceToObjectType = ContentTypeConfigurationInfo.OBJECT_TYPE,
            ReferenceType = ObjectDependencyEnum.Required
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(ContentTypeAllowedTypeInfo.ContentTypeAllowedTypeContentTypeId),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "integer",
            ReferenceToObjectType = ContentTypeInfo.OBJECT_TYPE,
            ReferenceType = ObjectDependencyEnum.Required
        };
        formInfo.AddFormItem(formItem);

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            DataClassInfoProvider.SetDataClassInfo(info);
        }
    }

    /// <summary>
    /// Ensure that the form is upserted with any existing form
    /// </summary>
    /// <param name="info"></param>
    /// <param name="form"></param>
    private static void SetFormDefinition(DataClassInfo info, FormInfo form)
    {
        if (info.ClassID > 0)
        {
            var existingForm = new FormInfo(info.ClassFormDefinition);
            existingForm.CombineWithForm(form, new());
            info.ClassFormDefinition = existingForm.GetXmlDefinition();
        }
        else
        {
            info.ClassFormDefinition = form.GetXmlDefinition();
        }
    }
}