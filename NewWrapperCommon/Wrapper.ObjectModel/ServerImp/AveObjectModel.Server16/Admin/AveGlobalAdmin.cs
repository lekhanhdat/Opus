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



using System.Globalization;
using System.Collections;
using Microsoft.SharePoint.Administration;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server16
{
    class AveGlobalAdmin : IAveGlobalAdmin
    {
        private SPGlobalAdmin mGlobalAdmin;
        private AveVirtualServerCollection mVirtualServers;
        private IDictionary mApplicationPools;
        private AveGlobalConfig mGlobalConfig;

        public AveGlobalAdmin(SPGlobalAdmin globalAdmin)
        {
            mGlobalAdmin = globalAdmin;
        }

        public AveGlobalAdmin()
        {
            mGlobalAdmin = new SPGlobalAdmin();
        }

        #region IAveGlobalAdmin Members

        public CultureInfo ServerCulture
        {
            get { return SPGlobalAdmin.ServerCulture; }
        }

        public IAveVirtualServerCollection VirtualServers
        {
            get
            {
                if (mVirtualServers == null)
                {
                    mVirtualServers = new AveVirtualServerCollection(mGlobalAdmin.VirtualServers);
                }
                return mVirtualServers;
            }
        }

        public int MailCodePage
        {
            get
            {
                return mGlobalAdmin.MailCodePage;
            }
        }

        #endregion

        public void Dispose()
        {
            mGlobalAdmin.Dispose();
        }

        public IDictionary ApplicationPools
        {
            get
            {
                if (mApplicationPools == null)
                {
                    mApplicationPools = mGlobalAdmin.ApplicationPools;
                }
                return mApplicationPools;
            }
        }

        public IAveGlobalConfig Config
        {
            get
            {
                if (mGlobalConfig == null)
                {
                    mGlobalConfig = new AveGlobalConfig(mGlobalAdmin.Config);
                }
                return mGlobalConfig;
            }
        }
    }
}
