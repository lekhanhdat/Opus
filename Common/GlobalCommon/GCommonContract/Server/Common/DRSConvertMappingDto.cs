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

namespace AvePoint.GCommon.Contract.Server.Common
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DRSConvertMappingDto
    {
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        /// Docave 5 DB中的LogcialDevice Id
        /// </summary>
        [DataMember]
        public string LogicalDeviceId { get; set; }

        /// <summary>
        ///  Docave 5 DB中的Retention or Pruning Rule Id 
        /// </summary>
        [DataMember]
        public string RuleId { get; set; }

        /// <summary>
        /// 由D5的LogicalDevice升级成的D6中的Storage Policy Id
        /// </summary>
        [DataMember]
        public string StoragePolicyId { get; set; }
    }
}
