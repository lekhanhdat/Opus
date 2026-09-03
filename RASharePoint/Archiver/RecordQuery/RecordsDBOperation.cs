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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.SharePointBrowser;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.DBLocker;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RAPhysical.API;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ColumnType = AvePoint.RA.Contract.TemplateManagement.ColumnType;

namespace AvePoint.PhysicalCore.SQL
{
    public static class RecordsDBOperation
    {
        private static readonly AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
 

        private static IRMLockDao mRecordLock { get; set; }
        public static IRMLockDao RecordLock
        {
            get
            {
                if (mRecordLock == null)
                {
                    mRecordLock = PlatformWindsorManager.GetService(typeof(IRMLockDao)) as IRMLockDao;
                    return mRecordLock;
                }
                else
                {
                    return mRecordLock;
                }
            }
        }


        private static List<RMEXOLabel> rRMEXOLabels = null;

        public static List<RMEXOLabel> RMEXOLabels
        {
            get
            {
                if (rRMEXOLabels == null)
                {
                    rRMEXOLabels = GetAllRMEXOLabels();
                    return rRMEXOLabels;
                }
                else
                {
                    return rRMEXOLabels;
                }
            }
        }

        private static IRMDeclaredSettingLockDao mRecordSettingLock { get; set; }
        public static IRMDeclaredSettingLockDao RecordSettingLock
        {
            get
            {
                if (mRecordSettingLock == null)
                {
                    mRecordSettingLock = PlatformWindsorManager.GetService(typeof(IRMDeclaredSettingLockDao)) as IRMDeclaredSettingLockDao;
                    return mRecordSettingLock;
                }
                else
                {
                    return mRecordSettingLock;
                }
            }
        }

        private static IRMRemoteNodeDao mRMRemoteNodeDao;
        public static IRMRemoteNodeDao RMRemoteNodeDao
        {
            get
            {
                if (mRMRemoteNodeDao == null)
                {
                    mRMRemoteNodeDao = (IRMRemoteNodeDao)PlatformWindsorManager.GetService(typeof(IRMRemoteNodeDao));
                }
                return mRMRemoteNodeDao;
            }
        }

        private static IRMEXOLabelDao mEXOLabelDao;
        public static IRMEXOLabelDao EXOLabelDao
        {
            get
            {
                if (mEXOLabelDao == null)
                {
                    mEXOLabelDao = (IRMEXOLabelDao)PlatformWindsorManager.GetService(typeof(IRMEXOLabelDao));
                }
                return mEXOLabelDao;
            }
        }

        private static IRecordLoanAllianceDao mRecordLoanAllianceDao;
        public static IRecordLoanAllianceDao RecordLoanAllianceDao
        {
            get
            {
                if (mRecordLoanAllianceDao == null)
                {
                    mRecordLoanAllianceDao = (IRecordLoanAllianceDao)PlatformWindsorManager.GetService(typeof(IRecordLoanAllianceDao));
                }
                return mRecordLoanAllianceDao;
            }
        }

        public static void Initialize(string connectString, string tenantId, int timeout = 120000)
        {
        }

        public static Task<bool> GetLockerAsync(string lockKey)
        {
            return RMDBlLocker.GetRecordsLockerAsync(lockKey);
        }

        public static Task ReleaseLockerAsync(string lockKey, Guid lockerID)
        {
            return RMDBlLocker.ReleaseRecordsLockerAsync(lockKey);
        }

        public static void GetLockerStatus()
        {
            //need a object, include key, lockid, etc all field value.
        }

        /// <summary>
        /// The table is obsolete
        /// </summary>
        /// <param name="sourceRecordsId"></param>
        /// <param name="desRecordsId"></param>
        public static void UpdateRMRecordAlliancesTableRecordsId(Guid sourceRecordsId, Guid desRecordsId)
        {
            //mLog.Info("UpdateRMRecordAlliancesTableRecordsId.sourceRecordsId:{0}.desRecordsId:{1}. ", sourceRecordsId, desRecordsId);
            //using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
            //{
            //    connection.Open();
            //    using (var command = connection.CreateCommand())
            //    {
            //        Guid id = Guid.NewGuid();
            //        command.CommandText = string.Format(RecordQueryString.UpdateRMRecordAlliancesTableRecordsId, RMRecordAlliancesTableName);
            //        command.Parameters.AddWithValue("@sourceRecordsId", sourceRecordsId);
            //        command.Parameters.AddWithValue("@desRecordsId", desRecordsId);
            //        command.ExecuteNonQuery();
            //    }
            //}
        }

        /// <summary>
        /// The table is obsolete
        /// </summary>
        /// <param name="sourceRecordsId"></param>
        /// <param name="desRecordsId"></param>
        public static void ApplySourceHoldInfoForDestination(Guid sourceRecordsId, Guid desRecordsId)
        {
            //mLog.Info("ApplySourceHoldInfoForDestination.sourceRecordsId:{0}.desRecordsId:{1}. ", sourceRecordsId, desRecordsId);
            //using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
            //{
            //    connection.Open();
            //    using (var command = connection.CreateCommand())
            //    {
            //        Guid id = Guid.NewGuid();
            //        command.CommandText = string.Format(RecordQueryString.ApplySourceHoldInfoForDestination, RMRecordAlliancesTableName);
            //        command.Parameters.AddWithValue("@sourceRecordsId", sourceRecordsId);
            //        command.Parameters.AddWithValue("@desRecordsId", desRecordsId);
            //        command.ExecuteNonQuery();
            //    }
            //}
        }

        //public static string GetClassificationColumnNameFromRMSharePointSettingsTable(string groupID)
        //{
        //    string bcsColumnName = string.Empty;
        //    bool IsUsingExistColumnName = false;
        //    string existColumnName = string.Empty;
        //    mLog.Info("GetClassificationColumnNameFromRMSharePointSettingsTable.groupID:{0}. ", groupID);
        //    try
        //    {
        //        using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //        {
        //            connection.Open();
        //            using (var command = connection.CreateCommand())
        //            {
        //                Guid id = Guid.NewGuid();
        //                command.CommandText = string.Format(RecordQueryString.GetRMSharePointSettingsInfoByScopeId, RMSharePointSettingsTableName);
        //                command.Parameters.AddWithValue("@ScopeId", groupID);
        //                using (SqlDataReader sdr = command.ExecuteReader())
        //                {
        //                    if (sdr.Read())
        //                    {
        //                        if (!sdr.IsDBNull(0))
        //                        {
        //                            bcsColumnName = sdr.GetString(0);
        //                        }
        //                        if (!sdr.IsDBNull(1))
        //                        {
        //                            existColumnName = sdr.GetString(1);
        //                        }
        //                        if (!sdr.IsDBNull(2))
        //                        {
        //                            IsUsingExistColumnName = sdr.GetBoolean(2);
        //                        }
        //                        if (IsUsingExistColumnName)
        //                        {
        //                            bcsColumnName = existColumnName;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        mLog.Info(string.Format("Can not GetClassificationColumnNameFromRMSharePointSettingsTable.groupID :{0}", groupID));
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        mLog.Info("Failed GetClassificationColumnNameFromRMSharePointSettingsTable.Message:{0}. ", ex.ToString());
        //    }
        //    return bcsColumnName;
        //}

        //public static System.Tuple<bool, bool, string> GetClassificationColumnNameFromRMSharePointSettingsTableBySiteUrl(string siteUrl)
        //{
        //    string bcsColumnName = string.Empty;
        //    bool IsUsingExistColumnName = false;
        //    bool exist = false;
        //    string existColumnName = string.Empty;
        //    mLog.Info("GetClassificationColumnNameFromRMSharePointSettingsTableBySiteUrl.siteUrl:{0}. ", siteUrl);
        //    try
        //    {
        //        using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //        {
        //            connection.Open();
        //            using (var command = connection.CreateCommand())
        //            {
        //                Guid id = Guid.NewGuid();
        //                command.CommandText = string.Format(RecordQueryString.GetRMSharePointSettingsInfoBySiteUrl, RMSharePointSettingsTableName, RMRemoteNodesTableName);
        //                command.Parameters.AddWithValue("@SiteUrl", siteUrl);
        //                using (SqlDataReader sdr = command.ExecuteReader())
        //                {
        //                    if (sdr.Read())
        //                    {
        //                        exist = true;
        //                        if (!sdr.IsDBNull(0))
        //                        {
        //                            bcsColumnName = sdr.GetString(0);
        //                        }
        //                        if (!sdr.IsDBNull(1))
        //                        {
        //                            existColumnName = sdr.GetString(1);
        //                        }
        //                        if (!sdr.IsDBNull(2))
        //                        {
        //                            IsUsingExistColumnName = sdr.GetBoolean(2);
        //                        }
        //                        if (IsUsingExistColumnName)
        //                        {
        //                            bcsColumnName = existColumnName;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        mLog.Info(string.Format("Can not GetClassificationColumnNameFromRMSharePointSettingsTableBySiteUrl.siteUrl :{0}", siteUrl));
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        mLog.Info("Failed GetClassificationColumnNameFromRMSharePointSettingsTable.Message:{0}. ", ex.ToString());
        //    }
        //    return new System.Tuple<bool, bool, string>(exist, IsUsingExistColumnName, bcsColumnName);
        //}

        //public static RemoteSiteCollection GetSiteFromRecords(string siteUrl)
        //{
        //    mLog.Info("GetSiteFromRecords.siteUrl:{0}. ", siteUrl);
        //    var site = RMRemoteNodeDao.GetRemoteSiteCollectionByUrl(siteUrl);
        //    return site;
        //    RemoteSiteCollection remoteSiteCollection = null;
        //    try
        //    {
        //        using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //        {
        //            connection.Open();
        //            using (var command = connection.CreateCommand())
        //            {
        //                Guid id = Guid.NewGuid();
        //                command.CommandText = string.Format(RecordQueryString.GetRemoteSiteCollectionBySiteUrl, RMRemoteNodesTableName);
        //                command.Parameters.AddWithValue("@SiteUrl", siteUrl);
        //                using (SqlDataReader reader = command.ExecuteReader())
        //                {
        //                    if (reader.Read())
        //                    {
        //                        remoteSiteCollection = new RemoteSiteCollection();
        //                        remoteSiteCollection.id = reader[0].ToString();
        //                        remoteSiteCollection.domain = reader[1].ToString();
        //                        remoteSiteCollection.username = reader[2].ToString();
        //                        remoteSiteCollection.url = reader[4].ToString();
        //                        remoteSiteCollection.parentId = reader[5].ToString();
        //                        remoteSiteCollection.state = (SiteCollectionState)int.Parse(reader[6].ToString());
        //                        remoteSiteCollection.TenantGroupId = reader[7].ToString();
        //                        //remoteSiteCollection.AgentGroupName = reader[8].ToString();
        //                        //remoteSiteCollection.Description = reader[9].ToString();
        //                        //remoteSiteCollection.ModifiedDate = reader[10].ToString();
        //                        //remoteSiteCollection.BposMode = reader[11].ToString();
        //                        remoteSiteCollection.CreateTime = long.Parse(reader[12].ToString());
        //                        remoteSiteCollection.TemplateName = reader[13].ToString();
        //                        remoteSiteCollection.SPVersion = reader[14].ToString();
        //                        remoteSiteCollection.NodeType = (RemoveNodeType)ConvertNodeLevelToType(int.Parse(reader[15].ToString()));
        //                        remoteSiteCollection.Name = reader[16].ToString();
        //                        //remoteSiteCollection.DisplayName = reader[17].ToString();
        //                        //remoteSiteCollection.AvailableAgentIds = reader[18].ToString();
        //                        remoteSiteCollection.TemplateTitle = reader[19].ToString();
        //                        remoteSiteCollection.IsPublicWebSite = bool.Parse(reader[20].ToString());
        //                        remoteSiteCollection.SiteCollectionType = (SiteCollectionType)int.Parse(reader[21].ToString());
        //                        remoteSiteCollection.AdminUrl = reader[22].ToString();
        //                        remoteSiteCollection.ServiceAccountId = reader[23].ToString();
        //                        remoteSiteCollection.TenantId = reader[24].ToString();
        //                        remoteSiteCollection.AuthType = (BposConnectionType)int.Parse(reader[25].ToString());
        //                        remoteSiteCollection.AppType = (AppType)int.Parse(reader[26].ToString());
        //                        remoteSiteCollection.ScanSource = (RemoteNodeScanSource)int.Parse(reader[27].ToString());
        //                        remoteSiteCollection.TeamId = reader[28].ToString();
        //                    }
        //                    else
        //                    {
        //                        mLog.Info(string.Format("Can not GetSiteFromRecords.siteUrl :{0}", siteUrl));
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        mLog.Info("Failed GetSiteFromRecords.Message:{0}. ", ex.ToString());
        //    }
        //    return remoteSiteCollection;
        //}

        //private static RemoveNodeType ConvertNodeLevelToType(int level)
        //{
        //    switch (level)
        //    {
        //        case (int)NodeLevel.WebApplication:
        //            return RemoveNodeType.SiteCollection;
        //        case (int)NodeLevel.SkyDriveProGroup:
        //        case (int)NodeLevel.SkyDrivePro:
        //            return RemoveNodeType.SkyDrivePro;
        //        case (int)NodeLevel.O365GroupSitesGroup:
        //            return RemoveNodeType.O365GroupSites;
        //        case (int)NodeLevel.PrivateChannelGroup:
        //            return RemoveNodeType.PrivateChannel;
        //        default:
        //            return RemoveNodeType.SiteCollection;
        //    }
        //}

        //public static bool GetPhysicalScheduleByLocationId(string UniqueIdPath)
        //{
        //    bool hasBreakInheritNode = false;
        //    mLog.Info("GetPhysicalScheduleByLocationId.UniqueId:{0}.", UniqueIdPath);
        //    try
        //    {
        //        using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //        {
        //            connection.Open();
        //            using (var command = connection.CreateCommand())
        //            {
        //                command.CommandText = string.Format(RecordQueryString.GetPhysicalScheduleByLocationId, RMSchedulesTableName);
        //                command.Parameters.AddWithValue("@ProfileId", UniqueIdPath);
        //                command.Parameters.AddWithValue("@IsRemoved", false);
        //                using (SqlDataReader sdr = command.ExecuteReader())
        //                {
        //                    if (sdr.Read())
        //                    {
        //                        if (!sdr.IsDBNull(0))
        //                        {
        //                            hasBreakInheritNode = sdr.GetInt32(0) >= 1;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        mLog.Info(string.Format("Can not GetPhysicalScheduleByLocationId.UniqueId :{0}", UniqueIdPath));
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        mLog.Info("Failed GetPhysicalScheduleByLocationId.Message:{0}. ", ex.ToString());
        //    }
        //    return hasBreakInheritNode;
        //}

        //public static bool IsRecordsHold(List<Guid> ids, long ticks)
        //{
        //    bool IsRecordsHold = false;
        //    mLog.Info("IsRecordsHold.");
        //    try
        //    {
        //        List<RMRecordAlliance> rMRecordAlliances = GetAllRMRecordAlliance();              
        //        int disposalCount = rMRecordAlliances.Count(a => a.HoldReleaseTime > ticks && ids.Any(temp => temp == a.RecordsId));
        //        if (disposalCount > 0)
        //        {
        //            return true;
        //        }
        //        List<RMRecordLoanAlliance> loanAlliances = GetPhyRecordAllianceByIds(ids);
        //        int loanCount = loanAlliances.Count;
        //        return loanCount > 0;
        //    }
        //    catch (Exception ex)
        //    {
        //        mLog.Info("Failed IsRecordsHold.Message:{0}. ", ex.ToString());
        //    }
        //    return IsRecordsHold;
        //}

