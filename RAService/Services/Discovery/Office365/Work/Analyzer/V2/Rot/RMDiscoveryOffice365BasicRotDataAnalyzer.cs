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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Office365;
using Microsoft.SharePoint.Client.CompliancePolicy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V2.Rot
{
    public class RMDiscoveryOffice365BasicRotDataAnalyzer
    {

        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365BasicRotDataAnalyzer));

        private readonly IRMDiscoveryOffice365DataDao _dataDao;

        private readonly IRMDiscoveryOffice365NodeDao _nodeDao;

        private readonly RMDiscoveryJobType _jobType;

        private readonly Guid _o365TenantId;

        private readonly SourceFlag _contentSource;

        private readonly List<RMDiscoveryOffice365BasicRotData> _dataList;

        public RMDiscoveryOffice365BasicRotDataAnalyzer(
            RMDiscoveryJobType jobType,
            Guid o365TenantId,
            SourceFlag contentSource
        )
        {
            _dataDao = new RMDiscoveryOffice365DataDao();
            _nodeDao = new RMDiscoveryOffice365NodeDao();
            _jobType = jobType;
            _o365TenantId = o365TenantId;
            _contentSource = contentSource;
            _dataList = [];
        }

        public void Increse(List<RMDiscoveryOffice365SiteRotData> siteDataList)
        {
            if(_jobType == RMDiscoveryJobType.Retry)
            {
                return;
            }

            foreach(var siteData in siteDataList)
            {
                var data = _dataList.FirstOrDefault(item =>
                        item.FileExtension == siteData.FileExtension &&
                        item.SizeRange == siteData.SizeRange &&
                        item.WithoutInDate == siteData.WithoutInDate &&
                        item.Rule == siteData.Rule
                    );
                if (data == null)
                {
                    data = new RMDiscoveryOffice365BasicRotData
                    {
                        WithoutInDate = siteData.WithoutInDate,
                        FileExtension = siteData.FileExtension,
                        SizeRange = siteData.SizeRange,
                        Rule = siteData.Rule,
                        ContentSource = _contentSource
                    };
                    _dataList.Add(data);
                }
                data.FileTotalSize += siteData.FileTotalSize;
                data.FileSumCount += siteData.FileSumCount;
            }
        }

        public async Task<bool> SaveAsync()
        {
            try
            {
                if(_jobType == RMDiscoveryJobType.Append)
                {
                    return await AppendAndSaveAsync();
                }
                else if(_jobType == RMDiscoveryJobType.Retry)
                {
                    return await RecalculateAndSaveAsync();
                }

                await _dataDao.AddOrUpdateBasicRotDataAsync(_o365TenantId, _contentSource, _dataList.ToArray());

                _logger.Info($"Succeed save tenant [{_o365TenantId}] [{_contentSource}] [{_dataList.Count}] basic rot data.");

                return true;
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while save tenant [{_o365TenantId}] [{_contentSource}] [{_dataList.Count}] basic rot data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> AppendAndSaveAsync()
        {
            try
            {
                var existsDataList = await _dataDao.GetBasicRotDataListAsync(_o365TenantId, _contentSource);
                foreach(var data in _dataList)
                {
                    var existsBaicData = existsDataList.FirstOrDefault(item =>
                        item.FileExtension == data.FileExtension &&
                        item.SizeRange == data.SizeRange &&
                        item.WithoutInDate == data.WithoutInDate &&
                        item.Rule == data.Rule
                    );
                    if (existsBaicData == null)
                    {
                        existsBaicData = new RMDiscoveryOffice365BasicRotData
                        {
                            WithoutInDate = data.WithoutInDate,
                            FileExtension = data.FileExtension,
                            SizeRange = data.SizeRange,
                            Rule = data.Rule,
                            ContentSource = _contentSource
                        };
                        existsDataList.Add(existsBaicData);
                    }
                    existsBaicData.FileTotalSize += data.FileTotalSize;
                    existsBaicData.FileSumCount += data.FileSumCount;
                }

                await _dataDao.AddOrUpdateBasicRotDataAsync(_o365TenantId, _contentSource, existsDataList.ToArray());

                _logger.Info($"Succeed append and save tenant [{_o365TenantId}] [{_contentSource}] [{existsDataList.Count}] basic rot data.");
                return true;
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while append and save tenant [{_o365TenantId}] [{_contentSource}] [{_dataList.Count}] basic rot data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> RecalculateAndSaveAsync()
        {
            try
            {
                var containerIds = (await _nodeDao.GetAllDiscoveryContainersAsync(_o365TenantId, _contentSource)).Select(item => item.Id).ToHashSet();
                var containerDataList = await _dataDao.GetContainerRotDataListAsync(_o365TenantId).ToListAsync();
                containerDataList = containerDataList.Where(item => containerIds.Contains(item.ContainerId)).ToList();

                foreach(var containerData in containerDataList)
                {
                    var data = _dataList.FirstOrDefault(item =>
                            item.WithoutInDate == containerData.WithoutInDate &&
                            item.FileExtension == containerData.FileExtension &&
                            item.SizeRange == containerData.SizeRange &&
                            item.Rule == containerData.Rule
                        );
                    if (data == null)
                    {
                        data = new RMDiscoveryOffice365BasicRotData
                        {
                            WithoutInDate = containerData.WithoutInDate,
                            FileExtension = containerData.FileExtension,
                            SizeRange = containerData.SizeRange,
                            Rule = containerData.Rule,
                            FileTotalSize = 0,
                            FileSumCount = 0,
                            ContentSource = _contentSource
                        };
                        _dataList.Add(data);
                    }

                    data.FileTotalSize += containerData.FileTotalSize;
                    data.FileSumCount += containerData.FileSumCount;
                }

                await _dataDao.AddOrUpdateBasicRotDataAsync(_o365TenantId, _contentSource, _dataList.ToArray());

                _logger.Info($"Succeed recalculate and save tenant [{_o365TenantId}] [{_contentSource}] [{_dataList.Count}] basic rot data.");
                return true;
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while recalculate and save tenant [{_o365TenantId}] [{_contentSource}] [{_dataList.Count}] basic rot data. Error: {e}");
                return false;
            }
        }
    }
}
