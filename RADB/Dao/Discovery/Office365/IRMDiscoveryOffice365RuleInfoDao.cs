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
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Office365;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.Office365
{
    public interface IRMDiscoveryOffice365RuleInfoDao
    {
        Task<List<RMDiscoveryOffice365RuleInfo>> GetRuleInfoesAsync(params RMDiscoveryRuleDefinitionKind[] kinds);

        Task<List<RMDiscoveryOffice365RuleInfo>> GetRuleInfoesAsync(bool enabled, params RMDiscoveryRuleDefinitionKind[] kinds);

        Task<List<RMDiscoveryOffice365RuleInfo>> GetRuleInfoesAsyncOrderByCategory(bool enabled, params RMDiscoveryRuleDefinitionKind[] kinds);

        Task<List<RMDiscoveryOffice365RuleInfo>> GetRuleInfoesAsync(bool enabled, params RMDiscoveryRuleCategory[] categories);

        Task<List<RMDiscoveryOffice365RuleInfo>> GetRuleInfoesAsync(bool enabled, params RMDiscoveryRuleAnalyseMethod[] methods);

        Task<List<RMDiscoveryOffice365RuleInfo>> GetRuleInfoesByCategoriesAsync(bool enabled, List<int> ruleCategoriesparams, RMDiscoveryRuleDefinitionKind kind);

        Task<int> AddOrUpdateAsync(List<RMDiscoveryOffice365RuleInfo> updateRuleInfo, RMDiscoveryDBEFContext context);

        Task<List<RMDiscoveryOffice365RuleInfo>> GetByIdsAsync(params int[] ruleIds);

        Task<bool> CheckExistingRuleByAnalyzeMethodsAsync(bool enabled, params RMDiscoveryRuleAnalyseMethod[] methods);
    }
}
