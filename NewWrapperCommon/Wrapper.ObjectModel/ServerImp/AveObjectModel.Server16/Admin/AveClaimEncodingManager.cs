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



using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Administration.Claims;

namespace AvePoint.ObjectModel.Server16
{
    class AveClaimEncodingManager : AvePersistedObject, IAveClaimEncodingManager
    {
        private object mClaimEncodingManager;
        private const string mClaimEncodingManager_Type = "Microsoft.SharePoint.Administration.Claims.SPClaimEncodingManager";

        public AveClaimEncodingManager()
            : this(AveAssemblyUtility.CreateInstance(mClaimEncodingManager_Type))
        { }

        public AveClaimEncodingManager(object claimEncodingManager)
            : base(claimEncodingManager as SPPersistedObject)
        {
            mClaimEncodingManager = claimEncodingManager;
        }

        public IAveClaim DecodeClaim(string value)
        {
            return new AveClaim((SPClaim)AveAssemblyUtility.InvokeStaticMethod(mClaimEncodingManager_Type, "DecodeClaim", new object[] { value }));
        }

        public bool IsEncodedClaim(string value)
        {
            return (bool)AveAssemblyUtility.InvokeStaticMethod(mClaimEncodingManager_Type, "IsEncodedClaim", new object[] { value });
        }
    }
}
