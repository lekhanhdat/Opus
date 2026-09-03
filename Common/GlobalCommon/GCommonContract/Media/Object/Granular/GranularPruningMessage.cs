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


namespace AvePoint.GCommon.Contract.Media.Object
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using AvePoint.GCommon.Contract.GranularBackup.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class GranularPruningMessage
    {
        #region ==GranularBackupDataPruningDto==
        
        [DataMember]
        public Boolean IsCycle { get; set; }

        [DataMember]
        public List<string> SiteUrls { get; set; }

        [DataMember]
        public List<string> PruningJobs { get; set; }

        [DataMember]
        public SiteMasterIndexDto PruningCycle { get; set; }

        [DataMember]
        public BackupRetentionRule RetentionRule { get; set; }

        [DataMember]
        public Dictionary<string, string> StoragInfoMap { get; set; }
        #endregion ==GranularBackupDataPruningDto==

        [DataMember]
        public string PlanId { get; set; }

        [DataMember]
        public string LogicalDeviceId { get; set; }

        [DataMember]
        public string RetentionJobId { get; set; }

        [DataMember]
        public string RetentionSubJobId { get; set; }

        [DataMember]
        public PruningAction PruningAction { get; set; }

        [DataMember]
        public List<string> PhysicalDeviceIds { get; set; }

        [DataMember]
        public bool DeletePlanData { get; set; }

        [DataMember]
        public RetentionType OperationType { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PruningAction
    {
        [EnumMember]
        None = 0,

        //run job
        [EnumMember]
        Prunedata = 1,

        [EnumMember]
        //delete in job moniotr
        DeleteJobAndData,
    }
}
