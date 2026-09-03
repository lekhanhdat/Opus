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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Dao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Dao;
using AvePoint.GCommon.Utility;
using System.Data.Common;
using AvePoint.RA.Common;

namespace AvePoint.RA.SharePoint.Archiver.Common
{
    public class ArchiverJobManagementService
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private IArchiverSiteMasterIndexDao _archiverSiteMasterIndexDao;
        protected IArchiverSiteMasterIndexDao ArchiverSiteMasterIndexDao
        {
            get
            {
                if (_archiverSiteMasterIndexDao == null)
                {
                    _archiverSiteMasterIndexDao = new ArchiverSiteMasterIndexDao();
                }
                return _archiverSiteMasterIndexDao;
            }
        }

        private RMRemoteNodeDao _remoteNodeDao;
        protected RMRemoteNodeDao RemoteNodeDao
        {
            get
            {
                if (_remoteNodeDao == null)
                {
                    _remoteNodeDao = new RMRemoteNodeDao();
                }
                return _remoteNodeDao;
            }
        }

        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        public async Task UpdateSiteCollectionAfterAchiveredAsync(string scUrl, bool isArchivered, string groupId)
        {
            try
            {
                logger.Info("Update site collection {0} to archivered state {1}.", scUrl, isArchivered);
                ArchiverSiteMasterIndexExtension extention = new ArchiverSiteMasterIndexExtension();
                extention.IsSiteCollectionArchivered = isArchivered;
                extention.UpdateTime = DateTime.UtcNow.Ticks;
                var extentionStr = SerializerHelper.SerializeByDataContractSerializer(extention);
                var indexs = await ArchiverSiteMasterIndexDao.FindListAsync(i => i.SiteURL == scUrl);
                foreach (var index in indexs) { index.Extension = extentionStr; }
                ArchiverSiteMasterIndexDao.BatchUpdate(indexs);

                if (isArchivered)
                {
                    logger.Info("Update site collection state to access none.");
                    var site = RemoteNodeDao.Find(n => n.Url.Equals(scUrl, StringComparison.OrdinalIgnoreCase));
                    site.State = (int)GCommon.Contract.Server.ControlPanel.Office365.SiteCollectionState.AccessNone;
                    await RemoteNodeDao.UpdateAsync(site);
                    //CheckUpdateSiteCollectionState(tenantDB, SiteCollectionState.AccessNone, scUrl);
                    logger.Info("Get ids and plan ids by site url from SORuleNode.");
                    List<string> ids = new List<string>();
                    List<string> plans = new List<string>();

//Need todo
                    //using (var transaction = new DBTransaction(tenantDB))
                    //{
                    //    string queryText = "select s.Id, s.PlanId from {0}.SORuleNode as s where SiteUrl = @siteUrl";
                    //    DbParameter[] paras = new DbParameter[]
                    //    {
                    //        new SqlParameter(){ParameterName = "@siteUrl", Value = scUrl},
                    //    };
                    //    using (var reader = transaction.ExecuteReader(queryText, paras))
                    //    {
                    //        while (reader.Read())
                    //        {
                    //            ids.Add(reader[0].ToString());
                    //            plans.Add(reader[1].ToString());
                    //        }
                    //    }
                    //    transaction.Commit();
                    //}
                    //if (ids != null && ids.Count > 0)
                    //{
                    //    logger.Info("Get id by site url from SORuleNode successful, start deleting related rule alliance by nodeId...");
                    //    using (var dbTransation = new DBTransaction(tenantDB))
                    //    {
                    //        string queryText = "delete from {0}.SORuleAlliance where NodeId in (" + DBUtil.BuildInClause<string>(ids.ToArray()) + ")";
                    //        dbTransation.ExecuteNonQuery(queryText);
                    //        dbTransation.Commit();
                    //    }
                    //}
                    //if (plans != null && plans.Count > 0)
                    //{
                    //    logger.Info("Get plan id by site url from SORuleNode successful, start deleting related plan by planId...");
                    //    using (var dbTransation = new DBTransaction(tenantDB))
                    //    {
                    //        string queryText = "delete from {0}.[Plan] where Id in (" + DBUtil.BuildInClause<string>(plans.ToArray()) + ")";
                    //        dbTransation.ExecuteNonQuery(queryText);
                    //        dbTransation.Commit();
                    //    }
                    //}
                    //logger.Info("Delete related rule node by siteUrl...");
                    //using (var dbTransation = new DBTransaction(tenantDB))
                    //{
                    //    string queryText = "delete from {0}.SORuleNode where SiteUrl = @siteUrl";
                    //    DbParameter[] paras = new DbParameter[]
                    //            {
                    //                new  SqlParameter("@siteUrl",scUrl),
                    //            };
                    //    dbTransation.ExecuteNonQuery(queryText, paras);
                    //    dbTransation.Commit();
                    //}
                }
            }
            catch (Exception ex)
            {
                logger.Error("Update site collection state failed: {0}", ex.ToString());
                throw;
            }
        }

