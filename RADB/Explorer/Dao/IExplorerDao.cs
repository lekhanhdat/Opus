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
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.Tree;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.Tree.Base;

namespace AvePoint.RA.DB.Explorer.Dao
{
    public interface IExplorerDao
    {
        /// <summary>
        /// Add include/exclude path to index policy
        /// </summary>
        /// <param name="pathList"></param>
        /// <param name="includedPath">indicate if include or exclude path</param>
        void AddPath2IndexPolicy(List<string> pathList, bool includedPath = true);
        Record ReadById(Guid scopeId, Guid id);
        void Upsert(Record record);
        /// <summary>
        /// bulk upsert records. will return the records which are failed to upsert.
        /// </summary>
        /// <param name="recordList"></param>
        /// <returns>the failed record list with exception</returns>
        List<(Record, Exception)> BulkUpsert(List<Record> recordList);
        void Replace(Record record);
        void Add(Record record);
        List<Guid> BatchUpdate(List<Record> records, int bufferSize);
        void Delete(Record record);
        Task DeleteAsync(IEnumerable<Record> records);
        //[Obsolete]
        void Delete(int createDate, Guid id);
        /// <summary>
        /// 慎用,删除Tenant下的所有Explorer数据,目前为COP api调用此方法
        /// </summary>
        bool DeleteExplorerData(string tenantId);
        void BatchAddRecords(List<Record> records, bool forceUpdate = false);

        int UpdateAll(Expression<Func<Record, bool>> predicate, Action<Record> operation);

        /// <summary>
        /// Note: if you know the scope id and id, please call ReadById
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="convertCustomColumn2Metainfo"></param>
        /// <returns></returns>
        IEnumerable<Record> QueryAll(Expression<Func<Record, bool>> predicate, bool convertCustomColumn2Metainfo = true);


        Tuple<IEnumerable<Record>, string> QueryPageBySql(Microsoft.Azure.Cosmos.QueryDefinition queryDefinition, int pageCount = 15, string continuation = "");
        /// <summary>
        /// 取出满足条件的第一个记录.
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        Record GetFirstOrDefault(Expression<Func<Record, bool>> whereLambda);

        Record GetFirstOrDefault(Expression<Func<Record, bool>> predicate, Expression<Func<Record, dynamic>> orderLambda);

        Record GetFirstOrDefaultByOrderDesc(Expression<Func<Record, bool>> predicate, Expression<Func<Record, dynamic>> orderLambda);

        /// <summary>
        /// 查询Count
        /// </summary>
        /// <param name="sql">SELECT VALUE COUNT(1) FROM c where c.xxx</param>
        /// <returns></returns>
        int QueryCount(string sql, Dictionary<string, object> parameters = null);

        Dictionary<string, int> QueryRelatedTermCount(string sql);

        Dictionary<string, int> QuerySiteCollectionUsageCount(string sql);

        List<(List<int> Reviewers, int Count)> QueryReviewerWaitingApprovalItemCount(string sql);
        List<(string GControlCurrentApproverId, int Count)> QueryReviewerWaitingApprovalItemCountForGControl(string sql);

        List<(string date, int count)> QueryDashboardDataUsageOfDate(string sql);

        /// <summary>
        /// 判断是否存在满足条件的记录.
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        bool Exist(Expression<Func<Record, bool>> whereLambda);

        List<T> GetFilterList<T>(Expression<Func<Record, T>> selectLambda, Expression<Func<Record, bool>> whereLambda);
        /// <summary>
        /// 分页查询.
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <param name="pageCount">每页条数</param>
        /// <param name="continuation">continuation Token</param>
        /// <returns>Tuple.Item1=Result; Tuple.Item2=ContinuationToken</returns>
        Tuple<IEnumerable<Record>, string> QueryByPage(Expression<Func<Record, bool>> predicate, int pageCount = 15, string continuation = "", bool convertCustomColumn2Metainfo = true);

