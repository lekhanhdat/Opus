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



namespace AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineBackup.Object
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.Common;
    using AvePoint.GCommon.Contract.Server.Common.Profile.Object;

    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot(ElementName = "ExchangeOnlineFilterInfo")]
    public class ExchangeOnlineBackupFilterDto : IProfileContent
    {
        [DataMember]
        [XmlAttribute("Id")]
        public string Id { get; set; }

        [DataMember]
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [DataMember]
        [XmlAttribute("Description")]
        public string Description { get; set; }

        [DataMember]
        [XmlAttribute("LastModifyTime")]
        public DateTime LastModifyTime { get; set; }

        [DataMember]
        [XmlArray("FilterItems")]
        public List<BaseFilterItem> FItems { get; set; }

        [DataMember]
        [XmlAttribute]
        public string FilterExpression { get; set; }

        [DataMember]
        [XmlAttribute]
        public EOExpressionType ExpressionType { get; set; }

        [DataMember]
        public Dictionary<EOCategoryType, string> AndOrExpression { get; set; }

        [DataMember]
        [XmlAttribute]
        public PlanCategory Category { get; set; }

    }

    [KnownType(typeof(EODateTimeZoneValue))]
    [KnownType(typeof(EODateTimeRangeValue))]
    [KnownType(typeof(EOStringValue))]
    [KnownType(typeof(EOBoolValue))]
    [KnownType(typeof(EOEnumValue))]
    [KnownType(typeof(EOSizeValue))]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BaseFilterItem
    {
        [DataMember]
        [XmlAttribute]
        public string FilterItemId { get; set; }

        //index 标识
        [DataMember]
        [XmlAttribute]
        public string Order { get; set; }

        [DataMember]
        [XmlAttribute]
        public string OrderRange { get; set; }

        //category 标识
        [DataMember]
        [XmlAttribute]
        public EOCategoryType FilterCategoryType { get; set; }

        //rule 标识
        [DataMember]
        [XmlAttribute]
        public EORuleType FilterRuleType { get; set; }

        //rule中自定义名字
        [DataMember]
        [XmlAttribute]
        public string CustomName { get; set; }

        // 判断rule类型,string,datetime..
        [DataMember]
        [XmlAttribute]
        public EOCustomRuleType FilterCustomRuleType { get; set; }

        //condition 标识
        [DataMember]
        [XmlAttribute]
        public EOConditionType FilterConditionType { get; set; }

        /// <summary>
        /// 运算符属性
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public string Operator { get; set; }

        [DataMember]
        [XmlAttribute]
        public EOAndOrType AndOr { get; set; }

        /// <summary>
        /// 传递 value
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public object FilterValue { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum EOCategoryType : int
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        Message = 1,

        [EnumMember]
        Task = 2,

        [EnumMember]
        Post = 3,

        [EnumMember]
        Event = 4,

        [EnumMember]
        Journal = 5,

        [EnumMember]
        Note = 6,

        [EnumMember]
        Contact = 7,

        [EnumMember]
        Document = 8,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum EORuleType : int
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        Subject = 1,
        [EnumMember]
        ReceivedTime = 2,
        [EnumMember]
        From = 3,
        [EnumMember]
        To = 4,
        [EnumMember]
        Size = 5,

        [EnumMember]
        StartDate = 6,
        [EnumMember]
        DueDate = 7,
        [EnumMember]
        Priority = 8,
        [EnumMember]
        CreatedBy = 9,

        [EnumMember]
        Conversation = 10,
        [EnumMember]
        PostedOn = 11,
        [EnumMember]
        PostedTo = 12,

        [EnumMember]
        StartTime = 13,
        [EnumMember]
        EndTime = 14,

        [EnumMember]
        EntryType = 15,

        [EnumMember]
        Name = 16,
        [EnumMember]
        CreateTime = 17,
        [EnumMember]
        ModifyTime = 18,
        [EnumMember]
        ModifiedBy = 19,

        [EnumMember]
        FullName = 20,
        [EnumMember]
        LastName = 21,
        [EnumMember]
        FirstName = 22,

        [EnumMember]
        Status = 23,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum EOConditionType : int
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        StringContains = 1,
        [EnumMember]
        StringNotContains = 2,
        [EnumMember]
        StringEquals = 3,
        [EnumMember]
        StringNotEquals = 4,
        [EnumMember]
        Before = 5,
        [EnumMember]
        After = 6,
        [EnumMember]
        OldThan = 7,
        [EnumMember]
        Within = 8,
        [EnumMember]
        LargerThan = 9,
        [EnumMember]
        LessThan = 10,
        [EnumMember]
        EnumEquals = 11,
        [EnumMember]
        EnumNotEquals = 12,
        [EnumMember]
        EnumIs = 13,
        [EnumMember]
        EnumIsNot = 14,
        [EnumMember]
        StringIs = 15,
        [EnumMember]
        StringIsNot = 16,
    }

    /// <summary>
    /// Type以UserDefined，多出一个输入框
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum EOCustomRuleType : int
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Number = 1,
        [EnumMember]
        String = 2,
        [EnumMember]
        DateTime = 3,
        [EnumMember]
        Enum = 4,
        [EnumMember]
        UserDefinedSize = 5,
        [EnumMember]
        UserDefinedNumber = 6,
        [EnumMember]
        UserDefinedString = 7,
        [EnumMember]
        UserDefinedDateTime = 8,
        [EnumMember]
        UserDefinedEnum = 9,
        [EnumMember]
        Version = 10,
        [EnumMember]
        Size = 11,
        [EnumMember]
        CustomerEnum = 12,
        [EnumMember]
        UserDefinedCustomerEnum = 13,
    }


    #region == Value ==

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EODateTimeZoneValue
    {
        [DataMember]
        [XmlAttribute]
        public DateTime Value { get; set; }

        [DataMember]
        public string TimeZoneId { get; set; }

        [DataMember]
        public bool IsDayLightSaving { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EODateTimeRangeValue
    {
        [DataMember]
        [XmlAttribute]
        public string Value { get; set; }

        [DataMember]
        [XmlAttribute]
        public EODateTimeType TimeUnit { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EOStringValue
    {
        [DataMember]
        [XmlAttribute]
        public string Value { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EOSizeValue
    {
        [DataMember]
        [XmlAttribute]
        public string Value { get; set; }

        [DataMember]
        [XmlAttribute]
        public EOSizeType SizeUnit { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EOBoolValue
    {
        [DataMember]
        [XmlAttribute]
        public EOBoolType BoolFilterType { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EOEnumValue
    {
        //传递Enum string值
        [DataMember]
        [XmlAttribute]
        public string Value { get; set; }

        //用于gui 判断
        [DataMember]
        [XmlAttribute]
        public object EnumType { get; set; }
    }

    #endregion == Value ==

    #region == Enum ==
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum EOExpressionType : int
    {
        [EnumMember]
        Basic = 0,
        [EnumMember]
        Advanced = 1,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum EODateTimeType : int
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
        Years = 4,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum EOSizeType : int
    {
        [EnumMember]
        KB = 0,
        [EnumMember]
        MB = 1,
        [EnumMember]
        GB = 2
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum EOBoolType : int
    {
        [EnumMember]
        Yes = 0,
        [EnumMember]
        No = 1,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum EOAndOrType : int
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        And = 1,
        [EnumMember]
        Or = 2,
    }

    public enum EnumYesNoType : int
    {
        [EnumMember]
        Yes = 0,
        [EnumMember]
        No = 1,
    }
    #endregion == Unit ==

}
