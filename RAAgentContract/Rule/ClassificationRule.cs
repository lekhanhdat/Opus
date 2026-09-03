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
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.Server.ControlPanel.FilterPolicy.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Hybrid.AgentContract.Rule
{
    //public class ClassificationRule
    //{
    //    //public bool UseDefaultTerm;
    //    public bool IsDefaultRule { get; set; }
    //    public bool NoDefaultTerm { get; set; }
    //    public string TermId { get; set; }
    //    public string TermName { get; set; }
    //    public bool TermIsRemoved { get; set; }
    //    public bool TermIsDeprecated { get; set; }
    //    public PolicyLevel RuleLevel { get; set; }
    //    public PolicyLevel Category { get; set; }
    //    public int RuleOrder { get; set; }
    //    public List<FilterGroup> FilterGroups { get; set; }
    //    public string AndOrExpression { get; set; }
    //}

    //public class FilterGroup
    //{
    //    public List<RuleFilter> Filters { get; set; }
    //    public List<FilterGroup> FilterGroups { get; set; }
    //    /// <summary>
    //    /// And Or
    //    /// </summary>
    //    public ArchiverFilterCombineMode CombineMode { get; set; }
    //    /// <summary>
    //    /// True False
    //    /// </summary>
    //    public string TrueFalse { get; set; }
    //}

    //public class RuleFilter
    //{
    //    public int SequenceNo { get; set; }
    //    public PolicyLevel Level { get; set; }
    //    public ArchiverFilterCondition Condition { get; set; }
    //    public ArchiverFilterCombineMode CombineMode { get; set; }
    //    public ArchiverFilterRuleType RuleType { get; set; }
    //    public string filterName { get; set; }
    //    public string Value1 { get; set; }
    //    public string Value2 { get; set; }
    //    public PolicyValueUnit Value1Unit { get; set; }
    //    public PolicyValueUnit Value2Unit { get; set; }
    //    public string FilterCretia { get; set; }
    //    public PolicyRuleBase RuleBase { get; set; }

    //    public DisplayDateTime StartTimeInfo { get; set; }
    //    public DisplayDateTime EndTimeInfo { get; set; }
    //}
}
