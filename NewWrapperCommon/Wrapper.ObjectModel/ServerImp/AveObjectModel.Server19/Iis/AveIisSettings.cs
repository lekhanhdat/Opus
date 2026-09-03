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
using System.Collections.ObjectModel;
using System.IO;
using System.Web.Configuration;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Administration;


namespace AvePoint.ObjectModel.Server19
{
    class AveIisSettings : AveAutoSerializingObject, IAveIisSettings
    {
        static AveIisSettings()
        {
            //mAssembly = Assembly.GetCallingAssembly();
        }

        //protected static Assembly mAssembly;
        private SPIisSettings mIisSettings;
        private Collection<IAveSecureBinding> mSecureBindings;
        private Collection<IAveServerBinding> mServerBindings;
        private List<IAveAuthenticationProvider> mClaimsAuthenticationProviders;

        public AveIisSettings(SPIisSettings iisSettings)
            : base(iisSettings)
        {
            mIisSettings = iisSettings;
        }

        public AveIisSettings()
            : this(new SPIisSettings())
        { }
        
        public AveIisSettings(string serverComment, bool allowAnonymous, bool disableKerberos, IAveServerBinding serverBinding, IAveSecureBinding secureBinding, DirectoryInfo path)
            : this(new SPIisSettings(serverComment, allowAnonymous, disableKerberos, (serverBinding as AveServerBinding).ServerBinding, (secureBinding as AveSecureBinding).SecureBinding, path))
        { }

        internal SPIisSettings IisSettings
        {
            get
            {
                return mIisSettings;
            }
        }

        #region IAveIisSettings Members

        public AuthenticationMode AuthenticationMode
        {
            get
            {
                return mIisSettings.AuthenticationMode;
            }
            set
            {
                mIisSettings.AuthenticationMode = value;
            }
        }

        public Collection<IAveSecureBinding> SecureBindings
        {
            get
            {
                if (mSecureBindings == null)
                {
                    mSecureBindings = new Collection<IAveSecureBinding>();
                    foreach (SPSecureBinding secureBinding in mIisSettings.SecureBindings)
                    {
                        mSecureBindings.Add(new AveSecureBinding(secureBinding));
                    }
                }
                return mSecureBindings;
            }
        }

        public Collection<IAveServerBinding> ServerBindings
        {
            get
            {
                if (mServerBindings == null)
                {
                    mServerBindings = new Collection<IAveServerBinding>();
                    foreach (SPServerBinding serverBinding in mIisSettings.ServerBindings)
                    {
                        mServerBindings.Add(new AveServerBinding(serverBinding));
                    }
                }
                return mServerBindings;
            }
        }

        public string ServerComment
        {
            get
            {
                return mIisSettings.ServerComment;
            }
        }

        public int PreferredInstanceId
        {
            get
            {
                return mIisSettings.PreferredInstanceId;
            }
            set
            {
                mIisSettings.PreferredInstanceId = value;
            }
        }

        public bool AllowAnonymous
        {
            get
            {
                return mIisSettings.AllowAnonymous;
            }
            set
            {
                mIisSettings.AllowAnonymous = value;
            }
        }

        public bool UseWindowsIntegratedAuthentication
        {
            get
            {
                return mIisSettings.UseWindowsIntegratedAuthentication;
            }
            set
            {
                mIisSettings.UseWindowsIntegratedAuthentication = value;
            }
        }

        public bool DisableKerberos
        {
            get
            {
                return mIisSettings.DisableKerberos;
            }
            set
            {
                mIisSettings.DisableKerberos = value;
            }
        }

        public bool UseBasicAuthentication
        {
            get
            {
                return mIisSettings.UseBasicAuthentication;
            }
            set
            {
                mIisSettings.UseBasicAuthentication = value;
            }
        }

        public bool EnableClientIntegration
        {
            get
            {
                return mIisSettings.EnableClientIntegration;
            }
            set
            {
                mIisSettings.EnableClientIntegration = value;
            }
        }