        public static bool IsRecordsHold(IPhysicalFile file, long ticks)
        {
            bool IsRecordsHold = false;
            mLog.Info("IsRecordsHold.");
            try
            {
                //List<RMRecordAlliance> rMRecordAlliances = GetAllRMRecordAlliance();
                //int disposalCount = rMRecordAlliances.Count(a => a.HoldReleaseTime > ticks && ids.Any(temp => temp == a.RecordsId));
                if (file.HoldStatus && file.HoldReleaseTime > ticks
                    || (file.ParentBox != null && file.ParentBox.HoldStatus && file.ParentBox.HoldReleaseTime > ticks))
                {
                    return true;
                }
                List<Guid> ids = new List<Guid>();
                ids.Add(file.Id);
                if (file.ParentBox != null)
                {
                    ids.Add(file.ParentBox.Id);
                }
                List<RMRecordLoanAlliance> loanAlliances = GetPhyRecordAllianceByIds(ids);
                int loanCount = loanAlliances.Count;
                return loanCount > 0;
            }
            catch (Exception ex)
            {
                mLog.Info("Failed IsRecordsHold.Message:{0}. ", ex.ToString());
            }
            return IsRecordsHold;
        }
        //public static bool IsRecordsHold(IPhysicalBox box, long ticks)
        //{
        //    bool IsRecordsHold = false;
        //    mLog.Info("IsRecordsHold.");
        //    try
        //    {
        //        //List<RMRecordAlliance> rMRecordAlliances = GetAllRMRecordAlliance();
        //        //int disposalCount = rMRecordAlliances.Count(a => a.HoldReleaseTime > ticks && ids.Any(temp => temp == a.RecordsId));
        //        if (box.HoldStatus && box.HoldReleaseTime > ticks)
        //        {
        //            return true;
        //        }
        //        List<Guid> ids = new List<Guid>() { box.Id };
        //        List<RMRecordLoanAlliance> loanAlliances = GetPhyRecordAllianceByIds(ids);
        //        int loanCount = loanAlliances.Count;
        //        return loanCount > 0;
        //    }
        //    catch (Exception ex)
        //    {
        //        mLog.Info("Failed IsRecordsHold.Message:{0}. ", ex.ToString());
        //    }
        //    return IsRecordsHold;
        //}


        //public static int CountSubLocation(int locationID)
        //{
        //    mLog.Info("CountSubLocation.locationID:{0}.", locationID);
        //    int subLocationCount = 0;
        //    try
        //    {
        //        using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //        {
        //            connection.Open();
        //            using (var command = connection.CreateCommand())
        //            {
        //                command.CommandText = string.Format(RecordQueryString.CountSubLocation, RMLocationTableName);
        //                command.Parameters.AddWithValue("@ParentId", locationID);
        //                command.Parameters.AddWithValue("@IsRemoved", false);
        //                using (SqlDataReader sdr = command.ExecuteReader())
        //                {
        //                    if (sdr.Read())
        //                    {
        //                        if (!sdr.IsDBNull(0))
        //                        {
        //                            subLocationCount = sdr.GetInt32(0);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        mLog.Info(string.Format("Can not CountSubLocation.locationID :{0}", locationID));
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        mLog.Info("Failed CountSubLocation.Message:{0}. ", ex.ToString());
        //    }
        //    return subLocationCount;
        //}

        //public static List<RMLocation> GetAllSubLocationByParentId(int parentId)
        //{
        //    mLog.Info("GetAllSubLocationByParentId.parentId:{0}.", parentId);
        //    List<RMLocation> subLocations = new List<RMLocation>();
        //    try
        //    {
        //        using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //        {
        //            connection.Open();
        //            using (var command = connection.CreateCommand())
        //            {
        //                command.CommandText = string.Format(RecordQueryString.GetAllSubLocationByParentId, RMLocationTableName);
        //                command.Parameters.AddWithValue("@ParentId", parentId);
        //                command.Parameters.AddWithValue("@IsRemoved", false);
        //                using (SqlDataReader sdr = command.ExecuteReader())
        //                {
        //                    while (sdr.Read())
        //                    {
        //                        RMLocation location = new RMLocation();
        //                        if (!sdr.IsDBNull(0))
        //                        {
        //                            location.Id = sdr.GetInt32(0);
        //                        }
        //                        if (!sdr.IsDBNull(1))
        //                        {
        //                            location.UniqueId = sdr.GetGuid(1);
        //                        }
        //                        if (!sdr.IsDBNull(2))
        //                        {
        //                            location.ParentId = sdr.GetInt32(2);
        //                        }
        //                        if (!sdr.IsDBNull(3))
        //                        {
        //                            location.Name = sdr.GetString(3);
        //                        }
        //                        if (!sdr.IsDBNull(4))
        //                        {
        //                            location.Description = sdr.GetString(4);
        //                        }
        //                        if (!sdr.IsDBNull(5))
        //                        {
        //                            location.NodeType = sdr.GetInt32(5);
        //                        }
        //                        if (!sdr.IsDBNull(6))
        //                        {
        //                            location.IsRemoved = sdr.GetBoolean(6);
        //                        }
        //                        if (!sdr.IsDBNull(7))
        //                        {
        //                            location.AvailableSpace = sdr.GetDouble(7);
        //                        }
        //                        if (!sdr.IsDBNull(8))
        //                        {
        //                            location.DirPath = sdr.GetString(8);
        //                        }
        //                        if (!sdr.IsDBNull(9))
        //                        {
        //                            location.MetaInfo = sdr.GetString(9);
        //                        }
        //                        if (!sdr.IsDBNull(10))
        //                        {
        //                            location.CreatedUserId = sdr.GetString(10);
        //                        }
        //                        if (!sdr.IsDBNull(11))
        //                        {
        //                            location.CreatedTime = sdr.GetInt64(11);
        //                        }
        //                        if (!sdr.IsDBNull(12))
        //                        {
        //                            location.ModifiedUserId = sdr.GetString(12);
        //                        }
        //                        if (!sdr.IsDBNull(13))
        //                        {
        //                            location.ModifiedTime = sdr.GetInt64(13);
        //                        }
        //                        subLocations.Add(location);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        mLog.Info("Failed CountSubLocation.Message:{0}. ", ex.ToString());
        //    }
        //    if (subLocations != null)
        //    {
        //        foreach (var tempSubLocation in subLocations)
        //        {
        //            if (Convert.ToInt32(tempSubLocation.AvailableSpace) != 0)
        //            {
        //                tempSubLocation.AvailableSpace = Math.Round(tempSubLocation.AvailableSpace, 2);
        //            }
        //            tempSubLocation.SubLocationCount = CountSubLocation(tempSubLocation.Id);
        //        }
        //    }
        //    return subLocations;
        //}

        //public static List<RMLocation> GetAllRMLocations()
        //{
        //    mLog.Info("GetAllLocations.");
        //    List<RMLocation> rMLocations = new List<RMLocation>();
        //    try
        //    {
        //        using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //        {
        //            connection.Open();
        //            using (var command = connection.CreateCommand())
        //            {
        //                command.CommandText = string.Format(RecordQueryString.GetAllLocations, RMLocationTableName);
        //                using (SqlDataReader sdr = command.ExecuteReader())
        //                {
        //                    while (sdr.Read())
        //                    {
        //                        RMLocation location = new RMLocation();
        //                        if (!sdr.IsDBNull(0))
        //                        {
        //                            location.Id = sdr.GetInt32(0);
        //                        }
        //                        if (!sdr.IsDBNull(1))
        //                        {
        //                            location.UniqueId = sdr.GetGuid(1);
        //                        }
        //                        if (!sdr.IsDBNull(2))
        //                        {
        //                            location.ParentId = sdr.GetInt32(2);
        //                        }
        //                        if (!sdr.IsDBNull(3))
        //                        {
        //                            location.Name = sdr.GetString(3);
        //                        }
        //                        if (!sdr.IsDBNull(4))
        //                        {
        //                            location.Description = sdr.GetString(4);
        //                        }
        //                        if (!sdr.IsDBNull(5))
        //                        {
        //                            location.NodeType = sdr.GetInt32(5);
        //                        }
        //                        if (!sdr.IsDBNull(6))
        //                        {
        //                            location.IsRemoved = sdr.GetBoolean(6);
        //                        }
        //                        if (!sdr.IsDBNull(7))
        //                        {
        //                            location.AvailableSpace = sdr.GetDouble(7);
        //                        }
        //                        if (!sdr.IsDBNull(8))
        //                        {
        //                            location.DirPath = sdr.GetString(8);
        //                        }
        //                        if (!sdr.IsDBNull(9))
        //                        {
        //                            location.MetaInfo = sdr.GetString(9);
        //                        }
        //                        if (!sdr.IsDBNull(10))
        //                        {
        //                            location.CreatedUserId = sdr.GetString(10);
        //                        }
        //                        if (!sdr.IsDBNull(11))
        //                        {
        //                            location.CreatedTime = sdr.GetInt64(11);
        //                        }
        //                        if (!sdr.IsDBNull(12))
        //                        {
        //                            location.ModifiedUserId = sdr.GetString(12);
        //                        }
        //                        if (!sdr.IsDBNull(13))
        //                        {
        //                            location.ModifiedTime = sdr.GetInt64(13);
        //                        }
        //                        if (location != null && location.UniqueId != Guid.Empty)
        //                        {
        //                            location.PathForDisplay = GetLocationPath(location.DirPath);
        //                        }
        //                        rMLocations.Add(location);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        mLog.Info("Failed GetAllLocations.Message:{0}. ", ex.ToString());
        //    }
        //    return rMLocations;
        //}

        //public static List<RMScopePermission> GetAllRMScopePermissions()
        //{
        //    mLog.Info("GetAllRMScopePermissions.");
        //    List<RMScopePermission> rMScopePermissions = new List<RMScopePermission>();
        //    try
        //    {
        //        using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //        {
        //            connection.Open();
        //            using (var command = connection.CreateCommand())
        //            {
        //                command.CommandText = string.Format(RecordQueryString.GetAllRMScopePermissions, RMScopePermissionsTableName);
        //                using (SqlDataReader sdr = command.ExecuteReader())
        //                {
        //                    while (sdr.Read())
        //                    {
        //                        RMScopePermission scopePermission = new RMScopePermission();
        //                        if (!sdr.IsDBNull(0))
        //                        {
        //                            scopePermission.Id = sdr.GetInt32(0);
        //                        }
        //                        if (!sdr.IsDBNull(1))
        //                        {
        //                            scopePermission.Scope = sdr.GetString(1);
        //                        }
        //                        if (!sdr.IsDBNull(2))
        //                        {
        //                            scopePermission.ParentScope = sdr.GetString(2);
        //                        }
        //                        if (!sdr.IsDBNull(3))
        //                        {
        //                            scopePermission.ScopePath = sdr.GetString(3);
        //                        }
        //                        rMScopePermissions.Add(scopePermission);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        mLog.Info("Failed GetAllRMScopePermissions.Message:{0}. ", ex.ToString());
        //    }
        //    return rMScopePermissions;
        //}

        ////public static List<RMTemplateRelationship> GetAllRMTemplateRelationships()
        ////{
        ////    mLog.Info("GetAllRMTemplateRelationships.");
        ////    List<RMTemplateRelationship> rMScopePermissions = new List<RMTemplateRelationship>();
        ////    try
        ////    {
        ////        using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        ////        {
        ////            connection.Open();
        ////            using (var command = connection.CreateCommand())
        ////            {
        ////                command.CommandText = string.Format(RecordQueryString.GetAllRMTemplateRelationships, RMTemplateRelationshipTableName);
        ////                using (SqlDataReader sdr = command.ExecuteReader())
        ////                {
        ////                    while (sdr.Read())
        ////                    {
        ////                        RMTemplateRelationship scopePermission = new RMTemplateRelationship();
        ////                        if (!sdr.IsDBNull(0))
        ////                        {
        ////                            scopePermission.IdPath = sdr.GetString(0);
        ////                        }
        ////                        if (!sdr.IsDBNull(1))
        ////                        {
        ////                            scopePermission.Distance = sdr.GetInt32(1);
        ////                        }
        ////                        if (!sdr.IsDBNull(2))
        ////                        {
        ////                            scopePermission.Ancestor = sdr.GetGuid(2);
        ////                        }
        ////                        if (!sdr.IsDBNull(3))
        ////                        {
        ////                            scopePermission.Descendant = sdr.GetGuid(3);
        ////                        }
        ////                        if (!sdr.IsDBNull(4))
        ////                        {
        ////                            scopePermission.TemplateType = (TemplateType)sdr.GetInt32(4);
        ////                        }
        ////                        rMScopePermissions.Add(scopePermission);
        ////                    }
        ////                }
        ////            }
        ////        }
        ////    }
        ////    catch (Exception ex)
        ////    {
        ////        mLog.Info("Failed GetAllRMTemplateRelationships.Message:{0}. ", ex.ToString());
        ////    }
        ////    return rMScopePermissions;
        ////}


        //public static RMLocation GetLocationById(int id)
        //{
        //    mLog.Info("GetLocationById.id:{0}.", id);
        //    RMLocation location = new RMLocation();
        //    try
        //    {
        //        using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //        {
        //            connection.Open();
        //            using (var command = connection.CreateCommand())
        //            {
        //                command.CommandText = string.Format(RecordQueryString.GetLocationById, RMLocationTableName);
        //                command.Parameters.AddWithValue("@Id", id);
        //                command.Parameters.AddWithValue("@IsRemoved", false);
        //                using (SqlDataReader sdr = command.ExecuteReader())
        //                {
        //                    if (sdr.Read())
        //                    {
        //                        if (!sdr.IsDBNull(0))
        //                        {
        //                            location.Id = sdr.GetInt32(0);
        //                        }
        //                        if (!sdr.IsDBNull(1))
        //                        {
        //                            location.UniqueId = sdr.GetGuid(1);
        //                        }
        //                        if (!sdr.IsDBNull(2))
        //                        {
        //                            location.ParentId = sdr.GetInt32(2);
        //                        }
        //                        if (!sdr.IsDBNull(3))
        //                        {
        //                            location.Name = sdr.GetString(3);
        //                        }
        //                        if (!sdr.IsDBNull(4))
        //                        {
        //                            location.Description = sdr.GetString(4);
        //                        }
        //                        if (!sdr.IsDBNull(5))
        //                        {
        //                            location.NodeType = sdr.GetInt32(5);
        //                        }
        //                        if (!sdr.IsDBNull(6))
        //                        {
        //                            location.IsRemoved = sdr.GetBoolean(6);
        //                        }
        //                        if (!sdr.IsDBNull(7))
        //                        {
        //                            location.AvailableSpace = sdr.GetDouble(7);
        //                        }
        //                        if (!sdr.IsDBNull(8))
        //                        {
        //                            location.DirPath = sdr.GetString(8);
        //                        }
        //                        if (!sdr.IsDBNull(9))
        //                        {
        //                            location.MetaInfo = sdr.GetString(9);
        //                        }
        //                        if (!sdr.IsDBNull(10))
        //                        {
        //                            location.CreatedUserId = sdr.GetString(10);
        //                        }
        //                        if (!sdr.IsDBNull(11))
        //                        {
        //                            location.CreatedTime = sdr.GetInt64(11);
        //                        }
        //                        if (!sdr.IsDBNull(12))
        //                        {
        //                            location.ModifiedUserId = sdr.GetString(12);
        //                        }
        //                        if (!sdr.IsDBNull(13))
        //                        {
        //                            location.ModifiedTime = sdr.GetInt64(13);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        mLog.Info(string.Format("Can not GetLocationById.locationID :{0}", id));
        //                    }
        //                }
        //            }
        //        }
        //        if (location != null && location.UniqueId != Guid.Empty)
        //        {
        //            location.PathForDisplay = GetLocationPath(location.DirPath);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        mLog.Info("Failed GetLocationById.Message:{0}. ", ex.ToString());
        //    }
        //    return location;
        //}

