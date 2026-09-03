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
using AvePoint.RA.Common.Hybrid;
using AvePoint.RA.Contract.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RAFileSystem.FileSystem.Discovery.Tags;
using RAFileSystem.FileSystem.Discovery.Tags.Contract;
using RAFileSystem.FileSystem.Discovery.V1.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace RAFileSystem.FileSystem.Discovery.V1.Analyzer
{
    public class FSDiscoveryTagRuleService
    {
        private static readonly AveLogger s_logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private static FSDiscoveryTagRuleService _instance;

        private static readonly object _lock = new object();

        private static FileShareTagProcessor _fileShareTagProcessor;

        private FSDiscoveryTagRuleService()
        {
            _fileShareTagProcessor = new FileShareTagProcessor();
        }

        public static FSDiscoveryTagRuleService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                            _instance = new FSDiscoveryTagRuleService();
                    }
                }
                return _instance;
            }
        }

        private static List<FSDiscoveryTagRuleInfo> _tagRules = new List<FSDiscoveryTagRuleInfo>();

        private static List<FSDiscoveryCustomRuleInfo> _customTagRules = new List<FSDiscoveryCustomRuleInfo>()
        {
             new FSDiscoveryCustomRuleInfo
            {
                Id = FSTagRuleConstants.R_CATEGORY_RULE_UNIQUE_ID,
            },
            new FSDiscoveryCustomRuleInfo
            {
                Id = FSTagRuleConstants.O_CATEGORY_RULE_UNIQUE_ID,
            },
            new FSDiscoveryCustomRuleInfo
            {
                Id = FSTagRuleConstants.T_CATEGORY_RULE_UNIQUE_ID,
            },
            new FSDiscoveryCustomRuleInfo
            {
                Id = FSTagRuleConstants.ROT_RULE_UNIQUE_ID,
            }
        };

        public void InitTagRuleInfos()
        {
            var dataJson = HybridApiClient.Instance.GetDiscoveryFSTagRuleInfos();
            if (!string.IsNullOrEmpty(dataJson))
            {
                _tagRules = JsonConvert.DeserializeObject<List<FSDiscoveryTagRuleInfo>>(dataJson);
                _tagRules.Where(item => item.NeedCalculation).ToList().ForEach(item =>
                {
                    var tagInfo = JsonConvert.DeserializeObject<TagInfo>(item.Definition);
                    if (!tagInfo.IsBuildIn)
                    {
                        var ruleInfo = JsonConvert.DeserializeObject<RuleInfo>(tagInfo.TagDefinition, new JsonSerializerSettings
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                        });
                        if (ruleInfo.Method != AnalyseMethod.DuplicateDocument && ruleInfo.Method != AnalyseMethod.Version)
                        {
                            _customTagRules.Add(new FSDiscoveryCustomRuleInfo
                            {
                                Id = item.Id,
                            });
                        }
                    }
                });
            }
        }

        public Dictionary<string, object> GetTagValues(FileInfo fileInfo)
        {
            var tags = new Dictionary<string, object>();
            foreach (var rule in _tagRules)
            {
                try
                {
                    var tagKey = rule.ToTagColumn();
                    object tagValue = null;
                    if (rule.NeedCalculation)
                    {
                        tagValue = _fileShareTagProcessor.GetDocumentTagValue(rule.Definition, fileInfo);
                    }
                    if (tagValue != null)
                    {
                        tags.Add(tagKey, tagValue);
                    }
                }
                catch (Exception ex)
                {
                    s_logger.Error($"An error occured while get tag value for file info [{fileInfo.Name.LogBase64()}]. Ex: {ex}");
                }
            }
            return tags;
        }

        public List<FSDiscoveryTagRuleInfo> GetTagRules() => _tagRules;

        public List<FSDiscoveryCustomRuleInfo> GetCustomTagRules() => _customTagRules;
    }
}
