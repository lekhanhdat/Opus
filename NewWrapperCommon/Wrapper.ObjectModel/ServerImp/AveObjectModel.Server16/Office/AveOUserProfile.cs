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
using AvePoint.Wrapper.Common;
using Microsoft.Office.Server.UserProfiles;
using Microsoft.SharePoint;
using AvePoint.Wrapper.Common.Office;
using SPDisposeCheck;
using AvePoint.Common;
using Microsoft.SharePoint.Administration;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Security.Principal;
using AvePoint.GCommon.Contract.AgentService;
using AvePoint.GCommon.Utility.Cryptography;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AvePoint.GCommon;
using System.Threading;
using System.Linq;

namespace AvePoint.ObjectModel.Server16.Office
{
    class AveOUserProfile : AveOProfileBase, IAveOUserProfile
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private UserProfile mUserProfile;
        private AveOMembershipManager mMemberships;
        private AveOColleagueManager mColleagues;
        private AveOUserProfileManager mProfileManager;
        private Dictionary<string, AveOUserProfileValueCollection> mUserProfileFields;
        private AveOQuickLinkManager mQuickLinkManager;

        public AveOUserProfile(AveOUserProfileManager profileManager, UserProfile userProfile)
            : base(userProfile)
        {
            mUserProfile = userProfile;
            mProfileManager = profileManager;
            mUserProfileFields = new Dictionary<string, AveOUserProfileValueCollection>();
        }

        public AveOUserProfile(UserProfile userProfile)
            : base(userProfile)
        {
            mUserProfile = userProfile;
            mProfileManager = new AveOUserProfileManager(mMemberships, userProfile.ProfileManager);
            mUserProfileFields = new Dictionary<string, AveOUserProfileValueCollection>();
        }

        internal UserProfile UserProfile
        {
            get
            {
                return mUserProfile;
            }
        }

        #region IAveOUserProfile Members

        public IAveOUserProfileValueCollection this[string strPropName]
        {
            get
            {
                AveOUserProfileValueCollection values = null;
                if (!this.mUserProfileFields.TryGetValue(strPropName, out values))
                {
                    UserProfileValueCollection tempValue = mUserProfile[strPropName];
                    if (tempValue != null)
                    {
                        values = new AveOUserProfileValueCollection(this, tempValue);
                        this.mUserProfileFields.Add(strPropName, values);
                    }
                }
                return values;
            }
        }

        public IAveOMembershipManager Memberships
        {
            get
            {
                if (this.mMemberships == null)
                {
                    this.mMemberships = new AveOMembershipManager(mUserProfile.Memberships);
                }
                return this.mMemberships;
            }
        }

        public IAveOColleagueManager Colleagues
        {
            get
            {
                if (this.mColleagues == null)
                {
                    this.mColleagues = new AveOColleagueManager(mUserProfile.Colleagues);
                }
                return this.mColleagues;
            }
        }

        public IAveOUserProfileManager ProfileManager
        {
            get
            {
                return this.mProfileManager;
            }
        }

        public IAveOQuickLinkManager QuickLinks
        {
            get
            {
                if (mQuickLinkManager == null)
                {
                    mQuickLinkManager = new AveOQuickLinkManager(this, mUserProfile.QuickLinks);
                }
                return mQuickLinkManager;
            }
        }

        public string[] MultiloginAccounts
        {
            get
            {
                return mUserProfile.MultiloginAccounts;
            }
            set
            {
                throw new NotImplementedException();//server端不需要set，client端需要set方法；
            }
        }

        public IAveSite PersonalSite
        {
            [SPDisposeCheckIgnore(SPDisposeCheckID._400, "This site will be Disposed by AveSite")]
            get
            {
                SPSite site = mUserProfile.PersonalSite;//PersionalSite这个属性，每次取都会返回一个新的SPSite对象
                if (site != null)
                {
                    return new AveSite(site);
                }
                return null;
            }

        }


