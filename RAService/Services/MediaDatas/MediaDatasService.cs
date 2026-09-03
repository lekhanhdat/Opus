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
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.RMWeb.MediaDatas;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Service.Services.ManualApproval;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.MediaDatas
{
    [Audit]
    public class MediaDatasService : IMediaDatasService
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(RMManualApprovalService));
        private IMediaDataDao MediaDao => PlatformWindsorManager.GetService<IMediaDataDao>();

        public Task UpdateOrInsertMediaDataAsync(string key, string value)
        {
            return MediaDao.UpdateOrInsertMediaDataAsync(key, value);
        }

        public Task<List<MediaDataDto>> GetMediaDatasAsync(string key)
        {
            return MediaDao.GetMediaDatasAsync(key);
        }
    }
}
