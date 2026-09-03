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

namespace AvePoint.Wrapper.Common
{
    public interface IAveUserProfile
    {
        string AccountName { get; }
        string DisplayName { get; set; }
        // FollowedContent FollowedContent{get;}
        bool IsPeopleList { get; }
        bool IsPrivacySettingOn { get; }
        bool IsSelf { get; }
        string JobTitle { get; }
        int MySiteFirstRunExperience { get; set; }
        string MySiteHostUrl { get; }
        int O15FirstRunExperience { get; set; }
        IAveSite PersonalSite { get; }
        AvePersonalSiteCapabilities PersonalSiteCapabilities { get; }
        string PersonalSiteFirstCreationError { get; }
        DateTime PersonalSiteFirstCreationTime { get; }
        AvePersonalSiteInstantiationState PersonalSiteInstantiationState { get; }
        DateTime PersonalSiteLastCreationTime { get; }
        int PersonalSiteNumberOfRetries { get; }
        bool PictureImportEnabled { get; set; }
        string PictureUrl { get; }
        string Url { get; }
        string SipAddress { get; }
        string UrlToCreatePersonalSite { get; }
        void CreatePersonalSite();
        void CreatePersonalSiteEnque();
        //protected override bool InitOnePropertyFromJson(string peekedName, JsonReader reader);
        void ShareAllSocialData(bool shareAll);
        void SetMySiteFirstRunExperience(int value);
        // static ClientResult<int> CreatePersonalSiteSyncFromWorkItem(ClientRuntimeContext context, Guid workItemType);
        void CreatePersonalSite(int lcid);
        // ClientResult<int> CreatePersonalSiteFromWorkItem(Guid workItemType);
        void CreatePersonalSiteEnque(bool isInteractive);
    }
}
