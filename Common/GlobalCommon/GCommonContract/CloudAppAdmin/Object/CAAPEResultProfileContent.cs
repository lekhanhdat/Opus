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
namespace AvePoint.GCommon.Contract.CloudAppAdmin.Object
{
    using Common;
    using Server.Common.Profile.Object;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAAPEResultProfileContent : IProfileContent
    {
        [DataMember]
        public List<CAAPERuleResult> RuleResults { get; set; }

        [DataMember]
        public List<string> JobIds { get; set; }
        [DataMember]
        public long LastModifiedTime { get; set; }
        public CAAPEResultProfileContent()
        {
            RuleResults = new List<CAAPERuleResult>();
            JobIds = new List<string>();
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAAPERuleResult
    {
        [DataMember]
        public CAAPERuleCategory RuleCategory { get; set; }
        [DataMember]
        public List<CAAPERuleReportItem> RuleReportItem { get; set; }
        [DataMember]
        public bool hidden { get; set; }
        [DataMember]
        public int WithinPolicy
        {
            get
            {
                if (RuleReportItem == null)
                    return 0;
                else
                    return RuleReportItem.FindAll(a => a.Status == true).Count;
            }
            set { }
        }
        [DataMember]
        public int OutOfPolicy
        {
            get
            {
                if (RuleReportItem == null)
                    return 0;
                else
                    return RuleReportItem.FindAll(a => a.Status == false).Count;
            }
            set { }
        }

        public CAAPERuleResult()
        {
            RuleReportItem = new List<CAAPERuleReportItem>();
        }
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAAPERuleReportItem
    {
        [DataMember]
        public SimpleADUser SimpleADUser { get; set; }

        [DataMember]
        public bool? Status { get; set; }

        [DataMember]
        public bool? Action { get; set; }

        [DataMember]
        public bool InWhiteList { get; set; }
        [DataMember]
        public bool InBlackList { get; set; }

        [DataMember]
        public string Remark1 { get; set; }
    }
}
