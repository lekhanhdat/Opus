using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.Contract.Tenant;
using System;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.RMTasks
{
    internal class MultiGeoSyncCommonDataTaskExecutor : ITaskExecutor
    {
        private RALogger logger = RALogger.GetInstance(typeof(MultiGeoSyncCommonDataTaskExecutor));
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private IMultiGeoDataCenterService MultiGeoDataCenterService => PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();
        public async Task ExecutorAsync(TaskBase task)
        {
            try
            {
                var tInfos = TenantService.GetAllAvailableTenantInfo();
                foreach (var tInfo in tInfos)
                {
                    await TenantUtil.RunUnderTenantAsync(tInfo.TenantId, tInfo.RegisterEmail, async () =>
                    {
                        await MultiGeoSyncCommonData(tInfo.TenantId);
                    });
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to execute task {task.Id} of type {task.Type}. {ex}");
            }
        }

        private async Task MultiGeoSyncCommonData(string tenantId)
        {
            try
            {
                logger.Info($"Start to Multi Geo Sync Common Data for tenant {tenantId}.");
                await MultiGeoDataCenterService.RunMainDCSyncCommonDataJob(Contract.RMWeb.JobRunBy.Schedule);
            }
            catch (Exception e)
            {
                logger.Error($"Multi Geo Sync Common Data task for tenant {tenantId} has error: {e}");
            }
        }
    }
}
