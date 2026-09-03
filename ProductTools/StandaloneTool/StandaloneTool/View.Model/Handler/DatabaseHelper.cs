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
using Amazon.Runtime.Internal.Transform;
using AvePoint.Media.Core.Index;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.CommonUtil;
using StandaloneTool.Model.Common;

namespace StandaloneTool.View.Model.Handler
{
    public sealed class DatabaseHelper
    {
        private RALogger logger = RALogger.GetInstance(typeof(DatabaseHelper));
        private static readonly Lazy<DatabaseHelper> instance = new(() => new DatabaseHelper());
        private readonly IndexDatabaseHelper coreDB = new();

        public static DatabaseHelper Instance => instance.Value;

        private DatabaseHelper() { }

        public void Open(string cnn, string pwd)
        {
            coreDB.Open(cnn, pwd);
        }

        public List<ArchiverSiteMasterIndexExportDto> GetAllArchiverSites(Module module)
        {
            const string command = "SELECT MAX(ArchiverTime),* FROM ArchiverSiteMasterIndexes WHERE SourceFlag = @SourceFlag GROUP BY SiteURL";
            var param = new Dictionary<string, object> { { "SourceFlag", (int)module } };
            return coreDB.ExecuteReader<ArchiverSiteMasterIndexExportDto>(command, param) ?? new();
        }

        public List<ArchiverSiteMasterIndexExportDto> GetAllTeamsArchiver(Module module)
        {
            const string command = "SELECT MAX(ArchiverTime),* FROM CommonSiteMasterIndex WHERE DataType = @DataType GROUP BY SiteURL";
            var param = new Dictionary<string, object> { { "DataType", (int)module } };
            var result = coreDB.ExecuteReader<CommonSiteMasterIndexExportDto>(command, param) ?? new();
            return result.Select(_ => new ArchiverSiteMasterIndexExportDto
            {
                SiteId = _.SiteId,
                JobId = _.JobId,
                GroupMailboxAddress = _.SiteURL,
                SiteURL = _.SiteURL
            }).ToList();
        }

        public List<ArchiverSiteMasterIndexExportDto> GetAllTeamsArchiverInArchiverSiteMasterIndex(Module module)
        {
            const string command = "SELECT MAX(ArchiverTime),* FROM ArchiverSiteMasterIndexes WHERE SourceFlag = @SourceFlag and GroupMailBoxAddress is not null GROUP BY GroupMailBoxAddress";
            var param = new Dictionary<string, object> { { "SourceFlag", (int)module } };
            return coreDB.ExecuteReader<ArchiverSiteMasterIndexExportDto>(command, param) ?? new();
        }

        public List<ArchiverSiteMasterIndexExportDto> GetArchiverSiteMasterIndexesByGroupAddressAndJobId(List<string> groupAddress, List<string> jobIds)
        {
            if (!groupAddress.Any() && !jobIds.Any())
            {
                return new List<ArchiverSiteMasterIndexExportDto>();
            }

            var conditions = new List<string>();
            var param = new Dictionary<string, object>();

            if (jobIds.Any())
            {
                var jobIdParameters = jobIds.Select((id, index) => $"JobId LIKE @JobId{index}").ToList();
                conditions.Add($"({string.Join(" OR ", jobIdParameters)})");
                for (int i = 0; i < jobIds.Count; i++)
                {
                    param[$"@JobId{i}"] = jobIds[i] + "%";
                }
            }

            if (groupAddress.Any())
            {
                var groupAddressParameters = groupAddress.Select((address, index) => $"@GroupAddress{index}").ToList();
                conditions.Add($"GroupMailboxAddress IN ({string.Join(", ", groupAddressParameters)})");
                for (int i = 0; i < groupAddress.Count; i++)
                {
                    param[$"@GroupAddress{i}"] = groupAddress[i];
                }
            }

            string whereClause = string.Join(" OR ", conditions);
            string command = $@" SELECT MAX(ArchiverTime),* FROM ArchiverSiteMasterIndexes WHERE {whereClause} GROUP BY SiteUrl";
            return coreDB.ExecuteReader<ArchiverSiteMasterIndexExportDto>(command, param) ?? new();
        }

