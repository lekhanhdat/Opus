/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.RA.Contract.RMWeb.CP;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb
{
    public interface IGeneralSettingService
    {
        Task<GeneralSettingModel> GetGeneralSettingAsync();

        Task<bool> SaveOrUpdateGeneralSettingAsync(GeneralSettingModel generalSettingModel);

        Task<bool> CheckEmailSenderDefinition(EmailSenderDefinition definition);

        bool DeleteCurrentUserGeneralSetting();

        Task<TimeSettingModel> GetTimeSettingModelAsync(string tenantId);

        Task<TimeModel> ConvertTiksToDateTimeAsync(long tiks, bool isIncludeTimeZoneInFormat, bool isControlPlus = false);
        TimeModel ConvertTiksToDateTime(GeneralSettingModel gls, long tiks, bool isIncludeTimeZoneInFormat);
        TimeModel ConvertTiksToDateTime(GeneralSettingModel gls, long tiks, bool isIncludeTimeZoneInFormat, int timeZoneId, bool isDaylight, string timeFormat);
        long ConvertTiksToTimeZoneTicks(int timeZoneId, bool isDaylight, long tiks);
        Task<TimeModel> ConverTiksToDateTimeAsync(string timeZoneId, long tiks);
        TimeModel ConverTiksToDateTime(GeneralSettingModel gls, string timeZoneId, long tiks);
        TimeModel ConvertTiksToUTCDateTime(GeneralSettingModel gls, long tiks);
        TimeModel ConvertTiksToUTCDateTime(GeneralSettingModel gls, long tiks, bool isIncludeTimeZoneInFormat);

        Task<DateTime> ConvertDateTimeToUtcAsync(DateTime dt);
        Task<DateTime> ConvertDateTimeToUtcAsync(DateTime dt, string timeZoneId);

        Task<DateTime> ConvertDateTimeToUtcAsync(string dateTimeStr, GeneralSettingModel gls);

        Dictionary<AuditItems, string> GetAuditItems(GeneralSettingModel gsm);

        Task<string> GetDateTimeFormatAsync();
        string GetDateTimeFormat(GeneralSettingModel gls);

        Task<string> GetDateFormatAsync();

        Task<string> ConvertToUTCDateTimeAsync(string startTime, string format = null);

        string ConvertToUTCDateTime(string startTime, GeneralSettingModel gls, string format = null);

        Task<string> ConvertFromUTCDateTimeAsync(string startTime, string format = null);
        Task<SecurityProfileResult> SaveUsingSecurityProfileAsync();
        Task<Tuple<Guid, string>> VerifyAndCreateDefaultSecurityProfileAsync();

         string ConvertTiksToDateNoTime(GeneralSettingModel gls, long tiks);
        System.Threading.Tasks.Task EnsureDefaultMastkeySecurityProfileAsync(Guid securityProfileGuid);
        Task<string> VerfiyHasMastkeySecurityProfileAsync();
        string ConvertBrowserTimeZoneToWindows(string timezoneId);
        Task<bool> SaveOrUpdateRecordLabelAsync(string recordsLabel, bool isRequired = true);
    }
}
