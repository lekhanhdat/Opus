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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object.RealTime;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.RAPhysical.ExplorerMove;
using AvePoint.RA.SharePoint.RMExplorer;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AvePoint.RA.DB.Dao.Extension;
namespace RAGlobalSearch.Actions
{
    public class MoveAction : IGlobalSearchAction
    {
        private AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(MoveAction));
        private IRMReportManager mReportManager;
        public IRMReportManager ReportManager
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

        private ISharePointSettingDao mSharePointSettingDao = null;
        public ISharePointSettingDao SharePointSettingDao
        {
            get
            {
                if (mSharePointSettingDao == null)
                {
                    mSharePointSettingDao = (ISharePointSettingDao)PlatformWindsorManager.GetService(typeof(ISharePointSettingDao));
                }
                return mSharePointSettingDao;
            }
        }
        private ITeamsSettingDao mTeamsSettingDao = null;
        public ITeamsSettingDao TeamsSettingDao
        {
            get
            {
                if (mTeamsSettingDao == null)
                {
                    mTeamsSettingDao = (ITeamsSettingDao)PlatformWindsorManager.GetService(typeof(ITeamsSettingDao));
                }
                return mTeamsSettingDao;
            }
        }
        private ITermSetDao mTermSetDao { get; set; }
        public ITermSetDao TermSetDao
        {
            get
            {
                if (mTermSetDao == null)
                {
                    mTermSetDao = (ITermSetDao)PlatformWindsorManager.GetService(typeof(ITermSetDao));
                }
                return mTermSetDao;
            }
        }
        private ITermDao mTermDao;
        public ITermDao TermDao
        {
            get
            {
                if (mTermDao == null)
                {
                    mTermDao = (ITermDao)PlatformWindsorManager.GetService(typeof(ITermDao));
                }
                return mTermDao;
            }
        }

        private IOneDriveSettingDao mOneDriveSettingDao = null;
        public IOneDriveSettingDao OneDriveSettingDao
        {
            get
            {
                if (mOneDriveSettingDao == null)
                {
                    mOneDriveSettingDao = (IOneDriveSettingDao)PlatformWindsorManager.GetService(typeof(IOneDriveSettingDao));
                }
                return mOneDriveSettingDao;
            }
        }
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private bool avalibleTeams;
        private int mFailedCount;
        private int mSuccessCount;
        private Dictionary<string, RMSharePointSetting> mSettingDic = null;
        private Dictionary<string, RMTeamsSetting> mTeamsSettingDic = null;
        private ConcurrentDictionary<string, RemoteSiteCollection> mDestinationDic = new ConcurrentDictionary<string, RemoteSiteCollection>();

        private List<RMOneDriveSetting> mAllSettings = new List<RMOneDriveSetting>();
        private Dictionary<Guid, Dictionary<Guid, bool>> mTermAllowToParent = new Dictionary<Guid, Dictionary<Guid, bool>>();
        private Dictionary<Guid, string> mTermPaths = new Dictionary<Guid, string>();

        public MoveAction()
        {
            mSettingDic = new Dictionary<string, RMSharePointSetting>();
            mTeamsSettingDic = new Dictionary<string, RMTeamsSetting>();
            mAllSettings = OneDriveSettingDao.LoadAllSetting();
            avalibleTeams = RMKeyValueDao.HasUpgradeTeams() && RMKeyValueDao.EnableTeamsFeature();
        }
        public async Task DoActionAsync(List<BaseRecordDto> records, SourceFlag flag, object actionExtension, string jobId, bool isJob)
        {
            logger.Info("Start process move action.");
            try
            {
                switch (flag)
                {
                    case SourceFlag.SharePoint:
                    case SourceFlag.OneDrive:
                    case SourceFlag.Teams:
                    case SourceFlag.Groups:
                        RMExplorerMoveJobMessage msg = SerializerHelper.DeserializeByDataContractSerializer<RMExplorerMoveJobMessage>(actionExtension.ToString());
                        await MoveSPDataAsync(msg);
                        break;
                    case SourceFlag.Physical:
                        PhysicalMoveOption moveOption = SerializerHelper.DeserializeByDataContractSerializer<PhysicalMoveOption>(actionExtension.ToString());
                        moveOption.SourcePhyRecordIds = records.Select(r => r.NodeId).ToList();
                        await MovePhysicalDataAsync(moveOption, jobId);
                        break;
                }
            }
            catch (Exception e)
            {
                mFailedCount++;
                logger.Error($"An error occurred while doing MoveAction. Error:{e.ToString()}");
            }
            logger.Info("Process move action finished.");
            return;
        }

