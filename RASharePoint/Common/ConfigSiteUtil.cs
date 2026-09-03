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
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.EnforceRetention;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.Common
{
    public class ConfigSiteUtil
    {
        private readonly AveLogger logger = AveLogger.GetInstance(typeof(ConfigSiteUtil));
        private AveBPOSAccountInfo mBposInfo;
        private string mConfigSiteUrl;
        private const string ListUrl_ExcludeListUrl = "Lists/excludelists";
        private const string ListUrl_ExcludeFileTypeListUrl = "Lists/masterfiles";
        private const string ListUrl_Rules = "Lists/csdrules";
        private const string ListUrl_GeneralConfigurations = "Lists/appsettings";
        private const string ListUrl_ModifiedBasedFileType = "Lists/mdbrfiles";

        private const string CSD_AppSettingKey = "CSD_AppSettingKey";
        private const string CSD_AppSettingValue = "CSD_AppSettingValue";
        private const string KeyName_WhiteListDocumentClassId = "WhiteListDocumentClassId";
        private bool mIsUpgrade = false;

        public ConfigSiteUtil(AveBPOSAccountInfo bposInfo, string curSiteUrl)
        {
            mBposInfo = bposInfo;
            mConfigSiteUrl = GetConfigSiteUrl(curSiteUrl);
        }

        private string GetConfigSiteUrl(string currentSiteUrl)
        {
            string csdConfigSiteUrl = null;
            var reg = new Regex(@"https://([^/]+?)(-my|-admin)?\.(sharepoint[^/]*)(/.*)?");
            var matchs = reg.Match(currentSiteUrl);
            if (matchs.Success)
            {
                csdConfigSiteUrl = string.Format("https://{0}.{1}/sites/csd_config", matchs.Groups[1].Value, matchs.Groups[3].Value);
            }
            return csdConfigSiteUrl;
        }

        public ConfigSiteSetting GetConfigData()
        {
            var result = new ConfigSiteSetting();
            try
            {
                var factory = MultiAppUtil.CreateAveObjectModelFactory(mConfigSiteUrl, mBposInfo, AveContextKind.ClientObjectModel);
                using (var aveSite = factory.CreateSite(mConfigSiteUrl))
                {
                    using (var web = aveSite.OpenWeb())
                    {
                        this.mIsUpgrade = web.GetList(ListUrl_ModifiedBasedFileType) != null;
                        result.ExcludeList = GetItems(web, ListUrl_ExcludeListUrl);
                        result.ExcludeFileExtentions = GetItems(web, ListUrl_ExcludeFileTypeListUrl);
                        result.CSDRules = GetRules(web, ListUrl_Rules);
                        result.ExcludedFileTypeDefaultTerm = GetExcludeFileTypeDefaultTerm(web);
                        result.ModifiedBasedFileTypeMapping = this.mIsUpgrade ? 
                            GetModifiedBasedFileTypeMapping(web, ListUrl_ModifiedBasedFileType) 
                            : new Dictionary<string, IAveTerm>();
                        result.ModifiedBasedTermIds = this.mIsUpgrade ? 
                            result.CSDRules.Values.Where(r => r.IsModifiedBasedRule).Select(r => r.TermId).ToList() 
                            : new List<Guid>();
                        //result.ModifiedBasedFileExtentions = result.ModifiedBasedFileExtentionsTermMapping.Keys.ToList();
                    }
                }
            }
            catch (CSDConfigMissException e)
            {
                logger.Error($"GetConfigData error:{e.ToString()}");
                if (e.ConfigType == CSDConfigExceptionType.ExcludedFileTypesClassID)
                {
                    throw new Exception("RM_SS_CannotGetExcludedFileTypesClassID");
                }
                else
                {
                    throw new Exception("RM_SS_CannotConnectConfigSite");
                }
            }
            catch (Exception e)
            {
                logger.Error($"GetConfigData error:{e.ToString()}");
                throw new Exception("RM_SS_CannotConnectConfigSite");
            }
            return result;
        }

        private Dictionary<Guid, CSDRuleObject> GetRules(IAveWeb web, string listUrl)
        {
            var result = new Dictionary<Guid, CSDRuleObject>();
            var list = web.GetList(listUrl);
            var items = list.Items;
            foreach (var item in items)
            {
                var taxValue = (string)item[CSDFieldName.KSUClass];
                var termId = new Guid(taxValue.Substring(taxValue.LastIndexOf('|') + 1));
                if (!result.ContainsKey(termId))
                {
                    var csdRule = AssembleCSDRule(item);
                    if (csdRule != null)
                    {
                        result.Add(termId, csdRule);
                    }
                }
            }
            return result;
        }

        private CSDRuleObject AssembleCSDRule(IAveListItem item)
        {
            var taxValue = (string)item[CSDFieldName.KSUClass];
            var termId = new Guid(taxValue.Substring(taxValue.LastIndexOf('|') + 1));
            bool isModifiedBasedRule = this.mIsUpgrade ? Convert.ToBoolean(item[CSDFieldName.IsModifiedBased]) : false;
            if (!isModifiedBasedRule)
            {
                var creationRetention = GetRetentionSetting(item, CSDFieldName.CreationRetentionPeriod,
                                            CSDFieldName.CreationRetentionUnit, CSDFieldName.CreationRetentionLabel, !DataCenterUtil.Is21V());
                if (creationRetention != null)
                {
                    var eventRetention = GetRetentionSetting(item, CSDFieldName.EventRetentionPeriod,
                                            CSDFieldName.EventRetentionUnit, CSDFieldName.EventRetentionLabel, !DataCenterUtil.Is21V());
                    var labelForLockedDoc = item.Fields.ContainsField(CSDFieldName.LockedRetentionLabel) ?
                                                item[CSDFieldName.LockedRetentionLabel]?.ToString() : string.Empty;
                    var label4M2C = item.Fields.ContainsField(CSDFieldName.Label4ReclassModified2Creation) ?
                                                item[CSDFieldName.Label4ReclassModified2Creation]?.ToString() : string.Empty;
                    logger.Info($"ItemId:[{item.ID}] KSU Class:[{taxValue}] " +
                        $"CreationPeriodValue:[{creationRetention.Value}] CreationPeriodUnit:[{creationRetention.Unit.ToString()}] CreationLabelName:[{creationRetention.RetentionLabel}] " +
                        $"EventPeriodValue:[{eventRetention?.Value}] EventPeriodUnit:[{eventRetention?.Unit.ToString()}] EventLabelName:[{eventRetention?.RetentionLabel}]" +
                        $"RetentionLabelForLockedDocument:[{labelForLockedDoc}] RetentionLabel4ReclassModified2Creation:[{label4M2C}]");
                    return new CSDRuleObject()
                    {
                        TermId = termId,
                        IsModifiedBasedRule = isModifiedBasedRule,
                        CreationRetentionSetting = creationRetention,
                        EventRetentionSetting = eventRetention,
                        RetentionLabelForLockedDoc = labelForLockedDoc,
                        RetentionLabel4ReclassModified2Creation = label4M2C
                    };
                }
            }
            else
            {
                var modifiedBasedRetention = GetRetentionSetting(item, CSDFieldName.ModifiedBasedRetentionPeriod,
                                                CSDFieldName.ModifiedBasedRetentionUnit, CSDFieldName.ModifiedBasedRetentionLabel, !DataCenterUtil.Is21V());
                if (modifiedBasedRetention != null)
                {
                    logger.Info($"ItemId:[{item.ID}] KSU Class:[{taxValue}] " +
                        $"ModifiedBasedPeriodValue:[{modifiedBasedRetention.Value}] ModifiedBasedPeriodUnit:[{modifiedBasedRetention.Unit.ToString()}] ModifiedBasedLabelName:[{modifiedBasedRetention.RetentionLabel}]");
                    return new CSDRuleObject()
                    {
                        TermId = termId,
                        IsModifiedBasedRule = isModifiedBasedRule,
                        ModifiedBasedRetentionSetting = modifiedBasedRetention
                    };
                }
            }
            return null;
        }

        private RetentionSetting GetRetentionSetting(IAveListItem item, string periodColName, string unitColName, string labelColName, bool checkLabel)
        {
            int periodValue = Convert.ToInt32(item[periodColName]);
            string unitStr = Convert.ToString(item[unitColName]);
            PeriodUnit unit = PeriodUnit.None;
            bool unitIsAvailable = true;
            GetPeriodUnit(unitStr, ref unit, ref unitIsAvailable);
            if (periodValue > 0 && unitIsAvailable)
            {
                if (!checkLabel)
                {
                    return new RetentionSetting()
                    {
                        RetentionLabel = string.Empty,
                        Value = periodValue,
                        Unit = unit
                    };
                }
                else
                {
                    string labelName = Convert.ToString(item[labelColName]);
                    if (!string.IsNullOrEmpty(labelName))
                    {
                        return new RetentionSetting()
                        {
                            RetentionLabel = labelName,
                            Value = periodValue,
                            Unit = unit
                        };
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            else
            {
                return null;
            }
        }

        private void GetPeriodUnit(string periodUnitStr, ref PeriodUnit unit, ref bool unitIsAvailable)
        {
            if (string.IsNullOrEmpty(periodUnitStr))
            {
                unitIsAvailable = false;
            }
            else if (periodUnitStr.IndexOf("Day", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                unit = PeriodUnit.Days;
            }
            else if (periodUnitStr.IndexOf("Month", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                unit = PeriodUnit.Months;
            }
            else if (periodUnitStr.IndexOf("Year", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                unit = PeriodUnit.Years;
            }
            else
            {
                unitIsAvailable = false;
            }
        }

        private List<string> GetItems(IAveWeb web, string listUrl)
        {
            var result = new List<string>();
            var list = web.GetList(listUrl);
            var items = list.Items;
            foreach (var item in items)
            {
                string itemName = item[CSDFieldName.Title].ToString().ToLowerInvariant();
                result.Add(itemName);
                logger.Info($"Library url:[{listUrl}] Item Name:[{itemName}]");
            }
            return result;
        }

        private string GetItemValueByKey(IAveWeb web, string listUrl, string keyName)
        {
            var list = web.GetList(listUrl);
            var items = list.Items;
            foreach (var item in items)
            {
                //string itemName = item[CSDFieldName.Title].ToString().ToLowerInvariant();
                //result.Add(itemName);
                //logger.Info($"Library url:[{listUrl}] Item Name:[{itemName}]");
                if (item[CSD_AppSettingKey].ToString() == keyName)
                {
                    return item[CSD_AppSettingValue].ToString();
                }
            }
            return string.Empty;
        }

        private Dictionary<string, IAveTerm> GetModifiedBasedFileTypeMapping(IAveWeb web, string listUrl)
        {
            Dictionary<string, IAveTerm> results = new Dictionary<string, IAveTerm>();
            var list = web.GetList(listUrl);
            var items = list.Items;
            foreach (var item in items)
            {
                string fileType = item[CSDFieldName.Title].ToString().ToLowerInvariant();
                string termIdStr = item[CSDFieldName.ModifiedBasedDefaultTerm]?.ToString().ToLowerInvariant();
                logger.Info($"Library url: [{listUrl}] File Extention: [{fileType}] Default Term id of modified based file type: [{termIdStr}]");
                IAveTerm term = null;
                Guid termId;
                if (Guid.TryParse(termIdStr, out termId))
                {
                    term = GetTerm(web.Site, termId);
                }
                else
                {
                    logger.Warn($"The term id is not GUID. Term id string:[{termIdStr}]");
                }
                if (!results.ContainsKey(fileType))
                {
                    results.Add(fileType, term);
                }
            }
            return results;
        }

        private IAveTerm GetTerm(IAveSite site, Guid termId)
        {
            var termStore = site.AveSPTaxonomySession.TermStores[0];
            var term = termStore.GetTerm(termId);
            return term;
        }

        private IAveTerm GetExcludeFileTypeDefaultTerm(IAveWeb web)
        {
            try
            {
                var termIdStr = GetItemValueByKey(web, ListUrl_GeneralConfigurations, KeyName_WhiteListDocumentClassId);
                var excludedFileTypeDefaultCSDClassID = Guid.Parse(termIdStr);
                var term = GetTerm(web.Site, excludedFileTypeDefaultCSDClassID);
                if (term == null)
                {
                    throw new Exception($"Can not find the term. Term Id: [{termIdStr}]");
                }
                return term;
            }
            catch (Exception e)
            {
                throw new CSDConfigMissException(CSDConfigExceptionType.ExcludedFileTypesClassID, $"ID of Default CSD Class for Files in the Excluded File Types error: {e.ToString()}");
            }
        }
    }

    public class ConfigSiteSetting
    {
        public List<string> ExcludeList;
        public List<string> ExcludeFileExtentions;
        //public List<string> ModifiedBasedFileExtentions;
        public Dictionary<Guid, CSDRuleObject> CSDRules;
        public IAveTerm ExcludedFileTypeDefaultTerm;
        public Dictionary<string, IAveTerm> ModifiedBasedFileTypeMapping;
        public List<Guid> ModifiedBasedTermIds;
        //public IAveTerm ModfiedBasedDefaultTerm;
    }

    public class CSDRuleObject
    {
        public Guid TermId;
        public RetentionSetting CreationRetentionSetting;
        public RetentionSetting EventRetentionSetting;
        public RetentionSetting ModifiedBasedRetentionSetting;
        public string RetentionLabelForLockedDoc;
        public string RetentionLabel4ReclassModified2Creation;
        public bool IsModifiedBasedRule;
        public DateTime CalculateDeletionDate(DateTime baseTime, RetentionSetting rs)
        {
            switch (rs.Unit)
            {
                case PeriodUnit.Days:
                    return baseTime.AddDays(rs.Value);
                case PeriodUnit.Months:
                    return baseTime.AddMonths(rs.Value);
                case PeriodUnit.Years:
                    return baseTime.AddYears(rs.Value);
                default:
                    throw new Exception("The unit in RetentionSetting is wrong.");
            }
        }
    }

    public class RetentionSetting : CSDPeriod
    {
        public string RetentionLabel { get; set; }
    }

    public class CSDPeriod
    {
        public int Value { get; set; }
        public PeriodUnit Unit { get; set; }
    }

    public enum PeriodUnit
    {
        None = 0,
        Days = 1,
        Months = 2,
        Years = 3
    }

    public enum CSDAction
    {
        None = 0,
        UpdateDeletionDate = 1,
        UpdateRetentionLabel = 2
    }
    public enum CSDActionResult
    {
        None = 0,
        UpdatedDeletionDate = 1,
        UpdatedRetentionLabel = 2,
        UpdatedBoth = 3
    }

    public class OneNoteFileType
    {
        public const string One = "one";
        public const string Onetoc2 = "onetoc2";
    }
    public class CSDFieldName
    {
        public const string Title = "Title";
        public const string KSUClass = "CSD_KSUClass";
        public const string CreationRetentionPeriod = "CSD_CreationRetentionPeriod";
        public const string CreationRetentionUnit = "CSD_CreationRetentionUnit";
        public const string CreationRetentionLabel = "CSD_CreationRetentionLabel";
        public const string EventRetentionPeriod = "CSD_EventRetentionPeriod";
        public const string EventRetentionUnit = "CSD_EventRetentionUnit";
        public const string EventRetentionLabel = "CSD_EventRetentionLabel";
        public const string LockedRetentionLabel = "CSD_LockedRetentionLabel";
        public const string Label4ReclassModified2Creation = "CSD_Label4Modified2Creation";

        public const string ModifiedBasedRetentionPeriod = "CSD_ModifiedBasedRetentionPeriod";
        public const string ModifiedBasedRetentionUnit = "CSD_ModifiedBasedRetentionUnit";
        public const string ModifiedBasedRetentionLabel = "CSD_ModifiedBasedRetentionLabel";
        public const string IsModifiedBased = "CSD_IsModifiedBased";

        public const string Comments = "RevIMComments";
        public const string EventDate = "RevIMEventDate";
        public const string DeletionDate = "RevIMDeletionDate";
        public const string DocOwner = "RevIMDocumentOwner";
        public const string Extends = "RevIMExtends";
        public const string BCSColumn = "RevIMBCS";
        public const string ReclassDateOfModified2Creation = "RevIMDateOfModified2Creation";

        public const string Created = "Created";
        public const string Author = "Author";
        public const string RetentionLabel = "_ComplianceTag";

        public const string ModifiedBasedDefaultTerm = "CSD_DefaultModifiedBasedClassId";
    }
}
