using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMPublicAPI.JPMC;
using AvePoint.RA.Contract.RMWeb;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMPublicAPI.JPMC
{
    public interface IAgentMgmtPublicServices
    {
        Task<RAReturnMessage> QueryAgentsAsync(AgentQueryParam param);
        Task<RAReturnMessage> CreateAgentAsync(AgentCreateParam param);
        Task<RAReturnMessage> CreateAgentWithIdAsync(AgentCreateParam param);
        Task<RAReturnMessage> UpdateAgentAsync(AgentUpdateParam param);
        Task<RAReturnMessage> DeleteAgentAsync(AgentActionParam param);
        Task<RAReturnMessage> DisableAgentAsync(AgentActionParam param);
        Task<RAReturnMessage> EnableAgentAsync(AgentActionParam param);
        Task<RAReturnMessage> UpdateAgentJobLimitAsync(AgentJobLimitParam param);

    }
}