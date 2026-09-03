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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Explorer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGlobalSearch.Actions
{
    public class DeclareAction : IGlobalSearchAction
    {
        private AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(DeclareAction));
        private IExplorerService mExplorerService;
        public IExplorerService ExplorerService
        {
            get
            {
                if (mExplorerService == null)
                {
                    mExplorerService = (IExplorerService)PlatformWindsorManager.GetService(typeof(IExplorerService));
                }
                return mExplorerService;
            }
        }
        private GlobalSearchAction mAction;
        private int mFailedCount = 0;
        private int mSuccessCount = 0;
        public DeclareAction(GlobalSearchAction action)
        {
            mAction = action;
        }
        //only support sp
        public async Task DoActionAsync(List<BaseRecordDto> records, SourceFlag flag, object actionExtension, string jobId, bool isJob)
        {
            logger.Info("Start process declare action.");
            if (flag == SourceFlag.SharePoint || flag == SourceFlag.OneDrive)
            {
                try
                {
                    var declaredBy = actionExtension.ToString();
                    if (mAction == GlobalSearchAction.DeclareRecords || mAction == GlobalSearchAction.AddRecordLabel)
                    {
                        int failedCount = await ExplorerService.DeclareAsRecordForGlobalSearchAsync(records.Select(r => r.Id).ToList(), jobId, declaredBy, isJob);
                        mFailedCount += failedCount;
                        mSuccessCount += (records.Count - failedCount);
                    }
                    else if (mAction == GlobalSearchAction.UnDeclareRecords || mAction == GlobalSearchAction.RemoveRecordLabel)
                    {
                        int failedCount = await ExplorerService.UndeclareAsRecordForGlobalSearchAsync(records.Select(r => r.Id).ToList(), jobId, declaredBy, isJob);
                        mFailedCount += failedCount;
                        mSuccessCount += (records.Count - failedCount);
                    }
                }
                catch (Exception e)
                {
                    mFailedCount++;
                    logger.Error($"An error occurred while doing DeclareAction. Error:{e.ToString()}");
                }
            }
            else if (flag == SourceFlag.Teams)
            {
                try
                {
                    var declaredBy = actionExtension.ToString();
                    if (mAction == GlobalSearchAction.DeclareRecords || mAction == GlobalSearchAction.AddRecordLabel)
                    {
                        int failedCount = await ExplorerService.DeclareTeamsRecordForGlobalSearchAsync(records.Select(r => r.Id).ToList(), jobId, declaredBy, isJob);
                        mFailedCount += failedCount;
                        mSuccessCount += (records.Count - failedCount);
                    }
                    else if (mAction == GlobalSearchAction.UnDeclareRecords || mAction == GlobalSearchAction.RemoveRecordLabel)
                    {
                        int failedCount = await ExplorerService.UndeclareTeamsRecordForGlobalSearchAsync(records.Select(r => r.Id).ToList(), jobId, declaredBy, isJob);
                        mFailedCount += failedCount;
                        mSuccessCount += (records.Count - failedCount);
                    }
                }
                catch (Exception e)
                {
                    mFailedCount++;
                    logger.Error($"An error occurred while doing DeclareAction. Error:{e.ToString()}");
                }
            }
            else
            {
                throw new Exception("Invalid Data Source.");
            }
            logger.Info("Process declare action finished.");
        }

        public int GetSuccessCount()
        {
            return mSuccessCount;
        }

        public int GetFailedCount()
        {
            return mFailedCount; ;
        }
    }
}
