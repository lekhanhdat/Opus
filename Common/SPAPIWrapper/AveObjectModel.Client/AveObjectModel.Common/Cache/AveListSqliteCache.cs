using AvePoint.GCommon;
using AvePoint.GCommon.Utility;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using System.Text;

namespace AvePoint.ObjectModel.Common.Cache
{
    public class AveListSqliteCache : AveObjectSqliteDBBase, IDisposable
    {
        private string mListCache = "AveListCache";
        private AveLogger _logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private int _aveListCacheTypes;
        public readonly static object mLock = new object();
        public AveListSqliteCache(string tenantGroupId, string jobid, string aveObjectId, int aveListCacheTypes) : base(tenantGroupId, jobid, aveObjectId)
        {
            _aveListCacheTypes = aveListCacheTypes;
            InitializeDb();
        }

        // tableName: FileCollection, FolderCollection, UniqueIDMapping, etc.
        public override void CreateSchemaIfNotExists(IDbCommand command)
        {
            if ((_aveListCacheTypes & (int)AveListCacheType.FileCollection) == (int)AveListCacheType.FileCollection)
            {
                CreateSchema4FileCollection(command);
            }

            // may add other table schema creation here for different AveListCacheType, currently only FileCollection is used, so skip for now.
        }

        public void InsertValueToDB(List<AveListItemConflictBaseInfo> listItemEntities, AveListCacheType cacheType)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("DBForAveList.InsertValueToDB"))
            {
                ExecuteQueryWithAction(connection =>
                {
                    using (var command = connection.CreateCommand())
                    {
                        InternalInsertValueToDB(connection, command, listItemEntities, cacheType);
                    }
                });
            }
        }

        private void InternalInsertValueToDB(SQLiteConnection conn, IDbCommand command, List<AveListItemConflictBaseInfo> baseInfos, AveListCacheType cacheType)
        {
            try
            {
                switch (cacheType)
                {
                    case AveListCacheType.FileCollection:
                        InsertValuesToFileCollectionTable(conn, command, baseInfos, cacheType);
                        break;
                    case AveListCacheType.FolderCollection:
                    case AveListCacheType.UniqueIDMapping:
                    default:
                        _logger.Warn($"CacheType {cacheType} is not supported in InternalInsertValueToDB, so skip the insert operation.");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"InternalInsertValueToDB failed.Message:{ex}.");
                throw;
            }
        }

        #region FileCollection
        private static void CreateSchema4FileCollection(IDbCommand command)
        {
            var tableName = AveListCacheType.FileCollection.ToString();
            string query = string.Format($"CREATE TABLE IF NOT EXISTS {SecurityUtils.SanitizeSQLSchemaName(tableName)}(" +
                "[UniqueId] [nvarchar](50) not null," +
                "[WebRelativeUrl] [nvarchar] NOT NULL," +
                "[Type] [int]" +
                // other properties can be added here if needed in the future
                ");" +
                $"CREATE INDEX tableIndexWebRelativeUrl ON {SecurityUtils.SanitizeSQLSchemaName(tableName)}(WebRelativeUrl);"
                );

            command.CommandText = query;

            command.ExecuteNonQuery();
        }
        private void InsertValuesToFileCollectionTable(SQLiteConnection conn, IDbCommand command, List<AveListItemConflictBaseInfo> baseInfos, AveListCacheType cacheType)
        {
            try
            {
                var tableName = cacheType.ToString();
                using (SQLiteTransaction tr = conn.BeginTransaction())
                {
                    StringBuilder query = new StringBuilder();
                    query.AppendLine($"INSERT INTO {SecurityUtils.SanitizeSQLSchemaName(tableName)}(UniqueId,WebRelativeUrl,Type) ");
                    query.AppendLine(@"VALUES (@UniqueId,@WebRelativeUrl,@Type)");
                    // other properties can be added here if needed in the future

                    var pUniqueId = new SQLiteParameter("@UniqueId");
                    var pWebRelativeUrl = new SQLiteParameter("@WebRelativeUrl");
                    var pType = new SQLiteParameter("@Type");
                    command.Parameters.Add(pUniqueId);
                    command.Parameters.Add(pWebRelativeUrl);
                    command.Parameters.Add(pType);
                    command.CommandText = query.ToString();

                    foreach (var baseInfo in baseInfos)
                    {
                        pUniqueId.Value = baseInfo.UniqueId.ToString();

                        var webRelativeUrl = baseInfo.ServerRelativeUrl.Replace(baseInfo.WebServerRelativeUrl, "").TrimStart('/');
                        pWebRelativeUrl.Value = webRelativeUrl;

                        pType.Value = baseInfo.ObjectType;

                        command.ExecuteNonQuery();
                    }
                    tr.Commit();
                }
            }
            catch (Exception e)
            {
                _logger.Error($"InsertValuesToFileCollectionTable failed.Message:{e}.");
                throw;
            }
        }

        public bool TryGetCachedFile(string fileRelativeUrl, out AveListItemConflictBaseInfo file)
        {
            var itemProperties = new Dictionary<string, object>();
            file = null;
            ExecuteQueryWithAction(connection =>
            {
                using (var command = connection.CreateCommand())
                {
                    var tableName = AveListCacheType.FileCollection.ToString();
                    command.CommandText = $"SELECT UniqueId, WebRelativeUrl, Type FROM {SecurityUtils.SanitizeSQLSchemaName(tableName)} WHERE WebRelativeUrl = @WebRelativeUrl LIMIT 1";
                    command.Parameters.Add(new SQLiteParameter("@WebRelativeUrl", fileRelativeUrl));
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            itemProperties["UniqueId"] = Guid.Parse(reader["UniqueId"].ToString());
                            // other properties can be added here if needed in the future
                        }
                    }
                }
            });

            if (itemProperties.Count > 0)
            {
                file = new AveListItemConflictBaseInfo(null, "", itemProperties);
            }

            return file != null;
        }
        #endregion


        public void ExecuteQueryWithAction(Action<SQLiteConnection> action)
        {
            ExecuteWithConnection(action);
        }

        public void Dispose()
        {
            File.Delete(this.dbFilePath);
        }
    }

    public enum AveListCacheType
    {
        FileCollection = 1,
        FolderCollection = 2,
        UniqueIDMapping = 4,
        //....
    }
}
