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
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IExplorerDao
    {
     
        ///// <summary>
        ///// store data for deleted and archvied records
        ///// </summary>
        ///// <param name="rec"></param>
        //void AddDataToDestroyed(RMDestroyedRecord rec); 

        void AddRange(List<RMManagedRecord> list);
        ///// <summary>
        ///// delete record
        ///// </summary>
        ///// <param name="destroyed"></param>
        ///// <param name="scopeId"></param>
        ///// <param name="dirPath"></param>
        //void DeleteRecord(bool destroyed, Guid scopeId, string dirPath);
        /// <summary>
        /// get data by paging
        /// </summary>
        /// <param name="destroyed"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecord"></param>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        int QueryDataGetTotal(bool isArchived, string keyWord, Expression<Func<RMBaseRecord, bool>> whereLambda = null);
        List<RMBaseRecord> QueryDataWithoutTotal(bool isArchived, string keyWord, int pageIndex, int pageSize, out bool hasNext, Expression<Func<RMBaseRecord, bool>> whereLambda = null);

        //List<RMBaseRecord> QueryDataById(int pageIndex, int pageSize, out int totalRecord, string holdId);

        List<RMBaseRecord> GetRecordByAlliance(List<int> recordIds);

        /// <summary>
        /// change term
        /// to do (change rule setting)
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="termId"></param>
        void ChangeTerm(List<int> ids, Guid termId);

        void UpdateHoldSetting(List<int> ids, bool holdStatus, HoldSettingDto holdDto);

        void UpdateHoldSetting(List<int> recordIds, List<RMRecordAlliance> Records);

        void CancelHoldByRecords(List<int> recordsIds);

        /// <summary>
        /// declare as record
        /// </summary>
        /// <param name="ids"></param>
        void DeclareAsRecord(List<int> ids, bool declareStatus, string userName);


        Dictionary<Guid, int> TestUnion();


        void AddReocrdHistory(List<int> id, RecordHistoryXml xmlDto);


        List<RMRecordAlliance> GetSettingHoldByRecordIds(List<int> ids);

        void UpdateCollectionTime(Guid id, long timeTicks);
        List<RMBaseRecord> GetRelatedRecords(Expression<Func<RMBaseRecord, bool>> whereLambda = null);
        /// <summary>
        /// update all expired hold record and hold setting
        /// </summary>
        /// <returns></returns>
        int UpdateExpiredHold(List<int> ids);
        List<int> GetExpiredHold();
        List<RMRecordAlliance> GetRecordAllianceById(List<int> recordIds);
        RMBaseRecord GetRecordByUniqueId(Guid scopeId, Guid itemUniqueId);

        //RMBaseRecord GetFSRootNode();
        //List<RMBaseRecord> GetFSChildNodes(Guid parentId, int fsType);
        //RMManagedRecord GetFSConnGroupNode(Guid nodeId);


    }
}