        public async Task UpdateSiteCollectionAfterAchiveredAsync(string scUrl, bool isArchivered, string groupId, string jobId)
        {
            try
            {
                logger.Info("Update site collection {0} to archivered state {1}. jobId :{2}", scUrl, isArchivered, jobId);
                ArchiverSiteMasterIndexExtension extention = new ArchiverSiteMasterIndexExtension();
                extention.IsSiteCollectionArchivered = isArchivered;
                extention.UpdateTime = DateTime.UtcNow.Ticks;
                var extentionStr = SerializerHelper.SerializeByDataContractSerializer(extention);
                int rowsCount = 0;
                if (!string.IsNullOrEmpty(jobId))
                {
                    var indexs = await ArchiverSiteMasterIndexDao.FindListAsync(i => i.SiteURL.Equals(scUrl, StringComparison.OrdinalIgnoreCase) && i.JobId.Equals(jobId, StringComparison.OrdinalIgnoreCase));
                    rowsCount = indexs.Count();
                    foreach (var index in indexs) { index.Extension = extentionStr; }
                    ArchiverSiteMasterIndexDao.BatchUpdate(indexs);
                }
                if (rowsCount <= 0)
                {
                    logger.Info("Update rows less 0, will try to update all the records of {0}", scUrl);
                    var indexs = await ArchiverSiteMasterIndexDao.FindListAsync(i => i.SiteURL.Equals(scUrl, StringComparison.OrdinalIgnoreCase));
                    rowsCount = indexs.Count();
                    foreach (var index in indexs) { index.Extension = extentionStr; }
                    ArchiverSiteMasterIndexDao.BatchUpdate(indexs);
                }
                else
                {
                    logger.Info("Update row count :{0}", rowsCount);
                }
                if (isArchivered)
                {
                    logger.Info("Update site collection state to access none.");
                    var site = RemoteNodeDao.Find(n => n.Url.Equals(scUrl, StringComparison.OrdinalIgnoreCase));
                    site.State = (int)GCommon.Contract.Server.ControlPanel.Office365.SiteCollectionState.AccessNone;
                    await RemoteNodeDao.UpdateAsync(site);
                    //CheckUpdateSiteCollectionState(tenantDB, SiteCollectionState.AccessNone, scUrl);
                    logger.Info("Get ids and plan ids by site url from SORuleNode.");
                    List<string> ids = new List<string>();
                    List<string> plans = new List<string>();

                    //Need TODO
                    //using (var transaction = new DBTransaction(tenantDB))
                    //{
                    //    string queryText = "select s.Id, s.PlanId from {0}.SORuleNode as s where SiteUrl = @siteUrl";
                    //    DbParameter[] paras = new DbParameter[]
                    //    {
                    //        new SqlParameter(){ParameterName = "@siteUrl", Value = scUrl},
                    //    };
                    //    using (var reader = transaction.ExecuteReader(queryText, paras))
                    //    {
                    //        while (reader.Read())
                    //        {
                    //            ids.Add(reader[0].ToString());
                    //            plans.Add(reader[1].ToString());
                    //        }
                    //    }
                    //    transaction.Commit();
                    //}
                    //if (ids != null && ids.Count > 0)
                    //{
                    //    logger.Info("Get id by site url from SORuleNode successful, start deleting related rule alliance by nodeId...");
                    //    using (var dbTransation = new DBTransaction(tenantDB))
                    //    {
                    //        string queryText = "delete from {0}.SORuleAlliance where NodeId in (" + DBUtil.BuildInClause<string>(ids.ToArray()) + ")";
                    //        dbTransation.ExecuteNonQuery(queryText);
                    //        dbTransation.Commit();
                    //    }
                    //}
                    //if (plans != null && plans.Count > 0)
                    //{
                    //    logger.Info("Get plan id by site url from SORuleNode successful, start deleting related plan by planId...");
                    //    using (var dbTransation = new DBTransaction(tenantDB))
                    //    {
                    //        string queryText = "delete from {0}.[Plan] where Id in (" + DBUtil.BuildInClause<string>(plans.ToArray()) + ")";
                    //        dbTransation.ExecuteNonQuery(queryText);
                    //        dbTransation.Commit();
                    //    }
                    //}
                    //logger.Info("Delete related rule node by siteUrl...");
                    //using (var dbTransation = new DBTransaction(tenantDB))
                    //{
                    //    string queryText = "delete from {0}.SORuleNode where SiteUrl = @siteUrl";
                    //    DbParameter[] paras = new DbParameter[]
                    //            {
                    //                new  SqlParameter("@siteUrl",scUrl),
                    //            };
                    //    dbTransation.ExecuteNonQuery(queryText, paras);
                    //    dbTransation.Commit();
                    //}
                }
            }
            catch (Exception ex)
            {
                logger.Error("Update site collection state failed: {0}", ex.ToString());
                throw;
            }
        }

        public bool EnableFixFullPathForCGScan()
        {
            var key = RMKeyValueDao.GetValueByKey("EnableFixFullPathForCGScan");
            bool.TryParse(key?.Value, out bool result);
            return result;
        }

    }
}
