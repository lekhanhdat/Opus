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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;

namespace RAGoogle.Util
{
    public class SettingManager
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(SettingManager));

        private IRMGoogleSettingDao GoogleSettingDao = PlatformWindsorManager.GetService<IRMGoogleSettingDao>();

        private IRMSubJobDao SubJobDao = PlatformWindsorManager.GetService<IRMSubJobDao>();

        private readonly Dictionary<string, RMGoogleSetting> _settingInfoCache;

        public SettingManager()
        {
            _settingInfoCache = new();
        }

        public void LoadGoogleSettings(Dictionary<string, RMGoogleSetting> settingNodeMapping)
        {
            if (settingNodeMapping.IsNotNullOrEmpty())
            {
                _settingInfoCache.AddRange(settingNodeMapping, true);
            }
        }

        public RMGoogleSetting? TryGetGoogleSetting(string containerId, string scopeId, string driveId)
        {
            if (_settingInfoCache.TryGetValue(scopeId, out RMGoogleSetting? setting))
            {
                return setting;
            }
            setting = GoogleSettingDao.GetSettingInfoByScope(new Guid(containerId), new Guid(scopeId), new Guid(driveId));
            if (setting == null)
            {
                setting = GoogleSettingDao.GetSettingInfoByScope(new Guid(containerId), new Guid(containerId), Guid.Empty);
            }

            if (setting is not null && !_settingInfoCache.ContainsKey(setting.ScopeId.ToString()))
            {
                _settingInfoCache.Add(setting.ScopeId.ToString(), setting);
            }

            return setting;
        }

        public async Task ResetSettingInfoAsync(string jobId)
        {
            try
            {
                var parentJobId = jobId.Split("_")[0];
                var subJobs = await SubJobDao.FindListAsync(s => s.ParentId == parentJobId);

                if (!subJobs.Exists(s =>
                    s.Status == (int)JobStatus.Failed || s.Status == (int)JobStatus.Wait || s.Status == (int)JobStatus.InProgress))
                {
                    foreach (var setting in _settingInfoCache.Values)
                    {
                        logger.Info($"Reset setting {setting.Id} after job finish.");
                        await GoogleSettingDao.SetSettingJobTimeWithContainerIdAsync(setting.ContainerId, setting.ScopeId);
                    }
                }

            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while reset setting info. Error: {e}");
            }
        }
    }
}
