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
using System.Runtime.Serialization;
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.SharePointBrowser.Object;

namespace AvePoint.GCommon.Contract.Server.Common.Profile.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    //[KnownType(typeof(CASearchFilter))]
    //[XmlInclude(typeof(CASearchFilter))]
    [XmlRoot(ElementName = "FilterPolicy")]
    public class FilterPolicy
    {
        [DataMember]
        [XmlAttribute]
        public string Name { get; set; }

        [DataMember]
        [XmlAttribute]
        public string Description { get; set; }

        [DataMember]
        [XmlAttribute]
        public string AdvancedExpression { get; set; }

        [DataMember]
        [XmlArray("Filters")]
        [XmlArrayItem(typeof(TextFilter), ElementName = "TextFilter"),
        XmlArrayItem(typeof(TimeFilter), ElementName = "TimeFilter"),
        XmlArrayItem(typeof(NumberFilter), ElementName = "NumberFilter"),
        XmlArrayItem(typeof(UserFilter), ElementName = "UserFilter")]
        public List<BaseFilter> Filters { get; set; }

        public FilterPolicy()
        {
            Filters = new List<BaseFilter>();
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [KnownType(typeof(TextFilter))]
    [KnownType(typeof(TimeFilter))]
    [KnownType(typeof(NumberFilter))]
    [KnownType(typeof(UserFilter))]
    public abstract class BaseFilter
    {
        [DataMember]
        [XmlAttribute]
        public int SerialNumber { get; set; }

        [DataMember]
        [XmlAttribute]
        public bool Include { get; set; }

        [DataMember]
        [XmlAttribute]
        public SPObjectLevel Level { get; set; }

        [DataMember]
        [XmlAttribute]
        public string Property { get; set; }

        [DataMember]
        [XmlAttribute]
        public SearchFilterFlag FilterFlag { get; set; }
            
        [XmlIgnore]
        public abstract FilterType Type { get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [Flags]
    public enum SearchFilterFlag
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        ResultLevel = 1,
        [EnumMember]
        Url = 2,
        [EnumMember]
        Title = 4,
        [EnumMember]
        Name = 8,
        [EnumMember]
        Template = 16,
        [EnumMember]
        CreatedBy = 32,

        [EnumMember]
        CreatedTime = 64,
        [EnumMember]
        ModifiedTime = 128,

        [EnumMember]
        Owner = 256,

        [EnumMember]
        Inheritance = 512,

        [EnumMember]
        Permission = 1024,

        [EnumMember]
        Attribute = 2048,

        [EnumMember]
        FullTextIndex = 4096

    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum FilterType
    {
        [EnumMember]
        TimeFilter,
        [EnumMember]
        TextFilter,
        [EnumMember]
        NumberFilter,
        [EnumMember]
        BoolFilter,
        [EnumMember]
        UserFilter,
        [EnumMember]
        PermissionFilter
    }

    #region User Filter
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class UserFilter : BaseFilter
    {
        [DataMember]
        [XmlAttribute]
        public UserFilterCondition Condition { get; set; }

        [DataMember]
        [XmlAttribute]
        public string Criteria { get; set; }

        public override FilterType Type
        {
            get { return FilterType.UserFilter; }
        }

        public override bool Equals(object obj)
        {
            if (this.GetType() != obj.GetType())
            {
                return false;
            }
            else
            {
                UserFilter _UserFilter = obj as UserFilter;
                if (this.Include == _UserFilter.Include
                          && this.Condition.Equals(_UserFilter.Condition)
                          && this.Level.Equals(_UserFilter.Level)
                          && this.Property.Equals(_UserFilter.Property)
                          && this.Criteria.Equals(_UserFilter.Criteria))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public override int GetHashCode()
        {
            return this.Condition.GetHashCode() + this.Criteria.GetHashCode() + this.Type.GetHashCode();
        }
    }

    public enum UserFilterCondition
    {
        [EnumMember]
        Is,
        [EnumMember]
        Contains
    }
    #endregion

    #region Text Filter
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class TextFilter : BaseFilter
    {
        [DataMember]
        [XmlAttribute]
        public TextFilterCondition Condition { get; set; }

        [DataMember]
        [XmlAttribute]
        public string Criteria { get; set; }

        public override FilterType Type
        {
            get
            {
                return FilterType.TextFilter;
            }
        }

        public override bool Equals(object obj)
        {
            if (this.GetType() != obj.GetType())
            {
                return false;
            }
            else
            {
                TextFilter _TextFilter = obj as TextFilter;
                if (this.Include == _TextFilter.Include
                          && this.Condition.Equals(_TextFilter.Condition)
                          && this.Level.Equals(_TextFilter.Level)
                          && this.Property.Equals(_TextFilter.Property)
                          && this.Criteria.Equals(_TextFilter.Criteria))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public override int GetHashCode()
        {
            return this.Condition.GetHashCode() + this.Criteria.GetHashCode() + this.Type.GetHashCode();
        }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum TextFilterCondition
    {
        [EnumMember]
        Is,
        [EnumMember]
        Contains
    }
    #endregion

    #region Time Filter
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class TimeFilter : BaseFilter
    {
        [DataMember]
        [XmlAttribute]
        public TimeFilterCondition Condition { get; set; }

        [DataMember]
        [XmlAttribute]
        public DateTime BeginTime { get; set; }

        [DataMember]
        [XmlAttribute]
        public DateTime EndTime { get; set; }

        [DataMember]
        [XmlAttribute]
        public double TimeSpan { get; set; }

        [DataMember]
        [XmlAttribute]
        public TimeUnit Unit { get; set; }

        public override FilterType Type
        {
            get { return FilterType.TimeFilter; }
        }

        public override bool Equals(object obj)
        {
            if (this.GetType() != obj.GetType())
            {
                return false;
            }
            else
            {
                TimeFilter _TimeFilter = obj as TimeFilter;
                if (this.Condition == _TimeFilter.Condition && this.EndTime == _TimeFilter.EndTime && this.BeginTime == _TimeFilter.BeginTime
                    && this.TimeSpan == _TimeFilter.TimeSpan && this.Unit == _TimeFilter.Unit && this.Include ==_TimeFilter.Include 
                    && this.Level.Equals(_TimeFilter.Level) && this.Property==_TimeFilter.Property)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public override int GetHashCode()
        {
            return this.Unit.GetHashCode() + BeginTime.GetHashCode() + EndTime.GetHashCode();
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum TimeUnit
    {
        [EnumMember]
        None,
        [EnumMember]
        Year,
        [EnumMember]
        Month,
        [EnumMember]
        Week,
        [EnumMember]
        Day,
        [EnumMember]
        Hour,
        [EnumMember]
        Minute,
        [EnumMember]
        Second,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum TimeFilterCondition
    {
        [EnumMember]
        FromTo,
        [EnumMember]
        OlderThan,
        [EnumMember]
        LaterThan,
        [EnumMember]
        Is,
        [EnumMember]
        WithIn,
    }
    #endregion

    #region Number Filter
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class NumberFilter : BaseFilter
    {
        [DataMember]
        [XmlAttribute]
        public NumberFilterCondition Condition { get; set; }

        [DataMember]
        [XmlAttribute]
        public double Criteria { get; set; }

        [DataMember]
        [XmlAttribute]
        public NumberUnit Unit { get; set; }

        public override FilterType Type
        {
            get { return FilterType.NumberFilter; }
        }

        public override bool Equals(object obj)
        {
            if (this.GetType() != obj.GetType())
            {
                return false;
            }
            else
            {
                NumberFilter _NumberFilter = obj as NumberFilter;
                if (this.Condition == _NumberFilter.Condition && this.Criteria == _NumberFilter.Criteria && this.Unit == _NumberFilter.Unit
                    && this.Type == _NumberFilter.Type)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public override int GetHashCode()
        {
            return this.Unit.GetHashCode() + this.Criteria.GetHashCode() + Condition.GetHashCode();
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum NumberUnit
    {
        [EnumMember]
        None,
        [EnumMember]
        Year,
        [EnumMember]
        Month,
        [EnumMember]
        Day,
        [EnumMember]
        KB,
        [EnumMember]
        MB,
        [EnumMember]
        GB,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum NumberFilterCondition
    {
        [EnumMember]
        Is,
        [EnumMember]
        GreaterThan,
        [EnumMember]
        LessThan,
    }
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SPObjectLevel
    {
        [EnumMember]
        NoValue,
        [EnumMember]
        Farm,
        [EnumMember]
        WebApplication,
        [EnumMember]
        SiteCollection,
        [EnumMember]
        Site,
        [EnumMember]
        App,
        [EnumMember]
        ListOrLibrary,
        [EnumMember]
        Folder,
        [EnumMember]
        Item,
        [EnumMember]
        ItemVersion,
        [EnumMember]
        Document,
        [EnumMember]
        DocumentVersion,
        [EnumMember]
        Attachment
    }
}
