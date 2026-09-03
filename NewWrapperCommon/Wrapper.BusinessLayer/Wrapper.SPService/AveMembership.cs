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
using System.Linq;
using System.Text;
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.SPService;

namespace AvePoint.Wrapper.SPService
{
    public class AveMembership
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private AveUserProfile mUserProfile;

        public AveMembership(AveUserProfile profile)
        {
            mUserProfile = profile;
        }

        public void CreateMembership(AveMembershipInfo membershipInfo)
        {
            if (mUserProfile == null || mUserProfile.UserProfile == null)
            {
                return;
            }
            IAveOMembership membership = null;
            IAveOUserProfileManager profileManager = mUserProfile.UserProfile.ProfileManager;
            try
            {
                //XmlElement subxe = (XmlElement)xe.ChildNodes[0];
                //XmlElement subxe1 = (XmlElement)xe.ChildNodes[1];
                Guid source = membershipInfo.MembershipGroup.SourceInternal;// new Guid(subxe1.Attributes["SourceInternal"].Value);
                string displayName = membershipInfo.MembershipGroup.DisplayName;// subxe1.Attributes["DisplayName"].Value;
                string mailNickName = membershipInfo.MembershipGroup.MailNickName;// subxe1.Attributes["MailNickName"].Value;
                string description = membershipInfo.MembershipGroup.Description;// subxe1.Attributes["Description"].Value;
                string url = membershipInfo.MembershipGroup.Url;//subxe1.Attributes["Url"].Value;
                string sourceReference = membershipInfo.MembershipGroup.SourceReference;// subxe1.Attributes["SourceReference"].Value;

                IAveOMemberGroup memberGroup = null;
                try
                {
                    memberGroup = profileManager.GetMemberGroups().GetMemberGroupBySourceAndSourceReference(source, sourceReference);
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.INFO, "Can not find specific member group in the destination, error: {0}", e.ToString());
                    memberGroup = profileManager.GetMemberGroups().CreateMemberGroup(source, displayName, mailNickName, description, url, sourceReference);
                }
                try
                {
                    membership = mUserProfile.UserProfile.Memberships[memberGroup];
                }
                catch(Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperSPServiceResource.GetMemberShipFaild, e.ToString());
                    membership = null;
                }

                if (membership == null)
                {
                    AveMembershipGroupType groupType = (AveMembershipGroupType)membershipInfo.GroupType;// Convert.ToInt32(xe.Attributes["GroupType"].Value);
                    string groupName = membershipInfo.Group;//xe.Attributes["Group"].Value;
                    AvePrivacy privacyLevel = (AvePrivacy)membershipInfo.PrivacyLevel;//Convert.ToInt32(xe.Attributes["PrivacyLevel"].Value);
                    membership = mUserProfile.UserProfile.Memberships.Create(memberGroup, groupType, groupName, privacyLevel);
                    if (membership.IsTitleEditable)
                        membership.Title = membershipInfo.Title;//xe.Attributes["Title"].Value;
                    if (membership.IsUrlEditable)
                        membership.Url = membershipInfo.Url;//xe.Attributes["Url"].Value;
                    membership.Policy.DisplayName = membershipInfo.Policy.DisplayName;//subxe.Attributes["DisplayName"].Value;
                    membership.Policy.Group = membershipInfo.Policy.Group;//subxe.Attributes["Group"].Value;
                    membership.Policy.PrivacyPolicy = (AvePrivacyPolicy)membershipInfo.Policy.PrivacyPolicy;//Convert.ToInt32(subxe.Attributes["PrivacyPolicy"].Value);
                    membership.Policy.UserOverridePrivacy = membershipInfo.Policy.UserOverridePrivacy;//Convert.ToBoolean(subxe.Attributes["UserOverridePrivacy"].Value);
                }
                else
                {
                    membership.Group = membershipInfo.Group;//xe.Attributes["Group"].Value;
                    membership.GroupType = (AveMembershipGroupType)membershipInfo.GroupType;//Convert.ToInt32(xe.Attributes["GroupType"].Value);
                    if (membership.IsTitleEditable)
                        membership.Title = membershipInfo.Title;//xe.Attributes["Title"].Value;
                    if (membership.IsUrlEditable)
                        membership.Url = membershipInfo.Url;//xe.Attributes["Url"].Value;
                    membership.Policy.DisplayName = membershipInfo.Policy.DisplayName;//subxe.Attributes["DisplayName"].Value;
                    membership.Policy.Group = membershipInfo.Policy.Group;//subxe.Attributes["Group"].Value;
                    membership.Policy.PrivacyPolicy = (AvePrivacyPolicy)membershipInfo.Policy.PrivacyPolicy;//Convert.ToInt32(subxe.Attributes["PrivacyPolicy"].Value);
                    membership.Policy.UserOverridePrivacy = membershipInfo.Policy.UserOverridePrivacy;//Convert.ToBoolean(subxe.Attributes["UserOverridePrivacy"].Value);
                }
                try
                {
                    if (membership != null)
                        membership.Commit();
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.ERROR, WrapperSPServiceResource.UpdateMembershipError, membership.Title, e.ToString());
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.ERROR, WrapperSPServiceResource.CreateMembershipError, e);
            }
        }

        public void CreateMemberships(List<AveMembershipInfo> memberships)
        {
            if (memberships == null)
            {
                return;
            }
            foreach (AveMembershipInfo m in memberships)
            {
                CreateMembership(m);
            }
        }
    }
}
