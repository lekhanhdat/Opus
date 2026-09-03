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
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Server.Common;

namespace AvePoint.GCommon.Contract.Server.ExportAndImport
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EIDataPlan : PlanDto
    {
        [DataMember]
        public EIPlanExtension EIExtention { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EIPlanExtension
    {
        /// <summary>
        /// 包含D5数据的logical device
        /// </summary>
        [DataMember]
        public string OldIndexDeviceId { set; get; }
        /// <summary>
        /// 用户指定的Media 
        /// </summary>
        [DataMember]
        public string Media { set; get; }

        [DataMember]
        public EIDataType DataType{set;get;}

        [DataMember]
        public EIOperateType OperateType{set;get;}

        [DataMember]
        public ImportDataVersion DataVersion { set; get; }

        [DataMember]
        public PlatformType PlatformType { set; get; }
    }
}