        Tuple<IEnumerable<Record>, string> QueryByPage(Expression<Func<Record, bool>> predicate, Expression<Func<Record, dynamic>> orderBy = null, bool descending = false, int pageCount = 15, string continuation = "", bool convertCustomColumn2Metainfo = true);
        /// <summary>
        /// 根据指定的条件，取出满足条件的一页数据，按照Record中的TOrder字段来排序,输出为TOut类型的数据
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <typeparam name="TOrder"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="selectLambda"></param>
        /// <param name="orderByLambda"></param>
        /// <param name="orderAscending">最好和索引中顺序一样，如果索引是降序，那么此处需要设置为false，否则为true</param>
        /// <param name="pageCount"></param>
        /// <param name="continuation"></param>
        /// <returns></returns>
        //Tuple<IEnumerable<TOut>, string> QueryByPage<TOut, TOrder>(Expression<Func<Record, bool>> predicate, Expression<Func<Record, TOut>> selector, Expression<Func<Record, TOrder>> orderByLambda, bool orderAscending = true, int pageCount = 15, string continuation = "");
        List<Guid> UpdateExpiredHeldRecords();

        /// <summary>
        /// partial update record
        /// </summary>
        /// <param name="patchRecord"></param>
        /// <param name="byProperties">if true, will patch with the properties, otherwise, patch with FieldName collection</param>
        /// <returns></returns>
        List<Record> GetRecordsByTerms(Guid iD, List<Guid> changeTermIds, long ticks);
        List<Record> GetConnectorRecordsByTerms(int sourceFlag, List<Guid> changeTermIds);
        IEnumerable<Record> GetDoesNotMatchRuleConnectorItems(int sourceFlag);
        List<Record> GetRecordsByTermsByPage(Guid iD, List<Guid> changeTermIds, long ticks, long sortTicks, int pageSize);
        List<Record> GetEXORecordsByTerms(Guid iD, List<Guid> changeTermIds, long ticks, string emailAddress);
        //int QueryDataGetTotal(int status, string keyWord, Expression<Func<Record, bool>> whereLambda = null);
        //int QueryCountBySql(PhysicalExplorerQueryDto dto, SecurityTermPermissionDto termPermDto, bool withoutPhysicalRecord = false);
        Tuple<IEnumerable<Record>, string> QueryDataWithoutTotal(string continuation, int pageSize, out bool hasNext, Expression<Func<Record, bool>> whereLambda = null);

        Task<(Tuple<IEnumerable<Record>, string>, bool)> QueryDataBySqlWithoutTotalAsync(ExplorerQueryDto dto, bool isGlobalSearch, string continuation, int pageSize, SecurityTermPermissionDto termPermDto = null);

        //Tuple<IEnumerable<Record>, string> QueryDataBySqlWithoutTotal(PhysicalExplorerQueryDto dto, string continuation, int pageSize, out bool hasNext, SecurityTermPermissionDto termPermDto, bool withoutPhysicalRecord = false);
        Tuple<IEnumerable<Record>, string> QueryPageBySqlForBrowse(RMPhysicalExplorerNode currentRecord, List<int> permissionIds, bool hasScopePermission, int pageCount = 15, string continuation = "", SecurityTermPermissionDto termPermDto = null);
        //Tuple<IEnumerable<Record>, string> SearchRecords(string pageIndex, int pageSize, string value, List<Guid> exceptIds, List<int> permissions, SourceFlag sourceFlag, out bool hasNext, bool isEnduser = false);
        Tuple<IEnumerable<Record>, string> SearchRecordsV2(ExplorerQueryV2Dto dto, SqlQuerySpecBuilder sqlQuerySpecBuilder = null);
        /// <summary>
        /// returns the total number
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="sqlQuerySpecBuilder"></param>
        /// <returns></returns>
        int QueryCount(ExplorerQueryV2Dto dto, SqlQuerySpecBuilder sqlQuerySpecBuilder = null);

        #region advanced search
        Tuple<IEnumerable<Record>, string> SearchRecordsV3(ExplorerQueryV3Dto dto, ExplorerFilterOptionV2 builtinFilterOption, SqlQuerySpecBuilder sqlQuerySpecBuilder = null, bool suggestSearch = false);
        /// <summary>
        /// returns the total number
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="sqlQuerySpecBuilder"></param>
        /// <returns></returns>
        int QueryCountV3(ExplorerQueryV3Dto dto, ExplorerFilterOptionV2 builtinFilterOption, SqlQuerySpecBuilder sqlQuerySpecBuilder = null, bool suggestSearch = false);
        #endregion

