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
using AvePoint.Wrapper.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace AvePoint.Wrapper.Core.SPRestore.Mapping
{
    public class BuiltinContentTypeMapping:IContentTypeMapping
    {
        /// <summary>
        /// 用于兼容并构造原端Column Mapping，原有实现弃用后删除
        /// </summary>
        [Obsolete]
        internal XmlDocument XDoc { get; private set; }

        private readonly Dictionary<string, List<ContentTypeMappingInfo>> nameAndMappingInfo = new Dictionary<string, List<ContentTypeMappingInfo>>();
        
        /// <summary>
        /// 通过XmlDocument构造Mapping，兼用原有模式。需要增加一个通过对象构造的重载
        /// </summary>
        /// <param name="doc"></param>
        /// <exception cref="ArgumentNullException">doc is null, or must have element is missing</exception>
        public BuiltinContentTypeMapping(XmlDocument doc) 
        {
            if (doc == null)
            {
                throw new ArgumentNullException("doc");
            }
            this.Load(doc);
        }
        
        #region Load method
        private void Load(XmlDocument xDoc)
        {
            this.XDoc = xDoc;
            WrapperLogger.Instance.WriteToLogWithResourceKey(WrapperLogger.Level.Verbose, WrapperResourceKey.Wrapper_ColumnMappingXml, xDoc.OuterXml);
            XmlNodeList nodes = xDoc.GetElementsByTagName("ContentTypeMapping");
            
            foreach (XmlNode n in nodes)
            {
                XmlNode conditionNode = n.Cast<XmlNode>().Where(child => child is XmlElement && string.Equals(child.Name, "Condition", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                MappingCondition mappingCondition = new MappingCondition();
                mappingCondition.Load(conditionNode as XmlElement);
                //找不到Condition InvalidOperationException
                XmlNode mappingsNode = n.Cast<XmlNode>().Where(child => child is XmlElement && string.Equals(child.Name, "Mappings", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                LoadContentTypeMappings(mappingCondition, mappingsNode);
            }
            //todo:yzshao log xDoc诊断?异常处理?
        }

        private void LoadContentTypeMappings(MappingCondition mappingCondition, XmlNode mappingsNode)
        {
            if (mappingsNode == null || mappingsNode.ChildNodes.Count == 0)
            {
                throw new ArgumentNullException("Mappings");
            }

            foreach (XmlNode mappingNode in mappingsNode.ChildNodes)
            {
                ContentTypeMappingInfo info = ContentTypeMappingInfo.Create(mappingNode as XmlElement);
                //要保证所有info都引用一个condition对象，不能clone，避免内存问题
                info.MappingCondition = mappingCondition;
                if (!string.IsNullOrEmpty(info.SourceName))
                {
                    this.nameAndMappingInfo.AddNameMapping<ContentTypeMappingInfo>(info.SourceName, info);
                }
            }
        }
        #endregion


        public SPContentTypeInfo GetMappingContentTypeInfo(SPConditionableContentTypeInfo sourceContentTypeInfo)
        {
            if (this.nameAndMappingInfo.ContainsKey(sourceContentTypeInfo.Name))
            {
                foreach (var info in this.nameAndMappingInfo[sourceContentTypeInfo.Name])
                {
                    if (info.MappingCondition.IsQualified(sourceContentTypeInfo.ConditionInfo))
                    {
                        return info.ConvertToSPContentTypeInfo();
                    }
                }
            }
            return sourceContentTypeInfo as SPContentTypeInfo;
        }
    }
}
