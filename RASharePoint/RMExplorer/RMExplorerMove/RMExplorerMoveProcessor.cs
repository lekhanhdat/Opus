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
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Common.Report;
using AvePoint.Records.Core.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using System.Collections.Concurrent;

namespace AvePoint.RA.SharePoint.RMExplorer
{
    public class RMExplorerMoveProcessor
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMExplorerMoveProcessor));

        private Dictionary<string, RMSharePointSetting> mSettingDic = new Dictionary<string, RMSharePointSetting>();
        private ConcurrentDictionary<string, RemoteSiteCollection> mDestinationDic = new ConcurrentDictionary<string, RemoteSiteCollection>();
        public IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();

        public ISharePointSettingDao SharePointSettingDao => PlatformWindsorManager.GetService<ISharePointSettingDao>();

        public Task RunNowAsync(string subJobId)
        {
            //SubJobDao = (IRMSubJobDao)PlatformWindsorManager.GetService(typeof(IRMSubJobDao));
            RMSubJob subJobWithContext = SubJobDao.GetSubJob(subJobId, true);
            RMExplorerMoveJobMessage msg = SerializerHelper.DeserializeFromXmlString<RMExplorerMoveJobMessage>(subJobWithContext.JobContext.Content);
            msg.JobID = subJobId;
            return this.MoveAsync(msg);
        }

        private async Task MoveAsync(RMExplorerMoveJobMessage message)
        {
            logger.Info("Begin moving");
            JobManagement jobManager = JobManagement.GetInstance(message);
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
                    MoveSourceManager sourceInfo = new MoveSourceManager(message, desInfo);
                    long totalCount = await sourceInfo.CalculateTotalCountAsync();
                    jobManager.ReportManager.IncreaseBase(totalCount);
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
                            jobManager.ReportManager.Increase();
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
                                    if (jobResult.Status == Contract.RMWeb.JobMonitor.JobDetailsStatus.Successful)
                                    {
                                        if (source.NodeType == (int)GCommon.Contract.Tree.Object.NodeLevel.FSFolder)
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
                                                        jobResult.Status = Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed;
                                                        jobManager.HasErrorNode = true;
                                                        logger.Error(string.Format("Error in delete node, reason : {0}.", delException.ToString()));
                                                    }
                                                    try
                                                    {
                                                        dbUtil.UpdateMovedRecords(jobResult, source, desInfo, message.Operator,null,Guid.Empty);
                                                    }
                                                    catch (Exception exc)
                                                    {
                                                        logger.Error(string.Format("Error in update moved record, reason : {0}", exc.ToString()));
                                                    }
                                                    string desUrl = GetDestinationReportUrl(source, desInfo.Destination, moveSetting, appendMapping, jobResult);
                                                    //jobManager.ReportService.Commit(new MoveReportEntity(source.FileName, CommonUtil.ConvertNodeTypeToReportType(source.NodeType), source.SourceUrl, desUrl, jobResult.Status, jobResult.ErrorMessage));
                                                    jobManager.ReportManager.SendJobDetail(new Contract.RMWeb.JobMonitor.JMExplorerMoveJobDetails() { ObjectName = source.FileName, FullPath = source.SourceUrl, DestinationFullPath = desUrl, Status = jobResult.Status, Comment = jobResult.ErrorMessage });
                                                    logger.Debug(string.Format("finished move : {0}.", source.Id));
                                                }
                                                catch (Exception ex)
                                                {
                                                    logger.Warn(string.Format("Error in delete object, reason : {0}", ex.ToString()));
                                                }
                                            });
                                            logger.Debug(string.Format("finished add : {0} to deletion stack.", source.Id));
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
                                                    var siteSetting = GetDestinationSPSetting(destSite);
                                                    bool useExisting = siteSetting.IsUsingExistColumnName;
                                                    string columnName = useExisting ? siteSetting.ExistColumnName : siteSetting.ColumnName;
                                                    sourceTermID = source.GetSourceTermId(columnName);
                                                    logger.Info("Destination term is:{0}", sourceTermID);
                                                }
                                            }
                                            source.Delete();
                                        }
                                    }
                                    else
                                    {
                                        logger.Debug(string.Format("Skip deleting the source, as the status is : {0}.", jobResult.Status.ToString()));
                                        if (jobResult.Status == Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed)
                                        {
                                            jobManager.HasErrorNode = true;
                                        }
                                    }
                                }
                                catch (Exception deletionException)
                                {
                                    jobResult.ErrorMessage = deletionException.Message.ToString();
                                    jobResult.Status = Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed;
                                    jobManager.HasErrorNode = true;
                                    logger.Error("Error in deletion, reason : " + deletionException.ToString());
                                }
                            }
                            #region Exception Types
                            catch (ConetentSkipException contentExp)
                            {
                                jobResult.Status = Contract.RMWeb.JobMonitor.JobDetailsStatus.Skipped;
                                jobResult.ErrorMessage = contentExp.Message.ToString();
                                logger.Warn(string.Format("Content Skip: FileName: {0}, reason: {1}.", source.FileName, contentExp.ToString()));
                            }
                            //File length exceed 128 catch exception
                            catch (PathTooLongException e)
                            {
                                jobResult.Status = Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed;
                                jobResult.ErrorMessage = e.Message.ToString();
                                jobManager.HasErrorNode = true;
                                logger.Warn(string.Format("File name or list URL is too long. Reason : {0}.", e.ToString()));
                            }
                            catch (SkipException skipEx)
                            {
                                jobResult.Status = Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed;
                                jobResult.ErrorMessage = skipEx.Message.ToString();
                                jobManager.HasErrorNode = true;
                                logger.Warn("Content Type Or Column conflict, Skip Current file : {0}, Message : {1}", source.FileName, skipEx.ToString());
                            }
                            #endregion
                            catch (Exception exp)
                            {
                                jobResult.ErrorMessage = exp.Message.ToString();
                                jobResult.Status = Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed;
                                jobManager.HasErrorNode = true;
                                logger.Error("Error in move restore, source url : {0}, destination url : {1}, reason : {2}.", source.SourceUrl, desInfo.Destination.DestinationContainerUrl, exp.ToString());
                            }

                        }
                        catch (Exception ex)
                        {
                            jobResult.ErrorMessage = ex.Message.ToString();
                            jobResult.Status = Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed;
                            jobManager.HasErrorNode = true;
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
                            //jobResult.Status = Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed;
                            jobManager.HasErrorNode = true;
                            failedUpdateColumn = true;
                            logger.Error("An error occurred while updating bcs column, error:{0}", e.ToString());
                        }
                        try
                        {
                            var destSite = GetDestinationSite(desInfo.DestRootPath);
                            Guid destTermId = Guid.Empty;
                            if (destSite.NodeType != RemoveNodeType.SkyDrivePro)
                            {
                                logger.Info("Try to get term id in destination.");
                                //get destination term id
                                var siteSetting = GetDestinationSPSetting(destSite);
                                destTermId = desInfo.Destination.GetDestinationTermId(siteSetting.IsUsingExistColumnName ? siteSetting.ExistColumnName : siteSetting.ColumnName);
                            }
                            dbUtil.UpdateMovedRecords(jobResult, source, desInfo, message.Operator, destSite, destTermId, failedUpdateColumn);
                        }
                        catch (Exception exc)
                        {
                            logger.Error(string.Format("Error in update moved record, reason : {0}", exc.ToString()));
                        }
                        string desFileUrl = GetDestinationReportUrl(source, desInfo.Destination, moveSetting, appendMapping, jobResult);
                        //jobManager.ReportService.Commit(new MoveReportEntity(source.FileName, CommonUtil.ConvertNodeTypeToReportType(source.NodeType), source.SourceUrl, desFileUrl, jobResult.Status, jobResult.ErrorMessage));
                        if (failedUpdateColumn)
                        {
                            jobResult.Status = JobDetailsStatus.Failed;
                        }
                        jobManager.ReportManager.SendJobDetail(new Contract.RMWeb.JobMonitor.JMExplorerMoveJobDetails() { ObjectName = source.FileName, ItemType = CommonUtil.ConvertNodeTypeToReportType(source.NodeType), FullPath = source.SourceUrl, DestinationFullPath = desFileUrl, Status = jobResult.Status, Comment = jobResult.ErrorMessage });
                        logger.Debug(string.Format("finished move object: {0}, status is : {1}.", source.Id, jobResult.Status.ToString()));
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
                }
                logger.Info("Move Job finished");
            }
            catch (Exception ex)
            {
                jobManager.HasErrorNode = true;
                logger.Warn(string.Format("Error in Job, reason : {0}.", ex.ToString()));
                jobManager.ReportManager.SendJobDetail(new Contract.RMWeb.JobMonitor.JMExplorerMoveJobDetails() { ObjectName = "", ItemType = "", FullPath = "", DestinationFullPath = "", Status = JobDetailsStatus.Failed, Comment = ex.Message });
            }
            finally
            {
                logger.Info(string.Format("Update Job status to server, has error node : {0}.", jobManager.HasErrorNode.ToString()));
                jobManager.Finish();
            }
        }

        private void UpdateBCSColumn(SourceBase source, RMExplorerMoveDBUtil dbUtil, MoveDestinationManager desInfo, RemoveNodeType sourceType, Guid sourceTermId)
        {
            if (sourceType == RemoveNodeType.SkyDrivePro)
            {
                var sourceRcord = dbUtil.GetRecords(source.Id).First();
                sourceTermId = sourceRcord.TermId;
            }
            var destSite = GetDestinationSite(desInfo.DestRootPath);
            if (sourceTermId != Guid.Empty && source.NodeType != (int)GCommon.Contract.Tree.Object.NodeLevel.ItemVersion && destSite.NodeType != GCommon.Contract.Server.ControlPanel.Office365.RemoveNodeType.SkyDrivePro)
            {
                logger.Info("Start to update bsc column in destination.");
                var siteSetting = GetDestinationSPSetting(destSite);
                bool useExisting = siteSetting.IsUsingExistColumnName;
                string columnName = useExisting ? siteSetting.ExistColumnName : siteSetting.ColumnName;
                string errorMessage = desInfo.Destination.UpdateBCSColumn(useExisting, columnName, sourceTermId);
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    logger.Info("Update bcs column with exception. Message:{0}", errorMessage);
                    throw new Exception(errorMessage);
                }
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
            return desFileUrl;
        }
    }
}
