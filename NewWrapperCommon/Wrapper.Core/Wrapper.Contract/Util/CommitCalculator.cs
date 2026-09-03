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
using AvePoint.Wrapper.Core.Common;

namespace AvePoint.Wrapper.Core.Util
{
    internal class CommitCalculator
    {
        private int currentSize;
        private int maxCacheCount;

        public CommitCalculator(int maxCount)
        {
            maxCacheCount = maxCount;
        }

        internal bool NeedCommit(bool finallyTime)
        {
            var result = false;
            if (finallyTime)
            {
                result = currentSize != 0;
            }
            else
            {
                result = ++currentSize >= maxCacheCount;
            }
            if (result)
            {
                currentSize = 0;
            }
            return result;
        }
    }
}