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
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.Authentication;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.Service.Services.AuthenticationManager.AuditHandler
{
    public class AuthenticationBeforeAuditHandler : IBeforeAuditHandler
    {
        private AveLogger logger = AveLogger.GetInstance(typeof(AuthenticationBeforeAuditHandler));
        public IAuthenticationManagerService AuthenticationService { get; set; }

        [RACodeReview("Allen Yin")]
        public void Collect(out RMAuditInfo info, int model, int category, int action, object[] args, object target)
        {
            info = new RMAuditInfo();
            try
            {
                info.Module = (AuditModule)model;
                info.Category = (AuditCategory)category;
                info.Action = (AuditAction)action;
                switch (info.Action)
                {
                    case AuditAction.SetDefaultAuthenticationMode:
                        SetDefaultAuthenticationMode(ref info, args);
                        break;
                    case AuditAction.EnableAuthenticationMode:
                        EnableOrDisableAuthenticationMode(ref info, args);
                        break;
                    case AuditAction.DisableAuthenticationMode:
                        EnableOrDisableAuthenticationMode(ref info, args);
                        break;
                    case AuditAction.AddADDomain:
                        AddADDomain(ref info, args);
                        break;
                    case AuditAction.EditADDomain:
                        EditADDomain(ref info, args);
                        break;
                    case AuditAction.DeleteADDomain:
                        DeleteADDomain(ref info, args);
                        break;
                    case AuditAction.EnableADDomain:
                        EnableOrDisableADDomain(ref info, args);
                        break;
                    case AuditAction.DisableADDomain:
                        EnableOrDisableADDomain(ref info, args);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
            }
        }

        private void SetDefaultAuthenticationMode(ref RMAuditInfo info, object[] args)
        {
            string model = I18NEntity.GetString("RM_Audit_Authentication_Model");
            var oldMode = AuthenticationService.GetDefaultAuthenticationMode();
            var newMode = AuthenticationService.GetAuthenticationModeById((int)args[0]);
            info.ModifyContent = new List<AuditItem>();
            info.ModifyContent.Add(new AuditItem() { TargetSetting = model, OldValue = string.Format("{0}", oldMode.Name), NewValue = string.Format("{0}",  newMode.Name) });
        }

        private void EnableOrDisableAuthenticationMode(ref RMAuditInfo info, object[] args)
        {
            var mode = AuthenticationService.GetAuthenticationModeById((int)args[0]);
            info.Object = mode.Name;
        }

        private void AddADDomain(ref RMAuditInfo info, object[] args)
        {
            var domain = (RMDomainDto)args[0];
            info.Object = domain.DomainName;
        }

        private void EditADDomain(ref RMAuditInfo info, object[] args)
        {
            int domainId = (int)args[0];
            string userName = args[1].ToString();
            string password = args[2].ToString();
            var domain = AuthenticationService.GetADDomain(domainId, true);
            if (domain != null)
            {
                info.Object = domain.DomainName;
                info.ModifyContent = new List<AuditItem>();
                info.ModifyContent.Add(new AuditItem() { TargetSetting = "User Name", OldValue = domain.UserName, NewValue = userName });
                info.ModifyContent.Add(new AuditItem() {
                    TargetSetting = "Change Password",
                    NewValue = (!domain.UserName.Split('\\')[0].Equals(userName.Split('\\')[0], StringComparison.OrdinalIgnoreCase) || password != domain.Password).ToString()
                });
            }
        }

        private void DeleteADDomain(ref RMAuditInfo info, object[] args)
        {
            if (args[0] is List<int>)     //多选delete
            {
                List<int> ids = args[0] as List<int>;
                var domains = AuthenticationService.GetADDomains(ids);
                int total = domains.Count();
                if (total > 0)
                {
                    info.Object = domains[0].DomainName;
                    for (int i=1; i< total; i++)
                    {
                        info.Object += ";" + domains[i].DomainName;
                    }
                }
            }
            else    //单选delete
            {
                var domain = AuthenticationService.GetADDomain((int)args[0]);
                if (domain != null)
                {
                    info.Object = domain.DomainName;
                }
            }
        }

        private void EnableOrDisableADDomain(ref RMAuditInfo info, object[] args)
        {
            if (args[0] is List<int>)     //多选delete
            {
                List<int> ids = args[0] as List<int>;
                var domains = AuthenticationService.GetADDomains(ids);
                int total = domains.Count();
                if (total > 0)
                {
                    info.Object = domains[0].DomainName;
                    for (int i = 1; i < total; i++)
                    {
                        info.Object += ";" + domains[i].DomainName;
                    }
                }
            }
            else    //单选delete
            {
                var domain = AuthenticationService.GetADDomain((int)args[0]);
                if (domain != null)
                {
                    info.Object = domain.DomainName;
                }
            }

            info.Action = (bool)args[1] ? AuditAction.EnableADDomain : AuditAction.DisableADDomain;
        }

    }
}
