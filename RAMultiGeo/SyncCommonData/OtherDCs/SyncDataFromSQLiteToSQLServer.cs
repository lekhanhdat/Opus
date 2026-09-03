using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Multi_Geo.Enum;
using RAMultiGeo.Factories;
using RAMultiGeo.Helper;

namespace RAMultiGeo.SyncCommonData.OtherDCs
{
    internal class SyncDataFromSQLiteToSQLServer(string sQLiteFilePath, MultiGeoCommonSyncTable syncTable)
    {
        private readonly RALogger Logger = RALogger.GetInstance(typeof(SyncDataFromSQLiteToSQLServer));
        private HandleDataSQLFactory? factory;
        private HandleDataSQLFactory Factory
        {
            get
            {
                return factory ??= new HandleDataSQLFactory();
            }
        }
        private string SQLiteFilePath => sQLiteFilePath;
        private MultiGeoCommonSyncTable NeedSyncTables => syncTable;
        private const int pageSize = 50;
        private long SyncFailedTable = 0;
        public async Task StartSync()
        {
            Logger.Info($"Start sync data from SQLite to SQL Server");
            if (NeedSyncTables == MultiGeoCommonSyncTable.AllTable)
            {
                Logger.Info($"Sync all tables data from SQLite to SQL Server");
                var types = Enum.GetValues(typeof(MultiGeoCommonSyncTable));
                foreach (var item in types)
                {
                    if (!item.Equals(MultiGeoCommonSyncTable.AllTable) && !item.Equals(MultiGeoCommonSyncTable.None))
                    {
                        if (Enum.TryParse(item.ToString(), out MultiGeoCommonSyncTable table))
                        {
                            await ProcessSyncTable(table);
                        }
                    }
                }
                SQLiteHelper.Dispose();
                return;
            }
            var tableList = NeedSyncTables.ToString().Split(", ", StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in tableList)
            {
                if (Enum.TryParse(item, out MultiGeoCommonSyncTable table))
                {
                    await ProcessSyncTable(table);
                }
            }
            SQLiteHelper.Dispose();
        }

        public long GetSyncFailedTable()
        {
            return SyncFailedTable;
        }

        private async Task ProcessSyncTable(MultiGeoCommonSyncTable table)
        {
            Logger.Info($"Start sync data of table {table} from SQLite to SQL Server");
            try
            {
                var handleDataSQL = Factory.Create(table);
                await handleDataSQL.DeleteAllDataAsync();
                int index = 1;
                IEnumerable<object> datas = await QuerySyncSQLiteDataAsync(table, index, pageSize);
                do
                {
                    index++;
                    if (datas == null || datas.Count() == 0)
                    {
                        break;
                    }
                    if(!await WriteDataToSQLServerAsync(handleDataSQL, datas))
                    {
                        Logger.Warn($"Failed to write some data to SQL Server for table {table}");
                        SyncFailedTable |= (long)table;
                    }
                } while ((datas = await QuerySyncSQLiteDataAsync(table, index, pageSize)).Any());
            }
            catch (Exception ex)
            {
                Logger.Error($"Error occurred when sync data of table {table} from SQLite to SQL Server. Exception: {ex}");
                SyncFailedTable |= (long)table;
            }
        }

        private async Task<bool> WriteDataToSQLServerAsync(Interface.IHandleDataSQL handleDataSQL, IEnumerable<object> datas)
        {
            var dataCount = datas.Count();
            Logger.Info($"Start write data to SQL Server, data count: {dataCount}");
            var effectDataCount = await handleDataSQL.BatchInsertDataAsync(datas);
            Logger.Info($"Finish write data to SQL Server, data count: {dataCount}, effect data count: {effectDataCount}");
            if(dataCount != effectDataCount)
            {
                return false;
            }
            return true;
        }

        private async Task<IEnumerable<object>> QuerySyncSQLiteDataAsync(MultiGeoCommonSyncTable syncTable, int pageIndex, int pageSize)
        {
            if (!SyncTableConverterRegistry.TryGetConverter(syncTable, out var converterFactory))
            {
                return null;
            }

            var converter = converterFactory();

            dynamic convertedInstance = converter;

            return await convertedInstance
                 .SetTemplateSQLiteFileAndSyncTable(SQLiteFilePath, syncTable)
                 .GetAllDataFromSQLiteByPagerAsync(pageIndex, pageSize);
        }
    }
}
