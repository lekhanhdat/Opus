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
using AvePoint.RA.CommonUtil;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.Common.CAMLHelper.CAML;
using AvePoint.RA.SharePoint.Common.CAMLHelper.General;
using AvePoint.RA.SharePoint.Object;
using AvePoint.RA.SharePoint.RMSharePointTaxnomy;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.SharePoint.Client;
using System.Text.RegularExpressions;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Common.Utils;
using AvePoint.RA.Common.SystemSetting;
using System.Threading.Tasks;
using Aspose.Email.Storage.Pst;

namespace AvePoint.RA.SharePoint.RMLocationManagement
{
    public class RMLocationManagement : IDisposable
    {
        #region fields
        private RALogger logger = RALogger.GetInstance(typeof(RMLocationManagement));
        protected IRMReportManager mReportManager;
        protected IRMReportManager ReportManager
        {
            get
            {
                if (mReportManager == null)
                {
                    mReportManager = ReportMangerFactory.Instance.ReportManager;
                }
                return mReportManager;
            }
        }
        private IAveSite physicalSite { get; set; }
        private Guid mCurrentSiteId { get; set; }
        private IAveWeb physicalWeb { get; set; }
        private Guid mCurrentWebId { get; set; }
        private string mCurrentJobId { get; set; }

        private string mGlobalTimeZoneId { get; set; }
        private IAveList physicalList { get; set; }
        private List<IAveList> physicalLists { get; set; }
        private List<RMTermSet> AllTermSet { get; set; }
        private List<RMTermSet> PhysicalTermSet { get; set; }
        private List<RMTermSet> BusinessTermSet { get; set; }
        private string LocationColumn { get; set; }
        private string BoxColumnName { get; set; }
        private string BCColumnName { get; set; }
        private Dictionary<Guid, int> mTermWssidMappingsOfSite = new Dictionary<Guid, int>();
        private IAveTaxonomyField currentLocationColumn { get; set; }
        private List<RMSharePointSetting> PhysicalSettings { get; set; }
        private BaseJobDto baseJobDto { get; set; }
        private bool HasSuccessNode = false;
        private bool HasErrorNode = false;
        public List<Guid> NodeleteTerms = new List<Guid>();
        public List<JMPhysicalSyncJobDetails> syncJobDetails = new List<JMPhysicalSyncJobDetails>();
        private string commomErrorMessage = string.Empty;

        private List<Guid> MovedItemIds = null;
        private List<string> MovedItemUrl = null;
        private IAveTermStore termStore = null;
        private IAveTermSet termSet = null;
        private JobResult Result = null;
        private ISharePointSettingDao mSharePointSettingsDao { get; set; }
        private IRMLocationAssociationDao mLocationDao { get; set; }
        private ISPSettingTreeService mSPTreeService { get; set; }
        private IJobMonitorService mJobService;
        private JobType jobType = JobType.None;
        private ITermSetDao termSetDao { get; set; }
        private IRMReportService mRMReportService { get; set; }
        private int TotalSettingsCount
        {
            get
            {
                if (PhysicalSettings == null)
                {
                    return 0;
                }
                else
                {
                    return PhysicalSettings.Count;
                }
            }
        }
        private readonly static RASimpleLocker _simpleLocker = new RASimpleLocker();
        private IJobDetailService mJobDetailService { get; set; }
        public IJobDetailService JobDetailService
        {
            get
            {
                if (mJobDetailService == null)
                {
                    mJobDetailService = (IJobDetailService)PlatformWindsorManager.GetService(typeof(IJobDetailService));
                }
                return mJobDetailService;
            }
        }

        private IContainerDao mIContainerDao { get; set; }
        private IContainerDao IContainerDao
        {
            get
            {
                if (mIContainerDao == null)
                {
                    mIContainerDao = (IContainerDao)PlatformWindsorManager.GetService(typeof(IContainerDao));
                }
                return mIContainerDao;
            }
        }
        private ITermDao mITermDao { get; set; }
        private ITermDao termDao
        {
            get
            {
                if (mITermDao == null)
                {
                    mITermDao = (ITermDao)PlatformWindsorManager.GetService(typeof(ITermDao));
                }
                return mITermDao;
            }
        }
        protected IJobMonitorService JobMonitorService
        {
            get
            {
                if (mJobService == null)
                {
                    mJobService = (IJobMonitorService)PlatformWindsorManager.GetService(typeof(IJobMonitorService));
                }
                return mJobService;
            }
        }
        protected ISPSettingTreeService SPTreeService
        {
            get
            {
                if (mSPTreeService == null)
                {
                    mSPTreeService = (ISPSettingTreeService)PlatformWindsorManager.GetService(typeof(ISPSettingTreeService));
                }
                return mSPTreeService;
            }
        }
        /*protected IRMReportService RMReportService
        {
            get
            {
                if (mRMReportService == null)
                {
                    mRMReportService = (IRMReportService)PlatformWindsorManager.GetService(typeof(IRMReportService));
                }
                return mRMReportService;
            }
        }
        private async Task<List<RMSPTreeNode>> GetRegisteredSPSitesAsync()
        {
            List<RMSPTreeNode> returnList = new List<RMSPTreeNode>();
            List<RMSPTreeNode> registeredSites = SPTreeService.LoadFarm();
            var defaultSites = await SPTreeService.BrowseAsync(registeredSites[0]);
            foreach (var defaultSite in defaultSites)
            {
                returnList.AddRange(await SPTreeService.BrowseAsync(defaultSite));
            }
            return returnList;
        }*/
        public ISharePointSettingDao SharePointSettingsDao
        {
            get
            {
                if (mSharePointSettingsDao == null)
                {
                    mSharePointSettingsDao = (ISharePointSettingDao)PlatformWindsorManager.GetService(typeof(ISharePointSettingDao));
                }
                return mSharePointSettingsDao;
            }
        }
        public IRMLocationAssociationDao LocationDAO
        {
            get
            {
                if (mLocationDao == null)
                {
                    mLocationDao = (IRMLocationAssociationDao)PlatformWindsorManager.GetService(typeof(IRMLocationAssociationDao));
                }
                return mLocationDao;
            }
        }
         private IRMLocationDao RMLocationDAO => PlatformWindsorManager.GetService<IRMLocationDao>();

        private string physicalRecordsCSVPath = string.Empty;
        private RMSharePointSetting physicalSetting = null;
        private List<IAveFolder> folders = null;
        private IAveContentType physicalBoxContentType = null;
        private IAveContentType physicalFileContentType = null;
        private IAveContentType physicalRecordContentType = null;
        ClientContext context = null;
        public Dictionary<Guid, IAveList> phyListsDict = new Dictionary<Guid, IAveList>();
        public Dictionary<Guid, string> phyListIdBCSColNameMap = new Dictionary<Guid, string>();
        public Dictionary<Guid, RMSPTreeNode> phyListIdSiteNodeMap = new Dictionary<Guid, RMSPTreeNode>();
        #endregion

        #region constructor
        // private List<RMTermGroup> RMTermGroups { get; set; }
        // to do job later
        public RMLocationManagement(List<RMTermSet> allTermSet, List<RMSPTreeNode> allSiteCollections)
        {
            this.AllTermSet = allTermSet;
            PhysicalSettings = SharePointSettingsDao.GetAllPhysicalSiteSettings();
            //AllSiteCollectionNodes = allSiteCollections;
        }

