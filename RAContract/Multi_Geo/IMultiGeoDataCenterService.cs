using AvePoint.RA.Contract.Multi_Geo.Model;
using AvePoint.RA.Contract.RMWeb;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Multi_Geo
{
    public interface IMultiGeoDataCenterService
    {
        Task<MultiGeoDCInfo> GetMultiGeoDCInformation();
        Task<List<DataCenterInfo>> GetDCsSupported();
        string GetMainDC();
        bool IsMainDC();
        Task<bool> IsLimitMultiGeoManageContainer();
        Task<string> RunMainDCSyncCommonDataJob(JobRunBy jobRunBy);
        Task<string> RealRunMainDCSyncCommonDataJob(JobRunBy jobRunBy);
        string RunOtherDCSyncCommonDataJob(SyncCommonDataInforDto syncCommonDataInfor);
        Task<string> RealRunOtherDCSyncCommonDataJob(string param);
        Task<List<string>> GetOtherDataCentersAsync();
    }
}
