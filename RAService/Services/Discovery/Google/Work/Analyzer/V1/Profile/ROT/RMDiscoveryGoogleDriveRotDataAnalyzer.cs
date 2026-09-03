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
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.DB.Dao.Discovery.Impl.Google;
using AvePoint.RA.DB.Model.Discovery.Google;
using AvePoint.RA.DB.Model.Discovery.Profile;
using AvePoint.RA.Service.Services.Discovery.Google.Work.Extensions;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer;
using AvePoint.Records.Core.Utilities.Extensions;
using Cloud.Sdk.IE;
using Newtonsoft.Json;

namespace AvePoint.RA.Service.Services.Discovery.Google.Work.Analyzer.V1.Profile.ROT;

public class RMDiscoveryGoogleDriveRotDataAnalyzer
{
    private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryGoogleDriveRotDataAnalyzer));
    
    private readonly IRMDiscoveryGoogleDataDao _dataDao = new RMDiscoveryGoogleDataDao();

    private readonly IRMDiscoveryGoogleProfileDataDao _profileDataDao = new RMDiscoveryGoogleProfileDataDao();

    private readonly IEApiClient _ieApiClient = AosApiUtility.GetInsightsEngineApiClient();
    
    private readonly string _googleOrganizationId;
    
    private readonly RMDiscoveryGoogleDriveInfo _driveInfo;
    
    private readonly RMDiscoveryGoogleProfileInfo _profileInfo;
    
    private readonly List<RMDiscoveryGoogleRuleInfo> _rules;
    
    private readonly List<int> _sizeRangeIds;
    
    private readonly List<int> _dateRangeIds;
    
    private readonly List<string> _fileExtensions;

    public RMDiscoveryGoogleDriveRotDataAnalyzer(
        string googleOrganizationId,
        RMDiscoveryGoogleDriveInfo driveInfo,
        RMDiscoveryGoogleProfileInfo profileInfo,
        List<RMDiscoveryGoogleRuleInfo> rules,
        List<int> sizeRangeIds,
        List<int> dateRangeIds,
        List<string> fileExtensions)
    {
        _googleOrganizationId = googleOrganizationId;
        _driveInfo = driveInfo;
        _profileInfo = profileInfo;
        _rules = rules;
        _sizeRangeIds = sizeRangeIds;
        _dateRangeIds = dateRangeIds;
        _fileExtensions = fileExtensions;
    }

    
    public async Task<bool> AnalysisAsync()
    {
        try
        {
            _logger.Info($"Start analysis google organization [{_googleOrganizationId}] drive [{_driveInfo.DriveId}] profile [{_profileInfo.Id} {_profileInfo.Name}] rot data.");

            await _profileDataDao.DeleteDriveRotDataByDriveIdAsync(_googleOrganizationId, _profileInfo.Id, _driveInfo.Id);

            RMDiscoveryGoogleProfileDriveRotData dataInfo;

            if (_profileInfo.IsBuildIn)
            {
                dataInfo = await AnalysisBuildInProfileDataAsync();
            }
            else
            {
                var ruleIds = JsonConvert.DeserializeObject<List<int>>(_profileInfo.RuleIdsJson);
                var ruleInfoes = _rules.Where(item => ruleIds.Contains(item.Id)).ToList();
                if (ruleInfoes.Count == 1)
                {
                    dataInfo = await AnalysisSingleRuleProfileDataAsync(ruleInfoes);
                }
                else
                {
                    dataInfo = await AnalysisMultipleRuleProfileDataAsync(ruleInfoes);
                }
            }

            dataInfo.FileTotalSize = _driveInfo.FileTotalSize;
            dataInfo.ContainerId = _driveInfo.ContainerId;
            dataInfo.DriveId = _driveInfo.Id;

            await _profileDataDao.AddDriveRotDataListAsync(_googleOrganizationId, _profileInfo.Id, dataInfo);

            _logger.Info($"End analysis google organization [{_googleOrganizationId}] drive [{_driveInfo.Id}] profile [{_profileInfo.Id} {_profileInfo.Name}] rot data.");

            return true;
        }
        catch(Exception e)
        {
            _logger.Error($"An error occurred while analysis google organization [{_googleOrganizationId}] drive [{_driveInfo.Id}] profile [{_profileInfo.Id} {_profileInfo.Name}] rot data. Error: {e}");
            return false;
        }
    }

    private async Task<RMDiscoveryGoogleProfileDriveRotData> AnalysisMultipleRuleProfileDataAsync(List<RMDiscoveryGoogleRuleInfo> ruleInfoes)
    {
        _logger.Info($"Analysis google organization [{_googleOrganizationId}] site [{_driveInfo.DriveId}] profile [{_profileInfo.Id} {_profileInfo.Name}] multiple rule rot data.");

        var res = new RMDiscoveryGoogleProfileDriveRotData
        {
            DriveId = _driveInfo.Id,
            RotFileTotalSize = await AnalysisFileTotalSizeFromMongoDBAsync(ruleInfoes)
        };

        var rCategoryRules = ruleInfoes.Where(item => item.Category == RMDiscoveryRuleCategory.Redundant).ToList();
        res.RCategoryFileTotalSize = await AnalysisMutipleRuleFileTotalSizeAsync(rCategoryRules);

        var oCategoryRules = ruleInfoes.Where(item => item.Category == RMDiscoveryRuleCategory.Obsolete).ToList();
        res.OCategoryFileTotalSize = await AnalysisMutipleRuleFileTotalSizeAsync(oCategoryRules);

        var tCategoryRules = ruleInfoes.Where(item => item.Category == RMDiscoveryRuleCategory.Trivial).ToList();
        res.TCategoryFileTotalSize = await AnalysisMutipleRuleFileTotalSizeAsync(tCategoryRules);

        foreach(var ruleInfo in ruleInfoes)
        {
            var totalSize = await AnalysisDriveRuleLevelFileTotalSizeFromSqliteDBAsync(ruleInfo);
            res.CustomColumns.Add(new(ruleInfo.ToCustomColumn().Name, totalSize, typeof(long)));
        }

        return res;
    }

    private async Task<long> AnalysisMutipleRuleFileTotalSizeAsync(List<RMDiscoveryGoogleRuleInfo> ruleInfoes)
    {
        if(ruleInfoes.Count == 0)
        {
            return 0;
        }

        if(ruleInfoes.Count == 1)
        {
            return await AnalysisDriveRuleLevelFileTotalSizeFromSqliteDBAsync(ruleInfoes.First());
        }

        return await AnalysisFileTotalSizeFromMongoDBAsync(ruleInfoes);
    }

    private async Task<long> AnalysisFileTotalSizeFromMongoDBAsync(List<RMDiscoveryGoogleRuleInfo> ruleInfoes)
    {
        var versionRule = ruleInfoes.FirstOrDefault(item => item.AnalyseMethod == RMDiscoveryRuleAnalyseMethod.Version);

        var documentRules = ruleInfoes.Where(item => item.AnalyseMethod == RMDiscoveryRuleAnalyseMethod.GoogleDocument)
            .ToList();
        var sizeRangeIdsTemp = _sizeRangeIds;
        var dateRangeIdsTemp = _dateRangeIds.Where(item =>
            item > _profileInfo.GreaterThanEqualWithoutInDate &&
            item <= _profileInfo.LessThanEqualWithoutInDate).ToList();
        if (_profileInfo.GreaterThanEqualWithoutInDate == -1)
        {
            dateRangeIdsTemp.Add(-1);
        }


        var conditionalSql = " (" + string.Join(" or ", documentRules.ConvertAll(item =>
                                     $" tags_{item.UniqueId.ToString().ToLower().Replace("-", "")} gt 0 ")) +
                             ") ";
        var aggregateSql =
            $"FileSize with sum as file_total_size, HistoryVersionsSize with sum as file_history_version_total_size";
        if (versionRule != null)
        {
            var versionTagName = $"tags_{versionRule.UniqueId.ToString().ToLower().Replace("-", "")}";
            aggregateSql += $",{versionTagName}/total_size with sum as version_rule_total_size";
        }

        var fileTotalSize = 0L;
        var versionTotalSize = 0L;

        foreach (var sizeRangeId in sizeRangeIdsTemp)
        {
            foreach (var dateRangeId in dateRangeIdsTemp)
            {
                foreach (var fileExtension in _fileExtensions)
                {
                    var sql =
                        $"{RMDiscoveryGoogleAnalysisConfiguration.ODATA_URI}?$apply=filter(DriveId eq '{_driveInfo.DriveId}' " +
                        $"and {RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME} eq {sizeRangeId} " +
                        $"and {RMDiscoveryBuildInRule.WITHOUT_IN_DATE_COLUMN_NAME} eq {dateRangeId} " +
                        $"and {RMDiscoveryBuildInRule.ROT_RULE_NAME} gt 0 " +
                        $"and FileExtension eq '{fileExtension.EscapeSpecialCharacters()}' " +
                        $"and {conditionalSql} )" +
                        $"/aggregate({aggregateSql})";
                    var dataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(sql, _googleOrganizationId,
                        $"Drive [{_driveInfo.DriveId}] analysis profile rot data");
                    var dataObj = JsonConvert.DeserializeObject<List<ExpandoObject>>(dataJson).FirstOrDefault();
                    if (dataObj != null)
                    {
                        fileTotalSize += dataObj.GetValue<long>("file_total_size");
                        fileTotalSize += dataObj.GetValue<long>("file_history_version_total_size");
                        if (versionRule != null)
                        {
                            versionTotalSize += dataObj.GetValue<long>("version_rule_total_size");
                        }
                    }
                }
            }
        }


        if (versionRule != null)
        {
            var singleVersionRuleTotalSize = await AnalysisDriveRuleLevelFileTotalSizeFromSqliteDBAsync(versionRule);
            versionTotalSize = singleVersionRuleTotalSize - versionTotalSize;
        }

        return fileTotalSize + versionTotalSize;
    }

    private async Task<long> AnalysisDriveRuleLevelFileTotalSizeFromSqliteDBAsync(RMDiscoveryGoogleRuleInfo ruleInfo)
    {
        var (sqlConditionList, parameters) = GetSqlConditionalExpression();
        sqlConditionList.Add(" [Rule] = @Rule");
        parameters.Add(new SQLiteParameter("@Rule", ruleInfo.Id));
        var sqlConditionExpression = string.Join(" AND ", sqlConditionList);

        var siteDataEnumerable = _dataDao.GetDriveRuleLevelRotDataBySqlConditionalExpressionAsync(_googleOrganizationId, _driveInfo.Id, sqlConditionExpression, parameters);

        return await siteDataEnumerable.SumAsync(siteData => siteData.FileTotalSize);
    }

    private (List<string> sqlConditionList, List<SQLiteParameter> parameters) GetSqlConditionalExpression()
    {
        List<string> sqlConditionList = [];
        List<SQLiteParameter> sqlParameters = [];
        
        if (_profileInfo.IsBuildIn)
        {
            return ([], []);
        }
        
        if (!string.IsNullOrWhiteSpace(_profileInfo.FileExtensionIdsJson))
        {
            var fileExtensionIds = JsonConvert.DeserializeObject<List<int>>(_profileInfo.FileExtensionIdsJson);
            if (fileExtensionIds.Count > 0)
            {
                var fileExtensionSqlConditionList = new List<string>();
                foreach (var fileExtensionId in fileExtensionIds)
                {
                    fileExtensionSqlConditionList.Add($"FileExtension = @FileExtension{fileExtensionId}");
                    sqlParameters.Add(new($"@FileExtension{fileExtensionId}", fileExtensionId));
                }

                var fileExtensionSqlCondition = $"({string.Join(" OR ", fileExtensionSqlConditionList)})";
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

        return (sqlConditionList, sqlParameters);
    }

    private async Task<RMDiscoveryGoogleProfileDriveRotData> AnalysisSingleRuleProfileDataAsync(List<RMDiscoveryGoogleRuleInfo> ruleInfoes)
    {
        _logger.Info($"Analysis google organization [{_googleOrganizationId}] site [{_driveInfo.DriveId}] profile [{_profileInfo.Id} {_profileInfo.Name}] single rule rot data.");

        var res = new RMDiscoveryGoogleProfileDriveRotData
        {
            DriveId = _driveInfo.Id
        };

        var ruleInfo = ruleInfoes.First();
        var totalSize = await AnalysisDriveRuleLevelFileTotalSizeFromSqliteDBAsync(ruleInfo);

        res.RotFileTotalSize = totalSize;
        if(ruleInfo.Category == RMDiscoveryRuleCategory.Redundant)
        {
            res.RCategoryFileTotalSize = totalSize;
        }
        else if(ruleInfo.Category == RMDiscoveryRuleCategory.Obsolete)
        {
            res.OCategoryFileTotalSize = totalSize;
        }
        else
        {
            res.TCategoryFileTotalSize = totalSize;
        }

        res.CustomColumns.Add(new(ruleInfo.ToCustomColumn().Name, totalSize, typeof(long)));

        return res;
    }

    private async Task<RMDiscoveryGoogleProfileDriveRotData> AnalysisBuildInProfileDataAsync()
    {
        _logger.Info($"Analysis google organization [{_googleOrganizationId}] drive [{_driveInfo.DriveId}] profile [{_profileInfo.Id} {_profileInfo.Name}] build in rot data.");
        RMDiscoveryGoogleProfileDriveRotData res = new();

        var dataEnumerable = _dataDao.GetDriveRootLevelRotDataBySqlConditionalExpressionAsync(_googleOrganizationId, _driveInfo.Id, "", []);
        await foreach(var data in dataEnumerable)
        {
            res.RotFileTotalSize += data.FileTotalSize;
        }

        var rCategoryDataEnumerable = _dataDao.GetDriveCategoryLevelRotDataBySqlConditionalExpressionAsync(_googleOrganizationId, _driveInfo.Id, "Category = @Category", new List<SQLiteParameter> { new("@Category", RMDiscoveryRuleCategory.Redundant) });
        await foreach(var data in rCategoryDataEnumerable)
        {
            res.RCategoryFileTotalSize += data.FileTotalSize;
        }

        var oCategoryDataEnumerable = _dataDao.GetDriveCategoryLevelRotDataBySqlConditionalExpressionAsync(_googleOrganizationId, _driveInfo.Id, "Category = @Category", new List<SQLiteParameter> { new("@Category", RMDiscoveryRuleCategory.Obsolete) });
        await foreach (var data in oCategoryDataEnumerable)
        {
            res.OCategoryFileTotalSize += data.FileTotalSize;
        }

        var tCategoryDataEnumerable = _dataDao.GetDriveCategoryLevelRotDataBySqlConditionalExpressionAsync(_googleOrganizationId, _driveInfo.Id, "Category = @Category", new List<SQLiteParameter> { new("@Category", RMDiscoveryRuleCategory.Trivial) });
        await foreach (var data in tCategoryDataEnumerable)
        {
            res.TCategoryFileTotalSize += data.FileTotalSize;
        }

        return res;
    }
}