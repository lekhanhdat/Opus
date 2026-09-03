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
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using AvePoint.GCommon;

namespace AvePoint.Wrapper.Common
{
    public static class AveUserAndGroupUtilty
    {
        static public List<string> fakeUsers = new List<string>();

        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private static object obj = new object();

        static public IAveUser EnsureAvailableUser(this IAveWeb web, string logonName,bool needCheck = true)
        {
            lock (obj)//支持 Replicator 多线程
            {
                if (needCheck && fakeUsers.Contains(logonName.ToLower(CultureInfo.CurrentCulture)))
                {
                    throw new AveFakeUserException(AveInternalResourceKey.Wrapper_Exception_Common_FakeUserException, logonName);
                }
                try
                {
                    return web.EnsureUser(logonName);
                }
                catch (Exception e)
                {
                    logger.Debug("An error occurred while ensure user {0}, error message:{1}", logonName, e.ToString());
                    if (!fakeUsers.Contains(logonName.ToLower(CultureInfo.CurrentCulture)))
                    {
                        fakeUsers.Add(logonName.ToLower(CultureInfo.CurrentCulture));
                    }
                    throw;
                }
            }
        }
    }
}
