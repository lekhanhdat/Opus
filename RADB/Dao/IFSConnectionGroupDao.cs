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
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IFSConnectionGroupDao : IBaseDao<FSConnectionGroup>
    {
        List<FSConnectionGroup> LoadAllGroups();

        List<FSConnectionGroup> LoadAllGroupsByDCInternalName(string DCInternalName);

        List<FSConnectionGroup> LoadAllGroupsOfMainDC();

        List<string> LoadAllConnectionGroupNames();

        List<string> LoadAllConnectionGroupNamesByDCInternalName(string DCInternalName);

        IEnumerable<Guid> LoadAllConnectionGroupIdByDCInternalName(string DCInternalName);

        IEnumerable<Guid> LoadAllConnectionGroupIdOfMainDC();

        Task<List<FSConnectionGroup>> FsConnectionGroupWithSearchKey(string searchKey);
        Task<List<FSConnectionGroup>> FsConnectionGroupWithSearchKeyAndId(string searchKey, IEnumerable<Guid> groupIds);

        FSConnectionGroup GetGroup(Guid groupId);
        FSConnectionGroup GetGroupOrNull(Guid groupId);

        FSConnectionGroup GetGroupById(Guid groupId);

        FSConnectionGroup GetGroupByName(string groupName);

        List<FSConnectionGroup> GetGroupByIds(List<Guid> groupIds);

        Task<bool> SaveConnectionGroupAsync(FSConnectionGroup connectionGroup);

        void DeleteGroupConnectoin(Guid groupId);

        AccessConnectionType GetTypeByGroupId(Guid groupId);
        Task<IEnumerable<FSConnectionGroup>> LoadByPager(int pageIndex, int pageSize);
        Task<long> MultiGeoInsertFSConnectionGroupTableAsync(IEnumerable<FSConnectionGroup> fSConnectionGroups);
        Task<long> MultiGeoDeleteAllFSConnectionGroupAsync();

        Task<string> GetGroupDCInternalNameByConnectionId(Guid connectionId);
        Task<Dictionary<string, IEnumerable<string>>> GetGroupDCInternalNameByConnectionIdsAsync(IEnumerable<string> connectionIds);
        Task<(bool, string)> CheckPathAndGetDCInternalName(string fullPath);
    }
}
