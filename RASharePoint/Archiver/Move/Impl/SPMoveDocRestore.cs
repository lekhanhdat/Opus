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

using AvePoint.Cryptography;
using AvePoint.GCommon;
using AvePoint.RA.Common.Util;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.RA.SharePoint.RMExplorer;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Restore;
using Microsoft.SharePoint.Client;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Xml.Linq;

namespace AvePoint.RA.SharePoint.Archiver.Move
{
    public class SPMoveDocRestore : AveSPDoc, IDisposable
    {
        AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        ScheduleConfiguration mConfig;
        IAveRestoreStream importStream;
        AveSPSite aveSPSite;
        AveSPWeb aveSPWeb;
        Guid aveSPWebId;
        AveSPList aveSPList;
        //暴露此属性，为了调用Wrapper方法传值.
        public AveSPFolder aveSPFolder;
        Guid aveSPListId;
        IAveSite site;
        IAveWeb web;
        IAveList list;
        IAveFolder currentIAveFolder;
        IAveORecords records;
        Guid aveSPFolderId;
        string currentSubFolderUrl;
        private DateTime mInitialTime = DateTime.MinValue;//用于记录Site的生存时间
        private string mSiteUrl = string.Empty;
        private bool isKeepFolderStructure = false;
        private bool AutoDeclareRecordsChange = false;
        private string revertRecordDeclarationSetting = string.Empty;
        private readonly Guid BCSColumnID = new Guid("20f84bba906045b4af568ee102a52dcb");
        public SPMoveDocRestore(IAveRestoreStream stream, ScheduleConfiguration config)
        {
            importStream = stream;
            mConfig = config;
        }

        public SPMoveDocRestore()
        {

        }

        public void Init(IAveRestoreStream stream, ScheduleConfiguration config, bool isKeepFolderStructure)
        {
            importStream = stream;
            mConfig = config;
            this.isKeepFolderStructure = isKeepFolderStructure;
        }

        public IAveORecords Record
        {
            get
            {
                if (records == null)
                {

                    records = mConfig.recordManagerRestoreOMFactory.CreateRecords();
                }
                return records;
            }
        }

        public bool IsLockFileByRecordLabel(IAveListItem item)
        {
            mLog.Info("Start check current file hold by record label");
            try
            {
                var currentSite = item.Web.Site;
                var availableTags = currentSite.GetAvailableTagsForSite().ToDictionary(_ => _.TagName);
                var currentLabelOfItem = item.GetComplianceTagName();
                mLog.Info($"Current record label of file is {currentLabelOfItem}");
                if (availableTags.TryGetValue(currentLabelOfItem, out var tagInfo))
                {
                    if (tagInfo.BlockDelete && tagInfo.BlockEdit) return true;
                }
            }
            catch (Exception e)
            {
                mLog.Error($"IsLockFileByRecordLabel has error {e}");
            }
            return false;
        }

        public void AddRecordLabel(string fileName)
        {
            using (AvePerformanceScope performanceRestore = new AvePerformanceScope("SPMoveDocRestore.DeclareItem"))
            {
                string fileFullPath = string.Empty;
                if (isKeepFolderStructure && !string.IsNullOrEmpty(mConfig.subFolderUrl))
                {
                    fileFullPath = $"{list.RootFolder.ServerRelativeUrl.TrimEnd('/')}/{mConfig.subFolderUrl.Trim('/')}/{fileName}";
                }
                else
                {
                    fileFullPath = $"{list.RootFolder.ServerRelativeUrl.TrimEnd('/')}/{fileName}";
                }

                IAveFile file = aveSPWeb.SPWeb.GetFile(fileFullPath);
                mLog.Info("Declare file is:{0}.", file.UniqueId);
                //add for Office 365,destination file is checked out
                if (this.mConfig.recordManagerRestoreOMFactory != null && this.mConfig.recordManagerRestoreOMFactory.ContextKind == AveContextKind.ClientObjectModel)
                {
                    try
                    {
                        if (file.CheckedOutByUser != null && file.CheckedOutByUser.ID > 0)
                        {
                            mLog.Info("Destination file is checked out.In order to declare,file must be checked in.File Url:{0}", file.ServerRelativeUrl);
                            file.CheckIn("");
                        }
                    }
                    catch (Exception e)
                    {
                        mLog.Warn("An error occurred while checking in file.File Url:{0} Error:{1}", file.ServerRelativeUrl, e.ToString());
                    }
                }
                try
                {
                    string recordLabel = this.mConfig.GeneralRetentionLabel;
                    if (string.IsNullOrEmpty(recordLabel))
                    {
                        throw new Exception("StorageOptimization_SOARRecordLabelDoesNotSetValue");
                    }
                    IAveListItem item = file.Item;
                    var currentSite = item.Web.Site;
                    var availableTagInfo = currentSite.GetAvailableTagsForSite().ToDictionary(_ => _.TagName);
                    if (availableTagInfo.TryGetValue(recordLabel, out var tagInfo))
                    {
                        if(tagInfo.BlockDelete && tagInfo.BlockEdit)
                        {
                            item.SetComplianceTag(tagInfo.TagName, true, true, false, false);
                        }
                        else
                        {
                            throw new Exception("StorageOptimization_SOARCurrentLabelIsNotRecordLabel");
                        }
                    }
                    else
                    {
                        throw new Exception("StorageOptimization_SOARCanNotFindCurrentRecordLabelInSite");
                    }
                }
                catch (Exception ex)
                {
                    mLog.Info(string.Format("Add record item Failed, Reason : {0}", ex.ToString()));
                    throw;
                }
            }
        }

