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



using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.Wrapper.Backup
{
    public class AveSPMySite : AvePoint.Wrapper.Backup.IAveSPMySite
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private AveSPUserProfile mUserProfile = null;
        private string mLogin;

        public AveSPMySite(AveSPSite site)
        {
            mLogin = site.SPSite.Owner.LoginName;
            mUserProfile = new AveSPUserProfile(site, site.SPSite.Owner.LoginName);
        }

        public AveSPMySite(IAveWebApplication webApp, string loginName, AveContextKind contextKind)
        {
            mLogin = loginName;
            mUserProfile = new AveSPUserProfile(webApp, loginName, contextKind);
        }

        public string GetDetailXml()
        {
            return mUserProfile.GetDetailXml();
        }

        public string GetMembershipXml()
        {
            return mUserProfile.GetMembershipXml();
        }

        public string GetColleaguesXml()
        {
            return mUserProfile.GetColleaguesXml();
        }

        public string GetTagsXml()
        {
            return mUserProfile.GetTagsXml();
        }

        public string GetNotesXml()
        {
            return mUserProfile.GetNotesXml();
        }

        public string Export(string myProfileListName)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPMySite.Export"))
            {
                switch (myProfileListName)
                {
                    case AveConstants.MY_COLLEAGUES:
                        return GetColleaguesXml();
                    case AveConstants.MY_DETAILS:
                        return GetDetailXml();
                    case AveConstants.MY_MEMBERSHIPS:
                        return GetMembershipXml();
                    case AveConstants.MY_NOTES:
                        return GetNotesXml();
                    case AveConstants.MY_TAGS:
                        return GetTagsXml();
                    default:
                        return string.Empty;
                }
            }
        }

        public void Export(IAveBackupStream stream, string myProfileListName)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPMySite.Export"))
            {
                AveUserProfileInfo profileInfo = new AveUserProfileInfo();
                profileInfo.LoginName = mLogin;
                stream.WriteMetadata(AveMetadataType.UserProfile, profileInfo);
                switch (myProfileListName)
                {
                    case AveConstants.MY_COLLEAGUES:
                        mUserProfile.ExportColleagues(stream);
                        break;
                    case AveConstants.MY_DETAILS:
                        mUserProfile.ExportDetails(stream);
                        break;
                    ////do not need to backup and restore memberships,remove for Userprofile performance 
                    //case AveConstants.MY_MEMBERSHIPS:
                    //    mUserProfile.ExportMemberships(stream);
                    //    break;
                    case AveConstants.MY_NOTES:
                        mUserProfile.ExportNotes(stream);
                        break;
                    case AveConstants.MY_TAGS:
                        mUserProfile.ExportTags(stream);
                        break;
                    default:
                        return;
                }
            }
        }
    }
}