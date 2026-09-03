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
    class NintexFormValueFormat
    {
        private int originalVersion;
        private static AveLogger logger = AveLogger.GetInstance(typeof(NintexFormValueFormat));
        private Dictionary<Guid, AveNintexFormControlType> uniqueIdMapping;
        private Dictionary<string, AveNintexFormControlType> displayNameMapping;
        private AveSPList mList;
        public NintexFormValueFormat(IAveField destField, AveSPList mList, string contentTypeId, int originalVersion)
        {
            var listId = mList.SPList.ID;
            this.mList = mList;
            var parentWeb = mList.ParentWeb;
            var tempContentTypeId = contentTypeId.ToLower();
            if (parentWeb.NintexFormControlTypeCache.ContainsKey(listId)
             && parentWeb.NintexFormControlTypeCache[listId].ContainsKey(tempContentTypeId))
            {
                uniqueIdMapping = parentWeb.NintexFormControlTypeCache[listId][tempContentTypeId].Item1;
                displayNameMapping = parentWeb.NintexFormControlTypeCache[listId][tempContentTypeId].Item2;
            }
            this.originalVersion = originalVersion;
        }

        private void FormatData(XmlNode node, AveNintexFormControlType controlType)
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
        /// <summary>
        /// Format Hyperlink value.
        /// Source: Url, display name
        ///         Example:  https://test.com, DisplayName
        /// </summary>
        /// <param name="sourceValue"></param>
        /// <returns></returns>
        private string FormatHyperlinkValue(string sourceValue)
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
            var mappedUrl = AveReplaceProcessor.UrlReplace(result[0], mList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings,
                                                               new ReplaceOption(true, true), mList.ParentSite.SourceSiteInfo, mList.ParentSite.ServerRelativeUrl);
            return string.Format("{0},{1}", mappedUrl, result[1]);
        }

        private string FormatData(string value)
        {
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
        public object CheckFieldValue(object value)
        {
            if (uniqueIdMapping == null)
            {
                return value;
            }

            var formData = FormatData(value.ToString());

            //on-premise to online, 当目的端不存在NFFormData这个column时,需要走post action还原
            if (!this.mList.SPList.Fields.ContainsFieldWithInternalName("NFFormData"))
            {
                mList.AveFields.ResetNintexFormDataFieldValue(new AveNintexFormDataFieldInfo { FormData = formData.ToString(), Version = originalVersion });
                return string.Empty;
            }
            return formData;
        }

        private string FormatPeopleValue(string sourceValue)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return string.Empty;
            }
            StringBuilder newUsers = new StringBuilder();
            try
            {
                string[] users;
                int userId;
                users = sourceValue.Split(new string[] { ";#" }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(u => int.TryParse(u, out userId)).ToArray();
                foreach (var id in users)
                {
                    try
                    {
                        int.TryParse(id, out userId);
                        //先从备份数据还原Principal，备份数据不存在则直接EnsureUser。
                        var member = mList.ParentSite.SPMembers.FindMember(userId);
                        if (member != null)
                        {
                            newUsers.Append(string.Format("{0};#{1};#", member.ID, member.LoginName));
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("Can not find this user, user id: {0}, Error: {1}", id, ex);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while format people value, error message: {0}", e);
            }
            if (newUsers.Length > 2)
            {
                newUsers.Length -= 2;
            }
            return newUsers.ToString();
        }
    }
}