        int QueryCount(Expression<Func<Record, bool>> whereLambda);
        //void AddReocrdHistory(List<Guid> id, RecordHistoryXml xmlDto);
        List<Record> GetRecordByIds(List<Guid> ids);
        List<Record> GetRecordByRecordsIds(List<string> recordsId);
        List<Record> GetActiveRecordsByIds(List<Guid> ids);
        List<Record> GetRecordsByNodeIds(List<Guid> ids);
        List<Record> GetHoldRecordsByIds(List<Guid> ids);
        List<Record> GetPhysicalRecordByRecordIds(List<string> uniqueIds);
        int UpdateRecordOwner(Guid scopeId, Guid nodeId, string owners);
        int UpdateRecordOwnerForFS(Guid nodeId, string owners);
        bool AddOrUpdateRecord(Record rec, bool forceUpdate, RMRule tempRule = null);
        bool AddOrUpdateRecordWithKeepManual(Record rec, bool forceUpdate, RMRule tempRule = null, bool isKeepManualColumn = true);

        /// <summary>
        /// Check if need to update the record to Cosmos DB.
        /// </summary>
        /// <param name="rec"></param>
        /// <param name="forceUpdate">if true, will update anyway</param>
        /// <returns></returns>
        bool NeedUpdateRecord(Record rec, bool forceUpdate, RMRule tempRule = null);
        bool NeedUpdateRecord(Record rec, bool forceUpdate, Record dbRecord, RMRule tempRule = null);
        bool NeedUpdateMLManualRecord(Record rec, bool forceUpdate, Record dbRec, RMRule tempRule = null);
        void UpdateRecordState(Record rec, int status, List<Guid> subFolderIds = null);
        List<Guid> GetAllSubFolderUnderFolder(Record rec);
        //Only used for EXO 
        void UpdateRecordState(Guid scopeId, Guid id, int status);
        Record ReadSPRecordById(Guid scopeId, Guid webId, Guid listId, int itemRowId);
        bool CheckHasData();
        //void WaitForIndexTransformationToComplete();
        #region Physicla Assosicated
        List<Record> GetWaitingApproveItemForPhysical();
        void UpdateItemToExportStatus(Guid id);
        void UpdateRecordOwnerForPhysical(Guid id, string owners);
        Record GetPhysicalRecordById(Guid id);

        /// <summary>
        /// get the raw data directly from Cosmos without any modification, e.g., will not change the CustomColumnDic field 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Record GetPhysicalRawDataById(Guid id);
        /// <summary>
        /// physical import check
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        Record GetPhysicalRecordByRecordsId(string uniqueId);
        Record GetPhysicalRecordByRecordsIdAndTemId(string uniqueId, int temId);
        void UpdateApproveStatus(Guid id, Contract.SOApproveDBStatus status);
        bool AddPhysicalRecord(Record rec);
        bool UpdatePhysicalRecord(Record rec, bool forceUpdate, bool isModifyPermissionId = false, bool isUpdateManualProperties = false);
        #endregion
        List<Record> GetRecordsByIdPermssions(List<int> scopePermissions, List<Guid> recordIds);
        Tuple<IEnumerable<Record>, string> GetRecordsByContainer(Guid scopeId, string containerId, string continuation, int pageSize);
        Tuple<IEnumerable<Record>, string> GetRecordsByContainerAndNodeType(Guid scopeId, string containerId, List<int> nodeTypes, string continuation, string url, int pageSize);
        Tuple<IEnumerable<Record>, string> SearchPhysicalBoxOrFolderByName(string searchKey, string continuation, int pageSize, bool isGlobalSearch, bool isSearchFolder, string locationId);
        Tuple<IEnumerable<Record>, string> SearchFileSystemBySearchKey(string searchKey, string continuation, int pageSize);
        Tuple<IEnumerable<Record>, string> SearchFileSystemBySearchKeyAndConnectionIds(string searchKey, IEnumerable<Guid> connectionIds, string continuation, int pageSize);