        //public static List<RMLocation> GetChildsLocationById(int id)
        //{
        //    mLog.Info("GetChildsLocationById.id:{0}.", id);
        //    List<RMLocation> locations = new List<RMLocation>();
        //    try
        //    {
        //        using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //        {
        //            connection.Open();
        //            using (var command = connection.CreateCommand())
        //            {
        //                command.CommandText = string.Format(RecordQueryString.GetChildsLocationById, RMLocationTableName);
        //                command.Parameters.AddWithValue("@ParentId", id);
        //                command.Parameters.AddWithValue("@IsRemoved", false);
        //                using (SqlDataReader sdr = command.ExecuteReader())
        //                {
        //                    while (sdr.Read())
        //                    {
        //                        RMLocation location = new RMLocation();
        //                        if (!sdr.IsDBNull(0))
        //                        {
        //                            location.Id = sdr.GetInt32(0);
        //                        }
        //                        if (!sdr.IsDBNull(1))
        //                        {
        //                            location.UniqueId = sdr.GetGuid(1);
        //                        }
        //                        if (!sdr.IsDBNull(2))
        //                        {
        //                            location.ParentId = sdr.GetInt32(2);
        //                        }
        //                        if (!sdr.IsDBNull(3))
        //                        {
        //                            location.Name = sdr.GetString(3);
        //                        }
        //                        if (!sdr.IsDBNull(4))
        //                        {
        //                            location.Description = sdr.GetString(4);
        //                        }
        //                        if (!sdr.IsDBNull(5))
        //                        {
        //                            location.NodeType = sdr.GetInt32(5);
        //                        }
        //                        if (!sdr.IsDBNull(6))
        //                        {
        //                            location.IsRemoved = sdr.GetBoolean(6);
        //                        }
        //                        if (!sdr.IsDBNull(7))
        //                        {
        //                            location.AvailableSpace = sdr.GetDouble(7);
        //                        }
        //                        if (!sdr.IsDBNull(8))
        //                        {
        //                            location.DirPath = sdr.GetString(8);
        //                        }
        //                        if (!sdr.IsDBNull(9))
        //                        {
        //                            location.MetaInfo = sdr.GetString(9);
        //                        }
        //                        if (!sdr.IsDBNull(10))
        //                        {
        //                            location.CreatedUserId = sdr.GetString(10);
        //                        }
        //                        if (!sdr.IsDBNull(11))
        //                        {
        //                            location.CreatedTime = sdr.GetInt64(11);
        //                        }
        //                        if (!sdr.IsDBNull(12))
        //                        {
        //                            location.ModifiedUserId = sdr.GetString(12);
        //                        }
        //                        if (!sdr.IsDBNull(13))
        //                        {
        //                            location.ModifiedTime = sdr.GetInt64(13);
        //                        }
        //                        locations.Add(location);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        mLog.Info("Failed GetLocationById.Message:{0}. ", ex.ToString());
        //    }
        //    return locations;
        //}

        //public static RMLocation GetLocationByName(string name)
        //{
        //    mLog.Info("GetLocationByName.name:{0}.", name);
        //    RMLocation location = new RMLocation();
        //    try
        //    {
        //        using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //        {
        //            connection.Open();
        //            using (var command = connection.CreateCommand())
        //            {
        //                command.CommandText = string.Format(RecordQueryString.GetLocationByName, RMLocationTableName);
        //                command.Parameters.AddWithValue("@Name", name);
        //                command.Parameters.AddWithValue("@IsRemoved", false);
        //                using (SqlDataReader sdr = command.ExecuteReader())
        //                {
        //                    if (sdr.Read())
        //                    {
        //                        if (!sdr.IsDBNull(0))
        //                        {
        //                            location.Id = sdr.GetInt32(0);
        //                        }
        //                        if (!sdr.IsDBNull(1))
        //                        {
        //                            location.UniqueId = sdr.GetGuid(1);
        //                        }
        //                        if (!sdr.IsDBNull(2))
        //                        {
        //                            location.ParentId = sdr.GetInt32(2);
        //                        }
        //                        if (!sdr.IsDBNull(3))
        //                        {
        //                            location.Name = sdr.GetString(3);
        //                        }
        //                        if (!sdr.IsDBNull(4))
        //                        {
        //                            location.Description = sdr.GetString(4);
        //                        }
        //                        if (!sdr.IsDBNull(5))
        //                        {
        //                            location.NodeType = sdr.GetInt32(5);
        //                        }
        //                        if (!sdr.IsDBNull(6))
        //                        {
        //                            location.IsRemoved = sdr.GetBoolean(6);
        //                        }
        //                        if (!sdr.IsDBNull(7))
        //                        {
        //                            location.AvailableSpace = sdr.GetDouble(7);
        //                        }
        //                        if (!sdr.IsDBNull(8))
        //                        {
        //                            location.DirPath = sdr.GetString(8);
        //                        }
        //                        if (!sdr.IsDBNull(9))
        //                        {
        //                            location.MetaInfo = sdr.GetString(9);
        //                        }
        //                        if (!sdr.IsDBNull(10))
        //                        {
        //                            location.CreatedUserId = sdr.GetString(10);
        //                        }
        //                        if (!sdr.IsDBNull(11))
        //                        {
        //                            location.CreatedTime = sdr.GetInt64(11);
        //                        }
        //                        if (!sdr.IsDBNull(12))
        //                        {
        //                            location.ModifiedUserId = sdr.GetString(12);
        //                        }
        //                        if (!sdr.IsDBNull(13))
        //                        {
        //                            location.ModifiedTime = sdr.GetInt64(13);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        if (location != null && location.UniqueId != Guid.Empty)
        //        {
        //            location.PathForDisplay = GetLocationPath(location.DirPath);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        mLog.Info("Failed GetLocationById.Message:{0}. ", ex.ToString());
        //    }
        //    return location;
        //}

        //public static RMLocation GetLocationByUniqueId(Guid uniqueId)
        //{
        //    mLog.Info("GetLocationByUniqueId.uniqueId:{0}.", uniqueId);
        //    RMLocation mLocation = new RMLocation();
        //    using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //    {
        //        connection.Open();
        //        using (var command = connection.CreateCommand())
        //        {
        //            command.CommandText = string.Format(RecordQueryString.GetLocationByUniqueId, RMLocationTableName);
        //            command.Parameters.AddWithValue("@UniqueId", uniqueId);
        //            command.Parameters.AddWithValue("@IsRemoved", false);
        //            using (SqlDataReader sdr = command.ExecuteReader())
        //            {
        //                if (sdr.Read())
        //                {
        //                    if (!sdr.IsDBNull(0))
        //                    {
        //                        mLocation.Id = sdr.GetInt32(0);
        //                    }
        //                    if (!sdr.IsDBNull(1))
        //                    {
        //                        mLocation.UniqueId = sdr.GetGuid(1);
        //                    }
        //                    if (!sdr.IsDBNull(2))
        //                    {
        //                        mLocation.ParentId = sdr.GetInt32(2);
        //                    }
        //                    if (!sdr.IsDBNull(3))
        //                    {
        //                        mLocation.Name = sdr.GetString(3);
        //                    }
        //                    if (!sdr.IsDBNull(4))
        //                    {
        //                        mLocation.Description = sdr.GetString(4);
        //                    }
        //                    if (!sdr.IsDBNull(5))
        //                    {
        //                        mLocation.NodeType = sdr.GetInt32(5);
        //                    }
        //                    if (!sdr.IsDBNull(6))
        //                    {
        //                        mLocation.IsRemoved = sdr.GetBoolean(6);
        //                    }
        //                    if (!sdr.IsDBNull(7))
        //                    {
        //                        mLocation.AvailableSpace = sdr.GetDouble(7);
        //                    }
        //                    if (!sdr.IsDBNull(8))
        //                    {
        //                        mLocation.DirPath = sdr.GetString(8);
        //                    }
        //                    if (!sdr.IsDBNull(9))
        //                    {
        //                        mLocation.MetaInfo = sdr.GetString(9);
        //                    }
        //                    if (!sdr.IsDBNull(10))
        //                    {
        //                        mLocation.CreatedUserId = sdr.GetString(10);
        //                    }
        //                    if (!sdr.IsDBNull(11))
        //                    {
        //                        mLocation.CreatedTime = sdr.GetInt64(11);
        //                    }
        //                    if (!sdr.IsDBNull(12))
        //                    {
        //                        mLocation.ModifiedUserId = sdr.GetString(12);
        //                    }
        //                    if (!sdr.IsDBNull(13))
        //                    {
        //                        mLocation.ModifiedTime = sdr.GetInt64(13);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    if (mLocation != null && mLocation.UniqueId != Guid.Empty)
        //    {
        //        mLocation.PathForDisplay = GetLocationPath(mLocation.DirPath);
        //    }
        //    return mLocation;
        //}

        //public static RMLocation CreateLocation(string name, int parentId)
        //{
        //    mLog.Info("CreateLocation.name:{0}.", name);
        //    RMLocation pLocation = GetLocationById(parentId);
        //    if (pLocation == null || pLocation.IsRemoved)
        //    {
        //        throw new Exception("Parent location is invalied.");
        //    }
        //    RMLocation tempLocation = new RMLocation();
        //    tempLocation.UniqueId = Guid.NewGuid();
        //    tempLocation.ParentId = parentId;
        //    tempLocation.Name = name;
        //    tempLocation.NodeType = (int)PhysicalNodeLevel.PhysicalNormalLocation;
        //    tempLocation.DirPath = pLocation.DirPath + pLocation.Id.ToString() + "/";
        //    var createdTime = DateTime.UtcNow.Ticks;
        //    tempLocation.CreatedUserId = pLocation.CreatedUserId;
        //    tempLocation.CreatedTime = createdTime;
        //    tempLocation.ModifiedUserId = pLocation.ModifiedUserId;
        //    tempLocation.ModifiedTime = createdTime;
        //    lock (lockCreateLocation)
        //    {
        //        RMLocation tLocation = GetLocationByName(name);
        //        if (tLocation != null && tLocation.Name == name)
        //        {
        //            throw new Exception("Location has same name.");
        //        }
        //        using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //        {
        //            connection.Open();
        //            using (var command = connection.CreateCommand())
        //            {
        //                command.CommandText = string.Format(RecordQueryString.CreateLocation, RMLocationTableName);
        //                command.Parameters.AddWithValue("@UniqueId", tempLocation.UniqueId);
        //                command.Parameters.AddWithValue("@Name", tempLocation.Name);
        //                command.Parameters.AddWithValue("@Description", tempLocation.Description);
        //                command.Parameters.AddWithValue("@NodeType", tempLocation.NodeType);
        //                command.Parameters.AddWithValue("@IsRemoved", tempLocation.IsRemoved);
        //                command.Parameters.AddWithValue("@AvailableSpace", tempLocation.AvailableSpace);
        //                command.Parameters.AddWithValue("@DirPath", tempLocation.DirPath);
        //                command.Parameters.AddWithValue("@MetaInfo", tempLocation.MetaInfo);
        //                command.Parameters.AddWithValue("@CreatedUserId", tempLocation.CreatedUserId);
        //                command.Parameters.AddWithValue("@CreatedTime", tempLocation.CreatedTime);
        //                command.Parameters.AddWithValue("@ModifiedUserId", tempLocation.ModifiedUserId);
        //                command.Parameters.AddWithValue("@ParentId", tempLocation.ParentId);
        //                command.Parameters.AddWithValue("@ModifiedTime", tempLocation.ModifiedTime);
        //                command.ExecuteNonQuery();
        //            }
        //        }
        //    }
        //    return tempLocation;
        //}

        //public static bool DeleteLocation(int locationId)
        //{
        //    mLog.Info("DeleteLocation.locationId:{0}.", locationId);
        //    bool result = false;
        //    try
        //    {
        //        List<RMLocation> rMLocation = GetChildsLocationById(locationId);
        //        if (rMLocation != null && rMLocation.Count() > 0)
        //        {
        //            mLog.Warn("The location has children location associated, cannot be deleted now.");
        //        }
        //        else
        //        {
        //            RMLocation location = GetLocationById(locationId);
        //            if (location != null)
        //            {
        //                location.IsRemoved = true;
        //                UpdateLocation(location);
        //                result = true;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        mLog.Error($"Error in DeleteLocation, reason : {ex.ToString()}.");
        //        throw;
        //    }
        //    return result;
        //}

        public static bool DeleteSiteInRecords(string siteUrl)
        {
            mLog.Info("DeleteSiteInRecords.siteUrl:{0}.", siteUrl);
            bool result = false;
            try
            {
                RMRemoteNodeDao.DeleteRemoteSiteCollectionsByUrl(new List<string>() { siteUrl });
            }
            catch (Exception ex)
            {
                mLog.Error($"Error in DeleteSiteInRecords, reason : {ex.ToString()}.");
                throw;
            }
            return result;
        }

        //public static bool UpdateLocation(RMLocation entity)
        //{
        //    mLog.Info("UpdateLocation.locationId:{0}.", entity.UniqueId);
        //    bool updateSuccess = true;
        //    using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //    {
        //        connection.Open();
        //        using (var command = connection.CreateCommand())
        //        {
        //            command.CommandText = string.Format(RecordQueryString.UpdateLocation, RMLocationTableName);
        //            command.Parameters.AddWithValue("@UniqueId", entity.UniqueId);
        //            command.Parameters.AddWithValue("@Name", entity.Name);
        //            command.Parameters.AddWithValue("@Description", entity.Description);
        //            command.Parameters.AddWithValue("@NodeType", entity.NodeType);
        //            command.Parameters.AddWithValue("@IsRemoved", entity.IsRemoved);
        //            command.Parameters.AddWithValue("@AvailableSpace", entity.AvailableSpace);
        //            command.Parameters.AddWithValue("@DirPath", entity.DirPath);
        //            command.Parameters.AddWithValue("@MetaInfo", entity.MetaInfo);
        //            command.Parameters.AddWithValue("@CreatedUserId", entity.CreatedUserId);
        //            command.Parameters.AddWithValue("@CreatedTime", entity.CreatedTime);
        //            command.Parameters.AddWithValue("@ModifiedUserId", entity.ModifiedUserId);
        //            command.Parameters.AddWithValue("@ParentId", entity.ParentId);
        //            command.Parameters.AddWithValue("@ModifiedTime", entity.ModifiedTime);
        //            command.ExecuteNonQuery();
        //        }
        //    }
        //    return updateSuccess;
        //}

