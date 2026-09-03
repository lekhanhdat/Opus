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
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.DB.Dao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.OneDrive.OneDriveExplorerSync.Util
{
    public class RMAutoClassificationQueryHelper
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(RMAutoClassificationQueryHelper));
        public ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();
        public void GetAutoTermRuleMappings(DateTime timePoint, List<ClassificationRule> autoRules)
        {
            Dictionary<Guid, RMRuleItemCollection> termRuleMappings = new Dictionary<Guid, RMRuleItemCollection>();
            var allTerms = TermDao.GetRMTermsByTermIds(autoRules.Select(r => new Guid(r.TermId)).ToList());
            foreach (var autoRule in autoRules)
            {
                var rmTerm = allTerms.Where(t => t.UniqueId == new Guid(autoRule.TermId)).FirstOrDefault();
                if (rmTerm == null)
                {
                    logger.Info("Term is null, term id:{0}", autoRule.TermId);
                    continue;
                }

                if (rmTerm.IsRemoved)
                {
                    logger.Info("Term is removed, term id:{0}", autoRule.TermId);
                    continue;
                }

                RuleCollection commonRules = new RuleCollection() { Rules = new Dictionary<int, Rule>() };
                List<RMRuleItem> rmRules = new List<RMRuleItem>();
                bool hasUnCamlQueryableCondition = false;
                Rule rule;
            }
        }
    }
}
