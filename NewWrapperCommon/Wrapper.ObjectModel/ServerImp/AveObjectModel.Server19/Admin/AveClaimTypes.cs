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



using Microsoft.SharePoint.Administration.Claims;
using AvePoint.Wrapper.Common;


namespace AvePoint.ObjectModel.Server19
{
    class AveClaimTypes : IAveClaimTypes
    {
        public string DistributionListClaimType
        {
            get { return SPClaimTypes.DistributionListClaimType; }
        }

        public string FarmId
        {
            get { return SPClaimTypes.FarmId; }
        }

        public string IdentityProvider
        {
            get { return SPClaimTypes.IdentityProvider; }
        }

        public string IsAuthenticated
        {
            get { return SPClaimTypes.IsAuthenticated; }
        }

        public string ProviderUserKey
        {
            get { return SPClaimTypes.ProviderUserKey; }
        }

        public string TokenReference
        {
            get { return SPClaimTypes.TokenReference; }
        }

        public string UserIdentifier
        {
            get { return SPClaimTypes.UserIdentifier; }
        }

        public string UserLogonName
        {
            get { return SPClaimTypes.UserLogonName; }
        }

        public bool Equals(string claimTypeOne, string claimTypeTwo)
        {
            return SPClaimTypes.Equals(claimTypeOne, claimTypeTwo);
        }

        public bool IsValid(string claimType)
        {
            return SPClaimTypes.IsValid(claimType);
        }
    }
}
