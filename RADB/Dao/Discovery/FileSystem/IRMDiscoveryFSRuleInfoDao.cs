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
using System.Collections.Generic;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Model.Discovery.FileSystem;

namespace AvePoint.RA.DB.Dao.Discovery.FileSystem
{
    public interface IRMDiscoveryFSRuleInfoDao
    {
        Task<List<RMDiscoveryFSRuleInfo>> GetRuleInfoesAsync(params RMDiscoveryRuleDefinitionKind[] kinds);

        Task<List<RMDiscoveryFSRuleInfo>> GetRuleInfoesAsync(bool enabled, params RMDiscoveryRuleDefinitionKind[] kinds);

        Task<List<RMDiscoveryFSRuleInfo>> GetRuleInfoesAsync(bool enabled, params RMDiscoveryRuleCategory[] categories);

        Task<List<RMDiscoveryFSRuleInfo>> GetRuleInfoesAsync(bool enabled, params RMDiscoveryRuleAnalyseMethod[] methods);

        Task<List<RMDiscoveryFSRuleInfo>> GetRuleInfoesByCategoriesAsync(bool enabled, List<int> ruleCategoriesparams, RMDiscoveryRuleDefinitionKind kind);

        Task<int> AddOrUpdateAsync(List<RMDiscoveryFSRuleInfo> updateRuleInfo, RMDiscoveryDBEFContext context);

        Task<List<RMDiscoveryFSRuleInfo>> GetByIdsAsync(params int[] ruleIds);
    }
}
