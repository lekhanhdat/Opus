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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;
using AvePoint.Wrapper.Common;
using AvePoint.Office365.Api;
using Microsoft.Office.Project.Server.Interfaces;
using System.ServiceModel;
using Microsoft.Office.Project.Server.Library;
using System.Xml;
using AvePoint.GCommon;
using Microsoft.Office.Project.Server.Schema;
using System.Threading;
using static Microsoft.Office.Project.Server.Schema.QueueStatusDataSet;

namespace AvePoint.ObjectModel.PSI
{
    public class AvePSIRequest
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AvePSIRequest));
        private string mSiteUrl;
        private ITokenProvider mTokenProvider;
        public AvePSIRequest(string siteUrl, ITokenProvider tokenProvider)
        {
            mSiteUrl = siteUrl;
            mTokenProvider = tokenProvider;
        }

        #region timeline

        public string ReadServerTimeLine()
        {
            using (AvePSIClient<IPWA> factory = new AvePSIClient<IPWA>(mSiteUrl, mTokenProvider))
            {
                IPWA pwa = factory.ServiceChannel;
                return pwa.ProjectReadServerTimelineDataForJSON(Guid.Empty, Microsoft.Office.Project.Server.Library.DataStoreEnum.PublishedStore);
            }
        }

        public void UpdateTimeLine(string tlViewData)
        {
            using (AvePSIClient<IPWA> factory = new AvePSIClient<IPWA>(mSiteUrl, mTokenProvider))
            {
                IPWA pwa = factory.ServiceChannel;
                pwa.ProjectUpdateServerTimelineData(AveProjectConstants.ProjectCenterUID, tlViewData);
            }
        }

        #endregion

        #region enterprise type detail page

        public List<AveProjectDetailPageInfo> ReadEnterpriseTypePDPs(Guid projId)
        {
            using (AvePSIClient<IWorkflow> factory = new AvePSIClient<IWorkflow>(mSiteUrl, mTokenProvider))
            {
                IWorkflow workflow = factory.ServiceChannel;
                WorkflowDataSet dst = workflow.ReadEnterpriseProjectType(projId);
                List<AveProjectDetailPageInfo> pdplist = new List<AveProjectDetailPageInfo>();
                foreach (WorkflowDataSet.EnterpriseProjectTypePDPsRow enterpriseProjectTypePDPsRow in dst.EnterpriseProjectTypePDPs)
                {
                    AveProjectDetailPageInfo info = new AveProjectDetailPageInfo();
                    info.Id = enterpriseProjectTypePDPsRow.PDP_UID;
                    info.Name = enterpriseProjectTypePDPsRow.PDP_NAME;
                    info.Position = enterpriseProjectTypePDPsRow.PDP_POSITION;
                    info.RowId = enterpriseProjectTypePDPsRow.PDP_ID;
                    info.IsCreatePDP = enterpriseProjectTypePDPsRow.IS_CREATE_PDP;
                    if (info.IsCreatePDP)
                    {
                        pdplist.Insert(0, info);
                    }
                    else
                    {
                        pdplist.Add(info);
                    }
                }
                return pdplist;
            }
        }

        public void UpdateEnterpriseTypeByPSI(Guid projId, AveProjectEnterpriseProjectTypeInfo eptInfo)
        {
            using (AvePSIClient<IWorkflow> factory = new AvePSIClient<IWorkflow>(mSiteUrl, mTokenProvider))
            {
                IWorkflow workflow = factory.ServiceChannel;
                WorkflowDataSet dst = workflow.ReadEnterpriseProjectType(projId);
                foreach (WorkflowDataSet.EnterpriseProjectTypePDPsRow enterpriseProjectTypePDPsRow in dst.EnterpriseProjectTypePDPs)
                {
                    if (enterpriseProjectTypePDPsRow.IS_CREATE_PDP && enterpriseProjectTypePDPsRow.ENTERPRISE_PROJECT_TYPE_UID == projId)
                    {
                        enterpriseProjectTypePDPsRow.Delete();
                        break;
                    }
                }
                foreach (AveProjectDetailPageInfo info in eptInfo.ProjectDetailPages)
                {
                    if (info.IsCreatePDP)
                    {
                        WorkflowDataSet.EnterpriseProjectTypePDPsRow enterpriseProjectTypePDPsRow2 = dst.EnterpriseProjectTypePDPs.NewEnterpriseProjectTypePDPsRow();
                        enterpriseProjectTypePDPsRow2.ENTERPRISE_PROJECT_TYPE_UID = projId;
                        enterpriseProjectTypePDPsRow2.PDP_UID = info.Id;
                        //enterpriseProjectTypePDPsRow2.PDP_ID = pDPInfo.ID;
                        enterpriseProjectTypePDPsRow2.PDP_NAME = info.Name;
                        dst.EnterpriseProjectTypePDPs.AddEnterpriseProjectTypePDPsRow(enterpriseProjectTypePDPsRow2);
                        
                    }
                    else
                    {
                        WorkflowDataSet.EnterpriseProjectTypePDPsRow enterpriseProjectTypePDPsRow2 = dst.EnterpriseProjectTypePDPs.FindByENTERPRISE_PROJECT_TYPE_UIDPDP_UIDIS_CREATE_PDP(projId, info.Id, false);
                        if (enterpriseProjectTypePDPsRow2 != null)
                        {
                            enterpriseProjectTypePDPsRow2.PDP_POSITION = info.Position;
                        }
                        else
                        {
                            //int num = this.PDPList.BinarySearch(new EnterpriseProjectTypeDetails.PDPInfo
                            //{
                            //    Uid = guid
                            //});
                            //if (num >= 0)
                            //{
                                enterpriseProjectTypePDPsRow2 = dst.EnterpriseProjectTypePDPs.NewEnterpriseProjectTypePDPsRow();
                                enterpriseProjectTypePDPsRow2.ENTERPRISE_PROJECT_TYPE_UID = projId;
                                enterpriseProjectTypePDPsRow2.PDP_UID = info.Id;
                                //enterpriseProjectTypePDPsRow2.PDP_ID = this.PDPList[num].ID;
                                enterpriseProjectTypePDPsRow2.PDP_NAME = info.Name;
                                enterpriseProjectTypePDPsRow2.IS_CREATE_PDP = false;
                                enterpriseProjectTypePDPsRow2.PDP_POSITION = info.Position;
                                dst.EnterpriseProjectTypePDPs.AddEnterpriseProjectTypePDPsRow(enterpriseProjectTypePDPsRow2);
                            //}
                        }
                    }
                  
                }
                //if (eptInfo.WorkflowAssociationId == Guid.Empty)
                //{
                //    dst.EnterpriseProjectType[0].SetWORKFLOW_ASSOCIATION_UIDNull();
                //    dst.EnterpriseProjectType[0].SetWORKFLOW_ASSOCIATION_NAMENull();
                //}
                //else
                //{
                //    dst.EnterpriseProjectType[0].WORKFLOW_ASSOCIATION_UID = eptInfo.WorkflowAssociationId;
                //}
                workflow.UpdateEnterpriseProjectType(dst);
            }
        }

        #endregion

        public bool WaitForQueue(Guid jobId)
        {
            using (AvePSIClient<IQueueSystem> factory = new AvePSIClient<IQueueSystem>(mSiteUrl, mTokenProvider))
            {
                const int QUEUE_WAIT_TIME = 5;
                int retryCount = 0;
                bool jobDone = false;
                IQueueSystem queue = factory.ServiceChannel;
                string errorMessage;
                try
                {
                    do
                    {
                        QueueConstants.JobState state = queue.GetJobCompletionState(jobId, out errorMessage);
                        if (state == QueueConstants.JobState.Success)
                        {
                            jobDone = true;
                        }
                        else
                        {
                            if (state == QueueConstants.JobState.Unknown
                                || state == QueueConstants.JobState.Failed
                                || state == QueueConstants.JobState.FailedNotBlocking
                                || state == QueueConstants.JobState.Canceled)
                            {
                                mLogger.Warn("job state:{0}, jobid:{1}", state, jobId);
                                if (retryCount < 2)
                                {
                                    retryCount++;
                                    mLogger.Info("retry job, jobid:{0}, retry index:{1}", jobId, retryCount);
                                    queue.RetryJob(jobId);
                                    int waitTime = queue.GetJobWaitTime(jobId);
                                    Thread.Sleep(waitTime * 1000);
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else if (state == QueueConstants.JobState.CorrelationBlocked)
                            {
                                mLogger.Warn("job state:{0}, jobid:{1}", state, jobId);
                                QueueStatusDataSet qd = queue.ReadJobStatusSimple(new Guid[] { jobId }, true);
                                Guid correlationId = Guid.Empty;
                                if (qd.Status.Count > 0)
                                {
                                    foreach (StatusRow row in qd.Status)
                                    {
                                        if (row.JobCompletionState == (int)QueueConstants.JobState.CorrelationBlocked)
                                        {
                                            correlationId = qd.Status[0].CorrelationGUID;
                                            queue.UnblockCorrelation(correlationId);
                                        }
                                        mLogger.Info("job Id:{0}, job completion state:{1}, errorInfo:{2}", jobId, row.JobCompletionState, row.ErrorInfo);
                                    }
                                }
                                if (retryCount < 2)
                                {
                                    retryCount++;
                                    mLogger.Info("retry job, jobid:{0}, retry index:{1}", jobId, retryCount);
                                    queue.RetryJob(jobId);
                                    int waitTime = queue.GetJobWaitTime(jobId);
                                    Thread.Sleep(waitTime * 1000);
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                mLogger.Info("job state:{0}, jobid:{1}", state, jobId);
                                Thread.Sleep(QUEUE_WAIT_TIME * 1000);
                            }
                        }
                    }
                    while (!jobDone);
                }
                catch (Exception e)
                {
                    mLogger.Warn("job failed. jobid:{0}, error:{1}", jobId, e.ToString());
                }
                return jobDone;
            }
        }

        private PSClientError GetPSClientError(FaultException e, out string errOut)
        {
            const string PREFIX = "GetPSClientError() returns null: ";
            errOut = string.Empty;
            PSClientError psClientError =null;

            if (e == null)
            {
                errOut = PREFIX + "Null parameter (FaultException e) passed in.";
                psClientError = null;
            }
            else
            {
                // Get a ServiceModel.MessageFault object.
                var messageFault = e.CreateMessageFault();

                if (messageFault.HasDetail)
                {
                    using (var xmlReader = messageFault.GetReaderAtDetailContents())
                    {
                        var xml = new XmlDocument();
                        xml.Load(xmlReader);

                        psClientError = new PSClientError(xml.OuterXml);

                        var serverExecutionFault = xml["ServerExecutionFault"];
                        if (serverExecutionFault != null)
                        {
                            var exceptionDetails = serverExecutionFault["ExceptionDetails"];
                            if (exceptionDetails != null)
                            {
                                try
                                {
                                    errOut = exceptionDetails.InnerXml + "\r\n";
                                    //psClientError = new PSClientError(exceptionDetails.InnerXml);
                                }
                                catch (InvalidOperationException ex)
                                {
                                    errOut = PREFIX + "Unable to convert fault exception info ";
                                    errOut += "a valid Project Server error message. Message: \n\t";
                                    errOut += ex.Message;
                                    psClientError = null;
                                }
                            }
                            else
                            {
                                errOut = PREFIX
                                    + "The FaultException e is a ServerExecutionFault, "
                                    + "but does not have ExceptionDetails.";
                            }
                        }
                        else
                        {
                            errOut = PREFIX
                                + "The FaultException e is not a ServerExecutionFault.";
                        }
                    }
                }
                else // No detail in the MessageFault.
                {
                    errOut = PREFIX + "The FaultException e does not have any detail.";
                }
            }
            errOut += "\r\n" + e.ToString() + "\r\n";
            return psClientError;
        }

        

    }
}
