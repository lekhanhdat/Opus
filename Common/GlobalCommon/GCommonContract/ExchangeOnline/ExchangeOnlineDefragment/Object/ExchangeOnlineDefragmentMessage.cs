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

namespace AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineDefragment.Object
{
    #region == using directives ==
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Media.TCPRequest.Backup;
    using AvePoint.GCommon.Contract.Media.TCPRequest.Restore;
    using AvePoint.GCommon.Contract.Storage.Entity;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ExchangeOnlineDefragmentMessage : AveMessage
    {
        [DataMember]
        public string PlanId { get; set; }

        [DataMember]
        public string JobId { get; set; }

        [DataMember]
        public ExchangeRestoreRequest RestoreForMedia { get; set; }

        [DataMember]
        public ExchangeBackupRequest BackupForMedia { get; set; }

        /// <summary>
        /// Defragment需要Update的Index所在的LogicalDevice
        /// </summary>
        [DataMember]
        public Dictionary<string, LogicalDeviceDto> LogicalDeviceForUpdatingIndex { get; set; }

        /// <summary>
        /// Exist FB Job Id
        /// </summary>
        [DataMember]
        public List<string> FBJobIds { get; set; }

        /// <summary>
        /// Delete FB & IB
        /// </summary>
        [DataMember]
        public Dictionary<String, Dictionary<String, String>> IndexStorageInfoMap { get; set; }

        /// <summary>
        /// Current Cycle StorageInfo
        /// </summary>
        [DataMember]
        public Dictionary<String, Dictionary<String, String>> CurrentCycleIndexStorageInfo { get; set; }

    }


}
