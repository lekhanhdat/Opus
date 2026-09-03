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
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Administration;

namespace AvePoint.ObjectModel.Server19
{
    class AveTrustedRootAuthorityManager : AvePersistedObject, IAveTrustedRootAuthorityManager
    {
        private const string mTrustedRootAuthorityManager_Type = "Microsoft.SharePoint.Administration.SPTrustedRootAuthorityManager";
        private object mTrustedRootAuthorityManager;
        private AveTrustedRootAuthorityCollection mRootAuthorities;

        public AveTrustedRootAuthorityManager(IAveFarm farm)
            : this(AveAssemblyUtility.InvokeStaticMethod(mTrustedRootAuthorityManager_Type, "GetLocal", new Type[] { typeof(SPFarm) }, new object[] { (farm as AveFarm).Farm }))
        { }

        public AveTrustedRootAuthorityManager()
            : this(AveAssemblyUtility.CreateInstance(mTrustedRootAuthorityManager_Type))
        { }

        public AveTrustedRootAuthorityManager(object trustedRootAuthorityManager)
            : base(trustedRootAuthorityManager as SPPersistedObject)
        {
            mTrustedRootAuthorityManager = trustedRootAuthorityManager;
        }

        public IAveTrustedRootAuthorityCollection RootAuthorities
        {
            get
            {
                if (mRootAuthorities == null)
                {
                    mRootAuthorities = new AveTrustedRootAuthorityCollection(AveAssemblyUtility.GetPropertyValue(mTrustedRootAuthorityManager, "RootAuthorities"));
                }
                return mRootAuthorities;
            }
        }

        public IAveTrustedRootAuthorityManager GetLocal(IAveFarm farm)
        {
            object trustedRootAutorityManager = AveAssemblyUtility.InvokeStaticMethod(mTrustedRootAuthorityManager_Type, "GetLocal", new Type[] { typeof(SPFarm) }, new object[] { (farm as AveFarm).Farm });
            if (trustedRootAutorityManager == null)
            {
                return null;
            }
            return new AveTrustedRootAuthorityManager(trustedRootAutorityManager);
        }
    }
}
