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
using AvePoint.Hybrid.Utility.Cryptography;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.Wrapper.Common;

namespace RAFileSystem.FileSystem.Disposal.Utils
{
    internal static class DisposalFilterHelper
    {
        public static bool IsBreakInheritNode(string url)
        {
            string sha1Url = RAEncodeUtil.EncryptBySHA1(url);
            if (FSJobCache.Instance.BreakNodeUrls != null && FSJobCache.Instance.BreakNodeUrls.Contains(sha1Url))
            {
                if (WrapperConfiguration.IsProcessApprovalDatasOnly)
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        public static bool HasRunningJob(string url)
        {
            string sha1Url = RAEncodeUtil.EncryptBySHA1(url);
            if (FSJobCache.Instance.RunningJobNodeUrls != null && FSJobCache.Instance.RunningJobNodeUrls.Contains(sha1Url))
            {
                return true;
            }
            return false;
        }
    }
}

