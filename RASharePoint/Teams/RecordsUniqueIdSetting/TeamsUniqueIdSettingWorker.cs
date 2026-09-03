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
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.SharePoint.RecordsUniqueIdSetting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.SharePoint.EnforceRetention;
using AvePoint.RA.SharePoint.Teams.ColumnSetting;
using static AvePoint.GCommon.Utility.I18N.ContextValues.Configuration;

namespace AvePoint.RA.SharePoint.Teams.RecordsUniqueIdSetting
{
    public class TeamsUniqueIdSettingWorker
    {
        protected static readonly RALogger logger = RALogger.GetInstance(typeof(TeamsUniqueIdSettingWorker));

        private List<SPTreeNodeDto> mTreeNodeList;
        private string mJobId;
        protected RMUniqueIdSetting curUniqueIdSetting;

        #region Interfaces
        private ISPSettingTreeService mSPTreeService;
        protected ISPSettingTreeService RMSPTreeService
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

        private ITeamsSettingTreeService mRMTeamsTreeService;

        protected ITeamsSettingTreeService RMTeamsTreeService
        {
            get
            {
                if (mRMTeamsTreeService == null)
                {
                    mRMTeamsTreeService = (ITeamsSettingTreeService)PlatformWindsorManager.GetService(typeof(ITeamsSettingTreeService));
                }
                return mRMTeamsTreeService;
            }
        }



        private IUniqueIdSettingDao mUniqueIdSettingDao;
        protected IUniqueIdSettingDao UniqueIdSettingDao
        {
            get
            {
                if (mUniqueIdSettingDao == null)
                {
                    mUniqueIdSettingDao = new UniqueIdSettingDao();
                }
                return mUniqueIdSettingDao;
            }
        }
        private ITeamsSettingDao mTeamsSettingDao;
        protected ITeamsSettingDao TeamsSettingDao
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

        //IOneDriveSettingDao
        private IRMSubJobDao SubJobDao { set; get; }
        #endregion

        protected IRMReportManager reportManager;

        public TeamsUniqueIdSettingWorker(string subJobId, string message)
        {
            mJobId = subJobId;
            ReportMangerFactory.Instance.Init(mJobId, Contract.JobMonitor.JobType.TeamsUniqueIDSettingIncrementalSchedule);
            reportManager = ReportMangerFactory.Instance.ReportManager;
            reportManager.Increase(1);
            reportManager.StartUpdateJobProgress();
            SubJobDao = (IRMSubJobDao)PlatformWindsorManager.GetService(typeof(IRMSubJobDao));
            curUniqueIdSetting = UniqueIdSettingDao.LoadingUniqueIdSetting(UniqueIdType.Teams);
            reportManager.Increase(2);
        }

        public async Task ConfigUniqueIDSettingAsync()
        {
            var haveErrorNode = false;
            var hasJobStop = false;
            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    InitTreeNode();
                    if (curUniqueIdSetting == null)
                    {
                        reportManager.SetJobFinished(Contract.RMWeb.JobMonitor.JobStatus.Failed, "RM_BCM_UniqueId_NotConfigSetting");
                        return;
                    }
                    foreach (var mTreeNode in mTreeNodeList)
                    {
                        var groupNode = mTreeNode.Parent;
                        if (!CheckGroupSetting(mTreeNode.Parent))
                        {
                            continue;
                        }
                        if (mTreeNode.Level == NodeLevel.Office365GroupEntire)
                        {
                            List<SPTreeNodeDto> virtualSiteCollectionNode = RMTeamsTreeService.BrowseTeamsTreeNode(mTreeNode);
                            if (virtualSiteCollectionNode == null || virtualSiteCollectionNode.Count == 0) continue;
                            List<SPTreeNodeDto> siteCollectionNodes = RMTeamsTreeService.BrowseTeamsTreeNode(virtualSiteCollectionNode[0]);
                            foreach (SPTreeNodeDto site in siteCollectionNodes)
                            {
                                if (site.Url == null)
                                {
                                    site.Url = site.FullPath;
                                }
                                UniqueIdSettingInrementalProcessor processor = new UniqueIdSettingInrementalProcessor(site, curUniqueIdSetting);
                                var temp = await processor.RunAsync();
                                if (temp)
                                {
                                    haveErrorNode = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (JobStopException ex)
            {
                hasJobStop = true;
                logger.Info("Unique ID Settings Incremental Job is stopped.");
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while set unique id:{0}", ex.ToString());
            }
            finally
            {
                if (hasJobStop)
                {
                    reportManager.SetJobFinished(Contract.RMWeb.JobMonitor.JobStatus.Stopped, "");
                }
                if (haveErrorNode)
                {
                    reportManager.SetJobFinished(Contract.RMWeb.JobMonitor.JobStatus.FinishWithException, "RM_SS_CommonErrorMessage");
                }
                else
                {
                    reportManager.SetJobFinished(Contract.RMWeb.JobMonitor.JobStatus.Finished, "");
                }
            }
        }

        private void InitTreeNode()
        {
            List<SPTreeNodeDto> treeList = new List<SPTreeNodeDto>();
            if (AvePoint.RA.Common.JobService.JobServiceUtility.IsSubJob(mJobId))
            {
                //从子job的Context中获取当前需要处理的节点.   更新进度和状态用JobInfoUpdater
                RMSubJob subJobWithContext = SubJobDao.GetSubJob(mJobId, true);
                List<RMSPTreeNode> tempList = SerializerHelper.DeserializeByDataContractSerializer<List<RMSPTreeNode>>(subJobWithContext.JobContext.Settings);
                tempList.ForEach(node => treeList.Add(RMDtoConverter.ConvertRMTree2SPTree(node)));
            }
            if (!treeList.IsNullOrEmpty())
            {
                mTreeNodeList = treeList;
            }
            else
            {
                throw new Exception("no tree node was found.");
            }
        }

        private bool CheckGroupSetting(SPTreeNodeDto groupNode)
        {
            var groupId = new Guid(groupNode.SPObjectId);
            var tesmsGroupSetting = TeamsSettingDao.LoadTeamsSetting(groupId, Guid.Empty, Guid.Empty);
            if (tesmsGroupSetting == null || !(tesmsGroupSetting.IsShowUniqueId == null || (bool)tesmsGroupSetting.IsShowUniqueId))
            {
                logger.Info($"This teams container has not been set global setting or not enable show unqiueId, Id:{groupId}, showUniqueId:{tesmsGroupSetting?.IsShowUniqueId}");
                return false;
            }
            return true;
        }
    }
}
