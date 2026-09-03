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
    using AvePoint.GCommon;
    using System.Reflection;

    #endregion

    public class IndexDatabaseVersionManager
      : IIndexDatabaseVersionManager
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public IndexDatabaseHelper DatabaseHelper { get; set; }

        public IndexDatabaseVersionManager()
        { }

        public IndexDatabaseVersionManager(IndexDatabaseHelper databaseHelper)
        {
            this.DatabaseHelper = databaseHelper;
        }

        public virtual String GetVersion()
        {
            var result = default(String);
            var selectCommand = "SELECT COL_VALUE FROM tb_job_info WHERE COL_GUID='{0}'".FormatWith(ServiceConstants.VersionGuid);
            try
            {
                var version = this.DatabaseHelper.ExecuteScalar(selectCommand);
                if (version != null) result = version.ToString();
            }
            catch(Exception e)
            {
                logger.Warn($"IndexDatabase VersionManager GetVersion:{e}");
            }
            return result;
        }

        //public virtual void UpdateVersion()
        //{
        //    var dataVersion = this.GetVersion();
        //    var updateCommand = default(String);
        //    if (string.IsNullOrEmpty(dataVersion))
        //    {
        //        updateCommand = "INSERT INTO tb_job_info (COL_GUID, COL_KEY, COL_VALUE) VALUES('{0}', 'version', '{1}')".FormatWith(
        //            ServiceConstants.VersionGuid,
        //            MediaEnvironment.MediaServer.MediaServerVersion);
        //    }
        //    else
        //    {
        //        updateCommand = "UPDATE tb_job_info SET COL_VALUE='{0}' WHERE COL_GUID='{1}'".FormatWith(
        //              MediaEnvironment.MediaServer.MediaServerVersion,
        //              ServiceConstants.VersionGuid);
        //    }

        //    this.DatabaseHelper.ExecuteNonQuery(updateCommand, default(Dictionary<String, Object>));
        //}
    }
}
