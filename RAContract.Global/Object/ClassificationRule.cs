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
using AvePoint.RA.Contract.RMRuleManageMent;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.Global.Object
{
    [DataContract]
    public class ClassificationRule
    {
        //public bool UseDefaultTerm;
        [DataMember(EmitDefaultValue = false)]
        public bool IsDefaultRule { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool NoDefaultTerm { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string TermId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string TermName { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool TermIsRemoved { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool TermIsDeprecated { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int RuleLevel { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int Category { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int RuleOrder { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public List<FilterGroup> FilterGroups { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string AndOrExpression { get; set; }
    }
    [DataContract]
    public class FilterGroup
    {
        [DataMember(EmitDefaultValue = false)]
        public List<RuleFilter> Filters { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public List<FilterGroup> FilterGroups { get; set; }
        /// <summary>
        /// And Or
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public ArchiverFilterCombineMode CombineMode { get; set; }
        /// <summary>
        /// True False
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string TrueFalse { get; set; }
    }

    [DataContract]
    public class RuleFilter
    {
        [DataMember(EmitDefaultValue = false)]
        public int SequenceNo { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int Level { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int Condition { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int CombineMode { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int RuleType { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string filterName { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string Value1 { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string Value2 { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int Value1Unit { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int Value2Unit { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string FilterCretia { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string RuleBaseString { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public DisplayDateTime StartTimeInfo { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public DisplayDateTime EndTimeInfo { get; set; }
    }   
}
