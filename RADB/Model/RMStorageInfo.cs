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
using AvePoint.RA.Contract.Task;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Model
{
    public class RMStorageDeviceInfo: BaseModel
    {
        [Key]
        [Column(TypeName = "uniqueidentifier", Order = 1)]
        public Guid Id { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        public string Name { get; set; }

        [Column(TypeName = "int")]
        public int Type { get; set; }

        [Column(TypeName = "nvarchar")]
        public string DriveInfo { get; set; }

        [Column(TypeName = "nvarchar")]
        public string InfoExtension { get; set; }

        [Column(TypeName = "bigint")]
        public long ModifiedTime { set; get; }

        [Column(TypeName = "int")]
        public int Status { get; set; }

        [Column(TypeName = "nvarchar(MAX)")]
        public string ConnectionString { get; set; }

        //TODO How to move this from DAO(physical mapping to multiple storage policy)
        [Column(TypeName = "nvarchar(MAX)")]
        public string Retention { get; set; }

        [Column(TypeName = "bigint")]
        public long RetentionNextTime { set; get; }

        [Column(TypeName = "nvarchar")]
        public string Notification { get; set; }
        [Column(TypeName = "bigint")]
        public long LastModifiedTime { set; get; }
        [Column(TypeName = "bigint")]
        public long LastArchivedTime { set; get; }

        [Column(TypeName = "bit")]
        public bool IsSystemStorage { get; set; }

        [Column(TypeName = "bit")]
        public bool? DAOMigrated { set; get; }
        [Column(TypeName = "nvarchar")]
        public string DAOStoragePolicyId { get; set; }
        [Column(TypeName = "nvarchar")]
        public string DAOLogicalDeviceId { get; set; }
        [Column(TypeName = "nvarchar")]
        public string DAOPhysicalDeviceId { get; set; }


    }

    public enum StorageStatus
    {
        UsedStorage = 0,
        OldStorage = 1,
    }
    public enum IndexDeviceUsed
    {
        Used = 0,
        Not = 1,
    }
    public enum CreateOrEditStatus
    {
        Success = 0,
        Failed = 1,
    }

    //用于二进制计算，int值必须为2的n次方
    [System.Flags]
    public enum StorageFlag
    {
        //2^0 不允许删除
        NotAllowedDelete = 1,
        //2^1 不允许修改非Retention settings
        NotAllowedEditSettings = 2,
    }
}
