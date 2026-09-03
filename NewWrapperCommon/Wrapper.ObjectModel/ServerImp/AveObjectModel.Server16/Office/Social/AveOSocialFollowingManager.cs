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



using AvePoint.GCommon.Utility.Exceptions.SharePoint;
using AvePoint.Wrapper.Common;
using Microsoft.Office.Server.UserProfiles;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.GCommon.Utility;
using System;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Office.Server.Social;
using Microsoft.SharePoint;
using System.Linq;

namespace AvePoint.ObjectModel.Server16.Office
{
    class AveOSocialFollowingManager : IAveOSocialFollowingManager
    {
        private SPSocialFollowingManager mSPSocialFollowingManager;

        public AveOSocialFollowingManager(SPSocialFollowingManager spSocialFollowingManager)
        {
            mSPSocialFollowingManager = spSocialFollowingManager;
        }

        public AveOSocialFollowingManager(IAveOUserProfile profile, IAveServiceContext context)
        {
            UserProfile tmpProfile = ((profile as AveOUserProfile) == null) ? null : (profile as AveOUserProfile).UserProfile;
            SPServiceContext tmpServiceContext = ((context as AveServiceContext) == null) ? null : (context as AveServiceContext).ServiceContext;
            mSPSocialFollowingManager = new SPSocialFollowingManager(tmpProfile, tmpServiceContext);
        }

        public IAveOSocialActor[] GetFollowed(AveOSocialActorTypes types)
        {
            List<IAveOSocialActor> first = new List<IAveOSocialActor>{};
            SPSocialActor[] tmpFollowed = mSPSocialFollowingManager.GetFollowed((SPSocialActorTypes)types);
            foreach (SPSocialActor socialActor in tmpFollowed)
            {
                if (socialActor != null)
                {
                    first.Add(new AveOSocialActor(socialActor));
                }
                else 
                {
                    first.Add(null);
                }
            }
            return first.ToArray();
        }

        public AveOSocialFollowResult Follow(IAveOSocialActorInfo actor)
        {
            var actorInfo = actor as AveOSocialActorInfo;
            if (actorInfo != null)
            {
                return (AveOSocialFollowResult)mSPSocialFollowingManager.Follow(actorInfo.Actor);
            }
            else
            {
                return AveOSocialFollowResult.InternalError;
            }
        }
    }
}
