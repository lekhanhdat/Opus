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



using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.StorageOptimization.Object
{
    /// <summary>
    /// Docave 5 ArchiverSiteMaster 数据; 用于数据升级Media Tool;
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ArchiverSiteMasterIndexVersionFive
    {
        [DataMember]
        public string Id { set; get; }
        [DataMember]
        public long BackupTime { set; get; }
        [DataMember]
        public string CycleId { set; get; }
        [DataMember]
        public string PhysicalDrive { set; get; }
        [DataMember]
        public string FarmName { set; get; }
        [DataMember]
        public int PlanType { set; get; }
        [DataMember]
        public string JobId { set; get; }
        [DataMember]
        public string LogicalDrive { set; get; }
        [DataMember]
        public string WebappName { set; get; }
        [DataMember]
        public string SiteUrl { set; get; }
        [DataMember]
        public string PlanId { set; get; }
        [DataMember]
        public int SiteUrlStatus { set; get; }
        [DataMember]
        public string RetentionXml { set; get; }
        [DataMember]
        public string SiteInfo { set; get; }
        [DataMember]
        public string Mark1 { set; get; }
        [DataMember]
        public string Mark2 { set; get; }
        [DataMember]
        public string Mark3 { set; get; }
        [DataMember]
        public int Mark4 { set; get; }
        [DataMember]
        public long Mark5 { set; get; }
        [DataMember]
        public string ClipId { set; get; }
        [DataMember]
        public int ImportData { set; get; }
        [DataMember]
        public string CRC32 { set; get; }
        [DataMember]
        public string ImportJobId { set; get; }
        [DataMember]
        public string ImportPlanId { set; get; }
        [DataMember]
        public int IndexState { set; get; }
        [DataMember]
        public string StoragePolicyID { set; get; }
    }
}
