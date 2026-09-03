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
using System.Collections;

namespace AvePoint.ObjectModel.Server19.Office
{
    class AveOUserProfileManager : IAveOUserProfileManager
    {
        private UserProfileManager mUserProfileManager;
        private AveOProfileSubtypePropertyManager mDefaultProfileSubtypeProperties;
        private AveOMemberGroupManager mMemberGroupManager;
        private AveOMembershipManager mMemvershipManager;
        private AveOPropertyCollection mProperties;
        private AveOPropertyCollection mPropertiesWithSection;

        public AveOUserProfileManager(AveOMembershipManager membershipManager, UserProfileManager userProfileManager)
        {
            mMemvershipManager = membershipManager;
            mUserProfileManager = userProfileManager;
        }

        public AveOUserProfileManager(UserProfileManager userProfileManager)
        {
            mUserProfileManager = userProfileManager;
        }

        public AveOUserProfileManager(IAveServiceContext serviceContext)
        {
            try
            {
                mUserProfileManager = new UserProfileManager((serviceContext as AveServiceContext).ServiceContext);
            }
            catch (NullReferenceException)
            {
                throw new UserProfileInaccessibleException();
            }
        }

        public AveOUserProfileManager(IAveServiceApplication serviceApplication, List<Guid> PartitionIDs)
        {
            AveServiceApplication app = serviceApplication as AveServiceApplication;

            Guid DefaultPartitionId = new Guid("0C37852B-34D0-418e-91C6-2AC25AF4BE5B");

            Assembly assembly = typeof(UserProfileManager).Assembly;

            //Get UserProfileApplicationProxy Instance by UserProfileApplication
            object proxy_Obj = AveAssemblyUtility.InvokeStaticMethod(assembly.GetType("Microsoft.Office.Server.Administration.UserProfileApplicationProxy"),
                "GetProxy", new Type[] { assembly.GetType("Microsoft.Office.Server.Administration.UserProfileApplication") }, app.ServiceApplication);

            if (PartitionIDs != null && PartitionIDs.Count > 0)
            {
                foreach (Guid pid in PartitionIDs)
                {
                    mUserProfileManager = (UserProfileManager)AveAssemblyUtility.CreateInstance(assembly, "Microsoft.Office.Server.UserProfiles.UserProfileManager",
                    new Type[] { assembly.GetType("Microsoft.Office.Server.Administration.UserProfileApplicationProxy"), typeof(Guid) },
                    new object[] { proxy_Obj, pid });
                    try
                    {
                        bool test = mUserProfileManager.IsPersonalSiteMultipleLanguage;
                        break;
                    }
                    catch (InvalidOperationException)
                    {
                        //try to get the user profile manager again
                    }
                }
            }
            else
            {
                mUserProfileManager = (UserProfileManager)AveAssemblyUtility.CreateInstance(assembly, "Microsoft.Office.Server.UserProfiles.UserProfileManager",
                    new Type[] { assembly.GetType("Microsoft.Office.Server.Administration.UserProfileApplicationProxy"), typeof(Guid) },
                    new object[] { proxy_Obj, DefaultPartitionId });
            }
        }

        #region IAveUserProfileManager Members

        public IAveOUserProfile GetUserProfile(string strAccountName)
        {
            return new AveOUserProfile(this, mUserProfileManager.GetUserProfile(strAccountName));
        }

        public IAveOUserProfile GetUserProfile(byte[] rgbySID)
        {
            return new AveOUserProfile(this, mUserProfileManager.GetUserProfile(rgbySID));
        }

        public IAveOProfileSubtypePropertyManager DefaultProfileSubtypeProperties
        {
            get
            {
                if (mDefaultProfileSubtypeProperties == null)
                {
                    ProfileSubtypePropertyManager profileSubtypePropertyManager = mUserProfileManager.DefaultProfileSubtypeProperties;
                    if (profileSubtypePropertyManager != null)
                    {
                        mDefaultProfileSubtypeProperties = new AveOProfileSubtypePropertyManager(profileSubtypePropertyManager);
                    }
                }
                return mDefaultProfileSubtypeProperties;
            }
        }

        public bool UserExists(string strAccountName)
        {
            return mUserProfileManager.UserExists(strAccountName);
        }

        public string MySiteHostUrl
        {
            get
            {
                return mUserProfileManager.MySiteHostUrl;
            }
            set
            {
                mUserProfileManager.MySiteHostUrl = value;
            }
        }

        public IAveOUserProfile CreateUserProfile(string strAccountName)
        {
            UserProfile tempUserProfile = mUserProfileManager.CreateUserProfile(strAccountName);
            return new AveOUserProfile(this, tempUserProfile);
        }

        public IAveOPropertyCollection Properties
        {
            get
            {
                if (mProperties == null)
                {
                    PropertyCollection tempColl = mUserProfileManager.Properties;
                    if (tempColl != null)
                    {
                        mProperties = new AveOPropertyCollection(tempColl);
                    }
                }
                return mProperties;
            }
        }

