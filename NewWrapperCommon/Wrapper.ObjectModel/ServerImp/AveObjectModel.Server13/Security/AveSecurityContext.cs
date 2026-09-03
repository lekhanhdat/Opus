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

namespace AvePoint.ObjectModel.Server13
{
    class AveSecurityContext : IAveSecurityContext
    {
        private const string mSecurityContext_Type = "Microsoft.SharePoint.Utilities.SecurityContext";
        private const string mSaveThreadTokenAndRevertToSelf_Method = "SaveThreadTokenAndRevertToSelf";
        private object mSecurityContext;

        public AveSecurityContext()
        { }

        public AveSecurityContext(IntPtr priorToken)
        {
            mSecurityContext = AveAssemblyUtility.CreateInstance(mSecurityContext_Type, new Type[] { typeof(IntPtr) }, new object[] { priorToken });
        }

        public AveSecurityContext(object securityContext)
        {
            mSecurityContext = securityContext;
        }

        public IAveSecurityContext RevertToSelf()
        {
            return this.SaveThreadTokenAndRevertToSelf();
        }

        public IAveSecurityContext SaveThreadTokenAndRevertToSelf()
        {
            object securityContext = AveAssemblyUtility.InvokeStaticMethod(mSecurityContext_Type, mSaveThreadTokenAndRevertToSelf_Method, new object[] { });
            if (securityContext == null)
            {
                return null;
            }
            return new AveSecurityContext(securityContext);
        }

        public void Dispose()
        {
            AveAssemblyUtility.InvokeMethod(mSecurityContext, "Dispose", new Type[] { }, new object[] { });
        }
    }
}