        //public static RMRule GetRuleById(Guid ruleId)
        //{
        //    mLog.Info("GetRuleById.ruleId:{0}.", ruleId);
        //    RMRule rule = new RMRule();
        //    using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //    {
        //        connection.Open();
        //        using (var command = connection.CreateCommand())
        //        {
        //            command.CommandText = string.Format(RecordQueryString.GetRMRuleById, RMRuleTableName);
        //            command.Parameters.AddWithValue("@RuleId", ruleId);
        //            using (SqlDataReader sdr = command.ExecuteReader())
        //            {
        //                if (sdr.Read())
        //                {
        //                    if (!sdr.IsDBNull(0))
        //                    {
        //                        rule.Id = sdr.GetInt32(0);
        //                    }
        //                    if (!sdr.IsDBNull(1))
        //                    {
        //                        rule.RuleId = sdr.GetGuid(1);
        //                    }
        //                    if (!sdr.IsDBNull(2))
        //                    {
        //                        rule.RuleName = sdr.GetString(2);
        //                    }
        //                    if (!sdr.IsDBNull(3))
        //                    {
        //                        rule.RuleLevel = sdr.GetInt32(3);
        //                    }
        //                    if (!sdr.IsDBNull(4))
        //                    {
        //                        rule.DisposalAction = sdr.GetInt32(4);
        //                    }
        //                    if (!sdr.IsDBNull(5))
        //                    {
        //                        rule.DeleteRecords = sdr.GetBoolean(5);
        //                    }
        //                    if (!sdr.IsDBNull(6))
        //                    {
        //                        rule.IsRemoved = sdr.GetBoolean(6);
        //                    }
        //                    if (!sdr.IsDBNull(7))
        //                    {
        //                        rule.Description = sdr.GetString(7);
        //                    }
        //                    if (!sdr.IsDBNull(8))
        //                    {
        //                        rule.ModifyTime = sdr.GetInt64(8);
        //                    }
        //                    if (!sdr.IsDBNull(9))
        //                    {
        //                        rule.ExchangeDisposalAction = sdr.GetInt32(9);
        //                    }
        //                    if (!sdr.IsDBNull(10))
        //                    {
        //                        rule.PhysicalDisposalAction = sdr.GetInt32(10);
        //                    }
        //                }
        //                else
        //                {
        //                    mLog.Info(string.Format("Can not GetRuleById.locationID :{0}", ruleId));
        //                }
        //            }
        //        }
        //    }
        //    return rule;
        //}

        //public static List<RMTemplateCategory> LoadCategories(Guid UniqueId)
        //{
        //    mLog.Info("LoadCategories.UniqueId:{0}.", UniqueId);
        //    List<RMTemplateCategory> categories = new List<RMTemplateCategory>();
        //    try
        //    {
        //        using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //        {
        //            connection.Open();
        //            using (var command = connection.CreateCommand())
        //            {
        //                command.CommandText = string.Format(RecordQueryString.LoadCategories, RMTemplateCategoryTableName);
        //                command.Parameters.AddWithValue("@TemplateUniqueId", UniqueId);
        //                using (SqlDataReader sdr = command.ExecuteReader())
        //                {
        //                    while (sdr.Read())
        //                    {
        //                        RMTemplateCategory rMTemplateCategory = new RMTemplateCategory();
        //                        //Id,UniqueId,,TemplateId,TemplateUniqueId,LastModifiedOn,IsDefault
        //                        if (!sdr.IsDBNull(0))
        //                        {
        //                            rMTemplateCategory.Id = sdr.GetInt32(0);
        //                        }
        //                        if (!sdr.IsDBNull(1))
        //                        {
        //                            rMTemplateCategory.UniqueId = sdr.GetGuid(1);
        //                        }
        //                        if (!sdr.IsDBNull(2))
        //                        {
        //                            rMTemplateCategory.Name = sdr.GetString(2);
        //                        }
        //                        if (!sdr.IsDBNull(3))
        //                        {
        //                            rMTemplateCategory.TemplateId = sdr.GetInt32(3);
        //                        }
        //                        if (!sdr.IsDBNull(4))
        //                        {
        //                            rMTemplateCategory.TemplateUniqueId = sdr.GetGuid(4);
        //                        }
        //                        if (!sdr.IsDBNull(5))
        //                        {
        //                            rMTemplateCategory.LastModifiedOn = sdr.GetDateTime(5);
        //                        }
        //                        if (!sdr.IsDBNull(6))
        //                        {
        //                            rMTemplateCategory.IsDefault = sdr.GetBoolean(6);
        //                        }
        //                        categories.Add(rMTemplateCategory);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        mLog.Info("Failed LoadCategories.Message:{0}. ", ex.ToString());
        //    }
        //    return categories;
        //}

        //public static RMTemplate GetTemplateByIdToDto(int id)
        //{
        //    mLog.Info("GetTemplateByIdToDto.id:{0}.", id);
        //    RMTemplate result = new RMTemplate();
        //    using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //    {
        //        connection.Open();
        //        using (var command = connection.CreateCommand())
        //        {
        //            command.CommandText = string.Format(RecordQueryString.GetTemplateByIdToDto, RMTemplateTableName);
        //            command.Parameters.AddWithValue("@Id", id);
        //            using (SqlDataReader sdr = command.ExecuteReader())
        //            {
        //                if (sdr.Read())
        //                {
        //                    if (!sdr.IsDBNull(0))
        //                    {
        //                        result.Id = sdr.GetInt32(0);
        //                    }
        //                    if (!sdr.IsDBNull(1))
        //                    {
        //                        result.UniqueId = sdr.GetGuid(1);
        //                    }
        //                    if (!sdr.IsDBNull(2))
        //                    {
        //                        result.Name = sdr.GetString(2);
        //                    }
        //                    if (!sdr.IsDBNull(3))
        //                    {
        //                        result.Type = (TemplateType)sdr.GetInt32(3);
        //                    }
        //                    if (!sdr.IsDBNull(4))
        //                    {
        //                        result.Prefix = sdr.GetString(4);
        //                    }
        //                    if (!sdr.IsDBNull(5))
        //                    {
        //                        result.NumberOfDigits = sdr.GetInt32(5);
        //                    }
        //                    if (!sdr.IsDBNull(6))
        //                    {
        //                        result.ParentId = sdr.GetInt32(6);
        //                    }
        //                    if (!sdr.IsDBNull(7))
        //                    {
        //                        result.ParentUniqueId = sdr.GetGuid(7);
        //                    }
        //                    if (!sdr.IsDBNull(8))
        //                    {
        //                        result.Size = sdr.GetDouble(8);
        //                    }
        //                    if (!sdr.IsDBNull(9))
        //                    {
        //                        result.Creater = sdr.GetInt32(9);
        //                    }
        //                    if (!sdr.IsDBNull(10))
        //                    {
        //                        result.CreatedOn = sdr.GetDateTime(10);
        //                    }
        //                    if (!sdr.IsDBNull(11))
        //                    {
        //                        result.Modifier = sdr.GetInt32(11);
        //                    }
        //                    if (!sdr.IsDBNull(12))
        //                    {
        //                        result.LastModifiedOn = sdr.GetDateTime(12);
        //                    }
        //                    if (!sdr.IsDBNull(13))
        //                    {
        //                        result.ColumnSchema = sdr.GetString(13);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return result;
        //}

        //public static RMTemplate GetTemplateByTemplateType(TemplateType type)
        //{
        //    mLog.Info("GetTemplateByTemplateType.type:{0}.", type);
        //    RMTemplate result = new RMTemplate();
        //    using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //    {
        //        connection.Open();
        //        using (var command = connection.CreateCommand())
        //        {
        //            command.CommandText = string.Format(RecordQueryString.GetTemplateByTemplateType, RMTemplateTableName);
        //            command.Parameters.AddWithValue("@Type", type);
        //            using (SqlDataReader sdr = command.ExecuteReader())
        //            {
        //                if (sdr.Read())
        //                {
        //                    if (!sdr.IsDBNull(0))
        //                    {
        //                        result.Id = sdr.GetInt32(0);
        //                    }
        //                    if (!sdr.IsDBNull(1))
        //                    {
        //                        result.UniqueId = sdr.GetGuid(1);
        //                    }
        //                    if (!sdr.IsDBNull(2))
        //                    {
        //                        result.Name = sdr.GetString(2);
        //                    }
        //                    if (!sdr.IsDBNull(3))
        //                    {
        //                        result.Type = (TemplateType)sdr.GetInt32(3);
        //                    }
        //                    if (!sdr.IsDBNull(4))
        //                    {
        //                        result.Prefix = sdr.GetString(4);
        //                    }
        //                    if (!sdr.IsDBNull(5))
        //                    {
        //                        result.NumberOfDigits = sdr.GetInt32(5);
        //                    }
        //                    if (!sdr.IsDBNull(6))
        //                    {
        //                        result.ParentId = sdr.GetInt32(6);
        //                    }
        //                    if (!sdr.IsDBNull(7))
        //                    {
        //                        result.ParentUniqueId = sdr.GetGuid(7);
        //                    }
        //                    if (!sdr.IsDBNull(8))
        //                    {
        //                        result.Size = sdr.GetDouble(8);
        //                    }
        //                    if (!sdr.IsDBNull(9))
        //                    {
        //                        result.Creater = sdr.GetInt32(9);
        //                    }
        //                    if (!sdr.IsDBNull(10))
        //                    {
        //                        result.CreatedOn = sdr.GetDateTime(10);
        //                    }
        //                    if (!sdr.IsDBNull(11))
        //                    {
        //                        result.Modifier = sdr.GetInt32(11);
        //                    }
        //                    if (!sdr.IsDBNull(12))
        //                    {
        //                        result.LastModifiedOn = sdr.GetDateTime(12);
        //                    }
        //                    if (!sdr.IsDBNull(13))
        //                    {
        //                        result.ColumnSchema = sdr.GetString(13);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return result;
        //}

        //public static void BatchDeleteRecordAllianceByIds(List<Guid> ids, int allianceType = RecordsConstants.RecordHold_Electronic)
        //{

        //    using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //    {
        //        connection.Open();
        //        foreach (Guid id in ids)
        //        {
        //            mLog.Info("BatchDeleteRecordAllianceByIds.id:{0}.", id);
        //            using (var command = connection.CreateCommand())
        //            {
        //                command.CommandText = string.Format(RecordQueryString.DeleteRecordAllianceById, RMRecordAllianceTableName);
        //                command.Parameters.AddWithValue("@RecordsId", id);
        //                // command.Parameters.AddWithValue("@AllianceType", allianceType);
        //                command.ExecuteNonQuery();
        //            }
        //        }
        //    }
        //}

        //public static void PhysicalFileMoveWithHold(Guid fileId, Guid srcBoxId, Guid destBoxId, Guid destLocationId, PhysicalHoldConflictOption physicalHoldConflict)
        //{
        //    mLog.Info("PhysicalFileMoveWithHold.fileId:{0}.", fileId);
        //    List<RMRecordAlliance> rMRecordAlliances = GetAllRMRecordAlliance();
        //    //先检查源端的Hold状态
        //    //&& a.AllianceType == RecordsConstants.RecordHold_PhyProfile
        //    List<RMRecordAlliance> srcHolds = rMRecordAlliances.Where(a => (a.RecordsId == fileId || a.RecordsId == srcBoxId)).ToList();
        //    RMRecordAlliance destHold = destBoxId == Guid.Empty ? null : rMRecordAlliances.FirstOrDefault(a => a.RecordsId == destBoxId);
        //    if (srcHolds.Any(a => a.RecordsId == fileId))
        //    {
        //        //File本身是Hold的
        //        mLog.Info("PhysicalFileMoveWithHold.Current file:{0} is hold.", fileId);
        //        RMRecordAlliance srcHold = srcHolds.First(a => a.RecordsId == fileId);
        //        if (destHold == null)
        //        {
        //            mLog.Info("PhysicalFileMoveWithHold.DesBox:{0} doesn't have hold and use source:{1} hold.", destBoxId, srcHold.BoxId);
        //            //目的端Container没有Hold,  只更新本身的ParentId
        //            srcHold.BoxId = destBoxId;
        //            srcHold.LocationId = destLocationId;
        //            UpdateRMRecordAllianceByRecordsId(srcHold);
        //        }
        //        else
        //        {
        //            ////目的端的Container 有Hold,需要根据冲突解决方案处理
        //            if (physicalHoldConflict == PhysicalHoldConflictOption.UseDesDefinedHoldSetting)
        //            {
        //                mLog.Info("PhysicalFileMoveWithHold.Source:{0} and Des:{1} all has hold and UseDesDefinedHoldSetting.", srcBoxId, destBoxId);
        //                DeleteRMRecordAllianceByRecordsId(srcHold.RecordsId);
        //            }
        //            else if (physicalHoldConflict == PhysicalHoldConflictOption.CompareHoldSetting)
        //            {
        //                mLog.Info("PhysicalFileMoveWithHold.Source:{0} and Des:{1} all has hold and CompareHoldSetting.srcHold.HoldReleaseTime:{2}.destHold.HoldReleaseTime:{3}.", srcBoxId, destBoxId, new DateTime(srcHold.HoldReleaseTime).ToString(), new DateTime(destHold.HoldReleaseTime).ToString());
        //                if (srcHold.HoldReleaseTime > destHold.HoldReleaseTime)
        //                {
        //                    //将目的端box记录hold相关属性设置成源端的(最长的),删除source file的记录
        //                    destHold.HoldId = srcHold.HoldId;
        //                    destHold.HoldReleaseTime = srcHold.HoldReleaseTime;
        //                    destHold.HoldBy = srcHold.HoldBy;
        //                    UpdateRMRecordAllianceByRecordsId(destHold);
        //                    DeleteRMRecordAllianceByRecordsId(srcHold.RecordsId);
        //                }
        //                else
        //                {
        //                    //删除删除source file的记录
        //                    DeleteRMRecordAllianceByRecordsId(srcHold.RecordsId);
        //                }
        //            }
        //            else
        //            {
        //                if (srcHold.HoldId != destHold.HoldId)
        //                {
        //                    //异常失败, 不允许Move (这种情况会再之前的逻辑中弹出冲突解决的提示框)
        //                    throw new GCommon.Utility.AveException("The destination box has a different hold release time than that of the source folder");
        //                }
        //            }
        //        }
        //    }
        //    else if (srcBoxId != Guid.Empty && srcHolds.Any(a => a.RecordsId == srcBoxId))
        //    {
        //        //File本身没有Hold,  但源端Box有Hold
        //        mLog.Info("PhysicalFileMoveWithHold.Current file:{0} is not hold but file parent Box:{1} is hold.", fileId, srcBoxId);
        //        if (destHold == null)
        //        {
        //            RMRecordAlliance srcContainerHOld = srcHolds.First(a => a.RecordsId == srcBoxId);
        //            //目的端Container没有Hold, 按源端Container新建一个File级别的Hold
        //            mLog.Info("PhysicalFileMoveWithHold.Current file:{0} is not hold but file parent Box:{1} is hold and des Box:{2} doesn't have hold.", fileId, srcBoxId, destBoxId);
        //            var entity = new RMRecordAlliance()
        //            {
        //                RecordsId = fileId,
        //                BoxId = destBoxId,
        //                AllianceType = srcContainerHOld.AllianceType,
        //                HoldBy = srcContainerHOld.HoldBy,
        //                HoldId = srcContainerHOld.HoldId,
        //                Level = (int)PhysicalNodeLevel.PhysicalFile,
        //                HoldReleaseTime = srcContainerHOld.HoldReleaseTime
        //            };
        //            InsertRMRecordAlliance(entity);
        //        }
        //        else
        //        {
        //            mLog.Info("PhysicalFileMoveWithHold.Current file:{0} is not hold but file parent Box:{1} is hold and des Box:{2} have hold.So we don't do anything.", fileId, srcBoxId, destBoxId);
        //            //目的端Container 有Hold, 啥也不用做
        //        }
        //    }
        //}

