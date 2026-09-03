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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.SharePoint.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.ExplorerSyncNew.Report
{
    public class RMSPDashboardCalculator
    {

        private static readonly RALogger logger = RALogger.GetInstance(typeof(RMSPDashboardCalculator));
        private string mJobId;
        private bool hasErrorNode = false;
        private JobContext jobContext = null;
        private int TotalCountIncrement = 0;
        private int TotalCount = 0;
        #region castle properties
        private RA.DB.Explorer.Dao.IExplorerDao _explorerDao;
        public RA.DB.Explorer.Dao.IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new RA.DB.Explorer.Dao.CosmosImp.ExplorerDao();
                }
                return _explorerDao;
            }
        }
        
        private IRMSiteCollectionSizeDao mRMSiteCollectionSizeDao;
        protected IRMSiteCollectionSizeDao RMSiteCollectionSizeDao
        {
            get
            {
                if (mRMSiteCollectionSizeDao == null)
                {
                    mRMSiteCollectionSizeDao = (IRMSiteCollectionSizeDao)PlatformWindsorManager.GetService(typeof(IRMSiteCollectionSizeDao));
                }
                return mRMSiteCollectionSizeDao;
            }
        }
        private IRMNodeFlagDao mRMNodeFlagDao;
        protected IRMNodeFlagDao NodeFlagDao
        {
            get
            {
                if(mRMNodeFlagDao == null)
                {
                    mRMNodeFlagDao = (IRMNodeFlagDao)PlatformWindsorManager.GetService(typeof(IRMNodeFlagDao));
                }
                return mRMNodeFlagDao;
            }
        }
        private IBoardTotalDao mBoardTotalDao;
        protected IBoardTotalDao BoardTotalDao
        {
            get
            {
                if (mBoardTotalDao == null)
                {
                    mBoardTotalDao = (IBoardTotalDao)PlatformWindsorManager.GetService(typeof(IBoardTotalDao));
                }
                return mBoardTotalDao;
            }

        }
        #endregion
        public RMSPDashboardCalculator(string jobId)
        {
            this.mJobId = jobId;
            jobContext = JobContext.GetInstance(jobId, Contract.JobMonitor.JobType.DataSynchronisation);
            jobContext.ReportManager.StartUpdateJobProgress();
        }

        public void Work()
        {
            try
            {
                logger.Info("Start...");
                List<RMSiteCollectionSize> existingSize = GetAllExistingSizeData();
                foreach(RMSiteCollectionSize size in existingSize)
                {
                    InnerProcessSite(size);
                }
                //有的客户的NodeFlag中存的ScopeId为Guid.Empty， 不再使用这段逻辑
                //List<RMNodeFlag> nodeFlags = GetUncollectedSites(existingSize); 
                //if (nodeFlags.Count > 0)
                //{
                //    jobContext.ReportManager.IncreaseBase(nodeFlags.Count);
                //    foreach(RMNodeFlag flag in nodeFlags)
                //    {
                //        InnerProcessSite(flag);
                //    }
                //}
                if (!this.hasErrorNode)
                {
                    int sourceFlg = (int)SourceFlag.SharePoint;
                    BoardTotal total = BoardTotalDao.Find(a => a.SourceFlag == sourceFlg);
                    if(total != null)
                    {
                        logger.Info("Total in DB: {0}, total in job {1}", total.CreatedTotal, TotalCount);
                        total.CreatedTotal = TotalCount;
                        BoardTotalDao.Update(total);
                        jobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                        {
                            ObjectName = "Total count",
                            FullPath = "Dashboard total",
                            Status = JobDetailsStatus.Successful,
                            Comment = "Update total count.",
                        });
                    }
                }
                else
                {
                    logger.Info("Some of the sites failed, skip updating total count");
                }
            }
            catch (JobStopException)
            {
                jobContext.JobHasStopped = true;
                logger.Warn("the job has stopped.");
            }
            catch (Exception e)
            {
                jobContext.HasErrorNode = true;
                logger.Error($"error occurred while calculating items in sites, ERROR:{e.ToString()}");
                jobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                {
                    ObjectName = string.Empty,
                    FullPath = string.Empty,
                    Status = JobDetailsStatus.Failed,
                    Comment = e.Message,
                });
            }
            finally
            {
                jobContext.HasErrorNode = this.hasErrorNode;
                jobContext.Finish();
            }
        }

        private void UpdateBoardTotal()
        {
            try
            {
                if (TotalCount > 0 && !this.hasErrorNode)
                {
                    int sourceFlg = (int)SourceFlag.SharePoint;
                    BoardTotal total = BoardTotalDao.Find(a => a.SourceFlag == sourceFlg);
                    if (total != null)
                    {
                        logger.Info("Start to update board total, total in DB {0}, total in job {1}", total.CreatedTotal, TotalCount);
                        total.CreatedTotal = TotalCount;
                        BoardTotalDao.Update(total);
                    }
                }
                else
                {
                    logger.Warn("Job status is not finished, failed to update board total.");
                }
            }
            catch (Exception e)
            {
                logger.Warn("Failed to update total." + e.ToString());
            }
        }
        [Obsolete]
        private List<RMNodeFlag> GetUncollectedSites(List<RMSiteCollectionSize> existingSize)
        {
            List<RMNodeFlag> list = NodeFlagDao.GetExistScopeInfo(Contract.Object.NodeFlagType.ExplorerSync);
            List<RMNodeFlag> notCollected = list.Where(a => !existingSize.Any(e => e.ScopeId == a.NodeId)).ToList();
            logger.Info("Get site count {0}, never collected size ", notCollected.Count);
            return notCollected;
        }

        private List<RMSiteCollectionSize> GetAllExistingSizeData()
        {
            List<RMSiteCollectionSize> allSize = RMSiteCollectionSizeDao.FindAll();
            logger.Info("All existing size data count {0}", allSize.Count);
            return allSize;
        }

        private void InnerProcessSite(RMSiteCollectionSize currentSize)
        {
            using (var performance = new PerformanceScope("RMSPDashboardCalculator.Work.Existing"))
            {
                try
                {
                    logger.Info("Start to process site collection:{0}, {1}", currentSize.SiteUrl, currentSize.ScopeId);
                    string sql = this.GetCountQuerySql(currentSize.ScopeId);
                    int itemCount = ExplorerDao.QueryCount(sql);
                    if (itemCount > 0)
                    {
                        TotalCount += itemCount;
                        logger.Info("Add {0} to temp total", itemCount);
                        SaveOrUpdateSCCount(currentSize, itemCount);
                    }else
                    {
                        logger.Info("No explorer record found in site {0}", currentSize.SiteUrl);
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e.Message, e);
                    hasErrorNode = true;
                    jobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                    {
                        ObjectName = currentSize.Title,
                        FullPath = currentSize.SiteUrl,
                        Status = JobDetailsStatus.Failed,
                        Comment = e.Message,
                    });
                }
            }
        }

        private void InnerProcessSite(RMNodeFlag nodeFlag)
        { 
            using (var performance = new PerformanceScope("RMSPDashboardCalculator.Work"))
            {
                try
                {
                    logger.Info("Start to process site:{0}", nodeFlag.FullPath);
                    string sql = this.GetCountQuerySql(nodeFlag.NodeId);
                    int itemCount = ExplorerDao.QueryCount(sql);
                    if (itemCount > 0)
                    {
                        SaveOrUpdateSCCount(nodeFlag, itemCount);
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e.Message, e);
                    hasErrorNode = true;
                    jobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                    {
                        ObjectName = nodeFlag.Title,
                        FullPath = nodeFlag.FullPath,
                        Status = JobDetailsStatus.Failed,
                        Comment = e.Message,
                    });
                }
            }
        }

        private string GetCountQuerySql(Guid siteUniqueId)
        {
            //Document， Active，in Specified Site
            string siteId = siteUniqueId.ToString().ToLower();
            string sql = string.Format("SELECT VALUE COUNT(1) FROM c where c.scopeId = \"{0}\" and c.nodeType = 500 and c.recordStatus = 1", siteId);
            return sql;
        }

        private void SaveOrUpdateSCCount(RMNodeFlag flag, int count)
        {
            logger.Info("Start to update site collection {0}, count {1}", flag.FullPath, count);
            RMSiteCollectionSize currentSize = RMSiteCollectionSizeDao.Find(a => a.ScopeId == flag.NodeId && a.SiteUrl.Equals(flag.FullPath, StringComparison.OrdinalIgnoreCase));
            if(currentSize != null)
            {
                logger.Info("Current site {0} count {1}", currentSize.SiteUrl, currentSize.Size);
                if(count > currentSize.Size)
                { 
                    this.TotalCountIncrement += count - (int)currentSize.Size;
                    currentSize.Size = count;
                    RMSiteCollectionSizeDao.UpdateSiteCollectionSizes(currentSize);
                    jobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                    {
                        ObjectName = flag.Title,
                        FullPath = flag.FullPath,
                        Status = JobDetailsStatus.Successful,
                        Comment = string.Format("Update size {0}",count) ,
                    });
                }
                else
                {
                    jobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                    {
                        ObjectName = flag.Title,
                        FullPath = flag.FullPath,
                        Status = JobDetailsStatus.Skipped,
                        Comment = string.Format("New size {0} less than current one {1}", count, currentSize.Size),
                    });
                }
            }
            else
            {
                logger.Info("No record for this site, create one.");

                this.TotalCountIncrement += count;
                RMSiteCollectionSize newSize = new RMSiteCollectionSize();
                newSize.ScopeId = flag.NodeId;
                newSize.SiteUrl = flag.FullPath;
                newSize.Title = flag.Title;
                newSize.Size = count;
                RMSiteCollectionSizeDao.Create(newSize);
                jobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                {
                    ObjectName = flag.Title,
                    FullPath = flag.FullPath,
                    Status = JobDetailsStatus.Successful,
                    Comment = string.Format("New size {0}", count),
                });
            }
        }

        private void SaveOrUpdateSCCount(RMSiteCollectionSize currentSize, int count)
        {
            logger.Info("Start to update site collection {0}, current count {1}, new count {2}", currentSize.SiteUrl, currentSize.Size, count);  
            //this.TotalCountIncrement += count - (int)currentSize.Size;
            currentSize.Size = count;
            RMSiteCollectionSizeDao.UpdateSiteCollectionSizes(currentSize);
            if (count > currentSize.Size || count == currentSize.Size)
            {
                jobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                {
                    ObjectName = currentSize.Title,
                    FullPath = currentSize.SiteUrl,
                    Status = JobDetailsStatus.Successful,
                    Comment = string.Format("Update size: {0}", count),
                });
            }
            else
            {
                jobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                {
                    ObjectName = currentSize.Title,
                    FullPath = currentSize.SiteUrl,
                    Status = JobDetailsStatus.Skipped,
                    Comment = string.Format("New size {0} is equals to current one {1}", count, currentSize.Size),
                });
            }
            
        }
    }
}
