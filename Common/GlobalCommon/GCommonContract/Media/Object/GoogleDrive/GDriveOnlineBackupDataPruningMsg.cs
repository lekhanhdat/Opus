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
using System.Collections.Generic;
using AvePoint.GCommon.Contract.Storage.Entity;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Media.Object.GDrive
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class GDriveOnlineBackupDataPruningMsg
    {
        [DataMember]
        public Boolean IsCycle { get; set; }

        [DataMember]
        public List<String> EmailAddressList { get; set; }

        [DataMember]
        public List<String> PruningJobs { get; set; }

        //[DataMember]
        //public EOSiteMasterIndexDto PruningCycle { get; set; }

        [DataMember]
        public BackupRetentionRule RetentionRule { get; set; }

        [DataMember]
        public Dictionary<String, String> StoragInfoMap { get; set; }

        [DataMember]
        public Boolean NeedDeleteJobAndData { get; set; }

        [DataMember]
        public PruningAction PruningAction { get; set; }

        [DataMember]
        public String RetentionJobId { get; set; }

        [DataMember]
        public string RetentionSubJobId { get; set; }

        [DataMember]
        public string BackupPlanId { get; set; }

        [DataMember]
        public Boolean DeletePlan { get; set; }

        [DataMember]
        public RetentionType OperationType { get; set; }

        [DataMember]
        public List<string> PhysicalDeviceIds { get; set; }
    }
}
