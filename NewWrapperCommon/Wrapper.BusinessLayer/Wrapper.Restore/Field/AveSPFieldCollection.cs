/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */



using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Mapping;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.GCommon.Utility.Exceptions.SharePoint;
using AvePoint.Wrapper.Resource.Restore;

namespace AvePoint.Wrapper.Restore
{
    [AveCodeReview("2012/10/08", "xihe.you@avepoint.com", "fengfu.zhang@avepoint.com", null, "ADO-40834", true)]
    public abstract class AveSPFieldCollection : IReportable, IAveSPFieldCollection, IDisposable
    {
        protected static AveLogger log = AveLogger.GetInstance(typeof(AveSPFieldCollection));

        protected AveSPSite mAveParentSite;
        protected AveSPWeb mAveSPWeb;
        protected AveSPList mAveSPList;

        protected List<string> mSkippedRestoreFields = new List<string>();
        private List<string> workflowStatusFields = new List<string>();
        private readonly ThreadSafeDictionary<Guid, Guid> restoredFieldIdMapping = new ThreadSafeDictionary<Guid, Guid>();
        protected AveFieldRestoreOption restoreOption;

        protected readonly IReport reportor = new AveWrapperReport();
        internal List<string> needUpdateUniqueValueFields = new List<string>();
        protected Dictionary<string, Exception> restoreFailedFields = new Dictionary<string, Exception>();
        private List<AveCustomFieldInfo> needPostCreateCustomFields = new List<AveCustomFieldInfo>();

        public List<string> FieldTypeFilter { get; set; }
        public bool NeedReloadfieldsIfCreateMetadataField;
        //支持多次调用RestoreFields方法，该变量表示本次调用需要还原的field的备份信息
        private Dictionary<string, AveXmlField> currentXmlFields;
        private Dictionary<string, AveXmlField> mXmlFields = new Dictionary<string, AveXmlField>();

        public List<string> WorkflowStatusFields
        {
            get
            {
                return workflowStatusFields;
            }
        }

        //表示所有需要还原的colunn的备份信息
        public Dictionary<string, AveXmlField> XmlFields
        {
            get
            {
                return mXmlFields;
            }
            protected set
            {
                mXmlFields = value;
            }
        }

        public Dictionary<string, string> SourceTextTaxonomyDic { get; private set; }

        //当skip when conflict时，会对每个item进行比较，而经测试，如果group不存在，多次获取的效率比较低，因此添加该Dic 用来catch group mapping，避免多次查找
        private Dictionary<int, int> groupIdMapping = new Dictionary<int, int>();

        protected abstract AveReportObjectType ObjectType { get; }
        protected abstract FieldType FieldType { get; }

        protected abstract string ObjectTitle { get; }

        protected abstract IAveFieldCollection FieldCollection { get; }
        protected abstract IAveFieldCollection AllFieldCollection { get; }
        protected IAveFieldMapping mFieldMapping;
        public abstract IAveFieldMapping FieldMapping { get; }
        private List<string> needDeletedFieldsForCalculatedField = new List<string>();
        private bool isFieldRestored = false;

        public static AveSPFieldCollection CreateInstance(object obj)
        {
            AveSPFieldCollection instance;
            if (obj is AveSPWeb)
            {
                instance = new AveSPWebFieldCollection((AveSPWeb)obj);
            }
            else if (obj is AveSPList)
            {
                instance = new AveSPListFieldCollection((AveSPList)obj);
            }
            else
            {
                throw new Exception("Cannot construct an instance for this object type: " + obj.GetType());
            }
            return instance;
        }

        #region Load Fields

        public void LoadFields(string fieldsXml)
        {
            log.Debug("The {0}'s fields xml is:{1}", mAveSPList != null ? "list:" + mAveSPList.Name : "web:" + mAveSPWeb.SPWeb.Title, fieldsXml);
            var doc = new XmlDocument();
            doc.LoadXml(fieldsXml);
            currentXmlFields = LoadXmlFields(doc.DocumentElement);
            foreach (var pair in currentXmlFields)
            {
                mXmlFields[pair.Key] = pair.Value;
            }
            if (mAveSPList != null && mAveSPList.AveList.BaseType == AveBaseType.Survey)
            {
                HandleFieldsSchemaXmlForSurveyList();
            }
            UpdateFieldXmls(mXmlFields);
        }

        protected virtual Dictionary<string, AveXmlField> LoadXmlFields(XmlElement fieldsXml)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.LoadXmlFields"))
            {
                var refFields = fieldsXml
                    .Cast<XmlElement>()
                    .Where(fieldXml => fieldXml.Name.Equals("FieldRef", StringComparison.OrdinalIgnoreCase))
                    .Select(GetFieldByFieldRefXml)
                    .Where(field => field != null)
                    .GroupBy(xmlField => xmlField.ID)
                    .Select(group => group.First())
                    .ToDictionary(field => field.ID);

                var xmlFields = fieldsXml
                    .Cast<XmlElement>()
                    .Where(NeedRestore)
                    .Select(fieldXml => new AveXmlField(fieldXml, (int)mAveSPWeb.SPWeb.Language))
                    .GroupBy(xmlField => xmlField.ID)
                    .Select(group => group.First())
                    .ToDictionary(xmlField => xmlField.ID);

                AddTaxonomyTextFieldMapping(refFields, xmlFields, SourceTextTaxonomyDic);
                return xmlFields.Values.ToList()
                    .GroupBy(xmlField => xmlField.XmlElement.GetAttribute("Name"))
                    .Select(group => group.First())
                    .ToDictionary(xmlField => xmlField.XmlElement.GetAttribute("Name"), xmlField => xmlField);
            }
        }

        #region Update fields info before restore

        private void UpdateFieldXmls(Dictionary<string, AveXmlField> xmlFields)
        {
            foreach (var field in xmlFields.Values)
            {
                UpdateFieldXml(field);
            }
            ConfigureCustomFields(xmlFields);
        }

