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
using System;
using System.Threading;
using System.Threading.Tasks;
using AvePoint.GCommon;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.Wrapper.Common.MultiThread;

namespace AvePoint.RA.SharePoint.Archiver
{
    internal class MultiBackupTask : MultiBackupBaseTask<BackupNodeParameters, ArchiverBackupStreamWriter>
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(DisposalActivityManagementProcessor));
        public MultiBackupTask(BackupNodeParameters nodeParameters)
            : base(nodeParameters)
        {
            writer = new ArchiverBackupStreamWriter();
        }

        protected override async Task BackupAsync()
        {
            Logger.Info($"begin to backup entity :{nodeParameters.Node.NodeId} {nodeParameters.Node.CacheNodeType}");
            var sender = new BackupInfoSender(writer, nodeParameters.Sender?.BackupPermission); //CreateAveSender(ArchiverBackupJob);//SAAS-12879 由于多线程新建了BackupInfoSender对象，在打破继承时backuppermission需要传递过来。
            if (nodeParameters.Sender?.FileHeaderAttribute != null)
            {
                sender.FileHeaderAttribute = new System.Collections.Hashtable(nodeParameters.Sender?.FileHeaderAttribute);
            }
            int backupResult = await BackupAsync(nodeParameters.CacheSPObjs.ParentValueInCacheOfLevel(nodeParameters.Node.CacheNodeType), nodeParameters, sender);
            //SafeReleaseNode(current, entity.CacheNodeType);
        }

        private async Task<int> BackupAsync(CacheNode parent, BackupNodeParameters nodeParameters, BackupInfoSender sender)
        {
            if (nodeParameters.Node.IsRepeatProcess)
            {
                if (nodeParameters.Configuration.ArchiveJobSplitedDBInfo.IsLatestSplitedDB == true)
                {
                    return await nodeParameters.BackupObj.RepeatProcessContainerNode(parent, nodeParameters.CacheNode, nodeParameters.Node, nodeParameters.RuleName, nodeParameters.SubJobId, nodeParameters.RuleLevel, nodeParameters.MediaName, sender);
                }
                else
                {
                    Logger.Info($"Current virtual job is not latest vritual job of current sc, will skip process processed node:{nodeParameters.Node.NodeId}");
                    return (int)JobDetailsStatus.Successful;
                }
            }
            else
            {
                return await nodeParameters.BackupObj.BackupAsync(parent, nodeParameters.CacheNode, nodeParameters.Node, nodeParameters.RuleName, nodeParameters.SubJobId, nodeParameters.RuleLevel, nodeParameters.MediaName, sender);
            }
        }

        public override void CompleteTask()
        {
            CompleteTask(null);
        }

        public override void CompleteTask(Exception ex)
        {
            if (ex != null)
            {
                Logger.Error("backup the node:{0} failed:{1}", nodeParameters.Node.FullPath, ex);
            }
        }

        protected override void Close()
        {
            nodeParameters.CacheSPObjs.PutIn(nodeParameters.CacheNode, nodeParameters.Node.CacheNodeType, false);
            base.Close();
        }
    }
}
