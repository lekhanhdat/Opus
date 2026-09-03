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





using System.Collections.Generic;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Replicator.Object.ProfileContents;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Storage.Entity;

namespace AvePoint.GCommon.Contract.Replicator.Object.OperationResults
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ProfileOperationResult : ReplicatorOperationResult
    {
        public ProfileOperationResult()
            : base(false, null)
        {

        }

        public ProfileOperationResult(bool hasError, ReplicatorOperationResultError exception)
            : base(hasError, exception)
        {

        }
        public static ProfileOperationResult Empty = new ProfileOperationResult();

        [DataMember]
        public ProfileDto Profile { get; set; }

        [DataMember]
        public List<ProfileDto> Profiles { get; set; }

        [DataMember]
        public Dictionary<ProfileType, List<ProfileDto>> ProfilesByType { get; set; }

        [DataMember]
        public List<FarmConfigDBInfo> FarmConfigDBInfos { get; set; }

        [DataMember]
        public List<FarmByteLevelInfo> FarmByteLevelInfos { get; set; }

        [DataMember]
        public Dictionary<string, ReplicatorConfigDBContent> DefaultConfigDB { get; set; }

        [DataMember]
        public List<StoragePolicyDto> StoragePolicySummaries { get; set; }

        [DataMember]
        public bool Exist { get; set; }

        [DataMember]
        public ReplicatorConfigDBContent ConfigDbContent { get; set; }

        [DataMember]
        public bool CacheDBUsedByMultiFarms { get; set; }

        [DataMember]
        public List<ReplicatorPlan> RunningPlans { get; set; }
    }
}
