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

namespace AvePoint.Wrapper.Core.SPRestore.Mapping
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using System.Linq;

    /// <summary>
    /// Wrapper Builtin的Column Mapping实现。支持通过Condition过滤，对应Control Panel中的column mapping
    /// 通过工程构造，返回接口，外围不能直接构造。
    /// </summary>
    class BuiltinColumnMapping: IFieldMapping
    {
        private readonly Dictionary<string, List<FieldMappingInfo>> InternalNameAndMappingInfo = new Dictionary<string, List<FieldMappingInfo>>();
        private readonly Dictionary<string, List<FieldMappingInfo>> DisplayNameAndMappingInfo = new Dictionary<string, List<FieldMappingInfo>>();
        /// <summary>
        /// 通过XmlDocument构造Mapping，兼用原有模式。需要增加一个通过对象构造的重载
        /// </summary>
        /// <param name="doc"></param>
        public BuiltinColumnMapping(XmlDocument doc) 
        {
            this.Load(doc);
        }


        private void Load(XmlDocument xDoc)
        {
            XmlNodeList nodes = xDoc.GetElementsByTagName("FieldMapping");
            foreach (XmlNode n in nodes)
            {
                XmlNode conditionNode = n.Cast<XmlNode>().Where(child => child is XmlElement && string.Equals(child.Name, "Condition", StringComparison.OrdinalIgnoreCase)).First();
                FieldMappingCondition mappingCondition = new FieldMappingCondition();
                mappingCondition.Load(conditionNode as XmlElement);
                XmlNode mappingsNode = n.Cast<XmlNode>().Where(child => child is XmlElement && string.Equals(child.Name, "Mappings", StringComparison.OrdinalIgnoreCase)).First();
                List<FieldMappingInfo> customFieldInfos = new List<FieldMappingInfo>();
                foreach (XmlNode mappingNode in mappingsNode.ChildNodes)
                {
                    FieldMappingInfo info = FieldMappingInfoFactory.Create(mappingNode as XmlElement);
                    //要保证所有info都引用一个condition对象，不能clone，避免内存问题
                    info.MappingCondition = mappingCondition;
                    if(!string.IsNullOrEmpty(info.SourceInternalName))
                    {
                        this.InternalNameAndMappingInfo.AddInternalNameMapping(info);
                    }
                    if(!string.IsNullOrEmpty(info.SourceDisplayName))
                    {
                        this.DisplayNameAndMappingInfo.AddDisplayNameMapping(info);
                    }
                }
            }
            //todo:Oliver log xDoc诊断?异常处理?
        }

        public SPFieldInfo GetMappingFieldInfo(SPConditionableFieldInfo sourceFieldInfo)
        {
            throw new System.NotImplementedException();
        }

        public string GetMappingFieldValue(SPFieldValueInfo sourceFieldValueInfo)
        {
            throw new System.NotImplementedException();
        }

        public System.Collections.Generic.List<SPFieldInfo> GetNewAddedField()
        {
            throw new System.NotImplementedException();
        }

       
    }

    internal static class FieldMappingDictionayExtension
    {
        internal static void AddInternalNameMapping(this Dictionary<string, List<FieldMappingInfo>> self, FieldMappingInfo info)
        {
            SafeAddToMapping(self, info, info.SourceInternalName);
        }

        internal static void AddDisplayNameMapping(this Dictionary<string, List<FieldMappingInfo>> self, FieldMappingInfo info)
        {
            SafeAddToMapping(self, info, info.SourceDisplayName);
        }

        private static void SafeAddToMapping(Dictionary<string, List<FieldMappingInfo>> self, FieldMappingInfo info, string key)
        {
            if (!self.ContainsKey(key))
            {
                self.Add(key, new List<FieldMappingInfo>());
            }
            self[key].Add(info);
        }
    }
        

}
