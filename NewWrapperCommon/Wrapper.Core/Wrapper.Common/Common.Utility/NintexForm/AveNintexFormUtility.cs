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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace AvePoint.Wrapper.Common
{
    public class AveNintexFormUtility
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveNintexFormUtility));

        public static List<string> mUnSupportedFormControlTypeUniqueId = new List<string> {
                "7733d5bf-11c6-4bdc-a430-79c3065a796c",//Sql Request local默认支持
                "aeada2b6-24ad-46e2-894f-562c2a01d38a",//Web Request local 默认支持
                "ff9f65fe-f979-4312-a35b-50f0d3769069",//Change Content Type local 默认支持
                "c0a89c70-0781-4bd4-8623-f73675005e21",//External Data Column  在同一个Service下，是支持的
                "2c285c16-d4e6-49eb-8a6a-d9aa41e9e71b",//List Item   online不支持，local需要做替换。
                "4420d111-8869-49bb-8685-c1b6cdec4873",//List View   online不支持，local需要做替换。
                "2212c7db-a29d-4666-86dd-14e8ad4b3fc9",//Workflow Diagram   online不支持，local默认支持
            };

        private static XmlNode mPlaceHolderNode = null;
        private static XmlNode placeHolderNode
        {
            get
            {
                if (mPlaceHolderNode == null)
                {
                    var stream = Assembly.GetCallingAssembly().GetManifestResourceStream("AvePoint.Wrapper.Common.Common.Utility.NintexForm.PlaceholderLabel.txt");
                    using (var sr = new StreamReader(stream))
                    {
                        var xd = new XmlDocument();
                        xd.LoadXml(sr.ReadToEnd());
                        XmlNamespaceManager nsManager = new XmlNamespaceManager(xd.NameTable);
                        nsManager.AddNamespace("ns", "http://schemas.datacontract.org/2004/07/Nintex.Forms"); //namespace is in export file
                        nsManager.AddNamespace("d2p1", "http://schemas.datacontract.org/2004/07/Nintex.Forms.FormControls"); //namespace is in export file
                        XmlNode formControls = xd.SelectSingleNode("/ns:Form/ns:FormControls", nsManager);
                        mPlaceHolderNode = formControls.ChildNodes[0];
                    }
                }
                return mPlaceHolderNode;
            }
            set
            {
                mPlaceHolderNode = value;
            }
        }
        private static string placeHolderContentForRepsonive = null;
        private static string PlaceHolderContentForRepsonive
        {
            get
            {
                if (string.IsNullOrEmpty(placeHolderContentForRepsonive))
                {
                    var stream = Assembly.GetCallingAssembly().GetManifestResourceStream("AvePoint.Wrapper.Common.Common.Utility.NintexForm.ResponsivePlaceholderLabel.txt");
                    using (var sr = new StreamReader(stream))
                    {
                        placeHolderContentForRepsonive = sr.ReadToEnd();
                    }
                }
                return placeHolderContentForRepsonive;
            }
        }
        private static XmlNode GetPlaceHolderNodeForResponsive(string prefix)
        {
            var stream = Assembly.GetCallingAssembly().GetManifestResourceStream("AvePoint.Wrapper.Common.Common.Utility.NintexForm.PlaceholderLabel.txt");
            using (var sr = new StreamReader(stream))
            {
                var content = sr.ReadToEnd();
                content = content.Replace("d2p1", prefix);

                var xd = new XmlDocument();
                xd.LoadXml(content);
            }
            return null;
        }
        private static XmlNamespaceManager mnsManager = null;
        private static XmlNamespaceManager nsManager
        {
            get
            {
                if (mnsManager == null)
                {
                    mnsManager = new XmlNamespaceManager(placeHolderNode.OwnerDocument.NameTable);
                    mnsManager.AddNamespace("ns", "http://schemas.datacontract.org/2004/07/Nintex.Forms"); //namespace is in export file
                    mnsManager.AddNamespace("d2p1", "http://schemas.datacontract.org/2004/07/Nintex.Forms.FormControls"); //namespace is in export file
                }
                return mnsManager;
            }
        }
        public static string ReplaceContent(XmlDocument document, IAveList list, Func<string, string> UrlReplace)
        {
            ReplaceContent(document.DocumentElement, UrlReplace);
            return ReplaceItemProperty(document.InnerXml, list);
        }

        private static void ReplaceContent(XmlNode node, Func<string, string> UrlReplace)
        {
            XmlElement nodeElement = node as XmlElement;
            if (nodeElement == null)
            {
                return;
            }
            DecodeNodeValue(nodeElement);
            ReplaceUrl(nodeElement, UrlReplace);

            #region ADO-188307 Reset notsupport data
            ResetMaximumEntitiesValue(nodeElement);
            RemvoeNotSupportInsertReferenceNode(nodeElement);
            #endregion

            string variableExpression = string.Empty;
            bool hasExpressionValueNode = false;
            XmlNode expressionNode = null;
            foreach (XmlNode child in nodeElement.ChildNodes)
            {
                ReplaceContent(child, UrlReplace);
                if (string.Equals(child.Name, "Expression"))
                {
                    expressionNode = child;
                    variableExpression = child.InnerText;
                }
                if (string.Equals(child.Name, "ExpressionValue"))
                {
                    hasExpressionValueNode = true;
                }
            }

            if (!string.IsNullOrEmpty(variableExpression) && !hasExpressionValueNode)
            {
                var expressionValueNode = node.OwnerDocument.CreateElement("ExpressionValue", node.NamespaceURI);
                expressionValueNode.InnerText = variableExpression;
                node.InsertAfter(expressionValueNode, expressionNode);
            }
        }

        /// <summary>
        /// ADO-188309 由于On-premise和Online的差异，对于User类型的Column需要替换
        /// </summary>
        /// <param name="content"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private static string ReplaceItemProperty(string content, IAveList list)
        {
            if (list == null)
            {
                return content;
            }
            var resultContent = content;
            Regex reg = new Regex(@"(?<={ItemProperty:).*?(?=})");
            MatchCollection matches = reg.Matches(content);
            foreach (Match match in matches)
            {
                try
                {
                    var fieldInternalName = match.Value;
                    var field = list.Fields.GetFieldByInternalName(fieldInternalName, false);
                    if (field is IAveFieldUser)
                    {
                        var oldValue = "{ItemProperty:" + fieldInternalName + "}";
                        var newValue = "{ItemProperty:" + fieldInternalName + "DisplayName}";
                        resultContent = resultContent.Replace(oldValue, newValue);
                    }
                }
                catch (Exception e)
                {
                    logger.Debug("An error occurred while replace ItemProperty, error message: {0}", e);
                }
            }
            return resultContent;
        }
        private static void DecodeNodeValue(XmlElement nodeElement)
        {
            if (string.Equals("d2p1:Name", nodeElement.Name, StringComparison.OrdinalIgnoreCase)
                || string.Equals("d2p1:Text", nodeElement.Name, StringComparison.OrdinalIgnoreCase))
            {
                nodeElement.InnerText = System.Web.HttpUtility.HtmlDecode(nodeElement.InnerText);
            }
        }
        private static void ReplaceUrl(XmlElement nodeElement, Func<string, string> UrlReplace)
        {
            if (string.Equals("d2p1:ImageUrl", nodeElement.Name, StringComparison.OrdinalIgnoreCase))
            {
                nodeElement.InnerText = UrlReplace(nodeElement.InnerText);
            }
        }
        private static void ResetMaximumEntitiesValue(XmlElement nodeElement)
        {
            if (string.Equals("d2p1:MaximumEntities", nodeElement.Name, StringComparison.OrdinalIgnoreCase)
              && string.Equals("0", nodeElement.InnerText, StringComparison.OrdinalIgnoreCase))
            {
                nodeElement.InnerText = "25";
            }
        }
        private static void RemvoeNotSupportInsertReferenceNode(XmlElement nodeElement)
        {
            if (string.Equals("d2p1:InsertReferences", nodeElement.Name, StringComparison.OrdinalIgnoreCase))
            {
                foreach (XmlNode node in nodeElement.ChildNodes)
                {
                    foreach (XmlNode subNode in node.ChildNodes)
                    {
                        if (string.Equals("d4p1:Key", subNode.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            if (string.Equals("MaximumEntities", subNode.InnerText, StringComparison.OrdinalIgnoreCase)
                                || string.Equals("MaxLength", subNode.InnerText, StringComparison.OrdinalIgnoreCase))
                            {
                                nodeElement.RemoveChild(node);
                                break;
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="nintexFormXml"></param>
        /// <param name="replaceUnsupportedFormControl">if true, unsupported form control will be replaced with label form control</param>
        /// <returns></returns>
        public static string RemoverUnsupportedFormControl(string nintexFormXml, bool replaceUnsupportedFormControl)
        {
            XmlDocument xd = new XmlDocument();
            xd.LoadXml(nintexFormXml);
            XmlNode formControls = xd.SelectSingleNode("/ns:Form/ns:FormControls", nsManager);
            XmlNode formLayouts = xd.SelectSingleNode("/ns:Form/ns:FormLayouts", nsManager);


            #region handle XmlNode: FormControls
            Dictionary<XmlNode, XmlNode> needRemovedFormControls = new Dictionary<XmlNode, XmlNode>();
            foreach (XmlNode formControl in formControls.ChildNodes)
            {
                if (mUnSupportedFormControlTypeUniqueId.Contains(formControl.SelectSingleNode("d2p1:FormControlTypeUniqueId", nsManager).InnerText.ToLower()))
                {
                    if (replaceUnsupportedFormControl)
                    {
                        var newFormControl = GenerateLabelFormControl(formControl);
                        needRemovedFormControls.Add(formControl, newFormControl);
                    }
                    else
                    {
                        needRemovedFormControls.Add(formControl, null);
                    }
                }
            }
            if (replaceUnsupportedFormControl)
            {
                foreach (XmlNode key in needRemovedFormControls.Keys)
                {
                    formControls.ReplaceChild(needRemovedFormControls[key], key);
                }
            }
            else
            {
                foreach (XmlNode key in needRemovedFormControls.Keys)
                {
                    formControls.RemoveChild(key);
                }
            }
            #endregion

            #region handle XmlNode: FormLayouts
            if (!replaceUnsupportedFormControl)
            {
                Dictionary<XmlNode, List<XmlNode>> needRemovedFormLayouts = new Dictionary<XmlNode, List<XmlNode>>();
                foreach (XmlNode layout in formLayouts.ChildNodes)
                {
                    List<XmlNode> tempList = new List<XmlNode>();
                    foreach (XmlNode controlLayout in layout.SelectSingleNode("ns:FormControlLayouts", nsManager).ChildNodes)
                    {
                        if (ContainsXmlNode(needRemovedFormControls.Keys, nsManager, controlLayout.SelectSingleNode("ns:FormControlUniqueId", nsManager).InnerText.ToLower(CultureInfo.InvariantCulture)))
                        {
                            tempList.Add(controlLayout);
                        }
                    }
                    needRemovedFormLayouts.Add(layout, tempList);
                }
                foreach (XmlNode key in needRemovedFormLayouts.Keys)
                {
                    foreach (XmlNode node in needRemovedFormLayouts[key])
                    {
                        key.SelectSingleNode("ns:FormControlLayouts", nsManager).RemoveChild(node);
                    }
                }
            }
            #endregion

            #region
            var responsiveFormNode = xd.SelectSingleNode("/ns:Form/ns:ResponsiveForm", nsManager);
            if (responsiveFormNode != null)
            {
                ReplaceUnsupportedResponsiveNode("ns", responsiveFormNode);
            }
            #endregion
            return xd.InnerXml;
        }

        private static void ReplaceUnsupportedResponsiveNode(string prefix, XmlNode parentNode)
        {
            var rowContainers = parentNode.SelectNodes(string.Format("{0}:Rows/{0}:RowContainer", prefix), nsManager);

            foreach (XmlNode rowContainer in rowContainers)
            {
                Dictionary<XmlNode, XmlNode> needRemovedFormControls = new Dictionary<XmlNode, XmlNode>();
                var columns = rowContainer.SelectSingleNode(string.Format("{0}:Columns", "ns"), nsManager);
                var tempPrefix = columns.GetPrefixOfNamespace("http://schemas.datacontract.org/2004/07/Nintex.Forms.FormControls");
                nsManager.AddNamespace(tempPrefix, "http://schemas.datacontract.org/2004/07/Nintex.Forms.FormControls"); //namespace is in export file
                foreach (XmlNode formControl in columns.ChildNodes)
                {
                    if (mUnSupportedFormControlTypeUniqueId.Contains(formControl.SelectSingleNode(string.Format("{0}:FormControlTypeUniqueId", tempPrefix), nsManager).InnerText.ToLower()))
                    {
                        var placeholderNode = GenerateLabelNodeForResponsive(formControl, tempPrefix);
                        needRemovedFormControls.Add(formControl, placeholderNode);
                    }
                }
                foreach (var replaceNode in needRemovedFormControls)
                {
                    columns.ReplaceChild(replaceNode.Value, replaceNode.Key);
                }
                ReplaceUnsupportedResponsiveNode(prefix, rowContainer);
            }

        }
        private static XmlNode GenerateLabelNodeForResponsive(XmlNode oldFormControl, string prefix)
        {
            var attributeNode = oldFormControl.SelectSingleNode(string.Format("{0}:Attributes", prefix), nsManager);
            string labelContent = PlaceHolderContentForRepsonive.Replace("d5p1", prefix);
            XmlDocument labelDocument = new XmlDocument();
            labelDocument.LoadXml(labelContent);
            var labelNode = labelDocument.DocumentElement.FirstChild;
            labelNode = oldFormControl.OwnerDocument.ImportNode(labelNode, true);
            labelNode.AppendChild(attributeNode);
            return labelNode;
        }

        private static XmlNode GenerateLabelFormControl(XmlNode oldFormControl)
        {
            XmlNode resultNode = placeHolderNode.CloneNode(true);

            resultNode.SelectSingleNode("d2p1:UniqueId", nsManager).InnerText = oldFormControl.SelectSingleNode("d2p1:UniqueId", nsManager).InnerText;
            resultNode.SelectSingleNode("d2p1:DisplayName", nsManager).InnerText = "Placeholder " + (oldFormControl.SelectSingleNode("d2p1:DisplayName", nsManager) == null ? string.Empty : oldFormControl.SelectSingleNode("d2p1:DisplayName", nsManager).InnerText);
            resultNode.SelectSingleNode("d2p1:Text", nsManager).InnerText = "Placeholder " + (oldFormControl.SelectSingleNode("d2p1:Text", nsManager) == null ? string.Empty : oldFormControl.SelectSingleNode("d2p1:Text", nsManager).InnerText);
            resultNode = oldFormControl.OwnerDocument.ImportNode(resultNode, true);

            return resultNode;
        }

        private static bool ContainsXmlNode(Dictionary<XmlNode, XmlNode>.KeyCollection col, XmlNamespaceManager nsManager, string formControlUniqueId)
        {
            foreach (XmlNode node in col)
            {
                if (node.SelectSingleNode("d2p1:UniqueId", nsManager).InnerText.Equals(formControlUniqueId, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
