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

using AvePoint.RA.Common.Threads;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RMExplorer
{
    //最底层：PCContainer
    //外层1：IEnumable，遍历PCContainer 去获取discover出的节点。（DiscoverCache 对象）
    //外层2：MoveSourceManager 对象，里面有DiscoverCache 对象，也负责起线程进行discover。
    public class MoveSourceManager : IDisposable
    {
        private RALogger logger = RALogger.GetInstance(typeof(MoveSourceManager));
        private RMExplorerMoveJobMessage msg = null;
        private DiscoverCache discoverCache = null;
        private MoveDestinationManager destInfo = null;
        private string logonGroupId= string.Empty;
        private string logonUserEmail = string.Empty;
        private bool mIsGlobalSearch = false;
        private RMAccountType logonUserAccountType = RMAccountType.None;
        public int FailedCount = 0;
        public int SuccessCount = 0;
        public MoveSourceManager(RMExplorerMoveJobMessage message, MoveDestinationManager mDestInfo, bool isGlobalSearch = false)
        {
            logonGroupId = TenantLocalValue.LogonGroupId;
            logonUserEmail = TenantLocalValue.LogonUserEmail;
            logonUserAccountType = TenantLocalValue.AccountType;
            mIsGlobalSearch = isGlobalSearch;
            msg = message;
            destInfo = mDestInfo;
            discoverCache = new RMExplorer.DiscoverCache();
            StartDiscoverThread();
        }

        public int UpdateProgressNodeLevel { get; private set; }

        public DiscoverCache DiscoverCache
        {
            get { return discoverCache; }
            private set { discoverCache = value; }
        }

        public Task<long> CalculateTotalCountAsync()
        {
            int totalSelectedObject = msg.SourceRecords.Count;
            if(totalSelectedObject>5)
            {
                UpdateProgressNodeLevel = 1;
            }
            else if(totalSelectedObject > 2)
            {
                UpdateProgressNodeLevel = 2;
            }
            else
            {
                UpdateProgressNodeLevel = 3;
            }
            return CalculateAsync(UpdateProgressNodeLevel);
        }

        private void StartDiscoverThread()
        {
            discoverCache.PCContainer.StartProduce();
            AveTenantThread t = new AveTenantThread(new ThreadStart(Discover));
            t.IsBackground = true;
            t.Start();
        }

        private async Task<long> CalculateAsync(int level)
        {
            long total = 0;
            logger.Info("Calculate source info.");
            PCContainer<SourceBase> container = new PCContainer<SourceBase>(long.MaxValue);
            foreach (var record in msg.SourceRecords)
            {
                try
                {
                    if (CheckIfNeedSkip(record))
                    {
                        continue;
                    }
                    switch (record.SourceFlag)
                    {
                        case RecordFlag.SP:
                        case RecordFlag.OneDrive:
                        case RecordFlag.Teams:
                        case RecordFlag.Groups:
                            {
                                SPDiscover discoverWorker = new SPDiscover(record, level);
                                await discoverWorker.StartDiscoverAsync();
                                total += discoverWorker.TotalCount;
                                break;
                            }
                        //case RecordFlag.FS:
                        //    {
                        //        FileSystemDiscover discoverWorker = new FileSystemDiscover(record, level);
                        //        discoverWorker.StartDiscover();
                        //        total += discoverWorker.TotalCount;
                        //        break;
                        //    }
                        case RecordFlag.None:
                        default:
                            {
                                logger.Error("Current process cannot recognize the source type : " + record.SourceFlag.ToString());
                                break;
                            }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(string.Format("Error in calculate source info, reason : {0}.", ex.ToString()));
                }
            }
            logger.Info(string.Format("Calculate source info finished, total count is : {0}.", total.ToString()));
            return total;
        }

        private void Discover()
        {
            //Init sources
            TenantLocalValue.LogonGroupId = logonGroupId;
            TenantLocalValue.LogonUserEmail = logonUserEmail;
            TenantLocalValue.AccountType = logonUserAccountType;
            logger.Info("Discover source info.");
            foreach (SourceRecord record in msg.SourceRecords)
            {
                try
                {
                    if (CheckIfNeedSkip(record))
                    {
                        var jobManager = JobManagement.GetInstance(msg);
                        logger.Warn("Current object is included in the destination");
                        if (mIsGlobalSearch)
                        {
                            jobManager.ReportManager.SendJobDetail(new Contract.RMWeb.JobMonitor.JMGlobalSearchActionJobDetails()
                            {
                                ObjectName = record.LeafName,
                                Type = "RM_JS_Rule_CreateRule_FilterLevel_Document",
                                Action = "RM_JS_JM_DataOperation_SharePointMoveAction",
                                FullPath = record.FullPath,
                                DestinationLocation = destInfo.Destination.DestinationContainerUrl,
                                Status = Contract.RMWeb.JobMonitor.JobDetailsStatus.Skipped,
                                Comment = I18NString.ItemIncludedInDestination,
                            });
                            SuccessCount++;
                        }
                        else
                        {
                            jobManager.ReportManager.SendJobDetail(new Contract.RMWeb.JobMonitor.JMExplorerMoveJobDetails()
                            {
                                ObjectName = record.LeafName,
                                ItemType = CommonUtil.ConvertNodeTypeToReportType(record.NodeType),
                                FullPath = record.FullPath,
                                DestinationFullPath = destInfo.Destination.DestinationContainerUrl,
                                Status = Contract.RMWeb.JobMonitor.JobDetailsStatus.Skipped,
                                Comment = I18NString.ItemIncludedInDestination,
                            });
                        }
                        continue;
                    }
                    switch (record.SourceFlag)
                    {
                        case RecordFlag.SP:
                        case RecordFlag.OneDrive:
                        case RecordFlag.Teams:
                        case RecordFlag.Groups:
                            {
                                IMoveDiscover discoverWorker = new SPDiscover(record, discoverCache.PCContainer);
                                discoverWorker.StartDiscoverAsync().Wait();
                                break;
                            }
                        //case RecordFlag.FS:
                        //    {
                        //        if (!sourceUrlCache.Exists(s => record.FullPath.StartsWith(s, StringComparison.OrdinalIgnoreCase)))
                        //        {
                        //            IMoveDiscover discoverWorker = new FileSystemDiscover(record, discoverCache.PCContainer);
                        //            discoverWorker.StartDiscover();
                        //            sourceUrlCache.Add(record.FullPath);
                        //        }
                        //        else
                        //        {
                        //            logger.Info(string.Format("current path : {0} is included in another node, no need to discover.", record.FullPath));
                        //        }
                        //        break;
                        //    }
                        case RecordFlag.None:
                        default:
                            {
                                logger.Error("Current process cannot recognize the source type : " + record.SourceFlag.ToString());
                                break;
                            }
                    }
                }
                catch (Exception ex)
                {
                    var jobManager = JobManagement.GetInstance(msg);
                    jobManager.HasErrorNode = true;
                    if (mIsGlobalSearch)
                    {
                        jobManager.ReportManager.SendJobDetail(new Contract.RMWeb.JobMonitor.JMGlobalSearchActionJobDetails()
                        {
                            ObjectName = record.LeafName,
                            Type = "RM_JS_Rule_CreateRule_FilterLevel_Document",
                            Action = "RM_JS_JM_DataOperation_SharePointMoveAction",
                            FullPath = record.FullPath,
                            DestinationLocation = destInfo.Destination.DestinationContainerUrl,
                            Status = Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed,
                            Comment = (ex.InnerException ?? ex).Message,
                        });
                        FailedCount++;
                    }
                    else
                    {
                        jobManager.ReportManager.SendJobDetail(new Contract.RMWeb.JobMonitor.JMExplorerMoveJobDetails()
                        {
                            ObjectName = record.LeafName,
                            ItemType = CommonUtil.ConvertNodeTypeToReportType(record.NodeType),
                            FullPath = record.FullPath,
                            DestinationFullPath = destInfo.Destination.DestinationContainerUrl,
                            Status = Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed,
                            Comment = (ex.InnerException ?? ex).Message,
                        });
                    }
                    logger.Error(string.Format("Error in generate source info, reason : {0}.", ex.ToString()));
                }
            }
            logger.Info("Discover source info finished.");
            //Discover 结束，必须调用EndProduce，这样Consume 才能返回null，让foreach 结束 
            discoverCache.PCContainer.EndProduce();
        }

        private bool CheckIfNeedSkip(SourceRecord record)
        {
            bool needSkip = false;

            if (record.SourceFlag == destInfo.Destination.DestType || 
                ((record.SourceFlag == RecordFlag.OneDrive || record.SourceFlag == RecordFlag.Teams) && destInfo.Destination.DestType == RecordFlag.SP))
            {
                char slash = '/';
                switch (record.SourceFlag)
                {
                    case RecordFlag.FS:
                        slash = '\\';
                        break;
                    case RecordFlag.SP:
                        break;
                    default:
                        break;
                }
                int index = record.FullPath.LastIndexOf(slash);
                string parentPath = record.FullPath.Substring(0, index);
                if (parentPath.Equals(destInfo.Destination.DestinationContainerUrl))
                {
                    logger.Info(string.Format("Need to skip select node, source is : {0}, destination is : {1}.", record.FullPath, destInfo.Destination.DestinationContainerUrl));
                    needSkip = true;
                }
                //REC-5120 对于folder，会出问题的case是，将folder向父folder进行move（也就是原地move）；向子folder进行move。所以需要block这两种情况。
                if (record.SourceFlag == RecordFlag.FS &&
                    record.NodeType == (int)GCommon.Contract.Tree.Object.NodeLevel.FSFolder &&
                    destInfo.Destination.DestinationContainerUrl.Contains(record.FullPath))
                {
                    logger.Info(string.Format("Cannot move folder to it's children folder, source is : {0}, destination is : {1}.", record.FullPath, destInfo.Destination.DestinationContainerUrl));
                    needSkip = true;
                }
            }
            return needSkip;
        }


        //public List<SourceBase> Sources
        //{
        //    get
        //    {
        //        return sources;
        //    }
        //    private set
        //    {
        //        sources = value;
        //    }
        //}

        //private void Init(MoveRecordsJobMessage msg)
        //{
        //    //Init sources
        //    logger.Info("Init source info");
        //    foreach (SourceRecord record in msg.SourceRecords)
        //    {
        //        try
        //        {
        //            switch (record.SourceFlag)
        //            {
        //                case RecordFlag.SP:
        //                    {
        //                        IMoveDiscover discoverWorker = new SPDiscover(record, Sources);
        //                        discoverWorker.StartDiscover();
        //                        break;
        //                    }
        //                case RecordFlag.FS:
        //                    {
        //                        if (!sourceUrlCache.Exists(s => record.FullPath.StartsWith(s, StringComparison.OrdinalIgnoreCase)))
        //                        {
        //                            IMoveDiscover discoverWorker = new FileSystemDiscover(record, Sources);
        //                            discoverWorker.StartDiscover();
        //                            sourceUrlCache.Add(record.FullPath);
        //                        }
        //                        else
        //                        {
        //                            logger.Info(string.Format("current path : {0} is included in another node, no need to discover.", record.FullPath));
        //                        }
        //                        break;
        //                    }
        //                case RecordFlag.None:
        //                default:
        //                    {
        //                        logger.Error("Current process cannot recognize the source type : " + record.SourceFlag.ToString());
        //                        break;
        //                    }
        //            }
        //        }
        //        catch(Exception ex)
        //        {
        //            var jobManager = JobManagement.GetInstance(msg);
        //            jobManager.HasErrorNode = true;
        //            jobManager.ReportService.Commit(new MoveReportEntity(record.LeafName, CommonUtil.ConvertNodeTypeToReportType(record.NodeType), record.FullPath, "", JobReportDetailStatus.Failed, string.Format("Cannot find the source node, reason : {0}.", ex.Message)));
        //            logger.Error(string.Format("Error in generate source info, reason : {0}.", ex.ToString()));
        //        }
        //    }
        //}
        public void Dispose()
        {
            discoverCache.Dispose();
        }
    }
}
