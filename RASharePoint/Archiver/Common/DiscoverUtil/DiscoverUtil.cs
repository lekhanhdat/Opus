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
using AvePoint.GCommon;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.DB.Dao.Discovery.AOSP;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.AOSP;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using AvePoint.RA.DB.Model.Discovery.Office365;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.Archiver.Common.DiscoverUtil
{
    public class DiscoverUtil
    {
        private static readonly AveLogger Logger = AveLogger.GetInstance(typeof(DiscoverUtil));
        public static async Task<List<RMDiscoveryOffice365RuleInfo>> GetInactiveRuleAsync(InactiveRuleQueryParameter rulePara, int archiveDataType)
        {
            IRMDiscoveryOffice365RuleInfoDao ruleInfoDao = new RMDiscoveryOffice365RuleInfoDao();
            if (archiveDataType == (int)ArchiverDataType.All)
            {
                Logger.Info($"[GetInactiveRuleAsync] Select all file, will return null.");
                return null;
            }
            if (rulePara.Enable)
            {
                if (rulePara.RuleIds.IsNullOrEmpty())
                {
                    Logger.Info($"Inactive rule Enable is true and all select");
                    return await ruleInfoDao.GetRuleInfoesAsync(true, RMDiscoveryRuleDefinitionKind.Inactive);
                }
                else
                {
                    Logger.Info($"Inactive rule Enable is true and rule id is {SerializerHelper.SerializeByJsonSerializer(rulePara.RuleIds)}");
                    return await ruleInfoDao.GetByIdsAsync(rulePara.RuleIds.ToArray());
                }
            }
            else
            {
                Logger.Info($"Inactive rule Enable is false");
                return null;
            }
        }

        public static async Task<List<RMDiscoveryAOSPRuleInfo>> GetAOSPInactiveRuleAsync(string o365TenantId ,InactiveRuleQueryParameter rulePara, int archiveDataType)
        {
            IRMDiscoveryAOSPRuleInfoDao ruleInfoDao = new RMDiscoveryAOSPRuleInfoDao();
            if (archiveDataType == (int)ArchiverDataType.All)
            {
                Logger.Info($"[GetInactiveRuleAsync] Select all file, will return null.");
                return null;
            }
            if (rulePara.Enable)
            {
                if (rulePara.RuleIds.IsNullOrEmpty())
                {
                    Logger.Info($"Inactive rule Enable is true and all select");
                    return await ruleInfoDao.GetRuleInfoesAsync(true, o365TenantId, RMDiscoveryRuleDefinitionKind.Inactive);
                }
                else
                {
                    Logger.Info($"Inactive rule Enable is true and rule id is {SerializerHelper.SerializeByJsonSerializer(rulePara.RuleIds)}");
                    return await ruleInfoDao.GetByIdsAsync(rulePara.RuleIds.ToArray());
                }
            }
            else
            {
                Logger.Info($"Inactive rule Enable is false");
                return null;
            }
        }


        public static async Task<List<RMDiscoveryAOSPRuleInfo>> GetInactiveRuleAsync(string o365TenantId, InactiveRuleQueryParameter rulePara, int archiveDataType)
        {
            IRMDiscoveryAOSPRuleInfoDao ruleInfoDao = new RMDiscoveryAOSPRuleInfoDao();
            if (archiveDataType == (int)ArchiverDataType.All)
            {
                Logger.Info($"[GetInactiveRuleAsync] Select all file, will return null.");
                return null;
            }
            if (rulePara.Enable)
            {
                if (rulePara.RuleIds.IsNullOrEmpty())
                {
                    Logger.Info($"Inactive rule Enable is true and all select");
                    return await ruleInfoDao.GetRuleInfoesAsync(true, o365TenantId, RMDiscoveryRuleDefinitionKind.Inactive);
                }
                else
                {
                    Logger.Info($"Inactive rule Enable is true and rule id is {SerializerHelper.SerializeByJsonSerializer(rulePara.RuleIds)}");
                    return await ruleInfoDao.GetByIdsAsync(rulePara.RuleIds.ToArray());
                }
            }
            else
            {
                Logger.Info($"Inactive rule Enable is false");
                return null;
            }
        }


        public static async Task<List<RMDiscoveryOffice365RuleInfo>> GetROTRuleAsync(ROTRuleQueryParameter rulePara, int archiveDataType)
        {
            IRMDiscoveryOffice365RuleInfoDao ruleInfoDao = new RMDiscoveryOffice365RuleInfoDao();
            List<int> ruleIds = new List<int>();
            bool selectAll = true;
            if (archiveDataType == (int)ArchiverDataType.All)
            {
                Logger.Info($"[GetROTRuleAsync] Select all file, will return null.");
                return null;
            }
            if (rulePara.Enable)
            {
                Logger.Info($"[GetROTRuleAsync] Enabled, RuleCategories {SerializerHelper.SerializeByJsonSerializer(rulePara.RuleCategories)}");
                List<int> ruleCategories = new List<int>();
                foreach (var tempPara in rulePara.RuleCategories)
                {
                    if (tempPara.Checked)
                    {
                        ruleCategories.Add(tempPara.RuleCategory);
                        if (!tempPara.RuleIds.IsNullOrEmpty())
                        {
                            ruleIds.AddRange(tempPara.RuleIds);
                            selectAll = false;
                        }
                    }
                }
                if (selectAll)
                {
                    Logger.Info($"[GetROTRuleAsync] Select all rule, RuleCategories {SerializerHelper.SerializeByJsonSerializer(ruleCategories)}");
                    if (ruleCategories.Count > 0)
                    {
                        return (await ruleInfoDao.GetRuleInfoesByCategoriesAsync(true, ruleCategories, RMDiscoveryRuleDefinitionKind.ROT)).Where(item => item.AnalyseMethod != RMDiscoveryRuleAnalyseMethod.DuplicatedDocument).ToList();
                    }
                    else
                    {
                        return (await ruleInfoDao.GetRuleInfoesAsync(true, RMDiscoveryRuleDefinitionKind.ROT)).Where(item => item.AnalyseMethod != RMDiscoveryRuleAnalyseMethod.DuplicatedDocument).ToList();
                    }
                }
                else
                {
                    Logger.Info($"[GetROTRuleAsync] Get rule by ids {SerializerHelper.SerializeByJsonSerializer(ruleIds)}");
                    return await ruleInfoDao.GetByIdsAsync(ruleIds.ToArray());
                }
            }
            else
            {
                Logger.Info($"ROT rule Enable is false");
                return null;
            }
        }
        public static RMDiscoveryAOSPRuleInfo ConvertDiscoverRuleDefinationToRMDiscoveryAOSPRuleInfo(RMDiscoveryRuleDefinition ruleDefinition)
        {
            RMDiscoveryAOSPRuleInfo result = new RMDiscoveryAOSPRuleInfo()
            {
                CriteriaInfoesJson = JsonConvert.SerializeObject(ruleDefinition.CriteriaInfoes),
                AnalyseMethod = ruleDefinition.AnalyseMethod,
                Id = ruleDefinition.Id,
                UniqueId = ruleDefinition.UniqueId,
                Name = ruleDefinition.Name,
                Description = ruleDefinition.Description,
                Order = ruleDefinition.Order,
                IsEnable = ruleDefinition.IsEnable,
                ProcessActionParameter = JsonConvert.SerializeObject(ruleDefinition.ProcessActionParameter),
            };
            return result;
        }
        public static async Task<List<RMDiscoveryAOSPRuleInfo>> GetROTRuleAsync(string o365TenantId, ROTRuleQueryParameter rulePara, int archiveDataType)
        {
            IRMDiscoveryAOSPRuleInfoDao ruleInfoDao = new RMDiscoveryAOSPRuleInfoDao();
            List<int> ruleIds = new List<int>();
            bool selectAll = true;
            if (archiveDataType == (int)ArchiverDataType.All)
            {
                Logger.Info($"[GetROTRuleAsync] Select all file, will return null.");
                return null;
            }
            if (rulePara.Enable)
            {
                Logger.Info($"[GetROTRuleAsync] Enabled, RuleCategories {SerializerHelper.SerializeByJsonSerializer(rulePara.RuleCategories)}");
                List<int> ruleCategories = new List<int>();
                foreach (var tempPara in rulePara.RuleCategories)
                {
                    if (tempPara.Checked)
                    {
                        ruleCategories.Add(tempPara.RuleCategory);
                        if (!tempPara.RuleIds.IsNullOrEmpty())
                        {
                            ruleIds.AddRange(tempPara.RuleIds);
                            selectAll = false;
                        }
                    }
                }
                if (selectAll)
                {
                    Logger.Info($"[GetROTRuleAsync] Select all rule, RuleCategories {SerializerHelper.SerializeByJsonSerializer(ruleCategories)}");
                    if (ruleCategories.Count > 0)
                    {
                        return (await ruleInfoDao.GetRuleInfoesByCategoriesAsync(true, o365TenantId, ruleCategories, RMDiscoveryRuleDefinitionKind.ROT)).Where(item => item.AnalyseMethod != RMDiscoveryRuleAnalyseMethod.DuplicatedDocument).ToList();
                    }
                    else
                    {
                        return (await ruleInfoDao.GetRuleInfoesAsync(true, o365TenantId, RMDiscoveryRuleDefinitionKind.ROT)).Where(item => item.AnalyseMethod != RMDiscoveryRuleAnalyseMethod.DuplicatedDocument).ToList();
                    }
                }
                else
                {
                    Logger.Info($"[GetROTRuleAsync] Get rule by ids {SerializerHelper.SerializeByJsonSerializer(ruleIds)}");
                    return await ruleInfoDao.GetByIdsAsync(ruleIds.ToArray());
                }
            }
            else
            {
                Logger.Info($"ROT rule Enable is false");
                return null;
            }
        }
    }
}
