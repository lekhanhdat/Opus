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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvePoint.RA.DB.Model
{
    /// <summary>
    /// This table is used to store the job info has Failed or Finished with exception status which is related to the file system connection.
    /// </summary>
    public class FSConnectionRelatedJobInfo : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(TypeName = "int")]
        public int Id { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(512)]
        public string FolderPath { get; set; }

        [Index("IX_FSConnRelatedJob_ConnectionId_JobId", 1)]
        [Column(TypeName = "uniqueidentifier")]
        public Guid ConnectionId { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(512)]
        public string ConnectionPath { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(256)]
        public string ConnectionGroupName { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid ConnectionGroupId { get; set; }

        [Index("IX_FSConnRelatedJob_ConnectionId_JobId", 2)]
        [Column(TypeName = "nvarchar")]
        [MaxLength(64)]
        public string JobId { get; set; } //main job id

        [Column(TypeName = "int")]
        public int JobType { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(256)]
        public string JobRunBy { get; set; }

        [Column(TypeName = "int")]
        public int Status { get; set; } //sub job status

        [Column(TypeName = "nvarchar(max)")]
        public string Comment { get; set; }

        [Column(TypeName = "bigint")]
        public long StartTime { get; set; }

        [Column(TypeName = "bigint")]
        public long EndTime { get; set; }
    }
}