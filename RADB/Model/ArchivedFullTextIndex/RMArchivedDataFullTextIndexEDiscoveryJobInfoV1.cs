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
using Cloud.Sdk.Data.EDiscovery;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvePoint.RA.DB.Model.ArchivedFullTextIndex;

public class RMArchivedDataFullTextIndexEDiscoveryJobInfoesV1
{
    [Key]
    [Column(TypeName = "bigint")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Column(TypeName = "bigint")]
    public long FullTextIndexJobId { get; set; }

    [Column(TypeName = "nvarchar(Max)")]
    public string EDiscoveryJobId { get; set; }

    [Column(TypeName = "int")]
    public IndexType IndexType { get; set; }

    [Column(TypeName = "int")]
    public JobState EDiscoveryJobState { get; set; }

    [Column(TypeName = "int")]
    public int EDiscoveryErrorCode { get; set; }

    [Column(TypeName = "bigint")]
    public long StartTime { get; set; }

    [Column(TypeName = "bigint")]
    public long EndTime { get; set; }
}
