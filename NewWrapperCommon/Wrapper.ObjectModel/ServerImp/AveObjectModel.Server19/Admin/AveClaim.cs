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
    class AveClaim : IAveClaim
    {
        private SPClaim mClaim;

        public AveClaim(SPClaim claim)
        {
            mClaim = claim;
        }

        public AveClaim(string type, string value, string valueType, string originalIssuer)
            : this(new SPClaim(type, value, valueType, originalIssuer))
        { }

        internal SPClaim Claim
        {
            get
            {
                return mClaim;
            }
        }

        public string ClaimType
        {
            get { return this.mClaim.ClaimType; }
        }

        public string OriginalIssuer
        {
            get { return this.mClaim.OriginalIssuer; }
        }

        public string Value
        {
            get { return this.mClaim.Value; }
        }

        public string ValueType
        {
            get { return this.mClaim.ValueType; }
        }

        public override string ToString()
        {
            return this.mClaim.ToString();
        }

        public string ToEncodedString()
        {
            return this.mClaim.ToEncodedString();
        }
    }
}
