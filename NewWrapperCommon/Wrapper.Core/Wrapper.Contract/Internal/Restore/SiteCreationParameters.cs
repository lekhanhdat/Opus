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

namespace AvePoint.Wrapper.Core.Internal.Restore
{
    /// <summary>
    /// Site Crea
    /// </summary>
    public sealed class SiteCreationParameters
    {
        /// <summary>
        /// Site Collection URL
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Web Application URL
        /// </summary>
        public string WebApplicationUrl { get; set; }

        /// <summary>
        /// Is Host header
        /// </summary>
        public bool IsHostHeader { get; set; }

        /// <summary>
        /// Template Name
        /// </summary>
        public string Template { get; set; }

        /// <summary>
        /// LCID
        /// </summary>
        public uint LCID { get; set; }

        /// <summary>
        /// Owner Login
        /// </summary>
        public string OwnerLogin { get; set; }

        /// <summary>
        /// Owner Name
        /// </summary>
        public string OwnerName { get; set; }

        /// <summary>
        /// Owner Email
        /// </summary>
        public string OwnerEmail { get; set; }

        /// <summary>
        /// Secondary Contact Login
        /// </summary>
        public string SecondaryContactLogin { get; set; }

        /// <summary>
        /// Secondary Contact Name
        /// </summary>
        public string SecondaryContactName { get; set; }

        /// <summary>
        /// Secondary Contact Email
        /// </summary>
        public string SecondaryContactEmail { get; set; }

        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Compatibility Level
        /// </summary>
        public int CompatibilityLevel { get; set; }

        /// <summary>
        /// ContentDBId
        /// </summary>
        public Guid ContentDBId { get; set; }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendFormat(
                "WebApplication:{0}, URL:{1}, Title:{2}, Description:{3}, Template:{4}, LCID:{5}, CompatibilityLevel:{6}, IsHostHeader:{7}, OwnerLogin:{8}, OwnerName:{9}, OwnerEmail:{10}, SecondaryContactLogin:{11}, SecondaryContactName:{12}, SecondaryContactEmail:{13}, ContentDBId:{14}",
                WebApplicationUrl, Url, Title, Description, Template, LCID, CompatibilityLevel,
                IsHostHeader, OwnerLogin, OwnerName, OwnerEmail, SecondaryContactLogin,
                SecondaryContactName, SecondaryContactEmail, ContentDBId);

            return builder.ToString();
        }
    }
}
