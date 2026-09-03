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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.RAPhysical.ConfiguePermission.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGlobalSearch.Actions
{
    public class AccessControlAction : IGlobalSearchAction
    {
        private IRALogger logger = RALogger.GetInstance(typeof(AccessControlAction));
        private int mFailedCount = 0;
        private int mSuccessCount = 0;
      
        private IPhysicalPermissionProccessor mPhysicalPermissionProccessor;
        public IPhysicalPermissionProccessor PhysicalPermissionProccessor
        {
            get
            {
                if (mPhysicalPermissionProccessor == null)
                {
                    mPhysicalPermissionProccessor = (IPhysicalPermissionProccessor)PlatformWindsorManager.GetService(typeof(IPhysicalPermissionProccessor));
                }
                return mPhysicalPermissionProccessor;
            }
        }

        public async Task DoActionAsync(List<BaseRecordDto> records, SourceFlag flag, object actionExtension, string jobId, bool isJob)
        {
            logger.Info("Start process access control action.");
            if (flag == SourceFlag.Physical)
            {
                try
                {
                    ScopePermissionJobContextDto dto = SerializerHelper.DeserializeByDataContractSerializer<ScopePermissionJobContextDto>(actionExtension.ToString());
                    dto.GSJobContextDto.NodeIds = records.Select(r => r.Id).ToList();
                    await PhysicalPermissionProccessor.ProcessByGlobalSearch(ConvertDtoToOption(dto, jobId));
                    if (PhysicalPermissionProccessor.HasSuccessNode)
                    {
                        mSuccessCount++;
                    }
                    if (PhysicalPermissionProccessor.HasErrorNode)
                    {
                        mFailedCount++;
                    }
                }
                catch (Exception e)
                {
                    mFailedCount++;
                    logger.Error($"An error occurred while doing AccessControlAction, Error:{e.ToString()}");
                }
            }
            logger.Info("Process access control action finished.");
        }

        public int GetFailedCount()
        {
            return mFailedCount;
        }

        public int GetSuccessCount()
        {
            return mSuccessCount;
        }

        private PermissionOption ConvertDtoToOption(ScopePermissionJobContextDto PermissionOption, string jobId)
        {
            return new PermissionOption()
            {
                Scopes = PermissionOption.Scopes,
                GSJobContext = PermissionOption.GSJobContextDto,
                JobId = jobId
            };
        }
    }
}