        //public static bool CanPhysicalFileMove(Guid fileId, Guid srcParentId, Guid destParentId)
        //{
        //    mLog.Info("CanPhysicalFileMove.fileId:{0}.", fileId);
        //    List<RMRecordAlliance> rMRecordAlliances = GetAllRMRecordAlliance();
        //    //&& a.AllianceType == RecordsConstants.RecordHold_PhyProfile
        //    List<RMRecordAlliance> srcHolds = rMRecordAlliances.Where(a => (a.RecordsId == fileId || a.RecordsId == srcParentId)).ToList();
        //    RMRecordAlliance destHold = rMRecordAlliances.FirstOrDefault(a => a.RecordsId == destParentId);
        //    if (srcHolds.Any(a => a.RecordsId == fileId))
        //    {
        //        RMRecordAlliance srcHold = srcHolds.First(a => a.RecordsId == fileId);
        //        //File本身是Hold的
        //        if (destHold != null)
        //        {
        //            //目的端的Container 有Hold, 比较HOld Id或者ReleaseTime是否相同
        //            if (srcHold.HoldReleaseTime != destHold.HoldReleaseTime)
        //            {
        //                //异常失败, 不允许Move
        //                //throw new GCommon.Utility.AveException("Dest container has a different hold time.");
        //                return false;
        //            }

        //        }
        //    }
        //    return true;
        //}

        //public static void UpdateRMRecordAllianceByRecordsId(RMRecordAlliance rMRecordAlliance)
        //{
        //    using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //    {
        //        connection.Open();
        //        using (var command = connection.CreateCommand())
        //        {
        //            command.CommandText = string.Format(RecordQueryString.UpdateRMRecordAllianceByRecordsId, RMRecordAllianceTableName);
        //            command.Parameters.AddWithValue("@RecordsId", rMRecordAlliance.RecordsId);
        //            command.Parameters.AddWithValue("@HoldId", rMRecordAlliance.HoldId);
        //            command.Parameters.AddWithValue("@HoldReleaseTime", rMRecordAlliance.HoldReleaseTime);
        //            command.Parameters.AddWithValue("@HoldBy", rMRecordAlliance.HoldBy);
        //            command.Parameters.AddWithValue("@AllianceType", rMRecordAlliance.AllianceType);
        //            command.Parameters.AddWithValue("@BoxId", rMRecordAlliance.BoxId);
        //            command.Parameters.AddWithValue("@LocationId", rMRecordAlliance.LocationId);
        //            command.Parameters.AddWithValue("@Level", rMRecordAlliance.Level);
        //            command.ExecuteNonQuery();
        //        }
        //    }
        //}

        //public static void InsertRMRecordAlliance(RMRecordAlliance rMRecordAlliance)
        //{
        //    using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //    {
        //        connection.Open();
        //        using (var command = connection.CreateCommand())
        //        {
        //            command.CommandText = string.Format(RecordQueryString.InsertRMRecordAlliance, RMRecordAllianceTableName);
        //            command.Parameters.AddWithValue("@RecordsId", rMRecordAlliance.RecordsId);
        //            command.Parameters.AddWithValue("@HoldId", rMRecordAlliance.HoldId);
        //            command.Parameters.AddWithValue("@HoldReleaseTime", rMRecordAlliance.HoldReleaseTime);
        //            command.Parameters.AddWithValue("@HoldBy", rMRecordAlliance.HoldBy);
        //            command.Parameters.AddWithValue("@AllianceType", rMRecordAlliance.AllianceType);
        //            command.Parameters.AddWithValue("@BoxId", rMRecordAlliance.BoxId);
        //            command.Parameters.AddWithValue("@LocationId", rMRecordAlliance.LocationId);
        //            command.Parameters.AddWithValue("@Level", rMRecordAlliance.Level);
        //            command.ExecuteNonQuery();
        //        }
        //    }
        //}

        //public static void DeleteRMRecordAllianceByRecordsId(Guid recordsId)
        //{
        //    using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //    {
        //        connection.Open();
        //        using (var command = connection.CreateCommand())
        //        {
        //            command.CommandText = string.Format(RecordQueryString.DeleteRMRecordAllianceByRecordsId, RMRecordAllianceTableName);
        //            command.Parameters.AddWithValue("@RecordsId", recordsId);
        //            command.ExecuteNonQuery();
        //        }
        //    }
        //}

        //public static List<RMRecordAlliance> GetAllRMRecordAlliance()
        //{
        //    List<RMRecordAlliance> rMRecordAlliances = new List<RMRecordAlliance>();
        //    using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //    {
        //        connection.Open();
        //        using (var command = connection.CreateCommand())
        //        {
        //            command.CommandText = string.Format(RecordQueryString.GetAllRMRecordAlliance, RMRecordAllianceTableName);
        //            using (SqlDataReader sdr = command.ExecuteReader())
        //            {
        //                while (sdr.Read())
        //                {
        //                    RMRecordAlliance rMRecordAlliance = new RMRecordAlliance();
        //                    if (!sdr.IsDBNull(0))
        //                    {
        //                        rMRecordAlliance.RecordsId = sdr.GetGuid(0);
        //                    }
        //                    if (!sdr.IsDBNull(1))
        //                    {
        //                        rMRecordAlliance.HoldId = sdr.GetString(1);
        //                    }
        //                    if (!sdr.IsDBNull(2))
        //                    {
        //                        rMRecordAlliance.HoldReleaseTime = sdr.GetInt64(2);
        //                    }
        //                    if (!sdr.IsDBNull(3))
        //                    {
        //                        rMRecordAlliance.HoldBy = sdr.GetString(3);
        //                    }
        //                    if (!sdr.IsDBNull(4))
        //                    {
        //                        rMRecordAlliance.AllianceType = sdr.GetInt32(4);
        //                    }
        //                    if (!sdr.IsDBNull(5))
        //                    {
        //                        rMRecordAlliance.BoxId = sdr.GetGuid(5);
        //                    }
        //                    if (!sdr.IsDBNull(6))
        //                    {
        //                        rMRecordAlliance.LocationId = sdr.GetGuid(6);
        //                    }
        //                    if (!sdr.IsDBNull(7))
        //                    {
        //                        rMRecordAlliance.Level = sdr.GetInt32(7);
        //                    }
        //                    rMRecordAlliances.Add(rMRecordAlliance);
        //                }
        //            }
        //        }
        //    }
        //    return rMRecordAlliances;
        //}

        //public static List<RMRecordLoanAlliance> GetPhyRecordAllianceById(Guid id)
        //{
        //    mLog.Info("GetPhyRecordAllianceById.id:{0}.", id);
        //    List<RMRecordLoanAlliance> loanAlliances = new List<RMRecordLoanAlliance>();
        //    using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //    {
        //        connection.Open();
        //        using (var command = connection.CreateCommand())
        //        {
        //            command.CommandText = string.Format(RecordQueryString.GetPhyRecordAllianceById, RMRecordLoanAllianceTableName);
        //            command.Parameters.AddWithValue("@RecordsId", id);
        //            using (SqlDataReader sdr = command.ExecuteReader())
        //            {
        //                while (sdr.Read())
        //                {
        //                    RMRecordLoanAlliance rRMRecordLoanAlliance = new RMRecordLoanAlliance();
        //                    if (!sdr.IsDBNull(0))
        //                    {
        //                        rRMRecordLoanAlliance.Id = sdr.GetInt32(0);
        //                    }
        //                    if (!sdr.IsDBNull(1))
        //                    {
        //                        rRMRecordLoanAlliance.RecordsId = sdr.GetGuid(1);
        //                    }
        //                    if (!sdr.IsDBNull(2))
        //                    {
        //                        rRMRecordLoanAlliance.HoldReleaseTime = sdr.GetInt64(2);
        //                    }
        //                    if (!sdr.IsDBNull(3))
        //                    {
        //                        rRMRecordLoanAlliance.HoldBy = sdr.GetString(3);
        //                    }
        //                    if (!sdr.IsDBNull(4))
        //                    {
        //                        rRMRecordLoanAlliance.ParentId = sdr.GetGuid(4);
        //                    }
        //                    loanAlliances.Add(rRMRecordLoanAlliance);
        //                }
        //            }
        //        }
        //    }
        //    return loanAlliances;
        //}
        public static List<RMRecordLoanAlliance> GetPhyRecordAllianceByIds(List<Guid> ids)
        {
            mLog.Info("GetPhyRecordAllianceByIds.");
            List<RMRecordLoanAlliance> loanAlliances = new List<RMRecordLoanAlliance>();
            loanAlliances = RecordLoanAllianceDao.GetPhyRecordAllianceByIds(ids);
            return loanAlliances.Where(a => ids.Any(temp => temp == a.RecordsId)).ToList();
        }

        //public static List<RMTermRuleAssociation> GetTermWithRule()
        //{

        //    mLog.Info("GetTermWithRule.");
        //    List<RMTermRuleAssociation> ruleAssociations = new List<RMTermRuleAssociation>();
        //    using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //    {
        //        connection.Open();
        //        using (var command = connection.CreateCommand())
        //        {
        //            command.CommandText = string.Format(RecordQueryString.GetTermWithRule, RMTermRuleAssociationTableName);
        //            using (SqlDataReader sdr = command.ExecuteReader())
        //            {
        //                while (sdr.Read())
        //                {
        //                    RMTermRuleAssociation ruleAssociation = new RMTermRuleAssociation();
        //                    if (!sdr.IsDBNull(0))
        //                    {
        //                        ruleAssociation.Id = sdr.GetInt32(0);
        //                    }
        //                    if (!sdr.IsDBNull(1))
        //                    {
        //                        ruleAssociation.TermId = sdr.GetInt32(1);
        //                    }
        //                    if (!sdr.IsDBNull(2))
        //                    {
        //                        ruleAssociation.TermName = sdr.GetString(2);
        //                    }
        //                    if (!sdr.IsDBNull(3))
        //                    {
        //                        ruleAssociation.RuleId = sdr.GetGuid(3);
        //                    }
        //                    if (!sdr.IsDBNull(4))
        //                    {
        //                        ruleAssociation.RuleName = sdr.GetString(4);
        //                    }
        //                    if (!sdr.IsDBNull(5))
        //                    {
        //                        ruleAssociation.RuleLevel = sdr.GetString(5);
        //                    }
        //                    if (!sdr.IsDBNull(6))
        //                    {
        //                        ruleAssociation.RuleOrder = sdr.GetInt32(6);
        //                    }
        //                    ruleAssociations.Add(ruleAssociation);
        //                }
        //            }
        //        }
        //    }
        //    return ruleAssociations;
        //}

        //public static Dictionary<int, List<int>> FindListWithColumns()
        //{
        //    mLog.Info("FindListWithColumns.");
        //    List<RMTermSetMembership> rMTermSetMemberships = new List<RMTermSetMembership>();
        //    using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //    {
        //        connection.Open();
        //        using (var command = connection.CreateCommand())
        //        {
        //            command.CommandText = string.Format(RecordQueryString.GetAllRMTermSetMembership, RMTermSetMembershipTableName);
        //            command.Parameters.AddWithValue("@IsRemoved", false);
        //            using (SqlDataReader sdr = command.ExecuteReader())
        //            {
        //                while (sdr.Read())
        //                {
        //                    RMTermSetMembership rMTermSetMembership = new RMTermSetMembership();
        //                    if (!sdr.IsDBNull(0))
        //                    {
        //                        rMTermSetMembership.TermId = sdr.GetInt32(0);
        //                    }
        //                    if (!sdr.IsDBNull(1))
        //                    {
        //                        rMTermSetMembership.TermSetId = sdr.GetInt32(1);
        //                    }
        //                    if (!sdr.IsDBNull(2))
        //                    {
        //                        rMTermSetMembership.ParentTermId = sdr.GetInt32(2);
        //                    }
        //                    if (!sdr.IsDBNull(3))
        //                    {
        //                        rMTermSetMembership.TermName = sdr.GetString(3);
        //                    }
        //                    if (!sdr.IsDBNull(4))
        //                    {
        //                        rMTermSetMembership.Path = sdr.GetString(4);
        //                    }
        //                    if (!sdr.IsDBNull(5))
        //                    {
        //                        rMTermSetMembership.IsSource = sdr.GetBoolean(5);
        //                    }
        //                    if (!sdr.IsDBNull(6))
        //                    {
        //                        rMTermSetMembership.IsRemoved = sdr.GetBoolean(6);
        //                    }
        //                    rMTermSetMemberships.Add(rMTermSetMembership);
        //                }
        //            }
        //        }
        //    }
        //    Dictionary<int, List<int>> memberships = new Dictionary<int, List<int>>();
        //    memberships = rMTermSetMemberships.Select(c => new { c.TermId, c.ParentTermId }).GroupBy(t => t.ParentTermId, v => v.TermId).ToDictionary(t => t.Key, v => v.ToList());
        //    return memberships;
        //}

