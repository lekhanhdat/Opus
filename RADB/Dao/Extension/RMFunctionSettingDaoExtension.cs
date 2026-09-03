using AvePoint.RA.Contract.FunctionSetting;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Extension
{
    public static class RMFunctionSettingDaoExtension
    {
        public static async Task<bool> IsEnableMultiGeoFeature(this IRMFunctionSettingDao functionSettingDao, IRMKeyValueDao keyValueDao)
        {
            if (!keyValueDao.IsSupportMultipleGeoFeature()) return false;
            var setting = await functionSettingDao.GetSettingInfo(FunctionSettingType.EnableMultiGEOFeature);
            return bool.TryParse(setting, out var result) && result;
        }
    }
}