        public void DeleteRecordLabel(IAveListItem item)
        {
            using (AvePerformanceScope performanceRestore = new AvePerformanceScope("SPMoveDocRestore.DeleteRecordLabel"))
            {
                try
                {
                    var currentSite = item.Web.Site;
                    var availableTagInfo = currentSite.GetAvailableTagsForSite().ToDictionary(_ => _.TagName);
                    var currentRecordLabelNameOfItem = item.GetComplianceTagName();
                    if (availableTagInfo.TryGetValue(currentRecordLabelNameOfItem, out var tagInfo))
                    {
                        if (tagInfo.BlockDelete && tagInfo.BlockEdit)
                        {
                            mLog.Info($"Remove current record label of file {item.UniqueId}");
                            item.SetComplianceTagOnBulkItems("");
                        }
                        else
                        {
                            mLog.Info($"Current label of file is not record label {item.UniqueId}");
                        }
                    }
                    else
                    {
                        mLog.Info($"Can not file current record label of file in site {item.UniqueId}");
                    }
                }
                catch (Exception ex)
                {
                    mLog.Info("An error occur while Undo Declare Item,Item Name:{0},Message:{1}.", item.Name, ex.ToString());
                    throw;
                }
            }
        }

        public void DeclareItem(string fileName,bool needCheckStatus = false)
        {
            using (AvePerformanceScope performanceRestore = new AvePerformanceScope("SPMoveDocRestore.DeclareItem"))
            {
                string fileFullPath = string.Empty;
                if (isKeepFolderStructure && !string.IsNullOrEmpty(mConfig.subFolderUrl))
                {
                    fileFullPath = $"{list.RootFolder.ServerRelativeUrl.TrimEnd('/')}/{mConfig.subFolderUrl.Trim('/')}/{fileName}";
                }
                else
                {
                    fileFullPath = $"{list.RootFolder.ServerRelativeUrl.TrimEnd('/')}/{fileName}";
                }

                IAveFile file = aveSPWeb.SPWeb.GetFile(fileFullPath);
                mLog.Info("Declare file is:{0}.", file.UniqueId);
                //add for Office 365,destination file is checked out
                if (this.mConfig.recordManagerRestoreOMFactory != null && this.mConfig.recordManagerRestoreOMFactory.ContextKind == AveContextKind.ClientObjectModel)
                {
                    try
                    {
                        if (file.CheckedOutByUser != null && file.CheckedOutByUser.ID > 0)
                        {
                            mLog.Info("Destination file is checked out.In order to declare,file must be checked in.File Url:{0}", file.ServerRelativeUrl);
                            file.CheckIn("");
                        }
                    }
                    catch (Exception e)
                    {
                        mLog.Warn("An error occurred while checking in file.File Url:{0} Error:{1}", file.ServerRelativeUrl, e.ToString());
                    }
                }
                try
                {
                    IAveListItem item = file.Item;
                    if (!needCheckStatus || !ArchiverCommonStaticMethod.CheckisRecord(item))
                    {
                        Record.DeclareItemAsRecord(item);
                    }
                }
                catch (Exception ex)
                {
                    mLog.Info(string.Format("Declare item Failed, Reason : {0}", ex.ToString()));
                    if (ex.InnerException != null && ex.InnerException.Message.Contains("This item cannot be declared a record because it is checked out"))
                    {
                        throw new CheckOutDocumentDeclareException("");
                    }
                    else if (ex.InnerException != null && ex.InnerException.Message.Contains("This item cannot be declared a record because it is a Folder content type"))
                    {
                        throw new DocumentSetContentTypeFileDeclareException("");
                    }
                    throw;
                }
            }
        }

        public void UnDeclareItem(IAveListItem item)
        {
            using (AvePerformanceScope performanceRestore = new AvePerformanceScope("SPMoveDocRestore.UnDeclareItem"))
            {
                try
                {
                    Record.UndeclareItemAsRecord(item);
                    mLog.Info("Undo Declare Item Success,Item Name:{0}.", item.UniqueId);
                }
                catch (Exception ex)
                {
                    mLog.Info("An error occur while Undo Declare Item,Item Name:{0},Message:{1}.", item.Name, ex.ToString());
                    throw;
                }
            }
        }

        public void ModifySiteRecordDeclarationSetting()
        {
            using (AvePerformanceScope performanceRestore = new AvePerformanceScope("SPMoveDocRestore.ModifySiteRecordDeclarationSetting"))
            {
                try
                {
                    if (site.RootWeb.AllProperties.ContainsKey("ecm_siterecordrestrictions") && site.RootWeb.AllProperties["ecm_siterecordrestrictions"].ToString() != ScheduleConfiguration.BlockDelete)
                    {
                        revertRecordDeclarationSetting = site.RootWeb.AllProperties["ecm_siterecordrestrictions"].ToString();
                        site.RootWeb.AllProperties["ecm_siterecordrestrictions"] = ScheduleConfiguration.BlockDelete;
                        site.RootWeb.Update();
                    }
                }
                catch (Exception ex)
                {
                    mLog.Info("An error occur while ModifySiteRecordDeclarationSetting.Message:{0}.", ex.ToString());
                }
            }
        }


