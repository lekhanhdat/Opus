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
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb.Physical
{
    [DataContract(Namespace = GCommon.Contract.Common.ContractConstants.Namespace)]
    public class LoanRequestDto
    {
        [DataMember(EmitDefaultValue = false)]
        public List<RequestFileDto> Items { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<AOSUserDto> OnBehalf { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public RequestDateTimeDto ReturnDate { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Comment { get; set; }
    }

    [DataContract(Namespace = GCommon.Contract.Common.ContractConstants.Namespace)]
    public class RequestDateTimeDto
    {
        [DataMember(EmitDefaultValue = false)]
        public string DateTimeStr { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string TimeZoneId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool AutoAdjustClock { get; set; }
    }

    [DataContract(Namespace = GCommon.Contract.Common.ContractConstants.Namespace)]
    public class RequestFileDto
    {
        [DataMember(EmitDefaultValue = false)]
        public string Id { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string UniqueId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public RMNodeType NodeType { get; set; }
    }
}
