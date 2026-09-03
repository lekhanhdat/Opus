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
namespace AvePoint.Wrapper.Restore
{
    public interface IAveSPUserProfile
    {
        System.Collections.Hashtable AbsoluteUrlMapping { get; set; }
        bool CheckServiceAvailable();
        bool EnableTag { set; }
        bool ExistSkip { set; }
        AvePoint.Wrapper.Common.Office.IAveOUserProfile FindOrCreatePersonalSite(string loginName);
        void Restore(AvePoint.Wrapper.Common.Office.AveUserProfileInfo profileInfo);
        void Restore(System.Collections.Generic.Dictionary<string, string> userProfileLists);
        void RestoreColleague(AvePoint.Wrapper.Common.Office.AveColleagueInfo colleagueInfo);
        void RestoreComment(AvePoint.Wrapper.Common.AveSocialCommentInfo commentInfo);
        void RestoreDetail(AvePoint.Wrapper.Common.Office.AveUserProfileValueInfo valueInfo);
        void RestoreDetail(AvePoint.Wrapper.Common.Office.AveUserProfileValueInfo valueInfo, bool isOverwrite);
        void RestoreDetails(System.Collections.Generic.List<AvePoint.Wrapper.Common.Office.AveUserProfileValueInfo> valueInfos);
        void RestoreDetails(System.Collections.Generic.List<AvePoint.Wrapper.Common.Office.AveUserProfileValueInfo> valueInfos, bool isOverwrite);
        void RestoreMembership(AvePoint.Wrapper.Common.Office.AveMembershipInfo mInfo);
        void RestoreQuickLink(AvePoint.Wrapper.Common.Office.AveQuickLinkInfo lInfo);
        void RestoreTag(AvePoint.Wrapper.Common.AveSocialTagInfo tagInfo);
        void RestoreUserProfile(System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string>>> userProfiles);
        void RestoreUserProfileProperties(System.Collections.Generic.List<AvePoint.Wrapper.Common.Office.AvePropertyInfo> properties);
        void RestoreUserProfileProperties(System.Collections.Generic.List<AvePoint.Wrapper.Common.Office.AvePropertyInfo> properties, bool isOverwrite);
        void RestoreUserProfileSubTypes(System.Collections.Generic.List<AvePoint.Wrapper.Common.Office.AveUserProfileSubTypeInfo> subTypes);
        void SetWorkingProfileList(string listName);
        IAveSPSite Site { get; set; }
        void UpdateColleages(string xml);
        void UpdateDetails(string xml);
        void UpdateLinks(string xml);
        void UpdateMemberships(string xml);
        void UpdateNotes(string xml);
        void UpdateTags(string xml);
        void UpdateUserProfileColleages(string xml);
        void UpdateUserProfileDetails(string xml);
        void UpdateUserProfileMemberships(string xml);
        void UpdateUserProfileNotes(string xml);
        void UpdateUserProfileTags(string xml);
    }
}
