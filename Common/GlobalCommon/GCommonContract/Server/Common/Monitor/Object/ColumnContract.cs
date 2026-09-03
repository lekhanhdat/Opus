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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Server.Common.Monitor.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ColumnModel
    {
        public ColumnModel() { }
        public ColumnModel(string name)
            : this()
        {
            this.Name = name;
        }
        public ColumnModel(string name, string i18n)
            : this(name)
        {
            this.I18N = i18n;
        }

        public ColumnModel(string name, string i18n, Func<string> I18NExp)
            : this(name, i18n)
        {
            this.I18NExp = I18NExp;
        }

        public ColumnModel(string key, string name, string i18n, Func<string> I18NExp)
            : this(name, i18n, I18NExp)
        {
            this.Key = key;
        }

        public ColumnModel(string key, string name, string i18n, string style, Func<string> I18NExp)
            : this(key, name, i18n, I18NExp)
        {
            this.StyleKey = style;
        }

        public ColumnModel(string key, string name, string i18n, ColumnOption option, Func<string> I18NExp)
            : this(key, name, i18n, I18NExp)
        {
            this.Options = option;
        }

        public ColumnModel(string key, string name, string i18n, string style, ColumnOption option, Func<string> I18NExp)
            : this(key, name, i18n, style, I18NExp)
        {
            this.Options = option;
        }

        public ColumnModel(string name, string i18n, ColumnOption options, Func<string> I18NExp)
            : this(name, i18n, I18NExp)
        {
            this.Options = options;
        }

        public ColumnModel(string name, string i18n, ColumnOption options, List<Filter> filters, Func<string> I18NExp)
            : this(name, i18n, options, I18NExp)
        {
            this.Filters = filters;
        }

        public Func<string> I18NExp { get; set; }

        [DataMember]
        public string Key { get; set; }

        [DataMember]
        public string StyleKey { get; set; }
        /// <summary>
        /// 前台需要绑定列的属性
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// 返回前台的国际化信息
        /// </summary>
        [DataMember]
        public string I18N { get; set; }

        /// <summary>
        /// 静态Filter集
        /// </summary>
        [DataMember]
        public List<Filter> Filters { get; set; }

        /// <summary>
        /// 项集
        /// </summary>
        [DataMember]
        public ColumnOption Options { get; set; }

        /// <summary>
        /// 是否排序
        /// </summary>
        public bool Sortable { get { return (Options & ColumnOption.Sortable) == ColumnOption.Sortable; } }

        /// <summary>
        /// 不可隐藏
        /// </summary>
        public bool Unhidable { get { return (Options & ColumnOption.Unhidable) == ColumnOption.Unhidable; } }

        /// <summary>
        /// 是否过滤
        /// </summary>
        public bool StaticFilterable { get { return (Options & ColumnOption.StaticFilterable) == ColumnOption.StaticFilterable; } }


        /// <summary>
        /// 是否需要动态查询filterList
        /// </summary>
        public bool DynamicFilterable { get { return (Options & ColumnOption.DynamicFilterable) == ColumnOption.DynamicFilterable; } }

        /// <summary>
        /// 是否为默认隐藏
        /// </summary>
        public bool Bashful { get { return (Options & ColumnOption.Bashful) == ColumnOption.Bashful; } }

        public ColumnModel Clone()
        {
            return new ColumnModel()
            {
                I18N = this.I18N,
                Key = this.Key,
                Name = this.Name,
                StyleKey = this.StyleKey,
                Filters = this.Filters,
                Options = this.Options
            };
        }
    }

    [Flags, DataContract(Namespace = ContractConstants.Namespace)]
    public enum ColumnOption
    {
        [EnumMember]
        Nil = 0,
        [EnumMember]
        Sortable = 1,
        [EnumMember]
        Unhidable = 1 << 1,
        [EnumMember]
        StaticFilterable = 1 << 2,
        [EnumMember]
        DynamicFilterable = 1 << 3,
        [EnumMember]
        Bashful = 1 << 4,
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Filter
    {
        /// <summary>
        /// 用于前台显示的str
        /// </summary>
        [DataMember]
        public string Name { get; set; }
        /// <summary>
        ///  前台给后台返回时的值
        /// </summary>
        [DataMember]
        public string Value { get; set; }

        public Func<string> I18NExp { get; set; }

        public Filter() { }

        public Filter(string name, string val)
        {
            this.Name = name;
            this.Value = val;
        }

        public Filter(string name, int val)
        {
            this.Name = name;
            this.Value = val + "";
        }
        public Filter(string name, string val, Func<string> I18NExp)
            : this(name, val)
        {
            this.I18NExp = I18NExp;
        }

        public Filter(string name, int val, Func<string> I18NExp)
            : this(name, val)
        {
            this.I18NExp = I18NExp;
        }
    }
}
