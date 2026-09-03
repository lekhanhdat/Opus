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
    using AvePoint.Wrapper.Core.Util;
    using AvePoint.GCommon.Utility;
    using System.Diagnostics.CodeAnalysis;

    #region Field Mapping Info -- Internal use
    /// <summary>
    /// 用于存放并构造column mapping的信息，对应界面上Column Mapping(The columns can be changed according to following mappings)的一个Tab
    /// </summary>
    abstract class FieldMappingInfo
    {
        /// <summary>
        /// 原端Internal Name
        /// </summary>
        internal string SourceInternalName { get; set; }
        /// <summary>
        /// 目的端Internal Name
        /// </summary>
        internal string DestinationInternalName { get; set; }
        /// <summary>
        /// 原端Display Name
        /// </summary>
        internal string SourceDisplayName { get; set; }
        /// <summary>
        /// 目的端Display Name
        /// </summary>
        internal string DestinationDisplayName { get; set; }
        /// <summary>
        /// Field value mapping. <source,dest>
        /// todo:oliver cultureIgnoreCase
        /// </summary>
        internal Dictionary<string, string> ValueMapping = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
        /// <summary>
        /// 所引用的MappingCondition, 不要Clone新的对象，只保存一份引用
        /// </summary>
        internal MappingCondition MappingCondition { get; set; }

        internal virtual void Load(XmlElement xmlInfo)
        {
            this.SourceInternalName = xmlInfo.GetAttributeEx("sourceName");
            this.DestinationInternalName = xmlInfo.GetAttributeEx("destinationName");
            this.SourceDisplayName = xmlInfo.GetAttributeEx("sourceDisplayName");
            this.DestinationDisplayName = xmlInfo.GetAttributeEx("destinationDisplayName");
            XmlNode valueMappingsNode = xmlInfo.Cast<XmlNode>().Where(child => child is XmlElement && string.Equals(child.Name, "ValueMappings", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            LoadValueMappings(valueMappingsNode);
        }

        private void LoadValueMappings(XmlNode valueMappingsNode)
        {
            if (valueMappingsNode == null)
            {
                return;
            }
            foreach (XmlNode n in valueMappingsNode.ChildNodes)
            {
                var ele = n as XmlElement;
                if (ele != null && ele.Name.Equals("ValueMapping", StringComparison.OrdinalIgnoreCase))
                {
                    string sourceValue = ele.GetAttributeEx("sourceValue", true);
                    string destinationValue = ele.GetAttributeEx("destinationValue", true);
                    ValueMapping[sourceValue] = destinationValue;
                }
            }
        }

      

        internal virtual SPFieldInfo ConvertToSPFieldInfo()
        {
            return new SPFieldInfo()
            {
                DisplayName = this.DestinationDisplayName,
                InternalName = this.DestinationInternalName,
            };
        }
    }

    class SameTypeFieldMappingInfo : FieldMappingInfo { }

    class ChangeToDestinationFieldMappingInfo : FieldMappingInfo { }

    class ChangeToMetadataFieldMappingInfo : FieldMappingInfo
    {
        const string TaxonomyFieldTypeString = "TaxonomyFieldType";
        internal string TermSetPath { get; set; }
        internal bool AllowMultiValue { get; set; }
        internal string SeparateChar { get; set; }

        private object isTermInfoInitLock = new object();
        private bool isTermInfoInit = false;

        internal string TermGroup { get { InitTermInfo(); return this.termGroup; } }
        internal string TermSet { get { InitTermInfo(); return this.termSet; } }
        internal string Terms { get { InitTermInfo(); return this.terms; } }

        private string termGroup;
        private string termSet;
        private string terms;

        private void InitTermInfo()
        {
            if (!isTermInfoInit)
            {
                lock (isTermInfoInitLock)
                {
                    if (!isTermInfoInit)
                    {
                        var termInfo = GenerateTermInfo(this.TermSetPath);
                        this.termGroup = termInfo.Item1;
                        this.termSet = termInfo.Item2;
                        this.terms = termInfo.Item3;
                    }
                }
            }
        }
        internal override void Load(XmlElement xmlInfo)
        {
            base.Load(xmlInfo);
            var settingNode = xmlInfo.Cast<XmlNode>().Where(child => child is XmlElement && string.Equals(child.Name, "Setting", StringComparison.OrdinalIgnoreCase)).First() as XmlElement;
            TermSetPath = settingNode.GetAttributeEx("termSetPath", true);
            if (!IsTermSetPathValid(TermSetPath))
            {
                throw new ArgumentException("TermSetPath", TermSetPath);
            }
            AllowMultiValue = Boolean.Parse(settingNode.GetAttributeEx("allowMultiValue"));
            SeparateChar = settingNode.GetAttributeEx("separateChar");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">TermSetPath is null or string.Empty</exception>
        /// <exception cref="ArgumentException">TermSetPath is invalid</exception>
        internal override SPFieldInfo ConvertToSPFieldInfo()
        {
            var termInfo = GenerateTermInfo(this.TermSetPath);
            return new SPMetadataFieldInfo()
            {
                DisplayName = this.DestinationDisplayName,
                InternalName = this.DestinationInternalName,
                SeparateChar = this.SeparateChar,
                TermGroup = this.TermGroup,
                TermSet = this.TermSet,
                Terms = this.Terms,
                TypeAsString = TaxonomyFieldTypeString,
                AllowMultiValue = this.AllowMultiValue,
            };

        }
        /// <summary>
        /// 工具方法尽量不要引用类成员变量，减少依赖， 在调用该方法之前需要保证termSetPath合法，否则会有不可预知的异常
        /// </summary>
        /// <param name="termSetPath">TermGroup;TermSet[;Term1][;Term2][;Term3]...</param>
        /// <returns>(termGroup,termSet;terms)</returns>
        /// <exception cref="ArgumentNullException">termSetPath is null or string.Empty</exception>
        /// <exception cref="ArgumentException">termSetPath is invalid</exception>
        private static Tuple<string, string, string> GenerateTermInfo(string termSetPath)
        {
            if (string.IsNullOrEmpty(termSetPath))
            {
                return new Tuple<string, string, string>(string.Empty, string.Empty, string.Empty);
            }
            string termGroup = null;
            string termSet = null;
            string terms = null;

            var paths = termSetPath.Split(new char[] { ':', ';' }, StringSplitOptions.RemoveEmptyEntries);
            termGroup = paths[0];
            termSet = paths[1];
            terms = HasTermGroupAndTermSetAndTerm(paths.Length) ?
                GetTermsPath(termSetPath, termGroup.Length + 1 + termSet.Length)//TermGroup;TermSet
                : string.Empty;

            return new Tuple<string, string, string>(termGroup, termSet, terms);
        }
        #region 处理Term字符串
        private static string GetTermsPath(string termSetPath, int termGroupAndTermSetLength)
        {
            return termSetPath.Substring(termGroupAndTermSetLength + 1);
        }

        private static bool IsTermSetPathValid(string termSetPath)
        {
            if (string.IsNullOrEmpty(termSetPath))
            {
                return true;
            }
            return IsTermSetPathValid(termSetPath.Split(new char[] { ':', ';' }, StringSplitOptions.RemoveEmptyEntries).Length);
        }

        private static bool IsTermSetPathValid(int length)
        {
            //todo:oliver term中是否有冒号引号
            return HasTermGroupAndTermSetAndTerm(length) || OnlyHasTermGroupAndTermSet(length);
        }

        private static bool HasTermGroupAndTermSetAndTerm(int length)
        {
            return length >= 3;
        }

        private static bool OnlyHasTermGroupAndTermSet(int length)
        {
            return length == 2;
        }
        #endregion
    }

    class ChangeToLookupFieldMappingInfo : FieldMappingInfo
    {
        internal string ListTitle { get; set; }
        internal string FieldName { get; set; }
        internal string SeparateChar { get; set; }
        public bool AllowMultiValue { get; set; }

        internal override void Load(XmlElement xmlInfo)
        {
            base.Load(xmlInfo);
            var settingNode = xmlInfo.Cast<XmlNode>().Where(child => child is XmlElement && string.Equals(child.Name, "Setting", StringComparison.OrdinalIgnoreCase)).FirstOrDefault() as XmlElement;
            ListTitle = settingNode.GetAttributeEx("listTitle", true);
            FieldName = settingNode.GetAttributeEx("columnName", true);
            AllowMultiValue = Boolean.Parse(settingNode.GetAttributeEx("allowMultiValue"));
            SeparateChar = settingNode.GetAttributeEx("separateChar");
        }

        internal override SPFieldInfo ConvertToSPFieldInfo()
        {
            return new SPLookupFieldInfo()
            {
                InternalName = this.DestinationInternalName,
                DisplayName = this.DestinationDisplayName,
                ListTitle = this.ListTitle,
                FieldName = this.FieldName,
                AllowMultiValue = this.AllowMultiValue,
                SeparateChar = this.SeparateChar,
                TypeAsString = "Lookup",
            };
        }
    }

    /// <summary>
    /// 创建FieldMapping的工厂类,支持通过xml,todo:oliver?其他方式 实例化FieldMapping对象.
    /// 只包含基本信息，不包括Condition
    /// </summary>
    static class FieldMappingInfoFactory
    {
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "lower case used in swith-case.")]
        internal static FieldMappingInfo Create(XmlElement xmlInfo)
        {
            string type = xmlInfo.GetAttribute("type");
            FieldMappingInfo info = null;
            switch (type.ToLowerInvariant())
            {
                case "sametype":
                    info = new SameTypeFieldMappingInfo();
                    break;
                case "changetometadata":
                    info = new ChangeToMetadataFieldMappingInfo();
                    break;
                case "changetodes":
                    info = new ChangeToDestinationFieldMappingInfo();
                    break;
                case "changetolookup":
                    info = new ChangeToLookupFieldMappingInfo();
                    break;
                default:
                    //不需要国际化。
                    throw new ArgumentException("type is invalid, only support SameType, ChangeToMetadata, ChangeToDes, ChangeToLookUp", "xmlInfo");
            }
            info.Load(xmlInfo);
            return info;
        }
    }
    #endregion
}