        public void RevertSiteRecordDeclarationSetting()
        {
            using (AvePerformanceScope performanceRestore = new AvePerformanceScope("SPMoveDocRestore.RevertSiteRecordDeclarationSetting"))
            {
                try
                {
                    if (site.RootWeb.AllProperties.ContainsKey("ecm_siterecordrestrictions") && revertRecordDeclarationSetting != string.Empty)
                    {
                        site.RootWeb.AllProperties["ecm_siterecordrestrictions"] = revertRecordDeclarationSetting;
                        site.RootWeb.Update();
                    }
                }
                catch (Exception ex)
                {
                    mLog.Info("An error occur while RevertSiteRecordDeclarationSetting.Message:{0}.", ex.ToString());
                }
            }
        }

        public IAveFolder EnsureFolder(string folderUrl)
        {
            using (AvePerformanceScope performanceRestore = new AvePerformanceScope("SPMoveDocRestore.EnsureFolder"))
            {
                if (currentIAveFolder == null || !folderUrl.Equals(currentIAveFolder.ParentWeb.Url + "/" + currentIAveFolder.Url))
                {
                    currentIAveFolder = web.GetFolder(folderUrl);
                    if (!currentIAveFolder.Exists)
                    {
                        throw new Exception(string.Format("Folder Not Exists :{0}", currentIAveFolder.Name));
                    }
                }
                return currentIAveFolder;
            }
        }

        public void RestoreParentInfo(string listUrl)
        {
            using (AvePerformanceScope performanceRestore = new AvePerformanceScope("SPMoveDocRestore.RestoreParentInfo"))
            {
                AveBPOSAccountInfo user = null;
                string siteUrl = string.Empty;
                AveObjectModelFactory factory = null;
                siteUrl = mConfig.siteUrl;
                user = mConfig.user;
                factory = mConfig.recordManagerRestoreOMFactory;

                if (site == null)
                {
                    mInitialTime = DateTime.Now;
                    // 重新实例化site 对象，必须释放aveSPSite，并且把aveSPSite 置空，保证能走到restore site 逻辑中
                    if (aveSPSite != null)
                    {
                        aveSPSite.Dispose();
                        aveSPSite = null;
                        aveSPWeb = null;
                        aveSPList = null;
                        aveSPFolder = null;
                        currentIAveFolder = null;
                    }
                    site = factory.CreateSite(siteUrl);
                    mSiteUrl = siteUrl;
                    mConfig.DestinationIsOneDriveSite = mConfig.GetRemoteSiteCollectionByDAO(mSiteUrl) == null ? false : mConfig.GetRemoteSiteCollectionByDAO(mSiteUrl).NodeType == GCommon.Contract.Server.ControlPanel.Office365.RemoveNodeType.SkyDrivePro;
                    web = site.OpenWeb(site.GetWebServerRelativeUrl(listUrl));
                }
                else if ((string.Compare(siteUrl, mSiteUrl, StringComparison.OrdinalIgnoreCase) != 0)
                            || mInitialTime.AddHours(23) < DateTime.Now)
                {
                    site.Dispose();
                    // 重新实例化site 对象，必须释放aveSPSite，并且把aveSPSite 置空，保证能走到restore site 逻辑中
                    if (aveSPSite != null)
                    {
                        SetAutoDeclareRecordsTrue();
                        aveSPSite.Dispose();
                        aveSPSite = null;
                        aveSPWeb = null;
                        aveSPList = null;
                        aveSPFolder = null;
                        currentIAveFolder = null;
                    }
                    mInitialTime = DateTime.Now;
                    site = factory.CreateSite(siteUrl);
                    mSiteUrl = siteUrl;
                    mConfig.DestinationIsOneDriveSite = mConfig.GetRemoteSiteCollectionByDAO(mSiteUrl) == null ? false : mConfig.GetRemoteSiteCollectionByDAO(mSiteUrl).NodeType == GCommon.Contract.Server.ControlPanel.Office365.RemoveNodeType.SkyDrivePro;
                    
                    web = site.OpenWeb(site.GetWebServerRelativeUrl(listUrl));
                }

                if (listUrl.Contains("#/"))
                {
                    //list = web.GetListFromUrl(desUrl.Substring(desUrl.IndexOf("#/", StringComparison.OrdinalIgnoreCase) + 2));
                    listUrl = listUrl.Substring(listUrl.IndexOf("#/", StringComparison.OrdinalIgnoreCase) + 2);
                }
                list = web.GetList(listUrl);
               //string listUrl = list.ParentWeb.Url + "/" + list.RootFolder.Url;
                EnsureFolder(list.RootFolder.ServerRelativeUrl);
                RestoreSiteInfo(site, user);
                RestoreWebInfo();
                RestoreListInfo();
                if (isKeepFolderStructure)
                {
                    //KeepFolderStructure 仅用来Archiver Move操作，由于是单线程，因此此处使用全局变量没有问题.
                    RestoreFolderInfo(mConfig.subFolderUrl);
                }
                else
                {
                    RestoreFolderInfo();
                }
            }
        }
        /// <summary>
        /// destFolderUrl  e.g: dest url substring list url
        /// </summary>
        /// <param name="parentFolder"></param>
        /// <param name="destFolderUrl"></param>
        /// <returns></returns>
        private AveSPFolder GetSubSPFolder(AveSPFolder rootFolder, string destFolderUrl)
        {
            if (string.IsNullOrEmpty(destFolderUrl))
            {
                return rootFolder;
            }
            if (!destFolderUrl.Contains("/"))
            {
                AveSPFolder subFolder = new AveSPFolder(rootFolder, destFolderUrl);
                subFolder.InitSPFolder();
                return subFolder;
            }
            int pos = destFolderUrl.IndexOf("/");
            if (pos > -1)
            {
                string subDest = destFolderUrl.Substring(0, pos);
                string subLastDest = destFolderUrl.Substring(pos + 1);
                AveSPFolder subFolder = new AveSPFolder(rootFolder, subDest);
                subFolder.InitSPFolder();
                return this.GetSubSPFolder(subFolder, subLastDest);
            }
            return rootFolder;
        }

