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




namespace AvePoint.GCommon.Media.StorageService
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    #endregion using directives

    internal class XmlMetaDataBuilder
        : MetaDataBuilderBase
    {
        String metaDataTemplate = @"<?xml version=""1.0"" encoding=""utf-8"" ?>"
                                + @"<configuration>"
                                    + @"<configSections>"
                                      + @"<section name=""metaDataHandler"" type=""AvePoint.GCommon.Media.StorageService.MetaDataSectionHandler, CommonMediaStorageService"" />"
                                    + @"</configSections>"
                                    + @"<metaDataHandler>"
                                      + @"<metaData>"
                                        + @"{0}"
                                        + @"{1}"
                                      + @"</metaData>"
                                    + @"</metaDataHandler>"
                                + @"</configuration>";

        public override Byte[] Build(MetaData metaData)
        {
            var stringBuilder = new StringBuilder();
            var stringValue = new StringBuilder();
            var properties = metaData.GetType().GetProperties().ToList()
                .FindAll(item => item.GetAttribute<HoldServiceMetaDataAttribute>() != null);
            for (int i = 0; i < properties.Count; i++)
            {
                var attribute = properties[i].GetAttribute<HoldServiceMetaDataAttribute>();
                var member = properties[i].FastGetValue(metaData) ?? String.Empty;
                stringValue.AppendFormat(@"<metaDataItem key=""{0}"" value=""{1}""/>", TransferXMLChar(attribute.Name), TransferXMLChar(member.ToString()));
            }
            var metaInfoList = new List<MetaDataItemInfo>(metaData.MetadataInfo);
            metaInfoList.ForEach(item =>
            {
                stringBuilder.AppendFormat(@"<metaDataInfo key=""{0}"" value=""{1}"" type=""{2}""/>", TransferXMLChar(item.Name), TransferXMLChar(item.Value), TransferXMLChar(item.ItemType.AssemblyQualifiedName));
            });
            return Encoding.UTF8.GetBytes(metaDataTemplate.FormatWith(stringValue.ToString(), stringBuilder.ToString()));
        }

        private string TransferXMLChar(string rawAttribute)
        {
            if (rawAttribute == null)
            {
                return "";
            }
            rawAttribute = ReplaceString(rawAttribute, "&", "&amp;");
            rawAttribute = ReplaceString(rawAttribute, "<", "&lt;");
            rawAttribute = ReplaceString(rawAttribute, ">", "&gt;");
            rawAttribute = ReplaceString(rawAttribute, "'", "&apos;");
            rawAttribute = ReplaceString(rawAttribute, "\"", "&quot;");
            return rawAttribute;
        }

        private String ReplaceString(String strData, String regex, String replacement)
        {
            if (strData == null)
            {
                return null;
            }
            int index;
            index = strData.IndexOf(regex, StringComparison.OrdinalIgnoreCase);
            String strNew = "";
            if (index >= 0)
            {
                while (index >= 0)
                {
                    strNew += strData.Substring(0, index) + replacement;
                    strData = strData.Substring(index + regex.Length);
                    index = strData.IndexOf(regex, StringComparison.OrdinalIgnoreCase);
                }
                strNew += strData;
                return strNew;
            }
            return strData;
        }
    }
}