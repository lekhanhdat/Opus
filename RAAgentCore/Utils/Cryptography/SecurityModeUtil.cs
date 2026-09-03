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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using System;
using System.Configuration;
using System.Reflection;

namespace AvePoint.Hybrid.Utility.Cryptography
{

    public enum ControlSecurityMode
    {
        EncryptMessage = 0, //默认加密模式
        None = 1,
    }

    public class SecurityModeUtil
    {
        private static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static ControlSecurityMode securityMode = ControlSecurityMode.EncryptMessage;
        private static bool isShowProductVersion = true;


        public static ControlSecurityMode SecurityMode
        {
            get { return securityMode; }
        }

        public static bool IsShowProductVersion
        {
            get { return isShowProductVersion; }
        }
        public static void InitControlSecurityMode()
        {
            try
            {
                GetSecurityModeFromConfig();
            }
            catch (Exception e)
            {
                logger.Warn(string.Format("Init SecurityMode Error, Exception:{0}", e.ToString()));
                securityMode = ControlSecurityMode.EncryptMessage;
                isShowProductVersion = true;
            }

        }

        private static void GetSecurityModeFromConfig()
        {
            string securityModeFromConfig = ConfigurationManager.AppSettings["SecurityMode"];
            string showVersionFromConfig = ConfigurationManager.AppSettings["ShowProductVersion"];

            if (!string.IsNullOrEmpty(securityModeFromConfig) && string.Equals("true", securityModeFromConfig, StringComparison.OrdinalIgnoreCase))
            {
                securityMode = ControlSecurityMode.EncryptMessage;
            }
            else
            {
                securityMode = ControlSecurityMode.None;
            }

            if (!string.IsNullOrEmpty(showVersionFromConfig) && string.Equals("true", showVersionFromConfig, StringComparison.OrdinalIgnoreCase))
            {
                isShowProductVersion = true;
            }
            else
            {
                isShowProductVersion = false;
            }


        }
    }
}
