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
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.ObjectModel.Server16
{
    class AveWebService : AveService, IAveWebService
    {
        private SPWebService mWebService;
        private AveWebService mAdministrationService;
        private AveApplicationPoolCollection mApplicationPools;
        private AveWebService mContentService;
        private AveAntivirusSettings mAntivirusSettings;
        private AveWebApplicationCollection mWebAppCol;
        private AveFeatureCollection mFeatures;
        private AveQuotaTemplateCollection mQuotaTemplates;
        private AveIrmSettings mIrmSettings;
        private AveDatabaseServiceInstance mDatabaseServiceInstance;
        private AveHtmlTransformSettings mAveHtmlTransformSettings;
        private AvePersistedCustomWebTemplateCollection mGalleryCustomTemplates;

        public AveWebService()
            : this(new SPWebService())
        { }

        public AveWebService(SPService webService)
            : base(webService)
        {
            mWebService = (SPWebService)webService;
        }

        public AveWebService(string name, IAveFarm farm)
            : this(new SPWebService(name, (farm as AveFarm).Farm))
        { }

        internal SPWebService WebService
        {
            get
            {
                return mWebService;
            }
        }

        public void ApplyWebConfigModifications()
        {
            mWebService.ApplyWebConfigModifications();
        }

        public void Update()
        {
            mWebService.Update();
        }

        #region IAveWebService Members

        public IAveWebService AdministrationService
        {
            get
            {
                if (mAdministrationService == null)
                {
                    SPWebService webService = SPWebService.AdministrationService;
                    if (webService != null)
                    {
                        mAdministrationService = new AveWebService(webService);
                    }
                }
                return mAdministrationService;
            }
        }

        public IAveWebService ContentService
        {
            get
            {
                if (mContentService == null)
                {
                    SPWebService webService = SPWebService.ContentService;
                    if (webService != null)
                    {
                        mContentService = new AveWebService(webService);
                    }
                }
                return mContentService;
            }
        }

        public IAveAntivirusSettings AntivirusSettings
        {
            get
            {
                if (mAntivirusSettings == null)
                {
                    SPAntivirusSettings antiVirusSettings = mWebService.AntivirusSettings;
                    if (antiVirusSettings != null)
                    {
                        mAntivirusSettings = new AveAntivirusSettings(antiVirusSettings);
                    }
                }
                return mAntivirusSettings;
            }
        }

        public IAveWebApplicationCollection WebApplications
        {
            get
            {
                if (mWebAppCol == null)
                {
                    mWebAppCol = new AveWebApplicationCollection(mWebService.WebApplications);
                }
                return mWebAppCol;
            }
        }

        public bool OnlineHelpEnabled
        {
            get
            {
                return mWebService.OnlineHelpEnabled;
            }
            set
            {
                mWebService.OnlineHelpEnabled = value;
            }
        }

        public IAveFeatureCollection Features
        {
            get
            {
                if (mFeatures == null)
                {
                    mFeatures = new AveFeatureCollection(mWebService.Features);
                }
                return mFeatures;
            }
        }

        public IAveQuotaTemplateCollection QuotaTemplates
        {
            get
            {
                if (mQuotaTemplates == null)
                {
                    mQuotaTemplates = new AveQuotaTemplateCollection(mWebService.QuotaTemplates);
                }
                return mQuotaTemplates;
            }
        }

        public IAveApplicationPoolCollection ApplicationPools
        {
            get
            {
                if (mApplicationPools == null)
                {
                    mApplicationPools = new AveApplicationPoolCollection(mWebService.ApplicationPools);
                }
                return mApplicationPools;
            }
        }

        public IAveIrmSettings IrmSettings
        {
            get
            {
                if (mIrmSettings == null)
                {
                    SPIrmSettings irmSettings = mWebService.IrmSettings;
                    if (irmSettings != null)
                    {
                        mIrmSettings = new AveIrmSettings(irmSettings);
                    }
                }
                return mIrmSettings;
            }
        }

        public IAveHtmlTransformSettings HtmlTransformSettings
        {
            get
            {
                if (mAveHtmlTransformSettings == null)
                {
                    SPHtmlTransformSettings htmlTransformSettings = mWebService.HtmlTransformSettings;
                    if (htmlTransformSettings != null)
                    {
                        mAveHtmlTransformSettings = new AveHtmlTransformSettings(htmlTransformSettings);
                    }
                }
                return mAveHtmlTransformSettings;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords")]
        public IAveDatabaseServiceInstance DefaultDatabaseInstance
        {
            get
            {
                if (mDatabaseServiceInstance == null)
                {
                    SPDatabaseServiceInstance databaseServiceInstance = mWebService.DefaultDatabaseInstance;
                    if (databaseServiceInstance != null)
                    {
                        mDatabaseServiceInstance = new AveDatabaseServiceInstance(databaseServiceInstance);
                    }
                }
                return mDatabaseServiceInstance;
            }
            set
            {
                mDatabaseServiceInstance = value as AveDatabaseServiceInstance;
                if (mDatabaseServiceInstance != null)
                {
                    mWebService.DefaultDatabaseInstance = mDatabaseServiceInstance.DatabaseServiceInstance;
                }
                else
                {
                    mWebService.DefaultDatabaseInstance = null;
                }
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords")]
        public string DefaultDatabasePassword
        {
            get
            {
                return mWebService.DefaultDatabasePassword;
            }
            set
            {
                mWebService.DefaultDatabasePassword = value;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords")]
        public string DefaultDatabaseUsername
        {
            get
            {
                return mWebService.DefaultDatabaseUsername;
            }
            set
            {
                mWebService.DefaultDatabaseUsername = value;
            }
        }

        public IAvePersistedCustomWebTemplateCollection GalleryCustomTemplates
        {
            get
            {
                if (mGalleryCustomTemplates == null)
                {
                    mGalleryCustomTemplates = new AvePersistedCustomWebTemplateCollection((SPPersistedCustomWebTemplateCollection)AveAssemblyUtility.GetPropertyValue(mWebService, "GalleryCustomTemplates"));
                }
                return mGalleryCustomTemplates;
            }
        }

        public IAvePrefixCollection HostHeaderPrefixes
        {
            get
            {
                //API中有非空判断，不需要在封装层添加非空判断
                return new AvePrefixCollection(mWebService.HostHeaderPrefixes);
            }
        }

        #endregion
    }
}