        private Task MovePhysicalDataAsync(PhysicalMoveOption moveOption, string jobId)
        {
            RMPhysicalExplorerMoveUtility phyMoveUtility = new RMPhysicalExplorerMoveUtility();
            return phyMoveUtility.MoveAsync(moveOption, jobId);
        }

        private async Task MoveSPDataAsync(RMExplorerMoveJobMessage message)
        {
            logger.Info("Begin moving");
            RMExplorerMoveDBUtil dbUtil = null;
            try
            {
                //Init Explorer Dao 
                dbUtil = new RMExplorerMoveDBUtil();
                //Init Record DB
                //RMDBSetting.ConnectionDatabaseString = Encoding.UTF8.GetString(CspCommunicationWrapper.UnWrapKey(message.RecordsDBInfo.ConnString));
                MoveSettingInfo moveSetting = new MoveSettingInfo(message.MoveSetting);
                //Analysis destination and check destination
                AppendItemMapping appendMapping = new AppendItemMapping();
                //REC-5120 如果想要精确去重，就需要先获取真正的目的端，所以把实例化目的端对象放在前面，方便获取目的端。先检查目的端，也可以避免在目的端无效的时候无意义的discover
                using (MoveDestinationManager desInfo = new MoveDestinationManager(message, moveSetting, appendMapping))
                {
                    //Analysis source
                    MoveSourceManager sourceInfo = new MoveSourceManager(message, desInfo, true);
                    long totalCount = await sourceInfo.CalculateTotalCountAsync();
                    ReportManager.IncreaseBase(totalCount);
                    Stack<Action> stack = new Stack<Action>();
                    Thread t = new Thread(new ThreadStart(() =>
                    {
                        try
                        {
                            while (true)
                            {
                                logger.Info("PCContainer count : {0}", sourceInfo.DiscoverCache.PCContainer.Count);
                                Thread.Sleep(5000);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Warn("Error in output container count, reason : " + ex.ToString());
                        }
                    }));
                    t.IsBackground = true;
                    t.Start();
                    foreach (SourceBase source in sourceInfo.DiscoverCache)
                    {
                        Guid sourceTermID = Guid.Empty;
                        RemoveNodeType sourceType = RemoveNodeType.SiteCollection;
                        if (source.NodeLevel <= sourceInfo.UpdateProgressNodeLevel)
                        {
                            ReportManager.Increase();
                        }
                        JobResult jobResult = new JobResult();
                        try
                        {
                            source.MoveBackup();
                            logger.Info($"Export source object finish, node id : {source.NodeId}, file type : {source.NodeType}, id : {source.Id}, scopeId : {source.ScopeId}.");
                            try
                            {
                                jobResult = await desInfo.Destination.MoveRestoreAsync(source);
                                logger.Info($"Import source object finish, node id : {source.NodeId}, status is : {jobResult.Status.ToString()}, id : {source.Id}, scopeId : {source.ScopeId}.");
                                try
                                {
                                    if (jobResult.Status == JobDetailsStatus.Successful)
                                    {
                                        if (source.NodeType == (int)NodeLevel.FSFolder)
                                        {
                                            //提前获取desName，防止Append mapping中下个文件与当前文件同名，导致append name 被改变
                                            //string desName = appendMapping.ContainsKeyAppendName(source.FileName) ? appendMapping.GetValueAppendName(source.FileName) : source.FileName;
                                            stack.Push(delegate
                                            {
                                                try
                                                {
                                                    try
                                                    {
                                                        source.Delete();
                                                    }
                                                    catch (Exception delException)
                                                    {
                                                        jobResult.ErrorMessage = delException.Message.ToString();
                                                        jobResult.Status = JobDetailsStatus.Failed;
                                                        mFailedCount++;
                                                        logger.Error(string.Format("Error in delete node, reason : {0}.", delException.ToString()));
                                                    }
                                                    try
                                                    {
                                                        dbUtil.UpdateMovedRecords(jobResult, source, desInfo, message.Operator, null, Guid.Empty);
                                                    }
                                                    catch (Exception exc)
                                                    {
                                                        logger.Error(string.Format("Error in update moved record, reason : {0}", exc.ToString()));
                                                    }
                                                    string desUrl = GetDestinationReportUrl(source, desInfo.Destination, moveSetting, appendMapping, jobResult);
                                                    //jobManager.ReportService.Commit(new MoveReportEntity(source.FileName, CommonUtil.ConvertNodeTypeToReportType(source.NodeType), source.SourceUrl, desUrl, jobResult.Status, jobResult.ErrorMessage));
                                                    //mReportManager.SendJobDetail(new JMExplorerMoveJobDetails() { ObjectName = source.FileName, FullPath = source.SourceUrl, DestinationFullPath = desUrl, Status = jobResult.Status, Comment = jobResult.ErrorMessage });
                                                    AddDetail(source, desUrl, JobDetailsStatus.Successful, jobResult.ErrorMessage);
                                                    logger.Info(string.Format("finished move : {0}.", source.Id));
                                                }
                                                catch (Exception ex)
                                                {
                                                    logger.Warn(string.Format("Error in delete object, reason : {0}", ex.ToString()));
                                                }
                                            });
                                            logger.Info(string.Format("finished add : {0} to deletion stack.", source.Id));
                                            continue;
                                        }
                                        else
                                        {
                                            if (source is SPSource)
                                            {
                                                var spSource = (SPSource)source;
                                                var siteUrl = spSource.SiteUrl;
                                                var destSite = GetDestinationSite(siteUrl);
                                                sourceType = destSite.NodeType;
                                                if (destSite.NodeType != RemoveNodeType.SkyDrivePro)
                                                {
                                                    if ((destSite.NodeType == RemoveNodeType.O365GroupSites || destSite.NodeType == RemoveNodeType.PrivateChannel)&& avalibleTeams)
                                                    {
                                                        var siteSetting = GetDestinationTeamsSetting(destSite);
                                                        bool useExisting = siteSetting.IsUsingExistColumnName;
                                                        string columnName = useExisting ? siteSetting.ExistColumnName : siteSetting.ColumnName;
                                                        sourceTermID = source.GetSourceTermId(columnName);
                                                        logger.Info("Destination teams term is:{0}", sourceTermID);
                                                    }
                                                    else
                                                    {
                                                        var siteSetting = GetDestinationSPSetting(destSite);
                                                        bool useExisting = siteSetting.IsUsingExistColumnName;
                                                        string columnName = useExisting ? siteSetting.ExistColumnName : siteSetting.ColumnName;
                                                        sourceTermID = source.GetSourceTermId(columnName);
                                                        logger.Info("Destination term is:{0}", sourceTermID);
                                                    }
                                                }
                                            }
                                            source.Delete();
                                        }

                                    }
                                    else
                                    {
                                        logger.Info(string.Format("Skip deleting the source, as the status is : {0}.", jobResult.Status.ToString()));
                                        if (jobResult.Status == JobDetailsStatus.Failed)
                                        {
                                            mFailedCount++;
                                        }
                                    }
                                }
                                catch (Exception deletionException)
                                {
                                    jobResult.ErrorMessage = deletionException.Message.ToString();
                                    jobResult.Status = JobDetailsStatus.Failed;
                                    mFailedCount++;
                                    logger.Error("Error in deletion, reason : " + deletionException.ToString());
                                }
                            }
                            #region Exception Types
                            catch (ConetentSkipException contentExp)
                            {
                                jobResult.Status = JobDetailsStatus.Skipped;
                                jobResult.ErrorMessage = contentExp.Message.ToString();
                                mSuccessCount++;
                                logger.Warn(string.Format("Content Skip: FileName: {0}, reason: {1}.", source.FileName, contentExp.ToString()));
                            }
                            //File length exceed 128 catch exception
                            catch (PathTooLongException e)
                            {
                                jobResult.Status = JobDetailsStatus.Failed;
                                jobResult.ErrorMessage = e.Message.ToString();
                                mFailedCount++;
                                logger.Warn(string.Format("File name or list URL is too long. Reason : {0}.", e.ToString()));
                            }
                            catch (SkipException skipEx)
                            {
                                jobResult.Status = JobDetailsStatus.Failed;
                                jobResult.ErrorMessage = skipEx.Message.ToString();
                                mFailedCount++;
                                logger.Warn("Content Type Or Column conflict, Skip Current file : {0}, Message : {1}", source.FileName, skipEx.ToString());
                            }
                            #endregion
                            catch (Exception exp)
                            {
                                jobResult.ErrorMessage = exp.Message.ToString();
                                jobResult.Status = JobDetailsStatus.Failed;
                                mFailedCount++;
                                logger.Error("Error in move restore, source url : {0}, destination url : {1}, reason : {2}.", source.SourceUrl, desInfo.Destination.DestinationContainerUrl, exp.ToString());
                            }

                        }
                        catch (Exception ex)
                        {
                            jobResult.ErrorMessage = ex.Message.ToString();
                            jobResult.Status = JobDetailsStatus.Failed;
                            mFailedCount++;
                            logger.Error("Error in move backup, source url : {0}, reason : {1}.", source.SourceUrl, ex.ToString());
                        }
                        bool failedUpdateColumn = false;

                        try
                        {
                            //update bsc column
                            if (jobResult.Status == JobDetailsStatus.Successful)
                            {
                                UpdateBCSColumn(source, dbUtil, desInfo, sourceType, sourceTermID);
                            }
                        }
                        catch (Exception e)
                        {
                            jobResult.ErrorMessage = e.Message.ToString();
                            //jobResult.Status = JobDetailsStatus.Failed;
                            logger.Error("An error occurred while updating bcs column, error:{0}", e.ToString());
                            failedUpdateColumn = true;
                        }
                        try
                        {
                            var destSite = GetDestinationSite(desInfo.DestRootPath);
                            Guid destTermId = Guid.Empty;
                            if (destSite.NodeType != RemoveNodeType.SkyDrivePro)
                            {
                                if ((destSite.NodeType == RemoveNodeType.O365GroupSites || destSite.NodeType == RemoveNodeType.PrivateChannel) && avalibleTeams)
                                {
                                    logger.Info("Try to get teams term id in destination.");
                                    var siteSetting = GetDestinationTeamsSetting(destSite);
                                    destTermId = desInfo.Destination.GetDestinationTermId(siteSetting.IsUsingExistColumnName ? siteSetting.ExistColumnName : siteSetting.ColumnName);
                                }
                                else
                                {
                                    logger.Info("Try to get term id in destination.");
                                    //get destination term id
                                    var siteSetting = GetDestinationSPSetting(destSite);
                                    destTermId = desInfo.Destination.GetDestinationTermId(siteSetting.IsUsingExistColumnName ? siteSetting.ExistColumnName : siteSetting.ColumnName);
                                }
                            }
                            dbUtil.UpdateMovedRecords(jobResult, source, desInfo, message.Operator, destSite, destTermId, failedUpdateColumn);
                        }
                        catch (Exception exc)
                        {
                            logger.Error(string.Format("Error in update moved record, reason : {0}", exc.ToString()));
                        }
                        string desFileUrl = GetDestinationReportUrl(source, desInfo.Destination, moveSetting, appendMapping, jobResult);
                        //jobManager.ReportService.Commit(new MoveReportEntity(source.FileName, CommonUtil.ConvertNodeTypeToReportType(source.NodeType), source.SourceUrl, desFileUrl, jobResult.Status, jobResult.ErrorMessage));
                        //mReportManager.SendJobDetail(new JMExplorerMoveJobDetails() { ObjectName = source.FileName, ItemType = CommonUtil.ConvertNodeTypeToReportType(source.NodeType), FullPath = source.SourceUrl, DestinationFullPath = desFileUrl, Status = jobResult.Status, Comment = jobResult.ErrorMessage });
                        if (failedUpdateColumn)
                        {
                            jobResult.Status = JobDetailsStatus.Failed;
                            mFailedCount++;
                        }
                        else
                        {
                            mSuccessCount++;
                        }
                        //AddDetail(source, desFileUrl, jobResult.Status, jobResult.ErrorMessage);
                        AddDetail(source, desFileUrl, jobResult.Status, jobResult.ErrorMessage, jobResult);
                        logger.Info(string.Format("finished move object: {0}, status is : {1}.", source.Id, jobResult.Status.ToString()));
                    }
                    logger.Info("start to run post deletion");
                    foreach (var action in stack)
                    {
                        try
                        {
                            action();
                        }
                        catch (Exception delException)
                        {
                            logger.Error(string.Format("Error in delete node, reason : {0}.", delException.ToString()));
                        }
                    }
                    //Final update db, clear the updateDBInfo cache
                    //dbUtil.RealUpdateExplorerDB();
                    mFailedCount += sourceInfo.FailedCount;
                    mSuccessCount += sourceInfo.SuccessCount;
                }
                logger.Info("Move Job finished");
            }
            catch (Exception ex)
            {
                mFailedCount++;
                logger.Warn(string.Format("Error in Job, reason : {0}.", ex.ToString()));
                //mReportManager.SendJobDetail(new JMExplorerMoveJobDetails() { ObjectName = "", ItemType = "", FullPath = "", DestinationFullPath = "", Status = JobDetailsStatus.Failed, Comment = ex.Message });
                AddDetail(null, "", JobDetailsStatus.Failed, ex.Message);
            }
            finally
            {
                logger.Info(string.Format("Update Job status to server, has error node : {0}.", mFailedCount));
            }
        }
        private RemoteSiteCollection GetDestinationSite(string destUrl)
        {
            RemoteSiteCollection site;
            if (mDestinationDic.ContainsKey(destUrl))
            {
                site = mDestinationDic[destUrl];
            }
            else
            {
                site = RABrowserClient.GetRemoteSiteCollectionByListUrl(destUrl);
                mDestinationDic.TryAdd(destUrl, site);
            }
            return site;
        }
        private Guid UpdateBCSColumn(SourceBase source, RMExplorerMoveDBUtil dbUtil, MoveDestinationManager desInfo, RemoveNodeType sourceType, Guid sourceTermId)
        {
            Guid destTermId = Guid.Empty;
            var destSite = GetDestinationSite(desInfo.DestRootPath);

            if (desInfo.KeepSourceClassification)
            {
                if (sourceType == RemoveNodeType.SkyDrivePro)
                {
                    var sourceRcord = dbUtil.GetRecords(source.Id).First();
                    sourceTermId = sourceRcord.TermId;
                }

                if (destSite.NodeType != RemoveNodeType.SkyDrivePro && source.NodeType != (int)NodeLevel.ItemVersion)
                {
                    if (sourceTermId != Guid.Empty)
                    {
                        if ((destSite.NodeType == RemoveNodeType.O365GroupSites || destSite.NodeType == RemoveNodeType.PrivateChannel) && avalibleTeams)
                        {
                            //update teams bcs column
                            logger.Info("Start to update teams bcs column in destination. Destination url:{0}", desInfo.DestRootPath);
                            var siteSetting = GetDestinationTeamsSetting(destSite);
                            bool useExisting = siteSetting.IsUsingExistColumnName;
                            string columnName = useExisting ? siteSetting.ExistColumnName : siteSetting.ColumnName;
                            string errorMessage = desInfo.Destination.UpdateBCSColumn(useExisting, columnName, sourceTermId);
                            if (!string.IsNullOrWhiteSpace(errorMessage))
                            {
                                if (errorMessage.Equals("StorageOptimization_SOARRecordManagerEXONotInSameTermScope"))
                                {
                                    destTermId = desInfo.Destination.UpdateClassificationColumnWithDestination(useExisting, columnName);
                                }
                                logger.Info("Update teams bcs column with exception. Message:{0}", errorMessage);
                                throw new Exception(errorMessage);
                            }
                            else
                            {
                                destTermId = sourceTermId;
                            }
                        }
                        else
                        {
                            logger.Info("Start to update bsc column in destination. Destination url:{0}", desInfo.DestRootPath);
                            var siteSetting = GetDestinationSPSetting(destSite);
                            bool useExisting = siteSetting.IsUsingExistColumnName;
                            string columnName = useExisting ? siteSetting.ExistColumnName : siteSetting.ColumnName;
                            string errorMessage = desInfo.Destination.UpdateBCSColumn(useExisting, columnName, sourceTermId);
                            if (!string.IsNullOrWhiteSpace(errorMessage))
                            {
                                if (errorMessage.Equals("StorageOptimization_SOARRecordManagerEXONotInSameTermScope"))
                                {
                                    destTermId = desInfo.Destination.UpdateClassificationColumnWithDestination(useExisting, columnName);
                                }
                                logger.Info("Update bcs column with exception. Message:{0}", errorMessage);
                                throw new Exception(errorMessage);
                            }
                            else
                            {
                                destTermId = sourceTermId;
                            }
                        }
                    }
                    else
                    {
                        if ((destSite.NodeType == RemoveNodeType.O365GroupSites || destSite.NodeType == RemoveNodeType.PrivateChannel) && avalibleTeams)
                        {
                            //source term is null so destination term should also be null
                            logger.Info("Source teams term is null, update destination to null.");
                            var siteSetting = GetDestinationTeamsSetting(destSite);
                            bool useExisting = siteSetting.IsUsingExistColumnName;
                            string columnName = useExisting ? siteSetting.ExistColumnName : siteSetting.ColumnName;
                            destTermId = desInfo.Destination.UpdateClassificationColumnWithDestination(useExisting, columnName, true);
                        }
                        else
                        {
                            //source term is null so destination term should also be null
                            logger.Info("Source term is null, update destination to null.");
                            var siteSetting = GetDestinationSPSetting(destSite);
                            bool useExisting = siteSetting.IsUsingExistColumnName;
                            string columnName = useExisting ? siteSetting.ExistColumnName : siteSetting.ColumnName;
                            destTermId = desInfo.Destination.UpdateClassificationColumnWithDestination(useExisting, columnName, true);
                        }
                    }
                }
                else if (destSite.NodeType == RemoveNodeType.SkyDrivePro)
                {
                    if (desInfo.KeepSourceClassification && sourceTermId != Guid.Empty)
                    {
                        if (!IsSameTermScope(sourceTermId, new Guid(destSite.parentId), desInfo.Destination.DestinationContainerUrl))
                        {
                            logger.Error("Destination is onedrive, current term is not under the term scope.");
                            throw new Exception("StorageOptimization_SOARRecordManagerEXONotInSameTermScope");
                        }
                    }
                }
            }
            else
            {
                if (destSite.NodeType != RemoveNodeType.SkyDrivePro)
                {
                    if ((destSite.NodeType == RemoveNodeType.O365GroupSites || destSite.NodeType == RemoveNodeType.PrivateChannel) && avalibleTeams)
                    {
                        var siteSetting = GetDestinationTeamsSetting(destSite);
                        bool useExisting = siteSetting.IsUsingExistColumnName;
                        string columnName = useExisting ? siteSetting.ExistColumnName : siteSetting.ColumnName;
                        destTermId = desInfo.Destination.UpdateClassificationColumnWithDestination(useExisting, columnName);
                    }
                    else
                    {
                        var siteSetting = GetDestinationSPSetting(destSite);
                        bool useExisting = siteSetting.IsUsingExistColumnName;
                        string columnName = useExisting ? siteSetting.ExistColumnName : siteSetting.ColumnName;
                        destTermId = desInfo.Destination.UpdateClassificationColumnWithDestination(useExisting, columnName);
                    }
                }
            }
            return destTermId;
        }

        private bool IsSameTermScope(Guid sourceTermId, Guid groupId, string destinationUrl)
        {
            RMOneDriveSetting bindSetting = mAllSettings.Where(s => s.SiteGroupId == groupId && destinationUrl.StartsWith(s.FullPath)).OrderBy(s => s.FullPath.Length).FirstOrDefault();
            if (bindSetting == null)
            {
                bindSetting = GetGroupLevelSetting(groupId);
            }

            if (bindSetting == null)
            {
                return false;
            }

            if (CheckTermValue(bindSetting, sourceTermId))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private RMOneDriveSetting GetGroupLevelSetting(Guid groupId)
        {
            var groupSetting = mAllSettings.Where(s => s.SiteGroupId == groupId && s.SiteId == Guid.Empty).FirstOrDefault();
            if (groupSetting != null)
            {
                return groupSetting;
            }
            else
            {
                logger.Warn("Cannot find group setting for site, groupId:{0}", groupId);
                return null;
            }
        }

        private bool CheckTermValue(RMOneDriveSetting setting, Guid termId)
        {
            bool bindTermSet = setting.TermId == Guid.Empty;
            var parentId = bindTermSet ? setting.TermSetId : setting.TermId;
            return CheckTermValue(bindTermSet, parentId, termId);
        }

        private bool CheckTermValue(bool bindTermSet, Guid parentId, Guid termId)
        {
            string termPath = null;
            if (!mTermPaths.TryGetValue(termId, out termPath))
            {
                termPath = TermDao.GetTermIdPath(termId);
                mTermPaths[termId] = termPath;
            }

            if (string.IsNullOrEmpty(termPath))
            {
                return false;
            }

            Dictionary<Guid, bool> parentNodes = null;
            if (!mTermAllowToParent.TryGetValue(termId, out parentNodes))
            {
                parentNodes = new Dictionary<Guid, bool>();
                mTermAllowToParent[termId] = parentNodes;
            }

            string parentNodePath = null;
            bool isSubTerm = false;
            if (!parentNodes.TryGetValue(parentId, out isSubTerm))
            {
                if (bindTermSet)
                {
                    parentNodePath = (TermSetDao.GetRMTermSetByGuid(parentId)?.Id)?.ToString() + "/";
                }
                else
                {
                    parentNodePath = TermDao.GetTermIdPath(parentId) + "/";
                }
                isSubTerm = termPath.StartsWith(parentNodePath, StringComparison.OrdinalIgnoreCase);
                parentNodes[parentId] = isSubTerm;
            }
            return isSubTerm;
        }

        private RMSharePointSetting GetDestinationSPSetting(RemoteSiteCollection destSite)
        {
            if (mSettingDic.ContainsKey(destSite.url))
            {
                return mSettingDic[destSite.url];
            }
            else
            {
                var siteSetting = SharePointSettingDao.LoadSharePointSetting(new Guid(destSite.parentId), new Guid(destSite.id));
                if (siteSetting == null)
                {
                    logger.Info("Site level setting is null, will use group level setting");
                    siteSetting = SharePointSettingDao.LoadSharePointSetting(new Guid(destSite.parentId), Guid.Empty);
                    if (siteSetting == null)
                    {
                        throw new Exception("Cannot find group level setting.");
                    }
                }
                mSettingDic.Add(destSite.url, siteSetting);
                return siteSetting;
            }
        }
        private RMTeamsSetting GetDestinationTeamsSetting(RemoteSiteCollection destSite)
        {
            if (mTeamsSettingDic.ContainsKey(destSite.url))
            {
                return mTeamsSettingDic[destSite.url];
            }
            else
            {
                RMTeamsSetting siteSetting = null;
                if (destSite.NodeType == RemoveNodeType.PrivateChannel)
                {
                    siteSetting = TeamsSettingDao.LoadChannalSetting(new Guid(destSite.TeamId), new Guid(destSite.id));
                    if (siteSetting == null)
                    {
                        logger.Info("Teams channal Site level setting is null, will use group level setting");
                        siteSetting = TeamsSettingDao.LoadChannalSetting( new Guid(destSite.TeamId), Guid.Empty);
                        if (siteSetting == null)
                        {
                            throw new Exception("Cannot find Teams group channal level setting.");
                        }
                    }
                    mTeamsSettingDic.Add(destSite.url, siteSetting);
                    return siteSetting;
                }
                else
                {
                    siteSetting = TeamsSettingDao.LoadTeamsSetting(new Guid(destSite.parentId), new Guid(destSite.TeamId), new Guid(destSite.id));
                    if (siteSetting == null)
                    {
                        logger.Info("Teams Site level setting is null, will use group level setting");
                        siteSetting = TeamsSettingDao.LoadTeamsSetting(new Guid(destSite.parentId), new Guid(destSite.TeamId), Guid.Empty);
                        if (siteSetting == null)
                        {
                            throw new Exception("Cannot find Teams group level setting.");
                        }
                    }
                    mTeamsSettingDic.Add(destSite.url, siteSetting);
                    return siteSetting;
                }
            }
        }
        private string GetDestinationReportUrl(SourceBase source, DestinationBase destination, MoveSettingInfo moveSettingInfo, AppendItemMapping appendMapping, JobResult jobResult)
        {
            string desFileUrl = jobResult?.DestStub?.FullPath;
            if (string.IsNullOrEmpty(desFileUrl))
            {
                string desFileName = appendMapping.ContainsKeyAppendName(source.FileName) ? appendMapping.GetValueAppendName(source.FileName) : source.FileName;
                desFileUrl = CommonUtil.GeneralJobReportDesUrl(source.NodeType, destination.DestType, moveSettingInfo.IllegalCharMap, destination.DestinationContainerUrl, source.MoveParentPath, desFileName);
            }
            else
            {
                if (null != jobResult?.DestStub?.UIVersion)
                {
                    int uiVersion = jobResult.DestStub.UIVersion;
                    if (uiVersion > 0)
                    {
                        desFileUrl = desFileUrl + ":" + CommonUtil.ConvertUIVersionToVersionLabel(uiVersion);
                    }
                }
            }

            desFileUrl = GetFolderUrl(desFileUrl);

            return desFileUrl;
        }

        private static string GetFolderUrl(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
                return fileUrl;

            if (!Uri.TryCreate(fileUrl, UriKind.Absolute, out var uri))
                return fileUrl;

            var path = uri.AbsolutePath;

            var cleanPath = path.LastIndexOf(':') is > 0 and var colonIndex ? path[..colonIndex] : path;

            var folderPath = cleanPath[..cleanPath.LastIndexOf('/')];

            return $"{uri.Scheme}://{uri.Host}{folderPath}";
        }

        private void AddDetail(SourceBase source, string destUrl, JobDetailsStatus status, string comment)
        {
            ReportManager.SendJobDetail(new JMGlobalSearchActionJobDetails()
            {
                ObjectName = source?.FileName,
                FullPath = source?.SourceUrl,
                Action = "RM_JS_RDM_CreateRule_Options_MoveRecord",
                DestinationLocation = destUrl,
                Status = status,
                Comment = comment,
                Type = source == null ? "" : "RM_JS_Rule_CreateRule_FilterLevel_Document"
            });
        }
        private void AddDetail(SourceBase source, string destUrl, JobDetailsStatus status, string comment, JobResult jobResult)
        {
            if (!string.IsNullOrEmpty(comment) && comment.Contains("Cannot delete file"))
            {
                comment = "RM_SS_CannotDeleteFileCurrentLocation";
            }
            if (null != jobResult?.DestStub?.UIVersion)
            {
                int uiVersion = jobResult.DestStub.UIVersion;
                if (uiVersion > 0)
                {
                    string FullPath = source?.SourceUrl;
                    if (!string.IsNullOrEmpty(FullPath))
                    {
                        if (FullPath.EndsWith($":{CommonUtil.ConvertUIVersionToVersionLabel(uiVersion)}"))
                        {
                            ReportManager.SendJobDetail(new JMGlobalSearchActionJobDetails()
                            {
                                ObjectName = source?.FileName,
                                FullPath = source?.SourceUrl,
                                Action = "RM_JS_RDM_CreateRule_Options_MoveRecord",
                                DestinationLocation = destUrl,
                                Status = status,
                                Comment = comment,
                                Type = "RM_JS_Rule_ObjectLevel_DocumentVersion"
                            });
                        }
                        else
                        {
                            ReportManager.SendJobDetail(new JMGlobalSearchActionJobDetails()
                            {
                                ObjectName = source?.FileName,
                                FullPath = source?.SourceUrl,
                                Action = "RM_JS_RDM_CreateRule_Options_MoveRecord",
                                DestinationLocation = destUrl,
                                Status = status,
                                Comment = comment,
                                Type = "RM_JS_Rule_CreateRule_FilterLevel_Document"
                            });
                        }
                        return;
                    }
                }
            }
            ReportManager.SendJobDetail(new JMGlobalSearchActionJobDetails()
            {
                ObjectName = source?.FileName,
                FullPath = source?.SourceUrl,
                Action = "RM_JS_RDM_CreateRule_Options_MoveRecord",
                DestinationLocation = destUrl,
                Status = status,
                Comment = comment,
                Type = source == null ? "" : "RM_JS_Rule_CreateRule_FilterLevel_Document"
            });
        }

        public int GetSuccessCount()
        {
            return mSuccessCount;
        }

        public int GetFailedCount()
        {
            return mFailedCount;
        }
    }
}
