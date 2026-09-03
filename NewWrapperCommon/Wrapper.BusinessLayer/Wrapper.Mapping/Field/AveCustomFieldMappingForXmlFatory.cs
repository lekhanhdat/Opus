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
using System.Xml;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon.Contract.CodeReview;

namespace AvePoint.Wrapper.Mapping
{
    [AveCodeReview("2012/04/13", "yuzhi.jiang@avepoint.com", "jin.zhang@AvePoint.com", new string[2] { CodeReviewConstants.CHECK_LIST_ID_CS_1, CodeReviewConstants.CHECK_LIST_ID_EH_2 }, null, true)]
    public class AveCustomFieldMappingForXmlFatory : IAveCustomFieldMappingFactory
    {
        /// <summary>
        /// 由于支持contenttype condition，必须在还原column时才能check条件是否符合，所以dictionary的value为一个list
        /// </summary>
        private Dictionary<AveMappingCondition, List<AveCustomFieldForXmlInfo>> customFieldMappings;
        public AveCustomFieldMappingForXmlFatory(XmlDocument xDoc)
        {
            this.Load(xDoc);
        }

        public IAveCustomFieldMapping GetMappingForListOrWeb(object listOrWeb)
        {
            Dictionary<AveSourceFieldInfo, List<AveCustomFieldForXmlInfo>> mappings = GetListCustomFieldMappings(listOrWeb);
            return new AveCustomFieldMappingForXml(mappings);
        }
        [Obsolete]
        public IAveCustomFieldMapping GetMappingForList(AveFieldMappingConditionInfo condition)
        {
            var mappings = GetListCustomFieldMappings(condition);
            return new AveCustomFieldMappingForXml(mappings);
        }
        IAveCustomFieldMapping IAveCustomFieldMappingFactory.GetMappingForList(IAveFieldMappingConditionInfo condition)
        {
            return GetMappingForList(condition as AveFieldMappingConditionInfo);
        }

        private void Load(XmlDocument xDoc)
        {
            customFieldMappings = new Dictionary<AveMappingCondition, List<AveCustomFieldForXmlInfo>>();
            XmlNodeList nodes = xDoc.GetElementsByTagName("FieldMapping");
            foreach (XmlNode n in nodes)
            {
                XmlNode conditionNode = n.Cast<XmlNode>().Where(child => child is XmlElement && string.Equals(child.Name, "Condition", StringComparison.OrdinalIgnoreCase)).First();
                AveMappingCondition mappingCondition = new AveMappingCondition();
                mappingCondition.Load(conditionNode as XmlElement, true);
                XmlNode mappingsNode = n.Cast<XmlNode>().Where(child => child is XmlElement && string.Equals(child.Name, "Mappings", StringComparison.OrdinalIgnoreCase)).First();
                List<AveCustomFieldForXmlInfo> customFieldInfos = new List<AveCustomFieldForXmlInfo>();
                foreach (XmlNode mappingNode in mappingsNode.ChildNodes)
                {
                    AveCustomFieldForXmlInfo info = AveCustomFieldForXmlInfo.CreateCustomFieldInfo(mappingNode as XmlElement);
                    info.Load(mappingNode as XmlElement);
                    info.GetCondition(mappingCondition);
                    customFieldInfos.Add(info);
                }
                customFieldMappings[mappingCondition] = customFieldInfos;
            }
        }

        /// <summary>
        /// 获取mapping 预先check部分条件
        /// </summary>
        /// <param name="listOrWeb"></param>
        /// <returns></returns>
        private Dictionary<AveSourceFieldInfo, List<AveCustomFieldForXmlInfo>> GetListCustomFieldMappings(object listOrWeb)
        {
            Dictionary<AveSourceFieldInfo, List<AveCustomFieldForXmlInfo>> CustomFieldMappings = new Dictionary<AveSourceFieldInfo, List<AveCustomFieldForXmlInfo>>(new AveCustomFieldInfoEqualityComparer());
            if (customFieldMappings != null)
            {
                foreach (AveMappingCondition condition in customFieldMappings.Keys)
                {
                    if (condition.CheckCondition(listOrWeb, Guid.Empty))
                    {
                        List<AveCustomFieldForXmlInfo> fieldInfos = customFieldMappings[condition];
                        foreach (AveCustomFieldForXmlInfo info in fieldInfos)
                        {
                            info.SetConditonsMappingSourceSPListOrWeb(listOrWeb);
                            ///internal和display分别mapping 实际只有一个mapping有效 internal name优先mapping
                            if (!string.IsNullOrEmpty(info.SourceName))
                            {
                                ///先根据info创建一个internal name的AveSourceFieldInfo
                                AveSourceFieldInfo internalNameKey = new AveSourceFieldInfo()
                                {
                                    SourceInternalName = info.SourceName,
                                    SourceDisplayName = string.Empty
                                };
                                if (!CustomFieldMappings.ContainsKey(internalNameKey))
                                {
                                    List<AveCustomFieldForXmlInfo> newCustomFieldForXmlInfoList = new List<AveCustomFieldForXmlInfo>();
                                    CustomFieldMappings.Add(internalNameKey, newCustomFieldForXmlInfoList);
                                }
                                CustomFieldMappings[internalNameKey].Add(info);
                            }
                            else if (!string.IsNullOrEmpty(info.SourceDisplayName))
                            {
                                ///再根据info创建一个displayName的AveSourceFieldInfo
                                AveSourceFieldInfo displayNameKey = new AveSourceFieldInfo()
                                {
                                    SourceInternalName = string.Empty,
                                    SourceDisplayName = info.SourceDisplayName
                                };
                                if (!CustomFieldMappings.ContainsKey(displayNameKey))
                                {
                                    List<AveCustomFieldForXmlInfo> newCustomFieldForXmlInfoList = new List<AveCustomFieldForXmlInfo>();
                                    CustomFieldMappings.Add(displayNameKey, newCustomFieldForXmlInfoList);
                                }
                                CustomFieldMappings[displayNameKey].Add(info);
                            }
                        }
                    }
                }
            }
            return CustomFieldMappings;
        }

        private Dictionary<AveSourceFieldInfo, List<AveCustomFieldForXmlInfo>> GetListCustomFieldMappings(AveFieldMappingConditionInfo sourceCondition)
        {
            var listCustomFieldMappings = new Dictionary<AveSourceFieldInfo, List<AveCustomFieldForXmlInfo>>(new AveCustomFieldInfoEqualityComparer());
            if (customFieldMappings != null)
            {
                foreach (AveMappingCondition condition in customFieldMappings.Keys)
                {
                    if (condition.CheckCondition(sourceCondition))
                    {
                        List<AveCustomFieldForXmlInfo> fieldInfos = customFieldMappings[condition];
                        foreach (AveCustomFieldForXmlInfo info in fieldInfos)
                        {
                            AveSourceFieldInfo key = new AveSourceFieldInfo()
                            {
                                SourceInternalName = info.SourceName,
                                SourceDisplayName = info.SourceDisplayName
                            };
                            if (!listCustomFieldMappings.ContainsKey(key))
                            {
                                List<AveCustomFieldForXmlInfo> newCustomFieldForXmlInfoList = new List<AveCustomFieldForXmlInfo>();
                                listCustomFieldMappings.Add(key, newCustomFieldForXmlInfoList);
                            }
                            listCustomFieldMappings[key].Add(info);

                        }
                    }
                }
            }
            return listCustomFieldMappings;
        }
    }
}
