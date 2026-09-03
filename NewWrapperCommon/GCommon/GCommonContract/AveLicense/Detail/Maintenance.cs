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
using System.Text;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using System.Xml.Serialization;

namespace AvePoint.GCommon.Contract.AveLicense.Detail
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot("Maintenance")]
    public class Maintenance
    {
        public static readonly DateTime ExpirationUnlimited = DateTime.MaxValue;
        public static readonly int DurationUnlimited = -1;

        [DataMember]
        [XmlAttribute("EnableTime")]
        public DateTime EnableTime { get; set; }

        [DataMember]
        [XmlAttribute("ExpireTime")]
        public DateTime ExpireTime { get; set; }

        [DataMember]
        public int? EffectiveDays { get; set; }

        [DataMember]
        [XmlAttribute("HasEverRegisteredEnterprise")]
        public bool HasEverRegisteredEnterprise { get; set; }

        [XmlAttribute("IsUsingSharepointTime")]
        public bool IsUsingSharepointTime { get; set; }

        public void SetEnableTime(DateTime time)
        {
            this.EnableTime = time;
            if (this.EffectiveDays.HasValue)
            {
                this.ExpireTime = this.EffectiveDays == DurationUnlimited ? ExpirationUnlimited
                    : this.EnableTime + new TimeSpan(this.EffectiveDays.Value, 0, 0, 0);
            }
        }
    }
}
