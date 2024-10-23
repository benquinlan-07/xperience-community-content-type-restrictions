using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BQ.Xperience.Extensions.ContentTypeRestrictions.Models;
using BQ.Xperience.Extensions.ContentTypeRestrictions.UI;
using CMS.ContentEngine;
using CMS.DataEngine;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Admin.Base.UIPages;
using IFormItemCollectionProvider = Kentico.Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider;

[assembly: UIPage(parentType: typeof(ContentTypeEditSection), slug: "allowed-types", uiPageType: typeof(ContentTypeConfigurationEdit), name: "Allowed types", templateName: TemplateNames.EDIT, order: 10000)]

namespace BQ.Xperience.Extensions.ContentTypeRestrictions.UI
{
    public class ContentTypeConfigurationEdit : ModelEditPage<ContentTypeConfigurationEdit.ContentTypeConfigurationModel>
    {
        private readonly IInfoProvider<ContentTypeConfigurationInfo> _contentTypeConfigurationInfoProvider;
        private readonly IInfoProvider<ContentTypeAllowedTypeInfo> _contentTypeAllowedTypeInfoProvider;
        private ContentTypeConfigurationModel _model;

        public ContentTypeConfigurationEdit(IFormItemCollectionProvider formItemCollectionProvider, 
            IFormDataBinder formDataBinder,
            IInfoProvider<ContentTypeConfigurationInfo> contentTypeConfigurationInfoProvider,
            IInfoProvider<ContentTypeAllowedTypeInfo> contentTypeAllowedTypeInfoProvider) 
            : base(formItemCollectionProvider, formDataBinder)
        {
            _contentTypeConfigurationInfoProvider = contentTypeConfigurationInfoProvider;
            _contentTypeAllowedTypeInfoProvider = contentTypeAllowedTypeInfoProvider;
        }

        [PageParameter(typeof(IntPageModelBinder), typeof(ContentTypeEditSection))]
        public int ContentTypeId { get; set; }

        /// <inheritdoc />
        public override async Task ConfigurePage()
        {
            var configuration = GetConfiguration(ContentTypeId);

            if (configuration != null)
            {
                var contentTypes = GetAllowedTypes(configuration.ContentTypeConfigurationId);

                Model.AllowAtRoot = configuration.ContentTypeConfigurationAllowAtRoot;
                Model.AllowedChildTypes = contentTypes.Select(x => x.ContentTypeAllowedTypeContentTypeId);
            }

            await base.ConfigurePage();
        }

        private ContentTypeConfigurationInfo GetConfiguration(int contentTypeId)
        {
            return _contentTypeConfigurationInfoProvider.Get()
                .WhereEquals(nameof(ContentTypeConfigurationInfo.ContentTypeConfigurationContentTypeId), contentTypeId)
                .TopN(1)
                .FirstOrDefault();
        }

        private ContentTypeAllowedTypeInfo[] GetAllowedTypes(int contentTypeConfigurationId)
        {
            return _contentTypeAllowedTypeInfoProvider.Get()
                .WhereEquals(nameof(ContentTypeAllowedTypeInfo.ContentTypeAllowedTypeContentTypeConfigurationId), contentTypeConfigurationId)
                .ToArray();
        }

        public override Task<EditTemplateClientProperties> ConfigureTemplateProperties(EditTemplateClientProperties properties)
        {
            properties.Headline = "Allowed types";
            return base.ConfigureTemplateProperties(properties);
        }

        /// <inheritdoc />
        protected override async Task<ICommandResponse> ProcessFormData(ContentTypeConfigurationModel model, ICollection<IFormItem> formItems)
        {
            var configuration = _contentTypeConfigurationInfoProvider.Get()
                .WhereEquals(nameof(ContentTypeConfigurationInfo.ContentTypeConfigurationContentTypeId), ContentTypeId)
                .TopN(1)
                .FirstOrDefault();

            if (configuration == null)
            {
                configuration = new ContentTypeConfigurationInfo()
                {
                    ContentTypeConfigurationGuid = Guid.NewGuid(),
                    ContentTypeConfigurationContentTypeId = ContentTypeId
                };
            }

            configuration.ContentTypeConfigurationAllowAtRoot = model.AllowAtRoot;

            _contentTypeConfigurationInfoProvider.Set(configuration);

            model.AllowedChildTypes ??= Array.Empty<int>();

            var contentTypes = GetAllowedTypes(configuration.ContentTypeConfigurationId);
            var toRemove= contentTypes.Where(x => !model.AllowedChildTypes.Contains(x.ContentTypeAllowedTypeContentTypeId));
            foreach (var contentTypeToRemove in toRemove)
                contentTypeToRemove.Delete();

            var toAdd = model.AllowedChildTypes.Where(x => !contentTypes.Any(y => y.ContentTypeAllowedTypeContentTypeId == x));
            foreach (var contentTypeToAdd in toAdd)
            {
                var allowedContentType = new ContentTypeAllowedTypeInfo()
                {
                    ContentTypeAllowedTypeGuid = Guid.NewGuid(),
                    ContentTypeAllowedTypeContentTypeConfigurationId = configuration.ContentTypeConfigurationId,
                    ContentTypeAllowedTypeContentTypeId = contentTypeToAdd
                };
                _contentTypeAllowedTypeInfoProvider.Set(allowedContentType);
            }

            // Initializes a client response
            var response = ResponseFrom(new FormSubmissionResult(FormSubmissionStatus.ValidationSuccess)
            {
                // Returns the submitted field values to the client (repopulates the form)
                Items = await formItems.OnlyVisible().GetClientProperties(),
            });

            response.AddSuccessMessage("Content type configuration has been updated.");

            return response;
        }

        protected override ContentTypeConfigurationModel Model
        {
            get { return _model ??= new ContentTypeConfigurationModel(); }
        }

        public class ContentTypeConfigurationModel
        {
            [CheckBoxComponent(Label = "Allow at root", ExplanationText = "Allow page to be created at the root level")]
            public bool AllowAtRoot { get; set; }

            [ObjectIdSelectorComponent(ContentTypeInfo.OBJECT_TYPE, Label = "Allowed child types", MaximumItems = 0, WhereConditionProviderType = typeof(AllowedChildTypesWhere))]
            public IEnumerable<int> AllowedChildTypes { get; set; }

            public class AllowedChildTypesWhere : IObjectSelectorWhereConditionProvider
            {
                // Where condition limiting the objects
                public WhereCondition Get() => new WhereCondition().WhereStartsWith(nameof(ContentTypeInfo.ClassType), "Content");

            }
        }
    }


}
