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



namespace AvePoint.GCommon.Contract.Media.Object
{
    #region using directives
    using System;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRBlobRestoreItemDto
    {
        /// <summary>
        /// please construct the select command using as, for example 
        /// SELECT Folder.Id as Folder_ID, SPObject.Id as SPOBject_Id FROM Folder, SPObject, SPObjectFolderMap where SPObject.Id=SPObjectFolderMap.SPObjectid and Folder.Id = SPObjectFolderMap.FolderId
        /// </summary>
        #region SPObject table
        [DataMember]
        public String SPObject_Id { get; set; }

        [DataMember]
        public String SPObject_SPId { get; set; }

        [DataMember]
        public String SPObject_ContentDBId { get; set; }

        [DataMember]
        public String SPObject_Name { get; set; }

        [DataMember]
        public String SPObject_Title { get; set; }

        [DataMember]
        public String SPObject_URL { get; set; }

        [DataMember]
        public Int32 SPObject_BlobType { get; set; }

        [DataMember]
        public String SPObject_SPType { get; set; }

        [DataMember]
        public String SPObject_ParentId { get; set; }
        #endregion

        #region  SPObjectFolderMap table
        [DataMember]
        public String SPObjectFolderMap_SPObjectId { get; set; }
        [DataMember]
        public String SPObjectFolderMap_FolderId { get; set; }
        #endregion

        #region PhysicalDevice table
        [DataMember]
        public String PhysicaDevice_Id { get; set; }
        [DataMember]
        public String PhysicalDevice_PhysicalDeviceId { get; set; }
        [DataMember]
        public String PhysicaDevice_Type { get; set; }
        [DataMember]
        public String PhysicalDevice_Name { get; set; }
        [DataMember]
        public String PhysicaDevice_SpaceType { get; set; }
        [DataMember]
        public String PhysicalDevice_Location { get; set; }
        [DataMember]
        public String PhysicaDevice_Path { get; set; }
        [DataMember]
        public String PhysicalDevice_ConnectionString { get; set; }
        [DataMember]
        public String PhysicalDevice_PhysicalDeviceDto { get; set; }
        #endregion

        #region  Folder table
        [DataMember]
        public String Folder_Id { get; set; }
        [DataMember]
        public String Folder_Path { get; set; }
        [DataMember]
        public String Folder_DestinationPath { get; set; }
        [DataMember]
        public String Folder_DstHighName { get; set; }
        [DataMember]
        public String Folder_DstLowName { get; set; }
        [DataMember]
        public String Folder_DstExtraStorageInfo { get; set; }
        [DataMember]
        public String Folder_Name { get; set; }
        [DataMember]
        public String Folder_Type { get; set; }
        [DataMember]
        public Int64 Folder_Length { get; set; }
        [DataMember]
        public String Folder_ParentId { get; set; }
        [DataMember]
        public String Folder_PhysicalDeviceId { get; set; }
        [DataMember]
        public String Folder_StubDBId { get; set; }
        [DataMember]
        public String Folder_ExInt1 { get; set; }
        [DataMember]
        public String Folder_ExInt2 { get; set; }
        [DataMember]
        public String Folder_ExNChar1 { get; set; }
        [DataMember]
        public String Folder_ExNChar2 { get; set; }
        [DataMember]
        public String Folder_ExText { get; set; }
        #endregion

        public override String ToString()
        {
            return String.Format("SP Object Id: {0}, SP Object Folder Map SP Object Id: {1}, Physica Device Id: {2}",
                this.SPObject_Id,
                this.SPObjectFolderMap_SPObjectId,
                this.PhysicaDevice_Id);
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum CommandType
    {
        [DataMember]
        Site_Lists = 0,

        [DataMember]
        ContentDB_Lists = 1,

        [DataMember]
        Lists = 2,

        [DataMember]
        Unknown = -1
    }
}
