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



using Microsoft.SharePoint.Administration;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server13
{
    class AveService : AvePersistedUpgradableObject, IAveService
    {
        protected SPService mService;
        private AveServiceInstanceDependencyCollection mInstances;
        private AveServiceApplicationCollection mApplications;
        private AveJobDefinitionCollection mJobDefinitions;

        public AveService()
        { }

        public AveService(SPService service)
            : base(service)
        {
            mService = service;
        }

        internal SPService Service
        {
            get
            {
                return mService;
            }
        }

        #region IAveService Members

        public IAveServiceApplicationCollection Applications
        {
            get
            {
                if (mApplications == null)
                {
                    mApplications = new AveServiceApplicationCollection(mService.Applications);
                }
                return mApplications;
            }
        }

        public IAveServiceInstanceDependencyCollection Instances
        {
            get
            {
                if (mInstances == null)
                {
                    mInstances = new AveServiceInstanceDependencyCollection(this);
                }
                return mInstances;
            }
        }

        public IAveJobDefinitionCollection JobDefinitions
        {
            get
            {
                if (mJobDefinitions == null)
                {
                    mJobDefinitions = new AveJobDefinitionCollection(mService.JobDefinitions);
                }
                return mJobDefinitions;
            }
        }

        public bool Required
        {
            get { return mService.Required; }
        }

        public string RealServiceType
        {
            get
            {
                return mService.GetType().ToString();
            }
        }

        #endregion
    }
}
