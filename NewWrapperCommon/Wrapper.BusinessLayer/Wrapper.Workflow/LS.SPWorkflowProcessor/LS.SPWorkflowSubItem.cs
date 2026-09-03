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
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using AvePoint.Common;
using LS.SPWorkflowProcessor.SerializableObjects;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using AvePoint.Wrapper.Common.Office;
using System.Linq;
using AvePoint.Wrapper.Resource.Workflow;

namespace LS.SPWorkflowProcessor
{

    public class SPWorkflowSubItemUnit
    {
        #region Serializable Data
        private Hashtable mProperties;
        private WorkflowSubItemType mType = WorkflowSubItemType.Invalid;
        private List<SPWorkflowSubItemUnit> mChildUnits;
        private SPPermissionUnit mPermissionUnit;
        private SPWorkflowSubItemUnit mParentUnit;
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public string UnitId
        { get; set; }
        public Hashtable Properties
        {
            get
            {
                if (mProperties == null)
                    mProperties = new Hashtable(StringComparer.OrdinalIgnoreCase);
                return mProperties;
            }

        }
        public WorkflowSubItemType ItemType
        {
            get
            {
                return mType;
            }
        }
        public List<SPWorkflowSubItemUnit> ChildUnits
        {
            get
            {
                if (mChildUnits == null)
                    mChildUnits = new List<SPWorkflowSubItemUnit>();
                return mChildUnits;
            }
        }
        public SPPermissionUnit PermissionUnit
        {
            get { return mPermissionUnit; }
            set { mPermissionUnit = value; }
        }
        public SPWorkflowSubItemUnit ParentUnit
        {
            get { return mParentUnit; }
            set { mParentUnit = value; }
        }

        public SPWorkflowSubItemUnit(WorkflowSubItemType type)
        {
            mType = type;
        }

        public SPWorkflowSubItemUnit(WorkflowSubItemType type, SPWorkflowSubItemUnit parentUnit)
        {
            mType = type;
            mParentUnit = parentUnit;
        }

        public void Dispose()
        {
            foreach (SPWorkflowSubItemUnit unit in ChildUnits)
            {
                unit.Dispose();
            }
            this.Properties.Clear();
            mProperties = null;
        }


