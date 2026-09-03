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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.Archiver.CAMLHelper
{
    public class RuleItemCollection
    {
        public bool HasUnCamlQueryableCondition { get; set; }
        public RuleCollection CommonRules { get; set; }
        public List<RuleItem> Rules { get; set; }

        #region for disposal job
        public Guid TermId { get; set; }

        public string TermName { get; set; }

        public int Index4TermGroup { get; set; }

        #endregion
        public override bool Equals(object obj)
        {
            if (obj is not RuleItemCollection) return false;
            RuleItemCollection obj2 = obj as RuleItemCollection;
            var thisRuleIds = GetRuleIdString(Index4TermGroup, Rules);
            var obj2RuleIds = GetRuleIdString(obj2.Index4TermGroup, obj2.Rules);
            return thisRuleIds.Equals(obj2RuleIds);
        }

        public override int GetHashCode()
        {
            return (GetRuleIdString(Index4TermGroup, Rules)).GetHashCode();
        }

        private string GetRuleIdString(int index ,List<RuleItem> rules)
        {
            return $"[{index}]{string.Join("|", rules.OrderBy(r => r.RuleId).Select(r => r.RuleId).ToList())}";
        }
    }

    public class RuleItem
    {
        public string RuleId { get; set; }
        public string RuleName { get; set; }
        public bool IsManualApproval { get; set; }
        public ExportTypeValue ExportType { get; set; }
        public List<ArchiverRuleFilter> RuleFilters { get; set; }
        public bool HasUnCamlQueryableCondition { get; set; }
        public bool DeleteRecords { get; set; }
        public RelatedRecordOption RelatedRecordOption { get; set; }
        public string DisposalClass { get; set; }
    }
}
