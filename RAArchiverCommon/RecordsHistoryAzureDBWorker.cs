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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.RMWeb;

namespace AvePoint.StorageOptimization.Schedule.Common.AzureTable
{
    public class RecordsHistoryAzureDBWorker : IDisposable
    {
        public static readonly string Separator = "|I18NSplit|";

        private IRecordsHistoryService mRecordsHistoryService = null;
        public IRecordsHistoryService RecordsHistoryService
        {
            get
            {
                if (mRecordsHistoryService == null)
                {
                    mRecordsHistoryService = (IRecordsHistoryService)PlatformWindsorManager.GetService(typeof(IRecordsHistoryService));
                }
                return mRecordsHistoryService;
            }
        }

        public RecordsHistoryAzureDBWorker()
        {
        }

        public void AddRecordsHistory(Guid sourceRecordsId, Guid destinationRecordsId, string sourceUrl, string destUrl)
        {
            RecordsHistoryService.AddRecordsHistory(new List<Guid> { sourceRecordsId }, $"RM_Explorer_RecordHistorySuccessfulInformation{Separator}{sourceUrl}{Separator}{destUrl}");
            RecordsHistoryService.CloneMoveHistoryRecords(sourceRecordsId, destinationRecordsId);
        }

        public void Dispose()
        {
        }
    }    
}