        #region ************************Backup  Region************************
        public void SetPropsFromDataReader(IAveQueryDataReader sdr, int startIndex, Dictionary<string, string> fieldMap, int curRowOrdinal)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "SetPropsFromDataReader");
            try
            {
                int fieldCount = sdr.FieldCount;
                int i;
                string rowOrdinalStr = curRowOrdinal.ToString();
                string COLOUMN_SET = "tp_ColumnSet";
                for (i = startIndex; i < fieldCount; i++)
                {
                    if (sdr.IsDBNull(i))
                        continue;
                    string backupName = rowOrdinalStr + "_" + sdr.GetName(i).ToLower(CultureInfo.CurrentCulture);
                    StringBuilder b1 = new StringBuilder();
                    int b2 = curRowOrdinal;
                    StringBuilder b3 = new StringBuilder();
                    if (sdr.GetName(i).Equals(COLOUMN_SET, StringComparison.OrdinalIgnoreCase))
                    {
                        ParseColumnSet(sdr.GetValue(i), fieldMap, curRowOrdinal);
                    }
                    else if (fieldMap != null)
                    {
                        if (fieldMap.ContainsKey(backupName))
                        {
                            b1.Append("_");
                            b3.Append("_");
                            b3.Append(fieldMap[backupName]);
                        }
                        else
                        {
                            b1.Append("~");
                            b3.Append("_");
                            b3.Append(sdr.GetName(i));
                        }

                        while (true)
                        {
                            StringBuilder realName = new StringBuilder();
                            realName.Append(b1.ToString());
                            realName.Append(b2.ToString());
                            realName.Append(b3.ToString());
                            if (this.Properties.ContainsKey(realName.ToString()))
                            {
                                if (b1.ToString() == "_" && b2 == 0)
                                    b2++;
                                b2++;
                            }
                            else
                            {
                                this.Properties.AddEx(realName.ToString(), sdr.GetValue(i));
                                break;
                            }
                        }
                    }
                    else
                    {
                        b1.Append("#");
                        b1.Append(sdr.GetName(i));
                        this.Properties.AddEx(b1.ToString(), sdr.GetValue(i));
                    }
                }
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.IP_GetPropertiesFromReaderException, e.Message);
                logger.Log(AveLogLevel.DEBUG, "An error occurred while setting properties from data reader, error message: {0}", e);
                throw new SPWFProcessorException(SPWFProcessorErrorCode.SetPropsFromDataReaderError, e);
            }
            finally
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "SetPropsFromDataReader");
            }
        }

        public void SetPropsFromDataRow(DataRow dr, DataColumnCollection columns)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "SetPropsFromDataRow");
            try
            {
                int fieldCount = columns.Count;

                StringBuilder b1 = new StringBuilder();
                foreach (DataColumn column in columns)
                {
                    if (dr.IsNull(column))
                        continue;
                    b1.Remove(0, b1.Length);
                    b1.Append("#");
                    b1.Append(column.ColumnName);
                    this.Properties.AddEx(b1.ToString(), dr[column]);
                }
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.IP_GetPropertiesFromReaderException, e.Message);
                logger.Log(AveLogLevel.DEBUG, "An error occurred while setting properties from row, error message: {0}", e);
                throw new SPWFProcessorException(SPWFProcessorErrorCode.SetPropsFromDataReaderError, e);
            }
            finally
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "SetPropsFromDataRow");
            }
        }

        /// <summary>
        /// This method is used to parse columnset
        /// </summary>
        /// <param name="value"></param>
        /// <param name="data"></param>
        private void ParseColumnSet(object value, Dictionary<string, string> fieldMap, int curRowOrdinal)
        {
            if (string.IsNullOrEmpty(value as string))
                return;
            Dictionary<string, object> columnSetCells = GetCellsFromColumnSet(value);
            string columnName = string.Empty;
            object columnValue = null;
            foreach (var cell in columnSetCells)
            {
                columnName = cell.Key;
                columnValue = cell.Value;
                string rowOrdinalStr = curRowOrdinal.ToString();
                string backupName = rowOrdinalStr + "_" + columnName.ToLower(CultureInfo.CurrentCulture);
                StringBuilder b1 = new StringBuilder();
                int b2 = curRowOrdinal;
                StringBuilder b3 = new StringBuilder();

                if (fieldMap != null)
                {
                    if (fieldMap.ContainsKey(backupName))
                    {
                        b1.Append("_");
                        b3.Append("_");
                        b3.Append(fieldMap[backupName]);
                    }
                    else
                    {
                        b1.Append("~");
                        b3.Append("_");
                        b3.Append(columnName);
                    }

                    while (true)
                    {
                        StringBuilder realName = new StringBuilder();
                        realName.Append(b1.ToString());
                        realName.Append(b2.ToString());
                        realName.Append(b3.ToString());
                        if (this.Properties.ContainsKey(realName.ToString()))
                        {
                            if (b1.ToString() == "_" && b2 == 0)
                                b2++;
                            b2++;
                        }
                        else
                        {
                            this.Properties.AddEx(realName.ToString(), columnValue);
                            break;
                        }
                    }
                }
                else
                {
                    b1.Append("#");
                    b1.Append(columnName);
                    this.Properties.AddEx(b1.ToString(), columnValue);
                }
            }
        }

        private Dictionary<string, object> GetCellsFromColumnSet(object value)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml("<ColumnSet>" + value.ToString() + "</ColumnSet>");
            XmlNodeList columnSet = xmlDoc.FirstChild.ChildNodes;
            foreach (XmlNode column in columnSet)
            {
                string columnName = column.Name;
                object columnValue = null;

                if (columnName.StartsWith("bit", StringComparison.OrdinalIgnoreCase))
                {
                    columnValue = Convert.ToBoolean(Int32.Parse(column.InnerText));
                }
                else if (columnName.StartsWith("datetime", StringComparison.OrdinalIgnoreCase))
                {
                    columnValue = Convert.ToDateTime(column.InnerText);
                }
                else if (columnName.StartsWith("float", StringComparison.OrdinalIgnoreCase))
                {
                    columnValue = Double.Parse(column.InnerText, CultureInfo.GetCultureInfo("en-US").NumberFormat);
                }

                else if (columnName.StartsWith("int", StringComparison.OrdinalIgnoreCase))
                {
                    columnValue = Convert.ToInt32(column.InnerText);
                }
                //else if (columnName.StartsWith("ntext", StringComparison.OrdinalIgnoreCase))
                //{

                //}
                //目前nvarchar都是255个字符的
                else if (columnName.StartsWith("nvarchar", StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(column.InnerText) && column.InnerText.Length >= 255)
                    {
                        logger.Debug("The inner text length:{0}, value:{1}", column.InnerText.Length, column.InnerText);
                        columnValue = column.InnerText.Replace("\r", "");
                    }
                    else
                    {
                        columnValue = column.InnerText;
                    }
                }
                //else if (columnName.StartsWith("sql_variant", StringComparison.OrdinalIgnoreCase))
                //{

                //}
                else if (columnName.StartsWith("uniqueidentifier", StringComparison.OrdinalIgnoreCase))
                {
                    columnValue = new Guid(column.InnerText);
                }
                //else if (columnName.StartsWith("geography", StringComparison.OrdinalIgnoreCase))
                //{
                //    //圆球形坐标系中的数据
                //    columnValue = Encoding.UTF8.GetBytes(column.InnerText);
                //}
                else
                {
                    columnValue = column.InnerText;
                }
                dic.Add(columnName, columnValue);
            }
            return dic;
        }

        #endregion


        #region ************************Restore Region************************

        #endregion


        internal SPWorkflowSubItemSerializableData ConvertToData()
        {
            SPWorkflowSubItemSerializableData data = new SPWorkflowSubItemSerializableData();

            if (this.mChildUnits != null)
            {
                data.mChildUnits = new List<SPWorkflowSubItemSerializableData>();
                foreach (SPWorkflowSubItemUnit unit in this.mChildUnits)
                    data.mChildUnits.Add(unit.ConvertToData());
            }
            //data.mParentUnit = this.mParentUnit.ConvertToData();
            if (this.mPermissionUnit != null)
                data.mPermissionUnit = this.mPermissionUnit.ConvertToData();
            data.mProperties = this.mProperties;
            data.mType = this.mType;
            data.mUnitId = this.UnitId;
            return data;
        }

        internal static SPWorkflowSubItemUnit ConvertToObject(SPWorkflowSubItemSerializableData data)
        {
            if (data == null)
                return null;
            SPWorkflowSubItemUnit unit = new SPWorkflowSubItemUnit(data.mType, SPWorkflowSubItemUnit.ConvertToObject(data.mParentUnit));

            if (data.mChildUnits != null)
            {
                unit.mChildUnits = new List<SPWorkflowSubItemUnit>();
                foreach (SPWorkflowSubItemSerializableData d in data.mChildUnits)
                {
                    SPWorkflowSubItemUnit childUnit = SPWorkflowSubItemUnit.ConvertToObject(d);
                    childUnit.ParentUnit = unit;
                    unit.mChildUnits.Add(childUnit);
                }
            }

            unit.mPermissionUnit = SPPermissionUnit.ConvertToObject(data.mPermissionUnit);
            unit.mProperties = data.mProperties;
            unit.mType = data.mType;
            unit.UnitId = data.mUnitId;
            return unit;
        }

    }


    public class SPWorkflowSubFileUnit
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        #region Serializable Data
        private SPWorkflowSubFileSerializableData mSerializableData = null;
        public SPWorkflowSubFileSerializableData SerializableData
        {
            get
            {
                if (mSerializableData == null)
                    mSerializableData = new SPWorkflowSubFileSerializableData();
                return mSerializableData;
            }
        }
        #endregion

        public IAveFile mSPFile;

        public SPWorkflowSubFileUnit()
        { }

        public SPWorkflowSubFileUnit(SPWorkflowSubFileSerializableData data)
        {
            mSerializableData = data;
        }
        /// <summary>
        /// 用于处理IsAllowDuplicateSPDAndNintexInSameWeb=false时，workflow带有version的情况的缓存。
        /// Guid：workflow parent id
        /// string(key) workflow name
        /// string(value) workflow对应的folder。
        /// </summary>
        /// <summary>
        /// safe 下面已经添加lock
        /// </summary>
        public static Dictionary<string, Dictionary<string, string>> OriginalIdCacheForWorkflowVersions = new Dictionary<string, Dictionary<string, string>>();

        #region ************************Backup  Region************************
        public static SPWorkflowSubFileUnit GenerateSPFileUnit(IAveFile file)
        {
            return GenerateSPFileUnit(file, file.UIVersionLabel);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = ".xoml.wfconfig.xml:Config file name.")]
        public static SPWorkflowSubFileUnit GenerateSPFileUnit(IAveFile file, string fileUIVersionLabel)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("BackupOneAssociation.GenerateSPFileUnit"))
            {
                SPWorkflowSubFileUnit fileUnit = null;
                try
                {
                    fileUnit = new SPWorkflowSubFileUnit();
                    fileUnit.SerializableData.mCharSetName = file.CharSetName;
                    fileUnit.SerializableData.mUIVersion = file.UIVersion;
                    fileUnit.SerializableData.mDocFlags = GetWorkflowTemplateFileDocFlags(file);
                    fileUnit.SerializableData.mCreated = file.TimeCreated;
                    fileUnit.SerializableData.mModified = file.TimeLastModified;
                    //当author或editor的user在site中不存在时，备份这两个属性API会出错，
                    //备份不出来不会影响还原结果，所以加上异常处理
                    try
                    {
                        fileUnit.SerializableData.mAuthorId = file.Author.ID;
                        fileUnit.SerializableData.mAuthorLogin = file.Author.LoginName;
                        fileUnit.SerializableData.mEditorId = file.ModifiedBy.ID;
                        fileUnit.SerializableData.mEditorLogin = file.ModifiedBy.LoginName;
                    }
                    catch (Exception ex)
                    {
                        logger.Info("Generate File author and editor failed,Name:{0},exception:{1}", file.Name, ex);
                    }
                    if (string.IsNullOrEmpty(fileUnit.SerializableData.mCharSetName))
                        fileUnit.SerializableData.mCharSetName = "utf-8";
                    if (file.Properties != null && file.Properties.ContainsKey("ipfs_streamhash"))
                    {
                        fileUnit.SerializableData.ipfs_streamhash = file.Properties["ipfs_streamhash"].ToString();
                    }
                    if (file.UIVersionLabel.Equals(fileUIVersionLabel, StringComparison.OrdinalIgnoreCase))
                    {
                        fileUnit.SerializableData.mContent = file.OpenBinary();
                        fileUnit.SerializableData.mIsCurrentVersion = true;
                    }
                    else
                    {
                        IAveFileVersion version = file.Versions.GetVersionFromLabel(fileUIVersionLabel);
                        fileUnit.SerializableData.mContent = version.OpenBinary();
                        fileUnit.SerializableData.mIsCurrentVersion = version.IsCurrentVersion;
                        fileUnit.SerializableData.mUIVersion = version.ID;
                        if (version.Properties != null && version.Properties.ContainsKey("vti_timelastmodified"))
                        {
                            fileUnit.SerializableData.mModified = (DateTime)version.Properties["vti_timelastmodified"];
                        }
                    }
                    fileUnit.SerializableData.mDirName = file.Url;
                    fileUnit.SerializableData.mUniqueId = file.UniqueId;
                    if (file.Item != null)
                        fileUnit.SerializableData.mItemId = file.Item.ID;
                    fileUnit.SerializableData.mParentFolderName = file.ParentFolder.Name;

                    fileUnit.SerializableData.mListRelativeUrl = file.ServerRelativeUrl.Substring(file.ParentFolder.ParentWeb.ServerRelativeUrl.Length);
                    if (!fileUnit.SerializableData.mListRelativeUrl.StartsWith("/", StringComparison.Ordinal))
                        fileUnit.SerializableData.mListRelativeUrl = "/" + fileUnit.SerializableData.mListRelativeUrl;
                    fileUnit.SerializableData.mFirstParentFolderRelativeUrl = file.ServerRelativeUrl.Substring(file.ParentFolder.ParentWeb.Lists[file.ParentFolder.ParentListId].RootFolder.ServerRelativeUrl.Length);

                    string temp = fileUnit.SerializableData.mListRelativeUrl.Substring(0, fileUnit.SerializableData.mListRelativeUrl.Length - fileUnit.SerializableData.mFirstParentFolderRelativeUrl.Length);
                    string[] splitedTemp = temp.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    if (splitedTemp.Length > 1)
                        fileUnit.SerializableData.mRootFolderRelativeUrl = "/" + splitedTemp[splitedTemp.Length - 1] + fileUnit.SerializableData.mFirstParentFolderRelativeUrl;
                    else
                        fileUnit.SerializableData.mRootFolderRelativeUrl = fileUnit.SerializableData.mFirstParentFolderRelativeUrl;

                    if (file.Properties != null && file.Properties.ContainsKey("vti_setuppath"))
                        fileUnit.SerializableData.mSetupPath = (string)file.Properties["vti_setuppath"];
                    fileUnit.SerializableData.mLeafName = file.ServerRelativeUrl.Substring(file.ParentFolder.ServerRelativeUrl.Length + 1);
                    fileUnit.SerializableData.mDirName = file.ParentFolder.ServerRelativeUrl.Substring(1);
                    fileUnit.SerializableData.mName = file.Name;
                    try
                    {
                        if (file.Item.Fields.ContainsField("Category"))
                        {
                            fileUnit.SerializableData.mCategorySchemalXml = file.Item.Fields["Category"].SchemaXml;
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.GetItemAttributeError, e.ToString());
                    }

                    try
                    {
                        if (file.Name.ToLower(CultureInfo.CurrentCulture).EndsWith(".xoml.wfconfig.xml", StringComparison.OrdinalIgnoreCase))
                        {
                            InitConfigurationFileRelatedInfo(file, fileUnit);
                        }
                        else if (file.Name.ToLower(CultureInfo.CurrentCulture).EndsWith(".xoml", StringComparison.OrdinalIgnoreCase))
                        {
                            string content = Encoding.GetEncoding(fileUnit.mSerializableData.mCharSetName).GetString(fileUnit.mSerializableData.mContent);
                            XmlDocument xDoc = new XmlDocument();
                            xDoc.LoadXml(content);
                            XmlNodeList copyToSharePointSiteNodes = xDoc.GetElementsByTagName("ns1:CopyToSharepointSite2Activity");
                            foreach (XmlElement xe in copyToSharePointSiteNodes.OfType<XmlElement>())
                            {
                                if (xe.HasAttribute("TargetSiteId"))
                                {
                                    string targetSiteId = xe.GetAttribute("TargetSiteId");
                                    Guid siteId = new Guid(targetSiteId);
                                    if (file.ParentFolder.ParentWeb.Site.ID == siteId)
                                    {
                                        if (xe.HasAttribute("TargetWebId") && xe.HasAttribute("TargetFolderId"))
                                        {
                                            string targetWebId = xe.GetAttribute("TargetWebId");
                                            string targetFolderId = xe.GetAttribute("TargetFolderId");
                                            if (string.IsNullOrEmpty(targetFolderId))
                                            {
                                                continue;
                                            }
                                            Guid webId = new Guid(targetWebId);
                                            Guid folderId = new Guid(targetFolderId);
                                            try
                                            {
                                                using (IAveWeb web = file.ParentFolder.ParentWeb.Site.OpenWeb(webId))
                                                {
                                                    IAveFolder folder = web.GetFolder(folderId);
                                                    string webUrl = web.ServerRelativeUrl;
                                                    string folderUrl = folder.ServerRelativeUrl;
                                                    fileUnit.SerializableData.mGUIDDictionary.AddEx(folderId.ToString().ToUpper(CultureInfo.InvariantCulture), "[FolderId]" + webUrl + "|" + folderUrl);
                                                }
                                            }
                                            catch (Exception ignoreException)
                                            {
                                                logger.Log(AveLogLevel.DEBUG, "Cannot open this web: {0}, exception:{1}", webId, ignoreException);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        try
                        {
                            Regex guidRE = new Regex(AveRegexCommon.GUIDREG, RegexOptions.IgnoreCase);
                            //the GUID include siteId, webId, listId
                            MatchCollection guids = guidRE.Matches(Encoding.GetEncoding(fileUnit.mSerializableData.mCharSetName).GetString(fileUnit.mSerializableData.mContent));
                            foreach (Match m in guids)
                            {
                                if (!fileUnit.SerializableData.mGUIDDictionary.ContainsKey(m.Value.ToUpper(CultureInfo.InvariantCulture)))
                                {

                                    object obj = null;
                                    Guid id = new Guid(m.Value);
                                    if (file.ParentFolder.ParentWeb.Site.ID == id)
                                    {
                                        //only handle siteId equal current site id.
                                        fileUnit.SerializableData.mGUIDDictionary.AddEx(m.Value.ToUpper(CultureInfo.InvariantCulture), "[SiteID]");
                                    }
                                    else
                                    {
                                        obj = file.ParentFolder.ParentWeb.Lists.GetListById(id, false);
                                        if (obj != null)
                                        {
                                            //is a list id of current web.
                                            string title = ((IAveList)obj).Title;
                                            fileUnit.SerializableData.mGUIDDictionary.AddEx(m.Value.ToUpper(CultureInfo.InvariantCulture), "[ListID]" + file.ParentFolder.ParentWeb.ServerRelativeUrl + "|" + title);
                                            SPWorkflowProcessorRuntime.Log(Logs.Markup_FoundListByTitle, title, m.Value);
                                        }
                                        else
                                        {
                                            try
                                            {
                                                obj = file.ParentFolder.ParentWeb.Site.OpenWeb(id);
                                            }
                                            catch (Exception ignoreException)
                                            {
                                                logger.Log(AveLogLevel.DEBUG, "Cannot open this web.{0}.", ignoreException.Message);
                                            }
                                            if (obj != null)
                                            {
                                                //is a web id of current site.
                                                string webUrl = ((IAveWeb)obj).ServerRelativeUrl;
                                                ((IAveWeb)obj).Dispose();
                                                fileUnit.SerializableData.mGUIDDictionary.AddEx(m.Value.ToUpper(CultureInfo.InvariantCulture), "[WebID]" + webUrl);
                                            }
                                            else
                                            {
                                                fileUnit.SerializableData.mGUIDDictionary.AddEx(m.Value.ToUpper(CultureInfo.InvariantCulture), null);
                                                SPWorkflowProcessorRuntime.Log(Logs.Markup_CannotHandleGUID, m.Value);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.DEBUG, "An error occurred while generating file unit, error message: {0}", e);
                            SPWorkflowProcessorRuntime.Log(Logs.Markup_CannotHandleGUIDUnknown, e.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.GenerateSPFileUnitError, ex);
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.GenerateSPFileUnitError, ex);
                    return null;
                }

                return fileUnit;
            }
        }

        private static void InitConfigurationFileRelatedInfo(IAveFile file, SPWorkflowSubFileUnit fileUnit)
        {
            var properties = ConfigFileProc.GetTemplateProperties(fileUnit.SerializableData.mContent);
            string templateListIdStr;
            if (properties.TryGetValue("DocLibID", out templateListIdStr))
            {
                Guid templateListId = new Guid(templateListIdStr);
                fileUnit.SerializableData.mTemplateLibTitle = file.ParentFolder.ParentWeb.Lists[templateListId].Title;
            }
            string category;
            if (properties.TryGetValue("Category", out category))
            {
                string categoryString = ConfigFileProc.AnalyzeContentTypeInfoInCategory(category, file);
                fileUnit.SerializableData.mGUIDDictionary.AddEx(AveWorkflowConstants.ReplaceDictionary_Category, categoryString);
            }
            //ContentTypeID
            string contentTypeId;
            if (properties.TryGetValue("ContentTypeID", out contentTypeId))
            {
                IAveContentType contentType = file.Web.AvailableContentTypes.GetById(contentTypeId);
                if (contentType != null)
                {
                    string value = string.Format("{0};{1}", contentTypeId, contentType.Name);
                    fileUnit.SerializableData.mGUIDDictionary.AddEx(AveWorkflowConstants.ReplaceDictionary_ContentTypeID, value);
                }
            }
        }



        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = ".xoml.wfconfig.xml:Config file name.")]
        public static List<SPWorkflowSubFileUnit> GenerateSPFileUnitCollection(IAveFolder parentFolder, int cfgFileVersion)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("BackupOneAssociation.GenerateSPFileUnitCollection"))
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "GenerateSPFileUnitCollection");

                List<SPWorkflowSubFileUnit> fileUnitCollection = new List<SPWorkflowSubFileUnit>();

                string cfgVersionLabel = string.Empty;
                string xomlVersionLabel = string.Empty;
                string rulesVersionLabel = string.Empty;
                string previewName = string.Empty;

                #region Get Files' UI Version
                foreach (IAveFile file in parentFolder.Files)
                {
                    if (file.Name.ToLower(CultureInfo.CurrentCulture).EndsWith(".xoml.wfconfig.xml", StringComparison.Ordinal))
                    {
                        if (cfgFileVersion == -1)
                            cfgFileVersion = file.UIVersion;

                        string charSetName = file.CharSetName;
                        if (string.IsNullOrEmpty(charSetName))
                            charSetName = "utf-8";

                        string strContent = string.Empty;
                        if (file.UIVersion == cfgFileVersion)
                        {
                            strContent = Encoding.GetEncoding(charSetName).GetString(file.OpenBinary());
                            cfgVersionLabel = file.UIVersionLabel;
                        }
                        else
                        {
                            IAveFileVersion version = file.Versions.GetVersionFromID(cfgFileVersion);
                            strContent = Encoding.GetEncoding(charSetName).GetString(version.OpenBinary());
                            cfgVersionLabel = version.VersionLabel;
                        }
                        XmlDocument xmlConfig = null;
                        try
                        {
                            xmlConfig = new XmlDocument();
                            xmlConfig.LoadXml(strContent);
                            if (xmlConfig.SelectSingleNode("/WorkflowConfig/Template/@XomlVersion") != null)
                            {
                                xomlVersionLabel = xmlConfig.SelectSingleNode("/WorkflowConfig/Template/@XomlVersion").Value;
                                if (xomlVersionLabel.StartsWith("V", StringComparison.OrdinalIgnoreCase))
                                {
                                    xomlVersionLabel = xomlVersionLabel.Substring(1);
                                }
                            }
                            if (xmlConfig.SelectSingleNode("/WorkflowConfig/Template/@RulesVersion") != null)
                            {
                                rulesVersionLabel = xmlConfig.SelectSingleNode("/WorkflowConfig/Template/@RulesVersion").Value;
                                if (rulesVersionLabel.StartsWith("V", StringComparison.OrdinalIgnoreCase))
                                {
                                    rulesVersionLabel = rulesVersionLabel.Substring(1);
                                }
                            }
                            if (xmlConfig.SelectSingleNode("/WorkflowConfig/Template/@PreviewHref") != null)
                            {
                                var previewHref = xmlConfig.SelectSingleNode("/WorkflowConfig/Template/@PreviewHref").Value.TrimEnd('/');
                                var index = previewHref.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
                                previewName = index < 0 ? previewHref : previewHref.Substring(index + 1);
                            }
                        }
                        catch (Exception ex)
                        {
                            SPWorkflowProcessorRuntime.Log(Logs.Common_XmlFileHandleException, ex.Message);
                            logger.Warn("An exception occurred while handle xml file. exception:{0}", ex.ToString());
                        }
                        finally
                        {
                            if (xmlConfig != null)
                                xmlConfig.RemoveAll();
                        }
                        break;
                    }
                }
                #endregion

                foreach (IAveFile file in parentFolder.Files)
                {
                    string fileVersionLabel = file.UIVersionLabel;
                    if (file.Name.ToLower(CultureInfo.CurrentCulture).EndsWith(".xoml.wfconfig.xml", StringComparison.Ordinal))
                        fileVersionLabel = cfgVersionLabel;
                    else if (file.Name.ToLower(CultureInfo.CurrentCulture).EndsWith(".xoml", StringComparison.Ordinal) && !string.IsNullOrEmpty(xomlVersionLabel))
                        fileVersionLabel = xomlVersionLabel;
                    else if (file.Name.ToLower(CultureInfo.CurrentCulture).EndsWith(".rules", StringComparison.Ordinal))
                        if (!string.IsNullOrEmpty(rulesVersionLabel))
                        {
                            fileVersionLabel = rulesVersionLabel;
                        }
                        else
                        {//do not have rule version related to current workflow template,do not backup it.
                            continue;
                        }
                    else if (file.Name.ToLower(CultureInfo.CurrentCulture).EndsWith(".vdw", StringComparison.Ordinal) && !file.Name.Equals(previewName, StringComparison.OrdinalIgnoreCase))
                        continue;
                    if (float.Parse(fileVersionLabel, CultureInfo.GetCultureInfo("en-US").NumberFormat) > float.Parse(file.UIVersionLabel, CultureInfo.GetCultureInfo("en-US").NumberFormat))
                        fileVersionLabel = file.UIVersionLabel;

                    //特殊情况下，可能会出现config file中关联的其他file 的 version label为0.0或空的情况，需要做下判断
                    if (string.IsNullOrEmpty(fileVersionLabel) || string.Equals(fileVersionLabel, "0.0", StringComparison.OrdinalIgnoreCase))
                    {
                        logger.Warn("Invaild file version label.:{0},FileName:{1}", fileVersionLabel, file.Name);
                        continue;
                    }

                    SPWorkflowSubFileUnit fileUnit = GenerateSPFileUnit(file, fileVersionLabel);
                    if (fileUnit != null)
                        fileUnitCollection.Add(fileUnit);
                }
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "GenerateSPFileUnitCollection");
                return fileUnitCollection;
            }
        }

        public static List<SPWorkflowSubFileUnit> GenerateWFSvcFileUnitCollection(IAveFolder parentFolder)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("BackupOneAssociation.GenerateWFSvcFileUnitCollection"))
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "GenerateSPFileUnitCollection");

                List<SPWorkflowSubFileUnit> fileUnitCollection = new List<SPWorkflowSubFileUnit>();

                string cfgVersionLabel = string.Empty;
                string xomlVersionLabel = string.Empty;
                string rulesVersionLabel = string.Empty;
                string previewName = string.Empty;

                foreach (IAveFile file in parentFolder.Files)
                {
                    if (file.Name.ToLowerInvariant().Equals("workflow.xaml", StringComparison.Ordinal))
                    {
                        if (file.Properties != null && file.Properties.ContainsKey("WSPublishState") && !file.Properties["WSPublishState"].ToString().Equals("1")
                            || file.Item != null && !file.Item["WSPublishState"].ToString().Equals("1"))
                        {
                            SPWorkflowSubFileUnit fileUnit = GenerateSPFileUnit(file);
                            if (fileUnit != null)
                            {
                                fileUnitCollection.Add(fileUnit);
                            }
                        }
                        foreach (IAveFileVersion fileVersion in file.Versions)
                        {
                            if (fileVersion.Properties != null && file.Properties.ContainsKey("WSPublishState") && !fileVersion.Properties["WSPublishState"].ToString().Equals("1"))
                            {
                                SPWorkflowSubFileUnit fileUnit = GenerateSPFileUnit(file, fileVersion.VersionLabel);
                                if (fileUnit != null)
                                {
                                    fileUnitCollection.Add(fileUnit);
                                }
                            }
                        }
                    }
                }
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "GenerateSPFileUnitCollection");
                return fileUnitCollection;
            }
        }
        #endregion

        #region ************************Restore Region************************

        public static bool HandleReusableTemplateSPFileUnits(SPWFAssociationUnit assoUnit, SPWorkflowSubListUnit containerListUnit, bool createAssociation)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("RestoreOneWFTemplate.HandleReusableTemplateSPFileUnits"))
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "HandleReusableTemplateSPFileUnits");
                try
                {
                    //ensure workflow template library
                    IAveWeb tempWeb = assoUnit.mTemplateLibUnit != null && assoUnit.mTemplateLibUnit.SerializableData.IsRootWebList ? assoUnit.ParentWeb.Site.RootWeb : assoUnit.ParentWeb;
                    IAveList tempList = SPWorkflowSubListUnit.GetOrCreateSPList(assoUnit, tempWeb.Lists, assoUnit.mTemplateLibUnit, assoUnit.WebLevelFieldProcessorCollection);
                    //restore template files
                    string configFileRelativeUrl = RestoreWorkflowTemplateFiles(assoUnit, containerListUnit, createAssociation, true);

                    //reusable workflow还原完template后，需要reload下，否则WorkflowTemplates已经初始化完，新还的template加不进去
                    //后续需要考虑如何更新那个缓存，这样比reload更好
                    //Reload parent web after restore workflow template files,for refresh workflow template collection
                    //work for reusable workflow temporary
                    assoUnit.ReloadParentWeb();

                    return true;
                }
                catch (SPWFProcessorException procException)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.Markup_ProcessTemplateFilesException, procException.Message);
                    throw;
                }
                catch (Exception e)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.Markup_ProcessTemplateFilesException, e.Message);
                    logger.Warn("An exception occurred while process template file. exception:{0}", e.ToString());
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationDefinitionRestoreError, e);
                }
                finally
                {
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "HandleReusableTemplateSPFileUnits");
                }
            }
        }

        /// <summary>
        /// for 10 mode workflow
        /// </summary>
        /// <param name="assoUnit"></param>
        /// <param name="containerListUnit"></param>
        /// <param name="createAssociation"></param>
        /// <param name="spAssociation"></param>
        /// <returns></returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = ".xoml.wfconfig.xml:Config file name.")]
        public static bool HandleTemplateSPFileUnits(SPWFAssociationUnit assoUnit, SPWorkflowSubListUnit containerListUnit, bool createAssociation, out IAveWorkflowAssociation spAssociation)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("RestoreOneAssociation.HandleTemplateSPFileUnits"))
            {

                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "HandleTemplateSPFileUnits");
                spAssociation = null;
                try
                {

                    string configFileRelativeUrl = RestoreWorkflowTemplateFiles(assoUnit, containerListUnit, createAssociation, false);

                    #region create association

                    if (createAssociation)
                    {
                        //List<Guid> oldIdList = new List<Guid>();
                        Dictionary<Guid, string> oldIdAssoNameMap = new Dictionary<Guid, string>();
                        foreach (IAveWorkflowAssociation wa in assoUnit.SPAssoicationCollection)
                        {
                            oldIdAssoNameMap.Add(wa.ID, wa.Name);
                        }

                        AssociateWorkflowMarkup(assoUnit, containerListUnit, configFileRelativeUrl);

                        //list workflow需要reload，因为此处创建association的API内会从web上新取list，然后将新的association add进去，如果不reload新创建的association获取不到，
                        //对于site workflow，是直接在web.WorkflowAssociations集合中操作的，所以不需要reload
                        //对于contentType workflow，都是reusable的，不需要在此处考虑
                        //具体逻辑可以参照WebPartPagesWebService.AssociateWorkflowMarkup方法的内部实现
                        if (assoUnit.ParentObjectType == SPWFAssociationParentType.List)
                        {
                            assoUnit.ReloadParentList();
                        }

                        //365 使用AssociateWorkflowMarkup方式创建association后，由于Ave对象中的缓存是我们维护的，无法更新，所以需要主动更新下web.WorkflowAssociations
                        //local API内部会更新SPWeb.WorkflowAssociations缓存,不需要额外处理
                        if (assoUnit.ParentObjectType == SPWFAssociationParentType.Web &&
                            SPWorkflowProcessorRuntime.ObjectModelFactory.ContextKind == AveContextKind.ClientObjectModel)
                        {
                            assoUnit.ReloadParentWeb();
                        }

                        UpdateStatusField(assoUnit, oldIdAssoNameMap, out spAssociation);

                        #region 如果association name存在冲突，那么markup association时API会自动将association名修改为不冲突   ADO-154827 不能用foreach，因为update会change AssociationCollection

                        for (int k = 0; k < assoUnit.SPAssoicationCollection.Count; k++)
                        {
                            var asso = assoUnit.SPAssoicationCollection[k];
                            string name = string.Empty;
                            if (oldIdAssoNameMap.TryGetValue(asso.ID, out name) && !string.Equals(name, asso.Name, StringComparison.OrdinalIgnoreCase))
                            {
                                if (SPWorkflowProcessorRuntime.ObjectModelFactory.ContextKind != AveContextKind.ClientObjectModel)
                                {
                                    SPWFAssociationProcNative.UpdateAssociationName(asso, name, assoUnit.IsRenamed);
                                    assoUnit.UpdateWorkflowAssociation(asso);
                                }
                                else
                                {
                                    if (!assoUnit.IsRenamed)
                                    {
                                        asso.Name = name;
                                        assoUnit.UpdateWorkflowAssociation(asso);
                                    }
                                }
                            }
                        }

                        #endregion

                    }

                    #endregion

                    //reusable workflow还原完template后，需要reload下，否则WorkflowTemplates已经初始化完，新还的template加不进去
                    //后续需要考虑如何更新那个缓存，这样比reload更好
                    //Reload parent web after restore workflow template files,for refresh workflow template collection
                    //work for reusable workflow temporary
                    if (assoUnit != null && spAssociation == null)
                    {
                        assoUnit.ReloadParentWeb();
                    }

                    return true;
                }
                catch (SPWFProcessorException procException)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.Markup_ProcessTemplateFilesException, procException.Message);
                    throw;
                }
                catch (Exception e)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.Markup_ProcessTemplateFilesException, e.Message);
                    logger.Warn("An exception occurred while process template file. exception:{0}", e.ToString());
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationDefinitionRestoreError, e);
                }
                finally
                {
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "HandleTemplateSPFileUnits");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assoUnit"></param>
        /// <param name="containerListUnit"></param>
        /// <param name="configFileRelativeUrl"></param>
        private static void AssociateWorkflowMarkup(SPWFAssociationUnit assoUnit, SPWorkflowSubListUnit containerListUnit, string configFileRelativeUrl)
        {
            IAveWeb tempWeb = assoUnit.mTemplateLibUnit != null && assoUnit.mTemplateLibUnit.SerializableData.IsRootWebList ? assoUnit.ParentWeb.Site.RootWeb : assoUnit.ParentWeb;
            IAveList tempLib = GetTemplateLibrary(tempWeb, containerListUnit.SerializableData);
            IAveWebPartPagesWebService objWebPartPages = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateWebPartPagesWebService(tempWeb);

            try
            {
                AssociateWorkflowMarkupInternal(tempLib, objWebPartPages, configFileRelativeUrl);
            }
            catch (SPWFProcessorException procException)
            {
                SPWorkflowProcessorRuntime.Log(Logs.Markup_APIResultException, procException.Message);
                logger.Log(AveLogLevel.WARN, "An processor error occurred while marking up workflow association, Error message: {0}", procException);
                throw;
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.AddWorkFlowToPageError, e.ToString());
                try
                {
                    AssociateWorkflowMarkupInternal(tempLib, objWebPartPages, configFileRelativeUrl);
                }
                catch (Exception exception)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.Markup_APIResultException, exception.Message);
                    logger.Log(AveLogLevel.DEBUG, "An error occurred while marking up workflow association, error message: {0}", exception);
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.SoapServerException, exception);
                }
            }
        }

        /// <summary>
        /// 避免代码冗余提出来的方法，异常在调用的地方处理
        /// </summary>
        /// <param name="tempLib"></param>
        /// <param name="objWebPartPages"></param>
        /// <param name="configFileRelativeUrl"></param>
        private static void AssociateWorkflowMarkupInternal(IAveList tempLib, IAveWebPartPagesWebService objWebPartPages, string configFileRelativeUrl)
        {
            try
            {
                tempLib.Update();
            }
            catch (Exception ex)
            {
                logger.Debug(WrapperWorkflowResource.UpdateListError, ex);
            }
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "AssociateWorkflowMarkup");
            string strResult2 = objWebPartPages.AssociateWorkflowMarkup(configFileRelativeUrl, string.Empty);
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "AssociateWorkflowMarkup");
            logger.Debug("AssociateWorkflowMarkup ConfigUrl:{0},Result:{1}", configFileRelativeUrl, strResult2);
            CheckWebServiceSuccess("AssociatingWorkflow", strResult2);
        }

        /// <summary>
        /// 根据Workflow configuration file的Editor重新构造IAveWeb对象
        /// </summary>
        /// <param name="web"></param>
        /// <param name="containerListUnit"></param>
        /// <returns></returns>
        private static IAveWeb CreateWebWithUserToken(IAveWeb web, SPWorkflowSubListUnit containerListUnit)
        {
            try
            {
                foreach (SPWorkflowSubFileUnit fileUnit in containerListUnit.mTemplateFileUnits)
                {
                    if (fileUnit.SerializableData.mName.EndsWith(".xoml.wfconfig.xml", StringComparison.OrdinalIgnoreCase) && fileUnit.SerializableData.mIsCurrentVersion)
                    {
                        var mEditor = SPWorkflowProcessorRuntime.OnUserMapping(fileUnit.SerializableData.mEditorLogin);
                        if (mEditor != null)
                        {
                            if (!web.Site.UserToken.CompareUser(web.SiteUsers[mEditor.LoginName].UserToken))
                            {
                                logger.Log(AveLogLevel.INFO, "Use user token of user:{0} to create site.", mEditor.LoginName);
                                AveObjectModelFactory tmpFactory = AveObjectModelFactory.CreateObjectModelFactory(null, null);
                                var tmpSite = tmpFactory.CreateSite(web.Site.ID, mEditor.UserToken);
                                Guid tempWebId = web.ID;
                                return tmpSite.OpenWeb(tempWebId);
                            }
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, "An error occurred while create web with user token. Detail:{0}", ex);
            }
            return web;
        }

        /// <summary>
        /// restore workflow template files
        /// 10mode workflow的template 文件替换Content并添加到目的端
        /// </summary>
        /// <param name="assoUnit"></param>
        /// <param name="containerListUnit"></param>
        /// <param name="createAssociation"></param>
        /// <param name="isReusable"></param>
        /// <returns></returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = ".xoml.wfconfig.xml:Config file name.")]
        private static string RestoreWorkflowTemplateFiles(SPWFAssociationUnit assoUnit, SPWorkflowSubListUnit containerListUnit, bool createAssociation, bool isReusable)
        {


            WorkflowFileReplaceMapping workflowReplaceMapping = WorkflowFileMappingHelper.GenerateMapping(assoUnit, containerListUnit);
            Dictionary<string, object> replaceDic = workflowReplaceMapping.TemplateFileMapping;
            Dictionary<string, object> replaceConfigDic = workflowReplaceMapping.ConfigFileMapping;

            bool isListAlreadyContainWf = false;
            //查看当前list中是否已经存在了workflow。如果已经存在说明可能是merge这个list，在生成对应的配置文件folder时会进一步处理。
            isListAlreadyContainWf = IsListAlreadyContainWf(assoUnit.SerializableData.mOriginalName, assoUnit.ParentList);
            IAveWeb web = assoUnit.ParentWeb;
            {

                //ADO-80069， site collection level的Reusable Nintex Workflow的template 文件存在Root web下的list “wfpub” 下，Nintex template 文件存在Root Web的list “NintexWorkflows”下。故需要特殊处理。
                IAveWeb tempWeb = assoUnit.mTemplateLibUnit != null && assoUnit.mTemplateLibUnit.SerializableData.IsRootWebList ? assoUnit.ParentWeb.Site.RootWeb : assoUnit.ParentWeb;
                //tempWeb = CreateWebWithUserToken(tempWeb, containerListUnit);
                //GetListByName 在client中会通过request重取list，避免取到以前cache的list
                //client 需要重取list，否则list下的folder，file都是从cache中取，如果还原多个version的template文件就会有问题
                IAveList templateLibrary = GetTemplateLibrary(tempWeb, containerListUnit.SerializableData);

                IAveFolder parentFolder = null;
                string oldFolderName = string.Empty;
                IAveFile configSPFile = null;
                SPWorkflowSubFileUnit configFileUnit = null;
                bool needReplaceConfigFile = true;

                string configFileContent = string.Empty;
                string xomlFileContent = string.Empty;
                string rulesFileContent = string.Empty;
                string xomlFileVersion = string.Empty;
                string rulesFileVersion = string.Empty;
                string configFileRelativeUrl = string.Empty;
                bool hasSameWorkflowNameinWeb = false;
                //排序,保证config文件最后处理
                containerListUnit.mTemplateFileUnits.Sort(new CompareWorkflowTemplateFileUnit());
                foreach (SPWorkflowSubFileUnit fileUnit in containerListUnit.mTemplateFileUnits)
                {
                    bool needUpdate;

                    //todo:wbhu,挪到处理fileUnit前面，foreach外面
                    #region Get Parent Folder

                    if (parentFolder == null)
                    {
                        int fileNameIndex = fileUnit.SerializableData.mFirstParentFolderRelativeUrl.LastIndexOf("/", StringComparison.Ordinal);
                        if (fileNameIndex == 0)
                        {
                            parentFolder = templateLibrary.RootFolder;
                            oldFolderName = templateLibrary.RootFolder.Name;
                        }
                        else
                        {
                            //同时会处理对应的dupliacate SPD/Nintex workflow配置文件folder的逻辑。
                            parentFolder = GetOrCreateParentFolder(templateLibrary, fileUnit.SerializableData.mFirstParentFolderRelativeUrl.Substring(1, fileNameIndex - 1), isListAlreadyContainWf, assoUnit, ref hasSameWorkflowNameinWeb);
                            string folderPath = fileUnit.SerializableData.mFirstParentFolderRelativeUrl.Substring(1, fileNameIndex - 1);
                            oldFolderName = folderPath.Split('/').Last();
                        }
                    }

                    #endregion

                    #region restore tempalte file,替换content
                    //todo:wbhu  1.根据File类型分别处理不同file 2.config file处理逻辑合并,前面已经将config文件排序到最后面,可以将逻辑整合到一起 3.修改逻辑顺序,先替换content再add file or save binary.(需要修改SPWorkflowFileContentProc内部逻辑)
                    string curFileContent = string.Empty;

                    #region handle template file name changed
                    //如果源端workflow name和已经生成的folder name不同，说明需要在同一个web中的不同list中添加同名的SPD/Nintex workflow，需要对源端数据处理。
                    if (!fileUnit.SerializableData.mName.StartsWith(parentFolder.Name, StringComparison.OrdinalIgnoreCase) && fileUnit.SerializableData.mName.Contains("."))
                    {
                        //如果workflow template folder name没有变化， 那么不需要进行rename
                        if (!string.Equals(parentFolder.Name, oldFolderName, StringComparison.OrdinalIgnoreCase))
                        {
                            string fileType = null;
                            if (fileUnit.SerializableData.mName.EndsWith(".xoml.wfconfig.xml", StringComparison.OrdinalIgnoreCase))
                            {
                                fileType = ".xoml.wfconfig.xml";
                            }
                            else if (fileUnit.SerializableData.mName.EndsWith(".xoml.rules", StringComparison.OrdinalIgnoreCase))
                            {
                                fileType = ".xoml.rules";
                            }
                            else
                            {
                                fileType = fileUnit.SerializableData.mName.Substring(fileUnit.SerializableData.mName.LastIndexOf('.'));
                            }

                            string oldFileName = fileUnit.SerializableData.mName;
                            //添加task action也会产生xsn文件，而且不以workflow name开头，需要特殊判断
                            //此处逻辑需要进行优化，目前如果有其他类型不以workflow name开头的文件，在web下还原同名spd workflow时还会有问题
                            string newFullName = parentFolder.Name + fileType;
                            if (fileUnit.SerializableData.mName.EndsWith(".xsn", StringComparison.OrdinalIgnoreCase))
                            {
                                string fileNameWithoutExtension = fileUnit.SerializableData.mName.Substring(0, fileUnit.SerializableData.mName.Length - 4);
                                if (string.Equals(fileNameWithoutExtension, oldFolderName, StringComparison.OrdinalIgnoreCase))
                                {
                                    //the form of the workflow ,need rename
                                    //newFullName = parentFolder.Name + ".xsn";
                                }
                                else
                                {
                                    //we should not rename the task form template file
                                    newFullName = fileUnit.SerializableData.mName;
                                }
                            }
                            fileUnit.SerializableData.mDirName = parentFolder.ServerRelativeUrl;
                            fileUnit.SerializableData.mFirstParentFolderRelativeUrl = "/" + parentFolder.Name + "/" + newFullName;
                            fileUnit.SerializableData.mLeafName = newFullName;
                            fileUnit.SerializableData.mListRelativeUrl = "/Workflows" + fileUnit.SerializableData.mFirstParentFolderRelativeUrl;
                            fileUnit.SerializableData.mName = newFullName;
                            fileUnit.SerializableData.mParentFolderName = parentFolder.Name;
                            fileUnit.SerializableData.mRootFolderRelativeUrl = fileUnit.SerializableData.mFirstParentFolderRelativeUrl;
                        }
                    }

                    #endregion handle template file name changed

                    IAveFile currentTemplateFile = CreateTemplateFileIfNotExist(parentFolder, fileUnit, templateLibrary, ref needReplaceConfigFile, out needUpdate);

                    if (currentTemplateFile == null) return string.Empty;

                    fileUnit.mSPFile = currentTemplateFile;
                    //Update nintex template file properties in nintex template library
                    if (needUpdate && !string.IsNullOrEmpty(containerListUnit.SerializableData.mUnitId)
                        && string.Equals(containerListUnit.SerializableData.mUnitId, "NintexWorkflow", StringComparison.OrdinalIgnoreCase))
                    {
                        currentTemplateFile = parentFolder.ParentWeb.GetFile(currentTemplateFile.ServerRelativeUrl);
                        fileUnit.mSPFile = currentTemplateFile;
                        UpdateNWFileProperties(assoUnit, fileUnit);
                    }

                    #region xoml.wfconfig.xml
                    if (currentTemplateFile.Name.ToLower(CultureInfo.CurrentCulture).EndsWith(".xoml.wfconfig.xml", StringComparison.Ordinal))
                    {
                        FixupDictionary(web, replaceConfigDic, fileUnit.mSerializableData.mGUIDDictionary);
                        if (string.IsNullOrEmpty(configFileRelativeUrl) || currentTemplateFile.Name.Equals(parentFolder.Name + ".xoml.wfconfig.xml", StringComparison.OrdinalIgnoreCase))
                        {
                            configSPFile = currentTemplateFile;
                            configFileUnit = fileUnit;
                            configFileRelativeUrl = fileUnit.SerializableData.mListRelativeUrl.Substring(1);
                        }
                        //for ADO-174953:在还原workflow Configuration file的时候增加对其内容中template路径的替换逻辑
                        foreach (SPWorkflowSubFileUnit unit in containerListUnit.mTemplateFileUnits)
                        {
                            if (!string.IsNullOrEmpty((unit.mSerializableData.mListRelativeUrl)))
                            {
                                if (unit.mSerializableData.mListRelativeUrl.ToLower(CultureInfo.CurrentCulture).EndsWith(".xoml", StringComparison.Ordinal))
                                {
                                    replaceConfigDic.Add("XomlHref", unit.mSerializableData.mListRelativeUrl.TrimStart('/'));
                                }
                                if (unit.mSerializableData.mListRelativeUrl.ToLower(CultureInfo.CurrentCulture).EndsWith(".xoml.rules", StringComparison.Ordinal))
                                {
                                    replaceConfigDic.Add("RulesHref", unit.mSerializableData.mListRelativeUrl.TrimStart('/'));
                                }
                            }
                        }
                    }
                    #endregion                   
                    #region xsn
                    else if (currentTemplateFile.Name.ToLower(CultureInfo.CurrentCulture).EndsWith(".xsn", StringComparison.OrdinalIgnoreCase) && needUpdate)
                    {
                        SPWorkflowProcessorRuntime.MappingManager.SiteMappingManager.UnReplaceGuidAndUrlInfoPathCache.Add(currentTemplateFile.ServerRelativeUrl + "," + tempWeb.ID);
                        if (!string.IsNullOrEmpty(fileUnit.SerializableData.ipfs_streamhash) && currentTemplateFile.Properties != null)
                        {
                            currentTemplateFile.Properties["ipfs_streamhash"] = fileUnit.SerializableData.ipfs_streamhash;
                            currentTemplateFile.Update();
                            UpdateTemplateFileVersion(templateLibrary, currentTemplateFile, fileUnit.SerializableData, false);
                        }
                        else if (SPWorkflowProcessorRuntime.ObjectModelFactory.ContextKind == AveContextKind.ClientObjectModel)
                        {
                            UpdateTemplateFileVersion(templateLibrary, currentTemplateFile, fileUnit.SerializableData, false);
                            try
                            {
                                IAveFormsServicesWebService svc = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateFormsServicesWebService(assoUnit.ParentWeb);
                                if (svc != null)
                                {
                                    svc.BrowserEnableUserFormTemplate(assoUnit.ParentWeb.Url.TrimEnd('/') + "/" + currentTemplateFile.Url.TrimStart('/'));
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Log(AveLogLevel.WARN, "An error occurred while enable user form template. Detail:{0}", ex.Message);
                            }
                        }
                        else if (string.IsNullOrEmpty(fileUnit.SerializableData.ipfs_streamhash))
                        {
                            UpdateTemplateFileVersion(templateLibrary, currentTemplateFile, fileUnit.SerializableData, false);
                            try
                            {
                                IAveOFormsService formsService = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateFormsService();
                                formsService.BrowserEnableUserFormTemplate(currentTemplateFile);
                            }
                            catch (Exception ex)
                            {
                                logger.Log(AveLogLevel.WARN, "An error occurred while enable user form template. Detail:{0}", ex.Message);
                            }
                        }
                    }
                    #endregion
                    #region other files
                    else
                    {
                        SPWorkflowFileContentProc fileContentProc = SPWorkflowFileContentProc.CreateInstance(assoUnit, currentTemplateFile);
                        if (fileContentProc == null) continue;

                        FixupDictionary(web, replaceDic, fileUnit.mSerializableData.mGUIDDictionary);
                        ////ReplaceFileSPVersionInfo(web, replaceDic);
                        if (needUpdate)
                        {
                            curFileContent = fileContentProc.ReplaceContent(replaceDic);
                            if (!currentTemplateFile.Name.ToLower(CultureInfo.CurrentCulture).Equals("nwf", StringComparison.OrdinalIgnoreCase))
                            {
                                UpdateTemplateFileVersion(templateLibrary, currentTemplateFile, fileUnit.SerializableData, false);
                            }
                        }
                        if (currentTemplateFile.Name.ToLower(CultureInfo.CurrentCulture).EndsWith("xoml", StringComparison.OrdinalIgnoreCase))
                        {
                            xomlFileContent = curFileContent;
                            replaceConfigDic.Add("XomlFileVersion", "V" + currentTemplateFile.UIVersionLabel);
                        }
                        if (currentTemplateFile.Name.ToLower(CultureInfo.CurrentCulture).EndsWith("rules", StringComparison.OrdinalIgnoreCase))
                        {
                            rulesFileContent = curFileContent;
                            replaceConfigDic.Add("RulesFileVersion", "V" + currentTemplateFile.UIVersionLabel);
                        }
                        if (currentTemplateFile.Name.ToLower(CultureInfo.CurrentCulture).Equals("nwf", StringComparison.OrdinalIgnoreCase))
                        {
                            fileUnit.SerializableData.mContent = Encoding.UTF8.GetBytes(curFileContent);
                            currentTemplateFile.Delete();
                            currentTemplateFile = null;
                        }
                    }
                    #endregion
                    #endregion
                }

                #region Replace Config File Content

                if (configSPFile != null && configFileUnit != null)
                {
                    if (needReplaceConfigFile)
                    {
                        UpdateConfigFileReplaceDictionary(configFileUnit, replaceConfigDic, NintexWorkflowUtility.IsNintexWorkflow(assoUnit) && hasSameWorkflowNameinWeb, assoUnit.SerializableData.mBaseId);
                        SPWorkflowFileContentProc configFileContentProc = SPWorkflowFileContentProc.CreateInstance(assoUnit, configSPFile);
                        configFileContent = configFileContentProc.ReplaceContent(replaceConfigDic);
                        UpdateTemplateFileVersion(templateLibrary, configSPFile, configFileUnit.mSerializableData, false);

                        if (!createAssociation && !string.Equals(templateLibrary.Title, "NintexWorkflows", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!isReusable)
                            {
                                try
                                {
                                    IAveWorkflowAssociation tempAssociation = assoUnit.mSPAssoicationCollection.GetAssociationByName(assoUnit.SerializableData.mName, CultureInfo.CurrentUICulture);
                                    string noCodeWorkflowName = null;
                                    Guid listId = Guid.Empty;
                                    int cfgFileItemId = -1;
                                    int cfgFileVersion = -1;
                                    SPWorkflowSubListUnit.GetInfoFromInternalName(tempAssociation.InternalName, out noCodeWorkflowName, out listId, out cfgFileItemId, out cfgFileVersion);
                                    if (cfgFileVersion > 0 && cfgFileVersion != configSPFile.UIVersion)
                                    {
                                        string newInternalName = string.Format("{0}\n\n<Cfg.{1}.{2}.{3}.>", noCodeWorkflowName, listId.ToString().Replace('-', '_'), cfgFileItemId, configSPFile.UIVersion);
                                        SPWFAssociationProcNative.UpdateAssociationName(tempAssociation, newInternalName, false);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Log(AveLogLevel.WARN, "cannot update exist workflow association internal name. exception:{0}", ex.ToString());
                                }
                            }
                        }
                    }
                    else
                    {
                        string strContent = string.Empty;
                        using (StreamReader objReader = new StreamReader(configSPFile.OpenBinaryStream(WrapperConfiguration.OpenBinaryOptions)))
                        {
                            strContent = objReader.ReadToEnd();
                        }
                        XmlDocument xmlConfig = null;
                        bool removeTemplateFolder = false;
                        try
                        {
                            xmlConfig = new XmlDocument();
                            xmlConfig.LoadXml(strContent);
                            if (xmlConfig.SelectSingleNode("/WorkflowConfig/Association/@ListID") != null)
                            {
                                string listId = xmlConfig.SelectSingleNode("/WorkflowConfig/Association/@ListID").Value;
                                if (!string.IsNullOrEmpty(listId) && !string.Equals(listId, assoUnit.ParentId, StringComparison.OrdinalIgnoreCase))
                                {
                                    logger.Warn("Workflow:{0} associate with different list:{1}, current list:{2}", configSPFile.Name, listId, assoUnit.ParentId);
                                    Guid listGuid = new Guid(listId);
                                    IAveList tempList = null;
                                    try
                                    {
                                        tempList = web.Lists[listGuid];
                                    }
                                    catch (Exception ex)
                                    {
                                        logger.Info("Cannot get workflow association list by id:{0}, will remove the workflow tempalte folder. exception:{1}", listGuid, ex.ToString());
                                        removeTemplateFolder = true;
                                    }
                                    if (tempList != null)
                                    {
                                        logger.Warn("The workflow is associated with another exist list:{0}", tempList.Title);
                                        throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociatingWorkflowException);
                                    }
                                }
                            }
                        }
                        catch (SPWFProcessorException procException)
                        {
                            SPWorkflowProcessorRuntime.Log(Logs.Markup_APIResultException, procException.Message);
                            logger.Log(AveLogLevel.WARN, "An processor error occurred while handling template for file units, Error message: {0}", procException);
                            throw;
                        }
                        catch (Exception ex)
                        {
                            logger.Warn("An exception occurred while try to verify template folder. exception:{0}", ex.ToString());
                        }
                        if (removeTemplateFolder)
                        {
                            parentFolder.Delete();
                            return RestoreWorkflowTemplateFiles(assoUnit, containerListUnit, createAssociation, isReusable);
                        }
                    }
                }

                #endregion

                return configFileRelativeUrl;
            }

        }

        private static IAveList GetTemplateLibrary(IAveWeb tempWeb, SPWorkflowSubListSerializableData serializableData)
        {
            IAveList templateLibrary = tempWeb.GetListByName(serializableData.mTitle, false);
            if (templateLibrary == null)
            {
                logger.Debug("Cannot get template library by title {0},trying to get it by name {1}.", serializableData.mTitle, serializableData.mLeafName);
                templateLibrary = tempWeb.GetListByName(serializableData.mLeafName, true);
            }
            return templateLibrary;
        }
        private static void UpdateConfigFileReplaceDictionary(SPWorkflowSubFileUnit configFileUnit, Dictionary<string, object> replaceConfigDic, bool needReplaceBaseId, Guid baseId)
        {
            string category;
            if (configFileUnit.SerializableData.mGUIDDictionary.TryGetValue(AveWorkflowConstants.ReplaceDictionary_Category, out category))
            {
                replaceConfigDic.Add(AveWorkflowConstants.ReplaceDictionary_Category, category);
            }
            string contentTypeId;
            if (configFileUnit.SerializableData.mGUIDDictionary.TryGetValue(AveWorkflowConstants.ReplaceDictionary_ContentTypeID, out contentTypeId))
            {
                replaceConfigDic.Add(AveWorkflowConstants.ReplaceDictionary_ContentTypeID, contentTypeId);
            }
            if (needReplaceBaseId)
            {
                Guid mappingBaseId;
                if (!SPWorkflowProcessorRuntime.MappingManager.SiteMappingManager.TryGetWorkflowBaseId(baseId, out mappingBaseId))
                {
                    mappingBaseId = Guid.NewGuid();
                    SPWorkflowProcessorRuntime.MappingManager.SiteMappingManager.AddWorkflowBaseIdMapping(baseId, mappingBaseId);
                }
                replaceConfigDic["BaseID"] = mappingBaseId.ToString("B");
                logger.Info("Replace workflow baseid from {0} to {1}.", baseId, mappingBaseId);
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "wfconfig is part of file name")]
        private static IAveFile CreateTemplateFileIfNotExist(IAveFolder parentFolder, SPWorkflowSubFileUnit fileUnit, IAveList tempLib, ref bool needReplaceConfigFile, out bool needUpdate)
        {
            IAveFile templateFile = null;
            needUpdate = true;
            try
            {
                templateFile = parentFolder.Files[fileUnit.SerializableData.mName];
                //curFile.Delete();
                //curFile = null;
                if (templateFile.UIVersion > fileUnit.SerializableData.mUIVersion)
                {
                    //Office 365 do not support File Version's properties.
                    IAveFileVersion fileVersion = templateFile.Versions.GetVersionFromID(fileUnit.SerializableData.mUIVersion);
                    if (fileVersion.Properties != null && fileVersion.Properties["vti_timelastmodified"] != null && (DateTime)fileVersion.Properties["vti_timelastmodified"] == fileUnit.SerializableData.mModified)
                    {
                        if (templateFile.Name.ToLower(CultureInfo.CurrentCulture).EndsWith(".xoml.wfconfig.xml", StringComparison.Ordinal))
                        {
                            needReplaceConfigFile = false;
                        }
                        needUpdate = false;
                    }
                }
                else if (templateFile.UIVersion == fileUnit.SerializableData.mUIVersion)
                {
                    if (templateFile.TimeLastModified == fileUnit.SerializableData.mModified)
                    {
                        if (templateFile.Name.ToLower(CultureInfo.CurrentCulture).EndsWith(".xoml.wfconfig.xml", StringComparison.Ordinal))
                        {
                            needReplaceConfigFile = false;
                        }
                        needUpdate = false;
                    }
                }
                if (SPWorkflowProcessorRuntime.TemplateFileConflictRules == TemplateFileConflictRulesEnum.KeepSource && needUpdate)
                {
                    try
                    {
                        if (templateFile.Level != AveFileLevel.Checkout)
                        {
                            templateFile.CheckOut(false, string.Empty);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.CheckOutFileError, e.ToString());
                    }
                    templateFile.SaveBinary(fileUnit.mSerializableData.mContent);
                    //调用SaveBinary后,如果对DB没有Full Control权限,后续走API更新,会出现对象不一致的现象。在这里需要重新获取一下File对象。
                    if (templateFile.Web.Site.APIType == AveAPIType.Server &&
                        templateFile.ParentFolder.ParentWeb.Site.NativeApiPermission != WrapperNativeApiPermission.FullControl)
                    {
                        templateFile = templateFile.ParentFolder.ParentWeb.GetFile(templateFile.ServerRelativeUrl);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.GetFileByNameError, e.Message);
                if (templateFile == null)
                {
                    bool needDisableForceCheckout = false;
                    if (!tempLib.ForceCheckout)
                    {
                        tempLib.ForceCheckout = true;
                        tempLib.Update();
                        needDisableForceCheckout = true;
                    }
                    try
                    {
                        templateFile = parentFolder.Files.Add(fileUnit.SerializableData.mName, fileUnit.mSerializableData.mContent);
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("An exception occurred while create workflow template file {0}, exception:{1}", fileUnit.SerializableData.mName, ex.ToString());
                    }
                    finally
                    {
                        if (needDisableForceCheckout)
                        {
                            tempLib.ForceCheckout = false;
                            tempLib.Update();
                        }
                    }
                    int docFlags = fileUnit.SerializableData.mDocFlags;
                    if (templateFile != null && docFlags != 0 && (docFlags & 0x00080000) != 0)
                    {
                        UpdateWorkflowTemplateFileDocFlags(templateFile);
                        templateFile = parentFolder.ParentWeb.GetFile(templateFile.ServerRelativeUrl);
                    }
                }
            }
            return templateFile;
        }

        private static void UpdateStatusField(SPWFAssociationUnit assoUnit, Dictionary<Guid, string> oldIdAssoNameMap, out IAveWorkflowAssociation spAssociation)
        {
            spAssociation = null;
            bool getSameNameFirst = false;
            for (int index = 0; index < assoUnit.SPAssoicationCollection.Count; index++)
            {
                var wa = assoUnit.SPAssoicationCollection[index];
                if (wa.Name.Equals(assoUnit.SerializableData.mName))
                {
                    spAssociation = wa;
                    getSameNameFirst = true;
                    UpdateStatusFieldInternal(assoUnit, ref spAssociation);
                    break;
                }
            }
            if (!getSameNameFirst)
            {
                for (int index = 0; index < assoUnit.SPAssoicationCollection.Count; index++)
                {
                    var wa = assoUnit.SPAssoicationCollection[index];
                    if (!oldIdAssoNameMap.ContainsKey(wa.ID))
                    {
                        spAssociation = wa;
                        UpdateStatusFieldInternal(assoUnit, ref spAssociation);
                        break;
                    }
                }
            }
        }

        private static void UpdateStatusFieldInternal(SPWFAssociationUnit assoUnit, ref IAveWorkflowAssociation spAssociation)
        {
            if (string.IsNullOrEmpty(assoUnit.SerializableData.mStatusFieldName))
            {
                return;
            }
            string statusFieldInternalName = spAssociation.InternalNameStatusField;
            if (string.IsNullOrEmpty(statusFieldInternalName))
            {
                return;
            }
            try
            {
                if (SPWorkflowCommon.ContainsStatusField(assoUnit.SerializableData.mStatusFieldName))
                {
                    if (!statusFieldInternalName.Equals(SPWorkflowCommon.GetStatusFieldValue(assoUnit.SerializableData.mStatusFieldName), StringComparison.OrdinalIgnoreCase))
                    {
                        object statusFieldObj = spAssociation.ParentList.Fields.GetFieldByInternalName(statusFieldInternalName, false);
                        if (statusFieldObj != null)
                        {
                            IAveField statusField = statusFieldObj as IAveField;
                            statusField.ReadOnlyField = false;
                            statusField.Update();
                            statusField.Delete();
                        }
                        spAssociation.InternalNameStatusField = SPWorkflowCommon.GetStatusFieldValue(assoUnit.SerializableData.mStatusFieldName);
                        assoUnit.UpdateWorkflowAssociation(spAssociation);
                    }
                }
                else
                {
                    SPWorkflowCommon.AddStatusFieldValue(assoUnit.SerializableData.mStatusFieldName, statusFieldInternalName);
                }
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.StatusFieldSetError, statusFieldInternalName, e);
            }
        }

        private static bool IsListAlreadyContainWf(string wfName, IAveList parentList)
        {
            bool isListAlreadyIncludeWf = false;
            if (parentList != null && parentList.WorkflowAssociations != null)
            {
                foreach (var wf in parentList.WorkflowAssociations)
                {
                    if (wf.Name.Equals(wfName, StringComparison.OrdinalIgnoreCase))
                    {
                        isListAlreadyIncludeWf = true;
                        break;
                    }
                }
            }
            return isListAlreadyIncludeWf;
        }

        /// <summary>
        /// 更新nintex workflow tempalte library中的template file上的property
        /// </summary>
        /// <param name="parentAsso"></param>
        /// <param name="fileUnit"></param>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Update is a whole word")]
        private static void UpdateNWFileProperties(SPWFAssociationUnit parentAsso, SPWorkflowSubFileUnit fileUnit)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("RestoreOneAssociation.RestoreNintexWorkflowData.UpdateNWFileProperties"))
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "UpdateNWFileProperties:" + fileUnit.SerializableData.mName);
                logger.Debug("Update nintex workflow tempalte file properties.FileName:{0}", fileUnit.SerializableData.mName);
                if (!fileUnit.SerializableData.mName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) &&
                    !fileUnit.SerializableData.mName.EndsWith(".xoml", StringComparison.OrdinalIgnoreCase) &&
                    !fileUnit.SerializableData.mName.EndsWith(".rules", StringComparison.OrdinalIgnoreCase) &&
                    !fileUnit.SerializableData.mName.EndsWith(".xsn", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
                try
                {
                    IAveFile file = fileUnit.mSPFile;
                    try
                    {
                        if (file.Level != AveFileLevel.Checkout)
                        {
                            file.CheckOut(false, string.Empty);
                        }
                    }
                    catch (Exception e)
                    {
                        SPWorkflowProcessorRuntime.Log(Logs.Common_SPFileCheckOutException, e.Message);
                        logger.Warn("An exception occurred while checkout file. exception:{0}", e.ToString());
                    }

                    Dictionary<string, string> needUpdateFileProperties = new Dictionary<string, string>();
                    Dictionary<string, object> needUpdateFieldValues = new Dictionary<string, object>();

                    string name = (parentAsso.SerializableData.mIsNintexReusableWorkflow || parentAsso.SerializableData.mIsNintexReusableWorkflow
                        || parentAsso.SPAssociation == null) ? file.ParentFolder.Name : parentAsso.SPAssociation.Name;
                    needUpdateFileProperties.Add("vti_title", name);

                    string workflowCategory = "";
                    if (parentAsso.SerializableData.mIsNintexSiteCollectionReusableWorklfow)
                    {
                        workflowCategory = "GloballyReusable";
                    }
                    else if (parentAsso.SerializableData.mIsNintexReusableWorkflow)
                    {
                        workflowCategory = "Reusable";
                    }
                    else if (parentAsso.ParentObjectType == SPWFAssociationParentType.Web)
                    {
                        workflowCategory = "Site";
                    }
                    else
                    {
                        workflowCategory = "List";
                    }
                    if (!string.IsNullOrEmpty(workflowCategory))
                    {
                        needUpdateFileProperties.Add("WorkflowCategory", workflowCategory);
                        needUpdateFieldValues.Add("WorkflowCategory", workflowCategory);
                    }

                    string baseIdStr = parentAsso.SerializableData.mBaseId.ToString("B");
                    needUpdateFileProperties.Add("NintexWorkflowID", baseIdStr);
                    needUpdateFieldValues.Add("NintexWorkflowID", baseIdStr);

                    if (parentAsso.SPAssociation != null)
                    {
                        string description = parentAsso.SPAssociation.Description;
                        needUpdateFileProperties.Add("NintexWorkflowDescription", description);
                        needUpdateFieldValues.Add("NintexWorkflowDescription", description);

                        if (!parentAsso.SerializableData.mIsNintexReusableWorkflow && parentAsso.ParentObjectType == SPWFAssociationParentType.List)
                        {
                            string listId = parentAsso.SPAssociation.ParentList.ID.ToString("B");
                            needUpdateFileProperties.Add("AssociatedListID", listId);
                            needUpdateFieldValues.Add("AssociatedListID", listId);
                        }
                    }
                    else
                    {
                        var configFileUnit = parentAsso.TemplateLibUnit.mTemplateFileUnits.FirstOrDefault(subFile => subFile.SerializableData.mName.EndsWith(".xoml.wfconfig.xml", StringComparison.OrdinalIgnoreCase));
                        if (configFileUnit != null)
                        {
                            var content = configFileUnit.SerializableData.mContent;
                            if (content != null)
                            {
                                var properties = ConfigFileProc.GetTemplateProperties(content);
                                string description = null;
                                if (properties.TryGetValue("Description", out description))
                                {
                                    needUpdateFileProperties.Add("NintexWorkflowDescription", description);
                                    needUpdateFieldValues.Add("NintexWorkflowDescription", description);
                                }
                            }
                        }
                    }

                    var templateParentWebId = parentAsso.SerializableData.mIsNintexSiteCollectionReusableWorklfow ?
                           parentAsso.ParentWeb.Site.RootWeb.ID.ToString("B").ToUpper(CultureInfo.InvariantCulture) :
                           parentAsso.ParentWeb.ID.ToString("B").ToUpper(CultureInfo.InvariantCulture);
                    needUpdateFileProperties.Add("WebID", templateParentWebId);
                    needUpdateFileProperties.Add("NWAssociatedWebID", templateParentWebId);
                    needUpdateFieldValues.Add("WebID", templateParentWebId);
                    needUpdateFieldValues.Add("NWAssociatedWebID", templateParentWebId);

                    #region ensure field

                    if (!file.Item.Fields.ContainsField("WorkflowCategory"))
                    {
                        IAveField webField = file.ParentFolder.ParentWeb.AvailableFields.GetFieldById(NWSharePointObjects.FieldWorkflowCategory, false);
                        if (webField != null)
                        {
                            file.Item.Fields.Add(webField);
                        }
                        else
                        {
                            logger.Warn("The web do not have the field:WorkflowCategory,The file url:{0}.", file.Url);
                        }
                    }

                    #endregion ensure field

                    foreach (var property in needUpdateFileProperties)
                    {
                        file.Item.Properties[property.Key] = property.Value;
                    }

                    foreach (var field in needUpdateFieldValues.Where(field => file.Item.Fields.ContainsField(field.Key)))
                    {
                        file.Item[field.Key] = field.Value;
                    }

                    file.Item.Update();
                }
                catch (Exception e)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_UpdateTempFilePropsUnknownException, e.Message);
                    logger.Warn("An exception occurred while update nintex workflow file proprties. exception:{0}", e);
                }
                finally
                {
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "UpdateNWFileProperties:" + fileUnit.SerializableData.mName);
                }
            }
        }

        private static void UpdateTemplateFileVersion(IAveList list, IAveFile file, SPWorkflowSubFileSerializableData data, bool removeMiddleVersion)
        {
            int uiVersion = data.mUIVersion;
            int majorVersion = uiVersion / 512;
            int minorVersion = uiVersion % 512;
            bool enableModeration = list.EnableModeration;
            bool enableMinorVersion = list.EnableMinorVersions;
            bool changed = false;
            if (list.EnableModeration == true)
            {
                list.EnableModeration = false;
                changed = true;
            }
            if (minorVersion > 0 && !list.EnableMinorVersions)
            {
                list.EnableMinorVersions = true;
                changed = true;
            }
            if (changed)
            {
                list.Update();
            }
            List<int> middleVersions = new List<int>();
            while (file.UIVersion < majorVersion * 512)
            {
                if (file.Level == AveFileLevel.Checkout)
                {
                    file.CheckIn(string.Empty, AveCheckinType.MajorCheckIn);
                    middleVersions.Add(file.UIVersion);
                }
                else
                {
                    file.CheckOut();
                    file.CheckIn(string.Empty, AveCheckinType.MajorCheckIn);
                    middleVersions.Add(file.UIVersion);
                }
            }
            while (file.UIVersion < (majorVersion * 512 + minorVersion))
            {
                if (file.Level == AveFileLevel.Checkout)
                {
                    file.CheckIn(string.Empty, AveCheckinType.MinorCheckIn);
                    middleVersions.Add(file.UIVersion);
                }
                else
                {
                    file.CheckOut();
                    file.CheckIn(string.Empty, AveCheckinType.MinorCheckIn);
                    middleVersions.Add(file.UIVersion);
                }
            }
            if (file.Level == AveFileLevel.Checkout)
            {
                if (data.mUIVersion % 512 > 0)
                {
                    file.CheckIn(string.Empty, AveCheckinType.MinorCheckIn);
                }
                else
                {
                    file.CheckIn(string.Empty, AveCheckinType.MajorCheckIn);
                }
            }
            if (removeMiddleVersion)
            {
                foreach (int versionId in middleVersions)
                {
                    try
                    {
                        IAveFileVersion version = file.Versions.GetVersionFromID(versionId);
                        version.Delete();
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("cannot remove middle version, file:{0}, exception:{1}", file.ServerRelativeUrl, ex.ToString());
                    }
                }
            }
            if (file.Web.Site.APIType == AveAPIType.Server)
            {
                using (IAveBackupRestoreQueryService queryService = AveObjectModelFactory.CreateObjectModelFactory(null, null, AveContextKind.Auto).CreateQueryService<IAveBackupRestoreQueryService>(file.ParentFolder.ParentWeb.Site))
                {
                    string editor = file.ModifiedBy.ID.ToString();
                    var mEditor = SPWorkflowProcessorRuntime.OnUserMapping(data.mEditorLogin);
                    if (mEditor != null)
                    {
                        editor = mEditor.ID.ToString();
                    }
                    string author = file.Author.ID.ToString();
                    var mAuthor = SPWorkflowProcessorRuntime.OnUserMapping(data.mAuthorLogin);
                    if (mAuthor != null)
                    {
                        author = mAuthor.ID.ToString();
                    }
                    DateTime modified = data.mModified;
                    DateTime created = data.mCreated;

                    var item = file.Item;
                    AveBaseItemInfo info = new AveBaseItemInfo();
                    info.SiteId = file.ParentFolder.ParentWeb.Site.ID;
                    //info.GUID = file.UniqueId;
                    info.ParentId = file.ParentFolder.UniqueId;
                    info.Level = (int)file.Level;
                    //info.OriginalVersion = file.UIVersion;
                    if (file.ParentFolder.ParentWeb.Site.NativeApiPermission == WrapperNativeApiPermission.FullControl)
                    {
                        //queryService.UpdateAllDocsPropertyByNative(info, created, modified, file.UIVersion);
                        //queryService.UpdateSpecialPropertyByNative(editor, author, modified, created, info);
                        Dictionary<string, object> docData = new Dictionary<string, object>
                        {
                            { "TimeCreated",created},
                             { "TimeLastModified",modified}
                        };
                        Dictionary<string, object> userData = new Dictionary<string, object>
                        {
                            { "tp_Created",created},
                             { "tp_Modified",modified}
                        };
                        if (mEditor != null)
                        {
                            userData.AddEx("tp_Editor", mEditor.ID);
                            SetFieldValueIntoChangedUserData(item, "Modified_x0020_By", mEditor.LoginName, userData);
                        }
                        if (mAuthor != null)
                        {
                            userData.AddEx("tp_Author", mAuthor.ID);
                            SetFieldValueIntoChangedUserData(item, "Created_x0020_By", mAuthor.LoginName, userData);
                        }
                        queryService.ChangeDocdataByNative(info, file.UniqueId, docData);
                        queryService.ChangeUserdataByNative(info, file.UniqueId, userData);
                    }
                    else
                    {
                        IAveTimeZone zone = file.ParentFolder.ParentWeb.RegionalSettings.TimeZone;
                        DateTime modified_Local = zone.UTCToLocalTime(modified);
                        DateTime created_Local = zone.UTCToLocalTime(created);
                        file.Item["Editor"] = editor;
                        file.Item["Author"] = author;
                        file.Item["Modified"] = modified_Local;
                        file.Item["Created"] = created_Local;
                        file.Item.UpdateOverwriteVersion();
                        logger.Warn("Update template file properties using UpdateOverwriteVersion. File Url:{0}", file.ServerRelativeUrl);
                    }
                }
            }
            else
            {
                if (file.Item != null)//Office 365 do not support QueryService, Update the item with API.
                {
                    string editor = file.ModifiedBy.ID.ToString();
                    var mEditor = SPWorkflowProcessorRuntime.OnUserMapping(data.mEditorLogin);
                    if (mEditor != null)
                    {
                        editor = mEditor.ID.ToString();
                    }
                    string author = file.Author.ID.ToString();
                    var mAuthor = SPWorkflowProcessorRuntime.OnUserMapping(data.mAuthorLogin);
                    if (mAuthor != null)
                    {
                        author = mAuthor.ID.ToString();
                    }
                    DateTime modified = data.mModified;
                    DateTime created = data.mCreated;
                    file.Item["Editor"] = editor;
                    file.Item["Author"] = author;
                    file.Item["Modified"] = modified;
                    file.Item["Created"] = created;
                    file.Item.SystemUpdate(false);
                }
            }
            if (changed)
            {
                list.EnableModeration = enableModeration;
                list.EnableMinorVersions = enableMinorVersion;
                list.Update();
            }
        }

        private static void SetFieldValueIntoChangedUserData(IAveListItem item, string fieldName, object fieldValue, Dictionary<string, object> userData)
        {
            var fieldModified = item.ParentList.Fields.GetField(fieldName);
            string columnName = fieldModified.GetProperty("ColName");
            if (!string.IsNullOrEmpty(columnName))
            {
                userData.AddEx(columnName, fieldValue);
            }
        }

        internal static int GetWorkflowTemplateFileDocFlags(IAveFile file)
        {
            int docFlags = 0;
            if (file.Web.Site.APIType != AveAPIType.Server)
            {
                return 0;
            }
            using (IAveBackupRestoreQueryService queryService = AveObjectModelFactory.CreateObjectModelFactory(null, null, AveContextKind.Auto).CreateQueryService<IAveBackupRestoreQueryService>(file.ParentFolder.ParentWeb.Site))
            {
                AveBaseItemInfo itemInfo = new AveBaseItemInfo();
                itemInfo.GUID = file.UniqueId;
                itemInfo.SiteId = file.ParentFolder.ParentWeb.Site.ID;
                itemInfo.ParentId = file.ParentFolder.UniqueId;
                itemInfo.Version = file.UIVersion;
                docFlags = queryService.GetDocFlag(itemInfo);
            }
            return docFlags;
        }

        private static void UpdateWorkflowTemplateFileDocFlags(IAveFile file)
        {
            if (file.Web.Site.APIType != AveAPIType.Server)
            {
                return;
            }
            if (file.Web.Site.NativeApiPermission != WrapperNativeApiPermission.FullControl)
            {
                logger.Log(AveLogLevel.WARN, "Skip updating workflow template file flags because of permission. File url:{0}", file.ServerRelativeUrl);
                return;
            }
            using (IAveBackupRestoreQueryService queryService = AveObjectModelFactory.CreateObjectModelFactory(null, null, AveContextKind.Auto).CreateQueryService<IAveBackupRestoreQueryService>(file.ParentFolder.ParentWeb.Site))
            {
                queryService.UpdateWorkflowTemplateFileDocFlags(file.ParentFolder.ParentWeb.Site.ID, file.ParentFolder.UniqueId, file.UniqueId, (byte)file.Level);
            }
        }

        /// <summary>
        /// 获取workflow association上的associated field 数据
        /// </summary>
        /// <param name="listUnit">template文件的备份数据</param>
        /// <returns>返回field id，internal name的dictionary集合</returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = ".xoml.wfconfig.xml:Config file name.")]
        public static Dictionary<Guid, string> GetAssociatedFields(SPWorkflowSubListUnit listUnit)
        {
            Dictionary<Guid, string> associatedFields = new Dictionary<Guid, string>();

            if (listUnit == null)
            {
                logger.Warn("Invalid parameter listUnit for getting workflow associated fields.");
                return associatedFields;
            }

            if (listUnit.mTemplateFileUnits == null)
            {
                logger.Warn("Invalid parameter listUnit.mTemplateFileUnits for getting workflow associated fields.");
                return associatedFields;
            }
            try
            {
                bool isUTF8Unicode = false;
                var configFile = GetConfigFile(listUnit);
                string strContent = GetContentString(configFile, out isUTF8Unicode);

                if (!string.IsNullOrEmpty(strContent))
                {

                    var xmlConfig = new XmlDocument();
                    xmlConfig.LoadXml(strContent);
                    XmlNodeList fieldLists = xmlConfig.SelectNodes("/WorkflowConfig/Extended/Fields/Field");
                    if (fieldLists != null && fieldLists.Count > 0)
                    {
                        foreach (XmlNode node in fieldLists)
                        {
                            if (node.Attributes != null)
                            {
                                XmlAttribute idAttr = node.Attributes["ID"];
                                XmlAttribute nameAttr = node.Attributes["Name"];
                                if (idAttr != null && nameAttr != null)
                                {
                                    string id = idAttr.Value;

                                    if (Validator.IsGuid(id))
                                    {
                                        Guid fieldId = new Guid(id);
                                        string name = nameAttr.Value;
                                        associatedFields.Add(fieldId, name);
                                    }
                                }
                            }
                        }
                    }

                }
            }
            catch (SPWFProcessorException procException)
            {
                SPWorkflowProcessorRuntime.Log(Logs.Markup_APIResultException, procException.Message);
                logger.Log(AveLogLevel.WARN, "An processor error occurred while handling template for file units, Error message: {0}", procException);
                throw;
            }
            catch (Exception ex)
            {
                logger.Warn("An exception occurred while try to verify template folder. exception:{0}", ex.ToString());
            }
            return associatedFields;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = ".xoml.wfconfig.xml:Config file name.")]
        private static SPWorkflowSubFileUnit GetConfigFile(SPWorkflowSubListUnit listUnit)
        {
            return listUnit.mTemplateFileUnits.FirstOrDefault(fileUnit =>
               !string.IsNullOrEmpty(fileUnit.SerializableData.mName)
               && fileUnit.SerializableData.mName.EndsWith(".xoml.wfconfig.xml", StringComparison.OrdinalIgnoreCase));
        }

        private static string GetContentString(SPWorkflowSubFileUnit fileUnit, out bool isUTF8Unicode)
        {
            string strContent = string.Empty;
            isUTF8Unicode = false;
            byte[] content = fileUnit.SerializableData.mContent;
            if (content == null || content.Length == 0)
            {
                return strContent;
            }

            if (content[0] == 255 && content[1] == 254)
            {
                //Unicode
                strContent = Encoding.Unicode.GetString(content);
            }
            else
            {
                //UTF8
                strContent = Encoding.UTF8.GetString(content);
                isUTF8Unicode = true;
            }
            return strContent;
        }

        /// <summary>
        /// 根据mapping关系更新workflow config文件中的associated field id，internal name
        /// </summary>
        /// <param name="listUnit">template文件备份数据</param>
        /// <param name="fieldMapping">web field mapping</param>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = ".xoml.wfconfig.xml:Config file name.")]
        public static void UpdateAssociatedFields(SPWorkflowSubListUnit listUnit, IAveFieldMapping fieldMapping)
        {
            if (listUnit == null)
            {
                logger.Warn("Invalid parameter listUnit for updating associated fields in workflow template.");
                return;
            }

            if (listUnit.mTemplateFileUnits == null)
            {
                logger.Warn("Invalid parameter listUnit.mTemplateFileUnits for updating associated fields in workflow template.");
                return;
            }

            if (fieldMapping == null)
            {
                logger.Warn("Invalid parameter fieldMapping for updating associated fields in workflow template.");
                return;
            }

            SPWorkflowSubFileUnit configFile = GetConfigFile(listUnit);
            if (configFile == null)
            {
                return;
            }

            bool isUTF8Unicode = false;
            bool replaced = false;
            string strContent = string.Empty;
            try
            {
                strContent = GetContentString(configFile, out isUTF8Unicode);

                if (!string.IsNullOrEmpty(strContent))
                {
                    var xmlConfig = new XmlDocument();
                    xmlConfig.LoadXml(strContent);
                    XmlNodeList fieldLists = xmlConfig.SelectNodes("/WorkflowConfig/Extended/Fields/Field");
                    if (fieldLists != null && fieldLists.Count > 0)
                    {
                        foreach (XmlNode node in fieldLists)
                        {
                            if (node.Attributes != null)
                            {
                                XmlAttribute idAttr = node.Attributes["ID"];
                                XmlAttribute nameAttr = node.Attributes["Name"];
                                if (idAttr != null && nameAttr != null)
                                {
                                    string id = idAttr.Value;

                                    if (Validator.IsGuid(id))
                                    {
                                        Guid fieldId = new Guid(id);
                                        //Name attribute format __field+InternalName
                                        string nameAttributeValue = nameAttr.Value;
                                        //prefix length is 7
                                        if (nameAttributeValue.Length > 7)
                                        {
                                            string namePrefix = nameAttributeValue.Substring(0, 7);
                                            string name = nameAttributeValue.Substring(7);
                                            Guid mappedFieldId = fieldMapping.GetMappingRestoredFieldId(fieldId);
                                            if (mappedFieldId != Guid.Empty)
                                            {
                                                node.Attributes["ID"].Value = mappedFieldId.ToString("B").ToUpperInvariant();
                                                replaced = true;
                                                logger.Debug("Associated field ID mapping: {0}  {1}", fieldId, mappedFieldId);
                                            }
                                            string mappedFieldName = fieldMapping.GetMappingRestoredFieldInternalName(name);
                                            if (!string.IsNullOrEmpty(mappedFieldName) && !string.Equals(mappedFieldName, name, StringComparison.Ordinal))
                                            {
                                                node.Attributes["Name"].Value = namePrefix + mappedFieldName;
                                                replaced = true;
                                                logger.Debug("Associated field name mapping: {0}  {1}", fieldId, mappedFieldName);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    strContent = xmlConfig.OuterXml;
                }
            }
            catch (Exception ex)
            {
                logger.Warn("An exception occurred while update associated fields. Exception: {0}", ex.ToString());
                replaced = false;
            }
            if (replaced)
            {
                byte[] content = null;
                if (isUTF8Unicode)
                {
                    content = Encoding.UTF8.GetBytes(strContent);
                }
                else
                {
                    content = Encoding.Unicode.GetBytes(strContent);
                }
                configFile.SerializableData.mContent = content;
            }
        }

        /// <summary>
        /// for 13mode workflow
        /// 处理workflow template file的content的替换逻辑
        /// </summary>
        /// <param name="assoUnit"></param>
        /// <param name="containerListUnit"></param>
        /// <param name="xamlFileContent"></param>
        /// <returns></returns>
        public static bool HandleTemplateSPFileUnits(SPWFAssociationUnit assoUnit, SPWorkflowSubListUnit containerListUnit, out string xamlFileContent)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("RestoreOneAssociation.HandleTemplateSPFileUnits1"))
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "HandleTemplateSPFileUnitsFor13Model");
                try
                {
                    IAveWeb web = assoUnit.ParentWeb;
                    {
                        IAveList taskList = (assoUnit.mTaskListUnit != null) ? web.Lists.GetListByName(assoUnit.mTaskListUnit.SerializableData.mTitle, false) : null;
                        //由于task在前面有可能是用mLeafName取的，所以在此处加上用mLeafname取task list的代码.
                        if (taskList == null && assoUnit.mTaskListUnit != null)
                        {
                            taskList = web.Lists.GetListByName(assoUnit.mTaskListUnit.SerializableData.mLeafName, false);
                        }
                        IAveList histList = (assoUnit.mHistListUnit != null) ? web.GetListByName(assoUnit.mHistListUnit.SerializableData.mTitle, true) : null;
                        IAveList tempLib = web.GetListByName(containerListUnit.SerializableData.mTitle, true);
                        Dictionary<string, object> replaceDic = new Dictionary<string, object>();
                        if (assoUnit.mTemplateLibUnit != null)
                        { replaceDic.Add(assoUnit.mTemplateLibUnit.SerializableData.mId.ToString().ToUpperInvariant(), tempLib.ID.ToString().ToUpperInvariant()); }
                        if (assoUnit.mTaskListUnit != null)
                        { replaceDic.Add(assoUnit.mTaskListUnit.SerializableData.mId.ToString().ToUpperInvariant(), taskList.ID.ToString().ToUpperInvariant()); }
                        if (assoUnit.mHistListUnit != null)
                        { replaceDic.Add(assoUnit.mHistListUnit.SerializableData.mId.ToString().ToUpperInvariant(), histList.ID.ToString().ToUpperInvariant()); }
                        if (!string.IsNullOrEmpty(assoUnit.OriginalParentId))
                        {
                            string originalParentId = assoUnit.OriginalParentId.ToUpperInvariant();
                            string parentId = assoUnit.ParentId.ToUpperInvariant();
                            replaceDic.Add(originalParentId, parentId);
                            //在处理CT association时，原端备份的OriginalParentId就是CT.id.tostring()，而不像list,web那样是id.tostring("B")，所以CT association的OriginalParentId是不存在{}的.
                            string originalParentIdTrimChars = originalParentId.Trim(new char[] { '{', '}' });
                            if (!replaceDic.ContainsKey(originalParentIdTrimChars))
                            {
                                replaceDic.Add(originalParentIdTrimChars, parentId.Trim(new char[] { '{', '}' }));
                            }
                        }
                        Dictionary<string, object> replaceConfigDic = new Dictionary<string, object>();
                        replaceConfigDic.Add("ParentId", assoUnit.ParentId.ToUpperInvariant());
                        if (taskList != null)
                        { replaceConfigDic.Add("TaskListId", taskList.ID.ToString("B").ToUpperInvariant()); }
                        if (histList != null)
                        { replaceConfigDic.Add("HistListId", histList.ID.ToString("B").ToUpperInvariant()); }
                        replaceConfigDic.Add("BaseID", assoUnit.SerializableData.mBaseId.ToString("B").ToUpperInvariant());
                        object tempSiteCT = string.IsNullOrEmpty(assoUnit.reusableWFContentTypeName) ? null : web.ContentTypes[assoUnit.reusableWFContentTypeName];
                        if (tempSiteCT == null)
                        {
                            if (!string.IsNullOrEmpty(assoUnit.reusableWFContentTypeName) && assoUnit.ParentObjectType == SPWFAssociationParentType.ListContentType)
                            {
                                tempSiteCT = assoUnit.ParentContentType.ParentList.ContentTypes[assoUnit.reusableWFContentTypeName].Parent;
                            }
                        }
                        if (tempSiteCT != null)
                        { replaceConfigDic.Add("ContentTypeId", ((IAveContentType)tempSiteCT).ID.ToString()); }

                        if (assoUnit.mTaskListUnit != null && assoUnit.mTaskListUnit.mContentTypeIdMapping != null)
                        {
                            foreach (KeyValuePair<string, string> pair in assoUnit.mTaskListUnit.mContentTypeIdMapping)
                            {
                                replaceDic.Add(pair.Key, pair.Value);
                                replaceConfigDic.Add(pair.Key, pair.Value);
                            }
                        }

                        // Not support versions now...
                        SPWorkflowSubFileUnit fileUnit = null;
                        //找到最大的published template version,因为某些情况下无法保证备份出来的template的最大version始终在第一位(比如原端新publish一个version，然后再save一个version,current version是unpublish的，不会备份，所以第一个version会是最小version)
                        foreach (var unit in containerListUnit.mTemplateFileUnits)
                        {
                            if (unit != null)
                            {
                                if (fileUnit == null)
                                {
                                    fileUnit = unit;
                                }
                                else if (unit.SerializableData.mUIVersion > fileUnit.SerializableData.mUIVersion)
                                {
                                    fileUnit = unit;
                                }
                            }
                        }

                        SPWorkflowFileContentProc fileContentProc = SPWorkflowFileContentProc.CreateInstance(assoUnit, null, fileUnit.mSerializableData.mContent);
                        if (fileContentProc == null)
                        {
                            xamlFileContent = Encoding.UTF8.GetString(fileUnit.mSerializableData.mContent);
                            return false;
                        }

                        FixupDictionary(web, replaceDic, fileUnit.mSerializableData.mGUIDDictionary);
                        xamlFileContent = fileContentProc.ReplaceContent(replaceDic);
                        return true;
                    }
                }
                catch (SPWFProcessorException procException)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.Markup_ProcessTemplateFilesException, procException.Message);
                    logger.Log(AveLogLevel.DEBUG, "An processor error occurred while handling workflow sub items, error message: {0}", procException);
                    throw;
                }
                catch (Exception e)
                {
                    //SPWorkflowProcessorRuntime.Log(Logs.Markup_ProcessTemplateFilesException, e.Message);
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationDefinitionRestoreError, e);
                }
                finally
                {
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "HandleTemplateSPFileUnitsFor13Model");
                }
            }
        }

        internal static void FixupDictionary(IAveWeb currentWeb, Dictionary<string, object> repDic, Dictionary<string, string> dic)
        {
            if (dic == null)
                return;
            AveWorkflowReplaceProcessor replaceProcessor = new AveWorkflowReplaceProcessor(SPWorkflowProcessorRuntime.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true, true), SPWorkflowProcessorRuntime.MappingManager.SiteMappingManager.SourceSiteInfo, SPWorkflowProcessorRuntime.MappingManager.SiteMappingManager.DestSiteInfo.ServerRelativeUrl);
            var notFindListOrFolderName = new StringBuilder();
            foreach (KeyValuePair<string, string> pair in dic)
            {
                if (repDic.ContainsKey(pair.Key))
                {
                    continue;
                }
                string objName = pair.Value;
                if (!string.IsNullOrEmpty(objName))
                {
                    if (objName.StartsWith("[SiteID]", StringComparison.OrdinalIgnoreCase))
                    {
                        repDic.Add(pair.Key, currentWeb.Site.ID.ToString().ToUpper(CultureInfo.InvariantCulture));
                    }
                    else if (objName.StartsWith("[WebID]", StringComparison.OrdinalIgnoreCase))
                    {
                        Guid sourceWebId = new Guid(pair.Key);
                        if (SPWorkflowProcessorRuntime.MappingManager.SiteMappingManager.WebIDMapping.ContainsKey(sourceWebId))
                        {
                            repDic.Add(pair.Key, SPWorkflowProcessorRuntime.MappingManager.SiteMappingManager.WebIDMapping[sourceWebId].ToString().ToUpper(CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            string webUrl = objName.Substring(7);
                            webUrl = replaceProcessor.UrlReplace(webUrl);
                            try
                            {
                                using (IAveWeb web = currentWeb.Site.OpenWeb(webUrl))
                                {
                                    if (web != null)
                                    {
                                        repDic.Add(pair.Key, web.ID.ToString().ToUpper(CultureInfo.InvariantCulture));
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                SPWorkflowProcessorRuntime.Log("cannot get web by url:{0} in FixupDictionary, it may cause workflow definition not work property. exception:{1}", webUrl, ex.Message);
                                logger.Log(AveLogLevel.DEBUG, "Cannot get web by url:{0} in FixupDictionary, it may cause workflow definition not work property. Exception: {1}", webUrl, ex);
                            }
                        }
                    }
                    else if (objName.StartsWith("[FolderId]", StringComparison.OrdinalIgnoreCase))
                    {
                        string folderUrl = objName.Substring(10);
                        string webUrl = folderUrl.Substring(0, folderUrl.IndexOf('|'));
                        folderUrl = folderUrl.Substring(folderUrl.IndexOf('|') + 1);
                        webUrl = replaceProcessor.UrlReplace(webUrl);
                        folderUrl = replaceProcessor.UrlReplace(folderUrl);
                        IAveFolder folder = null;
                        try
                        {
                            using (IAveWeb web = currentWeb.Site.OpenWeb(webUrl))
                            {
                                folder = web.GetFolder(folderUrl);
                            }
                        }
                        catch (Exception ex)
                        {
                            SPWorkflowProcessorRuntime.Log("cannot get folder by url:{0} in FixupDictionary, it may cause workflow definition not work property. exception:{1}", folderUrl, ex.Message);
                            logger.Log(AveLogLevel.DEBUG, "Cannot get folder by url:{0} in FixupDictionary, It may cause workflow definition not work property. Exception: {1}", folderUrl, ex);
                        }
                        if (folder != null && folder.Exists)
                        {
                            repDic.Add(pair.Key, folder.UniqueId.ToString().ToUpper(CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            SPWorkflowProcessorRuntime.Log(Logs.Markup_MissingFolder, folderUrl);
                            notFindListOrFolderName.Append(string.Format(" {0} ,", folderUrl));
                        }
                    }
                    else if (string.Equals(pair.Key, AveWorkflowConstants.ReplaceDictionary_Category, StringComparison.OrdinalIgnoreCase)
                             || string.Equals(pair.Key, AveWorkflowConstants.ReplaceDictionary_ContentTypeID, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    else
                    {
                        string listName = objName;
                        string webUrl = string.Empty;
                        if (objName.StartsWith("[ListID]", StringComparison.OrdinalIgnoreCase))
                        {
                            listName = objName.Substring(8);
                            var index = listName.IndexOf("|", StringComparison.OrdinalIgnoreCase);
                            if (index > 0)
                            {
                                webUrl = replaceProcessor.UrlReplace(listName.Substring(0, index));
                                listName = listName.Substring(index + 1);
                            }
                        }
                        object listObj = null;
                        string mappingName;
                        if (SPWorkflowProcessorRuntime.MappingManager.SiteMappingManager.GetValueFromListTitleMappnig(currentWeb.ID, listName, out mappingName))
                        {
                            logger.Log(AveLogLevel.INFO, "Using list title mapping, {0}-->{1}", listName, mappingName);
                            listName = mappingName;
                        }
                        if (!string.IsNullOrEmpty(webUrl))
                        {
                            using (var web = currentWeb.Site.OpenWeb(webUrl))
                            {
                                listObj = GetListByTitle(web, listName);
                            }
                        }
                        else
                        {
                            listObj = GetListByTitle(currentWeb, listName);
                        }
                        if (listObj != null)
                        {
                            string idStr = ((IAveList)listObj).ID.ToString().ToUpper(CultureInfo.InvariantCulture);
                            repDic.Add(pair.Key, idStr);
                            SPWorkflowProcessorRuntime.Log(Logs.Markup_FoundListByTitle, listName, idStr);
                        }
                        else
                        {
                            SPWorkflowProcessorRuntime.Log(Logs.Markup_MissingList, listName);
                            notFindListOrFolderName.Append(string.Format(" {0} ,", listName));
                        }
                    }
                }
            }
            foreach (var ct in SPWorkflowProcessorRuntime.MappingManager.ListMappingManager.ListLevelCTIdMapping)
            {
                repDic.AddEx(ct.Key, ct.Value.ToString());
            }
            if (notFindListOrFolderName.Length > 0)
            {
                notFindListOrFolderName.Length = notFindListOrFolderName.Length - 2; //去掉结尾的' ,'
                throw new SPWFProcessorException(SPWFProcessorErrorCode.PutIntoPostAction, AveInternalResourceKey.Wrapper_Exception_Workflow_NotFindListOrFolder, notFindListOrFolderName.ToString(), currentWeb.Url);
            }
        }

        /// <summary>
        /// CI-44080 当web存在第二语言，用第二语言的list title 使用Web.Lists.GetByTitle 方法获取list会失败，只有使用default language的list title才能正常获取list，
        /// 如果使用Web.Lists.GetByTitle 方式获取失败 那就使用wrapper自己封装的方法 再尝试获取
        /// </summary>
        /// <param name="web"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        private static IAveList GetListByTitle(IAveWeb web, string title)
        {
            IAveList list = null;
            try
            {
                list = web.GetListByName(title, false);
                if (list == null)
                {

                    logger.Warn("Can not get list by name, sub web url: {0}, listName: {1}", web.Url, title);
                    list = web.Lists.GetByTitle(title);
                }
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while sub web get list by title, error: {0}", e);
            }
            return list;
        }

        private static void CheckWebServiceSuccess(string op, string resultText)
        {
            if ((resultText == null) || !resultText.Contains("<Success"))
            {
                if (op.Equals("ValidatingWorkflow"))
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.ValidatingWorkflowException, null, resultText);
                else if (op.Equals("AssociatingWorkflow"))
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociatingWorkflowException, null, resultText);
                else
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.WebServiceOperationNotSupported, AveInternalResourceKey.Wrapper_Exception_Workflow_ServiceNotAvailable, resultText);
            }
        }

        private static void GenerateWorkflowBaesIdMapping(IAveFolder tempFolder, Guid sourceBaseId)
        {
            foreach (IAveFile file in tempFolder.Files)
            {
                if (file.Name.EndsWith(".xoml.wfconfig.xml", StringComparison.OrdinalIgnoreCase))
                {
                    string strContent = Encoding.UTF8.GetString(file.OpenBinary(AveOpenBinaryOptions.SkipVirusScan));
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(strContent);
                    var xmlNodeList = doc.GetElementsByTagName("Template");
                    if (xmlNodeList != null && xmlNodeList.Count != 0)
                    {

                        try
                        {
                            string baseId = (xmlNodeList[0] as XmlElement).Attributes["BaseID"].InnerText.Trim('{', '}');
                            SPWorkflowProcessorRuntime.MappingManager.SiteMappingManager.AddWorkflowBaseIdMapping(sourceBaseId, new Guid(baseId));
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.WARN, "An error occurred while generate worklfow base id mapping, source baseId: {0}, parent folder url: {1}, error: {2}", sourceBaseId, tempFolder.ServerRelativeUrl, e);
                        }
                    }
                    break;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="templateLib"></param>
        /// <param name="parentFolderUrl"></param>
        /// <param name="isListAlreadyContainWf"></param>
        /// <param name="assoUnit"></param>
        /// <param name="hasSameWorkflowNameinWeb">CI-40825 针对NintexWorkflow的特殊返回值</param>
        /// <returns></returns>
        private static IAveFolder GetOrCreateParentFolder(IAveList templateLib, string parentFolderUrl, bool isListAlreadyContainWf, SPWFAssociationUnit assoUnit, ref bool hasSameWorkflowNameinWeb)
        {
            IAveFolder parentFolder = templateLib.RootFolder;
            string parentID = GetListOrWebID(assoUnit);
            string wfOriginalParentId = string.IsNullOrEmpty(assoUnit.SerializableData.mOriginalParentId) ? string.Empty : assoUnit.SerializableData.mOriginalParentId.Trim('{', '}');
            string[] folderPath = parentFolderUrl.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            Guid associationListId = Guid.Empty;
            foreach (string name in folderPath)
            {
                string tempName = null;
                IAveFolder temp = null;
                try
                {
                    if (SPWorkflowProcessorRuntime.IsAllowDuplicateSPDAndNintexInSameWeb)
                    {
                        int flag = 0;
                        while (true)
                        {
                            tempName = name;
                            if (flag == 1)
                            {
                                hasSameWorkflowNameinWeb = true;
                                tempName = tempName + "_" + parentID;
                                temp = parentFolder.SubFolders[tempName];
                                GenerateWorkflowBaesIdMapping(temp, assoUnit.SerializableData.mBaseId);
                                break;
                            }
                            temp = parentFolder.SubFolders[tempName];
                            if (isListAlreadyContainWf && !parentFolder.Name.Equals("NintexWorkflows", StringComparison.OrdinalIgnoreCase))
                            {
                                break;
                            }
                            if (temp.Files.Count == 0)// 07 的nwf 结构： workflows\NintexWorkflows\[wf name]
                            {
                                break;
                            }
                            if (!CheckAssoiciatedListExists(temp, ref associationListId))
                            {
                                //说明存在脏数据，即list level的SPD/Nintex workflow对应的folder仍然存在，但是list本身已经不在了，需要把脏数据folder删除。
                                temp.Delete();
                                flag = 0;
                                continue;
                            }
                            if (assoUnit.ParentList != null && associationListId != Guid.Empty && associationListId == assoUnit.ParentList.ID)
                            {
                                break;
                            }
                            flag++;
                        }
                    }
                    else
                    {
                        tempName = name;
                        temp = parentFolder.SubFolders[tempName];
                    }
                    parentFolder = temp;
                }
                catch (AveWrapperSkipException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.GetFolderFromParentError, ex.ToString());
                    try
                    {
                        IAveFolder newFolder = parentFolder.SubFolders.Add(tempName);
                        parentFolder = newFolder;
                        if (!string.IsNullOrEmpty(wfOriginalParentId))
                        {
                            lock (OriginalIdCacheForWorkflowVersions)
                            {
                                if (!OriginalIdCacheForWorkflowVersions.ContainsKey(wfOriginalParentId))
                                {
                                    //说明第一次还原这个workflow，可能有version也可能没有version，添加到version的缓存中。
                                    OriginalIdCacheForWorkflowVersions[wfOriginalParentId] = new Dictionary<string, string>();
                                    OriginalIdCacheForWorkflowVersions[wfOriginalParentId][name] = tempName;
                                }
                                else
                                {
                                    OriginalIdCacheForWorkflowVersions[wfOriginalParentId][name] = tempName;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Workflow_NotFindParentFolder, e.Message);
                    }
                }
            }

            return parentFolder;
        }

        private static string GetListOrWebID(SPWFAssociationUnit assoUnit)
        {
            string id = string.Empty;
            try
            {
                switch (assoUnit.ParentObjectType)
                {
                    case SPWFAssociationParentType.List:
                    case SPWFAssociationParentType.ListContentType:
                        id = assoUnit.ParentList.ID.ToString("N");
                        break;
                    case SPWFAssociationParentType.Web:
                    case SPWFAssociationParentType.WebContentType:
                        id = assoUnit.ParentWeb.ID.ToString("N");
                        break;
                    case SPWFAssociationParentType.Invalid:
                        ;
                        break;
                }
            }
            catch (Exception e)
            {
                logger.Debug("Get parent id error, error message: {0}", e);
            }
            return id;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "wfconfig is the name of the file")]
        private static bool CheckAssoiciatedListExists(IAveFolder tempFolder, ref Guid listId)
        {
            bool isAssociatedListExisted = false;
            foreach (IAveFile file in tempFolder.Files)
            {
                if (file.Name.EndsWith(".xoml.wfconfig.xml", StringComparison.OrdinalIgnoreCase))
                {
                    string strContent = Encoding.UTF8.GetString(file.OpenBinary(AveOpenBinaryOptions.SkipVirusScan));
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(strContent);
                    var xmlNodeList = doc.GetElementsByTagName("Association");
                    if (xmlNodeList != null && xmlNodeList.Count != 0)
                    {
                        string assoListId = (xmlNodeList[0] as XmlElement).Attributes["ListID"].InnerText.Trim('{', '}');
                        IAveList tempList = null;
                        try
                        {
                            listId = new Guid(assoListId);
                            tempList = tempFolder.ParentWeb.GetList(listId);
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.INFO, "Cannot find association list id in current web: {0}, and need to delete this SPD/Nintex workflow folder, workflow config file name: {1}, error message: {2}", tempFolder.ParentWeb.Url, file.Name, e);
                        }
                        if (tempList != null)
                        {
                            isAssociatedListExisted = true;
                        }
                    }
                    break;
                }
            }
            return isAssociatedListExisted;
        }
        #endregion
    }
}
