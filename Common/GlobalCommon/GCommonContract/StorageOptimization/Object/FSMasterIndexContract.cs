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
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.FullTextIndex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.GCommon.Contract.StorageOptimization.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FSMasterIndexContract
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string PlanId { get; set; }
        [DataMember]
        public string JobId { get; set; }

        [DataMember]
        public string ConnectionName { get; set; }

        [DataMember]
        public long ArchiverTime { get; set; }

        [DataMember]
        public string FarmName { get; set; }

        [DataMember]
        public string ConnectionId { get; set; }
        [DataMember]
        public string AgentId { get; set; }

        [DataMember]
        public int JobState { get; set; }

        //[DataMember]
        //public int FullTextIndexState { get; set; }

        [DataMember]
        public string StoragePolicyId { get; set; }  //转到子表中

        [DataMember]
        public string MediaServiceId { get; set; } //子表

        [DataMember]
        public string IndexDeviceId { get; set; }

        [DataMember]
        public string FarmId { get; set; }

        [DataMember]
        public int SPVersion { get; set; }

        [DataMember]
        public string RuleId { set; get; }

        [DataMember]
        public int SourceFlag { set; get; }

        [DataMember]
        public IndexModule Module { set; get; }
        //[DataMember]
        //public string StorageInfo { get; set; }

        //[DataMember]
        //public string Crc32 { get; set; }

        [DataMember]
        public ArchiverSiteMasterIndexExtension Extension { get; set; }

        [DataMember]
        public MergeIndexState MergeIndexState { get; set; }

        [DataMember]
        public SiteJobLockEnum SiteJobLockEnum { get; set; }

        [DataMember]
        public string LockedJobId { get; set; }

        [DataMember]
        public VersionDetails VersionDetails { get; set; }

        [DataMember]
        public List<ArchiverIndexSubInfoContract> SubInfo { get; set; }

        [DataMember]
        public string StorageInfo { get; set; }

        #region properties for compliace module
        [DataMember]
        public CrawlStatus CrawlStatus { set; get; }
        [DataMember]
        public CrawlIndexStatus CrawlTreatedStatus { set; get; }
        [DataMember]
        public string CrawlProfileId { set; get; }
        [DataMember]
        public string CrawlDeviceId { set; get; }
        #endregion

        [DataMember]
        public bool DAOMigrated { set; get; }

        [DataMember]
        public int BackupFileType { get; set; }

        [DataMember]
        public int DuplicateStatus { get; set; }
    }



}
