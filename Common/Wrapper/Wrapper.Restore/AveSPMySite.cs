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
using System.IO;
using System.Xml;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Common;
using System.Collections;
using AvePoint.Wrapper.Common.Office;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.Wrapper.Resource;

namespace AvePoint.Wrapper.Restore
{
    public class AveSPMySite : RestoreableObject
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private AveSPUserProfile mUserProfile = null;
        private string mWorkingListName;
        private AveSPSite mAveParentSite;

        public AveSPSite ParentSite
        {
            get { return mAveParentSite; }
        }

        public AveSPMySite(AveSPSite _aveSite)
        {
            mAveParentSite = _aveSite;
            mUserProfile = new AveSPUserProfile(mAveParentSite);
        }

        //internal AveSPMySite(IAveWebApplication webApp, string loginName, AveContextKind contextKind)
        //{
        //    mUserProfile = new AveSPUserProfile(mAveParentSite, webApp, loginName, contextKind);
        //    mUserProfile.ParentSite = mAveParentSite;
        //}

        public void SetAbsoluteUrlMapping(Hashtable mapping)
        {
            mapping = mUserProfile.AbsoluteUrlMapping;
        }

        [SPDisposeCheck.SPDisposeCheckIgnore(SPDisposeCheck.SPDisposeCheckID._400, "Ignoring this error")]
        public static IAveSite FindOrCreatePersonalSite(IAveWebApplication webApp, string loginName, uint LCID, AveObjectModelFactory oMFactory)
        {
            if (!AveSPUtility.IfServiceAvailable(webApp, ServiceApplicationType.UserProfileService))
            {
                //mLog.Log(AveLogLevel.ERROR, string.Format("There is no User Profile Service associate with the web application: {0}", webApp.AlternateUrls.GetResponseUrl(AveUrlZone.Default).Uri));
                log.Error("There is no User Profile Service associate with the web application: {0}", webApp.AlternateUrls.GetResponseUrl(AveUrlZone.Default).Uri.ToString());
                return null;
            }

            //AveObjectModelFactory oMFactory = AveObjectModelFactory.CreateObjectModelFactory("", new AveBPOSAccountInfo(), contextKind);
            IAveSiteSubscriptionIdentifier siteSubscriptionIdentifier = oMFactory.CreateSiteSubscriptionIdentifier();
            IAveServiceContext context = oMFactory.CreateServiceContext();
            context = context.GetContext(webApp.ServiceApplicationProxyGroup, siteSubscriptionIdentifier.Default);
            IAveOUserProfileManager userProfileManager = oMFactory.CreateUserProfileManager(context);
            IAveOUserProfile userProfile = null;
            if(userProfileManager.UserExists(loginName))
            {
                userProfile = userProfileManager.GetUserProfile(loginName);
            }
            else
            {
                userProfile = userProfileManager.CreateUserProfile(loginName);
            }
            if (userProfile.PersonalSite == null)
            {
                userProfile.CreatePersonalSite((int)LCID);
            }
            return userProfile.PersonalSite;
        }

        private void UpdateDetails(string xml)
        {
            mUserProfile.UpdateDetails(xml);
        }

        private void UpdateMemberships(string xml)
        {
            mUserProfile.UpdateMemberships(xml);
        }

        private void UpdateColleages(string xml)
        {
            mUserProfile.UpdateColleages(xml);
        }

        private void UpdateTags(string xml)
        {
            mUserProfile.UpdateTags(xml);
        }

        private void UpdateNotes(string xml)
        {
            mUserProfile.UpdateNotes(xml);
        }

        private void UpdateLinks(string xml)
        {
            mUserProfile.UpdateLinks(xml);
        }

        public void Restore(string xml)
        {
            switch (mWorkingListName)
            {
                case AveConstants.MY_COLLEAGUES:
                    UpdateColleages(xml);
                    break;
                case AveConstants.MY_DETAILS:
                    UpdateDetails(xml);
                    break;
                case AveConstants.MY_MEMBERSHIPS:
                    UpdateMemberships(xml);
                    break;
                case AveConstants.MY_NOTES:
                    UpdateNotes(xml);
                    break;
                case AveConstants.MY_TAGS:
                    UpdateTags(xml);
                    break;
                case AveConstants.MY_LINKS:
                    UpdateLinks(xml);
                    break;
                default:
                    throw new Exception("Invalid profile list name: " + mWorkingListName);
            }
        }

        public void SetWorkingProfileList(string listName)
        {
            mWorkingListName = listName;
        }
    }
}
