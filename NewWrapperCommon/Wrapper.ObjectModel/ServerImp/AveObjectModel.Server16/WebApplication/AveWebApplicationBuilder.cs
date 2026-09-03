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
using System.IO;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Search.Administration;
using AvePoint.Wrapper.Common;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.ObjectModel.Server16
{
    class AveWebApplicationBuilder : IAveWebApplicationBuilder, IDisposable
    {
        private SPWebApplicationBuilder mWebApplicationBuilder;
        private AveSearchServiceInstance mSearchServiceInstance;
        private AveWebService mWebService;

        public AveWebApplicationBuilder(IAveFarm farm)
        {
            mWebApplicationBuilder = new SPWebApplicationBuilder((farm as AveFarm).Farm);
        }

        public string ApplicationPoolId
        {
            get
            {
                return mWebApplicationBuilder.ApplicationPoolId;
            }
            set
            {
                mWebApplicationBuilder.ApplicationPoolId = value;
            }
        }

        public AveIdentityType IdentityType
        {
            get
            {
                return (AveIdentityType)mWebApplicationBuilder.IdentityType;
            }
            set
            {
                mWebApplicationBuilder.IdentityType = (IdentityType)value;
            }
        }

        public IAveManagedAccount ManagedAccount
        {
            get
            {
                SPManagedAccount managedAccount = mWebApplicationBuilder.ManagedAccount;
                if (managedAccount == null)
                {
                    return null;
                }
                return new AveManagedAccount(managedAccount);
            }
            set
            {
                if (value != null)
                {
                    mWebApplicationBuilder.ManagedAccount = (value as AveManagedAccount).ManagedAccount;
                }
                else
                {
                    mWebApplicationBuilder.ManagedAccount = null;
                }
            }
        }

        public IAveSearchServiceInstance SearchServiceInstance
        {
            get
            {
                if (mSearchServiceInstance == null)
                {
                    SPSearchServiceInstance searchServiceInstance = mWebApplicationBuilder.SearchServiceInstance;
                    if (searchServiceInstance != null)
                    {
                        mSearchServiceInstance = new AveSearchServiceInstance(searchServiceInstance);
                    }
                }
                return mSearchServiceInstance;
            }
            set
            {
                mSearchServiceInstance = value as AveSearchServiceInstance;
                if (mSearchServiceInstance != null)
                {
                    mWebApplicationBuilder.SearchServiceInstance = mSearchServiceInstance.SearchServiceInstance;
                }
                else
                {
                    mWebApplicationBuilder.SearchServiceInstance = null;
                }
            }
        }

        public bool AllowAnonymousAccess
        {
            get
            {
                return mWebApplicationBuilder.AllowAnonymousAccess;
            }
            set
            {
                mWebApplicationBuilder.AllowAnonymousAccess = value;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords")]
        public bool CreateNewDatabase
        {
            get
            {
                return mWebApplicationBuilder.CreateNewDatabase;
            }
            set
            {
                mWebApplicationBuilder.CreateNewDatabase = value;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords")]
        public string DatabaseName
        {
            get
            {
                return mWebApplicationBuilder.DatabaseName;
            }
            set
            {
                mWebApplicationBuilder.DatabaseName = value;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords")]
        public string DatabasePassword
        {
            get
            {
                return mWebApplicationBuilder.DatabasePassword;
            }
            set
            {
                mWebApplicationBuilder.DatabasePassword = value;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords")]
        public string DatabaseServer
        {
            get
            {
                return mWebApplicationBuilder.DatabaseServer;
            }
            set
            {
                mWebApplicationBuilder.DatabaseServer = value;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords")]
        public string DatabaseUsername
        {
            get
            {
                return mWebApplicationBuilder.DatabaseUsername;
            }
            set
            {
                mWebApplicationBuilder.DatabaseUsername = value;
            }
        }

        public Uri DefaultZoneUri
        {
            get
            {
                return mWebApplicationBuilder.DefaultZoneUri;
            }
            set
            {
                mWebApplicationBuilder.DefaultZoneUri = value;
            }
        }

        public string HostHeader
        {
            get
            {
                return mWebApplicationBuilder.HostHeader;
            }
            set
            {
                mWebApplicationBuilder.HostHeader = value;
            }
        }

        public Guid ID
        {
            get
            {
                return mWebApplicationBuilder.Id;
            }
            set
            {
                mWebApplicationBuilder.Id = value;
            }
        }

        public int Port
        {
            get
            {
                return mWebApplicationBuilder.Port;
            }
            set
            {
                mWebApplicationBuilder.Port = value;
            }
        }

        public DirectoryInfo RootDirectory
        {
            get
            {
                return mWebApplicationBuilder.RootDirectory;
            }
            set
            {
                mWebApplicationBuilder.RootDirectory = value;
            }
        }

        public string ServerComment
        {
            get
            {
                return mWebApplicationBuilder.ServerComment;
            }
            set
            {
                mWebApplicationBuilder.ServerComment = value;
            }
        }

        public bool UseNTLMExclusively
        {
            get
            {
                return mWebApplicationBuilder.UseNTLMExclusively;
            }
            set
            {
                mWebApplicationBuilder.UseNTLMExclusively = value;
            }
        }

        public bool UseSecureSocketsLayer
        {
            get
            {
                return mWebApplicationBuilder.UseSecureSocketsLayer;
            }
            set
            {
                mWebApplicationBuilder.UseSecureSocketsLayer = value;
            }
        }

        public IAveWebService WebService
        {
            get
            {
                if (mWebService == null)
                {
                    SPWebService webService = mWebApplicationBuilder.WebService;
                    if (webService != null)
                    {
                        mWebService = new AveWebService(webService);
                    }
                }
                return mWebService;
            }
            set
            {
                mWebService = value as AveWebService;
                if (mWebService != null)
                {
                    mWebApplicationBuilder.WebService = mWebService.WebService;
                }
                else
                {
                    mWebApplicationBuilder.WebService = null;
                }
            }
        }

        public IAveWebApplication Create()
        {
            SPWebApplication webApplication = mWebApplicationBuilder.Create();
            if (webApplication == null)
            {
                return null;
            }
            return new AveWebApplication(webApplication);
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (mSearchServiceInstance != null)
            {
                mSearchServiceInstance.Dispose();
                mSearchServiceInstance = null;
            }
        }

        #endregion
    }
}
