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



namespace AvePoint.GCommon.Contract.StorageOptimization.Connector.Object.Settings
{
    #region using directives
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class LibrarySetting
    {
        [DataMember]
        public bool UsePathFromParent { get; set; }

        [DataMember]
        public bool LoadPermissionFromFileSystem { get; set; }

        [DataMember]
        public LoadPermissionOption LoadPermissionOption { get; set; }

        [DataMember]
        public bool LoadMetadataFromFileSystem { get; set; }

        [DataMember]
        public bool KeepConsistent { get; set; }

        [DataMember]
        public bool AllowLinkLargeFile { get; set; }

        [DataMember]
        public List<string> SPRoleDefinitions { get; set; }

        //for MediaLibrarySetting:
        [DataMember]
        public bool EnableRTPlayer { get; set; }
        [DataMember]
        public int ThumbnailSize { get; set; }
        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }
        [DataMember]
        public bool AutoPlay { get; set; }
        [DataMember]
        public VideoFormatOption VideoFormat { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum LoadPermissionOption
    {
        [EnumMember]
        LoadRootFolderPermissionOnly,

        [EnumMember]
        LoadRootFolderAndSubFolders,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum VideoFormatOption
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
