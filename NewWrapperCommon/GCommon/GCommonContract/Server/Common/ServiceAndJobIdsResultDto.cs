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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

namespace AvePoint.GCommon.Contract.Server.Common
{
   /// <summary>
   ///用于存储创建该job的相关service的信息。
   ///注意如果某些模块所起的job与Agent，Media或Report Service无关则返回null即可。
   /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ServiceAndJobIdsResultDto
    {
        [DataMember]
        public string serviceId { get; set; } //与jobIds相关的serviceid。
        [DataMember]
        public List<string>jobIds{ get; set; } //该serviceId所起的jobIds
    }
}
