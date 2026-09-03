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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.RA.SharePoint.Archiver.Common.RuleConverter
{
    public abstract class DO2SOFilterConverterBase: IDO2SOFilterConverter
    {
        public abstract string AndOrString { get;}

        public abstract List<SOFilterPolicy> Convert();

        public string GetSOFilterExpression(List<SOFilterPolicy> policies)
        {
            string AndOrExpression = "(";
            for (int i = 0; i < policies.Count; i++)
            {
                SOFilterPolicy filterDto = policies[i];
                if (i == policies.Count - 1)
                {
                    AndOrExpression += string.Format("{0}", filterDto.SequenceNo);
                }
                else
                {
                    AndOrExpression += string.Format("{0} {1} ", filterDto.SequenceNo, filterDto.IsAnd ? "And" : "Or");
                }
            }
            AndOrExpression += ")";
            return AndOrExpression;
        }

        public static int GetLastSequenceNo(List<SOFilterPolicy> policies)
        {
            int lastSequenceNo = 0;
            foreach (var policy in policies)
            {
                if (policy.SequenceNo > lastSequenceNo)
                {
                    lastSequenceNo = policy.SequenceNo;
                }
            }
            return lastSequenceNo;
        }

        public static List<SOFilterPolicy> GetMergedFilters(List<FilterGroup> filterGroups)
        {
            List<SOFilterPolicy> filters = new List<SOFilterPolicy>();
            foreach (var fg in filterGroups.OrderBy(f=>f.Order))
            {
                if (fg.Filters != null && fg.Filters.Count > 0)
                {
                    filters.AddRange(fg.Filters);
                }
            }

            return filters;
        }

        public static string GetMergedAndOrExpression(List<FilterGroup> filterGroups)
        {
            StringBuilder mAndOrExpressionBuilder = new StringBuilder();
            for (int i = 0; i < filterGroups.Count; i++)
            {
                string andorLogicString = string.Empty;

                var fg = filterGroups[i];

                if (fg.LogicType == Contract.Discovery.Model.Rule.RMDiscoveryCriteriaLogicType.Or)
                {
                    andorLogicString = "Or";
                }
                else
                {
                    andorLogicString = "And";
                }
                if (i + 1 == filterGroups.Count)
                {
                    mAndOrExpressionBuilder.Append($"{fg.AndOrString}");
                }
                else
                {
                    mAndOrExpressionBuilder.Append($"{fg.AndOrString} {andorLogicString} ");
                }
            }

            return mAndOrExpressionBuilder.ToString();
        }
    }

    public interface IDO2SOFilterConverter
    {
        public string AndOrString { get; }
        public List<AvePoint.GCommon.Contract.StorageOptimization.Object.SOFilterPolicy> Convert();
    }
}
