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





namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{

    #region using directives
    using System;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAWebApplicationResourceThrottlingOperation : CAOperation
    {
        [DataMember]
        public String WebAppUrl { get; set; }

        [DataMember]
        public UInt32 MaxItemsPerThrottledOperation { get; set; }

        [DataMember]
        public Boolean AllowOMCodeOverrideThrottleSettings { get; set; }

        [DataMember]
        public UInt32 MaxItemsPerThrottledOperationOverride { get; set; }

        [DataMember]
        public UInt32 MaxQueryLookupFields { get; set; }

        [DataMember]
        public Boolean UnthrottledPrivilegedOperationWindowEnabled { get; set; }

        [DataMember]
        public DailyUnthrottledPrivilegedOperationWindowInfo DailyUnthrottledPrivilegedOperationWindowInfo { get; set; }
        
        [DataMember]
        public UInt32 MaxUniquePermScopesPerList { get; set; }

        [DataMember]
        public Boolean EventHandlersEnabled  { get; set; }

        [DataMember]
        public Boolean HttpThrottleSettingsPerformThrottle { get; set; }

        [DataMember]
        public Boolean ChangeLogExpirationEnabled  { get; set; }

        [DataMember]
        public Int32 ChangeLogRetentionPeriod  { get; set; }
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DailyUnthrottledPrivilegedOperationWindowInfo
    {
        //Summary
        //this property valid scope is 0-24
        [DataMember]
        public UInt32 StartHour { get; set; }

        //Summary
        //this property valid scope is 0-3
        [DataMember]
        public UInt32 StartMinute { get; set; }

        //Summary
        //The valid scope of this value is 0-24
        [DataMember]
        public UInt32 Duration { get; set; }
    }
}
