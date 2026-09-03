using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class MultiGeoSettingDao : BaseDao<MultiGeoSettingInfo>, IMultiGeoSettingDao
    {
        private readonly RALogger Logger = RALogger.GetInstance(typeof(MultiGeoSettingDao));
        public async Task AddOrUpdateMultipleGeoSettings(List<MultiGeoSettingInfo> settings)
        {
            using var context = GetNewContext();
            using var transaction = context.Database.BeginTransaction();
            try
            {
                long currentTime = DateTime.UtcNow.Ticks;
                foreach (var setting in settings)
                {
                    var dbSetting = await context.MultiGeoSettingInfos.FirstOrDefaultAsync(dcSetting => dcSetting.DataCenter.ToLower() == setting.DataCenter.ToLower());
                    if(dbSetting != null)
                    {
                        dbSetting.IPAddresses = setting.IPAddresses;
                        dbSetting.UpdateTime = currentTime;
                    }
                    else
                    {
                        context.MultiGeoSettingInfos.Add(new MultiGeoSettingInfo
                        {
                            Id = setting.Id == Guid.Empty ? Guid.NewGuid() : setting.Id,
                            IPAddresses = setting.IPAddresses,
                            DataCenter = setting.DataCenter,
                            CreateTime = currentTime
                        });
                    }
                    await context.SaveChangesAsync();
                }
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public Dictionary<string, string> GetDicDCAndIpAddresses()
        {
            using var context = GetNewContext();
            return context.MultiGeoSettingInfos.AsNoTracking().Where(dc => !dc.IsDeleted).Select(dc => new { dc.DataCenter, dc.IPAddresses }).ToDictionary(dc => dc.DataCenter, dc => dc.IPAddresses, StringComparer.OrdinalIgnoreCase);
        }

        public async Task<long> MultiGeoInsertMultiGeoSettingTableAsync(IEnumerable<MultiGeoSettingInfo> multiGeoSettingInfos)
        {
            try
            {
                using var context = GetNewContext();
                context.MultiGeoSettingInfos.AddRange(multiGeoSettingInfos);
                return await context.SaveChangesAsync();
            }
            catch (Exception ex) 
            {
                Logger.Error("Error occurred while inserting multi-geo setting info.", ex);
                return 0;
            }
        }
        public async Task<long> MultiGeoDeleteAllMultiGeoSettingAsync()
        {
            return await TruncateAllDataInTableAsync("MultiGeoSettingInfoes");
        }
        public async Task<IEnumerable<object>> LoadByPager(int pageIndex, int pageSize)
        {
            using var context = GetNewContext();
            return await context.MultiGeoSettingInfos.AsNoTracking().OrderBy(dc => dc.CreateTime).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        }
    }
}
