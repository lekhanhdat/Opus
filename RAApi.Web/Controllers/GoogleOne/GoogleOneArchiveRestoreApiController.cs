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
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.ArchiverRestore;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.I18N.Core;
using Cloud.sdk.Data.Opus.GoogleOne.Common;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Controllers.GoogleOne
{
    [Route("api/googleone/archiverestore")]
    public class GoogleOneArchiveRestoreApiController : GoogleOneApiBaseController
    {
        private readonly IRALogger _logger = new RALogger(typeof(GoogleOneArchiveRestoreApiController));
        private IRestoreSearchService _restoreSearchService => PlatformWindsorManager.GetService<IRestoreSearchService>();
        private IKeyValueService _keyValueService => PlatformWindsorManager.GetService<IKeyValueService>();

        [HttpGet("getgoogledriveInfo")]
        public async Task<string> GetGoogleDrivesInfo()
        {
            using var performance = new PerformanceScope("GoogleOneArchiveRestoreApiController.GetGoogleDriveInfo");
            try
            {
                return JsonConvert.SerializeObject(await _restoreSearchService.GetAllGoogleDriveNodesAsync());
            }
            catch (Exception ex)
            {
                _logger.Info($"get google drive info error,msg:{ex.Message}");
                return I18NEntity.GetString("RM_RESTORE_PUB_ArchivedDataNotFound");
            }
        }
        [HttpPost("getallsearchresult")]
        public async Task<String> GetAllSearchResult([FromBody] ArchiverRestoreResult searchContract)
        {
            using var performance = new PerformanceScope("GoogleOneArchiveRestoreApiController.GetAllSearchResult");

            return JsonConvert.SerializeObject(await _restoreSearchService.GetDriveSearchTreeResultAsync(searchContract, true, true));
        }
        [HttpPost("exportsearchresult")]
        public String ExportSearchResult([FromBody] ArchiverRestoreResult info)
        {
            return JsonConvert.SerializeObject(_restoreSearchService.ExportSearchResult(info));
        }

        [HttpPost("saverestoresettingandrun")]
        public String SaveRestoreSettingAndRun([FromBody] RestoreInfo info)
        {
            info.IsEndUserJob = false;
            GCommon.Contract.StorageOptimization.Object.RestoreType tempRestoreType;

            tempRestoreType = GCommon.Contract.StorageOptimization.Object.RestoreType.InPlace;

            try
            {
                foreach (var tempInfo in BuildGDriveRestoreInfos(info))
                {
                    var tempResult = _restoreSearchService.SaveAndRunDriveRestoreJob(info, tempRestoreType);
                    if (tempResult.MessageType == RAMessageType.Failed)
                    {
                        return JsonConvert.SerializeObject(tempResult);
                    }
                }
                _logger.Info($"finish run restore job");
            }
            catch (Exception e)
            {
                _logger.Error($"something went wrong when save restore setting,error :{e}");
                return JsonConvert.SerializeObject(new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, ErrorMessage = I18NEntity.GetString("RM_RS_SaveRestoreSettingError") });
            }
            return JsonConvert.SerializeObject(new RAReturnMessage());
        }
        [HttpGet("enablesoftdelete")]
        public Boolean EnableSoftDelete()
        {
            return _keyValueService.IsEnableSoftDeleteSetting();
        }
        private List<RestoreInfo> BuildGDriveRestoreInfos(RestoreInfo info)
        {
            info.DataSource = (int)RestoreDataSource.GoogleDrive;
            List<RestoreInfo> restoreInfos = new List<RestoreInfo>();
            Dictionary<string, List<ArchiverRestoreSerchResult>> driveWithObject = new Dictionary<string, List<ArchiverRestoreSerchResult>>();
            _logger.Info("start generate restore setting nodes");
            var needRestoreObjects = info.NodeObjects;
            foreach (ArchiverRestoreSerchResult obj in needRestoreObjects)
            {
                if (driveWithObject.ContainsKey(obj.SitePath))
                {
                    driveWithObject[obj.SitePath].Add(obj);
                }
                else
                {
                    driveWithObject.Add(obj.SitePath, new List<ArchiverRestoreSerchResult>());
                    driveWithObject[obj.SitePath].Add(obj);
                    _logger.Info($"driveWithObject not containe key:{obj.SitePath},add it");
                }
            }
            _logger.Info($"finish ganerate driveWithObject,count:{driveWithObject.Count}");
            foreach (var tempKeyValue in driveWithObject)
            {
                _logger.Info($"this drive with object info is :key:{tempKeyValue.Key},value count:{tempKeyValue.Value?.Count}");
                var tempRestoreInfo = Clone(info);
                tempRestoreInfo.NodeObjects.Clear();
                tempRestoreInfo.NodeObjects = tempKeyValue.Value;
                restoreInfos.Add(tempRestoreInfo);
            }
            return restoreInfos;
        }
        private RestoreInfo Clone(RestoreInfo retoreInfo)
        {
            var serialized = JsonConvert.SerializeObject(retoreInfo);
            return JsonConvert.DeserializeObject<RestoreInfo>(serialized);
        }
    }
}
