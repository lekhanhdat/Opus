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
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Threads;
using AvePoint.RA.Contract.Workflow;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.Workflow.Common;
using System;
using System.Activities;
using System.Reflection;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Contract.ManualApproval;

namespace AvePoint.RA.Workflow.DisposalReview.Activities
{

    public sealed class DisposalReviewActivity : NativeActivity<DisposalReviewActionEnum>
    {
        private static readonly RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        [RequiredArgument]
        public InArgument<DisposalReviewRequestInfo> RequestInfo { get; set; }

        public string BookmarkName { get; set; }

        public Guid StepId { get; set; }

        /// <summary>
        /// 如果是start workflow时，因为可能workflow的instance会有很多，不需要在workflow里触发邮件通知功能.
        ///此标志用来表示，是否执行通知功能的操作.
        /// </summary>
        public bool NeedNotification { get; set; } = true;

        public OutArgument<DisposalReviewRequestInfo> OutRequestInfo { get; set; }


        private readonly IRMManualApprovalService ManualApprovalService = PlatformWindsorManager.GetService<IRMManualApprovalService>();

        private IWorkflowInstanceDao WorkflowInstanceDao
        {
            get
            {
                return (IWorkflowInstanceDao)PlatformWindsorManager.GetService(typeof(IWorkflowInstanceDao));
            }
        }

        private IRMEmailItemDao EmailItemDao
        {
            get
            {
                return (IRMEmailItemDao)PlatformWindsorManager.GetService(typeof(IRMEmailItemDao));
            }
        }


        //private IManualApprovalService ManualApprovalService
        //{
        //    get
        //    {
        //        return (IManualApprovalService)PlatformWindsorManager.GetService(typeof(IManualApprovalService));
        //    }
        //}

        private IExplorerDao ExplorerDao
        {
            get
            {
                return (IExplorerDao)PlatformWindsorManager.GetService(typeof(IExplorerDao));
            }
        }

        protected override bool CanInduceIdle
        { //override when the custom activity is allowed to make he workflow go idle
            get
            {
                return true;
            }
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);

            if (string.IsNullOrWhiteSpace(BookmarkName)) metadata.AddValidationError($"Property 'BookmarkName' is not set for DisposalReviewActivity : '{this.DisplayName}'");

            if (StepId == Guid.Empty) metadata.AddValidationError($"Property 'StepId' is not set for DisposalReviewActivity : '{this.DisplayName}'");

        }

        protected override void Execute(NativeActivityContext context)
        {
            var req = RequestInfo.Get(context);
            WFThreadHelper.SetForCurrentThread(req); //set the setting for current thread

            //send email to reviewers logic
            if (req.IsSendEmail && NeedNotification)
            {
                //ManualApprovalService.SendEmailToReviewers(StepId, req);
                //add 记录
                if (req.InstanceId != Guid.Empty)
                {
                    logger.Info("Ceate workflow manual instance item,Id is {0}", req.InstanceId.ToString());
                    RMEmailItem emailItem = new RMEmailItem();
                    emailItem.Id = req.InstanceId;
                    emailItem.Status = RMSendEmailStatus.WaittingSendEmail;
                    emailItem.ModifyTime = DateTime.UtcNow;
                    EmailItemDao.AddWorkflowManualItem(emailItem);
                }
            }

            ////clear escalate info
            //ManualApprovalService.ClearWorkflowEscalateInfo(context.WorkflowInstanceId);
            //update related information
            logger.Info($"Create bookmark: {BookmarkName}, Instance id : {context.WorkflowInstanceId}, StepId: {StepId}, StepName: {this.DisplayName}");
            context.CreateBookmark(BookmarkName, new BookmarkCallback(this.OnReadComplete));

            //add other logic here.....
            try
            {

                //需要更新workflow instance表中的step id，step name....
                WorkflowInstanceDao.UpdateStepInfo(context.WorkflowInstanceId, StepId.ToString(), this.DisplayName);
                //ManualApprovalService.UpdateRecordOwner(context.WorkflowInstanceId);
                ManualApprovalService.UpdateReviewByWorkflow(req);
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                throw;
            }
        }

        void OnReadComplete(NativeActivityContext context, Bookmark bookmark, object obj)
        {
            var requestInfo = obj as DisposalReviewRequestInfo;
            logger.Info($"WF Test OnReadComplete TenantId: {requestInfo.TenantGroupId}, Instance id : {context.WorkflowInstanceId}");
            WFThreadHelper.SetForCurrentThread(requestInfo); //set the setting for current thread

            logger.Info($"Execute bookmark : {bookmark.Name}, Instance id : {context.WorkflowInstanceId}, StepId: {StepId}, StepName: {this.DisplayName}, Execute action : {requestInfo.Action}");
            //ManualApprovalService.AddWorkflowHistory(requestInfo);
            //to be add other logic here....
            ManualApprovalService.AddAuditByWorkflow(requestInfo);
            this.Result.Set(context, requestInfo.Action);
            this.OutRequestInfo.Set(context, requestInfo);
        }
    }
}
