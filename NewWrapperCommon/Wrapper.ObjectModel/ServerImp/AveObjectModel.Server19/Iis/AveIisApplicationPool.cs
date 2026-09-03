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
    class AveIisApplicationPool : AveMetabaseObject, IAveIisApplicationPool
    {
        private const string mIisApplicationPool_Type = "Microsoft.SharePoint.Administration.SPIisApplicationPool";
        private object mIisApplicationPool;

        public AveIisApplicationPool(object iisApplicationPool)
            : base((SPMetabaseObject)iisApplicationPool)
        {
            mIisApplicationPool = iisApplicationPool;
        }

        public AveIisApplicationPool(string name)
            : this(GetIisApplicationPool(name))
        { }

        private static object GetIisApplicationPool(string name)
        {
            return AveAssemblyUtility.CreateInstance(mIisApplicationPool_Type, new Type[] { typeof(string) }, new object[] { name });
        }

        internal object IisApplicationPool
        {
            get
            {
                return mIisApplicationPool;
            }
        }

        #region IAveIisApplicationPool Members

        public AveIdentityType CurrentIdentityType
        {
            get
            {
                return (AveIdentityType)AveAssemblyUtility.GetPropertyValue(mIisApplicationPool, "CurrentIdentityType");
            }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mIisApplicationPool, "CurrentIdentityType", value);
            }
        }

        #endregion
    }
}
