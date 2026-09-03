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
using AvePoint.GCommon.Contract.Server.Common.ExportLocation.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.SystemSetting.Object;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.UpdateManager.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class UpdateSettingDto : ISystemSettingContent
    {
        [DataMember]
        public ExportLocationDto LocationDto { set; get; }

        [DataMember]
        public AutomaticUpdateSetting AutomaticUpdateSetting { set; get; }

        [DataMember]
        public NotificationDto Notification { get; set; }

        [DataMember]
        public Proxy ProxyDto { set; get; }

        [DataMember]
        public bool MoveUninstallToNewLocation { set; get; }

        [DataMember]
        public bool DelInstalledPatch { set; get; }

        [DataMember]
        public bool MaintenanceExpired { set; get; }

        [DataMember]
        public int DownloadPort { set; get; }

        [DataMember]
        public bool IsChangeUNCPassword { set; get; }

        [DataMember]
        public bool IsUsedNetShareLoction { set; get; }

        [DataMember]
        public bool IsChanageProxyPassword { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AutomaticUpdateSetting
    {
        [EnumMember]
        Download,

        [EnumMember]
        Notify,

        [EnumMember]
        TurnOff
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Proxy
    {
        [DataMember]
        public ProxyType ProxyType { set; get; }

        [DataMember]
        public string ProxyHost { set; get; }

        [DataMember]
        public string ProxyPort { set; get; }

        [DataMember]
        public string UserName { set; get; }

        [DataMember]
        public string Password { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ProxyType
    {
        [EnumMember]
        NoProxy,
        [EnumMember]
        Http,
        [EnumMember]
        Socket
    }
}
