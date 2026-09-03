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
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.ObjectModel.Common
{
    public class FormAuthenticationProvider : IAuthenticationProvider
    {
        private static AveLogger log = AveLogger.GetInstance(typeof(FormAuthenticationProvider));
        public AuthenticationResult Login(string siteUrl, AveBPOSAccountInfo userAccountInfo)
        {
            AuthenticationResult auResult = new AuthenticationResult(AutheStatus.Failed, AveAuthenticationMode.Forms); 
            try
            {
                Uri uri = new Uri(siteUrl + "/_vti_bin/authentication.asmx");

                //不想在此工程引用sharepoint dll，因此用反射处理
                Assembly clientRuntime = System.Reflection.Assembly.LoadFrom(System.IO.Path.GetDirectoryName(typeof(WrapperConfiguration).Assembly.Location) + "\\2013\\Microsoft.SharePoint.Client.Runtime.dll");
                Type spoCredential = clientRuntime.GetType("Microsoft.SharePoint.Client.Application.Authentication");
                MethodInfo method = spoCredential.GetMethod("Login", new Type[] { typeof(string), typeof(string) });
                object spoInstance = Activator.CreateInstance(spoCredential, new object[] { uri });
                AveAssemblyUtility.SetPropertyValue(spoInstance, "Timeout", 1 * 60 * 1000);
                AveAssemblyUtility.SetPropertyValue(spoInstance, "Url", uri.AbsoluteUri);
                AveAssemblyUtility.SetPropertyValue(spoInstance, "CookieContainer", new CookieContainer());
                AveAssemblyUtility.SetPropertyValue(spoInstance, "AllowAutoRedirect", true);
                var result = method.Invoke(spoInstance, new object[] { userAccountInfo.UserName, userAccountInfo.Password });
                var resultCode = (int)AveAssemblyUtility.GetPropertyValue(result, clientRuntime.GetType("Microsoft.SharePoint.Client.Application.LoginResult"), "ErrorCode");
                CookieContainer cookie = null;
                switch (resultCode)
                {
                    case 0:
                        cookie = AveAssemblyUtility.GetPropertyValue(spoInstance, spoCredential, "CookieContainer") as CookieContainer;
                        break;
                    case 1:
                        throw new Exception("ServerNotInFormsAveAuthenticationMode");
                    case 2:
                        throw new Exception("FormsAuthenticationCannotLogin");
                }
                log.Debug("login site {0} successfully using forms authentication", siteUrl);
                auResult = new AuthenticationResult(AutheStatus.Successful, AveAuthenticationMode.Forms, cookie);
            }
            catch(Exception e)
            {
                log.Warn("Login failed by Forms authentication. Url:{0}, user:{1}, Error:{2}", siteUrl, userAccountInfo.UserName, e);
            }
            return auResult;
        }
    }
}
