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
using AvePoint.Common;
using System;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.ObjectModel.ServerSE
{
    [SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords")]
    class AveDatabase : AvePersistedUpgradableObject, IAveDatabase
    {
        protected SPDatabase mDatabase;
        private AveServer mServer;
        private AveDatabaseServiceInstance mServiceInstance;
        private AveDatabaseServiceInstance mFailoverServiceInstance;

        public AveDatabase(SPDatabase database)
            : base(database)
        {
            mDatabase = database;
        }

        [SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords")]
        internal SPDatabase Database
        {
            get
            {
                return mDatabase;
            }
        }

        public IAveServer Server
        {
            get
            {
                if (mServer == null)
                {
                    SPServer server = mDatabase.Server;
                    if (server != null)
                    {
                        mServer = new AveServer(server);
                    }
                }
                return mServer;
            }
        }

        public void AddFailoverServiceInstance(string failoverServerInstance)
        {
            mDatabase.AddFailoverServiceInstance(failoverServerInstance);
        }

        public string NormalizedDataSource
        {
            get { return mDatabase.NormalizedDataSource; }
        }

        public bool Exists
        {
            get
            {
                return mDatabase.Exists;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords")]
        public string DatabaseConnectionString
        {
            get
            {
                return mDatabase.DatabaseConnectionString;
            }
        }

        public IAveQuerySession SqlSession
        {
            get { return new AveQuerySession(AveAssemblyUtility.GetPropertyValue(mDatabase, "SqlSession")); }
        }

        public string Password
        {
            get
            {
                return mDatabase.Password;
            }
            set
            {
                mDatabase.Password = value;
            }
        }

        public IAveDatabaseServiceInstance ServiceInstance
        {
            get
            {
                if (mServiceInstance == null)
                {
                    SPDatabaseServiceInstance databaseServiceInstance = mDatabase.ServiceInstance;
                    if (databaseServiceInstance != null)
                    {
                        mServiceInstance = new AveDatabaseServiceInstance(databaseServiceInstance);
                    }
                }
                return mServiceInstance;
            }
        }

        public string Username
        {
            get
            {
                return mDatabase.Username;
            }
            set
            {
                mDatabase.Username = value;
            }
        }

        public ulong DiskSizeRequired
        {
            get { return mDatabase.DiskSizeRequired; }
        }

        public IAveDatabaseServiceInstance FailoverServiceInstance
        {
            get
            {
                if (mFailoverServiceInstance == null)
                {
                    SPDatabaseServiceInstance databaseServiceInstance = mDatabase.FailoverServiceInstance;
                    if (databaseServiceInstance != null)
                    {
                        mFailoverServiceInstance = new AveDatabaseServiceInstance(databaseServiceInstance);
                    }
                }
                return mFailoverServiceInstance;
            }
            set
            {
                mFailoverServiceInstance = value as AveDatabaseServiceInstance;
                if (mFailoverServiceInstance != null)
                {
                    mDatabase.FailoverServiceInstance = mFailoverServiceInstance.DatabaseServiceInstance;
                }
                else
                {
                    mDatabase.FailoverServiceInstance = null;
                }
            }
        }

        public bool IsReadOnly
        {
            get { return mDatabase.IsReadOnly; }
        }

        public string SchemaVersionXml
        {
            get { return mDatabase.SchemaVersionXml; }
        }

        public Version BuildVersion
        {
            get { return (Version)AveAssemblyUtility.GetPropertyValue(mDatabase, "BuildVersion"); }
        }

        public string RealDatabaseType
        {
            get
            {
                return mDatabase.GetType().ToString();
            }
        }
    }
}
