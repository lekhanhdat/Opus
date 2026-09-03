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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.Records.Core.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Util
{
    public class RuleUtil
    {

        private static readonly RALogger logger = RALogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// For Move Action, if the dest location == source location,  we should ignore the rule.
        /// </summary>
        /// <param name="rules"></param>
        /// <param name="srcFolder"></param>
        /// <returns></returns>
        public static List<Rule> FilterMoveRules(List<Rule> rules, string srcFolder)
        {
            CodeContract.NullThrowing(rules, "rules");
            List<Rule> result = new List<Rule>();
            foreach (var rule in rules)
            {
                if (rule.FSRule != null && rule.FSRule.spMoveOption != null && rule.FSRule.spMoveOption.MoveDestination != null)
                {
                    CodeContract.NullThrowing(rule.FSRule.spMoveOption.MoveDestination, "Rule.FSMoveOption.MoveDestination");
                    string destUrl = rule.FSRule.spMoveOption.MoveDestination.DestMode == DestMode.UrlMode
                        ? rule.FSRule.spMoveOption.MoveDestination.FSPath
                        : GetFSPathFromTree(rule.FSRule.spMoveOption.MoveDestination.FSTreeNode);
                    if (string.IsNullOrWhiteSpace(destUrl) || destUrl.Eq(srcFolder))
                    {
                        logger.Info($"Current rule is FS Move and source url [{srcFolder}] is the same as des url[{destUrl}].");
                        continue;
                    }
                }
                result.Add(rule);
            }
            return result;
        }

        private static string GetFSPathFromTree(FSTreeNodeDto fSTreeNode)
        {
            if (fSTreeNode == null || string.IsNullOrWhiteSpace(fSTreeNode.FullPath))
            {
                return "";
            }

            return EncodeUtil.DecryptByCommunicationKey(fSTreeNode.FullPath);
        }

        public static void ModifyDisplayDateTimeByPolicyValue(DisplayDateTime displayDateTime, string policyValue, GeneralSettingModel gls)
        {
            if (displayDateTime != null)
            {
                string timeFormat = GeneralSettingConfig.TimeFormats[(TimeFormat)Enum.Parse(typeof(TimeFormat), Enum.GetName(typeof(TimeFormat), gls.TimeFormatId), true)];
                string dateFormat = GeneralSettingConfig.DateFormats[(DateFormat)Enum.Parse(typeof(DateFormat), Enum.GetName(typeof(DateFormat), gls.DataFormatId), true)];
                displayDateTime.DateTimeFormat = $"{dateFormat} {timeFormat}";
            }
            if (displayDateTime != null && displayDateTime.TimeZoneId == gls.TimeZoneId && displayDateTime.IsDayLightSaving == gls.DayLight)
            {
                logger.Info($"When the time zones are the same, no conversion is needed, time zone id:{displayDateTime.TimeZoneId}");
                return;
            }
            if (displayDateTime != null && !string.IsNullOrEmpty(displayDateTime.TimeZoneId))
            {
                displayDateTime.StartTime = DateTimeUtil.ConvertFromUTCDateTime(policyValue, gls, DateTimeUtil.DATETYPEForRuleFilter);
                displayDateTime.IsDayLightSaving = gls.DayLight;
                displayDateTime.TimeZoneId = gls.TimeZoneId;
            }
        }
    }
}
