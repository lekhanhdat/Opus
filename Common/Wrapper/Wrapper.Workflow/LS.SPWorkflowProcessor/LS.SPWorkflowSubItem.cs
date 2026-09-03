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
using LS.SPWorkflowProcessor.SerializableObjects;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Resource;
using System.Diagnostics.CodeAnalysis;

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
                for (i = startIndex; i < fieldCount; i++)
                {
                    if (sdr.IsDBNull(i))
                        continue;
                    string backupName = rowOrdinalStr + "_" + sdr.GetName(i).ToLower();
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
                throw new SPWFProcessorException(SPWFProcessorErrorCode.SetPropsFromDataReaderError, e);
            }
            finally
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "SetPropsFromDataReader");
            }
        }
        public void SetPropsFromDataReader(SqlDataReader sdr, int startIndex, Dictionary<string, string> fieldMap, int curRowOrdinal)
        {
            try
            {
                int fieldCount = sdr.FieldCount;
                int i;
                string rowOrdinalStr = curRowOrdinal.ToString();
                for (i = startIndex; i < fieldCount; i++)
                {
                    if (sdr.IsDBNull(i))
                        continue;
                    string backupName = rowOrdinalStr + "_" + sdr.GetName(i).ToLower();
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
                throw new SPWFProcessorException(SPWFProcessorErrorCode.SetPropsFromDataReaderError, e);
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
                throw new SPWFProcessorException(SPWFProcessorErrorCode.SetPropsFromDataReaderError, e);
            }
            finally
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "SetPropsFromDataRow");
            }
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

        #region ************************Backup  Region************************
        public static SPWorkflowSubFileUnit GenerateSPFileUnit(IAveFile file)
        {
            return GenerateSPFileUnit(file, file.UIVersionLabel);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = ".xoml.wfconfig.xml:Config file name.")]
        public static SPWorkflowSubFileUnit GenerateSPFileUnit(IAveFile file, string fileUIVersionLabel)
        {
            SPWorkflowSubFileUnit fileUnit = null;
            try
            {
                fileUnit = new SPWorkflowSubFileUnit();
                fileUnit.SerializableData.mCharSetName = file.CharSetName;
                if (string.IsNullOrEmpty(fileUnit.SerializableData.mCharSetName))
                    fileUnit.SerializableData.mCharSetName = "utf-8";
                if (file.Properties != null && file.Properties.ContainsKey("ipfs_streamhash"))
                {
                    fileUnit.SerializableData.ipfs_streamhash = file.Properties["ipfs_streamhash"].ToString();
                }
                if (file.Properties != null && file.Properties.ContainsKey("vti_privatelistexempt"))
                {
                    fileUnit.SerializableData.vti_privatelistexempt = Convert.ToBoolean(file.Properties["vti_privatelistexempt"]);
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
                    //fileUnit.SerializableData.mUIVersion=version.u;
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
                    fileUnit.SerializableData.mCategorySchemalXml = file.Item.Fields["Category"].SchemaXml;
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.GetItemAttributeError, e.ToString());
                }

                try
                {
                    if (file.Name.ToLower().EndsWith(".xoml.wfconfig.xml", StringComparison.OrdinalIgnoreCase))
                    {
                        string templateListIdStr = ConfigFileProc.GetTemplateLibIdStr(fileUnit.SerializableData.mContent);
                        Guid templateListId = new Guid(templateListIdStr);
                        fileUnit.SerializableData.mTemplateLibTitle = file.ParentFolder.ParentWeb.Lists[templateListId].Title;
                    }
                    else
                    {
                        try
                        {
                            Regex guidRE = new Regex(SPWorkflowCommon.GUIDREG, RegexOptions.IgnoreCase);
                            MatchCollection guids = guidRE.Matches(Encoding.GetEncoding(fileUnit.mSerializableData.mCharSetName).GetString(fileUnit.mSerializableData.mContent));
                            foreach (Match m in guids)
                            {
                                if (!fileUnit.SerializableData.mGUIDDictionary.ContainsKey(m.Value.ToUpper()))
                                {

                                    object listObj = null;

                                    listObj = file.ParentFolder.ParentWeb.Lists.GetListById(new Guid(m.Value), false); ;
                                    if (listObj != null)
                                    {
                                        string title = ((IAveList)listObj).Title;
                                        fileUnit.SerializableData.mGUIDDictionary.AddEx(m.Value.ToUpper(), title);
                                        SPWorkflowProcessorRuntime.Log(Logs.Markup_FoundListByTitle, title, m.Value);
                                    }
                                    else
                                    {
                                        fileUnit.SerializableData.mGUIDDictionary.AddEx(m.Value.ToUpper(), null);
                                        SPWorkflowProcessorRuntime.Log(Logs.Markup_CannotHandleGUID, m.Value);
                                    }

                                }
                            }
                        }
                        catch (Exception e)
                        {
                            SPWorkflowProcessorRuntime.Log(Logs.Markup_CannotHandleGUIDUnknown, e.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.GenerateSPFileUnitError, ex);
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.GenerateSPFileUnitError, ex);
            }

            return fileUnit;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = ".xoml.wfconfig.xml:Config file name.")]
        public static List<SPWorkflowSubFileUnit> GenerateSPFileUnitCollection(IAveFolder parentFolder, int cfgFileVersion)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "GenerateSPFileUnitCollection");

            List<SPWorkflowSubFileUnit> fileUnitCollection = new List<SPWorkflowSubFileUnit>();

            string cfgVersionLabel = string.Empty;
            string xomlVersionLabel = string.Empty;
            string rulesVersionLabel = string.Empty;

            #region Get Files' UI Version
            foreach (IAveFile file in parentFolder.Files)
            {
                if (file.Name.ToLower().EndsWith(".xoml.wfconfig.xml"))
                {
                    if (cfgFileVersion == -1)
                        cfgFileVersion = file.UIVersion;

                    string charSetName = file.CharSetName;
                    if (string.IsNullOrEmpty(charSetName))
                        charSetName = "utf-8";

                    string strContent = string.Empty; ;
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
                    }
                    catch (Exception ex)
                    {
                        SPWorkflowProcessorRuntime.Log(Logs.Common_XmlFileHandleException, ex.Message);
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
                if (file.Name.ToLower().EndsWith(".xoml.wfconfig.xml"))
                    fileVersionLabel = cfgVersionLabel;
                else if (file.Name.ToLower().EndsWith(".xoml") && !string.IsNullOrEmpty(xomlVersionLabel))
                    fileVersionLabel = xomlVersionLabel;
                else if (file.Name.ToLower().EndsWith(".rules") && !string.IsNullOrEmpty(rulesVersionLabel))
                    fileVersionLabel = rulesVersionLabel;
                if (float.Parse(fileVersionLabel) > float.Parse(file.UIVersionLabel))
                    fileVersionLabel = file.UIVersionLabel;
                SPWorkflowSubFileUnit fileUnit = GenerateSPFileUnit(file, fileVersionLabel);
                if (fileUnit != null)
                    fileUnitCollection.Add(fileUnit);
            }
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "GenerateSPFileUnitCollection");
            return fileUnitCollection;
        }

        public static List<SPWorkflowSubFileUnit> GenerateWFSvcFileUnitCollection(IAveFolder parentFolder)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "GenerateSPFileUnitCollection");

            List<SPWorkflowSubFileUnit> fileUnitCollection = new List<SPWorkflowSubFileUnit>();

            string cfgVersionLabel = string.Empty;
            string xomlVersionLabel = string.Empty;
            string rulesVersionLabel = string.Empty;
            string previewName = string.Empty;

            foreach (IAveFile file in parentFolder.Files)
            {
                if (file.Name.ToLower().Equals("workflow.xaml", StringComparison.Ordinal))
                {
                    if (file.Properties != null && !file.Properties["WSPublishState"].ToString().Equals("1")
                        || file.Item != null && !file.Item["WSPublishState"].ToString().Equals("1"))
                    {
                        SPWorkflowSubFileUnit fileUnit = GenerateSPFileUnit(file);
                        if (fileUnit != null)
                        {
                            fileUnitCollection.Add(fileUnit);
                        }
                    }
                    if (file.Item != null)
                    {
                        foreach (IAveListItemVersion version in file.Item.Versions)
                        {
                            if (!version["WSPublishState"].ToString().Equals("1"))
                            {
                                SPWorkflowSubFileUnit fileUnit = GenerateSPFileUnit(file, version.VersionLabel);
                                if (fileUnit != null)
                                {
                                    fileUnitCollection.Add(fileUnit);
                                }
                            }
                        }
                    }
                    //foreach (IAveFileVersion fileVersion in file.Versions)
                    //{
                    //    if (fileVersion.Properties != null && !fileVersion.Properties["WSPublishState"].ToString().Equals("1"))
                    //    {
                    //        SPWorkflowSubFileUnit fileUnit = GenerateSPFileUnit(file, fileVersion.VersionLabel);
                    //        if (fileUnit != null)
                    //        {
                    //            fileUnitCollection.Add(fileUnit);
                    //        }
                    //    }
                    //}
                }
            }

            if (fileUnitCollection.Count == 0)
            {
                logger.Warn("The folder:{0} doesn't have publish version workflow.", parentFolder.ServerRelativeUrl);
            }

            SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "GenerateSPFileUnitCollection");
            return fileUnitCollection;
        }
        #endregion

        #region ************************Restore Region************************
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = ".xoml.wfconfig.xml:Config file name.")]
        public static bool HandleTemplateSPFileUnits(SPWFAssociationUnit assoUnit, SPWorkflowSubListUnit containerListUnit, bool createAssociation, out IAveWorkflowAssociation spAssociation)
        {
            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "HandleTemplateSPFileUnits");
            spAssociation = null;
            try
            {
                IAveWeb web = assoUnit.ParentWeb;
                IAveList taskList = (assoUnit.mTaskListUnit != null) ? web.Lists.GetListByName(assoUnit.mTaskListUnit.SerializableData.mTitle, false) : null;
                //由于task在前面有可能是用mLeafName取的，所以在此处加上用mLeafname取task list的代码.
                if (taskList == null && assoUnit.mTaskListUnit != null)
                {
                    taskList = web.Lists.GetListByName(assoUnit.mTaskListUnit.SerializableData.mLeafName, false);
                }
                IAveList histList = (assoUnit.mHistListUnit != null) ? web.Lists[assoUnit.mHistListUnit.SerializableData.mTitle] : null;
                IAveList tempLib = web.Lists[containerListUnit.SerializableData.mTitle];
                Dictionary<string, object> replaceDic = new Dictionary<string, object>();
                if (assoUnit.mTemplateLibUnit != null)
                { replaceDic.Add(assoUnit.mTemplateLibUnit.SerializableData.mId.ToString().ToUpper(), tempLib?.ID.ToString().ToUpper()); }
                ArgumentCheck.CheckNotNull(taskList);
                if (assoUnit.mTaskListUnit != null)
                { replaceDic.Add(assoUnit.mTaskListUnit.SerializableData.mId.ToString().ToUpper(), taskList?.ID.ToString().ToUpper()); }
                ArgumentCheck.CheckNotNull(histList);
                if (assoUnit.mHistListUnit != null)
                { replaceDic.Add(assoUnit.mHistListUnit.SerializableData.mId.ToString().ToUpper(), histList?.ID.ToString().ToUpper()); }
                replaceDic.Add(assoUnit.OriginalParentId.ToUpper(), assoUnit.ParentId.ToUpper());
                //在处理CT association时，原端备份的OriginalParentId就是CT.id.tostring()，而不像list,web那样是id.tostring("B")，所以CT association的OriginalParentId是不存在{}的.
                if (!replaceDic.ContainsKey(assoUnit.OriginalParentId.ToUpper().Trim(new char[] { '{', '}' })))
                { replaceDic.Add(assoUnit.OriginalParentId.ToUpper().Trim(new char[] { '{', '}' }), assoUnit.ParentId.ToUpper().Trim(new char[] { '{', '}' })); }
                Dictionary<string, object> replaceConfigDic = new Dictionary<string, object>();
                replaceConfigDic.Add("ParentId", assoUnit.ParentId.ToUpper());
                if (taskList != null)
                { replaceConfigDic.Add("TaskListId", taskList.ID.ToString("B").ToUpper()); }
                if (histList != null)
                { replaceConfigDic.Add("HistListId", histList.ID.ToString("B").ToUpper()); }
                replaceConfigDic.Add("BaseID", assoUnit.SerializableData.mBaseId.ToString("B").ToUpper());
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

                IAveFolder parentFolder = null;
                IAveFile configSPFile = null;

                string configFileContent = string.Empty;
                string xomlFileContent = string.Empty;
                string rulesFileContent = string.Empty;
                string xomlFileVersion = string.Empty;
                string rulesFileVersion = string.Empty;
                string configFileRelativeUrl = string.Empty;
                string ruleFileUrl = string.Empty;
                string xomlFileUrl = string.Empty;
                foreach (SPWorkflowSubFileUnit fileUnit in containerListUnit.mTemplateFileUnits)
                {
                    #region Get Parent Folder
                    if (parentFolder == null)
                    {
                        int fileNameIndex = fileUnit.SerializableData.mFirstParentFolderRelativeUrl.LastIndexOf("/", StringComparison.Ordinal);
                        if (fileNameIndex == 0)
                            parentFolder = tempLib.RootFolder;
                        else
                            parentFolder = GetOrCreateParentFolder(tempLib, fileUnit.SerializableData.mFirstParentFolderRelativeUrl.Substring(1, fileNameIndex - 1));
                    }
                    #endregion

                    #region Replace Content
                    string curFileContent = string.Empty;
                    IAveFile curFile = null;
                    try
                    {
                        curFile = parentFolder.Files[fileUnit.SerializableData.mName];
                        string version = curFile.UIVersionLabel;
                        //for O365 cache issue, need to get refresh file here
                        curFile = parentFolder.ParentWeb.GetFile(curFile.ServerRelativeUrl);
                        //curFile.Delete();
                        //curFile = null;
                        logger.Info("Get template file {0}: folder file version:{1}, web file version:{2},sourceversion:{3}", curFile.ServerRelativeUrl, version, curFile.UIVersionLabel, fileUnit.SerializableData.mUIVersion);
                        if (SPWorkflowProcessorRuntime.TemplateFileConflictRules == TemplateFileConflictRulesEnum.KeepSource)
                        {
                            try
                            {
                                curFile.CheckOut(false, string.Empty);
                            }
                            catch (Exception e)
                            {
                                logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.CheckOutFileError, e.ToString());
                            }

                            curFile.SaveBinary(fileUnit.SerializableData.mContent);
                            curFile.Update();
                            try
                            {
                                curFile.CheckIn(string.Empty);
                            }
                            catch (Exception e)
                            {
                                SPWorkflowProcessorRuntime.Log(Logs.Common_SPFileCheckInException, e.Message);
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.GetFileByNameError, e.ToString());
                        if (curFile == null)
                        {
                            curFile = parentFolder.Files.Add(fileUnit.SerializableData.mName, fileUnit.SerializableData.mContent);
                        }
                    }

                    if (curFile == null)
                        return false;

                    fileUnit.mSPFile = curFile;

                    if (curFile.Name.ToLower().EndsWith(".xoml.wfconfig.xml", StringComparison.Ordinal))
                    {
                        if (string.IsNullOrEmpty(configFileRelativeUrl) || curFile.Name.Equals(parentFolder.Name + ".xoml.wfconfig.xml", StringComparison.OrdinalIgnoreCase))
                        {
                            configSPFile = curFile;
                            configFileRelativeUrl = fileUnit.SerializableData.mListRelativeUrl.Substring(1);
                        }

                        if (string.IsNullOrEmpty(fileUnit.SerializableData.mTemplateLibTitle))
                        {
                            replaceConfigDic.Add("TemplateListId", tempLib.ID.ToString("B").ToUpper());
                        }
                        else
                        {
                            try
                            {
                                replaceConfigDic.Add("TemplateListId", web.Lists[fileUnit.SerializableData.mTemplateLibTitle].ID.ToString("B").ToUpper());
                            }
                            catch (Exception e)
                            {
                                logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.AddListToDictionaryError, e.ToString());
                            }

                        }
                    }
                    else if (curFile.Name.ToLower().EndsWith(".xsn", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!string.IsNullOrEmpty(fileUnit.SerializableData.ipfs_streamhash) && curFile.Properties != null)
                        {
                            curFile.Properties["ipfs_streamhash"] = fileUnit.SerializableData.ipfs_streamhash;
                            if (fileUnit.SerializableData.vti_privatelistexempt != null)
                            {
                                curFile.Properties["vti_privatelistexempt"] = fileUnit.SerializableData.vti_privatelistexempt.Value;
                            }
                            curFile.Update();
                        }
                        else if (SPWorkflowProcessorRuntime.ObjectModelFactory.ContextKind == AveContextKind.ClientObjectModel)
                        {
                            try
                            {
                                IAveFormsServicesWebService svc = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateFormsServicesWebService(assoUnit.ParentWeb);
                                if (svc != null)
                                {
                                    svc.BrowserEnableUserFormTemplate(assoUnit.ParentWeb.Url.TrimEnd(new char[] { '/' }) + "/" + curFile.Url.TrimStart(new char[] { '/' }));
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Trace.WriteLine(ex.ToString());
                            }
                        }
                    }
                    else
                    {
                        SPWorkflowFileContentProc fileContentProc = SPWorkflowFileContentProc.CreateInstance(assoUnit.SerializableData.mBaseId, curFile);
                        if (fileContentProc == null)
                            continue;

                        FixupDictionary(web.Lists, replaceDic, fileUnit.mSerializableData.mGUIDDictionary);
                        curFileContent = fileContentProc.ReplaceContent(replaceDic);

                        if (curFile.Name.ToLower().EndsWith("xoml", StringComparison.OrdinalIgnoreCase))
                        {
                            xomlFileContent = curFileContent;
                            xomlFileUrl = curFile.ServerRelativeUrl;
                            replaceConfigDic.Add("XomlFileVersion", "V" + curFile.UIVersionLabel);
                        }
                        if (curFile.Name.ToLower().EndsWith("rules", StringComparison.OrdinalIgnoreCase))
                        {
                            rulesFileContent = curFileContent;
                            ruleFileUrl = curFile.ServerRelativeUrl;
                            replaceConfigDic.Add("RulesFileVersion", "V" + curFile.UIVersionLabel);
                        }
                    }
                    #endregion
                }

                #region refresh template file version
                RefershTemplateFileVersionInConfig(assoUnit, xomlFileUrl, ruleFileUrl, replaceConfigDic);
                #endregion

                #region Replace Config File Content
                SPWorkflowFileContentProc configFileContentProc = SPWorkflowFileContentProc.CreateInstance(assoUnit.SerializableData.mBaseId, configSPFile);
                configFileContent = configFileContentProc.ReplaceContent(replaceConfigDic);
                #endregion

                if (createAssociation)
                {
                    //List<Guid> oldIdList = new List<Guid>();
                    Dictionary<Guid, string> oldIdAssoNameMap = new Dictionary<Guid, string>();
                    foreach (IAveWorkflowAssociation wa in assoUnit.SPAssoicationCollection)
                    {
                        oldIdAssoNameMap.Add(wa.ID, wa.Name);
                    }

                    IAveWebPartPagesWebService objWebPartPages = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateWebPartPagesWebService(web);

                    try
                    {
                        //SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "ValidateWorkflowMarkupAndCreateSupportObjects");
                        //string strResult1 = objWebPartPages.ValidateWorkflowMarkupAndCreateSupportObjects(xomlFileContent, rulesFileContent, configFileContent, "2");
                        //SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "ValidateWorkflowMarkupAndCreateSupportObjects");
                        //CheckWebServiceSuccess("ValidatingWorkflow", strResult1);
                        try
                        {
                            tempLib.Update();
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.UpdateListError, e.ToString());
                        }
                        SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "AssociateWorkflowMarkup");
                        string strResult2 = objWebPartPages.AssociateWorkflowMarkup(configFileRelativeUrl, string.Empty);
                        SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "AssociateWorkflowMarkup");
                        CheckWebServiceSuccess("AssociatingWorkflow", strResult2);
                    }
                    catch (SPWFProcessorException procException)
                    {
                        //SPWorkflowProcessorRuntime.Log(Logs.Markup_APIResultException, procException.Message);
                        logger.Log(AveLogLevel.ERROR, "Method HandleTemplateSPFileUnits failed,due to:{0}", procException);
                        throw;
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.AddWorkFlowToPageError, e.ToString());
                        try
                        {
                            try
                            {
                                tempLib.Update();
                            }
                            catch (Exception ex)
                            {
                                logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.UpdateListError, ex.ToString());
                            }
                            SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "AssociateWorkflowMarkup");
                            string strResult2 = objWebPartPages.AssociateWorkflowMarkup(configFileRelativeUrl, string.Empty);
                            SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "AssociateWorkflowMarkup");
                            CheckWebServiceSuccess("AssociatingWorkflow", strResult2);
                        }
                        catch (Exception exception)
                        {
                            SPWorkflowProcessorRuntime.Log(Logs.Markup_APIResultException, exception.Message);
                            throw new SPWFProcessorException(SPWFProcessorErrorCode.SoapServerException, exception);
                        }
                    }

                    bool getSameNameFirst = false;
                    foreach (IAveWorkflowAssociation wa in assoUnit.SPAssoicationCollection)
                    {
                        if (wa.Name.Equals(assoUnit.SerializableData.mName))
                        {
                            spAssociation = wa;
                            getSameNameFirst = true;
                            string statusFieldInternalName = LSInvoker.GetProperty(spAssociation, "InternalNameStatusField") as string;
                            if (string.IsNullOrEmpty(statusFieldInternalName))
                                break;

                            if (string.IsNullOrEmpty(assoUnit.SerializableData.mStatusFieldName))
                                break;
                            try
                            {
                                if (SPWorkflowCommon.StatusFieldMapping.ContainsKey(assoUnit.SerializableData.mStatusFieldName.ToLower()))
                                {
                                    if (!statusFieldInternalName.Equals(SPWorkflowCommon.StatusFieldMapping[assoUnit.SerializableData.mStatusFieldName.ToLower()], StringComparison.OrdinalIgnoreCase))
                                    {
                                        object statusFieldObj = spAssociation.ParentList.Fields.GetFieldByInternalName(statusFieldInternalName, false);
                                        if (statusFieldObj != null)
                                        {
                                            IAveField statusField = statusFieldObj as IAveField;
                                            statusField.ReadOnlyField = false;
                                            statusField.Update();
                                            statusField.Delete();
                                        }
                                        LSInvoker.SetProperty(spAssociation, "InternalNameStatusField", SPWorkflowCommon.StatusFieldMapping[assoUnit.SerializableData.mStatusFieldName.ToLower()]);
                                        assoUnit.UpdateWorkflowAssociation(spAssociation);
                                    }
                                }
                                else
                                {
                                    SPWorkflowCommon.StatusFieldMapping.Add(assoUnit.SerializableData.mStatusFieldName.ToLower(), statusFieldInternalName);
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.StatusFieldSetError, statusFieldInternalName, e);
                            }
                            break;
                        }
                    }
                    if (!getSameNameFirst)
                    {
                        foreach (IAveWorkflowAssociation wa in assoUnit.SPAssoicationCollection)
                        {
                            if (!oldIdAssoNameMap.ContainsKey(wa.ID))
                            {
                                spAssociation = wa;
                                string statusFieldInternalName = LSInvoker.GetProperty(spAssociation, "InternalNameStatusField") as string;
                                if (string.IsNullOrEmpty(statusFieldInternalName))
                                    break;

                                if (string.IsNullOrEmpty(assoUnit.SerializableData.mStatusFieldName))
                                    break;
                                try
                                {
                                    if (SPWorkflowCommon.StatusFieldMapping.ContainsKey(assoUnit.SerializableData.mStatusFieldName.ToLower()))
                                    {
                                        if (!statusFieldInternalName.Equals(SPWorkflowCommon.StatusFieldMapping[assoUnit.SerializableData.mStatusFieldName.ToLower()], StringComparison.OrdinalIgnoreCase))
                                        {
                                            object statusFieldObj = spAssociation.ParentList.Fields.GetFieldByInternalName(statusFieldInternalName, false);
                                            if (statusFieldObj != null)
                                            {
                                                IAveField statusField = statusFieldObj as IAveField;
                                                statusField.ReadOnlyField = false;
                                                statusField.Update();
                                                statusField.Delete();
                                            }
                                            LSInvoker.SetProperty(spAssociation, "InternalNameStatusField", SPWorkflowCommon.StatusFieldMapping[assoUnit.SerializableData.mStatusFieldName.ToLower()]);
                                            assoUnit.UpdateWorkflowAssociation(spAssociation);
                                        }
                                    }
                                    else
                                    {
                                        SPWorkflowCommon.StatusFieldMapping.Add(assoUnit.SerializableData.mStatusFieldName.ToLower(), statusFieldInternalName);
                                    }
                                }
                                catch (Exception e)
                                {
                                    logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.StatusFieldSetError, statusFieldInternalName, e);
                                }
                                break;
                            }
                        }
                    }
                    foreach (IAveWorkflowAssociation asso in assoUnit.SPAssoicationCollection)
                    {
                        if (oldIdAssoNameMap.ContainsKey(asso.ID) && oldIdAssoNameMap[asso.ID] != asso.Name)
                        {
                            if (SPWorkflowProcessorRuntime.ObjectModelFactory.ContextKind != AveContextKind.ClientObjectModel)
                            {
                                SPWFAssociationProcNative.UpdateAssociationName(asso, oldIdAssoNameMap[asso.ID], assoUnit.IsRenamed);
                                assoUnit.UpdateWorkflowAssociation(asso);
                            }
                            else
                            {
                                if (!assoUnit.IsRenamed)
                                {
                                    asso.Name = oldIdAssoNameMap[asso.ID];
                                    assoUnit.UpdateWorkflowAssociation(asso);
                                }
                            }
                        }
                    }
                }
                return true;
            }
            catch (SPWFProcessorException procException)
            {
                //SPWorkflowProcessorRuntime.Log(Logs.Markup_ProcessTemplateFilesException, procException.Message);
                logger.Log(AveLogLevel.ERROR, "SPWFProcessorException has been throw, due to :{0}", procException);
                throw;
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.Markup_ProcessTemplateFilesException, e.Message);
                throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationDefinitionRestoreError, e);
            }
            finally
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "HandleTemplateSPFileUnits");
            }
        }

        private static void RefershTemplateFileVersionInConfig(SPWFAssociationUnit associationUnit,string xomlUrl,string ruleUrl,Dictionary<string,object> replaceConfigDic)
        {
            try
            {
                if (!string.IsNullOrEmpty(ruleUrl))
                {
                    var tempRuleFile = associationUnit.ParentWeb.GetFile(ruleUrl);
                    string newRuleVersionlabel = "V" + tempRuleFile.UIVersionLabel;
                    logger.Info("Update rule file version to {0}", newRuleVersionlabel);
                    replaceConfigDic["RulesFileVersion"]= newRuleVersionlabel;
                }
                if (!string.IsNullOrEmpty(xomlUrl))
                {
                    var tempXomlFile = associationUnit.ParentWeb.GetFile(xomlUrl);
                    string newXomlVersionLabel = "V" + tempXomlFile.UIVersionLabel;
                    logger.Info("Update xoml file version to {0}", newXomlVersionLabel);
                    replaceConfigDic["XomlFileVersion"] = newXomlVersionLabel;
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while update replace config dictionary.{0}", e);
            }
        }

       /* private static void LogAfterReplaceConfigFile(SPWFAssociationUnit assoUnit, string configFileContent, string ruleFileUrl, string xomlFileUrl)
        {
            try
            {
                StringBuilder newFile = new StringBuilder();
                if (!string.IsNullOrEmpty(ruleFileUrl))
                {
                    var tempRuleFile = assoUnit.ParentWeb.GetFile(ruleFileUrl);
                    newFile.AppendLine(string.Format("[FileUrl:{0}][UIVersionLabel:{1}][CheckoutStatus:{2}]",
                        tempRuleFile.ServerRelativeUrl, tempRuleFile.UIVersionLabel, tempRuleFile.CheckOutStatus));
                }
                if (!string.IsNullOrEmpty(xomlFileUrl))
                {
                    var tempXomlFile = assoUnit.ParentWeb.GetFile(xomlFileUrl);
                    newFile.AppendLine(string.Format("[FileUrl:{0}][UIVersionLabel:{1}][CheckoutStatus:{2}]",
                        tempXomlFile.ServerRelativeUrl, tempXomlFile.UIVersionLabel, tempXomlFile.CheckOutStatus));
                }
                newFile.AppendLine(string.Format("ConfigFileContentAfterUpdate:{0}", configFileContent));
                logger.Info("After replace information in config file.{0}",
                  newFile.ToString());
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while log messsge after update config file.{0}",e);
            }
        }*/

        public static bool HandleTemplateSPFileUnits(SPWFAssociationUnit assoUnit, SPWorkflowSubListUnit containerListUnit, out string xamlFileContent)
        {
            //SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "HandleTemplateSPFileUnitsFor13Model");
            logger.Log(AveLogLevel.INFO, "HandleTemplateSPFileUnitsFor13Model");
            try
            {
                IAveWeb web = assoUnit.ParentWeb;
                //some workflow list needs reload
                web.ReloadWeb();
                IAveList taskList = (assoUnit.mTaskListUnit != null) ? web.Lists.GetListByName(assoUnit.mTaskListUnit.SerializableData.mTitle, false) : null;
                //由于task在前面有可能是用mLeafName取的，所以在此处加上用mLeafname取task list的代码.
                if (taskList == null && assoUnit.mTaskListUnit != null)
                {
                    taskList = web.Lists.GetListByName(assoUnit.mTaskListUnit.SerializableData.mLeafName, false);
                }
                IAveList histList = (assoUnit.mHistListUnit != null) ? web.Lists[assoUnit.mHistListUnit.SerializableData.mTitle] : null;
                IAveList tempLib = web.Lists[containerListUnit.SerializableData.mTitle];
                Dictionary<string, object> replaceDic = new Dictionary<string, object>();
                if (assoUnit.mTemplateLibUnit != null)
                { replaceDic.Add(assoUnit.mTemplateLibUnit.SerializableData.mId.ToString().ToUpper(), tempLib?.ID.ToString().ToUpper()); }
                if (assoUnit.mTaskListUnit != null)
                { replaceDic.Add(assoUnit.mTaskListUnit.SerializableData.mId.ToString().ToUpper(), taskList?.ID.ToString().ToUpper()); }
                ArgumentCheck.CheckNotNull(histList);
                if (assoUnit.mHistListUnit != null)
                { replaceDic.Add(assoUnit.mHistListUnit.SerializableData.mId.ToString().ToUpper(), histList?.ID.ToString().ToUpper()); }
                if (assoUnit.OriginalParentId != null)
                {
                    replaceDic.Add(assoUnit.OriginalParentId.ToUpper(), assoUnit.ParentId.ToUpper());
                    //在处理CT association时，原端备份的OriginalParentId就是CT.id.tostring()，而不像list,web那样是id.tostring("B")，所以CT association的OriginalParentId是不存在{}的.
                    if (!replaceDic.ContainsKey(assoUnit.OriginalParentId.ToUpper().Trim(new char[] { '{', '}' })))
                    { replaceDic.Add(assoUnit.OriginalParentId.ToUpper().Trim(new char[] { '{', '}' }), assoUnit.ParentId.ToUpper().Trim(new char[] { '{', '}' })); }
                }
                Dictionary<string, object> replaceConfigDic = new Dictionary<string, object>();
                replaceConfigDic.Add("ParentId", assoUnit.ParentId.ToUpper());
                if (taskList != null)
                { replaceConfigDic.Add("TaskListId", taskList.ID.ToString("B").ToUpper()); }
                if (histList != null)
                { replaceConfigDic.Add("HistListId", histList.ID.ToString("B").ToUpper()); }
                replaceConfigDic.Add("BaseID", assoUnit.SerializableData.mBaseId.ToString("B").ToUpper());
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
                SPWorkflowSubFileUnit fileUnit = containerListUnit.mTemplateFileUnits[0];

                SPWorkflowFileContentProc fileContentProc = SPWorkflowFileContentProc.CreateInstance(assoUnit.SerializableData.mBaseId, null, fileUnit.mSerializableData.mContent);
                if (fileContentProc == null)
                {
                    xamlFileContent = Encoding.UTF8.GetString(fileUnit.mSerializableData.mContent);
                    return false;
                }

                FixupDictionary(web.Lists, replaceDic, fileUnit.mSerializableData.mGUIDDictionary);
                xamlFileContent = fileContentProc.ReplaceContent(replaceDic);
                return true;
            }
            catch (SPWFProcessorException procException)
            {
                //SPWorkflowProcessorRuntime.Log(Logs.Markup_ProcessTemplateFilesException, procException.Message);
                logger.Log(AveLogLevel.ERROR, "Markup_ProcessTemplateFilesException,{0}", procException);
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

        internal static void FixupDictionary(IAveListCollection lists, Dictionary<string, object> repDic, Dictionary<string, string> dic)
        {
            if (dic == null)
                return;
            Dictionary<string, string> temp = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> pair in dic)
            {
                if (repDic.ContainsKey(pair.Key))
                {
                    temp.AddEx(pair.Key, (string)repDic[pair.Key]);
                    continue;
                }

                if (!string.IsNullOrEmpty(pair.Value))
                {
                    object listObj = null;
                    try
                    {
                        if (SPWorkflowCommon.StringIsGUIDFormat(pair.Value))
                        {
                            Guid listId = new Guid(pair.Value);
                            listObj = lists.GetListById(listId, false);
                            if (listObj == null)
                            {
                                //SAAS-29423诊断log
                                logger.Log(AveLogLevel.WARN, "Can not find assUnit list by Id:{0}", listId);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.GetListByIdError, e.ToString());
                    }//no need to log

                    var listTitle = pair.Value;

                    if (listObj == null)
                    {
                        listTitle = SPWorkflowProcessorRuntime.OnLanguageMapping(LanguageMappingScopeEnum.ListTitle, listTitle);
                        listObj = lists.GetListByName(listTitle, false);
                    }
                    if (listObj != null)
                    {
                        string idStr = ((IAveList)listObj).ID.ToString().ToUpper();
                        temp.Add(pair.Key, idStr);
                        //SPWorkflowProcessorRuntime.Log(Logs.Markup_FoundListByTitle, listTitle, idStr);
                        //SAAS-29423诊断log
                        logger.Log(AveLogLevel.INFO, "Find assoUnit listObj by title:{0},Id:{1}", listTitle, idStr);
                    }
                    else
                    {
                        //SPWorkflowProcessorRuntime.Log(Logs.Markup_MissingList, listTitle);
                        //SAAS-29423诊断log
                        logger.Warn("PWFProcessorException has been thrown due to we can not find the listObj by name,list name:{0}",listTitle);
                        throw new SPWFProcessorException(SPWFProcessorErrorCode.PutIntoPostAction);
                    }
                }
            }

            foreach (KeyValuePair<string, string> pair in temp)
            {
                dic[pair.Key] = pair.Value;
                repDic.AddEx(pair.Key, pair.Value);
            }

            temp.Clear();
        }

        private static void CheckWebServiceSuccess(string op, string resultText)
        {
            if ((resultText == null) || !resultText.Contains("<Success"))
            {
                if (op.Equals("ValidatingWorkflow"))
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.ValidatingWorkflowException);
                else if (op.Equals("AssociatingWorkflow"))
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociatingWorkflowException);
                else
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.WebServiceOperationNotSupported);
            }
        }

        private static IAveFolder GetOrCreateParentFolder(IAveList templateLib, string parentFolderUrl)
        {
            IAveFolder parentFolder = templateLib.RootFolder;
            string[] folderPath = parentFolderUrl.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string name in folderPath)
            {
                try
                {
                    IAveFolder temp = parentFolder.SubFolders[name];
                    parentFolder = temp;
                }
                catch (Exception ex)
                {
                    logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.GetFolderFromParentError, ex.ToString());
                    try
                    {
                        IAveFolder newFolder = parentFolder.SubFolders.Add(name);
                        parentFolder = newFolder;
                    }
                    catch (Exception e)
                    {
                        throw new Exception("The parent folder cannot be found.Exception:" + e.Message);
                    }
                }
            }

            return parentFolder;
        }
        #endregion
    }
}
