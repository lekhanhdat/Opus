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
using Microsoft.SharePoint.Administration;
using AvePoint.Wrapper.Common;
using AvePoint.Common;
using System.Security;
using Microsoft.SharePoint;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveFarm : AvePersistedUpgradableObject, IAveFarm
    {
        private readonly string validateLogonAccount_Type = "ValidateLogonAccount";
        private SPFarm mFarm;
        private AveFarm mLocalFarm;
        private AveTimerService mTimerService;
        private AveServiceCollection mServiceCol;
        private AveSolutionCollection mSolutionCol;
        private AveServiceProxyCollection mServiceProxies;
        private AveAlternateUrlCollectionManager mAlternateUrlCollections;
        private AveFeatureDefinitionCollection mFeatureDefinitions;
        private AveServerCollection mServerCol;

        internal SPFarm Farm
        {
            get { return mFarm; }
            set { mFarm = value; }
        }

        public AveFarm(SPFarm farm)
            : base(farm)
        {
            mFarm = farm;
        }

        public AveFarm()
            : base(SPServer.Local == null ? SPFarm.Local : SPServer.Local.Farm)
        {
        }

        #region IAveFarm Members

        public IAveFarm Local
        {
            get
            {
                if (mLocalFarm == null)
                {
                    SPFarm farm = SPFarm.Local;
                    if (farm != null)
                    {
                        mLocalFarm = new AveFarm(farm);
                    }
                }
                return mLocalFarm;
            }
        }

        public IAveTimerService TimerService
        {
            get
            {
                if (mTimerService == null)
                {
                    SPTimerService timerService = mFarm.TimerService;
                    if (timerService != null)
                    {
                        mTimerService = new AveTimerService(timerService);
                    }
                }
                return mTimerService;
            }
        }

        public IAveServiceCollection Services
        {
            get
            {
                if (mServiceCol == null)
                {
                    mServiceCol = new AveServiceCollection(mFarm.Services);
                }
                return mServiceCol;
            }
        }

        public IAveSolutionCollection Solutions
        {
            get
            {
                if (mSolutionCol == null)
                {
                    mSolutionCol = new AveSolutionCollection(mFarm.Solutions);
                }
                return mSolutionCol;
            }
        }

        public IAveAlternateUrlCollectionManager AlternateUrlCollections
        {
            get
            {
                if (mAlternateUrlCollections == null)
                {
                    mAlternateUrlCollections = new AveAlternateUrlCollectionManager(mFarm.AlternateUrlCollections);
                }
                return mAlternateUrlCollections;
            }
        }

        public IAveFeatureDefinitionCollection FeatureDefinitions
        {
            get
            {
                if (mFeatureDefinitions == null)
                {
                    mFeatureDefinitions = new AveFeatureDefinitionCollection(mFarm.FeatureDefinitions);
                }
                return mFeatureDefinitions;
            }
        }

        public IAveServerCollection Servers
        {
            get
            {
                if (mServerCol == null)
                {
                    mServerCol = new AveServerCollection(mFarm.Servers);
                }
                return mServerCol;
            }
        }

        public void ValidateLogonAccount(ref string username, SecureString password)
        {
            object[] paramObjects = new object[] { username, password };
            AveAssemblyUtility.InvokeStaticMethod(typeof(SPFarm), validateLogonAccount_Type, new Type[] { typeof(string).MakeByRefType(), typeof(SecureString) }, new object[] { username, password });
            username = (string)paramObjects[0];
        }

        public string PasswordChangeEmailAddress
        {
            get
            {
                return mFarm.PasswordChangeEmailAddress;
            }
            set
            {
                mFarm.PasswordChangeEmailAddress = value;
            }
        }

        public int PasswordChangeGuardTime
        {
            get
            {
                return mFarm.PasswordChangeGuardTime;
            }
            set
            {
                mFarm.PasswordChangeGuardTime = value;
            }
        }

        public int PasswordChangeMaximumTries
        {
            get
            {
                return mFarm.PasswordChangeMaximumTries;
            }
            set
            {
                mFarm.PasswordChangeMaximumTries = value;
            }
        }

        public int DaysBeforePasswordExpirationToSendEmail
        {
            get
            {
                return mFarm.DaysBeforePasswordExpirationToSendEmail;
            }
            set
            {
                mFarm.DaysBeforePasswordExpirationToSendEmail = value;
            }
        }

        public IAvePersistedObject GetObject(Guid id)
        {
            SPPersistedObject retObj = mFarm.GetObject(id);
            if (retObj == null)
            {
                return null;
            }
            return (AvePersistedObject)AveServerAssemblyInit.CreateElement(typeof(IAvePersistedObject), retObj);
        }

        public IAvePersistedObject GetObject(string name, Guid parentId, Type type)
        {
            SPPersistedObject retObj = mFarm.GetObject(name, parentId, type);
            if (retObj == null)
            {
                return null;
            }
            return (AvePersistedObject)AveServerAssemblyInit.CreateElement(typeof(IAvePersistedObject), retObj);
        }

        public void Unjoin()
        {
            mFarm.Unjoin();
        }

        public bool CEIPEnabled
        {
            get
            {
                return mFarm.CEIPEnabled;
            }
            set
            {
                mFarm.CEIPEnabled = value;
            }
        }

        public Version BuildVersion
        {
            get
            {
                return mFarm.BuildVersion;
            }
        }

        public IAveServiceProxyCollection ServiceProxies
        {
            get
            {
                if (mServiceProxies == null)
                {
                    mServiceProxies = new AveServiceProxyCollection(mFarm.ServiceProxies);
                }
                return mServiceProxies;
            }
        }

        public Guid ExternalBinaryStoreClassId
        {
            get
            {
                //add by adrian
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IAveWebTemplateCollection GetWebTemplates(uint LCID)
        {
            int platformVersion = 15;

            AveWebTemplateCollection webTemplateCollection = null;

            object spRequestObj = AveAssemblyUtility.GetStaticPropertyValue(typeof(SPFarm), "RequestNoAuth");

            string webTemplateString = (string)AveAssemblyUtility.InvokeMethod(spRequestObj, "GetWebTemplates", new object[] { LCID, platformVersion });

            spRequestObj.GetType().GetMethod("Dispose", new Type[] { }).Invoke(spRequestObj, new object[] { });

            SPWebTemplateCollection templates = (SPWebTemplateCollection)AveAssemblyUtility.CreateInstance(typeof(SPWebTemplateCollection), new Type[] { typeof(string), typeof(uint) }, new object[] { webTemplateString, LCID });

            SPWebService contentService = SPWebService.ContentService;


            foreach (SPPersistedCustomWebTemplate cwt in (SPPersistedCustomWebTemplateCollection)AveAssemblyUtility.GetPropertyValue(contentService, "GalleryCustomTemplates"))
            {
                if (cwt.LocaleId == LCID)
                {
                    var template=AveAssemblyUtility.CreateInstance(typeof (SPCustomWebTemplate), new Type[] {typeof (SPPersistedCustomWebTemplate)}, new object[] {cwt});
                    AveAssemblyUtility.InvokeMethod(templates, "Add", template);                    
                }
            }
            if (templates != null)
            {
                webTemplateCollection = new AveWebTemplateCollection(templates);
            }

            return webTemplateCollection;
        }

        public bool CurrentUserIsAdministrator()
        {
            return mFarm.CurrentUserIsAdministrator();
        }

        public IAveProcessAccount DefaultServiceAccount
        {
            get
            {
                if (mFarm.DefaultServiceAccount != null)
                {
                    return new AveProcessAccount(mFarm.DefaultServiceAccount);
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion
    }
}
