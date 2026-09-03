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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using System;
using System.Collections.Generic;
using AvePoint.RA.Common.Threads;
using System.Threading;
using AvePoint.RA.Contract.Tenant;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Audit
{
    /// <summary>
    /// have reviewed by allen yin
    /// </summary>
    public class AuditWorker
    {
       private static RALogger logger = RALogger.GetInstance(typeof(AuditWorker));

        public AuditWorker(AuditArg arg)
        {
            this.arg = arg;
        }
        private AuditArg arg;
        public IAuditCommonService AuditService => PlatformWindsorManager.GetService<IAuditCommonService>();

        public async Task WorkAsync()
        {
            if (arg.StartNewThread)
            {
                //AveTenantThread thread = new AveTenantThread(new ThreadStart(DoWorkAsync));
                //if this is not a thread pool, might cause performance issue.
                //AveThreadUtility.StartThread(DoWork, "AuditWorker", string.Empty);
                //thread.Start();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                //here we do audit without waiting result.
                Task.Run(() => { 
                    
                    
                    
                    
                    DoWorkAsync(); 
                });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            else
            {
                await DoWorkAsync();
            }
        }

        private async Task DoWorkAsync()
        {
            RMAuditInfo info = arg.auditInfo;
            try
            {
                IAfterAuditHandler AuditHandler = (IAfterAuditHandler)PlatformWindsorManager.GetService(arg.HandlerType.ToString(), arg.HandlerType);
                info = await AuditHandler.CollectAsync(info, arg.Module,arg.Category, arg.Action, arg.Args, arg.Target, arg.ReturnValue);
                
                //return if outside code handle audit info itself
                if (arg.IsHandled)
                {
                    return;
                }
                if (info != null && !info.NotNeedRecordAudit)
                {
                    info.ExecuteOn = DateTime.UtcNow;
                    ITenantService tenantService = (ITenantService)PlatformWindsorManager.GetService(typeof(ITenantService));
                    var tenantId = TenantLocalValue.LogonGroupId;
                    if (tenantService.CheckTenantExist(tenantId))
                    {
                        info.Method = info.Method == null ? arg.Method.DeclaringType + "." + arg.Method.Name : info.Method;
                        info.UserName = TenantLocalValue.PartnerUser ?? (info != null && info.UserName != null ? info.UserName : arg.UserName);
                        info.Role = info.Role == null ? "Administrator" : info.Role;
                        info.ClientIP = arg.ClientIP;
                        AuditService.AddAudits(new List<RMAuditInfo>() { info });
                    }
                }
                else
                {
                    logger.Warn(string.Format("Empty info returned.Module[{0}],Category[{1}],Action[{2}]", arg.Module, arg.Category, arg.Action));
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }
    }
}
