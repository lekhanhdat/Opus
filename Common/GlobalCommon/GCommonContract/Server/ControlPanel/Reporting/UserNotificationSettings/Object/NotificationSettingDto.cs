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

using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.SystemSetting.Object;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.Reporting.UserNotificationSettings.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class NotificationSettingDto : ISystemSettingContent
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string OutgoingMailServer { get; set; }

        [DataMember]
        public int Port { get; set; }

        [DataMember]
        public string Sender { get; set; }

        [DataMember]
        public string SenderDisplayName { get; set; }

        [DataMember]
        public string ExchangeServer { get; set; }

        [DataMember]
        public bool Useable { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public bool SslAuthentication { get; set; }

        [DataMember]
        public bool SecurePasswordAuthentication { get; set; }

        public NotificationSettingDto Clone()
        {
            return new NotificationSettingDto()
            {
                Id = this.Id,
                Name = this.Name,
                Useable = this.Useable,
                OutgoingMailServer = this.OutgoingMailServer,
                Sender = this.Sender,
                SenderDisplayName = this.SenderDisplayName,
                UserName = this.UserName,
                Port = this.Port,
                ExchangeServer = this.ExchangeServer,
                Password = this.Password,
                SslAuthentication = this.SslAuthentication,
                SecurePasswordAuthentication = this.SecurePasswordAuthentication
            };
        }
    }

    public enum NotificationSettingResult
    {
        [EnumMember]
        SaveSuccessful,

        [EnumMember]
        UpdateSuccessful,

        [EnumMember]
        SaveFailed,

        [EnumMember]
        EmptyParameters,

        [EnumMember]
        TestSuccessful,

        [EnumMember]
        TestFailed,

        [EnumMember]
        CanNotFindSender,

        [EnumMember]
        WrongSmtpOrPort,

        [EnumMember]
        SeverNotSupportSsl,
    }

    public struct NotificationSettingSavingParameter
    {
        public NotificationSettingDto Data { get; set; }

        public bool Testing { get; set; }
    }
}