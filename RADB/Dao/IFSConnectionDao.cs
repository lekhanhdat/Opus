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
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.MyHub.Model.QueryRequest.Views;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IFSConnectionDao : IBaseDao<FSConnection>
    {
        List<FSConnection> GetAllConnections(bool onlyNoGroup = false);
        List<FSConnection> GetAllNoGroupConnections(GetConnectionListParam param, out int totalCount);
        List<FSConnection> GetAllConnectionsByGroupId(Guid groupId);
        Task<List<FSConnection>> GetAllConnectionsByGroupIdAsync(Guid groupId);

        FSConnection GetConnectionById(Guid connectionId);

        List<FSConnection> GetConnectionByIds(List<Guid> connectionIds);
        Task<List<FSConnection>> GetConnectionBySearchKey(string searchKey);
        Task<List<FSConnection>> GetConnectionBySearchKeyAndGroupId(string searchKey, IEnumerable<Guid> groupIds);
        Task<IEnumerable<Guid>> GetAllConnectionIdsByGroupIdsAsync(IEnumerable<Guid> groupIds);
        FSConnection GetConnectionByName(string name);

        FSConnection GetConnectionByUNCPath(string uncPath);

        Task<bool> SaveConnectoinAsync(FSConnection connection);

        void DeleteConnectoin(Guid connectionId);

        Task<bool> UpdateConnectoinGroupIdAsync(Guid connectionId, Guid groupId);

        void UpdateConnectionsGroupId(Guid groupId, List<Guid> connectionIds);

        bool CheckConnectoinUNCPathExist(Guid connectionId, string uncPath);

        bool CheckConnectionIdExist(string JPMCConnectionId);
        bool CheckAllConnectionIdsExist(List<Guid> connectionIds);

        bool CheckUpdateConnectionIdExist(string JPMCConnectionId, Guid Id);

        List<FSConnection> QueryConnectionsPager(Expression<Func<FSConnection, bool>> whereLambda, GetConnectionListParam param, out int totalCount);

        List<FSConnection> QueryConnectionsPagerForOtherDCs(Expression<Func<FSConnection, bool>> whereLambda, GetConnectionListParam param, out int totalCount, string DCInternalName);

        FSConnection GetParentConnectionInfo(string uncPath);

        FSConnection GetParentConnectionInfoForImport(string uncPath);

        Task<bool> UpdateValidateResultAsync(List<Guid> connectionIds, Dictionary<Guid, string> uncPaths, Dictionary<Guid, int> pathTypes);
        Task<IEnumerable<FSConnection>> LoadByPager(int pageIndex, int pageSize);
        Task<long> MultiGeoInsertFSConnectionTableAsync(IEnumerable<FSConnection> fSConnections);
        Task<long> MultiGeoDeleteAllFSConnectionAsync();
        Task<Guid> GetConnectionGroupIdByNameAsync(string groupName);
        Task<Guid> GetConnectionGroupIdByConnectionIdAsync(Guid connectionId);

        #region JPMC
        Task<bool> UpdateLastSyncTimeAsync(Guid connectionId, long lastSyncTime);
        #endregion

        #region myhub
        Task<(List<FSConnection> Items, bool HasMore, int Count)> QueryConnectionPaginationAsync(List<int> userIntIds, RMMyhubDriveQueryInfo queryInfo);

        Task<bool> UpdateConnectoinIsPauseAsync(List<Guid> connectionIds, int isPause);
        bool AnyConnectionExistsOutsideGroup(List<Guid> connectionIds, Guid groupId, bool isCreate);
        #endregion
    }
}
