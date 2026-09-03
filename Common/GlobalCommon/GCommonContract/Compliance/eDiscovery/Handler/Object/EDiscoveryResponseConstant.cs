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

namespace AvePoint.GCommon.Contract.Compliance.eDiscovery.Handler.Object
{
    /// <summary>
    /// 此类用于标记在Response的Attribute Map 中 Key 值.
    /// </summary>
    public class EDiscoveryResponseConstant
    {
        /// <summary>
        /// Search Location Test 的操作结果.
        /// </summary>
        public static readonly string ATTRIBUTE_SEARCH_LOCATION_TEST_RESULT = "SearchLocationTestResult";

        /// <summary>
        /// Sync File Setting 的保存结果.
        /// </summary>
        public static readonly string ATTRIBUTE_SYNC_FILE_SETTING_SAVE_RESULT = "SyncFileSettingSaveResult";
    }

    /// <summary>
    /// 理想的情况下可用于Sync File/Sync Hold Name 时保存Setting的操作状态.
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SaveSyncSettingResult
    {
        [EnumMember]
        Success = 0, //保存成功.
        [EnumMember]
        Failed = 1  //保存失败(逻辑或者DB操作失误会导致).
    }
}