        private void RestoreFolderInfo(string destSubFolderUrl)
        {
            if (aveSPFolder == null || aveSPFolderId == null || !aveSPFolder.ParentList.RootFolder.UniqueId.Equals(aveSPList.RootFolder.UniqueId)
                || string.IsNullOrEmpty(currentSubFolderUrl) || !destSubFolderUrl.Equals(currentSubFolderUrl, StringComparison.OrdinalIgnoreCase))
            {
                mLog.Info("RestoreFolderInfo destSubFolderUrl:{0}.", destSubFolderUrl);
                aveSPFolder = new AveSPFolder(aveSPList, currentIAveFolder.Name);
                AveSPFolder subFolder = GetSubSPFolder(aveSPFolder, destSubFolderUrl);
                currentSubFolderUrl = destSubFolderUrl;
                aveSPFolder = subFolder;// add this locgic because using archive restore RM Data. Make it better in next version. 
                aveSPFolderId = subFolder.SPFolder.UniqueId;
                if (!aveSPList.Url.EndsWith(subFolder.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
                {
                    //aveSPRootFolder.ImportParentFolder(importStream);
                    importStream.Reset();
                }

            }
            else if (!aveSPList.Url.EndsWith(aveSPFolder.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
            {
                // var folderInfo = importStream.ReadMetadata().GetMetadata<SPFolderMetadataDto>();
                importStream.Reset();
            }
        }


        public void RestoreFileXML(string fileName, string xmlString)
        {
            mLog.Info("Move action restore file XML.");
            byte[] bytes = Encoding.UTF8.GetBytes(xmlString);
            string fileUrl = string.Empty;
            IAveFile file = null;
            if (isKeepFolderStructure && !string.IsNullOrEmpty(mConfig.subFolderUrl))
            {
                fileUrl = list.RootFolder.ServerRelativeUrl.TrimEnd('/') + "/" + mConfig.subFolderUrl.TrimEnd('/') + "/" + fileName;
                file = list.GetFolder(list.RootFolder.ServerRelativeUrl.TrimEnd('/') + "/" + mConfig.subFolderUrl.TrimEnd('/')).Files.Add(fileUrl, bytes, true);
            }
            else
            {
                fileUrl = list.RootFolder.ServerRelativeUrl.TrimEnd('/') + "/" + fileName;
                file = list.RootFolder.Files.Add(fileUrl, bytes, true);
            }
            try
            {
                //app profile auth can't get checkoutuser.
                file.CheckIn("");//对Check Out的File进行并且check out User被删除的文件,CheckIn need comment
                mLog.Info("Move action restore file XML CheckIn success.");
            }
            catch (Exception e)
            {
                mLog.Warn(string.Format("File {0} CheckIn RestoreFileXML Failed. Error:{1}.", fileName, e.Message));
            }
            mLog.Info("Move action restore file XML success.");
        }

        public void RestoreSiteInfo(IAveSite site, AveBPOSAccountInfo user)
        {
            using (AvePerformanceScope performanceRestore = new AvePerformanceScope("SPMoveDocRestore.RestoreSiteInfo"))
            {
                var siteInfo = importStream.ReadMetadata().GetMetadata<AveSiteInfo>();
                if (aveSPSite == null)
                {
                    if (user != null)//(site.IsOnlineSite)
                    {
                        aveSPSite = new AveSPSite(site.Url, site.Url, AveContextKind.ClientObjectModel, user);
                    }
                    else
                    {
                        aveSPSite = new AveSPSite(site.Url, site.Url, AveContextKind.Server13ObjectModel, null);
                    }
                    aveSPSite.RestoreSiteSelf(siteInfo);
                }
                importStream.Reset();
            }
        }

        private void RestoreWebInfo()
        {
            using (AvePerformanceScope performanceRestore = new AvePerformanceScope("SPMoveDocRestore.RestoreWebInfo"))
            {
                var webInfo = importStream.ReadMetadata().GetMetadata<AveWebInfo>();
                if (aveSPWeb == null || aveSPWebId == null || aveSPWebId != currentIAveFolder.ParentWeb.ID)
                {
                    aveSPWeb = new AveSPWeb(aveSPSite, web.ServerRelativeUrl);
                    aveSPWebId = web.ID;
                    aveSPWeb.RestoreWebSelf(webInfo);
                }
                importStream.Reset();
            }
        }

        private void RestoreListInfo()
        {
            using (AvePerformanceScope performanceRestore = new AvePerformanceScope("SPMoveDocRestore.RestoreListInfo"))
            {
                var listInfo = importStream.ReadMetadata().GetMetadata<AveListInfo>();
                var fieldXML = importStream.ReadMetadata().GetMetadata<string>();
                var contentTypeInfo = importStream.ReadMetadata().GetMetadata<AveContentTypeCollectionInfo>();
                if (aveSPList == null || aveSPListId == null || aveSPListId != list.ID)
                {
                    if (aveSPList != null)
                    {
                        SetAutoDeclareRecordsTrue();
                        AvePostAction.ListPostAction(aveSPList);
                    }
                    aveSPList = new AveSPList(aveSPWeb, list.Title);
                    //change list title to find the right list  //SAAS-29158 RECO-348
                    listInfo.Title = list.Title;
                    listInfo.ServerRelativeUrl = list.RootFolder.ServerRelativeUrl;
                    aveSPListId = list.ID;
                    listInfo.RootWebOnly = false;
                    aveSPList.RestoreListSelf(listInfo);
                    try
                    {
                        aveSPList.BackupListSetting();
                    }
                    catch (Exception ex)
                    {
                        mLog.Info("Exception in Backup List Setting for Record Manager job,Message: {0}.", ex.ToString());
                    }
                    if (aveSPList.RootFolder.Properties.ContainsKey("ecm_AutoDeclareRecords") && aveSPList.RootFolder.Properties["ecm_AutoDeclareRecords"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase))
                    {
                        mLog.Info("List ecm_AutoDeclareRecords is true and set false.ListUrl: {0}.", list.Title);
                        SetAutoDeclareRecordsFalse();
                    }
                }
                //SAAS-15676  由于结构原因导致Field和ContentType每次job只Reload一次,如果原端list改变则需要重新load
                if (listInfo.Id != mConfig.tempListId)
                {
                    if (mConfig.IsILMode)
                    {
                        var recordsTax = aveSPList.SPList.Fields.GetFieldById(BCSColumnID, false);
                        if (recordsTax != null)
                        {
                            var textField = recordsTax as IAveTaxonomyField;
                            if (!aveSPList.AveFields.RestoredFieldInternalNameList.Contains(textField.TextField.ToString()))
                            {
                                aveSPList.AveFields.RestoredFieldInternalNameList.Add(textField.TextField.ToString());
                            }
                            if (!aveSPList.AveFields.RestoredFieldInternalNameList.Contains("RevIMBCS"))
                            {
                                aveSPList.AveFields.RestoredFieldInternalNameList.Add("RevIMBCS");
                            }
                        }
                    }
                    aveSPList.AveFields.RestoreFields(fieldXML);
                    aveSPList.AveContentTypes.LoadContentTypes(contentTypeInfo);
                    mConfig.tempListId = listInfo.Id;
                }
                importStream.Reset();
            }
        }

        private void RestoreFolderInfo()
        {
            using (AvePerformanceScope performanceRestore = new AvePerformanceScope("SPMoveDocRestore.RestoreFolderInfo"))
            {
                if (aveSPFolder == null || aveSPFolderId == null || aveSPFolderId != currentIAveFolder.ParentList.RootFolder.UniqueId)
                {
                    aveSPFolder = new AveSPFolder(aveSPList, currentIAveFolder.Name);
                    aveSPFolderId = currentIAveFolder.UniqueId;
                    if (!aveSPList.Url.EndsWith(aveSPFolder.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        //aveSPRootFolder.ImportParentFolder(importStream);
                        importStream.Reset();
                    }
                }
                else if (!aveSPList.Url.EndsWith(aveSPFolder.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
                {
                    var folderInfo = importStream.ReadMetadata().GetMetadata<AveSPFolderMetadataDto>();
                    importStream.Reset();
                }
            }
        }

        public Record GetDesFileRecord(string fileName)
        {
            using (AvePerformanceScope performanceRestore = new AvePerformanceScope("SPMoveDocRestore.GetDesFileRecord"))
            {
                Record desDto = new Record();
                try
                {
                    var daoSite = mConfig.IsILMode ? mConfig.GetRemoteSiteCollectionByRecords(web.Site.Url) : mConfig.GetRemoteSiteCollectionByDAO(web.Site.Url);
                    IAveFile file = aveSPWeb.SPWeb.GetFile(list.RootFolder.ServerRelativeUrl.TrimEnd('/') + "/" + fileName);
                    desDto.ScopeId = web.Site.ID;
                    desDto.NodeId = file.UniqueId;
                    desDto.DirPath = file.ServerRelativeUrl;
                    desDto.NodeType = 500;
                    desDto.LeafName = file.Name;
                    desDto.RuleId = Guid.Parse(mConfig.currentRule.Id);
                    desDto.TimeModified = file.TimeLastModified.Ticks;
                    desDto.TimeCreated = file.TimeCreated.Ticks;
                    desDto.WebId = file.ParentFolder.ParentList.ParentWeb.ID;
                    desDto.ListId = file.ParentFolder.ParentList.ID;
                    desDto.FolderId = file.ParentFolder.UniqueId;
                    desDto.ItemId = file.UniqueId;
                    desDto.ItemRowId = file.Item.ID;
                    desDto.FullPath = new Uri(web.Site.Url).Scheme + @"://" + new Uri(web.Site.Url).Authority + file.ServerRelativeUrl.Replace("\\", "/");
                    desDto.Id = ArchiverCommonStaticMethod.GetRecordId(web.Site.ID, desDto.NodeId);
                    desDto.SourceFlag = daoSite == null ? (int)SOSourceFlag.SharePoint
                        : daoSite.NodeType == GCommon.Contract.Server.ControlPanel.Office365.RemoveNodeType.SkyDrivePro ? (int)SOSourceFlag.OneDrive
                        : !string.IsNullOrEmpty(daoSite.TeamId) ? (int)SOSourceFlag.Teams
                        : (int)SOSourceFlag.SharePoint;
                    desDto.ParentId = file.ParentFolder == null ? Guid.Empty : file.ParentFolder.UniqueId;
                    //RECO - 3615, RECO-3616 当前版本，Move行为仍然不去管所有属性，依赖后期data sync行为。所以create by modified by 还从sourceRecord 获取。
                    //desDto.CreatedBy = GetFileCreatedBy(file.Item);
                    desDto.DeclareAsRecord = ArchiverCommonStaticMethod.CheckisRecord(file.Item) && ArchiverCommonStaticMethod.IsBlockEditAndDeleteRecord(file.Item);
                    desDto.LockedByRecordLabel = ArchiverCommonStaticMethod.IsHaveRecordLabel(file.Item);
                    if (daoSite != null)
                    {
                        desDto.AveSiteId = daoSite.id;
                    }
                    else
                    {
                        mLog.Info("Can't get DAO SiteID:{0}.", web.Site.Url);
                    }
                    if (desDto.SourceFlag == (int)SOSourceFlag.SharePoint || desDto.SourceFlag == (int)SOSourceFlag.Teams)
                    {
                        var field = file.Item.Fields.GetFieldById(new Guid("20f84bba906045b4af568ee102a52dcb"), false);
                        if (field != null)
                        {
                            string termID = file.Item[field.ID].ToString();
                            if (termID.IndexOf('|') != 0)
                            {
                                desDto.TermId = new Guid(termID.Substring(termID.IndexOf('|') + 1));
                                desDto.TermName = termID.Substring(0, termID.IndexOf('|'));
                            }
                            else
                            {
                                desDto.TermId = new Guid(termID);
                            }
                        }
                    }
                }
                catch (Exception exceptionInGetProperty)
                {
                    mLog.Error(string.Format("Error in get records document properties, reason : {0}", exceptionInGetProperty.ToString()));
                }
                return desDto;
            }
        }

        public string UpdateClassificationColumn(string fileName, Guid termId)
        {
            string errorMessage = string.Empty;
            var colSetting = mConfig.GetDestinationColumnSetting(aveSPSite.SiteUrl);
            if (colSetting.Exist)
            {
                var bscField = GetBCSField(colSetting.UseExisting, colSetting.ColumnName);
                if (bscField == null)
                {
                    return ArchiverErrorMessage.BCSFieldNotFoundString;
                    //"StorageOptimization_SOARRecordManagerEXOListBCSNotExist";
                }
                var term = aveSPSite.SPSite.AveSPTaxonomySession.GetTerm(termId);
                if (term == null)
                {
                    return ArchiverErrorMessage.TermNotExistString;
                    //"StorageOptimization_SOARRecordManagerEXOSourceTermNotExist";
                }
                if (!InSameTermScope(termId, bscField))
                {
                    return ArchiverErrorMessage.NotUnderTermScopeString;
                    //"StorageOptimization_SOARRecordManagerEXONotInSameTermScope";
                }
                var fullUrl = aveSPFolder.ServerRelativeUrl.TrimEnd('/') + "/" + fileName;
                IAveFile desFile = aveSPWeb.SPWeb.GetFile(fullUrl);
                mLog.Info("File Exists in destination,file Name:{0}.", desFile.UniqueId);
                try
                {
                    var item = desFile.Item;
                    item[bscField.ID] = termId;
                    item[bscField.TextField] = term.Name;
                    item.SystemUpdate();
                    mLog.Info("Update destination file property successful.File Name:{0}.", desFile.UniqueId);
                }
                catch (Exception e)
                {
                    mLog.Info("Failed to update property for current file,Name:{0},Message{1}.", desFile.UniqueId, e.ToString());
                    errorMessage = "StorageOptimization_SOARRecordManagerEXOKeepClassificationFailed";
                }
            }
            else
            {
                return ArchiverErrorMessage.TermSettingNotFoundString;
                //"StorageOptimization_SOARRecordManagerEXOTermSettingNotFound";
            }
            return errorMessage;
        }

        public Tuple<Guid, string> UpdateClassificationColumnWithDestination(string fileName, bool forceSetNull = false)
        {
            string termName = string.Empty;
            Guid termId = Guid.Empty;
            try
            {
                var colSetting = mConfig.GetDestinationColumnSetting(aveSPSite.SiteUrl);
                if (colSetting.Exist)
                {
                    var fullUrl = aveSPFolder.ServerRelativeUrl.TrimEnd('/') + "/" + fileName;
                    IAveFile desFile = aveSPWeb.SPWeb.GetFile(fullUrl);
                    mLog.Info("File Exists in destination, file:{0}. exist:{1}", desFile.UniqueId, desFile.Exists);
                    var listSetting = GetListTermSetting(colSetting.UseExisting, colSetting.ColumnName);
                    if (!listSetting.HasDefaultTermValue || forceSetNull)
                    {
                        //目的端list没有default term value，当时目的端文件有term，需要将termid更新为空
                        try
                        {
                            var item = desFile.Item;
                            if (item.Fields.ContainsField(colSetting.ColumnName))
                            {
                                item[listSetting.FieldId] = null;
                                item[listSetting.TextFieldId] = null;
                                item.SystemUpdate();
                                mLog.Info("Update destination file property to null successful.File:{0}.", desFile.UniqueId);
                            }
                        }
                        catch (Exception e)
                        {
                            mLog.Warn("An error occurred while updating destination file bcs column to empty. File:{0} Error:{1}", desFile.Name, e.ToString());
                        }
                    }
                    else
                    {
                        //目的端list有default term value，将目的端文件bcs column更新为default value
                        try
                        {
                            var item = desFile.Item;
                            item[listSetting.FieldId] = listSetting.DefautTermId;
                            item[listSetting.TextFieldId] = listSetting.DefaultTermName;
                            item.SystemUpdate();
                            mLog.Info("Update destination file property successful.File Name:{0}.", desFile.UniqueId);
                            termId = listSetting.DefautTermId;
                            termName = listSetting.DefaultTermName;
                        }
                        catch (Exception e)
                        {
                            mLog.Info("Failed to update property for current file,Name:{0},Message{1}.", desFile.Name, e.ToString());
                        }
                    }
                }
                else
                {
                    mLog.Info($"Can not find DestinationColumnSetting for {aveSPSite.SiteUrl}");
                }
            }
            catch (Exception e)
            {
                mLog.Warn($"Erro occurred while updating destination bcs column.Name:{0},Message{1}", fileName, e.ToString());
            }
            return new Tuple<Guid, string>(termId, termName);
        }

        private ArchiverCommon.DestinationListTermSetting GetListTermSetting(bool useExisting, string columnName)
        {
            Guid listId = list.ID;
            if (!mConfig.DestinationListTermSettingCache.ContainsKey(listId))
            {
                var bscField = GetBCSField(useExisting, columnName);

                mLog.Info("Field={0}", bscField.InternalName);
                mLog.Info("DefaultValue=[{0}]", bscField.DefaultValue);
                if (bscField != null)
                {
                    string defaultValueStr = bscField.DefaultValue;

                    if (string.IsNullOrWhiteSpace(defaultValueStr) || defaultValueStr.IndexOf('|') <= 0)
                    {
                        defaultValueStr = GetFolderLevelTaxonomyDefaultValue(list.RootFolder, bscField.InternalName);
                    }

                    if (!string.IsNullOrWhiteSpace(defaultValueStr) && defaultValueStr.IndexOf('|') > 0)
                    {
                        var termId = new Guid(defaultValueStr.Substring(defaultValueStr.IndexOf('|') + 1));
                        var startIndex = defaultValueStr.IndexOf(";#");
                        var endIndex = defaultValueStr.IndexOf('|');
                        var termName = defaultValueStr.Substring(startIndex + 2, endIndex - startIndex);
                        mConfig.DestinationListTermSettingCache.TryAdd(listId,
                            new ArchiverCommon.DestinationListTermSetting()
                            {
                                HasDefaultTermValue = true,
                                DefautTermId = termId,
                                DefaultTermName = termName,
                                FieldId = bscField.ID,
                                TextFieldId = bscField.TextField
                            });
                    }
                    else
                    {
                        mLog.Info("Destination list doesn't have term defaut value. List Url:{0} Term Default Value:{1}",
                            list.RootFolder.ServerRelativeUrl, defaultValueStr);
                        mConfig.DestinationListTermSettingCache.TryAdd(listId,
                            new ArchiverCommon.DestinationListTermSetting()
                            {
                                HasDefaultTermValue = false,
                                FieldId = bscField.ID,
                                TextFieldId = bscField.TextField
                            });
                    }
                }
            }
            return mConfig.DestinationListTermSettingCache[listId];
        }

        private string GetFolderLevelTaxonomyDefaultValue(IAveFolder folder, string fieldInternalName)
        {
            try
            {
                var formsFolderUrl = folder.ServerRelativeUrl.TrimEnd('/') + "/Forms";
                var destWeb = list.ParentWeb; 
                var lookupFile = destWeb.GetFile(
                    formsFolderUrl + "/client_LocationBasedDefaults.html");

                mLog.Info("Exists={0}", lookupFile.Exists);
                if (lookupFile == null || !lookupFile.Exists)
                {
                    mLog.Warn("client_LookupField.aspx not found or null. Path:{0}", formsFolderUrl);
                    return null;
                }
                using (var content = lookupFile.OpenBinaryStream())
                {
                    var xml = XDocument.Load(content);

                    var node = xml.Descendants("DefaultValue")
                                  .FirstOrDefault(n => (string)n.Attribute("FieldName") == fieldInternalName);
                    return node?.Value;
                }
            }
            catch (Exception e)
            {
                mLog.Warn("Failed to read folder-level taxonomy default value. Error:{0}", e.ToString());
                return null;
            }
        }
        private IAveTaxonomyField GetBCSField(bool useExisting, string columnName)
        {
            IAveTaxonomyField taxField = null;
            if (!useExisting)
            {
                var bcsColumn = list.Fields.GetFieldById(BCSColumnID, false);
                if (bcsColumn == null)
                {
                    var tempField = list.Fields.Where(f => f.Title == columnName).FirstOrDefault();
                    if (tempField != null)
                    {
                        taxField = tempField as IAveTaxonomyField;
                    }
                }
                else
                {
                    taxField = bcsColumn as IAveTaxonomyField;
                }
            }
            else
            {
                taxField = list.Fields.GetRecordTaxonomyField(columnName);
            }
            return taxField;
        }

        private bool InSameTermScope(Guid termId, IAveTaxonomyField field)
        {
            try
            {
                if (field.AnchorId == Guid.Empty)
                {
                    //term scope is termset
                    var sourceTermSet = aveSPSite.SPSite.AveSPTaxonomySession.GetTerm(termId).TermSet;
                    return sourceTermSet.ID.Equals(field.TermSetId) ? true : false;
                }
                else
                {
                    //term scope is term
                    var destinationTerm = aveSPSite.SPSite.AveSPTaxonomySession.GetTerm(field.AnchorId);
                    if (destinationTerm == null)
                    {
                        return false;
                    }
                    //check if in the same termset
                    var sourceTerm = aveSPSite.SPSite.AveSPTaxonomySession.GetTerm(termId);
                    if (!destinationTerm.TermSet.ID.Equals(sourceTerm.TermSet.ID))
                    {
                        return false;
                    }

                    //check path of term
                    return sourceTerm.PathOfTerm.StartsWith(destinationTerm.PathOfTerm + ";") ? true : false;
                }
            }
            catch (Exception e)
            {
                mLog.Warn($"An error occurred while checking same term group. Error{e.ToString()}");
            }
            return false;
        }

        //private EXOMoveDestinationInfo GetDestinationColumnSetting(string siteUrl)
        //{
        //    var setting = RecordsDBOperation.GetClassificationColumnNameFromRMSharePointSettingsTableBySiteUrl(siteUrl);
        //    EXOMoveDestinationInfo info = new EXOMoveDestinationInfo()
        //    {
        //        Exist = setting.Item1,
        //        UseExisting = setting.Item2,
        //        ColumnName = setting.Item3
        //    };
        //    return info;
        //}

        private void SetAutoDeclareRecordsTrue()
        {
            using (AvePerformanceScope performanceRestore = new AvePerformanceScope("SPMoveDocRestore.SetAutoDeclareRecordsTrue"))
            {
                if (AutoDeclareRecordsChange && aveSPList != null)
                {
                    aveSPList.RootFolder.Properties["ecm_AutoDeclareRecords"] = "True";
                    aveSPList.RootFolder.Update();
                    AutoDeclareRecordsChange = false;
                }
            }
        }

        private void SetAutoDeclareRecordsFalse()
        {
            using (AvePerformanceScope performanceRestore = new AvePerformanceScope("SPMoveDocRestore.SetAutoDeclareRecordsFalse"))
            {
                if (!AutoDeclareRecordsChange && aveSPList != null)
                {
                    aveSPList.RootFolder.Properties["ecm_AutoDeclareRecords"] = "False";
                    aveSPList.RootFolder.Update();
                    AutoDeclareRecordsChange = true;
                }
            }
        }

        public void Dispose()
        {
            try
            {
                mLog.Info("Record Manager Begin Process List Post Action.");
                if (aveSPList != null)
                {
                    SetAutoDeclareRecordsTrue();
                    AvePostAction.ListPostAction(aveSPList);
                }
            }
            catch (Exception ex)
            {
                mLog.Info("Process List Post Action Exception,Message: {0}.", ex.ToString());
            }
            DisposeObj(site);
            DisposeObj(web);
            DisposeObj(aveSPSite);
            DisposeObj(aveSPWeb);
            //DisposeObj(aveSPList);
        }

        private void DisposeObj(IDisposable obj)
        {
            if (obj != null)
            {
                obj.Dispose();
                obj = null;
            }
        }
    }
}
