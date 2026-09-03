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

namespace CloudRecordDownloadManager.Utils.Other {

    public static class RandomString {

        public static string Generate(int size = 8) {
            if (size <= 0) return string.Empty;

            var random = new Random();
            var str = string.Empty;
            for (var i = 8 - 1; i >= 0; i--)
                switch (random.Next(3)) {
                    case 0:
                        str += (char) ('a' + random.Next(26));
                        continue;
                    case 1:
                        str += (char) ('A' + random.Next(26));
                        continue;
                    case 2:
                        str += (char) ('0' + random.Next(10));
                        continue;
                }

            return str;
        }

    }

}