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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface ICollectionDataDao
    {
       
     
        List<RMSiteCollectionSize> GetBoardCollectionTop10Data(int beginIndex);

        List<RMTermUsage> GetBoardTermUsageTop10Data(int beginIndex);

        List<RMDataOfDay> GetBoardCreatedRecords(int mstartIndex, int dstartIndex, int archiveIndex);
        List<RMDataOfDay> GetBoardDestroyedRecords(int beginIndex);
        long GetRecordsCount(bool destroyed);

        List<RMManagedRecord> GetRecordByNodeType(Guid scopeId, Guid spObjId, int nodeType);

        #region scope 
        //void AddSiteScope(RMScope scope);
        //long GetScopeCollectionTime(Guid scopeId);
        //List<RMScope> GetExistScopeInfo();

        //bool IsScopeInfoExist(Guid scopeId);
        #endregion

        #region boardIndex

        RMBoardIndex GetBoardIndex(SourceFlag sFlag);
        void UpdateBoardIndex(SourceFlag sFlag);
        #endregion
    }
}
