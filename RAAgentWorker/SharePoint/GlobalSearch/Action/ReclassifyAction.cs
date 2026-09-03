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
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Global.JobMessage;
using AvePoint.RA.SharePoint.RMExplorer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.GlobalSearch.Action
{
    public class ReclassifyAction : IGlobalSearchAction
    {
        private AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private int mFailedCount = 0;
        private int mSuccessCount = 0;
        public void DoAction(List<RecordDto> records, object actionExtension, string jobId)
        {
            ChangeTermOption changeTermDto = SerializerHelper.DeserializeByDataContractSerializer<ChangeTermOption>(actionExtension.ToString());
            int failedCount = 0;
            try
            {
                changeTermDto.SourceSPOnPremRecordIds = records.Select(r => r.Id).ToList();
                RMExplorerUtility explorerUtility = new RMExplorerUtility(true);
                explorerUtility.ChangeAllTerms(changeTermDto, jobId, false);
                failedCount = explorerUtility.FailedCount;
            }
            catch (Exception e)
            {
                logger.Warn("Update terms error {0}", e.ToString());
                failedCount = changeTermDto.SourceSPOnPremRecordIds.Count;
            }
            mFailedCount += failedCount;
            mSuccessCount += (records.Count - failedCount);
        }

        public int GetFailedCount()
        {
            return mFailedCount;
        }

        public int GetSuccessCount()
        {
            return mSuccessCount;
        }
    }
}
