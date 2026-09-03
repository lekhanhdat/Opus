using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Myhub.Model.QueryRequest.Views;
using AvePoint.RA.Contract.MyHub.Items.Views;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Myhub.Actions
{
    public class RMMyhubPendingDisposalFolderFilterMethod
    {
        public (string sql, List<SqlParameter> parameter) GetAllPendingDisposalFolderFilter(RMMyhubPendingDisposalFolderFilterQueryInfo queryInfo, string dirPath)
        {
            var sql = BaseSql();
            var parameter = BaseSqlParameters(queryInfo.PartitionKeyId, dirPath);
            if (!string.IsNullOrWhiteSpace(queryInfo.SearchValue))
            {
                sql += $" AND (CONTAINS(c.leafName,@SearchValue)) ";
                parameter.Add(new SqlParameter("@SearchValue", queryInfo.SearchValue));
            }
            return (sql, parameter);
        }
        private string BaseSql()
        {
            return @"SELECT VALUE {
    ""NodeId"":c.nodeId,
    ""PartitionKeyId"": c.l2PartitionKey,
    ""Name"": c.leafName,
    ""Path"": CONCAT(c.dirPath, '\\' ,c.leafName)
}
FROM c
WHERE c.sourceFlag = @sourceFlag
AND c.l2PartitionKey=@l2PartitionKey
AND c.nodeType = @nodeType
AND STARTSWITH(c.dirPath, @folderPath)
AND c.recordStatus=@statuses
AND IS_DEFINED(c.recordsId)
AND NOT IS_NULL(c.recordsId)";
        }
        private List<SqlParameter> BaseSqlParameters(string partitionKeyId, string dirPath)
        {
            return new List<SqlParameter>
            {
        new SqlParameter("@sourceFlag", (int)SourceFlag.FileSystem),
        new SqlParameter("@nodeType", (int)NodeLevel.FSFolder),
        new SqlParameter("@statuses", (int)RMRecordStatus.Active),
        new SqlParameter("@l2PartitionKey", partitionKeyId.ToLowerInvariant()),
        new SqlParameter("@folderPath", dirPath),
            };
        }
        public (string sql, List<SqlParameter> parameter) GetPendingDisposalFolderFilterPath(string partitionKeyId, string nodeId, bool isFullPath = false)
        {
            var sql = BaseGetPendingDisposalFolderFilterPathSql();
            if (isFullPath)
            {
                sql = BaseGetPendingDisposalFolderFilterFullPathSql();
            }
            var parameter = BaseGetPendingDisposalFolderFilterPathSqlParameters(partitionKeyId, nodeId);
            return (sql, parameter);
        }
        private string BaseGetPendingDisposalFolderFilterPathSql()
        {
            return @"SELECT VALUE c.dirPath
FROM c
WHERE c.sourceFlag = @sourceFlag
AND c.l2PartitionKey = @l2PartitionKey
AND c.nodeId=@nodeId
AND c.recordStatus = @statuses";

        }
        private string BaseGetPendingDisposalFolderFilterFullPathSql()
        {
            return @"SELECT VALUE CONCAT(c.dirPath, '\\' ,c.leafName)
FROM c
WHERE c.sourceFlag = @sourceFlag
AND c.l2PartitionKey = @l2PartitionKey
AND c.nodeId=@nodeId
AND c.recordStatus = @statuses";
        }
        private List<SqlParameter> BaseGetPendingDisposalFolderFilterPathSqlParameters(string partitionKeyId, string nodeId)
        {
            var sqlParameters = new List<SqlParameter>
            {
                new SqlParameter("@sourceFlag", (int)SourceFlag.FileSystem),
                new SqlParameter("@l2PartitionKey", partitionKeyId),
                new SqlParameter("@nodeId", nodeId),
                new SqlParameter("@statuses", (int)RMRecordStatus.Active),
            };
            return sqlParameters;
        }
    }
}