        [SuppressMessage("FxCopCustomRules", "C100013:DoNotMissExceptionHandlingInCatchBlocks")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "PersonalSpace", Justification = "PersonalSpace is a key")]
        public void CreatePersonalSite(int lcid)
        {
            try
            {
                mUserProfile.CreatePersonalSite(lcid);
            }
            catch (PersonalSiteCreateException)
            {
                bool failed = true;
                try
                {
                    mUserProfile.CreatePersonalSiteEnque(true);
                    //CreatePersonalSiteNative(mProfileManager.MySiteHostUrl, mUserProfile.AccountName);
                    int count = 0;
                    while (count++ < 60)
                    {
                        mUserProfile = mUserProfile.ProfileManager.GetUserProfile(mUserProfile.AccountName);
                        log.Info("Personal Space: {0}, {1} times.", mUserProfile["PersonalSpace"].Value, count);
                        if (mUserProfile.PersonalSite != null)
                        {
                            failed = false;
                            break;
                        }
                        else
                        {
                            Thread.Sleep(10000);
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Error("An error occurred while creating my site by native. Reason: {0}", e);
                }
                if (failed)
                {
                    log.Info("PersonalSiteInstantiationState: {0}", mUserProfile.PersonalSiteInstantiationState);
                    throw;
                }
            }
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        private bool CreatePersonalSiteNative(string mySiteHostUrl, string accountName)
        {
            using (SPSite site = new SPSite(mySiteHostUrl))
            {
                var poolUser = site.WebApplication.ApplicationPool.ManagedAccount;
                var script = string.Format(
@"Add-PsSnapin Microsoft.SharePoint.PowerShell;
$url = '{0}';
$user = '{1}';
$site=get-spsite $url;
$context = [Microsoft.Office.Server.ServerContext]::GetContext($site);
$upm =  New-Object Microsoft.Office.Server.UserProfiles.UserProfileManager($context);
$p = $upm.GetUserProfile($user);
$p.CreatePersonalSite();
exit
", mySiteHostUrl, accountName);
                var command = Convert.ToBase64String(Encoding.Unicode.GetBytes(script));
                var domain = poolUser.Username.Substring(0, poolUser.Username.LastIndexOf('\\'));
                var userName = poolUser.Username.Substring(poolUser.Username.LastIndexOf('\\') + 1);
                var arguments = "-encodedCommand " + command;
                var agentServices = WcfUtility.GetAgentProcessService<IAAgentService>();
                var pwd = (AveAssemblyUtility.GetFieldValue(poolUser, "m_Password") as SPEncryptedString).SecureStringValue;
                var pwdPtr = Marshal.SecureStringToBSTR(pwd);
                try
                {
                    var password = Convert.ToBase64String(CspCommunicationWrapper.WrapKey(CryptoUtil.ConvertStringToBytes(Marshal.PtrToStringBSTR(pwdPtr))));
                    return agentServices.StartAndWaitProcess(domain, userName, password, "powershell.exe", arguments);
                }
                finally
                {
                    Marshal.FreeBSTR(pwdPtr);
                }
            }
        }

        public void Commit()
        {
            mUserProfile.Commit();
        }

        #endregion

        public IAveOUserProfileChangeCollection GetChanges()
        {
            UserProfileChangeCollection userProfileChangeCollection = mUserProfile.GetChanges();
            AveOUserProfileChangeCollection aveOUserProfileChangeCollection = new AveOUserProfileChangeCollection(userProfileChangeCollection);
            foreach (var profile in userProfileChangeCollection)
            {
                if (profile != null)
                {
                    aveOUserProfileChangeCollection.Add(new AveOProfileBaseChange(profile));
                }
                else
                {
                    aveOUserProfileChangeCollection.Add(null);
                }
            }
            return aveOUserProfileChangeCollection;
        }



        public string AccountName
        {
            get { return mUserProfile.AccountName; }
        }


        public IAveOProfileValueCollectionBase GetProfileValueCollection(string propName)
        {
            return this[propName];
        }

        private const string mFilePrefixName = "img_BRT1023_";
        public string[] SaveTempFile(byte[] content, string fileName)
        {
            Type t = AveAssemblyUtility.GetType("Microsoft.Office.Server.UserProfiles.ProfileImageStore");
            var profileImageStore = AveAssemblyUtility.CreateInstance("Microsoft.Office.Server.UserProfiles.ProfileImageStore",
                new Type[1] { typeof(UserProfile) }, new object[1] { this.UserProfile });
            using (MemoryStream s = new MemoryStream(content))
            {
                var ret = AveAssemblyUtility.InvokeMethod(profileImageStore, t, "SaveUploadedFile", new object[] { 1, mFilePrefixName, true, fileName, (int)s.Length, s });
                return (ret as string[]);
            }
        }


        public IAveOProfileSubtype ProfileSubType
        {
            set { mUserProfile.ProfileSubtype = (value as AveOProfileSubtype).profileSubType; }
            get { return new AveOProfileSubtype(mUserProfile.ProfileSubtype); }
        }

        public IAveOFollowedContent FollowedContent
        {
            get
            {
                return new AveOFollowedContent(this.mUserProfile.FollowedContent);
            }
        }

        public IAveOUserProfile[] GetPeers()
        {
            var ups = this.mUserProfile.GetPeers();
            return ups == null ? null : ups.Select(up => up == null ? null : new AveOUserProfile(up)).ToArray();
        }

        public IAveOUserProfile[] GetDirectReports()
        {
            var ups = this.mUserProfile.GetDirectReports();
            return ups == null ? null : ups.Select(up => up == null ? null : new AveOUserProfile(up)).ToArray();
        }
    }
}
