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
using System.Xml;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource;

namespace AvePoint.Wrapper.Backup
{
    public class AveSPFieldInfo
    {
        private string mFiledXml = null;

        public string FieldXml
        {
            get
            {
                return mFiledXml;
            }
        }

        /// <summary>
        /// Create an object of AveSPFieldCollection and get SPFieldCollection information using SQL.
        /// </summary>
        //public static AveSPFieldCollection CreateInstance(object obj)
        //{
        //    return CreateInstance(obj, false);
        //}
        /// <summary>
        /// Create an object of AveSPFieldCollection and get SPFieldCollection information.
        /// </summary>
        /// <param name="obj">Object of AveSPWeb or AveSPList</param>
        /// <param name="getFullSchema">Get SPFieldCollection information using API if true, or using SQL.</param>
        /// <returns>Object of AveSPFieldCollection</returns>
        public static AveSPFieldCollection CreateInstance(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentException("There argument cannot be null");
            }
            if (obj is AveSPWeb)
            {
                return new AveSPWebFieldCollection((AveSPWeb)obj);
            }
            else if (obj is AveSPList)
            {
                return new AveSPListFieldCollection((AveSPList)obj);
            }
            else
            {
                throw new ArgumentException(string.Format("Unknown object type:{0}", obj.GetType().FullName));
            }
        }
    }

    public abstract class AveSPFieldCollection
    {
        protected static AveLogger log = AveLogger.GetInstance(typeof(AveSPFieldCollection));
        protected Hashtable mListTable = new Hashtable();
        protected Hashtable mWebTable = new Hashtable();

        private const string RowOrdinal = "RowOrdinal";
        private const string ColName = "ColName";
        private const string Name = "DisplayName";
        protected AveSPSite mAveParentSite;

        public AveSPSite ParentSite
        {
            get { return mAveParentSite; }
        }

        public IAveFieldCollection SPFieldCollection { get; set; }

        public abstract Dictionary<string, AveSPField> FieldMap { get; }

        protected Dictionary<string, object> NameToColName = new Dictionary<string, object>();

        public abstract void Export(IAveBackupStream output);

        public abstract void Export(IAveBackupStream output, AveBackupOption backupColumnOption);

        public abstract void ExportFullSchema(IAveBackupStream output);

        public abstract AveFieldCollectionInfo GetFieldInfoObj();

        public abstract AveFieldCollectionInfo GetFieldInfoObj(AveBackupOption backupOption);

        protected IAveBackupRestoreQueryService mQueryService;

        public static AveSPFieldCollection CreateInstance(object obj)
        {
            switch (obj.GetType().Name)
            {
                case "AveSPWeb":
                    return new AveSPWebFieldCollection((AveSPWeb)obj);
                case "AveSPList":
                    return new AveSPListFieldCollection((AveSPList)obj);
                default:
                    throw new ArgumentException(string.Format("Unknown object type:{0}", obj.GetType().FullName));
            }
        }

        private HashSet<string> GetDisplayFields(IAveViewFieldCollection viewFields)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPFieldCollection.GetDisplayFields"))
            {
                HashSet<string> displayFields = new HashSet<string>();
                foreach (string displayField in SPFieldCollection.GetDisplayFields(viewFields).Keys)
                {
                    displayFields.Add(displayField);
                }
                return displayFields;
            }
        }

        private HashSet<string> GetDisplayFields(string viewFieldsSchema)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPFieldCollection.GetDisplayFields_1"))
            {
                HashSet<string> displayFields = new HashSet<string>();
                foreach (string displayField in SPFieldCollection.GetDisplayFields(viewFieldsSchema).Keys)
                {
                    displayFields.Add(displayField);
                }
                return displayFields;
            }
        }

        protected void Load(IAveFieldCollection fields, IAveViewFieldCollection viewFields, Dictionary<string, AveSPField> fieldMap, Dictionary<Guid, AveSPField> idFieldMap)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPFieldCollection.Load"))
            {
                fieldMap.Clear();
                if (idFieldMap != null)
                {
                    idFieldMap.Clear();
                }
                XmlDocument mXDoc = new XmlDocument();
                HashSet<string> displayFields = GetDisplayFields(viewFields);
                Dictionary<string, string> fieldList = fields.GetFieldMap(fields);
                foreach (IAveField spField in fields)
                {
                    try
                    {
                        //每次获取schemaXml都需要load一次本地文件，增加了许多次IO读取影响效率,所以增加接口来获取field的文件路径
                        //再通过一次load来获取field schemaXml放在字典中，用的时候直接取
                        //mXDoc.InnerXml = spField.SchemaXml;
                        mXDoc.InnerXml = fieldList[spField.ID.ToString()];
                        XmlElement xmlFirstChild = (XmlElement)mXDoc.FirstChild;
                        bool isDisplayColumn = displayFields.Contains(spField.StaticName);
                        if (xmlFirstChild.HasAttribute(ColName))
                        {
                            string column = xmlFirstChild.Attributes[ColName].Value;
                            string row;
                            if (xmlFirstChild.HasAttribute(RowOrdinal))
                            {
                                row = xmlFirstChild.Attributes[RowOrdinal].Value;
                            }
                            else
                            {
                                row = "0";
                            }

                            AveSPField aveField = new AveSPField(spField.InternalName, spField.Title, column, isDisplayColumn, spField.Hidden, spField.Type);
                            fieldMap[row + column] = aveField;

                            if (xmlFirstChild.HasAttribute(Name))
                            {
                                string ncolumn = xmlFirstChild.Attributes[Name].Value;
                                NameToColName[ncolumn] = column;
                            }
                            if (idFieldMap != null)
                            {
                                idFieldMap[spField.ID] = aveField;
                            }
                            // the other column of the list field
                            uint i = 2;
                            while (true)
                            {
                                string colName = ColName + i;
                                if (!xmlFirstChild.HasAttribute(colName))
                                {
                                    break;
                                }
                                string rowOrdinal = RowOrdinal + i;
                                if (xmlFirstChild.HasAttribute(rowOrdinal))
                                {
                                    row = xmlFirstChild.Attributes[rowOrdinal].Value;
                                }
                                else
                                {
                                    row = "0";
                                }
                                column = xmlFirstChild.Attributes[colName].Value;
                                fieldMap[row + column] = new AveSPField(spField.InternalName + AveConstants.FIELD_SEPARATOR + i, spField.Title, column, false, spField.Hidden);
                                i++;
                            }
                        }
                        else if (idFieldMap != null)
                        {
                            idFieldMap[spField.ID] = new AveSPField(spField.InternalName, spField.Title, null, isDisplayColumn, spField.Hidden, spField.Type);
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, string.Format("An error occurred while load fields. field title:{0} \n error message:{1}", spField.Title, e));
                    }
                }
            }
        }

        protected void Load(string fieldSchema, string viewFields, Dictionary<string, AveSPField> fieldMap, Dictionary<Guid, AveSPField> idFieldMap)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPFieldCollection.Load_1"))
            {
                fieldMap.Clear();

                if (idFieldMap != null)
                {
                    idFieldMap.Clear();
                }
                XmlDocument mXDoc = new XmlDocument();
                HashSet<string> displayFields = GetDisplayFields(viewFields);
                List<string> fields = GetFieldsFromSchema(fieldSchema);
                foreach (string field in fields)
                {
                    try
                    {
                        mXDoc.InnerXml = field;
                        XmlElement xmlFirstChild = (XmlElement)mXDoc.FirstChild;
                        if (xmlFirstChild.HasAttribute(ColName))
                        {
                            if (!xmlFirstChild.HasAttribute("Name") || !xmlFirstChild.HasAttribute("DisplayName"))
                            {   //当field Schema中不存在displayname时，表示该field为从未编辑的build-in field，不需要备份。
                                continue;
                            }
                            string column = xmlFirstChild.Attributes[ColName].Value;
                            string row;
                            if (xmlFirstChild.HasAttribute(RowOrdinal))
                            {
                                row = xmlFirstChild.Attributes[RowOrdinal].Value;
                            }
                            else
                            {
                                row = "0";
                            }
                            string name = xmlFirstChild.Attributes["Name"].Value;
                            string displayName = xmlFirstChild.HasAttribute("DisplayName") ? xmlFirstChild.Attributes["DisplayName"].Value : name;
                            bool isDisplayColumn = displayFields.Contains(name);
                            bool isHidden = String.Equals(bool.TrueString, xmlFirstChild.GetAttribute("Hidden"), StringComparison.OrdinalIgnoreCase);
                            AveSPField aveField = new AveSPField(name, displayName, column, isDisplayColumn, isHidden);
                            fieldMap[row + column] = aveField;
                            if (idFieldMap != null && xmlFirstChild.HasAttribute("ID"))
                            {
                                idFieldMap[new Guid(xmlFirstChild.Attributes["ID"].Value)] = aveField;
                            }
                            // the other column of the list field
                            uint i = 2;
                            while (true)
                            {
                                string colName = ColName + i;
                                if (!xmlFirstChild.HasAttribute(colName))
                                {
                                    break;
                                }
                                string rowOrdinal = RowOrdinal + i;
                                if (xmlFirstChild.HasAttribute(rowOrdinal))
                                {
                                    row = xmlFirstChild.Attributes[rowOrdinal].Value;
                                }
                                else
                                {
                                    row = "0";
                                }
                                column = xmlFirstChild.Attributes[colName].Value;
                                fieldMap[row + column] = new AveSPField(name + AveConstants.FIELD_SEPARATOR + i, displayName, column, false, isHidden);
                                i++;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, string.Format("An error occurred while load field. field:{0} \n error message:{1}", field, e));
                    }
                }
            }
        }

        //TODO...Is there any need to implement the methods which only called in private area.
        //because these methods have been implemented in afterward.
        //private bool GetRelationship(AveSPWeb aveWeb, AveSPList aveList,  XmlElement FieldNode)
        //{
        //
        //}

        protected List<string> GetFieldsFromSchema(string fieldSchema)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPFieldCollection.GetFieldsFromSchema"))
            {
                List<string> fields = new List<string>();
                try
                {
                    fields = SPFieldCollection.GetFieldsFromSchema(fieldSchema);
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while get fields from schema. error message:{0}", e));
                }
                return fields;
            }
        }

        /// <summary>
        /// used to show mapping of field's the display name and column name in AllUserData
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> GetNameToColNameMapping()
        {
            return NameToColName;
        }

        public string TransListIdToTitle(IAveWeb aveWeb, IAveList aveList, string xml)
        {
            return SPFieldCollection.TransListIdToTitle(aveWeb, aveList, xml);
        }

        public virtual void Dispose()
        {
        }
    }

    public class AveSPWebFieldCollection : AveSPFieldCollection
    {
        private AveSPWeb mAveSPWeb;

        public AveSPWebFieldCollection(AveSPWeb aveSPWeb)
        {
            mAveSPWeb = aveSPWeb;
            mAveParentSite = aveSPWeb.ParentSite;
            SPFieldCollection = mAveSPWeb.SPWeb.Fields;
        }

        public override void Export(IAveBackupStream output)
        {
            this.Export(output, new AveBackupOption());
        }
        public override void Export(IAveBackupStream output, AveBackupOption backupColumnOption)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPWeb.Fields"))
            {
                AveFieldCollectionInfo fieldCollectionInfo = GetFieldInfoObj(backupColumnOption);
                if (fieldCollectionInfo.RelatedMetadataInfo.Count > 0)
                {
                    output.WriteMetadata(AveMetadataType.MetadataService, fieldCollectionInfo.RelatedMetadataInfo);
                }
                output.WriteMetadata(AveMetadataType.WebField, fieldCollectionInfo.AveSchemaXml);
            }
        }

        public override AveFieldCollectionInfo GetFieldInfoObj()
        {
            return SPFieldCollection.GetFieldInfoObj(new AveBackupOption());
        }
        public override AveFieldCollectionInfo GetFieldInfoObj(AveBackupOption backupColumnOption)
        {
            return SPFieldCollection.GetFieldInfoObj(backupColumnOption);
        }

        public override Dictionary<string, AveSPField> FieldMap
        {
            get { return null; }
        }

        public override void Dispose()
        {
            //TODO
        }

        public override void ExportFullSchema(IAveBackupStream output)
        {
            throw new NotImplementedException();
        }

        public List<string> GetFields(Guid siteId, string scope)
        {
            return SPFieldCollection.GetFields(siteId, scope);
        }
    }

    public class AveSPListFieldCollection : AveSPFieldCollection
    {
        private static readonly Dictionary<string, string> SKIP_FIELD_MAP = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> NEED_FIELD_MAP = new Dictionary<string, string>();

        private AveSPList mAveSPList;
        private Dictionary<string, AveSPField> mFieldMap;
        private Dictionary<Guid, AveSPField> mIdFieldMap;
        private string mListFieldsSchema;
        private string mListViewFieldsSchema;

        static AveSPListFieldCollection()
        {
            SKIP_FIELD_MAP["tp_ID"] = "#tp_ID";
            SKIP_FIELD_MAP["tp_ListId"] = "#tp_ListId";
            SKIP_FIELD_MAP["tp_SiteId"] = "#tp_SiteId";
            SKIP_FIELD_MAP["tp_RowOrdinal"] = "#tp_RowOrdinal";
            SKIP_FIELD_MAP["tp_Version"] = "#tp_Version";
            SKIP_FIELD_MAP["tp_Ordering"] = "#tp_Ordering";
            SKIP_FIELD_MAP["tp_ThreadIndex"] = "#tp_ThreadIndex";
            SKIP_FIELD_MAP["tp_HasAttachment"] = "#tp_HasAttachment";
            SKIP_FIELD_MAP["tp_ModerationStatus"] = "#tp_ModerationStatus";
            SKIP_FIELD_MAP["tp_IsCurrent"] = "#tp_IsCurrent";
            SKIP_FIELD_MAP["tp_ItemOrder"] = "#tp_ItemOrder";
            SKIP_FIELD_MAP["tp_InstanceID"] = "#tp_InstanceID";
            SKIP_FIELD_MAP["tp_GUID"] = "#tp_GUID";
            SKIP_FIELD_MAP["tp_CopySource"] = "#tp_CopySource";
            SKIP_FIELD_MAP["tp_HasCopyDestinations"] = "#tp_HasCopyDestinations";
            SKIP_FIELD_MAP["tp_AuditFlags"] = "#tp_AuditFlags";
            SKIP_FIELD_MAP["tp_InheritAuditFlags"] = "#tp_InheritAuditFlags";
            SKIP_FIELD_MAP["tp_Size"] = "#tp_Size";
            SKIP_FIELD_MAP["tp_WorkflowVersion"] = "#tp_WorkflowVersion";
            SKIP_FIELD_MAP["tp_WorkflowInstanceID"] = "#tp_WorkflowInstanceID";
            SKIP_FIELD_MAP["tp_ParentId"] = "#tp_ParentId";
            SKIP_FIELD_MAP["tp_DocId"] = "#tp_DocId";
            SKIP_FIELD_MAP["tp_DeleteTransactionId"] = "#tp_DeleteTransactionId";
            SKIP_FIELD_MAP["tp_ContentTypeId"] = "#tp_ContentTypeId";
            //SKIP_FIELD_MAP["uniqueidentifier1"] = "#uniqueidentifier1";
            SKIP_FIELD_MAP["tp_Level"] = "#tp_Level";
            SKIP_FIELD_MAP["tp_IsCurrentVersion"] = "#tp_IsCurrentVersion";
            SKIP_FIELD_MAP["tp_UIVersion"] = "#tp_UIVersion";
            SKIP_FIELD_MAP["tp_CalculatedVersion"] = "#tp_CalculatedVersion";
            SKIP_FIELD_MAP["tp_UIVersionString"] = "#tp_UIVersionString";
            SKIP_FIELD_MAP["tp_DraftOwnerId"] = "#tp_DraftOwnerId";

            NEED_FIELD_MAP["tp_Editor"] = "Editor";
            NEED_FIELD_MAP["tp_Author"] = "Author";
            NEED_FIELD_MAP["tp_Modified"] = "Modified";
            NEED_FIELD_MAP["tp_Created"] = "Created";
        }

        public AveSPListFieldCollection(AveSPList aveSPList)
        {
            mAveSPList = aveSPList;
            mAveParentSite = aveSPList.ParentWeb.ParentSite;
            mQueryService = mAveSPList.ParentWeb.ParentSite.QueryService;
            SPFieldCollection = mAveSPList.SPList.Fields;
            mFieldMap = new Dictionary<string, AveSPField>();
            mIdFieldMap = new Dictionary<Guid, AveSPField>();
            Load();
        }
        public override void Export(IAveBackupStream output)
        {
            this.Export(output, new AveBackupOption());
        }
        public override void Export(IAveBackupStream output, AveBackupOption backupColumnOption)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPListFieldCollection.Export"))
            {
                AveFieldCollectionInfo fieldCollectionInfo = GetFieldInfoObj(backupColumnOption);

                if (fieldCollectionInfo.RelatedMetadataInfo.Count > 0)
                {
                    output.WriteMetadata(AveMetadataType.MetadataService, fieldCollectionInfo.RelatedMetadataInfo);
                }

                output.WriteMetadata(AveMetadataType.ListField, fieldCollectionInfo.AveSchemaXml);
            }
        }

        public override AveFieldCollectionInfo GetFieldInfoObj()
        {
            return SPFieldCollection.GetFieldInfoObj(new AveBackupOption());
        }
        public override AveFieldCollectionInfo GetFieldInfoObj(AveBackupOption backupColumnOption)
        {  
            return SPFieldCollection.GetFieldInfoObj(backupColumnOption, mAveSPList.SPList,mListFieldsSchema);
        }

        public override Dictionary<string, AveSPField> FieldMap
        {
            get { return mFieldMap; }
        }

        public Dictionary<Guid, AveSPField> IdFieldMap
        {
            get { return mIdFieldMap; }
        }

        public void Load()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPListFieldCollection.Load"))
            {
                IAveList spList = mAveSPList.SPList;
                if (spList == null)
                {
                    return;
                }
                mListFieldsSchema = GetFields(mAveSPList.ParentWeb.SPWeb.ID, mAveSPList.SPList.ID);
                string name = spList.Title;
                try
                {
                    name = spList.DefaultViewUrl;
                    if (spList.DefaultView != null)
                    {
                        mListViewFieldsSchema = spList.DefaultView.ViewFields.SchemaXml;
                        Load(spList.Fields, spList.DefaultView.ViewFields, mFieldMap, mIdFieldMap);
                    }
                    else
                    {
                        mListViewFieldsSchema = GetViewFields(mAveSPList.ParentWeb.ParentSite.SPSite.ID, mAveSPList.SPList.ID);
                        Load(mListFieldsSchema, mListViewFieldsSchema, mFieldMap, mIdFieldMap);
                    }
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, WrapperBackupResource.AWBGetDefaultViewError, name, spList.ID.ToString(), spList.ParentWeb.Url, spList.ParentWeb.ID.ToString(), ex.ToString());
                }
            }
        }

        private string GetViewFields(Guid siteId, Guid listId)
        {
            return SPFieldCollection.GetViewFields(siteId, listId);
        }

        private string GetFields(Guid webId, Guid listId)
        {
            return SPFieldCollection.GetFields(webId, listId);
        }

        public string GetFields(IAveList list)
        {
            return list.Fields.SchemaXml;
        }

        public string GetViewSchema(IAveList list)
        {
            return list.GetListViewSchema(mAveSPList.ParentWeb.ParentSite.SPSite.ID, mAveSPList.Id);
        }

        public void ReplaceFieldNames(Dictionary<string, object> oldData, Dictionary<string, object> newData, byte rowOrdinal)
        {
            AveSPField field;
            string name;
            foreach (KeyValuePair<string, object> pair in oldData)
            {
                if (SKIP_FIELD_MAP.TryGetValue(pair.Key, out name))
                {
                    newData[name] = pair.Value;
                    continue;
                }
                if (NEED_FIELD_MAP.TryGetValue(pair.Key, out name))
                {
                    newData[name] = pair.Value;
                    continue;
                }
                name = rowOrdinal + pair.Key;
                if (mFieldMap.TryGetValue(name, out field))
                {
                    newData[field.BackupName] = pair.Value;
                }
                else if (rowOrdinal > 0)
                {
                    continue;
                }
                else
                {
                    newData[AveConstants.FIELD_SEPARATOR + pair.Key] = pair.Value;
                }
            }
        }

        public override void Dispose()
        {
            //TODO
        }

        public override void ExportFullSchema(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPListFieldCollection.ExportFullSchema"))
            {
                if (mAveSPList == null || mAveSPList.SPList == null)
                {
                    return;
                }
                output.WriteMetadata(AveMetadataType.FullSchemaXml, mAveSPList.SPList.Fields.SchemaXml);
            }
        }
    }

    public class AveSPField
    {
        public Guid FieldId { get; private set; }
        public string FieldTypeAsString { get; private set; }

        public bool IsDisplayColumn { get; private set; }

        public string ColumnName { get; private set; }

        public string BackupName { get; private set; }

        public AveFieldType FieldType { get; private set; }

        public bool IsHidden { get; private set; }

        public string DisplayName { get; private set; }

        public AveSPField() { }

        public AveSPField(string backupName, string displayName, string columnName, bool isDisplayColumn, bool isHidden)
            : this(backupName, displayName, columnName, isDisplayColumn, isHidden, AveFieldType.Invalid) { }

        public AveSPField(string backupName, string displayName, string columnName, bool isDisplayColumn, bool isHidden, AveFieldType fieldType)
        {
            this.BackupName = backupName;
            this.ColumnName = columnName;
            this.IsDisplayColumn = isDisplayColumn;
            this.IsHidden = isHidden;
            this.FieldType = fieldType;
            this.DisplayName = displayName;
        }
    }

    public class FieldInternalTypeAndGuiTypeMapping
    {
        private static Dictionary<string, string> typeMappings = new Dictionary<string, string>();

        private static void InitializeTypeMappings()
        {
            typeMappings.Add("Text", "Single line of text");
            typeMappings.Add("Note", "Multiple lines of text");
            typeMappings.Add("Choice", "Choice (menu to choose from)");
            typeMappings.Add("MultiChoice", "Choice (menu to choose from)_AllowMultiple");
            typeMappings.Add("Number", "Number (1, 1.0, 100)");
            typeMappings.Add("Currency", "Currency ($, ¥, €)");
            typeMappings.Add("DateTime", "Date and Time");
            typeMappings.Add("Lookup", "Lookup (information already on this site)");
            typeMappings.Add("LookupMulti", "Lookup (information already on this site)_AllowMultiple");
            typeMappings.Add("Boolean", "Yes/No (check box)");
            typeMappings.Add("User", "Person or Group");
            typeMappings.Add("UserMulti", "Person or Group_AllowMultiple");
            typeMappings.Add("URL", "Hyperlink or Picture");
            typeMappings.Add("Calculated", "Calculated (calculation based on other columns)");
            typeMappings.Add("TaxonomyFieldType", "Managed Metadata");
            typeMappings.Add("TaxonomyFieldTypeMulti", "Managed Metadata_AllowMultiple");
        }

        public static string GetGuiTypeByInternalType(string internalType)
        {
            if (typeMappings.Count == 0)
            {
                InitializeTypeMappings();
            }
            if (typeMappings.ContainsKey(internalType))
            {
                return typeMappings[internalType];
            }
            else
            {
                return internalType;
            }
        }
    }

    public class FieldInfoForExcel
    {
        public string Title { get; set; }

        public string TypeAsString { get; set; }

        public string TitleAndGuiType { get; set; }
    }
}