using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DalServices;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.Service.Services.Discovery.Office365.Work;
using AvePoint.RA.Service.Services.Tenant;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.RMTasks
{
    public class DiscoveryDalJobMonitorTimerExecutor : ITaskExecutor
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(DiscoveryDalJobMonitorTimerExecutor));

        private IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private static readonly IRMTenantDiscoveryDBInfoDao TenantService = new RMTenantDiscoveryDBInfoDao();
        public async Task ExecutorAsync(TaskBase task)
        {
            try
            {
                var tenants = await TenantService.GetAllAvaliableAsync();
                foreach (var tenant in tenants)
                {
                    await TenantUtil.RunUnderTenantAsync(tenant.Id, "", async () =>
                    {
                        try
                        {
                            var dalJobMonitor = new RMDiscoveryDalJobMonitor();
                            await dalJobMonitor.MonitorAsync();
                        }
                        catch (Exception ex)
                        {
                            _logger.Error($"DiscoveryDalJobMonitorTimerExecutor ExecutorAsync tenantId: {tenant.Id} error: {ex.Message}", ex);
                        }
                    });
                }

            }
            catch (Exception ex)
            {
                _logger.Error($"DiscoveryDalJobMonitorTimerExecutor ExecutorAsync error: {ex.Message}", ex);
            }

        }
    }
}
