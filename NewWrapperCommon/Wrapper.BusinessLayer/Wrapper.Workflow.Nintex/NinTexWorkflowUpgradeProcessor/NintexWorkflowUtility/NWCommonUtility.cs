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
using AvePoint.Wrapper.Common;
using Native13NinTexWorkflowEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LS.SPWorkflowProcessor
{
    public static class NWCommonUtility
    {
        public static string TryGetTheValueOfPrimitiveValue(ActivityParameter activityPara, string defaultValue)
        {
            if (activityPara == null || activityPara.PrimitiveValue == null)
            {
                return defaultValue;
            }
            else
            {
                return activityPara.PrimitiveValue.Value;
            }
        }

        public static ActivityParameter GetActivityParameterByName(ActivityParameter[] parameters, string parameterName, bool throwException)
        {
            var parameter = parameters.FirstOrDefault(para => string.Equals(parameterName, para.Name, StringComparison.OrdinalIgnoreCase));
            if (throwException && parameter == null)
            {
                throw new AveWrapperBaseException(string.Format("Can not found param by name, param name is {0}", parameterName));
            }
            return parameter;

        }

        /// <summary>
        /// If content includes "{ItemProperty:XXX}", "{Common:XXX}" or &lt;a href...&gt;...&lt;/a&gt;, the words will be replaced with {X}, and param:references will return the collection that includes the words
        /// </summary>
        /// <param name="content"></param>
        /// <param name="references">The key of KeyValuePair is reference name, the value of KeyValuePair means if the reference is in the link:&lt;a href...&gt;...&lt;/a&gt;</param>
        /// <returns></returns>
        public static string ReplaceNintexWorkflowContent(string content, ref List<KeyValuePair<string, bool>> references)
        {
            Dictionary<int, KeyValuePair<string, bool>> referencesDic = new Dictionary<int, KeyValuePair<string, bool>>();

            Regex regex = new Regex("(({ItemProperty:+).*?([}]))|(({Common:+).*?([}]))|(({WorkflowVariable:+).*?([}]))");
            var matchedResult = regex.Matches(content);

            #region parse <a href...>...</a>
            var document = new HtmlAgilityPack.HtmlDocument();
            document.LoadHtml(content);
            if (document.DocumentNode.SelectNodes("//a") != null)
            {
                foreach (HtmlAgilityPack.HtmlNode node in document.DocumentNode.SelectNodes("//a"))
                {
                    var tempKey1 = node.LinePosition + content.Substring(node.LinePosition).IndexOf(">") + 1;
                    var tempKey2 = node.LinePosition + content.Substring(node.LinePosition).IndexOf("href=\"") + 6;
                    if (referencesDic.ContainsKey(tempKey1) || referencesDic.ContainsKey(tempKey2))
                    {
                        continue;
                    }

                    if (regex.Matches(node.InnerHtml).Count <= 0)
                    {
                        referencesDic[tempKey1] = new KeyValuePair<string, bool>(node.InnerHtml, true);
                    }
                    if (regex.Matches(node.Attributes["href"].Value).Count <= 0)
                    {
                        referencesDic[tempKey2] = new KeyValuePair<string, bool>(node.Attributes["href"].Value, true);
                    }
                }
            }
            #endregion

            foreach (Match match in matchedResult)
            {
                if (!referencesDic.ContainsKey(match.Index))
                {
                    referencesDic.Add(match.Index, new KeyValuePair<string, bool>(match.Value, false));
                }
            }
            List<KeyValuePair<int, KeyValuePair<string, bool>>> referencesList = (from objDic in referencesDic orderby objDic.Key ascending select objDic).ToList();
            List<string> tempList = new List<string>();
            int startIndex = 0;
            foreach (KeyValuePair<int, KeyValuePair<string, bool>> pair in referencesList)
            {
                tempList.Add(content.Substring(startIndex, pair.Key - startIndex));
                startIndex = pair.Key + pair.Value.Key.Length;
                references.Add(pair.Value);
            }
            tempList.Add(content.Substring(startIndex));

            #region replace content with {X}
            string[] tempArrary = tempList.ToArray();
            string resultStr = string.Empty;
            int index = 0;
            for (int i = 0; i < tempArrary.Length; i++)
            {
                if (i == tempArrary.Length - 1)
                {
                    resultStr = resultStr + tempArrary[i];
                }
                else
                {
                    resultStr = resultStr + tempArrary[i] + string.Format("{{{0}}}", index);
                    index++;
                }
            }
            #endregion

            return resultStr;
        }

        /// <summary>
        /// 根据{} 来split 具体例子如下：
        /// "123{465}789" -> List<string>{{"123"},{"{456}"},{"789"}}
        /// </summary>
        /// <param name="sourceValue"></param>
        /// <returns></returns>
        public static List<String> SplitString(string sourceValue)
        {
            var tempStr = sourceValue;
            List<string> parameters = new List<string>();
            var startIndex = tempStr.IndexOf("{");
            var endIndex = tempStr.IndexOf("}");
            while (startIndex >= 0 && endIndex >= 0)
            {
                var noNeedReplace = tempStr.Substring(0, startIndex);
                if (!string.IsNullOrEmpty(noNeedReplace))
                {
                    parameters.Add(noNeedReplace);
                }
                var parameter = tempStr.Substring(startIndex, endIndex - startIndex + 1);
                parameters.Add(parameter);
                tempStr = tempStr.Substring(endIndex + 1);
                startIndex = tempStr.IndexOf("{");
                endIndex = tempStr.IndexOf("}");
            }
            if (!string.IsNullOrEmpty(tempStr))
            {
                parameters.Add(tempStr);
            }
            return parameters;
        }
    }
}
