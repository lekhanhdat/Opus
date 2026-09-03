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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common;

namespace AvePoint.GCommon.Contract.Vault.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class VaultProfileDto : PlanDto
    {
        [DataMember]
        public string ProcessingPoolId { set; get; }

        [DataMember]
        public string ProcessingPoolName { get; set; }

        [DataMember]
        public ExportType ExportType { get; set; }

        [DataMember]
        public String ExportLocationId { get; set; }

        [DataMember]
        public String ExportLocationName { get; set; }

        [DataMember]
        public List<VaultRule> Rules { set; get; }

        [DataMember]
        public string EmailId { set; get; }

        [DataMember]
        public string EmailName { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ExportType
    {
        [EnumMember]
        Autonomy = 0,
        [EnumMember]
        Concordance = 1
    }
}
