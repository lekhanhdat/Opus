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
using RAFileSystem.FileSystem.Discovery.Tags.Contract;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RAFileSystem.FileSystem.Discovery.Tags.Condition
{
    internal class ArrayConditionHandler : ConditionHandler
    {
        public override ConditionCategory Category => ConditionCategory.Array;

        public override bool Handle(ConditionInfo info, object dataObject)
        {
            var logic = (ArrayConditionLogicType)info.Logic;
            var conditionValue = JsonConvert.DeserializeObject<List<string>>(info.Value);

            var dataValues = ConvertToList(dataObject);

            switch (logic)
            {
                case ArrayConditionLogicType.In:
                    return dataValues.Any(dataValue => IsContains(conditionValue, dataValue));
                case ArrayConditionLogicType.NotIn:
                    return dataValues.All(dataValue => !IsContains(conditionValue, dataValue));
                case ArrayConditionLogicType.TextMatchIn:
                    return dataValues.Any(dataValue => IsMatchIn(conditionValue, dataValue));
                case ArrayConditionLogicType.TextNotMatchIn:
                    return dataValues.All(dataValue => !IsMatchIn(conditionValue, dataValue));
                default:
                    throw new NotSupportedException($"The [{Category}] does not support {logic}.");
            }
        }
        private List<string> ConvertToList(object dataObject)
        {
            if(dataObject.GetType() == typeof(string))
            {
                return new List<string> { Convert.ToString(dataObject) };
            }
            else if(dataObject.GetType() == typeof(List<string>))
            {
                return (List<string>)dataObject;
            }
            throw new NotSupportedException($"The [{Category}] only supports string or List<string> types.");
        }

        private static bool IsContains(List<string> criteriaValue, string dataValue) => criteriaValue.Contains(dataValue, StringComparer.OrdinalIgnoreCase);

        private static bool IsMatchIn(List<string> criteriaValue, string dataValue)
        {
            var dataValueList = dataValue.Split('/').Where(data => !string.IsNullOrEmpty(data)).ToList();
            foreach (var value in criteriaValue)
            {
                if (dataValueList.Any(data => IsMatch(value, data))) 
                { 
                    return true;
                }
            }

            return false;
        }

        private static bool IsMatch(string criteriaValue, string dataValue)
        {
            criteriaValue = criteriaValue.ToLower();
            dataValue = dataValue.ToLower();
            var regexExpression = string.Empty;
            var wildcardPatterns = criteriaValue.Split(new string[] { @"\*" }, StringSplitOptions.None);
            var wildcardExpressions = wildcardPatterns.ToList().ConvertAll(item =>
            {
                var subWildcardPatterns = item.Split(new string[] { @"\?" }, StringSplitOptions.None);
                var subWildcardExpressions = subWildcardPatterns.ToList().ConvertAll(subItem =>
                {
                    var regex = new Regex("[.$^{\\[(|)*+?\\\\]");
                    return regex.Replace(subItem, (m) =>
                    {
                        if (m.Value == "?")
                        {
                            return ".";
                        }
                        else if (m.Value == "." || m.Value == "*")
                        {
                            return ".*";
                        }

                        return "\\" + m.Value;
                    });
                });
                return string.Join(@"\?", subWildcardExpressions);
            });
            var wildcardExpression = "^" + string.Join(@"\*", wildcardExpressions) + "$";
            var regexRule = new Regex(wildcardExpression);
            return regexRule.IsMatch(dataValue);
        }
    }
}
