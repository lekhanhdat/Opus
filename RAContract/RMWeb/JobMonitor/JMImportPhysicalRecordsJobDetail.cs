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
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb.JobMonitor
{
    public class JMImportPhysicalRecordsJobDetail : JMJobDetails
    {
        public string SrcRecordType { set; get; }
        public string DestRecordType { set; get; }
        /// <summary>
        /// 支持多Template
        /// </summary>
        public string TemplateName { set; get; }

        public string UniqueId { set; get; }
        public string Title { set; get; }
        /// <summary>
        /// 当前REcord的上一导窗口的UniqueId
        /// </summary>
        public string Container { set; get; }
        public string SrcLocation { get; set; }
        /// <summary>
        /// 从Location开始的FullPath
        /// </summary>
        public string LocationFullPath { set; get; }

        public string Barcode { set; get; }

        //[Obsolete]
        //public string PhysicalLibraryUrl { get; set; }
        //[Obsolete]
        //public string ItemType { get; set; }
        //[Obsolete]
        //public string PhysicalFileName { get; set; }
        //[Obsolete]
        //public string PhysicalRecordName { get; set; }
        //[Obsolete]
        //public string BoxName { get; set; }
    }
    public class JMImportRecordsRelatedJobDetail : JMJobDetails
    {
        /// <summary>
        /// Unique Id
        /// </summary>
        public string SrcId { get; set; }
        /// <summary>
        /// Name or Title
        /// </summary>
        public string SrcName { get; set; }
        /// <summary>
        /// Record Type
        /// </summary>
        public string SrcType { get; set; }
        /// <summary>
        /// Location or SiteUrl 
        /// </summary>
        public string SrcLocation { get; set; }

        public string SrcSiteId { set; get; }
        public string SrcItemId { set; get; }
        public string SrcItemUrl { set; get; }
        /// <summary>
        /// Related UniqueId
        /// </summary>
        public string DestName { get; set; }
        public string DestType { get; set; }  
        /// <summary>
        /// SP
        /// </summary>
        public string DestItemId { set; get; }
        /// <summary>
        /// SP
        /// </summary>
        public string DestItemUrl { set; get; }
        /// <summary>
        /// SP
        /// </summary>
        public string DestSiteId { set; get; }
        /// <summary>
        /// SP
        /// </summary>
        public string DestSiteUrl { set; get; }
    }
    
    public class JMImportedPhysicalRecordsDeletionDetail : JMJobDetails
    {
        public string ObjectName { set; get; }
        public string UniqueId { set; get; } 
    }
}