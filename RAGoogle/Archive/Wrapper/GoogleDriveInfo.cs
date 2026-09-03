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
using AvePoint.GCommon.Contract.Media.Object;
using Google.Apis.Drive.v3.Data;
using RAGoogle.Models.GoogleObjectModel;
using System.Runtime.Serialization;

namespace RAGoogle.Archive.Wrapper
{
    [DataContract]
    [Serializable]
    public class GoogleDriveBasic
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public DateTime CreatedTime { get; set; }
        [DataMember]
        public string ColorRgb { get; set; }
        [DataMember]
        public bool Hidden { get; set; }
        [DataMember]
        public string Kind { get; set; }
    }
    [DataContract]
    [Serializable]
    public class GoogleDriveSetting
    {
        #region restrictions
        [DataMember]
        public bool CopyRequiresWriterPermission { get; set; }        //Allow viewers and commenters to download, print, and copy files
        [DataMember]
        public bool DomainUsersOnly { get; set; }         //Allow people outside of domain to access files
        [DataMember]
        public bool DriveMembersOnly { get; set; }        //Allow people who aren't shared drive members to access files
        [DataMember]
        public bool SharingFoldersRequiresOrganizerPermission { get; set; }        //Allow content managers to share folders
        [DataMember]
        public bool? AdminManagedRestrictions { get; set; }
        #endregion
    }
    [DataContract]
    [Serializable]
    public class GoogleDriveMember
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public string Role { get; set; }
        [DataMember]
        public string PhotoLink { get; set; }
        [DataMember]
        public string EmailAddress { get; set; }
        [DataMember]
        public string Domain { get; set; }
        [DataMember]
        public bool? AllowFileDiscovery { get; set; }
        [DataMember]
        public long ExpirationTime { get; set; }
        [DataMember]
        public List<AvePermissionDetailsData> PermissionDetails { get; set; }
    }
    [DataContract]
    [Serializable]
    public class PermissionInfo
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Role { get; set; }
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public string EmailAddress { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public bool? AllowFileDiscovery { get; set; }
        [DataMember]
        public bool? Deleted { get; set; }
        [DataMember]
        public string PhotoLink { get; set; }
        [DataMember]
        public string ExpirationTimeRaw { get; set; }
        [DataMember]
        public long ExpirationTime { get; set; }
        [DataMember]
        public string Domain { get; set; }
        [DataMember]
        public List<AvePermissionDetailsData> PermissionDetails { get; set; }
    }
    [DataContract]
    [Serializable]
    public class GDPermissionList
    {
        [DataMember]
        public List<PermissionInfo> Permissions { get; set; }
    }
    [DataContract]
    [Serializable]
    public class GDFileBasic
    {
        [DataMember]
        public string DocId { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string OriginalFilename { get; set; }
        [DataMember]
        public string Path { get; set; }
        [DataMember]
        public string MimeType { get; set; }
        [DataMember]
        public long? Size { get; set; }
        [DataMember]
        public string DriveName { get; set; }
        [DataMember]
        public string ParentId { get; set; }
        [DataMember]
        public string ParentIds { get; set; }
        [DataMember]
        public string Level { get; set; }
        [DataMember]
        public long CreatedTime { get; set; }
        [DataMember]
        public long ModifiedTime { get; set; }
        [DataMember]
        public UserData Owners { get; set; }
        [DataMember]
        public string ModifiedById { get; set; }
        [DataMember]
        public string ModifiedBy { get; set; }
        [DataMember]
        public string MemberEmail { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public bool? Starred { get; set; }
        [DataMember]
        public List<LabelData>? Labels { get; set; }
        [DataMember]
        public List<ContentRestrictionData>? ContentRestrictions { get; set; }
        public int Type { get; set; }
        [DataMember]
        public string AppProperties { get; set; }
        [DataMember]
        public string Properties { get; set; }
        [DataMember]
        public string ColorRgb { get; set; }
        [DataMember]
        public string CreatedBy { get; set; }
        [DataMember]
        public string DriveId { get; set; }
        [DataMember]
        public bool IsCurrentVersion { get; set; }


    }
    public class AvePermissionDetailsData
    {
        public string PermissionType { get; set; }
        public string PermissionRole { get; set; }
        public string InheritedFrom { get; set; }
        public bool? Inherited { get; set; }
    }
    public class UserData
    {
        public string OwnerEmail { get; set; }
        public string OwnerDisplayName { get; set; }
    }
    public class LabelData
    {
        public string Id { get; set; }
        public string Kind { get; set; }
        public string RevisionId { get; set; }
        public Dictionary<string, LabelFieldData> Fields { get; set; }
    }
    public class LabelFieldData
    {
        public List<string> DateString { get; set; }
        public string Id { get; set; }
        public List<long?> Integer { get; set; }
        public string Kind { get; set; }
        public List<string> Selection { get; set; }
        public List<string> Text { get; set; }
        public List<UserData> User { get; set; }
        public string ValueType { get; set; }
    }
    public class ContentRestrictionData
    {
        public bool? ReadOnly { get; set; }
        public string Reason { get; set; }
    }
}