        public RMLocationManagement(string jobId)
        {
            baseJobDto = new BaseJobDto() { Id = jobId, JobType = (int)JobType.PhysicalFolderSynchronization };
            JobMonitorService.UpdateJobProgress(jobId, 1);
            using (var processor = new RMSyncTermProcessor())
            {
                AllTermSet = processor.BuildRMTermSetTreeAsync(TermSetType.Physical, Guid.Empty).Result;
            }
            JobMonitorService.UpdateJobProgress(jobId, 5);
            PhysicalSettings = SharePointSettingsDao.GetAllPhysicalSiteSettings();
            //TotalSettingsCount = PhysicalSettings.Count;
            JobMonitorService.UpdateJobProgress(jobId, 10);
            //AllSiteCollectionNodes = this.GetRegisteredSPSites();
            JobMonitorService.UpdateJobProgress(jobId, 15);
            mCurrentJobId = jobId;
        }

        public RMLocationManagement(RMImportJobMessage msg)
        {
            this.jobType = msg.JobType;
            mCurrentJobId = msg.JobID;
            mGlobalTimeZoneId = msg.GlobalTimeZoneId;
            //ReportMangerFactory.Instance.Init(mCurrentJobId, this.jobType);

            //reportManager.BaseJobDto = new BaseJobDto() { Id = mCurrentJobId, JobType = (int)jobType };
            Result = new JobResult();

            switch (jobType)
            {
                case JobType.UpdateLocation:
                    #region UpdateLocation
                    MovedItemIds = new List<Guid>();
                    MovedItemUrl = new List<string>();
                    Result = new JobResult();
                    PhysicalSettings = SharePointSettingsDao.GetAllPhysicalSiteSettings();
                    //AllSiteCollectionNodes = this.GetRegisteredSPSites();
                    #endregion
                    break;
                case JobType.ImportPhysicalRecords:
                    #region ImportPhysicalRecords
                    //physicalRecordsCSVPath = msg.PhysicalRecordsCSVPath;

                    try
                    {
                        physicalRecordsCSVPath = JobReportUtility.GetImportJobCSVFile(msg.PhysicalRecordsCSVPath);
                    }
                    catch (Exception e)
                    {
                        logger.Error("can not download file:{0},error:{1}", msg.PhysicalRecordsCSVPath, e.ToString());
                        throw;
                    }


                    foreach (RMSharePointSetting setting in SharePointSettingsDao.GetAllPhysicalSiteSettings())
                    {
                        if (setting.Id == msg.SharePointSettingID)
                        {
                            physicalSetting = setting;
                            break;
                        }
                    }
                    if (physicalSetting == null)
                    {
                        throw new Exception("Get physical setting failed.");
                    }
                    //AllSiteCollectionNodes = this.GetRegisteredSPSites();
                    #endregion
                    break;
                default:
                    break;
            }

            //默认初始化 进度为2
            ReportManager.Increase(2);
            ReportManager.StartUpdateJobProgress();
        }

        /// <summary>
        /// for check term canbe delete or not 
        /// </summary>
        public RMLocationManagement()
        {
            PhysicalSettings = SharePointSettingsDao.GetAllPhysicalSiteSettings();
            //AllSiteCollectionNodes = this.GetRegisteredSPSites();
        }
        #endregion

        #region public method
        public bool UpdateLocationPath(IAveFolder folder, RMTerm term)
        {
            if (!folder.Name.Equals(term.Name, StringComparison.OrdinalIgnoreCase))
            {
                var keep1 = folder.Item["Modified"];
                var keep2 = folder.Item["Editor"];
                folder.Item["Title"] = term.Name;
                folder.Item["FileLeafRef"] = term.Name;
                folder.Item.Update();
                folder.Update();

                folder.Reload();
                folder.Item["Modified"] = keep1;
                folder.Item["Editor"] = keep2;
                folder.Item.SystemUpdate();
                folder.Update();

                logger.Info("Rename folder name,{0}:{1}", folder.Url, term.Name);
                SendJobDetail(new JMPhysicalSyncJobDetails()
                {
                    Action = "Update",
                    Comment = "Update location",
                    LocationPath = folder.Url,
                    SiteCollectionURL = this.physicalSite.Url,
                    Status = JobDetailsStatus.Successful,
                    TermName = term.Name
                });
                return true;
            }
            else
            {
                SendJobDetail(new JMPhysicalSyncJobDetails()
                {
                    Action = "Update",
                    Comment = "Update location",
                    LocationPath = folder.Url,
                    SiteCollectionURL = this.physicalSite.Url,
                    Status = JobDetailsStatus.Skipped,
                    TermName = term.Name
                });
                return false;
                //skip for log?
            }
        }

        public async System.Threading.Tasks.Task UpdateLocationAsync()
        {
            JobStatus status = JobStatus.None;
            try
            {
                foreach (var physicalSetting in PhysicalSettings)
                {

                    try
                    {
                        using (CheckJobStopScope jScope = new CheckJobStopScope())
                        {
                            ReportManager.Increase(0);
                            await InitAsync(physicalSetting);

                            CompareColumn(physicalList.RootFolder);


                        }
                    }
                    catch (JobStopException ex)
                    {
                        throw new JobStopException("This Job is stopped.");
                    }
                    catch (GetSiteFromDAException e)
                    {
                        logger.Warn("Get Site Collection from DocAve failed.{0}", e.ToString());
                    }
                    catch (Exception e)
                    {
                        Result.HasFailed = true;
                        logger.Error("UpdateLocation physical setting error:{0}", e);
                        JMUpdateLocationJobDetail detail = new JMUpdateLocationJobDetail();
                        detail.SiteCollectionURL = physicalSite == null ? string.Empty : physicalSite.Url;
                        detail.Status = JobDetailsStatus.Failed;
                        detail.Comment = string.Format("RM_CommonErrorMessage", e.Message); ;
                        ReportManager.SendJobDetail(detail);
                    }
                }

                status = Result.HasFailed
                    ? Result.HasSuccessful
                        ? JobStatus.FinishWithException
                        : JobStatus.Failed
                    : JobStatus.Finished;
            }
            catch (JobStopException ex)
            {
                status = JobStatus.Stopped;
                logger.Info(string.Format("This Job is stopped."));
            }
            catch (Exception e)
            {
                status = JobStatus.Failed;
                logger.Error(string.Format("UpdateLocation job Error :{0}", e.ToString()));
            }
            finally
            {
                ReportManager.SetJobFinished(status);
            }
        }

