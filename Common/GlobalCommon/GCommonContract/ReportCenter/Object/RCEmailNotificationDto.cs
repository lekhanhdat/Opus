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




namespace AvePoint.GCommon.Contract.ReportCenter.Object
{
    #region using directives
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCEmailNotificationDto : BaseConfigSetting
    {
        /// <summary>
        /// Email Notification Id 
        /// </summary>
        [DataMember]
        public string Id { set; get; }

        /// <summary>
        /// Notification Name ,Unique
        /// </summary>
        [DataMember]
        public string Name { set; get; }

        /// <summary>
        /// Notification Description
        /// </summary>
        [DataMember]
        public string Description { set; get; }

        /// <summary>
        /// Email level, high normal low
        /// </summary>
        [DataMember]
        public int SignLevel { set; get; }

        /// <summary>
        /// HTML,Plain Text
        /// </summary>
        [DataMember]
        public RCMessageFormat MessageFormat { set; get; }

        [DataMember]
        public List<NotificationDetail> Detail { set; get; }

        [DataMember]
        public List<string> IdList { set; get; }

        private string detailDisplay;

        [DataMember]
        public string DetailDisplay
        {
            get 
            {
                StringBuilder stringBuilder = new StringBuilder();
                if (Detail == null)
                {
                    return "";
                }
                for (int i = 0; i < Detail.Count; i++)
                {
                    stringBuilder.Append(Detail[i].Email).Append(";");
                }
                detailDisplay = stringBuilder.ToString().Substring(0, stringBuilder.Length - 1);
                return detailDisplay;
            
            }
            set { detailDisplay = value; }
        }

        public override string ToString()
        {
            return string.Format("RCEmailNotificationDto[Name {0}]", Name);
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RCMessageFormat
    {
        [EnumMember]
        HTML = 0,
        [EnumMember]
        PlainText = 1
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class NotificationDetail
    {
        [DataMember]
        public RCEmailNotificationReport Report { set; get; }
        [DataMember]
        public RCEmailNotificationType Type { set; get; }
        [DataMember]
        public string Email { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RCEmailNotificationReport
    {
        [EnumMember]
        Recipient=0
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RCEmailNotificationType
    { 
        [EnumMember]
        EmailAddress=0
    }
}