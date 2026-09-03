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


using System.Collections.Generic;
using System.Text;

namespace AvePoint.GCommon.Utility.Exceptions
{
    class Util
    {
        public static string ParseList(List<string> items)
        {
            StringBuilder obj = new StringBuilder();
            foreach (string item in items)
            {
                obj.Append(item).Append(";");
            }
            return obj.ToString();
        }

        public static string MillisecondToSecond(int millisecondTimeout)
        {
            double secondTimeout = (double)millisecondTimeout / 1000;
            return secondTimeout.ToString();
        }

        public static string ByteToMB(double byteSpace)
        {
            double mbSpace = byteSpace / 1024 / 1024;
            return mbSpace.ToString();
        }
    }
}
