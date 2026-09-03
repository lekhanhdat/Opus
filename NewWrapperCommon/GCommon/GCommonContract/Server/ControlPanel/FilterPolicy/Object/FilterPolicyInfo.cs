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
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Server.Common;
using System.ComponentModel;
using AvePoint.GCommon.Contract.CommonFilter;


namespace AvePoint.GCommon.Contract.Server.ControlPanel.FilterPolicy.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot(ElementName = "FilterPolicyInfo")]
    public class FilterPolicyInfo : IProfileContent, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {

            if (PropertyChanged != null)
            {

                PropertyChanged(this, new PropertyChangedEventArgs(info));

            }

        }


        [DataMember]
        [XmlAttribute("id")]
        public string Id { get; set; }

        [DataMember]
        [XmlAttribute("name")]
        public string Name { get; set; }

        [DataMember]
        [XmlArray("FilterItems")]
        public List<BaseFilterItem> FItems { get; set; }

        [DataMember]
        [XmlAttribute]
        public string FilterExpression { get; set; }

        [DataMember]
        [XmlAttribute]
        public FilterExpressionType ExpressionType { get; set; }

        [DataMember]
        public Dictionary<PolicyLevel, string> AndOrExpression { get; set; }

        [DataMember]
        [XmlAttribute]
        public PlanCategory Category { get; set; }

        private string description;
        [DataMember]
        [XmlAttribute]
        public string Description 
        {
            get
            {
                if (this.description == null)
                {
                    return string.Empty;
                }
                else
                {
                    return description;
                }
            }
            set
            {
                this.description = value;
            }
        }

        public override string ToString()
        {
            return this.Name;
        }


    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ValueFitler : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {

            if (PropertyChanged != null)
            {

                PropertyChanged(this, new PropertyChangedEventArgs(info));

            }

        }
        /// <summary>
        /// 这个属性对应FilterPolicyType中的DateTimeFilter类型
        /// </summary>
        [DataMember]
        [XmlElement("DateFilter")]
        public DateTimeFilter DateFilter { get; set; } 

        private TextFilterInfo _TextFilter;
        /// <summary>
        /// 这个属性对应FilterPolicyType中的TextFilter类型
        /// </summary>
        [DataMember]
        [XmlElement("TextFilter")]
        public TextFilterInfo TextFilter 
        {
            get
            {
                return this._TextFilter;
            }
            set
            {

                if (value != this._TextFilter)
                {
                    this._TextFilter = value;
                    NotifyPropertyChanged("TextFilter");
                }
            }
        }
        /// <summary>
        /// 这个属性对应FilterPolicyType中的NumberFilter
        /// </summary>
        [DataMember]
        [XmlElement("NumberFilter")]
        public NumberFilterInfo NumberFilter { get; set; }

        /// <summary>
        /// 这个属性对应FilterPolicyType中的UserFilter
        /// </summary>
        [DataMember]
        [XmlElement("UserFilter")]
        public UserFilterInfo UserFilter { get; set; }

        /// <summary>
        /// 这个属性对应FilterPolicyType中的ServiceFilter
        /// </summary>
        [DataMember]
        [XmlElement("ServiceFilter")]
        public ServiceFilterItem ServiceFilter { get; set; }
        
        /// <summary>
        /// 这个属性对应FilterPolicyType中的DomainFilter
        /// </summary>
        [DataMember]
        [XmlElement("DomainFilter")]
        public DomainFilterItem DomainFilter { get; set; }

        [DataMember]
        [XmlElement("BoolFilter")]
        public BoolFilterInfo BoolFilter { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BaseFilterItem : AvePoint.GCommon.Contract.CommonFilter.FilterPolicy, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public BaseFilterItem()
        {
            this.BeginTime = new DisplayDateTime();
            this.EndTime = new DisplayDateTime();
        }

        private void NotifyPropertyChanged(String info)
        {

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }

        }

        private FilterPolicyType _FilterType;

        [DataMember]
        [XmlAttribute]
        public FilterPolicyType FilterType
        {
            get
            {
                return this._FilterType;
            }
            set
            {

                if (value != this._FilterType)
                {
                    this._FilterType = value;
                    NotifyPropertyChanged("FilterType");
                }
            }
        }

        [DataMember]
        [XmlAttribute]
        public string FilterItemId { get; set; }


        private FilterCondition _Condition;
        [DataMember]
        [XmlAttribute]
        public FilterCondition FilterCondition
        {
            get
            {
                return this._Condition;
            }
            set
            {

                if (value != this._Condition)
                {

                    this._Condition = value;
                    NotifyPropertyChanged("FilterCondition");
                }
            }
        }

        /// <summary>
        /// 运算符属性
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public string Operator { get; set; }

        [DataMember]
        [XmlAttribute]
        public bool IsAnd { get; set; }

        [DataMember]
        public DisplayDateTime BeginTime { get; set; }

        [DataMember]
        public DisplayDateTime EndTime { get; set; }

        /// <summary>
        /// 标示Filter item的是include还是exclude.
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public FilterIncludeState IncludeState { get; set; }

        private SPFilterLevel _Level;
        /// <summary>
        /// sharepoint级别
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public SPFilterLevel FilterLevel 
        {
            get
            {
                return this._Level;
            }
            set
            {

                if (value != this._Level)
                {
                    this._Level = value;
                    NotifyPropertyChanged("FilterLevel");
                }
            }
        }

        private FileSystemLevel _FileSystemLevel;
        /// <summary>
        /// sql server data manager 模块使用
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public FileSystemLevel FileSystemLevel
        {
            get
            {
                return this._FileSystemLevel;
            }
            set
            {

                if (value != this._FileSystemLevel)
                {
                    this._FileSystemLevel = value;
                    NotifyPropertyChanged("FileSystemLevel");
                }
            }
        }
        private SPFilterRule _Rule;
        /// <summary>
        /// 过滤规则
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public SPFilterRule FilterRule
        {
            get
            {
                return this._Rule;
            }
            set
            {

                if (value != this._Rule)
                {

                    this._Rule = value;
                    NotifyPropertyChanged("FilterRule");
                }
            }
        }

        [DataMember]
        [XmlElement("ValueFilter")]
        public ValueFitler FilterValue { get; set; }


    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DisplayDateTime
    {
        [DataMember]
        [XmlElement("StartTime")]
        public string StartTime { get; set; }

        [DataMember]
        [XmlElement("TimeZoneId")]
        public string TimeZoneId { get; set; }

        [DataMember]
        [XmlElement("IsDayLightSaving")]
        public bool IsDayLightSaving { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ServiceFilterItem
    {
        [DataMember]
        [XmlAttribute]
        public ServiceFilterType Type { get; set; }
        [DataMember]
        [XmlAttribute]
        public string Username { get; set; }
        [DataMember]
        [XmlAttribute]
        public string Password { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DomainFilterItem
    {
        [DataMember]
        [XmlAttribute]
        public DomainFilterType Type { get; set; }
        [DataMember]
        [XmlAttribute]
        public string StartIPAddress { get; set; }
        [DataMember]
        [XmlAttribute]
        public string EndIPAddress { get; set; }
        [DataMember]
        [XmlAttribute]
        public string Username { get; set; }
        [DataMember]
        [XmlAttribute]
        public string Password { get; set; }
    }

    /*[DataContract(Namespace = ContractConstants.Namespace)]
    public class SPFilterItem : BaseFilterItem
    {
        
    }*/
        
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DateTimeFilter
    {
        /// <summary>
        /// FilterCondition的枚举为FromTo的时候，存储FromTime值
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public DateTime FromTime { get; set; }

        /// <summary>
        /// FilterCondition的枚举为FromTo的时候，存储ToTime值
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public DateTime ToTime { get; set; }

        /// <summary>
        /// FilterCondition的枚举为Before的时候，存储的日期值
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public DateTime BeforeTime { get; set; }
        
        /// <summary>
        /// FilterCondition的枚举为After的时候，存储的日期值
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public DateTime AfterTime { get; set; }

        /// <summary>
        /// FilterCondition的枚举为On的时候，存储的日期值
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public DateTime OnTime { get; set; }

        /// <summary>
        /// FilterCondition的枚举为Within的时候，存储时间单位
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public DateTimeUnit TimeUnit { get; set; }

        /// <summary>
        /// FilterCondition的枚举为Within的时候，存储的数值
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public String WithInValue { get; set; }

        /// <summary>
        /// 对应界面中ColumnName输入框
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public string ColumnName { get; set; }

        [DataMember]
        public string TimeZoneId { get; set; }

        [DataMember]
        public bool IsDayLightSaving { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class TextFilterInfo
    {
        [DataMember]
        [XmlAttribute]
        public string value { get; set; }

        /// <summary>
        /// 对应界面中ColumnName输入框
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public string ColumnName { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class NumberFilterInfo
    {
        [DataMember]
        [XmlAttribute]
        public int Number { get; set; }

        [DataMember]
        [XmlAttribute]
        public SizeUnit SizeUnit { get; set; }

        /// <summary>
        /// 对应界面中ColumnName输入框
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public string ColumnName { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class UserFilterInfo
    {
        [DataMember]
        [XmlAttribute]
        public string User { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BoolFilterInfo
    {
        [DataMember]
        [XmlAttribute]
        public BoolFilterType BoolFilterType { get; set; }

        [DataMember]
        [XmlAttribute]
        public string ColumnName { get; set; }
    }

    
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum FilterPolicyType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        ServiceFilter = 1,
        [EnumMember]
        DomainFilter = 2,
        [EnumMember]
        DateTimeFilter = 3,
        [EnumMember]
        TextFilter = 4,
        [EnumMember]
        NumberFilter = 5,
        [EnumMember]
        UserFilter = 6,
        [EnumMember]
        BoolFilter = 7
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum FileSystemLevel : int
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Folder = 1,
        [EnumMember]
        File =2,
    }
 
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ServiceFilterType : int
    {
        [EnumMember]
        ServerName = 0,
        [EnumMember]
        IPAddress = 1
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum DomainFilterType : int
    {
        [EnumMember]
        Domain = 0,
        [EnumMember]
        IPV4Range = 1,
        [EnumMember]
        HostName = 2
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SPFilterLevel : int
    {
        [EnumMember]
        Undefined = 0,
        [EnumMember]
        Farm = -1,
        [EnumMember]
        WebApplication = 2,
        [EnumMember]
        SiteCollection = 100,
        [EnumMember]
        Site = 200,
        [EnumMember]
        List = 300,
        [EnumMember]
        Folder = 400,
        [EnumMember]
        Items = 501,
        [EnumMember]
        Documents = 502,
        [EnumMember]
        Attachment = 601,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SPFilterRule : int
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        URL = 1,
        [EnumMember]
        SiteCollectionTitle = 2,
        [EnumMember]
        ModifiedTime = 3,
        [EnumMember]
        CreatedTime = 4,
        [EnumMember]
        Owner = 5,
        [EnumMember]
        TemplateName = 6,
        [EnumMember]
        CreateBy = 7,
        [EnumMember]
        ModifiedBy = 8,
        [EnumMember]
        ContentType = 9,
        [EnumMember]
        ColumnText = 10,
        [EnumMember]
        Versions = 11,
        [EnumMember]
        DocumentNameAndExtension = 12,
        [EnumMember]
        DocumentSize = 13,
        [EnumMember]
        AttachmentNameAndExtension = 14,
        [EnumMember]
        Size = 15,
        [EnumMember]
        SiteTitle = 16,
        [EnumMember]
        ListName = 17,
        [EnumMember]
        FolderName = 18,
        [EnumMember]
        ItemName = 19,
        [EnumMember]
        ColumnNumber = 20,
        [EnumMember]
        ColumnBool = 21,
        [EnumMember]
        ColumnDate = 22,
        [EnumMember]
        Columns = 23,
        [EnumMember]
        ContentTypeCollection = 24
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum FilterCondition :int
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Contains = 1,
        [EnumMember]
        IsExactly = 2,
        [EnumMember]
        FromTo = 3,
        [EnumMember]
        Before = 4,
        [EnumMember]
        After = 5,
        [EnumMember]
        On = 6,
        [EnumMember]
        Within = 7,
        [EnumMember]
        LastVersions = 8,
        [EnumMember]
        LastMajarVersions = 9,
        [EnumMember]
        MajorVersions = 10,
        [EnumMember]
        ApprovedVersions = 11,
        [EnumMember]
        LargeThen = 12,
        [EnumMember]
        LessThen = 13,
        [EnumMember]
        Equals = 14,
        [EnumMember]
        BoolEquals = 15
    }

    /// <summary>
    /// DataTime类型的Filter item,当FilterCondition为Within时的单位
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum DateTimeUnit : int
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Day = 1,
        [EnumMember]
        Weeks = 2,
        [EnumMember]
        Months = 3,
        [EnumMember]
        Years
    }

    /// <summary>
    /// Filter item的include类型
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum FilterIncludeState : int
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Inclusion = 1,
        [EnumMember]
        Exclusion = 2
    }

    /// <summary>
    /// Filter Policy表达式类型
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum FilterExpressionType : int
    {
        [EnumMember]
        BasicFilter = 0,
        [EnumMember]
        AdvancedFilter = 1
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SizeUnit : int
    {
        [EnumMember]
        KB = 0,
        [EnumMember]
        MB = 1,
        [EnumMember]
        GB = 2
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum BoolFilterType : int
    {
        [EnumMember]
        Yes = 0,
        [EnumMember]
        No = 1,
    }
}