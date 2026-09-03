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
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.SPService;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveUserProfileSerializer : IAveUserProfileSerializer,IDisposable
    {
        private AveSite site;
        private AveUserProfile aveProfile;
        public bool ExistSkip { get; set; }
        public AveUserProfileSerializer(AveSite site, string login, bool needInit, AveSiteInfo sourceSiteInfo, AveServerObjectModelFactory factory) : this(site, login, needInit, sourceSiteInfo, factory, null)
        {
        }
        public AveUserProfileSerializer(AveSite site,String login,bool needInit,AveSiteInfo sourceSiteInfo,AveServerObjectModelFactory factory,Func<String,String> userMapping)
        {
            this.site = site;
            var serviceContext = new AvePoint.Wrapper.SPService.AveServiceContext(site, factory);
            serviceContext.UserMap = userMapping.Invoke;
            aveProfile = new AveUserProfile(serviceContext, needInit, sourceSiteInfo, site.Url);
        }
        /// <summary>
        /// 还原user profile PropertiesWithSecton属性；
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="isOverWrite"></param>
        public void SetObjectData(List<AvePropertyInfo> properties, bool isOverWrite)
        {
            foreach (AvePropertyInfo info in properties)
            {
                aveProfile.RestoreUserProfileProperty(info, isOverWrite);
            }

        }

        /// <summary>
        /// 还原user profile，包括Colleagues，Properties，Memberships，Comments，Tags，Links；
        /// </summary>
        /// <param name="userProfile"></param>
        public void SetObjectData(AveUserProfileInfo userProfile)
        {
            aveProfile.ExistSkip = this.ExistSkip;
            aveProfile.Restore(userProfile);
        }

        public void SetObjectDataForArchiver(AveUserProfileInfo userProfile)
        {
            aveProfile.ExistSkip = this.ExistSkip;
            aveProfile.RestoreForArchiver(userProfile);
        }

        public object SetObjectData(object userProfile)
        {
            throw new NotImplementedException();
        }

        public object GetObjectData()
        {
            throw new NotImplementedException();
        }

        public void SetObjectData(List<AveSOcialRatingInfo> ratingInfo)
        {
            aveProfile.SocialRating.Restore(ratingInfo);
        }

        public void Dispose()
        {
            if (aveProfile != null)
                aveProfile.Dispose();
        }


        public void SetObjectData(List<AveUserProfileSubTypeInfo> subTypes)
        {
            aveProfile.RestoreUserProfileSubTypes(subTypes);
        }

    }
}
