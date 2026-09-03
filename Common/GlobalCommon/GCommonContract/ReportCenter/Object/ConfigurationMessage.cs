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

    #endregion

    [DataContract]
    public class ConfigurationMessage
    {
        [DataMember]
        public List<BaseConfigSetting> ConfigSettings { get; set; }

        [DataMember]
        public RCAction Action { get; set; }
    }

    [KnownType(typeof(ScopeProfile))]
    [KnownType(typeof(RCEmailNotificationDto))]
    [KnownType(typeof(ExportLocationSetting))]
    [KnownType(typeof(AuditDatabaseSetting))]
    [DataContract]
    public class BaseConfigSetting
    {

    }

    [DataContract]
    public enum RCAction
    {
        [EnumMember]
        Undefined = 0,
        [EnumMember]
        Add = 1,
        [EnumMember]
        Update = 2,
        [EnumMember]
        Delete = 3,
        [EnumMember]
        Get = 4,
        [EnumMember]
        GetAll = 5,
        [EnumMember]
        GetAllWithContent = 6,
        [EnumMember]
        DeleteSome = 7,
        [EnumMember]
        Test = 8,
        [EnumMember]
        Validate = 9,
        [EnumMember]
        DeleteAll = 10,
        [EnumMember]
        GetWithoutContent = 11,
        [EnumMember]
        ApplyRule = 12,
        [EnumMember]
        GetWithJob = 13,
        [EnumMember]
        UpdateAndShare = 14,
    }
}