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
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IRMFSConnectionAndOwnerRelationshipDao : IBaseDao<RMFSConnectionAndOwnerRelationship>
    {
        List<RMFSConnectionAndOwnerRelationship> GetOwnersByConnectionId(Guid connectionId);

        List<RMFSConnectionAndOwnerRelationship> GetOwnersByConnectionId(Guid connectionId, FSConnectionOwnerType ownerType);

        Task<List<RMFSConnectionAndOwnerRelationship>> GetOwnersByUserIntIdAsync(int userIntId);
        Task<List<RMFSConnectionAndOwnerRelationship>> GetOwnersByUserIntIdsAsync(List<int> userIntIds);

        void AddOwners(Guid connectionId, List<int> informationOwnerIntIds, List<int> recordOwnerIntIds);

        Task RemoveAllByConnectionIdAsync(Guid connectionId);

        Task RemoveAllRecordOwnersByConnectionIdAsync(Guid connectionId);

        Task RemoveAllByConnectionIdsAsync(List<Guid> connectionIds);
        Task<IEnumerable<RMFSConnectionAndOwnerRelationship>> LoadByPager(int pageIndex, int pageSize);
        Task<long> MultiGeoInsertFSConnectionAndOwnerRelationshipTableAsync(IEnumerable<RMFSConnectionAndOwnerRelationship> fSConnectionAndOwnerRelationships);
        Task<long> MultiGeoDeleteAllFSConnectionAndOwnerRelationshipAsync();
        Task<List<FSConnection>> GetConnectionsByUserIdsAndRoles(List<int> userIds, List<FSConnectionOwnerType>? userRoles = null);
    }
}
