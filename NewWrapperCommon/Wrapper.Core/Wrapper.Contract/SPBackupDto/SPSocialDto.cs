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
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.Wrapper.Core.SPBackupDto
{
    /// <summary>
    /// including social info and comments
    /// 
    /// 目前外围的restore没有使用socialFeed，以后要使用再在SocialDto中添加
    /// </summary>
    public class SPSocialDto
    {
        /// <summary>
        /// Social comments
        /// </summary>
        public List<AveSocialCommentInfo> Comments { get; set; }

        /// <summary>
        /// Social tags
        /// </summary>
        public List<AveSocialTagInfo> Tags { get; set; }

        /// <summary>
        /// Single social feed
        /// </summary>
        public AveSocialFeedInfo Feed { get; set; }
    }
}
