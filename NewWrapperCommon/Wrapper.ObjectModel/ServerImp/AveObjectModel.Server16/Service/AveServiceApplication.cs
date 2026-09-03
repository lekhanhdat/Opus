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



using Microsoft.SharePoint.Administration;
using AvePoint.Wrapper.Common;
using System;

namespace AvePoint.ObjectModel.Server16
{
    class AveServiceApplication : AvePersistedUpgradableObject, IAveServiceApplication
    {
        protected SPServiceApplication mServiceApplication;
        private AveServiceApplicationProxyGroup mServiceApplicationProxyGroup;

        public AveServiceApplication()
        { }

        public AveServiceApplication(SPServiceApplication serviceApplication)
            : base(serviceApplication)
        {
            mServiceApplication = serviceApplication;
        }

        public bool IsConnected(IAveServiceApplicationProxy proxy)
        {
            return mServiceApplication.IsConnected((proxy as AveServiceApplicationProxy).ServiceApplicationProxy);
        }

        public Guid ApplicationClassId
        {
            get { return this.mServiceApplication.ApplicationClassId; }
        }

        internal SPServiceApplication ServiceApplication
        {
            get { return mServiceApplication; }
        }

        public IAveServiceApplicationProxyGroup ServiceApplicationProxyGroup
        {
            get
            {
                if (mServiceApplicationProxyGroup == null)
                {
                    SPServiceApplicationProxyGroup serviceApplicationProxyGroup = mServiceApplication.ServiceApplicationProxyGroup;
                    if (serviceApplicationProxyGroup != null)
                    {
                        mServiceApplicationProxyGroup = new AveServiceApplicationProxyGroup(mServiceApplication.ServiceApplicationProxyGroup);
                    }
                }
                return mServiceApplicationProxyGroup;
            }
            set
            {
                if (value != null)
                {
                    mServiceApplicationProxyGroup = value as AveServiceApplicationProxyGroup;
                    mServiceApplication.ServiceApplicationProxyGroup = mServiceApplicationProxyGroup.ServiceApplicationProxyGroup;
                }
                else
                {
                    mServiceApplicationProxyGroup = null;
                    mServiceApplication.ServiceApplicationProxyGroup = null;
                }
            }
        }

        public bool CheckServiceApplicationPermission(object[] parameters)
        {
            IAveServiceApplicationPermissionChecker permissionChecker = AveServicePermissionCheckerFactory.CreatePermissionChecker(mServiceApplication);
            if (permissionChecker != null)
            {
                return permissionChecker.CheckPermission(this, parameters);
            }
            return true;
        }
    }

    class AveUserProfilePermissionChecker : IAveServiceApplicationPermissionChecker 
    {
        enum UserProfileApplicationUserRights : ulong
        {
            All = 15L,
            CreatePersonalSite = 2L,
            None = 0L,
            UseMicrobloggingAndFollowing = 8L,
            UsePersonalFeatures = 1L,
            UseSocialFeatures = 4L
        }

        public bool CheckPermission(IAveServiceApplication serviceApplication, object[] parameters)
        {
            bool result = false;
            SPServiceApplication spServiceApplication = (serviceApplication as AveServiceApplication).ServiceApplication;
            if (parameters.Length == 1 && spServiceApplication != null)
            {
                Guid partitionId = (Guid)parameters[0];
                object userAclObject = AveAssemblyUtility.InvokeMethod(spServiceApplication, "GetUserAcl", new object[] { partitionId });
                bool hasCreatePersonalSitePermission = (bool)AveAssemblyUtility.InvokeGenericMethod(userAclObject, "DoesUserHavePermissions", new object[] { UserProfileApplicationUserRights.CreatePersonalSite | UserProfileApplicationUserRights.UseMicrobloggingAndFollowing }, new Type[0]);
                bool hasFollowPeopleandEditProfilePermission = (bool)AveAssemblyUtility.InvokeGenericMethod(userAclObject, "DoesUserHavePermissions", new object[] { UserProfileApplicationUserRights.UsePersonalFeatures }, new Type[0]);
                bool hasUserTagsAndNotesPermission = (bool)AveAssemblyUtility.InvokeGenericMethod(userAclObject, "DoesUserHavePermissions", new object[] { UserProfileApplicationUserRights.UseSocialFeatures }, new Type[0]);
                if (hasCreatePersonalSitePermission && hasFollowPeopleandEditProfilePermission && hasUserTagsAndNotesPermission)
                {
                    result = true;
                }
            }
            return result;
        }
    }

    class AveServicePermissionCheckerFactory
    {
        public static IAveServiceApplicationPermissionChecker CreatePermissionChecker(SPServiceApplication serviceApplication)
        {
            Type type = serviceApplication.GetType();
            if (type.Name.Equals("UserProfileApplication", StringComparison.OrdinalIgnoreCase))
            {
                return new AveUserProfilePermissionChecker();
            }
            return null;
        }
    }
}
