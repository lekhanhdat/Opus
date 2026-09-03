using System;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Google.NexusGovernance
{
    public interface INexusGovernancePersonalSettingService
    {
        Task<string> GetPersonalSettingLanguage(String userId);
    }
}
