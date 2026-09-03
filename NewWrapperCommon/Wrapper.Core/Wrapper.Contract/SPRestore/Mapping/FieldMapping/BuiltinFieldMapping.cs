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
    using AvePoint.Wrapper.Core.Common;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;

    /// <summary>
    /// Wrapper Builtin的Column Mapping实现。支持通过Condition过滤，对应Control Panel中的column mapping
    /// 通过工程构造，返回接口，外围不能直接构造。
    /// </summary>
    class BuiltinFieldMapping: IFieldMapping
    {
        /// <summary>
        /// 用于兼容并构造原端Column Mapping，原有实现弃用后删除
        /// </summary>
        [Obsolete]
        internal XmlDocument XDoc { get; private set; }
        private readonly Dictionary<string, List<FieldMappingInfo>> internalNameAndMappingInfo = new Dictionary<string, List<FieldMappingInfo>>();
        private readonly Dictionary<string, List<FieldMappingInfo>> displayNameAndMappingInfo = new Dictionary<string, List<FieldMappingInfo>>();
        
        /// <summary>
        /// 通过XmlDocument构造Mapping，兼用原有模式。需要增加一个通过对象构造的重载
        /// </summary>
        /// <param name="doc"></param>
        /// <exception cref="ArgumentNullException">doc is null, or must have element is missing</exception>
        public BuiltinFieldMapping(XmlDocument doc) 
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
            XmlNodeList nodes = xDoc.GetElementsByTagName("FieldMapping");
            
            foreach (XmlNode n in nodes)
            {
                XmlNode conditionNode = n.Cast<XmlNode>().Where(child => child is XmlElement && string.Equals(child.Name, "Condition", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                MappingCondition mappingCondition = new MappingCondition();
                mappingCondition.Load(conditionNode as XmlElement);
                //找不到Condition InvalidOperationException
                XmlNode mappingsNode = n.Cast<XmlNode>().Where(child => child is XmlElement && string.Equals(child.Name, "Mappings", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                LoadFieldMappings(mappingCondition, mappingsNode);
            }
            //todo:Oliver log xDoc诊断?异常处理?
        }

        private void LoadFieldMappings(MappingCondition mappingCondition, XmlNode mappingsNode)
        {
            if (mappingsNode == null || mappingsNode.ChildNodes.Count == 0)
            {
                throw new ArgumentNullException("Mappings");
            }
            foreach (XmlNode mappingNode in mappingsNode.ChildNodes)
            {
                FieldMappingInfo info = FieldMappingInfoFactory.Create(mappingNode as XmlElement);
                //要保证所有info都引用一个condition对象，不能clone，避免内存问题
                info.MappingCondition = mappingCondition;
                if (!string.IsNullOrEmpty(info.SourceInternalName))
                {
                    this.internalNameAndMappingInfo.AddNameMapping<FieldMappingInfo>(info.SourceInternalName, info);
                }
                if (!string.IsNullOrEmpty(info.SourceDisplayName))
                {
                    this.displayNameAndMappingInfo.AddNameMapping<FieldMappingInfo>(info.SourceDisplayName, info);
                }
            }
        }
        #endregion

        /// <summary>
        /// Get the field info after mapping
        /// </summary>
        /// <param name="sourceFieldInfo">SPConditionableFieldInfo对象, DisplayName和InternalName必须赋值。 MappingConditionInfo为condition条件, 如果为null则不进行filter</param>
        /// <returns>
        /// mapping之后的SPFieldInfo对象，DisplayName和InternalName为mapping之后的值。
        /// 支持返回子类SPMetadataFieldInfo，SPLookupFieldInfo
        /// </returns>
        public SPFieldInfo GetMappingFieldInfo(SPConditionableFieldInfo sourceFieldInfo)
        {
            if (this.internalNameAndMappingInfo.ContainsKey(sourceFieldInfo.InternalName))
            {
                foreach (var info in this.internalNameAndMappingInfo[sourceFieldInfo.InternalName])
                {
                    if (info.MappingCondition.IsQualified(sourceFieldInfo.ConditionInfo))
                    {
                        return info.ConvertToSPFieldInfo();
                    }                    
                }
            }
            if (this.displayNameAndMappingInfo.ContainsKey(sourceFieldInfo.DisplayName))
            {
                foreach (var info in this.internalNameAndMappingInfo[sourceFieldInfo.DisplayName])
                {
                    if (info.MappingCondition.IsQualified(sourceFieldInfo.ConditionInfo))
                    {
                        return info.ConvertToSPFieldInfo();
                    }

                }
            }
            return null;
        }

        public string GetMappingFieldValue(SPFieldValueInfo sourceFieldValueInfo)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Mapping中新创建的Column, builtin的mapping不支持新创建, 永远返回null
        /// </summary>
        /// <returns></returns>
        public System.Collections.Generic.List<SPFieldInfo> GetNewAddedFields()
        {
            return null;
        }

        public Dictionary<string, string> GetNewAddedFieldValues()
        {
            throw new NotImplementedException();
        }
    }

    internal static class MappingDictionayExtension
    {
        //internal static void AddInternalNameMapping(this Dictionary<string, List<FieldMappingInfo>> self, FieldMappingInfo info)
        //{
        //    SafeAddToMapping(self, info, info.SourceInternalName);
        //}

        //internal static void AddDisplayNameMapping(this Dictionary<string, List<FieldMappingInfo>> self, FieldMappingInfo info)
        //{
        //    SafeAddToMapping(self, info, info.SourceDisplayName);
        //}

        //private static void SafeAddToMapping(Dictionary<string, List<FieldMappingInfo>> self, FieldMappingInfo info, string key)
        //{
        //    if (!self.ContainsKey(key))
        //    {
        //        self.Add(key, new List<FieldMappingInfo>());
        //    }
        //    self[key].Add(info);
        //}


        internal static void AddNameMapping<T>(this Dictionary<string, List<T>> self,string key ,T value)
        {
            if (!self.ContainsKey(key))
            {
                self.Add(key, new List<T>());
            }
            self[key].Add(value);
        }
    }
        

}