        protected virtual void UpdateFieldXml(AveXmlField xmlField)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.UpdateFieldXml"))
            {
                if (!xmlField.FieldInternalName.Equals("User_x0020_Name", StringComparison.OrdinalIgnoreCase))
                {//DOC-59253 publishing portal在Long Running Operation Status下还原这个column如果使用languageMapping会导致pressrelease站点无法打开，暂时不做mapping处理
                    if (xmlField.XmlElement.HasAttribute("DisplayName"))
                    {
                        string displayName = xmlField.XmlElement.GetAttribute("DisplayName");
                        string mappingDisplayName = mAveSPWeb.ParentSite.GetNameByLanguageMapping(displayName, AveLanguageMappingType.FieldMapping);
                        if (!displayName.Equals(mappingDisplayName))
                        {
                            xmlField.XmlElement.SetAttribute("DisplayName", mappingDisplayName);
                            xmlField.Title = mappingDisplayName;
                            xmlField.NeedFindByDisplayName = true;
                        }
                    }
                }
                if (xmlField.XmlElement.HasAttribute("Group"))
                {
                    string group = xmlField.XmlElement.GetAttribute("Group");
                    string mappingGroupName = mAveSPWeb.ParentSite.GetNameByLanguageMapping(group, AveLanguageMappingType.FieldMapping);
                    if (!group.Equals(mappingGroupName))
                    {
                        xmlField.XmlElement.SetAttribute("Group", mappingGroupName);
                        xmlField.Group = mappingGroupName;
                    }
                }
                XmlNode node = xmlField.XmlElement.SelectSingleNode("Default");
                if (node != null)
                {
                    xmlField.DefaultValue = GetResourceString(node.InnerText);
                }
                if (!String.IsNullOrEmpty(xmlField.Description))
                {
                    xmlField.Description = GetResourceString(xmlField.Description);
                    xmlField.Description = mAveSPWeb.ParentSite.GetNameByLanguageMapping(xmlField.Description, AveLanguageMappingType.FieldMapping);
                }
            }
        }

        #region Check Field Type

        protected bool IsTaxonomyField(string typeAsString)
        {
            if (string.Equals("TaxonomyFieldType", typeAsString, StringComparison.OrdinalIgnoreCase)
                                     || string.Equals("TaxonomyFieldTypeMulti", typeAsString, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }

        #endregion


        #region Update custom field

        private void ConfigureCustomFields(Dictionary<string, AveXmlField> xmlFields)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.ConfigureCustomFields"))
            {
                foreach (AveXmlField xmlField in xmlFields.Values)
                {
                    //ADO-97596 For SPMigration CollapseFolder.
                    if (xmlField.XmlElement.HasAttribute("IsCollapseFolder"))
                    {
                        xmlField.XmlElement.RemoveAttribute("IsCollapseFolder");
                        continue;
                    }
                    AveCustomFieldInfo mappingInfo =
                        FieldMapping.GetMappingFieldBeforeAdd(new AveSourceFieldInfo
                        {
                            SourceInternalName = xmlField.FieldInternalName,
                            SourceDisplayName = xmlField.Title,
                            SourceFieldId = xmlField.ID,
                            SourceType = xmlField.Type,
                            SourceTypeAsString = xmlField.TypeAsString,
                            IsHidenOrReadOnly = xmlField.Hidden || xmlField.ReadOnlyField
                        });
                    if (mappingInfo != null)
                    {
                        xmlField.CustomFieldInfo = mappingInfo;
                        if ((!mappingInfo.UseInternalOrDisplay && string.IsNullOrEmpty(mappingInfo.Name)) || (mappingInfo.UseInternalOrDisplay && string.IsNullOrEmpty(mappingInfo.InternalName)))
                        {//ADO-54175, Name为null时，Migration的逻辑为忽略掉该源端的还原
                            xmlField.CustomFieldInfo.NeedSkipRestore = true;
                        }
                        xmlField.CustomFieldInfo.SourceType = xmlField.Type;
                        if (IsTaxonomyField(xmlField.CustomFieldInfo.TypeAsString) && (xmlField.CustomFieldInfo as AveCustomMetadataFieldInfo) == null)
                        {//对于通过Excel设置的mapping，metadata column 不能设置termset等信息,需要走正常的column 创建逻辑
                            xmlField.CustomFieldInfo.TypeAsString = null;
                        }
                    }
                }
                if (FieldType == FieldType.List)
                {
                    var tmpFields = FieldMapping.GetNewFieldsBeforeAdd();
                    if (tmpFields != null)
                    {
                        foreach (var info in tmpFields)
                        {
                            try
                            {
                                if (info is AveCustomLookupFieldInfo && !string.IsNullOrEmpty((info as AveCustomLookupFieldInfo).ListTitle))
                                {
                                    needPostCreateCustomFields.Add(info);   //lookup column可能关联原端本list下column,需要在其它column后面还
                                    continue;
                                }
                                if (info is AveCustomMetadataFieldInfo)
                                {
                                    HandleAveCustomMetadataFieldInfo(info);
                                }
                                else if (!info.TypeAsString.Equals("Lookup", StringComparison.OrdinalIgnoreCase) &&
                                    !info.TypeAsString.Equals("LookupMulti", StringComparison.OrdinalIgnoreCase) &&
                                    !info.Type.Equals(AveFieldType.Invalid))
                                {
                                    HandleAveCustomFieldInfo(info);
                                }
                                var field = FieldCollection.GetFieldByInternalName(info.InternalName);
                                if (field != null && !String.IsNullOrEmpty(field.InternalName)
                                    && !field.InternalName.Equals("ID", StringComparison.OrdinalIgnoreCase)
                                    && !field.InternalName.Equals("FileLeafRef", StringComparison.OrdinalIgnoreCase)
                                    && !xmlFields.ContainsKey(info.InternalName))
                                {
                                    XmlDocument schemaXml = new XmlDocument();
                                    schemaXml.LoadXml(field.SchemaXml);
                                    AveXmlField xmlField = new AveXmlField(schemaXml.DocumentElement, (int)this.mAveSPWeb.SPWeb.Language);
                                    xmlField.CustomFieldInfo = info;
                                    if (!info.TypeAsString.Equals(xmlField.TypeAsString, StringComparison.OrdinalIgnoreCase))
                                    {
                                        xmlField.TypeAsString = info.TypeAsString;
                                    }
                                    xmlFields[xmlField.FieldInternalName] = xmlField;
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Warn("Add the custom field:{0} failed. Error:{1}", info.Name, ex.ToString());
                            }
                        }
                    }
                }
            }
        }

        private void HandleAveCustomFieldInfo(AveCustomFieldInfo info)
        {
            string internalName = string.Empty;
            if (!CheckFieldIfExist(info, ref internalName))
            {
                info.InternalName = GetInternalName(info);
                UpdateFieldsCreatedDirectly(info);
                mAveSPList.NeedUpdateToDefaultView.Add(info.InternalName);
                mAveSPList.NeedUpdateToDefaultContentType.Add(info.InternalName);
            }
            else
            {
                info.InternalName = internalName;
            }
        }

        private string GetInternalName(AveCustomFieldInfo info)
        {
            if (info is AveCustomMetadataFieldInfo)
            {
                return CreateNewFieldByCustomMappingForNoSource(info);
            }
            return FieldCollection.Add(info.Name, info.Type, false);
        }

        private void HandleAveCustomMetadataFieldInfo(AveCustomFieldInfo info)
        {
            HandleAveCustomFieldInfo(info);
            if (!mAveSPList.AveFields.TaxonomyFields.Contains(info.InternalName))
            {
                mAveSPList.AveFields.TaxonomyFields.Add(info.InternalName);
            }
        }

        private bool CheckFieldIfExist(AveCustomFieldInfo info, ref string internalName)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.CheckFieldIfExist"))
            {
                if (!FieldCollection.ContainsField(info.Name))
                {
                    return false;
                }
                foreach (IAveField field in FieldCollection)
                {
                    if (field.Title.Equals(info.Name, StringComparison.OrdinalIgnoreCase) && field.TypeAsString.Equals(info.TypeAsString, StringComparison.OrdinalIgnoreCase))
                    {
                        internalName = field.InternalName;
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// 确保创建出的column 的setting和通过sharepoint界面创建一致
        /// </summary>

        private void UpdateFieldsCreatedDirectly(AveCustomFieldInfo info)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.UpdateFieldsCreatedDirectly"))
            {
                var field = FieldCollection.GetFieldByInternalName(info.InternalName);
                if (info.TypeAsString.Equals("DateTime", StringComparison.OrdinalIgnoreCase))
                {
                    var dtField = field as IAveFieldDateTime;
                    dtField.DisplayFormat = AveDateTimeFieldFormatType.DateOnly;
                    field.Update();
                }
                else if (info.TypeAsString.Equals("User", StringComparison.OrdinalIgnoreCase) ||
                         info.TypeAsString.Equals("UserMulti", StringComparison.OrdinalIgnoreCase))
                {
                    var userField = field as IAveFieldUser;
                    userField.SelectionMode = AveFieldUserSelectionMode.PeopleOnly;
                    if (info.TypeAsString.Equals("UserMulti", StringComparison.OrdinalIgnoreCase))
                    {
                        userField.AllowMultipleValues = true;
                    }
                    field.Update();
                }
                else if (info.TypeAsString.Equals("Choice", StringComparison.OrdinalIgnoreCase) ||
                         info.TypeAsString.Equals("MultiChoice", StringComparison.OrdinalIgnoreCase))
                {
                    var choiceField = field as IAveFieldMultiChoice;
                    AveCustomChoiceFieldInfo choiceInfo = info as AveCustomChoiceFieldInfo;
                    if (choiceInfo != null && choiceInfo.Choices != null)
                    {
                        SetChoices(choiceField, choiceInfo.Choices, true);
                    }
                    field.Update();
                }
            }
        }

        #endregion

        #endregion

        private void AddTaxonomyTextFieldMapping(Dictionary<Guid, IAveField> refFields, Dictionary<Guid, AveXmlField> xmlFields, Dictionary<string, string> taxonomyTextMapping)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.AddTaxonomyTextFieldMapping"))
            {
                var taxonomyTextFields = refFields.Values
                    .Select(field => field as IAveTaxonomyField)
                    .Where(field => field != null)
                    .GroupBy(field => field.TextField)
                    .Select(group => group.First())
                    .ToDictionary(field => field.TextField, field => field.InternalName);
                var taxonomyXmlTextFields = xmlFields.Values
                    .Where(field => (field.TypeAsString.Equals("TaxonomyFieldType", StringComparison.OrdinalIgnoreCase)
                                     || field.TypeAsString.Equals("TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase))
                                     && field.GetCustomerProperty("TextField") != null)
                    .GroupBy(field => new Guid(field.GetCustomerProperty("TextField").ToString()))
                    .Select(group => group.First())
                    .ToDictionary(field => new Guid(field.GetCustomerProperty("TextField").ToString()),
                                  field => field.FieldInternalName);
                foreach (var mapping in taxonomyTextFields.Concat(taxonomyXmlTextFields))
                {
                    if (refFields.ContainsKey(mapping.Key))
                    {
                        taxonomyTextMapping[refFields[mapping.Key].InternalName] = mapping.Value;
                    }
                    else if (xmlFields.ContainsKey(mapping.Key))
                    {
                        taxonomyTextMapping[xmlFields[mapping.Key].FieldInternalName] = mapping.Value;
                        xmlFields.Remove(mapping.Key);//not need restore text field.
                    }
                }
            }
        }

        private IAveField GetFieldByFieldRefXml(XmlElement fieldXml)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.GetFieldByFieldRefXml"))
            {
                if (fieldXml.HasAttribute("Name"))
                {
                    var fieldName = fieldXml.GetAttribute("Name");
                    if (AllFieldCollection.ContainsField(fieldName))
                    {
                        return AllFieldCollection.GetFieldByInternalName(fieldName);
                    }
                }
                if (fieldXml.HasAttribute("ID"))
                {
                    var fieldId = new Guid(fieldXml.GetAttribute("ID"));
                    if (AllFieldCollection.Contains(fieldId))
                    {
                        return AllFieldCollection.GetById(fieldId);
                    }
                }
                return null;
            }
        }

        private bool NeedRestore(XmlElement fieldXml)
        {
            if (!fieldXml.Name.Equals("Field", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            if (!fieldXml.HasAttribute("Name") && !fieldXml.HasAttribute("ColName"))
            {
                return false;
            }
            string internalName = fieldXml.GetAttribute("Name");

            if (CheckFilter(fieldXml))
            {
                //ADO-189818 由于Nintex online对应的Column type不是WorkflowStatus，Cache源端WorkflowStatus field，在转移field value时用于判断field是否为WorkflowStatus field
                if (IsWorkflowStatusField(fieldXml))
                {
                    workflowStatusFields.Add(internalName);
                }

                mSkippedRestoreFields.Add(internalName);
                return false;
            }
            return true;
        }

        private bool IsWorkflowStatusField(XmlElement fieldXml)
        {
            if (fieldXml.HasAttribute("Type"))
            {
                string type = fieldXml.GetAttribute("Type");
                return string.Equals(type, AveFieldType.WorkflowStatus.ToString(), StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        private bool CheckFilter(XmlElement fieldXml)
        {
            if (fieldXml.HasAttribute("Type"))
            {
                string type = fieldXml.GetAttribute("Type");
                return CheckFilter(type);
            }
            return true;
        }

        private bool CheckFilter(string type)
        {//由于Column Mapping的缘故，此处不应该在目的端再去check  Type是否存在，若不存在默认走创建逻辑，创建失败
            return FieldTypeFilter != null && FieldTypeFilter.Contains(type);
            //|| (mAveSPWeb.SPWeb.FieldTypeDefinitionCollection != null &&
            //    mAveSPWeb.SPWeb.FieldTypeDefinitionCollection[type] == null);
        }

        #endregion

        #region Restore Field

        public void RestoreFields(string fieldsXml)
        {
            var restoreOption = new AveFieldRestoreOption();
            RestoreFields(fieldsXml, restoreOption);
        }

        public virtual void RestoreFields(string fieldsXml, AveFieldRestoreOption restoreOption)
        {
            LoadFields(fieldsXml);
            RestoreFields(currentXmlFields, restoreOption);
        }

        //改为internal,需要在还原workflow时调用，来反插AssociatedColumn
        internal void RestoreFields(Dictionary<string, AveXmlField> xmlFields, AveFieldRestoreOption restoreOption)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.RestoreFields"))
            {
                try
                {
                    this.restoreOption = restoreOption;
                    var sortedFields = SortFieldsForRestore(xmlFields);
                    bool needReload = false;
                    foreach (var fieldName in sortedFields)
                    {
                        bool fieldAddOrUpdate = false;
                        var xmlField = xmlFields[fieldName];
                        RestoreSingleField(restoreOption, xmlField, false, false, false, out fieldAddOrUpdate);
                        needReload |= fieldAddOrUpdate;
                    }
                    needReload |= PostCreateCustomFields(needPostCreateCustomFields);
                    if (needReload)
                    {
                        if (mAveSPList != null && mAveSPList.AveList != null)
                        {
                            mAveSPList.AveList.Reload();
                        }
                        else
                        {
                            log.Debug("list is null ,do not need to reload.");
                        }
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Warn("An error occurred while restore fields, Type:{0}, parent title:{1}, Reason:{2}.", ObjectType, ObjectTitle, ex);
                    reportor.AddDetail(new AveWrapperReportDto(ObjectTitle, ObjectTitle, ObjectType, AveStatus.Skipped, ex.Message));
                }
            }
        }

        private bool PostCreateCustomFields(List<AveCustomFieldInfo> needPostCreateCustomFields)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.PostCreateCustomFields"))
            {
                bool needReload = false;
                foreach (var info in needPostCreateCustomFields)
                {
                    try
                    {
                        string internalName = string.Empty;
                        if (!CheckFieldIfExist(info, ref internalName))
                        {
                            info.InternalName = CreateNewFieldByCustomMappingForNoSource(info);
                            UpdateFieldsCreatedDirectly(info);
                            mAveSPList.NeedUpdateToDefaultView.Add(info.InternalName);
                            mAveSPList.NeedUpdateToDefaultContentType.Add(info.InternalName);
                            needReload = true;
                        }
                        else
                        {
                            info.InternalName = internalName;
                        }
                        var field = FieldCollection.GetFieldByInternalName(info.InternalName);
                        if (field != null && !String.IsNullOrEmpty(field.InternalName)
                            && !mXmlFields.ContainsKey(info.InternalName))
                        {
                            XmlDocument schemaXml = new XmlDocument();
                            schemaXml.LoadXml(field.SchemaXml);
                            AveXmlField xmlField = new AveXmlField(schemaXml.DocumentElement, (int)this.mAveSPWeb.SPWeb.Language);
                            xmlField.CustomFieldInfo = info;
                            mXmlFields[xmlField.FieldInternalName] = xmlField;
                        }
                    }
                    catch (Exception e)
                    {
                        log.Warn("An error occurred while post create customField, field internalName:{0}, error:{1}", info.InternalName, e);
                    }
                }
                return needReload;
            }
        }

        /// <summary>
        /// ADO-143397 真实O365 特有的Field，无法还原到13、10环境，
        /// 鉴于SP16版本可能会支持这样的field，因此这样比较
        /// 当前已知的存在问题的FieldId如下:
        /// "786099e5-d20a-4232-86e5-cfc3d6face96"
        /// "14ee99cd-bed9-474a-bf99-8f753fbad6b4"
        /// </summary>
        /// <param name="xmlField"></param>
        /// <returns>
        /// true: 源端version较大
        /// false: 目的端version较大
        /// </returns>
        private bool CompareSPVersionWithSchemaVersion(string schemaVersion)
        {
            try
            {
                if (!string.IsNullOrEmpty(this.mAveParentSite.SPSite.SPVersion) && !string.IsNullOrEmpty(schemaVersion))
                {
                    var siteVersion = new Version(this.mAveParentSite.SPSite.SPVersion);
                    var fieldVersion = new Version(schemaVersion);
                    return fieldVersion.Major >= siteVersion.Major;
                }
            }
            catch (Exception e)
            {
                log.Warn("An error occurred while compare field schema version with SPVersion. Error: {1}", e);
            }
            return false;

        }

        private bool NeedSkipField(AveXmlField xmlField)
        {
            if ((xmlField.CustomFieldInfo != null && xmlField.CustomFieldInfo.NeedSkipRestore)
                               || xmlField.ID == new Guid("1390a86a-23da-45f0-8efe-ef36edadfb39")
                               || xmlField.ID == new Guid("f3b0adf9-c1a2-4b02-920d-943fba4b3611")
                               || xmlField.ID == new Guid("8f6b6dd8-9357-4019-8172-966fcd502ed2"))
            {//TaxKeywordTaxHTField，Taxonomy Catch All Column 这两个column在还原其他column的时候会自动创建出来，不再需要进行还原
                if (!mSkippedRestoreFields.Contains(xmlField.FieldInternalName))
                {
                    mSkippedRestoreFields.Add(xmlField.FieldInternalName);
                }
                FieldMapping.AddSkippedFields(xmlField.FieldInternalName);
                return true;
            }

            //Foundation 环境不支持meta data service
            if (!AveEnv.IsMoss && this.mAveParentSite.SPContextKind != AveContextKind.ClientObjectModel &&
                (xmlField.TypeAsString.Equals("TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase) ||
                xmlField.TypeAsString.Equals("TaxonomyFieldType", StringComparison.OrdinalIgnoreCase)))
            {
                log.Warn("Current SharePoint environment is foundation, so can not support metadata column, column name: {0}.", xmlField.Title);
                reportor.AddDetail(new AveWrapperReportDto(xmlField.Title, ObjectTitle, ObjectType, AveStatus.Skipped, AveReportResource.Wrapper_Report_SkipFoundationMetadataColum));
                return true;
            }

            if (CompareSPVersionWithSchemaVersion(xmlField.SchemaVersion))
            {
                return true;
            }
            //真实O365 特有的Field，无法还原到Local
            if (!this.mAveParentSite.SPSite.IsOnlineSite
             && (OnlineFieldId.NeedSkipOnlineField.FirstOrDefault(f => f == xmlField.ID) != Guid.Empty))
            {
                return true;
            }

            return false;
        }
        protected IAveField RestoreSingleField(AveFieldRestoreOption restoreOption, AveXmlField xmlField, bool throwWhenNotFound, bool throwWhenConflict, bool isEnsureField, out bool needReload)
        {
            needReload = false;
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.RestoreSingleField"))
            {
                if (NeedSkipField(xmlField))
                {
                    return null;
                }

                List<FieldFindOption> findOptions = restoreOption.FindOption.ToList();
                if (xmlField.CustomFieldInfo != null)
                {//Custom Mapping配置的Field只能通过CustomMapping去找
                    findOptions = new List<FieldFindOption> { FieldFindOption.CustomMapping };
                }
                if (xmlField.NeedFindByDisplayName)
                {
                    findOptions.Add(FieldFindOption.DisplayName);
                }
                IAveField field = Find(xmlField, findOptions);
                if (!restoreOption.OverwriteBuiltinField && AveBuiltInFieldId.Contains(xmlField.ID) && xmlField.XmlElement.HasAttribute("FromBaseType") && string.Equals("FALSE", xmlField.XmlElement.GetAttribute("FromBaseType"), StringComparison.OrdinalIgnoreCase))
                {
                    // 如果源端数据是buil-in且from base type有值且是false的情况，不还原这个field，find到加mapping即可。
                    if (field != null)
                    {
                        SetFieldMapping(xmlField, field);
                    }
                    log.Warn(string.Format("Build-in field has been changed, skip restore field:{0}.", xmlField.FieldInternalName));
                    return null;
                }
                if (isFieldRestored)
                {
                    isFieldRestored = false;
                    return field;
                }
                if (null == field && throwWhenNotFound)
                {
                    throw new AveSchemaDependencyNotFoundException(xmlField.Title, "field");
                }

                //添加NeedCompareField判断条件原因：因为ChangeToxxx 类型的column mapping，当源端与目的端冲突时，应保持目的端，不应再还原EnforceUniqueValue属性。
                if (FieldType == FieldType.List && xmlField.EnforceUniqueValues && restoreOption.ConflictOption != FieldConflictOption.Skip && NeedCompareField(xmlField))
                {
                    //若EnforceUniqueValues为ture的话，会导致item创建失败的情况(item1,item2的不同version，value可能一样，还原报错)，在此将该值先设置为false，在PostAction中再进行处理
                    //反插逻辑中，Skip还原的话，不会去改变目的端的column
                    //反插还存在个bug，目的端为True时，column不冲突情况下还原item还会存在原来的bug

                    xmlField.EnforceUniqueValues = false;
                    if (xmlField.XmlElement.HasAttribute("EnforceUniqueValues"))
                    {
                        xmlField.XmlElement.SetAttribute("EnforceUniqueValues", "False");
                    }
                    needUpdateUniqueValueFields.Add(xmlField.FieldInternalName);
                }
                return RestoreSingleField(xmlField, field, restoreOption, isEnsureField, throwWhenConflict, out needReload);
            }
        }

        private IAveField RestoreSingleField(AveXmlField xmlField, IAveField field, AveFieldRestoreOption restoreOption, bool isEnsureField, bool throwWhenConflict, out bool needReload)
        {
            needReload = false;
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.RestoreSingleField_1"))
            {
                try
                {
                    if (null != field)
                    {
                        bool isConflict = false;
                        if (NeedCompareField(xmlField))
                        {
                            if (isEnsureField && restoreOption.ConflictOption == FieldConflictOption.Skip)
                            {
                                isConflict = !CompareEnsureField(xmlField, field);
                            }
                            else
                            {
                                isConflict = !Compare(xmlField, field);
                            }

                            if (throwWhenConflict && isConflict)
                            {
                                throw new AveSchemaDependencyConflictException(field.Title, ObjectType.ToString());
                            }
                            if (isConflict)
                            {
                                field = HandleConflict(xmlField, field, restoreOption);
                                // update or create field之后，会导致web下lists和缓存的list不一致。需要reload一次缓存的list。
                                needReload = true;
                            }
                            else
                            {
                                if (UpdateNotConflictProperties(field, xmlField))
                                {
                                    //update field之后，会导致web下lists和缓存的list不一致。需要reload一次缓存的list。
                                    needReload = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        field = CreateNewField(xmlField, null, false);
                        // create field之后，会导致web下lists和缓存的list不一致。需要reload一次缓存的list。
                        needReload = true;
                    }
                    if (field != null)
                    {
                        if (restoreOption.CompareMd5)
                        {
                            AveFieldHelper.UpdateMD5ToSchemaXml(field);
                        }
                        SetFieldMapping(xmlField, field);
                    }
                    reportor.AddDetail(new AveWrapperReportDto(field == null ? xmlField.Title : field.Title, ObjectTitle, ObjectType, AveStatus.Successful, string.Empty));
                }
                catch (AveSchemaDependencyConflictException)
                {
                    throw;
                }
                catch (AveSchemaDependencyNotFoundException)
                {
                    throw;
                }
                catch (AveSecurityTrimingException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    var objectType = (ContextValues.SharePoint.ObjectType)Enum.Parse(typeof(ContextValues.SharePoint.ObjectType), FieldType.ToString());
                    log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper,
                            new EventIds.SharePoint.RestoreColumnFailedEventMessage(xmlField.FieldInternalName, xmlField.Title, objectType, ex));
                    if (!restoreFailedFields.ContainsKey(xmlField.FieldInternalName))
                    {
                        restoreFailedFields.Add(xmlField.FieldInternalName, ex);
                    }
                    FieldMapping.AddFailedFields(xmlField.FieldInternalName);
                    xmlField.RestoreStatus = FieldRestoreStatus.Exception;
                    if (ex is AveWrapperBaseException)
                    {
                        AveWrapperBaseException wrapperEx = (ex as AveWrapperBaseException);
                        reportor.AddDetail(new AveWrapperReportDto(wrapperEx.I18NKey, xmlField.Title, ObjectTitle, ObjectType, AveStatus.Failed, wrapperEx.Parameters));
                    }
                    else
                    {
                        reportor.AddDetail(new AveWrapperReportDto(xmlField.Title, ObjectTitle, ObjectType, AveStatus.Failed, ex.Message));
                    }
                    if (isEnsureField)
                    {
                        throw;//需要讨论，可能需要新加option来判断是否抛出异常
                    }
                    return null;
                }
                finally
                {
                    ClearNeedDeletedFields();//for calculated field, delete the dependency field.like Today
                }
                return field;
            }
        }

        private bool NeedCompareField(AveXmlField xmlField)
        {
            if (xmlField.CustomFieldInfo != null)
            {
                switch (xmlField.CustomFieldInfo.CustomFieldType)
                {
                    //通过CustomMapping找到的Column, ChangeToDes, ChangeToLookup, ChangeToMetadata, 不需要走Compare, 直接认为不冲突
                    case AveCustomFieldType.ChangeToDestination:
                    case AveCustomFieldType.ChangeToLookup:
                    case AveCustomFieldType.ChangeToMetadata:
                        return false;
                    case AveCustomFieldType.SameType:
                    default:
                        return true;
                }
            }
            return true;
        }

        protected void ClearNeedDeletedFields()
        {
            try
            {
                if (needDeletedFieldsForCalculatedField.Count > 0)
                {
                    foreach (string name in needDeletedFieldsForCalculatedField)
                    {
                        IAveField field = FieldCollection.GetFieldByInternalName(name);
                        if (field != null)
                        {
                            field.Delete();
                        }
                    }
                    needDeletedFieldsForCalculatedField.Clear();
                }
            }
            catch (Exception ex)
            {
                log.Warn("Clear need deleted field error.{0}", ex.ToString());
            }
        }

        private void SetFieldMapping(AveXmlField xmlField, IAveField field)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.SetFieldMapping"))
            {
                CacheFieldByType(xmlField, field);
                if (!restoredFieldIdMapping.ContainsKey(field.ID))
                {
                    restoredFieldIdMapping.Add(field.ID, xmlField.ID);
                }

                FieldMapping.AddFieldIdMapping(xmlField.ID, field.ID);
                FieldMapping.AddFieldInternalNameMapping(xmlField.FieldInternalName, field.InternalName);
                if (!field.Title.Equals(xmlField.SourceTitle) &&
                    String.IsNullOrEmpty(FieldMapping.GetMappingRestoredFieldDisplayName(xmlField.Title)))
                {
                    FieldMapping.AddFieldDisplayNameMapping(xmlField.SourceTitle, field.Title);
                }
                if (!field.ID.Equals(xmlField.ID) && FieldMapping.GetMappingSchemaFieldId(xmlField.ID) == Guid.Empty && xmlField.CustomFieldInfo == null)
                {
                    FieldMapping.AddFieldIdSchemaMapping(xmlField.ID, field.ID);
                }
            }
        }

        #region Sort fields by dependency before restore

        private List<string> SortFieldsForRestore(IDictionary<string, AveXmlField> xmlFields)
        {
            return SortFieldsForRestore(xmlFields.Keys.ToList(), xmlFields);
        }

        protected List<string> SortFieldsForRestore(List<string> fields, IDictionary<string, AveXmlField> xmlFields)
        {
            if (fields == null || xmlFields == null)
            {
                log.Debug("Fields info is null.Do not need to sort.");
            }

            var sorttedFields = new List<string>();
            var computedFields = new List<string>();
            var lookupFields = new List<string>();
            foreach (var field in fields)
            {
                if (field != null && xmlFields.ContainsKey(field) && xmlFields[field] != null)
                {
                    switch (xmlFields[field].Type)
                    {
                        case AveFieldType.Lookup:
                            lookupFields.Add(field);
                            break;
                        case AveFieldType.Calculated:
                        case AveFieldType.Computed:
                            computedFields.Add(field);
                            break;
                        default:
                            sorttedFields.Add(field);
                            break;
                    }
                }
            }
            SortForComputedFields(xmlFields, sorttedFields, computedFields);
            SortForLookupFields(xmlFields, sorttedFields, lookupFields);
            return sorttedFields.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        private void SortForLookupFields(IDictionary<string, AveXmlField> xmlFields, List<string> sorttedFields, List<string> lookupFields)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.SortForLookupFields"))
            {
                //PrimaryFieldId不为null的Field依赖于其他Lookup Field，所以放在后面
                for (int i = 0; i < lookupFields.Count; ++i)
                {
                    var lookupField = lookupFields[i];
                    if (xmlFields[lookupField].PrimaryFieldId == null)
                    {
                        sorttedFields.Add(lookupField);
                        lookupFields.RemoveAt(i);
                        --i;
                    }
                }
                sorttedFields.AddRange(lookupFields);
            }
        }

        private void SortForComputedFields(IDictionary<string, AveXmlField> xmlFields, List<string> sorttedFields, List<string> computedFields)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.SortForComputedFields"))
            {
                //Calculated Field不会有循环依赖，但是会有交叉依赖，这个方法按照依赖的顺序将Field排序。
                for (int i = 0; i < computedFields.Count; ++i)
                {
                    var computedField = computedFields[i];
                    bool hasDependancyNotRestore = false;
                    var xmlField = xmlFields[computedField];
                    RemoveOldCalculatedColumnRefs(xmlField);
                    var selectSingleNodes = xmlField.XmlElement.SelectNodes("FieldRefs");
                    if (selectSingleNodes != null)
                    {
                        foreach (XmlNode selectSingleNode in selectSingleNodes)
                        {
                            if (selectSingleNode != null)
                            {
                                List<string> refFields =
                                    selectSingleNode.ChildNodes.Cast<XmlElement>().Select(
                                        xe => xe.GetAttribute("Name")).ToList();
                                foreach (var refField in refFields)
                                {
                                    if (xmlFields.ContainsKey(refField) && !sorttedFields.Contains(refField) && !computedFields.Contains(refField))
                                    {//Ensure Field时，不会Ensure所有的，只会Ensure有Value的，这时需要反插依赖的Field
                                        switch (xmlFields[refField].Type)
                                        {
                                            case AveFieldType.Calculated:
                                            case AveFieldType.Computed:
                                                computedFields.Add(refField);
                                                break;
                                            default:
                                                sorttedFields.Add(refField);
                                                break;
                                        }
                                    }
                                    //ADO-189011 由于有两个column 的internal name 比较接近（“Value_x0020_of_x0020_assets_x002” & “Value_x0020_of_x0020_assets_x0020”），在还原“...0020” column 时，internal name 被sp 还原成“002”，导致后续更新formula时，使用mapping 更新成自引用形式。
                                    if (computedFields.Contains(refField) && !refField.Equals(computedField, StringComparison.OrdinalIgnoreCase))
                                    {
                                        hasDependancyNotRestore = true;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        log.Log(AveLogLevel.DEBUG, "computed field without ref {0}", computedField);
                    }

                    if (!hasDependancyNotRestore)
                    {
                        sorttedFields.Add(computedField);
                        computedFields.RemoveAt(i);
                        --i;
                    }
                }
                if (computedFields.Count > 0)
                {
                    SortForComputedFields(xmlFields, sorttedFields, computedFields);
                }
            }
        }

        /// <summary>
        /// We should remove the old field ref from schemaxml. 
        /// Otherwise, If a deleted column exists in the FieldRefs, the calculated column may never be restored to the destination.
        /// </summary>
        /// <param name="field"></param>
        private void RemoveOldCalculatedColumnRefs(AveXmlField field)
        {
            var beforeChanged = field.XmlElement.OuterXml;
            try
            {
                var selectSingleNodes = field.XmlElement.SelectNodes("FieldRefs");
                if (string.IsNullOrEmpty(field.Formula) || selectSingleNodes == null || selectSingleNodes.Count < 1)
                {
                    return;
                }


                List<XmlNode> needRemoveNodes = new List<XmlNode>();

                foreach (XmlNode node in selectSingleNodes)
                {
                    if (node == null) continue;

                    List<string> refFields =
                        node.ChildNodes.Cast<XmlElement>().Select(
                            xe => xe.GetAttribute("Name")).ToList();

                    foreach (var name in refFields)
                    {
                        if (field.Formula.IndexOf(name, StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            needRemoveNodes.Add(node);
                            break;
                        }
                    }
                }
                if (needRemoveNodes.Count > 0
                    && needRemoveNodes.Count != selectSingleNodes.Count)//There should be some errors here if two Count equal.
                {
                    foreach (var node in needRemoveNodes)
                    {
                        field.XmlElement.RemoveChild(node);
                    }
                    log.Info("Removed the old field ref rom calculated column. Before: {0}, After: {1}", beforeChanged, field.XmlElement.OuterXml);
                }
            }
            catch (Exception e)
            {
                log.Warn("Remove old culculated column FieldRefs failed. SchemaXml: {0}, Error: {1}", beforeChanged, e);
            }
        }

        #endregion

        #endregion

        #region Conflict

        private IAveField HandleConflict(AveXmlField xmlField, IAveField field, AveFieldRestoreOption restoreOption)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.HandleConflict"))
            {
                if (restoreOption.CompareMd5 && CompareMD5(field))
                {//对于需要比较MD5值的，若目的端SchemaXml中存在MD5属性，并且与当前Field的MD5值相同，则不认为冲突，直接进行update
                    UpdateField(field, xmlField);
                }
                else if (xmlField.CustomFieldInfo != null)
                {//对于find出来的field，如果配置过CustomMapping，则不再进行冲突处理,只有SameType的时候需要update目的端
                    if (xmlField.CustomFieldInfo.CustomFieldType == AveCustomFieldType.SameType)
                    {
                        UpdateField(field, xmlField);
                    }
                    else if (needUpdateUniqueValueFields.Contains(xmlField.FieldInternalName))
                    {
                        needUpdateUniqueValueFields.Remove(xmlField.FieldInternalName);
                    }
                }
                else
                {
                    field = HandleConflict(xmlField, field, restoreOption.ConflictOption);
                }
                return field;
            }
        }

        private IAveField HandleConflict(AveXmlField xmlField, IAveField field, FieldConflictOption conflictOption)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.HandleConflict_1"))
            {
                switch (conflictOption)
                {
                    case FieldConflictOption.Overwrite:
                        UpdateField(field, xmlField);
                        break;
                    case FieldConflictOption.AppendSourceWin:
                        if (xmlField.Title.Equals(field.Title, StringComparison.OrdinalIgnoreCase))
                        {
                            field.Title = AveFieldHelper.GetNewDisplayName(field.Title, AllFieldCollection);
                            field.Update();
                        }
                        field = CreateNewField(xmlField, field, false);
                        break;
                    case FieldConflictOption.AppendDestinationWin:
                        field = CreateNewField(xmlField, field, true);
                        break;
                    case FieldConflictOption.Skip:
                    default:
                        break;
                }
                return field;
            }
        }

        private bool CompareMD5(IAveField field)
        {
            var xmlMD5 = AveFieldHelper.GetMD5FromSchemaXml(field);
            return !String.IsNullOrEmpty(xmlMD5) && xmlMD5.Equals(AveFieldHelper.GetCurrentMD5Property(field), StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region Create Field

        private IAveField CreateNewField(AveXmlField xmlField, IAveField field, bool changeDisplayName)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.CreateNewField"))
            {
                if (xmlField.CustomFieldInfo != null &&
                    (!String.IsNullOrEmpty(xmlField.CustomFieldInfo.TypeAsString) ||
                     xmlField.CustomFieldInfo.Type != AveFieldType.Invalid))
                {//对于用户配置的有type的mapping的field，需要根据type来创建,不走通过通过源端SchemaXml添加的逻辑
                    return CreateNewFieldByCustomMapping(xmlField);
                }
                else
                {
                    UpdateNewFieldXml(xmlField, field, changeDisplayName);
                    return CreateNewField(xmlField);
                }
            }
        }

        private IAveField CreateNewField(AveXmlField xmlField)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.CreateNewField_1"))
            {
                if (xmlField.CustomFieldInfo != null)
                {//判断是否配置了CustomMapping，若是,则按照CustomMapping处理
                    if (!String.IsNullOrEmpty(xmlField.CustomFieldInfo.InternalName))
                    {
                        xmlField.XmlElement.SetAttribute("Name", xmlField.CustomFieldInfo.InternalName);
                    }
                    if (!String.IsNullOrEmpty(xmlField.CustomFieldInfo.Name))
                    {
                        xmlField.XmlElement.SetAttribute("DisplayName", xmlField.CustomFieldInfo.Name);
                    }
                }
                IAveField field = null;
                UpdateLookupAndTaxonomyFieldInfo(xmlField);
                if (xmlField.Type == AveFieldType.DateTime && !xmlField.EnforceUniqueValues)
                {
                    XmlNode node = xmlField.XmlElement.SelectSingleNode("Default");
                    if (node != null)
                    {
                        node.InnerText = ResetDateTimeDefaultValue(xmlField.DefaultValue);
                    }
                }
                if (xmlField.XmlElement.HasAttribute("Version"))
                {
                    xmlField.XmlElement.RemoveAttribute("Version");
                }

                //if (xmlField.XmlElement.HasAttribute("Sealed"))
                //{
                //    xmlField.XmlElement.RemoveAttribute("Sealed");
                //}
                try
                {
                    //Remove useless xml
                    if (xmlField.XmlElement.HasChildNodes && xmlField.XmlElement.ChildNodes.Count > 0)
                    {
                        //http://www.cnblogs.com/Miko2012/archive/2012/10/26/2740840.html   xpath grammar
                        XmlNode node = xmlField.XmlElement.SelectSingleNode("descendant::AveUserResource");
                        if (node != null)
                        {
                            node.ParentNode.RemoveChild(node);
                        }
                    }
                    field = FieldCollection.AddFieldAsXml(xmlField.XmlElement.OuterXml, false,
                                                 AveAddFieldOptions.AddFieldInternalNameHint |
                                                 AveAddFieldOptions.AddToNoContentType);
                    xmlField.RestoreStatus = FieldRestoreStatus.NewCreated;
                    //[ADO-150537]对于Formula里面有TODAY()函数的Calculate类型Column来说，当添加此column的时候，会使Web和List之间的关联关系丢失
                    if (!string.IsNullOrEmpty(xmlField.Formula)
                        && xmlField.Formula.IndexOf("TODAY()", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (this.mAveSPList != null)
                        {
                            this.mAveSPList.containsTODAY = true;
                        }
                    }
                    if (field != null && !string.IsNullOrEmpty(field.ValidationFormula) && !string.IsNullOrEmpty(field.Title) && field.ValidationFormula.Contains("#NAME?"))
                    {
                        string newFormulaStr = field.ValidationFormula.Replace("#NAME?", field.Title);
                        field.ValidationFormula = newFormulaStr;
                        field.Update();
                    }
                    if (field.TypeAsString == "TaxonomyFieldType" || field.TypeAsString == "TaxonomyFieldTypeMulti")
                    {
                        NeedReloadfieldsIfCreateMetadataField = true;
                    }
                    mAveSPWeb.SPWeb.AvailableFields.IsDirty = true;
                    SetTaxonomyField(xmlField, field, true);
                    CheckUserResource(field, xmlField);
                    return field;
                }
                catch (Exception ex)
                {
                    log.Warn("Failed to add field. Exception: {0}", ex);
                    if (field != null)
                    {
                        field.Delete();
                    }
                    throw;
                }
            }
        }

        private void CheckUserResource(IAveField field, AveXmlField xmlField)
        {
            bool needUpdate = false;
            if (field.TitleResource.SetUserResource(mAveSPWeb.SPWeb, xmlField.TitleResource))
            {
                needUpdate = true;
            }
            if (field.DescriptionResource.SetUserResource(mAveSPWeb.SPWeb, xmlField.DescriptionResource))
            {
                needUpdate = true;
            }
            if (needUpdate)
            {
                field.Update();
            }
        }

        private IAveField CreateNewFieldByCustomMapping(AveXmlField xmlField)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.CreateChoiceField"))
            {
                IAveField field;
                if (String.IsNullOrEmpty(xmlField.CustomFieldInfo.Name))
                {
                    xmlField.CustomFieldInfo.Name = xmlField.Title;
                }
                Guid sourceId = Guid.Empty;
                if (this.mAveSPWeb.Fields.restoredFieldIdMapping.ContainsKey(xmlField.ID))
                {
                    sourceId = this.mAveSPWeb.Fields.restoredFieldIdMapping[xmlField.ID];
                }
                else
                {
                    sourceId = xmlField.ID;
                }
                if (CheckFieldIdConflict(sourceId, null))
                {
                    sourceId = Guid.NewGuid();
                }

                xmlField.CustomFieldInfo.InternalName = GenerateInternalName(xmlField.CustomFieldInfo);

                if (xmlField.CustomFieldInfo.Type != AveFieldType.Invalid)
                {
                    switch (xmlField.CustomFieldInfo.Type)
                    {
                        case AveFieldType.Lookup:
                            field = CreateLookupField(xmlField.CustomFieldInfo as AveCustomLookupFieldInfo, sourceId);
                            break;
                        case AveFieldType.Choice:
                        case AveFieldType.MultiChoice:
                            field = CreateChoiceField(xmlField);
                            break;
                        default:
                            string internalName = FieldCollection.Add(xmlField.CustomFieldInfo.Name, xmlField.CustomFieldInfo.Type, false);
                            field = FieldCollection.GetFieldByInternalName(internalName);
                            break;
                    }
                }
                else
                {
                    //to do ,需要根据信息构造出相关SchemaXml再进行add
                    switch (xmlField.CustomFieldInfo.TypeAsString)
                    {
                        case "TaxonomyFieldType":
                        case "TaxonomyFieldTypeMulti":
                            field = CreateMetadataField(xmlField.CustomFieldInfo as AveCustomMetadataFieldInfo, sourceId);
                            break;
                        default: throw new NotSupportedColumnMappingException(xmlField.Title, xmlField.CustomFieldInfo.Name);//TO DO
                    }
                }
                mAveSPWeb.SPWeb.AvailableFields.IsDirty = true;
                if (!field.Group.Equals(xmlField.Group, StringComparison.OrdinalIgnoreCase))
                {
                    field.Group = xmlField.Group;
                    field.Update();
                }
                xmlField.RestoreStatus = FieldRestoreStatus.NewCreated;
                return field;
            }
        }

        protected virtual string GenerateInternalName(AveCustomFieldInfo customFieldInfo)
        {
            //List级别目前 不需要考虑InternalName冲突的情况
            return customFieldInfo.InternalName;
        }

        /// <summary>
        /// Excel 和 C# mapping 可能没有源端
        /// </summary>
        /// <returns></returns>
        private string CreateNewFieldByCustomMappingForNoSource(AveCustomFieldInfo customFieldInfo)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.CreateNewFieldByCustomMappingForNoSource"))
            {
                IAveField field;
                switch (customFieldInfo.TypeAsString)
                {
                    case "Lookup":
                    case "LookupMulti":
                        field = CreateLookupField(customFieldInfo as AveCustomLookupFieldInfo);
                        break;
                    case "TaxonomyFieldType":
                    case "TaxonomyFieldTypeMulti":
                        field = CreateMetadataField(customFieldInfo as AveCustomMetadataFieldInfo);
                        break;
                    default:
                        string internalName = FieldCollection.Add(customFieldInfo.Name, customFieldInfo.Type,
                            false);
                        field = FieldCollection.GetFieldByInternalName(internalName);
                        break;
                }
                return field.InternalName;
            }
        }
        #endregion

        #region Update field schema xml before add

        private bool CheckFieldIdConflict(Guid fieldId, IAveField field)
        {
            bool isIdConflict = false;
            isIdConflict = ((field != null) && (field.ID == fieldId)) || (AveFieldHelper.FindFieldById(fieldId, FieldCollection) != null);
            if (!isIdConflict && mAveParentSite.ObjectModelFactory.ContextKind.IsServerMode())
            {
                isIdConflict = FieldCollection.GetFieldInSiteChildren(GetScope(mAveSPWeb.ServerRelativeUrl), mAveParentSite.SPSite.ID, fieldId);
            }
            return isIdConflict;
        }

        private void UpdateNewFieldXml(AveXmlField xmlField, IAveField field, bool changeDisplayName)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.UpdateNewFieldXml"))
            {
                if (xmlField.FromParent.HasValue && xmlField.FromParent.Value)
                {
                    var des = AveFieldHelper.FindFieldByName(xmlField.FieldInternalName, xmlField.TypeAsString, mAveSPWeb.AveWeb.AvailableFields, FieldFindOption.InternalName);
                    if (des != null)
                    {
                        xmlField.XmlElement.SetAttribute("ID", des.ID.ToString());
                    }
                }
                else if (CheckFieldIdConflict(xmlField.ID, field))
                {
                    xmlField.XmlElement.SetAttribute("ID", Guid.NewGuid().ToString());
                }
                if (changeDisplayName)
                {
                    var newName = AveFieldHelper.GetNewDisplayName(xmlField.XmlElement.GetAttribute("DisplayName"), AllFieldCollection);
                    xmlField.XmlElement.SetAttribute("DisplayName", newName);
                }
                if (FieldType == FieldType.Site)
                {//web级别上的field通过schemaxml添加的时候，如果Name相同，会抛出异常
                    xmlField.XmlElement.SetAttribute("Name", AveFieldHelper.GetNewInternalName(xmlField.XmlElement.GetAttribute("Name"), AllFieldCollection));
                }
                UpdateSourceID(xmlField);
                if (field == null)
                {
                    UpdateCalculatedFieldInfo(xmlField);
                }
                UpdateUserFieldInfo(xmlField);
                UpdateChoiceFieldInfo(xmlField);
                UpdateFieldValidationInfo(xmlField);
                UpdateDefaultFormula(xmlField);
            }
        }

        /// <summary>
        /// ADO-99258  修改List级别Field的DefaultFormula值
        /// </summary>
        /// <param name="field"></param>
        private void UpdateDefaultFormula(AveXmlField field)
        {
            XmlNode node = field.XmlElement.SelectSingleNode("DefaultFormula");
            if (node != null && mAveSPList != null)
            {
                string newDefaultFormula = mAveSPWeb.SPWeb.GetFormula(mAveSPWeb.SPWeb.Url, mAveSPList.SPList.ID.ToString("B").ToUpper(mAveSPWeb.SPWeb.LanguageCulture), field.DefaultFormula, "");
                node.InnerText = newDefaultFormula;
            }
        }

        private string GetScope(string serverRelativeUrl)
        {
            if (!string.IsNullOrEmpty(serverRelativeUrl))
            {
                return serverRelativeUrl.TrimStart('/');
            }
            return serverRelativeUrl;
        }

        private void UpdateSourceID(AveXmlField srcXmlField)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.UpdateSourceID"))
            {
                if (srcXmlField.XmlElement.HasAttribute("SourceID"))
                {
                    if (srcXmlField.AveSourceType == "2" && mAveSPList != null)
                    {
                        srcXmlField.XmlElement.SetAttribute("SourceID", mAveSPList.SPList.ID.ToString("B"));
                    }
                    else if (srcXmlField.AveSourceType == "1" && mAveSPWeb != null)
                    {
                        srcXmlField.XmlElement.SetAttribute("SourceID", mAveSPWeb.SPWeb.ID.ToString("B"));
                    }
                    else if (srcXmlField.AveSourceType == "0")
                    {
                        srcXmlField.XmlElement.SetAttribute("SourceID", mAveSPWeb.ParentSite.SPSite.RootWeb.ID.ToString("B"));
                    }
                    //else //对于更改普通field（不是lookupfield）的sourceID，删掉schema中的SourceID后，再创建新的field的时候会自动添加当前SourceID
                    //{
                    //    srcXmlField.XmlElement.RemoveAttribute("SourceID");
                    //}
                }
            }
        }

        private void UpdateLookupAndTaxonomyFieldInfo(AveXmlField xmlField)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.UpdateLookupAndTaxonomyFieldInfo"))
            {
                if (xmlField.Type == AveFieldType.Lookup)
                {
                    UpdateLookupFieldInfo(xmlField, true);
                }
                if (xmlField.FieldBaseType.Equals("TaxonomyFieldType", StringComparison.OrdinalIgnoreCase) ||
                    xmlField.FieldBaseType.Equals("TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase))
                {
                    var rootWeb = mAveSPWeb.ParentSite.SPSite.RootWeb;
                    xmlField.XmlElement.SetAttribute("WebId", rootWeb.ID.ToString());
                    Guid listId;
                    if (rootWeb.Properties.ContainsKey("TaxonomyHiddenList"))
                    {
                        listId = new Guid(rootWeb.Properties["TaxonomyHiddenList"]);
                    }
                    else
                    {
                        listId = rootWeb.Lists["TaxonomyHiddenList"].ID;//For other language, the list title is same.
                    }
                    xmlField.XmlElement.SetAttribute("List", listId.ToString("B"));
                }
            }
        }

        private void UpdateFieldValidationInfo(AveXmlField xmlField)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.UpdateFieldValidationInfo"))
            {
                XmlNode validationNode = xmlField.XmlElement.SelectSingleNode("Validation");
                if (validationNode != null)
                {
                    if (!string.Equals(xmlField.FieldInternalName, xmlField.Title, StringComparison.Ordinal))
                    {
                        validationNode.InnerText = validationNode.InnerText.Replace(xmlField.FieldInternalName,
                                                                                    string.Format("[{0}]", xmlField.Title));
                    }
                    var currentDisplayName = xmlField.XmlElement.GetAttribute("DisplayName");
                    if (xmlField.CustomFieldInfo != null && !String.IsNullOrEmpty(xmlField.CustomFieldInfo.Name) && !string.Equals(xmlField.CustomFieldInfo.Name, xmlField.Title, StringComparison.Ordinal))
                    {
                        currentDisplayName = xmlField.CustomFieldInfo.Name;
                    }
                    if (!currentDisplayName.Equals(xmlField.Title, StringComparison.Ordinal))
                    {//Display name has mapped.
                        validationNode.InnerText = validationNode.InnerText.Replace(xmlField.Title, currentDisplayName);
                    }
                    if (mAveSPList != null && mAveSPList.SPList != null)
                    {
                        validationNode.InnerText = mAveSPWeb.SPWeb.GetFormula(mAveSPWeb.SPWeb.Url, mAveSPList.SPList.ID.ToString("B").ToUpper(mAveSPWeb.SPWeb.LanguageCulture), validationNode.InnerText, string.Empty);
                    }
                }
            }
        }

        private void UpdateChoiceFieldInfo(AveXmlField xmlField)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.UpdateChoiceFieldInfo"))
            {
                if (xmlField.CustomFieldInfo != null
                    && (xmlField.Type == AveFieldType.Choice || xmlField.Type == AveFieldType.MultiChoice))
                {
                    XmlElement fieldElement = (XmlElement)xmlField.XmlElement.SelectSingleNode("CHOICES");
                    AveSourceFieldInfo sourceFieldInfo = new AveSourceFieldInfo
                    {
                        SourceDisplayName = xmlField.Title,
                        SourceInternalName = xmlField.FieldInternalName,
                        SourceType = xmlField.Type
                    };
                    StringCollection choices = new StringCollection();
                    List<XmlNode> needDeleteNode = new List<XmlNode>();
                    foreach (XmlNode node in fieldElement.ChildNodes)
                    {
                        ReplaceChoice(node, sourceFieldInfo);
                        if (!choices.Contains(node.InnerText))
                        {
                            choices.Add(node.InnerText);
                        }
                        else
                        {
                            needDeleteNode.Add(node);
                        }
                    }
                    foreach (XmlNode node in needDeleteNode)
                    {
                        fieldElement.RemoveChild(node);
                    }
                    xmlField.Choices = choices;
                    ReplaceDefaultChoice(xmlField, sourceFieldInfo);
                }
            }
        }

        private void ReplaceDefaultChoice(AveXmlField xmlField, AveSourceFieldInfo sourceFieldInfo)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.ReplaceDefaultChoice"))
            {
                var valueInfo = new AveSourceFieldValueInfo();
                valueInfo.SourceFieldInfo = sourceFieldInfo;
                valueInfo.SourceValue = xmlField.DefaultValue;
                //we don't know the source item row Id yet,so we assign it with -1,so that it won't work when use metadata mapping
                valueInfo.SourceItemRowId = -1;
                string mappingChoice = FieldMapping.GetMappingValue(valueInfo);
                if (mappingChoice != null && !mappingChoice.Equals(xmlField.DefaultValue, StringComparison.Ordinal))
                {
                    xmlField.DefaultValue = mappingChoice;
                }
                XmlElement fieldElement = (XmlElement)xmlField.XmlElement.SelectSingleNode("Default");
                if (fieldElement != null)
                {
                    foreach (XmlNode node in fieldElement.ChildNodes)
                    {
                        ReplaceChoice(node, sourceFieldInfo);
                    }
                }
            }
        }

        private string GetMappedChoice(string choice, AveSourceFieldInfo sourceFieldInfo)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.GetMappedChoice"))
            {
                var valueInfo = new AveSourceFieldValueInfo();
                valueInfo.SourceFieldInfo = sourceFieldInfo;
                valueInfo.SourceValue = choice;
                //we don't know the source item row Id yet,so we assign it with -1,so that it won't work when use metadata mapping
                valueInfo.SourceItemRowId = -1;
                string mappingChoice = FieldMapping.GetMappingValue(valueInfo);
                if (mappingChoice != null && !mappingChoice.Equals(choice, StringComparison.Ordinal))
                {
                    return mappingChoice;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        private void ReplaceChoice(XmlNode node, AveSourceFieldInfo sourceFieldInfo)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.ReplaceChoice"))
            {
                var valueInfo = new AveSourceFieldValueInfo();
                valueInfo.SourceFieldInfo = sourceFieldInfo;
                valueInfo.SourceValue = node.InnerText;
                //we don't know the source item row Id yet,so we assign it with -1,so that it won't work when use metadata mapping
                valueInfo.SourceItemRowId = -1;
                string mappingChoice = FieldMapping.GetMappingValue(valueInfo);
                if (mappingChoice != null && !mappingChoice.Equals(node.InnerText, StringComparison.Ordinal))
                {
                    node.InnerText = mappingChoice;
                }
            }
        }

        private void UpdateUserFieldInfo(AveXmlField srcXf)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.UpdateUserFieldInfo"))
            {
                if (srcXf.XmlElement.HasAttribute("UserSelectionScope"))
                {
                    int srcMemberId = int.Parse(srcXf.XmlElement.GetAttribute("UserSelectionScope"));
                    if (srcMemberId != 0)
                    {
                        IAvePrincipal desPrincipal = mAveSPWeb.ParentSite.SPMembers.FindMember(srcMemberId, true);
                        if (desPrincipal != null)
                        {
                            srcXf.XmlElement.SetAttribute("UserSelectionScope",
                                                             desPrincipal.ID.ToString(
                                                                 CultureInfo.InvariantCulture));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 这个函数在restore的过程中，每个fieldxml只能调用一次。
        /// 因为第一次调用的时候会把formula里面的值换成displayname，所以当第二次调用的时候会用displayName去找name，如果当displayName和internalName不一样的时候就会出错。
        /// </summary>
        /// <param name="srcXf"></param>
        private void UpdateCalculatedFieldInfo(AveXmlField srcXf)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.UpdateCalculatedFieldInfo"))
            {
                XmlNodeList nodes = srcXf.XmlElement.GetElementsByTagName("Formula");
                XmlNodeList fieldRefNodes = srcXf.XmlElement.GetElementsByTagName("FieldRef");
                XmlElement element = srcXf.XmlElement;
                if (nodes.Count > 0)
                {
                    string formula = nodes[0].InnerText;
                    var fieldDic = new Dictionary<string, string>();
                    var refFieldsDispNames = new List<string>();
                    string internalName = string.Empty;
                    int destWebLanguageId = mAveSPWeb.SPWeb.LanguageCulture.LCID;
                    for (int i = 0; i < fieldRefNodes.Count; i++)
                    {
                        string oldInternalName = fieldRefNodes[i].Attributes["Name"].Value;
                        if (!formula.Contains(oldInternalName))
                            continue;
                        internalName = oldInternalName;
                        //语言不同时才需要走LanguageMapping
                        if (mAveSPWeb.WebSrcLanguageId != destWebLanguageId)
                        {
                            internalName = mAveSPWeb.ParentSite.GetNameByLanguageMapping(oldInternalName, AveLanguageMappingType.FieldMapping);
                        }
                        string mappingValue = FieldMapping.GetMappingRestoredFieldInternalName(internalName);
                        if (!String.IsNullOrEmpty(mappingValue) && !internalName.Equals(mappingValue))
                        {
                            internalName = mappingValue;
                            fieldRefNodes[i].Attributes["Name"].Value = internalName;
                        }

                        if (!ContainsFieldForCalculatedField(internalName))
                        {
                            if (internalName.Equals("Today", StringComparison.OrdinalIgnoreCase))
                            {
                                string tempName = FieldCollection.Add("Today", AveFieldType.Integer, false);
                                fieldDic.Add(oldInternalName, tempName.Trim());
                                if (!needDeletedFieldsForCalculatedField.Contains(tempName))
                                {
                                    needDeletedFieldsForCalculatedField.Add(tempName);
                                }
                            }
                            else
                            {
                                throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Restore_FieldNotExist, internalName);
                            }
                        }

                        if (!fieldDic.ContainsKey(oldInternalName) && FieldType == FieldType.List)
                        {
                            string displayName = FieldCollection.GetField(internalName).Title.Trim();
                            refFieldsDispNames.Add(displayName);
                            if (!mAveParentSite.ObjectModelFactory.ContextKind.IsServerMode())
                            {
                                if (oldInternalName.Equals("ContentType", StringComparison.OrdinalIgnoreCase))
                                {
                                    fieldDic.Add(oldInternalName, "#NAME?");
                                }
                                else
                                {
                                    displayName = "[" + displayName + "]";
                                    fieldDic.Add(oldInternalName, displayName);
                                }
                            }
                        }
                        if (!fieldDic.ContainsKey(oldInternalName) && FieldType == FieldType.Site)
                        {
                            fieldDic.Add(oldInternalName, internalName.Trim());
                        }


                        if (!fieldDic.ContainsKey(oldInternalName))
                        {
                            fieldDic.Add(oldInternalName, internalName.Trim());
                        }
                    }

                    formula = SetFormula(formula, fieldDic);
                    var formulaElements = new List<string>();
                    for (int i = 0; i < refFieldsDispNames.Count; i++)
                    {
                        var refFieldName = refFieldsDispNames[i];
                        for (int j = 0; j < formulaElements.Count; j++)
                        {
                            if (refFieldsDispNames[i].Contains(formulaElements[j]))
                            {
                                var tempFieldName = refFieldsDispNames[i].Replace(formulaElements[j], string.Format("@{0}@", j));
                                if (refFieldName.Equals(refFieldsDispNames[i], StringComparison.OrdinalIgnoreCase) || refFieldName.Length > tempFieldName.Length)
                                {
                                    refFieldName = tempFieldName;
                                }
                            }
                        }
                        if (formula.Contains(refFieldName))
                        {
                            formula = formula.Replace(refFieldName, string.Format("@{0}@", i));
                            formulaElements.Add(refFieldsDispNames[i]);
                        }
                    }
                    var format = WrapperConfiguration.AddBracketsForFormula ? "[{0}]" : "{0}";
                    for (int i = 0; i < formulaElements.Count; i++)
                    {
                        formula = formula.Replace(string.Format("@{0}@", i), string.Format(format, formulaElements[i]));
                    }
                    //foreach (var refFieldName in refFieldsDispNames)
                    //{
                    //    if (formula.Contains(refFieldName))
                    //    {
                    //        formula = formula.Replace(refFieldName, string.Format("[{0}]", refFieldName));
                    //    }
                    //}
                    //Sometimes there is already [] mark outside the displayname, so we need to remove the duplicated one. Changed by Austin Han.
                    formula = RemoveSurplusSquareBrackets(formula);
                    nodes[0].InnerText = formula;
                    srcXf.Formula = formula;
                }

                //此段代码把indexable为false，indexed为true的更新为了indexable为false，indexed为false。因为indexable为false的话，indexed为何值都没有意义。而且indexed为true就不能够创建出来。indexable为false那么indexed属性就不能更新。
                if (element.HasAttribute("Type") && element.GetAttribute("Type").Equals("calculated", StringComparison.OrdinalIgnoreCase) && element.HasAttribute("Indexed") && element.GetAttribute("Indexed").Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    element.RemoveAttribute("Indexed");
                }
            }
        }

        private string RemoveSurplusSquareBrackets(string formula)
        {
            Regex reg1 = new Regex(@"(\[)+\[");
            Regex reg2 = new Regex(@"(\])+\]");
            return reg2.Replace(reg1.Replace(formula, "["), "]");
        }

        private bool ContainsFieldForCalculatedField(string internalName)
        {
            //Web级别的Column，是可以继承的，需要使用AvailableFields属性。
            return FieldType == Restore.FieldType.List ?
                            FieldCollection.ContainsField(internalName)
                            : this.mAveSPWeb.SPWeb.AvailableFields.ContainsField(internalName);
        }

        private string SetFormula(string oldFormula, Dictionary<string, string> fieldDic)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.SetFormula"))
            {
                IOrderedEnumerable<KeyValuePair<string, string>> dic =
                        fieldDic.OrderByDescending(pair => pair.Key.Length);
                string tempString = "@";
                var columnMapping = new Dictionary<string, string>();
                int index = 0;
                foreach (var p in dic)
                {
                    string newKey = tempString + index + tempString;
                    oldFormula = oldFormula.Replace(p.Key, newKey);
                    columnMapping.Add(newKey, p.Value);
                    index++;
                }
                oldFormula = columnMapping.Aggregate(oldFormula, (current, d) => current.Replace(d.Key, d.Value));
                string newFormula = oldFormula;   //For Office365��ADO-69304
                if (mAveSPList != null && mAveSPList.SPList != null)
                {
                    newFormula = mAveSPWeb.SPWeb.GetFormula(mAveSPWeb.SPWeb.Url, mAveSPList.SPList.ID.ToString("B").ToUpper(mAveSPWeb.SPWeb.LanguageCulture), oldFormula, string.Empty);
                }
                return newFormula;
            }
        }

        #endregion

        private IAveField CreateChoiceField(AveXmlField xmlField)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.CreateChoiceField"))
            {
                IAveField field;
                try
                {
                    if (xmlField.CustomFieldInfo.IsMulti)
                    {
                        xmlField.CustomFieldInfo.Type = AveFieldType.MultiChoice;
                    }
                    string internalName = FieldCollection.Add(xmlField.CustomFieldInfo.Name, xmlField.CustomFieldInfo.Type, false);
                    field = FieldCollection.GetFieldByInternalName(internalName);
                    var choiceField = field as IAveFieldMultiChoice;
                    if (choiceField != null && xmlField.Choices != null && xmlField.Choices.Count > 0)
                    {
                        foreach (string ch in xmlField.Choices)
                        {
                            //string choice = SPUtility.GetLocalizedString(ch.Trim(), "core", mLanguageID);
                            if (!choiceField.Choices.Contains(ch))
                            {
                                choiceField.Choices.Add(ch);
                            }
                        }
                        choiceField.Update();
                    }
                }
                catch (Exception ex)
                {
                    log.Warn("Create the custom choice field error,Field:{0}, Exception:{1}", xmlField.CustomFieldInfo.Name, ex.ToString());
                    throw;
                }
                return field;
            }
        }

        /// <tag>ADO-116309</tag>
        /// <summary>
        /// 用于获取lookup field的InternalName。
        /// </summary>
        /// <param name="web">Lookup 目标Field所在Web</param>
        /// <param name="lookInfo">Lookup Field的具体信息</param>
        /// <returns>String类型的lookup field的InternalName</returns>

        private string GetLookupFieldInternalName(IAveWeb web, AveCustomLookupFieldInfo lookInfo)
        {
            //todo performance
            IAveList list = web.Lists[lookInfo.ListTitle];

            //if (list.Fields.Where(n => n.Title == lookInfo.FieldName).Count() > 0)
            //{
            //    return list.Fields[lookInfo.FieldName].InternalName;
            //}

            foreach (var field in list.Fields)
            {
                if (string.Equals(field.Title, lookInfo.FieldName, StringComparison.CurrentCultureIgnoreCase))
                {
                    return field.InternalName;
                }
            }
            foreach (IAveField field in list.Fields)
            {
                if (string.Equals(field.InternalName, lookInfo.FieldName, StringComparison.CurrentCultureIgnoreCase))
                {
                    return field.InternalName;
                }
            }
            return null;
        }

        private IAveField CreateLookupField(AveCustomLookupFieldInfo lookupInfo)
        {
            return CreateLookupField(lookupInfo, Guid.Empty);
        }
        private IAveField CreateLookupField(AveCustomLookupFieldInfo lookupInfo, Guid sourceId)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.CreateLookupField"))
            {
                IAveField field;
                Debug.Assert(lookupInfo != null, "lookupInfo != null");
                try
                {
                    var doc = new XmlDocument();
                    doc.LoadXml(CustomFieldXml.lookupfieldFieldXml);

                    if (sourceId != Guid.Empty)
                    {
                        doc.DocumentElement.SetAttribute("ID", sourceId.ToString());
                    }
                    doc.DocumentElement.SetAttribute("Name", lookupInfo.InternalName);
                    doc.DocumentElement.SetAttribute("DisplayName", lookupInfo.Name);
                    doc.DocumentElement.SetAttribute("StaticName", lookupInfo.Name);
                    if (doc.DocumentElement.HasAttribute("Version"))
                    {
                        doc.DocumentElement.RemoveAttribute("Version");
                    }
                    IAveWeb web = mAveSPWeb.SPWeb;
                    if (!string.IsNullOrEmpty(lookupInfo.WebRelativeUrl))
                    {
                        web = mAveParentSite.SPSite.OpenWeb(lookupInfo.WebRelativeUrl.Replace(mAveParentSite.SPSite.RootWeb.ServerRelativeUrl.TrimStart('/'), "").TrimStart('/'));
                    }
                    Guid lookupWebId = web.ID;
                    Guid lookupListId = mAveSPWeb.ParentSite.GetList(lookupWebId, lookupInfo.ListTitle);
                    string internalName = string.Empty;
                    if (lookupListId == Guid.Empty)
                    {
                        doc.DocumentElement.RemoveAttribute("List");
                        if (doc.DocumentElement.HasAttribute("RelationshipDeleteBehavior"))
                        {
                            doc.DocumentElement.SetAttribute("RelationshipDeleteBehavior", "None");
                        }
                        if (doc.DocumentElement.HasAttribute("IsRelationship"))
                        {
                            doc.DocumentElement.SetAttribute("IsRelationship", "False");
                        }
                    }
                    else
                    {
                        doc.DocumentElement.SetAttribute("List", lookupListId.ToString());
                        ///<tag>ADO-116309
                        ///<summary>修正了使用Replicator时，Change to lookup 中 Column输入大小写不匹配时还原错误的bug。 </summary>
                        ///利用GetLookupFieldInternalName(IAveWeb,AveCustomLookupFieldInfo)方法获取LookupField的InternalName，再进行
                        ///Lookup Field的创建。
                        internalName = GetLookupFieldInternalName(web, lookupInfo);
                        if (string.IsNullOrEmpty(internalName))
                        {
                            ///<exception>在输入Column在目标List中不存在时，抛出AveWrapperLookupFieldNotFoundException异常</exception>
                            throw new AveWrapperLookupFieldNotFoundException(AveInternalResourceKey.Wrapper_Exception_Mapping_LookupFieldNotFound, lookupInfo.ListTitle, lookupInfo.FieldName);
                        }
                    }

                    field = FieldCollection.AddFieldAsXml(doc.OuterXml, false, AveAddFieldOptions.AddFieldInternalNameHint | AveAddFieldOptions.AddToNoContentType);
                    IAveFieldLookup lookupField = field as IAveFieldLookup;
                    lookupField.LookupField = !string.IsNullOrEmpty(internalName) ? internalName : lookupInfo.FieldName;
                    ///</tag>
                    //需要根据源端类型去决定mapping成Mult类型的field
                    if (lookupInfo.IsMulti)
                    {
                        lookupField.AllowMultipleValues = true;
                    }
                    lookupField.Update();
                    if (lookupListId == Guid.Empty)
                    {
                        Guid currentListId = mAveSPList == null ? Guid.Empty : mAveSPList.SPList.ID;//Current List Id, Guid.Empty if web lookup column
                        mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.AddNeedPostActionlookupColumnsForColumnMapping(lookupInfo.ListTitle, currentListId, field.ID, lookupInfo.FieldName);
                    }

                }
                catch (Exception ex)
                {
                    log.Warn("Create the custom lookup field error,Field:{0}, Exception:{1}", lookupInfo.Name, ex.ToString());
                    throw;
                }
                return field;
            }
        }

        private IAveField CreateMetadataField(AveCustomMetadataFieldInfo metadataInfo)
        {
            return CreateMetadataField(metadataInfo, Guid.Empty);
        }

        private IAveField CreateMetadataField(AveCustomMetadataFieldInfo metadataInfo, Guid sourceId)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.CreateMetadataField"))
            {
                IAveField field;
                try
                {
                    var doc = new XmlDocument();
                    doc.LoadXml(CustomFieldXml.MetadataFieldXml);
                    doc.DocumentElement.SetAttribute("DisplayName", metadataInfo.Name);
                    doc.DocumentElement.SetAttribute("StaticName", metadataInfo.Name);
                    doc.DocumentElement.SetAttribute("Name", metadataInfo.InternalName);
                    //doc.DocumentElement.SetAttribute("List", mAveSPList.SPList.ID.ToString());
                    //doc.DocumentElement.SetAttribute("SourceID", mAveSPList.SPList.ID.ToString());
                    //doc.DocumentElement.SetAttribute("WebId", mAveSPList.ParentWeb.SPWeb.ID.ToString());
                    if (sourceId != Guid.Empty)
                    {
                        doc.DocumentElement.SetAttribute("ID", sourceId.ToString());
                    }
                    else
                    {
                        doc.DocumentElement.SetAttribute("ID", Guid.NewGuid().ToString());
                    }
                    if (doc.DocumentElement.HasAttribute("Version"))
                    {
                        doc.DocumentElement.RemoveAttribute("Version");
                    }
                    if (sourceId != Guid.Empty)
                    {
                        string textFieldProperty = "<Property><Name>TextField</Name><Value xmlns:q6=\"http://www.w3.org/2001/XMLSchema\" p4:type=\"q6:string\" xmlns:p4=\"http://www.w3.org/2001/XMLSchema-instance\">{00000000-0000-0000-0000-000000000000}</Value></Property>";

                        var webField = mAveSPWeb.SPWeb.AvailableFields.GetFieldById(sourceId, false);
                        if (webField != null && webField is IAveTaxonomyField)
                        {
                            var webTextFieldId = (webField as IAveTaxonomyField).TextField;
                            string newTextFieldProperty = textFieldProperty.Replace("00000000-0000-0000-0000-000000000000", webTextFieldId.ToString());
                            doc.DocumentElement.InnerXml = doc.DocumentElement.InnerXml.Replace(textFieldProperty, newTextFieldProperty);
                        }
                    }

                    field = FieldCollection.AddFieldAsXml(doc.OuterXml, false,
                                                 AveAddFieldOptions.AddFieldInternalNameHint |
                                                 AveAddFieldOptions.AddToNoContentType);

                    //更新xml中的termGroup和termSet等设置
                    if (doc.DocumentElement.SelectNodes("//Customization//ArrayOfProperty").Count > 0)
                    {
                        var arrayProperty =
                            doc.DocumentElement.SelectNodes("//Customization//ArrayOfProperty")[0] as XmlElement;
                        foreach (XmlElement propertyElement in arrayProperty.ChildElements())
                        {
                            if (propertyElement.Name.Equals("Property"))
                            {
                                XmlNodeList elements = propertyElement.GetElementsByTagName("Name");
                                if (elements.Count > 0)
                                {
                                    var nameElement = (XmlElement)elements[0];
                                    string name = nameElement.InnerText;
                                    elements = propertyElement.GetElementsByTagName("Value");
                                    if (elements.Count > 0)
                                    {
                                        var valueElement = (XmlElement)elements[0];
                                        if (name.Equals("GroupId"))
                                        {
                                            valueElement.InnerText = valueElement.InnerText + "|" +
                                                                     metadataInfo.TermGroup;
                                        }
                                        else if (name.Equals("TermSetId"))
                                        {
                                            valueElement.InnerText = valueElement.InnerText + "|" + metadataInfo.TermSet;
                                        }
                                        else if (name.Equals("AnchorId", StringComparison.OrdinalIgnoreCase))
                                        {
                                            valueElement.InnerText = valueElement.InnerText + "|" + metadataInfo.Terms;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    var tmpXmlField = new AveXmlField(doc.DocumentElement, (int)mAveSPWeb.SPWeb.Language);//ADO-65244
                    if (metadataInfo.IsMulti)
                    {
                        tmpXmlField.SetAllowMultipleValues(true);
                    }
                    if (AveTaxonomyField.UpdateTaxonomyFieldCommonProperties(mAveSPWeb.ParentSite, field, tmpXmlField))
                    {
                        field.Update();
                    }
                }
                catch (Exception ex)
                {
                    log.Warn("Create the custom metadata field error,Field:{0}, Exception:{1}",
                             metadataInfo.Name, ex.ToString());
                    throw;
                }
                return field;
            }
        }

        protected virtual void CacheFieldByType(AveXmlField xmlField, IAveField field)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.CacheFieldByType"))
            {
                if (xmlField.LookupNeedPostAction)
                {
                    mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.AddNotUpdateLookupField(CreateLookupObjectInfo(xmlField));
                }
            }
        }

        protected virtual AveLookupObject CreateLookupObjectInfo(AveXmlField xmlField)
        {
            return new AveLookupObject
            {
                Id = xmlField.ID,
                Type = xmlField.AveSourceType,
                WebId = mAveSPWeb.SPWeb.ID,
                SourceListId = xmlField.AveLookupListID,
                DeleteBehavior = xmlField.RelationshipDeleteBehavior,
                Sealed = xmlField.Sealed,
            };
        }


        #region Find Field
        internal IAveField Find(AveXmlField xmlField, FieldFindOption findOption)
        {
            IAveField field = FindField(xmlField, findOption);
            if (null != field && restoredFieldIdMapping.ContainsKey(field.ID))
            {
                //如果这个restoredFieldIdMapping中的value里包含源端的fieldid那么说明这个field在此次job中已经还原过，若是不包含，那么就说明这个field在此次job中没有还原过，应该对其还原。
                if (restoredFieldIdMapping.Values.Contains(xmlField.ID))
                {
                    isFieldRestored = true;
                }
                else
                {
                    field = null;
                }
            }
            return field;
        }

        internal IAveField Find(AveXmlField xmlField, IEnumerable<FieldFindOption> findOptions)
        {
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.Find"))
            {
                return findOptions.
                    Select(findOption => Find(xmlField, findOption))
                    .FirstOrDefault(field => null != field);
            }
        }

        protected virtual IAveField FindField(AveXmlField xmlField, FieldFindOption findOption)
        {
            return FindField(FieldCollection, xmlField, findOption);
        }

        private IAveField FindField(IAveFieldCollection fields, AveXmlField xmlField, FieldFindOption findOption)
        {
            var schemaMapping = new List<IAveFieldMapping> { FieldMapping };
            return FindField(fields, xmlField, findOption, schemaMapping);
        }

        protected IAveField FindField(IAveFieldCollection fields, AveXmlField xmlField, FieldFindOption findOption, List<IAveFieldMapping> schemaIdMapping)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.FindField"))
            {
                IAveField field = null;
                try
                {
                    switch (findOption)
                    {
                        case FieldFindOption.Id:
                            field = AveFieldHelper.FindFieldById(new Guid(xmlField.XmlElement.GetAttribute("ID")), fields);
                            try
                            {
                                if (field != null && !field.TypeAsString.Equals(xmlField.TypeAsString, StringComparison.OrdinalIgnoreCase) && restoreOption.CheckFieldTypeWhenSameId)
                                {
                                    log.Warn("There is a column named '{0}' in the destination having the SAME ID with the source column '{1}', but the column type is Different.", field.Title, xmlField.Title);
                                    field = null;
                                }
                            }
                            catch (Exception e)
                            {
                                log.Warn("An error occurred when finding column by Id: {0}", e.Message);
                            }
                            break;
                        case FieldFindOption.InternalName:
                            field = AveFieldHelper.FindFieldByName(xmlField.FieldInternalName, xmlField.TypeAsString, fields, findOption);
                            break;
                        case FieldFindOption.Schema:
                            field = AveFieldHelper.FindFieldBySchema(new Guid(xmlField.XmlElement.GetAttribute("ID")), fields, schemaIdMapping);
                            break;
                        case FieldFindOption.StaticName:
                            field = AveFieldHelper.FindFieldByName(xmlField.XmlElement.GetAttribute("StaticName"), xmlField.TypeAsString, fields, findOption);
                            break;
                        case FieldFindOption.DisplayName:
                            field = AveFieldHelper.FindFieldByDisplayName(xmlField.XmlElement.GetAttribute("DisplayName"), fields, xmlField);
                            break;
                        case FieldFindOption.CustomMapping:
                            if (xmlField.CustomFieldInfo != null)
                            {
                                field = AveFieldHelper.FindFieldByCustomMapping(xmlField, fields);
                            }
                            break;
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.FindFieldByObjectError, findOption.ToString(), e);
                }
                return field;
            }
        }
        #endregion

        #region Compare fields

        /// <summary>
        /// When the field is conflict with the destination, return false
        /// </summary>
        /// <param name="xmlField"></param>
        /// <param name="spField"></param>
        /// <returns>true, when not conflict</returns>
        private bool Compare(AveXmlField xmlField, IAveField spField)
        {

            using (new AvePerformanceScope("Restore.AveSPFieldCollection.Compare"))
            {

                try
                {
                    //对于需要Mapping的属性，在compare之前先全部处理完，防止compare的时候在compare这类属性之前就返回false而导致xmlField中该类属性没有赋值
                    SetMappingValuesForXmlFieldBeforeCompare(spField, xmlField);

                    if (!CompareBaseField(spField, xmlField))
                    {
                        return false;
                    }

                    switch (spField.Type)
                    {
                        case AveFieldType.Lookup:
                            //case AveFieldType.Facilities:
                            if (!CompareLookupField(spField as IAveFieldLookup, xmlField))
                            {
                                return false;
                            }
                            break;
                        case AveFieldType.User:
                            //case AveFieldType.CallTo:
                            //case AveFieldType.SendTo:
                            if (!CompareUserField(spField as IAveFieldUser, xmlField))
                            {
                                return false;
                            }
                            break;
                        case AveFieldType.DateTime:
                            //case AveFieldType.From:
                            //case AveFieldType.DueDate:
                            //case AveFieldType.CallTime:
                            //case AveFieldType.Until:
                            if (!CompareDateTimeField(spField as IAveFieldDateTime, xmlField))
                            {
                                return false;
                            }
                            break;
                        case AveFieldType.Boolean:
                        //case AveFieldType.WhatsNew:
                        //case AveFieldType.Confidential:
                        case AveFieldType.AllDayEvent:
                            //case AveFieldType.AllowEditing:
                            if (!CompareBoolField(spField as IAveFieldBoolean, xmlField))
                            {
                                return false;
                            }
                            break;
                        case AveFieldType.Choice:
                        //case AveFieldType.ContactInfo:
                        //case AveFieldType.Whereabout:
                        case AveFieldType.WorkflowStatus:
                        case AveFieldType.OutcomeChoice:
                            if (!CompareChoiceField(spField as IAveFieldChoice, xmlField))
                            {
                                return false;
                            }
                            break;
                        case AveFieldType.MultiChoice:
                            if (!CompareMultiChocieField(spField as IAveFieldMultiChoice, xmlField))
                            {
                                return false;
                            }
                            break;
                        case AveFieldType.Calculated:
                            if (!CompareCalculatedField(spField as IAveFieldCalculated, xmlField))
                            {
                                return false;
                            }
                            break;
                        case AveFieldType.Computed:
                            if (!CompareComputed(spField as IAveFieldComputed, xmlField))
                            {
                                return false;
                            }
                            break;
                        case AveFieldType.Currency:
                            if (!CompareCurrency(spField as IAveFieldCurrency, xmlField))
                            {
                                return false;
                            }
                            break;
                        case AveFieldType.Number:
                        case AveFieldType.Integer:
                        case AveFieldType.WorkflowEventType:
                            if (!CompareNumberField(spField as IAveFieldNumber, xmlField))
                            {
                                return false;
                            }
                            break;
                        case AveFieldType.Note:
                            if (!CompareNoteField(spField as IAveFieldMultiLineText, xmlField))
                            {
                                return false;
                            }
                            break;
                        case AveFieldType.GridChoice:
                            if (!CompareGridField(spField as IAveFieldRatingScale, xmlField))
                            {
                                return false;
                            }
                            break;
                        case AveFieldType.Text:
                            //case AveFieldType.Confirmations:
                            if (!CompareTextField(spField as IAveFieldText, xmlField))
                            {
                                return false;
                            }
                            break;
                        case AveFieldType.URL:
                            if (!CompareUrlField(spField as IAveFieldUrl, xmlField))
                            {
                                return false;
                            }
                            break;
                        case AveFieldType.Invalid:
                            if (spField.TypeAsString.Equals("Facilities", StringComparison.OrdinalIgnoreCase))
                            {
                                //can to do something
                                break;
                            }
                            if (spField.BaseTypeString.Equals("TaxonomyFieldType", StringComparison.OrdinalIgnoreCase) ||
                                spField.BaseTypeString.Equals("TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase))
                            {
                                //10 模拟不支持metadata column
                                if (mAveParentSite.SPSite.APIType == AveAPIType.BPOS_S && mAveParentSite.SPSite.SPVersion.StartsWith("14.", StringComparison.OrdinalIgnoreCase))
                                {
                                    return true;
                                }
                                if (!CompareTaxonomyField(spField as IAveTaxonomyField, xmlField))
                                {
                                    return false;
                                }
                                break;
                            }
                            if (spField.BaseTypeString.Equals("Lookup", StringComparison.OrdinalIgnoreCase) ||
                                spField.BaseTypeString.Equals("LookupMulti", StringComparison.OrdinalIgnoreCase))
                            {
                                if (!CompareLookupField(spField as IAveFieldLookup, xmlField))
                                {
                                    return false;
                                }
                                break;
                            }
                            if (!CompareInvalidField(spField, xmlField))
                            {
                                return false;
                            }
                            break;
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while compare field. filed id:{0}, field title:{1}\n error message:{2}", spField.ID, spField.Title, e));
                }
                return true;

            }

        }

        private void SetMappingValuesForXmlFieldBeforeCompare(IAveField field, AveXmlField xmlField)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.SetMappingValuesForXmlFieldBeforeCompare"))
            {
                if (xmlField.ShowInVersionHistory == null)
                {
                    xmlField.ShowInVersionHistory = GetShowInVersionHistoryDefaultValue(field, xmlField);
                }
                if (xmlField.Type == AveFieldType.Lookup)
                {
                    UpdateLookupFieldInfo(xmlField, false);
                }
                if (xmlField.Type == AveFieldType.Calculated)
                {
                    UpdateCalculatedFieldInfo(xmlField);
                }

                UpdateChoiceFieldInfo(xmlField);
            }
        }

        private void UpdateLookupFieldInfo(AveXmlField xmlField, bool updateXml)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.UpdateLookupFieldInfo"))
            {
                Guid webId = mAveSPWeb.SPWeb.ID;
                xmlField.LookupWebFound = true;
                if (!string.IsNullOrEmpty(xmlField.AveLookupWebTitle))
                {
                    if (mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.WebUrlMapping.ContainsKey(xmlField.AveLookupWebTitle))
                    {
                        webId = mAveSPWeb.ParentSite.GetWeb(mAveSPWeb.QueryService, mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.WebUrlMapping[xmlField.AveLookupWebTitle]);
                    }
                    else if (mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.SiteUrlMapping.ContainsKey(xmlField.AveLookupWebTitle))
                    {
                        string weburl = AveReplaceProcessor.UrlReplace(xmlField.AveLookupWebTitle, mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.SiteUrlMapping, new ReplaceOption(true), mAveSPWeb.ParentSite.SourceSiteInfo, mAveSPWeb.ParentSite.ServerRelativeUrl);
                        webId = mAveSPWeb.ParentSite.GetWeb(mAveSPWeb.QueryService, weburl);
                        log.Debug("Use the rootWeb as the lookup web. source Url:{0} , replacing url:{1}", xmlField.AveLookupWebTitle, weburl);
                    }
                    //如果在weburl在WebUrlMapping里没有找到，说明被lookup的list所在web不在当前web且没有还原。 这时不应直接把当前web id直接覆盖给目的端column。
                    //在CI中用LookupWebFound控制，False时不覆盖目的端LookupWebID属性。 Merge时请酌情考虑。
                    else
                    {
                        log.Warn("lookup web had not restore. do not overwirte LookupWebId property. Source WebUrl: {0}, Field Name: {0}", xmlField.AveLookupWebTitle, xmlField.SourceTitle);
                        xmlField.LookupWebFound = false;
                    }
                }
                xmlField.LookupWebId = webId.ToString();
                if (updateXml)
                {
                    xmlField.XmlElement.SetAttribute("WebId", xmlField.LookupWebId);
                }

                if (!String.IsNullOrEmpty(xmlField.AveLookupListTitle))
                {
                    Guid srcMappedListId;
                    Debug.Assert(!string.IsNullOrEmpty(xmlField.AveLookupListID));
                    var srcListId = new Guid(xmlField.AveLookupListID);
                    //var listIdMapping = mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.ListIdMapping; 多线程，锁在mappingmanager 内部，重新复制变量会使锁失效
                    var value = Guid.Empty;
                    if (mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.GetValueFromListIdMapping(srcListId, out value))
                    {
                        srcMappedListId = value;
                    }
                    else
                    {
                        srcMappedListId = mAveSPWeb.ParentSite.GetList(webId, xmlField.AveLookupListTitle);
                    }
                    xmlField.LookupList = srcMappedListId.ToString("B");
                    xmlField.LookupNeedPostAction = srcMappedListId == Guid.Empty;//If not find list by id or title, srcMappedListId will set to Guid.Empty. then need post action
                    if (updateXml)
                    {
                        if (xmlField.LookupNeedPostAction)
                        {
                            xmlField.XmlElement.RemoveAttribute("List");
                            if (xmlField.XmlElement.HasAttribute("RelationshipDeleteBehavior"))
                            {
                                xmlField.XmlElement.SetAttribute("RelationshipDeleteBehavior", "None");
                            }
                            if (xmlField.XmlElement.HasAttribute("IsRelationship"))
                            {
                                xmlField.XmlElement.SetAttribute("IsRelationship", "False");
                            }
                            if (xmlField.XmlElement.HasAttribute("Sealed"))
                            {
                                //ADO-186449 Sealed为true时需要早post action中更新，否则lookup list 等信息在post action时会更新不上
                                xmlField.XmlElement.SetAttribute("Sealed", "False");
                            }
                        }
                        else
                        {
                            xmlField.XmlElement.SetAttribute("List", xmlField.LookupList);
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(xmlField.LookupList))
                {
                    if (xmlField.LookupList.Equals("Self", StringComparison.OrdinalIgnoreCase))
                    {
                        if (mAveSPList != null && mAveSPList.SPList != null)
                        {
                            xmlField.LookupList = mAveSPList.SPList.ID.ToString("B");
                        }
                        else
                        {
                            xmlField.LookupList = string.Empty;
                        }
                    }
                    else if (xmlField.LookupList.Equals("Docs", StringComparison.OrdinalIgnoreCase))
                    {
                        xmlField.LookupList = string.Empty;
                    }
                    else if (xmlField.LookupList.Equals("AppPrincipals", StringComparison.OrdinalIgnoreCase))
                    {
                        xmlField.LookupList = string.Empty;
                    }
                    else if (xmlField.LookupList.Equals("UserInfo", StringComparison.OrdinalIgnoreCase))
                    {
                        xmlField.LookupList = mAveSPWeb.SPWeb.SiteUserInfoList.ID.ToString("B");
                    }
                    if (updateXml)
                    {
                        xmlField.XmlElement.SetAttribute("List", xmlField.LookupList);
                    }
                }
                if (!String.IsNullOrEmpty(xmlField.PrimaryFieldId))
                {//replace the FieldRef ID
                    string value;
                    if (AveTypeHelper.IsGuid(xmlField.PrimaryFieldId))
                    {
                        Guid mappingValue = FieldMapping.GetMappingRestoredFieldId(new Guid(xmlField.PrimaryFieldId));
                        Debug.Assert(mappingValue != Guid.Empty, "the dependent field not restore.");
                        value = mappingValue.ToString();

                        if (!String.IsNullOrEmpty(value) && !value.Equals(xmlField.PrimaryFieldId, StringComparison.OrdinalIgnoreCase))
                        {
                            xmlField.PrimaryFieldId = value;
                            if (updateXml)
                            {
                                xmlField.XmlElement.SetAttribute("FieldRef", value);
                            }
                        }
                    }
                }
            }
        }

        private bool CompareBaseField(IAveField field, AveXmlField xmlField)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.CompareBaseField"))
            {
                if (!xmlField.Title.Equals(field.Title, StringComparison.Ordinal))
                {
                    if (xmlField.CustomFieldInfo == null || xmlField.CustomFieldInfo.UseInternalOrDisplay)
                    {
                        //如果有custommapping，并且用internal mapping 同type不判断title(display name)。
                        //现在使用custommapping就不走别的find逻辑，如果以后有修改的话，这个判断就不足了，必须确定是custommappingfind并且用display mapping上的才不判断title
                        return false;
                    }
                }
                if (field.StaticName != xmlField.StaticName)
                {
                    return false;
                }
                if (field.Type != xmlField.Type)
                {
                    return false;
                }
                if (field.TypeAsString != xmlField.TypeAsString)
                {
                    return false;
                }
                if (field.NoCrawl != xmlField.NoCrawl)
                {
                    return false;
                }
                if (field.AggregationFunction != xmlField.AggregationFunction)
                {
                    return false;
                }
                if (field.AllowDeletion != xmlField.AllowDeletion)
                {
                    return false;
                }
                if (field.DefaultFormula != xmlField.DefaultFormula)
                {
                    return false;
                }
                if (field.DefaultValue != xmlField.DefaultValue)
                {
                    return false;
                }
                //wrapper不再对仅有Description更改的Field提供支持。
                //if (field.Description != xmlField.Description)
                //{
                //    return false;
                //}
                //indexed是在list的Metadata navigation settings中配置导致的，不属于field定义范围内，在此不用其进行比较
                //if (field.Indexed != xmlField.Indexed)
                //{
                //    return false;
                //}
                //if (field.LinkToItemAllowed != xmlField.LinkToItemAllowed)
                //{
                //    return false;
                //}
                if (field.Direction != xmlField.Direction)
                {
                    return false;
                }
                if (field.DisplaySize != xmlField.DisplaySize)
                {
                    return false;
                }
                if (field.ValidationFormula != xmlField.ValidationFormula)
                {
                    return false;
                }
                if (field.ValidationMessage != xmlField.ValidationMessage)
                {
                    return false;
                }
                if (!(field is IAveFieldNumber))
                {
                    if (field.IMEMode != xmlField.IMEMode)
                    {
                        return false;
                    }
                }
                if (field.Hidden != xmlField.Hidden)
                {
                    return false;
                }
                if (!field.Group.Equals(xmlField.Group, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                if (field.JumpToField != xmlField.JumpToField)
                {
                    return false;
                }
                if (field.LinkToItem != xmlField.LinkToItem)
                {
                    return false;
                }
                if (field.PIAttribute != xmlField.PIAttribute)
                {
                    return false;
                }
                if (field.PITarget != xmlField.PITarget)
                {
                    return false;
                }
                if (field.PrimaryPIAttribute != xmlField.PrimaryPIAttribute)
                {
                    return false;
                }
                if (field.PrimaryPITarget != xmlField.PrimaryPITarget)
                {
                    return false;
                }
                if (field.ReadOnlyField != xmlField.ReadOnlyField)
                {
                    return false;
                }
                if (field.RelatedField != xmlField.RelatedField)
                {
                    return false;
                }
                if (field.Required != xmlField.Required)
                {
                    return false;
                }
                if (field.Sealed != xmlField.Sealed)
                {
                    return false;
                }
                if (field.ShowInDisplayForm != xmlField.ShowInDisplayForm)
                {
                    return false;
                }
                if (field.ShowInEditForm != xmlField.ShowInEditForm)
                {
                    return false;
                }
                if (field.ShowInListSettings != xmlField.ShowInListSettings)
                {
                    return false;
                }
                if (field.ShowInNewForm != xmlField.ShowInNewForm)
                {
                    return false;
                }

                if (field.ShowInVersionHistory != xmlField.ShowInVersionHistory.Value)
                {
                    return false;
                }
                if (field.ShowInViewForms != xmlField.ShowInViewForms)
                {
                    return false;
                }
                if (field.TranslationXml != xmlField.TranslationXml)
                {
                    return false;
                }
                if (field.EnforceUniqueValues != xmlField.EnforceUniqueValues)
                {
                    return false;
                }
                if (field.CalloutMenu != xmlField.CalloutMenu)
                {
                    return false;
                }
                if (!string.IsNullOrEmpty(xmlField.JSLink) && field.JSLink != xmlField.JSLink)
                {
                    return false;
                }
                if (!field.TitleResource.CompareUserResource(mAveSPWeb.SPWeb, xmlField.TitleResource))
                {
                    return false;
                }
                if ((xmlField.FromBaseType == true) && (field.FromBaseType != xmlField.FromBaseType))
                {
                    return false;
                }
                return true;
            }
        }

        private bool CompareLookupField(IAveFieldLookup lookupField, AveXmlField xmlField)
        {
            if (lookupField.IsRelationship != xmlField.IsRelationship)
            {
                return false;
            }
            if (lookupField.AllowMultipleValues && lookupField.PrependId != xmlField.PrependId)
            {
                return false;
            }
            if (string.IsNullOrEmpty(lookupField.PrimaryFieldId) && AveTypeHelper.IsGuid(xmlField.PrimaryFieldId))
            {
                return false;
            }
            if (lookupField.RelationshipDeleteBehavior != xmlField.RelationshipDeleteBehavior)
            {
                return false;
            }
            if (lookupField.UnlimitedLengthInDocumentLibrary != xmlField.UnlimitedLengthInDocumentLibrary)
            {
                return false;
            }
            if (lookupField.AllowMultipleValues != xmlField.AllowMultipleValues)
            {
                return false;
            }
            if (lookupField.CountRelated != xmlField.CountRelated)
            {
                return false;
            }
            if (xmlField.LookupField != null && !xmlField.LookupField.Equals(lookupField.LookupField, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (xmlField.Type == AveFieldType.Lookup)
            {
                if (lookupField.LookupWebId != new Guid(xmlField.LookupWebId))
                {
                    return false;
                }
                if (AveTypeHelper.IsGuid(lookupField.LookupList) && AveTypeHelper.IsGuid(xmlField.LookupList))
                {
                    //SP13中有些field的lookupList属性不包含{}的，导致id一样的判断为冲突，在此转化为Guid再进行比较
                    if (new Guid(lookupField.LookupList) != new Guid(xmlField.LookupList))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(xmlField.LookupList) && lookupField.LookupList != xmlField.LookupList)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool CompareUserField(IAveFieldUser userField, AveXmlField xmlField)
        {
            if (!CompareLookupField(userField, xmlField))
            {
                return false;
            }
            if (userField.AllowDisplay != xmlField.AllowDisplay)
            {
                return false;
            }
            if (userField.Presence != xmlField.Presence)
            {
                return false;
            }
            if (userField.SelectionMode != xmlField.SelectionMode)
            {
                return false;
            }
            if (IsUserFieldSelectionGroupConflict(userField, xmlField))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 通过源端Group ID查找目的端对应的Group ID,并对查找结果与目的端field的Selection Group进行比较
        /// </summary>
        /// <param name="userField"></param>
        /// <param name="xmlField"></param>
        /// <returns></returns>
        private bool IsUserFieldSelectionGroupConflict(IAveFieldUser userField, AveXmlField xmlField)
        {
            if (xmlField.SelectionGroup == 0)
            {
                if (userField.SelectionGroup == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            else
            {
                if (userField.SelectionGroup == 0)
                {
                    return true;
                }
                else
                {
                    int destinationGroupId;
                    if (!groupIdMapping.TryGetValue(xmlField.SelectionGroup, out destinationGroupId))
                    {
                        destinationGroupId = mAveSPWeb.ParentSite.SPMembers.FindMemberId(xmlField.SelectionGroup, false, false);
                        //该Mapping仅仅对于Skip item when Conflict生效，暂时没有发现别的option需要用到该Dic
                        groupIdMapping[xmlField.SelectionGroup] = destinationGroupId;
                    }
                    //源端目的端不等 认为冲突
                    return destinationGroupId != userField.SelectionGroup;
                }
            }
        }


        private bool CompareDateTimeField(IAveFieldDateTime timeField, AveXmlField xmlField)
        {
            if (timeField.CalendarType != xmlField.CalendarType)
            {
                return false;
            }
            if (timeField.DisplayFormat != xmlField.DisplayFormat)
            {
                return false;
            }
            if (timeField.FriendlyDisplayFormat != xmlField.FriendlyDisplayFormat)
            {
                return false;
            }
            return true;
        }

        private bool CompareBoolField(IAveFieldBoolean boolField, AveXmlField xmlField)
        {
            if (boolField.JumpToNoField != xmlField.JumpToNoField)
            {
                return false;
            }
            if (boolField.JumpToYesField != xmlField.JumpToYesField)
            {
                return false;
            }
            return true;
        }

        private bool CompareChoiceField(IAveFieldChoice choiceField, AveXmlField xmlField)
        {
            //if (choiceField.FillinChoiceJumpTo != xmlField.FillinChoiceJumpTo)
            //{
            //    return false;
            //}
            if (choiceField.EditFormat != xmlField.EditFormat)
            {
                return false;
            }
            if (!CompareMultiChocieField(choiceField, xmlField))
            {
                return false;
            }
            return true;
        }

        private bool CompareMultiChocieField(IAveFieldMultiChoice field, AveXmlField xmlField)
        {
            if (!CompareChocies(field, xmlField.Choices))
            {
                return false;
            }
            if (field.FillInChoice != xmlField.FillInChoice)
            {
                return false;
            }
            return true;
        }

        private bool CompareChocies(IAveFieldMultiChoice field, StringCollection choices)
        {
            return choices.Cast<string>().Select(GetResourceString).All(ch => field.Choices.Contains(ch));
        }

        private bool CompareCalculatedField(IAveFieldCalculated calField, AveXmlField xmlField)
        {
            //if (calField.CurrencyLocaleId != localeId)
            //{
            //    return false;
            //}
            //CSOM doesn't have this property
            if (mAveParentSite.ObjectModelFactory.ContextKind != AveContextKind.ClientObjectModel)
            {
                if (calField.DateFormat != xmlField.DateFormat)
                {
                    return false;
                }
                if (calField.DisplayFormat != xmlField.DisplayFormatCalculated)
                {
                    return false;
                }
                if (calField.ShowAsPercentage != xmlField.ShowAsPercentage)
                {
                    return false;
                }
            }
            if (calField.Formula != xmlField.Formula)
            {
                return false;
            }
            if (calField.OutputType != xmlField.OutputType)
            {
                return false;
            }
            if (calField.FieldRefsXml != xmlField.FieldRefXml)
            {
                return false;
            }

            return true;
        }

        private bool CompareComputed(IAveFieldComputed computedField, AveXmlField xmlField)
        {
            return computedField.EnableLookup == xmlField.EnableLookup;
        }

        private bool CompareCurrency(IAveFieldCurrency currencyField, AveXmlField xmlField)
        {
            if (currencyField.CurrencyLocaleId != xmlField.CurrencyLocaleId)
            {
                return false;
            }
            if (!CompareNumberField(currencyField, xmlField))
            {
                return false;
            }
            return true;
        }

        private bool CompareNumberField(IAveFieldNumber numberField, AveXmlField xmlField)
        {
            if (numberField.DisplayFormat != xmlField.DisplayFormatNumber)
            {
                return false;
            }
            if (numberField.MaximumValue != xmlField.MaximumValue)
            {
                return false;
            }
            if (numberField.MinimumValue != xmlField.MinimumValue)
            {
                return false;
            }
            if (numberField.DefaultValue != xmlField.DefaultValue)
            {
                return false;
            }
            if (numberField.ShowAsPercentage != xmlField.ShowAsPercentage)
            {
                return false;
            }
            return true;
        }

        private bool CompareNoteField(IAveFieldMultiLineText mulTextField, AveXmlField xmlField)
        {
            if (mulTextField.AllowHyperlink != xmlField.AllowHyperlink)
            {
                return false;
            }
            if (mulTextField.AppendOnly != xmlField.AppendOnly)
            {
                return false;
            }
            if (mulTextField.DifferencingLimit != xmlField.DifferencingLimit)
            {
                return false;
            }
            if (mulTextField.IsolateStyles != xmlField.IsolateStyles)
            {
                return false;
            }
            if (mulTextField.NumberOfLines != xmlField.NumberOfLines)
            {
                return false;
            }
            //mulTextField.RestrictedMode = xmlField.RestrictedMode;
            if (mulTextField.RichText != xmlField.RichText)
            {
                return false;
            }
            if (mulTextField.RichTextMode != xmlField.RichTextMode)
            {
                return false;
            }
            if (mulTextField.UnlimitedLengthInDocumentLibrary != xmlField.UnlimitedLengthInDocumentLibrary)
            {
                return false;
            }
            return true;
        }

        private bool CompareGridField(IAveFieldRatingScale gridField, AveXmlField xmlField)
        {
            if (gridField.GridEndNumber != xmlField.GridEndNumber)
            {
                return false;
            }
            if (gridField.GridNAOptionText != xmlField.GridNAOptionText)
            {
                return false;
            }
            if (gridField.GridTextRangeAverage != xmlField.GridTextRangeAverage)
            {
                return false;
            }
            if (gridField.GridTextRangeHigh != xmlField.GridTextRangeHigh)
            {
                return false;
            }
            if (gridField.GridTextRangeLow != xmlField.GridTextRangeLow)
            {
                return false;
            }
            if (!CompareMultiChocieField(gridField, xmlField))
            {
                return false;
            }
            return true;
        }

        private bool CompareTextField(IAveFieldText textField, AveXmlField xmlField)
        {
            if (textField.MaxLength != xmlField.MaxLength)
            {
                return false;
            }
            if (textField.DifferencingLimit != xmlField.DifferencingLimit)
            {
                return false;
            }
            return true;
        }

        private bool CompareUrlField(IAveFieldUrl urlField, AveXmlField xmlField)
        {
            if (urlField.DisplayFormat != xmlField.DisplayFormatUrl)
            {
                return false;
            }
            return true;
        }

        private bool CompareTaxonomyField(IAveTaxonomyField taxField, AveXmlField xmlField)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.CompareTaxonomyField"))
            {
                AveMetadataService metadataService = mAveParentSite.MetadataService;
                string termStoreIdWithName = xmlField.GetCustomerProperty("SspId").ToString();
                Guid termStoreId = new Guid(termStoreIdWithName.Split('|')[0]);
                Guid destTermStoreId = Guid.Empty;
                if (termStoreId == Guid.Empty || metadataService == null || !metadataService.TermStoreIdMapping.TryGetValue(termStoreId, out destTermStoreId))
                {
                    destTermStoreId = termStoreId;
                }
                if (taxField.SspId != destTermStoreId)
                {
                    return false;
                }
                string termSetIDWithName = xmlField.GetCustomerProperty("TermSetId").ToString();
                string termSetId = termSetIDWithName.Split('|')[0];
                if (new Guid(termSetId) != Guid.Empty && metadataService != null && metadataService.TermSetIdMapping.ContainsKey(new Guid(termSetId)))
                {
                    termSetId = mAveParentSite.MetadataService.TermSetIdMapping[new Guid(termSetId)].ToString();
                }
                if (!taxField.TermSetId.ToString().Equals(termSetId, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                string anchorIdWithName = xmlField.GetCustomerProperty("AnchorId").ToString();
                string anchorId = anchorIdWithName.Split('|')[0];
                if (new Guid(anchorId) != Guid.Empty && metadataService != null && metadataService.TermIdMapping.ContainsKey(new Guid(anchorId)))
                {
                    anchorId = metadataService.TermIdMapping[new Guid(anchorId)].ToString();
                }
                if (!taxField.AnchorId.ToString().Equals(anchorId, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                bool isPathRenderedSource = xmlField.GetCustomerProperty("IsPathRendered") != null ?
                    (bool)xmlField.GetCustomerProperty("IsPathRendered") : false;
                if (taxField.IsPathRendered != isPathRenderedSource)
                {
                    return false;
                }
                return true;
            }
        }

        private bool CompareInvalidField(IAveField field, AveXmlField xmlField)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.CompareInvalidField"))
            {
                var fieldElement = field.Node as XmlElement;
                //client api doesn't have Node property
                if (fieldElement != null)
                {
                    if (!CompareAttributes(fieldElement, xmlField.XmlElement, mBaseFieldAttributes))
                    {
                        return false;
                    }
                    if (!CompareElements(fieldElement, xmlField.XmlElement, mBaseFieldNodes))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        #region Compare fields.Tool methods
        private bool CompareElements(XmlElement fieldElement, XmlElement xmlElement, List<string> ignoreNodes)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.CompareElements"))
            {
                var fieldNodes = fieldElement.ChildNodes.Cast<XmlElement>().Where(node => !ignoreNodes.Contains(node.Name)).ToList();
                var xmlNodes = xmlElement.ChildNodes.Cast<XmlElement>().Where(node => !ignoreNodes.Contains(node.Name)).ToList();
                if (fieldNodes.Count != xmlNodes.Count)
                {
                    return false;
                }
                Comparison<XmlElement> comparer = (x, y) => string.CompareOrdinal(x.Name, y.Name);
                fieldNodes.Sort(comparer);
                xmlNodes.Sort(comparer);
                for (int i = 0; i < fieldNodes.Count; ++i)
                {
                    if (!fieldNodes[i].Name.Equals(xmlNodes[i].Name, StringComparison.Ordinal)
                        || !fieldNodes[i].InnerXml.Equals(xmlNodes[i].InnerXml, StringComparison.Ordinal)
                        || !CompareAttributes(fieldNodes[i], xmlNodes[i]))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        private bool CompareAttributes(XmlElement fieldElement, XmlElement xmlElement)
        {
            return CompareAttributes(fieldElement, xmlElement, new List<string>());
        }

        private bool CompareAttributes(XmlElement fieldElement, XmlElement xmlElement, List<string> ignoredAttributes)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.CompareAttributes"))
            {
                var fieldAttributes = fieldElement.Attributes.Cast<XmlAttribute>().Where(attribute => !ignoredAttributes.Contains(attribute.Name)).ToList();
                var xmlAttributes = xmlElement.Attributes.Cast<XmlAttribute>().Where(attribute => !ignoredAttributes.Contains(attribute.Name)).ToList();
                if (fieldAttributes.Count != xmlAttributes.Count)
                {
                    return false;
                }
                Comparison<XmlAttribute> comparer = (x, y) => string.CompareOrdinal(x.Name, y.Name);
                fieldAttributes.Sort(comparer);
                xmlAttributes.Sort(comparer);
                for (int i = 0; i < fieldAttributes.Count; ++i)
                {
                    if (!fieldAttributes[i].Name.Equals(xmlAttributes[i].Name, StringComparison.Ordinal)
                      || !fieldAttributes[i].Value.Equals(xmlAttributes[i].Value, StringComparison.Ordinal))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        private string GetResourceString(string source)
        {
            if (!source.StartsWith("$Resources", StringComparison.OrdinalIgnoreCase))
            {
                return source;
            }
            return mAveParentSite.ObjectModelFactory.Utility.GetLocalizedString(source, "core", (uint)mAveSPWeb.SPWeb.UICulture.LCID);
        }

        private bool GetShowInVersionHistoryDefaultValue(IAveField field, AveXmlField xmlField)
        {
            if (field is IAveFieldComputed)
            {
                return false;
            }
            if (field is IAveFieldAttachments)
            {
                return false;
            }
            if (field is IAveFieldCalculated || field is IAveFieldModStat)
            {
                return !xmlField.Hidden;
            }
            return (!xmlField.Hidden && !xmlField.ReadOnlyField);
        }
        #endregion


        private bool CompareEnsureField(AveXmlField xmlField, IAveField spField)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.CompareEnsureField"))
            {
                SetMappingValuesForXmlFieldBeforeCompare(spField, xmlField);
                if (spField.Type != xmlField.Type)
                {
                    return false;
                }
                //if (spField.TypeAsString != xmlField.TypeAsString)
                //{
                //    return false;
                //}
                switch (spField.Type)
                {
                    case AveFieldType.Lookup:
                        //case AveFieldType.Facilities:
                        if (!CompareLookupField(spField as IAveFieldLookup, xmlField))
                        {
                            return false;
                        }
                        break;
                    case AveFieldType.User:
                        if (!CompareUserField(spField as IAveFieldUser, xmlField))
                        {
                            return false;
                        }
                        break;
                    case AveFieldType.Choice:
                    //case AveFieldType.ContactInfo:
                    //case AveFieldType.Whereabout:
                    case AveFieldType.WorkflowStatus:
                    case AveFieldType.OutcomeChoice:
                        if (!CompareChoiceField(spField as IAveFieldChoice, xmlField))
                        {
                            return false;
                        }
                        break;
                    case AveFieldType.MultiChoice:
                        if (!CompareMultiChocieField(spField as IAveFieldMultiChoice, xmlField))
                        {
                            return false;
                        }
                        break;
                    case AveFieldType.Calculated:
                        if (!CompareCalculatedField(spField as IAveFieldCalculated, xmlField))
                        {
                            return false;
                        }
                        break;
                    case AveFieldType.GridChoice:
                        if (!CompareGridField(spField as IAveFieldRatingScale, xmlField))
                        {
                            return false;
                        }
                        break;
                }
                return true;
            }
        }
        #endregion

        #region Update Fields
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint Property")]
        private readonly List<string> mBaseFieldAttributes = new List<string>
                                                                 {
                                         "ID", "Aggregation", "AllowDeletion", //"AllowDuplicateValues",
                                         "Description", "Direction", "DisplaySize", "EcbMenuAllowed", "EcbMenu",
                                         "Group", "Hidden", "IMEMode", "Indexed", "JumpTo",
                                         "LinkToItemAllowed", "ReadOnly", "RelatedField", "Required", "Sealed",
                                         "PrimaryPITarget", "NoCrawl", "PIAttribute", "PITarget", "PrimaryPIAttribute",
                                         "ShowInDisplayForm", "ShowInEditForm", "ShowInListSettings", "ShowInNewForm",
                                         "ShowInViewForms",
                                         "StaticName", "DisplayName", "Translations", "Type", "ShowInVersionHistory",
                                         "Version", "SourceID", "AddFieldOption"
                                     };
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint Property")]
        private readonly List<string> mBaseFieldNodes = new List<string> { "DefaultFormula", "FieldRefs", "Validation" };

        public AveSPFieldCollection()
        {
            SourceTextTaxonomyDic = new Dictionary<string, string>();
        }

        private void UpdateField(IAveField spField, AveXmlField xmlField)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.UpdateField"))
            {
                try
                {
                    if (!ReferenceEquals(spField.Fields, FieldCollection))
                    { //对于通过查找parentsite上的fields所匹配到的field，sharepoint界面上就是不能编辑的，不再更新
                        return;
                    }
                    if (AveFieldHelper.UpdateFieldType(spField, xmlField, mAveSPList))
                    {
                        spField = FieldCollection.GetFieldByInternalName(spField.InternalName);
                    }

                    SetBaseField(spField, xmlField);
                    switch (spField.Type)
                    {
                        case AveFieldType.Lookup:
                            //case AveFieldType.Facilities:
                            SetLookupField(spField as IAveFieldLookup, xmlField, false);
                            break;
                        case AveFieldType.User:
                            //case AveFieldType.CallTo:
                            //case AveFieldType.SendTo:
                            SetUserField(spField as IAveFieldUser, xmlField);
                            break;
                        case AveFieldType.DateTime:
                            //case AveFieldType.From:
                            //case AveFieldType.DueDate:
                            //case AveFieldType.CallTime:
                            //case AveFieldType.Until:
                            SetDateTimeField(spField as IAveFieldDateTime, xmlField);
                            break;
                        case AveFieldType.Boolean:
                        //case AveFieldType.WhatsNew:
                        //case AveFieldType.Confidential:
                        case AveFieldType.AllDayEvent:
                            //case AveFieldType.AllowEditing:
                            SetBoolField(spField as IAveFieldBoolean, xmlField);
                            break;
                        case AveFieldType.Choice:
                        //case AveFieldType.ContactInfo:
                        //case AveFieldType.Whereabout:
                        case AveFieldType.WorkflowStatus:
                        case AveFieldType.OutcomeChoice:
                            SetChoiceField(spField as IAveFieldChoice, xmlField);
                            break;
                        case AveFieldType.MultiChoice:
                            SetMultiChocieField(spField as IAveFieldMultiChoice, xmlField);
                            break;
                        case AveFieldType.Calculated:
                            SetCalculatedField(spField as IAveFieldCalculated, xmlField);
                            break;
                        case AveFieldType.Computed:
                            var computedField = spField as IAveFieldComputed;
                            SetValue(computedField.EnableLookup, xmlField.EnableLookup, value => computedField.EnableLookup = value);
                            break;
                        case AveFieldType.Currency:
                            var currencyField = spField as IAveFieldCurrency;
                            SetValue(currencyField.CurrencyLocaleId, xmlField.CurrencyLocaleId, value => currencyField.CurrencyLocaleId = value);
                            SetNumberField(currencyField, xmlField, false);
                            break;
                        case AveFieldType.Number:
                        case AveFieldType.Integer:
                        case AveFieldType.WorkflowEventType:
                            SetNumberField(spField as IAveFieldNumber, xmlField, true);
                            break;
                        case AveFieldType.Note:
                            SetNoteField(spField as IAveFieldMultiLineText, xmlField);
                            break;
                        case AveFieldType.GridChoice:
                            SetGridField(spField as IAveFieldRatingScale, xmlField);
                            break;
                        case AveFieldType.Text:
                            //case AveFieldType.Confirmations:
                            var textField = spField as IAveFieldText;
                            SetValue(textField.XPath, xmlField.XPath, value => textField.XPath = value);
                            SetValue(textField.MaxLength, xmlField.MaxLength, value => textField.MaxLength = value);
                            SetValue(textField.DifferencingLimit, xmlField.DifferencingLimit, value => textField.DifferencingLimit = value);
                            break;
                        case AveFieldType.URL:
                            var urlField = spField as IAveFieldUrl;
                            SetValue(urlField.DisplayFormat, xmlField.DisplayFormatUrl, value => urlField.DisplayFormat = value);
                            break;
                        case AveFieldType.Invalid:
                            switch (spField.BaseTypeString)
                            {
                                case "Facilities":
                                    break;
                                case "TaxonomyFieldType":
                                case "TaxonomyFieldTypeMulti":
                                    SetTaxonomyField(xmlField, spField, false);
                                    break;
                                case "Lookup":
                                case "LookupMulti":
                                    if (!spField.TypeAsString.Equals("Facilities", StringComparison.OrdinalIgnoreCase))
                                    {
                                        SetLookupField(spField as IAveFieldLookup, xmlField, false);
                                    }
                                    break;
                                default:
                                    SetInvalidField(spField, xmlField);
                                    break;
                            }
                            break;
                    }
                    if (spField.ReadOnlyField)
                    {
                        spField.UpdateReadOnlyField();
                    }
                    else
                    {
                        spField.Update();
                    }
                    UpdateFieldFromBaseType(spField, xmlField);
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN,
                            string.Format(
                                "An error occurred while update field. filed id:{0}, field title:{1}\n error message:{2}",
                                spField.ID, spField.Title, e));
                }
            }
        }

        private void UpdateFieldFromBaseType(IAveField field, AveXmlField xmlField)
        {
            try
            {
                if (field.FromBaseType == false && xmlField.FromBaseType == true)
                {
                    log.Debug("Start change FromBaseType. SchemaXml: {0}", field.SchemaXml);
                    var doc = new XmlDocument();
                    doc.LoadXml(field.SchemaXml);
                    if (doc.DocumentElement.Attributes["FromBaseType"] != null)
                    {
                        doc.DocumentElement.Attributes["FromBaseType"].Value = "TRUE";
                    }
                    else
                    {
                        doc.DocumentElement.SetAttribute("FromBaseType", "TRUE");
                    }

                    field.SchemaXml = doc.OuterXml;
                    if (field.ReadOnlyField)
                    {
                        field.UpdateReadOnlyField();
                    }
                    else
                    {
                        field.Update();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed update field FromBaseType. FieldName: {0}, Error: {1}", field.InternalName, e);
            }
        }

        private string ResetDateTimeDefaultValue(string defaultValue)
        {
            try
            {
                if (!String.IsNullOrEmpty(defaultValue) && !defaultValue.Equals("[today]", StringComparison.OrdinalIgnoreCase))
                {
                    if (defaultValue.EndsWith("Z", StringComparison.OrdinalIgnoreCase))
                    {
                        return defaultValue;
                    }
                    return this.mAveParentSite.ObjectModelFactory.Utility.CreateISO8601DateTimeFromSystemDateTime(Convert.ToDateTime(defaultValue, DateTimeFormatInfo.InvariantInfo));
                }
            }
            catch (Exception e)
            {
                log.Warn("An error occurred while check filed default value, default value is:{0}, error:{1}", defaultValue, e.ToString());
            }
            return defaultValue;
        }

        private void SetBaseField(IAveField field, AveXmlField xmlField)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.SetBaseField"))
            {
                //如果有custommapping，并且用displayname mapping 同type也不能覆盖title。
                //现在使用custommapping就不走别的find逻辑，如果以后有修改的话，这个判断就不足了，必须确定是custommappingfind并且用display mapping上的才不覆盖title
                if (xmlField.CustomFieldInfo == null || xmlField.CustomFieldInfo.UseInternalOrDisplay)
                {
                    if (xmlField.CustomFieldInfo != null && !String.IsNullOrEmpty(xmlField.CustomFieldInfo.Name))
                    {
                        SetValue(field.Title, xmlField.CustomFieldInfo.Name, v => field.Title = v);
                    }
                    else
                    {
                        SetValue(field.Title, xmlField.Title, v => field.Title = v);
                    }
                    if (field.TitleResource.SetUserResource(mAveSPWeb.SPWeb, xmlField.TitleResource))
                    {
                        field.SetFieldAttributeValue("DisplayName", field.Title);
                    }
                }
                SetValue(field.StaticName, xmlField.StaticName, v => field.StaticName = v);
                SetValue(field.TypeAsString, xmlField.TypeAsString, v => field.TypeAsString = v);
                SetValue(field.NoCrawl, xmlField.NoCrawl, v => field.NoCrawl = v);
                SetValue(field.AggregationFunction, xmlField.AggregationFunction, v => field.AggregationFunction = v);
                SetValue(field.AllowDeletion, xmlField.AllowDeletion, v => field.AllowDeletion = v);
                SetValue(field.DefaultFormula, xmlField.DefaultFormula, v => field.DefaultFormula = v);
                if (xmlField.Type == AveFieldType.DateTime)
                {
                    xmlField.DefaultValue = ResetDateTimeDefaultValue(xmlField.DefaultValue);
                }
                //ADO-148154 DefaultFormula要想更新为null，需要更新DefaultValue才可以 否则无法更新成功
                if (string.IsNullOrEmpty(xmlField.DefaultFormula) && !string.IsNullOrEmpty(field.DefaultFormula))
                {
                    SetValueWithOutCheckValue(field.DefaultValue, xmlField.DefaultValue, v => field.DefaultValue = v);
                }
                else
                {
                    SetValue(field.DefaultValue, xmlField.DefaultValue, v => field.DefaultValue = v);
                }

                SetValue(field.Description, xmlField.Description, v => field.Description = v);
                field.DescriptionResource.SetUserResource(mAveSPWeb.SPWeb, xmlField.DescriptionResource);
                SetValue(field.Direction, xmlField.Direction, v => field.Direction = v);
                SetValue(field.DisplaySize, xmlField.DisplaySize, v => field.DisplaySize = v);
                //if (field.AllowDuplicateValues != xmlField.AllowDuplicateValues)
                //{
                //    field.AllowDuplicateValues = xmlField.AllowDuplicateValues;
                //}
                //if (field.EcbMenu != xmlField.EcbMenu)
                //{
                //    field.EcbMenu = xmlField.EcbMenu;
                //}
                //if (field.EcbMenuAllowed != xmlField.EcbMenuAllowed)
                //{
                //    field.EcbMenuAllowed = xmlField.EcbMenuAllowed;
                //}
                //if (field.LinkToItemAllowed != xmlField.LinkToItemAllowed)
                //{
                //    field.LinkToItemAllowed = xmlField.LinkToItemAllowed;
                //}
                SetValue(field.ValidationFormula, xmlField.ValidationFormula, v => field.ValidationFormula = v);
                if (field.ValidationFormula != null && field.ValidationFormula.Contains("#NAME?"))
                {
                    field.ValidationFormula = field.ValidationFormula.Replace("#NAME?", field.Title);
                }
                else if (field.ValidationFormula != null)
                {
                    field.ValidationFormula = field.ValidationFormula.Replace(xmlField.Title, field.Title);
                }
                SetValue(field.ValidationMessage, xmlField.ValidationMessage, v => field.ValidationMessage = v);
                //此处需要跳过lookup column和metadata column，它们会单独还原这个属性。这个属性由于与其他属性有关联关系，还原有先后顺序。
                if (!((field is IAveFieldLookup) || field.BaseTypeString.Equals("TaxonomyFieldType", StringComparison.OrdinalIgnoreCase) || field.BaseTypeString.Equals("TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase)))
                {
                    SetValue(field.Indexed, xmlField.Indexed, v => field.Indexed = v);
                }
                if (!(field is IAveFieldNumber))
                {
                    SetValue(field.IMEMode, xmlField.IMEMode, v => field.IMEMode = v);
                }
                SetValue(field.Hidden, xmlField.Hidden, v => field.Hidden = v);
                SetValue(field.Group, xmlField.Group, v => field.Group = v);
                SetValue(field.JumpToField, xmlField.JumpToField, v => field.JumpToField = v);
                SetValue(field.LinkToItem, xmlField.LinkToItem, v => field.LinkToItem = v);
                SetValue(field.PIAttribute, xmlField.PIAttribute, v => field.PIAttribute = v);
                SetValue(field.PITarget, xmlField.PITarget, v => field.PITarget = v);
                SetValue(field.PrimaryPIAttribute, xmlField.PrimaryPIAttribute, v => field.PrimaryPIAttribute = v);
                SetValue(field.PrimaryPITarget, xmlField.PrimaryPITarget, v => field.PrimaryPITarget = v);
                SetValue(field.ReadOnlyField, xmlField.ReadOnlyField, v => field.ReadOnlyField = v);
                SetValue(field.RelatedField, xmlField.RelatedField, v => field.RelatedField = v);
                SetValue(field.Required, xmlField.Required, v => field.Required = v);
                if (field.Sealed != xmlField.Sealed)
                {
                    if (!AveBuiltInFieldId.Contains(field.ID))
                    {
                        field.Sealed = xmlField.Sealed;
                    }
                }
                SetValue(field.ShowInDisplayForm, xmlField.ShowInDisplayForm, v => field.ShowInDisplayForm = v);
                SetValue(field.ShowInEditForm, xmlField.ShowInEditForm, v => field.ShowInEditForm = v);
                SetValue(field.Required, xmlField.Required, v => field.Required = v);
                SetValue(field.ShowInNewForm, xmlField.ShowInNewForm, v => field.ShowInNewForm = v);
                Debug.Assert(xmlField.ShowInVersionHistory != null, "xmlField.ShowInVersionHistory != null");
                SetValue(field.ShowInVersionHistory, xmlField.ShowInVersionHistory.Value, v => field.ShowInVersionHistory = v);
                SetValue(field.ShowInViewForms, xmlField.ShowInViewForms, v => field.ShowInViewForms = v);
                SetValue(field.TranslationXml, xmlField.TranslationXml, v => field.TranslationXml = v);
                SetValue(field.EnforceUniqueValues, xmlField.EnforceUniqueValues, v => field.EnforceUniqueValues = v);
                SetValue(field.CalloutMenu, xmlField.CalloutMenu, v => field.CalloutMenu = v);
                //SetValue(field.JSLink, xmlField.JSLink, v => field.JSLink = v);
                SetValueIgnoreDefaultJSLink(field.JSLink, xmlField.JSLink, v => field.JSLink = v);
            }
        }

        private void SetLookupField(IAveFieldLookup lookupField, AveXmlField xmlField, bool isUserField)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.SetLookupField"))
            {
                bool needUpdateIndexedFirst = xmlField.AllowMultipleValues & lookupField.Indexed;
                bool needUpdateMultipleFirst = xmlField.Indexed & lookupField.AllowMultipleValues;
                SetValue(lookupField.IsRelationship, xmlField.IsRelationship, v => lookupField.IsRelationship = v);
                if (!isUserField || (isUserField && !string.IsNullOrEmpty(xmlField.LookupField)))
                {
                    SetValueIgnoreNullOrEmpty(lookupField.LookupField, xmlField.LookupField, v => lookupField.LookupField = v);
                }
                //ADO-135461 Indexed和AllowMultipleValues更新需要有先后顺序
                if (needUpdateIndexedFirst)
                {
                    SetValue(lookupField.Indexed, xmlField.Indexed, v => lookupField.Indexed = v);
                    SetValue(lookupField.RelationshipDeleteBehavior, xmlField.RelationshipDeleteBehavior, v => lookupField.RelationshipDeleteBehavior = v);
                    lookupField.Update();
                    SetValue(lookupField.AllowMultipleValues, xmlField.AllowMultipleValues, v => lookupField.AllowMultipleValues = v);
                }
                else if (needUpdateMultipleFirst)
                {
                    SetValue(lookupField.AllowMultipleValues, xmlField.AllowMultipleValues, v => lookupField.AllowMultipleValues = v);
                    lookupField.Update();
                    SetValue(lookupField.Indexed, xmlField.Indexed, v => lookupField.Indexed = v);
                    SetValue(lookupField.RelationshipDeleteBehavior, xmlField.RelationshipDeleteBehavior, v => lookupField.RelationshipDeleteBehavior = v);
                }
                else
                {
                    SetValue(lookupField.AllowMultipleValues, xmlField.AllowMultipleValues, v => lookupField.AllowMultipleValues = v);

                    //10以上版本的SP，DiscussionBoard里的Author column的Index属性，必须保持默认值。
                    if (!Is10UpperDiscussionBoardAuthorColumn(lookupField))
                    {
                        SetValue(lookupField.Indexed, xmlField.Indexed, v => lookupField.Indexed = v);
                    }
                    SetValue(lookupField.RelationshipDeleteBehavior, xmlField.RelationshipDeleteBehavior, v => lookupField.RelationshipDeleteBehavior = v);
                }
                SetValue(lookupField.UnlimitedLengthInDocumentLibrary, xmlField.UnlimitedLengthInDocumentLibrary, v => lookupField.UnlimitedLengthInDocumentLibrary = v);
                //SetValue(lookupField.AllowMultipleValues, xmlField.AllowMultipleValues, v => lookupField.AllowMultipleValues = v);
                SetValue(lookupField.CountRelated, xmlField.CountRelated, v => lookupField.CountRelated = v);
                if (String.IsNullOrEmpty(lookupField.PrimaryFieldId) && AveTypeHelper.IsGuid(xmlField.PrimaryFieldId))
                {//经过Reflector查看，该属性只有在Filed本身该属性为null并且value是Guid的时候才可以赋值，否则会抛出异常
                    SetValue(lookupField.PrimaryFieldId, xmlField.PrimaryFieldId, v => lookupField.PrimaryFieldId = v);
                }
                if (xmlField.Type == AveFieldType.Lookup)
                {
                    if (xmlField.LookupWebFound)
                    {
                        SetValue(lookupField.LookupWebId, new Guid(xmlField.LookupWebId), v => lookupField.LookupWebId = v);
                        if (!String.IsNullOrEmpty(xmlField.LookupList) && new Guid(xmlField.LookupList) != Guid.Empty)
                        {
                            SetValue(lookupField.LookupList, xmlField.LookupList, v => lookupField.LookupList = v);
                        }
                    }
                }
            }
        }

        private void SetMultiChocieField(IAveFieldMultiChoice field, AveXmlField xmlField)
        {
            SetChoices(field, xmlField.Choices, restoreOption != null ? restoreOption.OverwriteChoices : false);
            SetValue(field.FillInChoice, xmlField.FillInChoice, v => field.FillInChoice = v);
        }

        private void SetChoices(IAveFieldMultiChoice field, StringCollection choices, bool isOverWriteChoices)
        {
            if (isOverWriteChoices)
            {
                field.Choices.Clear();
            }
            foreach (string ch in choices)
            {
                if (!field.Choices.Contains(GetResourceString(ch)))
                {
                    field.Choices.Add(ch);
                }
            }
        }

        /// <summary>
        /// Restore field selection group and set user field selection group value
        /// </summary>
        /// <param name="userField"></param>
        /// <param name="xmlField"></param>
        private void SetFieldSelectionGroup(IAveFieldUser userField, AveXmlField xmlField)
        {
            if (xmlField.SelectionGroup == 0)
            {
                SetValue(userField.SelectionGroup, xmlField.SelectionGroup, v => userField.SelectionGroup = v);
                return;
            }
            int desPrincipalId = mAveSPWeb.ParentSite.SPMembers.FindMemberId(xmlField.SelectionGroup, true, false);
            if (desPrincipalId != AveSPMemberInfo.FAKE_GROUP.NewId)
            {
                xmlField.SelectionGroup = desPrincipalId;
                SetValue(userField.SelectionGroup, desPrincipalId, v => userField.SelectionGroup = v);
            }
            else
            {
                log.Warn("An error occurred while restoring user field selection group, source selection group id is {0}.", xmlField.SelectionGroup);
            }
        }

        private void SetUserField(IAveFieldUser userField, AveXmlField xmlField)
        {
            SetValue(userField.AllowDisplay, xmlField.AllowDisplay, v => userField.AllowDisplay = v);
            SetValue(userField.Presence, xmlField.Presence, v => userField.Presence = v);
            SetValue(userField.SelectionMode, xmlField.SelectionMode, v => userField.SelectionMode = v);
            UpdateUserFieldInfo(xmlField);
            SetFieldSelectionGroup(userField, xmlField);
            SetLookupField(userField, xmlField, true);
        }

        private void SetDateTimeField(IAveFieldDateTime timeField, AveXmlField xmlField)
        {
            SetValue(timeField.CalendarType, xmlField.CalendarType, v => timeField.CalendarType = v);
            SetValue(timeField.DisplayFormat, xmlField.DisplayFormat, v => timeField.DisplayFormat = v);
            SetValue(timeField.FriendlyDisplayFormat, xmlField.FriendlyDisplayFormat, v => timeField.FriendlyDisplayFormat = v);
        }

        private void SetCalculatedField(IAveFieldCalculated calField, AveXmlField xmlField)
        {
            SetValue(calField.DateFormat, xmlField.DateFormat, v => calField.DateFormat = v);
            SetValue(calField.DisplayFormat, xmlField.DisplayFormatCalculated, v => calField.DisplayFormat = v);
            SetValue(calField.Formula, xmlField.Formula, v => calField.Formula = v);
            SetValue(calField.OutputType, xmlField.OutputType, v => calField.OutputType = v);
            SetValue(calField.ShowAsPercentage, xmlField.ShowAsPercentage, v => calField.ShowAsPercentage = v);
            SetValue(calField.FieldRefsXml, xmlField.FieldRefXml, v => calField.FieldRefsXml = v);
        }

        private void SetNumberField(IAveFieldNumber numberField, AveXmlField xmlField, bool checkPercentage)
        {
            if (numberField.MinimumValue > xmlField.MaximumValue || numberField.MaximumValue < xmlField.MinimumValue)
            {
                numberField.MinimumValue = double.MinValue;
                numberField.MaximumValue = double.MaxValue;
            }
            SetValue(numberField.DisplayFormat, xmlField.DisplayFormatNumber, v => numberField.DisplayFormat = v);
            SetValue(numberField.MaximumValue, xmlField.MaximumValue, v => numberField.MaximumValue = v);
            SetValue(numberField.MinimumValue, xmlField.MinimumValue, v => numberField.MinimumValue = v);
            SetValue(numberField.DefaultValue, xmlField.DefaultValue, v => numberField.DefaultValue = v);
            if (checkPercentage)
            {
                SetValue(numberField.ShowAsPercentage, xmlField.ShowAsPercentage, v => numberField.ShowAsPercentage = v);
            }
        }

        private void SetBoolField(IAveFieldBoolean boolField, AveXmlField xmlField)
        {
            SetValue(boolField.JumpToNoField, xmlField.JumpToNoField, v => boolField.JumpToNoField = v);
            SetValue(boolField.JumpToYesField, xmlField.JumpToYesField, v => boolField.JumpToYesField = v);
        }

        private void SetInvalidField(IAveField field, AveXmlField xmlField)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.SetInvalidField"))
            {
                var fieldElement = field.Node as XmlElement;
                if (fieldElement != null)
                {
                    SetAttributes(fieldElement, xmlField.XmlElement, mBaseFieldAttributes);
                    SetElements(fieldElement, xmlField.XmlElement, mBaseFieldNodes);
                }
            }
        }

        private void SetTaxonomyField(AveXmlField xmlField, IAveField field, bool needUpdate)
        {
            if (field.BaseTypeString == "TaxonomyFieldType" || field.BaseTypeString == "TaxonomyFieldTypeMulti")
            {
                if (AveEnv.IsMoss || (mAveParentSite.SPSite.APIType == AveAPIType.BPOS_S && !mAveParentSite.SPSite.SPVersion.StartsWith("14", StringComparison.Ordinal)))
                {
                    bool isChanged = AveTaxonomyField.UpdateTaxonomyFieldCommonProperties(mAveSPWeb.ParentSite, field, xmlField);
                    if (needUpdate && isChanged)
                    {
                        field.Update();
                    }

                    if (needUpdate && string.IsNullOrEmpty(field.DefaultValue) && !string.IsNullOrEmpty(xmlField.DefaultValue))
                    {//在TermSet信息没有更新正确的情况下，DefualtValue的WssId会获取不到，因此需要在更新了TermSet信息之后重新更新一次DefaultValue
                        if (AveTaxonomyField.UpdateTaxonomyFieldCommonProperties(mAveSPWeb.ParentSite, field, xmlField))
                        {
                            field.Update();
                        }
                    }
                }
            }
        }

        private void SetChoiceField(IAveFieldChoice choiceField, AveXmlField xmlField)
        {
            //if (choiceField.FillinChoiceJumpTo != xmlField.FillinChoiceJumpTo)
            //{
            //    choiceField.FillinChoiceJumpTo = xmlField.FillinChoiceJumpTo;
            //    needUpdate = true;
            //}
            SetValue(choiceField.EditFormat, xmlField.EditFormat, v => choiceField.EditFormat = v);
            SetMultiChocieField(choiceField, xmlField);
        }

        private void SetNoteField(IAveFieldMultiLineText mulTextField, AveXmlField xmlField)
        {
            SetValue(mulTextField.XPath, xmlField.XPath, v => mulTextField.XPath = v);
            SetValue(mulTextField.AllowHyperlink, xmlField.AllowHyperlink, v => mulTextField.AllowHyperlink = v);
            SetValue(mulTextField.AppendOnly, xmlField.AppendOnly, v => mulTextField.AppendOnly = v);
            SetValue(mulTextField.DifferencingLimit, xmlField.DifferencingLimit, v => mulTextField.DifferencingLimit = v);
            SetValue(mulTextField.IsolateStyles, xmlField.IsolateStyles, v => mulTextField.IsolateStyles = v);
            SetValue(mulTextField.NumberOfLines, xmlField.NumberOfLines, v => mulTextField.NumberOfLines = v);
            SetValue(mulTextField.RichText, xmlField.RichText, v => mulTextField.RichText = v);
            SetValue(mulTextField.RichTextMode, xmlField.RichTextMode, v => mulTextField.RichTextMode = v);
            SetValue(mulTextField.UnlimitedLengthInDocumentLibrary, xmlField.UnlimitedLengthInDocumentLibrary, v => mulTextField.UnlimitedLengthInDocumentLibrary = v);
        }

        private void SetGridField(IAveFieldRatingScale gridField, AveXmlField xmlField)
        {
            SetValue(gridField.GridEndNumber, xmlField.GridEndNumber, v => gridField.GridEndNumber = v);
            SetValue(gridField.GridNAOptionText, xmlField.GridNAOptionText, v => gridField.GridNAOptionText = v);
            SetValue(gridField.GridTextRangeAverage, xmlField.GridTextRangeAverage, v => gridField.GridTextRangeAverage = v);
            SetValue(gridField.GridTextRangeHigh, xmlField.GridTextRangeHigh, v => gridField.GridTextRangeHigh = v);
            SetValue(gridField.GridTextRangeLow, xmlField.GridTextRangeLow, v => gridField.GridTextRangeLow = v);
            SetMultiChocieField(gridField, xmlField);
        }

        /// <summary>
        /// 对于修改List的setting而导致field的属性变化的那些属性，例如Indexed属性，不作为该field的定义进行compare，但是此类属性也需要更新，添加此方法对此类属性进行更新
        /// ADO-123588但是只更新这个属性也会有问题，如果EnforceUniqueValues属性是true，而只把indexed属性更新成false就会出现问题。
        /// </summary>
        private bool UpdateNotConflictProperties(IAveField field, AveXmlField xmlField)
        {
            if ((xmlField.Indexed == true && field.Indexed == false) || (xmlField.Indexed == false && field.Indexed == true && field.EnforceUniqueValues == false))
            {
                //10以上版本的SP，DiscussionBoard里的Author column的Index属性，必须保持默认值。
                if (!Is10UpperDiscussionBoardAuthorColumn(field))
                {
                    field.Indexed = xmlField.Indexed;
                    field.Update();
                    return true;
                }
            }
            return false;
        }

        private bool Is10UpperDiscussionBoardAuthorColumn(IAveField field)
        {
            return mAveSPList != null
                && !mAveSPList.ParentSite.AveSite.SPVersion.StartsWith("14", StringComparison.OrdinalIgnoreCase)
                && mAveSPList.SPList.BaseTemplate == AveListTemplateType.DiscussionBoard
                && field.InternalName.Equals("Author", StringComparison.OrdinalIgnoreCase);
        }

        #region Update fields.Tool method

        private void SetElements(XmlElement fieldElement, XmlElement xmlElement, List<string> ignoredNodes)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.SetElements"))
            {
                var xmlElements = xmlElement.ChildNodes.Cast<XmlElement>()
                    .Where(node => !ignoredNodes.Contains(node.Name))
                    .ToDictionary(node => node.Name);

                var currentNode = fieldElement.FirstChild;
                while (currentNode != null)
                {
                    var nextNode = currentNode.NextSibling;
                    var currentElement = currentNode as XmlElement;
                    if (currentElement != null && !ignoredNodes.Contains(currentElement.Name))
                    {
                        if (xmlElements.ContainsKey(currentElement.Name))
                        {
                            if (!currentElement.InnerXml.Equals(xmlElements[currentElement.Name].InnerXml, StringComparison.Ordinal))
                            {
                                currentElement.InnerXml = xmlElements[currentElement.Name].InnerXml;
                            }
                            SetAttributes(currentElement, xmlElements[currentElement.Name]);
                            xmlElements.Remove(currentElement.Name);
                        }
                        else
                        {
                            fieldElement.RemoveChild(currentElement);
                        }
                    }
                    currentNode = nextNode;
                }
                if (xmlElements.Count > 0)
                {
                    xmlElements.Values.Select(node => fieldElement.AppendChild(fieldElement.OwnerDocument.ImportNode(node, true))).ToList().Clear();
                }
            }
        }

        private void SetAttributes(XmlElement fieldElement, XmlElement xmlElement)
        {
            SetAttributes(fieldElement, xmlElement, new List<string>());
        }

        private void SetAttributes(XmlElement fieldElement, XmlElement xmlElement, List<string> ignoredAttributes)
        {
            using (new AvePerformanceScope("Restore.AveSPFieldCollection.SetAttributes"))
            {
                var xmlAttributes = xmlElement.Attributes.Cast<XmlAttribute>()
                    .Where(attribute => !ignoredAttributes.Contains(attribute.Name))
                    .ToDictionary(attribute => attribute.Name);
                for (int i = 0; i < fieldElement.Attributes.Count; ++i)
                {
                    var currentAttribute = fieldElement.Attributes[i];
                    if (ignoredAttributes.Contains(currentAttribute.Name))
                    {
                        continue;
                    }
                    if (xmlAttributes.ContainsKey(currentAttribute.Name))
                    {
                        if (!currentAttribute.Value.Equals(xmlAttributes[currentAttribute.Name].Value, StringComparison.Ordinal))
                        {
                            currentAttribute.Value = xmlAttributes[currentAttribute.Name].Value;
                        }
                        xmlAttributes.Remove(currentAttribute.Name);
                    }
                    else
                    {
                        fieldElement.RemoveAttributeAt(i);
                        --i;
                    }
                }
                if (xmlAttributes.Count > 0)
                {
                    xmlAttributes.Values.ToList().ForEach(attribute => fieldElement.SetAttribute(attribute.Name, attribute.Value));
                }
            }
        }

        private void SetValue<T>(T dest, T source, Action<T> setter) where T : struct
        {
            if (!source.Equals(dest))
            {
                setter(source);
            }
        }

        private void SetValue<T>(T? dest, T? source, Action<T?> setter) where T : struct
        {
            if (!source.Equals(dest))
            {
                setter(source);
            }
        }

        private void SetValue(string dest, string source, Action<string> setter)
        {
            if (!string.Equals(source, dest, StringComparison.Ordinal))
            {
                setter(source);
            }
        }
        private void SetValueIgnoreNullOrEmpty(string dest, string source, Action<string> setter)
        {
            if (!string.IsNullOrEmpty(dest) || !string.IsNullOrEmpty(source))
            {
                SetValue(dest, source, setter);
            }
        }

        private void SetValueIgnoreDefaultJSLink(string dest, string source, Action<string> setter)
        {
            if (!string.IsNullOrEmpty(source))
            {
                SetValue(dest, source, setter);
            }
        }

        private void SetValueWithOutCheckValue(string dest, string source, Action<string> setter)
        {
            setter(source);
        }


        protected void HandleFieldsSchemaXmlForSurveyList()
        {
            string columnSchemaXmlForSurveyList = "<Field DisplayName=\"GUID\" Type=\"Guid\" Required=\"FALSE\" ID=\"{fafdd1c4-a270-4179-9595-13f8feedf05b}\" SourceID=\"{30b54aa5-f5c5-48fa-8a3a-d6bb499b72e4}\" StaticName=\"GUID\" Name=\"GUID\" ColName=\"uniqueidentifier1\" RowOrdinal=\"0\" Hidden=\"TRUE\" CanToggleHidden=\"TRUE\" Version=\"1\" />";
            if (!this.currentXmlFields.ContainsKey("GUID"))
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(columnSchemaXmlForSurveyList);

                AveXmlField hiddenField = new AveXmlField(doc.DocumentElement, (int)mAveSPWeb.SPWeb.Language);
                currentXmlFields.Add(hiddenField.FieldInternalName, hiddenField);
            }
        }
        #endregion

        #endregion

        #region IReportable Members

        public IReport GetReport()
        {
            return reportor;
        }

        #endregion

        public void DisposeReport()
        {
            if (reportor != null)
            {
                reportor.Dispose();
            }
        }

        public void Dispose()
        {
            if (reportor != null)
            {
                reportor.Dispose();
            }
            if (mFieldMapping != null)
            {
                mFieldMapping.Dispose();
                mFieldMapping = null;
            }
        }

        public abstract void SetIfCreateFieldIfNotExist(bool create);
    }

    [AveCodeReview("2012/10/08", "xihe.you@avepoint.com", "fengfu.zhang@avepoint.com", null, "ADO-40834", true)]
    public class AveSPWebFieldCollection : AveSPFieldCollection
    {
        private readonly List<IAveFieldMapping> mAvailableMappings;

        public AveSPWebFieldCollection(AveSPWeb aveSPWeb)
        {
            mAveSPWeb = aveSPWeb;
            mAveParentSite = aveSPWeb.ParentSite;
            mAvailableMappings = AveFieldHelper.GetAvailableFieldIdMappings(mAveSPWeb.SPWeb);
        }

        protected override FieldType FieldType
        {
            get { return FieldType.Site; }
        }

        protected override AveReportObjectType ObjectType
        {
            get { return AveReportObjectType.WebField; }
        }

        protected override string ObjectTitle
        {
            get { return mAveSPWeb.SPWeb.Title; }
        }

        protected override IAveFieldCollection FieldCollection
        {
            get { return mAveSPWeb.SPWeb.Fields; }
        }

        protected override IAveFieldCollection AllFieldCollection
        {
            get { return mAveSPWeb.SPWeb.AvailableFields; }
        }

        public override IAveFieldMapping FieldMapping
        {
            get
            {
                if (mFieldMapping == null)
                {
                    mFieldMapping = new AveFieldMapping();
                    mFieldMapping.SetFieldIdSchemaMappings(AveFieldHelper.GetFieldMapping(mAveSPWeb.SPWeb.AllProperties));
                }
                return mFieldMapping;
            }
        }

        protected override IAveField FindField(AveXmlField xmlField, FieldFindOption findOption)
        {
            using (new AvePerformanceScope("Restore.AveSPWebFieldCollection.FindField"))
            {
                IAveField field = base.FindField(xmlField, findOption);
                if (null == field)
                {
                    field = FindField(mAveSPWeb.SPWeb.AvailableFields, xmlField, findOption, mAvailableMappings);
                }
                return field;
            }
        }

        public override void RestoreFields(string fieldsXml, AveFieldRestoreOption restoreOption)
        {
            using (new AvePerformanceScope("Restore.AveSPWebFieldCollection.RestoreFields"))
            {
                base.RestoreFields(fieldsXml, restoreOption);
                AveFieldHelper.UpdateFieldSchemaIdMappingProperty(mAveSPWeb.SPWeb, FieldMapping);
            }
        }

        protected override string GenerateInternalName(AveCustomFieldInfo customFieldInfo)
        {
            //Web级别如果InternalName如果冲突了需要重新生成一个InternalName

            string newInternalName = string.IsNullOrEmpty(customFieldInfo.InternalName)
                ? customFieldInfo.Name
                : customFieldInfo.InternalName;

            newInternalName = AveFieldHelper.GetNewInternalName(newInternalName, FieldCollection);


            return newInternalName;
        }

        public override void SetIfCreateFieldIfNotExist(bool create)
        {
            throw new NotImplementedException();
        }
    }

    [AveCodeReview("2012/10/08", "xihe.you@avepoint.com", "fengfu.zhang@avepoint.com", null, "ADO-40834", true)]
    public class AveSPListFieldCollection : AveSPFieldCollection
    {
        internal static readonly HashSet<string> NoRestoreFieldMap;
        internal static readonly HashSet<string> NeedRestoreFieldMap;

        private readonly Dictionary<string, Dictionary<string, Dictionary<string, string>>> lookUpListItemIDAndValues = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

        private bool mCreateFieldIfNotExist = true;
        private bool mModifiedFieldHidden;
        private bool mModifiedFieldReadOnly;
        private bool mSkipIfConflict;
        private Dictionary<Guid, string> mValidationFields;
        private Dictionary<Guid, string> mFieldsDefaultValue;
        private List<string> sourceFieldOrder = new List<string>();
        private Dictionary<Guid, List<Guid>> mListFieldIndexesCache;
        [ThreadStatic]
        private static List<AveLookupFieldInfo> mLookupFieldIdValue;
        private static List<AveUrlFieldInfo> mUrlFieldValue;
        //mNintexFormDataVale 一个list只能有一个FormData field
        private static AveNintexFormDataFieldInfo mNintexFormDataVale;
        private bool needForceCreateTerm = false;

        private bool hasReloaded = false;

        public bool NeedForceCreateTerm
        {
            get
            {
                return needForceCreateTerm;
            }
        }

        public override void SetIfCreateFieldIfNotExist(bool create)
        {
            mCreateFieldIfNotExist = create;
        }

        public void SetSkipIfConflict(bool skip)
        {
            mSkipIfConflict = skip;
        }

        internal bool HasCreateFieldWhenEnsureFields { get; private set; }

        internal List<string> TaxonomyFields { get; private set; }

        static AveSPListFieldCollection()
        {
            NoRestoreFieldMap = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                                    {
                                        "#tp_ID",
                                        "#tp_ListId",
                                        "#tp_SiteId",
                                        "#tp_RowOrdinal",
                                        "#tp_Version",
                                        "#tp_Ordering",
                                        "#tp_ThreadIndex",
                                        "#tp_HasAttachment",
                                        "#tp_ModerationStatus",
                                        "#tp_IsCurrent",
                                        //"#tp_ItemOrder",
                                        "#tp_InstanceID",
                                        "#tp_GUID",
                                        "#tp_CopySource",
                                        "#tp_HasCopyDestinations",
                                        "#tp_AuditFlags",
                                        "#tp_InheritAuditFlags",
                                        "#tp_Size",
                                        "#tp_WorkflowVersion",
                                        "#tp_WorkflowInstanceID",
                                        "#tp_ParentId",
                                        "#tp_DocId",
                                        "#tp_DeleteTransactionId",
                                        "#uniqueidentifier1",
                                        "#tp_Level",
                                        "#tp_IsCurrentVersion",
                                        "#tp_UIVersion",
                                        "#tp_CalculatedVersion",
                                        "#tp_UIVersionString",
                                        "#tp_DraftOwnerId",
                                        "FileType",
                                        "PreviewOnForm",
                                        "ImageSize",

                                        //DOC-67843
                                        //report metadata 下的item的这个column指向的是report template下面的doc的guid，这里不能使用源端数据
                                        "_dlc_Reporting_TemplateId",
                                        "_dlc_Reporting_QueryAssembly",
                                        "_dlc_Reporting_InjectionAssembly",
                                        "_dlc_Reporting_InjectionClass",
                                        "_dlc_Reporting_IconUrl",
                                        "_dlc_Reporting_HttpContentType",

                                        //don't restore holds field values
                                        "_vti_ItemHoldRecordStatus",
                                        //"IconOverlay",
                                    };


            //应用field filter的时候，有些field是不能被filter的，添加到NEED_RESTORE_FIELD_MAP中,用小写字符表示
            NeedRestoreFieldMap = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                                      {
                                          "WikiField",
                                          "Editor",
                                          "Author",
                                          "Modified",
                                          "Created",
                                          "PublishingPageImage",
                                          "SummaryLinks",
                                      };
        }

        public AveSPListFieldCollection(AveSPList aveSPList)
        {
            mAveSPList = aveSPList;
            var list = mAveSPList.AveList;
            mAveSPWeb = aveSPList.ParentWeb;
            mAveParentSite = mAveSPWeb.ParentSite;
            FieldTypeFilter = new List<string> { AveFieldType.WorkflowStatus.ToString() };
            TaxonomyFields = new List<string>();
        }

        protected override FieldType FieldType { get { return FieldType.List; } }

        protected override AveReportObjectType ObjectType { get { return AveReportObjectType.ListField; } }

        protected override string ObjectTitle { get { return mAveSPList.Name; } }

        protected override IAveFieldCollection FieldCollection
        {
            get { return mAveSPList.SPList.Fields; }
        }

        protected override IAveFieldCollection AllFieldCollection
        {
            get { return mAveSPList.SPList.Fields; }
        }

        public override IAveFieldMapping FieldMapping
        {
            get
            {
                var listId = mAveSPList.SPList != null ? mAveSPList.SPList.ID : Guid.Empty;
                IAveFieldMapping value;
                if (mAveParentSite.MappingManager.SiteMappingManager.TryGetOrAddListFieldsMapping(listId, new AveFieldMapping(), out value))
                {
                    if (listId != Guid.Empty)
                    {
                        value.SetFieldIdSchemaMappings(AveFieldHelper.GetFieldMapping(mAveSPList.SPList.RootFolder.Properties));
                    }
                }
                return value;
            }
        }

        public void ResetNintexFormDataFieldValue(AveNintexFormDataFieldInfo nintexFormDataVale)
        {
            mNintexFormDataVale = nintexFormDataVale;
        }

        public void ResetListLookupFieldIdValues(AveLookupFieldInfo lookupInfo)
        {
            if (mLookupFieldIdValue == null)
            {
                mLookupFieldIdValue = new List<AveLookupFieldInfo>();
            }
            mLookupFieldIdValue.Add(lookupInfo);
        }

        public void ResetUrlFieldValues(AveUrlFieldInfo urlInfo)
        {
            if (mUrlFieldValue == null)
            {
                mUrlFieldValue = new List<AveUrlFieldInfo>();
            }
            mUrlFieldValue.Add(urlInfo);
        }

        protected override Dictionary<string, AveXmlField> LoadXmlFields(XmlElement fieldsXml)
        {
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.LoadXmlFields"))
            {
                mListFieldIndexesCache = fieldsXml.Cast<XmlElement>().Where(fieldXml => fieldXml.Name.Equals("Index", StringComparison.OrdinalIgnoreCase))
                    .ToDictionary(
                    xe => new Guid(xe.GetAttribute("ID")),
                    xe => new List<Guid>
                    {
                        new Guid(xe.FirstChild.Attributes["ID"].Value),
                        new Guid(xe.LastChild.Attributes["ID"].Value),
                    }
                    );
                sourceFieldOrder = GetFieldOrder(fieldsXml);

                var xmlFields = base.LoadXmlFields(fieldsXml);
                if (mAveSPList.SPList.TemplateFeatureId == new Guid("065c78be-5231-477e-a972-14177cc5b3c7"))
                {//Kpi lists
                    var dataSourceField = xmlFields.Values.FirstOrDefault(xmlField => xmlField.Title.Equals("Data Source", StringComparison.Ordinal));
                    if (dataSourceField != null)
                    {
                        string key = mAveSPList.SPList.ID.ToString() + ":" + mAveSPWeb.SPWeb.ID.ToString();
                        if (!mAveSPWeb.ParentSite.KpiListIdCol.Contains(key))
                        {
                            mAveSPWeb.ParentSite.KpiListIdCol.Add(key);
                        }
                    }
                }
                return xmlFields;
            }
        }

        private List<string> GetFieldOrder(string fieldsXml)
        {
            var doc = new XmlDocument();
            doc.LoadXml(fieldsXml);
            return GetFieldOrder(doc.DocumentElement);
        }

        private List<string> GetFieldOrder(XmlElement fieldsXml)
        {
            return fieldsXml.Cast<XmlElement>().Where(
                fieldXml => fieldXml.HasAttribute("Name") && !"WorkflowStatus".Equals(fieldXml.GetAttribute("Type")))
                .Select(fieldXml => fieldXml.GetAttribute("Name")).ToList();
        }

        protected override void UpdateFieldXml(AveXmlField xmlField)
        {
            base.UpdateFieldXml(xmlField);
            var mapping = mAveSPWeb.Fields.FieldMapping;
            var webId = mAveSPWeb.SPWeb.ID;
            Guid newID = mapping.GetMappingRestoredFieldId(xmlField.ID);
            if (newID != Guid.Empty)
            {
                xmlField.XmlElement.SetAttribute("ID", newID.ToString());
            }
            if (xmlField.TypeAsString.Equals("TaxonomyFieldType", StringComparison.OrdinalIgnoreCase)
                || xmlField.TypeAsString.Equals("TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase))
            {
                IAveFieldCollection fields = mAveSPWeb.SPWeb.AvailableFields;
                IAveField webField = null;
                if (fields.Contains(newID))
                {
                    webField = fields.GetById(newID);
                }
                if (webField != null && webField is IAveTaxonomyField)
                {
                    var tWebField = webField as IAveTaxonomyField;
                    if (xmlField.CustomProperties.ContainsKey("TextField") && xmlField.CustomProperties["TextField"] != null)
                    {
                        xmlField.XmlElement.InnerXml = xmlField.XmlElement.InnerXml.Replace(xmlField.CustomProperties["TextField"].ToString(), tWebField.TextField.ToString());
                    }
                }
            }
        }

        public override void RestoreFields(string fieldsXml, AveFieldRestoreOption restoreOption)
        {
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.RestoreFields"))
            {
                mAveSPList.IsLoadFieldXml = false;
                base.RestoreFields(fieldsXml, restoreOption);
                RestoreListFieldOrder();
                //ADO-149427 
                if (this.mAveSPList.containsTODAY && !this.hasReloaded)
                {
                    mAveSPWeb.ReloadWeb();
                    mAveSPList.ReloadList();
                }
            }
        }

        protected override void CacheFieldByType(AveXmlField xmlField, IAveField field)
        {
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.CacheFieldByType"))
            {
                if (!String.IsNullOrEmpty(xmlField.AveLookupListTitle))
                {
                    var obj = new AveLookupObject
                    {
                        Id = field.ID,
                        ListTitle = xmlField.AveLookupListTitle,
                        WebUrl = xmlField.AveLookupWebTitle,
                        Type = xmlField.AveSourceType,
                        WebId = mAveSPWeb.SPWeb.ID,
                        SourceListId = xmlField.AveLookupListID,
                        ListId = mAveSPList.SPList.ID,
                    };
                    mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.AddLookupField(obj);
                }

                //为了兼容继承TaxonomyField的第三方的Column
                if ((xmlField.FieldBaseType.Equals("TaxonomyFieldType", StringComparison.OrdinalIgnoreCase)
                    || xmlField.FieldBaseType.Equals("TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase)) && xmlField.CustomFieldInfo == null
                    || string.Equals(field.TypeAsString, "TaxonomyFieldType", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(field.TypeAsString, "TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase))
                {
                    if (WrapperRuntime.CurrentContext.IsMoss)
                    {
                        if (!TaxonomyFields.Contains(field.InternalName))
                        {
                            TaxonomyFields.Add(field.InternalName);
                        }
                    }
                }
                base.CacheFieldByType(xmlField, field);
            }
        }

        protected override AveLookupObject CreateLookupObjectInfo(AveXmlField xmlField)
        {
            var objectInfo = base.CreateLookupObjectInfo(xmlField);
            objectInfo.ListId = mAveSPList.SPList.ID;
            return objectInfo;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint query")]
        private void RestoreListFieldOrder()
        {
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.RestoreListFieldOrder"))
            {
                try
                {
                    if (mAveSPList == null || mAveSPList.SPList == null || sourceFieldOrder.Count == 0 || mAveParentSite.SPContextKind == AveContextKind.ClientObjectModel)
                    {
                        return;
                    }
                    List<string> destFieldOrder = GetFieldOrder(mAveSPList.SPList.Fields.GetFields(mAveSPList.ParentWeb.SPWeb.ID, mAveSPList.SPList.ID));

                    if (destFieldOrder.Count == sourceFieldOrder.Count)
                    {
                        bool isSame = sourceFieldOrder.Where((t, i) => string.Equals(t, destFieldOrder[i])).Count() == sourceFieldOrder.Count;
                        if (isSame)
                        {
                            return;
                        }
                    }
                    var mappedSourceFields = new List<string>();
                    if (XmlFields != null)
                    {
                        foreach (string sourceFieldInternalName in sourceFieldOrder)
                        {
                            string fieldIntername = FieldMapping.GetMappingRestoredFieldInternalName(sourceFieldInternalName);
                            if (string.IsNullOrEmpty(fieldIntername))
                            {
                                fieldIntername = sourceFieldInternalName;
                            }
                            if (FieldCollection.ContainsField(fieldIntername) && !mappedSourceFields.Contains(fieldIntername))
                            {
                                mappedSourceFields.Add(fieldIntername);
                            }
                        }
                        mappedSourceFields.AddRange(FieldCollection.Where(field => !mappedSourceFields.Contains(field.InternalName)).Select(field => field.InternalName).ToList());

                        const string rpcMethod =
                            @"<?xml version=""1.0"" encoding=""UTF-8""?>  
                                <Method ID=""0,REORDERFIELDS"">  
                                <SetList Scope=""Request"">{0}</SetList>  
                                <SetVar Name=""Cmd"">REORDERFIELDS</SetVar>  
                                <SetVar Name=""ReorderedFields"">{1}</SetVar>  
                                <SetVar Name=""owshiddenversion"">{2}</SetVar>  
                                </Method>";
                        var sb = new StringBuilder();
                        var xmlWriter = new XmlTextWriter(new StringWriter(sb)) { Formatting = Formatting.Indented };
                        xmlWriter.WriteStartElement("Fields");
                        foreach (string field in mappedSourceFields)
                        {
                            xmlWriter.WriteStartElement("Field");
                            xmlWriter.WriteAttributeString("Name", field);
                            xmlWriter.WriteEndElement();
                        }
                        xmlWriter.WriteEndElement();
                        xmlWriter.Flush();
                        string rpcCall = string.Format(rpcMethod, mAveSPList.SPList.ID, HttpUtility.HtmlEncode(sb.ToString()), mAveSPList.SPList.Version);
                        mAveSPList.ParentWeb.SPWeb.AllowUnsafeUpdates = true;
                        string result = mAveSPList.ParentWeb.SPWeb.ProcessBatchData(rpcCall);
                        mAveSPList.ParentWeb.ReloadWeb();
                        mAveSPList.ReloadList();
                        this.hasReloaded = true;
                        var xdoc = new XmlDocument();
                        xdoc.LoadXml(result);
                        if (xdoc.DocumentElement.HasAttribute("Code") &&
                            xdoc.DocumentElement.GetAttribute("Code").Equals("0"))
                        {
                            log.Debug("reorder list fields successful.");
                        }
                        else
                        {
                            log.Warn("reorder list fields failed.Exception:" + xdoc.InnerText);
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, "An error occurred while reorder list fields.List ID:{0}. error message:{1}", mAveSPList.SPList.ID, e.ToString());
                }
            }
        }

        public void LoadExistLookupFields()
        {
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.LoadExistLookupFields"))
            {
                foreach (IAveField field in FieldCollection)
                {
                    var fieldlookup = field as IAveFieldLookup;
                    if (fieldlookup != null && !(field is IAveFieldUser) && !string.IsNullOrEmpty(fieldlookup.LookupList))
                    {
                        var aloj = new AveLookupObject
                        {
                            WebId = fieldlookup.LookupWebId,
                            ListId = mAveSPList.SPList.ID,
                            SourceListId = fieldlookup.LookupList,
                            Id = field.ID
                        };
                        mAveParentSite.MappingManager.SiteMappingManager.AddLookupField(aloj);
                    }
                }
            }
        }

        #region Ensure field
        public AveXmlField GetXmlFieldBySourceFieldId(Guid fieldId)
        {
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.GetXmlFieldBySourceFieldId"))
            {
                AveXmlField xmlField = null;
                if (XmlFields != null)
                {
                    xmlField = XmlFields.Values.FirstOrDefault(xf => xf.ID == fieldId);
                }
                if (xmlField == null)
                {//对于找不到的，一般是自带的buildin的column，通过id即可直接找到，在此通过id找到，构造出来xmlField
                    try
                    {
                        IAveField field = FieldCollection[fieldId];
                        XmlDocument schemaXml = new XmlDocument();
                        schemaXml.LoadXml(field.SchemaXml);
                        xmlField = new AveXmlField(schemaXml.DocumentElement, (int)this.mAveSPWeb.SPWeb.Language);
                    }
                    catch (Exception ex)
                    {
                        log.Warn("The field with the ID:{0} can not find.Error:{1}", fieldId.ToString(), ex.ToString());
                    }
                }
                return xmlField;
            }
        }

        [Obsolete("no use now, will remove later")]
        public IAveField EnsureField(Guid fieldId, out bool needReload)
        {
            needReload = false;
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.EnsureField"))
            {
                IAveField field = null;
                if (XmlFields != null)
                {
                    //field = (from key in XmlFields.Keys where fieldId == XmlFields[key].ID select EnsureField(key, out needReload)).FirstOrDefault();
                    field = GetField(fieldId, out needReload);
                }
                if (field == null)
                {
                    try
                    {
                        field = FieldCollection[fieldId];
                    }
                    catch (Exception ex)
                    {
                        log.Warn("The field with the ID:{0} can not find.Error:{1}", fieldId.ToString(), ex.ToString());
                    }
                }
                return field;
            }
        }

        private IAveField GetField(Guid fieldId, out bool needReload)
        {
            needReload = false;
            foreach (var key in XmlFields.Keys)
            {
                if (fieldId == XmlFields[key].ID)
                {
                    return EnsureField(key, out needReload);
                }
            }
            return null;
        }

        public IAveField GetFieldById(Guid fieldId)
        {
            IAveField field = null;
            try
            {
                field = FieldCollection[fieldId];
            }
            catch (Exception ex)
            {
                log.Warn("The field with the ID:{0} can not find.Error:{1}", fieldId.ToString(), ex.ToString());
            }
            return field;
        }

        public void EnsureFields(Dictionary<string, object> data, List<Dictionary<string, object>> junctionData, bool throwWhenNotFound, bool throwWhenConflict, AveFieldRestoreOption restoreOption)
        {
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.EnsureFields"))
            {
                reportor.Dispose();
                var beforeRestoreCount = mAveSPList.SPList != null ? FieldCollection.Count : 0;
                var fieldNames = data.Keys.Select(
                    name =>
                    {
                        if (name.Equals("#tp_ContentTypeId", StringComparison.Ordinal))
                        {
                            return "ContentType";
                        }
                        if (name.Contains("#"))
                        {
                            return name.Substring(0, name.IndexOf("#", StringComparison.OrdinalIgnoreCase));
                        }
                        if (SourceTextTaxonomyDic.ContainsKey(name))
                        {
                            return SourceTextTaxonomyDic[name];
                        }
                        return name;
                    }).Where(name => !NoRestoreFieldMap.Contains(name) && !name.StartsWith(AveConstants.FIELD_SEPARATOR, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                fieldNames = GetFieldInternalNameByJunction(junctionData, fieldNames);
                fieldNames = SortFieldsForRestore(fieldNames, XmlFields);
                Exception ex = null;
                Dictionary<Guid, string> mEnsureFieldsMapping = new Dictionary<Guid, string>();
                bool needReload = false;
                foreach (var fieldName in fieldNames)
                {
                    try
                    {
                        bool fieldAddOrUpdate;
                        IAveField field = EnsureField(fieldName, throwWhenNotFound, throwWhenConflict, restoreOption, out fieldAddOrUpdate);
                        needReload |= fieldAddOrUpdate;
                        if (field != null)
                        {
                            mEnsureFieldsMapping.Add(field.ID, field.InternalName);
                        }
                    }
                    catch (Exception excep)
                    {
                        log.Warn("Try ro ensure field:{0} failed.Error is:{1}", fieldName, excep);
                        FieldMapping.AddFailedFields(fieldName);
                        ex = excep;
                    }
                }
                if (needReload)
                {
                    mAveSPList.AveList.Reload();
                }
                //把Ensure出来的field 的mapping也放到ContentTypeHelper中
                if (mAveSPList.AveContentTypes != null && mAveSPList.AveContentTypes.ContentTypeHelper != null)
                {
                    mAveSPList.AveContentTypes.ContentTypeHelper.Initialize(FieldMapping, mAveSPList.AveContentTypes.ContentTypeMapping, mEnsureFieldsMapping);
                }
                var afterRestoreCount = mAveSPList.SPList != null ? FieldCollection.Count : 0;
                HasCreateFieldWhenEnsureFields = beforeRestoreCount != afterRestoreCount;
                if (ex != null)
                {
                    throw ex;
                }
            }
        }
        [SuppressMessage("Microsoft.Globalization", "CA1302:DoNotHardcodeLocaleSpecificStrings", Justification = "SendTo is a field type")]
        private List<string> GetFieldInternalNameByJunction(List<Dictionary<string, object>> junctionData, List<string> fieldNames)
        {
            if (junctionData == null)
            {
                return fieldNames;
            }
            int originalVersion = 0;
            Dictionary<Guid, Dictionary<int, string>> fieldValues = new Dictionary<Guid, Dictionary<int, string>>();//<FieldId,<ItemId,DisplayValue>>
            foreach (Dictionary<string, object> dic in junctionData)
            {
                if (originalVersion == 0)
                {
                    originalVersion = (int)dic["tp_UIVersion"];
                }
                Guid fieldId = (Guid)dic["tp_FieldId"];
                int id = (int)dic["tp_Id"];
                if (!fieldValues.ContainsKey(fieldId))
                {
                    fieldValues.Add(fieldId, new Dictionary<int, string>());
                }
                string displayValue = string.Empty;
                if (dic.ContainsKey("DisplayValue"))
                {
                    displayValue = dic["DisplayValue"].ToString();
                }
                fieldValues[fieldId][id] = displayValue;
            }
            foreach (KeyValuePair<Guid, Dictionary<int, string>> kv in fieldValues)
            {

                Guid sourceFieldId = kv.Key;
                AveXmlField xmlField = GetXmlFieldBySourceFieldId(sourceFieldId);

                if (xmlField == null || !(xmlField.FieldBaseType.Equals("LookupMulti", StringComparison.OrdinalIgnoreCase) || xmlField.FieldBaseType.Equals("UserMulti", StringComparison.OrdinalIgnoreCase) || xmlField.TypeAsString.Equals("SendTo", StringComparison.OrdinalIgnoreCase) || xmlField.TypeAsString.Equals("CallTo", StringComparison.OrdinalIgnoreCase) || xmlField.TypeAsString.Equals("Facilities", StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }
                Dictionary<int, string> value = kv.Value;
                string fieldName = null;
                if (XmlFields != null)
                {
                    fieldName = (from key in XmlFields.Keys where sourceFieldId == XmlFields[key].ID select XmlFields[key].FieldInternalName).FirstOrDefault();
                }
                if (!string.IsNullOrEmpty(fieldName) && !fieldNames.Contains(fieldName))
                {
                    fieldNames.Add(fieldName);
                }
            }
            return fieldNames;
        }

        private IAveField EnsureField(string name, out bool needReloadList)
        {
            needReloadList = false;
            return EnsureField(name, !mCreateFieldIfNotExist, mSkipIfConflict, new AveFieldRestoreOption(), out needReloadList);
        }

        private IAveField EnsureField(string name, bool throwWhenNotFound, bool throwWhenConflict, AveFieldRestoreOption restoreOption, out bool needReload)
        {
            needReload = false;
            this.restoreOption = restoreOption;
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.EnsureField_1"))
            {
                if (mSkippedRestoreFields.Contains(name))
                {
                    log.Log(AveLogLevel.INFO, "Skip restore field:{0}", name);
                    return null;
                }
                if (restoreFailedFields.ContainsKey(name))
                {
                    throw restoreFailedFields[name];
                }
                string mappingName = FieldMapping.GetMappingRestoredFieldInternalName(name);
                try
                {
                    if (!String.IsNullOrEmpty(mappingName))
                    {
                        return GetFieldByInternalName(mappingName);
                    }
                }
                catch (Exception e)
                {
                    log.Debug("Find field error by mappingName,Name: {0},MappingName:{1},Error:{2}", name, mappingName, e.ToString());
                }

                if (XmlFields == null || !XmlFields.ContainsKey(name))
                {
                    IAveField field = null;
                    try
                    {
                        field = GetFieldByInternalName(name);
                    }
                    catch (Exception ex)
                    {
                        log.Warn("Can't get field by internal name:{0}.Detail:{1}", name, ex.ToString());
                    }
                    return field;
                }
                AveXmlField srcXf = XmlFields[name];
                var restoredField = RestoreSingleField(restoreOption, srcXf, throwWhenNotFound, throwWhenConflict, true, out needReload);
                CacheNeedResetValidationField(restoredField);
                return restoredField;
            }
        }
        private void CacheNeedResetValidationField(IAveField field)
        {
            if (field != null && !string.IsNullOrEmpty(field.ValidationFormula))
            {
                if (mValidationFields == null)
                {
                    mValidationFields = new Dictionary<Guid, string>();
                }
                try
                {
                    mValidationFields[field.ID] = field.ValidationFormula;
                    field.ValidationFormula = string.Empty;
                    field.Update();
                }
                catch (Exception e)
                {
                    log.Warn("Set field validation formula to null failed. Field: {0}, Error: {1}", field.InternalName, e);
                }
            }
        }

        internal IAveField GetFieldByInternalName(string internalName)
        {
            if (FieldCollection.ContainsField(internalName))
            {
                return FieldCollection.GetFieldByInternalName(internalName);
            }
            return null;
        }

        #endregion

        #region Get Field Value
        [Obsolete]
        public Dictionary<string, object> GetFieldValues(string docName, int docRowId, int version, Dictionary<string, object> data)
        {
            return GetFieldValues(docName, docRowId, version, data, false);
        }

        //isMergeToFolder参数 为CM import folder专用,其他模块不需要传值
        [Obsolete]//用ItemUserAndJunctionData.GetItemFieldValues()取代该方法
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Special file in SharePoint,wrkstat.aspx")]
        public Dictionary<string, object> GetFieldValues(string docName, int docRowId, int version, Dictionary<string, object> data, bool getAveField, bool isMergeToFolder = false)
        {
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.GetFieldValues"))
            {
                var newdata = new Dictionary<string, object>();
                needForceCreateTerm = false;
                AddNeedRestoreData(data);
                bool needReload = false;
                foreach (var kv in data)
                {
                    try
                    {
                        string key = kv.Key;
                        object value = kv.Value;
                        bool isSetByCustomMapping = false;
                        if (key == "#tp_ContentTypeId")
                        {
                            key = "ContentType";
                        }
                        else if (key == "#tp_ItemOrder")
                        {
                            key = "Order";
                        }
                        else if (key == "#tp_HasCopyDestinations")
                        {
                            key = "_HasCopyDestinations";
                        }
                        if ((NoRestoreFieldMap.Contains(key) ||
                           key.StartsWith(AveConstants.FIELD_SEPARATOR, StringComparison.OrdinalIgnoreCase))
                          || SourceTextTaxonomyDic.Values.Contains(key))
                        {
                            continue;
                        }
                        if (key.Contains("#"))
                        {//Url类型的通过Url Field直接一起处理，Description Field直接忽略即可。
                            continue;
                        }
                        var fieldName = GetFieldInternalName(key);
                        bool fieldAddOrUpdate;
                        var spField = GetField(fieldName, out fieldAddOrUpdate);
                        needReload |= fieldAddOrUpdate;
                        if (SourceTextTaxonomyDic.ContainsKey(key) && !data.ContainsKey(SourceTextTaxonomyDic[key]) && spField.TypeAsString == "TaxonomyFieldType")
                        {
                            continue;
                        }
                        if (spField == null || AveBuiltInFieldId.ContentTypeId == spField.ID ||
                            spField.TypeAsString == "WorkflowStatus")
                        {
                            continue;
                        }
                        if (!NeedRestoreFieldMap.Contains(spField.InternalName))
                        {
                            if (FilterOut(spField))
                            {
                                log.Log(AveLogLevel.DEBUG, string.Format("Field filter out. Name:{0}.", spField.Title));
                                continue;
                            }
                        }
                        if (data.ContainsKey("#tp_ID"))
                        {
                            if (!(isMergeToFolder && (key.Equals("FileLeafRef", StringComparison.OrdinalIgnoreCase) || key.Equals("Title", StringComparison.OrdinalIgnoreCase))))
                            {
                                value = GetCustomMappingValue(value, docName, Convert.ToInt32(data["#tp_ID"]), spField, fieldName, ref isSetByCustomMapping);
                            }
                        }
                        value = ConvertValueByFieldName(data, spField, value);
                        value = ConvertValueByFieldType(key, value, spField, docRowId, version, data, isSetByCustomMapping);
                        if (value == null)
                        {
                            continue;
                        }
                        //现在所有的调用此开关都是True，需要UpdateFields去判断一下类型是否是Url类型然后转换处理一下
                        if (getAveField)
                        {
                            var aveField = new AveFieldValueInfo { ColValue = value, ColName = spField.ColName, FieldType = spField.Type, RowOrdinal = spField.RowOrdinal };
                            if (spField is IAveFieldUrl)
                            {
                                var fieldUrlValue = value as IAveFieldUrlValue;
                                if (fieldUrlValue.Url.IndexOf("wrkstat.aspx", StringComparison.OrdinalIgnoreCase) > 0)
                                {
                                    const string workflowPattern = @"(?<=wrkstat.aspx\?List=)([A-F0-9]{8}-([A-F0-9]{4}-){3}[A-F0-9]{12})&WorkflowInstanceName=([A-F0-9]{8}-([A-F0-9]{4}-){3}[A-F0-9]{12})";
                                    Regex regex = new Regex(workflowPattern, RegexOptions.IgnoreCase);
                                    if (regex.IsMatch(fieldUrlValue.Url))
                                    {
                                        continue;
                                    }
                                }
                                //Url的description 也走一次url的mapping逻辑，避免实际url替换，显示的url未替换
                                //fieldUrlValue.Description = GetCustomMappingValue(fieldUrlValue.Description, docName, Convert.ToInt32(data["#tp_ID"]), spField, fieldName, ref isSetByCustomMapping).ToString();
                                var descriptionValue = new AveFieldValueInfo { ColValue = fieldUrlValue.Description, FieldType = AveFieldType.URL };
                                descriptionValue.ColName = spField.GetFieldAttributeValue("ColName2");
                                int descriptionRowOrdinal = 0;
                                try
                                {
                                    string rowOrdinal2 = spField.GetFieldAttributeValue("RowOrdinal2");
                                    if (!string.IsNullOrEmpty(rowOrdinal2))
                                    {
                                        int.TryParse(rowOrdinal2, out descriptionRowOrdinal);
                                    }
                                }
                                catch (Exception e)
                                {
                                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.ConverToFieldAttributeError, e.ToString());
                                    descriptionRowOrdinal = 0;
                                }
                                descriptionValue.RowOrdinal = descriptionRowOrdinal;
                                newdata[spField.InternalName + "#2"] = descriptionValue;
                                aveField.ColValue = fieldUrlValue.Url;
                            }
                            if (kv.Key.Equals("#tp_ContentTypeId"))
                            {
                                aveField.ColName = "tp_ContentTypeId";
                            }
                            value = aveField;
                        }
                        newdata[spField.InternalName] = value;
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, "An error occurred while get field values. key:{0}, value:{1}\n error message:{2}", kv.Key, kv.Value, e);
                    }
                }
                if (needReload)
                {
                    mAveSPList.AveList.Reload();
                }
                return newdata;
            }
        }

        private void AddNeedRestoreData(Dictionary<string, object> data)
        {
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.AddNeedRestoreData"))
            {
                if (XmlFields != null)//对于web下的文件，mXmlFields为空
                {
                    foreach (var fieldName in XmlFields.Keys)
                    {
                        var mappedNullValue = FieldMapping.GetMappingNullValue(fieldName);
                        if (mappedNullValue != null && !data.ContainsKey(fieldName))
                        {
                            data.Add(fieldName, mappedNullValue);
                        }
                        var mapping = XmlFields[fieldName].CustomFieldInfo;
                        if (mapping != null && !data.ContainsKey(fieldName))
                        {
                            if (IsTaxonomyField(XmlFields[fieldName].TypeAsString) && SourceTextTaxonomyDic.ContainsValue(fieldName))
                            {//对于excel mapping，metadata column如果源端没有值在excel中设置了值，需要把textField也add进去
                                foreach (KeyValuePair<string, string> kv in SourceTextTaxonomyDic)
                                {
                                    if (!data.ContainsKey(kv.Key) && kv.Value.Equals(fieldName, StringComparison.OrdinalIgnoreCase))
                                    {
                                        data.Add(kv.Key, null);
                                        break;
                                    }
                                }
                            }
                            data.Add(fieldName, null);
                        }
                    }
                }
                var tmpFields = FieldMapping.GetNewFieldsBeforeAdd();
                if (tmpFields != null)
                {
                    foreach (var field in tmpFields)
                    {
                        if (!String.IsNullOrEmpty(field.InternalName) && !field.InternalName.Equals("ID", StringComparison.OrdinalIgnoreCase) && !data.ContainsKey(field.InternalName))
                        {
                            data.Add(field.InternalName, null);
                        }
                    }
                }
            }
        }

        private string GetFieldInternalName(string key)
        {
            string fieldName = key;
            if (SourceTextTaxonomyDic.ContainsKey(fieldName))
            {
                //如果是taxonomy field关联的Text Field，将fieldName设置成对应taxonomy field的Name
                //我们还原taxonomy field的value是通过对应Text Field上的value来还原的
                fieldName = SourceTextTaxonomyDic[fieldName];
            }
            return fieldName;
        }

        public IAveField GetField(string fieldName, out bool needReload)
        {
            needReload = false;
            try
            {
                return EnsureField(fieldName, out needReload);
            }
            catch (AveSchemaDependencyConflictException e)
            {
                log.Log(AveLogLevel.WARN, "Cannot find field when restore value. field:{0}, error:{1}", fieldName, e);
                return mAveSPList.SPList.Fields[e.SchemaDependencyName];
            }
            catch (AveSchemaDependencyNotFoundException e)
            {
                log.Log(AveLogLevel.WARN, "Cannot find field when restore value. field:{0}, error:{1}", fieldName, e);
            }
            return null;
        }

        private object ConvertValueByFieldName(Dictionary<string, object> data, IAveField field, object value)
        {
            if (value == null)
            {
                return null;
            }
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.ConvertValueByFieldName"))
            {
                var option = new ReplaceOption(true) { NeedReplaceAbsoluteUrl = true };
                switch (field.InternalName)
                {
                    case "RoutingContentTypeInternal":
                        var sourceValues = value.ToString().Split('|');
                        var valueId = sourceValues[0];
                        var valueName = sourceValues[1];
                        var tempName = mAveSPWeb.ContentTypes.ContentTypeMapping.GetMappingRestoredContentTypeName(valueName);
                        var tempValueId = mAveSPWeb.ContentTypes.ContentTypeMapping.GetMappingRestoredContentTypeId(valueId);
                        if (!tempName.Equals(valueName, StringComparison.OrdinalIgnoreCase))
                        {
                            valueName = tempName;
                        }
                        if (tempValueId != string.Empty)
                        {
                            valueId = tempValueId;
                        }
                        value = valueId + "|" + valueName;
                        break;

                    case "MasterSeriesItemID":
                        if (mAveSPList.SPList.BaseTemplate == AveListTemplateType.Events)
                        {//CI-19064
                            var itemId = (int)value;
                            //var idMapping = mAveParentSite.MappingManager.SiteMappingManager.ItemIdMapping;
                            int tempCoventValue = mAveParentSite.MappingManager.SiteMappingManager.GetMappingItemId(mAveSPList.SPList.ID, itemId);
                            if (tempCoventValue != -1)
                                return tempCoventValue;
                        }
                        break;
                    case "Target_x0020_Audiences":
                        return ReplaceAudienceId(mAveParentSite.MappingManager.SiteMappingManager, value.ToString());
                    case "Modified_x0020_By":
                    case "Created_x0020_By":
                        try
                        {
                            string mappingValue = mAveParentSite.SPMembers.GetMappingUserLogin(@value.ToString());
                            var user = mAveParentSite.SPSite.RootWeb.EnsureAvailableUser(mappingValue);
                            if (user != null)
                            {
                                return user.LoginName;
                            }
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetUserByUrlError, e.ToString());
                        }
                        return value;
                    case "RoutingTargetPath":
                        if (field is IAveFieldText)
                        {
                            //Replace the url in "RoutingTargetPath" for Content Organizer Rule list
                            //we only need to change patch when location is in the same site, otherwise keep the original data.
                            if (data.ContainsKey("RoutingTargetLibrary") && data["RoutingTargetLibrary"] != null)
                            {
                                return ChangeServerRelativeUrl(value.ToString());
                            }
                        }
                        break;
                    case "TemplateUrl":
                        bool replace = false;
                        string url = value as string;
                        if (!string.IsNullOrEmpty(url) && url.Contains("&#39;"))
                        {
                            url = url.Replace("&#39;", "'");
                            replace = true;
                        }
                        url = AveReplaceProcessor.UrlReplace(url, mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, option, mAveParentSite.SourceSiteInfo, mAveParentSite.ServerRelativeUrl);
                        if (replace && !string.IsNullOrEmpty(url) && url.Contains("'"))
                        {
                            url = url.Replace("'", "&#39;");
                        }
                        return url;

                    case "ViewGuid":
                        //应该是同时包含ViewGuid和ViewName需要添加到KPI Mapping中            
                        //check contenttype instead of viewname for compatible reason,more details here:
                        var itemContentTypeId = GetContentTypeId((byte[])data["#tp_ContentTypeId"]);
                        if (itemContentTypeId != null && itemContentTypeId.IsChildOf(AveSystemContentTypeId.SharePointListbasedStatusIndicator))
                        {
                            mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.AddListIdToWebIdMapping(mAveSPList.SPList.ID, mAveSPWeb.SPWeb.ID);
                        }
                        break;
                    case "ContentType":
                        string ctId = AveConvert.ConvertByteToContentTypeId(mAveParentSite.ObjectModelFactory, (byte[])data["#tp_ContentTypeId"]).ToString();
                        var listCTIdMapping = mAveSPList.ParentSite.MappingManager.ListMappingManager.ListLevelCTIdMapping;
                        if (mAveParentSite.ContentTypeIdMapping.ContainsKey(ctId))
                        {
                            ctId = mAveParentSite.ContentTypeIdMapping[ctId];
                            log.Info("ContentTypeId Mapping To: {0}", ctId);
                        }
                        if (listCTIdMapping.ContainsKey(ctId))
                        {
                            return listCTIdMapping[ctId];
                        }
                        else
                        {
                            IAveContentType ct = mAveSPList.SPList.ContentTypes[mAveSPList.SPList.ContentTypes.BestMatch(mAveParentSite.ObjectModelFactory.CreateContentTypeId(ctId))];
                            if (ct != null)
                            {
                                value = ct.ID;
                            }
                            else
                            {
                                value = null;
                            }
                        }
                        break;
                    //Project Policy Item List Column
                    case "ProjectWebGuid":
                    case "ProjectParentWebGuid":
                        string webIdString = value.ToString();
                        if (!string.IsNullOrEmpty(webIdString))
                        {
                            Guid webGuid = new Guid(webIdString);
                            if (webGuid != Guid.Empty && mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.WebIDMapping.ContainsKey(webGuid))
                            {
                                value = mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.WebIDMapping[webGuid];
                            }
                        }
                        break;
                    case "Label"://VariationLabel
                        if (mAveSPList.SPList != null && mAveSPList.SPList.IsRelationshipsList())
                        {
                            value = ReplaceLabelId(value);
                        }
                        break;
                    case "_SourceUrl":
                        return AveReplaceProcessor.UrlReplace(value.ToString(), mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, option, mAveParentSite.SourceSiteInfo, mAveParentSite.ServerRelativeUrl);
                }
                return value;
            }
        }
        private Guid ReplaceLabelId(object value)
        {
            var stringValue = value.ToString();
            var splitString = stringValue.Split(";".ToCharArray(), 2, StringSplitOptions.None);
            switch (splitString.Length)
            {
                case 1:
                    {
                        var sourceLabelId = splitString[0];
                        return new Guid(sourceLabelId);
                    }
                case 2:
                    {
                        var sourceLabelId = splitString[0];
                        var sourceLabelName = splitString[1];
                        var newId = mAveParentSite.SPSite.GetVariationLabelId(sourceLabelName);
                        return newId != Guid.Empty ? newId : new Guid(sourceLabelId);
                    }
                default: break;
            }
            throw new ArgumentException(string.Format("Variation label format error, label: {0}", stringValue));
        }



        [SuppressMessage("Microsoft.Globalization", "CA1302:DoNotHardcodeLocaleSpecificStrings", MessageId = "SendTo")]
        private object ConvertValueByFieldType(string key, object value, IAveField spField, int docRowId, int version,
            Dictionary<string, object> data, bool isSetByCustomMapping)
        {
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.ConvertValueByFieldType"))
            {
                if (value == null)
                {
                    return value;
                }
                if (spField.Type == AveFieldType.Geolocation)
                {
                    string hexString = string.Empty;
                    if (value is byte[])
                    {
                        byte[] tempBytes = value as byte[];
                        hexString = mAveParentSite.ObjectModelFactory.Utility.HexStringFromBytes(tempBytes);
                    }
                    else
                    {
                        hexString = mAveParentSite.ObjectModelFactory.Utility.HexStringFromBytes(Convert.FromBase64String(value.ToString()));
                    }
                    value = (spField as IAveFieldGeolocation).ConvertHexToWellKnownText(hexString);
                }
                if (spField.Type == AveFieldType.DateTime)
                {
                    return Convert.ToDateTime(value, DateTimeFormatInfo.InvariantInfo);
                }
                if (spField is IAveFieldUrl)
                {
                    return GetUrlValue(key, value, data, docRowId, spField);
                }
                if (spField.TypeAsString.Equals("HTML", StringComparison.OrdinalIgnoreCase) || spField.TypeAsString.Equals("Link", StringComparison.OrdinalIgnoreCase) || spField.TypeAsString.Equals("Image", StringComparison.OrdinalIgnoreCase) || spField.TypeAsString.Equals("SummaryLinks", StringComparison.OrdinalIgnoreCase) || spField.TypeAsString.Equals("MediaFieldType", StringComparison.OrdinalIgnoreCase) || spField.TypeAsString.Equals("Note", StringComparison.OrdinalIgnoreCase))
                {
                    bool needReplaceLast = false;
                    string xmlLinks = AveReplaceProcessor.ReplaceXmlLinks(value.ToString(), mAveParentSite.MappingManager, mAveParentSite.SourceSiteInfo, mAveParentSite.ServerRelativeUrl, this.mAveSPList.SPList, ref needReplaceLast);
                    if (needReplaceLast)
                    {
                        mAveSPList.ParentWeb.ParentSite.AddUnReplaceUrlIDCache(mAveSPList.ParentWeb.SPWeb.ID, mAveSPList.SPList.ID, docRowId, spField.InternalName);
                    }
                    return xmlLinks;
                }
                //if (spField.TypeAsString.Equals("Note", StringComparison.OrdinalIgnoreCase))
                //{
                //    string noteLinks = ReplaceNoteLinks(value.ToString(), mAveParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, mAveParentSite.SourceSiteInfo, mAveParentSite.ServerRelativeUrl);
                //    return noteLinks;
                //}
                if (spField.TypeAsString.Equals("SendTo", StringComparison.OrdinalIgnoreCase))
                {
                    return GetMutiUserValueById(value.ToString());
                }
                if (spField.TypeAsString.Equals("Boolean", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(Convert.ToString(value)))
                {
                    return null;
                }
                if (spField.TypeAsString.Equals("Number", StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(spField.GetProperty("Min")) && Convert.ToDouble(value) < Convert.ToDouble(spField.GetProperty("Min")))
                    {
                        return null;
                    }
                    if (!string.IsNullOrEmpty(spField.GetProperty("Max")) && Convert.ToDouble(value) > Convert.ToDouble(spField.GetProperty("Max")))
                    {
                        return null;
                    }
                }

                if (!isSetByCustomMapping)
                {
                    var userField = spField as IAveFieldUser;
                    if (userField != null && value != null)
                    {
                        if (!userField.AllowMultipleValues)
                        {
                            return mAveSPList.ParentWeb.ParentSite.SPMembers.FindMemberId((int)value);
                        }
                        return GetMutiUserValueById(value.ToString());
                    }
                    if (spField.Type == AveFieldType.Choice || spField.Type == AveFieldType.MultiChoice)
                    {//对于没有配置过custom mapping的choice value，如果目的端field下的choice里面没有这个choice，则不进行还原
                        IAveFieldMultiChoice choiceField = spField as IAveFieldMultiChoice;
                        if (!value.ToString().Contains(";#") && !choiceField.Choices.Contains(value.ToString()) && !choiceField.FillInChoice)
                        {
                            return null;
                        }
                    }

                    if (spField.BaseTypeString.Equals("Lookup", StringComparison.OrdinalIgnoreCase) || spField.BaseTypeString.Equals("LookupMulti", StringComparison.OrdinalIgnoreCase))
                    {
                        if (value.ToString().Contains(';'))
                        {
                            value = value.ToString().Split(';')[0];
                        }
                        AveLookupObject obj;
                        mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.TryGetValueFromLookupFieldMapping(mAveSPList.SPList.ID, spField.ID, out obj);
                        if (mLookupFieldIdValue == null)
                        {
                            mLookupFieldIdValue = new List<AveLookupFieldInfo>();
                        }
                        if (obj != null)
                        {
                            var listId = obj.ListId; //new Guid(((IAveFieldLookup)spField).LookupList);
                            if (!string.IsNullOrEmpty(obj.SourceListId))
                            {
                                var valueId = Guid.Empty;
                                if (mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.GetValueFromListIdMapping(new Guid(obj.SourceListId), out valueId))
                                {
                                    listId = valueId;
                                }
                            }
                            int itemId = mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.GetMappingItemId(listId, Convert.ToInt32(value));
                            if (itemId != -1)
                            {
                                return itemId.ToString();
                            }
                            if (value != null)
                            {
                                var lookupFieldIdValue = new AveLookupFieldInfo
                                {
                                    LookupListID = new Guid(obj.SourceListId),
                                    LookupFieldID = spField.ID,
                                    Version = version,
                                    LookupFieldValue = new ArrayList { Convert.ToInt32(value) },
                                };
                                mLookupFieldIdValue.Add(lookupFieldIdValue);
                            }
                            if (WrapperConfiguration.UpdateLookupColumnValueBeforePost)
                            {
                                return value;
                            }
                            else
                            {
                                return null;
                            }
                        }
                        else
                        {
                            /*对于replicator做increment job时,可能会由于list的field没有变化,而没有备份还原list上的field,所以obj就会是null.
                             *但是有可能在lista中新建一个item,在listb中新建一个item lookup到lista中新建的item,此时有两种情况:1. lista在listb之前还原,
                             *此时还原listb中item的lookup field value时,应该能找到对应目的端对应的item;2. lista在listb之后还原,此时还原lista中item的lookupfield的时候,
                             *就会涉及到要在PostAction中来还原了
                            */
                            /*
                             * 对于sealed的lookup field,在此obj也会是null的。
                             */
                            if (((IAveFieldLookup)spField).LookupList.Equals("AppPrincipals", StringComparison.OrdinalIgnoreCase))
                            {
                                return value;
                            }
                            Guid listId = new Guid(((IAveFieldLookup)spField).LookupList);
                            if (!listId.Equals(Guid.Empty) && !String.IsNullOrEmpty(value.ToString()))
                            {
                                int itemId = mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.GetMappingItemId(mAveSPList.SPList.ID, Convert.ToInt32(value));
                                //如果lista在listb之前还原,就有可能找到正确的lookup关系
                                if (itemId != -1)
                                {
                                    return itemId.ToString();
                                }
                                //如果没有找到正确的对应关系,将将其加入到PostAction中,需要注意此时lookupID传的是listId,而不是obj.List,所以在PostAction中还原的时候需要稍加处理
                                else
                                {
                                    if (!String.IsNullOrEmpty(value.ToString()))
                                    {
                                        ArrayList list = new ArrayList();
                                        list.Add(Convert.ToInt32(value));
                                        //mAveSPWeb.ParentSite.AddNotUpdateLookupFieldValue(listId, mAveSPWeb.SPWeb.ID, mAveSPList.SPList.ID, docRowId, version, spField.Id, list);
                                        AveLookupFieldInfo lookupFieldIdValue = new AveLookupFieldInfo
                                        {
                                            LookupListID = listId,
                                            LookupFieldID = spField.ID,
                                            Version = version,
                                            LookupFieldValue = list,
                                        };
                                        mLookupFieldIdValue.Add(lookupFieldIdValue);
                                        return null;
                                    }
                                }
                            }
                            log.Info("Cannot find lookupObject. FieldName:{0},  FieldDisplayName:{1}.", spField.InternalName, spField.Title);
                        }
                    }
                    if (spField.BaseTypeString.Equals("TaxonomyFieldType", StringComparison.OrdinalIgnoreCase) || spField.BaseTypeString.Equals("TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase))
                    {
                        if (XmlFields.ContainsKey(key) && XmlFields[key].Type == AveFieldType.Lookup)
                        {
                            if (value.ToString().Contains(";"))
                            {
                                return value.ToString().Split(';')[1];
                            }
                        }
                        value = value.ToString();
                    }
                }
                return value;
            }
        }

        private List<string> GetNotesUrlValues(string fieldValue)
        {
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.GetNotesUrlValues"))
            {
                int fieldLength = fieldValue.Length;
                Dictionary<string, string> noteUrlValueDic = new Dictionary<string, string>();
                List<string> urlValueList = new List<string>();
                noteUrlValueDic.Add("www.", " ");
                noteUrlValueDic.Add("http://", " ");
                noteUrlValueDic.Add("https://", " ");
                noteUrlValueDic.Add("\"www.", "\"");
                noteUrlValueDic.Add("\"http://", "\"");
                noteUrlValueDic.Add("\"https://", "\"");
                foreach (KeyValuePair<string, string> key in noteUrlValueDic)
                {
                    int i = 0;
                    while (i < fieldLength)
                    {
                        int startUrlIndex = fieldValue.IndexOf(key.Key, i, StringComparison.OrdinalIgnoreCase);
                        if (startUrlIndex != -1)
                        {
                            int endUrlIndex = fieldValue.IndexOf(key.Value, startUrlIndex + 1, StringComparison.OrdinalIgnoreCase);
                            if (endUrlIndex < 0)
                            {
                                string url2 = fieldValue.Substring(startUrlIndex);
                                urlValueList.Add(url2);
                                break;
                            }
                            string url = fieldValue.Substring(startUrlIndex, endUrlIndex - startUrlIndex);
                            i = endUrlIndex;
                            urlValueList.Add(url);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                return urlValueList;
            }
        }


        /// <summary>
        /// 这个方法是重新设置PostAction中的LookupFields
        /// </summary>
        /// <param name="docRowId"></param>
        public void ResetNotUpdateLookupFieldValue(int docRowId)
        {
            if (mLookupFieldIdValue != null)
            {
                foreach (AveLookupFieldInfo lookupFieldIdValue in mLookupFieldIdValue)
                {
                    mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.AddNotUpdateLookupFieldValue(lookupFieldIdValue.LookupListID, mAveSPWeb.SPWeb.ID, mAveSPList.SPList.ID, docRowId, lookupFieldIdValue.Version, lookupFieldIdValue.LookupFieldID, lookupFieldIdValue.LookupFieldValue);
                }
                mLookupFieldIdValue.Clear();
            }
        }
        public void ResetNintexFormDataFieldValue(int rowId)
        {
            if (mNintexFormDataVale == null)
            {
                return;
            }
            mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.AddNintexFormDatatoCache(mAveSPWeb.SPWeb.ServerRelativeUrl, mAveSPList.SPList.ID, rowId, mNintexFormDataVale.Version, mNintexFormDataVale.FormData);
            mNintexFormDataVale = null;
        }
        public void ResetNotUpdateUrlFieldValue(int docRowId)
        {
            if (mUrlFieldValue != null)
            {
                foreach (AveUrlFieldInfo urlFieldVaule in mUrlFieldValue)
                {
                    mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.AddDurableLinkCache(mAveSPWeb.SPWeb.ID, mAveSPList.SPList.ID, docRowId, urlFieldVaule.Version, urlFieldVaule.FieldId, urlFieldVaule.SourceItemId);
                }
                mUrlFieldValue.Clear();
            }
        }

        #region Url Field Value

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "/_catalogs/masterpage")]
        private IAveFieldUrlValue GetUrlValue(string key, object value, Dictionary<string, object> data, int docRowId, IAveField spField)
        {
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.GetUrlValue"))
            {
                bool needSiteCollectionLevel = mAveSPList.SPList != null
                    && (mAveSPList.SPList.BaseTemplate == AveListTemplateType.DesignCatalog
                         && (spField.InternalName.Equals("ThemeUrl", StringComparison.Ordinal)
                             || spField.InternalName.Equals("FontSchemeUrl", StringComparison.Ordinal)
                            )
                        );
                var urlValue = mAveParentSite.ObjectModelFactory.CreateFieldUrlValue();
                string url = value.ToString();
                if (url.StartsWith(mAveParentSite.SourceSiteInfo.Url, StringComparison.OrdinalIgnoreCase))
                {
                    if (mAveParentSite.SourceSiteInfo.ServerRelativeUrl.Equals("/", StringComparison.OrdinalIgnoreCase))
                    {
                        url = url.Substring(mAveParentSite.SourceSiteInfo.Url.Length);
                    }
                    else
                    {
                        url = url.Replace(mAveParentSite.SourceSiteInfo.Url, mAveParentSite.SourceSiteInfo.ServerRelativeUrl);
                    }
                }
                urlValue.Url = ReplaceUrl(docRowId, spField, url, needSiteCollectionLevel && url.IndexOf("/_catalogs/theme/", StringComparison.OrdinalIgnoreCase) > 0);

                var descriptionKey = key + "#2";
                if (data.ContainsKey(descriptionKey))
                {
                    string description = data[descriptionKey].ToString();
                    var isSetByCustomMapping = false;
                    description = GetCustomMappingValue(description, "", Convert.ToInt32(data["#tp_ID"]), spField, key, ref isSetByCustomMapping).ToString();
                    if (HttpUtility.UrlDecode(description).Equals(HttpUtility.UrlDecode(url), StringComparison.OrdinalIgnoreCase))
                    {
                        description = urlValue.Url;
                    }
                    else if (description.StartsWith("http:", StringComparison.OrdinalIgnoreCase) ||
                             description.StartsWith("https:", StringComparison.OrdinalIgnoreCase))
                    {
                        description = ReplaceUrl(docRowId, spField, description, needSiteCollectionLevel && description.IndexOf("/_catalogs/theme/", StringComparison.OrdinalIgnoreCase) > 0);
                    }
                    urlValue.Description = description;
                }

                //wiki page中的PublishingPageLayout，应该指向的是root site上masterpage中的文件,当做root site到sub site的mapping时，
                //UrlReplace替换成指向sub site上的masterpage，导致还原后的wiki page打开出错，在此处理这种case，
                //将PublishingPageLayout指向的Url换成指向root site上masterpage中的文件。
                if (spField.InternalName == "PublishingPageLayout")
                {
                    if (urlValue.Url.Contains("/_catalogs/masterpage"))
                    {
                        string temUrl = mAveSPWeb.ParentSite.SPSite.ServerRelativeUrl.TrimEnd('/') +
                                        "/_catalogs/masterpage";
                        if (!urlValue.Url.StartsWith(temUrl, StringComparison.OrdinalIgnoreCase))
                        {
                            urlValue.Url = mAveSPWeb.ParentSite.SPSite.ServerRelativeUrl.TrimEnd('/') +
                                           urlValue.Url.Substring(
                                               urlValue.Url.IndexOf("/_catalogs/masterpage",
                                                                    StringComparison.OrdinalIgnoreCase));
                        }
                    }
                }
                return urlValue;
            }
        }

        private string ReplaceUrl(int docRowId, IAveField spField, string url, bool siteCollectionLevel)
        {
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.ReplaceUrl"))
            {
                if (siteCollectionLevel)
                {
                    url = AveReplaceProcessor.UrlReplace(url, mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.SiteUrlMapping,
                                                                 new ReplaceOption(true, true), mAveParentSite.SourceSiteInfo, mAveParentSite.ServerRelativeUrl);
                }
                else
                {
                    url = AveReplaceProcessor.UrlReplace(url, mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings,
                                                                 new ReplaceOption(true, true), mAveParentSite.SourceSiteInfo, mAveParentSite.ServerRelativeUrl);
                }
                if (url.Contains("?")) //替换Url中的Id
                {
                    bool needReplaceLast = false;
                    url = AveReplaceProcessor.IdReplace(url, mAveSPWeb.ParentSite.MappingManager, ref needReplaceLast);
                    if (needReplaceLast)
                    {
                        mAveSPWeb.ParentSite.AddUnReplaceUrlIDCache(mAveSPWeb.SPWeb.ID,
                                                                    mAveSPList.SPList.ID, docRowId,
                                                                    spField.InternalName);
                    }
                }
                return url;
            }
        }

        #endregion

        #region Get Custom Mapping Value

        private object GetCustomMappingValue(object value, string docName, int srcRowId, IAveField spField, string fieldName, ref bool isSetByCustomMapping)
        {
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.GetCustomMappingValue"))
            {
                var valueInfo = new AveSourceFieldValueInfo { SourceItemName = docName, SourceItemRowId = srcRowId };

                if (XmlFields != null && XmlFields.ContainsKey(fieldName))
                {
                    var xmlField = XmlFields[fieldName];
                    string sourceValue = value != null ? value.ToString() : null;
                    if (xmlField.Type == AveFieldType.Lookup)
                    {
                        if (xmlField.CustomFieldInfo != null && !String.IsNullOrEmpty(sourceValue))
                        {
                            if (sourceValue.Contains(';'))
                            {
                                value = sourceValue.Substring(sourceValue.IndexOf(';') + 1);
                            }
                            //Merge CI NET-11584 用于处理look up column的目标已经被删除，但是mapping后的metadata column在还原后有值的问题。如果value中没有‘;’,即认为其不是一个有效的lookup 类型column值。进行特殊处理，还原为null，来保证目的端不会出现数据。
                            else if (spField.TypeAsString.Equals("TaxonomyFieldType", StringComparison.OrdinalIgnoreCase) || spField.TypeAsString.Equals("TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase))
                            {
                                value = null;
                            }
                        }
                    }
                    if (!xmlField.TypeAsString.Equals(spField.TypeAsString, StringComparison.OrdinalIgnoreCase) &&
                        (spField.TypeAsString.Equals("TaxonomyFieldType", StringComparison.OrdinalIgnoreCase) || spField.TypeAsString.Equals("TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase)))
                    {
                        needForceCreateTerm = true;
                    }
                    if (xmlField.CustomFieldInfo != null)
                    {
                        valueInfo.SourceFieldInfo = new AveSourceFieldInfo
                        {
                            SourceDisplayName = xmlField.Title,
                            SourceInternalName = xmlField.FieldInternalName,
                            SourceType = xmlField.Type,
                            SourceTypeAsString = xmlField.TypeAsString
                        };
                        AveCustomFieldInfo customMappingInfo = xmlField.CustomFieldInfo;
                        value = CustomMappingValue(value, spField, customMappingInfo, valueInfo, ref isSetByCustomMapping);
                        if (!isSetByCustomMapping && xmlField.Type == AveFieldType.Lookup && spField.Type == AveFieldType.Lookup)
                        {
                            return sourceValue;
                        }
                        else
                        {
                            return value;
                        }
                    }
                }
                else
                {
                    if (spField.TypeAsString.Equals("TaxonomyFieldType", StringComparison.OrdinalIgnoreCase) || spField.TypeAsString.Equals("TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase))
                    {
                        needForceCreateTerm = true;
                    }
                    AveCustomFieldInfo customMappingInfo = null;
                    var tmpFields = FieldMapping.GetNewFieldsBeforeAdd();
                    if (tmpFields != null)
                    {
                        customMappingInfo = tmpFields.FirstOrDefault(info => string.Equals(info.InternalName, fieldName));
                    }
                    if (customMappingInfo != null)
                    {
                        valueInfo.SourceFieldInfo = new AveSourceFieldInfo
                        {
                            SourceDisplayName = spField.Title,
                            SourceInternalName = spField.InternalName,
                            SourceType = spField.Type
                        };
                        return CustomMappingValue(value, spField, customMappingInfo, valueInfo, ref isSetByCustomMapping);
                    }
                }
                return value;
            }
        }

        [Obsolete]
        public string CustomMappingValue(AveSourceFieldValueInfo sourceFieldValueInfo, ref bool isMapped)
        {
            string sourceValue = sourceFieldValueInfo.SourceValue.ToString();
            string mappingValue = sourceFieldValueInfo.SourceValue.ToString();
            if (sourceFieldValueInfo.SourceFieldInfo.SourceType == AveFieldType.Lookup)
            {
                if (!string.IsNullOrEmpty(mappingValue))
                {
                    sourceFieldValueInfo.SourceValue = mappingValue.Substring(sourceValue.IndexOf(';') + 1);
                }
            }
            mappingValue = FieldMapping.GetMappingValue(sourceFieldValueInfo);
            if (mappingValue == null)
            {
                return sourceValue;
            }
            isMapped = true;
            return mappingValue;
        }

        /// <returns>如果没有mapping成功 返回原始值</returns>
        [Obsolete]
        public object CustomMappingValue(object value, IAveField spField, AveCustomFieldInfo info, AveSourceFieldValueInfo sourceFieldValueInfo, ref bool isSetByCustomMapping)
        {
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.CustomMappingValue"))
            {
                if (info == null)
                    return value;

                string srcValue = value == null ? string.Empty : value.ToString();
                try
                {
                    string userstring = "";
                    string userName = string.Empty;
                    if (info.SourceType.Equals(AveFieldType.User))
                    {
                        srcValue = GetLoginNameFromId(srcValue, ref userName);
                        if (srcValue.StartsWith("i:0#.w|", StringComparison.OrdinalIgnoreCase))
                        {
                            userstring = "i:0#.w|";
                            srcValue = srcValue.Substring(userstring.Length);
                        }
                    }
                    else if (info.SourceType == AveFieldType.URL)
                    {
                        var sourceSiteInfo = mAveParentSite.SourceSiteInfo;
                        //如果Site collection是hostheader，不能用web app url拼absolute url,直接用Site url拼。
                        string sourceWebAppUrl = sourceSiteInfo.IsHostheader ? sourceSiteInfo.Url : sourceSiteInfo.WebAppUrl;

                        //link 的 url，如果是本web app，备份的值是相对Url，如果是其他web app的url，则为绝对Url。
                        if (!srcValue.StartsWith("Http", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(mAveParentSite.SourceSiteInfo.WebAppUrl) && !srcValue.StartsWith(sourceWebAppUrl, StringComparison.OrdinalIgnoreCase))
                        {
                            srcValue = mAveParentSite.SourceSiteInfo.WebAppUrl.TrimEnd('/') + '/' + srcValue.TrimStart('/');
                        }
                    }
                    var splitChar = GetSplitChar(info);
                    if (sourceFieldValueInfo.SourceFieldInfo.SourceType == AveFieldType.MultiChoice)
                    {//ADO-66079，对于源端是多值Choice的，需要设置默认分隔符对其做各个单值的mapping
                        splitChar = ";#";
                    }
                    //ADO-128341,对于源端是多值的metadata column如果没有分隔符，那么将分隔符设置成；
                    if (string.IsNullOrEmpty(splitChar) && sourceFieldValueInfo.SourceFieldInfo.SourceTypeAsString.Equals("TaxonomyFieldTypeMulti", StringComparison.Ordinal))
                    {
                        splitChar = ";";
                    }
                    //ADO-116315,对于HTML类型的column value多行时在library中分隔符sp10s是\r\n，sp13是\n，在list中sp10中分隔符是\r\n，sp13中则没有分隔符
                    if (sourceFieldValueInfo.SourceFieldInfo.SourceTypeAsString.Equals("HTML", StringComparison.OrdinalIgnoreCase))
                    {
                        if (srcValue.Contains("\r\n"))
                            splitChar = "\r\n";
                        else if (srcValue.Contains("\n"))
                            splitChar = "\n";
                        else if (srcValue.Length > 7)
                        {
                            srcValue = srcValue.Substring(3, srcValue.Length - 7);
                            splitChar = "</p><p>";
                        }
                        else
                            splitChar = "\r\n";
                        if (this.mAveSPList.SPList.BaseType == AveBaseType.GenericList)
                        {
                            string[] splitValues = srcValue.Split(new[] { splitChar }, StringSplitOptions.RemoveEmptyEntries);
                            srcValue = string.Empty;
                            for (int i = 0; i < splitValues.Length; i++)
                            {
                                if (splitValues[i].StartsWith("<p>", StringComparison.OrdinalIgnoreCase) && splitValues[i].EndsWith("</p>", StringComparison.OrdinalIgnoreCase))
                                {
                                    srcValue = srcValue + splitValues[i].Substring(3, splitValues[i].Length - 7) + splitChar;
                                }
                                else
                                {
                                    srcValue = srcValue + splitValues[i] + splitChar;
                                }
                            }
                            srcValue = srcValue.Substring(0, srcValue.Length - splitChar.Length);
                        }
                    }
                    //ADO-125080,对于源端是note类型的column mapping成text类型的column只需要处理第一个value
                    //Note类型的column，SP10 value的分隔符为\r\n，SP13 value的分隔符为\n
                    if (sourceFieldValueInfo.SourceFieldInfo.SourceType == AveFieldType.Note)
                    {
                        if (srcValue.Contains("\r\n"))
                            splitChar = "\r\n";
                        else if (srcValue.Contains("\n"))
                            splitChar = "\n";
                        else
                            splitChar = "\r\n";

                        if (this.mAveSPList.SPList.BaseType == AveBaseType.GenericList)
                        {
                            XmlDocument xmlDoc = new XmlDocument();
                            xmlDoc.LoadXml(srcValue);
                            srcValue = string.Empty;
                            if (xmlDoc.ChildNodes.Count > 0 && xmlDoc.ChildNodes[0].ChildNodes.Count > 0)
                            {
                                if (spField.TypeAsString.Equals("Text"))
                                {
                                    srcValue = xmlDoc.ChildNodes[0].ChildNodes[0].InnerText;
                                    splitChar = string.Empty;
                                }
                                else
                                {
                                    foreach (XmlNode node in xmlDoc.ChildNodes[0].ChildNodes)
                                    {
                                        if (!string.IsNullOrEmpty(node.InnerText) && node.Name.Equals("p"))
                                        {
                                            srcValue = srcValue + node.InnerText + splitChar;
                                        }
                                    }
                                    srcValue = srcValue.Substring(0, srcValue.Length - splitChar.Length);
                                }
                            }
                        }
                        else if (spField.TypeAsString.Equals("Text"))
                        {
                            srcValue = srcValue.Split(new[] { splitChar }, StringSplitOptions.RemoveEmptyEntries).Length > 0 ? srcValue.Split(new[] { splitChar }, StringSplitOptions.RemoveEmptyEntries)[0] : srcValue;
                            splitChar = string.Empty;
                        }
                    }
                    //部分value前面有asc码值为8203的字符，导致value不能被mapping
                    if (srcValue != string.Empty && srcValue.Length > 1 && srcValue[0] == 8203)
                        srcValue = srcValue.Substring(1, srcValue.Length - 1);
                    var mappingValue = GetMappingValue(srcValue, splitChar, sourceFieldValueInfo);
                    if (mappingValue == null && spField.Type == AveFieldType.Lookup)
                    {//ADO-77874 对于目的端是lookup的，如果没有配置value mapping，则用源端value去反找id
                        mappingValue = value.ToString();
                    }
                    if (mappingValue == null)
                    {
                        if (splitChar != null && ((spField.TypeAsString.Equals("TaxonomyFieldType", StringComparison.Ordinal) || (spField.TypeAsString.Equals("TaxonomyFieldTypeMulti", StringComparison.Ordinal)))
                                && srcValue.Contains(splitChar)))
                        {
                            return srcValue.Replace(splitChar, ";");
                        }
                        //比如，当源端是user类型column，目的端是Boolean，number，currency，返回空。
                        //如果今后还有相关bug，在if后加相关类型判断
                        if (info.SourceType.Equals(AveFieldType.User) || info.SourceType.Equals(AveFieldType.Note) || info.SourceType.Equals(AveFieldType.Lookup) || info.SourceType.Equals(AveFieldType.Choice) || info.SourceType.Equals(AveFieldType.Text))
                        {
                            switch (spField.TypeAsString)
                            {
                                case "Text":
                                    if (info.SourceType.Equals(AveFieldType.User))
                                    {
                                        return userName;
                                    }
                                    if (info.SourceType.Equals(AveFieldType.Note))
                                    {
                                        return srcValue;
                                    }
                                    break;
                                case "Note":
                                case "Choice":
                                case "MultiChoice":
                                case "TaxonomyFieldType":
                                case "TaxonomyFieldTypeMulti":
                                    if (info.SourceType.Equals(AveFieldType.User))
                                    {
                                        return userName;
                                    }
                                    break;
                                case "Boolean":
                                case "Number":
                                case "Currency":
                                    return null;
                            }
                        }
                        return value;
                    }
                    isSetByCustomMapping = true;
                    //ADO-113050:存在 Mapping value时，含有“;”value的mapping错误。会将；替换成；#。这段逻辑目前没有用到，暂时去掉。
                    //if (string.IsNullOrEmpty(splitChar) && mappingValue.Contains(";")
                    //     && (info.CustomFieldTypeAsString.Equals(AveCustomFieldType.SameType.ToString(), StringComparison.OrdinalIgnoreCase)
                    //     || info.CustomFieldTypeAsString.Equals(AveCustomFieldType.ChangeToDes.ToString(), StringComparison.OrdinalIgnoreCase)))
                    //{
                    //    splitChar = ";";
                    //}
                    if (!string.IsNullOrEmpty(splitChar))
                    {
                        mappingValue = mappingValue.Replace(splitChar, ";#");
                    }

                    switch (spField.TypeAsString)
                    {
                        case "Lookup":
                        case "LookupMulti":
                            mappingValue = GetLookUpItemIdByMappingValue(mappingValue, spField as IAveFieldLookup);
                            if (String.IsNullOrEmpty(mappingValue))
                            {
                                isSetByCustomMapping = false;
                            }
                            log.Debug("Get value mapping of lookup.value:" + mappingValue);
                            break;
                        case "TaxonomyFieldType":
                        case "TaxonomyFieldTypeMulti":
                            return mappingValue.Replace(";#", ";");
                        case "Choice":
                        case "MultiChoice":
                            string[] values = mappingValue.Split(new[] { ";#" }, StringSplitOptions.RemoveEmptyEntries);
                            mappingValue = string.Empty;
                            IAveFieldMultiChoice choiceField = spField as IAveFieldMultiChoice;
                            List<string> addedChoices = new List<string>();
                            foreach (string temValue in values)
                            {
                                if (!string.IsNullOrEmpty(temValue))
                                {
                                    if ((choiceField.Choices.Contains(temValue) || choiceField.FillInChoice) && !addedChoices.Contains(temValue))
                                    {
                                        addedChoices.Add(temValue);
                                        mappingValue = mappingValue + temValue + ";#";
                                        if (spField.TypeAsString.Equals("Choice", StringComparison.OrdinalIgnoreCase))
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                            if (mappingValue.EndsWith(";#", StringComparison.OrdinalIgnoreCase))
                            {
                                mappingValue = mappingValue.Substring(0, mappingValue.Length - 2);
                            }
                            return mappingValue;
                        case "UserMulti":
                            return GetMutiUserValueByName(mappingValue);
                        case "User":
                            string[] users = mappingValue.Split(new[] { ";#" }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string user in users)
                            {
                                string tempUser = user;
                                if (!tempUser.StartsWith("i:0#.w|", StringComparison.OrdinalIgnoreCase))
                                {
                                    tempUser = userstring + tempUser;
                                }
                                return mAveSPWeb.GetUserIdByName(tempUser, true);
                            }
                            break;
                        case "Text":
                        case "Note":
                            if (sourceFieldValueInfo.SourceFieldInfo.SourceType == AveFieldType.MultiChoice)
                            {
                                mappingValue = mappingValue.Replace(";#", ";");
                            }
                            if (sourceFieldValueInfo.SourceFieldInfo.SourceType == AveFieldType.Note)
                            {
                                if (spField.TypeAsString.Equals("Text"))
                                {
                                    return mappingValue;
                                }
                                if (this.mAveSPList.SPList.BaseType == AveBaseType.GenericList)
                                {
                                    XmlDocument xmlDoc = new XmlDocument();
                                    xmlDoc.LoadXml(value.ToString());
                                    string[] newValues = mappingValue.Split(new[] { ";#" }, StringSplitOptions.RemoveEmptyEntries);
                                    if (xmlDoc.ChildNodes.Count > 0)
                                    {
                                        for (int i = xmlDoc.ChildNodes[0].ChildNodes.Count - 1; i > -1; i--)
                                        {
                                            //当原values的个数多余被mapping到的alues的个数时，去掉多余的节点，只返回被mapping到的value
                                            if (i > newValues.Length - 1)
                                            {
                                                xmlDoc.ChildNodes[0].RemoveChild(xmlDoc.ChildNodes[0].ChildNodes[i]);
                                            }
                                            else
                                            {
                                                xmlDoc.ChildNodes[0].ChildNodes[i].InnerText = newValues[i];
                                            }
                                        }
                                    }
                                    return xmlDoc.InnerXml;
                                }
                                else
                                {
                                    mappingValue = mappingValue.Replace(";#", splitChar);
                                }
                            }
                            return mappingValue;
                        case "Boolean":
                            //对于源端是多值的，选择第一个mapping value 作为其mapping值
                            string[] bools = mappingValue.Split(new[] { ";#" }, StringSplitOptions.RemoveEmptyEntries);
                            if (bools.Length > 0)
                            {
                                mappingValue = bools[0];
                            }
                            if (!mappingValue.Equals("true", StringComparison.OrdinalIgnoreCase) && !mappingValue.Equals("false", StringComparison.OrdinalIgnoreCase))
                            {
                                mappingValue = null;
                            }
                            break;
                        case "HTML":
                            string[] htmls = mappingValue.Split(new[] { ";#" }, StringSplitOptions.RemoveEmptyEntries);
                            string newMappingValue = string.Empty;
                            if (htmls.Length > 1)
                            {
                                for (int i = 0; i < htmls.Length; i++)
                                {
                                    if (this.mAveSPList.SPList.BaseType == AveBaseType.GenericList)
                                    {
                                        newMappingValue = newMappingValue + "<p>" + htmls[i] + "</p>";
                                    }
                                    else
                                    {
                                        newMappingValue = newMappingValue + htmls[i];
                                    }
                                    if (!splitChar.Equals("</p><p>", StringComparison.OrdinalIgnoreCase))
                                    {
                                        newMappingValue = newMappingValue + splitChar;
                                    }
                                }
                                return newMappingValue.Substring(0, newMappingValue.Length - 2);
                            }
                            break;
                        case "Number":
                            if (string.IsNullOrEmpty(mappingValue))
                            {
                                return null;
                            }
                            else
                            {
                                return Convert.ToDouble(mappingValue);
                            }
                    }
                    return mappingValue;
                }
                catch (Exception ex)
                {
                    log.Error("Get Custom Mapping Value Error.value:{0} Exception:{1}", srcValue, ex.ToString());
                }
                return srcValue;
            }
        }

        private string GetMappingValue(string srcValue, string splitChar, AveSourceFieldValueInfo sourceFieldValueInfo)
        {
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.GetMappingValue"))
            {
                string mappingValue = "";
                if (!string.IsNullOrEmpty(splitChar) && srcValue.Contains(splitChar))
                {
                    string[] values = srcValue.Split(new[] { splitChar }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string value in values)
                    {
                        sourceFieldValueInfo.SourceValue = value;
                        string temp = FieldMapping.GetMappingValue(sourceFieldValueInfo);
                        if (!string.IsNullOrEmpty(temp))
                        {
                            mappingValue = mappingValue + temp + splitChar;
                        }
                        else if (sourceFieldValueInfo.SourceFieldInfo.SourceType == AveFieldType.MultiChoice)
                        {
                            mappingValue = mappingValue + value + splitChar;
                        }
                    }
                    if (mappingValue.Length > 0)
                    {
                        mappingValue = mappingValue.Substring(0, mappingValue.Length - splitChar.Length);
                    }
                    else
                    {
                        mappingValue = null;
                    }
                }
                else
                {
                    sourceFieldValueInfo.SourceValue = srcValue;
                    mappingValue = FieldMapping.GetMappingValue(sourceFieldValueInfo);
                }
                return mappingValue;
            }
        }

        private string GetLookUpItemIdByMappingValue(string mappedValue, IAveFieldLookup field)
        {
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.GetLookUpItemIdByMappingValue"))
            {
                var lookupValue = new StringBuilder();
                try
                {
                    if (!lookUpListItemIDAndValues.ContainsKey(field.LookupList) || !lookUpListItemIDAndValues[field.LookupList].ContainsKey(field.LookupField))
                    {
                        var idValue = new Dictionary<string, string>();
                        IAveWeb web = mAveSPWeb.SPWeb;
                        try
                        {
                            if (mAveSPWeb.SPWeb.ID != field.LookupWebId)
                            {
                                web = mAveParentSite.SPSite.OpenWeb(field.LookupWebId);
                            }
                            IAveList lookupList = web.Lists[new Guid(field.LookupList)];
                            foreach (IAveListItem item in lookupList.Items)
                            {
                                if (item[field.LookupField] != null)
                                {
                                    idValue[item[field.LookupField].ToString()] = item.ID.ToString();
                                }
                            }
                            foreach (IAveListItem item in lookupList.Folders)
                            {
                                if (item[field.LookupField] != null)
                                {
                                    idValue[item[field.LookupField].ToString()] = item.ID.ToString();
                                }
                            }
                        }
                        finally
                        {
                            if (web != mAveSPWeb.SPWeb)
                            {
                                web.Dispose();
                            }
                        }
                        log.Debug("Get id value.count:" + idValue.Count);
                        if (lookUpListItemIDAndValues.ContainsKey(field.LookupList))
                        {
                            lookUpListItemIDAndValues[field.LookupList].Add(field.LookupField, idValue);
                        }
                        else
                        {
                            var tempDic = new Dictionary<string, Dictionary<string, string>>();
                            tempDic.Add(field.LookupField, idValue);
                            lookUpListItemIDAndValues.Add(field.LookupList, tempDic);
                        }
                    }
                    string[] values = mappedValue.Split(new[] { ";#", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    if (values.Length == 1 && lookUpListItemIDAndValues[field.LookupList][field.LookupField].ContainsKey(values[0]))
                    {
                        return lookUpListItemIDAndValues[field.LookupList][field.LookupField][values[0]];
                    }
                    foreach (string value in values)
                    {
                        if (!lookUpListItemIDAndValues[field.LookupList][field.LookupField].ContainsKey(value))
                        {
                            continue;
                        }
                        lookupValue.AppendFormat("{0};#{1};#", lookUpListItemIDAndValues[field.LookupList][field.LookupField][value], value);
                    }
                    if (lookupValue.Length > 0)
                    {
                        lookupValue.Length -= 2;
                    }
                }
                catch (Exception ex)
                {
                    if (this.mAveParentSite.xmlFieldCache.ContainsKey(field.ID))
                    {
                        this.mAveParentSite.xmlFieldCache[field.ID] = XmlFields[field.InternalName];
                    }
                    else
                    {
                        this.mAveParentSite.xmlFieldCache.Add(field.ID, XmlFields[field.InternalName]);
                    }
                    log.Error("Get Lookup Mapping Value Error.value:{0}.  Exception:{1}", mappedValue, ex.ToString());
                }
                return lookupValue.ToString();
            }
        }

        private string GetMutiUserValueByName(string mappingValue)
        {
            return GetMutiUserValue(mappingValue, true);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "The wrong word is method name.")]
        private string GetMutiUserValue(string srcValue, bool isNameOrId)
        {
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.GetMutiUserValue"))
            {
                log.Debug("User srcValue:" + srcValue);
                var userValue = new StringBuilder();
                string[] values = srcValue.Split(new[] { ";#" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var value in values)
                {
                    try
                    {
                        if (isNameOrId)
                        {
                            int userId = mAveSPWeb.GetUserIdByName(value, true);
                            if (userId != -1)
                            {
                                userValue.AppendFormat("{0};#{1};#", userId, value);
                            }
                        }
                        else
                        {
                            int userIdSrc;
                            if (int.TryParse(value, out userIdSrc))
                            {
                                var user = mAveSPList.ParentWeb.ParentSite.SPMembers.FindMember(userIdSrc, true);
                                if (user != null && user.LoginName != null)
                                {
                                    userValue.AppendFormat("{0};#{1};#", user.ID, user.LoginName);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("Get User Mapping Value Error.value:{0} Exception:{1}", value, ex.ToString());
                    }
                }
                return userValue.ToString();
            }
        }

        public string GetLoginNameFromId(string sourceUserValue, ref string sourceUserName)
        {
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.GetLoginNameFromId"))
            {
                int principalID;
                string sourceLoginName = string.Empty;
                if (int.TryParse(sourceUserValue, out principalID))
                {
                    IAvePrincipal principal = mAveSPList.ParentSite.SPMembers.FindMember(principalID, true, false);
                    if (principal != null)
                    {
                        string newsourceLoginName = principal.LoginName;
                        sourceLoginName = mAveSPList.ParentSite.SPMembers.GetMappingUserLogin(newsourceLoginName);
                        sourceUserName = principal.Name;
                    }
                    else
                    {
                        AveSPMemberInfo memberInfo = mAveSPList.ParentSite.SPMembers.UserAndDomainMapping.GetUserMapping(principalID) as AveSPMemberInfo;
                        if (memberInfo != null)
                        {
                            if (memberInfo.SourceInfo is AveUserInfo)
                            {
                                sourceLoginName = (memberInfo.SourceInfo as AveUserInfo).Login;
                                sourceUserName = (memberInfo.SourceInfo as AveUserInfo).Title;
                            }
                            else if (memberInfo.SourceInfo is AveGroupInfo)
                            {
                                sourceUserName = (memberInfo.SourceInfo as AveGroupInfo).Title;
                            }
                        }
                    }
                }
                return sourceLoginName;
            }
        }

        private string GetSplitChar(AveCustomFieldInfo info)
        {
            if (info is AveCustomLookupFieldInfo)
            {
                return (info as AveCustomLookupFieldInfo).SeparateChar;
            }
            if (info is AveCustomMetadataFieldInfo)
            {
                return (info as AveCustomMetadataFieldInfo).SeparateChar;
            }
            return null;
        }

        #endregion

        #region Get Field Value.Tool Methods

        private string GetMutiUserValueById(string value)
        {
            return GetMutiUserValue(value, false);
        }

        private string ReplaceAudienceId(AveSiteMappingManager siteMappingMnager, string oldValue)
        {
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.ReplaceAudienceId"))
            {
                if (string.IsNullOrEmpty(oldValue) || oldValue.IndexOf(";;", StringComparison.OrdinalIgnoreCase) <= 0)
                {
                    return oldValue;
                }
                var result = oldValue;
                string tempValue = oldValue.Substring(0, oldValue.IndexOf(";;", StringComparison.OrdinalIgnoreCase));
                string[] tValues = tempValue.Split(',');
                foreach (var v in tValues)
                {
                    string mappingValue;
                    if (siteMappingMnager.GetValueFromAudienceIDMapping(v, out mappingValue))
                    {
                        result = result.Replace(v, mappingValue);
                    }
                }
                return oldValue;
            }
        }

        private IAveContentTypeId GetContentTypeId(byte[] contentTypeId)
        {
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.GetContentTypeId"))
            {
                string ctId = AveConvert.ConvertByteToContentTypeId(mAveParentSite.ObjectModelFactory,
                    contentTypeId).ToString();
                if (mAveSPList.ParentSite.MappingManager.ListMappingManager.ListLevelCTIdMapping.ContainsKey(ctId))
                {
                    return mAveSPList.ParentSite.MappingManager.ListMappingManager.ListLevelCTIdMapping[ctId];
                }
                IAveContentType ct;
                //CT不再反插 
                //源端为普通ContentType，目的端为ConnectorList时，此时ContentType赋值为connector的默认ContentType而不是通过BestMatch去获取
                if (mAveSPList.SPList.TemplateFeatureId != new Guid(AveWrapperConstants.AVEFSDLFEATRUEID)
                    && mAveSPList.SPList.TemplateFeatureId != new Guid(AveWrapperConstants.AVEVDLFEATRUEID))
                {
                    ct = mAveSPList.SPList.ContentTypes[
                            mAveSPList.SPList.ContentTypes.BestMatch(mAveParentSite.ObjectModelFactory.CreateContentTypeId(ctId))];
                    return ct != null ? ct.ID : null;
                }
                ct = mAveSPList.SPList.ContentTypes[mAveParentSite.ObjectModelFactory.CreateContentTypeId(ctId)];
                if (ct != null)
                {
                    return ct.ID;
                }
                if (mAveSPList.SPList.TemplateFeatureId == new Guid(AveWrapperConstants.AVEFSDLFEATRUEID))
                {
                    ct = mAveSPList.SPList.ContentTypes.FirstOrDefault(
                        contentType => contentType.ID != null && contentType.ID.ToString().StartsWith(
                            "0x01010003F8831469804144AE3F259EF433E9EB", StringComparison.OrdinalIgnoreCase));
                    return ct != null ? ct.ID : null;
                }
                ct = mAveSPList.SPList.ContentTypes.FirstOrDefault(
                    contentType => contentType.ID != null && contentType.ID.ToString().StartsWith(
                            "0x010100806213320A313D4DA11D1B1D6CC700CF", StringComparison.OrdinalIgnoreCase));
                return ct != null ? ct.ID : null;
            }
        }

        private bool FilterOut(IAveField field)
        {
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.FilterOut"))
            {
                if (mAveSPList.ParentWeb.ParentSite.ItemFieldFilter != null)
                {
                    int mode = mAveSPList.ParentWeb.ParentSite.ItemFieldFilter.Mode;
                    HashSet<string> includeFields = mAveSPList.ParentWeb.ParentSite.ItemFieldFilter.IncludeFields;
                    HashSet<string> excludeFields = mAveSPList.ParentWeb.ParentSite.ItemFieldFilter.ExcludeFields;

                    if (mode == 0)
                    {
                        if (includeFields.Contains(field.Title))
                        {
                            return false;
                        }
                        if (excludeFields.Contains(field.Title))
                        {
                            return true;
                        }
                    }
                    else if (mode == 1)
                    {
                        //include all
                        return false;
                    }
                    else if (mode == 2)
                    {
                        //exclude all
                        return true;
                    }
                }

                return false;
            }
        }

        private string ChangeServerRelativeUrl(string value)
        {
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.ChangeServerRelativeUrl"))
            {
                var index = value.IndexOf(',');
                if (index >= 0)
                {
                    string result = ChangeServerRelativeUrl(value.Substring(0, index)) + ", " +
                                    ChangeServerRelativeUrl(value.Substring(index + 1));
                    return result;
                }
                string destWebUrl = '/' + mAveSPWeb.ScopeString;
                string srcWebUrl =
                    mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.WebUrlDestToSourceMapping[destWebUrl];
                value = value.TrimStart();
                if (value.StartsWith(srcWebUrl, StringComparison.OrdinalIgnoreCase))
                {
                    if (destWebUrl.Length == 1 && srcWebUrl.Length != 1) //目的端是top site
                    {
                        value = value.Substring(srcWebUrl.Length);
                    }
                    else if (srcWebUrl.Length == 1 && destWebUrl.Length != 1) //源端是top site
                    {
                        value = destWebUrl + value.Substring(0);
                    }
                    else
                    {
                        value = destWebUrl + value.Substring(srcWebUrl.Length);
                    }
                }
                return value;
            }
        }

        #endregion

        [Obsolete("no use now, will remove later")]
        public Dictionary<string, object> GetFieldValuesInMetaInfo(int docRowId, int version, Dictionary<string, string> metaInfoDic, Guid webId, Guid parentListId)
        {

            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.GetFieldValuesInMetaInfo"))
            {


                Dictionary<string, object> fieldsInMetaInfo = null;
                IAveFieldCollection tempCollection = mAveSPList.SPList.Fields;
                foreach (var ent in metaInfoDic)
                {
                    string value = ent.Value;
                    string key = ent.Key;
                    if (tempCollection.ContainsField(key))
                    {
                        IAveField sf = tempCollection.GetField(key);

                        #region Lookup field with AllowMultipleValues is ture

                        if (sf is IAveFieldLookup && (sf as IAveFieldLookup).AllowMultipleValues)
                        {
                            var sLookUp = sf as IAveFieldLookup;
                            try
                            {
                                AveSPSite tempAveSite = mAveSPWeb.ParentSite;
                                AveLookupObject obj;
                                tempAveSite.MappingManager.SiteMappingManager.TryGetValueFromLookupFieldMapping(parentListId, sf.ID, out obj);
                                if (obj != null)
                                {
                                    var valueList = new ArrayList();
                                    if (fieldsInMetaInfo == null)
                                    {
                                        fieldsInMetaInfo = new Dictionary<string, object>();
                                    }
                                    string[] lookupValues = value.Split(new[] { ";#" }, StringSplitOptions.None);
                                    foreach (string v in lookupValues)
                                    {
                                        int lookupRowId;
                                        if (Int32.TryParse(v, out lookupRowId))
                                        {
                                            valueList.Add(lookupRowId);
                                        }
                                    }
                                    var listId = Guid.Empty;
                                    if (tempAveSite.MappingManager.SiteMappingManager.GetValueFromListIdMapping(new Guid(obj.SourceListId), out listId))
                                    {
                                        IAveFieldLookupValueCollection lookupCol =
                                            mAveParentSite.ObjectModelFactory.CreateFieldLookupValueCollection();
                                        foreach (int oldId in valueList)
                                        {
                                            int itemId = tempAveSite.MappingManager.SiteMappingManager.GetMappingItemId(listId, oldId);
                                            if (itemId != -1)
                                            {
                                                lookupCol.Add(mAveParentSite.ObjectModelFactory.CreateFieldLookupValue(itemId, sLookUp.LookupField));
                                            }
                                        }
                                        if (!fieldsInMetaInfo.ContainsKey(sf.InternalName) && lookupCol.Count > 0)
                                        {
                                            fieldsInMetaInfo.Add(sf.InternalName, lookupCol);
                                        }
                                    }
                                    else
                                    {
                                        tempAveSite.MappingManager.SiteMappingManager.AddNotUpdateLookupFieldValue(new Guid(obj.SourceListId), webId, parentListId, docRowId, version, sf.ID, valueList);
                                        fieldsInMetaInfo.Add(sf.InternalName, value);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                log.Log(AveLogLevel.WARN, WrapperRestoreResource.GetFieldError, e);
                            }
                        }

                        #endregion
                    }
                }
                return fieldsInMetaInfo;

            }

        }

        #endregion

        # region Field Post Action
        public void FieldRestorePostAction()
        {
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.FieldRestorePostAction"))
            {
                RestoreValidationFields();
                RestoreFieldDefaultValue();
                RestoreListFieldIndexes();
                AveFieldHelper.UpdateFieldSchemaIdMappingProperty(mAveSPList.SPList, FieldMapping);
                UpdateLookupColumnForColumnMapping();
                IAveWeb originalWeb = null;
                IAveList originalList = null;
                try
                {
                    if (mAveSPList.ListInfo != null)
                    {
                        mAveSPWeb.ParentSite.RestoreLookupFields(mAveSPList.ListInfo.Id);
                        mAveSPWeb.ParentSite.RestoreLookupFieldValues(mAveSPList.ListInfo.Id, ref originalWeb, ref originalList);
                    }
                    if (mAveSPList.SPList != null)
                    {
                        mAveSPWeb.ParentSite.RestoreLookupFieldValues(mAveSPList.SPList.ID, ref originalWeb, ref originalList);
                    }
                }
                finally
                {
                    if (originalWeb != null)
                    {
                        originalWeb.Dispose();
                    }
                }
                UpdateFieldUniqueProperty();
            }
        }

        public void RestoreListFieldIndexes()
        {
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.RestoreListFieldIndexes"))
            {
                if (mAveSPList == null || mAveSPList.SPList == null || mAveSPList.IsLoadFieldXml)
                {
                    return;
                }
                if (mListFieldIndexesCache != null)
                {
                    IAveFieldIndexCollection fieldIndexes = mAveSPList.SPList.FieldIndexes;
                    foreach (var indexField in mListFieldIndexesCache.Values)
                    {
                        try
                        {
                            Guid firstFieldId = indexField[0];
                            Guid mappingFieldId = FieldMapping.GetMappingRestoredFieldId(indexField[0]);
                            if (mappingFieldId != Guid.Empty)
                            {
                                firstFieldId = mappingFieldId;
                            }
                            Guid secondFieldId = indexField[1];
                            mappingFieldId = FieldMapping.GetMappingRestoredFieldId(indexField[1]);
                            if (mappingFieldId != Guid.Empty)
                            {
                                secondFieldId = mappingFieldId;
                            }
                            fieldIndexes.Add(FieldCollection[firstFieldId], FieldCollection[secondFieldId]);
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.WARN, "An error occurred while add field to FieldIndexes.List ID:{0}. error message:{1}", mAveSPList.SPList.ID, e.ToString());
                        }
                    }
                    mListFieldIndexesCache.Clear();
                }
            }
        }

        public void BackupValidationFields()
        {
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.BackupValidationFields"))
            {
                if (mValidationFields != null || mAveSPList.SPList == null)
                {
                    return;
                }
                for (int i = FieldCollection.Count - 1; i >= 0; i--)
                {
                    IAveField field = FieldCollection[i];
                    CacheNeedResetValidationField(field);
                }
            }
        }

        public void RestoreValidationFields()
        {
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.RestoreValidationFields"))
            {
                if (mValidationFields == null || mValidationFields.Count == 0)
                {
                    return;
                }
                try
                {
                    foreach (Guid fieldId in mValidationFields.Keys)
                    {
                        IAveField field = AveFieldHelper.FindFieldById(fieldId, FieldCollection);
                        if (field != null)
                        {
                            field.ValidationFormula = mValidationFields[fieldId];
                            field.Update();
                        }
                    }
                }
                catch (Exception ex)
                {//ADO-5534, 当存在单个version的checkout file的时候，field更新报错，在此先进行web的reload，再进行更新
                    log.Info("Update field exception:{0}", ex.ToString());
                    mAveSPWeb.ReloadWeb();
                    try
                    {
                        foreach (Guid fieldId in mValidationFields.Keys)
                        {
                            IAveField field = AveFieldHelper.FindFieldById(fieldId, FieldCollection);
                            if (field != null)
                            {
                                field.ValidationFormula = mValidationFields[fieldId];
                                field.Update();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        log.Warn("Update exception:{0}", e.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// 如果list上的field设置了enforce unique value，并且设置了default value时，在创建item时会抛出SPDuplicateValuesFoundException异常。
        /// </summary>
        public void BackupFieldsDefaultValue()
        {
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.BackupFieldsDefaultValue"))
            {
                if (mFieldsDefaultValue != null || mAveSPList.SPList == null)
                {
                    return;
                }
                mFieldsDefaultValue = new Dictionary<Guid, string>();
                for (int i = FieldCollection.Count - 1; i >= 0; i--)
                {
                    IAveField field = FieldCollection[i];
                    if (field.EnforceUniqueValues && !string.IsNullOrEmpty(field.DefaultValue))
                    {
                        mFieldsDefaultValue.Add(field.ID, field.DefaultValue);
                        field.DefaultValue = string.Empty;
                        field.Update();
                    }
                }
            }
        }

        public void RestoreFieldDefaultValue()
        {
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.RestoreFieldDefaultValue"))
            {
                if (mFieldsDefaultValue == null || mFieldsDefaultValue.Count == 0)
                {
                    return;
                }
                foreach (Guid fieldId in mFieldsDefaultValue.Keys)
                {
                    IAveField field = AveFieldHelper.FindFieldById(fieldId, FieldCollection);
                    if (field != null)
                    {
                        field.DefaultValue = mFieldsDefaultValue[fieldId];
                        field.Update();
                    }
                }
            }
        }

        public void UpdateFieldUniqueProperty()
        {
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.UpdateFieldUniqueProperty"))
            {
                foreach (string internalName in needUpdateUniqueValueFields)
                {
                    try
                    {
                        string mappingName = FieldMapping.GetMappingRestoredFieldInternalName(internalName);
                        if (string.IsNullOrEmpty(mappingName))
                        {
                            mappingName = internalName;
                        }
                        IAveField field = mAveSPList.SPList.Fields.GetFieldByInternalName(mappingName);
                        if (field != null)
                        {
                            field.EnforceUniqueValues = true;
                            field.Update();
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Warn("UpdateFieldUniqueProperty Error. Field:{0}. Error:{1}", internalName, ex.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// 用于获取field的InternalName。
        /// </summary>
        /// <param name="filedName">Field Name</param>
        /// <returns>String类型的lookup field的InternalName</returns>

        private string GetListFieldInternalName(string filedName)
        {
            foreach (var field in this.mAveSPList.SPList.Fields)
            {
                if (string.Equals(field.Title, filedName, StringComparison.CurrentCultureIgnoreCase))
                {
                    return field.InternalName;
                }
            }
            foreach (IAveField field in this.mAveSPList.SPList.Fields)
            {
                if (string.Equals(field.InternalName, filedName, StringComparison.CurrentCultureIgnoreCase))
                {
                    return field.InternalName;
                }
            }
            return null;
        }

        public void UpdateLookupColumnForColumnMapping()
        {
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.UpdateLookupColumnForColumnMapping"))
            {
                if (mAveSPList.SPList != null)
                {
                    Dictionary<Guid, Dictionary<Guid, string>> lookupColumnMapping;
                    if (mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.TryGetValueFromNeedPostActionlookupColumnsForColumnMappingOnlyForPostAction(mAveSPList.SPList.Title, out lookupColumnMapping))
                    {
                        foreach (KeyValuePair<Guid, Dictionary<Guid, string>> kv in lookupColumnMapping)
                        {
                            try
                            {
                                Guid listId = kv.Key;
                                IAveList list = null;
                                if (listId != Guid.Empty)
                                {
                                    list = mAveSPWeb.SPWeb.Lists[listId];
                                }
                                foreach (KeyValuePair<Guid, string> pair in kv.Value)
                                {
                                    IAveFieldLookup field;
                                    if (list != null)
                                    {
                                        field = list.Fields[pair.Key] as IAveFieldLookup;
                                    }
                                    else
                                    {
                                        field = mAveSPWeb.SPWeb.Fields.GetFieldById(pair.Key, true) as IAveFieldLookup;
                                    }
                                    field.LookupList = mAveSPList.SPList.ID.ToString();
                                    string internalName = GetListFieldInternalName(pair.Value);
                                    field.LookupField = !String.IsNullOrEmpty(internalName) ? internalName : pair.Value;
                                    field.Update();
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Warn("UpdateLookupColumnForColumnMapping error.Exception:{0}", ex);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        //keep modified in bpos-s
        public bool EnableModifiedField(bool readOnly)
        {
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.EnableModifiedField"))
            {
                try
                {
                    IAveField modifiedField = FieldCollection[AveBuiltInFieldId.Modified];
                    if (modifiedField != null)
                    {
                        if (readOnly)
                        {
                            modifiedField.ReadOnlyField = mModifiedFieldReadOnly;
                            modifiedField.Hidden = mModifiedFieldHidden;
                            modifiedField.Update();
                        }
                        else if (modifiedField.ReadOnlyField)
                        {
                            mModifiedFieldHidden = modifiedField.Hidden;
                            mModifiedFieldReadOnly = modifiedField.ReadOnlyField;
                            modifiedField.ReadOnlyField = false;
                            modifiedField.Update();
                            return true;
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.EnableModifiedFieldError, e);
                }
                return false;
            }
        }
    }
}