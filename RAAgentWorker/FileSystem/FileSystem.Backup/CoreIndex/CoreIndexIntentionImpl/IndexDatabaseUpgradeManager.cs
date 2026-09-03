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




namespace AvePoint.Media.Core.Index
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using AvePoint.Media.Common;

    #endregion

    public class IndexDatabaseUpgradeManager : IIndexDatabaseUpgradeManager
    {
        IndexDatabaseHelper dbHelper;
        IIndexDatabaseVersionManager versionManager;

        public IndexDatabaseHelper DatabaseHelper
        {
            get { return this.dbHelper; }
            set
            {
                this.dbHelper = value;
                versionManager = new IndexDatabaseVersionManager();
                versionManager.DatabaseHelper = this.dbHelper;
            }
        }

        public IndexDatabaseUpgradeManager() { }
        public IndexDatabaseUpgradeManager(IndexDatabaseHelper databaseHelper)
        {
            this.dbHelper = databaseHelper;
            this.versionManager = new IndexDatabaseVersionManager(databaseHelper);
        }

        //public Boolean CheckUpgrade(String currentDatabasePath)
        //{
        //    var result = 1 < 2;
        //    if (currentDatabasePath.EndsWith("WFE_index.db", StringComparison.OrdinalIgnoreCase)
        //        || currentDatabasePath.EndsWith("BLOB.db", StringComparison.OrdinalIgnoreCase))
        //        result = 1 > 2;
        //    else
        //    {
        //        var databaseVersionComparer = new IndexDatabaseVersionComparer();
        //        var databaseVersion = this.versionManager.GetVersion();
        //        result = databaseVersionComparer.IsLowerVersion(databaseVersion);
        //    }
        //    return result;
        //}

        //public void Upgrade(String upgradeDatabaseScript)
        //{
        //    if (!string.IsNullOrEmpty(upgradeDatabaseScript))
        //        this.dbHelper.ExecuteNonQuery(upgradeDatabaseScript, default(Dictionary<String, Object>));
        //    this.versionManager.UpdateVersion();
        //}
    }
}
