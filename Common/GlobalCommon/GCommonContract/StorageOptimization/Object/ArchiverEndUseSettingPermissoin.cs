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
using System.Collections.Generic;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.StorageOptimization.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ArchiveAndRestore
    {
        [DataMember]
        public List<string> WebAppID { get; set; }
        [DataMember]
        public List<string> SiteCollectionID { get; set; }
        [DataMember]
        public List<string> GroupName { get; set; }
        [DataMember]
        public Dictionary<string, bool> GroupNameWithDifStatus { get; set; }
        [DataMember]
        public List<SPPermissionGroup> PermissionGroup { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PermissionInfo
    {
        [DataMember]
        public ArchiveRBtnSelect ArchiveRBtnSelect { get; set; }
        [DataMember]
        public RestoreRBtnSelect RestoreRBtnSelect { get; set; }
        [DataMember]
        public List<string> ArchiveSPPermissionGroup { get; set; }
        [DataMember]
        public List<string> RestoreSPPerGroup { get; set; }
        [DataMember]
        public List<PermissionLevel> PermissionLevel { get; set; }
        [DataMember]
        public string ArchiveGroupToGUI { get; set; }
        [DataMember]
        public string RestoreGroupToGUI { get; set; }
    }



    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ArchiveRBtnSelect : int
    {
        [EnumMember]
        ContributePermissionLevel = 0,
        [EnumMember]
        SiteCollectionAdministrator = 1,
        [EnumMember]
        UserInSPGroup = 2
    }

    public enum RestoreRBtnSelect : int
    {
        [EnumMember]
        SiteCollectionAdministrator = 0,
        [EnumMember]
        UserInSPGroup = 1,
        [EnumMember]
        UserHasSpecialPermission = 2
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PermissionLevel
    {
        [DataMember]
        public long PermissionID { get; set; }
        [DataMember]
        public string PermissionName { get; set; }
        [DataMember]
        public string Level { get; set; }
        [DataMember]
        public string Description { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SPPermissionGroup
    {
        [DataMember]
        public string SiteCollectionID { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public string PermissionLevel { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ReadOnlyStubSetting
    {

        /// <summary>
        /// delete和keep的三个选项,详细内容参考SORules.cs中enum KeepDataOption
        /// </summary>
        [DataMember]
        public int KeepDataOption { get; set; }

        ///// <summary>
        ///// keep的三个选项
        ///// </summary>
        //[DataMember]
        //public List<TagContentInfo> TagContentInfo { get; set; }

        /// <summary>
        /// Leave only a SharePoint stub使用的Logical
        /// </summary>
        [DataMember]
        public string LogicalDeviceId { get; set; }
    }

}
