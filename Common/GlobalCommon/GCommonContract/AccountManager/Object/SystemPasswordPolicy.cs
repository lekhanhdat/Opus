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
using System.Runtime.Serialization;
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Common;


namespace AvePoint.GCommon.Contract.AccountManager.Object
{
    /// <summary>
    /// 用于Account Manager中设置用户的密码规则，判断密码强度是否足够等
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot]
    public class SystemPasswordPolicy
    {
        

        /// <summary>
        /// 用户是否必须account is inactive
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public bool IsAccountInactive { get; set; }

        /// <summary>
        /// 用户是否必须每次登陆都要修改密码
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public bool ChangePasswordAtNextLogon { get; set; }

        /// <summary>
        /// 用户是否能够修改密码
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public bool CanChangePassword { get; set; }

        /// <summary>
        /// 密码是否永不过期
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public bool PasswordNeverExpired { get; set; }

        /// <summary>
        /// 密码的最短长度
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public int MinLength { get; set; }

        /// <summary>
        /// 密码的最长长度
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public int MaxLength { get; set; }


        /// <summary>
        /// 密码中最少含有多少个数字
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public int MinNumberRequired { get; set; }

        /// <summary>
        /// 密码中最少含有多少个Alpha字符
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public int MinAlphaRequired { get; set; }

        /// <summary>
        /// 密码中最少含有多少个特殊字符
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public int MinSpecialCharsRequired { get; set; }

        /// <summary>
        /// 是否不允许密码中包含用户名
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public bool NotAllowedContainUserID { get; set; }

        /// <summary>
        /// 是否不允许密码中包含空格
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public bool NotAllowedContainSpace { get; set; }

        /// <summary>
        /// 设置距离密码过期还剩多长时间对用户提醒
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public int SendMsgBeforeTime { get; set; }

        /// <summary>
        /// 过期时间的单位
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public PeriodType TimeOutUnit { get; set; }

        /// <summary>
        /// 是否有弹出式提醒
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public bool PopupMsg { get; set; }

        /// <summary>
        /// 弹出式提醒的消息内容
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public string Message { get; set; }

        /// <summary>
        /// 是否邮件提醒
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public bool EmailNotification { get; set; }

        [DataMember]
        public SystemExpirationPolicy ExpirationPolicy { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is SystemPasswordPolicy)) return false;
            SystemPasswordPolicy policy = obj as SystemPasswordPolicy;
            return this.MinLength == policy.MinLength &&
                this.MinNumberRequired == policy.MinNumberRequired &&
                this.MinSpecialCharsRequired == policy.MinSpecialCharsRequired &&
                this.NotAllowedContainSpace == policy.NotAllowedContainSpace &&
                this.NotAllowedContainUserID == policy.NotAllowedContainUserID &&
                this.MinAlphaRequired == policy.MinAlphaRequired &&
                this.EmailNotification == policy.EmailNotification &&
                this.PopupMsg == policy.PopupMsg &&
                this.CanChangePassword == policy.CanChangePassword &&
                this.ChangePasswordAtNextLogon == policy.ChangePasswordAtNextLogon &&
                this.IsAccountInactive == policy.IsAccountInactive &&
                this.PasswordNeverExpired == policy.PasswordNeverExpired &&
                this.Message == policy.Message;
        }

        public override int GetHashCode()
        {
            return $"{MinLength}{MinNumberRequired}{MinSpecialCharsRequired}{MinAlphaRequired}".GetHashCode();
        }

        public SystemPasswordPolicy()
        {
            this.CanChangePassword = true;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SystemExpirationPolicy
    {
        [DataMember]
        public ExpirationType Type { get; set; }

        [DataMember]
        public int AfterDays { get; set; }

        [DataMember]
        public DateTime AtSpecificDay { get; set; }

        [DataMember]
        public string TimeZoneId { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ExpirationType
    {
        [EnumMember]
        Never = 0,

        [EnumMember]
        AfterDays = 1,

        [EnumMember]
        AtSpecificDay = 2
    }
}
