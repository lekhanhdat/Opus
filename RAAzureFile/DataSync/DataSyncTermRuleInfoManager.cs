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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.DB.Dao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAAzureFile.DataSync
{
    public class DataSyncTermRuleInfoManager
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(DataSyncTermRuleInfoManager));

        private static readonly ITermRuleAssociationDao TermRuleAssociationDao =
    PlatformWindsorManager.GetService<ITermRuleAssociationDao>();

        private static readonly IRuleManagerService RuleManagerService =
    PlatformWindsorManager.GetService<IRuleManagerService>();

        private static readonly Dictionary<string, List<Rule>> RuleCache =
            new Dictionary<string, List<Rule>>();

        private static readonly object Locker = new object();

        public static bool TryGetTermRelatedRule(string termId, out List<Rule> rules)
        {
            if (string.IsNullOrEmpty(termId) || !Guid.TryParse(termId, out _))
            {
                rules = new List<Rule>();  
                Logger.Warn($"Invalid termId [{termId}], skip get term related rule.");
                return false;
            }

            using (new PerformanceScope("AzureFileShare:DataSync:GetTermRelatedRule", "", true))
            {
                if (!RuleCache.TryGetValue(termId, out rules))
                {
                    lock (Locker)
                    {
                        if (!RuleCache.TryGetValue(termId, out rules))
                        {
                            Logger.Info($"Can't find term [{termId}] related rule from cache.");
                            var termRelatedRuleInfoes = TermRuleAssociationDao.GetTermRuleInfoByTermUniqueId(new Guid(termId));
                            if (termRelatedRuleInfoes.Count == 0)
                            {
                                Logger.Warn($"Current term [{termId}] not found related rule infoes.");
                                RuleCache[termId] = new List<Rule>();
                                return false;
                            }

                            var ruleIds = termRelatedRuleInfoes.Select(item => item.RuleId).ToList();
                            Logger.Info($"Term [{termId}] related rules [{string.Join(", ", ruleIds)}].");
                            rules = RuleManagerService.GetRulesByIds(ruleIds);
                            rules = rules.Where(item => item.AzureFileRule != null).OrderBy(item => termRelatedRuleInfoes.First(i => i.RuleId.ToString() == item.Id).RuleOrder).ToList();
                            if (rules.Count == 0)
                            {
                                Logger.Warn($"Current term related rules not found in record.");
                                RuleCache[termId] = new List<Rule>();
                                return false;
                            }

                            RuleCache[termId] = rules;
                        }
                    }
                }

                return rules.Count != 0;
            }
        }
    }
}
