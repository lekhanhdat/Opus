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
using System.Text;
using System.Web.Configuration;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Common
{
    class AveIisSettings : AveClientObject, IAveIisSettings
    {
        private AveFormsAuthenticationProvider mFormsAuthProvider;

        public AveIisSettings(Dictionary<string, object> iisProperties)
        {
            base.DataCache.AddPropertyies(iisProperties);
        }

        #region IAveIisSettings Members

        public AuthenticationMode AuthenticationMode
        {
            get
            {                
                return base.DataCache.GetProperty<AuthenticationMode>("AveAuthenticationMode");
            }
            set
            {
                base.DataCache.AddChangedProperty("AveAuthenticationMode", value);
            }
        }

        #endregion

        public bool AllowAnonymous
        {
            get
            {
                return base.DataCache.GetProperty<bool>("AllowAnonymous");
            }
            set
            {
                base.DataCache.AddChangedProperty("AllowAnonymous", value);
            }
        }

        public bool UseWindowsIntegratedAuthentication
        {
            get
            {
                return base.DataCache.GetProperty<bool>("UseWindowsIntegratedAuthentication");
            }
            set
            {
                base.DataCache.AddChangedProperty("UseWindowsIntegratedAuthentication", value);
            }
        }

        public bool DisableKerberos
        {
            get
            {
                return base.DataCache.GetProperty<bool>("DisableKerberos");
            }
            set
            {
                base.DataCache.AddChangedProperty("DisableKerberos", value);
            }
        }

        public bool UseBasicAuthentication
        {
            get
            {
                return base.DataCache.GetProperty<bool>("UseBasicAuthentication");
            }
            set
            {
                base.DataCache.AddChangedProperty("UseBasicAuthentication", value);
            }
        }

        public bool EnableClientIntegration
        {
            get
            {
                return base.DataCache.GetProperty<bool>("EnableClientIntegration");
            }
            set
            {
                base.DataCache.AddChangedProperty("EnableClientIntegration", value);
            }
        }

        public bool ClientObjectModelRequiresUseRemoteAPIsPermission
        {
            get
            {
                return base.DataCache.GetProperty<bool>("ClientObjectModelRequiresUseRemoteAPIsPermission");
            }
            set
            {
                base.DataCache.AddChangedProperty("ClientObjectModelRequiresUseRemoteAPIsPermission", value);
            }
        }

        public string RoleManager
        {
            get
            {
                return base.DataCache.GetProperty<string>("RoleManager");
            }
            set
            {
                base.DataCache.AddChangedProperty("RoleManager", value);
            }
        }

        public string MembershipProvider
        {
            get
            {
                return base.DataCache.GetProperty<string>("MembershipProvider");
            }
            set
            {
                base.DataCache.AddChangedProperty("MembershipProvider", value);
            }
        }


        public System.Collections.ObjectModel.Collection<IAveSecureBinding> SecureBindings
        {
            get { throw new NotImplementedException(); }
        }

        public System.Collections.ObjectModel.Collection<IAveServerBinding> ServerBindings
        {
            get { throw new NotImplementedException(); }
        }

        public string ServerComment
        {
            get { return base.DataCache.GetProperty<string>("ServerComment"); }
        }

        public int PreferredInstanceId
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #region IAveIisSettings Members


        public IAveFormsAuthenticationProvider FormsClaimsAuthenticationProvider
        {
            get 
            {
                if (base.DataCache.IsPropertyNotLoaded("FormsClaimsAuthenticationProvider") && base.DataCache.IsPropertyAvailable("FormsClaimsAuthenticationProvider" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    mFormsAuthProvider = new AveFormsAuthenticationProvider(this, base.DataCache.GetProperty<Dictionary<string, object>>("FormsClaimsAuthenticationProvider" + AveObjectModelConstant.ObjectPropertySuffix));
                }
                return mFormsAuthProvider;
            }
        }

        public System.IO.DirectoryInfo Path
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool UseClaimsAuthentication
        {
            get { return base.DataCache.GetProperty<bool>("UseClaimsAuthentication"); }
        }

        #endregion


        public bool ClaimsAuthenticationEnabled
        {
            get
            {
                return base.DataCache.GetProperty<bool>("ClaimsAuthenticationEnabled");
            }
            set
            {
                base.DataCache.AddChangedProperty("ClaimsAuthenticationEnabled", value);
            }
        }

        public Uri ClaimsAuthenticationRedirectionUrl
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void ReplaceClaimsAuthenticationProviders(IEnumerable<IAveAuthenticationProvider> providers)
        {
            throw new NotImplementedException();
        }

        public System.Xml.XmlDocument GetStateXml()
        {
            throw new NotImplementedException();
        }


        public IEnumerable<IAveAuthenticationProvider> ClaimsAuthenticationProviders
        {
            get { throw new NotImplementedException(); }
        }

        public bool UseFormsClaimsAuthenticationProvider
        {
            get { return base.DataCache.GetProperty<bool>("UseFormsClaimsAuthenticationProvider"); }
        }

        public bool UseTrustedClaimsAuthenticationProvider
        {
            get { return base.DataCache.GetProperty<bool>("UseTrustedClaimsAuthenticationProvider"); }
        }

        public bool UseWindowsClaimsAuthenticationProvider
        {
            get { return base.DataCache.GetProperty<bool>("UseWindowsClaimsAuthenticationProvider"); }
        }

        public bool SecurityPolicyChanged
        {
            get { return base.DataCache.GetProperty<bool>("SecurityPolicyChanged"); }
        }
    }
}
