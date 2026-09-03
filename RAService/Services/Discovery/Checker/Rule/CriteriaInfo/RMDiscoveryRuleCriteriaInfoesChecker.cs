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
using Aspose.Words.XAttr;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using DocumentFormat.OpenXml.Spreadsheet;
using PnP.Framework.Provisioning.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Checker.Rule.CriteriaInfo
{
    public abstract class RMDiscoveryRuleCriteriaInfoesChecker
    {
        private static readonly Dictionary<RMDiscoveryRuleAnalyseMethod, RMDiscoveryRuleCriteriaInfoesChecker> s_checker = new();

        public abstract RMDiscoveryRuleAnalyseMethod AnalyseMethod { get; }

        static RMDiscoveryRuleCriteriaInfoesChecker()
        {
            var checkerType = typeof(RMDiscoveryRuleCriteriaInfoesChecker);
            var assembly = Assembly.GetAssembly(checkerType);
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract || type.IsInterface) continue;
                if (type.BaseType?.Name == checkerType.Name)
                {
                    var instace = Activator.CreateInstance(type) as RMDiscoveryRuleCriteriaInfoesChecker;
                    s_checker.Add(instace.AnalyseMethod, instace);
                }
            }
        }

        protected abstract bool Check(RMDiscoveryRuleCriteriaInfo criteriaInfo);

        public static bool Check(RMDiscoveryRuleAnalyseMethod analyseMethod, IEnumerable<RMDiscoveryRuleCriteriaInfo> criteriaInfoes)
        {
            var criterias = criteriaInfoes.OrderBy(item => item.Order).ToList();
            if (
                criterias.Select(item => item.Order).GroupBy(item => item).Any(item => item.Count() > 1) ||
                criterias.Max(item => item.Order) != criterias.Count ||
                criterias.Any(item => item.Order < 1)
                )
            {
                return false;
            }

            if (criterias.Last().LogicType != RMDiscoveryCriteriaLogicType.None)
            {
                return false;
            }

            if (!criterias.Take(criterias.Count - 1).All(item => item.LogicType == RMDiscoveryCriteriaLogicType.And
            || item.LogicType == RMDiscoveryCriteriaLogicType.Or))
            {
                return false;
            }

            if (criterias.Any(item => item.CriteriaType == 0 || item.ConditionInfo == null))
            {
                return false;
            }

            return criteriaInfoes.All(item => s_checker[analyseMethod].Check(item));
        }
    }
}
