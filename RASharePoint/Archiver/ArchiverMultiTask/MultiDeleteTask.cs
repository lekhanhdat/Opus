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
using AvePoint.GCommon;
using System.Collections.Generic;
using AvePoint.Wrapper.Common.MultiThread;
using AvePoint.RA.CommonUtil;
using AvePoint.GCommon.Contract.StorageOptimization.Object;

namespace AvePoint.RA.SharePoint.Archiver
{
    internal class MultiDeleteTask : BaseTask
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(DisposalActivityManagementProcessor));
        //private readonly DeletionNode node;
        private readonly ArchiverDeletion deletion;
        private readonly List<DeletionNode> deletionInfos;

        //public MultiDeleteTask(DeletionNode deletionNode, ArchiverDeletion archiverDeletion)
        //{
        //    node = deletionNode;
        //    deletion = archiverDeletion;
        //}
        public MultiDeleteTask(List<DeletionNode> deletionInfos, ArchiverDeletion deletion)   //SAAS-12437 支持多个DeletionNode。
        {
            this.deletionInfos = deletionInfos;
            this.deletion = deletion;
        }

        public override void Process()
        {
            if (deletion.mConfig != null
                && deletion.mConfig.currentRule.PolicyLevel == GCommon.Contract.CommonFilter.PolicyLevel.DocumentVersion
                && deletionInfos.Count > 0)
            {
                try
                {
                    Logger.Info("Current rule is document version rule and need HandleResponseDocumentVersionRuleMessage.deletionInfos Count:{0}", deletionInfos.Count);
                    deletion.HandleResponseDocumentVersionRuleMessage(deletionInfos);
                }
                catch (Exception ex)
                {
                    Logger.Error("Handle ResponseDocumentVersionRuleMessage failed:{0}.", ex);
                }
            }
            else if (deletion.mConfig != null
                && deletion.mConfig.currentRule.PolicyLevel == GCommon.Contract.CommonFilter.PolicyLevel.Document
                && (deletion.mConfig.currentRule.KeepDataOption & (int)KeepDataOption.KeepLatestVersionAndArhiveOthers) == (int)KeepDataOption.KeepLatestVersionAndArhiveOthers
                && deletionInfos.Count > 0)
            {
                try
                {
                    Logger.Info("Current rule is document rule: Keep the current version and number of previous version and archive others , and need HandleResponseDocumentVersionRuleMessage.deletionInfos Count:{0}", deletionInfos.Count);
                    deletion.HandleResponseDocumentVersionRuleMessage(deletionInfos);
                }
                catch (Exception ex)
                {
                    Logger.Error("Fail Handle document rule: Keep the current version and number of previous version and archive others ,Message failed:{0}.", ex);
                }
            }
            else
            {
                foreach (var item in deletionInfos)
                {
                    try
                    {
                        deletion.HandleResponseMessage(item);
                        Logger.Info("Process Node Infomations:{0}", item.SPId);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Handle node:{0} Response Message failed:{1}", item.Document.OuterXml, ex);
                    }
                }
            }
        }

        public override void CompleteTask()
        {
            CompleteTask(null);
        }

        public override void CompleteTask(Exception ex)
        {
            deletion.Dispose();
            if (ex != null)
            {
                Logger.Error("Handle node:{0} failed:{1}", deletionInfos[deletionInfos.Count - 1].Document.OuterXml, ex);
            }
        }

    }
}