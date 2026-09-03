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

using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace AvePoint.GCommon.Contract.Storage.Entity
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ValidateResultDto
    {
        /// <summary>
        /// 被占用的Profile id
        /// </summary>
        [DataMember]
        public string Profile { get; set; }

        /// <summary>
        /// 国际化后的模块名称
        /// </summary>
        [DataMember]
        public string ModuleName { get; set; }

        /// <summary>
        /// 占用该Profile的Plan/Profile/Running Jobs/....,
        /// 如果不是具体的名称,比如做过的数据，这个时候可以用一个描述性的名词代替用于说明用到了什么功能上
        /// </summary>
        [DataMember]
        public List<string> Reference { get; set; } 

    }
}
