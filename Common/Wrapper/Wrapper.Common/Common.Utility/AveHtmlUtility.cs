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
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace AvePoint.Wrapper.Common
{
    public class AveHtmlUtility
    {
        private const string AttributeSuffix = "=";

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Symbol used in html")]
        public static string StripHTML(string strHtml)
        {
            string[] aryReg = { @"<script[^>]*?>.*?</script>", @"<(\/\s*)?!?((\w+:)?\w+)(\w+(\s*=?\s*(([""'])(\\[""'tbnr]|[^\7])*?\7|\w+)|.{0})|\s)*?(\/\s*)?>", @"([\r\n])[\s]+", @"&(quot|#34);", @"&(amp|#38);", @"&(lt|#60);", @"&(gt|#62);", @"&(nbsp|#160);", @"&(iexcl|#161);", @"&(cent|#162);", @"&(pound|#163);", @"&(copy|#169);", @"&#(\d+);", @"-->", @"<!--.*\n" };
            string[] aryRep = { "", "", "", "\"", "&", "<", ">", " ", "\xa1", "\xa2", "\xa3", "\xa9", "", "\r\n", "" };
            string newReg = aryReg[0];
            string strOutput = strHtml;
            for (int i = 0; i < aryReg.Length; i++)
            {
                Regex regex = new Regex(aryReg[i], RegexOptions.IgnoreCase);
                strOutput = regex.Replace(strOutput, aryRep[i]);
            }
            strOutput = strOutput.Replace("<", "");
            strOutput = strOutput.Replace(">", "");
            strOutput = strOutput.Replace("\r\n", "");
            return strOutput;
        }

        public static List<Dictionary<string, string>> CollectAttributeValues(string html, string startElementName,string endElementName, string[] attributeNames)
        {
            List<Dictionary<string, string>> elements = new List<Dictionary<string, string>>();
            int startIndex = html.IndexOf(startElementName, StringComparison.OrdinalIgnoreCase);
            int endIndex = html.IndexOf(endElementName, StringComparison.OrdinalIgnoreCase);

            while (startIndex > 0 && startIndex < endIndex && endIndex <= html.Length)
            {
                int nextElementIndex = html.IndexOf(startElementName, ++startIndex, StringComparison.OrdinalIgnoreCase);
                nextElementIndex = nextElementIndex == -1 ? html.Length : nextElementIndex;
                Dictionary<string, string> element = new Dictionary<string, string>();
                foreach (string attributeName in attributeNames)
                {
                    int attributeIndex = html.IndexOf(attributeName + AttributeSuffix, startIndex, StringComparison.Ordinal);
                    if (attributeIndex >= 0 && attributeIndex <= endIndex)
                    {
                        string attributeValue = null;
                        attributeIndex = ReadAttributeValue(html, attributeIndex, out attributeValue);
                        if (attributeIndex < nextElementIndex)
                        {
                            element.Add(attributeName.Trim(), attributeValue);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                elements.Add(element);
                startIndex = nextElementIndex;
                endIndex = html.IndexOf(endElementName, startIndex, StringComparison.OrdinalIgnoreCase);
            }

            return elements;
        }



        public static Dictionary<string, string> CollectZoneIDAttribute(string html)
        {
            Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string startContent = ":WebPartZone";
            string endContent = "</ZoneTemplate>";
            int startIndex = html.IndexOf(startContent, StringComparison.OrdinalIgnoreCase);
            while (startIndex > 0 && startIndex < html.Length)
            {
                int nextIndex = html.IndexOf(endContent, ++startIndex, StringComparison.OrdinalIgnoreCase);
                if (nextIndex < 0) 
                {
                    break;
                }
                int attributeIndex = html.IndexOf(" ID" + AttributeSuffix, startIndex, StringComparison.OrdinalIgnoreCase);
                if (attributeIndex < 0)
                {
                    startIndex = html.IndexOf(startContent, ++nextIndex, StringComparison.OrdinalIgnoreCase);
                    continue;
                }
                string zoneId = null;
                attributeIndex = ReadAttributeValue(html, attributeIndex, out zoneId);
                attributeIndex = html.IndexOf("__WebPartId" + AttributeSuffix, ++startIndex, StringComparison.OrdinalIgnoreCase);
                while (attributeIndex > startIndex && attributeIndex < nextIndex)
                {
                    string webpartId;
                    int idValueIndex = ReadAttributeValue(html, attributeIndex, out webpartId);
                    if (webpartId.StartsWith("g_", StringComparison.OrdinalIgnoreCase))
                    {
                        result.Add(webpartId.Replace('_', '-').Substring(2), zoneId);
                    }
                    else
                    {
                        result.Add(webpartId.Trim('{', '}'), zoneId);
                    }
                    attributeIndex = html.IndexOf("__WebPartId" + AttributeSuffix, ++attributeIndex, StringComparison.OrdinalIgnoreCase);
                }
                startIndex = html.IndexOf(startContent, ++nextIndex, StringComparison.OrdinalIgnoreCase);
            }
            return result;
        }


        /// <summary>
        /// 从Html文本的节点中截取WebPart相关的信息
        /// </summary>
        /// <param name="node">webpartNode节点</param>
        /// <param name="defaultZoneID">当前webpart节点所在的webpartzone</param>
        /// <param name="properties">需要获取的属性</param>
        /// <param name="webpartOrder">webpart在当前zone的位置</param>
        /// <returns></returns>
        private static Dictionary<string, string> GetWebPartZoneIDAndParOrderFromNode(HtmlNode node, string defaultZoneID, string[] properties, ref int webpartOrder)
        {
            var webpartProperties = properties
                     .Where(prop => !string.IsNullOrEmpty(node.GetAttributeValue(prop, string.Empty)))
                     .ToDictionary(prop => prop, prop => node.GetAttributeValue(prop, string.Empty));
            string designClose = node.GetAttributeValue("__designer:IsClosed", string.Empty);
            if (designClose != string.Empty)
            {
                webpartProperties["IsClosed"] = designClose;
            }
            //string designValue = node.GetAttributeValue("__designer:Values", string.Empty);
            //if (!string.IsNullOrEmpty(designValue))
            //{//某些类型的WebPart，ZoneID和PartOrder存在__designer:Values节点中
            //    HtmlDocument designDoc = new HtmlDocument();
            //    designDoc.LoadHtml(System.Web.HttpUtility.HtmlDecode(designValue));
            //    HtmlNode zoneIdNodeFromDesigner = designDoc.DocumentNode.SelectSingleNode("p[@n='ZoneID']");
            //    if (zoneIdNodeFromDesigner != null)
            //    {
            //        var zoneIdFromDesigner = zoneIdNodeFromDesigner.GetAttributeValue("t", string.Empty);
            //        if (!string.IsNullOrEmpty(zoneIdFromDesigner))
            //        {
            //            webpartProperties["ZoneID"] = zoneIdFromDesigner;
            //            webpartProperties["PartOrder"] = (webpartOrder++).ToString();
            //            return webpartProperties;
            //        }
            //    }
            //}
            //var zoneIdNode = node.SelectSingleNode(".//zoneid");
            //if (zoneIdNode != null)
            //{//对于某些V2类型的WebPart，ZoneID和PartOrder以节点形式存在webpart子节点中
            //    webpartProperties["ZoneID"] = zoneIdNode.InnerText;
            //    var partOrder = node.SelectSingleNode(".//partorder").InnerText;
            //    webpartProperties["PartOrder"] = partOrder;
            //    webpartOrder = int.Parse(partOrder);
            //    webpartOrder++;
            //    return webpartProperties;
            //}
            //var zoneIdAttribute = node.SelectSingleNode(".//node()[@zoneid]");
            //if (zoneIdAttribute != null)
            //{//对于某些类型的webpart，ZoneId和PartOrder以属性形式存在webpart子节点中
            //    webpartProperties["ZoneID"] = zoneIdAttribute.GetAttributeValue("ZoneID", string.Empty);
            //    var partOrder = zoneIdAttribute.GetAttributeValue("PartOrder", webpartOrder.ToString());
            //    webpartProperties["PartOrder"] = partOrder;
            //    webpartOrder = int.Parse(partOrder);
            //    webpartOrder++;
            //    return webpartProperties;
            //}
            //string zoneId = node.GetAttributeValue("ZoneID", string.Empty);
            //if (!string.IsNullOrEmpty(zoneId))
            //{//某些类型的WebPart，ZoneId和PartOrder属性直接存在当前节点的属性中
            //    webpartProperties["ZoneID"] = zoneId;
            //    var partOrder = node.GetAttributeValue("PartOrder", webpartOrder.ToString());
            //    webpartProperties["PartOrder"] = partOrder;
            //    webpartOrder = int.Parse(partOrder);
            //    webpartOrder++;
            //    return webpartProperties;
            //}
            ////对于以上几种情况都取不到ZoneID的情况走如下逻辑
            ////对于v3类型的webpart，webpart本身没有zoneId的信息，需要通过他所在的webPartZone来获取相关信息
            //webpartProperties["ZoneID"] = defaultZoneID;
            ////无法获取到PartOrder信息， 他在WebPartZone里面的排序就是他的PartOrder
            //webpartProperties["PartOrder"] = Convert.ToString(webpartOrder++);
            return webpartProperties;
        }

        public static List<Dictionary<string, string>> CollectZoneIdAndPartOrders(string html, string pageContentInEditMode, string[] properties)
        {
            List<Dictionary<string, string>> zoneIdAndProperties = new List<Dictionary<string, string>>();
            HtmlDocument doc = new HtmlDocument();
            doc.OptionOutputOriginalCase = true;
            doc.LoadHtml(html);

            //获取所有的WebPartZone的信息
            var nodes = doc.DocumentNode.DescendantNodesAndSelf().Where(node => node.OriginalName.IndexOf(":WebPartZone") > 0);
            if (!nodes.Any())
            {
                return zoneIdAndProperties;
            }
            var allWebPartNodes = doc.DocumentNode.SelectNodes("//node()[@__webpartid]");
            foreach (HtmlNode webpartZone in nodes)
            {//遍历每一个WebPartZone
                var webparts = webpartZone.SelectNodes(".//node()[@__webpartid]");
                if (webparts == null)
                {
                    continue;
                }
                int webpartOrder = 0;
                foreach (var node in webparts)
                {
                    if (allWebPartNodes.Contains(node))
                    {
                        allWebPartNodes.Remove(node);
                    }
                    zoneIdAndProperties.Add(GetWebPartZoneIDAndParOrderFromNode(node, webpartZone.Id, properties, ref webpartOrder));
                }
            }
            //某些WebPart不包含在Zone下面，这种情况下ZoneId一般是wpz，如果有其他情况需要在这处理。
            int webpartOrderWithoutZone = 0;
            if (allWebPartNodes == null)
            {
                return zoneIdAndProperties;
            }
            foreach (var webPartNode in allWebPartNodes)
            {
                zoneIdAndProperties.Add(GetWebPartZoneIDAndParOrderFromNode(webPartNode, "wpz", properties, ref webpartOrderWithoutZone));
            }
            return zoneIdAndProperties;
        }

        private static int ReadAttributeValue(string html, int index, out string attributeValue)
        {
            while (html[index] != '"') { index++; };
            int attributeStartIndex = index + 1;
            while (html[++index] != '"') ;
            attributeValue = html.Substring(attributeStartIndex, index - attributeStartIndex);
            return index;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Symbol used in html")]
        public static string SimpleEncode(string data)
        {
            char[] ArraySeparator = new char[] { '"' };
            char[] NodeSeparator = new char[] { '<' };
            char[] PropertySeparator = new char[] { '>' };
            char[] SingleQuote = new char[] { '\'' };
            if (!string.IsNullOrEmpty(data))
            {
                data = data.Replace(ArraySeparator[0].ToString(), "&quot;");
                data = data.Replace(NodeSeparator[0].ToString(), "&lt;");
                data = data.Replace(PropertySeparator[0].ToString(), "&gt;");
                data = data.Replace(SingleQuote[0].ToString(), "&squot;");
            }
            return data;
        }
    }
}
