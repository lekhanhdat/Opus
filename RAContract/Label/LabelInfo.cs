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
using AvePoint.RA.Contract.RMRuleManageMent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Label
{
    [DataContract]
    public class LabelSettingsInfo
    {
        [DataMember]
        public int LabelId
        {
            get; set;
        }
        [DataMember]
        public string Description
        {
            get; set;
        }
        [DataMember]
        public List<RuleDisplayInfo> RuleInfos
        {
            get; set;
        }
    }

    [DataContract]
    public class LabelInfo
    {
        [DataMember]
        public int LabelId
        {
            get; set;
        }
        [DataMember]
        public string UniqueLabelId
        {
            get; set;
        }
        [DataMember]
        public string LabelName
        {
            get; set;
        }
        [DataMember]
        public string Description
        {
            get; set; 
        }
        [DataMember]
        public bool IsManually
        {
            get; set;
        }

        public LabelType Type
        {
            get; set;
        }


        [DataMember]
        public double Score
        {
            get; set;
        }

        public SmartLabelApplyType SmartLabelApplyType
        {
            get; set;
        }

        public bool NeedIpdateCosmosDB
        {
            get; set;
        }

        public ApplyLabelType ApplyLabelType { get; set; }
    }

    public class LabelValue
    {
        public string LabelId
        {
            get; set;
        }
        public string Title
        {
            get; set;
        }
        public string Name
        {
            get; set;
        }
        public string RevisionId
        {
            get; set;
        }
        public LabelType LabelType
        {
            get; set;
        }
        public long? CreateTime
        {
            get; set;
        } 
        public long? RevisionCreateTime
        {
            get; set;
        }
        public string CustomerId
        {
            get; set;
        }
        public State State
        {
            get; set;
        }
        public bool? HasUnpublishedChanges
        {
            get; set;
        }
    }
    public enum LabelType
    {
        None = 0,
        Shared,
        Admin
    }
    public enum State
    {
        None = 0,
        Published,
        Disabled,
        UnpublishedDraft,
        Deleted
    }

    public enum SmartLabelApplyType
    {
        None = 0,
        AutoApply = 1,
        ManualReview = 2
    }

    public enum ApplyLabelType
    {
        ApplyDefaultLabel = 0,
        AutoPopulateApply = 1,
        ApplyViaSmartTerm = 2,
        SkipApplyViaSmartTermByManual = 3,
    }
}