        #region fs
        bool AddFileSystemRecord(Record rec);
        Record GetFSRecordById(Guid id);
        Record GetGoogleDriveRecordById(Guid id);
        Record GetFSRecord(Guid ScopeId, Guid Id);
        List<Record> GetFSChildNodes(Guid parentId, int fsType);
        List<Record> GetFSConnectionUnderGroup(Guid connectionGroupId, int level);
        string GetFSConnectionIdByItemId(Guid itemId);
        Record GetFSRootNode();
        bool UpdateFileSystemRecord(Record rec, bool forceUpdate);
        void UpdateFileSystemFolderForSync(Record rec);
        List<Record> GetExplorerDataByFolder(string folderId, string scopeId, long sortTicks, int pageSize);
        bool HasFileMatchTerm(string path, string scopeId, List<Guid> classCodeIds);
        List<Record> GetExplorerDataByFolderAndEndTime(string folderId, string scopeId, long sortTicks, int pageSize);
        List<Record> GetExplorerDataByNodeIds(IEnumerable<Guid> nodeIds, string scopeId, long sortTicks);
        List<Record> GetExplorerDataByNodeIdsAndEndTime(IEnumerable<Guid> nodeIds, string scopeId, long sortTicks);
        List<Record> GetDBRecordsByClassCodeAndFilterByEndTime(IEnumerable<Guid> nodeIds, IEnumerable<Guid> classCodeIds, string scopeId, long sortTicks);
        int UpdateFSDeleteRecord(Guid id, Guid scopeId, int recordStatus);
        Tuple<IEnumerable<Record>, string> SearchByFullPath(List<string> fullPaths, int nodeType, int sourceFlag, string continuation, int pageSize);
        long GetTotalSizeByNodeTypeAndDirPaths(int nodeType, int sourceFlag, int status, List<string> dirPaths);
        #endregion

        #region sp on premise
        Record GetByItemRowId(Guid scopeId, Guid listId, int itemRowId);
        List<Record> GetOnPremiseSPExplorerDataByListId(string listId, string scopeId, long sortTicks, int pageSize);
        #endregion

        List<Record> GetRecordsBySql(List<Guid> recordIds);
        List<Record> GetRecordbyHoldId(string holdId);
        List<Record> GetRecordbyHoldIds(List<string> holdIds);
        bool CanPhysicalFileMove(Guid fileId, Guid srcParentId, Guid destParentId);
        bool IsRecordsHold(List<Guid> ids, long ticks);
        Dictionary<string, CustomColumn> GetUpdateColumns(Dictionary<string, string> metaInfoDic);

        Tuple<IEnumerable<Record>, string> QueryDueRecordsByPage(SearchFilterParam param);
        Tuple<IEnumerable<Record>, string> QueryPageBySqlForTermBrowse(RMNodeType nodeType, Guid termId, List<int> permissionIds, bool hasScopePermission, int pageCount = 15, string continuation = "");
        Tuple<IEnumerable<Record>, string> QueryPageBySqlForTermBrowse(RMNodeType nodeType, Guid termId, List<int> permissionIds, bool hasScopePermission, List<Guid> bottomLocationIds, int pageCount = 15, string continuation = "");
        void AddArchivedRelatedColumn(Guid scopeId, Guid id, string pathMd5, string jobId, string index);
        void UpdateRecordStatusAndDestroyedTime(Guid scopeId, Guid pathMD5, int recordStatus);
        void UpdateRecordStatusAndDestroyedTime4Manual(Guid scopeId, Guid pathMD5, int recordStatus);
        void BatchUpdateRecordStatusAndDestroyedTime4Manual(List<Tuple<Guid, Guid>> recordIdentities, int recordStatus);
        void UpdateRecordStatusToCancel(Guid scopeId, Guid pathMD5, int recordStatus);
        Task<List<string>> DistinctQueryAsync(Expression<Func<Record, string>> selectLambda, Expression<Func<Record, bool>> whereLambda);
        Record GetBoxRecordById(Guid id);
        Record GetPhysicalRecordByBarcode(string barcode);
        void DeleteGoogleItem(int createDate, string itemId); // temp, todo -> will change when already defined an id field for google
        public Task<List<Record>> GetGoogleRecordsByFolderIdAsync(Guid scopeId, List<Guid> folderIds);
        public Task<List<Record>> GetAllGoogleFilesByBatchBFSAsync(Guid scopeId, Guid folderId);
        List<Record> GetRecordBoxsByBoxIds(List<Guid> boxIds);
        List<Record> GetChildRecordsByBoxIds(List<Guid> boxIds);

        #region FS JPMC
        List<Record> QueryJPMCRecords(int sourceFlag, string aveSiteId, List<Guid> ids);
        List<Record> QueryJPMCRecords(int sourceFlag, string aveSiteId, List<string> ids);
        bool HasJPMCConnectionRecord(int sourceFlag, string aveSiteId);
        #endregion

        #region Maestro AI
        int ResetMARecordsForRemovedMLTerms(List<Guid> predictTermIds);
        #endregion

        Task<Dictionary<string, int>> GetRecordCountByHoldIdAndHoldReleaseAsync(List<RMHold> holds);
        Record FindRecordBySiteAndPath(string aveSiteId, string dirPath, string leafName, bool isFileSystem);
    }
}
