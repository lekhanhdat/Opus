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
using AvePoint.ObjectModel.WebService;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Client.UserProfiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.ObjectModel.ClientOM
{
    public partial class AveClientOMOffice365Request : AveClientOM2019Request
    {
        [NoAPI("现有client API不足以获取当前UserProfile信息。")]
        public override Dictionary<string, object> GetUserProfileByName(string accountName, bool isOnlineSite)
        {
            return base.GetUserProfileByName(accountName, isOnlineSite);
        }
        [NoAPI]
        public override Dictionary<string, object> GetUserProfileManager()
        {
            return base.GetUserProfileManager();
        }
        [NoAPI]
        public override Dictionary<string, object> GetAudienceManager()
        {
            return base.GetAudienceManager();
        }

        [NoAPIAttribute]
        public override Dictionary<string, object> AddUserProfile(string accountName)
        {
            return base.AddUserProfile(accountName);
        }

        [NoAPI]
        public override Dictionary<string, object> RestoreUserProfileProperties(Dictionary<string, object> userProfilePropertiesInfo, bool isOverWrite)
        {
            return base.RestoreUserProfileProperties(userProfilePropertiesInfo, isOverWrite);
        }
        [NoAPI("There is no API to restore Colleagues, MemberShips, Links")]
        public override Dictionary<string, object> RestoreUserProfileInfo(Dictionary<string, object> userProfileInfo, bool isOnlineSite, bool isExistSkip)
        {
            return base.RestoreUserProfileInfo(userProfileInfo, isOnlineSite, isExistSkip);
        }
        [NoAPI]
        public override void UpdateUserProfileDetails(string accountName, string xml)
        {
            base.UpdateUserProfileDetails(accountName, xml);
        }
        [NoAPI]
        public override void UpdateUserProfileMemberships(string accountName, string xml)
        {
            base.UpdateUserProfileMemberships(accountName, xml);
        }
        [NoAPI]
        public override void UpdateUserProfileColleages(string accountName, string xml)
        {
            base.UpdateUserProfileColleages(accountName, xml);
        }
        [NoAPI]
        public override void UpdateUserProfileTags(string accountName, string xml)
        {
            base.UpdateUserProfileTags(accountName, xml);
        }
    }
}
