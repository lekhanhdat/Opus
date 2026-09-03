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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object.Extension;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using Microsoft.Extensions.Options;
using System;
using AvePoint.RA.Api.Contract;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.DeleteArchivedData
{
    public class RMDeleteArchivedDataSettingManager
    {

        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDeleteArchivedDataSettingManager));
        
        private readonly long _nowTicks = DateTime.Now.Ticks;

        private readonly IRMArchiverSettingDao _archiverSettingDao = PlatformWindsorManager.GetService<IRMArchiverSettingDao>();

        private readonly RestoredSitesInfo _restoredSiteInfo;

        private readonly RMArchiverSetting _archiveSetting;

        private CleanRestoredItemsExtension _cleanRestoredOption;

        public RMDeleteArchivedDataSettingManager(RestoredSitesInfo restoredSiteInfo)
        {
            _restoredSiteInfo = restoredSiteInfo;
            if (!string.IsNullOrEmpty(_restoredSiteInfo.DeleteRestoredArchivedDataSettings))
            {
                _logger.Info($"[SettingSource] Site [{_restoredSiteInfo?.SiteUrl}] found DeleteRestoredArchivedDataSettings, prefer API setting branch.");
                DeleteRestoredArchivedDataSettings deleteInfo = SerializerHelper.DeserializeByDataContractSerializer<DeleteRestoredArchivedDataSettings>(_restoredSiteInfo?.DeleteRestoredArchivedDataSettings);
                if (deleteInfo != null)
                {
                    _cleanRestoredOption = new CleanRestoredItemsExtension
                    {
                        EnableDelArchivedData = true,
                        EnableCleanStubs = false,
                        CleanupAndDelRestoredType = Contract.TaxonomyModel.CleanRestoreOption.FileOrVersionOnly,
                        DayNum = deleteInfo.DayNum
                    };
                    _logger.Info($"[SettingSource] Site [{_restoredSiteInfo?.SiteUrl}] API setting resolved. DayNum [{_cleanRestoredOption?.DayNum}].");
                }
                else
                {
                    _logger.Warn($"[SettingSource] Site [{_restoredSiteInfo?.SiteUrl}] API setting payload exists but failed to deserialize.");
                }
                _archiveSetting = _archiverSettingDao.LoadSiteArchiverSettingByUrl(_restoredSiteInfo?.SiteUrl);
            }
            else
            {
                _logger.Info($"[SettingSource] Site [{_restoredSiteInfo?.SiteUrl}] has no DeleteRestoredArchivedDataSettings, use page CleanRestoredOption branch.");
                _archiveSetting = _archiverSettingDao.LoadSiteArchiverSettingByUrl(restoredSiteInfo.SiteUrl);
            }
        }

        public bool IsEnableDeleteArchivedData()
        {
            if (_cleanRestoredOption != null)
            {
                _logger.Info($"[IsEnableDeleteArchivedData] Site [{_restoredSiteInfo?.SiteUrl}] hit _cleanRestoredOption != null branch. EnableDelArchivedData [{_cleanRestoredOption.EnableDelArchivedData}], DayNum [{_cleanRestoredOption.DayNum}].");
                return _cleanRestoredOption.EnableDelArchivedData;
            }

            if (_archiveSetting == null || string.IsNullOrWhiteSpace(_archiveSetting.CleanRestoredOption))
            {
                _logger.Warn($"[SettingNotFound] Unable to find clean restored setting for site [{_restoredSiteInfo.SiteUrl}].");
                return false;
            }

            _cleanRestoredOption = SerializerHelper.DeserializeByDataContractSerializer<CleanRestoredItemsExtension>(_archiveSetting.CleanRestoredOption);
            if(_cleanRestoredOption == null || !_cleanRestoredOption.EnableDelArchivedData)
            {
                _logger.Warn($"[OptionNotEnable] The option item for site [{_restoredSiteInfo.SiteUrl}] is not enabled");
                return false;
            }

            _logger.Info($"[IsEnableDeleteArchivedData] Site [{_restoredSiteInfo?.SiteUrl}] loaded page CleanRestoredOption. EnableDelArchivedData [{_cleanRestoredOption?.EnableDelArchivedData}], DayNum [{_cleanRestoredOption?.DayNum}].");

            return true;
        }

        public bool IsEnableDeleteAllStub()
        {
            return _cleanRestoredOption.EnableCleanStubs;
        }

        public bool IsEnableDeleteRelatedVersion()
        {
            return _cleanRestoredOption.CleanupAndDelRestoredType == Contract.TaxonomyModel.CleanRestoreOption.FileAndReletedVersions;
        }

        public long DailyDays()
        {
            _logger.Info($"[DailyDays] Site [{_restoredSiteInfo?.SiteUrl}] returning DayNum [{_cleanRestoredOption?.DayNum}].");
            return _cleanRestoredOption.DayNum;
        }

        public bool HasTheDeletionTimeBeenReached(long restoredTicks)
        {
            _logger.Info($"[HasTheDeletionTimeBeenReached] Site [{_restoredSiteInfo?.SiteUrl}] restoredTicks [{restoredTicks}], DayNum [{_cleanRestoredOption?.DayNum}].");
            return new DateTime(restoredTicks).AddDays(_cleanRestoredOption.DayNum).Ticks <= _nowTicks;
        }
    }
}
