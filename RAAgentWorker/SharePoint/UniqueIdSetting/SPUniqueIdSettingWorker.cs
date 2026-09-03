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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.Common;

using AvePoint.RA.Contract.Object;

using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.Common.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.FileSystem;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Global.JobMessage;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Common.Hybrid;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.SharePoint.UniqueIdSetting;
using AvePoint.RA.SharePoint.UniqueIdSetting.Base;
using AvePoint.RA.Contract.Global.RMWeb.JobMonitor;
using System.Net;
using AgentUtil = AvePoint.RA.SharePoint.Common.Util;
using Microsoft.IdentityModel.Tokens;

namespace AvePoint.RA.SharePoint.RecordsUniqueIdSetting
{
    public class SPUniqueIdSettingWorker: IScheduleJobWorker
    {
        private AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public string currentJobId { get; set; }
        private List<SPTreeNodeDto> mTreeNodeList;
        private List<Contract.Global.Object.NodeFlag> siteNodeFlags = null;
        private UniqueIdSettingJobMessage mJobMessage;
        private string mErrorMessage;
        private bool mHasError;
        
        public void Bind(string msg)
        {
            currentJobId = JobContext.Current.JobId;
            mJobMessage = SerializerHelper.DeserializeByDataContractSerializer<UniqueIdSettingJobMessage>(msg);
        }
        public void Run()
        {
            try
            {
                JobContext.Current.mProgressManager.Create().IncreaseBase(100);
                ConfigUniqueIDSetting();
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while running unique id setting job. Error: {e}");
                mHasError = true;
            }
        }
        public SPUniqueIdSettingWorker()
        {
            siteNodeFlags = new List<Contract.Global.Object.NodeFlag>();
        }
        private void ConfigUniqueIDSetting()
        {
            try
            {
                InitTreeNode();
                if (mJobMessage.CurUniqueIdSetting != null)
                {
                    foreach (var site in mTreeNodeList)
                    {
                        var groupNodeId = new Guid(site.Parent.SPObjectId);
                        var groupShowUniqueIdSettingDic = mJobMessage.SiteGroupEnableUniqueIdDic;
                        if (!groupShowUniqueIdSettingDic.ContainsKey(groupNodeId) || !groupShowUniqueIdSettingDic[groupNodeId])
                        {
                            logger.Info($"This group has not been set global setting or not enable show unqiueId,Id:{groupNodeId}, showUniqueId:{false}");
                            continue;
                        }
                        ProcessSiteNode(site);
                    }
                }
                else 
                {
                    mHasError = true;
                    mErrorMessage = "RM_BCM_UniqueId_NotConfigSetting";
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while set unique id:{0}", ex.ToString());
            }
            finally
            {
                SendSiteNodeFlag();
                try
                {
                    JobContext.Current.Cleanup();
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while cleaning up. Error:" + e.ToString());
                }

                if (mHasError)
                {
                    if (!string.IsNullOrEmpty(mErrorMessage))
                    {
                        HybridApiClient.Instance.UpdateJobState(currentJobId, (int)JobStatus.Failed, mErrorMessage);
                    } 
                    else 
                    {
                        HybridApiClient.Instance.UpdateJobState(currentJobId, (int)JobStatus.FinishWithException, "RM_SS_CommonErrorMessage");
                    }
                }
                else
                {
                    JobContext.Current.JobSummaryService.NotifyManager((int)JobStatus.Finished, currentJobId);
                }
                logger.Info("set unique id setting job finished.");
            }
        }
        public void ProcessSiteNode(SPTreeNodeDto curNode)
        {
            bool isEnableRecordManagement = false;
            bool curSiteHasError = false;
            BaseUniqueIdSettingProcessor processor = null;
            try
            {
                long startTime = mJobMessage.SiteInformationDic[curNode.FullPath].LastScanTime;
                var siteIsEnableRecordsManagement = mJobMessage.SiteEnableSettings != null && mJobMessage.SiteEnableSettings.Any(o => o.GroupId == new Guid(curNode.Parent.SPObjectId) && o.SiteId == new Guid(curNode.SPObjectId) && o.EnableRecordsManagement);
                if (siteIsEnableRecordsManagement)
                {
                    isEnableRecordManagement = true;
                    if (startTime == DateTime.MinValue.Ticks || NeedRunSearchDiscover(startTime))
                    {
                        processor = new UniqueIdSettingFullProcessor(curNode, null, null, mJobMessage);
                    }
                    else
                    {
                        processor = new UniqueIdSettingInrementalProcessor(curNode, mJobMessage);
                    }
                    processor.Run();
                    if (processor.haveErrorNode)
                    {
                        curSiteHasError = true;
                        mHasError = true;
                    }
                }
                else
                {
                    JobContext.Current.JobDetailManager.Create().Commit(new JMUniqueIDSettingJobDetails() { ObjectName = curNode.Name, SourceURL = curNode.FullPath, ColumnName = "Document ID", AgentName = AvePoint.GCommon.Utility.OSInformation.HostName, Status = JobDetailsStatus.Skipped, Comment = "RM_JS_JMD_DisableRecordManagement" });
                }
            }
            catch (Exception e)
            {
                logger.Warn("Set Unique Id error {0}", e.ToString());
                JobContext.Current.JobDetailManager.Create().Commit(new JMUniqueIDSettingJobDetails() { ObjectName = curNode.Name, SourceURL = curNode.FullPath, ColumnName = "Document ID", AgentName = AvePoint.GCommon.Utility.OSInformation.HostName, Status = JobDetailsStatus.Failed, Comment = AgentUtil.GetExceptionMessage(e) });
                curSiteHasError = true;
                mHasError = true;
            }
            finally
            {
                
                if (isEnableRecordManagement && !curSiteHasError)
                {
                    logger.Info($"update the site node flag info, url:{curNode.FullPath.LogBase64()}");
                    siteNodeFlags.Add(new Contract.Global.Object.NodeFlag()
                    {
                        NodeId = new Guid(curNode.SPObjectId),
                        Title = curNode.Name,
                        FullPath = curNode.FullPath,
                        CollectionTime = mJobMessage.MainJobStartTime,
                        GroupId = new Guid(curNode.Parent.SPObjectId),
                        IsRemoved = false,
                        NodeFlagType = 0
                    });
                }
            }
        }

        //上次运行job是在59天以前，本次Job采用CAML Query方式，防止由于change log被冲掉了导致少查数据
        private bool NeedRunSearchDiscover(long lastJobTimeTicks)
        {
            var lastJobTime = DateTime.SpecifyKind(new DateTime(lastJobTimeTicks), DateTimeKind.Utc);
            return lastJobTime.AddDays(59) < DateTime.UtcNow;
        }

        private void InitTreeNode()
        {
            try
            {
                List<SPTreeNodeDto> treeList = new List<SPTreeNodeDto>();
                mJobMessage.TreeNodes.ForEach(node => treeList.Add(DtoConverter.ConvertRMTree2SPTree(node)));
                if (!(treeList == null || treeList.Count==0))
                {
                    mTreeNodeList = treeList;
                    logger.Info("success to init tree node.");
                }
                else
                {
                    throw new Exception("no tree node was found.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to init tree nodes, Error: {ex}");
            }
        }
        private void SendSiteNodeFlag()
        {
            if (siteNodeFlags.Count > 0)
            {
                for (int i = 0; i < siteNodeFlags.Count; i += 100)
                {
                    try
                    {
                        var nodeFlags = siteNodeFlags.Skip(i).Take(100).ToList();
                        HybridApiClient.Instance.AddSiteFlagInfos(nodeFlags);
                    }
                    catch (Exception e)
                    {
                        logger.Warn("An error occurred while sending site node flags. Error:{0}", e.ToString());
                    }
                }
            }
        }
    }
}
