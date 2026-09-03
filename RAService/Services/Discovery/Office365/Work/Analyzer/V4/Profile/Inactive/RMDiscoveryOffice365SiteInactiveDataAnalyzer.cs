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
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery.Profile;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V4.Profile.Inactive
{
    public class RMDiscoveryOffice365SiteInactiveDataAnalyzer
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365SiteInactiveDataAnalyzer));

        private readonly IRMDiscoveryOffice365DataV3Dao _dataDao;

        private readonly IRMDiscoveryOffice365ProfileDataDao _profileDataDao;

        private readonly Guid _o365TenantId;

        private readonly RMDiscoveryOffice365SiteInfo _siteInfo;

        private readonly RMDiscoveryOffice365ProfileInfo _profileInfo;

        private readonly List<RMDiscoveryOffice365RuleInfo> _rules;

        public RMDiscoveryOffice365SiteInactiveDataAnalyzer(
            Guid o365TenantId,
            RMDiscoveryOffice365SiteInfo siteInfo,
            RMDiscoveryOffice365ProfileInfo profileInfo,
            List<RMDiscoveryOffice365RuleInfo> rules
        )
        {
            _dataDao = new RMDiscoveryOffice365DataV3Dao();
            _profileDataDao = new RMDiscoveryOffice365ProfileDataDao();
            _o365TenantId = o365TenantId;
            _siteInfo = siteInfo;
            _profileInfo = profileInfo;
            _rules = rules;
        }

        public async Task<bool> AnalysisAsync()
        {
            try
            {
                _logger.Info($"Start analysis o365 tenant [{_o365TenantId}] site [{_siteInfo.Id}] profile [{_profileInfo.Id} {_profileInfo.Name}] inactive data.");

                await _profileDataDao.DeleteSiteInactiveDataBySiteIdAsync(_o365TenantId, _profileInfo.Id, _siteInfo.Id);

                var customColumns = _rules.ConvertAll(item => item.ToCustomColumn());

                var dataInfo = new RMDiscoveryProfileSiteInactiveData
                {
                    SiteId = _siteInfo.Id,
                    ContainerId = _siteInfo.ContainerId,
                    FileTotalSize = _siteInfo.FileTotalSize,
                    FileSumCount = _siteInfo.FileSumCount,
                };
                foreach (var customColumn in customColumns)
                {
                    dataInfo.CustomColumns.Add(new(customColumn.Name, 0L, typeof(long)));
                }

                var (sqlConditionalExpression, parameters) = GetSqlConditionalExpression();
                var dataEnumerable = _dataDao.GetSiteInactiveDataBySqlConditionalExpressionAsync(_o365TenantId, _siteInfo.Id, sqlConditionalExpression, parameters, customColumns);

                await foreach(var data in dataEnumerable)
                {
                    dataInfo.InactiveFileTotalSize += data.FileTotalSize;
                    dataInfo.InactiveFileSumCount += data.FileSumCount;
                    foreach (var customColumn in customColumns)
                    {
                        var targetDataCustomColumn = dataInfo.CustomColumns.First(item => item.Name == customColumn.Name);
                        var sourceDataCustomColumn = data.CustomColumns.First(item => item.Name == customColumn.Name);
                        targetDataCustomColumn.Value = Convert.ToInt64(targetDataCustomColumn.Value) + Convert.ToInt64(sourceDataCustomColumn.Value);
                    }
                }

                await _profileDataDao.AddSiteInactiveDataListAsync(_o365TenantId, _profileInfo.Id, dataInfo);

                _logger.Info($"End analysis o365 tenant [{_o365TenantId}] site [{_siteInfo.Id}] profile [{_profileInfo.Id} {_profileInfo.Name}] inactive data.");

                return true;
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while analysis o365 tenant [{_o365TenantId}] site [{_siteInfo.Id}] profile [{_profileInfo.Id} {_profileInfo.Name}] inactive data. Error: {e}");
                return false;
            }
        }

        private (string sqlConditionalExpression, List<SQLiteParameter> parameters) GetSqlConditionalExpression()
        {
            var sqlConditionList = new List<string>();
            var sqlParameters = new List<SQLiteParameter>();

            if(_profileInfo.IsBuildIn)
            {
                return ("", []);
            }

            if(_profileInfo.SizeRange > 0)
            {
                if(_profileInfo.SizeRangeQueryMode == RMDiscoverySizeRangeQueryMode.LessThanEqual)
                {
                    sqlConditionList.Add("SizeRange <= @SizeRange");
                }
                else
                {
                    sqlConditionList.Add("SizeRange >= @SizeRange");
                } 
                sqlParameters.Add(new("@SizeRange", _profileInfo.SizeRange));
            }

            if(!string.IsNullOrWhiteSpace(_profileInfo.FileExtensionIdsJson))
            {
                var fileExtensionIds = JsonConvert.DeserializeObject<List<int>>(_profileInfo.FileExtensionIdsJson);
                if(fileExtensionIds.Count > 0)
                {
                    var fileExtensionSqlConditionList = new List<string>();
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
}