        public async System.Threading.Tasks.Task SyncTermFolderActionAsync(RMTerm term, IAveFolder parent)
        {
            IAveFolder currentFolder = null;
            var location = LocationDAO.GetLocationByTermId(physicalSite.ID, physicalWeb.ID, physicalList.ID, term.UniqueId);
            try
            {
                if (location != null)
                {
                    currentFolder = parent.SubFolders.AsQueryable().Where(f => f.UniqueId.Equals(location.FolderId)).FirstOrDefault();
                    if ((currentFolder == null || !currentFolder.Exists) && !term.IsRemoved)
                    {
                        currentFolder = parent.SubFolders.Add(term.Name);
                        currentFolder.Update();
                        location.FolderId = currentFolder.UniqueId;
                        await LocationDAO.UpdateRMLocationAssociationAsync(location);
                        SendJobDetail(new JMPhysicalSyncJobDetails()
                        {
                            Action = "New",
                            Comment = "New location",
                            LocationPath = currentFolder.Url,
                            SiteCollectionURL = this.physicalSite.Url,
                            Status = JobDetailsStatus.Successful,
                            TermName = term.Name
                        });
                    }
                    else if (!term.IsRemoved)
                    {
                        if (UpdateLocationPath(currentFolder, term))
                        {
                            var discoverFolders = physicalList.Folders.Select(i => i.Folder).ToList();
                            //reload list after modified folder name
                            physicalList = physicalWeb.GetList(physicalList.ID);
                            discoverFolders.Insert(0, physicalList.RootFolder);
                            currentFolder = discoverFolders.AsQueryable().Where(f => f.UniqueId.Equals(location.FolderId)).FirstOrDefault();
                        }

                    }
                    else if (term.IsRemoved)
                    {
                        if (currentFolder != null && currentFolder.Exists)
                        {
                            DeleteLocation(currentFolder, term, location);
                        }
                        else if (currentFolder == null)
                        {
                            logger.Info("Folder have been deleted before {0}", term.Name);
                        }
                    }
                }
                else
                {
                    if ((currentFolder == null || !currentFolder.Exists) && !term.IsRemoved)
                    {
                        currentFolder = parent.SubFolders.Add(term.Name);
                        currentFolder.Update();
                        location = new RMLocationAssociation()
                        {
                            SiteId = physicalSite.ID,
                            WebId = physicalWeb.ID,
                            ListId = physicalList.ID,
                            FolderId = currentFolder.UniqueId,
                            TermUniqueId = term.UniqueId
                        };
                        LocationDAO.AddRMLocationAssociation(location);
                        SendJobDetail(new JMPhysicalSyncJobDetails()
                        {
                            Action = "New",
                            Comment = "New location",
                            LocationPath = currentFolder.Url,
                            SiteCollectionURL = this.physicalSite.Url,
                            Status = JobDetailsStatus.Successful,
                            TermName = term.Name
                        });
                    }
                }
                HasSuccessNode = true;
            }
            catch (Exception e)
            {
                HasErrorNode = true;
                logger.Warn("sync folder error {0}", e.ToString());
                ArgumentCheck.CheckNotNull(currentFolder);
                SendJobDetail(new JMPhysicalSyncJobDetails()
                {
                    Action = "None",
                    Comment = "None",
                    LocationPath = currentFolder?.Url,
                    SiteCollectionURL = this.physicalSite.Url,
                    Status = JobDetailsStatus.Failed,
                    TermName = term.Name
                });
            }
            if (term.subTerms != null)
            {
                foreach (var subTerm in term.subTerms)
                {
                    try
                    {
                        await SyncTermFolderActionAsync(subTerm, currentFolder);
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Sync subfolder error {0}:{1}", subTerm.Name, ex.ToString());
                        SendJobDetail(new JMPhysicalSyncJobDetails()
                        {
                            Action = "None",
                            Comment = "None",
                            SiteCollectionURL = this.physicalSite.Url,
                            Status = JobDetailsStatus.Failed,
                            TermName = term.Name
                        });
                        HasErrorNode = true;
                    }
                }
            }

        }

