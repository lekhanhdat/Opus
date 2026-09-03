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
using System;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;

namespace LS.SPWorkflowProcessor
{
    /// <summary>
    /// 由于On-premise和Online对相同的数据类型，condition不完全一致，
    /// 该类用于区分出无法支持的condition并throw Exception
    /// </summary>
    class NWConditionRuleChecker
    {
        private static IAveLogger logger = AveLogger.GetInstance(typeof(NWConditionRuleChecker));
        private INintexDataMappingManager dataMappingManager;
        public NWConditionRuleChecker(INintexDataMappingManager dataMappingManager)
        {
            this.dataMappingManager = dataMappingManager;
        }

        private IAveList GetParentList(ListLookup listLookup)
        {
            if (string.Equals(listLookup.SelectList, "[Current Item]", StringComparison.OrdinalIgnoreCase))
            {
                return dataMappingManager.GetParentList();
            }
            var sourceListId = new Guid(listLookup.SelectList);
            var web = dataMappingManager.GetParentWeb();
            return web.GetList(dataMappingManager.GetListIdFromMapping(sourceListId));
        }

        private bool CannotSupportCalculateField(ListLookup listLookup)
        {
            try
            {
                IAveList parentList = GetParentList(listLookup);
                var selectField = parentList.Fields.GetFieldByInternalName(listLookup.SelectField);
                //对于Calculate output type是Currency的情况 无法支持
                if (selectField is IAveFieldCalculated && ((IAveFieldCalculated)selectField).OutputType.Equals(AveFieldType.Currency))
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while check calculate field condition, error: {0}", e);
            }
            return false;

        }

        public void CheckCondition(DictionaryValue left, DictionaryValue condition)
        {
            if (CannotSupportCondition(left, condition))
            {
                throw new UnSupportedSettingException();
            }
        }

        private bool CannotSupportCondition(DictionaryValue left, DictionaryValue condition)
        {
            if (condition.Value.PrimitiveValue.Value.StringValue.StartsWith("Equal", StringComparison.OrdinalIgnoreCase)
             || condition.Value.PrimitiveValue.Value.StringValue.StartsWith("NotEqual", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (left.Value.ListLookup != null)
            {
                var listLookup = left.Value.ListLookup;
                if (string.Equals(listLookup.SelectFieldType, "URL", StringComparison.OrdinalIgnoreCase)
                 || string.Equals(listLookup.SelectFieldType, "User", StringComparison.OrdinalIgnoreCase)
                 || string.Equals(listLookup.SelectFieldType, "Lookup", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                //Calculate field type 是String
                if (string.Equals(listLookup.SelectFieldType, "String", StringComparison.OrdinalIgnoreCase))
                {
                    return CannotSupportCalculateField(listLookup);
                }
            }
            if (left.Value.Variable != null)
            {
                var variable = left.Value.Variable;
                if (string.Equals(variable.DataType, "User", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
