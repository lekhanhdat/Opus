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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Common.Audit;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RMWeb.Account.Security;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Service.Security;
using System;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Object;
using AvePoint.GCommon.Utility;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.AccountManager.AuditHandler
{
    public class LogInAfterAuditHandler : IAfterAuditHandler
    {
        private RALogger logger = RALogger.GetInstance(typeof(LogInAfterAuditHandler));

        public async Task<RMAuditInfo> CollectAsync(RMAuditInfo info, int model, int category, int action, object[] args, object target, object returnValue)
        {
            RMAuditInfo auditInfo = new RMAuditInfo();
            try
            {
                var result = (((RAReturnMessage,RMIdentity))returnValue).Item1;
                if (result == null || result.MessageType != RAMessageType.Successful)
                {
                    auditInfo.NotNeedRecordAudit = true;
                }
                auditInfo.Module = (AuditModule)model;
                auditInfo.Category = (AuditCategory)category;
                auditInfo.Action = (AuditAction)action;
                if (action == (int)AuditAction.LogIn)
                {
                    var logOnInfo = args[0] as LogOnInfo;
                    var user = SerializerHelper.DeserializeByJsonConvert<AosUserInfo>(logOnInfo.user);
                    auditInfo.UserName = user.Username;
                    auditInfo.Object = user.Username;
                }
                else if (action == (int)AuditAction.SSOLogIn)
                {
                    var loginUserName = (((RAReturnMessage, RMIdentity))returnValue).Item2?.Name;
                    var partnerUser = (((RAReturnMessage, RMIdentity))returnValue).Item2?.PartnerUser;
                    auditInfo.UserName = partnerUser ?? loginUserName;
                    auditInfo.Object = partnerUser ?? loginUserName;
                }
                if (info != null && info.E != null)
                {
                    auditInfo.Status = (int)AuditStatus.Failed;
                }
                else
                {
                    bool isAuthenticated = (((RAReturnMessage, RMIdentity))returnValue).Item1.MessageType == RAMessageType.Successful;
                    auditInfo.Status = isAuthenticated ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                }

            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                auditInfo.Status = (int)AuditStatus.Failed;
            }
            return auditInfo;
        }
    }
}
