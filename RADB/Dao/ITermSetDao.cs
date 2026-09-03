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
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface ITermSetDao : IBaseDao<RMTermSet>
    {
        RMTermSet AddTermSetInfo(string termSetName, string termDescription);

        RMTermSet GetRMTermSet(int termSetId);
        RMTermSet GetTermSetByName(string name);
        List<RMTermSet> GetTermSetsByGroupId(Guid termGroupId, TermSetType type, int pageIndex, int pageSize, FilterTermObjOption filterOption = null);
        Task<List<RMTermSet>> LoadTermSetAsync(TermSetType type, Guid parentTermGroupId, FilterTermObjOption filterOption = null);
        Task<List<RMTermSet>> LoadTermSetWithDeletedItemsAsync(TermSetType type, Guid parentTermGroupId);
        Task<RMTermSet> UpdateTermSetAsync(int termSetId, string termSetName, string description);

        Task<RMTermSet> RenameTermSetAsync(int termSetId, Guid termGroupId, string termSetName);
        Task UpdateGroupIdOfTermSetAsync(int termSetId, Guid termGroupId);
        List<RMTermSet> GetRMTermSetsByGroupUniqueId(Guid groupId, FilterTermObjOption filterOption = null);

        List<RMTermSet> GetTermSetsByGroupUniqueIdsAndIds(IEnumerable<Guid> groupIds, IEnumerable<int> termSetIds);

        List<RMTermSet> GetRMTermSetsByGroupUniqueIdAndTermSetName(Guid groupId, string termsetName);
        RMTermSet GetGoogleTermSetByGroupUniqueId(Guid groupId);
        bool HasExistsTermSet(Guid termGroupId);
        bool HasOtherTermSet(Guid termGroupId, Guid termSetId);
        //Create Term Set By UniqueId (parent groupid)
        void CreateTermSetByUniqueId(Guid termSetId, string termSetName, string description, Guid termGroupId);
        RMTermSet CreateTermSet(string name, Guid termGroupId, string desc = "");
        Task<RMTermSet> CreateGoogleTermSet(string name, Guid termGroupId);
        RMTermSet GetRMTermSetByGuid(Guid termSetId);
        void DeleteAllTermSet();
        Task DeleteTermSetAsync(int termSetId);
        bool HasSameNameTermSet(string termSetName, Guid termGroupId);
        List<RMTermSet> GetTermSets(Guid termGroupId, PagerInfo pager, out int totalCount);
        List<RMTermSet> LoadTermSetNodes(Guid parentTermGroupId);
        RMTermSet GetFirstTermSetByTermGroupId(Guid parentTermGroupId);
        Task<IEnumerable<RMTermSet>> LoadByPager(int pageIndex, int pageSize);
        Task<long> MultiGeoInsertTermSetTableAsync(IEnumerable<RMTermSet> termSets);
        Task<long> MultiGeoDeleteAllTermSetAsync();
        Task<List<RMTermSet>> GetTermSetsByTermSetIds(List<Guid> termSetIds);
    }
}
