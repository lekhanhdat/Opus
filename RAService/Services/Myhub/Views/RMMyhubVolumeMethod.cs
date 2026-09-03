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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Service.Services.MyHub.NewMethods;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Myhub.Views
{
    public class RMMyhubVolumeMethod
    {
        private RMMyhubQueryRecordsMethod _recordStore;
        private RMMyhubQueryRecordsMethod RecordStore => _recordStore ??= new RMMyhubQueryRecordsMethod();
        public (string sql, List<SqlParameter> parameter) GetDrivesVolume()
        {
            var sql = BaseGetDrivesVolumeSql();
            var parameter = BaseGetVolumeSqlParameters();
            return (sql, parameter);
        }

        public (string sql, List<SqlParameter> parameter) GetFolderAndFileVolume(string partitionKeyId, string folderFullPath)
        {
            var sql = BaseGetFolderAndFileVolumeSql();
            var parameter = BaseGetFolderAndFileVolumeSqlParameters(partitionKeyId, folderFullPath);
            return (sql, parameter);
        }

        public (string sql, List<SqlParameter> parameter) GetFolderSizeAndFileVolume(string partitionKeyId, string folderFullPath)
        {
            var sql = BaseGetFolderSizeAndFileVolumeSql();
            var parameter = BaseGetFolderSizeAndFileVolumeSqlParameters(partitionKeyId, folderFullPath);
            return (sql, parameter);
        }

        public (string sql, List<SqlParameter> parameters) GetFolderStatisticsBatch(
            string partitionKeyId,
            IReadOnlyCollection<string> folderPaths)
        {
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@sourceFlag", (int)SourceFlag.FileSystem),
                new SqlParameter("@statuses", (int)RMRecordStatus.Active),
                new SqlParameter("@l2PartitionKey", partitionKeyId.ToLowerInvariant()),
                new SqlParameter("@fileNodeType", (int)NodeLevel.FSFile)
            };
            var pathConditions = new List<string>();
            var index = 0;

            foreach (var folderPath in folderPaths)
            {
                var pathParameter = $"@folderPath{index}";
                var prefixParameter = $"@folderPathPrefix{index}";
                var normalizedPath = folderPath.TrimEnd('\\');

                pathConditions.Add(
                    $"(c.dirPath = {pathParameter} OR STARTSWITH(c.dirPath, {prefixParameter}))");
                parameters.Add(new SqlParameter(pathParameter, normalizedPath));
                parameters.Add(new SqlParameter(prefixParameter, normalizedPath + "\\"));
                index++;
            }

            var sql = $@"
SELECT c.dirPath AS DirPath, c.jpmcFileSize AS FileSize
FROM c
WHERE c.sourceFlag = @sourceFlag
AND c.l2PartitionKey = @l2PartitionKey
AND c.recordStatus = @statuses
AND c.nodeType = @fileNodeType
AND IS_DEFINED(c.jpmcFileSize)
AND ({string.Join(" OR ", pathConditions)})";

            return (sql, parameters);
        }

        public (string sql, List<SqlParameter> parameter) GetPendingDisposalPath(string partitionKeyId, Guid nodeId)
        {
            var sql = BaseGetPendingDisposalPathSql();
            var parameter = BaseGetPendingDisposalPathSqlParameters(partitionKeyId, nodeId);
            return (sql, parameter);
        }
        public (string sql, List<SqlParameter> parameter) GetPendingDisposalVolume(string partitionKeyId, string fullPath)
        {
            var sql = BaseGetPendingDisposalSql();
            var parameter = BaseGetPendingDisposalSqlParameters(partitionKeyId, fullPath);
            return (sql, parameter);
        }

        private string BaseGetFolderAndFileVolumeSql()
        {
            return @"SELECT 
    SUM(IIF(c.nodeType = @fileNodeType, 1, 0)) AS FileVolume,
    SUM(IIF(c.nodeType = @folderNodeType, 1, 0)) AS FolderVolume
FROM c
WHERE c.sourceFlag = @sourceFlag
AND c.l2PartitionKey = @l2PartitionKey
AND c.recordStatus = @statuses
AND (
            c.dirPath = @folderFullPath
            OR STARTSWITH(c.dirPath, @fullPathWithSeparator)
        )";
        }

        private List<SqlParameter> BaseGetFolderAndFileVolumeSqlParameters(string partitionKeyId, string folderFullPath)
        {
            var fullPathWithSeparator = folderFullPath.EndsWith("\\") ? folderFullPath : folderFullPath + "\\";
            var sqlParameters = new List<SqlParameter>
            {
        new SqlParameter("@sourceFlag", (int)SourceFlag.FileSystem),
        new SqlParameter("@statuses", (int)RMRecordStatus.Active),
        new SqlParameter("@l2PartitionKey", partitionKeyId.ToLowerInvariant()),
        new SqlParameter("@fileNodeType", (int)NodeLevel.FSFile),
        new SqlParameter("@folderNodeType", (int)NodeLevel.FSFolder),
        new SqlParameter("@folderFullPath", folderFullPath),
        new SqlParameter("@fullPathWithSeparator", fullPathWithSeparator)
            };
            return sqlParameters;
        }
        private string BaseGetDrivesVolumeSql()
        {
            return @"SELECT 
    SUM(IIF(c.nodeType = @fileNodeType, 1, 0)) AS FileVolume,
    SUM(IIF(c.nodeType = @folderNodeType, 1, 0)) AS FolderVolume
FROM c
WHERE c.sourceFlag = @sourceFlag
AND c.recordStatus = @statuses";
        }

        private string BaseGetFolderSizeAndFileVolumeSql()
        {
            return @"
        SELECT
            COUNT(1) AS FileCount,
            SUM(c.jpmcFileSize) AS TotalSize
        FROM c
        WHERE c.sourceFlag = @sourceFlag
        AND c.l2PartitionKey = @l2PartitionKey
        AND c.recordStatus = @statuses
        AND c.nodeType = @fileNodeType
        AND IS_DEFINED(c.jpmcFileSize)
        AND (
            c.dirPath = @folderFullPath
            OR STARTSWITH(c.dirPath, @fullPathWithSeparator)
        )";
        }

        private List<SqlParameter> BaseGetFolderSizeAndFileVolumeSqlParameters(string partitionKeyId, string folderFullPath)
        {
            var fullPathWithSeparator = folderFullPath.EndsWith("\\") ? folderFullPath : folderFullPath + "\\";
            var sqlParameters = new List<SqlParameter>
            {
                new SqlParameter("@sourceFlag", (int)SourceFlag.FileSystem),
                new SqlParameter("@statuses", (int)RMRecordStatus.Active),
                new SqlParameter("@l2PartitionKey", partitionKeyId.ToLowerInvariant()),
                new SqlParameter("@folderFullPath", folderFullPath),
                new SqlParameter("@fullPathWithSeparator", fullPathWithSeparator),
                new SqlParameter("@fileNodeType", (int)NodeLevel.FSFile)
            };
            return sqlParameters;
        }


        private List<SqlParameter> BaseGetVolumeSqlParameters()
        {
            var sqlParameters = new List<SqlParameter>
            {
                new SqlParameter("@sourceFlag", (int)SourceFlag.FileSystem),
                new SqlParameter("@statuses", (int)RMRecordStatus.Active),
                new SqlParameter("@fileNodeType", (int)NodeLevel.FSFile),
                new SqlParameter("@folderNodeType", (int)NodeLevel.FSFolder)
            };
            return sqlParameters;
        }

        private string BaseGetPendingDisposalPathSql()
        {
            return @"SELECT VALUE CONCAT(c.dirPath, ""\\"", c.leafName)
FROM c
WHERE c.sourceFlag = @sourceFlag
AND c.l2PartitionKey = @l2PartitionKey
AND c.nodeId=@nodeId
AND c.recordStatus = @statuses";

        }

        private List<SqlParameter> BaseGetPendingDisposalPathSqlParameters(string partitionKeyId, Guid nodeId)
        {
            var sqlParameters = new List<SqlParameter>
            {
                new SqlParameter("@sourceFlag", (int)SourceFlag.FileSystem),
                new SqlParameter("@l2PartitionKey", partitionKeyId),
                new SqlParameter("@nodeId",nodeId),
                new SqlParameter("@statuses", (int)RMRecordStatus.Active),
            };
            return sqlParameters;
        }
        private string BaseGetPendingDisposalSql()
        {
            return @"SELECT VALUE COUNT(1)
FROM c
WHERE c.sourceFlag = @sourceFlag
AND c.l2PartitionKey = @l2PartitionKey
AND c.nodeType = @nodeType
AND c.manual_approvedStatus=@approvedStatus
AND (
         c.manual_fullPath = @fullPath
         OR STARTSWITH(c.manual_fullPath, @fullPathWithSeparator)
        )
AND c.recordStatus = @statuses";
        }

        private List<SqlParameter> BaseGetPendingDisposalSqlParameters(string partitionKeyId, string fullPath)
        {
            var fullPathWithSeparator = fullPath.EndsWith("\\") ? fullPath : fullPath + "\\";
            var sqlParameters = new List<SqlParameter>
            {
                new SqlParameter("@sourceFlag", (int)SourceFlag.FileSystem),
                new SqlParameter("@l2PartitionKey", partitionKeyId),
                new SqlParameter("@statuses", (int)RMRecordStatus.Active),
                new SqlParameter("@nodeType", (int)NodeLevel.FSFile),
                new SqlParameter("@fullPath",fullPath),
                new SqlParameter("@fullPathWithSeparator", fullPathWithSeparator),
                new SqlParameter("@approvedStatus", (int)SOApproveDBStatus.WaitingApprove)
            };
            return sqlParameters;
        }

        public async Task<Dictionary<Guid, int>> GetChildFoldersVolumeAsync(string partitionKeyId, Guid parentNodeId)
        {
            var parentPath = await GetFolderPathAsync(partitionKeyId, parentNodeId);

            if (string.IsNullOrEmpty(parentPath))
                return new Dictionary<Guid, int>();

            var childFolders = await GetDirectChildFoldersAsync(partitionKeyId, parentPath);

            if (!childFolders.Any())
                return new Dictionary<Guid, int>();

            var allFiles = await GetAllFilesUnderFoldersAsync(partitionKeyId, childFolders);

            var result = childFolders.ToDictionary(f => f.NodeId, f => 0);

            foreach (var file in allFiles)
            {
                var filePath = (string)file.dirPath;

                // 找到该文件所属的直接子文件夹
                var matchedFolder = childFolders.Where(f => filePath.StartsWith(f.FullPath + "\\")).OrderByDescending(f => f.FullPath.Length).FirstOrDefault();

                if (matchedFolder.NodeId != Guid.Empty)
                {
                    result[matchedFolder.NodeId]++;
                }
            }

            return result;
        }
        private async Task<List<(Guid NodeId, string FullPath)>> GetDirectChildFoldersAsync(string partitionKeyId, string parentPath)
        {
            // 匹配路径：Path\子文件夹，后面不接\
            var sql = @"
        SELECT 
            f.nodeId AS NodeId,
            f.dirPath AS FullPath
        FROM c f
        WHERE f.l2PartitionKey = @l2PartitionKey
        AND STARTSWITH(f.dirPath, CONCAT(@parentPath, '\\'))
        AND f.nodeType = @folderNodeType
        AND f.dirPath NOT LIKE CONCAT(@parentPath, '\\%\\%')";

            var parameters = new List<SqlParameter>
    {
        new SqlParameter("@l2PartitionKey", partitionKeyId),
        new SqlParameter("@parentPath", parentPath),
        new SqlParameter("@folderNodeType", (int)NodeLevel.FSFolder)
    };

            var childFolders = new List<(Guid NodeId, string FullPath)>();
            string continuationToken = null;

            do
            {
                var (batch, token) = await RecordStore.QueryAsync<dynamic>(
                    sql, parameters, continuationToken);

                foreach (var item in batch)
                {
                    childFolders.Add(((Guid)item.NodeId, (string)item.FullPath));
                }
                continuationToken = token;
            }
            while (continuationToken != null);

            return childFolders;
        }
        private async Task<string> GetFolderPathAsync(string partitionKeyId, Guid nodeId)
        {
            var sql = @"
        SELECT VALUE f.dirPath
        FROM c f
        WHERE f.l2PartitionKey = @l2PartitionKey
        AND f.nodeId = @nodeId
        AND f.nodeType = @folderNodeType";

            var parameters = new List<SqlParameter>
    {
        new SqlParameter("@l2PartitionKey", partitionKeyId),
        new SqlParameter("@nodeId", nodeId),
        new SqlParameter("@folderNodeType", (int)NodeLevel.FSFolder)
    };

            return await RecordStore.QuerySingleAsync<string>(sql, parameters);
        }
        private async Task<List<dynamic>> GetAllFilesUnderFoldersAsync(string partitionKeyId, List<(Guid NodeId, string FullPath)> folders)
        {
            if (!folders.Any())
                return new List<dynamic>();

            //  OR STARTSWITH
            var orConditions = new List<string>();
            var parameters = new List<SqlParameter>
    {
        new SqlParameter("@sourceFlag", (int)SourceFlag.FileSystem),
        new SqlParameter("@l2PartitionKey", partitionKeyId),
        new SqlParameter("@nodeType", (int)NodeLevel.FSFile),
        new SqlParameter("@statuses", (int)RMRecordStatus.Active),
        new SqlParameter("@disposalStatus", (int)SOApproveDBStatus.None)
    };

            for (int i = 0; i < folders.Count; i++)
            {
                var pathParam = $"@folderPath{i}";
                orConditions.Add($"STARTSWITH(c.dirPath, CONCAT({pathParam}, '\\\\'))");
                parameters.Add(new SqlParameter(pathParam, folders[i].FullPath));
            }

            var sql = $@"
        SELECT 
            c.dirPath
        FROM c
        WHERE c.sourceFlag = @sourceFlag
        AND c.l2PartitionKey = @l2PartitionKey
        AND c.nodeType = @nodeType
        AND c.recordStatus = @statuses
        AND c.disposalStatus = @disposalStatus
        AND ({string.Join(" OR ", orConditions)})";

            var allFiles = new List<dynamic>();
            string continuationToken = null;

            do
            {
                var (batch, token) = await RecordStore.QueryAsync<dynamic>(
                    sql, parameters, continuationToken);

                allFiles.AddRange(batch);
                continuationToken = token;
            }
            while (continuationToken != null);

            return allFiles;
        }
    }
}
