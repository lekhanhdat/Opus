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
using System.IO;
using System.Xml;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using LS.SPWorkflowProcessor.SerializableObjects;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Diagnostics;
using AvePoint.Common;
using System.Text.RegularExpressions;
using LS.SPWorkflowProcessor.Services;

namespace LS.SPWorkflowProcessor
{
    public class NintexWorkflowAssociationProc : ICustomWorkflowAssociationProc
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        //private static string assemblyString = "Nintex.Workflow, Version=1.0.0.0, Culture=neutral, PublicKeyToken=913f6bae0ca5ae12";
        //private static Assembly assembly;
        //public static bool IsNintexDllInstalled
        //{
        //    get
        //    {
        //        try
        //        {
        //            if (assembly != null)
        //                return true;
        //            if (SPWorkflowProcessorRuntime.AllProcessorParams != null && SPWorkflowProcessorRuntime.AllProcessorParams.ContainsKey("NWAssemblyName"))
        //            {
        //                assemblyString = SPWorkflowProcessorRuntime.AllProcessorParams["NWAssemblyName"];
        //            }
        //            assembly = Assembly.Load(assemblyString);
        //            SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_IsInstalled, "true");
        //            return true;
        //        }
        //        catch(Exception e)
        //        {
        //            log.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.LoadAssemblyError, e.ToString());
        //            SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_IsInstalled, "false");
        //            return false;
        //        }
        //    }
        //}

        public string NintexDBConnectionString
        {
            get;
            set;
        }
        public static bool NintexPublishedWf = false;
        public Dictionary<Guid, string> ReusableWfName = new Dictionary<Guid, string>();
        private const string NintexNoCodeWorkflowLibName = "NintexWorkflows";

