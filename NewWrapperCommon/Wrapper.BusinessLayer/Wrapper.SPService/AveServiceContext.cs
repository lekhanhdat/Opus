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
using System.Collections;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.GCommon;
using System.Reflection;
using System.Globalization;

namespace AvePoint.Wrapper.SPService
{
    public delegate string AveUserMap(string loginName);

    public class AveServiceContext
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private AveObjectModelFactory mOMFactory;
        private IAveServiceContext mServiceContext;
        private IAveOUserProfileManager mUserProfileManager;
        private IAveOSocialTagManager mSocialTagManager;
        private IAveTaxonomySession mTaxonomySession;
        private IAveSite mSite;
        private int mDefaultLanguage = 0;
        private IAveTermStore mTermStore;
        private IAveTermSet mTermSet;
        private IAveOSocialCommentManager mCommentManager;
        private string mLoginName;
        private IAveOUserProfile mUserProfile;
        private Dictionary<string, ArrayList> mUserProfileCache = new Dictionary<string, ArrayList>();
        private AveUserMap mUserMap;
        private IAveOUserProfileSubTypeManager mSubTypeManager;
        private IAveOUserProfilePropertyManager mPropertyManager;

        public IAveOUserProfileSubTypeManager SubTypeManager
        {
            get {
                if (mSubTypeManager == null)
                {
                    mSubTypeManager = mOMFactory.CreateProfileSubTypeManager(this.ServiceContext);
                }
                return mSubTypeManager;
            }
        }
        public IAveOUserProfilePropertyManager PropertyManager
        {
            get
            {
                if (mPropertyManager == null)
                {
                    mPropertyManager = mOMFactory.CreateUserProfilePropertyManager(this.ServiceContext);
                }
                return mPropertyManager;
            }
            
        }
        public AveUserMap UserMap
        {
            set
            {
                mUserMap = value;
            }
            internal get
            {
                return mUserMap;
            }
        }

        public Dictionary<string, ArrayList> UserProfileCache
        {
            get { return mUserProfileCache; }
        }

        public IAveOUserProfile UserProfile
        {
            get { return mUserProfile; }
            set { mUserProfile = value; }
        }

        public string LoginName
        {
            get { return mLoginName; }
            set { mLoginName = value; }
        }

        public AveObjectModelFactory OMFactory
        {
            get { return mOMFactory; }
        }
        public IAveServiceContext ServiceContext
        {
            get
            {
                if (mServiceContext == null)
                {
                    if (mSite.WebApplication != null)
                    {
                        mServiceContext = mOMFactory.CreateServiceContext().GetContext(mSite.WebApplication.ServiceApplicationProxyGroup, mOMFactory.CreateSiteSubscriptionIdentifier().Default);
                    }
                    else
                    {
                        mServiceContext = mOMFactory.CreateServiceContext().GetContext(mSite);
                    }
                }
                return mServiceContext;
            }
        }
        public IAveOUserProfileManager UserProfileManager
        {
            get
            {
                if (mUserProfileManager == null)
                {
                    mUserProfileManager = mOMFactory.CreateUserProfileManager(this.ServiceContext);
                }
                return mUserProfileManager;
            }
        }
        public IAveOSocialTagManager SocialTagManager
        {
            get
            {
                if (mSocialTagManager == null)
                {
                    mSocialTagManager = mOMFactory.CreateSocialTagManager(this.ServiceContext);
                }
                return mSocialTagManager;
            }
        }
        public IAveTaxonomySession TaxonomySession
        {
            get
            {
                if (mTaxonomySession == null)
                {
                    mTaxonomySession = mOMFactory.CreateTaxonomySession(mSite);
                }
                return mTaxonomySession;
            }
        }
        public IAveSite Site
        {
            get { return mSite; }
            set
            {
                mSite = value;
                mServiceContext = null;
                mUserProfileManager = null;
                mSocialTagManager = null;
                mCommentManager = null;
                mTaxonomySession = null;
                mDefaultLanguage = 0;
                mTermStore = null;
                mTermSet = null;
            }
        }
        public int DefaultLanguage
        {
            get
            {
                if (mDefaultLanguage == 0)
                {
                    mDefaultLanguage = this.TermStore.DefaultLanguage;
                }
                return mDefaultLanguage;
            }
        }
        public IAveTermStore TermStore
        {
            get
            {
                if (mTermStore == null)
                {
                    if (this.TaxonomySession.DefaultKeywordsTermStore != null)
                    {
                        mTermStore = this.TaxonomySession.DefaultKeywordsTermStore;
                    }
                    else
                    {
                        mTermStore = this.TaxonomySession.TermStores[0];
                    }
                }
                return mTermStore;
            }
        }
        public IAveTermSet TermSet
        {
            get
            {
                //ADO-113358当存在多级别的Tag需要还原时 mTermSet的相关属性会产生变动 需要及时更新。
                //if (mTermSet == null)
                //{
                    mTermSet = this.TermStore.KeywordsTermSet;
                //}
                return mTermSet;
            }
        }
        public IAveOSocialCommentManager CommentManager
        {
            get
            {
                if (mCommentManager == null)
                {
                    mCommentManager = mOMFactory.CreateSocialCommentManager(this.ServiceContext);
                }
                return mCommentManager;
            }
        }

        public AveServiceContext(IAveSite site, AveObjectModelFactory fac)
        {
            mOMFactory = fac;
            mSite = site;
        }

        public void AddUserProfileCache(IAveOUserProfile userProfile)
        {
            string login = userProfile.MultiloginAccounts[0].ToLower(CultureInfo.CurrentCulture);
            if (!mUserProfileCache.ContainsKey(login))
            {
                mUserProfileCache[login] = new ArrayList();
                mUserProfileCache[login].Add(userProfile.RecordId);
                mUserProfileCache[login].Add(userProfile.ID);
            }
        }

        public void GetUserProfileCache(string login, out long recordId, out Guid userId)
        {
            if (!mUserProfileCache.ContainsKey(login))
            {
                IAveOUserProfile profile = null;
                if (this.UserProfileManager.UserExists(login))
                {
                    profile = this.UserProfileManager.GetUserProfile(login);
                }
                else
                {
                    profile = this.UserProfileManager.CreateUserProfile(login);
                }
                recordId = profile.RecordId;
                userId = profile.ID;
                AddUserProfileCache(profile);
                return;
            }
            ArrayList data = mUserProfileCache[login];
            recordId = (long)data[0];
            userId = (Guid)data[1];
        }

        internal string GetMappingUser(string login)
        {
            if (UserMap != null)
            {
                return UserMap(login);
            }
            return login;
        }
    }
}
