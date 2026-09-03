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
using AvePoint.RA.DB.Dao.Discovery.AOSP;
using AvePoint.RA.DB.Dao.Discovery.Impl.AOSP;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.AOSP.Work.Analyzer
{
    public class RMDiscoveryAOSPFileExtensionAnalysisManager
    {

        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryAOSPFileExtensionAnalysisManager));

        private readonly Guid _o365TenantId;

        private readonly Dictionary<string, int> _fileTypes = new();

        private readonly IRMDiscoveryAOSPFileExtensionDao _fileTypeDao;

        public RMDiscoveryAOSPFileExtensionAnalysisManager(Guid o365TenantId)
        {
            _o365TenantId = o365TenantId;
            _fileTypeDao = new RMDiscoveryAOSPFileExtensionDao();
        }


        public async Task InitAsync()
        {
            var fileTypes = await _fileTypeDao.GetAllAsync(_o365TenantId);
            foreach (var fileType in fileTypes)
            {
                _fileTypes.Add(fileType.Name, fileType.Id);
            }
            _logger.Info($"File types [{string.Join(", ", _fileTypes.Keys)}], [{string.Join(", ", _fileTypes.Values)}].");
        }

        public async Task AddOrUpdateAsync(params string[] fileTypes)
        {
            var convertedFileTypes = fileTypes.ConvertAll(item => string.IsNullOrWhiteSpace(item) ? "RM_FA_FileType_Empty" : item).ToList();
            var notExistentInMemoryFileTypes = convertedFileTypes.Except(_fileTypes.Keys).ToList();
            if (notExistentInMemoryFileTypes.Any())
            {
                _logger.Info($"The file types that does not exist in memory are [{string.Join(", ", notExistentInMemoryFileTypes)}].");
                var existsFileTypeModels = await _fileTypeDao.GetAllAsync(_o365TenantId);
                foreach (var fileTypeModel in existsFileTypeModels)
                {
                    _fileTypes[fileTypeModel.Name] = fileTypeModel.Id;
                }

                var existsFileTypes = existsFileTypeModels.Select(item => item.Name).ToList();
                var notExistentInDBFileTypes = notExistentInMemoryFileTypes.Except(existsFileTypes).ToList();
                if (notExistentInDBFileTypes.Any())
                {
                    _logger.Info($"The file types needs to be added are [{string.Join(", ", notExistentInDBFileTypes)}].");
                    var needAddFileTypes = notExistentInDBFileTypes.ConvertAll(item => new RMDiscoveryAOSPFileExtension
                    {
                        Name = item,
                    });
                    await _fileTypeDao.AddOrUpdateAsync(_o365TenantId, needAddFileTypes.ToArray());
                    _logger.Info($"Successful add file types to db.");
                    foreach (var needAddFileType in needAddFileTypes)
                    {
                        _fileTypes[needAddFileType.Name] = needAddFileType.Id;
                    }
                }
            }
        }

        public int GetId(string fileType)
        {
            var convertedCaseFileType = string.IsNullOrWhiteSpace(fileType) ? "RM_FA_FileType_Empty" : fileType;
            return _fileTypes[convertedCaseFileType];
        }

        public int GetIdAndAddOrUpdate(string fileType)
        {
            AddOrUpdateAsync(fileType).GetAwaiter().GetResult();
            return GetId(fileType);
        }

    }
}
