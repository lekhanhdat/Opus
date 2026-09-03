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
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.SPService
{
    public class AveQuickLink
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private AveUserProfile mUserProfile;

        public AveQuickLink(AveUserProfile profile)
        {
            mUserProfile = profile;
        }
        public void CreateQuickLinks(List<AveQuickLinkInfo> linkInfos)
        {
            if (linkInfos == null)
            {
                return;
            }
            foreach (AveQuickLinkInfo link in linkInfos)
            {
                CreateQuickLink(link);
            }
        }

        public void CreateQuickLink(AveQuickLinkInfo linkInfo)
        {
            if (mUserProfile == null || mUserProfile.UserProfile == null)
            {
                return;
            }
            try
            {
                IAveOQuickLink quickLink = null;
                string profileManagerUrl = linkInfo.ProfileManagerUrl;// xe.Attributes["ProfileManagerUrl"].Value;
                foreach (IAveOQuickLink link in mUserProfile.UserProfile.QuickLinks.GetItems())
                {
                    if (link.Title == linkInfo.Title)// xe.Attributes["NameValue"].Value)
                    {
                        if (mUserProfile.Overwrite)
                        {
                            link.Delete();
                            break;
                        }
                        else
                        {
                            return;
                        }
                    }
                }

                string strTitle = linkInfo.Title;// xe.Attributes["Title"].Value;
                string strUrl = linkInfo.Url;// xe.Attributes["Url"].Value;

                if (!mUserProfile.AbsoluteUrlMapping.ContainsKey(mUserProfile.SourceSiteInfo.Url))
                {
                    mUserProfile.AbsoluteUrlMapping[mUserProfile.SourceSiteInfo.Url] = mUserProfile.DestSiteUrl;
                }
                strUrl =  AveReplaceProcessor.UrlReplace(strUrl, mUserProfile.AbsoluteUrlMapping, new ReplaceOption(true, true), mUserProfile.SourceSiteInfo, mUserProfile.DestSiteUrl);
               
                if (strUrl.Contains(profileManagerUrl))//
                {
                    strUrl = strUrl.Replace(profileManagerUrl, mUserProfile.UserProfile.ProfileManager.MySiteHostUrl.ToString());
                }
                string strGroup = linkInfo.Group;// xe.Attributes["Group"].Value;
                AveQuickLinkGroupType groupType = (AveQuickLinkGroupType)linkInfo.GroupType;//Convert.ToInt32(xe.Attributes["GroupType"].Value);
                AvePrivacy privacyLevel = (AvePrivacy)linkInfo.PrivacyLevel;//Convert.ToInt32(xe.Attributes["PrivacyLevel"].Value);
                try
                {
                    quickLink = mUserProfile.UserProfile.QuickLinks.Create(strTitle, strUrl, groupType, strGroup, privacyLevel);
                    quickLink.Policy.DefaultPrivacy = (AvePrivacy)linkInfo.Policy.DefaultPrivacy;//Convert.ToInt32(policyXml.Attributes["DefaultPrivacy"].Value);
                    quickLink.Policy.DisplayName = linkInfo.Policy.DisplayName;//policyXml.Attributes["DisplayName"].Value;
                    quickLink.Policy.Group = linkInfo.Policy.Group;//policyXml.Attributes["Group"].Value;
                    quickLink.Policy.PrivacyPolicy = (AvePrivacyPolicy)linkInfo.Policy.PrivacyPolicy;//Convert.ToInt32(policyXml.Attributes["PrivacyPolicy"].Value);
                    quickLink.Policy.UserOverridePrivacy = linkInfo.Policy.UserOverridePrivacy;//Convert.ToBoolean(policyXml.Attributes["UserOverridePrivacy"].Value);
                }
                catch (Exception e)
                {
                    mLog.Log(AveLogLevel.ERROR, "WP10RTMySite0531", e);
                }
                quickLink?.Commit();
            }
            catch (Exception e)
            {
                mLog.Log(AveLogLevel.ERROR, "WP10RTMySite0530", e);
            }
        }
    }
}
