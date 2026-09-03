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




namespace AvePoint.GCommon.Contract.Compliance.eDiscovery.Object.HoldManager
{
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

    [KnownType(typeof(ApplyReleaseHoldMessage))]
    [KnownType(typeof(SyncHoldFilesMessage))]
    [KnownType(typeof(SyncHoldNameMessage))]
    [KnownType(typeof(InstallUninstallHoldHandlerMessage))]
    [KnownType(typeof(EditDeleteHoldItemMessage))]
    [KnownType(typeof(DBSettingMessage))]
    [KnownType(typeof(SyncHoldMessage))]
    [KnownType(typeof(OffLineHoldMessage))]
    [DataContract]
    public class HoldBaseMessage
    {
        [DataMember]
        public string PlanId { get; set; }
        [DataMember]
        public int PlanCategory { get; set; }
        [DataMember]
        public string JobId { get; set; }
        [DataMember]
        public string SubJobId { get; set; }
        [DataMember]
        public int JobType { get; set; }
        [DataMember]
        public OperationType Operation { get; set; }
        [DataMember]
        public ServiceDto AgentInfo { get; set; }
        [DataMember]
        public CplDBSettingsDto DBSetting { get; set; }
        /// <summary>
        /// 这个是Control那边存的FarmId
        /// </summary>
        [DataMember]
        public string FarmId { get; set; }
    }


    [DataContract]
    public enum OperationType
    {
        [EnumMember]
        ApplyHold,
        [EnumMember]
        ReleaseHold,
        [EnumMember]
        SyncHoldFiles,
        [EnumMember]
        SyncHoldName,
        [EnumMember]
        EditHoldItem,
        [EnumMember]
        DeleteHoldItem,
        [EnumMember]
        InstallHoldHandler,
        [EnumMember]
        UninstallHoldHandler,
        [EnumMember]
        SetDatabase,
        [EnumMember]
        RealTime,
        [EnumMember]
        DataExport,
        [EnumMember]
        OffLineHold
    }

}
