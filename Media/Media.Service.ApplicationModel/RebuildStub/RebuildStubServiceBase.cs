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
namespace AvePoint.Media.Service
{
    #region using directives

    using System;
    using System.Reflection;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.Server.Job.Object;
    using Merged18NResources.MediaServiceApplicationModel;
    using AvePoint.Media.Service.DomainModel;
    using Storage.Util;
    using AvePoint.RA.Contract.Exceptions;
    using AvePoint.RA.Contract.RMWeb.JobMonitor;

    #endregion using directives

    public abstract class RebuildStubServiceBase<TParameter>
        : ApplicationModelServiceBase
        , IRebuildStubService
        where TParameter : class, IRebuildStubInfo
    {
        AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        protected Action<JMArchiverRebuildStubJobDetails> mReportAction;
        public void RebuildStub(IRebuildStubInfo rebuildStubInfo, Action<JMArchiverRebuildStubJobDetails> reportAction)
        {
            var info = rebuildStubInfo as TParameter;
            mReportAction = reportAction;
            this.InternalRebuildStub(info);
        }

        private void InternalRebuildStub(TParameter rebuildStubInfo)
        {
            try
            {
                this.Open(rebuildStubInfo);
                RealRebuildStub(rebuildStubInfo);
            }
            catch (JobStopException e)
            {
                logger.Warn($"Job will stop, throw JobStopException in InternalRebuildStub.Message:{e}.");
                throw;
            }
            catch (Exception e)
            {
                logger.Warn($"InternalRebuildStub Job error.Message:{e}.");
                throw;
            }
            finally
            {
                this.Close();
            }
        }

        public abstract void Open(TParameter rebuildStubInfo);

        public abstract void RealRebuildStub(TParameter rebuildStubInfo);
        public virtual void Close()
        {
            this.Dispose();
        }

        public void AddToReport(string siteUrl, string stubUrl, JobDetailsStatus jobDetailsStatus, string jobId, string comment = "")
        {
            var report = new JMArchiverRebuildStubJobDetails();
            report.StubUrl = stubUrl;
            report.SiteUrl = siteUrl;
            report.Status = jobDetailsStatus;
            report.JobId = jobId;
            if (mReportAction != null && report != null)
            {
                mReportAction(report);
            }
        }

        #region IDisposable

        public abstract void Dispose();

        #endregion IDisposable
    }
}