using AvePoint.RA.Contract.Multi_Geo.Enum;
using RAMultiGeo.Interface;

namespace RAMultiGeo.Factories
{
    public class HandleDataSQLFactory
    {
        public IHandleDataSQL Create(MultiGeoCommonSyncTable tableType) 
        {
            string className = tableType.ToString() + "HandleDataSQL";
            //var instance = CreateInstance(tableType);
            Type typeToCreate = Type.GetType("RAMultiGeo.Implement." + className);
            var resultObj = Activator.CreateInstance(typeToCreate);
            if (resultObj is IHandleDataSQL typed)
            {
                return typed;
            }
            throw new InvalidOperationException(
                $"The table type '{tableType}' does not match the expected model type {tableType} Query Data SQL.");
        }
    }
}