        public bool ClientObjectModelRequiresUseRemoteAPIsPermission
        {
            get
            {
                return mIisSettings.ClientObjectModelRequiresUseRemoteAPIsPermission;
            }
            set
            {
                mIisSettings.ClientObjectModelRequiresUseRemoteAPIsPermission = value;
            }
        }

        public string RoleManager
        {
            get
            {
                return mIisSettings.RoleManager;
            }
            set
            {
                mIisSettings.RoleManager = value;
            }
        }

        public string MembershipProvider
        {
            get
            {
                return mIisSettings.MembershipProvider;
            }
            set
            {
                mIisSettings.MembershipProvider = value;
            }
        }

        public IAveFormsAuthenticationProvider FormsClaimsAuthenticationProvider
        {
            get
            {
                SPFormsAuthenticationProvider formsAuthenticationProvider = mIisSettings.FormsClaimsAuthenticationProvider;
                if (formsAuthenticationProvider == null)
                {
                    return null;
                }
                return new AveFormsAuthenticationProvider(formsAuthenticationProvider);
            }
        }

        public DirectoryInfo Path
        {
            get
            {
                return mIisSettings.Path;
            }
            set
            {
                mIisSettings.Path = value;
            }
        }

        public bool UseClaimsAuthentication
        {
            get { return mIisSettings.UseClaimsAuthentication; }
        }

        public bool ClaimsAuthenticationEnabled
        {
            get
            {
                return (bool)AveAssemblyUtility.GetPropertyValue(mIisSettings, "ClaimsAuthenticationEnabled");
            }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mIisSettings, "ClaimsAuthenticationEnabled", value);
            }
        }

        public Uri ClaimsAuthenticationRedirectionUrl
        {
            get
            {
                return mIisSettings.ClaimsAuthenticationRedirectionUrl;
            }
            set
            {
                mIisSettings.ClaimsAuthenticationRedirectionUrl = value;
            }
        }

        public void ReplaceClaimsAuthenticationProviders(IEnumerable<IAveAuthenticationProvider> providers)
        {
            List<SPAuthenticationProvider> authenticationProviders = new List<SPAuthenticationProvider>();
            foreach (IAveAuthenticationProvider authenticationProvider in providers)
            {
                authenticationProviders.Add((authenticationProvider as AveAuthenticationProvider).AuthenticationProvider);
            }
            mIisSettings.ReplaceClaimsAuthenticationProviders(authenticationProviders);
        }

        public IEnumerable<IAveAuthenticationProvider> ClaimsAuthenticationProviders
        {
            get
            {
                if (mClaimsAuthenticationProviders == null)
                {
                    mClaimsAuthenticationProviders = new List<IAveAuthenticationProvider>();
                    if (mIisSettings.ClaimsAuthenticationProviders != null)
                    {
                        foreach (SPAuthenticationProvider authenticationProvider in mIisSettings.ClaimsAuthenticationProviders)
                        {
                            mClaimsAuthenticationProviders.Add((AveAuthenticationProvider)CreateInstance(authenticationProvider));
                        }
                    }
                }
                return mClaimsAuthenticationProviders;
            }
        }

        private object CreateInstance(SPAuthenticationProvider authenticationProvider)
        {
            return AveServerAssemblyInit.CreateElement(typeof(IAveIisSettings), new object[] { authenticationProvider });
        }

        public bool UseFormsClaimsAuthenticationProvider
        {
            get
            {
                return mIisSettings.UseFormsClaimsAuthenticationProvider;
            }
        }

        public bool UseTrustedClaimsAuthenticationProvider
        {
            get
            {
                return mIisSettings.UseTrustedClaimsAuthenticationProvider;
            }
        }

        public bool UseWindowsClaimsAuthenticationProvider
        {
            get
            {
                return mIisSettings.UseWindowsClaimsAuthenticationProvider;
            }
        }

        public bool SecurityPolicyChanged
        {
            get
            {
                return (bool)AveAssemblyUtility.GetPropertyValue(mIisSettings, "SecurityPolicyChanged");
            }
        }

        #endregion
    }
}
