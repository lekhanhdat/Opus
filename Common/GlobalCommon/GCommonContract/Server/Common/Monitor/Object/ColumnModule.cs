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



using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace AvePoint.GCommon.Contract.Server.Common.Monitor.Object
{
    /// <summary>
    /// 每个列的配置信息
    /// </summary>
    [XmlRoot(DisplayModuleConstants.Column)]
    public class ColumnModule : INotifyPropertyChanged
    {
        /// <summary>
        /// 前台需要绑定列的属性
        /// </summary>
        [XmlAttribute(DisplayModuleConstants.Name)]
        public string Name { get; set; }

        /// <summary>
        /// 返回前台的国际化信息
        /// </summary>
        [XmlAttribute(DisplayModuleConstants.I18N)]
        public string I18N { get; set; }

        /// <summary>
        /// 当前列所占列的序号
        /// </summary>
        [XmlAttribute(DisplayModuleConstants.Seq)]
        public string Header { get; set; }

        /// <summary>
        /// 当前列所占列的序号
        /// </summary>
        [XmlAttribute(DisplayModuleConstants.PropertyName)]
        public string PropertyName { get; set; }

        private List<Filter> filters;

        [XmlArray(ElementName = DisplayModuleConstants.Filters)]
        public List<Filter> Filters
        {
            get
            {
                if (filters == null)
                {
                    filters = new List<Filter>();
                }
                return filters;
            }
            set
            {
                filters = value;
            }
        }

        /// <summary>
        /// 是否需要前台转换
        /// </summary>
        [XmlAttribute(DisplayModuleConstants.IsConvert)]
        public bool IsConvert { get; set; }

        /// <summary>
        /// 是否排序
        /// </summary>
        [XmlAttribute(DisplayModuleConstants.IsSortable)]
        public bool IsSortable { get; set; }

        /// <summary>
        /// 是否过滤
        /// </summary>
        [XmlAttribute(DisplayModuleConstants.IsFilterable)]
        public bool IsFilterable { get; set; }

        /// <summary>
        /// 是否需要动态查询filterList
        /// </summary>
        [XmlAttribute(DisplayModuleConstants.IsDynamicDistinct)]
        public bool IsDynamicDistinct { get; set; }

        /// <summary>
        /// 与checkbox绑定
        /// </summary>
        [XmlAttribute(DisplayModuleConstants.IsSelected)]
        public bool IsSelected { get; set; }

        // Implement INotifyPropertyChanged interface.
        public event PropertyChangedEventHandler PropertyChanged;

        /*private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }*/
    }

    public class DisplayModuleConstants
    {
        public const string Column = "Column";
        public const string Columns = "Columns";
        public const string Name = "Name";
        public const string I18N = "I18N";

        public const string Category = "Category";
        public const string Seq = "Seq";
        public const string IsConvert = "IsConvert";
        public const string IsSortable = "IsSortable";
        public const string IsFilterable = "IsFilterable";
        public const string IsDynamicDistinct = "IsDynamicDistinct";
        public const string IsSelected = "IsSelected";
        public const string JobMonitorModule = "JobMonitorModule";
        public const string Modules = "Modules";
        public const string JobMonitorDisplay = "JobMonitorDisplay";
        public const string Filters = "Filters";
        public const string Filter = "Filter";
        public const string Value = "Value";
        public const string PropertyName = "PropertyName";
    }
}
