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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;
using System.Xml;
using AvePoint.GCommon;
using System.Globalization;
using System.Collections.Specialized;
using System.Collections;
using AvePoint.Wrapper.Mapping;
using AvePoint.Wrapper.Resource;
using AvePoint.GCommon.Contract.CodeReview;
using System.IO;
using AvePoint.GCommon.Utility.I18N;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;
using Microsoft.SharePoint.Client;
using PnP.Core.Model.SharePoint;
using Newtonsoft.Json.Linq;

namespace AvePoint.Wrapper.Restore
{
    [Flags]
    public enum FieldMatchType
    {
        None = 0,
        DisplayName = 1,
        Name = 2,
        StaticName = 4,
        ID = 8,
        Schema = 16,
        Children = 32,
        CustomMapping = 64
    }

    public enum FieldRestoreStatus
    {
        NewCreated,
        Existed,
        Skipped,
        Exception,
        None
    }

    public enum FieldType
    {
        Web,
        List
    }

    //used for sort field order
    public enum FieldOrderType
    {
        LookupPrimary,
        LookupSecondary,
        Other
    }
    [AveCodeReview("2012/04/13", "yuzhi.jiang@avepoint.com", "jin.zhang@AvePoint.com", new string[2] { CodeReviewConstants.CHECK_LIST_ID_CS_1, CodeReviewConstants.CHECK_LIST_ID_EH_2 }, null, true)]
    [AveCodeReview("2012/06/07", "qinglong.luo@avepoint.com", "kexin.guo@AvePoint.com", new string[1] { CodeReviewConstants.CHECK_LIST_ID_WRAPPER_1 }, null, true)]
    public abstract class AveSPFieldCollection : IReportable,IDisposable
    {
        private List<Guid> publishingFeatureCreated = new List<Guid>();
        private const double DoubleAllowDifference = 1e-6;
        protected List<string> SourceFieldOrder = new List<string>();
        protected static AveLogger log = AveLogger.GetInstance(typeof(AveSPFieldCollection));
        protected IReport reportor = new AveWrapperReport();
        protected const string ColName = "ColName";
        protected const string TaxonomyFieldType = "TaxonomyFieldType";
        //internal Dictionary<Guid, Guid> FieldIdSchemaMappings;
        internal bool NeedSkip = false;
        protected uint mLanguageID;
        protected int mLocaleID;
        protected Guid mWebId;

        protected AveSPWeb mAveSPWeb;
        protected AveSPList mAveSPList;
        protected AveSPSite mAveParentSite;
        protected bool mIsOneDrive = false;
        protected Dictionary<string, AveXmlField> mXmlFields;
        public Dictionary<string, AveXmlField> XmlFields
        {
            get { return mXmlFields; }
        }
        private Dictionary<string, string> mSourceTextTaxonomyDic = new Dictionary<string, string>();
        public Dictionary<string, string> SourceTextTaxonomyDic
        {
            get { return mSourceTextTaxonomyDic; }
        }
        private string mExcelImportPath;
        public string ExcelImportPath
        {
            get
            {
                return mExcelImportPath;
            }
            set
            {
                mExcelImportPath = value;
            }
        }
        public List<AveXmlField> UnrestoredXmlFields = new List<AveXmlField>();
        public Dictionary<Guid, Guid> RestoredFieldIdMapping = new Dictionary<Guid, Guid>();
        public Dictionary<string, string> RelatedFieldInternalNameMapping = new Dictionary<string, string>();
        protected List<string> mSkippedRestoreFields;
        //提供过滤还原特定类型field的接口
        protected List<string> mFieldTypeFilter;
        public List<string> FieldTypeFilter
        {
            get { return mFieldTypeFilter; }
            set { mFieldTypeFilter = value; }
        }

        //private Dictionary<Guid, Guid> mFieldIdMapping = new Dictionary<Guid, Guid>();

        //public Dictionary<Guid, Guid> FieldIdMapping
        //{
        //    get { return mFieldIdMapping; }
        //}
        //private Dictionary<Guid, string> mEnsureFields = new Dictionary<Guid, string>();

        //public Dictionary<Guid, string> EnsureFields
        //{
        //    get { return mEnsureFields; }
        //}
        public List<string> RestoredFieldInternalNameList = new List<string>();
        //protected Dictionary<string, string> mFieldInternalNameMapping = new Dictionary<string, string>();
        //public Dictionary<string, string> FieldInternalNameMapping
        //{
        //    get { return mFieldInternalNameMapping; }
        //}
        //protected Dictionary<string, string> mFieldDisplayNameMapping = new Dictionary<string, string>();
        //public Dictionary<string, string> FieldDisplayNameMapping
        //{
        //    get
        //    {
        //        return mFieldDisplayNameMapping;
        //    }
        //}

        protected abstract IAveFieldCollection FieldCollection { get; }

        private AveCommonRestoreConfiguraion mRestoreConfig;

        //当GetFieldValues时，如果类型是Lookup，先跳过，在restore item之后，再回来进行LookupField的设置。
        //此处主要是为了提高还原效率，将操作SharePoint逻辑移到后台，因此需要预先得到FieldValue的值，而后
        //才能知道Item RowID。
        [ThreadStatic]
        protected static List<AveLookupFieldInfo> mLookupFieldIdValue;

        protected IAveFieldMapping mFieldMapping;
        public IAveFieldMapping FieldMapping
        {
            get
            {
                if (mFieldMapping == null)
                {
                    mFieldMapping = new AveFieldMapping();
                }
                return mFieldMapping;
            }
        }

        /// <summary>
        /// List Indexed field cache
        /// </summary>
        private Dictionary<Guid, List<Guid>> mListFieldIndexesCache = new Dictionary<Guid, List<Guid>>();
        protected Dictionary<Guid, List<Guid>> ListFieldIndexesCache
        {
            get
            {
                return mListFieldIndexesCache;
            }
        }

        public AveSPFieldCollection(AveCommonRestoreConfiguraion restoreConfig)
        {
            //mFieldDisplayNameMapping = new Dictionary<string, string>();
            mRestoreConfig = restoreConfig;
            //不还原TaxCatchAll field
            RelatedFieldInternalNameMapping.Add("TaxCatchAll", null);
            InitPublishingFields();
        }


        internal abstract void InitSchemaMappings();

        #region Load Xml Fields

        protected void LoadXmlFields(string xmlFields, Dictionary<string, AveXmlField> fieldMap)
        {
            log.Info("The {0}'s fields xml is:{1}", mAveSPList != null ? "list:" + mAveSPList.Name : "web:" + mAveSPWeb.SPWeb.Title, xmlFields);
            InitSchemaMappings();
            LoadFieldMap(xmlFields, fieldMap);
            if (fieldMap != null && fieldMap.Count > 0)
            {
                LoadTaxnomyFieldType(fieldMap);
            }
            LoadSourceFieldOrder(xmlFields);
        }

        protected virtual void LoadSourceFieldOrder(string fieldsXml)
        {
        }

        private void LoadTaxnomyFieldType(Dictionary<string, AveXmlField> fieldMap)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFieldCollection.LoadTaxnomyFieldType"))
            {
#endif
                foreach (AveXmlField srcXF in fieldMap.Values)
                {
                    try
                    {
                        if (srcXF.TypeAsString.Equals("TaxonomyFieldType", StringComparison.OrdinalIgnoreCase)
                            || srcXF.TypeAsString.Equals("TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase))
                        {
                            object textField = srcXF.GetCustomerProperty("TextField");
                            if (textField == null)
                            {
                                throw new AveWrapperException(AveWrapperErrorCode.TextFieldPropertyCannotFind, "Cannot find TextField property.");
                            }
                            Guid textFieldId = new Guid(textField.ToString());
                            bool canFind = false;
                            foreach (AveXmlField textXF in fieldMap.Values)
                            {
                                if (textXF.ID == textFieldId)
                                {
                                    mSourceTextTaxonomyDic[textXF.FieldInternalName] = srcXF.FieldInternalName;
                                    canFind = true;
                                    break;
                                }
                            }
                            if (!canFind)
                            {
                                throw new AveWrapperException(AveWrapperErrorCode.TextFieldCannotFind, "Cannot find TextField property.");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        log.Warn("An exception occurred while set taxonomy Text Dictionary, Taxonomy Name:{0}, exception:{1}", srcXF.Title, e.ToString());
                    }
                }
#if PerformanceLog
            }
#endif
        }

        private void LoadFieldMap(string xmlFields, Dictionary<string, AveXmlField> fieldMap)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFieldCollection.LoadFieldMap"))
            {
#endif
                fieldMap.Clear();
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlFields);
                foreach (XmlNode xmlNode in doc.DocumentElement.ChildNodes)
                {
                    try
                    {
                        if (xmlNode.NodeType != XmlNodeType.Element)
                        {
                            continue;
                        }
                        XmlElement xe = (XmlElement)xmlNode;
                        AddToXmlFieldMap(xe, fieldMap);
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, string.Format("An error occurred while load xmlField. xmlNode.OuterXml:{0}\n error message:{1}", xmlNode.OuterXml, e));
                    }
                }
#if PerformanceLog
            }
#endif
        }

        protected void AddToXmlFieldMap(XmlElement xe, Dictionary<string, AveXmlField> fieldMap)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFieldCollection.AddToXmlFieldMap"))
            {
#endif
                try
                {
                    if (string.Compare(xe.Name, "FieldRef", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        try
                        {
                            IAveField currentField = null;
                            Guid fieldId = Guid.Empty;
                            string fieldName = string.Empty;
                            bool invalid = false;
                            if (xe.HasAttribute("ID"))
                            {
                                fieldId = new Guid(xe.GetAttribute("ID"));
                            }
                            else if (xe.HasAttribute("Name"))
                            {
                                fieldName = xe.GetAttribute("Name");
                            }
                            else
                            {
                                invalid = true;//log
                                log.Warn("The FieldRef:{0} is invalid, cannot get id and name attribute.", xe.OuterXml);
                            }
                            if (!invalid)
                            {
                                if (mAveSPList != null && mAveSPList.SPList != null)
                                {
                                    if (fieldId == Guid.Empty)
                                    {
                                        currentField = mAveSPList.SPList.Fields.GetFieldByInternalName(fieldName, false);
                                    }
                                    else
                                    {
                                        currentField = mAveSPList.SPList.Fields.GetFieldById(fieldId, false);
                                    }
                                }
                                if (currentField == null && mAveSPWeb != null && mAveSPWeb.SPWeb != null)
                                {
                                    if (fieldId == Guid.Empty)
                                    {
                                        currentField = mAveSPWeb.SPWeb.AvailableFields.GetFieldByInternalName(fieldName, false);
                                    }
                                    else
                                    {
                                        currentField = mAveSPWeb.SPWeb.AvailableFields.GetFieldById(fieldId, false);
                                    }
                                }
                                if (currentField == null)
                                {
                                    log.Warn("The FieldRef:{0} is not installed correctly", xe.OuterXml);
                                }
                                else
                                {
                                    var tempElement = xe.OwnerDocument.CreateElement("Test");
                                    tempElement.InnerXml = currentField.SchemaXml;
                                    xe = (XmlElement)tempElement.ChildNodes[0];
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Warn("Get the field reference failed by schema:{0}, exception:{1}", xe.OuterXml, ex.ToString());
                        }
                    }

                    if (string.Compare(xe.Name, "Field", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        if (!xe.HasAttribute("Name") && !xe.HasAttribute(ColName))
                        {
                            return;
                        }
                        string internalName = xe.GetAttribute("Name");
                        if (xe.HasAttribute("Type"))
                        {
                            FieldRestoreStatus status = FieldRestoreStatus.None;
                            string type = xe.GetAttribute("Type");
                            if (CheckFilter(xe))
                            {
                                SkipFieldRestore(internalName, ref status);
                                return;
                            }
                        }
                        int lcid = mAveSPWeb?.SPWeb.Locale == null ? 1033 : mAveSPWeb.SPWeb.Locale.LCID;
                        fieldMap[internalName] = new AveXmlField(xe, lcid);

                        //int i = 2;
                        //while (true)
                        //{
                        //    string tempColName = ColName + i;
                        //    if (!xe.HasAttribute(tempColName))
                        //    {
                        //        break;
                        //    }
                        //    fieldMap[internalName + AveConstants.FIELD_SEPARATOR + i] = new AveXmlField(xe, internalName + AveConstants.FIELD_SEPARATOR + i);
                        //    i++;
                        //}
                    }
                    else if (string.Compare(xe.Name, "Index", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Guid fieldIndexId = new Guid(xe.Attributes["ID"].Value);
                        Guid firstIndexedColumn = new Guid(xe.FirstChild.Attributes["ID"].Value);
                        Guid secondIndexedColumn = new Guid(xe.LastChild.Attributes["ID"].Value);
                        Guid mappingValue = FieldMapping.GetMappingRestoredFieldId(firstIndexedColumn);
                        if (mappingValue != Guid.Empty)
                        {
                            firstIndexedColumn = mappingValue;
                            mappingValue = Guid.Empty;
                        }
                        mappingValue = FieldMapping.GetMappingRestoredFieldId(secondIndexedColumn);
                        if (mappingValue != Guid.Empty)
                        {
                            secondIndexedColumn = mappingValue;
                        }
                        if (!mListFieldIndexesCache.ContainsKey(fieldIndexId))
                        {
                            mListFieldIndexesCache.Add(fieldIndexId, new List<Guid> { firstIndexedColumn, secondIndexedColumn });
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new AveWrapperException(AveWrapperErrorCode.UnKnown, "An error occurred while adding to XmlFieldMap.", e); ;
                }
#if PerformanceLog
            }
#endif
        }

        public abstract void LoadFields(string fieldsXml);

        protected IEnumerable<AveXmlField> SortFields(IDictionary<string, AveXmlField> xmlFields)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFieldCollection.SortFields"))
            {
#endif
                LinkedList<AveXmlField> primaryLookupFieldsSec = new LinkedList<AveXmlField>();
                LinkedList<AveXmlField> secondaryLookupFieldsSec = new LinkedList<AveXmlField>();
                LinkedList<AveXmlField> orderedFields = new LinkedList<AveXmlField>();

                foreach (KeyValuePair<string, AveXmlField> xmlFieldPair in xmlFields)
                {
                    switch (GetLookupFieldType(xmlFieldPair.Value))
                    {
                        case FieldOrderType.LookupPrimary:
                            primaryLookupFieldsSec.AddLast(xmlFieldPair.Value);
                            break;
                        case FieldOrderType.LookupSecondary:
                            secondaryLookupFieldsSec.AddLast(xmlFieldPair.Value);
                            break;
                        default:
                            orderedFields.AddLast(xmlFieldPair.Value);
                            break;
                    }
                }
                return orderedFields.Concat(primaryLookupFieldsSec).Concat(secondaryLookupFieldsSec);
#if PerformanceLog
            }
#endif
        }
        #region Custom Field
        internal void ConfigueCustomFields(IEnumerable<AveXmlField> sortedFields)
        {
            try
            {
                if (!String.IsNullOrEmpty(ExcelImportPath))
                {
                    GetFieldInfoFromCustomMappingByExcel(sortedFields);
                }
                else
                {
                    GetFieldInfoFromCustomMappingByXml(sortedFields);
                }
            }
            catch (Exception ex)
            {
                log.Error("get custom field info error.Exception:" + ex.ToString());
            }
        }
        protected void GetFieldInfoFromCustomMappingByXml(IEnumerable<AveXmlField> sortedFields)
        {
            try
            {
                foreach (AveXmlField xmlField in sortedFields)
                {
                    AveCustomFieldInfo mappingInfo = FieldMapping.GetMappingFieldBeforeAdd(new AveSourceFieldInfo() { SourceInternalName = xmlField.FieldInternalName, SourceDisplayName = xmlField.Title, SourceFieldId = xmlField.ID });
                    if (mappingInfo != null)
                    {
                        xmlField.CustomFieldInfo = mappingInfo;
                        xmlField.CustomFieldInfo.SourceType = xmlField.Type;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("get custom field info from xml error.Exception:" + ex.ToString());
            }
        }
        protected void GetFieldInfoFromCustomMappingByExcel(IEnumerable<AveXmlField> sortedFields)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFieldCollection.GetFieldInfoFromCustomMappingByExcel"))
            {
#endif
                try
                {
                    Dictionary<string, AveCustomFieldInfo> NeedCreateCustomFields = new Dictionary<string, AveCustomFieldInfo>();
                    List<string> NotNeedCreateFieldKeys = new List<string>();//存excel没改变类型的column，不需要根据excel去创建
                    NotNeedCreateFieldKeys.Add("ID");
                    foreach (AveXmlField xmlField in sortedFields)
                    {
                        if (!xmlField.Hidden && !xmlField.ReadOnlyField)// backup时由于只把不是Hidden和readOnly的Field导出到了Excel里，在此只需判断这些是否更改过就可以
                        {
                            AveCustomFieldInfo info = (FieldMapping as AveFieldMapping).CustomMapping.GetMappingFieldBeforeAdd(new AveSourceFieldInfo() { SourceDisplayName = xmlField.Title });
                            if (info != null)
                            {
                                if (info.TypeAsString.Equals(xmlField.TypeAsString, StringComparison.OrdinalIgnoreCase))
                                {
                                    NotNeedCreateFieldKeys.Add(xmlField.Title);
                                }
                                else
                                {
                                    xmlField.NeedSkipForCustom = true;
                                }
                            }
                            else
                            {
                                xmlField.NeedSkipForCustom = true;
                            }
                        }
                    }
                    bool needUpdateList = false;
                    foreach (AveSourceFieldInfo key in ((FieldMapping as AveFieldMapping).CustomMapping as AveCustomFieldMappingForExcel).InternalExcelFieldMapping.Keys)
                    {
                        if (!NotNeedCreateFieldKeys.Contains(key.SourceDisplayName))
                        {
                            AveCustomFieldInfo info = ((FieldMapping as AveFieldMapping).CustomMapping as AveCustomFieldMappingForExcel).InternalExcelFieldMapping[key];
                            if (!info.TypeAsString.Equals("Lookup", StringComparison.OrdinalIgnoreCase) && !info.TypeAsString.Equals("LookupMulti", StringComparison.OrdinalIgnoreCase) && !info.Type.Equals(AveFieldType.Invalid))
                            {
                                if ((!mAveSPList.AveFields.FieldCollection.ContainsField(info.Name)) || !mAveSPList.AveFields.FieldCollection[info.Name].TypeAsString.Equals(info.TypeAsString, StringComparison.OrdinalIgnoreCase))
                                {
                                    string fieldName = mAveSPList.AveFields.FieldCollection.Add(info.Name, info.Type, false);
                                    UpdateFieldsCreatedByExcel(fieldName, info.TypeAsString);
                                    needUpdateList = true;
                                }
                            }
                        }
                    }
                    if (needUpdateList)
                    {
                        this.mAveSPList.SPList.Update();
                    }
                }
                catch (Exception ex)
                {
                    log.Error("get custom field info from excel error.Exception:" + ex.ToString());
                }
#if PerformanceLog
            }
#endif
        }

        #region 确保创建出的column 的setting和通过sharepoint界面创建一致
        private void UpdateFieldsCreatedByExcel(string fieldName, string typeAsString)
        {
            IAveField field = mAveSPList.AveFields.FieldCollection.GetFieldByInternalName(fieldName);
            bool needUpdate = false;
            if (typeAsString.Equals("DateTime", StringComparison.OrdinalIgnoreCase))
            {
                IAveFieldDateTime dtField = field as IAveFieldDateTime;
                dtField.DisplayFormat = AveDateTimeFieldFormatType.DateOnly;
                needUpdate = true;
            }
            else if (typeAsString.Equals("User", StringComparison.OrdinalIgnoreCase) || typeAsString.Equals("UserMulti", StringComparison.OrdinalIgnoreCase))
            {
                IAveFieldUser userField = field as IAveFieldUser;
                userField.SelectionMode = AveFieldUserSelectionMode.PeopleOnly;
                if (typeAsString.Equals("UserMulti", StringComparison.OrdinalIgnoreCase))
                {
                    userField.AllowMultipleValues = true;
                }
                needUpdate = true;
            }
            else
            {
            }
            if (needUpdate)
            {
                field.Update();
            }
        }

        #endregion
        #endregion

        private FieldOrderType GetLookupFieldType(AveXmlField xmlField)
        {
            if (xmlField.Type == AveFieldType.Lookup)
            {
                if (string.IsNullOrEmpty(xmlField.PrimaryFieldId))
                {
                    return FieldOrderType.LookupPrimary;
                }
                else
                {
                    return FieldOrderType.LookupSecondary;
                }
            }
            return FieldOrderType.Other;
        }
        #endregion

        protected void AddMappingToSite(FieldType fieldType, AveSPSite site)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFieldCollection.AddMappingToSite"))
            {
#endif
                //当把俩个Web Restore到一个Web里面或者把俩个List Restore到一个List里面时，第二个需要覆盖第一个的Mapping。
                Dictionary<Guid, Guid> mFieldIdMapping = FieldMapping.EnumFieldIdMapping().ToDictionary(pair => pair.Key, pair => pair.Value);
                Dictionary<string, string> mFieldInternalNameMapping = FieldMapping.EnumFieldInternalNameMapping().ToDictionary(pair => pair.Key, pair => pair.Value);
                Dictionary<string, string> mFieldDisplayNameMapping = FieldMapping.EnumFieldDisplayNameMapping().ToDictionary(pair => pair.Key, pair => pair.Value);
                if (fieldType == FieldType.Web)
                {

                    if (mFieldIdMapping != null)
                    {
                        site.MappingManager.SiteMappingManager.WebFieldsIdMapping[mWebId] = mFieldIdMapping;
                    }
                    if (mFieldInternalNameMapping != null)
                    {
                        site.MappingManager.SiteMappingManager.WebFieldsInternalNameMapping[mWebId] = mFieldInternalNameMapping;
                    }
                    if (mFieldDisplayNameMapping != null)
                    {
                        site.MappingManager.SiteMappingManager.WebFieldsDisplayNameMapping[mWebId] = mFieldDisplayNameMapping;
                    }
                }
                else if (fieldType == FieldType.List && mAveSPList.SPList != null)
                {
                    if (mFieldIdMapping != null)
                    {
                        site.MappingManager.SiteMappingManager.ListFieldsIdMapping[mAveSPList.SPList.ID] = mFieldIdMapping;
                    }
                    if (mFieldInternalNameMapping != null)
                    {
                        site.MappingManager.SiteMappingManager.ListFieldsInternalNameMapping[mAveSPList.SPList.ID] = mFieldInternalNameMapping;
                    }
                    if (mFieldDisplayNameMapping != null)
                    {
                        site.MappingManager.SiteMappingManager.ListFieldsDisplayNameMapping[mAveSPList.SPList.ID] = mFieldDisplayNameMapping;
                    }
                }
                //if (mEnsureFields != null)
                //{
                //    site.MappingManager.SiteMappingManager.ListEnsureFields[mAveSPList.SPList.ID] = mEnsureFields;
                //}
#if PerformanceLog
            }
#endif
        }

        #region Find Field
        internal abstract IAveField Find(AveXmlField xmlField, FieldFindOption findOption, ref FieldMatchType matchType);
        #endregion

        #region Restore Field
        public virtual void RestoreFields(string fieldsXml)
        {
            AveFieldRestoreOption restoreOption = new AveFieldRestoreOption();
            RestoreFields(fieldsXml, restoreOption);
        }

        public abstract void RestoreFields(string fieldsXml, AveFieldRestoreOption restoreOption);

        internal void RestoreFields(Dictionary<string, AveXmlField> xmlFields, FieldType fieldType, AveFieldRestoreOption restoreOption)
        {
            RestoreFields(xmlFields, fieldType, restoreOption, false);
        }

        protected void RestoreFields(Dictionary<string, AveXmlField> xmlFields, FieldType fieldType, AveFieldRestoreOption restoreOption, bool isPost)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFieldCollection.RestoreFields"))
            {
#endif
                try
                {
                    mLanguageID = mAveSPWeb.SPWeb.Language;
                    CultureInfo local = mAveSPWeb.SPWeb.Locale;
                    mLocaleID = local == null ? (int)mLanguageID : local.LCID;
                    mWebId = mAveSPWeb.SPWeb.ID;

                    Guid newID = Guid.Empty;
                    //calculated 和computed类型的field，如果它关联的其他field还没有还原的情况下，还原它会抛出异常，对其做postFields操作。
                    Dictionary<string, AveXmlField> postFields = new Dictionary<string, AveXmlField>();

                    #region New codes... this function should be modified after migrated to afterward...
                    FieldRestoreStatus status = FieldRestoreStatus.None;

                    IEnumerable<AveXmlField> sortedFields = SortFields(xmlFields);
                    //if (this is AveSPListFieldCollection)
                    //{
                    //先走UI mapping，再走language mapping。SAAS-11183
                    ConfigueCustomFields(sortedFields);
                    UpdateFieldXmls(xmlFields, fieldType);
                    //}
                    List<string> CustomFieldInternalNameList = new List<string>();
                    foreach (FieldFindOption findOption in restoreOption.FindOption)
                    {
                        foreach (AveXmlField xmlField in sortedFields)
                        {
                            if (IsFieldShouldSkip(xmlField, CustomFieldInternalNameList, findOption)
                                || postFields.ContainsKey(xmlField.KeyName))
                            {
                                continue;
                            }

                            status = FieldRestoreStatus.None;
                            FieldMatchType matchType = FieldMatchType.None;
                            IAveField field = Find(xmlField, findOption, ref matchType);
                            if (field == null && OpenPublishingFeature(xmlField))
                            {
                                field = Find(xmlField, findOption, ref matchType);
                            }
                            if (null == field && matchType != FieldMatchType.Children)
                            {
                                if (!UnrestoredXmlFields.Contains(xmlField))
                                {
                                    UnrestoredXmlFields.Add(xmlField);
                                }
                                continue;
                            }
                            field = Restore(xmlField, field, fieldType, matchType, restoreOption, ref status, false);

                            UnrestoredXmlFields.Remove(xmlField);

                            if (status == FieldRestoreStatus.Exception &&
                                (xmlField.Type == AveFieldType.Calculated || xmlField.Type == AveFieldType.Computed))
                            {
                                postFields.Add(xmlField.KeyName, xmlField);
                            }
                        }
                        if (UnrestoredXmlFields.Count == 0)
                        {
                            break;
                        }
                    }
                    Queue<AveXmlField> unrestoredQueue = new Queue<AveXmlField>();
                    foreach (AveXmlField xmlField in UnrestoredXmlFields)
                    {
                        unrestoredQueue.Enqueue(xmlField);
                    }

                    while (unrestoredQueue.Count > 0)
                    {
                        AveXmlField xmlField = unrestoredQueue.Dequeue();
                        IAveField field = Restore(xmlField, null, fieldType, FieldMatchType.None, restoreOption, ref status, false);
                        UnrestoredXmlFields.Remove(xmlField);
                        if (status == FieldRestoreStatus.Exception &&
                            (xmlField.Type == AveFieldType.Calculated || xmlField.Type == AveFieldType.Computed))
                        {
                            postFields.Add(xmlField.KeyName, xmlField);
                        }
                    }
                    if (!isPost && postFields.Count > 0)
                    {
                        RestoreFields(postFields, fieldType, restoreOption, true);
                    }
                    AddMappingToSite(fieldType, mAveSPWeb.ParentSite);
                    #endregion
                }
                catch (AveSecurityTrimingException ex)
                {
                    if (fieldType == FieldType.Web)
                    {
                        log.Warn("An error occurred while Restore Web Field. ", ex);
                        reportor.AddDetail(new AveWrapperReportDto(mAveSPWeb.Name, mAveSPWeb.Name, AveReportObjectType.WebField, AveStatus.Skipped, "You don't have permission to Restore Web Fields. " + ex.Message));
                    }
                    if (fieldType == FieldType.List)
                    {
                        log.Warn("An error occurred while Restore List Field. ", ex);
                        reportor.AddDetail(new AveWrapperReportDto(mAveSPList.Name, mAveSPList.Name, AveReportObjectType.ListField, AveStatus.Skipped, "You don't have permission to Restore List Fields. " + ex.Message));
                    }
                }
#if PerformanceLog
            }
#endif
        }

        // fields created by publishing feature 
        private void InitPublishingFields()
        {
            // summary links2
            publishingFeatureCreated.Add(new Guid("27761311-936a-40ba-80cd-ca5e7a540a36"));
            //summary links
            publishingFeatureCreated.Add(new Guid("b3525efe-59b5-4f0f-b1e4-6e26cb6ef6aa"));
        }

        private bool IsCommunicationSite()
        {
            try
            {
                return mAveSPWeb.ParentSite.SPSite.RootWeb.WebTemplateName == AveSPWebTemplate.COMMUNICATION_SITE;
            }
            catch (Exception ex)
            {
                log.Warn($"Check is communication site failed, {ex}");
            }
            return false;
        }

        private bool OpenPublishingFeature(AveXmlField xmlField)
        {
            if(mAveSPWeb.ParentSite.FailedToEnablePublishingFeature && IsCommunicationSite())
            {
                log.Warn($"retore field {xmlField.FieldInternalName} failed due to: open publishing feature failed.");
                return false;
            }

            Guid publishingFeatureId = new Guid("f6924d36-2fa8-4f0b-b16d-06b7250180fa");
            if (mAveSPWeb.ParentSite.SPSite.Features[publishingFeatureId] != null)
            {
                return false;
            }
            if (publishingFeatureCreated.Contains<Guid>(xmlField.ID))
            {
                int retryCount = 3;
                while (retryCount > 0)
                {
                    try
                    {
                        mAveSPWeb.ParentSite.SPSite.Features.Add(publishingFeatureId, true);
                        mAveSPWeb.ParentSite.NeedClosePublishingFeature = true;
                        return true;
                    }
                    catch (Exception e)
                    {
                        if (e.Message.Equals("The operation has timed out.", StringComparison.OrdinalIgnoreCase) && retryCount > 0)
                        {
                            retryCount--;
                            Thread.Sleep(10000);
                        }
                        else
                        {
                            mAveSPWeb.ParentSite.FailedToEnablePublishingFeature = true;
                            mAveSPWeb.ParentSite.NeedClosePublishingFeature = true;
                            log.Warn("retore field {0} failed dute to:open publishing feature failed.message:{1}", xmlField.FieldInternalName, e.Message);
                            return false;
                        }
                    }
                }
            }
            return false;
        }
        protected bool IsFieldShouldSkip(AveXmlField xmlField, List<string> CustomFieldInternalNameList, FieldFindOption findOption)
        {
            if (xmlField.NeedSkipForCustom)
            {
                return true;
            }
            if (mSourceTextTaxonomyDic.ContainsKey(xmlField.FieldInternalName))
            {
                log.Info("Skip restore TextField:{0}, it has been related to an taxonomyField.", xmlField.KeyName);
                return true;
            }
            else if (RestoredFieldInternalNameList.Contains(xmlField.FieldInternalName))
            {
                return true;
            }
            else if (xmlField.CustomFieldInfo != null && findOption != FieldFindOption.FindByCustomMapping)
            {
                return true;
            }
            else if (CustomFieldInternalNameList != null && CustomFieldInternalNameList.Contains(xmlField.FieldInternalName))
            {
                return true;
            }
            else if (xmlField.ID == new Guid("1390a86a-23da-45f0-8efe-ef36edadfb39")
                    || xmlField.ID == new Guid("f3b0adf9-c1a2-4b02-920d-943fba4b3611")
                    || xmlField.ID == new Guid("8f6b6dd8-9357-4019-8172-966fcd502ed2"))
            {//TaxKeywordTaxHTField，Taxonomy Catch All Column 这两个column在还原其他column的时候会自动创建出来，不再需要进行还原
                return true;
            }
            else if (mIsOneDrive && xmlField.TypeAsString.Equals(TaxonomyFieldType, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }

        protected bool IsOneDrive()
        {
            bool isOD4B = false;
            try
            {
                var siteInfo = mAveSPWeb.ParentSite.SPSite.SiteSerializer.GetObjectData() as AveSiteInfo;
                if (siteInfo != null && siteInfo.WebTemplate != null)
                {
                    isOD4B = siteInfo.WebTemplate.StartsWith("SPSPERS", StringComparison.OrdinalIgnoreCase);
                }
            }
            catch (Exception e)
            {
                log.Warn("An error occurred while checking if is onedrive site. Error:{0}", e.ToString());
            }
            log.Info("Is onedrive site:{0}", isOD4B);
            return isOD4B;
        }

        protected virtual IAveField Restore(AveXmlField xmlField, IAveField field, FieldType fieldType, FieldMatchType matchType, AveFieldRestoreOption restoreOption, ref FieldRestoreStatus status, bool isEnsureField)
        {
            return Restore(xmlField, field, fieldType, matchType, restoreOption, ref status, false, false, isEnsureField);
        }

        protected virtual IAveField Restore(AveXmlField xmlField, IAveField field, FieldType fieldType, FieldMatchType matchType, AveFieldRestoreOption restoreOption, ref FieldRestoreStatus status, bool throwWhenNotExist, bool throwWhenConflict, bool isEnsureField)
        {
            OpenPublishingFeature(xmlField);
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFieldCollection.Restore"))
            {
#endif
                status = FieldRestoreStatus.None;
                IAveFieldCollection fields = null;
                IAveFieldCollection availableFields = null;
                var reportType = fieldType == FieldType.List ? AveReportObjectType.ListField : AveReportObjectType.WebField;
                var objectTitle = fieldType == FieldType.List ? mAveSPList.SPList.Title : mAveSPWeb.SPWeb.Title;
                try
                {
                    if (fieldType == FieldType.Web)
                    {
                        fields = mAveSPWeb.SPWeb.Fields;
                        availableFields = mAveSPWeb.SPWeb.AvailableFields;
                    }
                    else if (fieldType == FieldType.List)
                    {
                        fields = mAveSPList.SPList.Fields;
                    }
                    if (matchType == FieldMatchType.Children)
                    {
                        SetFieldId(xmlField.XmlElement);
                    }
                    ///需要提前记录当前field的display name和internal name 否则有custom mapping时加入name mapping的name可能就是错的，导致断链等问题
                    string sourceFieldInternalName = xmlField.FieldInternalName;
                    string sourceFieldDisplayName = xmlField.SourceTitle;
                    Guid sourceFieldId = xmlField.ID;

                    bool isConflict = false;
                    if (null != field && matchType != FieldMatchType.None)
                    {
                        GetFieldsMatchType(xmlField, field, ref matchType);

                        //如果是same type或者没有custom field info就需要比较冲突。
                        if (xmlField.CustomFieldInfo == null || "SameType".Equals(xmlField.CustomFieldInfo.CustomFieldTypeAsString, StringComparison.OrdinalIgnoreCase))
                        {
                            if (field.Group.Equals("_Hidden"))//site column "Editor"更新后进行编辑出错 SAAS-27406 
                            {
                                log.Info("Skip compare field cause {0}'s group is _Hidden", field.InternalName);
                            }
                            else
                            {
                                isConflict = !Compare(xmlField, field);
                            }

                            //if (isConflict)
                            //{
                            //    log.Info("Conflict: source field:{0}\r\ntarget field:{1}", xmlField.XmlElement.OuterXml, field.SchemaXml);
                            //}
                        }

                    if (isConflict && WrapperConfiguration.WrapperConfigurationForBPOS.IsEndUserRestore)
                    {
                        log.Warn($"IsEndUserRestore need resest isConflict. Field Conflict:{field.Title}, source schema xml:{xmlField.XmlElement.OuterXml}, destination schema xml:{field.SchemaXml}.");
                        isConflict = false;
                    }
                    if (throwWhenConflict && isConflict)
                        {
                            throw new AveSchemaDependencyConflictException(field.Title, "field");
                        }
                        if (!WrapperConfiguration.WrapperConfigurationForBPOS.IsEndUserRestore && !isConflict)
                        {
                            UpdatePropertiesReleatedToSettings(field, xmlField);
                            UpdateListLookUpIdProperty(field, xmlField);
                            //UpdateUserResource(field, xmlField);
                        }
                    }
                    else
                    {
                        if (throwWhenNotExist)
                        {
                            throw new AveFieldSchemaDependencyNotFoundException(sourceFieldDisplayName);
                        }
                        if (RelatedFieldInternalNameMapping.ContainsKey(xmlField.FieldInternalName))
                        {
                            field = fields?.GetFieldByInternalName(RelatedFieldInternalNameMapping[xmlField.FieldInternalName]);
                        }
                        else
                        {
                            //对于用户配置的有name和type的mapping的field，需要根据name和type来创建
                            if (xmlField.CustomFieldInfo != null && (!String.IsNullOrEmpty(xmlField.CustomFieldInfo.TypeAsString) || xmlField.CustomFieldInfo.Type != AveFieldType.Invalid))
                            {
                                field = CreateNewFieldByCustomMapping(xmlField, fields, ref status);
                            }
                            else
                            {
                                field = CreateNewField(xmlField, fieldType, fields, ref status, isEnsureField);
                            }
                        }
                    }
                ArgumentCheck.CheckNotNull(field);
                //Archiver Restore field.Required need set false;
                if (field.Required)
                {
                    if (WrapperConfiguration.WrapperConfigurationForBPOS.IsEndUserRestore)
                    {
                        log.Info("IsEndUserRestore.Archiver skip update Required field title resource.FieldInfo:[{0},{1},{2},isConflict:{3}].", field.Title, field.InternalName, field.ID, isConflict);
                        field.Required = false;
                    }
                    else
                    {
                        log.Info("Archiver update Required field title resource.FieldInfo:[{0},{1},{2},isConflict:{3}].", field.Title, field.InternalName, field.ID, isConflict);
                        field.Required = false;
                    }
                }
                if (isConflict)
                    {
                        if (restoreOption.COMPARE_MD5 && !String.IsNullOrEmpty(AveFieldHelper.GetMD5FromSchemaXml(field)) && AveFieldHelper.GetMD5FromSchemaXml(field).Equals(AveFieldHelper.GetCurrentMD5Property(field), StringComparison.OrdinalIgnoreCase))
                        {
                            //对于需要比较MD5值的，若目的端SchemaXml中存在MD5属性，并且与当前Field的MD5值相同，则不认为冲突，直接进行update
                            UpdateField(fields, field, xmlField);
                        }
                        else
                        {
                            //修改Calculated类型字段时更新Formula
                            if (xmlField.Type == AveFieldType.Calculated)
                            {
                                UpdateCalculatedFieldInfo(xmlField, fields, fieldType, isEnsureField);
                            }
                            if (xmlField.CustomFieldInfo == null)//对于find出来的field，如果配置过CustomMapping，则不再进行冲突处理
                            {
                                HandleConflict(xmlField, ref field, fieldType, fields, restoreOption.ConflictOption, matchType);
                            }
                            //column mapping： 源端目的端类型判断相同则更新目的端column的属性
                            else if ("SameType".Equals(xmlField.CustomFieldInfo.CustomFieldTypeAsString, StringComparison.OrdinalIgnoreCase) &&
                                (xmlField.Type.Equals(field.Type) ||
                                (xmlField.Type.Equals(AveFieldType.Choice) && field.Type.Equals(AveFieldType.MultiChoice)) ||
                                (xmlField.Type.Equals(AveFieldType.MultiChoice) && field.Type.Equals(AveFieldType.Choice))))
                            {
                                UpdateField(fields, field, xmlField);
                            }
                        }
                    }
                    if (field != null)
                    {
                        log.Debug("Update field title resource.FieldInfo:[{0},{1},{2},isConflict:{3}]", field.Title, field.InternalName, field.ID, isConflict);
                        UpdateUserResource(field, xmlField,isConflict);
                        field.Update();
                    }

                    if (status == FieldRestoreStatus.NewCreated)
                    {
                        mAveSPWeb.SPWeb.AvailableFields.IsDirty = true;
                    }
                    if (field != null)
                    {
                        if (restoreOption.COMPARE_MD5)
                        {
                            AveFieldHelper.UpdateMD5ToSchemaXml(field);
                        }
                        if (!String.IsNullOrEmpty(xmlField.RelatedField) && !String.IsNullOrEmpty(field.RelatedField))
                        {
                            RelatedFieldInternalNameMapping[xmlField.RelatedField] = field.RelatedField;
                        }
                        //External column 还原之后将设置Add a column to show each of these additional fields时产生的column加到InternalNameMapping中，防止再次还原导致出现双份
                        if (field.TypeAsString.Equals("BusinessData", StringComparison.OrdinalIgnoreCase) && (field is IAveBusinessDataField))
                        {
                            string[] sourceSecondaryFieldWssNames = (field as IAveBusinessDataField).SplitSecondaryFieldNames(xmlField.XmlElement.GetAttribute("SecondaryFieldWssNames"));
                            string[] destSecondaryFieldWssNames = (field as IAveBusinessDataField).SplitSecondaryFieldNames(field.GetProperty("SecondaryFieldWssNames"));
                            if (destSecondaryFieldWssNames != null && sourceSecondaryFieldWssNames != null)
                            {
                                foreach (string secondaryFieldWssName in sourceSecondaryFieldWssNames)
                                {
                                    if (destSecondaryFieldWssNames.Contains(secondaryFieldWssName))
                                    {
                                        RelatedFieldInternalNameMapping[secondaryFieldWssName] = secondaryFieldWssName;
                                    }
                                }
                            }
                        }
                        SetFieldMapping(sourceFieldInternalName, sourceFieldDisplayName, sourceFieldId, field, isEnsureField);
                        RestoredFieldIdMapping[field.ID] = xmlField.ID;
                        //MetaData关联的隐藏column 不还原，但是需要将其加到IDMapping中以保证在还contenttype的fieldlink的时候能够找到
                        if (field.TypeAsString.Equals("TaxonomyFieldType", StringComparison.OrdinalIgnoreCase) || field.TypeAsString.Equals("TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase))
                        {
                            IAveTaxonomyField taxField = field as IAveTaxonomyField;
                            if (taxField.TextField != null && xmlField.GetCustomerProperty("TextField") != null && !taxField.TextField.ToString().Equals(xmlField.GetCustomerProperty("TextField").ToString(), StringComparison.OrdinalIgnoreCase))
                            {
                                FieldMapping.AddFieldIdMapping(new Guid(xmlField.GetCustomerProperty("TextField").ToString()), taxField.TextField);
                                //mFieldIdMapping[new Guid(xmlField.GetCustomerProperty("TextField").ToString())] = taxField.TextField;
                            }
                        }
                    }

                    //AnalyzeLookupField(field, mAveParentSite.SPSite);

                    if (xmlField.Type == AveFieldType.Lookup && field != null && isConflict)
                    {
                        log.Info("Restore lookup column:{0}\r\n{1}", xmlField.XmlElement.OuterXml, field.SchemaXml);
                    }
                    this.reportor.AddDetail(new AveWrapperReportDto(xmlField.Title, objectTitle, reportType, AveStatus.Successful, string.Empty));
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
                    log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper,
                        new EventIds.SharePoint.RestoreColumnFailedEventMessage(xmlField.FieldInternalName, xmlField.Title,
                            fieldType == FieldType.Web ? ContextValues.SharePoint.ObjectType.Site : ContextValues.SharePoint.ObjectType.List, ex));
                    if (isEnsureField)
                    {
                        throw;
                    }
                    status = FieldRestoreStatus.Exception;
                    if (ex is AveTermSetNotFoundException)
                    {
                        AveWrapperI18NException e = ex as AveWrapperI18NException;
                        this.reportor.AddDetail(new AveWrapperReportDto(xmlField.Title, objectTitle, reportType, AveStatus.Failed, AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(WrapperReportResourceKey.Wrapper_TermSetNotFound.ToString(), WrapperRestoreReportResource.Wrapper_TermSetNotFound, e.Args.ToArray())));
                    }
                    else
                    {
                        this.reportor.AddDetail(new AveWrapperReportDto(xmlField.Title, objectTitle, reportType, AveStatus.Failed, ex.Message));
                    }
                    return null;
                }
                return field;
#if PerformanceLog
            }
#endif
        }

        private bool UpdateUserResource(IAveField field, AveXmlField xmlField,bool isConflict)
        {
            bool needUpdate = false;
            // 新建field不走API 获取UserResource逻辑，减少request
            if (field.TitleResource.SetUserResource(mAveSPWeb.SPWeb, xmlField.TitleResource, false, isConflict))
            {
                field.TitleResource.Update();
                needUpdate = true;
            }
            if (field.DescriptionResource.SetUserResource(mAveSPWeb.SPWeb, xmlField.DescriptionResource, false))
            {
                field.DescriptionResource.Update();
                needUpdate = true;
            }
            return needUpdate;
        }

        /*private void AnalyzeLookupField(IAveField field, IAveSite site)
        {
            if (field == null)
            {
                throw new ArgumentNullException("field");
            }

            var lookupField = field as IAveFieldLookup;

            if (lookupField != null)
            {
                if ("Lookup".Equals(lookupField.TypeAsString, StringComparison.OrdinalIgnoreCase) ||
                    "LookupMulti".Equals(lookupField.TypeAsString, StringComparison.OrdinalIgnoreCase))
                {
                    Guid listId;

                    if (Guid.TryParse(lookupField.LookupList, out listId))
                    {
                        var webId = lookupField.LookupWebId;
                        var lookupColumn = lookupField.LookupField;
                        var web = site.OpenWeb(webId);
                        try
                        {
                            var list = web.Lists.GetById(listId);
                            var lkField = list.Fields.GetFieldByInternalName(lookupColumn);
                        }
                        catch (Exception e)
                        {
                            log.Error("The lookup field is invalid, please verify the lookup list id:{0}, web id:{1}, field name:{2} for field:{3}, details:{4}", listId, webId, lookupColumn, field.Title, e);
                            throw new Exception(string.Format("The lookup field is invalid, please verify the lookup list id:{0}, web id:{1}, field name:{2} for field:{3}", listId, webId, lookupColumn, field.Title));
                        }
                        finally
                        {
                            if (!web.IsRootWeb)
                            {
                                web.Dispose();
                            }
                        }
                    }
                }
            }
        }*/

        private void SetFieldId(XmlElement fieldElement)
        {
            if (fieldElement.HasAttribute("ID"))
            {
                fieldElement.SetAttribute("ID", Guid.NewGuid().ToString("B"));
            }
        }
        private void HandleConflict(AveXmlField xmlField, ref IAveField field, FieldType fieldType, IAveFieldCollection fields, FieldConflictOption conflictOption, FieldMatchType matchType)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFieldCollection.HandleConflict"))
            {
#endif
                try
                {
                    FieldRestoreStatus status = FieldRestoreStatus.None;
                    switch (conflictOption)
                    {
                        case FieldConflictOption.Skip:
                            return;
                        case FieldConflictOption.Overwrite:
                            UpdateField(fields, field, xmlField);
                            break;
                        case FieldConflictOption.AppendSourceWin:
                            if (xmlField.Title.Equals(field.Title, StringComparison.OrdinalIgnoreCase))
                            {
                                field.Title = GetNewDisplayName(field.Title);
                                field.Update();
                            }
                            UpdateNewFieldXml(xmlField, matchType, false);
                            field = CreateNewField(xmlField, fieldType, fields, ref status);
                            break;
                        case FieldConflictOption.AppendDestinationWin:
                            UpdateNewFieldXml(xmlField, matchType, true);
                            field = CreateNewField(xmlField, fieldType, fields, ref status);
                            break;
                    }
                }
                catch (AveWrapperException)
                {
                    throw;
                }
                catch (AveTermSetNotFoundException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new AveWrapperException(AveWrapperErrorCode.FieldHandleConflictError, string.Format("An error occurred while handling conflict. ConflictOption:{0}", conflictOption), ex);
                }
#if PerformanceLog
            }
#endif
        }

        private void UpdateNewFieldXml(AveXmlField xmlField, FieldMatchType matchType, bool isModifyDisplayName)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFieldCollection.UpdateNewFieldXml"))
            {
#endif
                string newName = "";
                if (isModifyDisplayName)
                {
                    newName = GetNewDisplayName(xmlField.XmlElement.GetAttribute("DisplayName"));
                    xmlField.XmlElement.SetAttribute("DisplayName", newName);
                }
                else
                {
                    if ((matchType & FieldMatchType.Name) == FieldMatchType.Name)
                    {
                        if (this is AveSPListFieldCollection)
                        {
                            newName = AveFieldHelper.GetNewInternalName(xmlField.XmlElement.GetAttribute("Name"), FieldCollection);
                        }
                        else
                        {
                            newName = AveFieldHelper.GetNewInternalName(xmlField.XmlElement.GetAttribute("Name"), mAveSPWeb.SPWeb.AvailableFields);
                        }
                        xmlField.XmlElement.SetAttribute("Name", newName);
                    }
                }
                if ((matchType & FieldMatchType.ID) == FieldMatchType.ID)
                {
                    xmlField.XmlElement.SetAttribute("ID", Guid.NewGuid().ToString());
                }
                //当通过Schema匹配到的时候，需要去判断目的端是否存在相同ID的column，若存在，需重新New
                if ((matchType & FieldMatchType.Schema) == FieldMatchType.Schema)
                {
                    string Id = xmlField.XmlElement.GetAttribute("ID");
                    if (this is AveSPListFieldCollection)
                    {
                        if (AveFieldHelper.FindFieldInCollection(new Guid(Id), FieldCollection) != null)
                        {
                            xmlField.XmlElement.SetAttribute("ID", Guid.NewGuid().ToString());
                        }
                    }
                    else
                    {
                        if (AveFieldHelper.FindFieldInCollection(new Guid(Id), mAveSPWeb.SPWeb.AvailableFields) != null)
                        {
                            xmlField.XmlElement.SetAttribute("ID", Guid.NewGuid().ToString());
                        }
                    }
                }
#if PerformanceLog
            }
#endif
        }

        private string GetNewDisplayName(string title)
        {
            string newName = "";
            if (this is AveSPListFieldCollection)
            {
                newName = AveFieldHelper.GetNewDisplayName(title, FieldCollection);
            }
            else
            {
                newName = AveFieldHelper.GetNewDisplayName(title, mAveSPWeb.SPWeb.AvailableFields);
            }
            return newName;

        }

        protected bool CheckFilter(string type)
        {
            bool needFilter = false;
            if (mFieldTypeFilter != null && mFieldTypeFilter.Contains(type))
            {
                needFilter = true;
            }
            else if (mAveSPWeb.SPWeb.FieldTypeDefinitionCollection != null && mAveSPWeb.SPWeb.FieldTypeDefinitionCollection[type] == null)
            {
                needFilter = true;
            }
            return needFilter;
        }

        protected bool CheckFilter(XmlElement xe)
        {
            string type = xe.GetAttribute("Type");
            bool needFilter = false;
            if (type == "Tag")
            {
                AveCustomFieldInfo mappingInfo = FieldMapping.GetMappingFieldBeforeAdd(new AveSourceFieldInfo() { SourceInternalName = xe.GetAttribute("Name"), SourceDisplayName = xe.GetAttribute("DisplayName"), SourceFieldId = new Guid(xe.GetAttribute("ID")) });
                if (mappingInfo != null && mappingInfo.CustomFieldTypeAsString == "ChangeToMetadata")
                {
                    needFilter = false;
                }
            }
            else if (mFieldTypeFilter != null && mFieldTypeFilter.Contains(type))
            {
                needFilter = true;
            }
            else if (mAveSPWeb.SPWeb.FieldTypeDefinitionCollection != null && mAveSPWeb.SPWeb.FieldTypeDefinitionCollection[type] == null)
            {
                needFilter = true;
            }

            return needFilter;
        }

        protected IAveField SkipFieldRestore(string name, ref FieldRestoreStatus status)
        {
            if (mSkippedRestoreFields == null)
            {
                mSkippedRestoreFields = new List<string>();
            }
            if (!mSkippedRestoreFields.Contains(name))
            {
                mSkippedRestoreFields.Add(name);
            }
            status = FieldRestoreStatus.Skipped;
            return null;
        }

        protected IAveField CreateNewField(AveXmlField xmlField, FieldType fieldType, IAveFieldCollection fields, ref FieldRestoreStatus status, bool isEnsureField = false)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFieldCollection.UpdateNewFieldXml"))
            {
#endif
                //判断是否配置了CustomMapping，若是,则按照CustomMapping处理

                if (xmlField.CustomFieldInfo != null)
                {
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
                UpdateSourceID(xmlField);
                AddDataSourceField(xmlField);
                bool needPostAction = AddLookupField(xmlField);
                UpdateCalculatedFieldInfo(xmlField, fields, fieldType, isEnsureField);
                if (xmlField.XmlElement.HasAttribute("Version"))
                {
                    xmlField.XmlElement.RemoveAttribute("Version");
                }
                if (xmlField.XmlElement.HasAttribute("Type"))
                {
                    string type = xmlField.XmlElement.GetAttribute("Type");
                    if (CheckFilter(type))
                    {
                        return SkipFieldRestore(xmlField.KeyName, ref status);
                    }
                    try
                    {
                        switch (type)
                        {
                            case "UserMulti":
                            case "User":
                                int srcMemberId = Convert.ToInt32(xmlField.XmlElement.Attributes["UserSelectionScope"].Value);
                                IAvePrincipal desPrincipal = this.mAveSPWeb.ParentSite.SPMembers.FindMember(srcMemberId, true);
                                if (desPrincipal != null)
                                {
                                    xmlField.XmlElement.Attributes["UserSelectionScope"].Value = desPrincipal.ID.ToString();
                                }
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.CreateUserSelectionScopeError, e);
                    }
                }

                string validationFormula = null;
                string validationMessage = null;
                bool hasValidation = GetValidationInfo(xmlField.XmlElement, ref validationMessage, ref validationFormula);
                //创建一个field的时候只要要判断目的端是否存在跟他internalName同名的field,否则需要修改然后添加
                UpdateNewFieldXml(xmlField, FieldMatchType.Name, false);
                //column mapping: 当column mapping判断需要添加新的column时 需判断fieldID是否有冲突,有则修改后添加
                if (AveFieldHelper.FindFieldInCollection(new Guid(xmlField.XmlElement.GetAttribute("ID")), fields) != null)
                {
                    UpdateNewFieldXml(xmlField, FieldMatchType.ID, false);
                }
                string sourceDeleteBehavior = string.Empty;
                if (xmlField.XmlElement.HasAttribute("RelationshipDeleteBehavior") && needPostAction)
                {
                    sourceDeleteBehavior = xmlField.XmlElement.Attributes["RelationshipDeleteBehavior"].Value;
                    xmlField.XmlElement.SetAttribute("RelationshipDeleteBehavior", "None");
                }
                string sourceRelationShip = string.Empty;
                if (xmlField.XmlElement.HasAttribute("IsRelationship") && needPostAction)
                {
                    sourceRelationShip = xmlField.XmlElement.Attributes["IsRelationship"].Value;
                    xmlField.XmlElement.SetAttribute("IsRelationship", "False");
                }
                try
                {

                    field = fields.AddFieldAsXml(xmlField.XmlElement.OuterXml, false, AveAddFieldOptions.AddFieldInternalNameHint | AveAddFieldOptions.AddToNoContentType);
                    if (!sourceDeleteBehavior.Equals(string.Empty) && needPostAction)
                    {
                        xmlField.XmlElement.SetAttribute("RelationshipDeleteBehavior", sourceDeleteBehavior);
                    }
                    if (!sourceRelationShip.Equals(string.Empty) && needPostAction)
                    {
                        xmlField.XmlElement.SetAttribute("IsRelationship", sourceRelationShip);
                    }
                    //field = fields.GetFieldByInternalName(internalName);
                    status = FieldRestoreStatus.NewCreated;
                    AddLookupField(xmlField, field.ID, needPostAction);
                    if (hasValidation)
                    {
                        AveFieldHelper.UpdateValidationInfo(field, validationMessage, validationFormula);
                    }
                    #region MetaDataColumn添加时没有对customproperty进行转换，导致跨metadataService的job情况，column还原后无法使用，所以在添加结束后也要update一下
                    //bpos-s doesn't support taxonomy field

                    bool needUpdate = false;
                    if (field.TypeAsString == "TaxonomyFieldType" || field.TypeAsString == "TaxonomyFieldTypeMulti")
                    {
                        if (AveTaxonomyField.UpdateTaxonomyFieldCommonProperties(mAveSPWeb.ParentSite, field, xmlField))
                        {
                            needUpdate = true;
                        }
                    }

                    #endregion
                    // needUpdate |= UpdateUserResource(field, xmlField);
                    if (needUpdate)
                    {
                        field.Update();
                    }
                    return field;
                }
                catch
                {
                    if (field != null)
                    {
                        field.Delete();
                    }
                    throw;
                }
#if PerformanceLog
            }
#endif
        }

        protected IAveField CreateNewFieldByCustomMapping(AveXmlField xmlField, IAveFieldCollection fields, ref FieldRestoreStatus status)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFieldCollection.CreateChoiceField"))
            {
#endif
                try
                {
                    IAveField field = null;
                    if (String.IsNullOrEmpty(xmlField.CustomFieldInfo.Name))
                    {
                        xmlField.CustomFieldInfo.Name = xmlField.Title;
                    }
                    //xmlField.CustomFieldInfo.InternalName = GenerateInternalName(xmlField.CustomFieldInfo);
                    status = FieldRestoreStatus.NewCreated;
                    if (xmlField.CustomFieldInfo.Type != AveFieldType.Invalid)
                    {
                        switch (xmlField.CustomFieldInfo.Type)
                        {
                            case AveFieldType.Lookup:
                                field = CreateLookupField(fields, xmlField);
                                break;
                            case AveFieldType.Choice:
                            case AveFieldType.MultiChoice:
                                field = CreateChoiceField(fields, xmlField);
                                break;
                            default:
                                string title = fields.Add(xmlField.CustomFieldInfo.Name, xmlField.CustomFieldInfo.Type, false);
                                field = fields[title];
                                break;
                        }

                    }
                    else
                    {
                        //to do ,需要根据信息构造出相关SchemaXml再进行add
                        switch (xmlField.CustomFieldInfo.TypeAsString)
                        {
                            case "TaxonomyFieldType":
                                field = CreateMetadataField(fields, xmlField);
                                break;
                            default:
                                break;
                        }
                    }

                    return field;
                }
                catch (AveWrapperException)
                {
                    throw;
                }
                catch (AveTermSetNotFoundException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new AveWrapperException(AveWrapperErrorCode.CreateFieldByCustomMappingError, "An error occurred while create field by custom mapping.", ex);
                }
#if PerformanceLog
            }
#endif
        }

        /*protected virtual string GenerateInternalName(AveCustomFieldInfo customFieldInfo)
        {
            //List级别目前 不需要考虑InternalName冲突的情况
            return customFieldInfo.InternalName;
        }*/

        private string GetLookupFieldInternalName(IAveWeb web, AveCustomLookupFieldInfo lookInfo)
        {
            //todo performance
            IAveList list = web.Lists[lookInfo.ListTitle];

            if (list.Fields.Where(n => n.Title == lookInfo.FieldName).Count() > 0)
            {
                return list.Fields[lookInfo.FieldName].InternalName;
            }
            return null;
        }

        protected IAveField CreateChoiceField(IAveFieldCollection fields, AveXmlField xmlField)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFieldCollection.CreateChoiceField"))
            {
#endif
                IAveField field = null;
                try
                {
                    if (xmlField.CustomFieldInfo.IsMulti)
                    {
                        xmlField.CustomFieldInfo.Type = AveFieldType.MultiChoice;
                    }
                    string title = fields.Add(xmlField.CustomFieldInfo.Name, xmlField.CustomFieldInfo.Type, false);
                    field = fields[title];
                    IAveFieldChoice choiceField = field as IAveFieldChoice;
                    if (xmlField.Choices != null && xmlField.Choices.Count > 0)
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
#if PerformanceLog
            }
#endif
        }
        protected IAveField CreateLookupField(IAveFieldCollection fields, AveXmlField xmlField)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFieldCollection.CreateLookupField"))
            {
#endif
                IAveField field = null;
                try
                {
                    AveCustomLookupFieldInfo lookupInfo = xmlField.CustomFieldInfo as AveCustomLookupFieldInfo;
                    IAveWeb web = mAveSPWeb.SPWeb;
                    if (!string.IsNullOrEmpty(lookupInfo.WebRelativeUrl))
                    {
                        web = mAveParentSite.SPSite.OpenWeb(lookupInfo.WebRelativeUrl.Replace(mAveParentSite.SPSite.RootWeb.ServerRelativeUrl.TrimStart('/'), "").TrimStart('/'));
                    }
                    Guid lookupWebId = web.ID;
                    IAveList list = web.Lists[lookupInfo.ListTitle];
                    Guid lookupListId = list.ID;
                    field = fields.AddLookup(lookupInfo.Name, lookupListId, lookupWebId, false);
                    string lookedFieldName = lookupInfo.FieldName;
                    string internalName = GetLookupFieldInternalName(web, lookupInfo);
                    ///如果用internal取不存在，则当做display name取，因为客户不一定知道internal name
                    IAveField lookedField = null;
                    try
                    {
                        if (!string.IsNullOrEmpty(internalName))
                        {
                            lookedField = list.Fields.GetFieldByInternalName(internalName);
                        }
                        lookedField = list.Fields.GetFieldByInternalName(lookedFieldName);
                    }
                    catch (Exception e)
                    {
                        log.Warn("Get lookup field error use internal name while creating mapping lookup field: {0}.", e.Message);
                        try
                        {
                            lookedField = list.Fields[lookedFieldName];
                            lookedFieldName = lookedField.InternalName;
                        }
                        catch (Exception e2)
                        {
                            log.Warn("Get lookup field error use display name while creating mapping lookup field: {0}.", e2.Message);
                        }
                    }
                    (field as IAveFieldLookup).LookupField = lookedFieldName;
                    //需要根据源端类型去决定mapping成Mult类型的field
                    if (lookupInfo.IsMulti)
                    {
                        (field as IAveFieldLookup).AllowMultipleValues = true;
                    }
                    field.Update();
                }
                catch (Exception ex)
                {
                    log.Warn("Create the custom lookup field error,Field:{0}, Exception:{1}", xmlField.CustomFieldInfo.Name, ex.ToString());
                    throw;
                }
                return field;
#if PerformanceLog
            }
#endif
        }
        protected IAveField CreateMetadataField(IAveFieldCollection fields, AveXmlField xmlField)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFieldCollection.CreateMetadataField"))
            {
#endif
                IAveField field = null;
                try
                {
                    AveCustomMetadataFieldInfo metadataInfo = xmlField.CustomFieldInfo as AveCustomMetadataFieldInfo;
                    string metadataXml = CustomFieldXml.MetadataFieldXml;
                    //metadataXml = metadataXml.Replace("TermSetForReplace", metadataInfo.TermSet);
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(CustomFieldXml.MetadataFieldXml);
                    doc.DocumentElement.SetAttribute("DisplayName", metadataInfo.Name);
                    doc.DocumentElement.SetAttribute("StaticName", metadataInfo.Name);
                    doc.DocumentElement.SetAttribute("Name", metadataInfo.Name);
                    //doc.DocumentElement.SetAttribute("Description", xmlField.Description);
                    //doc.DocumentElement.SetAttribute("List", mAveSPList.SPList.ID.ToString());
                    //doc.DocumentElement.SetAttribute("SourceID", mAveSPList.SPList.ID.ToString());
                    //doc.DocumentElement.SetAttribute("WebId", mAveSPList.ParentWeb.SPWeb.ID.ToString());
                    doc.DocumentElement.SetAttribute("ID", Guid.NewGuid().ToString());
                    if (doc.DocumentElement.HasAttribute("Version"))
                    {
                        doc.DocumentElement.RemoveAttribute("Version");
                    }

                    field = fields.AddFieldAsXml(doc.OuterXml, false, AveAddFieldOptions.AddFieldInternalNameHint | AveAddFieldOptions.AddToNoContentType);

                    //更新xml中的termGroup和termSet等设置
                    if (doc.DocumentElement.SelectNodes("//Customization//ArrayOfProperty").Count > 0)
                    {
                        XmlElement arrayProperty = doc.DocumentElement.SelectNodes("//Customization//ArrayOfProperty")[0] as XmlElement;
                        foreach (XmlElement propertyElement in arrayProperty.ChildNodes)
                        {
                            if (propertyElement.Name.Equals("Property"))
                            {
                                string name = null;
                                XmlNodeList elements = propertyElement.GetElementsByTagName("Name");
                                if (elements != null && elements.Count > 0)
                                {
                                    XmlElement nameElement = (XmlElement)elements[0];
                                    name = nameElement.InnerText;
                                    elements = propertyElement.GetElementsByTagName("Value");
                                    if (elements != null && elements.Count > 0)
                                    {
                                        XmlElement valueElement = (XmlElement)elements[0];
                                        if (name.Equals("GroupId"))
                                        {
                                            valueElement.InnerText = valueElement.InnerText + "|" + metadataInfo.TermGroup;

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

                    AveXmlField tmpXmlField = new AveXmlField(doc.DocumentElement);
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
                    log.Warn("Create the custom metadata field error,Field:{0}, Exception:{1}", xmlField.CustomFieldInfo.Name, ex.ToString());
                    throw;
                }
                return field;
#if PerformanceLog
            }
#endif
        }
        protected void AddLookupField(AveXmlField xmlField, Guid desFieldID, bool hasUpdate)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFieldCollection.AddLookupField"))
            {
#endif
                if (xmlField.Type == AveFieldType.User)
                {
                    return;
                }
                if (xmlField.TypeAsString.Equals("TaxonomyFieldType") || xmlField.TypeAsString.Equals("TaxonomyFieldTypeMulti"))
                {
                    return;
                }
                if ((xmlField.Type == AveFieldType.Lookup) && !String.IsNullOrEmpty(xmlField.AveLookupListTitle))
                {
                    AveLookupObject obj = new AveLookupObject();
                    obj.Id = desFieldID;
                    obj.ListTitle = xmlField.AveLookupListTitle;
                    obj.WebUrl = xmlField.AveLookupWebTitle;
                    obj.Type = xmlField.AveSourceType;
                    obj.WebId = mWebId;
                    obj.List = xmlField.AveLookupListID;

                    if (mAveSPList != null)
                    {
                        obj.ListId = mAveSPList.SPList.ID;
                        mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.AddLookupField(obj);
                    }
                }
                if ((!string.IsNullOrEmpty(xmlField.LookupList) && mAveSPList != null && AveFieldHelper.IsGuid((xmlField.LookupList))) || hasUpdate)
                {
                    //在这个地方出现了xmlField.LookupList为Self，Docs和UserInfo的情况，导致下面会抛出异常。
                    //在此暂作处理。
                    if (!AveFieldHelper.IsGuid(xmlField.LookupList))
                    {
                        return;
                    }
                    AveLookupObject obj = new AveLookupObject();
                    obj.Id = xmlField.ID;
                    obj.Type = xmlField.AveSourceType;
                    obj.WebUrl = xmlField.AveLookupWebTitle;
                    obj.WebId = mWebId;
                    obj.List = xmlField.LookupList;
                    if (xmlField.XmlElement.HasAttribute("RelationshipDeleteBehavior"))
                    {
                        if (xmlField.XmlElement.Attributes["RelationshipDeleteBehavior"].Value.Equals("Cascade", StringComparison.OrdinalIgnoreCase))
                        {
                            obj.DeleteBehavior = AveRelationshipDeleteBehavior.Cascade;
                        }
                        else if (xmlField.XmlElement.Attributes["RelationshipDeleteBehavior"].Value.Equals("Restrict", StringComparison.OrdinalIgnoreCase))
                        {
                            obj.DeleteBehavior = AveRelationshipDeleteBehavior.Restrict;
                        }
                        else
                        {
                            obj.DeleteBehavior = AveRelationshipDeleteBehavior.None;
                        }
                    }
                    if (mAveSPList != null)
                    {
                        obj.ListId = mAveSPList.SPList.ID;
                    }//TODOLMM
                    if (!mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.NotUpdateLookupFieldCache.ContainsKey(new Guid(obj.List)))
                    {
                        mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.NotUpdateLookupFieldCache[new Guid(obj.List)] = new List<AveLookupObject>();
                    }
                    mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.NotUpdateLookupFieldCache[new Guid(obj.List)].Add(obj);

                    log.Info("Add lookup field into post action:field id:{0}, lookup web id:{1}, lookup list:{2}, column listId:{3}", obj.Id, obj.WebId, obj.List, obj.ListId);
                }
#if PerformanceLog
            }
#endif
        }

        private bool GetValidationInfo(XmlElement fieldElement, ref string validationMessage, ref string validationFormula)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFieldCollection.GetValidationInfo"))
            {
#endif
                validationFormula = string.Empty;
                validationMessage = string.Empty;

                XmlNodeList nodeList = fieldElement.GetElementsByTagName("Validation");
                if (nodeList.Count > 0)
                {
                    XmlElement validationElement = nodeList[0] as XmlElement;
                    validationFormula = validationElement.InnerText;
                    validationMessage = validationElement.GetAttribute("Message");
                    fieldElement.RemoveChild(validationElement);
                    return true;
                }
                return false;
#if PerformanceLog
            }
#endif
        }

        protected void UpdateSourceID(AveXmlField srcXmlField)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFieldCollection.UpdateSourceID"))
            {
#endif
                if (srcXmlField.XmlElement.HasAttribute("SourceID"))
                {
                    if (srcXmlField.AveSourceType == "2" && mAveSPList != null)
                    {
                        srcXmlField.XmlElement.SetAttribute("SourceID", mAveSPList.SPList.ID.ToString("B"));
                    }
                    else if (srcXmlField.AveSourceType == "1" && mAveSPWeb != null)
                    {
                        if (mAveSPList != null)
                        {
                            IAveField parentField = FindFieldFromParentWeb(srcXmlField, mAveSPWeb.SPWeb.AvailableFields);
                            if (parentField != null)
                            {
                                Guid tempId = Guid.Empty;
                                if (Guid.TryParse(parentField.SourceId, out tempId))
                                {
                                    srcXmlField.XmlElement.SetAttribute("SourceID", tempId.ToString("B"));
                                }
                                else
                                {
                                    srcXmlField.XmlElement.SetAttribute("SourceID", parentField.SourceId);
                                }
                                return;
                            }
                        }
                        srcXmlField.XmlElement.SetAttribute("SourceID", mAveSPWeb.SPWeb.ID.ToString("B"));

                    }
                    else if (srcXmlField.AveSourceType == "0")
                    {
                        //==0,说明SourceID不是本list的或者本web以及parent web的，需要keep
                        //srcXmlField.XmlElement.SetAttribute("SourceID", mAveSPWeb.ParentSite.GetWeb(mAveSPWeb.QueryService, mAveSPWeb.ParentSite.SPSite.ServerRelativeUrl).ToString("B"));
                    }
                    else  //对于更改普通field（不是lookupfield）的sourceID，删掉schema中的SourceID后，再创建新的field的时候会自动添加当前SourceID
                    {
                        //修改逻辑后值只能是0,1,2，保留为了兼容老数据
                        srcXmlField.XmlElement.RemoveAttribute("SourceID");
                    }
                }
#if PerformanceLog
            }
#endif
        }

        protected void AddDataSourceField(AveXmlField srcXF)
        {
            if (mAveSPList != null && mAveSPList.SPList.TemplateFeatureId == new Guid("065c78be-5231-477e-a972-14177cc5b3c7") && srcXF.Title == "Data Source")
            {
                string key = mAveSPList.SPList.ID.ToString() + ":" + mAveSPWeb.SPWeb.ID.ToString();
                if (!mAveSPWeb.ParentSite.KpiListIdCol.Contains(key))
                {
                    mAveSPWeb.ParentSite.KpiListIdCol.Add(key);
                }
            }
        }

        protected bool AddLookupField(AveXmlField srcXF)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFieldCollection.AddLookupField"))
            {
#endif
                bool addLookup = false;
                if (srcXF.Type == AveFieldType.Lookup)
                {
                    if (UpdateLookupFieldWebIdAndListIdFromParentWeb(srcXF))
                    {
                        addLookup = false;
                    }
                    else
                    {
                        addLookup = true;
                        Guid mappingId = Guid.Empty;
                        if (srcXF.XmlElement.HasAttribute("WebId") && AveTypeHelper.IsGuid(srcXF.XmlElement.GetAttribute("WebId")))
                        {
                            string swebId = srcXF.XmlElement.Attributes["WebId"].Value;
                            Guid webId = new Guid(swebId);
                            if (mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.WebIDMapping.ContainsKey(webId))
                            {
                                mappingId = mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.WebIDMapping[webId];
                            }
                            if (mappingId == Guid.Empty)
                            {
                                string destWebUrl = AveReplaceProcessor.UrlReplace(srcXF.AveLookupWebTitle, mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true), mAveSPWeb.ParentSite.SourceSiteInfo, mAveSPWeb.ParentSite.ServerRelativeUrl);
                                mappingId = mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.GetMappingWeb(mAveSPWeb.ParentSite.SPSite, destWebUrl, true);
                            }
                        }
                        else
                        {
                            mappingId = mAveSPWeb.SPWeb.ID;
                        }
                        if (!mappingId.Equals(Guid.Empty) && (!string.IsNullOrEmpty(srcXF.AveLookupListTitle)) && (!string.IsNullOrEmpty(srcXF.AveLookupListID)))
                        {
                            srcXF.XmlElement.SetAttribute("WebId", mappingId.ToString());
                            string realListTitle = srcXF.AveLookupListTitle;
                            realListTitle = mAveSPWeb.ParentSite.GetNameByLanguageMapping(realListTitle, AveLanguageMappingType.ListMapping);
                            Guid listId = mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.GetMappingList(mAveSPWeb.ParentSite.SPSite, mappingId, realListTitle, new Guid(srcXF.AveLookupListID));
                            if (listId == Guid.Empty && mAveSPList != null)
                            {
                                IAveFieldLookup parentField = AveFieldHelper.FindFieldInCollection(srcXF.ID, mAveSPWeb.SPWeb.Fields) as IAveFieldLookup;
                                if (parentField != null)
                                {
                                    if (!string.IsNullOrEmpty(parentField.LookupList))
                                    {
                                        string parentFieldLookupListId = ((IAveFieldLookup)parentField).LookupList;
                                        listId = mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.GetMappingList(mAveSPWeb.ParentSite.SPSite, mappingId, realListTitle, new Guid(parentFieldLookupListId));
                                    }
                                    if (mappingId.Equals(Guid.Empty) && parentField.LookupWebId != Guid.Empty)
                                    {
                                        srcXF.XmlElement.SetAttribute("WebId", parentField.LookupWebId.ToString());
                                    }
                                }
                            }
                            if (!listId.Equals(Guid.Empty))
                            {
                                srcXF.XmlElement.SetAttribute("List", listId.ToString("B"));
                                addLookup = false;
                            }
                            else
                            {
                                addLookup = true;
                            }
                        }
                        else
                        {
                            //如果找不到关联的web则放到post action里还原
                            addLookup = true;
                        }
                    }
                    //replace the FieldRef ID
                    try
                    {
                        if (!String.IsNullOrEmpty(srcXF.PrimaryFieldId) && AveTypeHelper.IsGuid(srcXF.PrimaryFieldId))
                        {
                            Guid mappingValue = FieldMapping.GetMappingRestoredFieldId(new Guid(srcXF.PrimaryFieldId));
                            if (mappingValue != Guid.Empty)
                            {
                                srcXF.XmlElement.SetAttribute("FieldRef", mappingValue.ToString());
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, WrapperRestoreResource.ReplaceFieldRefIDFailed, e);
                    }
                }
                if (srcXF.TypeAsString == "TaxonomyFieldType" || srcXF.TypeAsString == "TaxonomyFieldTypeMulti")
                {
                    srcXF.XmlElement.SetAttribute("WebId", mAveSPWeb.ParentSite.SPSite.RootWeb.ID.ToString());
                    if (mAveSPWeb.ParentSite.SPSite.RootWeb.Properties.ContainsKey("TaxonomyHiddenList"))
                    {
                        string listId = (new Guid(mAveSPWeb.ParentSite.SPSite.RootWeb.Properties["TaxonomyHiddenList"])).ToString("B");
                        srcXF.XmlElement.SetAttribute("List", listId);
                    }
                    else
                    {
                        try
                        {
                            IAveList taxonomyList = mAveSPWeb.ParentSite.SPSite.RootWeb.Lists["TaxonomyHiddenList"];
                            srcXF.XmlElement.SetAttribute("List", taxonomyList.ID.ToString());
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.WARN, WrapperRestoreResource.GetTaxonomyListFailed, e);
                        }
                    }
                }
                //when update lookupfield linked list in postaction, there will be an error if don't remove the 'list' attribute
                if (addLookup)
                {
                    srcXF.XmlElement.RemoveAttribute("WebId");
                    srcXF.XmlElement.RemoveAttribute("List");
                }
                return addLookup;
#if PerformanceLog
            }
#endif
        }

        protected bool UpdateLookupFieldWebIdAndListIdFromParentWeb(AveXmlField srcXF)
        {
            bool updatedFromParentWeb = false;
            if (srcXF.Type == AveFieldType.Lookup)
            {
                if (srcXF.AveSourceType == "1")
                {
                    if (mAveSPWeb != null && mAveSPWeb.SPWeb != null)
                    {
                        IAveFieldLookup parentField = FindFieldFromParentWeb(srcXF, mAveSPWeb.SPWeb.AvailableFields) as IAveFieldLookup;
                        if (parentField != null)
                        {
                            if (!string.IsNullOrEmpty(parentField.LookupList) && parentField.LookupWebId != Guid.Empty)
                            {
                                IAveList list = null;

                                try
                                {
                                    list = mAveParentSite.SPSite.OpenWeb(parentField.LookupWebId).GetList(new Guid(parentField.LookupList));
                                }
                                catch (Exception ex)
                                {
                                    log.Info("the lookup column is invalid, ListId: {0}, Web: {1}, exception:{2}", parentField.LookupList, parentField.LookupWebId, ex.Message);
                                }

                                if (list != null)
                                {
                                    srcXF.XmlElement.SetAttribute("List", parentField.LookupList);
                                    srcXF.XmlElement.SetAttribute("WebId", parentField.LookupWebId.ToString());
                                    updatedFromParentWeb = true;
                                    log.Info("Update lookup field from parent web, ListId: {0}, Web: {1}", parentField.LookupList, parentField.LookupWebId);
                                }
                            }
                        }
                    }
                }
            }
            return updatedFromParentWeb;
        }

        protected IAveField FindFieldFromParentWeb(AveXmlField xmlField, IAveFieldCollection fieldCollection)
        {
            IAveField field = AveFieldHelper.FindFieldInCollection(xmlField.ID, fieldCollection);
            if (field == null)
            {
                field = AveFieldHelper.FindFieldInCollection(xmlField.KeyName, xmlField.Type, fieldCollection);
            }
            if (field == null)
            {
                field = AveFieldHelper.FindFieldInCollectionByStaticName(xmlField.XmlElement.GetAttribute("StaticName"), xmlField.Type, fieldCollection);
            }
            if (field == null)
            {
                field = AveFieldHelper.FindFieldInCollection(xmlField.XmlElement.GetAttribute("DisplayName"), xmlField.Type, true, fieldCollection);
            }
            return field;
        }


        protected void SetFieldMapping(string sourceFieldInternalName, string sourceFieldDisplayName, Guid sourceFieldID, IAveField field, bool isEnsureField)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFieldCollection.SetFieldMapping"))
            {
#endif
                RestoredFieldInternalNameList.Add(sourceFieldInternalName);
                if (!field.ID.Equals(sourceFieldID) && FieldMapping.GetMappingRestoredFieldId(sourceFieldID) == Guid.Empty)
                {
                    //mFieldIdMapping[sourceFieldID] = field.ID;
                    FieldMapping.AddFieldIdMapping(sourceFieldID, field.ID);

                }
                if (!field.InternalName.Equals(sourceFieldInternalName) && String.IsNullOrEmpty(FieldMapping.GetMappingRestoredFieldInternalName(sourceFieldInternalName)))
                {
                    //mFieldInternalNameMapping[sourceFieldInternalName] = field.InternalName;
                    FieldMapping.AddFieldInternalNameMapping(sourceFieldInternalName, field.InternalName);
                }
                if (!field.Title.Equals(sourceFieldDisplayName) && String.IsNullOrEmpty(FieldMapping.GetMappingRestoredFieldDisplayName(sourceFieldDisplayName)))
                {
                    //mFieldDisplayNameMapping[sourceFieldDisplayName] = field.Title;
                    FieldMapping.AddFieldDisplayNameMapping(sourceFieldDisplayName, field.Title);
                }
                if (!field.ID.Equals(sourceFieldID) && FieldMapping.GetMappingSchemaFieldId(sourceFieldID) == Guid.Empty)
                {
                    //FieldIdSchemaMappings[sourceFieldID] = field.ID;
                    FieldMapping.AddFieldIdSchemaMapping(sourceFieldID, field.ID);
                }
                //if (isEnsureField && !mEnsureFields.ContainsKey(field.ID))
                //{
                //    mEnsureFields[field.ID] = field.InternalName;
                //}
#if PerformanceLog
            }
#endif
        }

        protected void UpdateCalculatedFieldInfo(AveXmlField srcXF, IAveFieldCollection fields, FieldType type, bool isEnsureField = false)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFieldCollection.AddCalculatedField"))
            {
#endif
                XmlNodeList nodes = srcXF.XmlElement.GetElementsByTagName("Formula");
                XmlNodeList fieldRefNodes = srcXF.XmlElement.GetElementsByTagName("FieldRef");
                if (nodes != null && nodes.Count > 0)
                {
                    string formula = null;
                    formula = nodes[0].InnerText;
                    Dictionary<string, string> fieldDic = new Dictionary<string, string>();
                    for (int i = 0; i < fieldRefNodes.Count; i++)
                    {
                        string oldInternalName = fieldRefNodes[i].Attributes["Name"].Value;
                        if (!formula.Contains(oldInternalName))
                            continue;
                        string internalName = mAveSPWeb.ParentSite.GetNameByLanguageMapping(oldInternalName, AveLanguageMappingType.FieldMapping);
                        string mappingValue = FieldMapping.GetMappingRestoredFieldInternalName(internalName);
                        if (!String.IsNullOrEmpty(mappingValue) && !internalName.Equals(mappingValue))
                        {
                            internalName = mappingValue;
                            fieldRefNodes[i].Attributes["Name"].Value = internalName;
                        }
                        if (!fieldDic.ContainsKey(oldInternalName) && type == FieldType.List)
                        {
                            try
                            {
                                string displayName = fields.GetField(internalName).Title;
                                displayName = "[" + displayName.Trim() + "]";
                                fieldDic[oldInternalName] = displayName;
                            }
                            catch (Exception e)
                            {
                                //SAAS-22400 calculate column当formula中的column不存在或者删除后,会导致column还原失败,这里替换成"#NAME?"来保证formula可以被添加.由于关联field可能在后面还
                                //所以只有在走反插逻辑时才会这样替换.
                                if (isEnsureField)
                                {
                                    log.Warn("get formula field failed named {0} when create a calculated field.{1}", internalName, e.Message);
                                    fieldDic[oldInternalName] = "#NAME?";
                                    continue;
                                }
                                throw;
                            }
                        }
                        if (!fieldDic.ContainsKey(oldInternalName) && type == FieldType.Web)
                        {
                            fieldDic.Add(oldInternalName, internalName.Trim());
                        }
                    }

                    formula = SetFormula(formula, fieldDic);
                    nodes[0].InnerText = formula;
                    srcXF.Formula = formula;
                }
#if PerformanceLog
            }
#endif
        }

        private string SetFormula(string formula, Dictionary<string, string> fieldDic)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFieldCollection.SetFormula"))
            {
#endif
                var dic = fieldDic.OrderByDescending(pair => pair.Key.Length);
                //SAAS-14576 不能使用Guid
                //因为当fieldDic超过十个:00000000-0000-0000-0000-0000000000001
                //                      00000000-0000-0000-0000-00000000000010
                //这样的数据在第二个foreach替换的时候会有问题
                string tempString = "@";
                Dictionary<string, string> columnMapping = new Dictionary<string, string>();
                int index = 0;
                foreach (var p in dic)
                {
                    string newKey = tempString + index + tempString;
                    formula = formula.Replace(p.Key, newKey);
                    columnMapping.Add(newKey, p.Value);
                    index++;
                }

                foreach (var d in columnMapping)
                {
                    formula = formula.Replace(d.Key, d.Value);
                }
                //Shouldn't affect client mode.
                //merge code from CI-16090 if regional setting is not default, the formula should be ; 
                //if (mAveSPWeb.SPWeb.RegionalSettings.LocaleId != 1033)
                //{
                //    formula = formula.Replace(",", ";");
                //}
                return formula.Replace("[[", "[").Replace("]]", "]");
#if PerformanceLog
            }
#endif
        }

        #region Update Fields
        protected void UpdateField(IAveFieldCollection fields, IAveField spField, AveXmlField xmlField)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFieldCollection.UpdateField"))
            {
#endif
                try
                {
                    if (NeedSkip)//对于通过查找parentsite上的fields所匹配到的field，sharepoint界面上就是不能编辑的，不再更新
                    {
                        NeedSkip = false;
                        return;
                    }
                    if (AveFieldHelper.UpdateFieldType(spField, xmlField))
                    {
                        spField = fields.GetFieldByInternalName(spField.InternalName);
                    }
                    //restore NoCrawl for Searchable Columns
                    AveFieldHelper.UpdateNoCrawl(spField, xmlField);

                    if (spField.Sealed)
                    {
                        if (SetBaseField(spField, xmlField))
                        {
                            spField.Update();
                        }
                        return;
                    }
                    bool needUpdate = false;

                    if (spField.Type != xmlField.Type)
                    {
                        //TODO: [yye] Do we need handle name conflict? Should we override the conflict field?
                        spField.Type = xmlField.Type;
                        needUpdate = true;
                    }

                    if (SetBaseField(spField, xmlField))
                    {
                        needUpdate = true;
                    }

                    switch (spField.Type)
                    {
                        case AveFieldType.Lookup:
                            //case AveFieldType.Facilities:
                            IAveFieldLookup lookupField = spField as IAveFieldLookup;
                            if (lookupField != null)
                            {
                                if (SetLookupField(lookupField, xmlField))
                                {
                                    needUpdate = true;
                                }
                            }
                            break;
                        case AveFieldType.User:
                            //case AveFieldType.CallTo:
                            //case AveFieldType.SendTo:
                            IAveFieldUser userField = spField as IAveFieldUser;
                            if (userField != null)
                            {
                                if (SetUserField(userField, xmlField))
                                {
                                    needUpdate = true;
                                }
                            }
                            break;
                        case AveFieldType.DateTime:
                            //case AveFieldType.From:
                            //case AveFieldType.DueDate:
                            //case AveFieldType.CallTime:
                            //case AveFieldType.Until:
                            IAveFieldDateTime timeField = spField as IAveFieldDateTime;
                            if (timeField != null)
                            {
                                if (SetDateTimeField(timeField, xmlField))
                                {
                                    needUpdate = true;
                                }
                            }
                            break;
                        case AveFieldType.Boolean:
                        //case AveFieldType.WhatsNew:
                        //case AveFieldType.Confidential:
                        case AveFieldType.AllDayEvent:
                            //case AveFieldType.AllowEditing:
                            IAveFieldBoolean boolField = spField as IAveFieldBoolean;
                            if (boolField != null)
                            {
                                if (SetBoolField(boolField, xmlField))
                                {
                                    needUpdate = true;
                                }
                            }
                            break;
                        case AveFieldType.Choice:
                        //case AveFieldType.ContactInfo:
                        //case AveFieldType.Whereabout:
                        case AveFieldType.WorkflowStatus:
                        case AveFieldType.OutcomeChoice:
                            IAveFieldChoice choiceField = spField as IAveFieldChoice;
                            if (choiceField != null)
                            {
                                if (SetChoiceField(choiceField, xmlField))
                                {
                                    needUpdate = true;
                                }
                            }
                            break;
                        case AveFieldType.MultiChoice:
                            IAveFieldMultiChoice multiChocieField = spField as IAveFieldMultiChoice;
                            if (multiChocieField != null)
                            {
                                if (SetMultiChocieField(multiChocieField, xmlField))
                                {
                                    needUpdate = true;
                                }
                            }
                            break;
                        case AveFieldType.Calculated:
                            IAveFieldCalculated calField = spField as IAveFieldCalculated;
                            if (calField != null)
                            {
                                if (SetCalculatedField(calField, xmlField))
                                {
                                    needUpdate = true;
                                }
                            }
                            break;
                        case AveFieldType.Computed:
                            IAveFieldComputed computedField = spField as IAveFieldComputed;
                            if (computedField == null)
                            {
                                break;
                            }
                            computedField.EnableLookup = xmlField.EnableLookup;
                            break;
                        case AveFieldType.Currency:
                            IAveFieldCurrency currencyField = spField as IAveFieldCurrency;
                            if (currencyField != null)
                            {
                                int localeId = xmlField.CurrencyLocaleId;
                                if (localeId == -1)
                                {
                                    localeId = (int)mLanguageID;
                                }
                                if (currencyField.CurrencyLocaleId != localeId)
                                {
                                    currencyField.CurrencyLocaleId = localeId;
                                    needUpdate = true;
                                }
                                if (SetNumberField(currencyField, xmlField, false))
                                {
                                    needUpdate = true;
                                }
                            }
                            break;
                        case AveFieldType.Number:
                        case AveFieldType.Integer:
                        case AveFieldType.WorkflowEventType:
                            IAveFieldNumber numberField = spField as IAveFieldNumber;
                            if (numberField != null)
                            {
                                if (SetNumberField(numberField, xmlField, true))
                                {
                                    needUpdate = true;
                                }
                            }
                            break;
                        case AveFieldType.Note:
                            IAveFieldMultiLineText mulTextField = spField as IAveFieldMultiLineText;
                            if (mulTextField != null)
                            {
                                if (SetNoteField(mulTextField, xmlField))
                                {
                                    needUpdate = true;
                                }
                            }
                            break;
                        case AveFieldType.GridChoice:
                            IAveFieldRatingScale gridField = spField as IAveFieldRatingScale;
                            if (gridField != null)
                            {
                                if (SetGridField(gridField, xmlField))
                                {
                                    needUpdate = true;
                                }
                            }
                            break;
                        case AveFieldType.Text:
                            //case AveFieldType.Confirmations:
                            IAveFieldText textField = spField as IAveFieldText;
                            if (textField != null)
                            {
                                if (textField.MaxLength != xmlField.MaxLength)
                                {
                                    textField.MaxLength = xmlField.MaxLength;
                                    needUpdate = true;
                                }
                                if (textField.DifferencingLimit != xmlField.DifferencingLimit)
                                {
                                    textField.DifferencingLimit = xmlField.DifferencingLimit;
                                    needUpdate = true;
                                }
                            }
                            break;
                        case AveFieldType.URL:
                            IAveFieldUrl urlField = spField as IAveFieldUrl;
                            if (urlField != null)
                            {
                                if (urlField.DisplayFormat != xmlField.DisplayFormat_Url)
                                {
                                    urlField.DisplayFormat = xmlField.DisplayFormat_Url;
                                    needUpdate = true;
                                }
                            }
                            break;
                        case AveFieldType.Invalid:
                            if (spField.TypeAsString == "Facilities")
                            {
                                //can to do something
                                break;
                            }
                            if (spField.TypeAsString == "TaxonomyFieldType" || spField.TypeAsString == "TaxonomyFieldTypeMulti")
                            {
                                if (WrapperRuntime.CurrentContext.IsMoss)
                                {
                                    if (AveTaxonomyField.UpdateTaxonomyFieldCommonProperties(mAveSPWeb.ParentSite, spField, xmlField))
                                    {
                                        needUpdate = true;
                                    }
                                }
                                break;
                            }
                            if (SetInvalidField(spField, xmlField))
                            {
                                needUpdate = true;
                            }
                            break;
                        default:
                            break;
                    }

                    if (needUpdate)
                    {
                        if (spField.ReadOnlyField)
                        {
                            spField.UpdateReadOnlyField();
                        }
                        else
                        {
                            spField.Update();
                        }
                    }
                    if (xmlField.ShowInNewForm != null && spField.ShowInNewForm != xmlField.ShowInNewForm)
                    {
                        spField.SetShowInNewForm(xmlField.ShowInNewForm.Value);
                    }
                    if (xmlField.ShowInEditForm != null && spField.ShowInEditForm != xmlField.ShowInEditForm)
                    {
                        spField.SetShowInEditForm(xmlField.ShowInEditForm.Value);
                    }
                    if (xmlField.ShowInDisplayForm != null && spField.ShowInDisplayForm != xmlField.ShowInDisplayForm)
                    {
                        spField.SetShowInDisplayForm(xmlField.ShowInDisplayForm.Value);
                    }

                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN,
                        string.Format("An error occurred while update field. filed id:{0}, field title:{1}, field schema:{2}, source schema:{3} \n error message:{4}",
                        spField.ID, spField.Title, spField.SchemaXml, xmlField.XmlElement.OuterXml, e));
                    throw;
                }

                AddDataSourceField(xmlField);
#if PerformanceLog
            }
#endif
        }

        private bool SetBaseField(IAveField field, AveXmlField xmlField)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFieldCollection.SetBaseField"))
            {
#endif
            string fieldAttrValue = null;
            bool needUpdate = false;
            //if (field.AllowDuplicateValues != xmlField.AllowDuplicateValues)
            //{
            //    field.AllowDuplicateValues = xmlField.AllowDuplicateValues;
            //    needUpdate = true;
            //}
            if (field.AggregationFunction != xmlField.AggregationFunction)
            {
                field.AggregationFunction = xmlField.AggregationFunction;
                needUpdate = true;
            }
            fieldAttrValue = field.GetAttributeFromSchemaXml("AllowDeletion");
            bool? allowDeletion = fieldAttrValue == null ? (bool?)null : Convert.ToBoolean(fieldAttrValue);
            if (allowDeletion != xmlField.AllowDeletion)
            {
                field.AllowDeletion = xmlField.AllowDeletion;
                needUpdate = true;
            }
            if (field.DefaultFormula != xmlField.DefaultFormula)
            {
                field.DefaultFormula = xmlField.DefaultFormula;
                needUpdate = true;
            }
            string defaultValue = xmlField.DefaultValue;
            if (!string.IsNullOrEmpty(defaultValue) && defaultValue.StartsWith("$Resources", StringComparison.OrdinalIgnoreCase))
            {
                defaultValue = mAveParentSite.ObjectModelFactory.Utility.GetLocalizedString(defaultValue, "core", (uint)CultureInfo.CurrentUICulture.LCID);
            }
            if (defaultValue != field.DefaultValue)
            {
                if (!(string.IsNullOrEmpty(defaultValue) && string.IsNullOrEmpty(field.DefaultValue)))
                {
                    field.DefaultValue = defaultValue;
                    needUpdate = true;
                }
            }
            if (field.Description != xmlField.Description)
            {
                field.Description = xmlField.Description;
                needUpdate = true;
            }
            if (field.DescriptionResource.SetUserResource(mAveSPWeb.SPWeb, xmlField.DescriptionResource, true))
            {
                field.DescriptionResource.Update();
                needUpdate = true;
            }
            if (field.Direction != xmlField.Direction)
            {
                field.Direction = xmlField.Direction;
                needUpdate = true;
            }
            //private string mDirection = "none";
            if (field.DisplaySize != xmlField.DisplaySize)
            {
                field.DisplaySize = xmlField.DisplaySize;
                needUpdate = true;
            }
            ////private string mDisplaySize = null;
            //if (field.EcbMenu != xmlField.EcbMenu)
            //{
            //    field.EcbMenu = xmlField.EcbMenu;
            //    needUpdate = true;
            //}
            ////private bool mEcbMenu = false;
            //if (field.EcbMenuAllowed != xmlField.EcbMenuAllowed)
            //{
            //    field.EcbMenuAllowed = xmlField.EcbMenuAllowed;
            //    needUpdate = true;
            //}
            //private bool? mEcbMenuAllowed = null;
            if (field.TypeAsString != xmlField.TypeAsString)
            {
                field.TypeAsString = xmlField.TypeAsString;
                needUpdate = true;
            }
            //private string mTypeAsString = null;
            if (field.ValidationFormula != xmlField.ValidationFormula)
            {
                field.ValidationFormula = xmlField.ValidationFormula;
                needUpdate = true;
            }
            //private string mValidationFormula = null;
            if (field.ValidationMessage != xmlField.ValidationMessage)
            {
                field.ValidationMessage = xmlField.ValidationMessage;
                needUpdate = true;
            }
            //private string mValidationMessage = null;
            if (field.AggregationFunction != xmlField.AggregationFunction)
            {
                field.AggregationFunction = xmlField.AggregationFunction;
                needUpdate = true;
            }
            //private string mXPath = null;
            if (field.Indexed != xmlField.Indexed)
            {
                field.Indexed = xmlField.Indexed;
                needUpdate = true;
            }
            //private bool mIndexed = false;
            string ime = xmlField.IMEMode;
            uint lCID = (uint)mLocaleID;
            if ((((lCID != 0x404) && (lCID != 0x804)) && ((lCID != 0xc04) && (lCID != 0x1004))) && ((lCID != 0x411) && (lCID != 0x412)))
            {
                ime = null;
            }
            if ((!(field is IAveFieldNumber)) && field.IMEMode != null && field.IMEMode != ime && !(field.IMEMode.Equals("inactive") && ime == null))    //inactive is the default value, so does null
            {
                field.IMEMode = ime;
                needUpdate = true;
            }
            //private string mIMEMode = null;
            fieldAttrValue = field.GetAttributeFromSchemaXml("Hidden");
            bool hidden = fieldAttrValue == null ? false : Convert.ToBoolean(fieldAttrValue);
            //CanToggleHidden 为false，hidden需要更改schemaXml才能更新，保险起见CanToggleHidden为false跳过hidden属性的还原。
            var tempValue = field.GetAttributeFromSchemaXml("CanToggleHidden");
            bool canToggleHidden = fieldAttrValue != null && tempValue == null ? false : Convert.ToBoolean(tempValue);
            //3881510a-4e4a-4ee8-b102-8ee8e2d0dd4b is checked out user field, a built in column which hidden is true and uneditable.
            if (hidden != xmlField.Hidden && field.ID != new Guid("3881510a-4e4a-4ee8-b102-8ee8e2d0dd4b") && canToggleHidden)
            {
                field.Hidden = xmlField.Hidden;
                needUpdate = true;
            }
            //private bool mHidden = false;     
            string group = xmlField.Group;
            if (!string.IsNullOrEmpty(group))
            {
                group = mAveParentSite.GetNameByLanguageMapping(group, AveLanguageMappingType.FieldMapping);
            }
            if (field.Group != group)
            {
                field.Group = group;
                needUpdate = true;
            }
            //private string mGroup = null;
            if (field.JumpToField != xmlField.JumpToField)
            {
                field.JumpToField = xmlField.JumpToField;
                needUpdate = true;
            }
            //private string mJumpToField = null;
            if (field.LinkToItem != xmlField.LinkToItem)
            {
                field.LinkToItem = xmlField.LinkToItem;
                needUpdate = true;
            }
            //private bool mLinkToItem = false;
            //Need Change
            //if (field.LinkToItemAllowed != xmlField.LinkToItemAllowed)
            //{
            //    field.LinkToItemAllowed = xmlField.LinkToItemAllowed;
            //    needUpdate = true;
            //}
            //private bool? mLinkToItemAllowed = null;
            if (field.NoCrawl != xmlField.NoCrawl)
            {
                field.NoCrawl = xmlField.NoCrawl;
                needUpdate = true;
            }
            //private bool mNoCrawl = false;
            if (field.PIAttribute != xmlField.PIAttribute)
            {
                field.PIAttribute = xmlField.PIAttribute;
                needUpdate = true;
            }
            //private string mPIAttribute = null;
            if (field.PITarget != xmlField.PITarget)
            {
                field.PITarget = xmlField.PITarget;
                needUpdate = true;
            }
            //private string mPITarget = null;
            if (field.PrimaryPIAttribute != xmlField.PrimaryPIAttribute)
            {
                field.PrimaryPIAttribute = xmlField.PrimaryPIAttribute;
                needUpdate = true;
            }
            //private string mPrimaryPIAttribute = null;
            if (field.PrimaryPITarget != xmlField.PrimaryPITarget)
            {
                field.PrimaryPITarget = xmlField.PrimaryPITarget;
                needUpdate = true;
            }
            //private string mPrimaryPITarget = null;
            if (field.ReadOnlyField != xmlField.ReadOnlyField)
            {
                field.ReadOnlyField = xmlField.ReadOnlyField;
                needUpdate = true;
            }
            //private bool mReadOnlyField = false;
            if (field.RelatedField != xmlField.RelatedField)
            {
                field.RelatedField = xmlField.RelatedField;
                needUpdate = true;
            }
            //private string mRelatedField = null;
            if (field.Required != xmlField.Required)
            {
                field.Required = xmlField.Required;
                needUpdate = true;
            }
            if (field.Required || xmlField.Required)
            {
                if (field.Required)
                {
                    field.Required = false;
                    needUpdate = true;
                    log.Info($"ArchiverRestore reset Required.field.Required:{field.Required}.xmlField.Required:{xmlField.Required}.");
                }
                log.Info($"ArchiverRestore skip Required.field.Required:{field.Required}.xmlField.Required:{xmlField.Required}.");
            }
            //private bool mRequired = false;
            if (field.Sealed != xmlField.Sealed)
            {
                if (!AveBuiltInFieldId.Contains(field.ID))
                {
                    field.Sealed = xmlField.Sealed;
                    needUpdate = true;
                }
                else
                {
                    log.Debug("Can not set sealed attribute of built-in field.");
                }
            }
            //private bool mSealed = false;
            if (field.ShowInDisplayForm != xmlField.ShowInDisplayForm)
            {
                field.ShowInDisplayForm = xmlField.ShowInDisplayForm;
                needUpdate = true;
            }
            //private bool? mShowInDisplayForm = null;
            if (field.ShowInEditForm != xmlField.ShowInEditForm)
            {
                field.ShowInEditForm = xmlField.ShowInEditForm;
                needUpdate = true;
            }
            //private bool? mShowInEditForm = null;
            if (field.ShowInListSettings != xmlField.ShowInListSettings)
            {
                field.ShowInListSettings = xmlField.ShowInListSettings;
                needUpdate = true;
            }
            //private bool? mShowInListSettings = null;
            if (field.ShowInNewForm != xmlField.ShowInNewForm)
            {
                field.ShowInNewForm = xmlField.ShowInNewForm;
                needUpdate = true;
            }
            //private bool? mShowInNewForm = null;

            if (field.GetAttributeFromSchemaXml("ShowInVersionHistory") != null)
            {
                bool showInVersionHistory = GetShowInVersionHistory(field, xmlField);
                if (field.ShowInVersionHistory != showInVersionHistory)
                {
                    field.ShowInVersionHistory = showInVersionHistory;
                    needUpdate = true;
                }
            }

            fieldAttrValue = field.GetAttributeFromSchemaXml("ShowInViewForms");
            bool? showInViewForms = fieldAttrValue == null ? (bool?)null : Convert.ToBoolean(fieldAttrValue);
            //private bool mShowInVersionHistory = false;
            if (showInViewForms != xmlField.ShowInViewForms)
            {
                field.ShowInViewForms = xmlField.ShowInViewForms;
                needUpdate = true;
            }
            //private bool? mShowInViewForms = null;
            if (field.StaticName != xmlField.StaticName)
            {
                field.StaticName = xmlField.StaticName;
                needUpdate = true;
            }
            //private string mStaticName = null;
            //如果有custommapping，并且用displayname mapping 同type也不能覆盖title。
            //现在使用custommapping就不走别的find逻辑，如果以后有修改的话，这个判断就不足了，必须确定是custommappingfind并且用display mapping上的才不覆盖title
            if (field.Title != xmlField.Title && (xmlField.CustomFieldInfo == null || xmlField.CustomFieldInfo.UseInternalOrDisplay))
            {
                field.Title = xmlField.Title;
                needUpdate = true;
            }
            if (field.TitleResource.SetUserResource(mAveSPWeb.SPWeb, xmlField.TitleResource, true))
            {
                field.TitleResource.Update();
                needUpdate = true;
            }
            //private string mTitle = null;
            if (field.TranslationXml != xmlField.TranslationXml &&
                !(string.IsNullOrEmpty(field.TranslationXml) && string.IsNullOrEmpty(xmlField.TranslationXml)))
            {
                field.TranslationXml = xmlField.TranslationXml;
                needUpdate = true;
            }
            //private string mTranslationXml = null;
            if (field.EnforceUniqueValues != xmlField.EnforceUniqueValues)
            {
                field.EnforceUniqueValues = xmlField.EnforceUniqueValues;
                needUpdate = true;
            }
            return needUpdate;
#if PerformanceLog
            }
#endif
        }

        private bool GetShowInVersionHistory(IAveField field, AveXmlField xmlField)
        {
            if (xmlField.ShowInVersionHistory.HasValue)
            {
                return xmlField.ShowInVersionHistory.Value;
            }
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
            return (!field.Hidden && !field.ReadOnlyField);
        }

        private bool SetLookupField(IAveFieldLookup lookupField, AveXmlField xmlField)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFieldCollection.SetLookupField"))
            {
#endif
                //log.Info("Start to set lookup field.");
                bool needUpdate = false;
                if (lookupField.IsRelationship != xmlField.IsRelationship)
                {
                    lookupField.IsRelationship = xmlField.IsRelationship;
                    needUpdate = true;
                }
                if (xmlField.LookupField != null && lookupField.LookupField != null && !lookupField.LookupField.Equals(xmlField.LookupField, StringComparison.OrdinalIgnoreCase))//the default value of lookupfield is null, so skip the null field
                {
                    lookupField.LookupField = xmlField.LookupField;
                    needUpdate = true;
                }

                bool hasUpdate = false;
                bool needPost = true;
                Guid webId = Guid.Empty;
                if (xmlField.Type == AveFieldType.Lookup && xmlField.AveLookupWebTitle != null && mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.WebUrlMapping.ContainsKey(xmlField.AveLookupWebTitle))
                {
                    log.Info("Start to update lookup field. Current web ID: {0}, SourceFieldXML: {1}, DesFieldXML: {2}, AveLookupWebTitle: {3}, AveLookupListTitle: {4}, AveSourceType: {5}, AveLookupListID: {6}.", mWebId, xmlField.XmlElement.OuterXml, lookupField.SchemaXml, xmlField.AveLookupWebTitle, xmlField.AveLookupListTitle, xmlField.AveSourceType, xmlField.AveLookupListID);
                    if (String.IsNullOrEmpty(xmlField.LookupWebId) || xmlField.LookupWebId.Equals(Guid.Empty.ToString()))
                    {
                        webId = mWebId;
                    }
                    else
                    {
                        if (xmlField.AveSourceType == "1" && mAveSPList != null)
                        {
                            //IAveField parentField = AveFieldHelper.FindFieldInCollection(xmlField.ID, mAveSPList.ParentWeb.SPWeb.Fields);
                            IAveField parentField = FindFieldFromParentWeb(xmlField, mAveSPList.ParentWeb.SPWeb.AvailableFields);
                            if (parentField != null && parentField is IAveFieldLookup)
                            {
                                webId = ((IAveFieldLookup)parentField).LookupWebId;
                                log.Info("Get field from parent web. Web ID: {0}", webId);
                            }
                            else
                            {
                                webId = mAveSPWeb.ParentSite.GetWeb(mAveSPWeb.QueryService, mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.WebUrlMapping[xmlField.AveLookupWebTitle]);
                            }
                        }
                        else
                        {
                            webId = mAveSPWeb.ParentSite.GetWeb(mAveSPWeb.QueryService, mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.WebUrlMapping[xmlField.AveLookupWebTitle]);
                        }
                    }
                    log.Info("Web ID: {0}", webId);

                    if (String.IsNullOrEmpty(xmlField.AveLookupListTitle))
                    {
                        if (!lookupField.LookupList.Equals("Self", StringComparison.CurrentCultureIgnoreCase) && lookupField.LookupList.Equals(xmlField.AveLookupListID, StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (lookupField.LookupList != "")
                            {
                                lookupField.LookupList = "";
                                lookupField.SetFieldAttributeValue("List", "");
                                //field.lookupListSet = true;
                                //field.lookupList = value;
                                lookupField.SetFieldValue("lookupList", "");
                                lookupField.SetFieldValue("lookupListSet", true);
                                //AveAssemblyUtility.SetFieldValue(lookupField, "lookupList", "");
                                //AveAssemblyUtility.SetFieldValue(lookupField, "lookupListSet", true);

                                if (lookupField.Version == 0)
                                {
                                    lookupField.RemoveFieldAttributeValue("Version");
                                    // AveAssemblyUtility.InvokeMethod(lookupField, typeof(SPField), "RemoveFieldAttributeValue", new object[] { "Version" });
                                }
                                needUpdate = true;
                            }
                        }
                    }
                    else
                    {
                        Guid listId = Guid.Empty;
                        if (this.mAveSPWeb != null)
                        {
                            listId = this.mAveSPWeb.ParentSite.GetList(webId, xmlField.AveLookupListTitle);
                        }
                        else if (this.mAveSPList != null)
                        {
                            listId = this.mAveSPList.ParentWeb.ParentSite.GetList(webId, xmlField.AveLookupListTitle);
                        }
                        if (xmlField.AveSourceType == "1" && mAveSPList != null)
                        {
                            IAveField parentField = AveFieldHelper.FindFieldInCollection(xmlField.ID, mAveSPList.ParentWeb.SPWeb.Fields);
                            if (parentField == null)
                            {
                                log.Info("Can not find field in WebFields, try find in AvailableFields.");
                                parentField = FindFieldFromParentWeb(xmlField, mAveSPList.ParentWeb.SPWeb.AvailableFields);
                            }
                            if (parentField != null && parentField is IAveFieldLookup)
                            {
                                IAveFieldLookup parentLookupField = parentField as IAveFieldLookup;
                                if (AveTypeHelper.IsGuid(parentLookupField.LookupList))
                                {
                                    Guid lookupListId = new Guid(parentLookupField.LookupList);
                                    IAveWeb web = (parentLookupField.LookupWebId == Guid.Empty || parentLookupField.LookupWebId == this.mAveSPWeb.SPWeb.ID) ? this.mAveSPWeb.SPWeb : this.mAveSPWeb.ParentSite.SPSite.OpenWeb(parentLookupField.LookupWebId);
                                    if (web.Lists.GetById(lookupListId) != null)
                                    {
                                        listId = lookupListId;
                                        log.Info("Get list from parent web. listId: {0}", listId);
                                    }
                                }
                                else
                                {
                                    string fieldInfo = parentField.SchemaXml ?? parentField.InternalName;
                                    log.Warn("LookupList id is invalid guid in this field: {0}, LookupList id: {1}", fieldInfo, parentLookupField.LookupList);
                                }
                            }
                        }
                        log.Info("List ID: {0}", listId);
                        if (!listId.Equals(Guid.Empty))
                        {
                            if (string.IsNullOrEmpty(lookupField.LookupList) || lookupField.LookupList.ToUpper() != listId.ToString("B").ToUpper())
                            {
                                lookupField.LookupList = listId.ToString("B");
                                lookupField.LookupField = xmlField.LookupField;
                                needPost = false;
                                if (lookupField.Version == 0)
                                {
                                    lookupField.RemoveFieldAttributeValue("Version");
                                    //AveAssemblyUtility.InvokeMethod(lookupField, typeof(SPField), "RemoveFieldAttributeValue", new object[] { "Version" });
                                }
                                needUpdate = true;
                            }
                            else
                            {
                                hasUpdate = true;
                            }
                            if (lookupField.LookupWebId != webId)
                            {
                                lookupField.LookupWebId = webId;
                                if (lookupField.Version == 0)
                                {
                                    lookupField.RemoveFieldAttributeValue("Version");
                                    // AveAssemblyUtility.InvokeMethod(lookupField, typeof(SPField), "RemoveFieldAttributeValue", new object[] { "Version" });
                                }
                                needUpdate = true;
                            }
                        }
                        else
                        {
                            needPost = true;
                        }
                    }
                }

                if (needPost)
                {
                    AddLookupField(xmlField, lookupField.ID, !hasUpdate);
                }

                if (!string.IsNullOrEmpty(xmlField.LookupList) && mAveSPList != null)
                {
                    // mapping contains xmlField.loopuplist
                    // if need update at last    AveLookupObject obj = new AveLookupObject();
                    if (!AveFieldHelper.IsGuid(xmlField.LookupList))
                    {
                        if (string.IsNullOrEmpty(lookupField.LookupList) || lookupField.LookupList != xmlField.LookupList)
                        {
                            lookupField.LookupList = xmlField.LookupList;
                            // AveAssemblyUtility.InvokeMethod(lookupField, typeof(SPField), "SetFieldAttributeValue", new object[] { "List", xmlField.LookupList });
                            if (lookupField.Version == 0)
                            {
                                lookupField.RemoveFieldAttributeValue("Version");
                                // AveAssemblyUtility.InvokeMethod(lookupField, typeof(SPField), "RemoveFieldAttributeValue", new object[] { "Version" });
                            }
                            needUpdate = true;
                            hasUpdate = true;
                        }
                    }
                    else if (mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.ListIdMapping.ContainsKey(new Guid(xmlField.LookupList)))
                    {
                        string destListId = mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.ListIdMapping[new Guid(xmlField.LookupList)].ToString();
                        lookupField.LookupList = destListId;
                        //AveAssemblyUtility.InvokeMethod(lookupField, typeof(SPField), "SetFieldAttributeValue", new object[] { "List", destListId });
                        if (lookupField.Version == 0)
                        {
                            lookupField.RemoveFieldAttributeValue("Version");
                            // AveAssemblyUtility.InvokeMethod(lookupField, typeof(SPField), "RemoveFieldAttributeValue", new object[] { "Version" });
                        }
                        needUpdate = true;
                        hasUpdate = true;
                    }
                    else
                    {
                        bool needAdd = true;
                        if (xmlField.AveSourceType == "1" && mAveSPList != null)
                        {
                            //IAveField parentField = AveFieldHelper.FindFieldInCollection(xmlField.ID, mAveSPList.ParentWeb.SPWeb.Fields);
                            IAveField parentField = FindFieldFromParentWeb(xmlField, mAveSPList.ParentWeb.SPWeb.AvailableFields);
                            if (parentField != null && parentField is IAveFieldLookup)
                            {
                                needAdd = false;
                            }
                        }
                        if (needAdd)
                        {
                            AveLookupObject obj = new AveLookupObject();
                            obj.Id = xmlField.ID;
                            obj.Type = xmlField.AveSourceType;
                            obj.WebUrl = xmlField.AveLookupWebTitle;
                            obj.WebId = mWebId;
                            obj.List = xmlField.LookupList;
                            if (mAveSPList != null)
                            {
                                obj.ListId = mAveSPList.SPList.ID;
                            }//TODOLMM
                            if (!mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.NotUpdateLookupFieldCache.ContainsKey(new Guid(obj.List)))
                            {
                                mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.NotUpdateLookupFieldCache[new Guid(obj.List)] = new List<AveLookupObject>();
                            }
                            mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.NotUpdateLookupFieldCache[new Guid(obj.List)].Add(obj);
                        }
                    }
                }

                if (lookupField.AllowMultipleValues && lookupField.PrependId != xmlField.PrependId)
                {
                    lookupField.PrependId = xmlField.PrependId;
                    needUpdate = true;
                }
                if (lookupField.PrimaryFieldId != xmlField.PrimaryFieldId)
                {
                    lookupField.PrimaryFieldId = xmlField.PrimaryFieldId;
                    needUpdate = true;
                }
                if (lookupField.RelationshipDeleteBehavior != xmlField.RelationshipDeleteBehavior)
                {
                    lookupField.RelationshipDeleteBehavior = xmlField.RelationshipDeleteBehavior;
                    needUpdate = true;
                }
                if (lookupField.UnlimitedLengthInDocumentLibrary != xmlField.UnlimitedLengthInDocumentLibrary)
                {
                    lookupField.UnlimitedLengthInDocumentLibrary = xmlField.UnlimitedLengthInDocumentLibrary;
                    needUpdate = true;
                }
                if (lookupField.AllowMultipleValues != xmlField.AllowMultipleValues)
                {
                    lookupField.AllowMultipleValues = xmlField.AllowMultipleValues;
                    needUpdate = true;
                }
                if (lookupField.CountRelated != xmlField.CountRelated)
                {
                    lookupField.CountRelated = xmlField.CountRelated;
                    needUpdate = true;
                }
                return needUpdate;
#if PerformanceLog
            }
#endif
        }

        private bool SetMultiChocieField(IAveFieldMultiChoice field, AveXmlField xmlField)
        {
            bool needUpdate = AddChocies(field, xmlField.Choices);
            if (field.FillInChoice != xmlField.FillInChoice)
            {
                field.FillInChoice = xmlField.FillInChoice;
                needUpdate = true;
            }
            return needUpdate;
        }

        private bool AddChocies(IAveFieldMultiChoice field, StringCollection choices)
        {
            bool needUpdate = false;
            field.Choices.Clear();
            foreach (string ch in choices)
            {
                //string choice = SPUtility.GetLocalizedString(ch.Trim(), "core", mLanguageID);
                if (!field.Choices.Contains(ch))
                {
                    field.AddChoice(ch);
                    needUpdate = true;
                }
            }
            return needUpdate;
        }

        private bool SetUserField(IAveFieldUser userField, AveXmlField xmlField)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFieldCollection.SetUserField"))
            {
#endif
                bool needUpdate = false;
                if (userField.AllowDisplay != xmlField.AllowDisplay)
                {
                    userField.AllowDisplay = xmlField.AllowDisplay;
                    needUpdate = true;
                }
                if (userField.Presence != xmlField.Presence)
                {
                    userField.Presence = xmlField.Presence;
                    needUpdate = true;
                }
                IAvePrincipal desPrincipal = mAveSPWeb.ParentSite.SPMembers.FindMember(xmlField.SelectionGroup, true);
                if (desPrincipal != null && userField.SelectionGroup != desPrincipal.ID)
                {
                    userField.SelectionGroup = desPrincipal.ID;
                    needUpdate = true;
                }
                //当源端为All Users,目的端为SharePoint Group时目的端SelectionGroup设置为0.
                else if (desPrincipal == null && mAveParentSite.SPSite.RootWeb.SiteGroups.GetByID(userField.SelectionGroup) != null)
                {
                    userField.SelectionGroup = 0;
                    needUpdate = true;
                }
                if (userField.SelectionMode != xmlField.SelectionMode)
                {
                    userField.SelectionMode = xmlField.SelectionMode;
                    needUpdate = true;
                }

                if (SetLookupField(userField, xmlField))
                {
                    needUpdate = true;
                }
                return needUpdate;
#if PerformanceLog
            }
#endif
        }

        private bool SetDateTimeField(IAveFieldDateTime timeField, AveXmlField xmlField)
        {
            bool needUpdate = false;
            if (timeField.CalendarType != xmlField.CalendarType)
            {
                timeField.CalendarType = xmlField.CalendarType;
                needUpdate = true;
            }
            try
            {
                if (timeField.DisplayFormat != xmlField.DisplayFormat)
                {
                    timeField.DisplayFormat = xmlField.DisplayFormat;
                    needUpdate = true;
                }
                if (timeField.FriendlyDisplayFormat != xmlField.FriendlyDisplayFormat)
                {
                    timeField.FriendlyDisplayFormat = xmlField.FriendlyDisplayFormat;
                    needUpdate = true;
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.SetDisplayTimeError, e.ToString());
            }//不需要捕获异常
            return needUpdate;
        }

        private bool SetCalculatedField(IAveFieldCalculated calField, AveXmlField xmlField)
        {
            bool needUpdate = false;
            int localeId = xmlField.CurrencyLocaleId;
            if (localeId == -1)
            {
                localeId = (int)mLanguageID;
            }
            //if (calField.CurrencyLocaleId != localeId)
            //{
            //    calField.CurrencyLocaleId = localeId;
            //    needUpdate = true;
            //}
            if (calField.DateFormat != xmlField.DateFormat)
            {
                calField.DateFormat = xmlField.DateFormat;
                needUpdate = true;
            }
            if (calField.DisplayFormat != xmlField.DisplayFormat_Calculated)
            {
                calField.DisplayFormat = xmlField.DisplayFormat_Calculated;
                needUpdate = true;
            }
            if (calField.Formula != xmlField.Formula)
            {
                calField.Formula = xmlField.Formula;
                needUpdate = true;
            }
            if (calField.OutputType != xmlField.OutputType)
            {
                calField.OutputType = xmlField.OutputType;
                needUpdate = true;
            }
            if (calField.ShowAsPercentage != xmlField.ShowAsPercentage)
            {
                calField.ShowAsPercentage = xmlField.ShowAsPercentage;
                needUpdate = true;
            }
            return needUpdate;
        }

        private bool SetNumberField(IAveFieldNumber numberField, AveXmlField xmlField, bool checkPercentage)
        {
            if (numberField.MinimumValue > xmlField.MaximumValue || numberField.MaximumValue < xmlField.MinimumValue)
            {
                numberField.MinimumValue = double.MinValue;
                numberField.MaximumValue = double.MaxValue;
            }
            bool needUpdate = false;
            if (numberField.DisplayFormat != xmlField.DisplayFormat_Number)
            {
                numberField.DisplayFormat = xmlField.DisplayFormat_Number;
                needUpdate = true;
            }
            if (Math.Abs(numberField.MaximumValue - xmlField.MaximumValue) <= DoubleAllowDifference)
            {
                numberField.MaximumValue = xmlField.MaximumValue;
                needUpdate = true;
            }
            if (Math.Abs(numberField.MinimumValue - xmlField.MinimumValue) <= DoubleAllowDifference)
            {
                numberField.MinimumValue = xmlField.MinimumValue;
                needUpdate = true;
            }
            if (numberField.DefaultValue != xmlField.DefaultValue)
            {
                numberField.DefaultValue = xmlField.DefaultValue;
                needUpdate = true;
            }
            if (checkPercentage && numberField.ShowAsPercentage != xmlField.ShowAsPercentage)
            {
                numberField.ShowAsPercentage = xmlField.ShowAsPercentage;
                needUpdate = true;
            }
            return needUpdate;
        }

        private bool SetBoolField(IAveFieldBoolean boolField, AveXmlField xmlField)
        {
            bool needUpdate = false;
            if (boolField.JumpToNoField != xmlField.JumpToNoField)
            {
                boolField.JumpToNoField = xmlField.JumpToNoField;
                needUpdate = true;
            }
            if (boolField.JumpToYesField != xmlField.JumpToYesField)
            {
                boolField.JumpToYesField = xmlField.JumpToYesField;
                needUpdate = true;
            }
            return needUpdate;
        }

        private bool SetInvalidField(IAveField field, AveXmlField xmlField)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFieldCollection.SetInvalidField"))
            {
#endif
                bool needUpdate = false;
                if (mBaseFields == null)
                {
                    InitBaseFields();
                }
                XmlElement fieldElement = field.Node as XmlElement;
                if (fieldElement == null)
                {
                    return needUpdate;
                }
                if (SetElementField(fieldElement, xmlField.XmlElement, true))
                {
                    needUpdate = true;
                }

                ArrayList nodeNameList = new ArrayList();
                foreach (XmlElement child in xmlField.XmlElement.ChildNodes)
                {
                    if (mBaseNodeFields.Contains(child.Name))
                    {
                        continue;
                    }
                    if (SetInvalidNode(fieldElement, child))
                    {
                        needUpdate = true;
                    }
                    nodeNameList.Add(child.Name);
                }
                ArrayList removeList = new ArrayList();
                foreach (XmlElement child in fieldElement.ChildNodes)
                {
                    if (!nodeNameList.Contains(child.Name))
                    {
                        removeList.Add(child);
                    }
                }
                if (removeList.Count > 0)
                {
                    foreach (XmlElement child in removeList)
                    {
                        fieldElement.RemoveChild(child);
                    }
                    needUpdate = true;
                }
                return needUpdate;
#if PerformanceLog
            }
#endif
        }

        private bool SetElementField(XmlElement fieldElement, XmlElement sourceElement, bool checkBase)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFieldCollection.SetElementField"))
            {
#endif
                bool needUpdate = false;
                List<string> attributeList = new List<string>();
                foreach (XmlAttribute attri in sourceElement.Attributes)
                {
                    attributeList.Add(attri.Name);
                }
                foreach (XmlAttribute attri in fieldElement.Attributes)
                {
                    if (!attributeList.Contains(attri.Name))
                    {
                        attributeList.Add(attri.Name);
                    }
                }

                foreach (string attriName in attributeList)
                {
                    if (checkBase && mBaseFields.Contains(attriName))
                    {
                        continue;
                    }
                    if (SetInvalidField(fieldElement, attriName, sourceElement))
                    {
                        needUpdate = true;
                    }
                }
                return needUpdate;
#if PerformanceLog
            }
#endif
        }

        private bool SetInvalidNode(XmlElement fieldElement, XmlElement sourceElement)
        {
            bool needUpdate = false;
            XmlElement element = GetNode(fieldElement, sourceElement.Name);
            if (!element.InnerXml.Equals(sourceElement.InnerXml))
            {
                element.InnerXml = sourceElement.InnerXml;
                needUpdate = true;
            }
            if (SetElementField(element, sourceElement, false))
            {
                needUpdate = true;
            }
            return needUpdate;
        }

        private bool SetInvalidField(XmlElement fieldElement, string name, XmlElement element)
        {
            if (fieldElement.HasAttribute(name) && !element.HasAttribute(name))
            {
                fieldElement.RemoveAttribute(name);
                return true;
            }
            else if (fieldElement.GetAttribute(name) != element.GetAttribute(name))
            {
                fieldElement.SetAttribute(name, element.GetAttribute(name));
                return true;
            }
            return false;
        }



        private XmlElement GetNode(XmlElement fieldElment, string name)
        {
            foreach (XmlElement element in fieldElment.ChildNodes)
            {
                if (element.Name == name)
                {
                    return element;
                }
            }
            return null;
        }

        private List<string> mBaseFields = null;

        private List<string> mBaseNodeFields = null;

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint Property")]
        private void InitBaseFields()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFieldCollection.InitBaseFields"))
            {
#endif
                mBaseFields = new List<string>();
                mBaseNodeFields = new List<string>();
                string[] properties = new string[]
            { "ID", "Aggregation", "AllowDeletion", //"AllowDuplicateValues",
            "Description", "Direction", "DisplaySize", "EcbMenuAllowed", "EcbMenu" ,
            "Group", "Hidden", "IMEMode", "Indexed", "JumpTo" ,
            "LinkToItemAllowed", "ReadOnly", "RelatedField", "Required", "Sealed" ,
            "PrimaryPITarget", "NoCrawl", "PIAttribute", "PITarget", "PrimaryPIAttribute",
            "ShowInDisplayForm", "ShowInEditForm", "ShowInListSettings", "ShowInNewForm", "ShowInViewForms",
            "StaticName", "DisplayName", "Translations", "Type", "ShowInVersionHistory",
            "Version","SourceID","AddFieldOption"};
                foreach (string field in properties)
                {
                    if (!mBaseFields.Contains(field))
                    {
                        mBaseFields.Add(field);
                    }
                }
                string[] nodes = new string[] { "DefaultFormula", "FieldRefs", "Validation" };
                foreach (string field in nodes)
                {
                    if (!mBaseNodeFields.Contains(field))
                    {
                        mBaseNodeFields.Add(field);
                    }
                }

#if PerformanceLog
            }
#endif
        }

        private bool SetChoiceField(IAveFieldChoice choiceField, AveXmlField xmlField)
        {
            bool needUpdate = false;
            //if (choiceField.FillinChoiceJumpTo != xmlField.FillinChoiceJumpTo)
            //{
            //    choiceField.FillinChoiceJumpTo = xmlField.FillinChoiceJumpTo;
            //    needUpdate = true;
            //}
            if (choiceField.EditFormat != xmlField.EditFormat)
            {
                choiceField.EditFormat = xmlField.EditFormat;
                needUpdate = true;
            }

            if (SetMultiChocieField(choiceField, xmlField))
            {
                needUpdate = true;
            }
            return needUpdate;
        }

        private bool SetNoteField(IAveFieldMultiLineText mulTextField, AveXmlField xmlField)
        {
            bool needUpdate = false;
            if (mulTextField.AllowHyperlink != xmlField.AllowHyperlink)
            {
                mulTextField.AllowHyperlink = xmlField.AllowHyperlink;
                needUpdate = true;
            }
            if (mulTextField.AppendOnly != xmlField.AppendOnly)
            {
                mulTextField.AppendOnly = xmlField.AppendOnly;
                needUpdate = true;
            }
            //if (mulTextField.DifferencingLimit != xmlField.DifferencingLimit)
            //{
            //    mulTextField.DifferencingLimit = xmlField.DifferencingLimit;
            //    needUpdate = true;
            //}
            //if (mulTextField.IsolateStyles != xmlField.IsolateStyles)
            //{
            //    mulTextField.IsolateStyles = xmlField.IsolateStyles;
            //    needUpdate = true;
            //}
            if (mulTextField.NumberOfLines != xmlField.NumberOfLines)
            {
                mulTextField.NumberOfLines = xmlField.NumberOfLines;
                needUpdate = true;
            }
            //mulTextField.RestrictedMode = xmlField.RestrictedMode;
            if (mulTextField.RichText != xmlField.RichText)
            {
                mulTextField.RichText = xmlField.RichText;
                needUpdate = true;
            }
            if (mulTextField.RichTextMode != xmlField.RichTextMode)
            {
                mulTextField.RichTextMode = xmlField.RichTextMode;
                needUpdate = true;
            }
            if (mulTextField.UnlimitedLengthInDocumentLibrary != xmlField.UnlimitedLengthInDocumentLibrary)
            {
                mulTextField.UnlimitedLengthInDocumentLibrary = xmlField.UnlimitedLengthInDocumentLibrary;
                needUpdate = true;
            }
            return needUpdate;
        }

        private bool SetGridField(IAveFieldRatingScale gridField, AveXmlField xmlField)
        {
            bool needUpdate = false;
            if (gridField.GridEndNumber != xmlField.GridEndNumber)
            {
                gridField.GridEndNumber = xmlField.GridEndNumber;
                needUpdate = true;
            }
            if (gridField.GridNAOptionText != xmlField.GridNAOptionText)
            {
                gridField.GridNAOptionText = xmlField.GridNAOptionText;
                needUpdate = true;
            }
            if (gridField.GridEndNumber != xmlField.GridEndNumber)
            {
                gridField.GridStartNumber = xmlField.GridEndNumber;
                needUpdate = true;
            }
            if (gridField.GridTextRangeAverage != xmlField.GridTextRangeAverage)
            {
                gridField.GridTextRangeAverage = xmlField.GridTextRangeAverage;
                needUpdate = true;
            }
            if (gridField.GridTextRangeHigh != xmlField.GridTextRangeHigh)
            {
                gridField.GridTextRangeHigh = xmlField.GridTextRangeHigh;
                needUpdate = true;
            }
            if (gridField.GridTextRangeLow != xmlField.GridTextRangeLow)
            {
                gridField.GridTextRangeLow = xmlField.GridTextRangeLow;
                needUpdate = true;
            }

            if (SetMultiChocieField(gridField, xmlField))
            {
                needUpdate = true;
            }
            return needUpdate;
        }
        /// <summary>
        /// 对于修改List的setting而导致field的属性变化的那些属性，例如Indexed属性，不作为该field的定义进行compare，但是此类属性也需要更新，添加此方法对此类属性进行更新
        /// </summary>
        private void UpdatePropertiesReleatedToSettings(IAveField field, AveXmlField xmlField)
        {
            bool needUpdate = false;
            if (field.Indexed != xmlField.Indexed)
            {
                field.Indexed = xmlField.Indexed;
                needUpdate = true;
            }
            try
            {
                if (needUpdate)
                {
                    field.Update();
                }
            }
            catch (Exception ex)
            {
                log.Warn("update column indexed failed.source field:{0}  dest field:{1},error message:{2}", xmlField.XmlElement.OuterXml, field.SchemaXml, ex.Message);
            }
        }

        /// <summary>
        /// wrapper在还原listself结束之后将list目的端的lookup field作为cache存放起来，在目的端和远端不冲突的情况下，必须对cache中的lookup list重定位到原端的id上，否则在post Action中处理的lookup value还原可能出现问题
        /// </summary>
        /// <param name="field"></param>
        /// <param name="xmlField"></param>
        private void UpdateListLookUpIdProperty(IAveField field, AveXmlField xmlField)
        {
            if ((xmlField.Type == AveFieldType.Lookup) && !String.IsNullOrEmpty(xmlField.AveLookupListTitle))
            {
                try
                {
                    AveLookupObject obj = new AveLookupObject();
                    obj.Id = field.ID;
                    obj.ListTitle = xmlField.AveLookupListTitle;
                    obj.WebUrl = xmlField.AveLookupWebTitle;
                    obj.Type = xmlField.AveSourceType;
                    obj.WebId = mWebId;
                    obj.List = xmlField.AveLookupListID;

                    if (mAveSPList != null)
                    {
                        obj.ListId = mAveSPList.SPList.ID;
                        mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.AddLookupField(obj);
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, WrapperRestoreResource.UpdateListLookUpFailed, e);
                }
            }
        }
        #endregion

        #endregion

        #region Compare fields
        /// <summary>
        /// When the field is conflict with the destination, return false
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="xmlField"></param>
        /// <param name="spField"></param>
        /// <returns>false, when conflict</returns>
        protected bool Compare(AveXmlField xmlField, IAveField spField)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFieldCollection.Compare"))
            {
#endif
                try
                {
                    //如果有custommapping，并且用displayname mapping 同type不判断title。
                    //现在使用custommapping就不走别的find逻辑，如果以后有修改的话，这个判断就不足了，必须确定是custommappingfind并且用display mapping上的才不判断title
                    if (!xmlField.Title.Equals(spField.Title, StringComparison.Ordinal) && (xmlField.CustomFieldInfo == null || xmlField.CustomFieldInfo.UseInternalOrDisplay))
                    {
                        log.Warn("Field conflict, Title is different. Field internal name : {0}", spField.InternalName);
                        return false;
                    }
                    if (!AveFieldHelper.CompareFieldType(spField, xmlField))
                    {
                        log.Warn("Field conflict, field type is different. Field internal name : {0}", spField.InternalName);
                        return false;
                    }
                    //restore NoCrawl for Searchable Columns
                    if (!AveFieldHelper.CompareNoCrawl(spField, xmlField))
                    {
                        log.Warn("Field conflict, NoCrawl is different. Field internal name : {0}", spField.InternalName);
                        return false;
                    }

                    if (!CompareBaseField(spField, xmlField))
                    {
                        return false;
                    }

                    if (spField.Sealed)
                    {
                        return true;
                    }

                    switch (spField.Type)
                    {
                        case AveFieldType.Lookup:
                            //case AveFieldType.Facilities:
                            IAveFieldLookup lookupField = spField as IAveFieldLookup;
                            if (lookupField != null)
                            {
                                if (!CompareLookupField(lookupField, xmlField))
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                return false;
                            }
                            break;
                        case AveFieldType.User:
                            //case AveFieldType.CallTo:
                            //case AveFieldType.SendTo:
                            IAveFieldUser userField = spField as IAveFieldUser;
                            if (userField != null)
                            {
                                if (!CompareUserField(userField, xmlField))
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                return false;
                            }
                            break;
                        case AveFieldType.DateTime:
                            //case AveFieldType.From:
                            //case AveFieldType.DueDate:
                            //case AveFieldType.CallTime:
                            //case AveFieldType.Until:
                            IAveFieldDateTime timeField = spField as IAveFieldDateTime;
                            if (timeField != null)
                            {
                                if (!CompareDateTimeField(timeField, xmlField))
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                return false;
                            }
                            break;
                        case AveFieldType.Boolean:
                        //case AveFieldType.WhatsNew:
                        //case AveFieldType.Confidential:
                        case AveFieldType.AllDayEvent:
                            //case AveFieldType.AllowEditing:
                            IAveFieldBoolean boolField = spField as IAveFieldBoolean;
                            if (boolField == null)
                            {
                                return false;
                            }
                            if (!CompareBoolField(boolField, xmlField))
                            {
                                return false;
                            }
                            break;
                        case AveFieldType.Choice:
                        //case AveFieldType.ContactInfo:
                        //case AveFieldType.Whereabout:
                        case AveFieldType.WorkflowStatus:
                        case AveFieldType.OutcomeChoice:
                            IAveFieldChoice choiceField = spField as IAveFieldChoice;
                            if (choiceField == null)
                            {
                                return false;
                            }
                            if (!CompareChoiceField(choiceField, xmlField))
                            {
                                return false;
                            }
                            break;
                        case AveFieldType.MultiChoice:
                            IAveFieldMultiChoice multiChocieField = spField as IAveFieldMultiChoice;
                            if (multiChocieField == null)
                            {
                                return false;
                            }
                            if (!CompareMultiChocieField(multiChocieField, xmlField))
                            {
                                return false;
                            }
                            break;
                        case AveFieldType.Calculated:
                            IAveFieldCalculated calField = spField as IAveFieldCalculated;
                            if (calField == null)
                            {
                                return false;
                            }
                            if (!CompareCalculatedField(calField, xmlField))
                            {
                                return false;
                            }
                            break;
                        case AveFieldType.Computed:
                            IAveFieldComputed computedField = spField as IAveFieldComputed;
                            if (computedField == null)
                            {
                                return false;
                            }
                            if (!CompareComputed(computedField, xmlField))
                            {
                                return false;
                            }
                            break;
                        case AveFieldType.Currency:
                            IAveFieldCurrency currencyField = spField as IAveFieldCurrency;
                            if (currencyField == null)
                            {
                                return false;
                            }
                            if (!CompareCurrency(currencyField, xmlField))
                            {
                                return false;
                            }
                            break;
                        case AveFieldType.Number:
                        case AveFieldType.Integer:
                        case AveFieldType.WorkflowEventType:
                            IAveFieldNumber numberField = spField as IAveFieldNumber;
                            if (numberField == null)
                            {
                                return false;
                            }
                            if (!CompareNumberField(numberField, xmlField, true))
                            {
                                return false;
                            }
                            break;
                        case AveFieldType.Note:
                            IAveFieldMultiLineText mulTextField = spField as IAveFieldMultiLineText;
                            if (mulTextField == null)
                            {
                                return false;
                            }
                            if (!CompareNoteField(mulTextField, xmlField))
                            {
                                return false;
                            }
                            break;
                        case AveFieldType.GridChoice:
                            IAveFieldRatingScale gridField = spField as IAveFieldRatingScale;
                            if (gridField == null)
                            {
                                return false;
                            }
                            if (!CompareGridField(gridField, xmlField))
                            {
                                return false;
                            }
                            break;
                        case AveFieldType.Text:
                            //case AveFieldType.Confirmations:
                            IAveFieldText textField = spField as IAveFieldText;
                            if (textField == null)
                            {
                                return false;
                            }
                            if (!CompareTextField(textField, xmlField))
                            {
                                return false;
                            }
                            break;
                        case AveFieldType.URL:
                            IAveFieldUrl urlField = spField as IAveFieldUrl;
                            if (urlField == null)
                            {
                                return false;
                            }
                            if (!CompareUrlField(urlField, xmlField))
                            {
                                return false;
                            }
                            break;
                        case AveFieldType.Invalid:
                            if (spField.TypeAsString == "Facilities")
                            {
                                //can to do something
                                break;
                            }
                            if (spField.TypeAsString == "TaxonomyFieldType" || spField.TypeAsString == "TaxonomyFieldTypeMulti")
                            {
                                if (!CompareTaxonomyField(spField, xmlField))
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
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while compare field. filed id:{0}, field title:{1}\n error message:{2}", spField.ID, spField.Title, e));
                }
                return true;
#if PerformanceLog
            }
#endif
        }

        private bool GetFieldsMatchType(AveXmlField xmlField, IAveField spField, ref FieldMatchType matchType)
        {
            if (xmlField.XmlElement.HasAttribute("ID") && spField.ID.Equals(new Guid(xmlField.XmlElement.GetAttribute("ID"))))
            {
                matchType |= FieldMatchType.ID;
            }
            if (spField.InternalName == xmlField.KeyName)
            {
                matchType |= FieldMatchType.Name;
            }
            if (xmlField.XmlElement.HasAttribute("StaticName") && spField.StaticName.Equals(xmlField.XmlElement.GetAttribute("StaticName")))
            {
                matchType |= FieldMatchType.StaticName;
            }
            if (xmlField.XmlElement.HasAttribute("DisplayName") && spField.Title.Equals(xmlField.XmlElement.GetAttribute("DisplayName")))
            {
                matchType |= FieldMatchType.DisplayName;
            }
            return true;
        }

        private bool CompareBaseField(IAveField field, AveXmlField xmlField)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFieldCollection.CompareBaseField"))
            {
#endif
                string fieldAttrValue = null;
                //if (field.AllowDuplicateValues != xmlField.AllowDuplicateValues)
                //{
                //    field.AllowDuplicateValues = xmlField.AllowDuplicateValues;
                //    needUpdate = true;
                //}
                if (!AveTypeHelper.IsAllNullOrEmpty(field.AggregationFunction, xmlField.AggregationFunction)
                    && field.AggregationFunction != xmlField.AggregationFunction)
                {
                    log.Warn("CompareBaseField: Field conflict, AggregationFunction is different. Field internal name : {0}", field.InternalName);
                    return false;
                }
                fieldAttrValue = field.GetAttributeFromSchemaXml("AllowDeletion");
                bool? allowDeletion = fieldAttrValue == null ? (bool?)null : Convert.ToBoolean(fieldAttrValue);
                if (allowDeletion != xmlField.AllowDeletion)
                {
                    log.Warn("CompareBaseField: Field conflict, AllowDeletion is different. Field internal name : {0}", field.InternalName);
                    return false;
                }
                if (field.DefaultFormula != xmlField.DefaultFormula)
                {
                    log.Warn("CompareBaseField: Field conflict, DefaultFormula is different. Field internal name : {0}", field.InternalName);
                    return false;
                }
                string defaultValue = xmlField.DefaultValue;
                if (!string.IsNullOrEmpty(defaultValue) && defaultValue.StartsWith("$Resources", StringComparison.OrdinalIgnoreCase))
                {
                    defaultValue = mAveParentSite.ObjectModelFactory.Utility.GetLocalizedString(defaultValue, "core", (uint)CultureInfo.CurrentUICulture.LCID);
                }

                string fieldDefaultValue = field.DefaultValue;
                if (string.IsNullOrEmpty(fieldDefaultValue))
                {
                    fieldDefaultValue = null;
                }
                if (fieldDefaultValue != defaultValue)
                {
                    log.Warn("CompareBaseField: Field conflict, DefaultValue is different. Field internal name : {0}", field.InternalName);
                    return false;
                }
                //源端和目的端语言相同时判断冲突 SAAS-23280
                if (mAveSPWeb.ParentSite.AveLanguageProcesser == null || mAveSPWeb.ParentSite.AveLanguageProcesser.SrcId == mAveSPWeb.ParentSite.AveLanguageProcesser.DesId)
                {
                    if (field.Description != xmlField.Description)
                    {
                        log.Warn("CompareBaseField: Field conflict, Description is different. Field internal name : {0}", field.InternalName);
                        return false;
                    }
                }
                if (!field.DescriptionResource.CompareUserResource(mAveSPWeb.SPWeb, xmlField.DescriptionResource))
                {
                    log.Warn("CompareBaseField: Field conflict, DescriptionResource is different. Field internal name : {0}", field.InternalName);
                    return false;
                }
                if (field.Direction != xmlField.Direction)
                {
                    log.Warn("CompareBaseField: Field conflict, Direction is different. Field internal name : {0}", field.InternalName);
                    return false;
                }
                //private string mDirection = "none";
                if (field.DisplaySize != xmlField.DisplaySize)
                {
                    log.Warn("CompareBaseField: Field conflict, DisplaySize is different. Field internal name : {0}", field.InternalName);
                    return false;
                }
                ////private string mDisplaySize = null;
                //if (field.EcbMenu != xmlField.EcbMenu)
                //{
                //    field.EcbMenu = xmlField.EcbMenu;
                //    needUpdate = true;
                //}
                ////private bool mEcbMenu = false;
                //if (field.EcbMenuAllowed != xmlField.EcbMenuAllowed)
                //{
                //    field.EcbMenuAllowed = xmlField.EcbMenuAllowed;
                //    needUpdate = true;
                //}
                //private bool? mEcbMenuAllowed = null;
                if (field.TypeAsString != xmlField.TypeAsString)
                {
                    log.Warn("CompareBaseField: Field conflict, TypeAsString is different. Field internal name : {0}", field.InternalName);
                    return false;
                }
                //private string mTypeAsString = null;
                if (field.ValidationFormula != xmlField.ValidationFormula)
                {
                    log.Warn("CompareBaseField: Field conflict, ValidationFormula is different. Field internal name : {0}", field.InternalName);
                    return false;
                }
                //private string mValidationFormula = null;
                if (field.ValidationMessage != xmlField.ValidationMessage)
                {
                    log.Warn("CompareBaseField: Field conflict, ValidationMessage is different. Field internal name : {0}", field.InternalName);
                    return false;
                }

                //private string mXPath = null;
                //indexed是在list的Metadata navigation settings中配置导致的，不属于field定义范围内，在此不用其进行比较
                //if (field.Indexed != xmlField.Indexed)
                //{
                //    return false;
                //}
                //private bool mIndexed = false;
                string ime = xmlField.IMEMode;
                uint lCID = (uint)mLocaleID;
                if ((((lCID != 0x404) && (lCID != 0x804)) && ((lCID != 0xc04) && (lCID != 0x1004))) && ((lCID != 0x411) && (lCID != 0x412)))
                {
                    ime = null;
                }
                if ((!(field is IAveFieldNumber)) && field.IMEMode != null && field.IMEMode != ime && !(field.IMEMode.Equals("inactive") && ime == null))    //inactive is the default value, so does null
                {
                    log.Warn("CompareBaseField: Field conflict, IMEMode is different. Field internal name : {0}", field.InternalName);
                    return false;
                }
                //private string mIMEMode = null;
                fieldAttrValue = field.GetAttributeFromSchemaXml("Hidden");
                bool hidden = fieldAttrValue == null ? false : Convert.ToBoolean(fieldAttrValue);
                //由于AveXmlField.cs line：584中给xmlField.Hidden赋值的逻辑而来
                if (field.TypeAsString == AveFieldType.Guid.ToString())
                {
                    hidden = true;
                }
                //CanToggleHidden 为false，hidden需要更改schemaXml才能更新，保险起见CanToggleHidden为false跳过hidden属性的还原。
                var tempValue = field.GetAttributeFromSchemaXml("CanToggleHidden");
                bool canToggleHidden = fieldAttrValue != null && tempValue == null ? false : Convert.ToBoolean(tempValue);
                if (canToggleHidden && hidden != xmlField.Hidden && xmlField.ID != new Guid("3881510a-4e4a-4ee8-b102-8ee8e2d0dd4b") && xmlField.ID != new Guid("39360f11-34cf-4356-9945-25c44e68dade"))
                {
                    if (string.Equals(field.InternalName, RA.Common.Global.RcordsBuiltInColumn.ITEM_BCS_NAME, StringComparison.OrdinalIgnoreCase))
                    {
                        log.Warn("Skipped CompareBaseField: Field conflict, Hidden is different. Field internal name : {0}", field.InternalName);
                    }
                    else
                    {
                        log.Warn("CompareBaseField: Field conflict, Hidden is different. Field internal name : {0}", field.InternalName);
                        return false;
                    }
                }
                //private bool mHidden = false;
                string group = xmlField.Group;
                if (!string.IsNullOrEmpty(group))
                {
                    group = mAveParentSite.GetNameByLanguageMapping(group, AveLanguageMappingType.FieldMapping);
                }
                if (!field.Group.Equals(group, StringComparison.OrdinalIgnoreCase))
                {
                    var sourceGroup = xmlField.GetFieldAttributeValue("Group");
                    var destinationGroup = field.GetAttributeFromSchemaXml("Group");
                    if (sourceGroup != destinationGroup)
                    {
                        if ((bool?)!group?.Equals(AveSPResource.GetString(1033, "CustomColumnsGroup")) ?? true || !AveSPResource.GetStrings("CustomColumnsGroup").Contains(field.Group))
                        {
                            log.Warn("CompareBaseField: Field conflict, Group is different. Field internal name : {0}, source group name:{1}, destination group name:{2}", field.InternalName, group, field.Group);
                            return false;
                        }
                    }
                }
                //private string mGroup = null;
                if (field.JumpToField != xmlField.JumpToField)
                {
                    log.Warn("CompareBaseField: Field conflict, JumpToField is different. Field internal name : {0}", field.InternalName);
                    return false;
                }
                //private string mJumpToField = null;
                if (field.LinkToItem != xmlField.LinkToItem)
                {
                    log.Warn("CompareBaseField: Field conflict, LinkToItem is different. Field internal name : {0}", field.InternalName);
                    return false;
                }
                //private bool mLinkToItem = false;
                //Need Change
                //if (field.LinkToItemAllowed != xmlField.LinkToItemAllowed)
                //{
                //    field.LinkToItemAllowed = xmlField.LinkToItemAllowed;
                //    needUpdate = true;
                //}
                //private bool? mLinkToItemAllowed = null;
                if (field.NoCrawl != xmlField.NoCrawl)
                {
                    log.Warn("CompareBaseField: Field conflict, NoCrawl is different. Field internal name : {0}", field.InternalName);
                    return false;
                }
                //private bool mNoCrawl = false;
                if (!AveTypeHelper.IsAllNullOrEmpty(field.PIAttribute, xmlField.PIAttribute)
                    && field.PIAttribute != xmlField.PIAttribute)
                {
                    log.Warn("CompareBaseField: Field conflict, PIAttribute is different. Field internal name : {0}", field.InternalName);
                    return false;
                }
                //private string mPIAttribute = null;
                if (!AveTypeHelper.IsAllNullOrEmpty(field.PITarget, xmlField.PITarget)
                    && field.PITarget != xmlField.PITarget)
                {
                    log.Warn("CompareBaseField: Field conflict, PITarget is different. Field internal name : {0}", field.InternalName);
                    return false;
                }
                //private string mPITarget = null;
                if (!AveTypeHelper.IsAllNullOrEmpty(field.PrimaryPIAttribute, xmlField.PrimaryPIAttribute)
                    && field.PrimaryPIAttribute != xmlField.PrimaryPIAttribute)
                {
                    log.Warn("CompareBaseField: Field conflict, PrimaryPIAttribute is different. Field internal name : {0}", field.InternalName);
                    return false;
                }
                //private string mPrimaryPIAttribute = null;
                if (!AveTypeHelper.IsAllNullOrEmpty(field.PrimaryPITarget, xmlField.PrimaryPITarget)
                    && field.PrimaryPITarget != xmlField.PrimaryPITarget)
                {
                    log.Warn("CompareBaseField: Field conflict, PrimaryPITarget is different. Field internal name : {0}", field.InternalName);
                    return false;
                }
                //private string mPrimaryPITarget = null;
                if (field.ReadOnlyField != xmlField.ReadOnlyField)
                {
                    log.Warn("CompareBaseField: Field conflict, ReadOnlyField is different. Field internal name : {0}", field.InternalName);
                    return false;
                }
                //private bool mReadOnlyField = false;
                if (!AveTypeHelper.IsAllNullOrEmpty(field.RelatedField, xmlField.RelatedField) && field.RelatedField != xmlField.RelatedField)
                {
                    log.Warn("CompareBaseField: Field conflict, RelatedField is different. Field internal name : {0}", field.InternalName);
                    return false;
                }
                //private string mRelatedField = null;
                //if (WrapperRuntime.CurrentContext.ModelFactory.ContextKind != AveContextKind.ClientObjectModel)
                //if (mAveParentSite.SPSite.APIType != AveAPIType.BPOS_D && mAveParentSite.SPSite.APIType != AveAPIType.BPOS_S)
                //{
                //    if (field.Required != xmlField.Required)
                //    {
                //        log.Warn("CompareBaseField: Field conflict, Required is different. Field internal name : {0}", field.InternalName);
                //        return false;
                //    }
                //}
                //private bool mRequired = false;
                if (field.Sealed != xmlField.Sealed)
                {
                    log.Warn("CompareBaseField: Field conflict, Sealed is different. Field internal name : {0}", field.InternalName);
                    return false;
                }
                //private bool mSealed = false;
                if (field.ShowInDisplayForm != xmlField.ShowInDisplayForm)
                {
                    log.Warn("CompareBaseField: Field conflict, ShowInDisplayForm is different. Field internal name : {0}", field.InternalName);
                    return false;
                }
                //private bool? mShowInDisplayForm = null;
                if (field.ShowInEditForm != xmlField.ShowInEditForm)
                {
                    log.Warn("CompareBaseField: Field conflict, ShowInEditForm is different. Field internal name : {0}", field.InternalName);
                    return false;
                }
                //private bool? mShowInEditForm = null;
                if (field.ShowInListSettings != xmlField.ShowInListSettings)
                {
                    log.Warn("CompareBaseField: Field conflict, ShowInListSettings is different. Field internal name : {0}", field.InternalName);
                    return false;
                }
                //private bool? mShowInListSettings = null;
                if (field.ShowInNewForm != xmlField.ShowInNewForm)
                {
                    log.Warn("CompareBaseField: Field conflict, ShowInNewForm is different. Field internal name : {0}", field.InternalName);
                    return false;
                }
                //private bool? mShowInNewForm = null;

                if (field.GetAttributeFromSchemaXml("ShowInVersionHistory") != null)
                {
                    bool showInVersionHistory = GetShowInVersionHistory(field, xmlField);
                    if (field.ShowInVersionHistory != showInVersionHistory)
                    {
                        log.Warn("CompareBaseField: Field conflict, ShowInVersionHistory is different. Field internal name : {0}", field.InternalName);
                        return false;
                    }
                }

                fieldAttrValue = field.GetAttributeFromSchemaXml("ShowInViewForms");
                bool? showInViewForms = fieldAttrValue == null ? (bool?)null : Convert.ToBoolean(fieldAttrValue);
                //private bool mShowInVersionHistory = false;
                if (showInViewForms != xmlField.ShowInViewForms)
                {
                    log.Warn("CompareBaseField: Field conflict, ShowInViewForms is different. Field internal name : {0}", field.InternalName);
                    return false;
                }
                //private bool? mShowInViewForms = null;
                if (field.StaticName != xmlField.StaticName)
                {
                    log.Warn("CompareBaseField: Field conflict, StaticName is different. Field internal name : {0}", field.InternalName);
                    return false;
                }
                //private string mStaticName = null;
                //如果有custommapping，并且用displayname mapping 同type不判断title。
                //现在使用custommapping就不走别的find逻辑，如果以后有修改的话，这个判断就不足了，必须确定是custommappingfind并且用display mapping上的才不判断title
                if (field.Title != xmlField.Title && (xmlField.CustomFieldInfo == null || xmlField.CustomFieldInfo.UseInternalOrDisplay))
                {
                    log.Warn("CompareBaseField: Field conflict, Title is different. Field internal name : {0}", field.InternalName);
                    return false;
                }
                if (!field.TitleResource.CompareUserResource(mAveSPWeb.SPWeb, xmlField.TitleResource))
                {
                    log.Warn("CompareBaseField: Field conflict, TitleResource is different. Field internal name : {0}", field.InternalName);
                    return false;
                }
                //private string mTitle = null;
                if (field.TranslationXml != xmlField.TranslationXml &&
                    !(string.IsNullOrEmpty(field.TranslationXml) && string.IsNullOrEmpty(xmlField.TranslationXml)))
                {
                    log.Warn("CompareBaseField: Field conflict, TranslationXml is different. Field internal name : {0}", field.InternalName);
                    return false;
                }
                //private string mTranslationXml = null;
                if (field.EnforceUniqueValues != xmlField.EnforceUniqueValues)
                {
                    log.Warn("CompareBaseField: Field conflict, EnforceUniqueValues is different. Field internal name : {0}", field.InternalName);
                    return false;
                }
                return true;
#if PerformanceLog
            }
#endif
        }

        private bool CompareLookupField(IAveFieldLookup lookupField, AveXmlField xmlField)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFieldCollection.CompareLookupField"))
            {
#endif

                if (lookupField.IsRelationship != xmlField.IsRelationship)
                {
                    log.Warn("CompareLookupField: Field conflict, IsRelationship is different. Field internal name : {0}", lookupField.InternalName);
                    return false;
                }
                if (xmlField.LookupField != null && lookupField.LookupField != null && !lookupField.LookupField.Equals(xmlField.LookupField, StringComparison.OrdinalIgnoreCase))//the default value of lookupfield is null, so skip the null field
                {
                    log.Warn("CompareLookupField: Field conflict, LookupField is different. Field internal name : {0}", lookupField.InternalName);
                    return false;
                }

                Guid webId = Guid.Empty;
                if (xmlField.Type == AveFieldType.Lookup && xmlField.AveLookupWebTitle != null && mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.WebUrlMapping.ContainsKey(xmlField.AveLookupWebTitle))
                {
                    if (String.IsNullOrEmpty(xmlField.LookupWebId) || xmlField.LookupWebId.Equals(Guid.Empty.ToString()))
                    {
                        webId = mWebId;
                    }
                    else
                    {
                        if (xmlField.AveSourceType == "1" && mAveSPList != null)
                        {
                            //IAveField parentField = AveFieldHelper.FindFieldInCollection(xmlField.ID, mAveSPList.ParentWeb.SPWeb.Fields);
                            IAveField parentField = FindFieldFromParentWeb(xmlField, mAveSPList.ParentWeb.SPWeb.AvailableFields);
                            if (parentField != null && parentField is IAveFieldLookup)
                            {
                                webId = ((IAveFieldLookup)parentField).LookupWebId;
                            }
                            else
                            {
                                webId = mAveSPWeb.ParentSite.GetWeb(mAveSPWeb.QueryService, mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.WebUrlMapping[xmlField.AveLookupWebTitle]);
                            }
                        }
                        else
                        {
                            webId = mAveSPWeb.ParentSite.GetWeb(mAveSPWeb.QueryService, mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.WebUrlMapping[xmlField.AveLookupWebTitle]);
                        }
                    }
                    if (lookupField.LookupWebId != webId)
                    {
                        //log.Warn("CompareLookupField: Field conflict, LookupWebId is different. Field internal name : {0}", lookupField.InternalName);
                        log.Warn("CompareLookupField: Field conflict, LookupWebId is different: {0}-{1}, Field internal name : {2}", webId, lookupField.LookupWebId, lookupField.InternalName);
                        return false;
                    }

                    if (String.IsNullOrEmpty(xmlField.AveLookupListTitle))
                    {
                        //if (!lookupField.LookupList.Equals("Self", StringComparison.CurrentCultureIgnoreCase) && lookupField.LookupList.Equals(xmlField.AveLookupListID, StringComparison.CurrentCultureIgnoreCase))
                        //{
                        //    log.Warn("CompareLookupField: Field conflict, LookupList-1 is different. Field internal name : {0}", lookupField.InternalName);
                        //    return false;
                        //}
                    }
                    else
                    {
                        Guid listId = new Guid();
                        if (this.mAveSPWeb != null)
                        {
                            listId = this.mAveSPWeb.ParentSite.GetList(webId, xmlField.AveLookupListTitle);
                        }
                        else if (this.mAveSPList != null)
                        {
                            listId = this.mAveSPList.ParentWeb.ParentSite.GetList(webId, xmlField.AveLookupListTitle);
                        }
                        //need to get the list id when not found
                        if (listId == Guid.Empty && xmlField.AveSourceType == "1" && mAveSPList != null)
                        {
                            //IAveField parentField = AveFieldHelper.FindFieldInCollection(xmlField.ID, mAveSPList.ParentWeb.SPWeb.Fields);
                            IAveField parentField = FindFieldFromParentWeb(xmlField, mAveSPList.ParentWeb.SPWeb.AvailableFields);
                            if (parentField != null && parentField is IAveFieldLookup)
                            {
                                listId = new Guid(((IAveFieldLookup)parentField).LookupList);
                            }
                        }
                        if (!listId.Equals(Guid.Empty))
                        {
                            Guid dstListId = Guid.Empty;
                            if (!string.IsNullOrEmpty(lookupField.LookupList))
                                dstListId = new Guid(lookupField.LookupList);

                            if (dstListId != listId)
                            {
                                log.Warn("CompareLookupField: Field conflict, LookupList-2 is different. Field internal name : {0}", lookupField.InternalName);
                                return false;
                            }
                        }
                        else
                        {
                            log.Warn("CompareLookupField: Field conflict, LookupList is not found. Field internal name : {0}", lookupField.InternalName);
                            return false;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(xmlField.LookupList) && mAveSPList != null)
                {
                    // mapping contains xmlField.loopuplist
                    // if need update at last    AveLookupObject obj = new AveLookupObject();
                    if (!AveFieldHelper.IsGuid(xmlField.LookupList))
                    {
                        if (lookupField.GetFieldAttributeValue("List") != xmlField.LookupList)
                        {
                            log.Warn("CompareLookupField: Field conflict, LookupList-3 is different. Field internal name : {0}", lookupField.InternalName);
                            return false;
                        }
                    }
                    else if (mAveSPWeb != null && mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.ListIdMapping.ContainsKey(new Guid(xmlField.LookupList)))
                    {
                        Guid destListId = mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.ListIdMapping[new Guid(xmlField.LookupList)];
                        Guid dstListId = Guid.Empty;

                        if (!string.IsNullOrEmpty(lookupField.LookupList))
                            dstListId = new Guid(lookupField.LookupList);

                        if (dstListId != destListId)
                        {
                            log.Warn("CompareLookupField: Field conflict, LookupList-4 is different. Field internal name : {0}", lookupField.InternalName);
                            return false;
                        }
                    }
                }

                if (lookupField.AllowMultipleValues && lookupField.PrependId != xmlField.PrependId)
                {
                    log.Warn("CompareLookupField: Field conflict, PrependId is different. Field internal name : {0}", lookupField.InternalName);
                    return false;
                }
                if (lookupField.PrimaryFieldId != xmlField.PrimaryFieldId)
                {
                    log.Warn("CompareLookupField: Field conflict, PrimaryFieldId is different. Field internal name : {0}", lookupField.InternalName);
                    return false;
                }
                //if (lookupField.RelationshipDeleteBehavior != xmlField.RelationshipDeleteBehavior) //SAAS-1412 现有的代码逻辑是先将目的端的RelationshipDeleteBehavior更新成none 然后在postaction里赋成源端的值。这样比较该值没有意义
                //{
                //    return false;
                //}
                if (lookupField.UnlimitedLengthInDocumentLibrary != xmlField.UnlimitedLengthInDocumentLibrary)
                {
                    log.Warn("CompareLookupField: Field conflict, UnlimitedLengthInDocumentLibrary is different. Field internal name : {0}", lookupField.InternalName);
                    return false;
                }
                if (lookupField.AllowMultipleValues != xmlField.AllowMultipleValues)
                {
                    log.Warn("CompareLookupField: Field conflict, AllowMultipleValues is different. Field internal name : {0}", lookupField.InternalName);
                    return false;
                }
                if (lookupField.CountRelated != xmlField.CountRelated)
                {
                    log.Warn("CompareLookupField: Field conflict, CountRelated is different. Field internal name : {0}", lookupField.InternalName);
                    return false;
                }
                return true;
#if PerformanceLog
            }
#endif
        }

        private bool CompareUserField(IAveFieldUser userField, AveXmlField xmlField)
        {
            if (userField.AllowDisplay != xmlField.AllowDisplay)
            {
                log.Warn("CompareUserField: Field conflict, AllowDisplay is different. Field internal name : {0}", userField.InternalName);
                return false;
            }
            if (userField.Presence != xmlField.Presence)
            {
                log.Warn("CompareUserField: Field conflict, Presence is different. Field internal name : {0}", userField.InternalName);
                return false;
            }
            IAvePrincipal desPrincipal = mAveSPWeb.ParentSite.SPMembers.FindMember(xmlField.SelectionGroup, true);
            if ((desPrincipal != null && userField.SelectionGroup != desPrincipal.ID) ||
                (desPrincipal == null && mAveParentSite.SPSite.RootWeb.SiteGroups.GetByID(userField.SelectionGroup) != null))//当源端为All Users,目的端为SharePoint Group时判断为属性冲突.
            {
                log.Warn("CompareUserField: Field conflict, SelectionGroup Id is different. Field internal name : {0}", userField.InternalName);
                return false;
            }
            if (userField.SelectionMode != xmlField.SelectionMode)
            {
                log.Warn("CompareUserField: Field conflict, SelectionMode is different. Field internal name : {0}", userField.InternalName);
                return false;
            }

            if (!CompareLookupField(userField, xmlField))
            {
                return false;
            }
            return true;
        }

        private bool CompareDateTimeField(IAveFieldDateTime timeField, AveXmlField xmlField)
        {
            if (timeField.CalendarType != xmlField.CalendarType)
            {
                log.Warn("CompareDateTimeField: Field conflict, CalendarType is different. Field internal name : {0}", timeField.InternalName);
                return false;
            }
            if (timeField.DisplayFormat != xmlField.DisplayFormat)
            {
                log.Warn("CompareDateTimeField: Field conflict, DisplayFormat is different. Field internal name : {0}", timeField.InternalName);
                return false;
            }
            if (timeField.FriendlyDisplayFormat != xmlField.FriendlyDisplayFormat)
            {
                log.Warn("CompareDateTimeField: Field conflict, FriendlyDisplayFormat is different. Field internal name : {0}", timeField.InternalName);
                return false;
            }
            return true;
        }

        private bool CompareBoolField(IAveFieldBoolean boolField, AveXmlField xmlField)
        {
            if (boolField.JumpToNoField != xmlField.JumpToNoField)
            {
                log.Warn("CompareBoolField: Field conflict, JumpToNoField is different. Field internal name : {0}", boolField.InternalName);
                return false;
            }
            if (boolField.JumpToYesField != xmlField.JumpToYesField)
            {
                log.Warn("CompareBoolField: Field conflict, JumpToYesField is different. Field internal name : {0}", boolField.InternalName);
                return false;
            }
            return true;
        }

        private bool CompareChoiceField(IAveFieldChoice choiceField, AveXmlField xmlField)
        {
            //if (choiceField.FillinChoiceJumpTo != xmlField.FillinChoiceJumpTo)
            //{
            //    choiceField.FillinChoiceJumpTo = xmlField.FillinChoiceJumpTo;
            //    needUpdate = true;
            //}
            if (choiceField.EditFormat != xmlField.EditFormat)
            {
                log.Warn("CompareChoiceField: Field conflict, EditFormat is different. Field internal name : {0}", choiceField.InternalName);
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
                log.Warn("CompareMultiChocieField: Field conflict, FillInChoice is different. Field internal name : {0}", field.InternalName);
                return false;
            }
            return true;
        }

        private bool CompareChocies(IAveFieldMultiChoice field, StringCollection choices)
        {
            bool choiceValueNotExist = false;
            foreach (string ch in choices)
            {
                //string choice = SPUtility.GetLocalizedString(ch.Trim(), "core", mLanguageID);
                string choicesValue = ch;
                if (!string.IsNullOrEmpty(choicesValue) && choicesValue.StartsWith("$Resources", StringComparison.OrdinalIgnoreCase))
                {
                    choicesValue = mAveParentSite.ObjectModelFactory.Utility.GetLocalizedString(choicesValue, "core", (uint)mAveSPWeb.SPWeb.UICulture.LCID);
                }
                if (!field.Choices.Contains(choicesValue))
                {
                    log.Warn("CompareChocies: choice Value not exist. Field internal name : {0}, value : {1}", field.InternalName, choicesValue);
                    return false;
                }
            }
            //if (choiceValueNotExist)
            //{
            //    return false;
            //}
            return true;
        }

        private bool CompareCalculatedField(IAveFieldCalculated calField, AveXmlField xmlField)
        {
            int localeId = xmlField.CurrencyLocaleId;
            if (localeId == -1)
            {
                localeId = (int)mLanguageID;
            }
            //if (calField.CurrencyLocaleId != localeId)
            //{
            //    calField.CurrencyLocaleId = localeId;
            //    needUpdate = true;
            //}
            if (calField.DateFormat != xmlField.DateFormat)
            {
                log.Warn("CompareCalculatedField: Field conflict, DateFormat is different. Field internal name : {0}", calField.InternalName);
                return false;
            }
            //if (calField.DisplayFormat != xmlField.DisplayFormat_Calculated)//client api 不支持SAAS-967
            //{
            //    return false;
            //}
            if (calField.Formula != xmlField.Formula)
            {
                log.Warn("CompareCalculatedField: Field conflict, Formula is different. Field internal name : {0}", calField.InternalName);
                return false;
            }
            if (calField.OutputType != xmlField.OutputType)
            {
                log.Warn("CompareCalculatedField: Field conflict, OutputType is different. Field internal name : {0}", calField.InternalName);
                return false;
            }
            //if (calField.ShowAsPercentage != xmlField.ShowAsPercentage)//client api 不支持SAAS-967
            //{
            //    return false;
            //}
            return true;
        }

        private bool CompareComputed(IAveFieldComputed computedField, AveXmlField xmlField)
        {
            if (computedField.EnableLookup != xmlField.EnableLookup)
            {
                log.Warn("CompareComputed: Field conflict, EnableLookup is different. Field internal name : {0}", computedField.InternalName);
                return false;
            }
            return true;
        }

        private bool CompareCurrency(IAveFieldCurrency currencyField, AveXmlField xmlField)
        {
            int localeId = xmlField.CurrencyLocaleId;
            if (localeId == -1)
            {
                localeId = (int)mLanguageID;
            }
            if (currencyField.CurrencyLocaleId != localeId)
            {
                log.Warn("CompareCurrency: Field conflict, CurrencyLocaleId is different. Field internal name : {0}", currencyField.InternalName);
                return false;
            }
            if (!CompareNumberField(currencyField, xmlField, false))
            {
                return false;
            }
            return true;
        }

        private bool CompareNumberField(IAveFieldNumber numberField, AveXmlField xmlField, bool checkPercentage)
        {
            if (numberField.DisplayFormat != xmlField.DisplayFormat_Number)
            {
                log.Warn("CompareNumberField: Field conflict, DisplayFormat is different. Field internal name : {0}", numberField.InternalName);
                return false;
            }
            if (Math.Abs(numberField.MaximumValue - xmlField.MaximumValue) > 1E-06)
            {
                log.Warn("CompareNumberField: Field conflict, MaximumValue is different. Field internal name : {0}", numberField.InternalName);
                return false;
            }
            if (Math.Abs(numberField.MinimumValue - xmlField.MinimumValue) > 1E-06)
            {
                log.Warn("CompareNumberField: Field conflict, MinimumValue is different. Field internal name : {0}", numberField.InternalName);
                return false;
            }
            if (numberField.DefaultValue != xmlField.DefaultValue)
            {
                log.Warn("CompareNumberField: Field conflict, DefaultValue is different. Field internal name : {0}", numberField.InternalName);
                return false;
            }
            if (numberField.ShowAsPercentage != xmlField.ShowAsPercentage)
            {
                log.Warn("CompareNumberField: Field conflict, ShowAsPercentage is different. Field internal name : {0}", numberField.InternalName);
                return false;
            }
            return true;
        }

        private bool CompareNoteField(IAveFieldMultiLineText mulTextField, AveXmlField xmlField)
        {
            if (mulTextField.AllowHyperlink != xmlField.AllowHyperlink)
            {
                log.Warn("CompareNoteField: Field conflict, AllowHyperlink is different. Field internal name : {0}", mulTextField.InternalName);
                return false;
            }
            if (mulTextField.AppendOnly != xmlField.AppendOnly)
            {
                log.Warn("CompareNoteField: Field conflict, AppendOnly is different. Field internal name : {0}", mulTextField.InternalName);
                return false;
            }
            //if (mulTextField.DifferencingLimit != xmlField.DifferencingLimit)
            //{
            //    log.Warn("CompareNoteField: Field conflict, DifferencingLimit is different. Field internal name : {0}", mulTextField.InternalName);
            //    return false;
            //}
            //if (mulTextField.IsolateStyles != xmlField.IsolateStyles)
            //{
            //    log.Warn("CompareNoteField: Field conflict, IsolateStyles is different. Field internal name : {0}", mulTextField.InternalName);
            //    return false;
            //}
            if (mulTextField.NumberOfLines != xmlField.NumberOfLines)
            {
                log.Warn("CompareNoteField: Field conflict, NumberOfLines is different. Field internal name : {0}", mulTextField.InternalName);
                return false;
            }
            //if (mulTextField.RestrictedMode != xmlField.RestrictedMode)
            //{                
            //    log.Warn("CompareNoteField: Field conflict, NumberOfLines is different. Field internal name : {0}", mulTextField.InternalName);
            //    return false;
            //}
            if (mulTextField.RichText != xmlField.RichText)
            {
                log.Warn("CompareNoteField: Field conflict, RichText is different. Field internal name : {0}", mulTextField.InternalName);
                return false;
            }
            if (mulTextField.RichTextMode != xmlField.RichTextMode)
            {
                log.Warn("CompareNoteField: Field conflict, RichTextMode is different. Field internal name : {0}", mulTextField.InternalName);
                return false;
            }
            if (mulTextField.UnlimitedLengthInDocumentLibrary != xmlField.UnlimitedLengthInDocumentLibrary)
            {
                log.Warn("CompareNoteField: Field conflict, UnlimitedLengthInDocumentLibrary is different. Field internal name : {0}", mulTextField.InternalName);
                return false;
            }
            return true;
        }

        private bool CompareGridField(IAveFieldRatingScale gridField, AveXmlField xmlField)
        {
            if (gridField.GridEndNumber != xmlField.GridEndNumber)
            {
                log.Warn("CompareGridField: Field conflict, GridEndNumber is different. Field internal name : {0}", gridField.InternalName);
                return false;
            }
            if (gridField.GridNAOptionText != xmlField.GridNAOptionText)
            {
                log.Warn("CompareGridField: Field conflict, GridNAOptionText is different. Field internal name : {0}", gridField.InternalName);
                return false;
            }
            if (gridField.GridTextRangeAverage != xmlField.GridTextRangeAverage)
            {
                log.Warn("CompareGridField: Field conflict, GridTextRangeAverage is different. Field internal name : {0}", gridField.InternalName);
                return false;
            }
            if (gridField.GridTextRangeHigh != xmlField.GridTextRangeHigh)
            {
                log.Warn("CompareGridField: Field conflict, GridTextRangeHigh is different. Field internal name : {0}", gridField.InternalName);
                return false;
            }
            if (gridField.GridTextRangeLow != xmlField.GridTextRangeLow)
            {
                log.Warn("CompareGridField: Field conflict, GridTextRangeLow is different. Field internal name : {0}", gridField.InternalName);
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
                log.Warn("CompareTextField: Field conflict, MaxLength is different. Field internal name : {0}", textField.InternalName);
                return false;
            }
            if (textField.DifferencingLimit != xmlField.DifferencingLimit)
            {
                log.Warn("CompareTextField: Field conflict, DifferencingLimit is different. Field internal name : {0}", textField.InternalName);
                return false;
            }
            return true;
        }

        private bool CompareUrlField(IAveFieldUrl urlField, AveXmlField xmlField)
        {
            if (urlField.DisplayFormat != xmlField.DisplayFormat_Url)
            {
                log.Warn("CompareUrlField: Field conflict, DisplayFormat is different. Field internal name : {0}", urlField.InternalName);
                return false;
            }
            return true;
        }

        private bool CompareTaxonomyField(IAveField field, AveXmlField xmlField)
        {
            IAveTaxonomyField taxField = field as IAveTaxonomyField;
            string termSetIDWithName = xmlField.GetCustomerProperty("TermSetId").ToString();
            string termSetId = string.Empty;
            if (termSetIDWithName.Contains('|'))
            {
                string[] temp = termSetIDWithName.Split('|');
                if (temp.Length == 2)
                {
                    termSetId = temp[0].ToString();
                }
            }
            else
            {
                termSetId = termSetIDWithName;
            }
            if (!taxField.TermSetId.ToString().Equals(termSetId, StringComparison.OrdinalIgnoreCase))
            {
                log.Warn("CompareTaxonomyField: Field conflict, TermSetId is different. Field internal name : {0}", taxField.InternalName);
                return false;
            }
            return true;
        }

        private bool CompareInvalidField(IAveField field, AveXmlField xmlField)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFieldCollection.CompareInvalidField"))
            {
#endif
                if (mBaseFields == null)
                {
                    InitBaseFields();
                }
                XmlElement fieldElement = field.Node as XmlElement;
                if (fieldElement != null)
                {
                    if (!CompareElementField(fieldElement, xmlField.XmlElement, true))
                    {
                        return false;
                    }

                    ArrayList nodeNameList = new ArrayList();
                    foreach (XmlElement child in xmlField.XmlElement.ChildNodes)
                    {
                        if (mBaseNodeFields.Contains(child.Name))
                        {
                            continue;
                        }
                        if (!CompareInvalidNode(fieldElement, child))
                        {
                            return false;
                        }
                        nodeNameList.Add(child.Name);
                    }
                    foreach (XmlElement child in fieldElement.ChildNodes)
                    {
                        if (!nodeNameList.Contains(child.Name))
                        {
                            return false;
                        }
                    }
                }
                return true;
#if PerformanceLog
            }
#endif
        }

        private bool CompareInvalidField(XmlElement fieldElement, string name, XmlElement element)
        {
            if (fieldElement.HasAttribute(name) && !element.HasAttribute(name))
            {
                log.Warn("CompareInvalidField: Field conflict, element not found, name : {0}", name);
                return false;
            }
            else if (fieldElement.GetAttribute(name) != element.GetAttribute(name))
            {
                log.Warn("CompareInvalidField: Field conflict, element value not match, name : {0}", name);
                return false;
            }
            return true;
        }

        private bool CompareElementField(XmlElement fieldElement, XmlElement sourceElement, bool checkBase)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFieldCollection.CompareElementField"))
            {
#endif
                List<string> attributeList = new List<string>();
                foreach (XmlAttribute attri in sourceElement.Attributes)
                {
                    attributeList.Add(attri.Name);
                }
                foreach (XmlAttribute attri in fieldElement.Attributes)
                {
                    if (!attributeList.Contains(attri.Name))
                    {
                        attributeList.Add(attri.Name);
                    }
                }

                foreach (string attriName in attributeList)
                {
                    if (checkBase && mBaseFields.Contains(attriName))
                    {
                        continue;
                    }
                    if (!CompareInvalidField(fieldElement, attriName, sourceElement))
                    {
                        return false;
                    }
                }
                return true;

#if PerformanceLog
            }
#endif
        }

        private bool CompareInvalidNode(XmlElement fieldElement, XmlElement sourceElement)
        {
            XmlElement element = GetNode(fieldElement, sourceElement.Name);
            if (!element.InnerXml.Equals(sourceElement.InnerXml))
            {
                log.Warn("CompareInvalidNode: Field conflict, element InnerXml is different. element name : {0}", sourceElement.Name);
                return false;
            }
            if (!CompareElementField(element, sourceElement, false))
            {
                return false;
            }
            return true;
        }

        #endregion

        #region Update field basic info
        protected void UpdateFieldXmls(Dictionary<string, AveXmlField> xmlFields, FieldType fieldType)
        {
            foreach (KeyValuePair<string, AveXmlField> xmlFieldPair in xmlFields)
            {
                AveXmlField field = xmlFieldPair.Value;
                UpdateFieldXml(field, fieldType);
            }
        }

        protected void UpdateFieldXml(AveXmlField xmlField, FieldType fieldType)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFieldCollection.UpdateFieldXml"))
            {
#endif
                if (xmlField.XmlElement.HasAttribute("DisplayName"))
                {
                    string displayName = xmlField.XmlElement.GetAttribute("DisplayName");
                    if (!xmlField.FieldInternalName.Equals("User_x0020_Name"))////DOC-59253 publishing portal在Long Running Operation Status下还原这个column如果使用languageMapping会导致pressrelease站点无法打开，暂时不做mapping处理
                    {
                        string mappingDisplayName = mAveSPWeb.ParentSite.GetNameByLanguageMapping(displayName, AveLanguageMappingType.FieldMapping);
                        if (!displayName.Equals(mappingDisplayName))
                        {
                            xmlField.XmlElement.SetAttribute("DisplayName", mappingDisplayName);
                            xmlField.Title = mappingDisplayName;
                        }
                    }
                }

                if (fieldType == FieldType.List && mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.WebFieldsIdMapping.ContainsKey(mWebId)
                    && mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.WebFieldsIdMapping[mWebId].ContainsKey(xmlField.ID))
                {
                    Guid newID = mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.WebFieldsIdMapping[mWebId][xmlField.ID];
                    xmlField.XmlElement.SetAttribute("ID", newID.ToString());
                }

#if PerformanceLog
            }
#endif
        }

        #endregion

        protected void AddExistLookupFields(IAveFieldCollection fields)
        {
            if (fields != null)
            {
                foreach (IAveField field in fields)
                {
                    IAveFieldLookup fieldlookup = field as IAveFieldLookup;
                    if (fieldlookup != null && !(field is IAveFieldUser) && !string.IsNullOrEmpty(fieldlookup.LookupList))
                    {
                        AveLookupObject aloj = new AveLookupObject();
                        aloj.WebId = fieldlookup.LookupWebId;
                        aloj.ListId = mAveSPList.SPList.ID;
                        aloj.List = fieldlookup.LookupList;
                        aloj.Id = field.ID;
                        aloj.ListTitle = mAveSPList.SPList.Title;
                        mAveParentSite.MappingManager.SiteMappingManager.AddLookupField(aloj);
                    }
                }
            }
        }

        public IReport GetReport()
        {
            return reportor;
        }

        public void Dispose()
        {
            if(reportor!= null)
            {
                reportor.Dispose();
            }
        }
    }
    [AveCodeReview("2012/04/13", "yuzhi.jiang@avepoint.com", "jin.zhang@AvePoint.com", new string[2] { CodeReviewConstants.CHECK_LIST_ID_CS_1, CodeReviewConstants.CHECK_LIST_ID_EH_2 }, null, true)]
    public class AveSPWebFieldCollection : AveSPFieldCollection
    {
        private List<Dictionary<Guid, Guid>> mAvailableMappings;
        protected override IAveFieldCollection FieldCollection
        {
            get { return mAveSPWeb.SPWeb.Fields; }
        }

        public override void LoadFields(string fieldsXml)
        {
            LoadXmlFields(fieldsXml, mXmlFields);
        }

        public AveSPWebFieldCollection(AveSPWeb aveSPWeb)
            : base(aveSPWeb.RestoreConfiguraion)
        {
            mAveSPWeb = aveSPWeb;
            mAveParentSite = aveSPWeb.ParentSite;
            mXmlFields = new Dictionary<string, AveXmlField>();

        }

        public override void RestoreFields(string fieldsXml, AveFieldRestoreOption restoreOption)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWebFieldCollection.RestoreFields"))
            {
#endif
                LoadFields(fieldsXml);
                RestoreFields(mXmlFields, FieldType.Web, restoreOption);
                AveFieldHelper.UpdateFieldSchemaIdMappingProperty(mAveSPWeb.SPWeb, FieldMapping.EnumFieldSchemaMapping().ToDictionary(pair => pair.Key, pair => pair.Value));
#if PerformanceLog
            }
#endif
        }

        internal override IAveField Find(AveXmlField xmlField, FieldFindOption findOption, ref FieldMatchType matchType)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWebFieldCollection.Find"))
            {
#endif

                IAveField field = null;
                try
                {
                    switch (findOption)
                    {
                        case FieldFindOption.FindById:
                            field = AveFieldHelper.GetSiteField(new Guid(xmlField.XmlElement.GetAttribute("ID")), mAveSPWeb.SPWeb, ref NeedSkip);
                            if (null != field)
                            {
                                matchType = FieldMatchType.ID;
                            }
                            break;
                        case FieldFindOption.Children:
                            {
                                if (mAveParentSite.ObjectModelFactory.ContextKind == AveContextKind.ServerObjectModel)
                                {
                                    if (AveFieldHelper.GetSiteFieldInChildren(GetScope(mAveSPWeb.ServerRelativeUrl), mAveParentSite.SPSite.ID, new Guid(xmlField.XmlElement.GetAttribute("ID")), mAveSPWeb.SPWeb))
                                    {
                                        matchType = FieldMatchType.Children;
                                    }
                                }
                            }
                            break;
                        case FieldFindOption.FindByInternalName:
                            field = AveFieldHelper.GetSiteFieldByInternalName(xmlField.KeyName, xmlField.Type, mAveSPWeb.SPWeb, ref NeedSkip);
                            if (null != field)
                            {
                                matchType = FieldMatchType.Name;
                            }
                            break;
                        case FieldFindOption.FindBySchema:
                            field = AveFieldHelper.FindSiteFieldBySchema(new Guid(xmlField.XmlElement.GetAttribute("ID")), mAveSPWeb.SPWeb, FieldMapping.EnumFieldSchemaMapping().ToDictionary(pair => pair.Key, pair => pair.Value), mAvailableMappings);
                            if (null != field)
                            {
                                matchType = FieldMatchType.Schema;
                            }
                            break;
                        case FieldFindOption.FindByStaticName:
                            field = AveFieldHelper.GetSiteFieldByStaticName(xmlField.XmlElement.GetAttribute("StaticName"), xmlField.Type, mAveSPWeb.SPWeb);
                            if (null != field)
                            {
                                matchType = FieldMatchType.StaticName;
                            }
                            break;
                        case FieldFindOption.FindByDisplayName:
                            field = AveFieldHelper.GetSiteField(xmlField.XmlElement.GetAttribute("DisplayName"), xmlField.Type, mAveSPWeb.SPWeb, ref NeedSkip);
                            if (null != field)
                            {
                                matchType = FieldMatchType.DisplayName;
                            }
                            break;
                        case FieldFindOption.FindByCustomMapping:
                            if (xmlField.CustomFieldInfo != null)
                            {
                                field = AveFieldHelper.FindFieldInCollectionByCustomMapping(xmlField.CustomFieldInfo, FieldCollection, xmlField.Type);
                                if (null != field)
                                {
                                    matchType = FieldMatchType.CustomMapping;
                                }
                            }
                            break;
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.FindFieldByObjectError, findOption, e);
                }
                if (null != field && RestoredFieldIdMapping.ContainsKey(field.ID))
                {
                    field = null;
                    matchType = FieldMatchType.None;
                }
                return field;
#if PerformanceLog
            }
#endif
        }

        internal string GetScope(string ServerRelativeUrl)
        {
            try
            {
                if (ServerRelativeUrl.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    ServerRelativeUrl = ServerRelativeUrl.Substring(1);
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetScopeFailed, e);
            }
            return ServerRelativeUrl;
        }

        internal override void InitSchemaMappings()
        {
            if (null != mAveSPWeb.SPWeb && FieldMapping.EnumFieldSchemaMapping().ToDictionary(pair => pair.Key, pair => pair.Value).Count == 0)
            {
                FieldMapping.SetFieldIdSchemaMappings(AveFieldHelper.GetFieldMapping(mAveSPWeb.SPWeb.AllProperties));
                mAvailableMappings = AveFieldHelper.GetAvaliableFieldIdMappings(mAveSPWeb.SPWeb);
            }
        }
    }
    [AveCodeReview("2012/04/13", "yuzhi.jiang@avepoint.com", "jin.zhang@AvePoint.com", new string[2] { CodeReviewConstants.CHECK_LIST_ID_CS_1, CodeReviewConstants.CHECK_LIST_ID_EH_2 }, null, true)]
    [AveCodeReview("2012/06/08", "sid.you@avepoint.com", "kexin.guo@AvePoint.com", new string[0] { }, null, true)]
    public class AveSPListFieldCollection : AveSPFieldCollection
    {
        internal static readonly HashSet<string> NO_RESTORE_FIELD_MAP = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        internal static readonly HashSet<string> NEED_RESTORE_FIELD_MAP = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        internal static readonly HashSet<string> NEED_REPLACE_FIELD = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        internal bool HasCreateFieldWhenEnsureFields { get; private set; }

        //private AveSPList mAveSPList;
        //private Dictionary<string, AveSPField> mFields;
        //private Dictionary<string, AveXmlField> mXmlFields;
        private Dictionary<Guid, string> mValidationFields;
        private Dictionary<Guid, string> mFieldsDefaultValue;
        private Dictionary<string, Dictionary<string, string>> LooUpListItemIDAndValues = new Dictionary<string, Dictionary<string, string>>();
        private bool mCreateFieldIfNotExist = true;
        private bool mSkipIfConflict = false;
        public bool CreateFieldWhenEnsureFields = false;
        private bool mModifiedFieldReadOnly = false;
        private bool mModifiedFieldHidden = false;    
        //mNintexFormDataVale 一个list只能有一个FormData field
        private static AveNintexFormDataFieldInfo mNintexFormDataVale;

        public void SetIfCreateFieldIfNotExist(bool create)
        {
            mCreateFieldIfNotExist = create;
        }
        public void SetSkipIfConflict(bool skip)
        {
            mSkipIfConflict = skip;
        }

        static AveSPListFieldCollection()
        {
            NO_RESTORE_FIELD_MAP.Add("#tp_ID");
            NO_RESTORE_FIELD_MAP.Add("#tp_ListId");
            NO_RESTORE_FIELD_MAP.Add("#tp_SiteId");
            NO_RESTORE_FIELD_MAP.Add("#tp_RowOrdinal");
            NO_RESTORE_FIELD_MAP.Add("#tp_Version");
            NO_RESTORE_FIELD_MAP.Add("#tp_Ordering");
            NO_RESTORE_FIELD_MAP.Add("#tp_ThreadIndex");
            NO_RESTORE_FIELD_MAP.Add("#tp_HasAttachment");
            NO_RESTORE_FIELD_MAP.Add("#tp_ModerationStatus");
            NO_RESTORE_FIELD_MAP.Add("#tp_IsCurrent");
            //NO_RESTORE_FIELD_MAP.Add("#tp_ItemOrder");
            NO_RESTORE_FIELD_MAP.Add("#tp_InstanceID");
            NO_RESTORE_FIELD_MAP.Add("#tp_GUID");
            NO_RESTORE_FIELD_MAP.Add("#tp_CopySource");
            NO_RESTORE_FIELD_MAP.Add("#tp_HasCopyDestinations");
            NO_RESTORE_FIELD_MAP.Add("#tp_AuditFlags");
            NO_RESTORE_FIELD_MAP.Add("#tp_InheritAuditFlags");
            NO_RESTORE_FIELD_MAP.Add("#tp_Size");
            NO_RESTORE_FIELD_MAP.Add("#tp_WorkflowVersion");
            NO_RESTORE_FIELD_MAP.Add("#tp_WorkflowInstanceID");
            NO_RESTORE_FIELD_MAP.Add("#tp_ParentId");
            NO_RESTORE_FIELD_MAP.Add("#tp_DocId");
            NO_RESTORE_FIELD_MAP.Add("#tp_DeleteTransactionId");
            NO_RESTORE_FIELD_MAP.Add("#uniqueidentifier1");
            NO_RESTORE_FIELD_MAP.Add("#tp_Level");
            NO_RESTORE_FIELD_MAP.Add("#tp_IsCurrentVersion");
            NO_RESTORE_FIELD_MAP.Add("#tp_UIVersion");
            NO_RESTORE_FIELD_MAP.Add("#tp_CalculatedVersion");
            NO_RESTORE_FIELD_MAP.Add("#tp_UIVersionString");
            NO_RESTORE_FIELD_MAP.Add("#tp_DraftOwnerId");

            //DOC-67843
            //report metadata 下的item的这个column指向的是report template下面的doc的guid，这里不能使用源端数据
            NO_RESTORE_FIELD_MAP.Add("_dlc_Reporting_TemplateId");
            NO_RESTORE_FIELD_MAP.Add("_dlc_Reporting_QueryAssembly");
            NO_RESTORE_FIELD_MAP.Add("_dlc_Reporting_InjectionAssembly");
            NO_RESTORE_FIELD_MAP.Add("_dlc_Reporting_InjectionClass");
            NO_RESTORE_FIELD_MAP.Add("_dlc_Reporting_IconUrl");
            NO_RESTORE_FIELD_MAP.Add("_dlc_Reporting_HttpContentType");

            //don't restore holds field values
            NO_RESTORE_FIELD_MAP.Add("_vti_ItemHoldRecordStatus");
            //NO_RESTORE_FIELD_MAP.Add("IconOverlay");

            //SAAS-44647 skip migration column.
            NO_RESTORE_FIELD_MAP.Add("MigrationWizId");
            NO_RESTORE_FIELD_MAP.Add("MigrationWizIdVersion");
            NO_RESTORE_FIELD_MAP.Add("MigrationWizIdPermissions");

            //应用field filter的时候，有些field是不能被filter的，添加到NEED_RESTORE_FIELD_MAP中,用小写字符表示
            NEED_RESTORE_FIELD_MAP.Add("WikiField");
            NEED_RESTORE_FIELD_MAP.Add("Editor");
            NEED_RESTORE_FIELD_MAP.Add("Author");
            NEED_RESTORE_FIELD_MAP.Add("Modified");
            NEED_RESTORE_FIELD_MAP.Add("Created");
            NEED_RESTORE_FIELD_MAP.Add("PublishingPageImage");
            NEED_RESTORE_FIELD_MAP.Add("SummaryLinks");

            //有些field value是xml格式的，并且其中存在一些链接，需要对这些链接做源端到目的端的替换
            NEED_REPLACE_FIELD.Add("WikiField");
            NEED_REPLACE_FIELD.Add("PublishingPageImage");
            NEED_REPLACE_FIELD.Add("SummaryLinks");
            NEED_REPLACE_FIELD.Add("PublishingPageContent");
        }

        public AveSPListFieldCollection(AveSPList aveSPList)
            : base(aveSPList.RestoreConfiguraion)
        {
            mAveSPList = aveSPList;
            mAveSPWeb = aveSPList.ParentWeb;
            mWebId = mAveSPWeb.SPWeb.ID;
            mIsOneDrive = IsOneDrive();
            mAveParentSite = mAveSPWeb.ParentSite;
            //mFields = new Dictionary<string, AveSPField>();
            mXmlFields = new Dictionary<string, AveXmlField>();
            mFieldTypeFilter = new List<string>() { AveFieldType.WorkflowStatus.ToString() };
        }

        internal override void InitSchemaMappings()
        {
            if (null != mAveSPList.SPList && FieldMapping.EnumFieldSchemaMapping().ToDictionary(pair => pair.Key, pair => pair.Value).Count == 0)
            {
                //FieldIdSchemaMappings = AveFieldHelper.GetFieldMapping(mAveSPList.SPList.RootFolder.Properties);
                FieldMapping.SetFieldIdSchemaMappings(AveFieldHelper.GetFieldMapping(mAveSPList.SPList.RootFolder.Properties));
            }
        }

        protected override void LoadSourceFieldOrder(string fieldsXml)
        {
            try
            {
                SourceFieldOrder = LoadFieldOrder(fieldsXml);
            }
            catch (Exception ex)
            {
                log.Warn("Load the source field order error.Exception:" + ex.ToString());
            }
        }

        private List<string> LoadDestFieldOrder(string fieldsXml)
        {
            try
            {
                return LoadFieldOrder(fieldsXml);
            }
            catch (Exception ex)
            {
                log.Warn("Load the dest field order error.Exception:" + ex.ToString());
                return null;
            }
        }

        private List<string> LoadFieldOrder(string fieldsXml)
        {
            List<string> fieldOrder = new List<string>();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(fieldsXml);
            foreach (XmlNode xmlNode in doc.DocumentElement.ChildNodes)
            {
                XmlElement xe = xmlNode as XmlElement;
                if (xe.HasAttribute("Name"))
                {
                    string internalName = xe.GetAttribute("Name");
                    string type = xe.GetAttribute("Type");
                    //由于Workflow 的column 不会显示在Column Order 中，这里暂不还原它的order，以免还原order时由于不存在抛异常。
                    if (!fieldOrder.Contains(internalName) && (string.IsNullOrEmpty(type) || !type.Equals("WorkflowStatus", StringComparison.OrdinalIgnoreCase)))
                    {
                        fieldOrder.Add(internalName);
                    }
                }
            }
            return fieldOrder;
        }

        protected override IAveFieldCollection FieldCollection
        {
            get { return mAveSPList.SPList.Fields; }
        }

        public override void LoadFields(string fieldsXml)
        {
            LoadXmlFields(fieldsXml, mXmlFields);
        }

        internal override IAveField Find(AveXmlField xmlField, FieldFindOption findOption, ref FieldMatchType matchType)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPListFieldCollection.Find"))
            {
#endif
                IAveField field = null;
                try
                {
                    switch (findOption)
                    {
                        case FieldFindOption.FindById:
                            field = AveFieldHelper.FindFieldInCollection(new Guid(xmlField.XmlElement.GetAttribute("ID")), FieldCollection);
                            if (null != field)
                            {
                                matchType = FieldMatchType.ID;
                            }
                            break;
                        case FieldFindOption.FindByInternalName:
                            field = AveFieldHelper.FindFieldInCollection(xmlField.KeyName, xmlField.Type, FieldCollection);
                            if (null != field)
                            {
                                matchType = FieldMatchType.Name;
                            }
                            break;
                        case FieldFindOption.FindBySchema:
                            field = AveFieldHelper.FindListFieldBySchema(new Guid(xmlField.XmlElement.GetAttribute("ID")), xmlField.Type, mAveSPList.SPList, FieldMapping.EnumFieldSchemaMapping().ToDictionary(pair => pair.Key, pair => pair.Value));
                            if (null != field)
                            {
                                matchType = FieldMatchType.Schema;
                            }
                            break;
                        case FieldFindOption.FindByStaticName:
                            field = AveFieldHelper.FindFieldInCollectionByStaticName(xmlField.XmlElement.GetAttribute("StaticName"), xmlField.Type, FieldCollection);
                            if (null != field)
                            {
                                matchType = FieldMatchType.StaticName;
                            }
                            break;
                        case FieldFindOption.FindByDisplayName:
                            field = AveFieldHelper.FindFieldInCollection(xmlField.XmlElement.GetAttribute("DisplayName"), xmlField.Type, true, FieldCollection);
                            if (null != field)
                            {
                                matchType = FieldMatchType.DisplayName;

                                #region ADO-45122 如果两端field的ref情况不相等,认为没有找到

                                XmlDocument xdoc = new XmlDocument();
                                xdoc.LoadXml(field.SchemaXml);
                                if (xmlField.XmlElement.HasAttribute("FieldRef"))
                                {
                                    if (!xdoc.DocumentElement.HasAttribute("FieldRef"))
                                    {
                                        matchType = FieldMatchType.None;
                                        field = null;
                                    }
                                    else
                                    {
                                        if (!xmlField.XmlElement.GetAttribute("FieldRef").Equals(xdoc.DocumentElement.GetAttribute("FieldRef")))
                                        {
                                            matchType = FieldMatchType.None;
                                            field = null;
                                        }
                                    }
                                }
                                else
                                {
                                    if (xdoc.DocumentElement.HasAttribute("FieldRef"))
                                    {
                                        matchType = FieldMatchType.None;
                                        field = null;
                                    }
                                }
                                xdoc.RemoveAll();
                                xdoc = null;

                                #endregion
                            }
                            break;
                        case FieldFindOption.FindByCustomMapping:
                            if (xmlField.CustomFieldInfo != null)
                            {
                                //field = AveFieldHelper.FindFieldInCollectionByCustomMapping(xmlField.CustomFieldInfo.Name, xmlField.Type, xmlField.CustomFieldInfo.TypeAsString, FieldCollection);
                                field = AveFieldHelper.FindFieldInCollectionByCustomMapping(xmlField.CustomFieldInfo, FieldCollection, xmlField.Type);
                                if (null != field)
                                {
                                    matchType = FieldMatchType.CustomMapping;
                                }
                            }
                            break;
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.FindFieldByObjectError, findOption.ToString(), e);
                }
                //SAAS-11863该逻辑会导致反插calculated column时将其关联的column多还出一个同名的出来
                //目前没有发现该逻辑的用处，暂做注释，并且测试通过
                if (null != field && RestoredFieldIdMapping.ContainsKey(field.ID))
                {
                    field = null;
                    matchType = FieldMatchType.None;
                }
                return field;

#if PerformanceLog
            }
#endif
        }

        public void ResetNintexFormDataFieldValue(AveNintexFormDataFieldInfo nintexFormDataVale)
        {
            mNintexFormDataVale = nintexFormDataVale;
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

        public override void RestoreFields(string fieldsXml, AveFieldRestoreOption restoreOption)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPListFieldCollection.RestoreFields"))
            {
#endif
                LoadFields(fieldsXml);
                RestoreFields(mXmlFields, FieldType.List, restoreOption);
                RestoreListFieldOrder();
                //LoadFields(mAveSPList.SPList.Fields, mFields);
#if PerformanceLog
            }
#endif
        }

        //keep modified in bpos-s
        public bool EnableModifiedField(bool readOnly)
        {
            try
            {
                IAveFieldCollection fields = mAveSPList.SPList.Fields;
                IAveField modifiedField = fields[AveBuiltInFieldId.Modified];
                //IAveField createdField = fields[new Guid("8c06beca-0777-48f7-91c7-6da68bc07b69")];
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

        public void RestoreFieldDefaultValue()
        {
            if (mFieldsDefaultValue == null || mFieldsDefaultValue.Count == 0)
            {
                return;
            }
            foreach (Guid fieldId in mFieldsDefaultValue.Keys)
            {
                IAveField field = AveFieldHelper.FindFieldInCollection(fieldId, mAveSPList.SPList.Fields);
                if (field != null)
                {
                    field.DefaultValue = mFieldsDefaultValue[fieldId];
                    field.Update();
                }
            }
        }

        public IAveField GetFieldByInternalName(string internalName)
        {
            if (FieldCollection.ContainsField(internalName))
            {
                return FieldCollection.GetFieldByInternalName(internalName);
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

        public IAveField EnsureField(Guid fieldId)
        {
            foreach (string key in mXmlFields.Keys)
            {
                if (fieldId == mXmlFields[key].ID)
                {
                    return EnsureField(key, mCreateFieldIfNotExist, this.mAveSPList.SPList.Fields, mXmlFields);
                }
            }
            return null;
        }

        public IAveField EnsureField(string internalName)
        {
            return EnsureField(internalName, mCreateFieldIfNotExist, this.mAveSPList.SPList.Fields, mXmlFields);
        }

        private IAveField EnsureField(string name, bool createIfNotExist, IAveFieldCollection spFields, Dictionary<string, AveXmlField> xmlFields)
        {
            AveFieldRestoreOption restoreOption = new AveFieldRestoreOption();
            return EnsureField(name, createIfNotExist, spFields, xmlFields, !mCreateFieldIfNotExist, mSkipIfConflict, restoreOption);
        }

        private IAveField EnsureField(string name, bool restoreSchemDependency, IAveFieldCollection spFields, Dictionary<string, AveXmlField> xmlFields, bool throwWhenNotFound, bool throwWhenConflict, AveFieldRestoreOption restoreOption)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPListFieldCollection.EnsureField"))
            {
#endif
                if (string.IsNullOrEmpty(name))
                {
                    return null;
                }
                if (mSkippedRestoreFields != null && mSkippedRestoreFields.Contains(name))
                {
                    log.Log(AveLogLevel.INFO, "Skip restore field:{0}", name);
                    return null;
                }

                if (RestoredFieldInternalNameList.Contains(name))
                {
                    string mappingValue = FieldMapping.GetMappingRestoredFieldInternalName(name);
                    if (!String.IsNullOrEmpty(mappingValue))
                    {
                        name = mappingValue;
                    }
                    try
                    {
                        return spFields.GetFieldByInternalName(name);
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, WrapperRestoreResource.GetFieldByInternalNameError, e.ToString());
                        return null;
                    }
                }
                else
                {
                    //try
                    //{
                    //    IAveField field = spFields.GetFieldByInternalName(name);
                    //    if (AveBuiltInFieldId.Contains(field.ID)
                    //        || xmlFields == null || xmlFields.Count == 0 || !xmlFields.ContainsKey(name))
                    //    {
                    //        return field;
                    //    }
                    //}
                    //catch (Exception e)
                    //{
                    //    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetFieldError, e);
                    //}
                    if (xmlFields != null && xmlFields.ContainsKey(name))
                    {
                        AveXmlField srcXF = xmlFields[name];
                        FieldRestoreStatus status = FieldRestoreStatus.None;
                        if (IsFieldShouldSkip(srcXF, null, FieldFindOption.FindByInternalName))
                        {
                            return null;
                        }
                        var findOptions = restoreOption.FindOption;
                        srcXF.CustomFieldInfo = FieldMapping.GetMappingFieldBeforeAdd(new AveSourceFieldInfo() { SourceInternalName = srcXF.FieldInternalName, SourceDisplayName = srcXF.Title, SourceFieldId = srcXF.ID });
                        if (srcXF.CustomFieldInfo != null)
                        {
                            srcXF.CustomFieldInfo.SourceType = srcXF.Type;
                            findOptions = new FieldFindOption[] { FieldFindOption.FindByCustomMapping };
                        }
                        //check是否是metadata field的TextField，如果是需要把对应的metadata field反插出来。
                        //string metadataFieldName = null;
                        //if (CheckMetadataTextField(srcXF, xmlFields, ref metadataFieldName))
                        //{
                        //    RestoreField(metadataFieldName, xmlFields[metadataFieldName], FieldType.List, new AveRestoreOption(), ref status);
                        //}

                        //If the field is calculated field,restore fields in Formula.
                        UpdateFieldXml(srcXF, FieldType.List);
                        FieldMatchType matchType = FieldMatchType.None;
                        IAveField field = null;
                        foreach (FieldFindOption findOption in findOptions)
                        {
                            field = Find(srcXF, findOption, ref matchType);
                            if (null != field)
                            {
                                break;
                            }
                        }
                        if (!restoreSchemDependency && null == field && throwWhenNotFound)
                        {
                            throw new AveFieldSchemaDependencyNotFoundException(srcXF.Title);
                        }
                        List<string> calculatedRelatedField = new List<string>();
                        if (CheckCalculatedField(srcXF, mXmlFields, calculatedRelatedField))
                        {
                            foreach (string fieldName in calculatedRelatedField)
                            {
                                if (xmlFields.ContainsKey(fieldName))
                                {
                                    AveXmlField tmpXmlField = xmlFields[fieldName];

                                    //如果已经还原过了，则不需要再次ensure
                                    if (!RestoredFieldIdMapping.ContainsValue(tmpXmlField.ID))
                                    {
                                        var tmpFindOptions = restoreOption.FindOption;
                                        tmpXmlField.CustomFieldInfo = FieldMapping.GetMappingFieldBeforeAdd(new AveSourceFieldInfo() { SourceInternalName = tmpXmlField.FieldInternalName, SourceDisplayName = tmpXmlField.Title, SourceFieldId = tmpXmlField.ID });
                                        if (tmpXmlField.CustomFieldInfo != null)
                                        {
                                            tmpXmlField.CustomFieldInfo.SourceType = tmpXmlField.Type;
                                            tmpFindOptions = new FieldFindOption[] { FieldFindOption.FindByCustomMapping };
                                        }


                                        UpdateFieldXml(tmpXmlField, FieldType.List);
                                        IAveField calField = null;
                                        FieldMatchType tmpMatchType = FieldMatchType.None;

                                        foreach (FieldFindOption findOption in tmpFindOptions)
                                        {
                                            calField = Find(tmpXmlField, findOption, ref tmpMatchType);
                                            if (null != calField)
                                            {
                                                break;
                                            }
                                        }
                                        Restore(tmpXmlField, calField, FieldType.List, tmpMatchType, restoreOption, ref status, false);
                                    }
                                }
                            }
                        }
                        if (field == null)
                        {
                            CreateFieldWhenEnsureFields = true;
                        }
                        field = Restore(srcXF, field, FieldType.List, matchType, restoreOption, ref status, throwWhenNotFound, throwWhenConflict, true);
                        if (field != null)
                        {
                            if (field.TypeAsString.Equals("TaxonomyFieldType", StringComparison.OrdinalIgnoreCase)
                                || field.TypeAsString.Equals("TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase))
                            {
                                if (!mAveSPList.TaxonomyFields.Contains(field.InternalName))
                                {
                                    mAveSPList.TaxonomyFields.Add(field.InternalName);
                                }
                            }
                        }

                        #region 反插Field之后不再修改View和Content Type
                        //if (status == FieldRestoreStatus.NewCreated && field != null)
                        //{
                        //    try
                        //    {
                        //        if (mAveSPList.SPList.DefaultView != null)
                        //        {
                        //            SPView view = mAveSPList.SPList.DefaultView;
                        //            view.ViewFields.Add(field);
                        //            view.Update();
                        //        }
                        //        if (mAveSPList.SPList.ContentTypes.Count > 0)
                        //        {
                        //            SPContentType ct = mAveSPList.SPList.ContentTypes[0];
                        //            SPFieldLink fieldLink = new SPFieldLink(field);
                        //            ct.FieldLinks.Add(fieldLink);
                        //            ct.Update();
                        //        }
                        //    }
                        //    catch (Exception e)
                        //    {
                        //        mLog.Warn("An error occurred while set new created to default view and default contenttype. error:{0}", e.ToString());
                        //    }
                        //}
                        #endregion

                        return field;
                    }
                    else
                    {
                        try
                        {
                            IAveField field = spFields.GetFieldByInternalName(name);
                            if (AveBuiltInFieldId.Contains(field.ID)
                                || xmlFields == null || xmlFields.Count == 0 || !xmlFields.ContainsKey(name))
                            {
                                return field;
                            }
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetFieldError, e);
                        }
                    }
                }
                return null;

#if PerformanceLog
            }
#endif
        }

        public bool EnsureFields(Dictionary<string, object> data, List<Dictionary<string, object>> junctionData, bool restoreSchemaDependency, bool throwWhenNotFound, bool throwWhenConflict, AveFieldRestoreOption restoreOption)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPListFieldCollection.EnsureFields"))
            {
#endif
            CreateFieldWhenEnsureFields = false;
            var beforeRestoreCount = mAveSPList.SPList != null ? FieldCollection.Count : 0;
            //SAAS-24799,支持多值类型的反插
            List<string> fieldNames = new List<string>();
            if (junctionData != null && junctionData.Count > 0)
            {
                foreach (var multiValueCol in junctionData)
                {
                    object fieldId;
                    if (multiValueCol.TryGetValue("tp_FieldId", out fieldId))
                    {
                        Guid id = (Guid)fieldId;

                        var field = mXmlFields.FirstOrDefault(kv => kv.Value.ID.Equals(id));

                        if (field.Value != null && (!fieldNames.Contains(field.Value.FieldInternalName)))
                        {
                            fieldNames.Add(field.Value.FieldInternalName);
                        }
                    }
                }
            }
            foreach (KeyValuePair<string, object> kv in data)
            {
                string key = kv.Key;
                //object value = kv.Value;
                if (key == "#tp_ContentTypeId")
                {
                    key = "ContentType";
                }
                if ((AveSPListFieldCollection.NO_RESTORE_FIELD_MAP.Contains(key) || key.StartsWith(AveConstants.FIELD_SEPARATOR, StringComparison.OrdinalIgnoreCase)))
                {
                    if (key.EqualIgnoreCase("MigrationWizId") || key.EqualIgnoreCase("MigrationWizIdVersion") || key.EqualIgnoreCase("MigrationWizIdPermissions"))
                    {
                        log.Info($"EnsureFields skip specify migration column:{key}.");
                    }
                    continue;
                }
                string fieldName = key;
                if (fieldName.Contains("#"))
                {
                    fieldName = fieldName.Substring(0, fieldName.IndexOf("#", StringComparison.OrdinalIgnoreCase));
                }
                //如果是taxonomy field关联的Text Field，将fieldName设置成对应taxonomy field的Name
                //我们还原taxonomy field的value是通过对应Text Field上的value来还原的
                if (SourceTextTaxonomyDic.ContainsKey(fieldName))
                {
                    fieldName = SourceTextTaxonomyDic[fieldName];
                }
                if (!fieldNames.Contains(fieldName))
                {
                    fieldNames.Add(fieldName);
                }
            }
            bool hasRetry = false;
            foreach (var fieldName in fieldNames)
            {
                try
                {
                    IAveField spField = EnsureField(fieldName, restoreSchemaDependency, mAveSPList.SPList.Fields, mXmlFields, throwWhenNotFound, throwWhenConflict, restoreOption);
                }
                catch (Exception e)
                {
                    //SAAS-40292 Field:MediaServiceMetadata, Microsoft.SharePoint.Client.ServerException: A duplicate field name "617f8947-74b2-36bc-9f7e-21ded7029bb5" was found. 
                    //var needRetry = hasRetry == true ? false: NeedRetryFieldException(e);
                    log.Warn($"An error occured when ensure field:{fieldName}, {e}, hasRetry:{hasRetry}");
                    if (WrapperConfiguration.WrapperConfigurationForBPOS.IsRestoreToSPOLibOrFolder)
                    {
                        log.Info($"Skip ensure field:{fieldName} when restore to SPO library or folder.");
                        continue;
                    }
                    if (!hasRetry)
                    {
                        hasRetry = true;
                        if (ReloadAndFindField(fieldName) != null)
                        {
                            log.Info($"Success to find this field:{fieldName} after reloading list's fields and content types...");
                            EnsureField(fieldName, restoreSchemaDependency, mAveSPList.SPList.Fields, mXmlFields, throwWhenNotFound, throwWhenConflict, restoreOption);
                            continue;
                        }
                    }
                    throw e;
                }
            }
            AddMappingToSite(FieldType.List, mAveParentSite);
            var afterRestoreCount = mAveSPList.SPList != null ? FieldCollection.Count : 0;
            HasCreateFieldWhenEnsureFields = beforeRestoreCount != afterRestoreCount;
            return true;

#if PerformanceLog
            }
#endif
        }

        private IAveField ReloadAndFindField(string fieldName)
        {
            try
            {
                mAveSPList.ReloadList();
                return mAveSPList.SPList.Fields.GetFieldByInternalName(fieldName);
            }
            catch (Exception e)
            {
                log.Warn("An error occured when ReloadAndFindField due to {0}", e);
            }
            return null;
        }

        private bool CheckCalculatedField(AveXmlField srcXF, Dictionary<string, AveXmlField> xmlFields, List<string> fields)
        {
            XmlNodeList nodes = srcXF.XmlElement.GetElementsByTagName("Formula");
            XmlNodeList fieldRefNodes = srcXF.XmlElement.GetElementsByTagName("FieldRef");
            bool needRestore = false;
            if (nodes != null && nodes.Count > 0)
            {
                string formula = nodes[0].InnerText;
                for (int i = 0; i < fieldRefNodes.Count; i++)
                {
                    string oldInternalName = fieldRefNodes[i].Attributes["Name"].Value;
                    if (!formula.Contains(oldInternalName))
                        continue;
                    string internalName = mAveSPWeb.ParentSite.GetNameByLanguageMapping(oldInternalName, AveLanguageMappingType.FieldMapping);
                    fields.Add(internalName);
                    needRestore = true;
                }
            }
            return needRestore;
        }

        internal bool CheckMetadataTextField(AveXmlField textField, Dictionary<string, AveXmlField> xmlFields, ref string metadataFieldName)
        {
            Guid Id = textField.ID;
            foreach (string name in xmlFields.Keys)
            {
                if (xmlFields[name].TypeAsString.Equals("TaxonomyFieldType") || xmlFields[name].TypeAsString.Equals("TaxonomyFieldTypeMulti"))
                {
                    object customProperty = xmlFields[name].GetCustomerProperty("TextField");
                    Guid textFieldId = Guid.Empty;
                    if (customProperty != null)
                    {
                        textFieldId = new Guid(customProperty.ToString());
                        if (textFieldId == Id)
                        {
                            metadataFieldName = name;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public void LoadExistLookupFields()
        {
            AddExistLookupFields(mAveSPList.SPList.Fields);
        }

        public void BackupValidationFields()
        {
            if (mValidationFields != null || mAveSPList == null || mAveSPList.SPList == null)
            {
                return;
            }
            mValidationFields = new Dictionary<Guid, string>();
            for (int i = mAveSPList.SPList.Fields.Count - 1; i >= 0; i--)
            {
                IAveField field = mAveSPList.SPList.Fields[i];
                if (!string.IsNullOrEmpty(field.ValidationFormula))
                {
                    mValidationFields.Add(field.ID, field.ValidationFormula);
                    field.ValidationFormula = string.Empty;
                    field.Update();
                }
            }
        }

        public void RestoreValidationFields()
        {
            if (mValidationFields == null || mValidationFields.Count == 0)
            {
                return;
            }
            foreach (Guid fieldId in mValidationFields.Keys)
            {
                IAveField field = AveFieldHelper.FindFieldInCollection(fieldId, FieldCollection);
                if (field != null)
                {
                    field.ValidationFormula = mValidationFields[fieldId];
                    field.Update();
                }
            }
        }

        //如果list上的field设置了enforce unique value，并且设置了default value时，在创建item时会抛出SPDuplicateValuesFoundException异常。
        public void BackupFieldsDefaultValue()
        {
            if (mFieldsDefaultValue != null || mAveSPList == null || mAveSPList.SPList == null)
            {
                return;
            }
            mFieldsDefaultValue = new Dictionary<Guid, string>();
            foreach (IAveField field in mAveSPList.SPList.Fields)
            {
                //if (field.TypeAsString.Equals("TaxonomyFieldType") || field.TypeAsString.Equals("TaxonomyFieldTypeMulti"))
                //{
                if (field.EnforceUniqueValues && !string.IsNullOrEmpty(field.DefaultValue))
                {
                    mFieldsDefaultValue.Add(field.ID, field.DefaultValue);
                }
                //}
            }
            foreach (Guid fieldId in mFieldsDefaultValue.Keys)
            {
                IAveField field = mAveSPList.SPList.Fields[fieldId];
                field.DefaultValue = string.Empty;
                field.Update();
            }
        }

        public Dictionary<string, object> GetFieldValues(string docName, int docRowId, int version, Dictionary<string, object> data)
        {
            return GetFieldValues(docName, docRowId, version, data, mCreateFieldIfNotExist, false);
        }

        public Dictionary<string, object> GetFieldValues(string docName, int docRowId, int version, Dictionary<string, object> data, bool getAveField)
        {
            return GetFieldValues(docName, docRowId, version, data, mCreateFieldIfNotExist, getAveField);
        }

        public Dictionary<string, object> GetFieldValues(string docName, int docRowId, int version, Dictionary<string, object> data, bool createFieldIfNotExist, bool getAveField)
        {
            Dictionary<string, object> newData;
            Dictionary<string, object> uniqueFieldValues;
            GetFieldValues(docName, docRowId, version, data, createFieldIfNotExist, getAveField, out newData, out uniqueFieldValues);
            return newData;
        }

        private bool CheckIntegrity(object value, IAveList list, AveFieldType type,out PostActionType postType)
        {
            bool result = false;
            postType = PostActionType.None;
            if (null == value)
            {
                return result;
            }

            switch (type)
            {
                case AveFieldType.URL:
                    //TODO action, only support url begin with list attachment folder
                    var explicitValue = value as IAveFieldUrlValue;
                    if (list.BaseType == AveBaseType.GenericList)
                    {
                        if(AttachmentUrlUtility.IsCurrentListAttachmentUrl(explicitValue.Url, list))
                       // if (explicitValue.Url.StartsWith((list.RootFolder.ServerRelativeUrl + "/Attachments/"), StringComparison.OrdinalIgnoreCase))
                        {
                            postType = PostActionType.ListPostAction;
                            result = true;
                        }
                        else if (AttachmentUrlUtility.IsAttachmentUrl(explicitValue.Url))
                        {
                            postType = PostActionType.SitePostAction;
                            result = true;
                        }
                    }
                    //if (PermissionLinkUtility.IsListPermissionLink(explicitValue.Url))
                    //{
                    //    postType = PostActionType.SitePostAction;
                    //    result = true;
                    //}
                    break;
                case AveFieldType.Note:
                    if (list.BaseType == AveBaseType.GenericList)
                    {
                        postType = NoteFieldValueHandler.GetRichTextPostActionType((string)value, mAveSPList.SPList);
                        result = postType != PostActionType.None;
                    }
                    break;
                default:
                    break;
            }
            return result;
        }

        public void GetFieldValues(string docName, int docRowId, int version, Dictionary<string, object> data, bool getAveField, out Dictionary<string, object> userData, out Dictionary<string, object> uniqueFieldValues)
        {
            GetFieldValues(docName, docRowId, version, data, mCreateFieldIfNotExist, getAveField, out userData, out uniqueFieldValues);
        }

        public void GetFieldValues(string docName, int docRowId, int version, Dictionary<string, object> data, bool createFieldIfNotExist, bool getAveField, out Dictionary<string, object> userData, out Dictionary<string, object> uniqueFieldValues)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPListFieldCollection.GetFieldValues"))
            {
#endif
            Dictionary<string, object> newdata = new Dictionary<string, object>();
            Dictionary<string, object> tempUniqueValues = new Dictionary<string, object>();
            IAveContentTypeId itemContentTypeId = null;
            #region get field value
            foreach (var nullValue in FieldMapping.NullToDefaultValueMapping)
            {
                if (mXmlFields.ContainsKey(nullValue.Key) && !data.ContainsKey(nullValue.Key))
                {
                    data.Add(nullValue.Key, nullValue.Value);
                }
            }
            foreach (KeyValuePair<string, object> kv in data)
            {
                log.Debug("Begin to handle field value:[{0}],[{1}]", kv.Key, kv.Value);
                try
                {
                    string key = kv.Key;
                    object value = kv.Value;
                    bool IsSetByCustomMapping = false;
                    bool IsValueSetByCustomMapping = false;
                    if (key == "#tp_ContentTypeId")
                    {
                        key = "ContentType";
                    }
                    else if (key == "#tp_ItemOrder")
                    {
                        key = "Order";
                    }

                    if (key.Equals("MasterSeriesItemID") && mAveSPList.SPList.BaseTemplate == AveListTemplateType.Events) //CI-19064
                    {
                        int itemId = Convert.ToInt32(value);
                        if (mAveParentSite.MappingManager.SiteMappingManager.ItemIdMapping.ContainsKey(mAveSPList.SPList.ID)
                            && mAveParentSite.MappingManager.SiteMappingManager.ItemIdMapping[mAveSPList.SPList.ID].ContainsKey(itemId))
                        {
                            value = mAveParentSite.MappingManager.SiteMappingManager.ItemIdMapping[mAveSPList.SPList.ID][itemId];
                        }
                    }

                    if (key.Equals("ProjectWebGuid", StringComparison.OrdinalIgnoreCase))
                    {
                        mAveParentSite.MappingManager.SiteMappingManager.AddProjectWebGuidMapping(mAveSPList.SPList.ID, docRowId, new Guid(value.ToString()));
                        continue;
                    }

                    if ((NO_RESTORE_FIELD_MAP.Contains(key) || key.StartsWith(AveConstants.FIELD_SEPARATOR, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    string fieldName = key;
                    if (fieldName.Contains("#"))
                    {
                        continue;
                        // fieldName = fieldName.Substring(0, fieldName.IndexOf("#", StringComparison.OrdinalIgnoreCase));
                    }
                    //如果是taxonomy field关联的Text Field，将fieldName设置成对应taxonomy field的Name
                    //我们还原taxonomy field的value是通过对应Text Field上的value来还原的
                    if (SourceTextTaxonomyDic.ContainsKey(fieldName))
                    {
                        fieldName = SourceTextTaxonomyDic[fieldName];
                    }
                    IAveField spField = null;
                    try
                    {
                        spField = EnsureField(fieldName, createFieldIfNotExist, mAveSPList.SPList.Fields, mXmlFields);
                        log.Debug("Find field,[{0}][{1}]", spField?.Title, spField?.TypeAsString);
                    }
                    catch (AveSchemaDependencyConflictException e)
                    {
                        log.Log(AveLogLevel.WARN, string.Format("there is a field SchemaDependencyConflict with destination,field:{0}, error:{1}", kv.Key, e.ToString()));
                        spField = mAveSPList.SPList.Fields[e.SchemaDependencyName];
                    }
                    catch (AveSchemaDependencyNotFoundException e)
                    {
                        log.Log(AveLogLevel.WARN, string.Format("there is a field SchemaDependencyNotFound in destination,field:{0}, error:{1}", kv.Key, e.ToString()));
                    }
                    if (spField == null)
                    {
                        continue;
                    }
                    //过滤还原WorkflowStatus类型field的value。
                    if (spField.TypeAsString == "WorkflowStatus")
                    {
                        continue;
                    }

                    #region 放到NO_RESTORE_FIELD_MAP中处理了。
                    //DOC-67843
                    //report metadata 下的item的这个column指向的是report template下面的doc的guid，这里不能使用源端数据
                    //if (fieldName.Equals("_dlc_Reporting_TemplateId"))
                    //{
                    //    continue;
                    //}
                    //hold或者record的item不能还原这个属性，这个属性在还原hold or record会自动还原。
                    //if (fieldName.Equals("_vti_ItemHoldRecordStatus"))
                    //{
                    //    continue;
                    //}
                    #endregion

                    //如果是配置了CustomMapping的field，若配置了ValueMapping，需要获取
                    if (mXmlFields.ContainsKey(fieldName) && mXmlFields[fieldName].CustomFieldInfo != null)
                    {
                        //value = GetCustomMappingValue(value.ToString(), mXmlFields[fieldName].Title, spField);
                        string realvalue = value.ToString();
                        int realvalueIndex = realvalue.IndexOf(';');
                        if (spField.Type == AveFieldType.Lookup && realvalueIndex > 0)
                        {
                            value = realvalue.Substring(0, realvalueIndex);
                        }
                        value = GetCustomMappingValue(value, mXmlFields[fieldName], spField, docName, ref IsSetByCustomMapping, ref IsValueSetByCustomMapping);
                        if (spField.Type == AveFieldType.Lookup && !IsSetByCustomMapping && !IsValueSetByCustomMapping)
                        {
                            value = realvalue;
                        }
                        log.Info("mapping column value from [{0}] to [{1}]", realvalue, value);
                    }

                    //target audience value need to replace by audience mapping
                    if (fieldName.Equals("Target_x0020_Audiences", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            value = (object)ReplaceAudienceId(mAveParentSite.MappingManager.SiteMappingManager.AudienceIDMapping, value.ToString());
                        }
                        catch (Exception e)
                        {
                            log.Warn("Replace item audience id error. Exception:{0}.", e.ToString());
                        }
                    }
                    // 处理field mapping的情况
                    //AveFieldMappingInfo fieldMappingInfo = null;
                    //if (mFieldMapping != null && mFieldMapping.AveMappedFields.ContainsKey(spField.InternalName))
                    //{
                    //    fieldMappingInfo = mFieldMapping.AveMappedFields[spField.InternalName];
                    //    if (fieldMappingInfo.ValueMapping.ContainsKey(kv.Value))
                    //    {
                    //        value = fieldMappingInfo.ValueMapping[kv.Value];
                    //    }
                    //}
                    #region Format nintex form field value
                    if (key == "NFFormData")
                    {
                        var contentTypeId = GetContentTypeId((byte[])data["#tp_ContentTypeId"]);
                        if (contentTypeId != null)
                        {
                            var formater = new NintexFormValueFormat(spField, this.mAveSPList, contentTypeId.ToString(), version);
                            value = formater.CheckFieldValue(value);
                        }
                    }
                    #endregion


                    if (!NEED_RESTORE_FIELD_MAP.Contains(spField.InternalName))
                    {
                        if (FilterCheck(spField))
                        {
                            log.Log(AveLogLevel.INFO, string.Format("Filter restore field:{0}.", spField.Title));
                            //mLog.Info("Filter restore field:{0}", spField.Title);
                            continue;
                        }
                    }
                    if (key == "Modified_x0020_By" || key == "Created_x0020_By")
                    {
                        IAveUser user = null;
                        try
                        {
                            //Dictionary<string, string> userMap = mAveParentSite.SPMembers.UserAndDomainMapping.EnumCustomUserMapping().ToDictionary(pair => pair.Key, pair => pair.Value);
                            string mappingValue = mAveParentSite.SPMembers.GetMappingUserLogin(@value.ToString());
                            if (!String.IsNullOrEmpty(mappingValue))
                            {
                                user = mAveParentSite.SPSite.RootWeb.SiteUsers[mappingValue];
                            }
                            if (user == null)
                            {
                                user = mAveParentSite.SPSite.RootWeb.SiteUsers[@value.ToString()];
                            }
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetUserByUrlError, e.ToString());
                        }//不需要捕获异常
                        if (user == null)
                        {
                            value = mAveParentSite.SPSite.RootWeb.SiteUsers.GetByID(mAveParentSite.CURRENT_USER_ID).LoginName;
                        }
                    }
                    if (key == "ContentType")
                    {
                        //string ctId = AveMOSSUtility.GetSPContentTypeIdFromByte((byte[])data["#tp_ContentTypeId"]).ToString();
                        string ctId = AveConvert.ConvertByteToContentTypeId(mAveParentSite.ObjectModelFactory, (byte[])data["#tp_ContentTypeId"]).ToString();
                        if (mAveSPList.ParentSite.MappingManager.ListMappingManager.ListLevelCTIdMapping.ContainsKey(ctId))
                        {
                            value = mAveSPList.ParentSite.MappingManager.ListMappingManager.ListLevelCTIdMapping[ctId];
                        }
                        else
                        {
                            #region CT不再反插
                            //if (mAveSPList.AveContentTypes.ContentTypeMap.ContainsKey(ctId))
                            //{
                            //    mAveSPList.AveContentTypes.Restore(mAveSPList.AveContentTypes.ContentTypeMap[ctId], new AveRestoreOption(0));
                            //    if (mAveSPList.ListLevelCTMapping.ContainsKey(ctId))
                            //    {
                            //        value = mAveSPList.ListLevelCTMapping[ctId].Name;
                            //    }
                            //}
                            //else
                            //{
                            //    SPContentTypeId tempId = mAveSPList.SPList.ContentTypes.BestMatch(new SPContentTypeId(ctId));
                            //    SPContentType tempCT = mAveSPList.SPList.ContentTypes[tempId];
                            //    if (tempCT != null)
                            //    {
                            //        value = tempCT.Name;
                            //    }
                            //}
                            #endregion

                            //源端为普通ContentType，目的端为ConnectorList时，此时ContentType赋值为connector的默认ContentType而不是通过BestMatch去获取
                            if (mAveSPList.SPList.TemplateFeatureId == new Guid(AveWrapperConstants.AVEFSDLFEATRUEID) || mAveSPList.SPList.TemplateFeatureId == new Guid(AveWrapperConstants.AVEVDLFEATRUEID))
                            {
                                if (mAveSPList.SPList.ContentTypes[mAveParentSite.ObjectModelFactory.CreateContentTypeId(ctId)] != null)
                                {
                                    value = mAveSPList.SPList.ContentTypes[mAveParentSite.ObjectModelFactory.CreateContentTypeId(ctId)].ID;
                                }
                                else
                                {
                                    if (mAveSPList.SPList.TemplateFeatureId == new Guid(AveWrapperConstants.AVEFSDLFEATRUEID))
                                    {
                                        foreach (IAveContentType contentType in mAveSPList.SPList.ContentTypes)
                                        {
                                            if (contentType.ID != null && contentType.ID.ToString().StartsWith("0x01010003F8831469804144AE3F259EF433E9EB", StringComparison.OrdinalIgnoreCase))
                                            {
                                                value = contentType.ID;
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        foreach (IAveContentType contentType in mAveSPList.SPList.ContentTypes)
                                        {
                                            if (contentType.ID != null && contentType.ID.ToString().StartsWith("0x010100806213320A313D4DA11D1B1D6CC700CF", StringComparison.OrdinalIgnoreCase))
                                            {
                                                value = contentType.ID;
                                                break;
                                            }
                                        }
                                    }
                                }
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
                        }
                        itemContentTypeId = value as IAveContentTypeId;
                    }

                    #region Replace the url in "RoutingTargetPath" for Content Organizer Rule list
                    if (key == "RoutingTargetPath" && spField is IAveFieldText)
                    {
                        //we only need to change patch when location is in the same site, otherwise keep the original data.
                        if (data.ContainsKey("RoutingTargetLibrary") && data["RoutingTargetLibrary"] != null)
                        {
                            string sourceDir = value.ToString();
                            value = ChangeServerRelativeUrl(key, sourceDir);
                        }
                    }
                    #endregion

                    /* add for publishing site */
                    //value = ChangeServerRelativeUrl(key.ToString(), value.ToString());
                    /* end */


                    if (AveBuiltInFieldId.ContentTypeId == spField.ID)
                    {
                        //TODO:Get the content type id from list content type mapping
                        //value = new SPContentTypeId(AveConverter.BytesToString((byte[])value));
                        continue;
                    }
                    else if (spField.TypeAsString == "TargetTo")
                    {
                        string tempValue = value.ToString();
                        // TODO... find a way to solve audience mapping
                        //if (mAveSPWeb.ParentSite.AudienceIDMapping != null)
                        //{
                        //    value = AveAudienceManager.ReplaceAudienceId(mAveSPWeb.ParentSite.AudienceIDMapping, tempValue);
                        //}
                    }
                    else if (spField.TypeAsString == "SendTo")
                    {
                        if (value != null)
                        {
                            StringBuilder tempValue = new StringBuilder();
                            string[] principalInfos = value.ToString().Split(new string[] { ";#" }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string tempInfo in principalInfos)
                            {
                                int UserIdSrc = -1;
                                bool result = int.TryParse(tempInfo, out UserIdSrc);
                                if (result)
                                {
                                    IAvePrincipal UserIdDest = mAveSPList.ParentWeb.ParentSite.SPMembers.FindMember(UserIdSrc, true);
                                    if (UserIdDest.LoginName != null)
                                        tempValue.Append(UserIdDest.ID + ";#" + UserIdDest.LoginName + ";#");
                                }
                            }
                            try
                            {
                                value = tempValue.ToString(0, tempValue.Length - 2);
                            }
                            catch (Exception ex)
                            {
                                log.Warn("Restoring field : {0} error,FieldType : {1},List : {2} \r\tErrorMessage:{3}", spField.InternalName, spField.TypeAsString, spField.ParentList.Title, ex.ToString());
                            }
                        }
                    }
                    else if (spField.InternalName == "TemplateUrl")
                    {
                        ReplaceOption option = new ReplaceOption(true);
                        option.NeedReplaceAbsoluteUrl = true;
                        value = AveReplaceProcessor.UrlReplace(value.ToString(), mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, option, mAveParentSite.SourceSiteInfo, mAveParentSite.ServerRelativeUrl);
                    }
                    else if (spField is IAveFieldUser)
                    {
                        //如果做了column mapping mapping user在目的端不存在就不还原了
                        if (mXmlFields.ContainsKey(fieldName) && mXmlFields[fieldName].CustomFieldInfo != null)
                        {
                            int UserIdSrc = -1;
                            int.TryParse(value.ToString(), out UserIdSrc);
                            if (UserIdSrc == -1)
                            {
                                continue;
                            }
                            value = UserIdSrc;
                            log.Info("Convert user value to Int32:{0}", UserIdSrc);
                        }
                        else
                        {
                            if ((spField as IAveFieldUser).AllowMultipleValues == false)
                            {
                                int principalId = Convert.ToInt32(value);
                                value = mAveSPList.ParentWeb.ParentSite.SPMembers.FindMemberId(principalId);
                            }
                            else
                            {
                                if (value != null)
                                {
                                    StringBuilder tempValue = new StringBuilder();
                                    string[] principalInfos = value.ToString().Split(new string[] { ";#" }, StringSplitOptions.RemoveEmptyEntries);
                                    foreach (string tempInfo in principalInfos)
                                    {
                                        int UserIdSrc = -1;
                                        bool result = int.TryParse(tempInfo, out UserIdSrc);
                                        if (result)
                                        {
                                            IAvePrincipal UserIdDest = mAveSPList.ParentWeb.ParentSite.SPMembers.FindMember(UserIdSrc, true);
                                            if (UserIdDest.LoginName != null)
                                                tempValue.Append(UserIdDest.ID + ";#" + UserIdDest.LoginName + ";#");
                                        }
                                    }
                                    try
                                    {
                                        value = tempValue.ToString(0, tempValue.Length - 2);
                                    }
                                    catch (Exception ex)
                                    {
                                        log.Warn("Restoring field : {0} error,FieldType : {1},List : {2} \r\tErrorMessage:{3}", spField.InternalName, spField.TypeAsString, spField.ParentList.Title, ex.ToString());
                                    }
                                }
                            }
                        }
                    }
                    else if (spField is IAveFieldUrl)
                    {
                        value = GenerateUrlFieldValue(key, value, data);
                        //value = GetUrlValue(key, value, data, docRowId, spField);
                        if (mAveSPList.SPList != null)
                        {
                            PostActionType postType;
                            if (CheckIntegrity(value, mAveSPList.SPList, spField.Type, out postType))
                            {
                                mAveParentSite.FieldPostCache.AddCache(mAveSPWeb.SPWeb.ID, mAveSPList.SPList.ID, docRowId, version, fieldName, value, postType);
                                // mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.AddNotUpdateDenpendentFieldValue((value as IAveFieldUrlValue).Url, mAveSPWeb.SPWeb.ID, mAveSPList.SPList.ID, docRowId, version, spField.InternalName, value);
                            }
                            else
                            {
                                value = ReplaceUrlValue(value as IAveFieldUrlValue, docRowId, spField);
                            }
                        }
                    }
                    else if (spField.ID == new Guid("{E1FA3211-0188-4a95-A737-8775782CBAC0}")  //routingaufoldersettings
                            || spField.ID == new Guid("{FF4470AE-85D6-49ab-A501-E5772848F6C7}"))//routingconditions
                    {
                        //make sure these fields will not be proccessed by the following branches
                    }
                    else if (spField.TypeAsString.Equals("Note", StringComparison.OrdinalIgnoreCase) && spField.ID == AveBuiltInFieldId.SharedWithDetails)
                    {
                        value = ReplaceSharedWithDetails(value);
                    }
                    else if ((spField.TypeAsString.Equals("Note", StringComparison.OrdinalIgnoreCase) && spField.ID != AveBuiltInFieldId.RecurrenceData))
                    {
                        if (spField is IAveFieldMultiLineText && !(spField as IAveFieldMultiLineText).RichText)
                        {
                            value = value.ToString();
                        }
                        else
                        {
                            value = value.ToString();
                            PostActionType postType;
                            if (CheckIntegrity(value, mAveSPList.SPList, AveFieldType.Note, out postType))
                            {
                                mAveParentSite.FieldPostCache.AddCache(mAveSPWeb.SPWeb.ID, mAveSPList.SPList.ID, docRowId, version, fieldName, value, postType);
                            }
                            else
                            {
                                value = ReplaceXmlLinks(spField.Title, value.ToString());
                            }
                        }
                    }
                    else if (spField.TypeAsString.Equals("HTML", StringComparison.OrdinalIgnoreCase) || spField.TypeAsString.Equals("Link", StringComparison.OrdinalIgnoreCase) ||
                             spField.TypeAsString.Equals("Image", StringComparison.OrdinalIgnoreCase) ||
                             spField.TypeAsString.Equals("SummaryLinks", StringComparison.OrdinalIgnoreCase) || spField.TypeAsString.Equals("MediaFieldType", StringComparison.OrdinalIgnoreCase))
                    {
                        value = ReplaceXmlLinks(spField.Title, value.ToString());
                    }
                    else if (spField.TypeAsString.Equals("Thumbnail", StringComparison.OrdinalIgnoreCase))
                    {
                        if (value is string str && !string.IsNullOrWhiteSpace(str))
                        {
                            var jObj = JObject.Parse(str);
                            jObj.Remove("id");
                            value = jObj.ToString(Newtonsoft.Json.Formatting.None);
                        }
                    }
                    else if (spField.TypeAsString == "TaxonomyFieldTypeMulti" || spField.TypeAsString == "TaxonomyFieldType")
                    {
                        if (value is string)
                        {
                            //if (fieldMappingInfo != null)
                            //{
                            //    if (fieldMappingInfo.IgnoreType && fieldMappingInfo.IsTaxonomyField)
                            //    {
                            //        ////默认的term分隔符是';',如果不是';',在此替换成';'
                            //        //if (fieldMappingInfo.SplitChar != ";")
                            //        //{
                            //        //    value = value.ToString().Replace(fieldMappingInfo.SplitChar, ";");
                            //        //}
                            //        ////默认的层次分隔符是'<', 如果不是'<',在此替换成'<'
                            //        //if (fieldMappingInfo.HiberarchyChar != "<")
                            //        //{
                            //        //    value = value.ToString().Replace(fieldMappingInfo.HiberarchyChar, "<");
                            //        //}
                            //        value = fieldMappingInfo.FilterSpecialChars(value.ToString());
                            //    }
                            //    else
                            //    {
                            //        continue;
                            //    }
                            //}
                        }
                        else
                        {
                            //ArrayList list = new ArrayList();
                            //list.Add(Convert.ToInt32(value));
                            //mAveSPWeb.ParentSite.AddNotUpdateLookupFieldValue(mAveSPWeb.SPWeb.ID, mAveSPList.SPList.ID, docRowId, version, spField.ID, list);
                            continue;
                        }
                    }
                    else if (spField.Type == AveFieldType.Lookup && !IsSetByCustomMapping)
                    {
                        try
                        {
                            string realvalue = value.ToString();
                            int idValue = 0;
                            String[] lookupArray = new String[0];
                            //截取 rowId 和 realvalue
                            if (realvalue.Contains(";#"))
                            {
                                lookupArray = SplitString(value.ToString(), ";#");
                            }
                            else if (realvalue.Contains(";"))
                            {
                                lookupArray = SplitString(value.ToString(), ";");
                            }

                            if (lookupArray.Length >= 2)
                            {
                                int rowId = Convert.ToInt32(lookupArray[0]);
                                idValue = rowId;
                                if (rowId > 0)
                                {
                                    //找到 ； or ;#
                                    realvalue = lookupArray[1];
                                }
                                else
                                {
                                    log.Warn("lookup value {0} is not in correct foramt.Set realValue to empty.", realvalue);
                                    realvalue = "";
                                }
                            }
                            else
                            {
                                try
                                {
                                    //没找到； or ;#,证明就是Row ID
                                    idValue = Convert.ToInt32(realvalue);
                                }
                                catch (Exception ex)
                                {
                                    log.Warn($"lookup value {realvalue} cann't convert to lookup id. Set realValue to empty.");
                                    realvalue = "";
                                }
                            }

                            bool hasFound = false;
                            Guid listId = Guid.Empty;//new Guid(((IAveFieldLookup)spField).LookupList);
                            AveLookupObject obj = mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.GetLookupFieldMapping(mAveSPList.SPList.ID, spField.ID);
                            IAveFieldLookup fieldLookup = spField as IAveFieldLookup;
                            if (fieldLookup != null && fieldLookup.InternalName.Equals("TaxCatchAll"))
                            {
                                //SAAS-41072 Skip to restore TaxCatchAll
                                continue;
                            }
                            if (obj == null && fieldLookup != null && AveTypeHelper.IsGuid(fieldLookup.LookupList))
                            {
                                log.Log(AveLogLevel.WARN, "AveFieldRT001009", new string[] { spField.Title });
                                int itemId = mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.GetMappingItemId(new Guid(fieldLookup.LookupList), idValue);
                                if (itemId != -1)
                                {
                                    value = itemId.ToString();
                                    newdata[spField.InternalName] = value;
                                    if (spField.EnforceUniqueValues)
                                    {
                                        tempUniqueValues[spField.InternalName] = value;
                                    }
                                    //continue;
                                    hasFound = true;
                                }
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(obj.List) || !AveSPUtility.IsGuid(obj.List))
                                {
                                    listId = obj.ListId;
                                }
                                else
                                {
                                    listId = mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.GetListIdMapping(new Guid(obj.List));
                                }
                                if (!listId.Equals(Guid.Empty) && !String.IsNullOrEmpty(value.ToString()))
                                {
                                    int itemId = mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.GetMappingItemId(listId, idValue);
                                    if (itemId != -1)
                                    {
                                        value = itemId.ToString();
                                        newdata[spField.InternalName] = value;
                                        if (spField.EnforceUniqueValues)
                                        {
                                            tempUniqueValues[spField.InternalName] = value;
                                        }
                                        //continue;
                                        hasFound = true;
                                    }
                                }
                            }
                            if (!hasFound && obj != null && !IsValueSetByCustomMapping)
                            {
                                //在ID Mapping 找不到的情况下，尝试使用 column value 进行匹配 ，若匹配则返回得到的item ID 
                                try
                                {
                                    string internalName = fieldLookup.LookupField;
                                    IAveList lookupList;
                                    Guid objList = AveSPUtility.IsGuid(obj.List) ? new Guid(obj.List) : obj.ListId;//new Guid(obj.List);
                                    Dictionary<string, int> lookupIDValue = null;
                                    if (!mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.TryGetLookupListValueMapping(objList, internalName, out lookupIDValue))
                                    {
                                        lookupIDValue = new Dictionary<string, int>();

                                        // End User or item level Restore don't query lookup list.
                                        if (WrapperConfiguration.WrapperConfigurationForBPOS.IsEndUserRestore
                                            || WrapperConfiguration.WrapperConfigurationForBPOS.HasItemLevelNode
                                            || WrapperConfiguration.WrapperConfigurationForBPOS.SkipCacheLookColumn)
                                        {
                                            mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.AddLookupListValueMapping(objList, internalName, lookupIDValue);
                                        }
                                        else if (!mAveSPList.IsLookupListValid(string.Concat(fieldLookup.LookupWebId, "-", obj.ListTitle)))
                                        {
                                            if (fieldLookup.LookupWebId == Guid.Empty)
                                            {
                                                lookupList = this.mAveSPWeb.ParentSite.SPSite.OpenWeb(fieldLookup.ParentList.ParentWeb.ID).GetListByTitle(obj.ListTitle);
                                            }
                                            else
                                            {
                                                lookupList = this.mAveSPWeb.ParentSite.SPSite.OpenWeb(fieldLookup.LookupWebId).GetListByTitle(obj.ListTitle);
                                            }

                                            //mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.AddListIdMapping(objList, lookupList.ID);
                                            if (lookupList != null)
                                            {
                                                foreach (IAveListItem item in lookupList.GetItemsLightly(internalName))
                                                {
                                                    if (item[internalName] != null)
                                                    {
                                                        var fieldvalue = item[internalName].ToString();
                                                        lookupIDValue[fieldvalue] = item.ID;
                                                        log.Info($"[SAAS-38545]Success to find this field:{internalName} by get items lightly in target lookup list:{obj.ListTitle}, field value:{fieldvalue}, item id:{item.ID}");
                                                    }
                                                }
                                                mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.AddLookupListValueMapping(objList, internalName, lookupIDValue);
                                                
                                            }
                                            else
                                            {
                                                mAveSPList.AddInvalidLookupListTitle(string.Concat(fieldLookup.LookupWebId, "-", obj.ListTitle));
                                            }
                                        }
                                        else
                                        {
                                            log.Warn("Failed to find the list by title:{0} , when restore the lookup column:{1}", obj.ListTitle, fieldLookup.InternalName);
                                        }
                                    }
                                    int newID;
                                    if (lookupIDValue != null && lookupIDValue.TryGetValue(realvalue, out newID))
                                    {
                                        value = newID.ToString();
                                        newdata[spField.InternalName] = value;
                                        //Guid lookupListID = mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.ListIdMapping[objList];
                                        if (spField.EnforceUniqueValues)
                                        {
                                            tempUniqueValues[spField.InternalName] = value;
                                        }
                                        //mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.AddItemIdMapping(lookupListID, idValue, newID);
                                        hasFound = true;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    log.Warn("Lookup Column : {0} .Lookup List : {1} .Value Mapping failed : {2}", fieldLookup.InternalName, obj.List, ex);
                                    hasFound = false;
                                }
                            }
                            if (!hasFound && !String.IsNullOrEmpty(value.ToString()))
                            {
                                AveLookupFieldInfo fieldInfo = new AveLookupFieldInfo();
                                if (obj != null)
                                {
                                    fieldInfo.LookupListID = AveSPUtility.IsGuid(obj.List) ? new Guid(obj.List) : obj.ListId;
                                }
                                else if (fieldLookup != null && AveTypeHelper.IsGuid(fieldLookup.LookupList))
                                {
                                    fieldInfo.LookupListID = new Guid(fieldLookup.LookupList);
                                }
                                fieldInfo.LookupFieldID = spField.ID;
                                fieldInfo.Version = version;
                                ArrayList list = new ArrayList();
                                //SAAS-24888:此处value的值为:1;#1 这种类型，所以发生类型转换异常.
                                //list.Add(Convert.ToInt32(value));
                                list.Add(idValue);
                                fieldInfo.LookupFieldValue = list;
                                if (mLookupFieldIdValue == null)
                                {
                                    mLookupFieldIdValue = new List<AveLookupFieldInfo>();
                                }
                                mLookupFieldIdValue.Add(fieldInfo);
                                continue;

                            }
                            //else if (docRowId != -1 && !String.IsNullOrEmpty(value.ToString()))
                            //{
                            //    ArrayList list = new ArrayList();
                            //    list.Add(Convert.ToInt32(value));
                            //    mAveSPWeb.ParentSite.AddNotUpdateLookupFieldValue(new Guid(obj.List), mAveSPWeb.SPWeb.ID, mAveSPList.SPList.ID, docRowId, version, spField.ID, list);
                            //}
                        }
                        catch (Exception e)
                        {
                            log.Warn("An error occurred while getting look up field:{0} with value:{1}. Error message:{2}", key, value, e);
                        }
                    }
                    else if (spField.Type == AveFieldType.DateTime)
                    {
                        //if (spField.ID == SPBuiltInFieldId.Created || spField.ID == SPBuiltInFieldId.Modified)
                        //{
                        // value = mAveSPWeb.SPWeb.RegionalSettings.TimeZone.UTCToLocalTime(Convert.ToDateTime(value, System.Globalization.DateTimeFormatInfo.InvariantInfo));
                        value = Convert.ToDateTime(value, System.Globalization.DateTimeFormatInfo.InvariantInfo);
                        //}
                    }
                    //现在所有的调用此开关都是True，需要UpdateFields去判断一下类型是否是Url类型然后转换处理一下
                    if (getAveField)
                    {
                        var aveField = new AveFieldValueInfo { ColValue = value, Id = spField.ID, ColName = spField.ColName, FieldType = spField.Type, RowOrdinal = spField.RowOrdinal };
                        if (spField is IAveFieldUrl)
                        {
                            var fieldUrlValue = value as IAveFieldUrlValue;
                            if (fieldUrlValue.Url.IndexOf("wrkstat.aspx", StringComparison.OrdinalIgnoreCase) > 0)
                            {
                                const string workflowPattern = @"(?<=wrkstat.aspx\?List=)([A-F0-9]{8}-([A-F0-9]{4}-){3}[A-F0-9]{12})&WorkflowInstanceName=([A-F0-9]{8}-([A-F0-9]{4}-){3}[A-F0-9]{12})";
                                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(workflowPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                if (regex.IsMatch(fieldUrlValue.Url))
                                {
                                    continue;
                                }
                            }
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
                            if (spField.EnforceUniqueValues)
                            {
                                tempUniqueValues[spField.InternalName] = value;
                            }
                            aveField.ColValue = fieldUrlValue.Url;
                        }
                        else if (kv.Key.Equals("#tp_ContentTypeId"))
                        {
                            aveField.ColName = "tp_ContentTypeId";
                        }
                        else
                        {
                            aveField.ColName = spField.ColName;
                            //AveAssemblyUtility.GetPropertyValue(spField, ColName).ToString();
                            aveField.RowOrdinal = spField.RowOrdinal;
                        }
                        aveField.FieldType = spField.Type;
                        value = aveField;
                    }
                    //此处已没有必要再替换url，在SPFieldUrl的时候已经做了替换。
                    //if (key.Equals("PublishingPageLayout", StringComparison.CurrentCultureIgnoreCase) || key.Equals("PublishingPageImage", StringComparison.CurrentCultureIgnoreCase))
                    //{
                    //    //value = ChangeServerRelativeUrl(spField.InternalName, value.ToString());
                    //    value = AveReplaceProcessor.UrlReplace(value.ToString(), mAveSPWeb.ParentSite.SiteManagedMappings, new ReplaceOption(true)); 
                    //}
                    newdata[spField.InternalName] = value;
                    if (spField.EnforceUniqueValues)
                    {
                        tempUniqueValues[spField.InternalName] = value;
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while get field values. key:{0}, value:{1}\n error message:{2}", kv.Key, kv.Value, e));
                }
            }
            #endregion
            //替换field value中的链接
            //ReplaceFieldLinks(newdata);

            //SetExcelFieldValue
            if (!String.IsNullOrEmpty(ExcelImportPath) && data.Count > 0 && data["#tp_ID"] != null)
            {
                SetExcelFieldValue(newdata, Convert.ToInt32(data["#tp_ID"]), version, getAveField, itemContentTypeId);
            }

            //应该是同时包含ViewGuid和ViewName需要添加到KPI Mapping中            
            //check contenttype instead of viewname for compatible reason,more details here:
            if (data.ContainsKey("ViewGuid")
                && itemContentTypeId != null
                && itemContentTypeId.IsChildOf(AveSystemContentTypeId.SharePointListbasedStatusIndicator)
                && !mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.KpiListNeedUpdate.ContainsKey(mAveSPList.SPList.ID))
            {
                mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.KpiListNeedUpdate.Add(mAveSPList.SPList.ID, mAveSPWeb.SPWeb.ID);
            }
            userData = newdata;
            uniqueFieldValues = tempUniqueValues;
            //return newdata;
#if PerformanceLog
            }
#endif
        }

        /// <summary>
        /// 字符串分割
        /// </summary>
        /// <param name="str"></param>
        /// <param name="splitStr"></param>
        /// <returns></returns>
        private String[] SplitString(string str, string splitStr)
        {
            if (String.IsNullOrWhiteSpace(str) || String.IsNullOrWhiteSpace(splitStr)) return new String[0];
            return Regex.Split(str, splitStr, RegexOptions.IgnoreCase);
        }

        private IAveContentTypeId GetContentTypeId(byte[] contentTypeId)
        {
            using (new AvePerformanceScope("Restore.AveSPListFieldCollection.GetContentTypeId"))
            {
                string ctId = AveConvert.ConvertByteToContentTypeId(this.mAveParentSite.ObjectModelFactory,
                    contentTypeId).ToString();
                if (this.mAveParentSite.MappingManager.ListMappingManager.ListLevelCTIdMapping.ContainsKey(ctId))
                {
                    return this.mAveParentSite.MappingManager.ListMappingManager.ListLevelCTIdMapping[ctId];
                }
                IAveContentType ct;
                //CT不再反插 
                //源端为普通ContentType，目的端为ConnectorList时，此时ContentType赋值为connector的默认ContentType而不是通过BestMatch去获取
                if (this.mAveSPList.SPList.TemplateFeatureId != new Guid(AveWrapperConstants.AVEFSDLFEATRUEID)
                    && this.mAveSPList.SPList.TemplateFeatureId != new Guid(AveWrapperConstants.AVEVDLFEATRUEID))
                {
                    ct = this.mAveSPList.SPList.ContentTypes[
                            this.mAveSPList.SPList.ContentTypes.BestMatch(this.mAveParentSite.ObjectModelFactory.CreateContentTypeId(ctId))];
                    return ct != null ? ct.ID : null;
                }
                ct = this.mAveSPList.SPList.ContentTypes[this.mAveParentSite.ObjectModelFactory.CreateContentTypeId(ctId)];
                if (ct != null)
                {
                    return ct.ID;
                }
                if (this.mAveSPList.SPList.TemplateFeatureId == new Guid(AveWrapperConstants.AVEFSDLFEATRUEID))
                {
                    ct = this.mAveSPList.SPList.ContentTypes.FirstOrDefault(
                        contentType => contentType.ID != null && contentType.ID.ToString().StartsWith(
                            "0x01010003F8831469804144AE3F259EF433E9EB", StringComparison.OrdinalIgnoreCase));
                    return ct != null ? ct.ID : null;
                }
                ct = this.mAveSPList.SPList.ContentTypes.FirstOrDefault(
                    contentType => contentType.ID != null && contentType.ID.ToString().StartsWith(
                            "0x010100806213320A313D4DA11D1B1D6CC700CF", StringComparison.OrdinalIgnoreCase));
                return ct != null ? ct.ID : null;
            }
        }

        private object ReplaceSharedWithDetails(object sValue)
        {
            if (!(sValue is string))
            {
                return sValue;
            }
            try
            {
                var finalValues = new Dictionary<string, Dictionary<string, object>>();
                var value = sValue.ToString();
                var appsMetadata = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(value);
                foreach (var obj in appsMetadata)
                {
                    var keyUser = obj.Key;
                    var user = GetMappedUser(keyUser);
                    if (user == null)//当Key user不存在时，不添加到SharedWithDetails column里。
                    {
                        continue;
                    }
                    finalValues[user.LoginName] = obj.Value;
                    object valueUserObj;
                    if (obj.Value.TryGetValue("LoginName", out valueUserObj))
                    {
                        var valueUser = valueUserObj.ToString();
                        user = GetMappedUser(valueUser);
                        if (user == null)//当Value user不存在时，当成由Register user添加的。
                        {
                            user = mAveParentSite.SPSite.RootWeb.SiteUsers.GetByID(mAveParentSite.CURRENT_USER_ID);
                        }
                        string noPrefixLoginName = user.LoginName;
                        if (!string.IsNullOrEmpty(noPrefixLoginName) && noPrefixLoginName.LastIndexOf('|') > 0)
                        {
                            noPrefixLoginName =  noPrefixLoginName.Substring(noPrefixLoginName.LastIndexOf('|') + 1);
                        }
                        obj.Value["LoginName"] = noPrefixLoginName;//和SP保持一致，这里没有前缀。
                    }
                }
                return JsonConvert.SerializeObject(finalValues);
            }
            catch (Exception e)
            {
                log.Warn("Replace shared with details value failed. Value: {0}, Error: {1}", sValue, e);
            }
            return sValue;
        }
        private IAveUser GetMappedUser(string sourceUser)
        {
            if (string.IsNullOrEmpty(sourceUser))
            {
                return null;
            }
            try
            {
                string mappingValue = mAveParentSite.SPMembers.GetMappingUserLogin(sourceUser.ToString());
                return mAveParentSite.SPSite.RootWeb.EnsureUser(mappingValue);
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, WrapperRestoreResource.GetUserByUrlError, e.ToString());
            }
            return null;
        }

        private string ReplaceUrl(int docRowId, IAveField spField, string url, bool siteCollectionLevel = false)
        {
            if (siteCollectionLevel)
            {
                AveSiteMappingManager mappingManager = mAveSPWeb.ParentSite.MappingManager.SiteMappingManager;
                url = AveReplaceProcessor.UrlReplace(url, mappingManager.SiteUrlMapping, mappingManager.SiteFullUrlMapping,
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

                url = AveReplaceProcessor.SuffixReplace(url, mAveSPWeb.ParentSite.MappingManager.SiteMappingManager,
                     mAveParentSite.SourceSiteInfo, mAveParentSite.ServerRelativeUrl, ref needReplaceLast);
                //url = AveReplaceProcessor.IdReplace(url, mAveSPWeb.ParentSite.MappingManager.SiteMappingManager, ref needReplaceLast);

                if (needReplaceLast)
                {
                    mAveSPWeb.ParentSite.AddUnReplaceUrlIDCache(mAveSPWeb.SPWeb.ID,
                                                                mAveSPList.SPList.ID, docRowId,
                                                                spField.InternalName);
                }
            }
            return url;
        }

        private IAveFieldUrlValue GenerateUrlFieldValue(string key,object value, Dictionary<string, object> data)
        {
            var urlValue = mAveParentSite.ObjectModelFactory.CreateFieldUrlValue();
            string url = value.ToString();
            urlValue.Url = url;
            var descriptionKey = key + "#2";
            if (data.ContainsKey(descriptionKey))
            {
                string description = data[descriptionKey].ToString();
                urlValue.Description = description;
            }
            return urlValue;
        }

        private IAveFieldUrlValue ReplaceUrlValue(IAveFieldUrlValue urlValue,/*string key, object value, Dictionary<string, object> data,*/ int docRowId, IAveField spField)
        {
            bool needSiteCollectionLevel = this.mAveSPList.SPList != null
                && (this.mAveSPList.SPList.BaseTemplate == AveListTemplateType.DesignCatalog
                     && (spField.InternalName.Equals("ThemeUrl", StringComparison.Ordinal)
                         || spField.InternalName.Equals("ImageUrl", StringComparison.Ordinal)
                         || spField.InternalName.Equals("FontSchemeUrl", StringComparison.Ordinal)
                        )
                    );
            string url = urlValue.Url;
            if (url.StartsWith(mAveParentSite.SourceSiteInfo.Url, StringComparison.OrdinalIgnoreCase))
            {
                if (mAveParentSite.SourceSiteInfo.ServerRelativeUrl.Equals("/"))
                {
                    url = url.Replace(mAveParentSite.SourceSiteInfo.Url, string.Empty);
                }
                else
                {
                    url = url.Replace(mAveParentSite.SourceSiteInfo.Url, mAveParentSite.SourceSiteInfo.ServerRelativeUrl);
                }
            }

            urlValue.Url = ReplaceUrl(docRowId, spField, url, needSiteCollectionLevel && url.IndexOf("/_catalogs/theme/", StringComparison.OrdinalIgnoreCase) > -1);

            if (!string.IsNullOrEmpty(urlValue.Description))
            {
                string description = urlValue.Description;
                if (HttpUtility.UrlDecode(description).Equals(HttpUtility.UrlDecode(url), StringComparison.Ordinal))
                {
                    description = urlValue.Url;
                }
                else
                {
                    description = ReplaceUrl(docRowId, spField, description, needSiteCollectionLevel && description.IndexOf("/_catalogs/theme/", StringComparison.OrdinalIgnoreCase) > -1);
                }
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

        internal object ValueTypeChange(IAveField field, Dictionary<string, object> newdata, int rowId, int version, string mappingValue, IAveContentTypeId itemContentTypeId, bool getAveField)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPListFieldCollection.ValueTypeChange"))
            {
#endif
                try
                {
                    object value = mappingValue;
                    string fieldName = field.InternalName;
                    if (fieldName.Contains("#"))
                    {
                        fieldName = fieldName.Substring(0, fieldName.IndexOf("#", StringComparison.OrdinalIgnoreCase));
                    }

                    //如果是taxonomy field关联的Text Field，将fieldName设置成对应taxonomy field的Name
                    //我们还原taxonomy field的value是通过对应Text Field上的value来还原的
                    if (SourceTextTaxonomyDic.ContainsKey(fieldName))
                    {
                        fieldName = SourceTextTaxonomyDic[fieldName];
                    }
                    if (field.InternalName == "Modified_x0020_By" || field.InternalName == "Created_x0020_By")
                    {
                        IAveUser user = null;
                        try
                        {
                            string mappingUserValue = mAveParentSite.SPMembers.GetMappingUserLogin(@value.ToString());
                            if (!String.IsNullOrEmpty(mappingUserValue))
                            {
                                user = mAveParentSite.SPSite.RootWeb.SiteUsers[mappingUserValue];
                            }
                            else
                            {
                                user = mAveParentSite.SPSite.RootWeb.SiteUsers[@value.ToString()];
                            }
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetUserIdByNameFailed, e.ToString());
                        }
                        if (user == null)
                        {
                            value = mAveParentSite.SPSite.RootWeb.SiteUsers.GetByID(mAveParentSite.CURRENT_USER_ID).LoginName;
                        }
                    }
                    if (!NEED_RESTORE_FIELD_MAP.Contains(field.InternalName))
                    {
                        if (FilterCheck(field))
                        {
                            log.Log(AveLogLevel.INFO, string.Format("Filter restore field:{0}.", field.Title));
                            //mLog.Info("Filter restore field:{0}", spField.Title);
                            return value;
                        }
                    }

                    #region ContentType
                    if (field.InternalName.Equals("ContentType"))
                    {
                        try
                        {
                            IAveContentType destContentType = mAveSPList.SPList.ContentTypes[mappingValue];
                            if (destContentType != null)
                            {
                                itemContentTypeId = destContentType.ID;
                            }
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.DEBUG, "Get contentType: {0} is failed {1}", mappingValue, e.ToString());
                        }
                    }

                    #endregion
                    switch (field.TypeAsString)
                    {
                        case "AllDayEvent":
                        case "Boolean":
                            {

                                if (mappingValue.IndexOf(";#") > 0)
                                {
                                    mappingValue = mappingValue.Substring(0, mappingValue.IndexOf(";#"));
                                }
                                switch (mappingValue.ToLower())
                                {
                                    case "true":
                                        {
                                            value = true;
                                            break;
                                        }
                                    case "false":
                                        {
                                            value = false;
                                            break;
                                        }
                                    default:
                                        {
                                            value = null;
                                            break;
                                        }
                                }
                                break;
                            }
                        case "Counter":
                        case "Number":
                            {
                                if (mappingValue.IndexOf(";#") > 0)
                                {
                                    mappingValue = mappingValue.Substring(0, mappingValue.IndexOf(";#"));
                                }
                                try
                                {
                                    //value = Convert.ToInt32(mappingValue);
                                    value = Convert.ToDecimal(mappingValue);//mappingValue有时会传入decimal类型值,如"0.4".
                                }
                                catch (Exception e)
                                {
                                    value = null;
                                    log.Log(AveLogLevel.ERROR, "The value of {0} set {1} is invalid {2}", field.Title, mappingValue, e.ToString());
                                }
                                break;
                            }
                        case "User":
                            {
                                if (mappingValue.IndexOf(";#") > 0)
                                {
                                    mappingValue = mappingValue.Substring(0, mappingValue.IndexOf(";#"));
                                }
                                value = mAveSPList.ParentWeb.GetUserIdByName(value.ToString());
                                break;
                            }
                        case "Note":
                        case "HTML":
                        case "Image":
                        case "SummaryLinks":
                            {
                                value = ReplaceXmlLinks(field.Title, mappingValue);
                                break;
                            }
                        case "DateTime":
                            {
                                if (mappingValue.IndexOf(";#") > 0)
                                {
                                    mappingValue = mappingValue.Substring(0, mappingValue.IndexOf(";#"));
                                }
                                value = Convert.ToDateTime(mappingValue, System.Globalization.DateTimeFormatInfo.InvariantInfo);
                                break;
                            }
                    }
                    #region URl Replace
                    //if (field.InternalName == "TemplateUrl")
                    //{
                    //    ReplaceOption option = new ReplaceOption(true);
                    //    option.NeedReplaceAbsoluteUrl = true;
                    //    value = AveReplaceProcessor.UrlReplace(value.ToString(), mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, option, mAveParentSite.SourceSiteInfo, mAveParentSite.ServerRelativeUrl);
                    //}
                    //else if (field is IAveFieldUrl)
                    //{
                    //    IAveFieldUrlValue urlValue = null;
                    //    if (newdata.ContainsKey(field.InternalName))
                    //    {
                    //        object obj = newdata[field.InternalName];
                    //        AveFieldValueInfo fieldValueInfo = obj as AveFieldValueInfo;
                    //        if (fieldValueInfo != null && fieldValueInfo.ColValue != null)
                    //        {
                    //            urlValue = fieldValueInfo.ColValue as IAveFieldUrlValue;
                    //        }
                    //    }
                    //    if (urlValue == null)
                    //    {
                    //        urlValue = mAveParentSite.ObjectModelFactory.CreateFieldUrlValue();
                    //    }
                    //    if (field.InternalName.EndsWith("#2", StringComparison.OrdinalIgnoreCase))
                    //    {
                    //        string description = value.ToString();
                    //        if (description.StartsWith("http:", StringComparison.OrdinalIgnoreCase) || description.StartsWith("https:", StringComparison.OrdinalIgnoreCase))
                    //        {
                    //            description = AveReplaceProcessor.UrlReplace(description, mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), mAveParentSite.SourceSiteInfo, mAveParentSite.ServerRelativeUrl);
                    //            if (description.Contains("?"))  //替换description中的Id
                    //            {
                    //                bool needReplaceLast = false;
                    //                description = mAveSPWeb.ParentSite.IdReplace(description, ref needReplaceLast);
                    //                if (needReplaceLast)
                    //                {
                    //                    mAveSPWeb.ParentSite.AddUnReplaceUrlIDCache(mAveSPWeb.SPWeb.ID, mAveSPList.SPList.ID, rowId, field.InternalName);
                    //                }
                    //            }
                    //        }
                    //        else if (HttpUtility.UrlDecode(description) == HttpUtility.UrlDecode(urlValue.Description))
                    //        {
                    //            description = urlValue.Url;
                    //        }
                    //        urlValue.Description = description;
                    //    }
                    //    else
                    //    {
                    //        string url = mappingValue.ToString();
                    //        if (string.IsNullOrEmpty(urlValue.Description))
                    //        {
                    //            urlValue.Description = url;
                    //        }
                    //        else if (HttpUtility.UrlDecode(urlValue.Description) == HttpUtility.UrlDecode(url))
                    //        {
                    //            urlValue.Description = AveReplaceProcessor.UrlReplace(url, mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), mAveParentSite.SourceSiteInfo, mAveParentSite.ServerRelativeUrl);
                    //        }
                    //        urlValue.Url = AveReplaceProcessor.UrlReplace(url, mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), mAveParentSite.SourceSiteInfo, mAveParentSite.ServerRelativeUrl);
                    //        if (urlValue.Url.Contains("?"))  //替换url中的Id
                    //        {
                    //            bool needReplaceLast = false;
                    //            urlValue.Url = mAveSPWeb.ParentSite.IdReplace(urlValue.Url, ref needReplaceLast);
                    //            if (needReplaceLast)
                    //            {
                    //                mAveSPWeb.ParentSite.AddUnReplaceUrlIDCache(mAveSPWeb.SPWeb.ID, mAveSPList.SPList.ID, rowId, field.InternalName);
                    //            }
                    //        }
                    //        //wiki page中的PublishingPageLayout，应该指向的是root site上masterpage中的文件,当做root site到sub site的mapping时，UrlReplace替换成指向sub site上的masterpage，
                    //        //导致还原后的wiki page打开出错，在此处理这种case，将PublishingPageLayout指向的Url换成指向root site上masterpage中的文件。
                    //        if (field.InternalName == "PublishingPageLayout")
                    //        {
                    //            if (urlValue.Url.Contains("/_catalogs/masterpage"))
                    //            {
                    //                string temUrl = mAveSPWeb.ParentSite.SPSite.ServerRelativeUrl.TrimEnd('/') + "/_catalogs/masterpage";
                    //                if (!urlValue.Url.StartsWith(temUrl, StringComparison.OrdinalIgnoreCase))
                    //                {
                    //                    urlValue.Url = mAveSPWeb.ParentSite.SPSite.ServerRelativeUrl.TrimEnd('/') + urlValue.Url.Substring(urlValue.Url.IndexOf("/_catalogs/masterpage", StringComparison.OrdinalIgnoreCase));
                    //                }
                    //            }
                    //        }
                    //    }
                    //    value = urlValue;
                    //}
                    //else 
                    #endregion
                    if (fieldName.Equals("Target_x0020_Audiences", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            value = (object)ReplaceAudienceId(mAveParentSite.MappingManager.SiteMappingManager.AudienceIDMapping, value.ToString());
                        }
                        catch (Exception e)
                        {
                            log.Warn("Replace item audience id error. Exception:{0}.", e.ToString());
                        }
                    }
                    if (getAveField)
                    {
                        AveFieldValueInfo aveField = new AveFieldValueInfo();
                        aveField.ColValue = value;
                        if (field is IAveFieldUrl)
                        {
                            if (field.InternalName.EndsWith("#2", StringComparison.OrdinalIgnoreCase))
                            {
                                aveField.ColName = field.GetFieldAttributeValue("ColName2");
                                aveField.FieldType = AveFieldType.URL;
                                try
                                {
                                    aveField.RowOrdinal = Convert.ToInt32(field.GetFieldAttributeValue("RowOrdinal2"));
                                }
                                catch (Exception e)
                                {
                                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.UpdateListFailed, e.ToString());
                                    aveField.RowOrdinal = 0;
                                }
                                //Convert.ToInt32(AveAssemblyUtility.InvokeMethod(spField, typeof(SPField), "GetFieldAttributeValue", new Type[] { typeof(string) }, new object[] { "RowOrdinal2" }));
                                if (value is IAveFieldUrlValue)
                                {
                                    aveField.ColValue = ((IAveFieldUrlValue)value).Description;
                                }
                                value = aveField;
                                return value;
                            }
                            else
                            {
                                aveField.ColName = field.ColName;
                                aveField.FieldType = AveFieldType.URL;
                                aveField.RowOrdinal = field.RowOrdinal;
                                if (value is IAveFieldUrlValue)
                                {
                                    aveField.ColValue = ((IAveFieldUrlValue)value).Url;
                                }
                                if (aveField.ColValue is string)
                                {
                                    aveField.ColValue = AveReplaceProcessor.UrlReplace((string)aveField.ColValue, mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), mAveParentSite.SourceSiteInfo, mAveParentSite.ServerRelativeUrl);
                                }
                                value = aveField;
                                return value;
                            }
                        }
                        else if (field.InternalName.Equals("ContentType"))
                        {
                            aveField.ColName = field.ColName;
                            aveField.ColValue = itemContentTypeId;
                        }
                        else
                        {
                            aveField.ColName = field.ColName;
                            aveField.RowOrdinal = field.RowOrdinal;
                        }
                        aveField.FieldType = field.Type;
                        value = aveField;
                    }
                    return value;
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.UpdateListFailed, e.ToString());

                    return null;
                }
#if PerformanceLog
            }
#endif
        }

        #region Get Custom Mapping Value

        internal object GetCustomMappingValue(object srcObjectValue, AveXmlField xmlField, IAveField spField, string itemName, ref bool isCustom, ref bool isValueCheck)
        {
            //如果没有mapping成功 返回原始值
            string srcValue = srcObjectValue.ToString();
            object mappingValue = srcValue;
            try
            {
                AveCustomFieldInfo info = xmlField.CustomFieldInfo;
                if (xmlField.CustomFieldInfo.SourceType.Equals(AveFieldType.User))
                {
                    srcValue = GetLoginNameFromIdWithCustomMappingAndSourceUser(srcValue);
                }
                AveSourceFieldValueInfo sourceFieldValueInfo = new AveSourceFieldValueInfo()
                {
                    SourceFieldInfo = new AveSourceFieldInfo() { SourceDisplayName = xmlField.Title, SourceInternalName = xmlField.FieldInternalName, SourceType = xmlField.Type },
                    SourceValue = srcValue,
                    SourceItemName = itemName
                };
                if (info != null)
                {
                    switch (spField.TypeAsString)
                    {
                        case "Lookup":
                        case "LookupMulti":
                            if (xmlField.CustomFieldInfo.CustomFieldTypeAsString.Equals(AveCustomFieldType.ChangeToLookUp.ToString(), StringComparison.OrdinalIgnoreCase))
                            {
                                mappingValue = GetLookUpItemIdByMappingValue(sourceFieldValueInfo, info, spField.TypeAsString.Equals("LookupMulti", StringComparison.OrdinalIgnoreCase));
                                isCustom = true;
                            }
                            else
                            {
                                mappingValue = GetFieldMappingValue(xmlField, srcValue, itemName);
                            }
                            log.Debug("Get value mapping of lookup.value:" + mappingValue);
                            break;
                        case "TaxonomyFieldType":
                        case "TaxonomyFieldTypeMulti":
                            if (xmlField.CustomFieldInfo.CustomFieldTypeAsString.Equals(AveCustomFieldType.ChangeToMetadata.ToString(), StringComparison.OrdinalIgnoreCase))
                            {
                                mappingValue = GetMetadataValueByMappingValue(sourceFieldValueInfo, info, spField.TypeAsString.Equals("TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase));
                            }
                            else
                            {
                                mappingValue = GetFieldMappingValue(xmlField, srcValue, itemName);
                            }
                            break;
                        case "Choice":
                        case "MultiChoice":
                            mappingValue = GetChoiceValueByMappingValue(sourceFieldValueInfo, info, spField.TypeAsString.Equals("MultiChoice", StringComparison.OrdinalIgnoreCase));
                            break;
                        default:
                            mappingValue = GetFieldMappingValue(xmlField, srcValue, itemName);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Get Custom Mapping Value Error.value:{0} Exception:{1}", srcValue, ex.ToString());
            }
            //目的端是user类型 需要把loginname改为id
            if (spField.Type.Equals(AveFieldType.User))
            {
                mappingValue = GetPrincipalIDWithPrincipalName(mappingValue as string);
            }
            isValueCheck = true;
            //如果没有mapping成功 返回原来的值
            if (mappingValue.ToString().Equals(srcValue, StringComparison.OrdinalIgnoreCase))
            {
                isValueCheck = false;
                mappingValue = srcObjectValue;
            }
            return mappingValue;
        }

        public IAvePrincipal GetCustomMappingValueForDataJunction(string docName, object value, string fieldName, IAveField spField)
        {
            try
            {
                bool IsSetByCustomMapping = false;
                bool IsValueSetByCustomMapping = false;
                if (mXmlFields.ContainsKey(fieldName) && mXmlFields[fieldName].CustomFieldInfo != null)
                {
                    int mappingValue = (int)GetCustomMappingValue(value, mXmlFields[fieldName], spField, docName, ref IsSetByCustomMapping, ref IsValueSetByCustomMapping);
                    return mAveSPWeb.SPWeb.SiteUsers.GetByID(mappingValue);
                }
                else
                {
                    return mAveSPWeb.ParentSite.SPMembers.FindMember(Convert.ToInt32(value));
                }
            }
            catch (Exception ex)
            {
                log.Error("Get Principal By Mapping Value Error.value:{0} Exception:{1}", value.ToString(), ex.ToString());
            }
            return null;
        }

        private string GetFieldMappingValue(AveXmlField xmlField, string srcValue, string itemName)
        {
            AveSourceFieldValueInfo sourceFieldValue = new AveSourceFieldValueInfo()
            {
                SourceFieldInfo = new AveSourceFieldInfo() { SourceInternalName = xmlField.FieldInternalName, SourceDisplayName = xmlField.Title, SourceType = xmlField.Type },
                SourceValue = srcValue,
                SourceItemName = itemName
            };
            return FieldMapping.GetMappingValue(sourceFieldValue);
        }

        private string GetChoiceValueByMappingValue(AveSourceFieldValueInfo sourceFieldValueInfo, AveCustomFieldInfo info, bool isMultiChoice)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPListFieldCollection.GetChoiceValueByMappingValue"))
            {
#endif
                string choiceValue = String.Empty;
                try
                {
                    string[] intervals = new string[] { ";#" };
                    foreach (string interval in intervals)
                    {
                        if (sourceFieldValueInfo.SourceValue.Contains(interval))
                        {
                            string[] values = sourceFieldValueInfo.SourceValue.Split(new string[] { interval }, StringSplitOptions.None);
                            string srcDisplayValue = string.Empty;
                            foreach (string value in values)
                            {
                                if (!String.IsNullOrEmpty(value))
                                {
                                    AveSourceFieldValueInfo tempSourceFieldValueInfo = new AveSourceFieldValueInfo()
                                    {
                                        SourceFieldInfo = sourceFieldValueInfo.SourceFieldInfo,
                                        SourceValue = value,
                                        SourceItemName = sourceFieldValueInfo.SourceItemName
                                    };
                                    string mappingValue = FieldMapping.GetMappingValue(tempSourceFieldValueInfo);
                                    if (!isMultiChoice)
                                    {
                                        if (!mappingValue.Equals(value))
                                        {
                                            choiceValue = mappingValue;
                                        }
                                        srcDisplayValue = srcDisplayValue + "," + value;
                                        continue;
                                    }
                                    choiceValue = choiceValue + ";#" + mappingValue;
                                }
                            }
                            if (!isMultiChoice)//源端多值，目的端单值，如果能找到多值到单值的匹配，则使用，若找不到，则从源端多值中找出其第一个能匹配的
                            {
                                srcDisplayValue = srcDisplayValue.TrimStart(',');
                                string mapping = FieldMapping.GetMappingValue(sourceFieldValueInfo);
                                if (!mapping.Equals(srcDisplayValue, StringComparison.OrdinalIgnoreCase))
                                {
                                    return mapping;
                                }
                            }
                            else
                            {
                                choiceValue = choiceValue + ";#";
                            }
                            return choiceValue;
                        }
                    }
                    choiceValue = FieldMapping.GetMappingValue(sourceFieldValueInfo);
                }
                catch (Exception ex)
                {
                    log.Error("Get Choice Value By Mapping Value error. Exception:" + ex.ToString());
                }
                return choiceValue;

#if PerformanceLog
            }
#endif
        }

        //过滤";", "<", "|", ">", "\t" 特殊字符
        private string FilterSpecialChars(string value, string separateChar)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            string newValue = value;
            List<string> filterChars = new List<string>();
            filterChars.Add(";");
            filterChars.Add("<");
            filterChars.Add("|");
            filterChars.Add(">");
            filterChars.Add("\"");
            filterChars.Add("\t");

            if (separateChar != " ")
            {
                if (filterChars.Contains(separateChar))
                {
                    filterChars.Remove(separateChar);
                }
                foreach (string filter in filterChars)
                {
                    if (newValue.Contains(filter))
                    {
                        newValue = newValue.Replace(filter, " "); //将特殊字符替换成" "
                    }
                }
            }
            else
            {
                //当分割符是空格的时候，需要先分割后替换
                string[] sp = new string[1] { separateChar };
                string[] strs = value.ToString().Split(sp, StringSplitOptions.None);
                for (int i = 0; i < strs.Length; i++)
                {
                    //提高内存缓冲效率
                    StringBuilder tempStringBuilder = new StringBuilder(strs[i]);
                    foreach (string filterChar in filterChars)
                    {
                        if (strs[i].Contains(filterChar))
                        {
                            tempStringBuilder.Replace(filterChar, " ");
                        }
                    }
                    strs[i] = tempStringBuilder.ToString();
                }
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < strs.Length; i++)
                {
                    if (strs[i] == " ")
                    {
                        continue;
                    }
                    sb.Append(strs[i]);
                    sb.Append(";");
                }
                newValue = sb.ToString().TrimEnd(';');
            }
            return newValue;
        }

        private string GetMetadataValueByMappingValue(AveSourceFieldValueInfo sourceFieldValueInfo, AveCustomFieldInfo info, bool isMultiChoice)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPListFieldCollection.GetMetadataValueByMappingValue"))
            {
#endif
                log.Debug("metadata srcValue:" + sourceFieldValueInfo.SourceValue);
                string metaValue = String.Empty;
                try
                {
                    string[] intervals = new string[] { ";#", "\r\n" };
                    if (info != null && info is AveCustomMetadataFieldInfo)
                    {
                        string separateChar = (info as AveCustomMetadataFieldInfo).SeparateChar;
                        if (separateChar != null)
                        {
                            if (separateChar == string.Empty)
                            {
                                //这种情况为不勾选Migrate string separated with into columns 或者 虽然勾选了但是什么也没有输入
                            }
                            else if (separateChar == " ")
                            {
                                //如果是" "的话在FilterSpecialChars中进行分割，不在在面循环中进行处理
                                intervals = new string[] { };
                            }
                            else
                            {
                                intervals = new string[] { separateChar };
                            }
                            sourceFieldValueInfo.SourceValue = FilterSpecialChars(sourceFieldValueInfo.SourceValue, separateChar);
                        }
                    }
                    foreach (string interval in intervals)
                    {
                        if (sourceFieldValueInfo.SourceValue.Contains(interval))
                        {
                            string[] values = sourceFieldValueInfo.SourceValue.Split(new string[] { interval }, StringSplitOptions.None);
                            string srcDisplayValue = string.Empty;
                            foreach (string value in values)
                            {
                                if (!String.IsNullOrEmpty(value))
                                {
                                    AveSourceFieldValueInfo tempSourceFieldValueInfo = new AveSourceFieldValueInfo()
                                    {
                                        SourceFieldInfo = sourceFieldValueInfo.SourceFieldInfo,
                                        SourceValue = value,
                                        SourceItemName = sourceFieldValueInfo.SourceItemName
                                    };
                                    string mappingValue = FieldMapping.GetMappingValue(tempSourceFieldValueInfo);
                                    if (!isMultiChoice)
                                    {
                                        if (!mappingValue.Equals(value))
                                        {
                                            metaValue = mappingValue;
                                        }
                                        srcDisplayValue = srcDisplayValue + "," + value;
                                        continue;
                                    }
                                    //D5 SPMigration逻辑里是将对应的value转换成term的,所以注释掉下面的代码
                                    //if (mappingValue.Equals(value, StringComparison.OrdinalIgnoreCase))
                                    //{
                                    //    continue;
                                    //}
                                    metaValue = metaValue + ";" + mappingValue;
                                }
                            }
                            if (!isMultiChoice)//源端多值，目的端单值，如果能找到多值到单值的匹配，则使用，若找不到，则从源端多值中找出其一个能匹配的
                            {
                                srcDisplayValue = srcDisplayValue.TrimStart(',');
                                string mapping = FieldMapping.GetMappingValue(sourceFieldValueInfo);
                                if (!mapping.Equals(srcDisplayValue, StringComparison.OrdinalIgnoreCase))
                                {
                                    return mapping;
                                }
                            }
                            else
                            {
                                metaValue = metaValue.TrimStart(';');
                            }
                            log.Debug("metadata mapping value:" + metaValue);
                            return metaValue;
                        }
                    }

                    metaValue = FieldMapping.GetMappingValue(sourceFieldValueInfo);
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, WrapperRestoreResource.GetMetadataValueFailed, ex);
                }
                log.Debug("metadata mapping value:" + metaValue);
                return metaValue;
#if PerformanceLog
            }
#endif
        }


        private readonly object lockObj = new object();
        private string GetLookUpItemIdByMappingValue(AveSourceFieldValueInfo sourceFieldValueInfo, AveCustomFieldInfo info, bool isMultiChoice)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPListFieldCollection.GetLookUpItemIdByMappingValue"))
            {
#endif
                AveCustomLookupFieldInfo lookupInfo = info as AveCustomLookupFieldInfo;
                string lookupValue = String.Empty;
                try
                {
                    lock (lockObj)
                    {
                        if (!LooUpListItemIDAndValues.ContainsKey(lookupInfo.ListTitle))
                        {
                            Dictionary<string, string> idValue = new Dictionary<string, string>();
                            IAveWeb web = mAveSPWeb.SPWeb;
                            if (!string.IsNullOrEmpty(lookupInfo.WebRelativeUrl))
                            {
                                mAveParentSite.SPSite.OpenWeb(lookupInfo.WebRelativeUrl.Replace(mAveParentSite.SPSite.RootWeb.ServerRelativeUrl.TrimStart('/'), "").TrimStart('/'));
                            }
                            IAveList lookupList = web.Lists[lookupInfo.ListTitle];
                            foreach (IAveListItem item in lookupList.Items)
                            {
                                if (item[lookupInfo.FieldName] != null)
                                {
                                    idValue[item[lookupInfo.FieldName].ToString()] = item.ID.ToString();
                                }
                            }
                            log.Debug("Get id value.count:" + idValue.Count);
                            LooUpListItemIDAndValues.Add(lookupInfo.ListTitle, idValue);
                        }
                    }
                    string[] intervals = new string[] { ";#", "\r\n" };
                    if (!string.IsNullOrEmpty(lookupInfo.SeparateChar))
                    {
                        intervals = new string[] { lookupInfo.SeparateChar };
                    }
                    foreach (string interval in intervals)
                    {
                        if (sourceFieldValueInfo.SourceValue.Contains(interval))
                        {
                            string[] values = sourceFieldValueInfo.SourceValue.Split(new string[] { interval }, StringSplitOptions.None);
                            string srcDisplayValue = string.Empty;
                            foreach (string value in values)
                            {
                                if (!String.IsNullOrEmpty(value))
                                {
                                    AveSourceFieldValueInfo tempSourceFieldValueInfo = new AveSourceFieldValueInfo()
                                    {
                                        SourceFieldInfo = sourceFieldValueInfo.SourceFieldInfo,
                                        SourceValue = value,
                                        SourceItemName = sourceFieldValueInfo.SourceItemName
                                    };
                                    string mappingValue = FieldMapping.GetMappingValue(tempSourceFieldValueInfo);
                                    if (!isMultiChoice)
                                    {
                                        if (!mappingValue.Equals(value))
                                        {
                                            lookupValue = LooUpListItemIDAndValues[lookupInfo.ListTitle][mappingValue];
                                        }
                                        srcDisplayValue = srcDisplayValue + "," + value;
                                        continue;
                                    }
                                    lookupValue = lookupValue + "#" + LooUpListItemIDAndValues[lookupInfo.ListTitle][mappingValue] + ";#" + mappingValue + ";";
                                }
                            }
                            if (!isMultiChoice)//源端多值，目的端单值，如果能找到多值到单值的匹配，则使用，若找不到，则从源端多值中找出其第一个能匹配的
                            {
                                srcDisplayValue = srcDisplayValue.TrimStart(',');
                                string mapping = FieldMapping.GetMappingValue(sourceFieldValueInfo);
                                if (!mapping.Equals(srcDisplayValue, StringComparison.OrdinalIgnoreCase))
                                {
                                    return LooUpListItemIDAndValues[lookupInfo.ListTitle][mapping];
                                }
                            }
                            lookupValue = lookupValue.TrimEnd(';').TrimStart('#');
                            return lookupValue;
                        }
                    }

                    lookupValue = LooUpListItemIDAndValues[lookupInfo.ListTitle][FieldMapping.GetMappingValue(sourceFieldValueInfo)];
                }
                catch (Exception ex)
                {
                    log.Error("Get Lookup Mapping Value Error.value:{0} Exception:{1}", sourceFieldValueInfo.SourceValue, ex.ToString());
                }
                return lookupValue;

#if PerformanceLog
            }
#endif
        }

        private string GetLoginNameFromIdWithCustomMappingAndSourceUser(string sourceUserValue)
        {
            int principalID = 0;
            string sourceLoginName = string.Empty;
            if (int.TryParse(sourceUserValue, out principalID))
            {
                IAvePrincipal principal = mAveSPList.ParentSite.SPMembers.FindMember(principalID);
                if (principal != null)
                {
                    string newsourceLoginName = principal.LoginName;
                    sourceLoginName = mAveSPList.ParentSite.SPMembers.GetMappingUserLogin(newsourceLoginName);
                }
            }
            return sourceLoginName;
        }

        private int GetPrincipalIDWithPrincipalName(string principalName)
        {
            int principalID = -1;
            foreach (IAveGroup group in mAveSPList.ParentSite.SPSite.RootWeb.Groups)
            {
                if (group.LoginName.Equals(principalName, StringComparison.OrdinalIgnoreCase))
                {
                    principalID = group.ID;
                }
            }
            if (principalID == -1)
            {
                try
                {
                    IAveUser user = mAveSPList.ParentSite.SPSite.RootWeb.EnsureUser(principalName);
                    if (user != null)
                    {
                        principalID = user.ID;
                    }
                }
                catch (Exception e)
                {
                    log.Debug(WrapperRestoreResource.CannotFindUser, principalID, e.Message);
                }
            }
            return principalID;
        }

        internal void SetExcelFieldValue(Dictionary<string, object> newdata, int rowId, int version, bool getAveField, IAveContentTypeId itemContentTypeId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPListFieldCollection.Find"))
            {
#endif
                try
                {
                    if ((FieldMapping as AveFieldMapping).CustomMapping == null)
                    {
                        return;
                    }
                    foreach (AveSourceFieldInfo sourceFieldInfo in ((FieldMapping as AveFieldMapping).CustomMapping as AveCustomFieldMappingForExcel).InternalExcelFieldMapping.Keys)
                    {
                        if (!sourceFieldInfo.SourceDisplayName.Equals("ID", StringComparison.OrdinalIgnoreCase))
                        {
                            string fieldDisplayName = sourceFieldInfo.SourceDisplayName;
                            IAveField field = mAveSPList.SPList.Fields[fieldDisplayName];
                            if (field == null)
                            {
                                continue;
                            }
                            AveSourceFieldValueInfo sourceFieldValueInfo = new AveSourceFieldValueInfo()
                            {
                                SourceFieldInfo = sourceFieldInfo,
                                SourceItemRowId = rowId
                            };
                            string mappingValue = ((FieldMapping as AveFieldMapping).CustomMapping as AveCustomFieldMappingForExcel).GetMappingValue(sourceFieldValueInfo);
                            if (string.IsNullOrEmpty(mappingValue))
                            {
                                //Excel表中metadata的值可能为空，会导致后面抛异常整个file/item还原失败，先在这里控制一下。
                                if (!field.TypeAsString.Equals("TaxonomyFieldType", StringComparison.OrdinalIgnoreCase) && !field.TypeAsString.Equals("TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase))
                                {
                                    newdata[field.InternalName] = null;
                                }
                                continue;
                            }
                            GetCustomMappingValueForExcel(newdata, mappingValue.ToString(), field, rowId, version, itemContentTypeId, getAveField);
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.ERROR, "When set excel field value meet error:" + e.ToString());
                }
#if PerformanceLog
            }
#endif
        }
        internal string GetCustomMappingValueForExcel(Dictionary<string, object> newdata, string srcValue, IAveField spField, int docRowId, int version, IAveContentTypeId itemContentTypeId, bool getAveField)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPListFieldCollection.GetCustomMappingValueForExcel"))
            {
#endif
                string mappingValue = srcValue;
                try
                {

                    switch (spField.TypeAsString)
                    {
                        case "Lookup":
                        case "LookupMulti":
                            mappingValue = GetLookUpItemIdByMappingValueForExcel(newdata, srcValue, spField as IAveFieldLookup, spField.TypeAsString.Equals("LookupMulti", StringComparison.OrdinalIgnoreCase), docRowId);
                            log.Debug("Get value mapping of lookup.value:" + mappingValue);
                            break;
                        case "TaxonomyFieldTypeMulti":
                            mappingValue = GetMetadataValueByMappingValueForExcel(srcValue);
                            break;
                        case "UserMulti":
                            mappingValue = GetUserValueByMappingValueForExcel(srcValue);
                            break;
                        case "Boolean":
                            if (mappingValue.Equals("Yes", StringComparison.OrdinalIgnoreCase) || mappingValue.Equals("True", StringComparison.OrdinalIgnoreCase))
                            {
                                mappingValue = "True";
                            }
                            else if (mappingValue.Equals("No", StringComparison.OrdinalIgnoreCase) || mappingValue.Equals("False", StringComparison.OrdinalIgnoreCase))
                            {
                                mappingValue = "False";
                            }
                            else
                            {
                                mappingValue = "";
                            }
                            break;
                        default:
                            AveSourceFieldValueInfo sourceFieldValueInfo = new AveSourceFieldValueInfo()
                            {
                                SourceFieldInfo = new AveSourceFieldInfo() { SourceDisplayName = spField.Title },
                                SourceItemRowId = docRowId
                            };
                            mappingValue = ((FieldMapping as AveFieldMapping).CustomMapping as AveCustomFieldMappingForExcel).GetMappingValue(sourceFieldValueInfo);
                            break;
                    }
                    object value = null;
                    //if (!spField.TypeAsString.Equals("LookupMulti", StringComparison.OrdinalIgnoreCase))
                    //{
                    value = ValueTypeChange(spField, newdata, docRowId, version, mappingValue, itemContentTypeId, getAveField);
                    //}
                    if (value != null)
                    {
                        if (!(value is AveFieldValueInfo))
                        {
                            value = mappingValue;
                            AveFieldValueInfo mappingValueInfo = new AveFieldValueInfo();
                            mappingValueInfo.ColValue = value;
                            newdata[spField.InternalName] = mappingValueInfo;
                        }
                        else
                        {
                            newdata[spField.InternalName] = value;
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Get Custom Mapping Value Error.value:{0} Exception:{1}", srcValue, ex.ToString());
                }
                return mappingValue;
#if PerformanceLog
            }
#endif
        }
        private string GetMetadataValueByMappingValueForExcel(string mappingValue)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPListFieldCollection.GetMetadataValueByMappingValueForExcel"))
            {
#endif
                log.Debug("metadata srcValue:" + mappingValue);
                string metaValue = mappingValue;
                string[] intervals = new string[] { ";#" };
                foreach (string interval in intervals)
                {
                    if (metaValue.Contains(interval))
                    {
                        metaValue = metaValue.Replace(interval, ";");
                        break;
                    }
                }
                return metaValue;
#if PerformanceLog
            }
#endif
        }
        private string GetUserValueByMappingValueForExcel(string mappingValue)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPListFieldCollection.GetUserValueByMappingValueForExcel"))
            {
#endif
                log.Debug("User srcValue:" + mappingValue);
                string userValue = String.Empty;
                string[] intervals = new string[] { ";#" };
                foreach (string interval in intervals)
                {
                    if (mappingValue.Contains(interval))
                    {
                        string[] values = mappingValue.Split(new string[] { interval }, StringSplitOptions.None);
                        foreach (string value in values)
                        {
                            try
                            {
                                if (mAveSPWeb.GetUserIdByUserName(value) != -1)
                                {
                                    userValue = userValue + mAveSPWeb.GetUserIdByUserName(value) + ";#" + value + ";#";
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Error("Get User Mapping Value Error.value:{0} Exception:{1}", value, ex.ToString());
                            }
                        }
                        return userValue;
                    }
                }
                userValue = mAveSPWeb.GetUserIdByUserName(mappingValue) + ";#" + mappingValue + ";#";
                return userValue;
#if PerformanceLog
            }
#endif
        }
        private string GetLookUpItemIdByMappingValueForExcel(Dictionary<string, object> newdata, string mappingValue, IAveFieldLookup field, bool isMultiChoice, int docRowId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPListFieldCollection.GetLookUpItemIdByMappingValueForExcel"))
            {
#endif
                IAveWeb lookupWeb = mAveSPList.ParentSite.SPSite.OpenWeb((field as IAveFieldLookup).LookupWebId);
                IAveList lookupList = lookupWeb.Lists.GetById(new Guid((field as IAveFieldLookup).LookupList));
                string lookupValue = String.Empty;
                string[] intervals = new string[] { ";#" };
                try
                {

                    if (!LooUpListItemIDAndValues.ContainsKey(lookupList.Title))
                    {
                        Dictionary<string, string> idValue = new Dictionary<string, string>();
                        foreach (IAveListItem item in lookupList.Items)
                        {
                            idValue[item[field.LookupField].ToString()] = item.ID.ToString();
                        }
                        log.Debug("Get id value.count:" + idValue.Count);
                        LooUpListItemIDAndValues.Add(field.LookupListTitle, idValue);
                    }
                    if (isMultiChoice)
                    {
                        foreach (string interval in intervals)
                        {
                            if (mappingValue.Contains(interval))
                            {
                                string[] values = mappingValue.Split(new string[] { interval }, StringSplitOptions.None);
                                foreach (string value in values)
                                {
                                    lookupValue = lookupValue + LooUpListItemIDAndValues[lookupList.Title][value] + ";#" + value + ";#";
                                }
                            }
                            else
                            {
                                lookupValue = LooUpListItemIDAndValues[lookupList.Title][mappingValue];
                            }
                        }
                    }
                    else
                    {
                        lookupValue = LooUpListItemIDAndValues[lookupList.Title][mappingValue];
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Get Lookup Mapping Value Error.value:{0} Exception:{1}", mappingValue, ex.ToString());
                }
                return lookupValue;
#if PerformanceLog
            }
#endif
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="field"></param>
        /// <returns>true:过滤，false:不过滤</returns>
        public bool FilterCheck(IAveField field)
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

        private string ChangeServerRelativeUrl(string key, string value)
        {
            if (value.Contains(","))
            {
                string result = ChangeServerRelativeUrl(key, value.Substring(0, value.IndexOf(','))) + ", " + ChangeServerRelativeUrl(key, value.Substring(value.IndexOf(',') + 1));
                return result;
            }
            else
            {
                string destWebUrl = '/' + mAveSPWeb.ScopeString;
                string srcWebUrl = mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.WebUrlDestToSourceMapping[destWebUrl].ToString();
                if (value.TrimStart().StartsWith(srcWebUrl, StringComparison.OrdinalIgnoreCase))
                {
                    if (destWebUrl.Length == 1 && srcWebUrl.Length != 1) //目的端是top site
                    {
                        value = value.TrimStart().ToString().Substring(srcWebUrl.Length);
                    }
                    else if (srcWebUrl.Length == 1 && destWebUrl.Length != 1) //源端是top site
                    {
                        value = destWebUrl + value.TrimStart().ToString().Substring(0);
                    }
                    else
                    {
                        value = destWebUrl + value.TrimStart().ToString().Substring(srcWebUrl.Length);
                    }
                }
            }
            return value;
        }

        /// <summary>
        /// 替换xml格式的field value中的链接，暂时找出a,img，若还有其他类型的链接也加在此方法中
        /// </summary>
        /// <param name="fieldValue"></param>
        /// <returns></returns>
        private string ReplaceXmlLinks(string fieldName, string fieldValue)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPListFieldCollection.ReplaceXmlLinks"))
            {
#endif
                try
                {
                    HtmlDocument fieldDoc = new HtmlDocument();
                    fieldDoc.OptionOutputOriginalCase = true;
                    fieldDoc.LoadHtml("<ReplaceXmlLinks>" + fieldValue + "</ReplaceXmlLinks>");
                    List<HtmlNode> nodes = new List<HtmlNode>();
                    GetLinkNodes(nodes, fieldDoc.DocumentNode);
                    foreach (HtmlNode node in nodes)
                    {
                        ReplaceXmlLinks(node);
                    }
                    return fieldDoc.DocumentNode.FirstChild.InnerHtml;
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.ReplaceXmlLinksError, ex.ToString());
                    try
                    {
                        fieldValue = ReplaceStringLinks(fieldValue);
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, string.Format("An error occurred while replace xml Links. fieldName:{0}\n error message:{1}", fieldName, e));
                    }
                    return fieldValue;
                }
#if PerformanceLog
            }
#endif
        }

        /// <summary>
        /// HTML语言中忽略大小写，所以link可能是大写，也可能是小写
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="node"></param>
        private void GetLinkNodes(List<HtmlNode> nodes, HtmlNode node)
        {
            foreach (HtmlNode child in node.ChildNodes)
            {
                if (child.NodeType == HtmlNodeType.Element)
                {
                    if (child.Name.Equals("a", StringComparison.OrdinalIgnoreCase) || child.Name.Equals("img", StringComparison.OrdinalIgnoreCase))
                    {
                        nodes.Add(child);
                    }
                    GetLinkNodes(nodes, child);
                }
            }
        }

        private void ReplaceXmlLinks(HtmlNode node)
        {
            HtmlAttribute linkAttribute = null;

            //HTML语言中忽略大小写，所以href、src可能是大写，也可能是小写
            foreach (HtmlAttribute attr in node.Attributes)
            {
                if (node.Name.Equals("a", StringComparison.OrdinalIgnoreCase))
                {
                    if (attr.Name.Equals("href", StringComparison.OrdinalIgnoreCase))
                    {
                        linkAttribute = attr;
                        break;
                    }
                }
                else
                {
                    if (attr.Name.Equals("src", StringComparison.OrdinalIgnoreCase))
                    {
                        linkAttribute = attr;
                        break;
                    }
                }
            }
            ArgumentNullException.ThrowIfNull(linkAttribute);
            string hrefLink = linkAttribute?.Value;//HttpUtility.UrlDecode(linkAttribute.Value);
            string value = AveReplaceProcessor.UrlReplace(hrefLink, mAveSPList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), mAveParentSite.SourceSiteInfo, mAveParentSite.ServerRelativeUrl, true);
            linkAttribute.Value = HttpUtility.UrlPathEncode(value);
            if (node.HasChildNodes)
            {
                foreach (HtmlNode child in node.ChildNodes)
                {
                    HtmlTextNode textNode = child as HtmlTextNode;
                    if (textNode != null && textNode.NodeType == HtmlNodeType.Text)
                    {
                        if (HttpUtility.UrlDecode(textNode.Text).Equals(hrefLink))
                        {
                            textNode.Text = HttpUtility.UrlDecode(linkAttribute.Value);
                        }
                        else if (HttpUtility.UrlDecode(textNode.Text).EndsWith(hrefLink, StringComparison.OrdinalIgnoreCase) &&
                            (HttpUtility.UrlDecode(textNode.Text).StartsWith("http://", StringComparison.OrdinalIgnoreCase) || HttpUtility.UrlDecode(textNode.Text).StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
                        {
                            hrefLink = HttpUtility.UrlDecode(textNode.Text);
                            string textValue = AveReplaceProcessor.UrlReplace(hrefLink, mAveSPList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), mAveParentSite.SourceSiteInfo, mAveParentSite.ServerRelativeUrl);
                            textNode.Text = HttpUtility.UrlPathEncode(textValue);
                        }
                    }
                }
            }
        }

        private string ReplaceStringLinks(string strValue)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPListFieldCollection.ReplaceStringLinks"))
            {
#endif
                int length = strValue.Length;
                int index = 0;
                List<string> links = new List<string>();
                while (index < length)
                {
                    if (strValue[index] == '<')
                    {
                        if ((index + 2 < length) && strValue.Substring(index, 3) == "<a ")
                        {
                            int end = strValue.IndexOf('>', index);
                            if (end > 0)
                            {
                                int p1 = strValue.IndexOf("href=" + '"', index, StringComparison.OrdinalIgnoreCase);
                                int p2 = -1;
                                if (p1 > 0)
                                {
                                    p1 = p1 + 6;
                                    p2 = strValue.IndexOf('"', p1);
                                }
                                if ((index < p1) && (p1 < p2) && (p2 < end))
                                {
                                    string str = strValue.Substring(p1, p2 - p1);
                                    links.Add(strValue.Substring(p1, p2 - p1));
                                }
                            }
                            index = end;
                        }
                        else if ((index + 4 < length) && strValue.Substring(index, 5) == "<img ")
                        {
                            int end = strValue.IndexOf('>', index);
                            if (end > 0)
                            {
                                int p1 = strValue.IndexOf("src=" + '"', index, StringComparison.OrdinalIgnoreCase);
                                int p2 = -1;
                                if (p1 > 0)
                                {
                                    p1 = p1 + 5;
                                    p2 = strValue.IndexOf('"', p1);
                                }
                                if ((index < p1) && (p1 < p2) && (p2 < end))
                                {
                                    string str = strValue.Substring(p1, p2 - p1);
                                    links.Add(strValue.Substring(p1, p2 - p1));
                                }
                            }
                            index = end;
                        }
                    }
                    index++;
                }
                StringBuilder builder = new StringBuilder(strValue);
                foreach (string link in links)
                {
                    string newLink = AveReplaceProcessor.UrlReplace(link, mAveParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true), mAveParentSite.SourceSiteInfo, mAveParentSite.ServerRelativeUrl);
                    builder.Replace(link, newLink);
                }
                return builder.ToString();
#if PerformanceLog
            }
#endif
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

        /// <summary>
        /// Here we should handle the fields whose value saved in metaInfo.
        /// </summary>
        /// <param name="docRowId"></param>
        /// <param name="version"></param>
        /// <param name="metaInfoDic"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public Dictionary<string, object> GetFieldValuesInMetaInfo(int docRowId, int version, Dictionary<string, string> metaInfoDic, Guid webId, Guid parentListId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPListFieldCollection.GetFieldValuesInMetaInfo"))
            {
#endif

                Dictionary<string, object> fieldsInMetaInfo = null;
                IAveFieldCollection tempCollection = mAveSPList.SPList.Fields;
                foreach (KeyValuePair<string, string> ent in metaInfoDic)
                {
                    string value = ent.Value;
                    string key = ent.Key;
                    if (tempCollection.ContainsField(key))
                    {
                        IAveField sf = tempCollection.GetField(key);
                        #region Lookup field with AllowMultipleValues is ture
                        if (sf is IAveFieldLookup && (sf as IAveFieldLookup).AllowMultipleValues)
                        {
                            IAveFieldLookup sLookUp = sf as IAveFieldLookup;
                            try
                            {
                                AveSPSite tempAveSite = mAveSPWeb.ParentSite;
                                AveLookupObject obj = tempAveSite.MappingManager.SiteMappingManager.GetLookupFieldMapping(parentListId, sf.ID);
                                if (obj != null)
                                {
                                    ArrayList valueList = new ArrayList();
                                    if (fieldsInMetaInfo == null)
                                    {
                                        fieldsInMetaInfo = new Dictionary<string, object>();
                                    }
                                    string[] lookupValues = value.Split(new string[] { ";#" }, StringSplitOptions.None);
                                    foreach (string v in lookupValues)
                                    {
                                        int lookupRowId;
                                        if (Int32.TryParse(v, out lookupRowId))
                                        {
                                            valueList.Add(lookupRowId);
                                        }
                                    }

                                    Guid listId = new Guid(sLookUp.LookupList);
                                    listId = tempAveSite.MappingManager.SiteMappingManager.GetListIdMapping(new Guid(obj.List));
                                    if (!listId.Equals(Guid.Empty))
                                    {
                                        IAveFieldLookupValueCollection lookupCol = mAveParentSite.ObjectModelFactory.CreateFieldLookupValueCollection();
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
                                        tempAveSite.MappingManager.SiteMappingManager.AddNotUpdateLookupFieldValue(new Guid(obj.List), webId, parentListId, docRowId, version, sf.ID, valueList);
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
#if PerformanceLog
            }
#endif
        }

        public void RestoreListFieldIndexes()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPListFieldCollection.RestoreListFieldIndexes"))
            {
#endif
                if (mAveSPList == null || mAveSPList.SPList == null)
                {
                    return;
                }
                IAveFieldCollection fieldCollection = mAveSPList.SPList.Fields;
                IAveFieldIndexCollection fieldIndexes = mAveSPList.SPList.FieldIndexes;
                foreach (List<Guid> indexField in ListFieldIndexesCache.Values)
                {
                    try
                    {
                        Guid firstFieldId = FieldMapping.GetMappingRestoredFieldId(indexField[0]) != Guid.Empty ? FieldMapping.GetMappingRestoredFieldId(indexField[0]) : indexField[0];
                        Guid secondFieldId = FieldMapping.GetMappingRestoredFieldId(indexField[1]) != Guid.Empty ? FieldMapping.GetMappingRestoredFieldId(indexField[1]) : indexField[1];
                        fieldIndexes.Add(fieldCollection[firstFieldId], fieldCollection[secondFieldId]);
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, string.Format("An error occurred while add field to FieldIndexes.List ID:{0}. error message:{1}", mAveSPList.SPList.ID, e));
                    }
                }
                ListFieldIndexesCache.Clear();
#if PerformanceLog
            }
#endif
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint query")]
        public void RestoreListFieldOrder()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPListFieldCollection.RestoreListFieldOrder"))
            {
#endif
                try
                {
                    if (mAveSPList == null || mAveSPList.SPList == null /*|| mAveParentSite.SPContextKind == AveContextKind.ClientObjectModel*/)
                    {
                        return;
                    }
                    if (SourceFieldOrder.Count <= 0)
                        return;

                    List<string> destFieldOrder = LoadDestFieldOrder(mAveSPList.SPList.Fields.GetFields(mAveSPList.ParentWeb.SPWeb.ID, mAveSPList.SPList.ID));

                    if (destFieldOrder != null && destFieldOrder.Count == SourceFieldOrder.Count)
                    {
                        bool needRestoreOrder = false;
                        for (int i = 0; i < SourceFieldOrder.Count; i++)
                        {
                            if (!string.Equals(SourceFieldOrder[i], destFieldOrder[i]))
                            {
                                needRestoreOrder = true;
                                break;
                            }
                        }
                        if (!needRestoreOrder)
                        {
                            return;
                        }
                    }
                    IAveFieldCollection fieldCollection = mAveSPList.SPList.Fields;
                    List<string> mappedSourceFields = new List<string>();
                    if (mXmlFields != null)
                    {
                        foreach (string sourceFieldInternalName in SourceFieldOrder)
                        {
                            string fieldIntername = FieldMapping.GetMappingRestoredFieldInternalName(sourceFieldInternalName);
                            if (string.IsNullOrEmpty(fieldIntername))
                                fieldIntername = sourceFieldInternalName;
                            if (fieldCollection.ContainsFieldWithInternalName(fieldIntername) && !mappedSourceFields.Contains(fieldIntername))
                            {
                                mappedSourceFields.Add(fieldIntername);
                            }
                        }
                        IEnumerable<IAveField> destinationFields = fieldCollection.Where(field => !mappedSourceFields.Contains(field.InternalName));
                        foreach (IAveField item in destinationFields)
                        {
                            if (!mappedSourceFields.Contains(item.InternalName))
                                mappedSourceFields.Add(item.InternalName);
                        }
                        #region Client Api not support reorder list fields, so use web request
                        //                        const string rpcMethod =
                        //                            @"<?xml version=""1.0"" encoding=""UTF-8""?>  
                        //	                            <Method ID=""0,REORDERFIELDS"">  
                        //	                            <SetList Scope=""Request"">{0}</SetList>  
                        //	                            <SetVar Name=""Cmd"">REORDERFIELDS</SetVar>  
                        //	                            <SetVar Name=""ReorderedFields"">{1}</SetVar>  
                        //	                            <SetVar Name=""owshiddenversion"">{2}</SetVar>  
                        //	                            </Method>";
                        //                        StringBuilder sb = new StringBuilder();
                        //                        XmlTextWriter xmlWriter = new XmlTextWriter(new StringWriter(sb));
                        //                        xmlWriter.Formatting = Formatting.Indented;
                        //                        xmlWriter.WriteStartElement("Fields");
                        //                        for (int i = 0; i < mappedSourceFields.Count; i++)
                        //                        {
                        //                            xmlWriter.WriteStartElement("Field");
                        //                            xmlWriter.WriteAttributeString("Name", mappedSourceFields[i]);
                        //                            xmlWriter.WriteEndElement();
                        //                        }
                        //                        xmlWriter.WriteEndElement();
                        //                        xmlWriter.Flush();
                        //                        string rpcCall = string.Format(rpcMethod, mAveSPList.SPList.ID, HttpUtility.HtmlEncode(sb.ToString()),
                        //                                                      mAveSPList.SPList.Version);
                        //                        mAveSPList.ParentWeb.SPWeb.AllowUnsafeUpdates = true;
                        //                        string result = mAveSPList.ParentWeb.SPWeb.ProcessBatchData(rpcCall);
                        //                        XmlDocument xdoc = new XmlDocument();
                        //                        xdoc.LoadXml(result);
                        //                        if (xdoc.DocumentElement.HasAttribute("Code") && xdoc.DocumentElement.GetAttribute("Code").ToString().Equals("0"))
                        //                        {
                        //                            log.Debug("reorder list fields successful.");
                        //                        }
                        //                        else
                        //                        {
                        //                            log.Warn("reorder list fields failed.Exception:" + xdoc.InnerText);
                        //                        } 
                        #endregion
                        mAveSPList.ReorderListFields(mappedSourceFields);
                        mAveSPList.ParentWeb.ReloadWeb();
                        mAveSPList.ReloadList();
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while reorder list fields.List ID:{0}. error message:{1}", mAveSPList.SPList.ID, e));
                }
#if PerformanceLog
            }
#endif
        }

        /// <summary>
        /// replace audience id setting
        /// </summary>
        /// <param name="audienceIdMapping"></param>
        /// <param name="oldValue"></param>
        /// <returns></returns>
        private string ReplaceAudienceId(Dictionary<string, string> audienceIdMapping, string oldValue)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPListFieldCollection.ReplaceAudienceId"))
            {
#endif
                if (string.IsNullOrEmpty(oldValue))
                {
                    return oldValue;
                }
                if (oldValue.IndexOf(";;", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    return oldValue;
                }
                string tempValue = oldValue.Substring(0, oldValue.IndexOf(";;", StringComparison.OrdinalIgnoreCase));
                if (string.IsNullOrEmpty(tempValue))
                {
                    return oldValue;
                }
                string newValue = oldValue;
                string[] tValues = tempValue.Split(',');
                foreach (string tValue in tValues)
                {
                    if (audienceIdMapping.ContainsKey(tValue))
                    {
                        newValue = newValue.Replace(tValue, audienceIdMapping[tValue]);
                    }
                }
                return newValue;
#if PerformanceLog
            }
#endif
        }
    }

    public class AveFieldMultiColumnValue
    {
        // Fields
        private List<string> m_subColumnValues;

    // Methods
    public AveFieldMultiColumnValue()
        {
            this.m_subColumnValues = new List<string>();
        }

        public AveFieldMultiColumnValue(int numberOfSubColumns)
        {
            this.m_subColumnValues = new List<string>(numberOfSubColumns);
            for (int i = 0; i < numberOfSubColumns; i++)
            {
                this.Add(string.Empty);
            }
        }

        public AveFieldMultiColumnValue(string fieldValue)
        {
            this.m_subColumnValues = ParseMultiColumnValue(fieldValue);
        }

        public void Add(string subColumnValue)
        {
            this.m_subColumnValues.Add(subColumnValue);
        }

        internal static string ConvertMultiColumnValueToString(List<string> subColumnValues, bool bAddLeadingTailingDelimiter)
        {
            return ConvertMultiColumnValueToString(subColumnValues, bAddLeadingTailingDelimiter, false);
        }

        internal static string ConvertMultiColumnValueToString(List<string> subColumnValues, bool bAddLeadingTailingDelimiter, bool bPreserveEmpty)
        {
            bool flag = false;
            StringBuilder builder = new StringBuilder(0xff);
            for (int i = 0; i < subColumnValues.Count; i++)
            {
                string str = subColumnValues[i];
                if (!string.IsNullOrEmpty(str))
                {
                    str = str.Replace(";", ";;");
                }
                if (!string.IsNullOrEmpty(str))
                {
                    flag = true;
                }
                if (bAddLeadingTailingDelimiter || (i != 0))
                {
                    builder.Append(";#");
                }
                builder.Append(str);
            }
            if (!flag && !bPreserveEmpty)
            {
                return string.Empty;
            }
            if (bAddLeadingTailingDelimiter)
            {
                builder.Append(";#");
            }
            return builder.ToString();
        }

        internal static List<string> ParseMultiColumnValue(string fieldValue)
        {
            return ParseMultiColumnValue(fieldValue, DelimiterType.Internal);
        }

        internal static List<string> ParseMultiColumnValue(string fieldValue, DelimiterType delimiterType)
        {
            return ParseMultiColumnValue(fieldValue, delimiterType, false);
        }

        internal static List<string> ParseMultiColumnValue(string fieldValue, DelimiterType delimiterType, bool bIncludeEmpty)
        {
            List<string> subColumnValues = null;
            if (!TryParseMultiColumnValue(fieldValue, delimiterType, bIncludeEmpty, out subColumnValues))
            {
                throw new ArgumentException();
            }
            return subColumnValues;
        }

        public override string ToString()
        {
            return ConvertMultiColumnValueToString(this.m_subColumnValues, true);
        }

        internal static bool TryParseMultiColumnValue(string fieldValue, DelimiterType delimiterType, bool bIncludeEmpty, out List<string> subColumnValues)
        {
            subColumnValues = new List<string>();
            if (!string.IsNullOrEmpty(fieldValue))
            {
                string str = (delimiterType == DelimiterType.Internal) ? ";#" : ",#";
                if (str.Length != 2)
                {
                    return false;
                }
                char c = str[0];
                char ch2 = str[1];
                string oldValue = new string(c, 2);
                string newValue = new string(c, 1);
                int startIndex = 0;
                if (fieldValue.StartsWith(str, StringComparison.Ordinal))
                {
                    if (bIncludeEmpty)
                    {
                        subColumnValues.Add(string.Empty);
                    }
                    startIndex = str.Length;
                }
                int num2 = startIndex;
                bool flag = false;
                while (num2 < fieldValue.Length)
                {
                    if (fieldValue[num2] == c)
                    {
                        num2++;
                        if (num2 < fieldValue.Length)
                        {
                            if (fieldValue[num2] != ch2)
                            {
                                if (fieldValue[num2] != c)
                                {
                                    return false;
                                }
                                num2++;
                                flag = true;
                            }
                            else
                            {
                                if ((num2 - 1) > startIndex)
                                {
                                    string item = fieldValue.Substring(startIndex, (num2 - startIndex) - 1);
                                    if (flag)
                                    {
                                        item = item.Replace(oldValue, newValue);
                                    }
                                    subColumnValues.Add(item);
                                    flag = false;
                                }
                                else
                                {
                                    subColumnValues.Add(string.Empty);
                                }
                                num2++;
                                startIndex = num2;
                            }
                            continue;
                        }
                        break;
                    }
                    num2++;
                }
                if (num2 > startIndex)
                {
                    string str5 = fieldValue.Substring(startIndex, num2 - startIndex);
                    if (flag)
                    {
                        str5 = str5.Replace(oldValue, newValue);
                    }
                    subColumnValues.Add(str5);
                }
                else if (bIncludeEmpty)
                {
                    subColumnValues.Add(string.Empty);
                }
            }
            return true;
        }

        // Properties
        public List<string> ColumnValues
        {
            get
            {
                return this.m_subColumnValues;
            }
        }

        public int Count
        {
            get
            {
                return this.m_subColumnValues.Count;
            }
        }

        public static string Delimiter
        {
            get
            {
                return ";#";
            }
        }

        public string this[int index]
        {
            get
            {
                if (this.m_subColumnValues.Count == 0)
                {
                    return string.Empty;
                }
                return this.m_subColumnValues[index];
            }
            set
            {
                this.m_subColumnValues[index] = value;
            }
        }
    }

    internal enum DelimiterType
    {
        Internal,
        InternalSub
    }

}
