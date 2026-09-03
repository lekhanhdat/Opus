using AvePoint.RA.Common;
using AvePoint.RA.Common.Email;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Email.Model;
using AvePoint.RA.RACommonUtility.Email.Sender;
using AvePoint.RA.RACommonUtility.Email.Sender.Middleware;
using AvePoint.RA.RACommonUtility.Email.Sender.Storage;
using AvePoint.Wrapper.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.RMTasks
{
    public class ProcessHoldNotificationExecutor
    {
        private RALogger _logger => RALogger.GetInstance(typeof(ProcessHoldNotificationExecutor));

        private static readonly IHoldDao HoldDao = PlatformWindsorManager.GetService<IHoldDao>();

        private static readonly IGeneralSettingService GeneralSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();

        private static readonly ISecurityGroupManagementService SecurityGroupManagementService = PlatformWindsorManager.GetService<ISecurityGroupManagementService>();

        private readonly RMEmailSender _emailSender = new(new RMEmailMemoryStorage(new RMEmailStorageDefaultMiddleware()));

        private readonly int _batchSize = 50 ;
        private readonly int _delayMs = 2000;

        private const string EmailBody = @"<table cellspacing=""0"" style=""border-collapse:collapse; width:756px""><tbody><tr><td style=""background-color:#f2f2f2; border:1px solid white; padding:6px; font-weight:bold; text-align:left;"">Hold Title</td><td style=""background-color:#f2f2f2; border:1px solid white; padding:6px; font-weight:bold; text-align:center;"">Hold Until</td><td style=""background-color:#f2f2f2; border:1px solid white; padding:6px; font-weight:bold; text-align:center;"">Affected Items</td></tr>{0}</tbody></table>";
        private static string HoldRowTemplate = @"<tr><td style=""border:1px solid #ddd; padding:6px;""><strong>{0}</strong></td><td style=""border:1px solid #ddd; padding:6px; text-align:center;"">{1}</td><td style=""border:1px solid #ddd; padding:6px; text-align:center;"">{2}</td></tr>";
        public async Task ExecutorAsync()
        {
            using var pc = new AvePerformanceScope("ProcessHoldNotificationExecutor.ExecutorAsync");

            try
            {
                var gls = await GeneralSettingService.GetGeneralSettingAsync();
                var holdsNeedSendEmail = await GetHoldsNeedSendEmailAsync(gls);
                var usersHoldNotifications = await GetUsersHoldNotificationAsync(holdsNeedSendEmail);

                if (!usersHoldNotifications.Any())
                {
                    _logger.Info("No hold notification emails need to be sent.");
                    return;
                }
                var parameters = usersHoldNotifications.Select(x => BuildEmailParameter(x, gls)).ToList();

                for (int i = 0; i < parameters.Count; i += _batchSize)
                {
                    var batch = parameters.Skip(i).Take(_batchSize).ToList();

                    _emailSender.AddRange(RMEmailTemplateId.HOLD_NOTIFICATION, batch);
                    await _emailSender.SendAsync();

                    if (i + _batchSize < parameters.Count)
                    {
                        await Task.Delay(_delayMs);
                    }
                }
                _logger.Info($"Preparing hold notification emails. User count: [{parameters.Count}]");
                await HoldDao.UpdateLastSentEmailTimeAsync(holdsNeedSendEmail.Select(x => x.Id).ToList(), DateTime.UtcNow.Ticks);

                _logger.Info($"Successfully sent hold notification emails. User count: [{parameters.Count}]");
            }
            catch (Exception ex)
            {
                _logger.Error(
                    $"Failed to process hold notification emails. Error: {ex}");
            }
        }
        private RMHoldNotificationEmailTemplateParameters BuildEmailParameter(UserHoldNotification notification,GeneralSettingModel gls)
        {
            return new RMHoldNotificationEmailTemplateParameters
            {
                RequestReviewer = notification.User.DisplayName,
                RequestHoldsInformation = BuildEmailHoldDetails(
                    notification.Holds,
                    gls),
                ToUser = notification.User.UserPrincipalName,
                TemplateType = RMEmailTemplateType.HoldNotification,
            };
        }
        private async Task<List<RMHold>> GetHoldsNeedSendEmailAsync(GeneralSettingModel gls)
        {
            _logger.Info($"Begin to get hold need send email.");
            var today = DateTimeUtil.ConvertTimeFromUtc(DateTime.UtcNow, gls).Date;

            return (await HoldDao.GetHoldsPendingReminderEmailAsync())
                .Where(h =>
                {
                    var holdUntil = DateTimeUtil.ConvertTimeFromUtc(
                        new DateTime(h.CalendarTime),
                        gls).Date;

                    return (holdUntil - today).Days == h.ReminderDurationDays;
                })
                .ToList();
        }

        private async Task<List<UserHoldNotification>> GetUsersHaveManageHoldsPermissionAsync(List<UserHoldNotification> users)
        {
            var permissionTasks = users.Select(x => x.User.UserId).ToDictionary(
                userId => userId,
                userId => SecurityGroupManagementService.GetUserScopePermissionsAsync(userId));

            await Task.WhenAll(permissionTasks.Values);
            
            return users
                .Where(user =>SecurityGroupManagementService.HasManageHoldsPermission(permissionTasks[user.User.UserId].Result))
                .ToList();
        }

        private async Task<List<UserHoldNotification>> GetUsersHoldNotificationAsync(List<RMHold> holdsNeedSendEmail)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ProcessHoldNotificationExecutor.GetUserHoldNotifications"))
            {
                var explorerDao = new ExplorerDao();
                var recordCountByHold = await explorerDao.GetRecordCountByHoldIdAndHoldReleaseAsync(holdsNeedSendEmail);

                _logger.Info($"Record counts from Cosmos: {string.Join(", ", recordCountByHold.Select(x => $"{x.Key}:{x.Value}"))}");

                var usersHoldNotifications = holdsNeedSendEmail
                .SelectMany(hold => (JsonConvert.DeserializeObject<List<AOSUserDto>>(hold.EmailRecipients)
                    ?? new List<AOSUserDto>())
                    .DistinctBy(u => u.UserPrincipalName)
                    .Select(user => new { User = user, Hold = hold }))
                .GroupBy(x => x.User.UserPrincipalName)
                .Select(g => new UserHoldNotification
                {
                    User = g.First().User,
                    Holds = g
                        .GroupBy(x => x.Hold.Id)
                        .Select(hg => hg.First())
                        .Select(x => new HoldEmailItem
                        {
                            HoldId = x.Hold.Id,
                            HoldName = x.Hold.Name,
                            HoldUtil = x.Hold.CalendarTime,
                            RecordCount = recordCountByHold.TryGetValue(x.Hold.Id, out var count) ? count : 0
                        })
                        .ToList()
                })
                .ToList();

                return await GetUsersHaveManageHoldsPermissionAsync(usersHoldNotifications);
            }
        }
        private string BuildEmailHoldDetails(List<HoldEmailItem> holds, GeneralSettingModel gls)
        {
            if (holds == null || !holds.Any())
            {
                return string.Empty;
            }

            var rows = new StringBuilder();

            foreach (var hold in holds.OrderBy(x => x.HoldUtil))
            {
                DateTime localCalendarTime = DateTimeUtil.ConvertTimeFromUtc(hold.HoldUtil, gls);
                var holUntil = localCalendarTime.ToString(JSDateTimeFormat.DEFAULT_TIME_FORMAT);
                rows.AppendFormat(
                    HoldRowTemplate,
                    WebUtility.HtmlEncode(hold.HoldName),
                    holUntil,
                    hold.RecordCount);
            }

            return string.Format(EmailBody, rows.ToString());
        }
    }
}
