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
using AvePoint.Hybrid.ClientLibrary.Data;
using AvePoint.RA.Api.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RAApi.UnitTest
{
    [TestClass]
    public class JobMonitorUnitTest
    {
        //[TestMethod]
        //public void UpdateJobProgress()
        //{
        //    RMGlobalConfiguration.Init();
        //    HybridApiClient client = HybridApiClient.Instance;
        //    client.UpdateJobProgress(new HBJobStatusInfo() { JobId = "123", IsSubJob = false,  Progress = 12 });
        //}

        //[TestMethod]
        //public void UploadJobJobReport() 
        //{
        //    HBReportFileInfo reportInfo = new HBReportFileInfo() { JobId = "123", JobType = 5000, FileName = "test.rpt", TenantId = "321", File = new byte[12] };
        //    RMGlobalConfiguration.Init();
        //    HybridApiClient client = HybridApiClient.Instance;
        //    client.SendReport(Convert2HBReportInfo(reportInfo));
        //}
        private HBReportInfo Convert2HBReportInfo(HBReportFileInfo reportFileInfo) 
        {
            return new HBReportInfo()
            {
                JobId = reportFileInfo.JobId,
                FileName = reportFileInfo.FileName,
                JobType = reportFileInfo.JobType,
                File = reportFileInfo.File,
            };
        }
    }
}