        public async Task<bool> CheckLocationCanBeDeleteAsync(Guid termId)
        {
            foreach (var physicalSetting in PhysicalSettings)
            {
                var node = await InitObjectAsync(physicalSetting);
                if (null == node || this.physicalList == null)
                {
                    continue;
                }
                currentLocationColumn = GetTaxonomyField(this.physicalList.Fields, LocationColumn);
                var location = LocationDAO.GetLocationByTermId(physicalSite.ID, physicalWeb.ID, physicalList.ID, termId);
                if (location != null)
                {
                    var discoverFolders = physicalList.Folders.Select(i => i.Folder).ToList();
                    discoverFolders.Insert(0, physicalList.RootFolder);
                    IAveFolder currentFolder = discoverFolders.AsQueryable().Where(f => f.UniqueId.Equals(location.FolderId)).FirstOrDefault();
                    if (currentFolder != null && currentFolder.Exists)
                    {
                        if (!CheckLocationCanBeDelete(currentFolder, termId))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public async System.Threading.Tasks.Task SyncToPhysicalLibAsync()
        {
            //sync all set....foreach(site,web,list) how to calculate the progress.....
            List<string> deletedSettingIds = new List<string>();
            foreach (var physicalSetting in PhysicalSettings)
            {
                int processSettingCount = 0;
                try
                {
                    var node = await InitObjectAsync(physicalSetting);
                    if (node == null || physicalSite == null || physicalList == null)
                    {
                        if (node == null)
                        {
                            deletedSettingIds.Add(physicalSetting.SiteGroupId.ToString() + physicalSetting.SiteId.ToString() + physicalSetting.ScopeId.ToString());
                            SendJobDetail(new JMPhysicalSyncJobDetails()
                            {
                                Action = "RM_JS_Common_Pending",
                                Comment = "RM_SS_SiteRemovedFromDAO",
                                LocationPath = I18NEntity.GetString("RM_JS_Common_Pending"),
                                SiteCollectionURL = physicalSetting.FullPath,
                                Status = JobDetailsStatus.Skipped,
                                TermName = I18NEntity.GetString("RM_JS_Common_Pending")
                            });
                        }
                        logger.Warn("An error occurred when init physical site or list failed.  physical setting path:{0}, SiteId:{1}", physicalSetting.FullPath, physicalSetting.SiteId.ToString());
                        continue;
                    }
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("AvePoint.Common.Perm.PermissionControl.Util.PermissionDenyException"))
                    {
                        throw new Exception(e.ToString());
                    }
                    logger.Warn("{0}, physical setting path:{1},SiteId:{2}", e.ToString(), physicalSetting.FullPath, physicalSetting.SiteId.ToString());
                    continue;
                }

                foreach (RMTermSet rmTermSet in AllTermSet)
                {
                    UpdateJobProgress(CalculateProgress(processSettingCount, TotalSettingsCount));
                    //this.ProcessTermSet(rmTermSet, termGroup, termSetCollection);
                    foreach (RMTerm term in rmTermSet.RMTerms)
                    {
                        currentLocationColumn = GetTaxonomyField(this.physicalList.Fields, LocationColumn);
                        try
                        {
                            await SyncTermFolderActionAsync(term, physicalList.RootFolder);
                        }
                        catch (Exception ex)
                        {
                            logger.Error("Sync folder failed {0}:{1}", term.Name, ex.ToString());
                            HasErrorNode = true;
                            SendJobDetail(new JMPhysicalSyncJobDetails()
                            {
                                Action = "None",
                                Comment = "None",
                                SiteCollectionURL = this.physicalSite.Url,
                                Status = JobDetailsStatus.Failed,
                                TermName = term.Name
                            });
                        }
                        if (CheckJobStatusUtility.isStopping)
                        {
                            SendJobDetail();
                            JobDetailService.UploadJobDetailsAndReport(baseJobDto);
                            throw new JobStopException("This Job is stopped.");
                        }
                    }
                    processSettingCount++;
                }
            }
            SendJobDetail();
            JobDetailService.UploadJobDetailsAndReport(baseJobDto);
            if (deletedSettingIds.Count > 0)
            {
                PhysicalSettings = PhysicalSettings.Where(p => !deletedSettingIds.Contains(p.SiteGroupId.ToString() + p.SiteId.ToString() + p.ScopeId.ToString())).ToList();
            }
            if (null == PhysicalSettings || PhysicalSettings.Count == 0)
            {
                JobMonitorService.UpdateJobStatus(mCurrentJobId, JobStatus.Failed, "RM_PS_NoMarkPhysicalLibrary");
            }
            else
            {
                //Job结束，更新Job状态
                if (HasSuccessNode && HasErrorNode)
                {
                    JobMonitorService.UpdateJobStatus(mCurrentJobId, JobStatus.FinishWithException, "RM_TS_SS_Summary");
                }
                else if (!HasSuccessNode)
                {
                    JobMonitorService.UpdateJobStatus(mCurrentJobId, JobStatus.Failed, "RM_TS_SS_Summary");
                }
                else if (!HasErrorNode)
                {
                    JobMonitorService.UpdateJobStatus(mCurrentJobId, JobStatus.Finished, "");
                }
                else if (HasErrorNode)
                {
                    JobMonitorService.UpdateJobStatus(mCurrentJobId, JobStatus.FinishWithException, "RM_TS_SS_Summary");
                }
                else
                {
                    JobMonitorService.UpdateJobStatus(mCurrentJobId, JobStatus.Skipped, "RM_SS_JobSkip");
                }
            }
            //JobMonitorService.UpdateJobStatus(mCurrentJobId, JobStatus.Finished);
        }

        public bool IsLocationChanged(RMTerm term, IAveFolder folder)
        {
            if (term.Name.Equals(folder.Name))
            {
                return false;
            }
            return true;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public async System.Threading.Tasks.Task InitPhysicalListsReportInfoAsync()
        {
            Dictionary<string, int> infos = new Dictionary<string, int>();
            foreach (var setting in PhysicalSettings)
            {
                //var siteNode = AllSiteCollectionNodes.AsQueryable().Where(t => t.SPObjectId.Equals(setting.SiteId.ToString(), StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                //if (siteNode != null)
                //{
                try
                {
                    var node = await InitObjectAsync(setting);
                    if (node == null || physicalList == null || physicalSite == null)
                    {
                        logger.Warn("An error occurred when init physical site or list failed.  physical setting path:{0}, SiteId:{1}", setting.FullPath, setting.SiteId.ToString());
                        continue;
                    }
                    var phyListId = physicalList.ID;
                    this.phyListsDict.Add(phyListId, physicalList);
                    //this.phyListIdSiteNodeMap.Add(phyListId,siteNode);
                    this.phyListIdBCSColNameMap.Add(phyListId, setting.IsUsingExistColumnName ? setting.ExistColumnName : setting.ColumnName);
                }
                catch (Exception)
                {
                    logger.Info("physical web or physical list may have been deleted,listid:{0},webid:{1},siteid:{2}", setting.ListId, setting.WebId, setting.SiteId);
                    continue;
                }
                //}
            }
        }

        #endregion

        #region private method
        #region UpdateLocation
        private void CompareColumn(IAveFolder folder)
        {
            foreach (IAveFile file in folder.Files)
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    if (MovedItemIds.Contains(file.UniqueId))
                    {
                        continue;
                    }
                    try
                    {
                        if (file.Item == null)
                        {
                            continue;
                        }

                        CompareItemColumn(file.Item);
                    }
                    catch (Exception e)
                    {
                        Result.HasFailed = true;
                        logger.Warn(string.Format("CompareItemColumn error,Url:[{0}] Error:{1}", file.Item.Url, e.ToString()));
                    }
                }
            }

            foreach (IAveFolder subFolder in folder.SubFolders)
            {
                if (MovedItemIds.Contains(subFolder.UniqueId)
                    || (string.Equals(subFolder.Name, "Forms", StringComparison.OrdinalIgnoreCase) && string.Equals(physicalList.RootFolder.Url, folder.Url)))
                {
                    continue;
                }
                try
                {
                    CompareColumn(subFolder);

                    if (subFolder.Item == null)
                    {
                        continue;
                    }
                    CompareItemColumn(subFolder.Item);
                }
                catch (JobStopException ex)
                {
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception e)
                {
                    Result.HasFailed = true;
                    logger.Warn(string.Format("CompareItemColumn error,Url:[{0}] Error:{1}", subFolder.Item.Url, e.ToString()));
                }
                finally
                {
                    ReportManager.Increase();
                }
            }
        }

        private void CompareItemColumn(IAveListItem item)
        {
            using (CheckJobStopScope jScope = new CheckJobStopScope())
            {
                if (!(item.Fields.ContainsField(BoxColumnName) && item.Fields.ContainsField(LocationColumn)))
                {
                    return;
                }
                object boxColumnValue = item[BoxColumnName];
                object locationColumnValue = item[LocationColumn];
                if (boxColumnValue == null || locationColumnValue == null)
                {
                    return;
                }

                IAveTerm endTerm = null;
                string desFolderUrl = string.Empty;
                string boxColumnValueStr = string.Empty;

                try
                {
                    endTerm = termStore.GetTerm(termSet.ID, new Guid(locationColumnValue.ToString().Split('|')[1]));
                    desFolderUrl = endTerm.PathOfTerm.Replace(';', '/');
                    boxColumnValueStr = boxColumnValue.ToString();
                }
                catch (Exception e)
                {
                    JMUpdateLocationJobDetail detail = new JMUpdateLocationJobDetail();
                    detail.SiteCollectionURL = physicalSite == null ? string.Empty : physicalSite.Url;
                    detail.Status = JobDetailsStatus.Failed;
                    detail.SourceUrl = physicalWeb.Url + "/" + item.Url;
                    if (item.Folder != null)
                    {
                        detail.ItemType = UpdateLocationJobItemType.Folder.ToString();
                    }
                    if (item.File != null)
                    {
                        detail.ItemType = UpdateLocationJobItemType.Document.ToString();
                    }
                    detail.Comment = "RM_DesFoldIsExist";
                    ReportManager.SendJobDetail(detail);
                    throw;
                }


                string parentFolderUrl =
                    item.File == null
                        ? item.Folder == null
                            ? string.Empty
                            : item.Folder.ParentFolder.Url
                        : item.File.ParentFolder.Url;

                if (!string.Equals(string.Format("{0}/{1}/{2}", physicalList.RootFolder.Name, desFolderUrl, boxColumnValueStr), parentFolderUrl, StringComparison.OrdinalIgnoreCase))
                {
                    var discoverFolders = SPCommonUtility.GetAllFolders(physicalList);
                    //discoverFolders.Insert(0, physicalList.RootFolder);
                    IAveFolder desFolder = discoverFolders.AsQueryable().Where(f => f.Url.Equals(string.Format("{0}/{1}", physicalList.RootFolder.Name, desFolderUrl), StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    MoveFoldersAndDocuments(desFolder, item, boxColumnValueStr);
                }
            }
        }

        private void MoveFoldersAndDocuments(IAveFolder desFolder, IAveListItem souItem, string boxName)
        {
            if (souItem.Folder != null)
            {
                MoveFolder(desFolder, souItem.Folder, boxName);
            }
            else if (souItem.File != null)
            {

                MoveDocument(desFolder, souItem.File, boxName);
            }
            else
            {
                throw new Exception("Wrong object type.");
            }
        }

        private void MoveFolder(IAveFolder desFolder, IAveFolder souFolder, string boxName)
        {
            JMUpdateLocationJobDetail detail = new JMUpdateLocationJobDetail();
            try
            {
                detail.ItemType = UpdateLocationJobItemType.Folder.ToString();
                detail.SourceUrl = physicalWeb.Url + "/" + souFolder.Url;
                detail.DestinationUrl = physicalWeb.Url + "/" + desFolder.Url + "/" + boxName;
                detail.SiteCollectionURL = physicalSite.Url;

                Result.FolderCount++;

                IAveFolder boxFolder = CreateBoxFolder(desFolder, boxName, souFolder.Item[LocationColumn]);

                //string newFolderName = string.Empty;
                try
                {
                    try
                    {
                        if (MovedItemUrl.Contains(string.Format("{0}/{1}/{2}", detail.DestinationUrl, boxName, souFolder.Name)) || boxFolder.SubFolders[souFolder.Name].Exists)
                        {
                            throw new SkipItemException(I18NEntity.GetString("RM_PU_SkipItemMessage"));
                            //newFolderName = string.Format("{0}_{1}", souFolder.Name, DateTime.Now.ToString("yyyyMMddHHmmss"));
                        }
                    }
                    catch (Exception)
                    {
                        logger.Info("dest not exist need move");
                    }
                }
                catch (Exception e)
                {
                    if (!e.Message.Contains("Cannot find the folder"))
                    {
                        throw;
                    }
                }

                //souFolder.MoveTo(string.Format("{0}/{1}", boxFolder.ServerRelativeUrl, string.IsNullOrEmpty(newFolderName) ? souFolder.Name : newFolderName));
                souFolder.MoveTo(string.Format("{0}/{1}", boxFolder.ServerRelativeUrl, souFolder.Name));
                souFolder.Item["Parent ID"] = boxFolder.Item.ID;
                souFolder.Item.SystemUpdate();

                MovedItemIds.Add(souFolder.UniqueId);
                MovedItemUrl.Add(string.Format("{0}/{1}/{2}", detail.DestinationUrl, boxName, souFolder.Name));
                detail.Status = JobDetailsStatus.Successful;
                Result.HasSuccessful = true;
            }
            catch (SkipItemException ex)
            {
                detail.Status = JobDetailsStatus.Skipped;
                detail.Comment = ex.Message;
                logger.Warn("This folder already exists at the destination.Destination:[{0}],Source:[{1}]", desFolder.Url, souFolder.Url);
            }
            catch (Exception e)
            {
                detail.Status = JobDetailsStatus.Failed;
                detail.Comment = string.Format(I18NEntity.GetString("RM_CommonErrorMessage"), e.Message); ;
                Result.HasFailed = true;
                logger.Error("Move folder failed.Destination:[{0}],Source:[{1}],Error:[{2}]", desFolder.Url, souFolder.Url, e.ToString());
            }
            finally
            {
                ReportManager.SendJobDetail(detail);
            }
        }

        private void MoveDocument(IAveFolder desFolder, IAveFile souFile, string boxName)
        {
            JMUpdateLocationJobDetail detail = new JMUpdateLocationJobDetail();
            try
            {
                detail.ItemType = UpdateLocationJobItemType.Document.ToString();
                detail.SourceUrl = physicalWeb.Url + "/" + souFile.Url;
                detail.DestinationUrl = physicalWeb.Url + "/" + desFolder.Url;
                detail.SiteCollectionURL = physicalSite.Url;

                IAveFolder boxFolder = CreateBoxFolder(desFolder, boxName, souFile.Item[LocationColumn]);
                //string newFileName = string.Empty;
                try
                {
                    if (MovedItemUrl.Contains(string.Format("{0}/{1}/{2}", detail.DestinationUrl, boxName, souFile.Name)) || boxFolder.Files[souFile.Name].Exists)
                    {
                        //newFileName = string.Format("{0}_{1}{2}", Path.GetFileNameWithoutExtension(souFile.Name), DateTime.Now.ToString("yyyyMMddHHmmss"), Path.GetExtension(souFile.Name));
                        throw new SkipItemException(I18NEntity.GetString("RM_PU_SkipItemMessage"));
                    }
                }
                catch (Exception e)
                {
                    if (!e.Message.Contains("File not find"))
                    {
                        throw;
                    }
                }


                Result.FileCount++;
                souFile.MoveTo(string.Format("{0}/{1}/{2}", boxFolder.ParentWeb.Site.Url.Trim('/'), boxFolder.Url.Trim('/'), souFile.Name), AveMoveOperations.None);
                //souFile.MoveTo(string.Format("{0}/{1}/{2}", boxFolder.ParentWeb.Site.Url.Trim('/'), boxFolder.Url.Trim('/'), string.IsNullOrEmpty(newFileName) ? souFile.Name : newFileName), AveMoveOperations.None);
                logger.Info("Move file successful.Destination:[{0}],Source:[{1}]", boxFolder.Url, souFile.Url);

                MovedItemIds.Add(souFile.UniqueId);
                MovedItemUrl.Add(string.Format("{0}/{1}/{2}", detail.DestinationUrl, boxName, souFile.Name));
                detail.Status = JobDetailsStatus.Successful;
                Result.HasSuccessful = true;
            }
            catch (SkipItemException ex)
            {
                detail.Status = JobDetailsStatus.Skipped;
                detail.Comment = ex.Message;
                logger.Warn("This file already exists at the destination.Destination:[{0}],Source:[{1}]", desFolder.Url, souFile.Url);
            }
            catch (Exception e)
            {
                detail.Status = JobDetailsStatus.Failed;
                detail.Comment = string.Format(I18NEntity.GetString("RM_CommonErrorMessage"), e.Message); ;
                Result.HasFailed = true;
                logger.Error("Move file failed.Destination:[{0}],Source:[{1}],Error:[{2}]", desFolder.Url, souFile.Url, e.ToString());
            }
            finally
            {
                ReportManager.SendJobDetail(detail);
            }
        }

        private IAveFolder CreateBoxFolder(IAveFolder desFolder, string boxName, object homeLocation)
        {
            IAveFolder boxFolder = null;
            try
            {
                try
                {
                    if (desFolder.SubFolders[boxName].Exists)
                    {
                        boxFolder = desFolder.SubFolders[boxName];
                    }
                }
                catch (Exception e)
                {
                    logger.Info(e.Message);
                }

                if (boxFolder == null)
                {
                    boxFolder = desFolder.SubFolders.Add(boxName);
                    boxFolder.Update();//否则boxFolder.Item == null
                    if (boxFolder.Item[LocationColumn] == null)
                    {
                        boxFolder.Item[LocationColumn] = homeLocation;
                    }

                    if (boxFolder.Item["Availability"] == null)
                    {
                        boxFolder.Item["Availability"] = "Available";
                    }
                    if (boxFolder.Item["Box Type"] == null)
                    {
                        List<RMContainer> containerList = IContainerDao.GetDefaultContainers();
                        RMContainer defaultValue = null;
                        if (containerList != null && containerList.Count != 0)
                        {
                            defaultValue = IContainerDao.GetDefaultContainers().First();
                            boxFolder.Item["Box Type"] = defaultValue.TypeName;
                        }
                    }

                    if (physicalBoxContentType == null)
                    {
                        throw new Exception("Can not find physical box content type.");
                    }

                    boxFolder.Item["ContentTypeId"] = physicalBoxContentType.ID;
                    boxFolder.Item.SystemUpdate();
                }
            }
            catch (Exception e)
            {
                string msg = string.Format("Create box folder failed.Box name:[{0}],Error:{1}", boxName, e.ToString());
                logger.Error(msg);
                throw new Exception("Create box folder failed.");
            }
            return boxFolder;
        }

        private async System.Threading.Tasks.Task InitAsync(RMSharePointSetting setting)
        {
            SharePointSettingUtility spUtility = new SharePointSettingUtility();
            RemoteSiteCollection node = null;
            try
            {
                node = spUtility.GetRemoteSiteCollection(setting.SiteId.ToString());
            }
            catch (Exception e)
            {
                if (e.Message.Contains("GetRemoteSiteCollection Failed"))
                {
                    logger.Warn("An error occurred when get sitecollection by id from docave failed,siteId:{0}, path:{1} ,error:{2}", setting.FullPath, setting.SiteId.ToString(), e.ToString());
                    throw new GetSiteFromDAException(string.Format("Get tree node failed.Url:[{0}] SiteId:[{1}]", setting.FullPath, setting.SiteId.ToString()));
                }
            }
            if (node == null || string.IsNullOrEmpty(node.url))
            {
                logger.Warn("An error occurred when get sitecollection by id from docave failed,siteId:{0}, path:{1}", setting.FullPath, setting.SiteId.ToString());
                throw new GetSiteFromDAException(string.Format("Get tree node failed.Url:[{0}] SiteId:[{1}]", setting.FullPath, setting.SiteId.ToString()));
            }
            try
            {
                try
                {
                    CommonClientContext commonContext = new CommonClientContext();
                    context = commonContext.InitClientContext(node);
                }
                catch (Exception e)
                {
                    logger.Warn("init client context error:{0}", e.ToString());
                }
                if (physicalSite == null || !physicalSite.ID.Equals(mCurrentSiteId))
                {
                    var mfactory = MultiAppUtil.CreateAveObjectModelFactory(node.url, await PoolUserUtil.GetBPOSInfoAsync(node), AveContextKind.ClientObjectModel);
                    physicalSite = mfactory.CreateSite(node.url);
                }
                if (physicalWeb == null || !physicalWeb.ID.Equals(mCurrentWebId))
                {
                    if (setting.WebId == null || setting.WebId.Equals(Guid.Empty))
                    {
                        physicalWeb = physicalSite.RootWeb;
                    }
                    else
                    {
                        physicalWeb = physicalSite.OpenWeb(setting.WebId);
                    }
                }
                if (setting.ListId == null || setting.ListId.Equals(Guid.Empty))
                {
                    physicalList = physicalWeb.GetListByName(Common.Util.GetAppSettingValue("RevIMHoldPhysicalLibraryName"), false);
                }
                else
                {
                    physicalList = physicalWeb.GetList(setting.ListId);
                }

                foreach (IAveContentType contentType in physicalList.ContentTypes)
                {

                    if (string.Equals(contentType.Name, "Physical Box", StringComparison.OrdinalIgnoreCase))
                    {
                        physicalBoxContentType = contentType;
                    }
                    else if (string.Equals(contentType.Name, "Physical File", StringComparison.OrdinalIgnoreCase))
                    {
                        physicalFileContentType = contentType;
                    }
                    else if (string.Equals(contentType.Name, "Physical Record", StringComparison.OrdinalIgnoreCase))
                    {
                        physicalRecordContentType = contentType;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Init physical setting failed,message:{0}", ex.ToString()));
            }

            LocationColumn = Common.Util.GetAppSettingValue("RevIMHomeLocationName");
            BoxColumnName = Common.Util.GetAppSettingValue("RevIMBoxName");

            IAveTaxonomyField taxonomyField = physicalList.Fields[LocationColumn] as IAveTaxonomyField;
            IAveTaxonomySession session = physicalSite.AveSPTaxonomySession;
            int LCID = 0;
            termStore = AveTaxonomyFieldUtility.GetTermStore(taxonomyField, session, ref LCID);
            if (termStore == null)
            {
                throw new Exception("Get term store failed.");
            }

            if (!taxonomyField.TermSetId.Equals(Guid.Empty))
            {
                termSet = termStore.GetTermSet(taxonomyField.TermSetId);
            }
            else
            {
                throw new Exception("Taxonomy field term set id is null");
            }
            commomErrorMessage = "RM_TS_SS_Summary";
            switch (jobType)
            {
                case JobType.ImportPhysicalRecords:
                    #region ImportPhysicalRecords

                    //为了去掉CSV文件第一行,所以初始值为-1
                    int lineCount = -1;
                    using (StreamReader sr = new StreamReader(physicalRecordsCSVPath))
                    {
                        while (!sr.EndOfStream)
                        {
                            if (!string.IsNullOrEmpty(sr.ReadLine()))
                            {
                                lineCount++;
                            }
                        }
                    }
                    ReportManager.IncreaseBase(lineCount);
                    BCColumnName = SharePointSettingsDao.GetMedataColumn(setting.ScopeId);
                    folders = SPCommonUtility.GetAllFolders(physicalList);
                    //folders =physicalList.Folders.Select(i => i.Folder).ToList();
                    //folders.Insert(0, physicalList.RootFolder);
                    #endregion
                    break;
                case JobType.UpdateLocation:
                    #region UpdateLocation
                    var discoverFolders = SPCommonUtility.GetAllFolders(physicalList);
                    ReportManager.IncreaseBase(discoverFolders.Count);
                    #endregion
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region Import physical records






  #region 递归收集Location节点下所有的Box
        /// <summary>
        /// 递归收集Location节点下所有的Box
        /// </summary>
        /// <param name="location"></param>
        /// <param name="listBox">收集所有Box</param>
        /// <param name="listLocation">收集所有Location</param>
        void TraverseLocation(RMLocation location,List<RMLocationEx> listBox,List<RMLocation> listLocation)
        {
            double avail = location.AvailableSpace;
            string parentPath = location.DirPath;
            bool isHave = RMLocationDAO.HasSubLocation(location.Id);
            if(isHave)
            {
                List<RMLocation> locations = RMLocationDAO.GetAllSubLocationByParentId(location.ParentId);
                if (locations != null && locations.Count > 0)
                {
                    locations.ForEach(p=> 
                    {
                        if (p.NodeType == (int)RMNodeLevel.PhysicalFile
                        || p.NodeType == (int)RMNodeLevel.PhysicalRecord)
                        {
                            return;
                        }

                        //收集box
                        if (p.NodeType == (int)RMNodeLevel.PhysicalBox)
                        {
                            RMLocationEx loca = (RMLocationEx)p;
                            loca.Hierarchy = p.DirPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                            listBox.Add(loca);
                        }
                        //收集locaion
                        if(p.NodeType==(int)RMNodeLevel.PhysicalBottomLocation
                        ||p.NodeType==(int)RMNodeLevel.PhysicalNormalLocation
                        ||p.NodeType==(int)RMNodeLevel.PhysicalRootLocation)
                        {
                            listLocation.Add(p);
                        }
                        TraverseLocation(p,listBox,listLocation);
                    });
                }
            }
        }
        #endregion




        public void RunAvailableSpaceReportJobPhysical(string profileId, string jobId)
        {
            JobStatus status = JobStatus.None;
            //ReportMangerFactory.Instance.Init(jobId, JobType.AvailableSpaceReport, true);
            ReportManager.Increase(2);
            ReportManager.StartUpdateJobProgress();
            Result = new JobResult();
            try
            {
                logger.Info("Beging Available Space Report Job.");
                //TODO:
                /*
                 1.根据ProfileId,找到所有的location,
                 2.找到每个location所有下面box
                 */
                int locationId = 0;
                RMLocation location = RMLocationDAO.GetLocationById(locationId);
                List<RMLocationEx> lstBox = new List<RMLocationEx>();
                List<RMLocation> lstLocation = new List<RMLocation>();
                TraverseLocation(location, lstBox,lstLocation);

                //CollectReport



            }
            catch (JobStopException ex)
            {
                status = JobStatus.Stopped;
                logger.Info($"Available Space Report job is stopped,message:{ex.Message}");

            }
            catch(Exception ex)
            {
                status = JobStatus.Failed;
                logger.Error($"Available Space Report job error,message:{ex.Message}");
            }
            finally
            {
                ReportManager.SetJobFinished(status);
            }



        }






        
        public AveCamlQuery InitBoxSPQuery(IAveFolder boxFolder)
        {
            string boxUrl = boxFolder.ServerRelativeUrl.Substring(1, boxFolder.ServerRelativeUrl.Length - 1);
            CAMLManager caml = new CAMLManager();
            //caml.AddViewFields("REVIMBoxType");
            //caml.OrderBy.Add(new OrderBy("Created", true));
            caml.QueryGroup.AddCondition(new QueryCondition(Types.JoinTypes.And, Types.FieldRefTypes.Name, "ContentType", Types.FieldTypes.Text, Types.QueryTypes.Eq, "Physical Box"));
            caml.QueryGroup.AddCondition(new QueryCondition(Types.JoinTypes.And, Types.FieldRefTypes.Name, "FileRef", Types.FieldTypes.Text, Types.QueryTypes.Contains, boxUrl));

            AveCamlQuery query = new AveCamlQuery();
            caml.ScopeType = Types.ScopeTypes.RecursiveAll;
            //cm.RowLimit = rowLimit;
            string queryXml = caml.GetFullCAML();
            logger.Info("Test Physical Box xml:{0}", queryXml);
            query.ViewXml = queryXml;
            query.DatesInUtc = true;
            query.FolderServerRelativeUrl = boxFolder.ServerRelativeUrl;
            return query;
        }

        public string GetLocationTermPath(Guid id)
        {
            string termFullPath = termDao.GetTermNamesPathByTermId(id);
            if (!string.IsNullOrEmpty(termFullPath))
            {
                termFullPath = termFullPath.Substring(termFullPath.IndexOf("/") + 1);
                termFullPath = termFullPath.Replace("/", " - ");
            }
            return termFullPath;
        }
        #endregion

        #region Other
        /// <summary>
        /// to do next , get locationinfo from RMDB/
        /// use siteid or sitepath ,web id, default listname. how to get lib???one library init once....Parm  to do next..
        /// </summary>
        /// <param name="node"></param>
        private async Task<RemoteSiteCollection> InitObjectAsync(RMSharePointSetting setting)
        {
            SharePointSettingUtility spUtility = new SharePointSettingUtility();
            RemoteSiteCollection node = null;
            try
            {
                node = spUtility.GetRemoteSiteCollection(setting.SiteId.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            try
            {
                if (node != null)
                {
                    try
                    {
                        CommonClientContext commonContext = new CommonClientContext();
                        context = commonContext.InitClientContext(node);
                    }
                    catch (Exception e)
                    {
                        logger.Warn("init client context error:{0}", e.ToString());
                    }
                    if (physicalSite == null || !physicalSite.ID.Equals(mCurrentSiteId))
                    {
                        var mfactory = MultiAppUtil.CreateAveObjectModelFactory(node.url, await PoolUserUtil.GetBPOSInfoAsync(node), AveContextKind.ClientObjectModel);
                        physicalSite = mfactory.CreateSite(node.url);
                    }
                    if (physicalWeb == null || !physicalWeb.ID.Equals(mCurrentWebId))
                    {
                        if (setting.WebId == null || setting.WebId.Equals(Guid.Empty))
                        {
                            physicalWeb = physicalSite.RootWeb;
                        }
                        else
                        {
                            physicalWeb = physicalSite.OpenWeb(setting.WebId);
                        }
                    }
                    if (setting.ListId == null || setting.ListId.Equals(Guid.Empty))
                    {
                        physicalList = physicalWeb.GetListByName(Common.Util.GetAppSettingValue("RevIMHoldPhysicalLibraryName"), false);
                    }
                    else
                    {
                        physicalList = physicalWeb.GetList(setting.ListId);
                    }
                    LocationColumn = Common.Util.GetAppSettingValue("RevIMHomeLocationName");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("InitObject physical failed,message:{0}", ex.ToString()));
            }
            return node;
        }

        #region for sharepoint query
        protected CAMLManager InitCamlQuery(IAveTaxonomyField taxonomyField, List<Guid> termIds)
        {
            CAMLManager cm = new CAMLManager();
            foreach (var termId in termIds)
            {
                int wssid;
                if (GetWssidOfTerm(taxonomyField, termId, out wssid))
                {
                    QueryCondition condition = QueryConditionFactory.GetTaxonomyQueryCondition(taxonomyField.InternalName, new int[] { wssid }, Types.JoinTypes.Or);
                    cm.QueryGroup.AddCondition(condition);
                }

            }
            if (cm.QueryGroup.Conditions.Count > 0)
            {
                cm.AddViewFields("Title");
                cm.AddViewFields("FileRef");
                cm.AddViewFields(LocationColumn);
                cm.AddViewFields("Author");
                cm.AddViewFields("Created");
                cm.AddViewFields("Editor");
                cm.AddViewFields("Modified");
                return cm;
            }
            else
            {
                return null;
            }
        }
        protected bool GetWssidOfTerm(IAveTaxonomyField taxonomyField, Guid termId, out int wssid)
        {
            bool result = false;
            if (!mTermWssidMappingsOfSite.TryGetValue(termId, out wssid))
            {
                try
                {
                    wssid = int.Parse(GetWssIDForTerm(termId));
                    if (wssid > 0)
                    {
                        result = true;
                        mTermWssidMappingsOfSite.Add(termId, wssid);
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn("Get TermId And WssId Mapping failed! Term id: {0}. Error message: {1}.", termId, ex.ToString());
                }
            }
            else if (wssid != 0)
            {
                return true;
            }
            return result;
        }
        #endregion

        private bool CheckLocationCanBeDelete(IAveFolder folder, Guid termId, RMTerm term = null)
        {
            if (folder.ItemCount > 0)
            {
                logger.Info("Term can't delete because folder have items {0}", folder.ServerRelativeUrl);
                return false;
            }
            List<Guid> termIds = new List<Guid>();
            if (term == null)
            {
                termIds.Add(termId);
            }
            else
            {
                termIds = GetAllSubTermIds(term);
            }
            int rowLimit = GetMaxItemsPerThrottledOperation(this.physicalSite);
            List<CAMLManager> cms = new List<CAMLManager>();
            const int queryConditionMaxCount = 500;
            if (termIds.Count < queryConditionMaxCount)
            {
                CAMLManager cm = InitCamlQuery(currentLocationColumn, termIds);
                if (cm != null)
                {
                    cms.Add(cm);
                }
            }
            else
            {
                int index = 0;
                while (termIds.Skip(index).Take(queryConditionMaxCount) != null && termIds.Skip(index).Take(queryConditionMaxCount).Count() != 0)
                {
                    var queryIds = termIds.Skip(index).Take(queryConditionMaxCount).ToList();
                    index += queryConditionMaxCount;
                    if (queryIds.Count() != 0)
                    {
                        CAMLManager cm = InitCamlQuery(currentLocationColumn, queryIds);
                        if (cm != null)
                        {
                            cms.Add(cm);
                        }
                    }
                }
            }
            if (cms.Count != 0)
            {
                var discoverFolders = physicalList.Folders.Select(i => i.Folder).ToList();
                discoverFolders.Insert(0, physicalList.RootFolder);

                foreach (var discoverFolder in discoverFolders)
                {
                    foreach (CAMLManager cm in cms)
                    {
                        //UpdateJobWithoutProgressChange();//更新job进度，防止因为数据量太大导致job超时
                        AveCamlQuery query = new AveCamlQuery();
                        cm.ScopeType = Types.ScopeTypes.Default;
                        cm.RowLimit = rowLimit;
                        string queryXml = cm.GetFullCAML();
                        query.ViewXml = queryXml;
                        query.DatesInUtc = true;
                        query.FolderServerRelativeUrl = discoverFolder.ServerRelativeUrl;
                        logger.Info("check deletion term query xml {0}:{1}", discoverFolder.ServerRelativeUrl, queryXml);
                        IAveListItemCollection items = physicalList.GetItems(query);
                        if (items.Count > 0)
                        {
                            logger.Info("Term can't delete because have been used {0}", folder.ServerRelativeUrl);
                            return false;
                        }
                        while (items.ListItemCollectionPosition != null)
                        {
                            items = physicalList.GetItems(query);
                            if (items.Count > 0)
                            {
                                logger.Info("Term can't delete because have been used {0}", folder.ServerRelativeUrl);
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }


        /// <summary>
        /// delete path 
        /// </summary>
        public void DeleteLocation(IAveFolder folder, RMTerm term, RMLocationAssociation location)
        {
            try
            {
                if (CheckLocationCanBeDelete(folder, Guid.Empty, term))
                {
                    string url = folder.Url;
                    folder.Delete();
                    LocationDAO.DeleteLocationAssocation(location);
                    logger.Info("Delete folder {0} success", folder.Url);
                    SendJobDetail(new JMPhysicalSyncJobDetails()
                    {
                        Action = "Delete",
                        Comment = "Delete location",
                        LocationPath = url,
                        SiteCollectionURL = this.physicalSite.Url,
                        Status = JobDetailsStatus.Successful,
                        TermName = term.Name
                    });
                }
                else
                {
                    logger.Info("Can't delete folder {0} ", folder.Url);
                    if (!NodeleteTerms.Contains(term.UniqueId))
                    {
                        NodeleteTerms.Add(term.UniqueId);
                    }
                    SendJobDetail(new JMPhysicalSyncJobDetails()
                    {
                        Action = "Delete",
                        Comment = "Delete location failed",
                        LocationPath = folder.Url,
                        SiteCollectionURL = this.physicalSite.Url,
                        Status = JobDetailsStatus.Failed,
                        TermName = term.Name
                    });
                    HasErrorNode = true;
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Delete folder {0} failed:{1}", folder.Url, ex.ToString());
                if (!NodeleteTerms.Contains(term.UniqueId))
                {
                    NodeleteTerms.Add(term.UniqueId);
                }
                SendJobDetail(new JMPhysicalSyncJobDetails()
                {
                    Action = "Delete",
                    Comment = "Delete location failed",
                    LocationPath = folder.Url,
                    SiteCollectionURL = this.physicalSite.Url,
                    Status = JobDetailsStatus.Failed,
                    TermName = term.Name
                });
                HasErrorNode = true;
            }
        }
        private IAveTaxonomyField GetTaxonomyField(IAveFieldCollection fields, string rmFieldTitle)
        {
            var field = fields.GetField(rmFieldTitle);
            return field as IAveTaxonomyField;
        }
        /// <summary>
        /// including current term
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        private List<Guid> GetAllSubTermIds(RMTerm term)
        {
            List<Guid> termIds = new List<Guid>();
            termIds.Add(term.UniqueId);
            if (term.subTerms != null)
            {
                foreach (var subTerm in term.subTerms)
                {
                    AddSubTermUniqueId(subTerm, termIds);
                }
            }
            return termIds;
        }
        private void AddSubTermUniqueId(RMTerm term, List<Guid> termIds)
        {
            termIds.Add(term.UniqueId);
            if (term.subTerms != null)
            {
                foreach (var subTerm in term.subTerms)
                {
                    AddSubTermUniqueId(subTerm, termIds);
                }
            }
        }
        protected int GetMaxItemsPerThrottledOperation(IAveSite discoverSite)
        {
            int maxItemsPer = 5000;
            try
            {
                var dataCacheType = discoverSite.GetType().GetProperty("DataCache");
                var dataCacheObj = dataCacheType.GetValue(discoverSite);
                var propertiesCacheProp = dataCacheObj.GetType().GetProperty("PropertiesCache");
                var propertiesCacheObj = propertiesCacheProp.GetValue(dataCacheObj);
                var propertiesDic = (propertiesCacheObj as Dictionary<string, object>);
                object maxItemsPerObj;
                if (propertiesDic.TryGetValue("MaxItemsPerThrottledOperation", out maxItemsPerObj))
                {
                    maxItemsPer = Convert.ToInt32(maxItemsPerObj);
                }
            }
            catch (Exception ex)
            {
                logger.Warn("GetMaxItemsPerThrottledOperation by siteCollection NodeItem faild, Error message:", ex.ToString());
            }
            return maxItemsPer;
        }
        #region for job
        private void UpdateJobProgress(int progress)
        {
            if (!string.IsNullOrEmpty(this.mCurrentJobId))
            {
                JobMonitorService.UpdateJobProgress(this.mCurrentJobId, progress);
            }
        }
        //Thread to send detail
        private void SendJobDetail(JMPhysicalSyncJobDetails detail)
        {
            //locker
            RASimpleLocker.Locker locker = _simpleLocker.GetLocker(this.mCurrentJobId);

            lock (locker)
            {
                try
                {
                    if (this.syncJobDetails != null)
                    {
                        syncJobDetails.Add(detail);
                    }
                    ArgumentCheck.CheckNotNull(syncJobDetails);
                    if (syncJobDetails?.Count >= 5)
                    {
                        //use thread to update detail to do next......

                        List<JMPhysicalSyncJobDetails> details = new List<JMPhysicalSyncJobDetails>();
                        foreach (var sdetail in syncJobDetails)
                        {
                            details.Add(sdetail);
                        }
                        RunUpdateJobDetails(details);
                        this.syncJobDetails.Clear();
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("Add Report Error {0} {1}", this.mCurrentJobId, e.ToString());
                }
                finally
                {
                    _simpleLocker.FreeLocker(locker.Key);
                }
            }
        }
        private void SendJobDetail()
        {
            //locker
            SimpleLocker.Locker locker = _simpleLocker.GetLocker(this.mCurrentJobId);

            lock (locker)
            {
                try
                {
                    //use thread to update detail to do next......
                    List<JMPhysicalSyncJobDetails> details = new List<JMPhysicalSyncJobDetails>();
                    foreach (var sdetail in syncJobDetails)
                    {
                        details.Add(sdetail);
                    }
                    RunUpdateJobDetails(details);
                    this.syncJobDetails.Clear();

                }
                catch (Exception e)
                {
                    logger.Warn("Add Report Error {0} {1}", this.mCurrentJobId, e.ToString());
                }
                finally
                {
                    _simpleLocker.FreeLocker(locker.Key);
                }
            }
        }
        private void RunUpdateJobDetails(List<JMPhysicalSyncJobDetails> physicalDetails)
        {
            List<JMPhysicalSyncJobDetails> needUpdateDetails = physicalDetails;
            //AveThreadWrapper updateDetails = new AveThreadWrapper(new ParameterizedThreadStart(UpdateJobDetails), needUpdateDetails, "TermSyncUpdateDetails");
            //updateDetails.Start();
            UpdateJobDetails(physicalDetails);//暂时改成同步的
        }
        private void UpdateJobDetails(object details)
        {
            List<JMPhysicalSyncJobDetails> syncJobDetails = (List<JMPhysicalSyncJobDetails>)details;
            JobDetailService.UpdateJobDetails(syncJobDetails, baseJobDto);
        }
        private int CalculateProgress(int numerator, int denominator)
        {
            double progressCount = 0;
            if (numerator == denominator)
            {
                progressCount = 99;
            }
            else
            {
                progressCount = (double)numerator / (double)denominator * 85 + 15;
            }
            return (int)progressCount;
        }
        #endregion

        public Dictionary<string, int> GetPhysicalLibrarysInfo()
        {
            Dictionary<string, int> infos = new Dictionary<string, int>();
            foreach (var setting in PhysicalSettings)
            {
                try
                {
                    RMSharePointColumn.RMSharePointColumn rmSharePointColumn = new RMSharePointColumn.RMSharePointColumn();
                    string path = rmSharePointColumn.GetPhysicalLibraryPath(setting);
                    if (!string.IsNullOrEmpty(path))
                    {
                        infos.Add(path, setting.Id);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("An error occurred when get physical library info. fullpath:{0}, Error:{1}", setting.FullPath, ex.ToString());
                }
            }
            return infos;
        }
        #endregion
        #endregion

        private string GetWssIDForTerm(Guid termId)
        {
            try
            {
                string result = "-1";
                List taxonomyList = this.context.Web.Lists.GetByTitle("TaxonomyHiddenList");
                CamlQuery camlQueryForTerm = new CamlQuery();
                camlQueryForTerm.ViewXml = @"
<View>
    <Query>
        <Where>
            <Eq>
                <FieldRef Name='IdForTerm' />
                <Value Type='Text'>" + termId + @"</Value>
            </Eq>
        </Where>
    </Query>       
</View>";
                ListItemCollection termItems = taxonomyList.GetItems(camlQueryForTerm);
                this.context.Load(termItems);
                this.context.ExecuteQuery();
                //foreach (var termItem in termItems)
                //{
                //    return termItem["ID"].ToString();
                //}
                var termItem = termItems?.FirstOrDefault();
                if (termItem != null)
                {
                    result = termItem["ID"].ToString();
                }
                return result;
            }
            catch (Exception e1)
            {
                return "-1";
            }
        }
    }
    public class RMLocationEx : RMLocation
    {
        public List<string> Hierarchy { get; set; }
    }
}

