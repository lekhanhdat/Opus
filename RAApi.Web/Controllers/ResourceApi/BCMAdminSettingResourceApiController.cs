using AvePoint.RA.Api.Web.Common;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.I18N.Core;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Controllers.ResourceApi
{
    [Route("api/BCMAdminSettingApi/[action]")]
    public class BCMAdminSettingResourceApiController : RAWebApiBase
    {

        private readonly RALogger Logger = RALogger.GetInstance(typeof(BCMAdminSettingResourceApiController));
        public IUniqueIdSettingService _UniqueIdSettingService;
        public IUniqueIdSettingService UniqueIdSettingService => PlatformWindsorManager.GetService(ref _UniqueIdSettingService);

        public IScheduleService _ScheduleService;
        public IScheduleService ScheduleService => PlatformWindsorManager.GetService(ref _ScheduleService);

        [HttpPost]
        public async Task<RAReturnMessage> UpdateUniqueIdSetting([FromBody] UniqueIdSetting setting)
        {
            var result = new RAReturnMessage();
            result.MessageType = RAMessageType.Successful;
            try
            {
                if (setting.Prefix.Length < 4 || setting.Prefix.Length > 12)
                {
                    result.MessageType = RAMessageType.Failed;
                    return result;
                }

                if (setting.SourceFlag == AvePoint.RA.Contract.Explorer.SourceFlag.FileSystem)
                {
                    await UniqueIdSettingService.UpdateFileSystemUniqueIdSettingAsync(setting);
                }
                else
                {
                    await UniqueIdSettingService.UpdateUniqueIdSettingAsync(setting);

                    if (setting.IsActived)
                    {
                        await ScheduleService.CreateCustomScheduleAsync(false, ScheduleType.UniqueIDSettingSchedule);
                        await ScheduleService.CreateCustomScheduleAsync(false, ScheduleType.SPOnPremUniqueIDSettingSchedule);
                        await ScheduleService.CreateCustomScheduleAsync(false, ScheduleType.TeamsUniqueIDSettingSchedule);
                    }
                    else
                    {
                        ScheduleService.DeleteScheduleByType(ScheduleType.UniqueIDSettingSchedule);
                        ScheduleService.DeleteScheduleByType(ScheduleType.SPOnPremUniqueIDSettingSchedule);
                        ScheduleService.DeleteScheduleByType(ScheduleType.TeamsUniqueIDSettingSchedule);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("failed to loading uniqueIdSetting", e.ToString());
                throw;
            }
            return result;
        }
    }
}
