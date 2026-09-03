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

namespace Microsoft365Backup.DataBuilder.TeamHtml
{
    using HtmlAgilityPack;
    using System;

    internal abstract class HtmlFormatter
    {
        protected readonly HtmlDocument doc;
        protected const string ATTRIBUTE_STYLE = "style";
        protected const string ATTRIBUTE_SRC = "src";
        protected const string ATTRIBUTE_TARGET = "target";

        protected const string ENDPOINT_GRAPH = "https://graph.microsoft";

        protected ConversationItem Item { get; private set; }

        protected HtmlFormatter(HtmlDocument doc, ConversationItem item)
        {
            this.doc = doc;
            Item = item;
        }

        /// <summary>
        /// 预处理HtmlDocument中的html片段。
        /// 此处不需要调用HtmlDocument.Save()，如需save只要返回true即可
        /// </summary>
        /// <returns>html片段是否被改变，即是否需要save</returns>
        public abstract bool Process();

        protected static bool RemoveSubStringInAttribute(HtmlNode node, string attribute, params string[] subStrings)
        {
            var value = node.GetAttributeValue(attribute, string.Empty);
            var newValue = Remove(value, subStrings);
            if (!string.Equals(value, newValue, StringComparison.OrdinalIgnoreCase))
            {
                node.SetAttributeValue(attribute, newValue);
                return true;
            }
            return false;
        }

        private static string Remove(string str, params string[] keys)
        {
            if (string.IsNullOrEmpty(str)) return str;
            foreach (var key in keys)
            {
                if (str.IndexOf(key) >= 0)
                {
                    str = str.Replace(key, string.Empty);
                }
            }
            return str;
        }
    }
}