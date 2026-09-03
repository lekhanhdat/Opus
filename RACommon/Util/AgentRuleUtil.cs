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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Contract.Explorer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Util
{
    public class AgentRuleUtil
    {
        public static List<Rule> FilterRuleWithDataSource(List<Rule> rules, SourceFlag flag)
        {
            List<Rule> filterRules = new List<Rule>();
            foreach (var rule in rules)
            {
                switch (flag)
                {
                    case SourceFlag.FileSystem:
                        if (rule.FSRule != null)
                        {
                            rule.Filters = new List<GCommon.Contract.CommonFilter.FilterPolicy>();
                            rule.SOFilters = new List<SOFilterPolicy>();
                            rule.PhysicalRule = null;
                            rule.EXORule = null;
                            rule.EXORuleString = null;
                            rule.OneDriveRule = null;
                            rule.OneDriveRuleString = null;
                            rule.SPLocalRule = null;
                            rule.SPLocalRuleString = null;
                            filterRules.Add(rule);
                        }
                        break;
                    case SourceFlag.SharePointOnPrem:
                        if (rule.SPLocalRule != null)
                        {
                            rule.Filters = new List<GCommon.Contract.CommonFilter.FilterPolicy>();
                            rule.SOFilters = new List<SOFilterPolicy>();
                            rule.PhysicalRule = null;
                            rule.EXORule = null;
                            rule.EXORuleString = null;
                            rule.OneDriveRule = null;
                            rule.OneDriveRuleString = null;
                            rule.FSRule = null;
                            rule.FSRuleString = null;
                            filterRules.Add(rule);
                        }
                        break;
                    default:
                        break;
                }
            }
            return filterRules;
        }
    }
}
