using AvePoint.RA.Common;
using AvePoint.RA.Common.Security;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.I18N.Core;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ControlPanel.AuditHandler
{
    public class AppManagementAuditUtil
    {
        private static readonly IRALogger logger = RALogger.GetInstance(typeof(AppManagementAuditUtil));
        private static IMultiGeoDataCenterService MultiGeoDataCenterService => PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();
        public static async Task<string?> GetDataCenter(string selectedDC)
        {
            var mainDCInternalName = MultiGeoDataCenterService.GetMainDC();
            if (mainDCInternalName.IsNullOrEmpty())
            {
                logger.Info($"Not support multigeo. Skip add select data center to audit.");
                return null;
            }
            logger.Info($"Getting data center display name for agent audit. Current DC: {RMSSOHelper.CurrentDCName}, Selected DC: {selectedDC}");

            var isSelectedDefaultDC = string.IsNullOrWhiteSpace(selectedDC) || string.Equals(selectedDC, mainDCInternalName, StringComparison.OrdinalIgnoreCase);

            if (isSelectedDefaultDC)
            {
                return I18NEntity.GetString("RM_GEO_DefaultDC_DisplayName");
            }

            var supportedDCs = await MultiGeoDataCenterService.GetDCsSupported();
            return supportedDCs?.FirstOrDefault(dc => string.Equals(dc.DCInternalName, selectedDC, StringComparison.OrdinalIgnoreCase))?.DCDisplayName;
        }

        public static bool IsAuditRequired(string selectedDC)
        {
            var mainDC = MultiGeoDataCenterService.GetMainDC();
            if (mainDC.IsNullOrEmpty())
            {
                return true;
            }
            var currentDC = RMSSOHelper.CurrentDCName;
            var isCurrentMainDC = string.Equals(mainDC, currentDC, StringComparison.OrdinalIgnoreCase);
            var isSelectedCurrentDC = string.Equals(selectedDC, currentDC, StringComparison.OrdinalIgnoreCase);

            return isCurrentMainDC || isSelectedCurrentDC;
        }
    }
}
