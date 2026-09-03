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

namespace AvePoint.Media.Storage.Cloud.Common
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Net;
    using System.IO;
    #endregion

    class BasicRequest
    {
        public String URI { get; set; }
        public String UserName { get; set; }
        public String Password { get; set; }
        public Dictionary<String, String> Headers { get; set; }
        public Stream DataStream { get; set; }
        public String Method { get; set; }
        public Boolean KeepAlive { get; set; }

        public override String ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(" Uri:" + this.URI);
            stringBuilder.Append(" Method:" + this.Method);
            stringBuilder.Append(" KeepAlive:" + this.KeepAlive);
            stringBuilder.Append(" Headers:" + this.GetLine(this.Headers));
            return base.ToString();
        }

        String GetLine(Dictionary<String, String> headers)
        {
            var builder = new StringBuilder();
            foreach (var header in headers)
            {
                builder.Append(header.Key).Append(":").Append(header.Value).Append(',');
            }
            return builder.ToString().TrimEnd(',');
        }

    }
}
