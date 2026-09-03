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
using System.Reflection;
using LOGRESX = AutoInstallation.Records.App.Resources.LogResource;

namespace AutoInstallationCommon.Utility.Handler
{
    public class VersionHandler
    {
        private static readonly AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public static bool IsNewerVersion(string old, string now, char[] format)
        {
            var retValue = false;
            try
            {
                var olds = old.Split(format);
                var nows = now.Split(format);
                for (var n = 0; n < olds.Length; n++)
                    if (nows.Length > n)
                    {
                        var temp_old = 0;
                        var temp_now = 0;
                        int.TryParse(olds[n], out temp_old);
                        int.TryParse(nows[n], out temp_now);
                        if (temp_old > temp_now)
                        {
                            retValue = false;
                            break;
                        }

                        if (temp_old < temp_now)
                        {
                            retValue = true;
                            break;
                        }
                    }
                    else
                    {
                        retValue = false;
                        ;
                        break;
                    }
            }
            catch (Exception ex)
            {
                retValue = true;
                logger.Warn(LOGRESX.COMMONUTILITYLOG_COMPAREVERSIONERROR, old, now, format.ToString(), ex.ToString());
            }

            return retValue;
        }

        public static bool IsPocVersion(string old, char[] format)
        {
            var retValue = false;
            try
            {
                var olds = old.Split(format);
                if (olds.Length > 3 && Convert.ToInt32(olds[3]) > 0) retValue = true;
            }
            catch (Exception ex)
            {
                retValue = true;
                logger.Warn(LOGRESX.COMMONUTILITYLOG_COMPAREVERSIONERROR, old, "", format.ToString(), ex.ToString());
            }

            return retValue;
        }

        public static bool IsSameVersionExceptLast(string old, string now, char[] format)
        {
            var retValue = false;
            try
            {
                var olds = old.Split(format);
                var nows = now.Split(format);
                if (olds[0] == nows[0] && olds[1] == nows[1] && olds[2] == nows[2] && olds[3] != nows[3] &&
                    Convert.ToInt32(olds[3]) > 0)
                    retValue = true;
                else
                    retValue = false;
            }
            catch (Exception e)
            {
                retValue = false;
                logger.Warn(LOGRESX.COMMONUTILITYLOG_COMPAREVERSIONERROR, old, "", format.ToString(), e.ToString());
            }

            return retValue;
        }

        public static bool IsSameVersion(string old, string now, char[] format)
        {
            var retValue = false;
            try
            {
                var olds = old.Split(format);
                var nows = now.Split(format);
                if (olds[0] == nows[0] && olds[1] == nows[1] && olds[2] == nows[2] && olds[3] == nows[3])
                    retValue = true;
                else
                    retValue = false;
            }
            catch (Exception e)
            {
                retValue = false;
                logger.Warn(LOGRESX.COMMONUTILITYLOG_COMPAREVERSIONERROR, old, "", format.ToString(), e.ToString());
            }

            return retValue;
        }
    }
}