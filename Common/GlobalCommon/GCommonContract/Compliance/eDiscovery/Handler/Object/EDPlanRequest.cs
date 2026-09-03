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
    public class EDPlanRequest : EDiscoveryRequest
    {

        #region 必须属性

        [DataMember]
        public ActionEnum Action { get; set; }

        #endregion


        /// <summary>
        /// 执行Save，SaveAndRunNow操作，需要用到此属性
        /// </summary>
        [DataMember]
        public EDPlanDto Plan { get; set; }

        /// <summary>
        /// LoadById操作用到此属性
        /// </summary>
        [DataMember]
        public string PlanId { get; set; }

        /// <summary>
        /// RunNow，Delete操作用到此属性
        /// </summary>
        [DataMember]
        public List<string> PlanIds { get; set; }




        [DataContract(Namespace = ContractConstants.Namespace)]
        public enum ActionEnum
        {
            [EnumMember]
            LoadAll = 0,
            [EnumMember]
            LoadById = 1,
            [EnumMember]
            Save = 2,
            [EnumMember]
            SaveAndRunNow = 3,
            [EnumMember]
            RunNow = 4,
            [EnumMember]
            Delete = 5
        }

    }
}
