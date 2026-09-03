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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace AvePoint.Wrapper.Common
{
    [DataContract]
    public class AveUserInfo
    {
        [DataMember]
        public string Login; //[nvarchar](255) NOT NULL,

        //public Guid SiteID; //[uniqueidentifier] NOT NULL,
        [DataMember]
        public int ID; //[int] NOT NULL,
        [DataMember]
        public bool DomainGroup; //[bit] NOT NULL,
        [DataMember]
        public string Domain;
        [DataMember]
        public byte[] SystemID; //[dbo].[tSystemID] NOT NULL,
        [DataMember]
        public int Deleted; //[int] NOT NULL,
        [DataMember]
        public bool SiteAdmin; //[bit] NOT NULL,
        [DataMember]
        public bool IsActive; //[bit] NOT NULL,
        [DataMember]
        public string Title; //[nvarchar](255) NOT NULL,
        [DataMember]
        public string Email; //[nvarchar](255) NOT NULL,
        [DataMember]
        public string Notes; //[nvarchar](1023) NOT NULL,
        [DataMember]
        public byte[] Token; //[image] NULL,
        [DataMember]
        public byte[] ExternalToken; //[varbinary](max) NULL,
        [DataMember]
        public Nullable<DateTime> ExternalTokenLastUpdated = new Nullable<DateTime>(); //[datetime] NULL,
        [DataMember]
        public Nullable<int> Locale = new Nullable<int>(); //[int] NULL,
        [DataMember]
        public Nullable<short> CalendarType = new Nullable<short>(); //[smallint] NULL,
        [DataMember]
        public Nullable<short> AdjustHijriDays = new Nullable<short>(); //[smallint] NULL,
        [DataMember]
        public Nullable<short> TimeZone = new Nullable<short>(); //[smallint] NULL,
        [DataMember]
        public Nullable<bool> Time24 = new Nullable<bool>(); //[bit] NULL,
        [DataMember]
        public Nullable<byte> AltCalendarType = new Nullable<byte>(); //[tinyint] NULL,
        [DataMember]
        public Nullable<byte> CalendarViewOptions = new Nullable<byte>(); //[tinyint] NULL,
        [DataMember]
        public Nullable<short> WorkDays = new Nullable<short>(); //[smallint] NULL,
        [DataMember]
        public Nullable<short> WorkDayStartHour = new Nullable<short>(); //[smallint] NULL,
        [DataMember]
        public Nullable<short> WorkDayEndHour = new Nullable<short>(); //[smallint] NULL,
        [DataMember]
        public string Mobile; //[nvarchar](127) NULL,
        [DataMember]
        public Nullable<int> Flags = new Nullable<int>(); //[int] NOT NULL,
        [DataMember]
        public ArrayList Roles = new ArrayList();
        //记录是否有权限，便于还原时候判断是否还原,null和true都是有权限的
        [DataMember]
        public Nullable<bool> HasPermission = new Nullable<bool>(); //[bit] NULL,
    }


    // Add Default Value for Nullable Type
    public class AveUserInfoTableColumnValue
    {
        public const uint Loacal = 0;
        public const ushort TimeZone = 0;
    }

    public class AveUserList
    {
        public List<AveUserInfo> Users = new List<AveUserInfo>();
    }
}
