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
using AvePoint.GCommon.Contract.Compliance.eDiscovery.Object.HoldManager;

namespace AvePoint.GCommon.Contract.Compliance.eDiscovery.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EDSyncFilePlanDto : EDSyncItemPlanDto
    {
        public static readonly string IDPREFIX = "EDSYNC_";

        [DataMember]
        public RealTimeSyncStatus RealTimeSyncStatus { get; set; }

        [DataMember]
        public SyncType SyncType { get; set; }

        #region 从job中取出的last sync time,带着job中的time zone
        [DataMember]
        public long LastSyncTime { get; set; }

        [DataMember]
        public string TimeZoneId { get; set; }
        #endregion

        /// <summary>
        /// 生成ID
        /// </summary>
        /// <param name="farmId"></param>
        /// <param name="webappId"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public static string GenerateId(string farmId, string webappId)
        {
            return IDPREFIX + farmId + webappId;
        }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RealTimeSyncStatus
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Enabled = 1,
        [EnumMember]
        Disabled = 2
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum HandleRealTimeStatus
    {
        [EnumMember]
        Failed = 0,
        [EnumMember]
        Successful = 1
    }
}
