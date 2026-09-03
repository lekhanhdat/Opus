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
using System.Threading.Tasks;
using AvePoint.RA.Contract.Audit.Async;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Audit;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Audit;

public class RMDiscoveryOffice365ExportRowDataAfterAuditHandler : IAsyncAuditAfterHandler
{
    public async Task<RMAuditInfo> CollectAsync(RMAuditInfo auditInfo, AuditModule module, AuditAction action, AuditCategory category, object[] args,
        object returnValue)
    {
        if (returnValue is RAReturnMessage responseMessage)
        {
            var createJobFailed = responseMessage.MessageType == RAMessageType.Failed;
            auditInfo.Status = createJobFailed ? 1 : auditInfo.Status;
            return createJobFailed ? auditInfo : null;
        }
        auditInfo.Object = returnValue.ToString();
        return auditInfo;
    }
}