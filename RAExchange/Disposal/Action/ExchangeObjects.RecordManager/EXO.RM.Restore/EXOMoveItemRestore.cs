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

using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Restore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using AvePoint.RA.RAExchange.Disposal.Common;
using AvePoint.Cryptography;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.RAExchange.Disposal.Object;
using AvePoint.GCommon.Contract.Server.Common.RemoteNode;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.SharePoint.ArchiverCommon;
using DocumentFormat.OpenXml.Math;
using System.Configuration;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using System.Security;
using System.Diagnostics;

namespace AvePoint.RA.RAExchange.Disposal.Action
{
    public class EXOMoveItemRestore : AveSPDoc, IDisposable
    {
        AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);       
        EXOConfiguration mConfig;
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
        private readonly object mLock = new object();
        private bool hasRestoredParentInfo = false;
        public EXOMoveItemRestore(IAveRestoreStream stream, EXOConfiguration config)
        {
            importStream = stream;
            mConfig = config;
        }

        public EXOMoveItemRestore()
        {

        }

        public void Init(EXOConfiguration config, bool isKeepFolderStructure)
        {
            //importStream = stream;
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

        public void DeclareItem(string fileName)
        {
            using (var performance = new PerformanceScope("EXOMoveItemRestore.DeclareItem", "", true))            
            {
                string fileFullPath = string.Empty;
                if (isKeepFolderStructure && !string.IsNullOrEmpty(mConfig.subFolderUrl))
                {
                    fileFullPath = list.RootFolder.ServerRelativeUrl.TrimEnd('/') + "/" + mConfig.subFolderUrl.TrimEnd('/') + "/" + fileName;
                }
                else
                {
                    fileFullPath = list.RootFolder.ServerRelativeUrl.TrimEnd('/') + "/" + fileName;
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
                            mLog.Info("Destination file is checked out.In order to declare,file must be checked in.File Url:{0}", file.UniqueId);
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
                    if (!ArchiverCommonStaticMethod.CheckisRecord(item))
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
            using (var performance = new PerformanceScope("EXOMoveItemRestore.UnDeclareItem", "", true))            
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
            using (var performance = new PerformanceScope("EXOMoveItemRestore.ModifySiteRecordDeclarationSetting", "", true))            
            {
                try
                {
                    if (site.RootWeb.AllProperties.ContainsKey("ecm_siterecordrestrictions") && site.RootWeb.AllProperties["ecm_siterecordrestrictions"].ToString() != EXOConfiguration.BlockDelete)
                    {
                        revertRecordDeclarationSetting = site.RootWeb.AllProperties["ecm_siterecordrestrictions"].ToString();
                        site.RootWeb.AllProperties["ecm_siterecordrestrictions"] = EXOConfiguration.BlockDelete;
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
            using (var performance = new PerformanceScope("EXOMoveItemRestore.RevertSiteRecordDeclarationSetting", "", true))            
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
            using (var performance = new PerformanceScope("EXOMoveItemRestore.EnsureFolder", "", true))            
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

        public void RestoreParentInfo(string desUrl, Dictionary<string,string> properties)
        {
            if (!hasRestoredParentInfo)
            {
                using (var performance = new PerformanceScope("EXOMoveItemRestore.RestoreParentInfo", "", true))
                {
                    AveBPOSAccountInfo user = null;
                    string siteUrl = string.Empty;
                    string userName = string.Empty;
                    string passWordString = string.Empty;
                    AveObjectModelFactory factory = null;
                    AvePoint.GCommon.Contract.CentralAdmin.Object.BposInfo bposInfo = null;
                    factory = mConfig.recordManagerRestoreOMFactory;
                    userName = mConfig.CurrentRule.EXORule.MoveToRecordCenterAndDelareSetting.DestinationLocation.UserName;
                    bposInfo = mConfig.CurrentRule.EXORule.MoveToRecordCenterAndDelareSetting.DestinationLocation.BposInfo;
                    if (bposInfo != null && (bposInfo.ConnectionType == GCommon.Contract.CentralAdmin.Object.BposConnectionType.AppToken || bposInfo.ConnectionType == GCommon.Contract.CentralAdmin.Object.BposConnectionType.Modern))
                    {
                        desUrl = HttpUtility.UrlDecode(desUrl);
                        user = bposInfo.ConvertToAveBPOSAccountInfo();
                        siteUrl = factory.CreateSiteServiceHelper().TryToRectifySiteUrl(desUrl, user);//获取的是Web URL，而不是实际的SC URL
                    }
                    else if (!string.IsNullOrEmpty(userName))
                    {
                        desUrl = HttpUtility.UrlDecode(desUrl);
                        if (bposInfo != null)
                        {
                            user = bposInfo.ConvertToAveBPOSAccountInfo();
                        }
                        else
                        {
                            user = new AveBPOSAccountInfo() { Domain = "", UserName = userName, Password = CspCommunicationWrapper.UnWrapKeyToSecureString(mConfig.CurrentRule.EXORule.MoveToRecordCenterAndDelareSetting.DestinationLocation.Password), TenantGroupId = string.Empty };
                        }
                        mLog.Info("RestoreParentInfo: Init BPOS Factory , tenantId is:{0}.TenantGroupId:{1}.", user.TenantId, user.TenantGroupId);
                        siteUrl = factory.CreateSiteServiceHelper().TryToRectifySiteUrl(desUrl, user);//获取的是Web URL，而不是实际的SC URL
                    }
                    else
                    {
                        user = mConfig.user;
                        siteUrl = factory.CreateSiteServiceHelper().TryToRectifySiteUrl(desUrl, user);//获取的是Web URL，而不是实际的SC URL
                    }
                    lock (mLock)
                    {
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
                            web = site.OpenWeb();
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
                            //web = site.OpenWeb();
                            web = site.OpenWeb(AveUrlUtility.GetServerRelativeUrl(siteUrl));
                        }
                        if (desUrl.Contains("#/"))
                        {
                            //list = web.GetListFromUrl(desUrl.Substring(desUrl.IndexOf("#/", StringComparison.OrdinalIgnoreCase) + 2));
                            desUrl = desUrl.Substring(desUrl.IndexOf("#/", StringComparison.OrdinalIgnoreCase) + 2);
                        }
                        list = web.GetList(desUrl);
                        string listUrl = list.ParentWeb.Url + "/" + list.RootFolder.Url;
                        EnsureFolder(list.RootFolder.ServerRelativeUrl);
                        RestoreSiteInfo(site, user);
                        RestoreWebInfo();
                        Stopwatch stopwatchForRestoreParent = new Stopwatch();
                        stopwatchForRestoreParent.Start();
                        RestoreListInfo(properties);
                        mLog.Info($"restore list info for exo cost:{stopwatchForRestoreParent.ElapsedMilliseconds}");
                        stopwatchForRestoreParent.Stop();
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
                hasRestoredParentInfo = true;
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

        public void RestoreSiteInfo(IAveSite site, AveBPOSAccountInfo user)
        {
            using (var performance = new PerformanceScope("EXOMoveItemRestore.RestoreSiteInfo", "", true))            
            {
                //var siteInfo = importStream.ReadMetadata().GetMetadata<AveSiteInfo>();
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
                    aveSPSite.RestoreSiteSelf(site.SiteSerializer.GetObjectData());
                }
                //importStream.Reset();
            }
        }

        private void RestoreWebInfo()
        {
            using (var performance = new PerformanceScope("EXOMoveItemRestore.RestoreWebInfo", "", true))            
            {
                //var webInfo = importStream.ReadMetadata().GetMetadata<AveWebInfo>();
                if (aveSPWeb == null || aveSPWebId == null || aveSPWebId != currentIAveFolder.ParentWeb.ID)
                {
                    aveSPWeb = new AveSPWeb(aveSPSite, web.ServerRelativeUrl);
                    aveSPWebId = web.ID;
                    aveSPWeb.RestoreWebSelf(web.WebSerializer.GetObjectData());
                }
                //importStream.Reset();
            }
        }

        private void RestoreListInfo(Dictionary<string,string> properties)
        {
            using (var performance = new PerformanceScope("EXOMoveItemRestore.RestoreListInfo", "", true))            
            {
                //var listInfo = importStream.ReadMetadata().GetMetadata<AveListInfo>();
                //var fieldXML = importStream.ReadMetadata().GetMetadata<string>();
                //var contentTypeInfo = importStream.ReadMetadata().GetMetadata<AveContentTypeCollectionInfo>();
                List<MoveMetadataInfo> dataList = mConfig.CurrentRule.EXORule.spMoveOption.MoveToSPDataList;
                bool IsCheckedMoveMetedata = mConfig.CurrentRule.EXORule.spMoveOption.IsMoveToSP;
                if (aveSPList == null || aveSPListId == null || aveSPListId != list.ID)
                {
                    if (aveSPList != null)
                    {
                        SetAutoDeclareRecordsTrue();
                        AvePostAction.ListPostAction(aveSPList);
                    }
                    aveSPList = new AveSPList(aveSPWeb, list.Title);
                    //change list title to find the right list  //SAAS-29158 RECO-348
                    //listInfo.Title = list.Title;
                    aveSPListId = list.ID;
                    //listInfo.RootWebOnly = false;
                    aveSPList.RestoreListSelf(list.GetListInfo());
                    try
                    {
                        bool needUpdate = false;
                        if (properties != null && properties.Count > 0)
                        {
                            foreach (var property in properties)
                            {
                                if (IsCheckedMoveMetedata && dataList!=null && dataList.Select(a=>a.ExoColumn).Contains(property.Key))
                                {
                                    var moveInfoList = dataList.Where(a => a.ExoColumn == property.Key).ToList();
                                    MoveMetadataInfo moveInfo = new MoveMetadataInfo();
                                    if (moveInfoList != null && moveInfoList.Count > 0)
                                    {
                                        moveInfo = moveInfoList.First();
                                    }
                                    else
                                    {
                                        mLog.Warn($"column does not exsit,continue");
                                        continue;
                                    }
                                    try
                                    {
                                        string tempField = aveSPList.SPList.Fields[moveInfo.SPColumn].Title;
                                        if (!string.IsNullOrEmpty(tempField))
                                        {
                                            mLog.Info($"list column exsit,no need append new column,title:{moveInfo.SPColumn}");
                                            continue;
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        mLog.Warn($"column does not exsit,creat it,title:{moveInfo.SPColumn}");
                                    }
                                    needUpdate = true;
                                    aveSPList.SPList.Fields.AddFieldAsXml($"<Field Type='Note' DisplayName='{SecurityElement.Escape(moveInfo.SPColumn)}' Name='{SecurityElement.Escape(moveInfo.SPColumn)}' ID='{Guid.NewGuid().ToString()}' Hidden='FALSE'/>", true, AveAddFieldOptions.DefaultValue);
                                }
                            }
                            if (needUpdate)
                            {
                                aveSPList.SPList.Update();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.Info("Exception in add column to list,Message: {0}.", ex.ToString());
                    }
                    if (aveSPList.RootFolder.Properties.ContainsKey("ecm_AutoDeclareRecords") && aveSPList.RootFolder.Properties["ecm_AutoDeclareRecords"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase))
                    {
                        mLog.Info("List ecm_AutoDeclareRecords is true and set false.ListUrl: {0}.", list.Title);
                        SetAutoDeclareRecordsFalse();
                    }
                }
                //SAAS-15676  由于结构原因导致Field和ContentType每次job只Reload一次,如果原端list改变则需要重新load
                //if (listInfo.Id != mConfig.tempListId)
                //{
                //    aveSPList.AveFields.RestoreFields(fieldXML);
                //    aveSPList.AveContentTypes.LoadContentTypes(contentTypeInfo);
                //    mConfig.tempListId = listInfo.Id;
                //}
                //importStream.Reset();
            }
        }

        private void RestoreFolderInfo()
        {
            using (var performance = new PerformanceScope("EXOMoveItemRestore.RestoreFolderInfo", "", true))            
            {
                if (aveSPFolder == null || aveSPFolderId == null || aveSPFolderId != currentIAveFolder.ParentList.RootFolder.UniqueId)
                {
                    aveSPFolder = new AveSPFolder(aveSPList, currentIAveFolder.Name);
                    aveSPFolderId = currentIAveFolder.UniqueId;
                    //if (!aveSPList.Url.EndsWith(aveSPFolder.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
                    //{
                    //    //aveSPRootFolder.ImportParentFolder(importStream);
                    //    importStream.Reset();
                    //}
                }
                else if (!aveSPList.Url.EndsWith(aveSPFolder.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
                {
                    // var folderInfo = importStream.ReadMetadata().GetMetadata<AveSPFolderMetadataDto>();
                    // importStream.Reset();
                }
            }
        }

        public Record GetDesFileRecord(string fileName)
        {
            using (var performance = new PerformanceScope("EXOMoveItemRestore.GetDesFileRecord", "", true))            
            {
                Record desDto = new Record();
                try
                {
                    IAveFile file = aveSPWeb.SPWeb.GetFile(list.RootFolder.ServerRelativeUrl.TrimEnd('/') + "/" + fileName);
                    desDto.ScopeId = web.Site.ID;
                    desDto.NodeId = file.UniqueId;
                    desDto.DirPath = file.ServerRelativeUrl;
                    desDto.NodeType = 500;
                    desDto.LeafName = file.Name;
                    desDto.RuleId = Guid.Parse(mConfig.CurrentRule.Id);
                    desDto.TimeModified = file.TimeLastModified.Ticks;
                    desDto.TimeCreated = file.TimeCreated.Ticks;
                    desDto.WebId = file.ParentFolder.ParentList.ParentWeb.ID;
                    desDto.ListId = file.ParentFolder.ParentList.ID;
                    desDto.FolderId = file.ParentFolder.UniqueId;
                    desDto.ItemId = file.UniqueId;
                    desDto.ItemRowId = file.Item.ID;
                    desDto.FullPath = new Uri(web.Site.Url).Scheme + @"://" + new Uri(web.Site.Url).Authority + file.ServerRelativeUrl.Replace("\\", "/");
                    desDto.Id = AvePoint.RA.RACommonUtility.IDGenerator.GetRecordId(web.Site.ID, desDto.NodeId);
                    desDto.SourceFlag = 1;
                    desDto.ParentId = file.ParentFolder == null ? Guid.Empty : file.ParentFolder.UniqueId;
                    //RECO - 3615, RECO-3616 当前版本，Move行为仍然不去管所有属性，依赖后期data sync行为。所以create by modified by 还从sourceRecord 获取。
                    //desDto.CreatedBy = GetFileCreatedBy(file.Item);
                    desDto.DeclareAsRecord = ArchiverCommonStaticMethod.CheckisRecord(file.Item) && ArchiverCommonStaticMethod.IsBlockEditAndDeleteRecord(file.Item);
                    var daoSite = mConfig.GetRemoteSiteCollectionByRecords(web.Site.Url);
                    desDto.SourceFlag = daoSite != null ? daoSite.NodeType == GCommon.Contract.Server.ControlPanel.Office365.RemoveNodeType.SkyDrivePro ? (int)SourceFlag.OneDrive : (int)SourceFlag.SharePoint : (int)SourceFlag.SharePoint;
                    if (daoSite != null)
                    {
                        desDto.AveSiteId = daoSite.id;
                    }
                    else
                    {
                        mLog.Info("Can't get DAO SiteID:{0}.", web.Site.Url);
                    }
                    if (desDto.SourceFlag == (int)SourceFlag.SharePoint)
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

        private void SetAutoDeclareRecordsTrue()
        {
            using (var performance = new PerformanceScope("EXOMoveItemRestore.SetAutoDeclareRecordsTrue", "", true))            
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
            using (var performance = new PerformanceScope("EXOMoveItemRestore.SetAutoDeclareRecordsFalse", "", true))            
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
