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


namespace AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineRestore.Object
{
    #region == using directives ==
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.Common;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    #endregion ==

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ExchangeOnlineRestoreJobDto : BaseJobDto 
    {
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string BackupJobId { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string BackupCycleId { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_3)]
        public string BackupPlanId { get; set; }

        /// <summary> 该属性取值于SiteMasterIndex表记录的StoragePolicyId，主要用来在跑Restore job时，找available media service。 </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_4)]
        public string StoragePolicyId { get; set; }

        /// <summary> 该属性取值于SiteMasterIndex表记录的LogicalDevice信息。 </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_5)]
        public string LogicalDeviceId { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.LONG_1)]
        public long BackupTime { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_1)]
        public EORestoreType RestoreType { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ExchangeOnlineRestoreJobParams
    {
        [DataMember]
        public ExchangeOnlineRestorePlanDto PlanInfo { get; set; }

        [DataMember]
        public string TimeZoneId { get; set; }

        [DataMember]
        public long SkipTimeUTC { get; set; }

        [DataMember]
        public PlanCategory Category { get; set; }    
    }
}
