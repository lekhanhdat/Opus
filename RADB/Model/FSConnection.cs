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
using AvePoint.RA.Contract.Discovery.Job;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Model
{
    public class FSConnection: BaseModel
    {
        [Key]
        [Column(TypeName = "uniqueidentifier")]
        public Guid Id { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid GroupId { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        public string Name { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string Description { get; set; }

        [Column(TypeName = "bigint")]
        public long LastModifiedTime { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string UNCPath { get; set; }

        //[Column(TypeName = "uniqueidentifier")]
        //public Guid AgentGroupId { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        public string AgentId { get; set; }

        [Column(TypeName = "nvarchar")]
        public string JPMCConnectionId { get; set; }

        [NotMapped]
        public string GroupName { get; set; }

        //provide to discovery Job
        [NotMapped]
        public RMDiscoveryJobFailedCause FailedCause { get; set; }

        [Column(TypeName = "int")]
        public int PathType { get; set; }

        #region JPMC
        [Column(TypeName = "bigint")]
        public long LastSyncTime { get; set; }

        [NotMapped]
        [Column(TypeName = "int")]
        public int FailureJobCount { get; set; }

        [Column(TypeName = "int")]
        public int IsPause { get; set; }

        #endregion
    }
}