        public void EnsureNintexConfigDBConnection()
        {
            if (!SPWorkflowProcessorRuntime.HasSetNintexConfigDBConnection && SPWorkflowProcessorRuntime.NintexConfigDBConnection == null)
            {
                logger.Info("Get Nintex config DB connection.");
                if (SPWorkflowProcessorRuntime.IsNintexDllInstalled)
                {
                    //改成config
                    if (SPWorkflowProcessorRuntime.AllProcessorParams.ContainsKey("NWConfigDBConnectionStringOfBackup"))
                    {
                        this.NintexDBConnectionString = SPWorkflowProcessorRuntime.AllProcessorParams["NWConfigDBConnectionStringOfBackup"];
                    }
                    else
                    {
                        this.NintexDBConnectionString = NintexDatabaseUtility.GetConfigDBConnectionString();
                    }
                    if (string.IsNullOrEmpty(this.NintexDBConnectionString))
                    {
                        return;
                    }
                    SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_ContentDBConnString, this.NintexDBConnectionString);
                    SPWorkflowProcessorRuntime.NintexConfigDBConnection = new SqlConnection(this.NintexDBConnectionString);
                    SPWorkflowProcessorRuntime.NintexConfigDBConnection.Open();
                    logger.Info("Get Nintex config DB connection successfully.{0}",AvePoint.Wrapper.Common.AveQueryException.InternalCrypto.EncryptMessage(this.NintexDBConnectionString));
                }
            }
        }
        public void BackupNintexWorkflowAssociationDB(SPWFAssociationUnit parentAsso)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("BackupOneAssociation.BackupNintexWorkflowData.BackupNintexWorkflowAssociationDB"))
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "BackupNintexWorkflowAssociationDb");
                EnsureNintexConfigDBConnection();
                if (SPWorkflowProcessorRuntime.NintexConfigDBConnection != null)
                {
                    BackupNintexPublishedWorkflow(parentAsso);
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "BackupCustomWorkflowData");
                }
                else
                {
                    logger.Warn("Cannot get the Nintex workflow config DB info.");
                }
            }

        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "The wrong words are the parameter of sql statement. ")]
        public void BackupCustomWorkflowData(SPWFAssociationUnit parentAsso)
        {
            try
            {
                using (AvePerformanceScope pf = new AvePerformanceScope("BackupOneAssociation.BackupNintexWorkflowData"))
                {
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "BackupCustomWorkflowData");
                    //ADO-80069， site collection level的Reusable Nintex Workflow的template 文件存在Root web下的list “wfpub” 下，Nintex template 文件存在Root Web的list “NintexWorkflows”下。故需要特殊处理。
                    IAveWeb parentWeb = parentAsso.mTemplateLibUnit != null && parentAsso.mTemplateLibUnit.SerializableData.IsRootWebList ? parentAsso.ParentWeb.Site.RootWeb : parentAsso.ParentWeb;
                    {
                        object listObj = null;
                        listObj = parentWeb.GetListByName(NintexNoCodeWorkflowLibName, false);

                        IAveList mList = listObj as IAveList;
                        if (mList == null)
                        { return; }
                        try
                        {
                            if ((mList.Title != string.Empty) && (mList.Title == NintexNoCodeWorkflowLibName))
                            {

                            }
                        }
                        catch (Exception ex)
                        {
                            SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_TemplateLibraryMissing, NintexNoCodeWorkflowLibName, ex.Message);
                            logger.Warn("template library missing exception:{0}", ex.ToString());
                            return;
                        }
                        if (listObj == null)
                        {
                            SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_TemplateLibraryMissing, NintexNoCodeWorkflowLibName);
                            return;
                        }
                        BackupNintexWorkflowAssociationDB(parentAsso);
                        IAveList nintexNoCodeWorkflowLib = (IAveList)listObj;
                        string parentFolderName = parentAsso.SerializableData.mName;
                        //备份template时association为null
                        if (parentAsso.SPAssociation != null)
                        {
                            long maxTicks = parentAsso.SPAssociation.Created.Ticks;
                            foreach (IAveWorkflowAssociation asso in parentAsso.SPAssoicationCollection)
                            {
                                if (asso.BaseId == parentAsso.SPAssociation.BaseId && asso.Created.Ticks > maxTicks)
                                {
                                    maxTicks = asso.Created.Ticks;
                                    parentFolderName = asso.Name;
                                }
                            }
                        }
                        foreach (IAveFolder parentFolder in nintexNoCodeWorkflowLib.RootFolder.SubFolders)
                        {
                            if (!this.ReusableWfName.ContainsKey(parentAsso.SerializableData.mBaseId))
                            {
                                if (parentFolder.Name.Equals(parentFolderName))
                                {
                                    SPWorkflowSubListUnit listUnit = SPWorkflowSubListUnit.GetSubListInfo(nintexNoCodeWorkflowLib);
                                    string noCodeWorkflowName = null;
                                    int cfgFileItemId = -1;
                                    int cfgFileVersion = -1;
                                    Guid listId;
                                    SPWorkflowSubListUnit.GetInfoFromInternalName(parentAsso.SerializableData.mInternalName, out noCodeWorkflowName, out listId, out cfgFileItemId, out cfgFileVersion);
                                    //listUnit.mTemplateFileUnits = SPWorkflowSubFileUnit.GenerateSPFileUnitCollection(parentFolder, cfgFileVersion);
                                    listUnit.mTemplateFileUnits = BackupNintexWorkflowTemplateFiles(parentAsso, parentFolder, nintexNoCodeWorkflowLib);
                                    listUnit.SerializableData.mUnitId = "NintexWorkflow";
                                    parentAsso.SerializableData.mSerializableCustomData = listUnit.FixupSerializableData();
                                    //return;
                                    break;
                                }
                            }
                            else
                            {
                                if (parentFolder.Name.Equals(this.ReusableWfName[parentAsso.SerializableData.mBaseId]))
                                {
                                    SPWorkflowSubListUnit listUnit = SPWorkflowSubListUnit.GetSubListInfo(nintexNoCodeWorkflowLib);
                                    //listUnit.mTemplateFileUnits = SPWorkflowSubFileUnit.GenerateSPFileUnitCollection(parentFolder, -1);
                                    listUnit.mTemplateFileUnits = BackupNintexWorkflowTemplateFiles(parentAsso, parentFolder, nintexNoCodeWorkflowLib);
                                    listUnit.SerializableData.mUnitId = "NintexWorkflow";
                                    parentAsso.SerializableData.mSerializableCustomData = listUnit.FixupSerializableData();
                                    //return;
                                    break;
                                }

                                if (parentFolder.Name.Equals("__globallyReusable", StringComparison.OrdinalIgnoreCase))
                                {
                                    foreach (IAveFolder subFolder in parentFolder.Folders)
                                    {
                                        if (subFolder.Name.Equals(this.ReusableWfName[parentAsso.SerializableData.mBaseId]))
                                        {
                                            SPWorkflowSubListUnit listUnit = SPWorkflowSubListUnit.GetSubListInfo(nintexNoCodeWorkflowLib);
                                            //listUnit.mTemplateFileUnits = SPWorkflowSubFileUnit.GenerateSPFileUnitCollection(subFolder, -1);
                                            listUnit.mTemplateFileUnits = BackupNintexWorkflowTemplateFiles(parentAsso, subFolder, nintexNoCodeWorkflowLib);
                                            listUnit.SerializableData.mUnitId = "NintexWorkflow";
                                            parentAsso.SerializableData.mIsNintexSiteCollectionReusableWorklfow = true;
                                            parentAsso.SerializableData.mSerializableCustomData = listUnit.FixupSerializableData();
                                            //return;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    try
                    {
                        if (SPWorkflowProcessorRuntime.ReplaceSpecificForCompatibility)
                        {
                            ExportNintexWorkflowDefinitionByNWAdmin(parentAsso);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("An error occurred while export workflow by nwadmin.exe, detail: {0}", ex.ToString());
                    }
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "BackupCustomWorkflowData");
                }
            }
            catch (Exception ex)
            {
                logger.Warn("An error occurred while doing backup custom workflow data. exception:{0}", ex.ToString());
            }
        }

        /// <summary>
        /// 由于无法确定nintex template file的具体version(从internal name解析出来的只能确定workflows中template信息，确认不了nintex library中的(ADO-148368))，所以用备份workflow的template来拷贝一份nintex的template信息，这样对备份效率也会有一定提升
        /// </summary>
        /// <param name="assoUnit"></param>
        /// <param name="parentFolder"></param>
        /// <param name="parentLibrary"></param>
        /// <returns></returns>
        private List<SPWorkflowSubFileUnit> BackupNintexWorkflowTemplateFiles(SPWFAssociationUnit assoUnit,IAveFolder parentFolder,IAveList parentLibrary)
        {
            if (assoUnit == null || assoUnit.mTemplateLibUnit == null || assoUnit.mTemplateLibUnit.mTemplateFileUnits == null)
            {
                return null;
            }
            List<SPWorkflowSubFileUnit> files = new List<SPWorkflowSubFileUnit>();
            foreach (var fileUnit in assoUnit.mTemplateLibUnit.mTemplateFileUnits)
            {
                SPWorkflowSubFileUnit templateFileUnit = null;
                IAveFile templateFile = null;
                try
                {
                    templateFile = parentFolder.Files[fileUnit.SerializableData.mName];
                }
                catch (Exception e)
                {
                    logger.Warn("An error occurred while getting nintex workflow template file:{0}.Error:{1}.", fileUnit.SerializableData.mName, e);
                }
                if (templateFile != null && templateFile.Exists)
                {
                    templateFileUnit = BackupOneNintexWorkflowTemplateFile(fileUnit, templateFile, parentLibrary);
                }
                if (templateFileUnit != null)
                {
                    files.Add(templateFileUnit);
                }
            }
            return files;
        }

        /// <summary>
        /// 根据之前备份template file unit的信息以及nintex template library中对应file的信息 组成nintex template file unit
        /// </summary>
        /// <param name="sourceFileUnit"></param>
        /// <param name="file"></param>
        /// <param name="parentLibrary"></param>
        /// <returns></returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ipfs_streamhash is a property name.")]
        private SPWorkflowSubFileUnit BackupOneNintexWorkflowTemplateFile(SPWorkflowSubFileUnit sourceFileUnit, IAveFile file, IAveList parentLibrary)
        {
            SPWorkflowSubFileUnit fileUnit = new SPWorkflowSubFileUnit();
            try
            {
                #region init from file unit

                fileUnit.SerializableData.mCharSetName = sourceFileUnit.SerializableData.mCharSetName;
                fileUnit.SerializableData.mUIVersion = sourceFileUnit.SerializableData.mUIVersion;
                fileUnit.SerializableData.mCreated = sourceFileUnit.SerializableData.mCreated;
                fileUnit.SerializableData.mModified = sourceFileUnit.SerializableData.mModified;
                fileUnit.SerializableData.mAuthorId = sourceFileUnit.SerializableData.mAuthorId;
                fileUnit.SerializableData.mAuthorLogin = sourceFileUnit.SerializableData.mAuthorLogin;
                fileUnit.SerializableData.mEditorId = sourceFileUnit.SerializableData.mEditorId;
                fileUnit.SerializableData.mEditorLogin = sourceFileUnit.SerializableData.mEditorLogin;
                if (sourceFileUnit.SerializableData.mContent != null)
                {
                    byte[] sourceContent = sourceFileUnit.SerializableData.mContent;
                    fileUnit.SerializableData.mContent = new byte[sourceContent.Length];
                    Array.Copy(sourceContent, fileUnit.SerializableData.mContent, sourceContent.Length);
                }
                fileUnit.SerializableData.mIsCurrentVersion = sourceFileUnit.SerializableData.mIsCurrentVersion;
                Dictionary<string, string> sourceGuidDic = sourceFileUnit.SerializableData.mGUIDDictionary;
                if (sourceGuidDic != null && sourceGuidDic.Count > 0)
                {
                    fileUnit.SerializableData.mGUIDDictionary = new Dictionary<string, string>(sourceGuidDic);
                }
                else
                {
                    fileUnit.SerializableData.mGUIDDictionary = new Dictionary<string, string>();
                }
                fileUnit.SerializableData.mTemplateLibTitle = sourceFileUnit.SerializableData.mTemplateLibTitle;

                #endregion

                #region init from file property

                if (file.Properties != null && file.Properties.ContainsKey("ipfs_streamhash"))
                {
                    fileUnit.SerializableData.ipfs_streamhash = file.Properties["ipfs_streamhash"].ToString();
                }
                if (file.Properties != null && file.Properties.ContainsKey("vti_setuppath"))
                {
                    fileUnit.SerializableData.mSetupPath = (string)file.Properties["vti_setuppath"];
                }
                fileUnit.SerializableData.mDocFlags = SPWorkflowSubFileUnit.GetWorkflowTemplateFileDocFlags(file);
                fileUnit.SerializableData.mUniqueId = file.UniqueId;
                fileUnit.SerializableData.mItemId = file.Item == null ? 0 : file.Item.ID;
                fileUnit.SerializableData.mParentFolderName = file.ParentFolder.Name;
                fileUnit.SerializableData.mListRelativeUrl = file.ServerRelativeUrl.Substring(file.ParentFolder.ParentWeb.ServerRelativeUrl.Length);
                fileUnit.SerializableData.mFirstParentFolderRelativeUrl = file.ServerRelativeUrl.Substring(parentLibrary.RootFolder.ServerRelativeUrl.Length);
                fileUnit.SerializableData.mRootFolderRelativeUrl = fileUnit.SerializableData.mFirstParentFolderRelativeUrl;
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
                    logger.Log(AveLogLevel.DEBUG, "An error occurred while get nintex template file field Category schema xml.Error:{0}", e);
                }

                #endregion
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while generating nintex template file unit.FileInfo:{0},Error:{1}", file.ServerRelativeUrl, e);
                fileUnit = null;
            }
            return fileUnit;
        }

        public void BackupNintexPublishedWorkflow(SPWFAssociationUnit parentAsso)
        {
            try
            {
                Guid baseId = parentAsso.mSPAssociation == null ? parentAsso.SerializableData.mBaseId : parentAsso.mSPAssociation.BaseId;
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = SPWorkflowProcessorRuntime.NintexConfigDBConnection;

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@WorkflowID", baseId);
                cmd.CommandText = "SELECT * FROM PublishedWorkflows WITH(NOLOCK) WHERE WorkflowID=@WorkflowID AND (WorkflowType = 'Reusable' or WorkflowType = 'GloballyReusable') AND Version = 1";
                using (SqlDataAdapter nwAdapter = new SqlDataAdapter())
                {
                    nwAdapter.SelectCommand = cmd;
                    using (DataTable nwMemoryTable = new DataTable())
                    {
                        nwAdapter.Fill(nwMemoryTable);
                        foreach (DataRow dr in nwMemoryTable.Rows)
                        {
                            SPWFAssociationSerializableData tempUnit = new SPWFAssociationSerializableData();
                            tempUnit.SetPropsFromDataRow(dr, nwMemoryTable.Columns);
                            parentAsso.SerializableData.ChildUnits.Add(tempUnit);
                            if (!ReusableWfName.ContainsKey(baseId))
                            {
                                this.ReusableWfName.Add(baseId, tempUnit.Properties["#WorkflowName"].ToString());
                            }
                        }
                        if (nwMemoryTable.Rows.Count > 0)
                        {
                            nwMemoryTable.Clear();
                            parentAsso.SerializableData.mIsNintexReusableWorkflow = true;
                            NintexWorkflowAssociationProc.NintexPublishedWf = true;
                        }
                        else
                        {
                            nwMemoryTable.Clear();
                            parentAsso.SerializableData.mIsNintexReusableWorkflow = false;
                            NintexWorkflowAssociationProc.NintexPublishedWf = false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_HandleTableException, e.Message);
                logger.Warn("Backup nintex published workflow exception:{0}", e.ToString());
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "The wrong words are in the log info.")]
        public void RestoreCustomWorkflowData(SPWFAssociationUnit parentAsso)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("RestoreOneAssociation.RestoreNintexWorkflowData"))
            {
                SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "RestoreCustomWorkflowData");
                if (parentAsso.SerializableData.mSerializableCustomData == null)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_NoBackupData);
                    return;
                }
                SPWorkflowSubListUnit listUnit = new SPWorkflowSubListUnit((SPWorkflowSubListSerializableData)parentAsso.SerializableData.mSerializableCustomData);
                if (string.IsNullOrEmpty(listUnit.SerializableData.mUnitId) || listUnit.SerializableData.mUnitId != "NintexWorkflow")
                {
                    SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_NoBackupData);
                    return;
                }
                try
                {
                    IAveWorkflowAssociation temp = null;
                    if (listUnit.mTemplateFileUnits != null && listUnit.mTemplateFileUnits.Count > 0)
                    {
                        EnsureTemplateLibrary(parentAsso);
                        SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "Handle template files");
                        SPWorkflowSubFileUnit.HandleTemplateSPFileUnits(parentAsso, listUnit, false, out temp);
                        SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "Handle template files");
                        foreach (SPWorkflowSubFileUnit fileUnit in listUnit.mTemplateFileUnits)
                        {
                            if (fileUnit.mSPFile == null)
                            {
                                SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_SPFileMissing, fileUnit.SerializableData.mName);
                                continue;
                            }

                            if (fileUnit.SerializableData.mName.ToLower(CultureInfo.CurrentCulture).EndsWith(".xoml", StringComparison.Ordinal))
                            {
                                SetCustomWorkflowData(parentAsso, fileUnit);
                            }

                        }
                        if (SPWorkflowProcessorRuntime.ReplaceSpecificForCompatibility)
                        {
                            bool isNeedReload = ImportNintexWorkflowDefinitionByNWAdmin(parentAsso, listUnit);
                            if (isNeedReload)
                            {
                                //list或CT wf可能会导致web,list对象不一致,需要reload下list
                                parentAsso.ReloadParentWeb();
                                logger.Log(AveLogLevel.INFO, "Reload web and list after publish nintex workflow by nwadmin.");
                            }
                        }
                    }
                    else
                    {
                        SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_TemplateFileMissing, parentAsso.SerializableData.mOriginalName);
                    }
                }
                catch (Exception e)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_RestoreUnknownException, e.Message);
                    logger.Warn("An exception occurred while restore custom workflow data. exception:{0}", e.ToString());
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.AssociationCustomDataRestoreError, e, listUnit.SerializableData.mUnitId);
                }
                finally
                {
                    SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "RestoreCustomWorkflowData");
                }
            }
        }

        private void EnsureTemplateLibrary(SPWFAssociationUnit assoUnit)
        {
            IAveWeb web = assoUnit.SerializableData.mIsNintexSiteCollectionReusableWorklfow ? assoUnit.ParentWeb.Site.RootWeb : assoUnit.ParentWeb;
            Guid listId = Guid.Empty;
            try
            {
                var sPFolder = web.RootFolder.Folders[NWSharePointObjects.LibraryNameWorkflows];
                listId = sPFolder.ParentListId;
            }
            catch(Exception e)
            {
                logger.Warn("An error occurred while getting template library id.Error:{0}",e);
            }
            if (listId != Guid.Empty)
            {
                IAveList templateLibrary = web.Lists.GetListById(listId,false);
                if (templateLibrary != null)
                {
                    EnsureNWTemplateLibraryField(web, templateLibrary, NWSharePointObjects.FieldNameWorkflowCategory, NWSharePointObjects.FieldWorkflowCategory);
                    EnsureNWTemplateLibraryField(web, templateLibrary, NWSharePointObjects.FieldNameAssociatedContentType, NWSharePointObjects.FieldAssociatedContentType);
                }
            }
        }

        private void EnsureNWTemplateLibraryField(IAveWeb web, IAveList list, string fieldName, Guid fieldId)
        {
            try
            {
                if (!list.Fields.ContainsField(fieldName))
                {
                    list.Fields.Add(web.AvailableFields[fieldId]);
                    logger.Debug("Add field {0} to list {1} successful.", fieldName, list.Title);
                }
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while ensure list fields for nintex workflow template library.List:{0},Field:{1},Error:{2}", list.Title, fieldName, e);
            }
        }

        private string GetExportCommand(SPWFAssociationUnit parentAsso,string tempFilePath)
        {
            const string EXPORT_LIST_WF = "-o ExportWorkflow -siteUrl \"{0}\" -workflowName \"{1}\" -filename \"{2}\" -workflowType \"{3}\" -list \"{4}\" -username \"{5}\" -password \"{6}\" -domain \"{7}\"";
            
            string siteUrl = parentAsso.ParentWeb.Url;
            string workflowType = "list";
            string workflowName = parentAsso.SPAssociation.Name;
            string listName = parentAsso.ParentList.Title;
            SPWorkflowUserInfo userInfo = SPWorkflowProcessorRuntime.GetNWAdminUserInfo();
            if (userInfo == null)
            {
                throw new ArgumentNullException("userInfo");
            }
            return string.Format(EXPORT_LIST_WF, siteUrl, workflowName, tempFilePath, workflowType, listName, userInfo.UserName, userInfo.Password, userInfo.Domain);
        }

        /// <summary>
        /// from CI ADO-142524
        /// </summary>
        /// <param name="parentAsso"></param>
        /// <returns></returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Ignore the word:nwf,NWAdmin,Problematic")]
        private bool ExportNintexWorkflowDefinitionByNWAdmin(SPWFAssociationUnit parentAsso)
        {
            logger.Debug("ExportNintexWorkflowDefinitionByNWAdmin.IsCurrent :{0},parentType:{1}", parentAsso.IsCurrentVersion, parentAsso.ParentObjectType);
         
            if (parentAsso.ParentObjectType != SPWFAssociationParentType.List && parentAsso.ParentObjectType != SPWFAssociationParentType.ListContentType)
            {
                return false;
            }
            if (!parentAsso.IsCurrentVersion)
            {
                return false;
            }
            logger.Debug("Workflow name:{0}", parentAsso.SPAssociation.Name);
            string workflowFile = AveWrapperConstants.WrapperTempFolder + '\\' + Guid.NewGuid().ToString() + ".nwf";
            string nwAdminPath = SPWorkflowProcessorRuntime.ObjectModelFactory.Utility.GetGenericSetupPath("BIN").TrimEnd('\\') + "\\NWAdmin.exe";
            try
            {
                string command = GetExportCommand(parentAsso,workflowFile);
                StartExternalCode(nwAdminPath, command);
                byte[] content = File.ReadAllBytes(workflowFile);
                SPWorkflowSubListSerializableData serializableData = (SPWorkflowSubListSerializableData)parentAsso.SerializableData.mSerializableCustomData;
                serializableData.mTemplateFileDatas.Add(new SPWorkflowSubFileSerializableData() { mName = "NWF", mContent = content });
                return true;
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.ERROR, "An error occurred while export nintex workflow by nwadmin.exe, error:{0}", e);
                return false;
            }
            finally
            {
                DeleteTempFile(workflowFile);
            }
        }

        private void DeleteTempFile(string filePath)
        {
            try
            {
                File.Delete(filePath);
            }
            catch (FileNotFoundException notFoundEx)
            {
                logger.Warn("File not found. {0}", notFoundEx);
            }
            catch (FileLoadException notLoadEx)
            {
                logger.Warn("File not load. {0}", notLoadEx);
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Ignore the word:nwf,NWAdmin,Problematic")]
        private string GetImportCommand(SPWFAssociationUnit parentAsso, string tempFilePath)
        {
            const string IMPORT_LIST_WF = "-o DeployWorkflow -workflowName \"{0}\" -nwfFile \"{1}\" -siteUrl \"{2}\" -targetList \"{3}\" -overwrite";
            string siteUrl = parentAsso.ParentWeb.Url;
            string workflowName = parentAsso.SPAssociation.Name;
            string listName = parentAsso.ParentList.Title;
            return string.Format(IMPORT_LIST_WF, workflowName, tempFilePath, siteUrl, listName);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Ignore the word:nwf,NWAdmin,Problematic")]
        private bool ImportNintexWorkflowDefinitionByNWAdmin(SPWFAssociationUnit parentAsso, SPWorkflowSubListUnit listUnit)
        {
            logger.Debug("ImportNintexWorkflowDefinitionByNWAdmin.IsCurrent :{0},parentType:{1}", parentAsso.IsCurrentVersion, parentAsso.ParentObjectType);
            if (parentAsso.ParentObjectType != SPWFAssociationParentType.List && parentAsso.ParentObjectType != SPWFAssociationParentType.ListContentType)
            {
                return false;
            }
            SPWorkflowSubFileUnit fileUnit;
            if (!TryFindWorkflowExportFileUnit(listUnit, out fileUnit))
            {
                logger.Info("Exported workflow tempalte file unit is not found.");
                return false;
            }
            logger.Debug("Workflow name:{0}", parentAsso.SPAssociation.Name);
            string workflowFile = AveWrapperConstants.WrapperTempFolder + '\\' + Guid.NewGuid().ToString() + ".nwf";
            string nwAdminPath = SPWorkflowProcessorRuntime.ObjectModelFactory.Utility.GetGenericSetupPath("BIN").TrimEnd('\\') + "\\NWAdmin.exe";
            try
            {
                Dictionary<string, string> nintexActionReplaceDictionary = new Dictionary<string, string>();
                List<string> invaildWorkflowAction = new List<string>();
                SPWorkflowProcessorRuntime.GetNWFConvertConfigurations(ref nintexActionReplaceDictionary, ref invaildWorkflowAction);
                byte[] content = fileUnit.SerializableData.mContent;
                string tempContent = string.Empty;
                if (content != null)
                {
                    tempContent = System.Text.Encoding.UTF8.GetString(content);
                }
                if (string.IsNullOrEmpty(tempContent))
                {
                    throw new ArgumentException("NWF file content is empty.");
                }

                #region Replace nintex workflow external dic

                foreach (KeyValuePair<string, string> pair in nintexActionReplaceDictionary)
                {
                    tempContent.Replace(pair.Key, pair.Value);
                }

                #endregion

                tempContent = DisableProblematicWorkflowAction(tempContent, invaildWorkflowAction);
                content = System.Text.Encoding.UTF8.GetBytes(tempContent);
                File.WriteAllBytes(workflowFile, content);
                string command = GetImportCommand(parentAsso, workflowFile);
                StartExternalCode(nwAdminPath, command);
                return true;
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.ERROR, "An error occurred while import nintex workflow by nwadmin.exe, error:{0}", e);
                return false;
            }
            finally
            {
                DeleteTempFile(workflowFile);
            }
        }

        private bool TryFindWorkflowExportFileUnit(SPWorkflowSubListUnit listUnit, out SPWorkflowSubFileUnit fileUnit)
        {
            fileUnit = null;
            bool find = false;
            try
            {
                fileUnit = listUnit.mTemplateFileUnits.Find(file => string.Equals(file.SerializableData.mName, "NWF", StringComparison.OrdinalIgnoreCase));
                if (fileUnit != null)
                {
                    find = true;
                }
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while finding export template fileUnit.Error:{0}", e);
            }
            return find;
        }

        public static int StartExternalCode(string externalExe, string command, bool bIsWait = true)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.CreateNoWindow = true;
                startInfo.RedirectStandardError = true;
                startInfo.RedirectStandardInput = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
                startInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
                startInfo.UseShellExecute = false;
                startInfo.Arguments = command;
                startInfo.FileName = externalExe;
                Process tempPro = Process.Start(startInfo);
                if (bIsWait)
                {
                    tempPro.WaitForExit();
                    StreamReader swStandardOutput = tempPro.StandardOutput;
                    StreamReader swStandardError = tempPro.StandardError;
                    string output = swStandardOutput.ReadToEnd();
                    string error = swStandardError.ReadToEnd();
                    logger.Debug("External code completed. INFO:{0}, {1},{2}", AvePoint.Wrapper.Common.AveQueryException.InternalCrypto.EncryptMessage(command), output, error);
                    return tempPro.ExitCode;
                }
                return 0;
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.ERROR, "An error occurred while start external exe, detail:{0}", e.ToString());
                return 1;
            }
        }

        private void CheckUsedUserDefinedActionByContent(string wfConfigFileContent)
        {
            logger.Debug("Check used user define action in nintex content replace.");
            string GUIDREG = "[A-F0-9]{8}(-[A-F0-9]{4}){3}-[A-F0-9]{12}";
            string GUIDREG_WITH_HTML_ENCODE = "[A-F0-9]{8}(%2D[A-F0-9]{4}){3}%2D[A-F0-9]{12}";
            CheckUsedUserDefinedActionByContent("StaticId=\"", GUIDREG, wfConfigFileContent);
            CheckUsedUserDefinedActionByContent("StaticId=\"", GUIDREG_WITH_HTML_ENCODE, wfConfigFileContent);
        }

        private void CheckUsedUserDefinedActionByContent(string prefix, string regKey, string strContent)
        {
            Regex reg = new Regex(prefix + regKey, RegexOptions.IgnoreCase);
            int startPos = 0;
            while (true)
            {
                var match = reg.Match(strContent, startPos);
                if (match.Success)
                {
                    startPos = match.Index + 1;
                    var guidStr = strContent.Substring(match.Index + prefix.Length, match.Length - prefix.Length);
                    if (!string.IsNullOrEmpty(guidStr))
                    {
                        // if guidStr contains html encode "%2d", change to '-'
                        guidStr = guidStr.Replace("%2d", "-").Replace("%2D", "-");
                        var guid = new Guid(guidStr);
                        if (!SPWorkflowProcessorRuntime.NeedRestoreUserDefiniedActionId.Contains(guid))
                        {
                            logger.Debug("Need restore user defined action with static id by content, id: {0}", guid.ToString());
                            SPWorkflowProcessorRuntime.NeedRestoreUserDefiniedActionId.Add(guid);
                        }
                    }
                }
                else
                {
                    break;
                }
            }
        }


       /// <summary>
       /// disable掉在目的端无法使用的custom action
       /// </summary>
       /// <param name="originalNWF"></param>
       /// <param name="types"></param>
       /// <returns></returns>
       [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Problematic,Seralized are part of nintex workflow action type name.")]
        private string DisableProblematicWorkflowAction(string originalNWF, List<string> types)
        {
            logger.Info("Start Disable Problematic WorkflowAction");

            const string CONVERT_TO_PDF = "Pragmantic.NintexWorkflow.Actions.ConvertToPdfAdapter";
            if (!types.Contains(CONVERT_TO_PDF)) { types.Add(CONVERT_TO_PDF); }

            XmlDocument document = new XmlDocument();
            document.LoadXml(originalNWF);

            XmlDocument tempDocument = new XmlDocument();
            XmlElement tempElement = tempDocument.CreateElement("ExportedWorkflowSeralized");
            tempElement.InnerXml = document.DocumentElement["ExportedWorkflowSeralized"].InnerText;

            XmlNodeList xmlNodeList = tempElement.SelectNodes("//NWActionConfig");
            foreach (XmlNode node in xmlNodeList)
            {
                foreach (string type in types)
                {
                    if (node.ChildNodes != null && node["Type"] != null
                     && (node["Type"].InnerText.IndexOf(type, StringComparison.OrdinalIgnoreCase) > 0 || node["Type"].InnerText.Equals(type, StringComparison.OrdinalIgnoreCase)))
                    {
                        if (node["Enabled"] != null)
                        {
                            node["Enabled"].InnerText = "false";
                        }
                        else
                        {
                            XmlNode newNode = tempDocument.CreateNode(XmlNodeType.Element, "Enabled", string.Empty);
                            newNode.InnerText = "false";
                            node.AppendChild(newNode);
                        }
                        logger.Info("Disable WorkflowAction: {0}", type);
                    }
                }
            }
            document.DocumentElement["ExportedWorkflowSeralized"].InnerText = tempElement.InnerXml;
            logger.Info("End Disable Problematic WorkflowAction");
            return document.OuterXml;
        }

        private void SetCustomWorkflowData(SPWFAssociationUnit parentAsso, SPWorkflowSubFileUnit fileUnit)
        {
            //using (StreamReader objReader = new StreamReader(fileUnit.mSPFile.OpenBinaryStream(WrapperConfiguration.OpenBinaryOptions)))
            {
                XmlDocument doc = null;
                try
                {
                    Dictionary<string, NintexActivityMemberInfo> customData = new Dictionary<string, NintexActivityMemberInfo>();
                    string strContent = Encoding.UTF8.GetString(fileUnit.SerializableData.mContent, 0, fileUnit.SerializableData.mContent.Length);
                    CheckUsedUserDefinedActionByContent(strContent);
                    doc = new XmlDocument();
                    doc.LoadXml(strContent);
                    string startTag = "{ActivityBind ROOT,Path=";
                    string endTag = "}";
                    List<XmlElement> nodeList = GetAllXmlNodes(doc);
                    if (nodeList != null && nodeList.Count > 0)
                    {
                        NintexActivityMemberInfo humanWorkflowIdFields = new NintexActivityMemberInfo();
                        NintexActivityMemberInfo taskListItemIdFields = new NintexActivityMemberInfo();
                        foreach (XmlElement node in nodeList)
                        {
                            //HumanWorkflowId="{ActivityBind ROOT,Path=_taskId5b426386a7c7473299c32204138bf923}" 
                            string value = string.Empty;
                            if (node.HasAttribute("HumanWorkflowId"))
                            {
                                value = node.GetAttribute("HumanWorkflowId");
                                if (value != null
                                    && value.StartsWith(startTag, StringComparison.Ordinal)
                                    && value.EndsWith(endTag, StringComparison.Ordinal))
                                {
                                    string humanWorkflowIdField = value.Substring(0, value.Length - endTag.Length).Substring(startTag.Length);
                                    if (!humanWorkflowIdFields.Parameters.Contains(humanWorkflowIdField))
                                    {
                                        humanWorkflowIdFields.Parameters.Add(humanWorkflowIdField);
                                    }
                                }
                            }
                            //TaskListItemId="{ActivityBind ROOT,Path=_Int3211}"
                            if (node.HasAttribute("TaskListItemId"))
                            {
                                value = node.GetAttribute("TaskListItemId");
                                if (value != null
                                    && value.StartsWith(startTag, StringComparison.Ordinal)
                                    && value.EndsWith(endTag, StringComparison.Ordinal))
                                {
                                    string taskListItemId = value.Substring(0, value.Length - endTag.Length).Substring(startTag.Length);
                                    if (!taskListItemIdFields.Parameters.Contains(taskListItemId))
                                    {
                                        taskListItemIdFields.Parameters.Add(taskListItemId);
                                    }
                                }
                            }
                        }
                        customData.Add("HumanWorkflowId", humanWorkflowIdFields);
                        customData.Add("TaskListItemId", taskListItemIdFields);
                    }
                    parentAsso.mNonSerializedCustomData = customData;
                }
                catch (Exception e)
                {
                    SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_GetActionException, e.Message);
                    logger.Warn("An exception occurred while set custom workflow data. exception:{0}", e.ToString());
                }
                finally
                {
                    if (doc != null)
                    {
                        doc.RemoveAll();
                    }
                }
            }
        }

        private List<XmlElement> GetAllXmlNodes(XmlDocument xDoc)
        {
            List<XmlElement> nodeList = new List<XmlElement>();
            foreach (var node in xDoc.ChildNodes)
            {
                if (node is XmlElement)
                {
                    nodeList.Add((XmlElement)node);
                    AddSubNodes((XmlElement)node, nodeList);
                }
            }
            return nodeList;
        }

        private void AddSubNodes(XmlElement node, List<XmlElement> nodeList)
        {
            foreach (var subNode in node.ChildNodes)
            {
                if (subNode is XmlElement)
                {
                    nodeList.Add((XmlElement)subNode);
                    AddSubNodes((XmlElement)subNode, nodeList);
                }
            }
        }
        //[SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Update is a whole word")]
        //private void UpdateNWFileProperties(SPWFAssociationUnit parentAsso, SPWorkflowSubFileUnit fileUnit)
        //{
        //    using (AvePerformanceScope pf = new AvePerformanceScope("RestoreOneAssociation.RestoreNintexWorkflowData.UpdateNWFileProperties"))
        //    {
        //        SPWorkflowProcessorRuntime.Log(Logs.MonitorScope, "UpdateNWFileProperties:" + fileUnit.SerializableData.mName);
        //        if (!fileUnit.SerializableData.mName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) &&
        //            !fileUnit.SerializableData.mName.EndsWith(".xoml", StringComparison.OrdinalIgnoreCase) &&
        //            !fileUnit.SerializableData.mName.EndsWith(".rules", StringComparison.OrdinalIgnoreCase))
        //        {
        //            return;
        //        }

        //        try
        //        {
        //            //if (fileUnit.SerializableData.mName.ToLower().EndsWith(".xoml.wfconfig.xml"))
        //            //{
        //            IAveFile file = fileUnit.mSPFile;
        //            if (file.Properties.ContainsKey("vti_title"))
        //                file.Properties["vti_title"] = parentAsso.SPAssociation.Name;
        //            else
        //                file.Properties.Add("vti_title", parentAsso.SPAssociation.Name);
        //            if (!parentAsso.SerializableData.mIsNintexReusableWorkflow && parentAsso.ParentObjectType != SPWFAssociationParentType.Web)
        //            {
        //                if (file.Properties.ContainsKey("AssociatedListID"))
        //                    file.Properties["AssociatedListID"] = parentAsso.SPAssociation.ParentList.ID.ToString("B");
        //                else
        //                    file.Properties.Add("AssociatedListID", parentAsso.SPAssociation.ParentList.ID.ToString("B"));
        //            }
        //            if (file.Properties.ContainsKey("NintexWorkflowID"))
        //                file.Properties["NintexWorkflowID"] = parentAsso.SPAssociation.BaseId.ToString("B");
        //            else
        //                file.Properties.Add("NintexWorkflowID", parentAsso.SPAssociation.BaseId.ToString("B"));

        //            if (file.Properties.ContainsKey("NintexWorkflowDescription"))
        //                file.Properties["NintexWorkflowDescription"] = parentAsso.SPAssociation.Description;
        //            else
        //                file.Properties.Add("NintexWorkflowDescription", parentAsso.SPAssociation.Description);

        //            if (parentAsso.InternalVersion.Equals(SharePointVersion.SharePoint2010.ToString(), StringComparison.OrdinalIgnoreCase) && (!parentAsso.SerializableData.mIsNintexReusableWorkflow) && (parentAsso.ParentObjectType != SPWFAssociationParentType.Web))
        //            {
        //                if (file.Properties.ContainsKey("Category"))
        //                    file.Properties["Category"] = "List";
        //                else
        //                    file.Properties.Add("Category", "List");
        //            }
        //            if (parentAsso.SerializableData.mIsNintexReusableWorkflow)
        //            {
        //                if (file.Properties.ContainsKey("Category"))
        //                    file.Properties["Category"] = "Reusable";
        //                else
        //                    file.Properties.Add("Category", "Reusable");
        //            }
        //            if (parentAsso.ParentObjectType == SPWFAssociationParentType.Web)
        //            {
        //                if (file.Properties.ContainsKey("Category"))
        //                    file.Properties["Category"] = "Site";
        //                else
        //                    file.Properties.Add("Category", "Site");
        //            }
        //            if (file.Properties.ContainsKey("WebID"))
        //                file.Properties["WebID"] = parentAsso.SPAssociation.ParentWeb.ID.ToString("B");
        //            else
        //                file.Properties.Add("WebID", parentAsso.SPAssociation.ParentWeb.ID.ToString("B"));

        //            if (file.Properties.ContainsKey("NWAssociatedWebID"))
        //                file.Properties["NWAssociatedWebID"] = parentAsso.SPAssociation.ParentWeb.ID.ToString("B");
        //            else
        //                file.Properties.Add("NWAssociatedWebID", parentAsso.SPAssociation.ParentWeb.ID.ToString("B"));

        //            try
        //            {
        //                file.CheckOut(false, string.Empty);
        //            }
        //            catch (Exception e)
        //            {
        //                SPWorkflowProcessorRuntime.Log(Logs.Common_SPFileCheckOutException, e.Message);
        //            }

        //            try
        //            {
        //                file.Update();
        //            }
        //            catch (Exception e)
        //            {
        //                SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_UpdateTempFilePropsException, e.Message);
        //            }
        //            if (!parentAsso.SerializableData.mIsNintexReusableWorkflow && parentAsso.ParentObjectType != SPWFAssociationParentType.Web)
        //            {
        //                file.Item["AssociatedListID"] = parentAsso.SPAssociation.ParentList.ID.ToString("B");
        //            }
        //            if (parentAsso.SerializableData.mIsNintexReusableWorkflow)
        //            {
        //                try
        //                {
        //                    if (!parentAsso.SerializableData.mIsNintexSiteCollectionReusableWorklfow)
        //                    {
        //                        file.Item["Category"] = "Reusable";
        //                    }
        //                    else
        //                    {
        //                        file.Item["Category"] = "GloballyReusable";
        //                    }
        //                }
        //                catch (Exception e)
        //                {
        //                    log.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.SetItemAttributeError, e.ToString());

        //                    try
        //                    {
        //                        log.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.SetItemAttributeError, e.ToString());
        //                        XmlDocument xmlLoader = new XmlDocument();
        //                        xmlLoader.LoadXml(fileUnit.SerializableData.mCategorySchemalXml);
        //                        XmlElement xmlEle = xmlLoader["Field"];
        //                        xmlEle.SetAttribute("DisplayName", "WorkflowCategory");
        //                        fileUnit.SerializableData.mCategorySchemalXml = xmlLoader.OuterXml.ToString();
        //                        IAveField fieldNew = file.Item.Fields.AddFieldAsXml(fileUnit.SerializableData.mCategorySchemalXml);
        //                        fieldNew.Title = "Category";
        //                        fieldNew.Update();
        //                        if (!parentAsso.SerializableData.mIsNintexSiteCollectionReusableWorklfow)
        //                        {
        //                            file.Item["Category"] = "Reusable";
        //                        }
        //                        else
        //                        {
        //                            file.Item["Category"] = "GloballyReusable";
        //                        }
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        log.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.SetItemAttributeError, ex.ToString());
        //                    }
        //                }
        //            }
        //            if (parentAsso.ParentObjectType == SPWFAssociationParentType.Web)
        //            {
        //                try
        //                {
        //                    file.Item["Category"] = "Site";
        //                }
        //                catch (Exception ex)
        //                {
        //                    log.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.SetItemAttributeError, ex.ToString());

        //                    try
        //                    {
        //                        XmlDocument xmlLoader = new XmlDocument();
        //                        xmlLoader.LoadXml(fileUnit.SerializableData.mCategorySchemalXml);

        //                        XmlElement xmlEle = xmlLoader["Field"];
        //                        xmlEle.SetAttribute("DisplayName", "WorkflowCategory");
        //                        fileUnit.SerializableData.mCategorySchemalXml = xmlLoader.OuterXml.ToString();
        //                        IAveField fieldNew = file.Item.Fields.AddFieldAsXml(fileUnit.SerializableData.mCategorySchemalXml);
        //                        fieldNew.Title = "Category";
        //                        fieldNew.Update();
        //                        file.Item["Category"] = "Site";
        //                    }
        //                    catch (Exception exception)
        //                    {
        //                        log.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.SetItemAttributeError, exception.ToString());
        //                    }
        //                }
        //            }
        //            if (parentAsso.ParentObjectType != SPWFAssociationParentType.Web && !parentAsso.SerializableData.mIsNintexReusableWorkflow)
        //            {
        //                try
        //                {
        //                    file.Item["Category"] = "List";
        //                }
        //                catch (Exception ex)
        //                {
        //                    log.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.SetItemAttributeError, ex.ToString());

        //                    try
        //                    {
        //                        XmlDocument xmlLoader = new XmlDocument();
        //                        xmlLoader.LoadXml(fileUnit.SerializableData.mCategorySchemalXml);

        //                        XmlElement xmlEle = xmlLoader["Field"];
        //                        xmlEle.SetAttribute("DisplayName", "WorkflowCategory");
        //                        fileUnit.SerializableData.mCategorySchemalXml = xmlLoader.OuterXml.ToString();
        //                        IAveField fieldNew = file.Item.Fields.AddFieldAsXml(fileUnit.SerializableData.mCategorySchemalXml);
        //                        fieldNew.Title = "Category";
        //                        fieldNew.Update();
        //                        file.Item["Category"] = "List";
        //                    }
        //                    catch (Exception exception)
        //                    {
        //                        log.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.SetItemAttributeError, exception.ToString());
        //                    }
        //                }
        //            }
        //            file.Item["NintexWorkflowID"] = parentAsso.SPAssociation.BaseId.ToString("B");
        //            file.Item["NintexWorkflowDescription"] = parentAsso.SPAssociation.Description;
        //            try
        //            {
        //                file.Item["WebID"] = parentAsso.SPAssociation.ParentWeb.ID.ToString("B");
        //            }
        //            catch (Exception ex)
        //            {
        //                log.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.SetItemAttributeError, ex.ToString());
        //                try
        //                {
        //                    file.Item["NWAssociatedWebID"] = parentAsso.SPAssociation.ParentWeb.ID.ToString("B");
        //                }
        //                catch (Exception exception)
        //                {
        //                    log.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.SetItemAttributeError, exception.ToString());
        //                }//need not to log
        //            }

        //            try
        //            {
        //                file.Item.Update();
        //            }
        //            catch (Exception e)
        //            {
        //                SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_UpdateTempItemPropsException, e.Message);
        //            }

        //            try
        //            {
        //                file.CheckIn(string.Empty);
        //            }
        //            catch (Exception e)
        //            {
        //                SPWorkflowProcessorRuntime.Log(Logs.Common_SPFileCheckInException, e.Message);
        //            }
        //            try
        //            {
        //                UpdateNWFilePropertiesByNative(fileUnit);
        //            }
        //            catch (Exception e)
        //            {
        //                log.Warn("Update the nintex workflow file properties failed by native. Error message:{0}.", e);
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_UpdateTempFilePropsUnknownException, e.Message);
        //        }
        //        finally
        //        {
        //            SPWorkflowProcessorRuntime.Log(Logs.MonitorScopeLeave, "UpdateNWFileProperties:" + fileUnit.SerializableData.mName);
        //        }
        //    }
        //}

        //private void UpdateNWFilePropertiesByNative(SPWorkflowSubFileUnit fileUnit)
        //{
        //    try
        //    {
        //        //由于老数据未备份相关数据，之前判断return，不还原。
        //        if (fileUnit.SerializableData.mUIVersion == 0 || string.IsNullOrEmpty(fileUnit.SerializableData.mAuthorLogin) || string.IsNullOrEmpty(fileUnit.SerializableData.mEditorLogin)
        //            || fileUnit.SerializableData.mCreated == DateTime.MinValue || fileUnit.SerializableData.mModified == DateTime.MinValue)
        //        {
        //            log.Log(AveLogLevel.WARN, "The backup data is old,so it do not need to update some workflow fields by native.");
        //            return;
        //        }

        //        var file = fileUnit.mSPFile;
        //        using (IAveBackupRestoreQueryService queryService = AveObjectModelFactory.CreateObjectModelFactory(null, null, AveContextKind.Auto).CreateQueryService<IAveBackupRestoreQueryService>(file.ParentFolder.ParentWeb.Site))
        //        {
        //            var properties = new Dictionary<string, object>();
        //            properties.Add("TimeCreated", fileUnit.SerializableData.mCreated);
        //            properties.Add("TimeLastModified", fileUnit.SerializableData.mModified);
        //            properties.Add("UIVersion", fileUnit.SerializableData.mUIVersion);
        //            var author = SPWorkflowProcessorRuntime.OnUserMapping(fileUnit.SerializableData.mAuthorLogin);
        //            if (author != null)
        //            {
        //                properties.Add("tp_Author", author.ID);
        //                if (file.ParentFolder.ParentList.Fields.ContainsField("Created_x0020_By"))
        //                {
        //                    properties.Add("Created_x0020_By", new KeyValuePair<string, string>(file.ParentFolder.ParentList.Fields.GetField("Created_x0020_By").ColName, author.LoginName));
        //                }
        //            }
        //            else
        //            {
        //                log.Log(AveLogLevel.WARN, "Cannot get the Nintex workflow's author by the backup user's login.");
        //            }
        //            var editor = SPWorkflowProcessorRuntime.OnUserMapping(fileUnit.SerializableData.mEditorLogin);
        //            if (editor != null)
        //            {
        //                properties.Add("tp_Editor", editor.ID);
        //                if (file.ParentFolder.ParentList.Fields.ContainsField("Modified_x0020_By"))
        //                {
        //                    properties.Add("Modified_x0020_By", new KeyValuePair<string, string>(file.ParentFolder.ParentList.Fields.GetField("Modified_x0020_By").ColName, editor.LoginName));
        //                }
        //            }
        //            else
        //            {
        //                log.Log(AveLogLevel.WARN, "Cannot get the Nintex workflow's editor by the backup user's login.");
        //            }                                                     
        //            queryService.UpdateNintexWorkflowFileProperties(file.ParentFolder.ParentWeb.Site.ID, file.ParentFolder.UniqueId, file.UniqueId, (byte)file.Item.Level, properties);
        //            log.Log(AveLogLevel.INFO, "Update the workflow template file fields by native succussfully.");
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        log.Log(AveLogLevel.WARN, "An error occurred while updating some workflow fields by native.Message:{0}", e.ToString());
        //    }
        //}

        private void AddParameterToCollection(Dictionary<int, NintexActivityMemberInfo> collection, int index, string parameter)
        {
            NintexActivityMemberInfo paramInfo = null;
            if (collection.ContainsKey(index))
            {
                paramInfo = collection[index];
            }
            else
            {
                paramInfo = new NintexActivityMemberInfo();
                collection.Add(index, paramInfo);
            }
            if (!paramInfo.Parameters.Contains(parameter))
                paramInfo.Parameters.Add(parameter);
        }
    }

    internal class NintexActivityMemberInfo
    {
        internal bool Flag;
        internal List<string> Parameters = new List<string>();
    }
}
