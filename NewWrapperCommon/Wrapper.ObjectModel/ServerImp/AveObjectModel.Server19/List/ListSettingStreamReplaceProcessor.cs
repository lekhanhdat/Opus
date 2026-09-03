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
using System.IO;
using System.Xml;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.ServerAPI2010;

namespace AvePoint.ObjectModel.Server19.List
{
    public class ListSettingStreamReplaceProcessor
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(ListSettingStreamReplaceProcessor));
        /// <summary>
        /// list column default value xml replace before added
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="listRelativeUrl"></param>
        /// <param name="fieldNameMapping"></param>
        /// <returns></returns>
        public static Stream ReplaceListColumnDefaultValueStream(Stream stream, IAveList list, AveBaseItemInfo baseinfo)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.ListSettingStreamReplaceProcessor.ReplaceListColumnDefaultValueStream"))
            {

                try
                {
                    IAveFieldMapping fieldMapping;
                    baseinfo.MappingManager.SiteMappingManager.TryGetValueFromListFieldsMapping(list.ID, out fieldMapping);
                    MemoryStream newStream = new MemoryStream();
                    XmlDocument defaultValueXml = new XmlDocument();
                    defaultValueXml.Load(stream);
                    //处理stream中的url属性
                    XmlElement defaultEle = (XmlElement)defaultValueXml.DocumentElement.FirstChild;
                    defaultEle.SetAttribute("href", list.RootFolder.ServerRelativeUrl);
                    foreach (XmlElement defaultValueEle in defaultValueXml.GetElementsByTagName("DefaultValue"))
                    {
                        string sourceFieldName = defaultValueEle.GetAttribute("FieldName");
                        var mappedName = fieldMapping.GetMappingRestoredFieldInternalName(sourceFieldName);
                        if (!string.Equals(sourceFieldName, mappedName, StringComparison.Ordinal))
                        {
                            defaultValueEle.SetAttribute("FieldName", mappedName);
                        }
                    }
                    defaultValueXml.Save(newStream);
                    newStream.TryToResetStreamPosition();
                    return newStream;
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.WARN, ServerAPIResource.XmlProcessingException, e);
                }
                return stream;

            }

        }

        /// <summary>
        /// list Information management policy settings xml replace before added
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="listRelativeUrl"></param>
        /// <returns></returns>
        public static Stream ReplaceListRetentionStream(Stream stream, AveBaseItemInfo info)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.ListSettingStreamReplaceProcessor.ReplaceListRetentionStream"))
            {

                try
                {
                    MemoryStream newStream = new MemoryStream();
                    XmlDocument retentionXml = new XmlDocument();
                    retentionXml.Load(stream);
                    //处理stream中的url属性
                    //XmlElement retentionEle = (XmlElement)retentionXml.DocumentElement.FirstChild;
                    foreach (XmlElement retentionEle in retentionXml.FirstChild.ChildElements())
                    {
                        string href = retentionEle.GetAttribute("href");
                        href = AveReplaceProcessor.UrlReplace(href, info.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true), info.MappingManager.SiteMappingManager.SourceSiteInfo, info.MappingManager.SiteMappingManager.DestSiteInfo.ServerRelativeUrl);
                        retentionEle.SetAttribute("href", href);
                    }
                    retentionXml.Save(newStream);
                    newStream.TryToResetStreamPosition();
                    return newStream;
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.WARN, ServerAPIResource.XmlProcessingException, e);
                }
                return stream;

            }

        }

        public static Stream ReplaceNintexAutoStartRulesStream(Stream stream, AveBaseItemInfo info)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.ListSettingStreamReplaceProcessor.ReplaceNintexAutoStartRulesStream"))
            {

                try
                {
                    MemoryStream newStream = new MemoryStream();
                    XmlDocument autoStartRulesXml = new XmlDocument();
                    autoStartRulesXml.Load(stream);
                    //<AutoStartRule RuleType="List" ServerRelativeUrl="/sites/wfADO-91524" IsEnabled="true">
                    //替换ServerRelativeUrl
                    XmlNodeList ruleNodes = autoStartRulesXml.GetElementsByTagName("AutoStartRule");
                    if (ruleNodes != null && ruleNodes.Count > 0)
                    {
                        foreach (XmlElement rule in ruleNodes)
                        {
                            if (rule.HasAttribute("ServerRelativeUrl"))
                            {
                                string url = rule.GetAttribute("ServerRelativeUrl");
                                url = AveReplaceProcessor.UrlReplace(url, info.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true), info.MappingManager.SiteMappingManager.SourceSiteInfo, info.MappingManager.SiteMappingManager.DestSiteInfo.ServerRelativeUrl);
                                rule.SetAttribute("ServerRelativeUrl", url);
                            }
                            var users = rule.SelectNodes("ItemUpdatedConditions/AutoStartCondition/Operand2/Value");
                            foreach (XmlElement userNode in users)
                            {
                                try
                                {
                                    var userName = XmlConvert.DecodeName(userNode.InnerText);
                                    var mappingedUserName = info.GetUserFromMapping(userName);
                                    if (!string.Equals(userName, mappingedUserName, StringComparison.CurrentCulture))
                                    {
                                        userNode.InnerText = mappingedUserName;
                                    }
                                }
                                catch (Exception e)
                                {
                                    logger.Debug("Do mapping user for workflow rule file failed. Value:{0}, Error:{1}", userNode.InnerText, e);
                                }
                            }
                        }
                    }
                    autoStartRulesXml.Save(newStream);
                    newStream.TryToResetStreamPosition();
                    return newStream;
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.WARN, ServerAPIResource.XmlProcessingException, e);
                }
                return stream;

            }

        }
    }
}
