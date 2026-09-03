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
using System.Xml;
using AvePoint.Wrapper.Common.Office;
using System.Reflection;
using AvePoint.GCommon;
using System.Collections;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.SPService;

namespace AvePoint.Wrapper.SPService
{
    public class AveColleague
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private static AveVolatileCache<string, List<AveColleagueInfo>> mColleagues = new AveVolatileCache<string, List<AveColleagueInfo>>();

        private AveUserProfile mUserProfile;

        public AveColleague(AveUserProfile userProfile)
        {
            mUserProfile = userProfile;
        }

        public void CreateColleagues(List<AveColleagueInfo> colleagueInfos)
        {
            if (colleagueInfos == null)
            {
                return;
            }
            foreach (AveColleagueInfo info in colleagueInfos)
            {
                CreateColleague(info);
            }
        }

        public void CreateColleague(AveColleagueInfo colleagueInfo)
        {
            if (mUserProfile == null || mUserProfile.UserProfile == null)
            {
                return;
            }
            try
            {
                IAveOUserProfileManager profileManager = mUserProfile.UserProfile.ProfileManager;
                string profileName = mUserProfile.ServiceContext.GetMappingUser(colleagueInfo.AccountName);
                IAveOColleague colleague = null;

                foreach (IAveOColleague c in mUserProfile.UserProfile.Colleagues.GetItems())
                {
                    try
                    {
                        string title = string.Empty;
                        if (c.Title.IndexOf("\\", StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            title = c.Title.Substring(c.Title.IndexOf("\\", StringComparison.OrdinalIgnoreCase) + 1);
                        }
                        else
                            title = c.Title;
                        if (profileName == title)
                        {
                            if (mUserProfile.Overwrite)
                            {
                                c.Delete();
                                break;
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, WrapperSPServiceResource.UpdateCollegueError, e);
                    }
                }

                AvePolicyInfo policy = colleagueInfo.Policy;
                if (!profileManager.UserExists(profileName))
                {
                    profileManager.CreateUserProfile(profileName);
                }
                if (profileManager.UserExists(profileName))
                {

                    if (mUserProfile.UserProfile.Colleagues.IsColleague(profileManager.GetUserProfile(profileName).ID))
                    {
                        colleague = mUserProfile.UserProfile.Colleagues[profileManager.GetUserProfile(profileName)];
                        colleague.Group = colleagueInfo.Group;
                        colleague.GroupType = (AveColleagueGroupType)colleagueInfo.GroupType;
                        colleague.Policy.DisplayName = colleagueInfo.Policy.DisplayName;
                        colleague.Policy.Group = colleagueInfo.Group;
                        colleague.Policy.PrivacyPolicy = (AvePrivacyPolicy)colleagueInfo.Policy.PrivacyPolicy;
                        colleague.Policy.UserOverridePrivacy = colleagueInfo.Policy.UserOverridePrivacy;
                        if (colleague.IsTitleEditable)
                            colleague.Title = colleagueInfo.Title;
                        if (colleague.IsUrlEditable)
                            colleague.Url = colleagueInfo.Url;
                    }
                    else
                    {
                        string strGroup = colleagueInfo.Group;
                        AveColleagueGroupType colleagueGroupType = (AveColleagueGroupType)colleagueInfo.GroupType;
                        bool isInWorkGroup = colleagueInfo.IsInWorkGroup;
                        AvePrivacy privacyLevel = (AvePrivacy)colleagueInfo.PrivacyLevel;
                        try
                        {
                            colleague = mUserProfile.UserProfile.Colleagues.Create(profileManager.GetUserProfile(profileName), colleagueGroupType, strGroup, isInWorkGroup, privacyLevel);
                            colleague.Policy.DisplayName = colleagueInfo.Policy.DisplayName;
                            colleague.Policy.Group = colleagueInfo.Group;
                            colleague.Policy.PrivacyPolicy = (AvePrivacyPolicy)colleagueInfo.Policy.PrivacyPolicy;
                            colleague.Policy.UserOverridePrivacy = colleagueInfo.Policy.UserOverridePrivacy;
                            if (colleague.IsTitleEditable)
                                colleague.Title = colleagueInfo.Title;
                            if (colleague.IsUrlEditable)
                                colleague.Url = colleagueInfo.Url;
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.WARN, WrapperSPServiceResource.SetColleaguePolicyError, colleague.Title, e);
                        }
                    }
                    try
                    {
                        if (colleague != null)
                            colleague.Commit();
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, "Cannot update colleague: {0} error: {1}", colleague.Title, e.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, "UpdateColleagues error: {0}", e.ToString());
            }
        }
    }
}
