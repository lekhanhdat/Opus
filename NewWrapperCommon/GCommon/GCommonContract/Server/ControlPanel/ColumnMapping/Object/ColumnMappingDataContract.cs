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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using System.ComponentModel;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.ColumnMapping.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ColumnMappingDataContract : IProfileContent
    {

        [DataMember]
        public string mappingId { set; get; }

        [DataMember]
        public String mappingName { set; get; }

        [DataMember]
        public String description { set; get; }

        [DataMember]
        public long modifiedTime { get; set; }

        [DataMember]
        public List<ConditionAndColumnMapping> MappingList { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ConditionAndColumnMapping
    {
        [DataMember]
        public List<ColumnFilter> SiteFilterList { get; set; }
        [DataMember]
        public List<ColumnFilter> ListFilterList { get; set; }
        [DataMember]
        public List<ColumnFilter> ItemFilterList { get; set; }

        [DataMember]
        public List<ColumnMappingValue> ColumnMappingList { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ColumnFilter
    {
        [DataMember]
        public List<ConditionItem> Conditions { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ConditionItem
    {
        [DataMember]
        public MappingFilterCondition ConditionType { get; set; }
        [DataMember]
        public MappingFilterRule MetaDataType { get; set; }
        [DataMember]
        public string Value { get; set; }
        [DataMember]
        public AndOrType AndOr { get; set; }
        [DataMember]
        public int order { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AndOrType
    {
        [EnumMember]
        None,
        [EnumMember]
        And,
        [EnumMember]
        Or
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum MappingFilterRule : int
    {
        [EnumMember]
        URL = 0,
        [EnumMember]
        SiteContentType = 1,
        [EnumMember]
        TemplateID = 2,
        [EnumMember]
        ListTitle = 3,
        [EnumMember]
        ListContentType = 4,
        [EnumMember]
        Name = 5,
        [EnumMember]
        None = 6
    }
    public enum MappingFilterCondition : int
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Contains = 1,
        [EnumMember]
        Equal = 2,
        [EnumMember]
        NotEqual = 3,
        [EnumMember]
        DoesNotContain = 4,

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ColumnMappingValue : INotifyPropertyChanged
    {
        [DataMember]
        public ColumnType Type { get; set; }


        private string sourceColumnName;
        [DataMember]
        public string SourceColumnName
        {
            get { return sourceColumnName; }
            set
            {
                sourceColumnName = value;
                NotifyPropertyChanged("SourceColumnName");
            }
        }
        private string sourceInternalName;
        [DataMember]
        public string SourceInternalName
        {
            get { return sourceInternalName; }
            set
            {
                sourceInternalName = value;
                NotifyPropertyChanged("SourceInternalName");
            }
        }
        private string desColumnName;
        [DataMember]
        public string DesColumnName
        {
            get { return desColumnName; }
            set
            {
                desColumnName = value;
                NotifyPropertyChanged("DesColumnName");
            }
        }
        private string desInternalName;
        [DataMember]
        public string DesInternalName
        {
            get { return desInternalName; }
            set
            {
                desInternalName = value;
                NotifyPropertyChanged("DesInternalName");
            }
        }
       
        [DataMember]
        public MetadataSetting metadataSetting { get; set; }

        [DataMember]
        public LookUpSetting LookUpSetting { get; set; }

        [DataMember]
        public DestinationSetting DestinationSetting { get; set; }

        [DataMember]
        public List<ValueMapping> ValueList { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }  
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class MetadataSetting
    {
        [DataMember]
        public string TermSetPath { get; set; }
        [DataMember]
        public Boolean IsAllowMultiterm { get; set; }
        [DataMember]
        public Boolean IsMigrateString { get; set; }
        [DataMember]
        public string MigrateBy { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class LookUpSetting
    {
        [DataMember]
        public string ListTitle { get; set; }
        [DataMember]
        public string ColumnName { get; set; }
        [DataMember]
        public Boolean IsAllowMultiterm { get; set; }
        [DataMember]
        public Boolean IsMigrateString { get; set; }
        [DataMember]
        public string MigrateBy { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DestinationSetting
    {
        [DataMember]
        public Boolean IsMigrateString { get; set; }
        [DataMember]
        public string MigrateBy { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ValueMapping
    {
        private string sourceValue;
        [DataMember]
        public string SourceValue
        {
            get { return sourceValue; }
            set
            {
                sourceValue = value;
                NotifyPropertyChanged("SourceValue");
            }
        }

        private string desValue;
        [DataMember]
        public string DesValue
        {
            get { return desValue; }
            set
            {
                desValue = value;
                NotifyPropertyChanged("DesValue");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }  
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ColumnType
    {
        [EnumMember]
        SameType,
        [EnumMember]
        ChangeToDes,
        [EnumMember]
        ChangeToMetadata,
        [EnumMember]
        ChangeToLookUp,
        [EnumMember]
        ChangeToText
    }


}
