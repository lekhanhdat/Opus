using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Multi_Geo.Enum;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using Dapper;
using DocumentFormat.OpenXml.Wordprocessing;
using Newtonsoft.Json;
using RACommon.SQLiteDatabase;
using RAMultiGeo.Helper;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.SQLite;
using System.Reflection;
using ColumnAttribute = System.ComponentModel.DataAnnotations.Schema.ColumnAttribute;

namespace RAMultiGeo.Repositories
{
    internal class SQLiteSyncRepository<TModel> where TModel : BaseModel
    {
        public readonly RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public IJobDetailDao JobDetailDao => PlatformWindsorManager.GetService<IJobDetailDao>();
        protected string SQLiteFilePath;
        protected string CREATE_TABLE_SQL;
        protected string INSERT_DATA_SQL;
        protected MultiGeoCommonSyncTable SyncTable;
        protected string TABLE_NAME => SyncTable.ToString();

        public SQLiteSyncRepository<TModel> SetTemplateSQLiteFileAndSyncTable(string sQLiteFilePath, MultiGeoCommonSyncTable syncTable)
        {
            SQLiteFilePath = sQLiteFilePath;
            SyncTable = syncTable;
            SQLiteHelper.SetSQLiteFilePath(sQLiteFilePath);
            return this;
        }


        public async Task<IEnumerable<object>> GetAllDataFromSQLiteByPagerAsync(int pageIndex, int pageSize)
        {
            try
            {
                string selectSQL = string.Format(JobMonitorConstants.SELECT_DATA_FROM_TABLE, TABLE_NAME, pageSize, (pageIndex - 1) * pageSize);
                return await SQLiteHelper.DbHelper.QueryAsync<TModel>(selectSQL);
            }
            catch (Exception ex)
            {
                logger.Error($"failed to query data from table,Table name:{SyncTable}, exception: {ex}");
                return null;
            }
        }

        public async Task InitInsertDataAsync()
        {
            CREATE_TABLE_SQL = BuildCreateTableSQL();
            INSERT_DATA_SQL = BuildInsertSQL();
            if (!JobDetailDao.IsExistTable(SQLiteFilePath, TABLE_NAME))
            {
                await CreateNewTable();
            }
        }

        public async Task<bool> ProcessSyncSQLTableToSQLiteAsync(IEnumerable<object> datas)
        {
            try
            {
                using var _ = new PerformanceScope("ConvertSQLToSQLite");
                List<Dictionary<String, Object>> parameterList = new List<Dictionary<String, Object>>();
                TModel data;
                foreach (var objectData in datas)
                {
                    data = objectData as TModel;
                    parameterList.Add(BuildSQLiteParameters(data));
                }
                await SQLiteHelper.DbHelper.BatchExecuteNonQueryAsync(INSERT_DATA_SQL, parameterList);
                return true;
            }
            catch (Exception ex)
            {
                logger.Error($"failed to insert data into table, sqlite file path: {SQLiteFilePath},Table name:{SyncTable}");
                logger.Error(ex.ToString());
                return false;
            }
        }

        public Dictionary<String, Object> BuildSQLiteParameters(TModel model)
        {
            var parameters = new Dictionary<String, Object>();
            var properties = typeof(TModel)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttribute<NotMappedAttribute>() == null);

            foreach (var prop in properties)
            {
                var value = prop.GetValue(model);
                object paramValue;

                if (value == null)
                {
                    paramValue = DBNull.Value;
                }
                else if (prop.PropertyType == typeof(Guid) || prop.PropertyType == typeof(Guid?))
                {
                    paramValue = ((Guid)value).ToString();
                }
                else if (prop.PropertyType == typeof(byte[]))
                {
                    paramValue = value;
                }
                else
                {
                    paramValue = value;
                }

                parameters.Add($"@{prop.Name}", paramValue);
            }
            return parameters;
        }

        private async Task CreateNewTable()
        {
            try
            {
                CreateSQLiteFile();
                await SQLiteHelper.DbHelper.ExecuteNonQueryAsync(CREATE_TABLE_SQL);
                logger.Debug("Successful to create table {0}.", SyncTable);
            }
            catch (Exception ex)
            {
                logger.Error($"failed to create table,report file path:{SQLiteFilePath},Table name:{SyncTable}");
                logger.Error(ex.ToString());
            }
        }

        private void CreateSQLiteFile()
        {
            if (File.Exists(SQLiteFilePath)) return;
            logger.Info($"Create SQLite file {SQLiteFilePath}");
            string directoryPath = Path.GetDirectoryName(SQLiteFilePath);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            using (FileStream fs = File.Create(SQLiteFilePath))
            {
                Console.WriteLine($"File created at: {SQLiteFilePath}");
            }
            return;
        }

        public string BuildInsertSQL()
        {
            var properties = typeof(TModel)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttribute<NotMappedAttribute>() == null)
                .Select(p => p.Name)
                .ToList();

            var columns = string.Join(", ", properties);
            var parameters = string.Join(", ", properties.Select(p => $"@{p}"));

            return $"INSERT INTO {SyncTable} ({columns}) VALUES ({parameters})";
        }

        public string BuildCreateTableSQL()
        {
            var properties = typeof(TModel)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p =>  p.GetCustomAttribute<NotMappedAttribute>() == null)
                .ToList();

            var columnDefinitions = new List<string>();

            foreach (var prop in properties)
            {
                var colAttr = prop.GetCustomAttribute<ColumnAttribute>();
                var sqliteType = MapToSQLiteType(colAttr?.TypeName ?? prop.PropertyType.Name);
                var columnDef = $"{prop.Name} {sqliteType}";

                columnDefinitions.Add(columnDef);
            }

            var columns = string.Join(",\n    ", columnDefinitions);
            return $"CREATE TABLE IF NOT EXISTS {SyncTable} (\n    {columns}\n)";
        }

        private string MapToSQLiteType(string sqlType)
        {
            return sqlType?.ToLower() switch
            {
                "int" or "bigint" or "smallint" or "tinyint" => "INTEGER",
                "bit" => "INTEGER",
                "uniqueidentifier" => "TEXT",
                "nvarchar" or "varchar" or "ntext" or "text" or "char" or "nchar" => "TEXT",
                "datetime" or "datetime2" or "date" or "datetimeoffset" => "TEXT",
                "decimal" or "numeric" or "float" or "real" or "money" => "REAL",
                "varbinary(max)" or "varbinary" or "binary" or "image" or "byte[]" => "BLOB",
                _ => "TEXT"
            };
        }

    }

    public class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
    {
        public override void SetValue(IDbDataParameter parameter, Guid value)
        {
            parameter.Value = value.ToString();
        }

        public override Guid Parse(object value)
        {
            var result = Guid.Empty;
            if (value == null || value == DBNull.Value)
            {
                return Guid.Empty;
            }
            Guid.TryParse(value.ToString(), out result);
            return result;
        }
    }
}
