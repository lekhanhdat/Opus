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

namespace AvePoint.GCommon.Contract.StorageOptimization.Connector.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ListRoleSettingDto
    {
        [DataMember]
        public List<string> SPRoleDefinitions { get; set; }
        [DataMember]
        public LoadPermissionDto LoadPermission { get; set; }
        [DataMember]
        public bool LoadMetaData { get; set; }
        [DataMember]
        public bool KeepConsistent { get; set; }
        [DataMember]
        public bool AllowLargeFile { get; set; }
        [DataMember]
        public MediaLibrarySettingDto MediaLibrarySetting { get; set; }
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
     public enum  LoadPermissionDto
     {
         [EnumMember]
         None = 0,
         [EnumMember]
         RootFolderOnly = 1,
         [EnumMember]
         RootFolderAndSubs = 2
     }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class MediaLibrarySettingDto 
    {
        [DataMember]
        public PlayerSettingDto PlayerSetting { get; set; }
        [DataMember]
        public bool EnableRTPlayer { get; set; }
        [DataMember]
        public int ThumbnailSize { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PlayerSettingDto
    {
        [DataMember]
        public VideoFormatDto VideoFormat { get; set; }
        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }
        [DataMember]
        public bool AutoPlay { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum VideoFormatDto
    {
        [EnumMember]
        Default = 0,
        [EnumMember]
        Format4x3 = 1,
        [EnumMember]
        Format16x9 = 2,
        [EnumMember]
        Custom = 3
    }

}