        public List<CommonSiteMasterIndexExportDto> GetCommonArchiverSitesBySiteURLs(Module module, List<string> siteURLs)
        {
            if (!siteURLs.Any())
            {
                return new List<CommonSiteMasterIndexExportDto>();
            }

            var siteUrlParameters = siteURLs.Select((url, index) => $"@SiteURL{index}").ToList();
            string command = $"SELECT MAX(ArchiverTime),* FROM CommonSiteMasterIndex WHERE DataType = @DataType AND SiteURL IN ({string.Join(", ", siteUrlParameters)}) GROUP BY SiteURL";

            var param = new Dictionary<string, object> { { "DataType", (int)module } };
            for (int i = 0; i < siteURLs.Count; i++)
            {
                param[$"@SiteURL{i}"] = siteURLs[i];
            }

            return coreDB.ExecuteReader<CommonSiteMasterIndexExportDto>(command, param) ?? new();
        }

        public bool CheckUsingAveStorage(IEnumerable<string> siteUrls)
        {
            try
            {
                if (!siteUrls.Any()) return false;

                const int batchSize = 100;
                var siteUrlList = siteUrls.ToList();
                var allJobIdPrefixes = QueryArchiverMasterIndexJobIdByUrls(batchSize, siteUrlList);

                var distinctJobIdPrefixes = allJobIdPrefixes.Distinct().ToList();

                if (!distinctJobIdPrefixes.Any())
                {
                    return false;
                }

                bool isUsingAveStorage = CheckJobUsingAveStorage(batchSize, distinctJobIdPrefixes);

                var settingProfiles = coreDB.ExecuteReader<SettingProfileExportDto>("SELECT * FROM SettingProfiles WHERE Name = @Name", new Dictionary<string, object> { { "@Name", "UsingIndexDevice" } });
                GlobalInfo.IsUsingAveStorage = settingProfiles.First().Settings.Equals(GlobalInfo.AVEPOINT_STORAGE_ID, StringComparison.OrdinalIgnoreCase) || isUsingAveStorage;
                return GlobalInfo.IsUsingAveStorage;
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while check using avepoint storage. Ex: {ex}");
                return false;
            }
        }

        private bool CheckJobUsingAveStorage(int batchSize, List<string> distinctJobIdPrefixes)
        {

            for (int i = 0; i < distinctJobIdPrefixes.Count; i += batchSize)
            {
                var batch = distinctJobIdPrefixes.Skip(i).Take(batchSize).ToList();
                var jobIdParameters = batch.Select((prefix, index) => $"@JobIdPrefix{index}").ToList();
                string query = $"SELECT * FROM ArchiverIndexSubInfoes WHERE SubJobId in ({string.Join(",", jobIdParameters)})";

                var jobIdParam = new Dictionary<string, object>();
                for (int j = 0; j < batch.Count; j++)
                {
                    jobIdParam[$"@JobIdPrefix{j}"] = batch[j];
                }

                var domains = coreDB.ExecuteReader<ArchiverIndexSubInfoExportDto>(query, jobIdParam);
                if (domains.Any(d => d.StorageId.Equals(GlobalInfo.AVEPOINT_STORAGE_ID, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            return false;
        }

        private List<string> QueryArchiverMasterIndexJobIdByUrls(int batchSize, List<string> siteUrlList)
        {
            List<string> allJobIdPrefixes = new();
            for (int i = 0; i < siteUrlList.Count; i += batchSize)
            {
                var batch = siteUrlList.Skip(i).Take(batchSize).ToList();
                var siteUrlParameters = batch.Select((url, index) => $"@SiteURL{index}").ToList();
                string command = $"SELECT * FROM ArchiverSiteMasterIndexes WHERE SiteURL IN ({string.Join(", ", siteUrlParameters)}) AND SourceFlag = @SourceFlag";
                string commonSiteMaxterCommand = $"SELECT * FROM CommonSiteMasterIndex WHERE SiteURL IN ({string.Join(", ", siteUrlParameters)}) AND DataType = @SourceFlag";

                var param = new Dictionary<string, object>();
                for (int j = 0; j < batch.Count; j++)
                {
                    param[$"@SiteURL{j}"] = batch[j];
                }
                param["SourceFlag"] = (int)GlobalInfo.Module;

                var selectedSiteIndexs = coreDB.ExecuteReader<ArchiverSiteMasterIndexExportDto>(command, param) ?? [];
                var selectedCommonIndexs = coreDB.ExecuteReader<CommonSiteMasterIndexExportDto>(commonSiteMaxterCommand, param) ?? [];

                var jobIds = selectedSiteIndexs.Select(id => id.JobId)
                    .Concat(selectedCommonIndexs.Select(id => id.JobId))
                    .ToList();

                allJobIdPrefixes.AddRange(jobIds);
            }
            return allJobIdPrefixes;
        }
    }
}
