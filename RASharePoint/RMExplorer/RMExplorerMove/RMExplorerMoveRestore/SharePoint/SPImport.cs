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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMRelatedRecord;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.RelatedRecords;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Restore;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace AvePoint.RA.SharePoint.RMExplorer
{
    public class SPImport : IDisposable
    {
        private RALogger mLog = RALogger.GetInstance(typeof(SPImport));
        #region interface
        private IRMRemoteNodeDao RMRemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        
        #endregion

        private Guid recordFeatureId = new Guid("da2e115b-07e4-49d9-bb2c-35e93bb9fca9");
        private MoveDestination destinationInfo = null;
        private AppendItemMapping appendMapping = null;
        private MoveSettingInfo moveSettingInfo = null;
        private IAveRestoreStream importStream; 
        private AveObjectModelFactory objectModelFactory;

        private IAveORecords record;
        private AveBPOSAccountInfo user = null;
        private DateTime mInitialTime = DateTime.MinValue;//用于记录Site的生存时间
        private AveSPSite aveSPSite;
        private AveSPWeb aveSPWeb;
        private Guid aveSPWebId;
        private AveSPList aveSPList;
        private Guid aveSPListId;
        private AveSPFolder aveSPFolder;
        private Guid aveSPFolderId;
        string currentFolderUrl;
        private IAveSite site;
        private IAveWeb web;
        private IAveList list;
        private List<IAveField> needupdateField;
        private bool AutoDeclareRecordsChange = false;
        private readonly Guid BCSColumnID = new Guid("20f84bba906045b4af568ee102a52dcb");
        private string originalRestrictionsSetting = string.Empty;

        public bool isFirstItem = true;//first move destination item

 

        private string lastTimedesUrl = string.Empty;

        internal string fileName;

        internal bool isFirstVersion;


        private ItemDependencyOption itemDependencyOption;

        private ContentConflictResolution conflictResolution;

        private FilePropertiesMapping fsPropertyMappings;

        public AveSPFolder GetParentFolder
        {
            get
            {
                return aveSPFolder;
            }
        }

        public string destinationContainerUrl { get; private set; }

        public Guid SiteId { get; private set; }

        //用来记录Move过程，目的端原本文件的ID 和Move之后文件ID 的mapping关系
        private Dictionary<Guid, Guid> FileIDMapping = new Dictionary<Guid, Guid>();

        private ConcurrentDictionary<Guid, DestinationListTermSetting> mDestinationListTermSetting = new ConcurrentDictionary<Guid, DestinationListTermSetting>();

        private string mDestinationFullPath;

        public SPImport(MoveDestination desInfo, AppendItemMapping mAppendMapping, MoveSettingInfo moveSetting)
        {
            destinationInfo = desInfo;
            if (desInfo.DestMode == DestMode.UrlMode)
            {
                destinationContainerUrl = desInfo.SPUrl;
            }
            else if(desInfo.DestMode == DestMode.TreeMode)
            {
                destinationContainerUrl = desInfo.SPTreeNode.FullPath;
            }
            destinationContainerUrl = HttpUtility.UrlDecode(destinationContainerUrl);
            appendMapping = mAppendMapping;
            conflictResolution = moveSetting.ContentConflictResolution;
            itemDependencyOption = moveSetting.ItemDependencyOption;
            fsPropertyMappings = moveSetting.FSPropertyMappings;
            moveSettingInfo = moveSetting;
            InitSPObjectInfo();
            GetRecordRestrictions(site);
            RMGlobalLocker.Initialize();
        }

        public void Init(IAveRestoreStream stream, bool isFirstItem = false)
        {
            importStream = stream;
            isFirstVersion = isFirstItem;
        }

        public AveObjectModelFactory ObjectModelFactory
        {
            get
            {
                if (objectModelFactory == null)
                {
                    RemoteSiteCollection site = new RMExplorerUtility().GetRemoteSiteCollectionByListUrl(destinationContainerUrl);
                    user = PoolUserUtil.GetBPOSInfoAsync(site).Result;
                    objectModelFactory = MultiAppUtil.CreateAveObjectModelFactory(destinationContainerUrl, user, AveContextKind.ClientObjectModel);
                }
                return objectModelFactory;
            }
        }

        public IAveORecords Record
        {
            get
            {
                if (record == null)
                {
                    record = ObjectModelFactory.CreateRecords();
                }
                return record;
            }
        }

        private void InitSPObjectInfo()
        {
            if (site == null)
            {
                mInitialTime = DateTime.Now;
                var siteUrl = ObjectModelFactory.CreateSiteServiceHelper().TryToRectifySiteUrl(destinationContainerUrl, user);
                site = ObjectModelFactory.CreateSite(siteUrl);
                SiteId = site.ID;
                // 重新实例化site 对象，必须释放aveSPSite，并且把aveSPSite 置空，保证能走到restore site 逻辑中
                if (aveSPSite != null)
                {
                    this.Dispose();
                    aveSPSite = null;
                    aveSPWeb = null;
                    aveSPList = null;
                    aveSPFolder = null;
                }
                web = site.OpenWeb();
            }
            else if (mInitialTime.AddHours(23) < DateTime.Now
                     //OutOfPlace 目前只有下一个Rule才会出现换desUrl 的case
                     //InPlace 可能出现subsite，下次是subsite/subsite1 的现象 
                     //为了保证site 对象正确，并且site.Openweb 能够获取最底层site，只要目的端Url 改变，就重新获取site
                     || (!lastTimedesUrl.Equals(destinationContainerUrl, StringComparison.OrdinalIgnoreCase)))
            {
                site.Dispose();
                // 重新实例化site 对象，必须释放aveSPSite，并且把aveSPSite 置空，保证能走到restore site 逻辑中
                if (aveSPSite != null)
                {
                    this.Dispose();
                    aveSPSite = null;
                    aveSPWeb = null;
                    aveSPList = null;
                    aveSPFolder = null;
                }
                mInitialTime = DateTime.Now;
                site = ObjectModelFactory.CreateSite(destinationContainerUrl);
                web = site.OpenWeb();
            }
            if (destinationContainerUrl.Contains("#/"))
            {
                list = web.GetListFromUrl(destinationContainerUrl.Substring(destinationContainerUrl.IndexOf("#/", StringComparison.OrdinalIgnoreCase) + 2));
            }
            else
            {
                list = web.GetList(destinationContainerUrl);
            }
            //REC-4836 Get root folder url as des url now, if we support folder level later, we need to use folder url instead
            destinationContainerUrl = CommonUtil.MakeSPFullUrl(site.Url, list.RootFolder.ServerRelativeUrl);
            lastTimedesUrl = destinationContainerUrl;
            if (list != null && list.Fields != null)
            {
                needupdateField = list.Fields.Where(x => string.IsNullOrEmpty(x.DefaultValue) && x.Required && !x.Hidden).ToList();
                UpdateFieldRequired();
            }
        }

        public void RestoreParentInfo()
        {
            using (AvePerformanceScope pc1 = new AvePerformanceScope("RA.ItemRecordManager.RestoreParentInfo"))
            {
                RestoreSiteInfo(site);
                RestoreWebInfo();
                RestoreListInfo();
                string subFolderUrl = string.Empty;
                //Inplace Restore 需要指定标准subFolderUrl，例如：F1/F2/F3，不能有多余内容，如果只想还原到RootFolder，传string.Empty 即可
                //if (inplaceRestore)
                //{
                //    string desUrl = HttpUtility.UrlDecode(destinationContainerUrl);
                //    string listUrl = list.ParentWeb.Url + "/" + list.RootFolder.Url;
                //    subFolderUrl = desUrl.Substring(listUrl.Length).Trim('/');
                //}
                RestoreFolderInfo(subFolderUrl);

            }
        }

        private void RestoreSiteInfo(IAveSite site)
        {
            var siteInfo = importStream.ReadMetadata().GetMetadata<AveSiteInfo>();
            if (aveSPSite == null)
            {

                aveSPSite = new AveSPSite(site.Url, site.Url, AveContextKind.ClientObjectModel, user);
                aveSPSite.RestoreSiteSelf(siteInfo);
            }
            importStream.Reset();
        }

        private void RestoreWebInfo()
        {
            var webInfo = importStream.ReadMetadata().GetMetadata<AveWebInfo>();
            if (aveSPWeb == null || aveSPWebId == null || aveSPWebId != web.ID)
            {
                aveSPWeb = new AveSPWeb(aveSPSite, web.ServerRelativeUrl);
                aveSPWebId = web.ID;
                aveSPWeb.RestoreWebSelf(webInfo);
            }
            importStream.Reset();
        }

        private void RestoreListInfo()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("RecordOnline.SPImport.RestoreListInfo"))
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

                    try
                    {

                        IAveTaxonomyField textField = null;
                        if (CheckHasRecordsClassificationColumn(aveSPList, ref textField))
                        {
                            if (!aveSPList.AveFields.RestoredFieldInternalNameList.Contains(textField.TextField.ToString()))
                            {
                                aveSPList.AveFields.RestoredFieldInternalNameList.Add(textField.TextField.ToString());
                            }                           
                            if (!aveSPList.AveFields.RestoredFieldInternalNameList.Contains(textField.InternalName))
                            {
                                aveSPList.AveFields.RestoredFieldInternalNameList.Add(textField.InternalName);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        mLog.Error("An error occurred while skip restoring destination bcs column, error:{0}", e.ToString());
                    }
                    if (aveSPList.RootFolder.Properties.ContainsKey("ecm_AutoDeclareRecords") && aveSPList.RootFolder.Properties["ecm_AutoDeclareRecords"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase))
                    {
                        mLog.Info("List ecm_AutoDeclareRecords is true and set false.ListUrl: {0}.", list.ID);
                        SetAutoDeclareRecordsFalse();
                    }
                    //每个source 来都需要load 一遍List 的Field 和CT ，因为每个source可能属于不同的list。此处由于GUI 只允许选择15个文件，所以不会有效率问题
                    aveSPList.AveFields.RestoreFields(fieldXML);
                    aveSPList.AveContentTypes.LoadContentTypes(contentTypeInfo);
                }
                importStream.Reset();
            }
        }

        private bool CheckHasRecordsClassificationColumn(AveSPList list, ref IAveTaxonomyField textField)
        {
            bool result = true;
            try
            {
                var site = RMRemoteNodeDao.GetRemoteSiteCollectionByUrl(list.ParentSite.SiteUrl);
                Guid groupId = new Guid(site.parentId);
                var columnName = new SharePointSettingUtility().GetMedataColumn(groupId);
                string internalName = string.Empty;
                Guid bcsColumnId = Guid.Empty;
                if (!string.IsNullOrWhiteSpace(columnName))
                {
                    mLog.Info("Column name on group:{0}, groupId {1}", columnName, groupId);
                    var field = GetTaxonomyField(list.SPList.ParentWeb.Site.RootWeb.Fields, columnName);
                    if (field != null)
                    {
                        internalName = field.InternalName;
                        bcsColumnId = field.ID;
                        if (!list.SPList.Fields.ContainsFieldWithInternalName(internalName))
                        {
                            if (internalName != RcordsBuiltInColumn.ITEM_BCS_NAME)
                            {
                                var bcsColumn = list.SPList.Fields.GetFieldById(bcsColumnId, false);
                                if (bcsColumn != null)
                                {
                                    internalName = bcsColumn.InternalName;
                                }
                                else
                                {
                                    result = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        mLog.Info($"Cannot find bcs column in site:{list.ParentSite.SiteUrl}");
                        result = false;
                    }
                }
                else
                {
                    result = false;
                }
                if (!string.IsNullOrEmpty(internalName))
                {
                    textField = list.SPList.Fields.GetFieldByInternalName(internalName) as IAveTaxonomyField;
                    if (textField == null)
                    {
                        result = false;
                    }
                }
            }
            catch (Exception e)
            {
                mLog.Warn($"Error occurred while checking if bcs column exist. List:{list.SPList.Title} Error:{e.ToString()}");
            }
            return result;
        }       

        protected IAveTaxonomyField GetTaxonomyField(IAveFieldCollection fields, string rmFieldTitle)
        {
            return fields.GetRecordTaxonomyField(rmFieldTitle);
        }

        /// <summary>
        /// subFolderUrl = "", means restore root folder, or you need to set the right folder path like: F1/F2/F3
        /// </summary>
        /// <param name="subFolderUrl"></param>
        private void RestoreFolderInfo(string subFolderUrl)
        {
            if (aveSPFolder == null || aveSPFolderId == null ||
               //目的端url从一个root folder 切换到另一个root folder 的时候， 传来的subFolderUrl 都是空，所以 #3 判断会无法识别，此处通过上次还原的root folder id 跟本次还原的list root folder id 比较，进行判断是否切换了list; 目前逻辑在切换目的端的Url 的时候AveSPFolder已经置空，所以理论上没问题，保留逻辑为了代码健壮
               !aveSPFolder.ParentList.RootFolder.UniqueId.Equals(aveSPList.RootFolder.UniqueId) ||
               !subFolderUrl.Equals(currentFolderUrl, StringComparison.OrdinalIgnoreCase))
            {
                aveSPFolder = new AveSPFolder(aveSPList, list.RootFolder.Name);
                AveSPFolder subFolder = GetSubSPFolder(aveSPFolder, subFolderUrl);
                currentFolderUrl = subFolderUrl;
                aveSPFolder = subFolder;
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

        public async Task<JobResult> RestoreAveSPDocAsync(SPSource source, bool isLeaveDocLink = false)
        {
            string name = source.FileName;
            JobResult result = new RMExplorer.JobResult() { Status = Contract.RMWeb.JobMonitor.JobDetailsStatus.Successful };
            Guid originalFileUniqueId = Guid.Empty;
            AveSPDoc doc = new AveSPDoc(aveSPFolder, name);
            using (AvePerformanceScope performanceImportAveSPDoc = new AvePerformanceScope("RA.ItemRecordManager.RestoreAveSPDoc"))
            {
                var docInfo = this.importStream.ReadMetadata().GetMetadata<AveSPDocumentMetadataDto>();
                var userData = docInfo.UserDataInfo;
                AveRestoreOption mRestoreOption = new AveRestoreOption(0);
                switch (conflictResolution)
                {
                    case ContentConflictResolution.Overwrite:
                        mRestoreOption.ResetRestoreMode((int)AveRestoreMode.OverWrite);
                        if (isFirstVersion)
                        {
                            IAveFile originalDestFile = web.GetFile(aveSPFolder.ServerRelativeUrl.TrimEnd('/') +"/"+ name);
                            if(originalDestFile.Exists)
                            {
                                originalFileUniqueId = originalDestFile.UniqueId;
                                var utility = new RelatedRecordsUtility();
                                IAveListItem item = originalDestFile.Item;
                                utility.RemoveRelatedPropertyForListItem(item);
                                //var destRelatedProperties = utility.GetRelatedProperties(item);
                                //destRelatedProperties.ForEach(r => r.NeedDelete = true);
                                //var recordIds = destRelatedProperties.Select(d =>
                                //{
                                //    Guid selectResult;
                                //    if (d.SourceFlag == (int)SourceFlag.All || d.SourceFlag == (int)SourceFlag.SharePoint)
                                //    {
                                //        selectResult = IDGenerator.GetRecordId(d.SiteId, d.id);
                                //    }
                                //    else
                                //    {
                                //        selectResult = d.id;
                                //    }
                                //    return selectResult;
                                //}).ToList();
                                //var dbId = IDGenerator.GetRecordId(web.Site.ID, originalDestFile.UniqueId);
                                //recordIds.Add(dbId);
                                //var moveDBUtil = new RMExplorerMoveDBUtil();
                                //var allRecords = moveDBUtil.GetRecords(recordIds.ToArray());
                                //utility = new RelatedRecordsUtility(allRecords.Where(r => r.Id == dbId).First());
                                //utility.UpdateRelatedPropertiesForExplorer(destRelatedProperties, allRecords);
                            }
                            aveSPFolder.RestoringItem.ResetNewItemValues(true, "", "");
                        }
                        break;
                    case ContentConflictResolution.Skip:
                        //AveRestoreMode.Default  means skip 
                        mRestoreOption.ResetRestoreMode((int)AveRestoreMode.Default);
                        if (isFirstVersion)
                        {
                            aveSPFolder.RestoringItem.ResetNewItemValues(true, "", "");
                        }
                        break;
                    case ContentConflictResolution.Append:
                        mRestoreOption.ResetRestoreMode((int)AveRestoreMode.Append);
                        bool? isItemExistInDestination = false;
                        //Append 方式，Local 在check file在目的端是否存在时会查询DB，因此其它user 自动check  out 的文件也能正确Append.
                        //365则是用API方式获取的，暂时无法判断。
                        mRestoreOption.mAveRestoreMode = ResetDocNameIfNeedAppend(doc, doc.Name, isLeaveDocLink, ref isItemExistInDestination);
                        if (isFirstVersion)
                        {
                            aveSPFolder.RestoringItem.ResetNewItemValues(true, doc.Name, doc.Name);
                        }
                        break;
                }
                // DELETE_ITEM = true means Delete Des Document
                mRestoreOption.mAveItemRestoreOption.DELETE_ITEM = true;
                doc.SetRestoreOption(mRestoreOption);

                #region reload web,list
                try
                {
                    //ADO-160387 此方法每次reload大约15~30ms，为了保证对象统一，需要每次都要重新reload，保证content type 和field还原不冲突。
                    this.aveSPWeb.ReloadWeb();
                    this.aveSPList.ReloadList();
                    this.aveSPFolder.ReloadFolder();
                }
                catch (Exception ReloadEx)
                {
                    mLog.Warn("Can not Reload Web or List in Record Manager Job,but not affect Current job,Reason:{0}.", ReloadEx.ToString());
                }
                #endregion
                if (isLeaveDocLink)
                {
                    try
                    {
                        userData["File_x0020_Type"] = "aspx";
                        //for ghost page.ADO-206213
                        if (docInfo.DocInfo_Old.ContainsKey("SetupPath"))
                        {
                            docInfo.DocInfo_Old.Remove("SetupPath");
                            docInfo.DocInfo_Old["HasStream"] = 1;
                        }
                        //for picture library preview ADO-206271
                        if (userData.ContainsKey("PreviewExists"))
                        {
                            userData["PreviewExists"] = false;
                        }
                        if (userData.ContainsKey("MediaServiceMetadata"))
                        {
                            mLog.Info("Current file is leave link file and userData contains MediaServiceMetadata.MediaServiceMetadata:{0}.", userData["MediaServiceMetadata"].ToString());
                            userData.Remove("MediaServiceMetadata");
                        }
                        if (userData.ContainsKey("MediaServiceFastMetadata"))
                        {
                            mLog.Info("Current file is leave link file and userData contains MediaServiceFastMetadata.MediaServiceFastMetadata:{0}.", userData["MediaServiceFastMetadata"].ToString());
                            userData.Remove("MediaServiceFastMetadata");
                        }
                        //userData["URL"] = System.Web.HttpUtility.UrlEncode(linkAspxUrl).Replace("+", " ");
                        // userData["#tp_ContentTypeId"] = linkContentType.ID.ToByteArray();
                        //userData.Remove("Author");// not keep create by in Leave Stub.
                        //userData.Remove("Modified");
                        //userData.Remove("Created");
                        //userData.Remove("Modified_x0020_By");
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn("RA not keep property error {0}", ex.ToString());
                    }
                }
                try
                {
                    ItemLevelRestoreItemCTAndFields(userData, doc, itemDependencyOption);
                }
                catch(Exception e)
                {
                    if(e.Message.Equals("Invalid field type: TaxonomyFieldTypeMulti."))
                    {
                        mLog.Error(e.Message);
                    }
                    else
                    {
                        throw;
                    }
                }
               
                #region Match Exist CT Job
                //ADO-130694 Match Exist CT Job do not restore metadata
                if (itemDependencyOption == ItemDependencyOption.NotRestore)
                {
                    //#tp_ContentTypeId为ContentType 对应的Key，把这个属性还原，即可还原Content Type这个Column
                    string contentTypeString = "#tp_ContentTypeId";
                    object contentValue = null;
                    //File_x0020_Type为文件类型对应的key，还原时需要用它来还原图标  ADO-153817
                    string fileType = "File_x0020_Type";
                    object fileTypeValue = null;
                    //#tp_ModerationStatus 为当前List SPModerationStatusType，不赋值会导致wrapper底层UpdateModerationInfomation 方法出错，最终导致Version还原不对.  ADO-175144
                    string moderationStatus = "#tp_ModerationStatus";
                    object moderationStatusValue = null;
                    //原端文件如果是Declare 的，需要Keep 下面两个属性，防止Office文件因content中带有declare 属性，但是metadata 中不带。进而导致到目的端后API 与SP GUI 均无法declare 的问题 ADO-208136
                    string itemRecord = "_vti_ItemDeclaredRecord";
                    object itemRecordValue = null;
                    string itemHoldRecordStatus = "_vti_ItemHoldRecordStatus";
                    object itemHoldRecordStatusValue = null;
                    if (docInfo.UserDataInfo.ContainsKey(contentTypeString))
                    {
                        contentValue = docInfo.UserDataInfo[contentTypeString];
                    }
                    if (docInfo.UserDataInfo.ContainsKey(fileType))
                    {
                        fileTypeValue = docInfo.UserDataInfo[fileType];
                    }
                    if (docInfo.UserDataInfo.ContainsKey(moderationStatus))
                    {
                        moderationStatusValue = docInfo.UserDataInfo[moderationStatus];
                    }
                    if (docInfo.UserDataInfo.ContainsKey(itemRecord))
                    {
                        itemRecordValue = docInfo.UserDataInfo[itemRecord];
                    }
                    if (docInfo.UserDataInfo.ContainsKey(itemHoldRecordStatus))
                    {
                        itemHoldRecordStatusValue = docInfo.UserDataInfo[itemHoldRecordStatus];
                    }
                    docInfo.UserDataInfo = new Dictionary<string, object>();
                    if (null != contentValue)
                    {
                        docInfo.UserDataInfo.Add(contentTypeString, contentValue);
                    }
                    if (null != fileTypeValue)
                    {
                        docInfo.UserDataInfo.Add(fileType, fileTypeValue);
                    }
                    if (null != moderationStatusValue)
                    {
                        docInfo.UserDataInfo.Add(moderationStatus, moderationStatusValue);
                    }
                    if (null != itemRecordValue)
                    {
                        docInfo.UserDataInfo.Add(itemRecord, itemRecordValue);
                    }
                    if (null != itemHoldRecordStatusValue)
                    {
                        docInfo.UserDataInfo.Add(itemHoldRecordStatus, itemHoldRecordStatusValue);
                    }
                }
                #endregion

                if (isFirstVersion && conflictResolution == ContentConflictResolution.Overwrite)
                {
                    //DeleteDestinationFile(config);
                }

                

                RestoreDocumentMetadataDto(this.importStream, doc, docInfo, isLeaveDocLink);

                IAveFile desFile = this.aveSPWeb.SPWeb.GetFile(this.aveSPFolder.ServerRelativeUrl.TrimEnd('/') + "/" + doc.Name);
                mDestinationFullPath = desFile.Url;
                if (!FileIDMapping.ContainsKey(desFile.UniqueId))
                {
                    FileIDMapping.Add(desFile.UniqueId, originalFileUniqueId);
                }

                #region RecordsRelated
                //RecordsRelated Column type is NoteDataFormat which wrapper has logical to process this type column. we need special logic to keep current column value.
                if (userData != null && userData.ContainsKey("RecordsRelated"))
                {
                    string recordsRelatedValue = userData["RecordsRelated"].ToString();
                    try
                    {
                        XmlDocument xmlDoc = new XmlDocument();
                        recordsRelatedValue = recordsRelatedValue.Replace("&#58;", ":");
                        xmlDoc.LoadXml(recordsRelatedValue);
                        foreach (XmlElement ele in xmlDoc.GetElementsByTagName("a"))
                        {
                            var relatedObjString = ele.GetAttribute("rel");
                            relatedObjString = HttpUtility.UrlDecode(relatedObjString);
                            RMRelatedItemInfo relatedObj = SerializerHelper.DeserializeByJsonSerializer<RMRelatedItemInfo>(relatedObjString);
                            var relatedItemUrl = HttpUtility.UrlDecode(ele.GetAttribute("href"));
                            relatedItemUrl = new Uri(relatedObj.SiteUrl).Scheme + @"://" + new Uri(relatedObj.SiteUrl).Authority + relatedItemUrl;
                            ele.SetAttribute("href", relatedItemUrl);
                            mLog.Info("Replace RecordsRelated success,MoveDesSiteUrl:{0},relatedSiteUrl:{1},replaceRelatedItemUrl:{2}.", aveSPSite.SiteUrl, relatedObj.SiteUrl, relatedItemUrl);
                        }
                        recordsRelatedValue = xmlDoc.OuterXml.Replace(":", "&#58;");
                        if (desFile.Exists)
                        {
                            mLog.Info("File Exists,file id:{0}", desFile.UniqueId);
                            if (CommonUtil.IsRecord(desFile.Item))
                            {
                                mLog.Debug("current file is Declare Status and will be undo declare first. File name:{0}", desFile.UniqueId);
                                Record.UndeclareItemAsRecord(desFile.Item);
                                desFile.Item["RecordsRelated"] = recordsRelatedValue;
                                desFile.Item.SystemUpdate();
                                Record.DeclareItemAsRecord(desFile.Item);
                                mLog.Debug("Replace RecordsRelated Declare File Successful.File id:{0}", desFile.UniqueId);
                            }
                            else
                            {
                                desFile.Item["RecordsRelated"] = recordsRelatedValue;
                                desFile.Item.SystemUpdate();
                                mLog.Debug("Replace RecordsRelated File Successful.File id:{0}", desFile.UniqueId);
                            }
                        }
                        else
                        {
                            //进入这个判断有两种可能，1.当前文件是check out file. 2.当前文件在目的端不存在.
                            mLog.Info("File Not Exists when Replace RecordsRelated.file id:{0}", desFile.UniqueId);
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.Info("Replace RecordsRelated failed in move to action.Message:{0}.", ex.ToString());
                    }
                    //Update Dest item related field value
                    var util = new RelatedRecordsUtility();
                    //上面已经去过一次RMRelatedInfo 了。此处可以从上面循环获取，可优化的逻辑
                    var sourceProperties = util.GetRelatedPropertiesBySPColumnValue(userData["RecordsRelated"].ToString());
                    var destRelatedItemInfo = util.GenerateRMRelatedItemInfo(desFile.Item);
                    var dbUtil = new RMExplorerMoveDBUtil();
                    var ids = sourceProperties.Select(d =>
                    {
                        Guid selectResult;
                        if (d.SourceFlag == (int)SourceFlag.All || d.SourceFlag == (int)SourceFlag.SharePoint)
                        {
                            selectResult = IDGenerator.GetRecordId(d.SiteId, d.id);
                        }
                        else
                        {
                            selectResult = d.id;
                        }
                        return selectResult;
                    }).ToArray();
                    var allRecords = dbUtil.GetRecords(ids);
                    foreach (var property in sourceProperties)
                    {
                        if (property.SourceFlag == (int)SourceFlag.All || property.SourceFlag == (int)SourceFlag.SharePoint)
                        {
                            util.UpdateRelateColumnValue(property, source.SiteUrl, source.SourceUrl, source.NodeId, destRelatedItemInfo);
                        }
                        else if (property.SourceFlag == (int)SourceFlag.Physical)
                        {
                            util.UpdateRelateColumnValuePhysical(property, source.SiteUrl, source.SourceUrl, source.NodeId, destRelatedItemInfo, allRecords);
                        }
                    }
                }
                #endregion

                #region Check In if needed
                CheckInFile(desFile);
                #endregion

                #region Declare destination file
                int holdAndRecordStatus = 0;
                if (userData != null && userData.ContainsKey(ConstString.ItemHoldRecordStatus))
                {
                    var itemHoldRecordStatus = userData[ConstString.ItemHoldRecordStatus].ToString();

                    if (int.TryParse(itemHoldRecordStatus, out holdAndRecordStatus))
                    {
                        if (CommonUtil.IsRecord(holdAndRecordStatus))
                        {
                            if (IsOnedrive(site.Url))
                            {
                                mLog.Debug($"onedrive uable declare record, sc url:{site.Url}");
                                result.ErrorMessage = "RM_SO_OneDriveDeclareItem_ErrorMessage";
                            }
                            else
                            {
                                mLog.Debug("item need declare in destination.");
                                RecordRestrictions option = ConvertToRestrictionsOption(holdAndRecordStatus);
                                SetRecordRestrictions(site, option);
                                await DeclareItemAsync(desFile);
                            } 
                        }
                    }
                    else
                    {
                        mLog.Debug("Cannot convert column value to int, no need to declare the destination file");
                    }
                }
                #endregion

                result.DestStub = CommonUtil.GenerateDestStubInfo(desFile);
                result.DestStub.OriginalNodeId = FileIDMapping.ContainsKey(desFile.UniqueId) ? FileIDMapping[desFile.UniqueId] : Guid.Empty;
            }
            return result;
        }

        private bool IsOnedrive(string siteUrl)
        {
            var reg = new Regex(@"https://([^/]+?)-my\.(sharepoint[^/]*)(/.*)?");
            var matches = reg.Match(siteUrl);
            if (matches.Success)
            {
                mLog.Info($"Current site is onedrive site. Url:[{siteUrl}]");
            }
            return matches.Success;
        }

        public string UpdateBCSColumn(bool useExisting,string columnName, Guid termId)
        {
            string errorMessage = string.Empty;
            var bscField = GetBCSField(useExisting, columnName);
            if (bscField == null)
            {
                return "StorageOptimization_SOARRecordManagerEXOListBCSNotExist";
            }
            var term = site.AveSPTaxonomySession.GetTerm(termId);
            if (term == null)
            {
                return "StorageOptimization_SOARRecordManagerEXOSourceTermNotExist";
            }
            if (!InSameTermScope(termId, bscField))
            {
                return "StorageOptimization_SOARRecordManagerEXONotInSameTermScope";
            }          
            IAveFile desFile = web.GetFile(mDestinationFullPath);
            mLog.Info("File Exists in destination,file id:{0}.", desFile.UniqueId);
            try
            {
                var item = desFile.Item;
                item[bscField.ID] = termId;
                item[bscField.TextField] = term.Name;
                item.SystemUpdate();
                mLog.Info("Update destination file property successful.File id:{0}.", desFile.UniqueId);
            }
            catch (Exception e)
            {
                mLog.Info("Failed to update property for current file,Name:{0},Message{1}.", desFile.UniqueId, e.ToString());
                errorMessage = "StorageOptimization_SOARRecordManagerEXOKeepClassificationFailed";
            }
            return errorMessage;
        }

        public Guid GetDestinationTermId(string columnName)
        {
            Guid termId = Guid.Empty;
            try
            {
                IAveFile desFile = web.GetFile(mDestinationFullPath);
                if (desFile.Exists && desFile.Item.Fields.ContainsField(columnName))
                {
                    var termObj = desFile.Item[columnName];
                    if (termObj != null && !string.IsNullOrEmpty(termObj.ToString()))
                    {
                        var valueString = termObj.ToString().Split('|');
                        if (valueString.Length > 1)
                        {
                            termId = new Guid(valueString[1]);
                        }
                        else
                        {
                            mLog.Info($"{desFile.Url} invalid term format:{valueString}");
                        }
                    }
                }
                mLog.Info("File path:{0} Exist:{1} Destination termid{2} ColumnName:{3}", desFile?.UniqueId, desFile.Exists, termId, columnName);
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while getting term id in destination, error:{0}", e.ToString());
            }
            return termId;
        }

        public Guid UpdateClassificationColumnWithDestination(bool useExisting, string columnName, bool forceSetNull = false)
        {
            Guid termId = Guid.Empty;
            var listSetting = GetDestinationListTermSetting(useExisting, columnName);
            IAveFile desFile = web.GetFile(mDestinationFullPath);
            if (!listSetting.HasDefaultTermValue || forceSetNull)
            {
                //目的端list没有default term value，当时目的端文件有term，需要将termid更新为空
                try
                {
                    var item = desFile.Item;
                    if (item.Fields.ContainsField(columnName))
                    {
                        item[listSetting.FieldId] = null;
                        item[listSetting.TextFieldId] = null;
                        item.SystemUpdate();
                        mLog.Info("Update destination file property to null successful .File Name:{0}.", desFile.UniqueId);
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
                    mLog.Info("Update destination file property successful.File id:{0}.", desFile.UniqueId);
                    termId = listSetting.DefautTermId;
                }
                catch (Exception e)
                {
                    mLog.Info("Failed to update property for current file,id:{0},Message{1}.", desFile.UniqueId, e.ToString());
                }
            }
            return termId;
        }

        private DestinationListTermSetting GetDestinationListTermSetting(bool useExisting, string columnName)
        {
            var listId = list.ID;
            var bscField = GetBCSField(useExisting, columnName);
            if (!mDestinationListTermSetting.ContainsKey(listId))
            {
                if (bscField != null && !string.IsNullOrWhiteSpace(bscField.DefaultValue) && bscField.DefaultValue.IndexOf('|') > 0)
                {
                    var termId = new Guid(bscField.DefaultValue.Substring(bscField.DefaultValue.IndexOf('|') + 1));
                    var startIndex = bscField.DefaultValue.IndexOf(";#");
                    var endIndex = bscField.DefaultValue.IndexOf('|');
                    var termName = bscField.DefaultValue.Substring(startIndex + 2, endIndex - startIndex);
                    mDestinationListTermSetting.TryAdd(listId, new DestinationListTermSetting() { HasDefaultTermValue = true, DefautTermId = termId, DefaultTermName = termName, FieldId = bscField.ID, TextFieldId = bscField.TextField });
                }
                else
                {
                    mLog.Info("Destination list doesn't have term defaut value. List Url:{0} Term Default Value:{1}", aveSPList.SPList.ID, bscField?.DefaultValue);
                    mDestinationListTermSetting.TryAdd(listId, new DestinationListTermSetting() { HasDefaultTermValue = false, FieldId = (bscField != null) ? bscField.ID : Guid.Empty, TextFieldId = (bscField != null) ? bscField.TextField : Guid.Empty });
                }
            }
            return mDestinationListTermSetting[listId];
        }

        private IAveTaxonomyField GetBCSField(bool useExisting, string columnName)
        {
            IAveTaxonomyField taxField = null;
            if (list != null)
            {
                list.Reload();
            }
            AvePoint.GCommon.Utility.ArgumentCheck.NotNull(list, nameof(list));
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
                var tempField = list.Fields.Where(f => f.Title == columnName).FirstOrDefault();
                tempField ??= list.Fields.Where(f => f.InternalName == columnName).FirstOrDefault();
                if (tempField != null)
                {
                    taxField = tempField as IAveTaxonomyField;
                }
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
                    var sourceTermSet = site.AveSPTaxonomySession.GetTerm(termId).TermSet;
                    return sourceTermSet.ID.Equals(field.TermSetId) ? true : false;
                }
                else
                {
                    //term scope is term
                    var destinationTerm = site.AveSPTaxonomySession.GetTerm(field.AnchorId);
                    if (destinationTerm == null)
                    {
                        return false;
                    }
                    //check if in the same termset
                    var sourceTerm = site.AveSPTaxonomySession.GetTerm(termId);
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
        public void SetDestAveSPFolder(AveSPFolder aveSPFolder)
        {
            this.aveSPFolder = aveSPFolder;
        }



        private async System.Threading.Tasks.Task DeclareItemAsync(IAveFile file)
        {
            using (AvePerformanceScope pc1 = new AvePerformanceScope("RA.ItemRecordManager.DeclareItem"))
            {
                try
                {
                    if (site.Features[recordFeatureId] == null)
                    {
                        site.Features.Add(recordFeatureId, true);
                    }
                }
                catch (InvalidOperationException ex)
                {
                    mLog.Warn("In Place Records Management Feature is not installed in this farm, cannot declare document as a record:{0}", ex.ToString());
                    throw;
                }
                catch (Exception ex)
                {
                    mLog.Warn("Activate In Place Records Management feature error:{0}", ex.ToString());
                }
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
                bool lockStatus = false;
                string lockKey = site.ID.ToString();
                try
                {
                    IAveListItem item = file.Item;
                    if (!CommonUtil.IsRecord(item))
                    {
                        lockStatus = await RMGlobalLocker.GetRecordsLockerAsync(lockKey);
                        if (lockStatus)
                        {
                            Record.DeclareItemAsRecord(item);
                        }
                        else
                        {
                            mLog.Error("Cannot get locker, item : {0}.", file.Url);
                        }
                    }
                }
                catch (Exception ex)
                {
                    mLog.Debug(string.Format("Declare item Again, Reason : {0}", ex.ToString()));
                    //Get list again
                    aveSPWeb.ReloadWeb();
                    aveSPList.ReloadList();
                    aveSPFolder.ReloadFolder();
                    file = aveSPWeb.SPWeb.GetFile(list.RootFolder.ServerRelativeUrl.TrimEnd('/') + "/" + fileName);
                    //如果目的地开启了ForceCheckout ，我们必须先把File Checkout了之后，才能update，取到Item
                    if (list.ForceCheckout)
                    {
                        if (file.CheckedOutByUser == null)
                        {
                            file.CheckOut();
                        }
                    }
                    //ADO-130800 we must use file.Update here,or we'll get file.Item = null
                    file.Update();
                    IAveListItem finalitem = file.Item;
                    if (!CommonUtil.IsRecord(finalitem))
                    {
                        finalitem.Update();
                        //这个if 判断，为了最小权限User ，Item.Update 过后不是Declare文件，而且必须check in 才能执行Declare操作
                        if (!CommonUtil.IsRecord(finalitem))
                        {
                            //for minimum privilege
                            if (file.CheckedOutByUser != null)
                            {
                                try
                                {
                                    file.CheckIn("");//对Check Out的File进行并且check out User被删除的文件.CheckIn需要comment
                                }
                                catch (Exception e)
                                {
                                    mLog.Warn(string.Format("DeclareItem Failed. Error:{0}", e.ToString()));
                                }
                            }
                        }
                        file = aveSPWeb.SPWeb.GetFile(list.RootFolder.ServerRelativeUrl.TrimEnd('/') + "/" + fileName);
                        finalitem = file.Item;
                        if (!CommonUtil.IsRecord(finalitem))
                        {
                            Record.DeclareItemAsRecord(finalitem);
                        }
                    }
                }
                finally
                {
                    if (isFirstItem)
                    {
                        //[ADO-140024][ADO-139996]
                        //List下Item 第一次执行Declare 操作，会添加Column，导致List 增加Version。如果不是用System Account，Declare底层API 会有提权重新获取SPWeb,SPList对象的操作
                        //导致Wrapper对象与当前SP对象不匹配，需要进行Reload，此问题只有List 第一次添加文件时才需要。所以添加逻辑，每次Job 第一次进行Reload，防止影响效率 
                        aveSPWeb?.ReloadWeb();
                        aveSPList?.ReloadList();
                        //ADO-153731 Must reload folder here
                        aveSPFolder?.ReloadFolder();
                        mLog.Debug("Finally Reload Web,List in Declare");
                        isFirstItem = false;
                    }
                    if (lockStatus)
                    {
                        await RMGlobalLocker.ReleaseRecordsLockerAsync(lockKey);
                    }
                }
            }
        }

        private void CheckInFile(IAveFile file)
        {
            try
            {
                if (file.CheckedOutByUser != null && file.CheckedOutByUser.ID > 0)
                {
                    mLog.Info("Destination file is checked out, file need to be checked in. File url : {0}.", file.UniqueId);
                    file.CheckIn("");
                }
            }
            catch (Exception e)
            {
                mLog.Warn("An error occurred while checking in file. File Url : {0}, Error message : {1}.", file.ServerRelativeUrl, e.ToString());
            }
        }

        private RecordRestrictions ConvertToRestrictionsOption(int holdAndRecordStatus)
        {
            if(CommonUtil.IsBlockDeleteOnlyRecord(holdAndRecordStatus))
            {
                return RecordRestrictions.BlockDelete;
            }
            else
            {
                return RecordRestrictions.BlockDelete | RecordRestrictions.BlockEdit;
            }
        }

        private void SetRecordRestrictions(IAveSite site, RecordRestrictions option)
        {
            if (site.RootWeb.AllProperties.ContainsKey("ecm_siterecordrestrictions"))
            {
                site.RootWeb.AllProperties["ecm_siterecordrestrictions"] = option.ToString();
                site.RootWeb.Update();
            }
        }

        private void GetRecordRestrictions(IAveSite site)
        {
            if (site.RootWeb.AllProperties.ContainsKey("ecm_siterecordrestrictions"))
            {
                originalRestrictionsSetting = site.RootWeb.AllProperties["ecm_siterecordrestrictions"].ToString();
            }
        }

        private void UpdateFieldRequired()
        {
            if (needupdateField != null)
            {
                foreach (IAveField field in needupdateField)
                {
                    try
                    {
                        field.Required = false;
                        field.Update();
                        mLog.Debug("set required to false for field : {0} ", field.Title);
                    }
                    catch (Exception ex)
                    {
                        mLog.Info("Field update exception when moving file to SharePoint. Message : {0}.", ex.ToString());
                    }
                }
            }
        }

        private void RevertFieldRequired()
        {
            if (needupdateField != null)
            {
                foreach (IAveField field in needupdateField)
                {
                    try
                    {
                        field.Required = true;
                        field.Update();
                        mLog.Debug("set required to true for field : {0} ", field.Title);
                    }
                    catch (Exception ex)
                    {
                        mLog.Info("Revert field exception when moving file to SharePoint. Message : {0}.", ex.ToString());
                    }
                }
            }
        }

        #region Append
        private AveRestoreMode ResetDocNameIfNeedAppend(AveSPDoc doc, string realName, bool isLeaveDocLink, ref bool? isItemExistInDestination)
        {
            //if (isLeaveDocLink)
            //{
                //switch (linkFileType)
                //{
                //    case LinkFileType.ArchiveAndRemoveLink:
                //        return ResetItemNameIfNeedAppend(config, doc, realName, doc.ResetAvailableNameForRecordLinkFile, doc.ResetName, ref isItemExistInDestination);
                //    case LinkFileType.MoveToAndDeclareLink:
                //        return ResetItemNameIfNeedAppend(config, doc, realName, doc.ResetAvailableNameForLinkFile, doc.ResetName, ref isItemExistInDestination);
                //    default:
                //        return ResetItemNameIfNeedAppend(config, doc, realName, doc.ResetAvailableNameForLinkFile, doc.ResetName, ref isItemExistInDestination);
                //}
                //return ResetItemNameIfNeedAppend(config, doc, realName, doc.ResetAvailableNameForLinkFile, doc.ResetName, ref isItemExistInDestination);
            //}
            //else
            //{
                return ResetItemNameIfNeedAppend(doc, realName, doc.ResetAvailableName, doc.ResetName, ref isItemExistInDestination);
            //}
        }

        private AveRestoreMode ResetItemNameIfNeedAppend(RestoreableObject item, string realName, Func<DateTime, string> ResetAvailableName, Action<string> ResetName, ref bool? isItemExistInDestination)
        {
            bool isThumbnailData = false;
            string newName = string.Empty;
            if (NeedAppend(item, out isThumbnailData))
            {
                if (isThumbnailData)
                {
                    string picName = ChangeThumbnailNameToPicName(realName);
                    if (this.appendMapping.ContainsKeyAppendName(picName))
                    {
                        newName = AppendThumbnailName(this.appendMapping.GetValueAppendName(picName));
                        ResetName(newName);
                    }
                    else//name 保持不变
                    {
                    }
                    item.RestoreOption.ResetRestoreMode((int)AveRestoreMode.OverWrite);
                    isItemExistInDestination = isItemExistInDestination ?? false;
                    return AveRestoreMode.OverWrite;
                }
                else if (!this.appendMapping.ContainsKeyAppendName(realName))
                {
                    newName = ResetAvailableName(DateTime.MinValue);
                    if (realName.Equals(newName, StringComparison.OrdinalIgnoreCase))
                    {
                        this.appendMapping.AddToMappingAppendName(realName, newName);
                        return AveRestoreMode.OverWrite;
                    }
                    else
                    {
                        this.appendMapping.AddToMappingAppendName(realName, newName);
                        ResetName(this.appendMapping.GetValueAppendName(realName));
                    }
                }
                else
                {
                    ResetName(this.appendMapping.GetValueAppendName(realName));
                }
                if (!string.Equals(realName, this.appendMapping.GetValueAppendName(realName), StringComparison.Ordinal))
                {
                    isItemExistInDestination = isItemExistInDestination ?? false;//Append
                    return AveRestoreMode.Append;
                }
                else
                {
                    return AveRestoreMode.Default;
                }
            }
            else if (item.CheckRestoreOption(AveRestoreMode.Append))
            {
                return AveRestoreMode.Default;
            }
            return item.RestoreOption.mAveRestoreMode;
        }

        private string AppendThumbnailName(string realName)
        {
            try
            {
                string name = realName.Substring(0, realName.LastIndexOf('.'));
                string extension = realName.Substring(realName.LastIndexOf('.') + 1);
                return name + '_' + extension + ".jpg";
            }
            catch (Exception e)
            {
                mLog.Warn("An error occurred while append thumbnail name. Name: {0}. Error: {1}", realName, e.ToString());
                return realName;
            }
        }

        private string ChangeThumbnailNameToPicName(string realName)
        {
            try
            {
                string tempName = realName.Substring(0, realName.LastIndexOf('.'));
                string name = tempName.Substring(0, tempName.IndexOf('_'));
                string extension = tempName.Substring(tempName.LastIndexOf('_') + 1);
                return name + '.' + extension;
            }
            catch (Exception e)
            {
                mLog.Warn("An error occurred while change thumbnail name. Name: {0}. Error: {1}", realName, e.ToString());
                return realName;
            }
        }
        private bool IsThumbnail(AveSPDoc doc)
        {
            try
            {
                List<int> targetListTemplates = new List<int> { 109, 851, 2100 };//109 refer to Picture Library,851 refer to images Library,2100 refer to slide Library

                List<string> targetFolders = new List<string> { "_w", "_t" };//Hidden folder where thumbnails file placed
                if (targetFolders.Contains(doc.ParentFolder.Name)) //here we shouldn't use IgnoreCase
                {
                    if (targetListTemplates.Contains(Convert.ToInt32(doc.ParentFolder.ParentList.ListInfo.BaseTemplate)))
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                mLog.Warn("An error occurred while deciding if the document is thumbnail: {0}", e.ToString());
                return false;
            }
        }

        private bool NeedAppend(RestoreableObject itemObject, out bool isThumbnailData)
        {
            isThumbnailData = false;
            if (itemObject is AveSPDoc)
            {
                AveSPDoc doc = itemObject as AveSPDoc;
                if (doc.ParentFolder == null || doc.ParentFolder.ParentList == null)
                {
                    return false;
                }
                if (IsThumbnail(doc))
                {
                    isThumbnailData = true;
                    return true;
                }
                //dont need to append file if itemObject is system file or in system list
                //return !(AppendUtility.CheckIsSystemFile(doc));
                //We Only have Document RM Rule,We can pass system file at scan
                return true;
            }
            return false;
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemSchemaDependency">false means throw Exception if CT Not Exist  </param>
        /// <param name="skipItemWhenConflict">true means throw Exception if CT conflict</param>
        /// <param name="config"></param>
        private void ItemLevelRestoreItemCTAndFields(Dictionary<string, object> userData, RestoreableObject aveObject, ItemDependencyOption itemDependency)
        {
            using (AvePerformanceScope performanceItemLevelRestoreItemCTAndFields = new AvePerformanceScope("RA.ItemRecordManager.ItemLevelRestoreItemCTAndFields"))
            {
                AveSPItem aveSPItem;
                if (aveObject is AveSPDoc)
                {
                    aveSPItem = (aveObject as AveSPDoc).AveSPItem;
                }
                else if (aveObject is AveSPListItem)
                {
                    aveSPItem = (aveObject as AveSPListItem).AveSPItem;
                }
                else if (aveObject is AveSPFolder)
                {
                    aveSPItem = (aveObject as AveSPFolder).EnsureCTFieldItem;
                }
                else
                {
                    throw new ArgumentNullException("Wrong Item Type");
                }

                AveFieldRestoreOption fieldRestoreOptions = new AveFieldRestoreOption();
                bool itemSchemaDependency = false;
                bool skipItemWhenNotFound = false;
                bool skipItemWhenConflict = false;
                fieldRestoreOptions.FindOption = new FieldFindOption[] { FieldFindOption.FindBySchema, FieldFindOption.FindById, FieldFindOption.FindByInternalName, FieldFindOption.FindByStaticName };

                AveContentTypeRestoreOption ContentTypeRestoreOption = new AveContentTypeRestoreOption();
                ContentTypeRestoreOption.FindOption = new ContentTypeFindOption[] { ContentTypeFindOption.FindBySchema, ContentTypeFindOption.FindById, ContentTypeFindOption.FindByName };
                //FindScope Only FindOption = ContentTypeFindOption.FindByParent can be used 
                ContentTypeRestoreOption.FindScope = new ContentTypeFindScope[] { ContentTypeFindScope.Current, ContentTypeFindScope.Parent, ContentTypeFindScope.Children };
                ContentTypeRestoreOption.CreateOption = new ContentTypeCreateOption[] { ContentTypeCreateOption.UseId, ContentTypeCreateOption.ForceCreate, ContentTypeCreateOption.UseParent };
                ContentTypeRestoreOption.GetParentOption = GetParentContentTypeOption.RestoreFamily;

                switch (itemDependency)
                {
                    case ItemDependencyOption.NotRestore:
                        itemSchemaDependency = false;
                        skipItemWhenNotFound = true;
                        skipItemWhenConflict = false;
                        ContentTypeRestoreOption.ConflictHandleOption = ContentTypeConflictHandleOption.None;
                        fieldRestoreOptions.ConflictOption = FieldConflictOption.AppendDestinationWin;
                        break;
                    case ItemDependencyOption.Overwrite:
                        itemSchemaDependency = true;
                        skipItemWhenNotFound = false;
                        skipItemWhenConflict = false;
                        ContentTypeRestoreOption.ConflictHandleOption = ContentTypeConflictHandleOption.Overwrite;
                        fieldRestoreOptions.ConflictOption = FieldConflictOption.Overwrite;
                        break;
                    case ItemDependencyOption.Append:
                        itemSchemaDependency = true;
                        skipItemWhenNotFound = false;
                        skipItemWhenConflict = false;
                        ContentTypeRestoreOption.ConflictHandleOption = ContentTypeConflictHandleOption.Append;
                        fieldRestoreOptions.ConflictOption = FieldConflictOption.AppendSourceWin;
                        break;
                    case ItemDependencyOption.SkipConfilctItem:
                        itemSchemaDependency = true;
                        skipItemWhenNotFound = false;
                        skipItemWhenConflict = true;
                        ContentTypeRestoreOption.ConflictHandleOption = ContentTypeConflictHandleOption.Skip;
                        fieldRestoreOptions.ConflictOption = FieldConflictOption.Skip;
                        break;
                }
                try
                {
                    if (itemDependency == ItemDependencyOption.NotRestore)
                    {
                        aveSPItem.EnsureItemContentTypeDependency(userData, itemSchemaDependency, skipItemWhenNotFound, skipItemWhenConflict, ContentTypeRestoreOption);
                    }
                    else
                    {
                        aveSPItem.EnsureItemSchemaDependency(userData, null, itemSchemaDependency, skipItemWhenNotFound, skipItemWhenConflict, ContentTypeRestoreOption, fieldRestoreOptions);
                    }
                }
                catch (AveSchemaDependencyConflictException ce)
                {
                    throw new SkipException(ce.Message, ce);
                }
                catch (AveSchemaDependencyNotFoundException ne)
                {
                    throw new SkipException(ne.Message, ne);
                }
                catch(Exception ex)
                {
                    mLog.Error(string.Format("Error in restore content type and fields, reason : {0}", ex.ToString()));
                    throw;
                }
            }
        }

        /// <summary>
        /// Restore Document Metadata Dto
        /// </summary>
        /// <param name="fileRestoreOption"></param>
        /// <param name="documentMetadataDto"></param>
        /// <param name="restoreStream"></param>
        /// <returns></returns>
        private void RestoreDocumentMetadataDto(IAveRestoreStream restoreStream, AveSPDoc doc, AveSPDocumentMetadataDto documentMetadataDto, bool restorePermission = false)
        {
            using (AvePerformanceScope performanceRestoreDocumentMetadataDto = new AvePerformanceScope("RecordOnline.ItemRecordManager.RestoreDocumentMetadataDto"))
            {
                doc.SetStream(restoreStream);
                //doc.ParentSite.SPMembers.RestoreUsers(documentMetadataDto.UserCache.Users,
                //    new MembersRestoreOption()
                //    {
                //        IsSiteLevel = false,
                //        OverWrite = true,
                //        SkipWithoutPermissions = false,
                //        NeedDeleteUser = true
                //    });
                doc.ParentSite.SPMembers.RestoreUsers(documentMetadataDto.UserCache.Users, false, false, false);
                doc.ParentSite.SPMembers.RestoreGroups(documentMetadataDto.GroupCache.Groups);
                if (documentMetadataDto.MetadataInfo != null)
                {
                    doc.ParentSite.MetadataService.Restore(documentMetadataDto.MetadataInfo);
                }
                AveRestoreResult result = AveRestoreResult.Normal;
                //this.VerifyItemMetadataDependency(documentMetadataDto, fileRestoreOption);//
                //ADO-136115，原端Version<目的端当前Version,cache wapper异常，skip 
                try
                {
                    result = this.RestoreDocument(doc, documentMetadataDto, restorePermission);
                    if (restorePermission)
                    {
                        this.RestorePermission(doc, restoreStream);
                    }
                }
                catch (AveWrapperSkipException e)
                {
                    mLog.Debug("This is AveWrapperSkipException,Need Out Skip. Exception:{0}", e.Message);
                    throw new ConetentSkipException(I18NString.SkipRestoreObject);
                }
                //当源端version在目的端能find到或源端文件modify time和目的端modify time相同，skip
                if (result == AveRestoreResult.Omit || result == AveRestoreResult.SkipTheSameItem)
                {
                    mLog.Debug("AveRestoreResult is : {0}", result.ToString());
                    throw new ConetentSkipException(I18NString.SkipRestoreObject);
                }
            }
        }

        private AveRestoreResult RestoreDocument(AveSPDoc doc, AveSPDocumentMetadataDto documentMetadataDto, bool isRAJob = false)
        {
            if (doc == null)
            {
                mLog.Error("doc is null when restore document");
                throw new Exception("doc is null ");
            }
            var baseTemplate = (int?)doc?.ParentFolder?.ParentList?.SPList?.BaseTemplate;
            if (baseTemplate == null)
            {
                mLog.Error("baseTemplate is null when restore document");
                throw new Exception("baseTemplate is null ");
            }
            /*
            * 2100是slide library，这个library必须关闭才好用
            */
            using (new AveEventReceiverUtility((int)doc.ParentFolder.ParentList.SPList.BaseTemplate == 2100))
            {
                var restoreResult = doc.RestoreSelf(documentMetadataDto.DocInfo_Old, documentMetadataDto.UserDataInfo,
                                                     documentMetadataDto.DocDataJunction, documentMetadataDto.WebParts);

                #region Declare File restore lookup column need undeclare first.
                //Office 365 Declare File status is Declare when restore,so we need undeclare first then restore look up column.
                if (isRAJob && documentMetadataDto.ItemTPGUIDofLookupValue != null && documentMetadataDto.ItemTPGUIDofLookupValue.Count != 0)
                {
                    try
                    {
                        IAveFile file = aveSPWeb.SPWeb.GetFile(destinationContainerUrl + fileName);
                        IAveListItem newItem = file.Item;
                        //REC-2432 Host Header Site Collection通过IAveFile GetFile(string serverRelativeUrl);方式获取不到IAveListItem对象.
                        if (newItem == null)
                        {
                            mLog.Debug("Current IAveListItem is null and will ReGet IAveListItem by List GetItemByUniqueId.");
                            newItem = aveSPList.SPList.GetItemByUniqueId(file.UniqueId);
                            mLog.Info("ReGet IAveListItem successful by List GetItemByUniqueId. IAveListItem is null:{0}.", newItem == null);
                        }
                        //if (ScheduleConfiguration.CheckisRecord(file.Item))
                        //{
                        //    mLog.Debug("Current file is declare file,file name:{0}.", file.Name);
                        //    record.UndeclareItemAsRecord(file.Item);
                        //}
                    }
                    catch (Exception ex)
                    {
                        mLog.Info("Can not UnDeclareItemAsRecord when RestoreLookupFieldGuidValue,Message:{0}.", ex.Message);
                    }
                }
                #endregion

                doc.AveSPItem?.RestoreLookupFieldGuidValue(documentMetadataDto.ItemTPGUIDofLookupValue);

                return restoreResult;
            }
        }

        private void RestorePermission(AveSPDoc doc, IAveRestoreStream restoreStream)
        {
            try
            {
                mLog.Debug("Begin to restoring archiver document Users and Groups.");
                List<AveMetadata> archiveUserCacheMetadata = restoreStream.TryReadMetadataList(AveMetadataType.UserCache);
                if (archiveUserCacheMetadata != null)
                {
                    foreach (var user in archiveUserCacheMetadata)
                    {
                        var userList = user.GetMetadata<AveUserList>();
                        if (userList != null)
                        {
                            foreach (AveUserInfo userInfo in userList.Users)
                            {
                                doc.ParentSite.SPMembers.RestoreUser(userInfo);
                            }
                        }
                    }
                }
                List<AveMetadata> groupCacheMetadata = restoreStream.TryReadMetadataList(AveMetadataType.GroupCache);
                if (groupCacheMetadata != null)
                {
                    foreach (var groups in groupCacheMetadata)
                    {
                        var groupList = groups.GetMetadata<AveGroupList>();
                        if (groupList != null)
                        {
                            foreach (AveGroupInfo group in groupList.Groups)
                            {
                                doc.ParentSite.SPMembers.RestoreGroup(group);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("An error occurred while restoring archiver document Groups. Exception: {0}", ex.ToString());
            }
            finally
            {
                mLog.Debug("End to restoring archiver document Users and Groups.");
            }
            AveMetadata metadata;
            while ((metadata = restoreStream.ReadMetadata()) != null)
            {
                switch (metadata.MetadataType)
                {
                    case AveMetadataType.RoleAssignment:
                        {
                            var roleAssignments = metadata.GetMetadata<List<AveRoleAssignmentInfo>>();
                            AveObjectSecurity security = AveObjectSecurity.CreateInstance(this);
                            //IAveObjectSecurity security = this.RestoreOMFactory.CreateAveObjectSecurity(aveDoc);
                            security.SourceHasUniqueRoleAssignment = doc.AveSPItem.HasUniqueRoleAssignments;
                            security.RestoreRoleAssignments(roleAssignments, new SecurityRestoreOption() { ConflictResolutionForSecurityObject = ConflictResolutionForSecurityObject.OverWrite, ConflictResolutionForPincipal = ConflictResolutionForPincipal.OverWrite });
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void PostAction()
        {
            try
            {
                if (!string.IsNullOrEmpty(originalRestrictionsSetting))
                {
                    var recordRestrictions = (RecordRestrictions)Enum.Parse(typeof(RecordRestrictions), originalRestrictionsSetting);
                    SetRecordRestrictions(site, recordRestrictions);
                }
            }
            catch(Exception ex)
            {
                mLog.Warn(string.Format("Error in post action, error message is : {0}", ex.ToString()));
            }
            try
            {
                RevertFieldRequired();
            }
            catch(Exception revertExc)
            {
                mLog.Warn(string.Format("Error in revert fields, reason : {0}", revertExc.ToString()));
            }
            try
            {
                mLog.Info("Begin Process List Post Action.");
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
        }

        private void SetAutoDeclareRecordsTrue()
        {
            if (AutoDeclareRecordsChange && aveSPList != null)
            {
                aveSPList.RootFolder.Properties["ecm_AutoDeclareRecords"] = "True";
                aveSPList.RootFolder.Update();
                AutoDeclareRecordsChange = false;
            }
        }

        private void SetAutoDeclareRecordsFalse()
        {
            if (!AutoDeclareRecordsChange && aveSPList != null)
            {
                aveSPList.RootFolder.Properties["ecm_AutoDeclareRecords"] = "False";
                aveSPList.RootFolder.Update();
                AutoDeclareRecordsChange = true;
            }
        }

        public void Dispose()
        {
            PostAction();
            //sp object here do not neet to dispose ,we must use it next document and we dispose it in the end
            DisposeObj(aveSPWeb);
            DisposeObj(aveSPSite);
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
    public static class AveSPDocExtension
    {
        private static RALogger logger = RALogger.GetInstance(typeof(AveSPDocExtension));

        public static string ResetAvailableNameForLinkFile(this AveSPDoc spDoc, DateTime timeLastModified)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("RA.Restore.AveSPDoc.ResetAvailableNameForLinkFile"))
            {
                string newFileName = spDoc.Name;
                try
                {
                    DateTime destTimeLastModified = DateTime.MinValue;
                    if (!CheckFileExist(spDoc, newFileName, ref destTimeLastModified))
                    {
                        return newFileName;
                    }

                    if (destTimeLastModified == DateTime.MinValue || destTimeLastModified != timeLastModified)
                    {
                        string extension = string.Empty;
                        string prevName = newFileName;
                        int pos = newFileName.LastIndexOf(".stub.aspx");
                        if (pos > 0)
                        {
                            extension = newFileName.Substring(pos, newFileName.Length - pos);
                            prevName = newFileName.Substring(0, pos);
                        }
                        for (int i = 1; i <= 1000; ++i)
                        {
                            StringBuilder temp = new StringBuilder(prevName);
                            temp.Append("_");
                            temp.Append(i.ToString());
                            temp.Append(extension);

                            if (!CheckFileExist(spDoc, temp.ToString(), ref destTimeLastModified))
                            {
                                newFileName = temp.ToString();
                                break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Warn(string.Format("Error in ResetAvailableNameForLinkFile, error message:{0}", e.ToString()));
                }
                return newFileName;
            }

        }

        public static string ResetAvailableNameForRecordLinkFile(this AveSPDoc spDoc, DateTime timeLastModified)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("RA.Restore.AveSPDoc.ResetAvailableNameForRecordLinkFile"))
            {
                string newFileName = spDoc.Name;
                try
                {
                    DateTime destTimeLastModified = DateTime.MinValue;
                    if (!CheckFileExist(spDoc, newFileName, ref destTimeLastModified))
                    {
                        return newFileName;
                    }

                    if (destTimeLastModified == DateTime.MinValue || destTimeLastModified != timeLastModified)
                    {
                        string extension = string.Empty;
                        string prevName = newFileName;
                        int pos = newFileName.LastIndexOf(".aspx");
                        if (pos > 0)
                        {
                            extension = newFileName.Substring(pos, newFileName.Length - pos);
                            prevName = newFileName.Substring(0, pos);
                        }
                        for (int i = 1; i <= 1000; ++i)
                        {
                            StringBuilder temp = new StringBuilder(prevName);
                            temp.Append("_");
                            temp.Append(i.ToString());
                            temp.Append(extension);

                            if (!CheckFileExist(spDoc, temp.ToString(), ref destTimeLastModified))
                            {
                                newFileName = temp.ToString();
                                break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Warn(string.Format("Error in ResetAvailableNameForRecordLinkFile, error message:{0}", e.ToString()));
                }
                return newFileName;
            }

        }

        private static bool CheckFileExist(AveSPDoc spDoc, string fileName, ref DateTime lastModifiedTime)
        {
            var fileServerRelativeUrl = string.Format("{0}/{1}", spDoc.ParentFolder.ServerRelativeUrl.TrimEnd('/'), fileName);
            IAveFile file = spDoc.ParentFolder.ParentList.ParentWeb.SPWeb.GetCheckoutFile(fileServerRelativeUrl);
            if (file != null && file.Exists)
            {
                lastModifiedTime = (file.Item == null) ? file.TimeLastModified : ((DateTime)file.Item["Modified"]).ToUniversalTime();
                return true;
            }
            //if (this.mAveParentSite.ObjectModelFactory.ContextKind != AveContextKind.ClientObjectModel && this.mParentFolder.RestoringItem.IsIncludingRecycleBinData)
            //{
            //    RestoringDto dto = new RestoringDto();
            //    dto.NameMapping = fileName;
            //    mQueryService.CheckConflictInfo(mDocumentInfo.SiteId, mDocumentInfo.ParentId, dto);
            //    if (dto.ConflictType == ConflictType.RecycleBin)
            //    {
            //        return true;
            //    }
            //}
            //[ADO-126223]注释此处用于解决skip+Append+考虑recycle bin的时候还原Document不删除回收站且File Name变化还原的现象。
            return false;

        }
    }

    public static class IAveFileExtension
    {
        private static RALogger logger = RALogger.GetInstance(typeof(IAveFileExtension));

        public static string ResetAvailableName(this IAveFile aveFile)
        {
            return ResetAvailableName(aveFile, DateTime.MinValue);
        }

        public static string ResetAvailableName(this IAveFile aveFile, DateTime timeLastModified)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("RA.Restore.IAveFile.ResetAvailableName"))
            {
                string newFileName = aveFile.Name;
                try
                {
                    DateTime destTimeLastModified = DateTime.MinValue;
                    if (!CheckFileExist(aveFile, newFileName, ref destTimeLastModified))
                    {
                        return newFileName;
                    }

                    if (destTimeLastModified == DateTime.MinValue || destTimeLastModified != timeLastModified)
                    {
                        string extension = string.Empty;
                        string prevName = newFileName;
                        int pos = newFileName.LastIndexOf(".");
                        if (pos > 0)
                        {
                            extension = newFileName.Substring(pos, newFileName.Length - pos);
                            prevName = newFileName.Substring(0, pos);
                        }
                        for (int i = 1; i <= 1000; ++i)
                        {
                            StringBuilder temp = new StringBuilder(prevName);
                            temp.Append("_");
                            temp.Append(i.ToString());
                            temp.Append(extension);

                            if (!CheckFileExist(aveFile, temp.ToString(), ref destTimeLastModified))
                            {
                                newFileName = temp.ToString();
                                break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Warn(string.Format("Error in ResetAvailableName, error message:{0}", e.ToString()));
                }
                return newFileName;
            }

        }

        private static bool CheckFileExist(IAveFile aveFile, string fileName, ref DateTime lastModifiedTime)
        {
            var fileServerRelativeUrl = string.Format("{0}/{1}", aveFile.ParentFolder.ServerRelativeUrl.TrimEnd('/'), fileName);
            IAveFile file = aveFile.Web.GetCheckoutFile(fileServerRelativeUrl);
            if (file != null && file.Exists)
            {
                lastModifiedTime = (file.Item == null) ? file.TimeLastModified : ((DateTime)file.Item["Modified"]).ToUniversalTime();
                return true;
            }
            //if (this.mAveParentSite.ObjectModelFactory.ContextKind != AveContextKind.ClientObjectModel && this.mParentFolder.RestoringItem.IsIncludingRecycleBinData)
            //{
            //    RestoringDto dto = new RestoringDto();
            //    dto.NameMapping = fileName;
            //    mQueryService.CheckConflictInfo(mDocumentInfo.SiteId, mDocumentInfo.ParentId, dto);
            //    if (dto.ConflictType == ConflictType.RecycleBin)
            //    {
            //        return true;
            //    }
            //}
            //[ADO-126223]注释此处用于解决skip+Append+考虑recycle bin的时候还原Document不删除回收站且File Name变化还原的现象。
            return false;

        }

        public static IAveFile ReloadFile(this IAveFile file)
        {
            return file.Web.GetFile(file.ServerRelativeUrl);
        }
    }
}
