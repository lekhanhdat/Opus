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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V2.Rot
{
    public class RMDiscoveryOffice365ContainerRotDataAnalyzer
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365ContainerRotDataAnalyzer));

        private readonly IRMDiscoveryOffice365DataDao _dataDao;

        private readonly RMDiscoveryJobType _jobType;

        private readonly Guid _o365TenantId;

        private readonly int _containerId;

        private readonly List<RMDiscoveryOffice365ContainerRotData> _dataList;

        public RMDiscoveryOffice365ContainerRotDataAnalyzer(
            RMDiscoveryJobType jobType,
            Guid o365TenantId,
            int containerId
            )
        {
            _dataDao = new RMDiscoveryOffice365DataDao();
            _jobType = jobType;
            _o365TenantId = o365TenantId;
            _containerId = containerId;
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

                if(data == null)
                {
                    data = new RMDiscoveryOffice365ContainerRotData
                    {
                        ContainerId = _containerId,
                        WithoutInDate = siteData.WithoutInDate,
                        FileExtension = siteData.FileExtension,
                        SizeRange = siteData.SizeRange,
                        Rule = siteData.Rule
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
                if(_jobType == RMDiscoveryJobType.Retry)
                {
                    return await RecalculateAndSaveAsync();
                }

                if(_dataList.Count > 0)
                {
                    await _dataDao.AddOrUpdateContainerRotDataAsync(_o365TenantId, _dataList.ToArray());
                }

                _logger.Info($"Succeed save tenant [{_o365TenantId}] container [{_containerId}] [{_dataList.Count}] rot data.");

                return true;
            }
            catch(Exception e)
            {
                _logger.Info($"An error occurred while save tenant [{_o365TenantId}] container [{_containerId}] rot data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> RecalculateAndSaveAsync()
        {
            try
            {
                var enumerableDataList = _dataDao.GetSiteRotDataListByContainerAsync(_o365TenantId, _containerId);
                await foreach (var data in enumerableDataList)
                {
                    var containerRotData = _dataList.FirstOrDefault(item =>
                        item.WithoutInDate == data.WithoutInDate &&
                        item.FileExtension == data.FileExtension &&
                        item.SizeRange == data.SizeRange &&
                        item.Rule == data.Rule
                    );

                    if (containerRotData == null)
                    {
                        containerRotData = new RMDiscoveryOffice365ContainerRotData
                        {
                            ContainerId = _containerId,
                            WithoutInDate = data.WithoutInDate,
                            FileExtension = data.FileExtension,
                            SizeRange = data.SizeRange,
                            Rule = data.Rule,
                            FileTotalSize = 0,
                            FileSumCount = 0,
                        };
                        _dataList.Add(containerRotData);
                    }

                    containerRotData.FileTotalSize += data.FileTotalSize;
                    containerRotData.FileSumCount += data.FileSumCount;
                }

                await _dataDao.AddOrUpdateContainerRotDataAsync(_o365TenantId, _containerId, _dataList.ToArray());

                _logger.Info($"Succeed recalculate and save tenant [{_o365TenantId}] container [{_containerId}] [{_dataList.Count}] rot data.");

                return true;
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while recalculate and save tenant [{_o365TenantId}] container [{_containerId}] rot data. Error: {e}");
                return false;
            }
        }
    }
}
