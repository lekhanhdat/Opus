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
using AvePoint.RA.Contract.Discovery.Model.Query.Google.Parameter.Profile;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Google.Query.Profile.Rot
{
    public class RMDiscoveryGoogleRotProfileNodeTotalAggregateDataQuerier : RMDiscoveryGoogleRotProfileDataQuerier<Dictionary<string, object>>
    {
        public RMDiscoveryGoogleRotProfileNodeTotalAggregateDataQuerier(RMDiscoveryGoogleProfileQueryParameter queryParameter) : base(queryParameter)
        {
        }

        public override async Task<Dictionary<string, object>> QueryAsync()
        {
            var profileInfo = await _profileDao.GetProfileInfoByIdAsync(_queryParameter.OrganizationId, _queryParameter.ProfileId);
            var ruleIds = JsonConvert.DeserializeObject<HashSet<int>>(profileInfo.RuleIdsJson);

            var inactiveRules = await _ruleInfoDao.GetRuleInfoesAsync(true, RMDiscoveryRuleDefinitionKind.ROT);
            inactiveRules = inactiveRules.Where(item => ruleIds.Contains(item.Id)).ToList();

            var needSumColumns = inactiveRules.Select(item => item.ToCustomColumn().Name).ToList();

            var sql = $@"SELECT 
data.FileTotalSize AS fileTotalSize,
data.RotFileTotalSize AS rotFileTotalSize,
data.RCategoryFileTotalSize AS redundant,
data.OCategoryFileTotalSize AS obsolete,
data.TCategoryFileTotalSize AS trivial
{(needSumColumns.Any() ? "," + string.Join(",", needSumColumns.ConvertAll(item => $"data.{item} AS {item}")) : "")}
 FROM [{_profileSchemaName}].[RMGoogleProfileBasicRotData] as data";

            var list = await _queryDao.GetDataDictionaryListAsync(sql);

            var res = new Dictionary<string, object>
            {
                {$"redundant", 0L},
                {$"obsolete", 0L},
                {$"trivial", 0L},
                {$"fileTotalSize", 0L },
                {$"rotFileTotalSize", 0L },
            };
            foreach (var needSumColumn in needSumColumns)
            {
                res.Add(needSumColumn, 0L);
            }

            if (list.Count > 0)
            {
                var data = list[0];
                res["redundant"] = Convert.ToInt64(data["redundant"]);
                res["obsolete"] = Convert.ToInt64(data["obsolete"]);
                res["trivial"] = Convert.ToInt64(data["trivial"]);
                res["fileTotalSize"] = Convert.ToInt64(data["fileTotalSize"]);
                res["rotFileTotalSize"] = Convert.ToInt64(data["rotFileTotalSize"]);

                foreach (var needSumColumn in needSumColumns)
                {
                    res[needSumColumn] = Convert.ToInt64(res[needSumColumn]) + Convert.ToInt64(data[needSumColumn]);
                }
            }

            return res;
        }
    }
}
