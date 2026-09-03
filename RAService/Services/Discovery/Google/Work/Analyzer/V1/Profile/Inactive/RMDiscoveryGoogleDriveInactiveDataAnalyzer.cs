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
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Model.Query.Google.Parameter;
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.DB.Dao.Discovery.Impl.Google;
using AvePoint.RA.DB.Model.Discovery.Google;
using AvePoint.RA.DB.Model.Discovery.Profile;
using Newtonsoft.Json;

namespace AvePoint.RA.Service.Services.Discovery.Google.Work.Analyzer.V1.Profile.Inactive;

public class RMDiscoveryGoogleDriveInactiveDataAnalyzer
{
    private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryGoogleDriveInactiveDataAnalyzer));
    
    private readonly IRMDiscoveryGoogleDataDao _dataDao = new RMDiscoveryGoogleDataDao();

    private readonly IRMDiscoveryGoogleProfileDataDao _profileDataDao = new RMDiscoveryGoogleProfileDataDao();
    
    private readonly string _googleOrganizationId;
    
    private readonly RMDiscoveryGoogleDriveInfo _driveInfo;
    
    private readonly RMDiscoveryGoogleProfileInfo _profileInfo;
    
    private readonly List<RMDiscoveryGoogleRuleInfo> _rules;

    public RMDiscoveryGoogleDriveInactiveDataAnalyzer(
        string googleOrganizationId,
        RMDiscoveryGoogleDriveInfo driveInfo,
        RMDiscoveryGoogleProfileInfo profileInfo,
        List<RMDiscoveryGoogleRuleInfo> rules)
    {
        _googleOrganizationId = googleOrganizationId;
        _driveInfo = driveInfo;
        _profileInfo = profileInfo;
        _rules = rules;
    }

    public async Task<bool> AnalysisAsync()
    {
        try
        {
            _logger.Info($"Start analysis google organization [{_googleOrganizationId}] drive [{_driveInfo.DriveId}] profile [{_profileInfo.Id} {_profileInfo.Name}] inactive data.");

            await _profileDataDao.DeleteDriveInactiveDataByDriveIdAsync(_googleOrganizationId, _profileInfo.Id, _driveInfo.Id);

            var customColumns = _rules.ConvertAll(item => item.ToCustomColumn());

            var dataInfo = new RMDiscoveryGoogleProfileDriveInactiveData()
            {
                DriveId = _driveInfo.Id,
                ContainerId = _driveInfo.ContainerId,
                FileTotalSize = _driveInfo.FileTotalSize,
                FileSumCount = _driveInfo.FileSumCount,
            };
            foreach (var customColumn in customColumns)
            {
                dataInfo.CustomColumns.Add(new(customColumn.Name, 0L, typeof(long)));
            }

            var (sqlConditionalExpression, parameters) = GetSqlConditionalExpression();
            var dataEnumerable = _dataDao.GetDriveInactiveDataBySqlConditionalExpressionAsync(_googleOrganizationId,
                _driveInfo.Id, sqlConditionalExpression, parameters, customColumns);

            await foreach (var data in dataEnumerable)
            {
                dataInfo.InactiveFileTotalSize += data.FileTotalSize;
                dataInfo.InactiveFileSumCount += data.FileSumCount;
                foreach (var customColumn in customColumns)
                {
                    var targetDataCustomColumn = dataInfo.CustomColumns.First(item => item.Name == customColumn.Name);
                    var sourceDataCustomColumn = data.CustomColumns.First(item => item.Name == customColumn.Name);
                    targetDataCustomColumn.Value = Convert.ToInt64(targetDataCustomColumn.Value) +
                                                   Convert.ToInt64(sourceDataCustomColumn.Value);
                }
            }

            await _profileDataDao.AddDriveInactiveDataListAsync(_googleOrganizationId, _profileInfo.Id, dataInfo);

            _logger.Info(
                $"End analysis google organization [{_googleOrganizationId}] drive [{_driveInfo.Id}] profile [{_profileInfo.Id} {_profileInfo.Name}] inactive data.");

            return true;
        }
        catch (Exception e)
        {
            _logger.Error(
                $"An error occurred while analysis google organization [{_googleOrganizationId}] drive [{_driveInfo.Id}] profile [{_profileInfo.Id} {_profileInfo.Name}] inactive data. Error: {e}");
            return false;
        }
    }
    
    
        private (string sqlConditionalExpression, List<SQLiteParameter> parameters) GetSqlConditionalExpression()
        {
            List<string> sqlConditionList = [];
            List<SQLiteParameter> sqlParameters = [] ;

            if(_profileInfo.IsBuildIn)
            {
                return ("", []);
            }

            if(_profileInfo.SizeRange > 0)
            {
                sqlConditionList.Add(_profileInfo.SizeRangeQueryMode == RMDiscoveryGoogleSizeRangeQueryMode.LessThanEqual
                    ? "SizeRange <= @SizeRange"
                    : "SizeRange >= @SizeRange");
                sqlParameters.Add(new("@SizeRange", _profileInfo.SizeRange));
            }

            if(!string.IsNullOrWhiteSpace(_profileInfo.FileExtensionIdsJson))
            {
                var fileExtensionIds = JsonConvert.DeserializeObject<List<int>>(_profileInfo.FileExtensionIdsJson);
                if(fileExtensionIds.Count > 0)
                {
                    List<string> fileExtensionSqlConditionList = [];
                    foreach (var fileExtensionId in fileExtensionIds)
                    {
                        fileExtensionSqlConditionList.Add($"FileExtension = @FileExtension{fileExtensionId}");
                        sqlParameters.Add(new($"@FileExtension{fileExtensionId}", fileExtensionId));
                    }

                    var fileExtensionSqlCondition = $"({string.Join(" OR ",fileExtensionSqlConditionList)})";
                    sqlConditionList.Add(fileExtensionSqlCondition);
                }
            }

            if(_profileInfo.GreaterThanEqualWithoutInDate > -1)
            {
                sqlConditionList.Add("WithoutInDate > @GreaterThanEqualWithoutInDate");
                sqlParameters.Add(new("@GreaterThanEqualWithoutInDate", _profileInfo.GreaterThanEqualWithoutInDate));
            }

            sqlConditionList.Add("WithoutInDate <= @LessThanEqualWithoutInDate");
            sqlParameters.Add(new("@LessThanEqualWithoutInDate", _profileInfo.LessThanEqualWithoutInDate));

            return (string.Join(" AND ", sqlConditionList), sqlParameters);
        }
}