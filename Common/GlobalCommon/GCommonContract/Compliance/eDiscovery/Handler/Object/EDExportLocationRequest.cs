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
using AvePoint.GCommon.Contract.Compliance.eDiscovery.Object;

namespace AvePoint.GCommon.Contract.Compliance.eDiscovery.Handler.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EDExportLocationRequest : EDiscoveryRequest
    {
        [DataMember]
        public ExportLocationType LocationType { get; set; }
        [DataMember]
        public HandleAction Action { get; set; }
        [DataMember]
        public EDExportLocationDto LocationDto { get; set; }

        [DataMember]
        public List<EDExportLocationDto> NeedDeleteExportLocationList { get; set; }

        /// <summary>
        /// Test操作用到的参数
        /// true : 如果检测到路径不存在，那么报错
        /// flase ：如果检测到路径不存在，直接创建
        /// </summary>
        [DataMember]
        public bool IsCreateFolder { get; set; }

        [DataContract(Namespace = ContractConstants.Namespace)]
        public enum HandleAction
        {
            [EnumMember]
            None = 0,
            [EnumMember]
            Save = 1,
            [EnumMember]
            Load = 2,
            [EnumMember]
            HasExisted = 3,
            [EnumMember]
            Test = 4,
            [EnumMember]
            Delete = 5,
            [EnumMember]
            CheckBeUsed = 6
        }

        [DataContract(Namespace = ContractConstants.Namespace)]
        public enum ExportLocationType
        {
            [EnumMember]
            None = 0,
            [EnumMember]
            SearchResultLocation = 1,
            [EnumMember]
            DownloadLocation =2,
            [EnumMember]
            Both = 3
        }
    }



}
