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



using System.Collections.Generic;
using Microsoft.SharePoint.Administration;
using AvePoint.Wrapper.Common;
using AvePoint.Common;

namespace AvePoint.ObjectModel.Server16
{
    class AveServiceInstance : AvePersistedUpgradableObject, IAveServiceInstance
    {
        private SPServiceInstance mServiceInstance;
        private AveServer mServer;
        private AveService mService;
        private ICollection<string> mRoles;

        public AveServiceInstance(SPServiceInstance serviceInstance)
            : base(serviceInstance)
        {
            mServiceInstance = serviceInstance;
        }

        public AveServiceInstance()
            : this(new SPServiceInstance())
        { }

        internal SPServiceInstance ServiceInstance
        {
            get { return mServiceInstance; }
        }

        public IAveService Service
        {
            get
            {
                if (mService == null)
                {
                    SPService service = mServiceInstance.Service;
                    if (service != null)
                    {
                        mService = new AveService(service);
                    }
                }
                return mService;
            }
        }

        public bool Hidden
        {
            get
            {
                return mServiceInstance.Hidden;
            }
        }

        public bool SystemService
        {
            get
            {
                return mServiceInstance.SystemService;
            }
        }

        public IAveServer Server
        {
            get
            {
                if (mServer == null)
                {
                    SPServer server = mServiceInstance.Server;
                    if (server != null)
                    {
                        mServer = new AveServer(server);
                    }
                }
                return mServer;
            }
        }


        public bool TimerJobError
        {
            get
            {
                return (bool)AveAssemblyUtility.GetPropertyValue(mServiceInstance, "TimerJobError");
            }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mServiceInstance, "TimerJobError", value);
            }
        }

        public virtual ICollection<string> Roles
        {
            get
            {
                if (mRoles == null)
                {
                    mRoles = mServiceInstance.Roles;
                }
                return mRoles;
            }
        }

        public string RealServiceInstanceType
        {
            get 
            {
                return mServiceInstance.GetType().ToString();
            }
        }
    }
}
