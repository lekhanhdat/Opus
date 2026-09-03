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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Service.Services.AzureFileShare.Api;
using RADataSynchronize.TermCheck;
using RADataSynchronize.TermCheck.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAAzureFile.DataSync
{
    public class DataSyncTermManager
    {

        private static readonly IRMChangeClassificationDao ChangeClassificationDao =
    PlatformWindsorManager.GetService<IRMChangeClassificationDao>();

        private static readonly ITermDao TermDao =
    PlatformWindsorManager.GetService<ITermDao>();

        public static List<Guid> GetHasChangedTermIds(long ticks)
        {
            var res = new List<Guid>();

            var changedTerms = ChangeClassificationDao.GetAllChange(ticks, (int)TermChangeType.TermRule);
            res.AddRange(changedTerms);

            foreach(var changedTerm in changedTerms)
            {
                var subTerms = TermDao.GetAllSubTermUniqueIds(changedTerm);
                res.AddRange(subTerms);
            }

            return res;
        }

        public static TermInfo GetMatchedTermInfo(AzureFileShareApiItem item, AzureFileSettingDto setting)
        {
            if(setting.DeployTermMethod == DeployTermMethod.NoDefaultTerm)
            {
                return new TermInfo
                {
                    IsManually = true
                };
            }
            else if(setting.DeployTermMethod == DeployTermMethod.UseDefaultTerm)
            {
                return new TermInfo
                {
                    IsManually = false,
                    TermId = setting.DefaultTermId.ToString(),
                    TermName = setting.DefaultTermName,
                    TermIsRemoved = setting.IsDefaultTermRemoved,
                    TermIsDeprecated = setting.IsDefaultTermDeprecated,
                };
            }
            else
            {
                return GetAutoMatchedTermInfo(item, setting);
            }
        }

        private static TermInfo GetAutoMatchedTermInfo(AzureFileShareApiItem item, AzureFileSettingDto setting)
        {
            var values = GetRuleTypeMappingValue(item);
            if(!TermCriteriaChecker.TryGetAccordWithTermInfo(setting.AutoClassificationRules, values, out var termInfo))
            {
                throw new Exception($"The item [{item.Id} - {item.RealId}] find related term has an error.");
            }

            return termInfo;
        }

        private static Dictionary<ArchiverFilterRuleType, object> GetRuleTypeMappingValue(AzureFileShareApiItem item)
        {
            var nameArr = item.Name.Split('.');
            var extension = nameArr.Length > 1 ? nameArr.Last() : "";
            return new Dictionary<ArchiverFilterRuleType, object>
            {
                { ArchiverFilterRuleType.Name, item.Name },
                { ArchiverFilterRuleType.DocumentSize, item.Size },
                { ArchiverFilterRuleType.ModifiedTime, item.Modified },
                { ArchiverFilterRuleType.CreatedTime, item.Created },
                { ArchiverFilterRuleType.LastAccessedTime, item.LastAccessTime },
                { ArchiverFilterRuleType.Type, extension },
                { ArchiverFilterRuleType.FilePath, item.FullPath }
            };
        }
    }
}
