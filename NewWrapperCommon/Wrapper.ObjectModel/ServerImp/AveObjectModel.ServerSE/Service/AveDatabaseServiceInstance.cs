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
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.ObjectModel.ServerSE
{
    [SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords")]
    class AveDatabaseServiceInstance : AveServiceInstance, IAveDatabaseServiceInstance
    {
        private SPDatabaseServiceInstance mDatabaseServiceInstance;
        private ICollection<string> mRoles;
        private AveDatabaseCollection mDatabases;

        public AveDatabaseServiceInstance(SPDatabaseServiceInstance databaseServiceInstance)
            : base(databaseServiceInstance)
        {
            mDatabaseServiceInstance = databaseServiceInstance;
        }

        public AveDatabaseServiceInstance(string name, IAveServer server, IAveDatabaseService service)
            : this(new SPDatabaseServiceInstance(name, (server as AveServer).Server, (service as AveDatabaseService).DatabaseService))
        { }

        [SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords")]
        internal SPDatabaseServiceInstance DatabaseServiceInstance
        {
            get
            {
                return mDatabaseServiceInstance;
            }
        }

        public string NormalizedDataSource
        {
            get
            {
                return mDatabaseServiceInstance.NormalizedDataSource;
            }
        }

        public override ICollection<string> Roles
        {
            get
            {
                if (mRoles == null)
                {
                    mRoles = mDatabaseServiceInstance.Roles;
                }
                return mRoles;
            }
        }

        public IAveDatabaseCollection Databases
        {
            get 
            {
                if (mDatabases == null)
                {
                    mDatabases = new AveDatabaseCollection(mDatabaseServiceInstance.Databases);
                }
                return mDatabases;
            }
        }
    }
}
