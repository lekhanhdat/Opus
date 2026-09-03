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
using Microsoft.SharePoint.Portal.SiteDirectory;
using System;

namespace AvePoint.ObjectModel.Server13
{
    class AveLinksCheckerJob : AveJobDefinition, IAveLinksCheckerJob
    {
        private LinksCheckerJob mLinksCheckerJob;

        public AveLinksCheckerJob(SPPersistedObject persistedObject)
            : base((SPJobDefinition)persistedObject)
        {
            mLinksCheckerJob = (LinksCheckerJob)persistedObject;
        }

        public AveLinksCheckerJob(LinksCheckerJob linksCheckerJob)
            : base(linksCheckerJob)
        {
            mLinksCheckerJob = linksCheckerJob;
        }

        public AveLinksCheckerJob(IAveService service)
            : this((LinksCheckerJob)AveAssemblyUtility.CreateInstance(typeof(LinksCheckerJob), new Type[] { typeof(SPService) }, new object[] { (service as AveService).Service }))
        { }

        #region IAveLinksCheckerJob Members

        public bool IsMetaDataSyncEnabled
        {
            get
            {
                return mLinksCheckerJob.IsMetaDataSyncEnabled;
            }
            set
            {
                mLinksCheckerJob.IsMetaDataSyncEnabled = value;
            }
        }

        public string SiteDirectoryConfig
        {
            get
            {
                return mLinksCheckerJob.SiteDirectoryConfig;
            }
            set
            {
                mLinksCheckerJob.SiteDirectoryConfig = value;
            }
        }

        public void SetDefaults()
        {
            AveAssemblyUtility.InvokeMethod(mLinksCheckerJob, "SetDefaults", new Type[] { }, new object[] { });
        }

        #endregion
    }
}