        public IAveOPropertyCollection PropertiesWithSection
        {
            get
            {
                if (mPropertiesWithSection == null)
                {
                    PropertyCollection tempColl = mUserProfileManager.PropertiesWithSection;
                    if (tempColl != null)
                    {
                        mPropertiesWithSection = new AveOPropertyCollection(tempColl);
                    }
                }
                return mPropertiesWithSection;
            }
        }

        public IAveOMemberGroupManager GetMemberGroups()
        {
            if (mMemberGroupManager == null)
            {
                mMemberGroupManager = new AveOMemberGroupManager(mMemvershipManager, this, mUserProfileManager.GetMemberGroups());
            }
            return mMemberGroupManager;
        }

        public void RemoveUserProfile(string strAccountName)
        {
            mUserProfileManager.RemoveUserProfile(strAccountName);
        }

        public void RemoveUserProfile(Guid guidDelete)
        {
            mUserProfileManager.RemoveUserProfile(guidDelete);
        }

        public bool CheckServiceApplicationPermission(IAveServiceApplication serviceApp)
        {
            Guid partitionId = (Guid)AveAssemblyUtility.GetPropertyValue(mUserProfileManager, "PartitionID");
            return serviceApp.CheckServiceApplicationPermission(new object[] { partitionId });
        }

        public IAveOUserProfileChangeCollection GetChanges()
        {
            UserProfileChangeCollection userProfileChangeCollection = mUserProfileManager.GetChanges();
            AveOUserProfileChangeCollection aveOUserProfileChangeCollection = new AveOUserProfileChangeCollection(userProfileChangeCollection);
            foreach (var profile in userProfileChangeCollection)
            {
                if (profile != null)
                {
                    aveOUserProfileChangeCollection.Add(new AveOUserProfileChange(profile as UserProfileChange));
                }
                else
                {
                    aveOUserProfileChangeCollection.Add(null);
                }
            }
            return aveOUserProfileChangeCollection;
        }

        public IAveOUserProfileChangeCollection GetChanges(IAveOProfileBaseChangeQuery changeQuery)
        {
            UserProfileChangeCollection userProfileChangeCollection = mUserProfileManager.GetChanges((changeQuery as AveOProfileBaseChangeQuery).ProfileBaseChangeQuery);
            AveOUserProfileChangeCollection aveOUserProfileChangeCollection = new AveOUserProfileChangeCollection(userProfileChangeCollection);
            foreach (var profile in userProfileChangeCollection)
            {
                if (profile != null)
                {
                    aveOUserProfileChangeCollection.Add(new AveOUserProfileChange(profile as UserProfileChange));
                }
                else
                {
                    aveOUserProfileChangeCollection.Add(null);
                }
            }
            return aveOUserProfileChangeCollection;
        }

        public IAveOUserProfileChangeCollection GetChanges(IAveOUserProfileChangeToken changeToken)
        {
            UserProfileChangeCollection userProfileChangeCollection = mUserProfileManager.GetChanges((changeToken as AveOUserProfileChangeToken).UserProfileChangeToken);
            AveOUserProfileChangeCollection aveOUserProfileChangeCollection = new AveOUserProfileChangeCollection(userProfileChangeCollection);
            foreach (var profile in userProfileChangeCollection)
            {
                if (profile != null)
                {
                    aveOUserProfileChangeCollection.Add(new AveOUserProfileChange(profile as UserProfileChange));
                }
                else
                {
                    aveOUserProfileChangeCollection.Add(null);
                }
            }
            return aveOUserProfileChangeCollection;
        }

        public virtual IEnumerator GetEnumerator()
        {
            //List<IAveOUserProfile> profiles = new List<IAveOUserProfile> { };
            //foreach (UserProfile profile in mUserProfileManager)
            //{
            //    if (profile != null)
            //    {
            //        profiles.Add(new AveOUserProfile(profile));
            //    }
            //    else
            //    {
            //        profiles.Add(null);
            //    }
            //}
            //return profiles.GetEnumerator();
            return new AveOUserProfileCollection(mUserProfileManager);
        }

        public long Count
        {
            get { return mUserProfileManager.Count; }
        }


        public bool IsPersonalSiteMultipleLanguage
        {
            get
            {
                return mUserProfileManager.IsPersonalSiteMultipleLanguage;
            }
            set
            {
                mUserProfileManager.IsPersonalSiteMultipleLanguage = value;
            }
        }

        #endregion

        public void SetAccountForCreatePersonalSite(string account)
        {
            AveAssemblyUtility.SetFieldValue(mUserProfileManager, "m_strCurrentAccountName", account);
        }
    }

    internal class AveOUserProfileCollection : IEnumerator
    {
        private AveOUserProfileManager aveUserProfileManager;
        private IEnumerator enumerator;

        public AveOUserProfileCollection(UserProfileManager userProfileManager)
        {
            this.enumerator = userProfileManager.GetEnumerator();
            this.aveUserProfileManager = new AveOUserProfileManager(userProfileManager);
        }

        public object Current
        {
            get 
            { 
                var obj = enumerator.Current;
                if (obj != null)
                {
                    return new AveOUserProfile(aveUserProfileManager, obj as UserProfile);
                }
                return obj;
            }
        }

        public bool MoveNext()
        {
            return enumerator.MoveNext();
        }

        public void Reset()
        {
            enumerator.Reset();
        }
    }
}
