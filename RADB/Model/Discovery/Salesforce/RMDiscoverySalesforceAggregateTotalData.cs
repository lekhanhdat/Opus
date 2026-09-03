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
    [Table("RMSalesforceAggregateTotalData")]
    public class RMDiscoverySalesforceAggregateTotalData : RMDiscoveryDBTable
    {
        [Key]
        [Column(TypeName = "int")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column(TypeName = "nvarchar")]
        public string OrgId { get; set; }

        [Column(TypeName = "nvarchar")]
        public string OrgName { get; set; }

        [Column(TypeName = "int")]
        public int ObjectTotalCount { get; set; }

        [Column(TypeName = "bigint")]
        public long RecordsTotalCount { get; set; }//??Records Number + file Number, to do confirm.

        [Column(TypeName = "datetime")]
        public DateTime OldestRecordsCreatedTime { get; set; }

        [Column(TypeName = "bigint")]
        public long DataTotalSize { get; set; }

        [Column(TypeName = "bigint")]
        public long DataStorageLimit { get; set; }

        [Column(TypeName = "bigint")]
        public long FileTotalSize { get; set; }

        [Column(TypeName = "bigint")]
        public long FileStorageLimit { get; set; }
   
        [Column(TypeName = "nvarchar")]
        public string BiggestObjectByDataSize { get; set; }
        [Column(TypeName = "nvarchar")]
        public string BiggestObjectByFileSize { get; set; }
        [Column(TypeName = "nvarchar")]
        public string BiggestObjectByRecordCount { get; set; }//??File ? Records

    }
}
