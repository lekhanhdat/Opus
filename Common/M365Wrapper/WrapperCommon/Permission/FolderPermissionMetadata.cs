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

using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ExchangeCommonWrapper
{

    [DataContract]
    public class FolderPermissionCollectionM:IEnumerable<FolderPermissionM>
    {
        [DataMember]
        public List<FolderPermissionM> Permissions { get; set; }

        public IEnumerator<FolderPermissionM> GetEnumerator()
        {
            return Permissions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<FolderPermissionM>)Permissions).GetEnumerator();
        }
    }

    [DataContract]
    public class FolderPermissionM
    {
        //[DataMember]
        //public FolderPermissionLevelM DisplayPermissionLevel { get; set; }
        [DataMember]
        public FolderPermissionLevelM PermissionLevel { get; set; }

        [DataMember]
        public IndividualFolderPermissionsM PermissionDetails { get; set; }
        [DataMember]
        public UserIdM UserId { get; set; }

    }

    [DataContract]
    public class IndividualFolderPermissionsM
    {
        [DataMember]
        public bool CanCreateItems { get; set; }
        [DataMember]
        public bool CanCreateSubFolders { get; set; }
        [DataMember]
        public PermissionScopeM DeleteItems { get; set; }
        [DataMember]
        public PermissionScopeM EditItems { get; set; }
        [DataMember]
        public bool IsFolderContact { get; set; }
        [DataMember]
        public bool IsFolderOwner { get; set; }
        [DataMember]
        public bool IsFolderVisible { get; set; }
        [DataMember]
        public FolderPermissionReadAccessM ReadItems { get; set; }

    }

    [DataContract]
    public enum PermissionScopeM
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Owned = 1,
        [EnumMember]
        All = 2
    }

    [DataContract]
    public enum FolderPermissionLevelM
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Owner = 1,
        [EnumMember]
        PublishingEditor = 2,
        [EnumMember]
        Editor = 3,
        [EnumMember]
        PublishingAuthor = 4,
        [EnumMember]
        Author = 5,
        [EnumMember]
        NoneditingAuthor = 6,
        [EnumMember]
        Reviewer = 7,
        [EnumMember]
        Contributor = 8,
        [EnumMember]
        FreeBusyTimeOnly = 9,
        [EnumMember]
        FreeBusyTimeAndSubjectAndLocation = 10,
        [EnumMember]
        Custom = 11
    }

    [DataContract]
    public enum FolderPermissionReadAccessM
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        TimeOnly = 1,
        [EnumMember]
        TimeAndSubjectAndLocation = 2,
        [EnumMember]
        FullDetails = 3
    }

    [DataContract]
    public sealed class UserIdM 
    {
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public string PrimarySmtpAddress { get; set; }
        [DataMember]
        public string SID { get; set; }
        [DataMember]
        public StandardUserM? StandardUser { get; set; }

        public override string ToString()
        {
            return $"({this.DisplayName ?? string.Empty},{this.PrimarySmtpAddress ?? string.Empty},{this.SID ?? string.Empty},{this.StandardUser?.ToString() ?? string.Empty})";
        }
    }

    [DataContract]
    public enum StandardUserM
    {
        [EnumMember]
        Default = 0,
        [EnumMember]
        Anonymous = 1
    }
}