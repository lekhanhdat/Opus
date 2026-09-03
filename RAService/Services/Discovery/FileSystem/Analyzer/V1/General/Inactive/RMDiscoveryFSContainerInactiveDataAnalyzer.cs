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
using AvePoint.RA.DB.Core.Discovery;
using AvePoint.RA.DB.Dao.Discovery.FileSystem;
using AvePoint.RA.DB.Dao.Discovery.Impl.FileSystem;
using AvePoint.RA.DB.Model.Discovery.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.FileSystem.Analyzer.V1.General.Inactive
{
    public class RMDiscoveryFSContainerInactiveDataAnalyzer
    {

        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryFSContainerInactiveDataAnalyzer));

        private readonly IRMDiscoveryFSDataDao _dataDao;

        private readonly RMDiscoveryJobType _jobType;

        private readonly string _connectionId;

        private readonly int _containerId;

        private readonly List<RMDiscoveryFSRuleInfo> _rules;

        private readonly List<RMDiscoveryFSContainerInactiveData> _dataList;

        public RMDiscoveryFSContainerInactiveDataAnalyzer(
            RMDiscoveryJobType jobType,
            string connectionId,
            int containerId,
            List<RMDiscoveryFSRuleInfo> rules
        )
        {
            _dataDao = new RMDiscoveryFSDataDao();
            _jobType = jobType;
            _connectionId = connectionId;
            _containerId = containerId;
            _rules = rules;
            _dataList = [];
        }

        public void Increse(List<RMDiscoveryFSConnectionInactiveData> connectionDataList)
        {
            if (_jobType == RMDiscoveryJobType.Retry)
            {
                return;
            }

            var inactiveColumns = _rules.ConvertAll(item => item.ToCustomColumn());

            foreach (var connectionData in connectionDataList)
            {
                var data = _dataList.FirstOrDefault(item =>
                    item.FileExtension == connectionData.FileExtension &&
                    item.SizeRange == connectionData.SizeRange &&
                    item.WithoutInDate == connectionData.WithoutInDate
                );
                if (data == null)
                {
                    data = new RMDiscoveryFSContainerInactiveData
                    {
                        ContainerId = _containerId,
                        WithoutInDate = connectionData.WithoutInDate,
                        FileExtension = connectionData.FileExtension,
                        SizeRange = connectionData.SizeRange,
                        CustomColumns = new()
                    };
                    foreach (var inactiveColumn in inactiveColumns)
                    {
                        data.CustomColumns.Add(new RMDiscoveryCustomColumnWithValue(inactiveColumn.Name, 0, typeof(long)));
                    }

                    _dataList.Add(data);
                }

                foreach (var inactiveColumn in inactiveColumns)
                {
                    var siteColumnValue = connectionData.CustomColumns.First(item => item.Name == inactiveColumn.Name);
                    var containerColumnValue = data.CustomColumns.First(item => item.Name == inactiveColumn.Name);
                    containerColumnValue.Value = long.Parse(containerColumnValue.Value.ToString()) + long.Parse(siteColumnValue.Value.ToString());
                }

                data.FileTotalSize += connectionData.FileTotalSize;
                data.FileSumCount += connectionData.FileSumCount;
            }
        }

        public async Task<bool> SaveAsync()
        {
            try
            {
                if (_dataList.Count > 0)
                {
                    await _dataDao.AddOrUpdateContainerInactiveDataUnderSameContainerAsync(_dataList.ToArray());
                }

                _logger.Info($"Succeed save tenant [{_connectionId}] container [{_containerId}] [{_dataList.Count}] inactive data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Info($"An error occurred while save tenant [{_connectionId}] container [{_containerId}] inactive data. Error: {e}");
                return false;
            }
        }
    }
}
