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



using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;
using System;

namespace AvePoint.ObjectModel.Server13
{
    class AveContext : IAveContext, IDisposable 
    {
        private SPContext mContext;
        private static AveContext mCurrentContext;
        private AveWeb mWeb;

        public AveContext(AveWeb web, SPContext context)
        {
            mWeb = web;
            mContext = context;
        }

        public IAveContext Current
        {
            get
            {
                if (mCurrentContext == null)
                {
                    SPContext context = SPContext.Current;
                    if (context != null)
                    {
                        mCurrentContext = new AveContext(mWeb, context);
                    }
                }
                return mCurrentContext;
            }
        }

        #region IAveSPContext Members

        public IAveWeb Web
        {
            get
            {
                if (mWeb == null)
                {
                    SPWeb web = mContext.Web;
                    if (web != null)
                    {
                        mWeb = new AveWeb(new AveSite(web.Site), web);
                    }
                }
                return mWeb;
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (mWeb != null)
            {
                mWeb.Dispose();
                mWeb = null;
            }
        }

        #endregion
    }
}
