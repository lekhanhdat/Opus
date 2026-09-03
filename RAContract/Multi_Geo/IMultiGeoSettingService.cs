using AvePoint.RA.Contract.Multi_Geo.Model;
using AvePoint.RA.Contract.Object;
using CommonModel.DataModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Multi_Geo
{
    public interface IMultiGeoSettingService
    {
        Task<bool> IsEnableMultiGeoFeature();

        Task<RAReturnMessage> EnableMultiGeoFeature();

        Task<RAReturnMessage> SaveMultiGeoSettings(List<MultiGeoSettingInfoDto> multiGeoSettingInfo, bool isIgnoreAudit = false);

        Task<List<MultiGeoSettingInfoDto>> GetAllMultiGeoSetting();

        Task<bool> ValidateLoginIPAsync(string ipAddress, string dataCenter);
        ICollection<AgentInformation> GetAvailableAgentForMultiGeoRedirect(ICollection<AgentInformation> agents);


    }
}
