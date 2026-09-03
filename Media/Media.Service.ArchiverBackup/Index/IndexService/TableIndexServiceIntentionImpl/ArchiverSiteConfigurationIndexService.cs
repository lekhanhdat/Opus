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




namespace AvePoint.Media.Service.ArchiverBackup
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using AvePoint.Media.Service.DomainModel;
    #endregion

    public class ArchiverSiteConfigurationIndexService : ArchiverTableIndexServiceBase, IArchiverSiteConfigurationIndexService
    {
        public void UpdateSiteConfigInfo(ArchiverSiteConfigurationIndex config)
        {
            string sql = "select count(*) "
            + " from " + IndexConstants.TableNameArchiveSiteConfiguration
            + " where COL_JOB_ID = @COL_JOB_ID "
            + " and COL_ARCHIVE_TIME = @COL_ARCHIVE_TIME ";
            Dictionary<string, object> parameterDictionary = new Dictionary<string, object>();
            parameterDictionary["@COL_JOB_ID"] = config.JobId;
            parameterDictionary["@COL_ARCHIVE_TIME"] = config.ArchiveTime;

            long count = (long)IndexProcessor.ExecuteScalar(sql, parameterDictionary);
            if (count > 0)
            {
                sql = "update " + IndexConstants.TableNameArchiveSiteConfiguration
                    + " set COL_SITE_INFO = @COL_SITE_INFO, "
                    + " COL_STATUS = @COL_STATUS, "
                    + " COL_VERSION = @COL_VERSION "
                    + " where COL_JOB_ID = @COL_JOB_ID "
                    + " and COL_ARCHIVE_TIME = @COL_ARCHIVE_TIME ";
                parameterDictionary.Clear();
                parameterDictionary["@COL_SITE_INFO"] = config.SiteInfo;
                parameterDictionary["@COL_STATUS"] = config.Status;
                parameterDictionary["@COL_VERSION"] = config.Version;
                parameterDictionary["@COL_JOB_ID"] = config.JobId;
                parameterDictionary["@COL_ARCHIVE_TIME"] = config.ArchiveTime;
                IndexProcessor.Execute(sql, parameterDictionary);
            }
            else
            {
                config.Guid = Guid.NewGuid().ToString();
                IndexProcessor.Insert<ArchiverSiteConfigurationIndex>(config);
            }
        }

        public string LoadSiteInfo(string jobId)
        {
            String sql = "select * from " + IndexConstants.TableNameArchiveSiteConfiguration + " where COL_JOB_ID = @COL_JOBID order by COL_ARCHIVE_TIME desc";
            Dictionary<string, object> parameterDictionary = new Dictionary<string, object>();
            parameterDictionary["@COL_JOBID"] = jobId;
            List<ArchiverSiteConfigurationIndex> indexList = IndexProcessor.ExecuteQuery<ArchiverSiteConfigurationIndex>(sql, parameterDictionary);
            if (indexList.Count > 0)
            {
                return indexList[0].SiteInfo;
            }
            return null;
        }
    }
}