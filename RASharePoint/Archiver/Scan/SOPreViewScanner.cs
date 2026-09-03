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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common.JobExecutionProgress;
using AvePoint.RA.SharePoint.Discover.Base;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Discovery;
using Azure.ResourceManager.Resources;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LOGRESOURCE = Merged18NResources.Archive.Archive;

namespace AvePoint.RA.SharePoint.Archiver.Scan
{
    internal class SOPreViewScanner : ArchiverSharePointScanner
    {
        private static readonly RALogger mLog = RALogger.GetInstance(typeof(SOPreViewScanner));
        private IRMArchiverSettingDao ArchiverSettingDao => PlatformWindsorManager.GetService<IRMArchiverSettingDao>();
        public SOPreViewScanner(ScanJobSettings scanJobSettings) : base(scanJobSettings)
        {
        }

        public override async System.Threading.Tasks.Task ProcessSiteCollectionAsync(ArchiverNodeItem sitecollection)
        {
            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    try
                    {
                        await InitialSPObjectInfoAsync(discoverWorker, sitecollection);
                        //If the rootWeb has defined a unique rule, we should skip all the site collection.
                        //URL of RootWeb is same as sitecollection's
                        IAveSite tmpSite = mDependencyObjs.ValueInCacheOfLevel((int)CacheNodeType.SiteCollection) as IAveSite;

                        ProcessResult result = (await discoverWorker.ProcessContainerAsync(sitecollection, ProcessType.NeedProcess));
                        if (result == ProcessResult.SkipCurrentNode)
                        {
                            mLog.Info("skip current Node {0}", sitecollection.FullPath);
                            return;
                        }
                        else if (result == ProcessResult.FitRule)
                        {
                            mLog.Info("current Node fit rule {0}", sitecollection.FullPath);
                            return;
                        }

                        using (AveDiscoverSite discoverySite = sitecollection.DiscoverSPObject as AveDiscoverSite)
                        {
                            using (AveDiscoverWeb rootWeb = discoverySite.GetRootWeb())
                            {
                                if (discoverWorker.IsRuleBreakInheritNode(ArchiverCommonStaticMethod.GetBreakInheritSHA1String(sitecollection.FullPath)))
                                {
                                    var setting = ArchiverSettingDao.LoadArchiverSetting(rootWeb.WebID, sitecollection.ID);
                                    if (setting != null)
                                    {
                                        mLog.Warn("root web {0} is break inherit from parent", rootWeb.FullUrl);
                                        return;
                                    }
                                }
                                using (ArchiverNodeItem webnode = sitecollection.GenerateSiteNodeItem(rootWeb, mConfiguration, true))
                                {
                                    string rootWebSiteLogoDescription = rootWeb.AveWeb.SiteLogoDescription;//通过调用SiteLogoDescription自动创建出Site Assets List 
                                    await ProcessWebAsync(webnode);
                                }
                            }
                        }

                    }
                    catch (AveWrapperI18NException IUPEx)
                    {
                        mLog.Info("Site Collection UserName Or Password Incorrect. Path:{0}. Message:{1}.", sitecollection.FullPath, IUPEx.ToString());
                        throw;
                    }
                    catch (SPObjectReadOnlyException snfe)
                    {
                        mLog.Info("Site Collection is ReadOnly. Path:{0}. Message:{1}.", sitecollection.FullPath, snfe.ToString());

                        throw;
                    }
                    catch (SPObjectLockedException sle)
                    {
                        mLog.Info("Site Collection is Locked. Path:{0}. Message:{1}.", sitecollection.FullPath, sle.ToString());

                        throw;
                    }
                    catch (SPObjectNotFoundException ex)
                    {
                        mLog.Info("Site Collection Not Found. Path:{0}. Message:{1}.", sitecollection.FullPath, ex.ToString());
                        throw;
                    }
                    catch (Exception ex)
                    {
                        if (ex.InnerException != null && ex.InnerException.Message.Contains("The site do not meet the conditions."))
                        {
                            mLog.Error(string.Format("AveLATMgtApiNotEnabledException in Backup Site Collection :{0}.Site Collection Path:{1}.", ex.ToString(), sitecollection.FullPath));
                        }
                        else
                        {
                            mLog.Error("An unexpected error occurred while processing site collection node.Path:{0}.Message:{1}.", sitecollection.FullPath, ex);
                        }
                        throw;
                    }
                    finally
                    {
                        JobExecutionProgressStatisticExecutor.Instance.IncreaseOtherItems(Contract.RMWeb.JobMonitor.ActionTab.Scan, (int)CacheNodeType.SiteCollection, 0);
                    }
                }
            }
            catch (JobStopException)
            {
                throw;
            }
            catch (Exception e)
            {
                mLog.Error("Process sitecollection error {0}", e.ToString());
                if (e is AveExceedStorageLimitException)
                {
                    mConfiguration.JobReportDto.AddScanReport(sitecollection.SiteUrl, 0, (int)CacheNodeType.SiteCollection, "", JobDetailsStatus.Failed, "RM_JM_SiteStorageLimit_ErrorMessage");
                }
                //TO DO Add Detail
                //TO DO I18N
                //base.AddDetail(curNodeInfo.Title, curNodeInfo.Url, string.Empty,
                //    string.Empty, string.Empty, JobReportDetailStatus.Failed, e.Message);
            }
        }

        public override async System.Threading.Tasks.Task ProcessWebAsync(ArchiverNodeItem web, bool needInitInfo = false)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessWeb"))
            {
                try
                {
                    using (new CheckJobStopScope()) { }
                    if (needInitInfo)
                    {
                        await InitialSPObjectInfoAsync(discoverWorker, web);
                    }
                    else
                    {
                        IAveSite tmpSite = mDependencyObjs.ValueInCacheOfLevel((int)CacheNodeType.SiteCollection) as IAveSite;
                        if (mConfiguration.mInitialTime.AddHours(23) < DateTime.Now)
                        {
                            mLog.Info("The SPSite id Time out, New it again");
                            string mSiteUrl = tmpSite.Url;
                            tmpSite.Dispose();
                            mConfiguration.mInitialTime = DateTime.Now;
                            //tmpSite = new SPSite(mSiteUrl);
                            AveObjectModelFactory factory = mConfiguration.aveObjectModelFactory;

                            tmpSite = factory.CreateSite(mSiteUrl);
                            mDependencyObjs.PutIn(tmpSite, (int)CacheNodeType.SiteCollection, false);
                        }
                        IAveWeb tmpWeb = tmpSite.OpenWeb(web.ID);
                        if (tmpWeb == null)
                        {
                            throw new SPObjectNotFoundException(LOGRESOURCE.StorageOptimization13_SOARScanProcessWebSPObjectNotFoundException, "Site", web.FullPath);
                        }
                        //TODO:Disable language mapping
                        //ScheduleLanguageMapping.ProcessLanguageMapping(tmpWeb);
                        mDependencyObjs.PutIn(tmpWeb, (int)CacheNodeType.Web, false);
                    }
                    ProcessResult result = await discoverWorker.ProcessContainerAsync(web, ProcessType.NeedProcess);
                    if (result == ProcessResult.SkipCurrentNode)//web 级别 符合 web rule
                    {
                        return;
                    }
                    else if (result == ProcessResult.FitRule)
                    {
                        mLog.Info("current Node fit rule {0}", web.FullPath);
                        return;
                    }
                    //else if (result != ProcessResult.SkipListNode)
                    //{
                    //    await ProcessListCollectionAsync(web);
                    //}
                    await ProcessListCollectionAsync(web, result == ProcessResult.SkipListNode);
                    //Process web
                    await base.ProcessWebCollectionAsync(web);
                }
                catch (AveWrapperI18NException IUPEx)
                {
                    mLog.Info("Web UserName Or Password Incorrect. Path:{0}. Message:{1}.", web.FullPath, IUPEx.ToString());
                    throw;
                }
                catch (SPObjectReadOnlyException snfe)
                {
                    mLog.Info("Web is ReadOnly. Path:{0}. Message:{1}.", web.FullPath, snfe.ToString());
                    throw;
                }
                catch (SPObjectLockedException sle)
                {
                    mLog.Info("Web is Locked. Path:{0}. Message:{1}.", web.FullPath, sle.ToString());
                    throw;
                }
                catch (SPObjectNotFoundException ex)
                {
                    mLog.Info("Web Not Found. Path:{0}. Message:{1}.", web.FullPath, ex.ToString());
                    throw;
                }
                catch (Exception e)
                {
                    mLog.Error("An unexpected error occurred while processing web node.Path:{0}. Message:{1}.", web.FullPath, e.ToString());
                    throw;
                }
                finally
                {
                    JobExecutionProgressStatisticExecutor.Instance.IncreaseOtherItems(Contract.RMWeb.JobMonitor.ActionTab.Scan, (int)CacheNodeType.Web, 0);
                }
            }
        }

        public override async System.Threading.Tasks.Task ProcessListAsync(ArchiverNodeItem list, bool needInitInfo = false)
        {
            mLog.Info("Begin process list,title is:{0}.", list.Title);
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessList"))
            {
                try
                {
                    using (new CheckJobStopScope()) { }
                    if (ListSkipCheck(list))
                    {
                        return;
                    }

                    if (needInitInfo)
                    {
                        await InitialSPObjectInfoAsync(discoverWorker, list);
                        OutPutListItemCount(new()
                        {
                            { list.ListId, list.DiscoverSPObject as AveDiscoverList }
                        });
                    }

                    CheckAccessableForUserInfoList(list);

                    ProcessResult result = await discoverWorker.ProcessContainerAsync(list, ProcessType.NeedProcess);
                    if (result == ProcessResult.SkipCurrentNode)
                    {
                        return;
                    }
                    else if (result == ProcessResult.FitRule)
                    {
                        mLog.Info("current Node fit rule {0}", list.FullPath);
                        return;
                    }

                    if (IrmLeaveStubListSkipHelper.TryGetListLevelMatchedRule(mConfiguration, list.SPList, out var matchedRule))
                    {
                        mLog.Info(
                            "Skip list for leave-stub IRM restriction in SOPreScan. ListTitle:{0}, RuleId:{1}, RuleName:{2}, KeepDataOption:{3}, PolicyLevel:{4}, IrmEnabled:{5}, IrmReject:{6}.",
                            list.Title,
                            matchedRule?.Id,
                            matchedRule?.Name,
                            matchedRule?.KeepDataOption,
                            matchedRule?.PolicyLevel,
                            list.SPList?.IrmEnabled,
                            list.SPList?.IrmReject);

                        mConfiguration.JobReportDto.AddScanReport(
                            list.FullPath,
                            0,
                            (int)CacheNodeType.List,
                            string.Empty,
                            Contract.RMWeb.JobMonitor.JobDetailsStatus.Skipped,
                            IrmLeaveStubListSkipHelper.SkipReportMessageKey);
                        return;
                    }

                    AveDiscoverFolder rootFolder = null;
                    if (NeedDiscoverWithSPQuery(list.SPList))
                    {
                        try
                        {
                            mLog.Info("List Begin SPQuery to filter data. Path:{0}.", list.FullPath);
                            InitForSPQueryDiscover(list.SPList);
                            InitArchiverSPQueryRootFolder(list.SPList.RootFolder.ServerRelativeUrl);
                            if (SPORootFolder != null && SPORootFolder.SubFolders != null && SPORootFolder.SubFolders.Count > 0)
                            {
                                InitArchiverSPQueryFolderStructure(list.SPList.RootFolder.ServerRelativeUrl);
                            }
                            rootFolder = (list.DiscoverSPObject as AveDiscoverList).GetRootFolderForArchiverSPQuery(SPORootFolder);
                        }
                        catch (Exception ex)
                        {
                            mLog.Info("Can not use SPQuery to filter data and change query to Full Scan. Path:{0}. Message:{1}.", list.FullPath, ex.ToString());
                            ReleaseForSPQueryDiscover();
                            rootFolder = (list.DiscoverSPObject as AveDiscoverList).GetRootFolder(true);
                        }
                    }
                    else
                    {
                        rootFolder = (list.DiscoverSPObject as AveDiscoverList).GetRootFolder(true);
                    }
                    ArchiverNodeItem foldernode = list.GenerateFolderNodeItem(rootFolder, GCommon.Contract.Tree.Object.NodeLevel.RootFolder, mDiscoverSite.Site.Url, mConfiguration);
                    await ProcessFolderAsync(foldernode);
                }
                catch (AveWrapperI18NException IUPEx)
                {
                    mLog.Info("List UserName Or Password Incorrect. Path:{0}. Message:{1}.", list.FullPath, IUPEx.ToString());
                    throw;
                }

                catch (SPObjectReadOnlyException sroe)
                {
                    mLog.Info("List is ReadOnly. Path:{0}. Message:{1}.", list.FullPath, sroe.ToString());

                    throw;
                }
                catch (SPObjectLockedException sle)
                {
                    mLog.Info("List is Locked. Path:{0}. Message:{1}.", list.FullPath, sle.ToString());
                    throw;
                }
                catch (SPObjectNotFoundException ex)
                {
                    mLog.Info("List Not Found. Path:{0}. Message:{1}.", list.FullPath, ex.ToString());
                    throw;
                }
                catch (Exception e)
                {
                    if ((e.InnerException is ServerUnauthorizedAccessException) && (list.DiscoverSPObject as AveDiscoverList)?.ListTemplate == (int)AveListTemplateType.UserInformation)
                    {
                        mLog.Info("Skip the user info list {0}", list.FullPath);
                        mConfiguration.JobReportDto.AddScanReport(list.FullPath, 0, (int)CacheNodeType.List, "", Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed, e.InnerException.Message);
                        mConfiguration.JobReportDto.HasErrorNode = true;
                        throw;
                    }
                    else
                    {
                        mLog.Error("An unexpected error occurred while processing list node.Path:{0}.Message:{1}.", list.FullPath, e.ToString());
                        mConfiguration.JobReportDto.AddScanReport(list.FullPath, 0, (int)CacheNodeType.List, string.Empty, Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed, errorMessage: e.Message);
                        mConfiguration.JobReportDto.HasErrorNode = true;
                        throw;
                    }
                }
                finally
                {
                    if (needInitInfo)
                    {
                        JobExecutionProgressStatisticExecutor.Instance.IncreaseScannedFiles((list.DiscoverSPObject as AveDiscoverList).ItemCount);
                    }
                    mConfiguration.ProgressDto.UpdateProgress();
                }
            }
        }

        public override async System.Threading.Tasks.Task ProcessFolderAsync(ArchiverNodeItem folder, bool needInitInfo = false, List<int> itemIDs = null)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessFolder"))
            {
                try
                {
                    using (new CheckJobStopScope()) { }
                    if (folder.Parent != null && ListSkipCheck(folder.Parent))
                    {
                        return;
                    }

                    //Initialize parent node
                    if (needInitInfo)
                    {
                        await InitialSPObjectInfoAsync(discoverWorker, folder);
                    }
                    ProcessResult result = await discoverWorker.ProcessContainerAsync(folder, ProcessType.NeedProcess);
                    if (result == ProcessResult.SkipCurrentNode)//add for RevIM RECO-84
                    {
                        return;
                    }
                    else if (result == ProcessResult.FitRule)
                    {
                        mLog.Info("current Node fit rule {0}", folder.FullPath);
                        return;
                    }
                    await ProcessItemsAndSubfoldersAsync(folder, folder.Cache_NodeType, itemIDs);

                }
                catch (AveWrapperI18NException IUPEx)
                {
                    mLog.Info("Folder UserName Or Password Incorrect. Path:{0}. Message:{1}.", folder.FullPath, IUPEx.ToString());
                    throw;
                }
                catch (SPObjectReadOnlyException sroe)
                {
                    mLog.Info("Folder is ReadOnly. Path:{0}. Message:{1}.", folder.FullPath, sroe.ToString());
                    throw;
                }
                catch (SPObjectLockedException sle)
                {
                    mLog.Info("Folder is Locked. Path:{0}. Message:{1}.", folder.FullPath, sle.ToString());
                    throw;
                }
                catch (SPObjectNotFoundException ex)
                {
                    mLog.Info("Folder Not Found. Path:{0}. Message:{1}.", folder.FullPath, ex.ToString());
                    throw;
                }
                catch (Exception e)
                {
                    mLog.Error("An unexpected error occurred while processing folder node.Path:{0}.Message:{1}.", folder.FullPath, e.ToString());
                    //throw; 非特定异常Folder Scan失败，不应该影响整体Job状态，Folder失败即可。SAAS-38055
                }
            }
        }

        public override async System.Threading.Tasks.Task ProcessItemsAndSubfoldersAsync(ArchiverNodeItem folderNode, int folderLevel, List<int> itemIDs = null, bool needInitInfo = false)
        {
            using (new CheckJobStopScope()) { }
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.RealProcessItemsAndSubfolders"))
            {
                AveDiscoverFolder rootFolder = (folderNode.DiscoverSPObject as AveDiscoverFolder);
                #region process items/documents
                int totalItemCount = rootFolder.GetItemCount();
                try
                {
                    if (needInitInfo)
                    {
                        JobExecutionProgressStatisticExecutor.Instance.IncreaseTotalFiles(totalItemCount);
                    }
                    if (mConfiguration.SkipDiscoverItemForFolderLevelRule)
                    {
                        mLog.Info("Current rule is folder rule and skip discover folder sub items.Path:{0}.", folderNode.FullPath);
                    }
                    else
                    {
                        foreach (var items in rootFolder.GetItemsWithStructureForArchiver())
                        {
                            mLog.Info("Current GetItemsWithStructureForArchiver Items Count:{0}.", items.Count);
                            await ProcessDataAsync(items, itemIDs, rootFolder, folderNode, discoverWorker);
                            rootFolder.ClearSubItemsCache();
                        }
                    }
                }
                catch (Exception ex)
                {
                    mLog.Error("An error occurred while RealProcessItemsAndSubfolders.Path:{0}.Message:{1}.", folderNode.FullPath, ex.ToString());
                }
                finally
                {
                    JobExecutionProgressStatisticExecutor.Instance.IncreaseOtherItems(Contract.RMWeb.JobMonitor.ActionTab.Scan, (int)CacheNodeType.Folder, 0);
                    if (needInitInfo)
                    {
                        JobExecutionProgressStatisticExecutor.Instance.IncreaseScannedFiles(totalItemCount);
                    }
                }
                #endregion

                #region process folders
                try
                {
                    foreach (var folders in rootFolder.GetFoldersWithStructure(true))
                    {
                        mLog.Info("Curent GetFoldersWithStructure folders Count:{0}.", folders.Count);
                        var folderIds = folders.Where(x => x.ID != null).Count() != 0 ? folders.Where(x => x.ID != null).Select(x => x.ID.Value).ToList() : new List<int>();
                        await ProcessDataAsync(folders, itemIDs, folderNode, discoverWorker, needInitInfo);
                        rootFolder.ClearSubFoldersCache();
                        //Remove IAveFolder Cache.每次Query出的Folder外围处理结束后，清除当次Query缓存的IAveFolder，避免造成内存问题.
                        mLog.Info("Begin remove folder cache GetFoldersWithStructurForArchiver.RemomveCount:{0}.FullPath:{1}.", folderIds.Count, folderNode.FullPath);
                        rootFolder.RemoveFolderCache(folderIds);
                    }
                }
                catch (Exception ex)
                {
                    mLog.Error("An error occurred while RealProcessItemsAndSubfolders.Path:{0}.Message:{1}.", folderNode.FullPath, ex.ToString());
                }
                #endregion
                if (rootFolder != null)
                {
                    rootFolder.Dispose();
                }
            }
        }
        internal override async System.Threading.Tasks.Task ProcessVersionAndAttachmentsAsync(AveDiscoverItem item, AveDiscoverFolder rootFolder, ArchiverNodeItem folderNode, IDiscoverNodeWorker discoverWorker)
        {
            using (new CheckJobStopScope()) { }
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessVersionAndAttachments"))
            {
                using (ArchiverNodeItem itemNode = folderNode.GenerateItemNodeItem(item, rootFolder, mConfiguration))
                {
                    if(int.TryParse(itemNode.Author, out var creId))
                    {
                        itemNode.Author = item.GetUserInfoById(creId)?.LoginName ?? string.Empty;
                    }
                    if (int.TryParse(itemNode.Editor, out var modId))
                    {
                        itemNode.Editor = item.GetUserInfoById(modId)?.LoginName ?? string.Empty;
                    }

                    ProcessResult result = await discoverWorker.ProcessItemAsync(itemNode, folderNode);
                    if (result == ProcessResult.CurrentVersionHasApprove)
                    {
                        return;
                    }

                    Stopwatch watch = Stopwatch.StartNew();
                    //Progress attachments 
                    if (item.GetAttachments().Count > 0)
                    {
                        foreach (AveItemObject attachment in item.GetAttachments())
                        {
                            await ProcessAttachmentsAsync(folderNode, itemNode, attachment, discoverWorker);
                        }
                    }

                    List<AveVersionObject> versions = GetScanableVersionOfItem(item, itemNode);

                    //Progress item versions
                    if (versions.Count > 0)
                    {
                        foreach (AveVersionObject version in versions)
                        {
                            try
                            {
                                await ProcessVersionsAsync(itemNode, version, folderNode, discoverWorker);
                            }
                            catch (Exception ex)
                            {
                                mLog.Error(LOGRESOURCE.StorageOptimization13_SOARScanProcessItemVersionsError + ex.ToString());
                            }
                        }
                    }

                    watch.Stop();
                    mLog.Info("ProcessVersionAndAttachments GetAttachments GetVersions costs: {0}.", watch.Elapsed);
                }
            }
        }

        private List<AveVersionObject> GetScanableVersionOfItem(AveDiscoverItem item, ArchiverNodeItem itemNode)
        {
            var versions = item.GetVersions()
                        .Where(v => v.Uiversion != item.Uiversion && v.Uiversion != 0)
                        .OrderByDescending(v => v.ID)
                        .ToList();

            #region filter versions by KeepLatestVersionAndArhiveOthers option
            if (itemNode.ItemType == ArchiverCommon.ItemType.DOCUMENT)
            {
                var fitRule = mConfiguration.RuleCollection.Values.FirstOrDefault(r => r.Id == itemNode.RuleId);


                if (mConfiguration.IsOneDriverSite)
                {
                    fitRule = fitRule?.OneDriveRule;
                }

                bool hasKeepVersionOption = false;
                int keepVersionNum = 0;
                bool processLastestVersion = false;

                if (fitRule == null)
                {
                    return versions;
                }
                else if ((fitRule.KeepDataOption & (int)KeepDataOption.KeepLatestVersionAndArhiveOthers) == (int)KeepDataOption.KeepLatestVersionAndArhiveOthers)
                {
                    hasKeepVersionOption = true;
                    keepVersionNum = fitRule.KeepLatestMajorAndMinorVersionAndArchiveOthers;
                }
                else if ((fitRule.KeepDataOption & (int)KeepDataOption.KeepLatestVersion) == (int)KeepDataOption.KeepLatestVersion)
                {
                    hasKeepVersionOption = true;
                    keepVersionNum = fitRule.KeepLatestMajorAndMinorVersion;
                }
                else if ((fitRule.KeepDataOption & (int)KeepDataOption.ArchiveOnlyLastestVersion) == (int)KeepDataOption.ArchiveOnlyLastestVersion)
                {
                    hasKeepVersionOption = true;
                    keepVersionNum = fitRule.ArchiverOnlyLastestVersion;
                    processLastestVersion = true;
                }

                if (hasKeepVersionOption)
                {
                    var currentVersionIsMajorVer = item.Uiversion % 512 == 0;
                    var lastMajorVerFound = currentVersionIsMajorVer;
                    var keepOtherVerCount = 0;
                    var verList = new List<AveVersionObject>();
                    foreach (var version in versions)
                    {
                        if (!lastMajorVerFound && version.Uiversion % 512 == 0)
                        {
                            lastMajorVerFound = true;
                        }
                        else if (keepOtherVerCount < keepVersionNum)
                        {
                            keepOtherVerCount++;
                            if (processLastestVersion)
                            {
                                verList.Add(version);
                            }
                        }
                        else if (!processLastestVersion)
                        {
                            verList.Add(version);
                        }
                    }
                    versions = verList;
                }
            }
            #endregion
            return versions;
        }

        internal async override System.Threading.Tasks.Task ProcessAttachmentsAsync(ArchiverNodeItem folderNode, ArchiverNodeItem item, AveItemObject attachment, IDiscoverNodeWorker discoverWorker)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessAttachments"))
            {
                ProcessResult result = ProcessResult.Default;
                try
                {
                    ArchiverNodeItem attachmentNode = null;
                    switch (item.ItemType)
                    {
                        case ArchiverCommon.ItemType.ITEM_TYPE:
                            attachmentNode = item.GenerateAttachmentNodeItem(attachment, (AveDiscoverFolder)folderNode.DiscoverSPObject);
                            await discoverWorker.ProcessItemAsync(attachmentNode, item);
                            break;
                        default:
                            attachmentNode = item.GenerateAttachmentNodeFolder(attachment, (AveDiscoverFolder)item.DiscoverSPObject);
                            await discoverWorker.ProcessItemAsync(attachmentNode, item);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    mLog.Error("An error occurred while processing attachments.Path:{0}.Message:{1}.", item.FullPath, ex.ToString());
                }
            }
        }

        internal async override System.Threading.Tasks.Task ProcessVersionsAsync(ArchiverNodeItem item, AveVersionObject version, ArchiverNodeItem folder, IDiscoverNodeWorker discoverWorker)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessVersions"))
            {
                ArchiverNodeItem versionNode = item.GenerateItemVersionNodeItem(version, item, mConfiguration);
                var result = await discoverWorker.ProcessItemAsync(versionNode, item);
            }
        }
    }
}
