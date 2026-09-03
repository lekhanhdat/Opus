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



namespace AvePoint.GCommon.Contract.ReportCenter.Object
{
    using System;

    public class AuditSiteCollectionIndexInfo
    {


        /// <summary>
        /// 如果没设置id，则以Hash(FarmName + WebAppUrl + SiteUrl)为id
        /// </summary>
        public string Id { get;set; }
        public string Key
        {
            get
            {
                return FarmName + WebAppUrl + SiteUrl;
            }
        }
        public string FarmName { get; set; }
        public string WebAppUrl { get; set; }
        public string SiteId { get; set; }
     
        public string SiteUrl { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public long LastUpdateTimeTicks { get; set; }

        public override string ToString()
        {
            return string.Format("AuditSiteCollectionIndexInfo[FarmName {0}, WebAppUrl {1}, SiteUrl {2}]", 
                FarmName, WebAppUrl, SiteUrl);
        }
    }

    public class AuditViewItemIndex
    {

        /// <summary>
        /// 如果没设置id，则以Hash(WebAppUrl + SiteUrl)为id
        /// </summary>
        public string Id { get; set; }
        public string Key
        {
            get
            {
                return WebAppUrl + SiteUrl;
            }
        }
        public string FarmName { get; set; }
        public string WebAppUrl { get; set; }
        public string SiteId { get; set; }
        public string SiteUrl { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public long LastUpdateTimeTicks { get; set; }

        public override string ToString()
        {
            return string.Format("AuditViewItemIndex[WebAppUrl {0}, SiteUrl {1}]", WebAppUrl, SiteUrl);
        }
    }
}
