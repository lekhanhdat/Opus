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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace AvePoint.GCommon.Contract.Server.Common
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ValidateUsedProfileResult
    {
        /// <summary>
        /// 被占用的Profile, 只需要赋值Id
        /// </summary>
        [DataMember]
        public NameAndIdDto UsedProfile { get; set; }

        /// <summary>
        /// 国际化后的模块名称
        /// </summary>
        [DataMember]
        public string ModuleName { get; set; }

        /// <summary>
        /// 占用该Profile的Plan/Profile/Running Job
        /// </summary>
        [DataMember]
        public List<string> Reference { get; set; }

        /// <summary>
        /// 占用该Profile的类型
        /// </summary>
        [DataMember]
        public ReferenceType ReferenceType { get; set; }
    }

    public enum ReferenceType
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        Plan = 1,

        [EnumMember]
        Profile = 2,

        [EnumMember]
        RunningJob = 3,
    }
}