        //public static List<RMTerm> GetAllTermsForce()
        //{
        //    mLog.Info("GetAllTermsForce.");
        //    List<RMTerm> terms = new List<RMTerm>();
        //    try
        //    {
        //        using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //        {
        //            connection.Open();
        //            using (var command = connection.CreateCommand())
        //            {
        //                command.CommandText = string.Format(RecordQueryString.GetAllTermsForce, RMTermTableName);
        //                using (SqlDataReader sdr = command.ExecuteReader())
        //                {
        //                    while (sdr.Read())
        //                    {
        //                        RMTerm rMTerm = new RMTerm();
        //                        if (!sdr.IsDBNull(0))
        //                        {
        //                            rMTerm.Id = sdr.GetInt32(0);
        //                        }
        //                        if (!sdr.IsDBNull(1))
        //                        {
        //                            rMTerm.TermSetId = sdr.GetInt32(1);
        //                        }
        //                        if (!sdr.IsDBNull(2))
        //                        {
        //                            rMTerm.UniqueId = sdr.GetGuid(2);
        //                        }
        //                        if (!sdr.IsDBNull(3))
        //                        {
        //                            rMTerm.Name = sdr.GetString(3);
        //                        }
        //                        if (!sdr.IsDBNull(4))
        //                        {
        //                            rMTerm.Description = sdr.GetString(4);
        //                        }
        //                        if (!sdr.IsDBNull(5))
        //                        {
        //                            rMTerm.IsDeprecated = sdr.GetBoolean(5);
        //                        }
        //                        if (!sdr.IsDBNull(6))
        //                        {
        //                            rMTerm.IsRemoved = sdr.GetBoolean(6);
        //                        }
        //                        if (!sdr.IsDBNull(7))
        //                        {
        //                            rMTerm.BreakInheritFromParent = sdr.GetBoolean(7);
        //                        }
        //                        if (!sdr.IsDBNull(8))
        //                        {
        //                            rMTerm.TimeZoneId = sdr.GetString(8);
        //                        }
        //                        if (!sdr.IsDBNull(9))
        //                        {
        //                            rMTerm.RuleInfo = sdr.GetString(9);
        //                        }
        //                        if (!sdr.IsDBNull(10))
        //                        {
        //                            rMTerm.TermExpirationFrom = sdr.GetInt64(10);
        //                        }
        //                        if (!sdr.IsDBNull(11))
        //                        {
        //                            rMTerm.TermExpirationTo = sdr.GetInt64(11);
        //                        }
        //                        if (!sdr.IsDBNull(12))
        //                        {
        //                            rMTerm.IsRootTerm = sdr.GetBoolean(12);
        //                        }
        //                        if (!sdr.IsDBNull(13))
        //                        {
        //                            rMTerm.IsDayLight = sdr.GetBoolean(13);
        //                        }
        //                        if (!sdr.IsDBNull(14))
        //                        {
        //                            rMTerm.AvailableSpace = sdr.GetDouble(14);
        //                        }
        //                        if (!sdr.IsDBNull(15))
        //                        {
        //                            rMTerm.IsDefaultTerm = sdr.GetBoolean(15);
        //                        }
        //                        if (!sdr.IsDBNull(16))
        //                        {
        //                            rMTerm.EnforceRetention = sdr.GetInt32(16);
        //                        }
        //                        if (!sdr.IsDBNull(17))
        //                        {
        //                            rMTerm.EXORetentionLabel = sdr.GetString(17);
        //                        }
        //                        if (!sdr.IsDBNull(18))
        //                        {
        //                            rMTerm.SPRetentionLabel = sdr.GetString(18);
        //                        }
        //                        if (!sdr.IsDBNull(19))
        //                        {
        //                            rMTerm.IsPermanent = sdr.GetBoolean(19);
        //                        }
        //                        terms.Add(rMTerm);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        mLog.Info("Failed GetAllTermsForce.Message:{0}. ", ex.ToString());
        //    }
        //    return terms;
        //}

        //public static Dictionary<int, string> GetLocationIdNameMapping()
        //{
        //    Dictionary<int, string> mlocationIdNameMapping = new Dictionary<int, string>();
        //    using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //    {
        //        connection.Open();
        //        using (var command = connection.CreateCommand())
        //        {
        //            command.CommandText = string.Format(RecordQueryString.GetLocationIdNameMapping, RMLocationTableName);
        //            using (SqlDataReader sdr = command.ExecuteReader())
        //            {
        //                int tempId = 0;
        //                string tempName = string.Empty;
        //                while (sdr.Read())
        //                {
        //                    if (!sdr.IsDBNull(0))
        //                    {
        //                        tempId = sdr.GetInt32(0);
        //                    }
        //                    if (!sdr.IsDBNull(0))
        //                    {
        //                        tempName = sdr.GetString(1);
        //                    }
        //                    mlocationIdNameMapping.Add(tempId, tempName);
        //                }
        //            }
        //        }
        //    }
        //    return mlocationIdNameMapping;
        //}

        //public static string GetLocationPath(string dirPath)
        //{
        //    var result = string.Empty;
        //    if (!string.IsNullOrEmpty(dirPath))
        //    {
        //        var LocationIDNameMapping = GetLocationIdNameMapping();
        //        dirPath = dirPath.TrimEnd('/');
        //        List<string> locationIds = dirPath.Split('/').ToList();
        //        for (int i = 0; i < locationIds.Count; i++)
        //        {
        //            var tempPath = string.Empty;
        //            if (LocationIDNameMapping.TryGetValue(Convert.ToInt32(locationIds[i]), out tempPath))
        //            {
        //                if (i == 0)
        //                {
        //                    result = tempPath;
        //                }
        //                else
        //                {
        //                    result = result + "/" + tempPath;
        //                }
        //            }
        //            else
        //            {
        //                mLog.Warn($"Cannot get location : {locationIds[i]} in db.");
        //                throw new Exception($"Cannot get location by Path");
        //            }
        //        }
        //    }
        //    return result;
        //}

        //public static TemplateDto Convert2TemplateDto(RMTemplate template)
        //{
        //    var rstDto = new TemplateDto()
        //    {
        //        categories = new List<TemplateCategoryDto>()
        //    };
        //    //GeneralSettingModel gls = mGeneralSettingService.GetGeneralSetting();
        //    rstDto.id = template.Id;
        //    rstDto.uniqueId = template.UniqueId;
        //    rstDto.name = template.Name;
        //    rstDto.prefix = template.Prefix;
        //    rstDto.numberOfDigits = template.NumberOfDigits.HasValue ? template.NumberOfDigits.Value : 0;
        //    rstDto.type = template.Type;
        //    rstDto.createdOn = template.CreatedOn;
        //    rstDto.lastModifiedOn = template.LastModifiedOn;
        //    //rstDto.createdOnStr = mGeneralSettingService.ConvertTiksToDateTime(gls, rstDto.createdOn.Ticks, true).DataTime.ToString("MM/dd/yyyy HH:mm:ss");
        //    //rstDto.lastModifiedOnStr = mGeneralSettingService.ConvertTiksToDateTime(gls, rstDto.lastModifiedOn.Ticks, true).DataTime.ToString("MM/dd/yyyy HH:mm:ss");
        //    //if (template.Creater != -1)
        //    //{
        //    //    var account = AccountDao.GetUserById(template.Creater);
        //    //    //var account = ctx.Account.Where(a => a.Id == template.Creater).FirstOrDefault();
        //    //    if (account != null)
        //    //    {
        //    //        rstDto.creater = new ToUserInfo()
        //    //        {
        //    //            UserId = account.UserId,
        //    //            DisplayName = account.DisplayName,
        //    //            UserPrincipalName = account.UserPrincipalName,
        //    //        };
        //    //    }
        //    //}
        //    //else
        //    //{
        //    //    rstDto.creater = new ToUserInfo()
        //    //    {
        //    //        UserId = "-1",
        //    //        DisplayName = "Built-in",
        //    //        UserPrincipalName = "Built-in",
        //    //    };
        //    //}

        //    //TODO others
        //    var schema = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(template.ColumnSchema);
        //    var groups = schema.Columns.GroupBy(c => c.CategoryId).ToDictionary(key => key.Key, c => c.ToList());

        //    List<RMTemplateCategory> dbCategories = RecordsDBOperation.LoadCategories(template.UniqueId).ToList();
        //    foreach (var category in dbCategories)
        //    {
        //        var templateColumns = new List<TemplateColumnDto>();
        //        rstDto.categories.Add(new TemplateCategoryDto()
        //        {
        //            id = category.UniqueId,
        //            name = category.Name,
        //            allowEdit = !category.IsDefault,
        //            columns = templateColumns,
        //        });
        //        if (groups.ContainsKey(category.UniqueId))
        //        {
        //            var list = groups[category.UniqueId];
        //            for (int i = 0; i < list.Count; i++)
        //            {
        //                var item = list[i];
        //                var columnDto = new TemplateColumnDto()
        //                {
        //                    categoryId = item.CategoryId,
        //                    columnName = item.Name,
        //                    uniqueId = item.UniqueId,
        //                    required = item.Required,
        //                    typeId = (int)item.ColumnType,
        //                    showInEditForm = item.ShowInEditForm,
        //                    allowEdit = item.AllowEdit,
        //                    inheritFromParent = (item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.InheritFromParentBox) == (int)TemplateInheritSettingEnum.InheritFromParentBox,
        //                    inheritFromParentFolder = (item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.InheritFromParentFolder) == (int)TemplateInheritSettingEnum.InheritFromParentFolder,
        //                    pushToChild = (item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.PushToChild) == (int)TemplateInheritSettingEnum.PushToChild,
        //                    childInheritsValue = (item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.ChildInheritsValue) == (int)TemplateInheritSettingEnum.ChildInheritsValue,
        //                    allowModifyValue = (item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.AllowModifyValue) == (int)TemplateInheritSettingEnum.AllowModifyValue,
        //                    pushCategoryId = item.PushToRecordCategoryId,
        //                    pushFolderCategoryId = item.PushToFolderCategoryId,
        //                };
        //                //RECO-4254
        //                if (item.UniqueId == new Guid(DefaultColumnIDs.Description))
        //                {
        //                    columnDto.allowEdit = true;
        //                }
        //                switch (item.ColumnType)
        //                {
        //                    case ColumnType.SingleText:
        //                    case ColumnType.MultipleText:
        //                    case ColumnType.DateTime:
        //                    case ColumnType.PeopleOrGroup:
        //                    case ColumnType.Number:
        //                        break;
        //                    case ColumnType.Taxonomy:
        //                        break;
        //                    case ColumnType.SingleChoice:
        //                    case ColumnType.MultipleChoice:
        //                        columnDto.optionsJSON = item.OptionsJSON;
        //                        break;
        //                    default:
        //                        break;
        //                }
        //                templateColumns.Add(columnDto);
        //            }
        //        }
        //    }

        //    return rstDto;
        //}

        //public static TemplateDto LoadTemplateDto(int id)
        //{
        //    try
        //    {
        //        TemplateDto resultDto = new TemplateDto();
        //        RMTemplate childFolderTemplate = new RMTemplate();
        //        RMTemplate childRecordTemplate = new RMTemplate();
        //        var template = RecordsDBOperation.GetTemplateByIdToDto(id);
        //        resultDto = Convert2TemplateDto(template);
        //        if (template != null)
        //        {
        //            if (template.Type == TemplateType.Box)
        //            {
        //                //目前一个tempalte level 只会有一个模板，所以代码这么写，以后支持多个的时候，需要返回集合对象，不使用两个对象表示
        //                childFolderTemplate = RecordsDBOperation.GetTemplateByTemplateType(TemplateType.Folder);
        //                childRecordTemplate = RecordsDBOperation.GetTemplateByTemplateType(TemplateType.Records);
        //            }
        //            else if (template.Type == TemplateType.Folder)
        //            {
        //                childRecordTemplate = RecordsDBOperation.GetTemplateByTemplateType(TemplateType.Records);
        //            }
        //        }
        //        if (childRecordTemplate != null && childRecordTemplate.Id != 0)
        //        {
        //            var dbCategories = RecordsDBOperation.LoadCategories(childRecordTemplate.UniqueId).ToList();
        //            resultDto.childCategories = new List<TemplateCategoryDto>();
        //            foreach (var category in dbCategories)
        //            {
        //                var templateColumns = new List<TemplateColumnDto>();

        //                resultDto.childCategories.Add(new TemplateCategoryDto()
        //                {
        //                    id = category.UniqueId,
        //                    name = category.Name,
        //                    allowEdit = !category.IsDefault,
        //                    columns = templateColumns,
        //                });
        //            }
        //        }
        //        if (childFolderTemplate != null && childFolderTemplate.Id != 0)
        //        {
        //            var dbCategories = RecordsDBOperation.LoadCategories(childFolderTemplate.UniqueId).ToList();
        //            resultDto.childFolderCategories = new List<TemplateCategoryDto>();
        //            foreach (var category in dbCategories)
        //            {
        //                var templateColumns = new List<TemplateColumnDto>();

        //                resultDto.childFolderCategories.Add(new TemplateCategoryDto()
        //                {
        //                    id = category.UniqueId,
        //                    name = category.Name,
        //                    allowEdit = !category.IsDefault,
        //                    columns = templateColumns,
        //                });
        //            }
        //        }
        //        return resultDto;
        //    }
        //    catch (Exception e)
        //    {
        //        mLog.Error("LoadTemplateDto error {0}", e.ToString());
        //        return null;
        //    }
        //}

        //public static List<RMPhysicalPushColumn> GetPushColumns(Guid columnUniqueId, List<Guid> physicObjectIds)
        //{
        //    mLog.Info("GetPushColumns.");
        //    List<RMPhysicalPushColumn> rMPhysicalPushColumns = new List<RMPhysicalPushColumn>();
        //    try
        //    {
        //        using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //        {
        //            connection.Open();
        //            using (var command = connection.CreateCommand())
        //            {
        //                command.CommandText = string.Format(RecordQueryString.GetPushColumns, RMPhysicalPushColumnTableName);
        //                command.Parameters.AddWithValue("@ColumnUniqueId", columnUniqueId);
        //                using (SqlDataReader sdr = command.ExecuteReader())
        //                {
        //                    while (sdr.Read())
        //                    {
        //                        RMPhysicalPushColumn rMPhysicalPushColumn = new RMPhysicalPushColumn();
        //                        if (!sdr.IsDBNull(0))
        //                        {
        //                            rMPhysicalPushColumn.Id = sdr.GetInt32(0);
        //                        }
        //                        if (!sdr.IsDBNull(1))
        //                        {
        //                            rMPhysicalPushColumn.ColumnUniqueId = sdr.GetGuid(1);
        //                        }
        //                        if (!sdr.IsDBNull(2))
        //                        {
        //                            rMPhysicalPushColumn.PhysicalObjectId = sdr.GetGuid(2);
        //                        }
        //                        if (!sdr.IsDBNull(3))
        //                        {
        //                            rMPhysicalPushColumn.TemplateId = sdr.GetInt32(3);
        //                        }
        //                        if (!sdr.IsDBNull(4))
        //                        {
        //                            rMPhysicalPushColumn.ColumnValue = sdr.GetString(4);
        //                        }
        //                        rMPhysicalPushColumns.Add(rMPhysicalPushColumn);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        mLog.Info("Failed GetPushColumns.Message:{0}. ", ex.ToString());
        //    }
        //    return rMPhysicalPushColumns.Where(s => physicObjectIds.Contains(s.PhysicalObjectId)).ToList();
        //}

        //public static List<RMSuiteMembership> GetAllRMSuiteMemberships()
        //{
        //    mLog.Info("GetAllRMSuiteMemberships.");
        //    List<RMSuiteMembership> mRMSuiteMemberships = new List<RMSuiteMembership>();
        //    try
        //    {
        //        using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //        {
        //            connection.Open();
        //            using (var command = connection.CreateCommand())
        //            {
        //                command.CommandText = string.Format(RecordQueryString.GetAllRMSuiteMemberships, RMSuiteMembershipTableName);
        //                using (SqlDataReader sdr = command.ExecuteReader())
        //                {
        //                    while (sdr.Read())
        //                    {
        //                        RMSuiteMembership rRMSuiteMembership = new RMSuiteMembership();
        //                        if (!sdr.IsDBNull(0))
        //                        {
        //                            rRMSuiteMembership.Id = sdr.GetInt32(0);
        //                        }
        //                        if (!sdr.IsDBNull(1))
        //                        {
        //                            rRMSuiteMembership.SuiteUniqueId = sdr.GetGuid(1);
        //                        }
        //                        if (!sdr.IsDBNull(2))
        //                        {
        //                            rRMSuiteMembership.RootTemplateUniqueId = sdr.GetGuid(2);
        //                        }
        //                        if (!sdr.IsDBNull(3))
        //                        {
        //                            rRMSuiteMembership.BoxTemplateUniqueId = sdr.GetGuid(3);
        //                        }
        //                        if (!sdr.IsDBNull(4))
        //                        {
        //                            rRMSuiteMembership.FolderTemplateUniqueId = sdr.GetGuid(4);
        //                        }
        //                        if (!sdr.IsDBNull(5))
        //                        {
        //                            rRMSuiteMembership.RecordTemplateUniqueId = sdr.GetGuid(5);
        //                        }
        //                        mRMSuiteMemberships.Add(rRMSuiteMembership);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        mLog.Info("Failed GetAllRMSuiteMemberships.Message:{0}. ", ex.ToString());
        //    }
        //    return mRMSuiteMemberships;
        //}

