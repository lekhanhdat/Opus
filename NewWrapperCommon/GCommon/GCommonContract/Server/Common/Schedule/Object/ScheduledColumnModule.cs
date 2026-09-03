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
using System.Text;
using System.Xml.Serialization;
using System.ComponentModel;

namespace AvePoint.GCommon.Contract.Server.Common.Schedule.Object
{
    [XmlRoot(ScheduledDisplayModuleConstants.ScheduledColumnModule)]
    public class ScheduledColumnModule : INotifyPropertyChanged
    {
        /// <summary>
        /// 前台需要绑定列的属性
        /// </summary>
        [XmlAttribute(ScheduledDisplayModuleConstants.Name)]
        public string Name { get; set; }

        /// <summary>
        /// 返回前台的国际化信息
        /// </summary>
        [XmlAttribute(ScheduledDisplayModuleConstants.I18N)]
        public string I18N { get; set; }

        /// <summary>
        /// 当前列所占列的序号
        /// </summary>
        [XmlAttribute(ScheduledDisplayModuleConstants.Seq)]
        public string Header { get; set; }

        /// <summary>
        /// 当前列所占列的序号
        /// </summary>
        [XmlAttribute(ScheduledDisplayModuleConstants.PropertyName)]
        public string PropertyName { get; set; }

        private List<ScheduledFilter> filters;

        [XmlArray(ElementName = ScheduledDisplayModuleConstants.Filters)]
        public List<ScheduledFilter> Filters
        {
            get
            {
                if (filters == null)
                {
                    filters = new List<ScheduledFilter>();
                }
                return filters;
            }
            set
            {
                filters = value;
            }
        }

        /// <summary>
        /// 是否排序
        /// </summary>
        [XmlAttribute(ScheduledDisplayModuleConstants.IsSortable)]
        public bool IsSortable { get; set; }

        /// <summary>
        /// 是否过滤
        /// </summary>
        [XmlAttribute(ScheduledDisplayModuleConstants.IsFilterable)]
        public bool IsFilterable { get; set; }

        /// <summary>
        /// 是否需要动态查询filterList
        /// </summary>
        [XmlAttribute(ScheduledDisplayModuleConstants.IsDynamicDistinct)]
        public bool IsDynamicDistinct { get; set; }

        /// <summary>
        /// 与checkbox绑定
        /// </summary>
        [XmlAttribute(ScheduledDisplayModuleConstants.IsSelected)]
        public bool IsSelected { get; set; }

        /// <summary>
        /// 是否需要转换（此属性保留）
        /// </summary>
        [XmlIgnoreAttribute]
        public string ConverterName { get; set; }

        // Implement INotifyPropertyChanged interface.
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    [XmlRoot(ScheduledDisplayModuleConstants.Filter)]
    public class ScheduledFilter
    {
        /// <summary>
        /// 用于前台显示的str
        /// </summary>
        [XmlAttribute(ScheduledDisplayModuleConstants.Name)]
        public string Name { get; set; }
        /// <summary>
        ///  前台给后台返回时的值，由于可能类型不同，统一用string类型传递。
        /// </summary>
        [XmlAttribute(ScheduledDisplayModuleConstants.Value)]
        public string Value { get; set; }
    }

    public class ScheduledDisplayModuleConstants
    {
        public const string ScheduledColumnModule = "ScheduledColumnModule";
        public const string Columns = "Columns";
        public const string Name = "Name";
        public const string I18N = "I18N";

        public const string Category = "Category";
        public const string Seq = "Seq";
        public const string IsSortable = "IsSortable";
        public const string IsFilterable = "IsFilterable";
        public const string IsDynamicDistinct = "IsDynamicDistinct";
        public const string IsSelected = "IsSelected";
        public const string ScheduledJobMonitorModule = "ScheduledJobMonitorModule";
        public const string Modules = "Modules";
        public const string JobMonitorDisplay = "ScheduledJobMonitorDisplay";
        public const string Converter = "Converter";
        public const string Filters = "Filters";
        public const string Filter = "Filter";
        public const string Value = "Value";
        public const string PropertyName = "PropertyName";
    }
}
