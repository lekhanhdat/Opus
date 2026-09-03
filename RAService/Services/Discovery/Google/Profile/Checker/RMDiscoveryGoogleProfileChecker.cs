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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Model.Profile;
using AvePoint.RA.Contract.Discovery.Model.Query.Google.Parameter;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.DB.Dao.Discovery.Impl.Google;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Discovery.Office365.Profile.Checker;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Google.Profile.Checker
{
    public class RMDiscoveryGoogleProfileChecker
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryProfileChecker));
        private readonly IRMDiscoveryGoogleProfileDao _profileDao = new RMDiscoveryGoogleProfileDao();

        private readonly IRMDiscoveryGoogleSizeRangeDao _sizeRangeDao = new RMDiscoveryGoogleSizeRangeDao();

        private readonly IRMDiscoveryGoogleWithoutInDateDao _dateRangeDao = new RMDiscoveryGoogleWithoutInDateDao();

        private readonly IRMDiscoveryGoogleFileExtensionDao _fileExtensionDao = new RMDiscoveryGoogleFileExtensionDao();

        private readonly IRMDiscoveryGoogleRuleInfoDao _ruleInfoDao = new RMDiscoveryGoogleRuleInfoDao();
        private readonly Dictionary<RMDiscoveryProfileType, HashSet<string>> _supportSortByFields = new()
        {
            {RMDiscoveryProfileType.Inactive, new() { "FileTotalSize", "FileSumCount", "InactiveFileTotalSize", "InactiveFileSumCount" } },
            {RMDiscoveryProfileType.ROT, new() { "FileTotalSize", "RotFileTotalSize", "RCategoryFileTotalSize", "OCategoryFileTotalSize", "TCategoryFileTotalSize" } }
        };

        private readonly RMDiscoveryGoogleProfileDataInfo _dataInfo;

        private readonly RMDiscoveryProfileType _profileType;

        private readonly RAReturnMessage _failedReturnMessage;
        public RMDiscoveryGoogleProfileChecker(RMDiscoveryGoogleProfileDataInfo dataInfo, RMDiscoveryProfileType profileType)
        {
            _dataInfo = dataInfo;
            _profileType = profileType;
            _failedReturnMessage = new()
            {
                MessageType = RAMessageType.Failed,
                ErrorMessage = "Illegal parameters",
            };
        }
        public async Task<(bool succeed, RAReturnMessage failedReturnMessage)> AddCheckAsync()
        {
            if (_dataInfo.Id != Guid.Empty)
            {
                _logger.Error($"The profile add action not support has id.");
                return (false, _failedReturnMessage);
            }

            if (!await CheckAsync())
            {
                return (false, _failedReturnMessage);
            }

            return (true, null);
        }

        public async Task<(bool succeed, RAReturnMessage failedReturnMessage)> UpdateCheckAsync()
        {
            if (_dataInfo.Id == Guid.Empty)
            {
                _logger.Error($"The profile update action not support id is empty.");
                return (false, _failedReturnMessage);
            }

            if (!await CheckAsync())
            {
                return (false, _failedReturnMessage);
            }

            var profileInfo = await _profileDao.GetProfileInfoByIdAsync(_dataInfo.OrganizationId, _dataInfo.Id);
            if (profileInfo.IsBuildIn)
            {
                return (false, _failedReturnMessage);
            }

            return (true, null);
        }

        public async Task<(bool succeed, RAReturnMessage failedReturnMessage)> DeleteCheckAsync()
        {
            var profileInfo = await _profileDao.GetProfileInfoByIdAsync(_dataInfo.OrganizationId, _dataInfo.Id);
            if (profileInfo.IsBuildIn)
            {
                return (false, _failedReturnMessage);
            }

            return (true, null);
        }
        private async Task<bool> CheckAsync()
        {
            if (string.IsNullOrWhiteSpace(_dataInfo.Name))
            {
                _logger.Error($"The profile name is empty.");
                return false;
            }

            if (_dataInfo.OrganizationId.IsNullOrEmpty())
            {
                _logger.Error($"The google organization id is empty.");
                return false;
            }

            if (!_supportSortByFields[_profileType].Contains(_dataInfo.SortBy))
            {
                _logger.Error($"The sort by field [{_dataInfo.SortBy}] not support.");
                return false;
            }

            var profileInfoes = await _profileDao.GetProfileInfoesAsync(_dataInfo.OrganizationId, _profileType);
            var hasConflict = profileInfoes.Any(item => I18NEntity.GetString(item.Name.Trim()).Equals(_dataInfo.Name.Trim(), StringComparison.OrdinalIgnoreCase) && item.Id != _dataInfo.Id);
            if (hasConflict)
            {
                _logger.Error($"The google organization [{_dataInfo.OrganizationId}] profile name [{_dataInfo.Name}] has been existis.");
                _failedReturnMessage.ErrorMessage = I18NEntity.GetString("RM_DA_Profile_ProfileName_Exists");
                return false;
            }

            if (!await CheckSizeRangeAsync())
            {
                _logger.Error($"Check size range failed. Google organization [{_dataInfo.OrganizationId}]. Profile json [{JsonConvert.SerializeObject(_dataInfo)}]");
                return false;
            }

            if (!await CheckDateRangeAsync())
            {
                _logger.Error($"Check date range failed. Google organization [{_dataInfo.OrganizationId}]. Profile json [{JsonConvert.SerializeObject(_dataInfo)}]");
                return false;
            }

            if (!await CheckFileExtensionAsync())
            {
                _logger.Error($"Check file extension failed. Google organization [{_dataInfo.OrganizationId}]. Profile json [{JsonConvert.SerializeObject(_dataInfo)}]");
                return false;
            }

            if (!await CheckRuleAsync())
            {
                _logger.Error($"Check rule failed. Google organization [{_dataInfo.OrganizationId}]. Profile json [{JsonConvert.SerializeObject(_dataInfo)}]");
                return false;
            }

            return true;
        }
        private async Task<bool> CheckSizeRangeAsync()
        {
            if (_profileType == RMDiscoveryProfileType.ROT)
            {
                return true;
            }

            var sizeRange = _dataInfo.SizeRange;
            var sizeRangeQueryMode = _dataInfo.SizeRangeQueryMode;
            if (sizeRange < -1 ||
               sizeRange >= 999 ||
               sizeRangeQueryMode != RMDiscoveryGoogleSizeRangeQueryMode.LessThanEqual &&
               sizeRangeQueryMode != RMDiscoveryGoogleSizeRangeQueryMode.GenerateThanEqual
               )
            {
                return false;
            }

            var sizeRanges = (await _sizeRangeDao.GetAllAsync()).Select(item => item.Id).OrderBy(item => item).ToList();
            if (sizeRange != -1 && !sizeRanges.Any(item => item == sizeRange))
            {
                return false;
            }

            if (sizeRanges.First() == sizeRange && sizeRangeQueryMode != RMDiscoveryGoogleSizeRangeQueryMode.LessThanEqual)
            {
                return false;
            }

            if (sizeRanges.First() != sizeRange && sizeRangeQueryMode != RMDiscoveryGoogleSizeRangeQueryMode.GenerateThanEqual)
            {
                return false;
            }

            return true;
        }
        private async Task<bool> CheckDateRangeAsync()
        {
            var gteDateRange = _dataInfo.GreaterThanEqualWithoutInDate;
            var lteDateRange = _dataInfo.LessThanEqualWithoutInDate;

            if (gteDateRange < -1 || lteDateRange > 999 || gteDateRange >= lteDateRange)
            {
                return false;
            }

            var dateRanges = (await _dateRangeDao.GetAllAsync()).Select(item => item.Id).Concat(new List<int> { -1, 999 }).ToHashSet();

            return dateRanges.Contains(gteDateRange) && dateRanges.Contains(lteDateRange);
        }

        private async Task<bool> CheckFileExtensionAsync()
        {
            var fileExtensions = _dataInfo.FileExtensionIds;
            if (fileExtensions.Count == 0)
            {
                return true;
            }

            if (fileExtensions.Count != fileExtensions.ToHashSet().Count)
            {
                return false;
            }

            var dbFileExtensions = (await _fileExtensionDao.GetAllAsync(_dataInfo.OrganizationId)).Select(item => item.Id).ToHashSet();
            if (dbFileExtensions.Intersect(fileExtensions).ToHashSet().Count != fileExtensions.Count)
            {
                return false;
            }

            return true;
        }

        private async Task<bool> CheckRuleAsync()
        {
            if (_profileType == RMDiscoveryProfileType.Inactive)
            {
                return true;
            }

            var ruleIds = _dataInfo.RuleIds;
            if (ruleIds.Count == 0 || ruleIds.ToHashSet().Count != ruleIds.Count)
            {
                return false;
            }

            var dbRuleInfoes = (await _ruleInfoDao.GetRuleInfoesAsync(true, RMDiscoveryRuleDefinitionKind.ROT))
                .Where(item => item.AnalyseMethod != RMDiscoveryRuleAnalyseMethod.DuplicatedDocument).ToList();
            var dbRuleIds = dbRuleInfoes.Select(item => item.Id).ToHashSet();
            if (dbRuleIds.Intersect(ruleIds).ToHashSet().Count != ruleIds.Count)
            {
                return false;
            }

            if (ruleIds.Count == 1)
            {
                return true;
            }

            var versionRuleCount = dbRuleInfoes.Where(item => ruleIds.Contains(item.Id) && item.AnalyseMethod == RMDiscoveryRuleAnalyseMethod.Version)
                .Count();

            return versionRuleCount <= 1;
        }
    }
}