        //public static List<RMSuite> GetAllRMSuites()
        //{
        //    mLog.Info("GetAllRMSuites.");
        //    List<RMSuite> mRMSuites = new List<RMSuite>();
        //    try
        //    {
        //        using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //        {
        //            connection.Open();
        //            using (var command = connection.CreateCommand())
        //            {
        //                command.CommandText = string.Format(RecordQueryString.GetAllRMSuites, RMSuitesTableName);
        //                using (SqlDataReader sdr = command.ExecuteReader())
        //                {
        //                    while (sdr.Read())
        //                    {
        //                        RMSuite rRMSuite = new RMSuite();
        //                        if (!sdr.IsDBNull(0))
        //                        {
        //                            rRMSuite.Id = sdr.GetInt32(0);
        //                        }
        //                        if (!sdr.IsDBNull(1))
        //                        {
        //                            rRMSuite.UniqueId = sdr.GetGuid(1);
        //                        }
        //                        if (!sdr.IsDBNull(2))
        //                        {
        //                            rRMSuite.Name = sdr.GetString(2);
        //                        }
        //                        if (!sdr.IsDBNull(3))
        //                        {
        //                            rRMSuite.Description = sdr.GetString(3);
        //                        }
        //                        if (!sdr.IsDBNull(4))
        //                        {
        //                            rRMSuite.StartFromType = (SuiteStartFromType)sdr.GetInt32(4);
        //                        }
        //                        if (!sdr.IsDBNull(5))
        //                        {
        //                            rRMSuite.Creater = sdr.GetInt32(5);
        //                        }
        //                        if (!sdr.IsDBNull(6))
        //                        {
        //                            rRMSuite.CreatedOn = sdr.GetDateTime(6);
        //                        }
        //                        if (!sdr.IsDBNull(7))
        //                        {
        //                            rRMSuite.Modifier = sdr.GetInt32(7);
        //                        }
        //                        if (!sdr.IsDBNull(8))
        //                        {
        //                            rRMSuite.LastModifiedOn = sdr.GetDateTime(8);
        //                        }
        //                        if (!sdr.IsDBNull(9))
        //                        {
        //                            rRMSuite.RootTemplateCreateType = (SuiteRootTemplateCreateType)sdr.GetInt32(9);
        //                        }
        //                        mRMSuites.Add(rRMSuite);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        mLog.Info("Failed GetAllRMSuites.Message:{0}. ", ex.ToString());
        //    }
        //    return mRMSuites;
        //}

        //public static List<RMLocationSuiteAssociation> GetAllRMLocationSuiteAssociations()
        //{
        //    mLog.Info("GetAllRMLocationSuiteAssociations.");
        //    List<RMLocationSuiteAssociation> mRMLocationSuiteAssociations = new List<RMLocationSuiteAssociation>();
        //    try
        //    {
        //        using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //        {
        //            connection.Open();
        //            using (var command = connection.CreateCommand())
        //            {
        //                command.CommandText = string.Format(RecordQueryString.GetALLRMLocationSuiteAssociations, RMLocationSuiteAssociationTableName);
        //                using (SqlDataReader sdr = command.ExecuteReader())
        //                {
        //                    while (sdr.Read())
        //                    {
        //                        RMLocationSuiteAssociation rMLocationSuiteAssociation = new RMLocationSuiteAssociation();
        //                        if (!sdr.IsDBNull(0))
        //                        {
        //                            rMLocationSuiteAssociation.Id = sdr.GetInt32(0);
        //                        }
        //                        if (!sdr.IsDBNull(1))
        //                        {
        //                            rMLocationSuiteAssociation.LocationUniqueId = sdr.GetGuid(1);
        //                        }
        //                        if (!sdr.IsDBNull(2))
        //                        {
        //                            rMLocationSuiteAssociation.SuiteUniqueId = sdr.GetGuid(2);
        //                        }
        //                        mRMLocationSuiteAssociations.Add(rMLocationSuiteAssociation);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        mLog.Info("Failed GetAllRMLocationSuiteAssociations.Message:{0}. ", ex.ToString());
        //    }
        //    return mRMLocationSuiteAssociations;
        //}

        //public static List<Guid> GetSuiteIdsByLocationID(Guid locationId)
        //{
        //    mLog.Info("GetSuiteIdsByLocationID.");
        //    List<Guid> suiteIds = new List<Guid>();
        //    try
        //    {
        //        using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //        {
        //            connection.Open();
        //            using (var command = connection.CreateCommand())
        //            {
        //                command.CommandText = string.Format(RecordQueryString.GetSuiteIdsByLocationID, RMLocationSuiteAssociationTableName);
        //                command.Parameters.AddWithValue("@LocationUniqueId", locationId);
        //                using (SqlDataReader sdr = command.ExecuteReader())
        //                {
        //                    while (sdr.Read())
        //                    {

        //                        if (!sdr.IsDBNull(0))
        //                        {
        //                            Guid suiteId = sdr.GetGuid(0);
        //                            if (!suiteIds.Contains(suiteId))
        //                            {
        //                                suiteIds.Add(suiteId);
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        mLog.Info("Failed GetSuiteIdsByLocationID.Message:{0}. ", ex.ToString());
        //    }
        //    return suiteIds;
        //}

        //public static string GetTemplateRelationshipIdPathIfExist(List<string> idPaths)
        //{
        //    mLog.Info("TemplateRelationExistByIdPath.");
        //    string idPath = string.Empty;
        //    try
        //    {
        //        using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //        {
        //            connection.Open();
        //            using (var command = connection.CreateCommand())
        //            {
        //                command.CommandText = string.Format(RecordQueryString.TemplateRelationExistByIdPath, RMTemplateRelationshipTableName, RecordDBUtil.BuildInClause(idPaths));
        //                using (SqlDataReader sdr = command.ExecuteReader())
        //                {
        //                    while (sdr.Read())
        //                    {
        //                        if (!sdr.IsDBNull(0))
        //                        {
        //                            idPath = sdr.GetString(0);
        //                            break;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        mLog.Info("Failed TemplateRelationExistByIdPath.Message:{0}. ", ex.ToString());
        //    }
        //    return idPath;
        //}

        //public static Guid GetStartTemplateUniqueId(Guid suiteUniqueId)
        //{
        //    mLog.Info("GetStartTemplateUniqueId.");
        //    Guid templateUniqueId = Guid.Empty;
        //    try
        //    {
        //        using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //        {
        //            connection.Open();
        //            using (var command = connection.CreateCommand())
        //            {
        //                command.CommandText = string.Format(RecordQueryString.GetStartTemplateUniqueId, RMTemplateRelationshipTableName);
        //                command.Parameters.AddWithValue("@Ancestor", suiteUniqueId);
        //                using (SqlDataReader sdr = command.ExecuteReader())
        //                {
        //                    while (sdr.Read())
        //                    {
        //                        if (!sdr.IsDBNull(0))
        //                        {
        //                            templateUniqueId = sdr.GetGuid(0);

        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        mLog.Info("Failed GetStartTemplateUniqueId.Message:{0}. ", ex.ToString());
        //    }
        //    return templateUniqueId;
        //}

        //public static Guid GetSuiteUniqueIdByRootTemplateId(Guid rootTemplatedUniqueId)
        //{
        //    mLog.Info("GetSuiteUniqueIdByRootTemplateId.");
        //    Guid suiteUniqueId = Guid.Empty;
        //    try
        //    {
        //        using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //        {
        //            connection.Open();
        //            using (var command = connection.CreateCommand())
        //            {
        //                command.CommandText = string.Format(RecordQueryString.GetSuiteUniqueIdByRootTemplateId, RMTemplateRelationshipTableName);
        //                command.Parameters.AddWithValue("@Descendant", rootTemplatedUniqueId);
        //                using (SqlDataReader sdr = command.ExecuteReader())
        //                {
        //                    while (sdr.Read())
        //                    {
        //                        if (!sdr.IsDBNull(0))
        //                        {
        //                            suiteUniqueId = sdr.GetGuid(0);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        mLog.Info("Failed GetSuiteUniqueIdByRootTemplateId.Message:{0}. ", ex.ToString());
        //    }
        //    return suiteUniqueId;
        //}

        //public static void AddTemplateRelatonship(TemplateDto templateDto, int templateId, Dictionary<int, Guid> templateIdUniqueIdMapping)
        //{
        //    try
        //    {
        //        var parentTemplateIdList = templateDto.ParentTemplateIdList;
        //        var distance = parentTemplateIdList.Count;
        //        var isFirstOne = true;
        //        var idPath = TemplateUtil.Convert2Path(parentTemplateIdList) + templateId.ToString() + TemplateUtil.IdPathSeprator;
        //        using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //        {
        //            connection.Open();
        //            using (var command = connection.CreateCommand())
        //            {
        //                using (var tran = connection.BeginTransaction())
        //                {
        //                    command.Transaction = tran;
        //                    foreach (var parent in parentTemplateIdList) //first one is suite unique id
        //                    {
        //                        Guid parentUniqueId = isFirstOne ? Guid.Parse(parent) : templateIdUniqueIdMapping[int.Parse(parent)];
        //                        command.CommandText = string.Format(RecordQueryString.AddOneTemplateRelatonship, RMTemplateRelationshipTableName);
        //                        command.Parameters.Clear();
        //                        command.Parameters.AddWithValue("@IdPath", idPath);
        //                        command.Parameters.AddWithValue("@Distance", distance);
        //                        command.Parameters.AddWithValue("@Ancestor", parentUniqueId);
        //                        command.Parameters.AddWithValue("@Descendant", templateDto.uniqueId);
        //                        command.Parameters.AddWithValue("@TemplateType", (int)templateDto.type);
        //                        command.ExecuteNonQuery();
        //                        distance--;
        //                        isFirstOne = false;
        //                    }

        //                    tran.Commit();
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        mLog.Info("Failed AddTemplateRelatonship.Message:{0}. ", ex.ToString());
        //        throw;
        //    }
        //}

        //public static List<RMTemplate> GetAllRMTemplates()
        //{
        //    mLog.Info("GetAllRMTemplates.");
        //    List<RMTemplate> rMTemplates = new List<RMTemplate>();
        //    try
        //    {
        //        using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //        {
        //            connection.Open();
        //            using (var command = connection.CreateCommand())
        //            {
        //                command.CommandText = string.Format(RecordQueryString.GetAllRMTemplates, RMTemplateTableName);
        //                using (SqlDataReader sdr = command.ExecuteReader())
        //                {
        //                    while (sdr.Read())
        //                    {
        //                        RMTemplate rMTemplate = new RMTemplate();
        //                        if (!sdr.IsDBNull(0))
        //                        {
        //                            rMTemplate.Id = sdr.GetInt32(0);
        //                        }
        //                        if (!sdr.IsDBNull(1))
        //                        {
        //                            rMTemplate.UniqueId = sdr.GetGuid(1);
        //                        }
        //                        if (!sdr.IsDBNull(2))
        //                        {
        //                            rMTemplate.Name = sdr.GetString(2);
        //                        }
        //                        if (!sdr.IsDBNull(3))
        //                        {
        //                            rMTemplate.Type = (TemplateType)sdr.GetInt32(3);
        //                        }
        //                        if (!sdr.IsDBNull(4))
        //                        {
        //                            rMTemplate.Prefix = sdr.GetString(4);
        //                        }
        //                        if (!sdr.IsDBNull(5))
        //                        {
        //                            rMTemplate.NumberOfDigits = sdr.GetInt32(5);
        //                        }
        //                        if (!sdr.IsDBNull(6))
        //                        {
        //                            rMTemplate.ParentId = sdr.GetInt32(6);
        //                        }
        //                        if (!sdr.IsDBNull(7))
        //                        {
        //                            rMTemplate.ParentUniqueId = sdr.GetGuid(7);
        //                        }
        //                        if (!sdr.IsDBNull(8))
        //                        {
        //                            rMTemplate.Size = sdr.GetDouble(8);
        //                        }
        //                        if (!sdr.IsDBNull(9))
        //                        {
        //                            rMTemplate.Creater = sdr.GetInt32(9);
        //                        }
        //                        if (!sdr.IsDBNull(10))
        //                        {
        //                            rMTemplate.CreatedOn = sdr.GetDateTime(10);
        //                        }
        //                        if (!sdr.IsDBNull(11))
        //                        {
        //                            rMTemplate.Modifier = sdr.GetInt32(11);
        //                        }
        //                        if (!sdr.IsDBNull(12))
        //                        {
        //                            rMTemplate.LastModifiedOn = sdr.GetDateTime(12);
        //                        }
        //                        if (!sdr.IsDBNull(13))
        //                        {
        //                            rMTemplate.ColumnSchema = sdr.GetString(13);
        //                        }
        //                        rMTemplates.Add(rMTemplate);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        mLog.Info("Failed GetAllRMTemplates.Message:{0}. ", ex.ToString());
        //    }
        //    return rMTemplates;
        //}

        //public static void InsertRMLocationSuiteAssociation(Guid rLocationUniqueId, Guid rSuiteUniqueId)
        //{
        //    using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //    {
        //        connection.Open();
        //        using (var command = connection.CreateCommand())
        //        {
        //            command.CommandText = string.Format(RecordQueryString.InsertRMLocationSuiteAssociation, RMLocationSuiteAssociationTableName);
        //            command.Parameters.AddWithValue("@LocationUniqueId", rLocationUniqueId);
        //            command.Parameters.AddWithValue("@SuiteUniqueId", rSuiteUniqueId);
        //            command.ExecuteNonQuery();
        //        }
        //    }
        //}

        //public static void InsertRMLocationSuiteAssociation(RMLocationSuiteAssociation rMLocationSuiteAssociation)
        //{
        //    using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //    {
        //        connection.Open();
        //        using (var command = connection.CreateCommand())
        //        {
        //            command.CommandText = string.Format(RecordQueryString.InsertRMLocationSuiteAssociation, RMLocationSuiteAssociationTableName);
        //            command.Parameters.AddWithValue("@LocationUniqueId", rMLocationSuiteAssociation.LocationUniqueId);
        //            command.Parameters.AddWithValue("@SuiteUniqueId", rMLocationSuiteAssociation.SuiteUniqueId);
        //            command.ExecuteNonQuery();
        //        }
        //    }
        //}

