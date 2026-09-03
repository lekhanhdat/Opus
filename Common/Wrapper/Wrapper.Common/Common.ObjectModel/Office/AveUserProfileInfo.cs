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
using System.Collections.Generic;
using System.Text;

namespace AvePoint.Wrapper.Common.Office
{
    public class AveUserProfileInfo
    {
        public string LoginName;

        public List<AveColleagueInfo> Colleagues = new List<AveColleagueInfo>();
        public List<AveQuickLinkInfo> Links = new List<AveQuickLinkInfo>();
        public List<AveUserProfileValueInfo> Properties = new List<AveUserProfileValueInfo>();
        public List<AveSocialCommentInfo> Comments = new List<AveSocialCommentInfo>();
        public List<AveSocialTagInfo> Tags = new List<AveSocialTagInfo>();
        public List<AveMembershipInfo> Memberships = new List<AveMembershipInfo>();
    }

    public class AveUserProfileValueInfo
    {
        public string Name;
        public int Capacity;
        public int Count;
        public int Privacy;
        public List<string> Values = new List<string>();
        //public AvePropertyInfo Property;
    }

    public class AveColleagueInfo
    {
        public string NameValue;
        public string AccountName;
        public string Group;
        public int GroupType;
        public bool IsInWorkGroup;
        public int PrivacyLevel;
        public bool IsAssistant;
        public bool IsEditable;
        public bool IsPrivacyLevelEditable;
        public bool IsTitleEditable;
        public bool IsUrlEditable;
        public string Url;
        public string Title;
        public AvePolicyInfo Policy;
    }

    public class AvePolicyInfo
    {
        public bool AllowPolicyOverride;
        public int DefaultPrivacy;
        public string DisplayName;
        public bool FilterPrivacyItems;
        public string Group;
        public int PrivacyPolicy;
        public bool UserOverridePrivacy;
    }

    public class AveQuickLinkInfo
    {
        public string ProfileManagerUrl;

        public string Title;
        public string Url;
        public string Group;
        public int GroupType;
        public int PrivacyLevel;

        public AvePolicyInfo Policy;
    }

    public class AveMembershipInfo
    {
        public string Title;
        public string Group;
        public string Url;
        public int GroupType;
        public int PrivacyLevel;
        public bool IsAssistant;
        public bool IsEditable;
        public bool IsPrivacyLevelEditable;
        public bool IsTitleEditable;
        public bool IsUrlEditable;

        public AvePolicyInfo Policy;
        public AveMembershipGroup MembershipGroup;
    }

    public class AveMembershipGroup
    {
        public long Count;
        public string Description;
        public string DisplayName;
        public long Id;
        public long LastUpdate;
        public string MailNickName;
        public int Source;
        public Guid SourceInternal;
        public string SourceReference;
        public string Url;
    }

    public class AvePropertyInfo
    {
        public bool AllowPolicyOverride;
        public ChoiceTypes ChoiceType;
        public AvePrivacy DefaultPrivacy;
        public string Description;
        public int DescriptionLocalized;
        public string DisplayName;
        public int DisplayNameLocalized;
        public int DisplayOrder;

        public bool IsAdminEditable;
        public bool IsAlias;
        public bool IsColleagueEventLog;
        public bool IsImported;
        public bool IsMultivalued;
        public bool IsReplicable;
        public bool IsRequired;
        public bool IsSearchable;
        public bool IsSection;
        public bool IsSystem;
        public bool IsUpgrade;
        public bool IsUpgradePrivate;
        public bool IsUserEditable;
        public bool IsVisibleOnEditor;
        public bool IsVisibleOnViewer;

        public int Length;
        public string ManagedPropertyName;
        public int MaximumShown;
        public string Name;
        public int PrivacyPolicy;
        public int Separator;
        public string Type;
        public string URI;
        public bool UserOverridePrivacy;
    }
    public enum ChoiceTypes
    {
        Off,
        None,
        Open,
        Closed,
    }
}
