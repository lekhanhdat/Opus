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
using Microsoft.SharePoint.Administration.Claims;
using AvePoint.Wrapper.Common;
using AvePoint.Common;

namespace AvePoint.ObjectModel.Server13
{
    class AveClaimProviderManager : AvePersistedObject, IAveClaimProviderManager
    {
        private SPClaimProviderManager mClaimProviderManager;
        private AveClaimProviderManager mLocal;

        public AveClaimProviderManager(SPClaimProviderManager claimProviderManager)
            : base(claimProviderManager)
        {
            mClaimProviderManager = claimProviderManager;
        }

        public AveClaimProviderManager()
            : this(new SPClaimProviderManager())
        { }

        public IAveClaimProviderManager Local
        {
            get
            {
                if (mLocal == null)
                {
                    mLocal = new AveClaimProviderManager(SPClaimProviderManager.Local);
                }
                return mLocal;
            }
        }

        public IEnumerable<string> GetClaimProviderNamesForContext(Uri context, AveClaimProviderOperationOptions mode)
        {
            return (IEnumerable<string>)AveAssemblyUtility.InvokeMethod(mClaimProviderManager, "GetClaimProviderNamesForContext", new object[] { context, (SPClaimProviderOperationOptions)mode });
        }

        public IEnumerable<string> GetClaimProviderNamesForContext(Uri context, AveClaimProviderOperationOptions mode, IEnumerable<string> providerNames)
        {
            return (IEnumerable<string>)AveAssemblyUtility.InvokeMethod(mClaimProviderManager, "GetClaimProviderNamesForContext", new object[] { context, (SPClaimProviderOperationOptions)mode, providerNames });
        }

        public IEnumerable<IAveClaimProvider> GetClaimProvidersForContext(Uri context)
        {
            return GetClaimProvidersForContext(context, AveClaimProviderOperationOptions.None, null);
        }

        public IEnumerable<IAveClaimProvider> GetClaimProvidersForContext(Uri context, AveClaimProviderOperationOptions mode, IEnumerable<string> providerNames)
        {
            IEnumerable<SPClaimProvider> claimProviders = (IEnumerable<SPClaimProvider>)AveAssemblyUtility.InvokeMethod(mClaimProviderManager, "GetClaimProvidersForContext", new object[] { context, (SPClaimProviderOperationOptions)mode, providerNames });
            List<IAveClaimProvider> claimProviderList = new List<IAveClaimProvider>();
            foreach (SPClaimProvider claimProvider in claimProviders)
            {
                claimProviderList.Add(new AveClaimProvider(claimProvider));
            }
            return claimProviderList;
        }

        public string EncodeClaim(IAveClaim claim)
        {
            return mClaimProviderManager.EncodeClaim((claim as AveClaim).Claim);
        }

        public IAveClaim CreateUserClaim(string userIdentifier, AveOriginalIssuerType issuerType)
        {
            return new AveClaim(SPClaimProviderManager.CreateUserClaim(userIdentifier, (SPOriginalIssuerType)issuerType));
        }

        public IAveClaim CreateUserClaim(string userIdentifier, AveOriginalIssuerType issuerType, string issuerIdentifier)
        {
            return new AveClaim(SPClaimProviderManager.CreateUserClaim(userIdentifier, (SPOriginalIssuerType)issuerType, issuerIdentifier));
        }

        public IEnumerable<IAveTrustedClaimProvider> TrustedClaimProviders
        {
            get
            {
                var trustedClaimProviders = mClaimProviderManager.TrustedClaimProviders;
                if (trustedClaimProviders == null)
                {
                    return null;
                }
                List<IAveTrustedClaimProvider> trustedClaimProviderList = new List<IAveTrustedClaimProvider>();
                foreach (SPTrustedClaimProvider trustedClaimProvider in trustedClaimProviders)
                {
                    trustedClaimProviderList.Add(new AveTrustedClaimProvider(trustedClaimProvider));
                }
                return trustedClaimProviderList;
            }
        }
    }
}
