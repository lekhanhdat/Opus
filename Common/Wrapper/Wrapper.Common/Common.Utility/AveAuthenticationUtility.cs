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
using System.Text;
using System.IO;
using AvePoint.GCommon;
using System.Reflection;

namespace AvePoint.Wrapper.Common
{
    public class AveAuthenticationUtility
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private static object mLock = new object();
        private static IAveWebApplication mCurrentWebApplication;

        public static void InitAuthenticationProvider(IAveWebApplication webApp)
        {
            mLog.Info("Init Authentication Provider.");
            if (webApp == null || mCurrentWebApplication == webApp)
            {
                return;
            }
            lock (mLock)
            {
                mCurrentWebApplication = webApp;
                foreach (IAveIisSettings setting in webApp.IisSettings.Values)
                {
                    mLog.Info(ToString(setting));
                    if (setting.FormsClaimsAuthenticationProvider != null)
                    {
                        string configPath = Path.Combine(setting.Path.FullName, "Web.config");
                        AveFBAUtility.InitProvider(configPath);
                    }
                }
            }
        }

        private static string ToString(IAveIisSettings setting)
        {
            StringBuilder sb = new StringBuilder(100);
            sb.AppendFormat("AllowAnonymous:{0}. ", setting.AllowAnonymous);
            sb.AppendFormat("AuthenticationMode:{0}. ", setting.AuthenticationMode);
           
             sb.AppendFormat("ClaimsAuthenticationRedirectionUrl:{0}. ", setting.ClaimsAuthenticationRedirectionUrl);
            if (setting.FormsClaimsAuthenticationProvider != null)
            {
                sb.AppendFormat("FormsClaimsAuthenticationProvider MembershipProvider:{0}. ", setting.FormsClaimsAuthenticationProvider.MembershipProvider);
                sb.AppendFormat("FormsClaimsAuthenticationProvider RoleProvider:{0}. ", setting.FormsClaimsAuthenticationProvider.RoleProvider);
            }
            sb.AppendFormat("WindowsClaimsAuthenticationProvider:{0}. ", setting);
            sb.AppendFormat("UseBasicAuthentication:{0}. ", setting.UseBasicAuthentication);
            sb.AppendFormat("UseClaimsAuthentication:{0}. ", setting.UseClaimsAuthentication);
            
            sb.AppendFormat("UseTrustedClaimsAuthenticationProvider:{0}. ", setting.UseTrustedClaimsAuthenticationProvider);
            sb.AppendFormat("UseWindowsClaimsAuthenticationProvider:{0}. ", setting.UseWindowsClaimsAuthenticationProvider);
            sb.AppendFormat("UseWindowsIntegratedAuthentication:{0}. ", setting.UseWindowsIntegratedAuthentication);
           
            if (setting.ClaimsAuthenticationProviders != null)
            {
                sb.Append("ClaimsAuthenticationProviders:[");
                foreach (var provider in setting.ClaimsAuthenticationProviders)
                {
                    sb.Append(provider.ToString());//.ClaimProviderName + ":" + provider.DisplayName);
                    sb.Append(",");
                }
                sb.Append("]. ");
            }
            //if (setting.ClaimsProviders != null)
            //{
            //    sb.Append("ClaimsProviders:[");
            //    foreach (var provider in setting.ClaimsProviders)
            //    {
            //        sb.Append(provider);
            //        sb.Append(",");
            //    }
            //    sb.Append("]. ");
            //}
            return sb.ToString();
        }

        public static List<string> GetFBAProviders(IAveWebApplication webapp)
        {
            var providers = new List<string>();
            foreach (IAveIisSettings setting in webapp.IisSettings.Values)
            {
                if (setting.FormsClaimsAuthenticationProvider != null)
                {
                    providers.Add(setting.FormsClaimsAuthenticationProvider.MembershipProvider);
                    if (!string.IsNullOrEmpty(setting.FormsClaimsAuthenticationProvider.RoleProvider))
                    {
                        providers.Add(setting.FormsClaimsAuthenticationProvider.RoleProvider);
                    }
                }
            }
            return providers;
        }
    }
}
