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
    public class AveGroupInfo : IComparable<AveGroupInfo>
    {
        //public string SiteId; //[uniqueidentifier] NOT NULL,
        [DataMember]
        public int ID; //[int] NOT NULL,
        [DataMember]
        public string Domain;
        [DataMember]
        public string Title; //[nvarchar](255) NOT NULL,
        [DataMember]
        public string Description; //[nvarchar](512) NULL,
        [DataMember]
        public int Owner; //[int] NOT NULL,
        [DataMember]
        public AveUserInfo OwnerInfo;
        [DataMember]
        public bool OwnerIsUser; //[bit] NOT NULL,
        //SAAS-8191 增加Group Settings中的四个属性。
        [DataMember]
        public bool AllowMembersEditMembership;
        [DataMember]
        public bool AllowRequestToJoinLeave;
        [DataMember]
        public bool AutoAcceptRequestToJoinLeave;
        [DataMember]
        public bool OnlyAllowMembersViewMembership;
        [DataMember]
        public string DLAlias; //[nvarchar](128) NULL,
        [DataMember]
        public string DLErrorMessage; //[nvarchar](512) NULL,
        [DataMember]
        public Nullable<int> DLFlags = new Nullable<int>(); //[int] NULL,
        [DataMember]
        public Nullable<int> DLJobId = new Nullable<int>(); //[int] NULL,
        [DataMember]
        public string DLArchives; //[varchar](4000) NULL,
        [DataMember]
        public string RequestEmail; //[nvarchar](255) NULL,
        [DataMember]
        public int Flags; //[int] NOT NULL,
        [DataMember]
        public List<int> Memberships = new List<int>();
        [DataMember]
        public ArrayList Roles = new ArrayList();
        [DataMember]
        public ArrayList Users = new ArrayList();

        public int CompareTo(AveGroupInfo other)
        {
            if (other == null)
            {
                return 1;
            }
            return ID.CompareTo(other.ID);
        }
        [DataMember]
        public List<AveUserInfo> Members = new List<AveUserInfo>();
        //记录是否有权限，便于还原时候判断是否还原,null和true都是有权限的
        [DataMember]
        public Nullable<bool> HasPermission = new Nullable<bool>(); //[bit] NULL,
        //New Add Properties for SharingLinks, need to consider old backup data when restore 
        [DataMember]
        public bool IsVerifiedSharelinkGroup; //true:Include AveSharingLinkInfo  //false:currect obj doesn't have the AveSharingLinkInfo
        [DataMember]
        public Guid ShareLinkId;
        [DataMember]
        public AveSharingLinkInfo ShareLink = new AveSharingLinkInfo();
    }

    public class AveGroupList
    {
        public List<AveGroupInfo> Groups = new List<AveGroupInfo>();
    }

    [DataContract]
    public class AveSharingLinkInfo
    {
        [DataMember]
        public Guid ShareId;
        [DataMember]
        public int LinkKind;
        [DataMember]
        public string Expiration;
        [DataMember]
        public bool AllowsAnonymousAccess;
        [DataMember]
        public bool RestrictedShareMembership;
        [DataMember]
        public bool BlocksDownload;
        [DataMember]
        public bool IsEditLink;
        [DataMember]
        public int Scope;
        [DataMember]
        public bool IsReviewLink;
        [DataMember]
        public bool RequiresPassword;
    }
}
