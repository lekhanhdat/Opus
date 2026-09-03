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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Dao;
using RMSynchronize.SyncNodeFromAOS.ChangeLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RMSynchronize.SyncStorage.SqlServer
{
    public class RMSyncSqlServerSettingsProcessor : IRMSyncSqlServerChangeable, IRMSyncSqlServerMoveable, IRMSyncSqlServerDeletable
    {

        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(RMSyncSqlServerSettingsProcessor));

        private static readonly IRMSyncSqlServerDataDao s_syncSqlServerDataDao = PlatformWindsorManager.GetService<IRMSyncSqlServerDataDao>();

        public async Task<bool> ChangeNameAsync(RMSyncNodeChangeInfo changeInfo)
        {
            try
            {
                var count = 0;

                switch(changeInfo.ContentSource)
                {
                    case SourceFlag.SharePoint:
                        if(changeInfo.IsContainer)
                        {
                            count = await s_syncSqlServerDataDao.ChangeNameForSharePointOnlineSettingsByGroupAsync(new Guid(changeInfo.Id), changeInfo.Url);
                        }
                        else
                        {
                            count = await s_syncSqlServerDataDao.ChangeNameForSharePointOnlineSettingsBySiteAsync(new Guid(changeInfo.Id), changeInfo.BeforeUrl, changeInfo.Url);
                        }
                        break;
                    case SourceFlag.OneDrive:
                        if (changeInfo.IsContainer)
                        {
                            count = await s_syncSqlServerDataDao.ChangeNameForOneDriveSettingsByGroupAsync(new Guid(changeInfo.Id), changeInfo.Url);
                        }
                        else
                        {
                            count = await s_syncSqlServerDataDao.ChangeNameForOneDriveSettingsBySiteAsync(new Guid(changeInfo.Id), changeInfo.BeforeUrl, changeInfo.Url);
                        }
                        break;
                    case SourceFlag.Exchange:
                        count = await s_syncSqlServerDataDao.ChangeNameForExchangeOnlineSettingsAsync(new Guid(changeInfo.Id), changeInfo.Url);
                        break;
                }

                s_logger.Debug($"The [{changeInfo.ContentSource}] [{changeInfo.Url}] is succeed change name count [{count}].");

                return true;
            }
            catch(Exception e)
            {
                s_logger.Error($"An error occurred while process [{changeInfo.ContentSource} - {changeInfo.Url}] change name. Error: {e}");
                return false;
            }
        }

        public async Task<bool> DeleteAsync(RMSyncNodeChangeInfo changeInfo)
        {
            try
            {
                var count = 0;

                switch (changeInfo.ContentSource)
                {
                    case SourceFlag.SharePoint:
                        count = await s_syncSqlServerDataDao.DeleteSharePointOnlineSettings(changeInfo.IsContainer, new Guid(changeInfo.Id));
                        break;
                    case SourceFlag.OneDrive:
                        count = await s_syncSqlServerDataDao.DeleteOneDriveSettings(changeInfo.IsContainer, new Guid(changeInfo.Id));
                        break;
                    case SourceFlag.Exchange:
                        count = await s_syncSqlServerDataDao.DeleteExchangeOnlineSettings(changeInfo.IsContainer, new Guid(changeInfo.Id));
                        break;
                }

                s_logger.Debug($"The [{changeInfo.ContentSource}] [{changeInfo.Url}] settings have been successfully deleted count [{count}].");
                return true;
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while process [{changeInfo.ContentSource} - {changeInfo.Url}] delete. Error: {e}");
                return false;
            }
        }

        public async Task<bool> MoveContainerAsync(RMSyncNodeChangeInfo changeInfo)
        {
            try
            {
                var count = 0;

                switch (changeInfo.ContentSource)
                {
                    case SourceFlag.SharePoint:
                        count = await s_syncSqlServerDataDao.DeleteSharePointOnlineSettings(changeInfo.IsContainer, new Guid(changeInfo.Id));
                        break;
                    case SourceFlag.OneDrive:
                        count = await s_syncSqlServerDataDao.DeleteOneDriveSettings(changeInfo.IsContainer, new Guid(changeInfo.Id));
                        break;
                    case SourceFlag.Exchange:
                        count = await s_syncSqlServerDataDao.DeleteExchangeOnlineSettings(changeInfo.IsContainer, new Guid(changeInfo.Id));
                        break;
                }

                s_logger.Debug($"The [{changeInfo.ContentSource}] [{changeInfo.Url}] settings have been successfully moved container count [{count}].");
                return true;
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while process [{changeInfo.ContentSource} - {changeInfo.Url}] move container. Error: {e}");
                return false;
            }
        }
    }
}
