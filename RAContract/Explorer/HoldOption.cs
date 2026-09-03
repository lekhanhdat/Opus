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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Explorer
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class HoldOption
    {
        [DataMember(EmitDefaultValue = false)]
        public List<int> Records { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public List<Guid> RelatedRecords { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string HoldId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public long ReleaseTime { get; set; }

        /// <summary>
        /// Place hold/update/ =0
        /// Remove =1
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int Action { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<string> RemoveHolds { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string PlaceHoldAction { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string HoldBy { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool IsOverWrite { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public HoldDateUnit Unit { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int Number { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string FolderOriginalHoldId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string UserId { get; set; }
    }
}
