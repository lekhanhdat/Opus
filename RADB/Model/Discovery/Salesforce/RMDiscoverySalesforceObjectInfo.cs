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
using AvePoint.RA.Contract.Explorer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Model.Discovery.Salesforce
{
    [Table("RMSalesforceObjectInfoData")]
    public class RMDiscoverySalesforceObjectInfo : RMDiscoveryDBTable
    {
        [Key]
        [Column(TypeName = "uniqueidentifier")]
        public Guid Id { get; set; }

        [Column(TypeName = "nvarchar")]
        [Index]
        [MaxLength(300)]
        public string InternalName { get; set;} //APIName

        [Column(TypeName = "nvarchar")]
        [MaxLength(300)]
        public string DisplayName { get; set; } //LabelName

        //enum RMSFObjecType  Data Record or File Record
        //index
        [Column(TypeName = "int")]
        [Index]
        public int ObjectType { get; set; } //StandardObject or CustomObject, FIle or 

        // record = record count
        // file = file count
        [Column(TypeName = "bigint")]
        public long TotalItemCount { get; set; } //confirm? not include file ?

        // record = record count * (2KB * 1024)
        // file = file size
        // unit bit
        [Column(TypeName = "bigint")]
        public long TotalSize { get; set; }
        
        [Column(TypeName = "datetime2")]
        public DateTime? OldestRecordsCreatedTime { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? LatestModifiedTime { get; set; }
    }
}
