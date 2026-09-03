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
using AvePoint.Common.FilterEngine;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common.FileSystem;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Explorer.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RACloudFS.Util
{
    public class FSRuleUtil
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(FSRuleUtil));
        private List<Rule> mRules { get; set; }
        public FSRuleUtil(List<Rule> rules)
        {
            mRules = rules;
        }

        public void AssembleRule(Record record)
        {
            try
            {
                var filterObj = ConverDBRecord2FilterObj(record);
                var rules = mRules;
                var filteredRules = RuleUtil.FilterMoveRules(rules, record.DirPath).Where(x => x.FSRule != null).ToList();
                if (filteredRules.Count > 0)
                {
                    DisposalRuleEngine engine = new DisposalRuleEngine(filteredRules);
                    var rule = engine.MatchRule(filterObj);
                    if (rule != null)
                    {
                        record.RuleId = string.IsNullOrEmpty(rule.Id) ? Guid.Empty : new Guid(rule.Id);
                        record.RuleLevel = (int)rule.PolicyLevel;
                        record.DisposalDueDate = DueDateUtil.NextJob;
                        record.PreviosDisposalDueDate = DueDateUtil.NextJob;
                    }
                    else
                    {
                        var ruleObj = engine.MatchPotentialRule(filterObj, true);
                        if (ruleObj != null)
                        {
                            record.RuleId = new Guid(ruleObj.Item1.Id);
                            record.RuleLevel = (int)ruleObj.Item1.PolicyLevel;
                            record.DisposalDueDate = DateTime.UtcNow.Add(ruleObj.Item2).Ticks;
                            record.PreviosDisposalDueDate = DateTime.UtcNow.Add(ruleObj.Item2).Ticks;
                        }
                        else
                        {
                            record.RuleId = Guid.Empty;
                            record.RuleLevel = (int)PolicyLevel.None;
                            record.DisposalDueDate = 0;
                            record.PreviosDisposalDueDate = 0;
                        }
                    }
                }
                else
                {
                    logger.Info("No Results rules, make the rule obj empty");
                    record.RuleId = Guid.Empty;
                    record.RuleLevel = (int)PolicyLevel.None;
                    record.DisposalDueDate = 0;
                    record.PreviosDisposalDueDate = 0;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private ObjectInfoBase ConverDBRecord2FilterObj(Record record)
        {
            var metaInfo = JsonConvert.DeserializeObject<RecordMetaInfo>(record.MetaInfo);
            if (metaInfo.LastAccessTime == 0)
            {
                logger.Warn($"{record?.Id} LastAccessTime is 0, Need Recollect.");
            }
            if (string.IsNullOrEmpty(metaInfo.Owner))
            {
                logger.Warn($"{record?.Id} Owner is Empty, Need Recollect.");
            }
            FSFileInfo objectInfo = new FSFileInfo()
            {
                Name = Path.GetFileName(record.LeafName),
                Size = metaInfo.FileSize,
                Extension = Path.GetExtension(record.LeafName),
                AccessTime = new DateTime(metaInfo.LastAccessTime),
                Created = new DateTime(record.TimeCreated),
                Modified = new DateTime(record.TimeModified),
                Owner = metaInfo.Owner,
                FilePath = Path.Combine(record.DirPath, record.LeafName),
            };
            return objectInfo;
        }
    }
}
