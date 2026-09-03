using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Google.NexusGovernance;
using AvePoint.RA.RACommonUtility.Email.Model;
using System;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Google.NexusGovernance
{
    internal class NexusGovernancePersonalSettingService : NexusGovernanceBaseService, INexusGovernancePersonalSettingService
    {
        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(NexusGovernancePersonalSettingService));
        public async Task<string> GetPersonalSettingLanguage(string userId)
        {
            try
            {
                s_logger.Info("Start get personal setting language for user: " + userId);
                var result = await NexusGovernanceApiClient.PersonalSettingService.GetPersonalSetting(userId);
                s_logger.Info($"Succeed get personal setting language for user: {userId} and locale is {result?.LanguageID}");
                return result == null
                    ? "en-US"
                    : ((Locale)result.LanguageID) switch
                    {
                        Locale.EnUS => "en-US",
                        Locale.JaJP => "ja-JP",
                        _ => "en-US"
                    };
            }
            catch (Exception ex)
            {
                s_logger.Error($"An error occurred while getting personal setting language for user: {userId}. Error: {ex}");
                return null;
            }

        }
    }
}
