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




namespace AvePoint.GCommon.Contract.Storage.Entity
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using System.Xml;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ArchiverJobDto
    {
        //private static final long serialVersionUID = 5966371848924752622L;
        [DataMember]
        public String FarmName { get; set; }

        [DataMember]
        public String PlanId { get; set; }

        [DataMember]
        public String JobId { get; set; }

        [DataMember]
        public String ParentJobID { get; set; }

        [DataMember]
        public String WebPath { get; set; }

        [DataMember]
        public String SiteCollection { get; set; }

        [DataMember]
        public String TsmDataVolume { get; set; }

        [DataMember]
        public String IndexVolume { get; set; }

        [DataMember]
        public String DataVolume { get; set; }

        [DataMember]
        public String FullTextVolume { get; set; }

        [DataMember]
        public String ReviewVolume { get; set; }

        [DataMember]
        public String SiteUrl { get; set; }

        [DataMember]
        public String FullPath { get; set; }

        [DataMember]
        public long ArchiveTime { get; set; }

        [DataMember]
        public int NumberPrefix { get; set; }

        [DataMember]
        public long DataFileNumber { get; set; }

        [DataMember]
        public long DataFileOffset { get; set; }

        [DataMember]
        public long DataFileLength { get; set; }

        [DataMember]
        public PhysicalDeviceDto WorkingDriveDto { get; set; }

        [DataMember]
        public LogicalDeviceDto LogicalDriveDto { get; set; }

        [DataMember]
        public LogicalDeviceDto OldLogicalDriveDto { get; set; }

        [DataMember]
        public LogicalDeviceDto SearchLogicalDriveDto { get; set; }

        [DataMember]
        public LogicalDeviceDto IndexLogicalDriveDto { get; set; }

        [DataMember]
        public String DriverPath { get; set; }

        [DataMember]
        public Boolean CreateStub { get; set; }

        [DataMember]
        public int PlanType { get; set; }

        [DataMember]
        public String CacheMD5Folder { get; set; }

        [DataMember]
        public String DataPrefix { get; set; }

        [DataMember]
        public Boolean BackupPerformance { get; set; }

        [DataMember]
        public Boolean NeedCreateIndex { get; set; }

        //[DataMember]
        //public ExtraDriveInfoDto extraDriveInfo { get; set; }

        [DataMember]
        public long ContentOffset { get; set; }

        [DataMember]
        public long ContentLength { get; set; }

        [DataMember]
        public String ClipId { get; set; }

        [DataMember]
        public long KeepTime { get; set; }
    }
}
