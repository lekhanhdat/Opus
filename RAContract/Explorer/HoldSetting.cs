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
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Explorer
{
    public class HoldSetting
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public bool isChecked { get; set; }
        /// <summary>
        /// 前台显示用
        /// </summary>
        public string CreateTime { get; set; }
        public HoldDateType Type { get; set; }
        public int Number { get; set; }

        public HoldDateUnit Unit { get; set; }
        /// <summary>
        /// react 版本Calendar控件, 获取到的时间是Utc 的DateTime
        /// </summary>
        public DateTime CalendarDate { set; get; }
        /// <summary>
        /// 用于兼容旧版本knockout控件, 获取到的是显示时间, 也用于数据回显
        /// </summary>
        public string CalenderTime { get; set; }

        public bool IsDayLightSaving { get; set; }

        public string TimeZoneId { get; set; }

        public string Description { get; set; }

        public bool hasRelated { get; set; }

        public HoldProfileType ProfileType { set; get; }

        public HoldEmailNotification? EmailNotification { get; set; }
        public List<ToUserInfo> HoldUserManagers { get; set; }
        public bool IsHoldManagerEmailNotificationEnabled { get; set; }
    }

        //public class HoldSettingDto
        //{
        //    public List<HoldSettingIds> SettingId { get; set; }
        //    public HoldSetting Setting { get; set; }

        //}

        //public class HoldSettingIds
        //{
        //    public Guid scopeId { get; set; }
        //    public string dirPath { get; set; }
        //}
        public class HoldSettingDto
    {
        public string HoldId { get; set; }
        public int AllianceType { get; set; }
        public long ReleaseTime { get; set; }
        public string HoldBy { get; set; }
        public bool NeedCheckConflicted { set; get; }
        public bool IsOverride { set; get; }
        //"change/append"
        public string HoldAction { get; set; }
        public List<string> RemoveHolds { get; set; }
    }
    public enum HoldProfileType
    {
        All = -1,
        Normal = 0,
        Physical = 1
    }
    public enum HoldDateType
    {
        Custom = 0,
        Calendar = 1
    }
    public enum HoldDateUnit
    {
        Day=0,
        Week = 1,
        Month = 2,
        Years = 3
    }
    [DataContract]
    public class ChangeHoldDto
    {
        [DataMember]
        public List<Guid> recordsId { get; set; }
        [DataMember]
        public bool isFS { get; set; }
        [DataMember]
        public bool isPhysical { get; set; }
        [DataMember]
        public List<string> removeHoldIds { get; set; }
    }
    [DataContract]
    public class HoldUser
    {
        [DataMember]
        public string HoldId { get; set; }
        [DataMember]
        public string HoldBy { get; set; }
    }

    public class HoldUntilTime
    {
        public string HoldId { get; set; }
        public long UntilTime { get; set; }
    }

    public class RemoveHoldSetting
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string HoldUntilTime { get; set; }
    }
}
