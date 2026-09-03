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
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Tenant;
using System;
using System.Threading.Tasks;
using Cloud.Sdk.Data.MyHub;

namespace AvePoint.RA.Common.Audit
{
    public class MyHubAuditHelper
    {
        private RALogger logger = RALogger.GetInstance(typeof(MyHubAuditHelper));
        public async Task SendMyHubAduit(string objectName, AuditActionType action, AADAccount userInfo)
        {
            try
            {
                var myhubClient = AosApiUtility.GetMyhubClient(TenantLocalValue.LogonGroupId);
                logger.Info("Get myhub client success.");
                var userType = userInfo?.InviteType == AccountType.Group ? AuditUserType.Office365Group : AuditUserType.User;
                var auditModel = new AuditModel()
                {
                    Category = AuditCategory.OpusTask,
                    InstanceType = InstanceType.OpusCommonTask,
                    ActionTime = DateTime.UtcNow,
                    Action = action,
                    InstanceName = objectName,
                    Processor = new AuditUserModel(userInfo?.Id, userInfo?.UserPrincipalName, userInfo?.DisplayName, userInfo?.Mail)
                };
                auditModel.Processor.AuditUserType = userType;
                await myhubClient.AuditService.AddAuditAsync(auditModel);
                logger.Info("add myhub audit success.");
            }
            catch (Exception e)
            {
                logger.Error($"add myhub audit failed, error : {e}.");
                throw;
            }
        }
    }
}
