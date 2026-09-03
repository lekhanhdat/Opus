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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.DB.Dao;
using Microsoft.AspNetCore.Mvc;
using System;

namespace AvePoint.RA.Api.Web.Public.Controllers
{
    [Route("api/FileSystemMyhub/[action]")]
    public class FileSystemMyhubApiController
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(FileSystemMyhubApiController));
        private IRMKeyValueDao _RMKeyValueDao;
        public IRMKeyValueDao RMKeyValueDao => (IRMKeyValueDao)PlatformWindsorManager.GetService(ref _RMKeyValueDao);
        [HttpGet]
        public bool IsSupportLocatedFSExplorerFeature()
        {
            try
            {
                var enabledJPMCFeature = RMKeyValueDao.TryGetBoolValue(KeyNameCollection.EnableJPMCFileSystemFeature, out var enabled) && enabled;
                return enabledJPMCFeature;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to get key value for {KeyNameCollection.EnableJPMCFileSystemFeature}, exception: {ex}");
                return false;
            }
        }
    }
}
