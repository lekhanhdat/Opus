using AvePoint.RA.Contract.Multi_Geo.Enum;
using RAMultiGeo.Factories;
using RAMultiGeo.Helper;

namespace RAMultiGeo.SyncCommonData.MainDC.DataCenterSync
{
    public class SyncDataFromSQLServerToSQLite(string templateSQLiteFilePath)
    {
        private MultiGeoCommonSyncTable NeedSyncTables = MultiGeoCommonSyncTable.None;
        private string templateSQLiteFolder = templateSQLiteFilePath;
        private int pageSize = 1000;
        private HandleDataSQLFactory? factory;
        private HandleDataSQLFactory Factory
        {
            get
            {
                return factory ??= new HandleDataSQLFactory();
            }
        }

        public SyncDataFromSQLServerToSQLite SetNeedSyncTable(MultiGeoCommonSyncTable needSyncTables)
        {
            NeedSyncTables = needSyncTables;
            return this;
        }

        public async Task StartSyncTable()
        {
            if(NeedSyncTables == MultiGeoCommonSyncTable.AllTable)
            {
                var types = Enum.GetValues(typeof(MultiGeoCommonSyncTable));
                foreach (var item in types)
                {
                    if (!item.Equals(MultiGeoCommonSyncTable.AllTable) && !item.Equals(MultiGeoCommonSyncTable.None))
                    {
                        if (Enum.TryParse(item.ToString(), out MultiGeoCommonSyncTable p))
                        {
                            await ProcessSyncTable(p);
                        }
                    }
                }
                return;
            }
            var tableList = NeedSyncTables.ToString().Split(", ", StringSplitOptions.RemoveEmptyEntries);
            foreach (var table in tableList)
            {
                if (Enum.TryParse(table, out MultiGeoCommonSyncTable t))
                {
                    await ProcessSyncTable(t);
                }
            }
            SQLiteHelper.Dispose();
        }

        private async Task ProcessSyncTable(MultiGeoCommonSyncTable syncTable)
        {
            var handleDataSQL = Factory.Create(syncTable);
            int index = 1;
            var datas = await handleDataSQL.QueryByPagerAsync(index, pageSize);
            do
            {
                index++;
                await ProcessCreateSQLiteDataAsync(syncTable, datas);
            } while ((datas = await handleDataSQL.QueryByPagerAsync(index, pageSize)).Any());
        }

        private async Task<bool> ProcessCreateSQLiteDataAsync(MultiGeoCommonSyncTable syncTable, IEnumerable<object> datas)
        {
            if (!SyncTableConverterRegistry.TryGetConverter(syncTable, out var converterFactory))
            {
                return false;
            }

            var converter = converterFactory();

            dynamic convertedInstance = converter;

            await convertedInstance
                .SetTemplateSQLiteFileAndSyncTable(templateSQLiteFolder, syncTable)
                .InitInsertDataAsync();
            return await convertedInstance.ProcessSyncSQLTableToSQLiteAsync(datas);
        }
    }
}
