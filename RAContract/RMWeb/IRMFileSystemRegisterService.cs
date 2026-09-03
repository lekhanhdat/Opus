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
using AvePoint.RA.Contract.FileSystemRegister.JPMC;
using AvePoint.RA.Contract.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb
{
    public interface IRMFileSystemRegisterService
    {
        Task<List<ConnectionGroupDto>> LoadAllGroupsAsync();

        Task<List<ConnectionGroupDto>> GetAllGroupsAsync();

        Task<List<ConnectionDto>> LoadAllConnectionAsync(bool onlyNoGroup = false);
        Task<ConnectionResultData> LoadAllNoGroupConnectionAsync(GetConnectionListParam param);
        Task<ConnectionResultData> QueryConnectionByPagerAsync(GetConnectionListParam param);
        Task<List<string>> LoadAllConnectionGroupNamesAsync();
        Task<ConnectionGroupDto> GetGroupByIdAsync(Guid groupId);

        Task<ConnectionGroupDto> GetGroupAsync(Guid groupId);
        Task<ConnectionGroupDto> GetGroupOrNullAsync(Guid groupId);

        Task<List<ConnectionDto>> GetAllConnectionsByGroupIdAsync(Guid groupId);

        Task<ConnectionDto> GetConnectionByIdAsync(Guid connectionId);
        string GetConnectionNameByIdAsync(Guid connectionId);
        Task<List<ConnectionDto>> GetConnectionByIdsAsync(List<Guid> connectionIds);

        Task<Guid> CreateConnectionGroupAsync(ConnectionGroupDto connectionGroup);

        Task<int> CreateConnectoinAsync(ConnectionDto connection);

        RAReturnMessage ValidateConnection(ConnectionDto connectionDto, bool isCreate);

        System.Threading.Tasks.Task UpdateConnectionGroupAsync(ConnectionGroupDto connectionGroup);

        Task<int> UpdateConnectoinAsync(ConnectionDto connection);

        void DeleteGroupConnectoin(Guid groupId);

        void DeleteConnectoin(Guid connectionId);

        Task<int> DeleteGroupConnectoinAsync(List<Guid> groupIds);

        Task<int> DeleteConnectoinAsync(List<Guid> connectionIds);

        List<AgentInformationDto> GetAllAgent();

        Task<bool> CorrelateConnectionGroupAsync(CorrelateConnectionDto dto);

        #region JPMC
        public Task<FSConnectionMonitorResultData> QueryConnectionMonitorByPagerAsync(FSConnectionMonitorQueryPager pager);
        public Task<List<string>> QueryAllConnGroupNameRelatedJobAsync(Guid connectionId);
        public Task<List<string>> QueryAllConnPathRelatedJobAsync(Guid connectionId);

        Task<RAReturnMessage> UpdateRecordManagementStatus(string connectionId, RMFSTreeNode.EnableRecordManagementSetting status);
        #endregion
    }
}
