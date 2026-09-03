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
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;
using System.Xml;
using AvePoint.Common;
using AvePoint.GCommon;

namespace AvePoint.Wrapper.Restore
{
    abstract class NintexFormValueFormatBase:BaseDataFormat
    {
        protected static AveLogger logger = AveLogger.GetInstance(typeof(NintexFormValueFormatBase));
        protected Dictionary<Guid, AveNintexFormControlType> uniqueIdMapping;
        protected Dictionary<string, AveNintexFormControlType> displayNameMapping;
        public NintexFormValueFormatBase(AveXmlField xmlField, IAveField destField, AveSPItem mItem, string contentTypeId)
            : base(xmlField, destField, mItem)
        {
            var listId = mItem.ParentList.SPList.ID;
            var parentWeb = mItem.ParentWeb;
            var tempContentTypeId = contentTypeId.ToLower();
            if (parentWeb.NintexFormControlTypeCache.ContainsKey(listId)
             && parentWeb.NintexFormControlTypeCache[listId].ContainsKey(tempContentTypeId))
            {
                uniqueIdMapping = parentWeb.NintexFormControlTypeCache[listId][tempContentTypeId].Item1;
                displayNameMapping = parentWeb.NintexFormControlTypeCache[listId][tempContentTypeId].Item2;
            }
        }

        protected abstract string FormatPeopleValue(string sourceValue);

        /// <summary>
        /// Format Hyperlink value.
        /// Source: Url, display name
        ///         Example:  https://test.com, DisplayName
        /// </summary>
        /// <param name="sourceValue"></param>
        /// <returns></returns>
        protected string FormatHyperlinkValue(string sourceValue)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return string.Empty;
            }
            var result = sourceValue.Split(',');
            if (result.Length != 2)
            {
                return sourceValue;
            }
            var mappedUrl = AveReplaceProcessor.UrlReplace(result[0], mItem.ParentWeb.ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings,
                                                               new ReplaceOption(true, true), mItem.ParentSite.SourceSiteInfo, mItem.ParentSite.ServerRelativeUrl);
            return string.Format("{0},{1}", mappedUrl, result[1]);
        }

        protected void FormatData(XmlNode node, AveNintexFormControlType controlType)
        {
            switch (controlType)
            {
                case AveNintexFormControlType.People:
                    node.InnerText = FormatPeopleValue(node.InnerText);
                    break;
                case AveNintexFormControlType.Hyperlink:
                    node.InnerText = FormatHyperlinkValue(node.InnerText);
                    break;
                default:
                    break;
            }
        }
        public override object CheckFieldValue(object value)
        {
            if (uniqueIdMapping == null)
            {
                return value;
            }
            var nintexFormValueXml = value.ToString();
            XmlDocument document = new XmlDocument();
            document.LoadXml(nintexFormValueXml);
            foreach (XmlNode node in document.DocumentElement.ChildNodes)
            {
                var nodeName = XmlConvert.DecodeName(node.Name);
                if (AveTypeHelper.IsGuid(nodeName))
                {
                    var controlUniqueId = new Guid(nodeName);
                    if (uniqueIdMapping.ContainsKey(controlUniqueId))
                    {
                        FormatData(node, uniqueIdMapping[controlUniqueId]);
                    }
                }
                else if (displayNameMapping.ContainsKey(nodeName))
                {
                    FormatData(node, displayNameMapping[nodeName]);
                }
            }
            return document.InnerXml;
        }
    }
}
