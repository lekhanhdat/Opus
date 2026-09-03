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

namespace Microsoft365Backup.DataBuilder.TeamHtml
{
    using System.Collections.Generic;

    /// <summary>
    /// Team Channel Conversation中的一条topic或者reply.
    /// </summary>
    public abstract class ConversationItem
    {
        /// <summary>
        /// 发布者
        /// </summary>
        public string PostedBy { get; set; }
        /// <summary>
        /// 发布时间
        /// </summary>
        public string PostedTime { get; set; }

        /// <summary>
        /// 内容，Html格式
        /// </summary>
        public string Body { get; set; }

        public bool Important { get; set; }

        public string Type { get; set; }

        public Dictionary<string, string> HostedContents { get; set; }

        public string Reaction {  get; set; }

        public abstract string ToHtmlString();
    }
}