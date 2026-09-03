using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.I18N.Core;

namespace AvePoint.RA.Service.Services.Explorer.AuditHandler
{
    public class ExplorerAuditUtil
    {
        public static string GetEmailNotificationInfo(HoldEmailNotification info)
        {
            if (info == null)
            {
                return null;
            }

            if (!info.IsEnabled)
            {
                return I18NEntity.GetString("RM_JS_Common_No");
            }

            return string.Join("<br>",
                I18NEntity.GetString("RM_JS_Common_Yes"),
                I18NEntity.GetString("RM_CP_ManageHold_ReminderDuration"),
                $"{info.ReminderDurationDays} {I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Days")}");
        }
    }
}