        //public static void InsertRMSuiteMemberships(RMSuiteMembership rMSuiteMembership)
        //{
        //    using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //    {
        //        connection.Open();
        //        using (var command = connection.CreateCommand())
        //        {
        //            command.CommandText = string.Format(RecordQueryString.InsertRMSuiteMembership, RMSuiteMembershipTableName);
        //            command.Parameters.AddWithValue("@SuiteUniqueId", rMSuiteMembership.SuiteUniqueId);
        //            command.Parameters.AddWithValue("@RootTemplateUniqueId", rMSuiteMembership.RootTemplateUniqueId);
        //            command.Parameters.AddWithValue("@BoxTemplateUniqueId", rMSuiteMembership.BoxTemplateUniqueId);
        //            command.Parameters.AddWithValue("@FolderTemplateUniqueId", rMSuiteMembership.FolderTemplateUniqueId);
        //            command.Parameters.AddWithValue("@RecordTemplateUniqueId", rMSuiteMembership.RecordTemplateUniqueId);
        //            command.ExecuteNonQuery();
        //        }
        //    }
        //}

        //public static void InsertRMSuites(RMSuite rMSuite)
        //{
        //    using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //    {
        //        connection.Open();
        //        using (var command = connection.CreateCommand())
        //        {
        //            command.CommandText = string.Format(RecordQueryString.InsertRMSuite, RMSuitesTableName);
        //            command.Parameters.AddWithValue("@UniqueId", rMSuite.UniqueId);
        //            command.Parameters.AddWithValue("@Name", rMSuite.Name);
        //            command.Parameters.AddWithValue("@Description", rMSuite.Description);
        //            command.Parameters.AddWithValue("@StartFromType", rMSuite.StartFromType);
        //            command.Parameters.AddWithValue("@Creater", rMSuite.Creater);
        //            command.Parameters.AddWithValue("@CreatedOn", rMSuite.CreatedOn);
        //            command.Parameters.AddWithValue("@Modifier", rMSuite.Modifier);
        //            command.Parameters.AddWithValue("@LastModifiedOn", rMSuite.LastModifiedOn);
        //            command.Parameters.AddWithValue("@RootTemplateCreateType", rMSuite.RootTemplateCreateType);
        //            command.ExecuteNonQuery();
        //        }
        //    }
        //}

        //public static void InsertRMSuitesWithRelationship(RMSuite rMSuite, int rootTemplateId, Guid rootTemplateUniqueId)
        //{
        //    using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //    {
        //        connection.Open();
        //        using (var command = connection.CreateCommand())
        //        {
        //            using (var tran = connection.BeginTransaction())
        //            {
        //                command.Transaction = tran;
        //                command.CommandText = string.Format(RecordQueryString.InsertRMSuite, RMSuitesTableName);
        //                command.Parameters.AddWithValue("@UniqueId", rMSuite.UniqueId);
        //                command.Parameters.AddWithValue("@Name", rMSuite.Name);
        //                command.Parameters.AddWithValue("@Description", rMSuite.Description);
        //                command.Parameters.AddWithValue("@StartFromType", rMSuite.StartFromType);
        //                command.Parameters.AddWithValue("@Creater", rMSuite.Creater);
        //                command.Parameters.AddWithValue("@CreatedOn", rMSuite.CreatedOn);
        //                command.Parameters.AddWithValue("@Modifier", rMSuite.Modifier);
        //                command.Parameters.AddWithValue("@LastModifiedOn", rMSuite.LastModifiedOn);
        //                command.Parameters.AddWithValue("@RootTemplateCreateType", rMSuite.RootTemplateCreateType);
        //                command.ExecuteNonQuery();

        //                var suiteIdPath = rMSuite.UniqueId.ToString() + TemplateUtil.IdPathSeprator;
        //                command.CommandText = string.Format(RecordQueryString.AddOneTemplateRelatonship, RMTemplateRelationshipTableName);
        //                command.Parameters.Clear();
        //                command.Parameters.AddWithValue("@IdPath", suiteIdPath);
        //                command.Parameters.AddWithValue("@Distance", 0);
        //                command.Parameters.AddWithValue("@Ancestor", rMSuite.UniqueId);
        //                command.Parameters.AddWithValue("@Descendant", rMSuite.UniqueId);
        //                command.Parameters.AddWithValue("@TemplateType", 6);
        //                command.ExecuteNonQuery();

        //                if (rootTemplateUniqueId != Guid.Empty)
        //                {
        //                    var idPath = TemplateUtil.Convert2Path(new List<string> { rMSuite.UniqueId.ToString(), rootTemplateId.ToString() });
        //                    var startType = rMSuite.StartFromType == SuiteStartFromType.Custom ? TemplateType.Custom : rMSuite.StartFromType == SuiteStartFromType.Box ? TemplateType.Box : TemplateType.Folder;
        //                    command.CommandText = string.Format(RecordQueryString.AddOneTemplateRelatonship, RMTemplateRelationshipTableName);
        //                    command.Parameters.Clear();
        //                    command.Parameters.AddWithValue("@IdPath", idPath);
        //                    command.Parameters.AddWithValue("@Distance", 1);
        //                    command.Parameters.AddWithValue("@Ancestor", rMSuite.UniqueId);
        //                    command.Parameters.AddWithValue("@Descendant", rootTemplateUniqueId);
        //                    command.Parameters.AddWithValue("@TemplateType", (int)startType);
        //                    command.ExecuteNonQuery();
        //                }

        //                tran.Commit();
        //            }
        //        }
        //    }
        //}

        //public static void InsertRMTemplates(RMTemplate rMTemplate)
        //{
        //    using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //    {
        //        connection.Open();
        //        using (var command = connection.CreateCommand())
        //        {
        //            command.CommandText = string.Format(RecordQueryString.InsertRMTemplate, RMTemplateTableName);
        //            command.Parameters.AddWithValue("@UniqueId", rMTemplate.UniqueId);
        //            command.Parameters.AddWithValue("@Name", rMTemplate.Name);
        //            command.Parameters.AddWithValue("@Type", rMTemplate.Type);
        //            command.Parameters.AddWithValue("@Prefix", rMTemplate.Prefix);
        //            command.Parameters.AddWithValue("@NumberOfDigits", rMTemplate.NumberOfDigits);
        //            command.Parameters.AddWithValue("@ParentId", rMTemplate.ParentId);
        //            command.Parameters.AddWithValue("@ParentUniqueId", rMTemplate.ParentUniqueId);
        //            command.Parameters.AddWithValue("@Size", rMTemplate.Size);
        //            command.Parameters.AddWithValue("@Creater", rMTemplate.Creater);
        //            command.Parameters.AddWithValue("@CreatedOn", rMTemplate.CreatedOn);
        //            command.Parameters.AddWithValue("@Modifier", rMTemplate.Modifier);
        //            command.Parameters.AddWithValue("@LastModifiedOn", rMTemplate.LastModifiedOn);
        //            command.Parameters.AddWithValue("@ColumnSchema", rMTemplate.ColumnSchema);
        //            command.Parameters.AddWithValue("@Description", rMTemplate.Description);
        //            command.ExecuteNonQuery();
        //        }
        //    }
        //}

        //public static void AddPushColumnToFold(TemplateDto resultDto, Guid boxId)
        //{
        //    Record box = PhysicalConfiguration.PhysicalConfigurationForExplore.ExplorerDAO.GetPhysicalRecordById(boxId);
        //    if (box == null)
        //    {
        //        mLog.Error("Can't find fold's parent box,box id is {0}", boxId.ToString());
        //        return;
        //    }
        //    RMTemplate boxTemplate = GetTemplateByIdToDto(box.TemplateId);
        //    if (boxTemplate == null)
        //    {
        //        mLog.Error("Can't find box's template ,template id is {0}", box.TemplateId.ToString());
        //        return;
        //    }
        //    var columnSchema = boxTemplate.ColumnSchema;
        //    TemplateColumnsSchema schema = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(columnSchema);
        //    List<ColumnXmlSchema> columns = schema.Columns;
        //    for (int i = 0; i < columns.Count; i++)
        //    {
        //        var item = columns[i];
        //        if ((item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.PushToChild) == (int)TemplateInheritSettingEnum.PushToChild)
        //        {
        //            List<TemplateIdAndCategoryId> pushFoldTemplateCategoriesId = item.pushFoldTemplateCategoriesId;
        //            if (pushFoldTemplateCategoriesId != null && pushFoldTemplateCategoriesId.Count > 0)
        //            {
        //                TemplateIdAndCategoryId templateCategoryId = pushFoldTemplateCategoriesId.Find(t => t.tempalteId == resultDto.uniqueId.ToString());
        //                if (templateCategoryId != null)
        //                {
        //                    foreach (var category in resultDto.categories)
        //                    {
        //                        if (category.id.ToString() == templateCategoryId.categoryId)
        //                        {
        //                            TemplateColumnDto columnDto = ConvertToPageColumnDto(item);
        //                            category.columns.Add(columnDto);
        //                        }
        //                    }
        //                }
        //                //如果没有存储当前sub template的信息,则把push column add到默认category里 即第一个
        //                else
        //                {
        //                    TemplateColumnDto columnDto = ConvertToPageColumnDto(item);
        //                    resultDto.categories[0].columns.Add(columnDto);
        //                }
        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// 更新PushColumn DB.
        ///// </summary>
        //public static void UpdatePushColumnToFold(RMTemplate template, Guid boxId)
        //{
        //    bool needUpdate = false;
        //    Record box = PhysicalConfiguration.PhysicalConfigurationForExplore.ExplorerDAO.GetPhysicalRecordById(boxId);
        //    if (box == null)
        //    {
        //        mLog.Error("Can't find fold's parent box,box id is {0}", boxId.ToString());
        //        return;
        //    }
        //    RMTemplate boxTemplate = GetTemplateByIdToDto(box.TemplateId);
        //    if (boxTemplate == null)
        //    {
        //        mLog.Error("Can't find box's template ,template id is {0}", box.TemplateId.ToString());
        //        return;
        //    }
        //    var columnSchema = boxTemplate.ColumnSchema;
        //    TemplateColumnsSchema schema = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(columnSchema);
        //    List<ColumnXmlSchema> columns = schema.Columns;
        //    for (int i = 0; i < columns.Count; i++)
        //    {
        //        var item = columns[i];
        //        if ((item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.PushToChild) == (int)TemplateInheritSettingEnum.PushToChild)
        //        {
        //            List<TemplateIdAndCategoryId> pushFoldTemplateCategoriesId = item.pushFoldTemplateCategoriesId;
        //            if (pushFoldTemplateCategoriesId != null && pushFoldTemplateCategoriesId.Count > 0)
        //            {
        //                TemplateIdAndCategoryId templateCategoryId = pushFoldTemplateCategoriesId.Find(t => t.tempalteId == template.UniqueId.ToString());
        //                if (templateCategoryId == null)
        //                {
        //                    needUpdate = true;
        //                    List<RMTemplateCategory> categories = LoadCategories(template.UniqueId);
        //                    schema.Columns[i].pushFoldTemplateCategoriesId.Add(new TemplateIdAndCategoryId() { tempalteId = template.UniqueId.ToString(), categoryId = categories.FirstOrDefault().UniqueId.ToString() });
        //                }
        //            }
        //        }
        //    }
        //    if (needUpdate)
        //    {
        //        UpdateTemplateColumnsSchema(SerializerHelper.SerializeByDataContractSerializer(schema), boxTemplate.UniqueId);
        //    }
        //}

        //private static void UpdateTemplateColumnsSchema(string schema, Guid templateId)
        //{
        //    using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //    {
        //        connection.Open();
        //        using (var command = connection.CreateCommand())
        //        {
        //            command.CommandText = string.Format(RecordQueryString.UpdateTemplateColumnsSchema, RMTemplateTableName);
        //            command.Parameters.AddWithValue("@ColumnSchema", schema);
        //            command.Parameters.AddWithValue("@UniqueId", templateId);
        //            command.ExecuteNonQuery();
        //        }
        //    }
        //}

        //public static TemplateColumnDto ConvertToPageColumnDto(ColumnXmlSchema item)
        //{
        //    var columnDto = new TemplateColumnDto()
        //    {
        //        categoryId = item.CategoryId,
        //        columnName = item.Name,
        //        uniqueId = item.UniqueId,
        //        required = item.Required,
        //        typeId = (int)item.ColumnType,
        //        showInEditForm = item.ShowInEditForm,
        //        allowEdit = item.AllowEdit,
        //        inheritFromParent = true,
        //        inheritFromParentFolder = false,
        //        pushToChild = (item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.PushToChild) == (int)TemplateInheritSettingEnum.PushToChild,
        //        //childInheritsValue = (item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.ChildInheritsValue) == (int)TemplateInheritSettingEnum.ChildInheritsValue,
        //        allowModifyValue = (item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.AllowModifyValue) == (int)TemplateInheritSettingEnum.AllowModifyValue,
        //        pushFoldTemplateCategoriesId = item.pushFoldTemplateCategoriesId,
        //        pushRecordTemplateCategoriesId = item.pushRecordTemplateCategoriesId,
        //    };
        //    //RECO-4254
        //    if (item.UniqueId == new Guid(DefaultColumnIDs.Description))
        //    {
        //        columnDto.allowEdit = true;
        //    }
        //    switch (item.ColumnType)
        //    {
        //        case ColumnType.SingleText:
        //        case ColumnType.MultipleText:
        //        case ColumnType.DateTime:
        //        case ColumnType.PeopleOrGroup:
        //        case ColumnType.Number:
        //            break;
        //        case ColumnType.Taxonomy:
        //            break;
        //        case ColumnType.SingleChoice:
        //        case ColumnType.MultipleChoice:
        //            columnDto.optionsJSON = item.OptionsJSON;
        //            columnDto.optionsMaxIdReachedValue = item.OptionsMaxIdReachedValue;
        //            break;
        //        default:
        //            break;
        //    }
        //    return columnDto;
        //}

        private static List<RMEXOLabel> GetAllRMEXOLabels()
        {
            mLog.Info("GetAllRMEXOLabels.");
            List<RMEXOLabel> mRMEXOLabels = new List<RMEXOLabel>();
            
            try
            {
                mRMEXOLabels = EXOLabelDao.FindAll();
            }
            catch (Exception ex)
            {
                mLog.Info("Failed GetAllRMEXOLabels.Message:{0}. ", ex.ToString());
            }
            return mRMEXOLabels;
        }

        //public static void DeleteDifferentScopeDataFromManualApproveTable(string partitionKey, string nodeId)
        //{
        //    using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //    {
        //        connection.Open();
        //        using (var command = connection.CreateCommand())
        //        {
        //            command.CommandText = string.Format(RecordQueryString.DeleteDifferentScopeDataFromManualApproveTable, RMManualApprovesTableName);
        //            command.Parameters.AddWithValue("@NodeId", nodeId);
        //            command.Parameters.AddWithValue("@PartKey", partitionKey);
        //            command.ExecuteNonQuery();
        //        }
        //    }
        //}

        //public static void UpdateRMScopePermission(RMScopePermission rMScopePermission)
        //{
        //    using (var connection = new SqlConnection(RecordsTenantDBConnectionString))
        //    {
        //        connection.Open();
        //        using (var command = connection.CreateCommand())
        //        {
        //            command.CommandText = string.Format(RecordQueryString.UpdateRMScopePermission, RMScopePermissionsTableName);
        //            command.Parameters.AddWithValue("@Id", rMScopePermission.Id);
        //            command.Parameters.AddWithValue("@Scope", rMScopePermission.Scope);
        //            command.Parameters.AddWithValue("@ParentScope", rMScopePermission.ParentScope);
        //            command.Parameters.AddWithValue("@ScopePath", rMScopePermission.ScopePath);
        //            command.ExecuteNonQuery();
        //        }
        //    }
        //}
    }
}