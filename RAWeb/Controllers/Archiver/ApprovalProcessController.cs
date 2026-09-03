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
using AvePoint.RA.Contract.ManualApproval;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Service.Services.Archiver;
using AvePoint.RA.Web.Common;
using Microsoft.AspNetCore.Mvc;
using System;

namespace AvePoint.RA.Web.Controllers.Archiver
{
    public class ApprovalProcessController : BaseApiController
    {
        private ITenantService _TenantService;
        private ITenantService TenantService => PlatformWindsorManager.GetService(ref _TenantService);
        private IRMArchiverSettingsService _RMArchiverSettingsService;
        private IRMArchiverSettingsService RMArchiverSettingsService => PlatformWindsorManager.GetService(ref _RMArchiverSettingsService);
        private IRMManualApprovalService RMManualApprovalService => PlatformWindsorManager.GetService<IRMManualApprovalService>();
        [HttpPost]
        public RAReturnMessage RunApprovalProcessJob([FromBody] bool fromTimerJobPage)
        {
            if (TenantService.IsNewOpusTenant())
            {
                var message = RMArchiverSettingsService.RunApprovalProcessJob(JobRunBy.Control, TenantLocalValue.LogonUserEmail);
                if (message == null || message.MessageType == RAMessageType.Failed)
                {
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
                return new RAReturnMessage();
            }
            else
            {
            }
            return new RAReturnMessage();
        }
        [Obsolete("This API is only for test.")]
        [HttpPost]
        public RAReturnMessage TestDeleteInvalidRecordsJob([FromBody] bool fromTimerJobPage)
        {
            if (TenantService.IsNewOpusTenant())
            {
                var message = RMManualApprovalService.RunDeleteInvalidRecordsJob(JobRunBy.Control, TenantLocalValue.LogonUserEmail);
                if (message == null || message.MessageType == RAMessageType.Failed)
                {
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
                return new RAReturnMessage();
            }
            else
            {
            }
            return new RAReturnMessage();
        }
    }
}
