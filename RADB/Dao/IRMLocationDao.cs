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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.LocationManagement;
using AvePoint.RA.Contract.RMWeb.Tree;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IRMLocationDao : IBaseDao<RMLocation>
    {
        RMLocation GetLocationById(int id);
        RMLocation GetLocationWitPathById(int id, bool isLoadPath);
        Dictionary<int, RMLocation> GetLocationByIDs(IEnumerable<int> ids);
        List<int> GetChildIDsOrderByName(int parentId, List<int> userAndGroupIds = null, List<Guid> topLocationIds = null, bool needCheckPermission = false);
        List<RMLocation> GetAllLocations();
        RMLocation GetLocationByUniqueId(Guid uniqueId, bool isReplaceI18NKey = true);
        List<RMLocation> GetLocationByUniqueIds(List<Guid> uniqueId);
        List<RMLocation> LoadRootNode(int pageIndex, int pageCount);
        RMLocation GetRootLocation();
        RMLocationProfileNode GetLocationChildren(RMLocationProfileNode node);
        RMLocationProfileNode GetRootLocationChildrenWithPermission(RMLocationProfileNode node, List<Guid> topLocationIds);
        RMLocationProfileNode Convert2ProfileNode(int locationId, bool widthChildIDs = false, bool isChecked = false, List<Guid> topLocationIds = null, bool needCheckPermission = false);
        List<RMLocation> GetSubLocationByParentId(int parentId, int pageIndex, int pageCount, List<int> userAndGroupIds = null);
        List<RMLocation> GetTopLocationByParentIdAndId(int parentId, int pageIndex, int pageCount, List<Guid> locationIds = null);
        List<RMLocation> GetAllSubLocationByParentId(int parentId);
        List<RMLocation> GetAllSubLocationByParentIdAndUniqueIds(int parentId, List<Guid> locationIds);
        Task<RMLocation> RenameLocationAsync(int locationId, string name, bool ensureConflict);
        RMLocation CreateLocation(string name, int parentId);
        Task<bool> DeleteLocationAsync(int locationId);
        RMLocation GetLocationsBySearch(string locationStr);
        RMLocationProfileNode SearchLocationTree(string searchKey);
        List<RMLocationProfileNode> GetSubLocationByParentId(int parentId);
        Task<RMLocation> SaveLocationSettingAsync(LocationInfo locationSetting);
        int CountSubLocation(int parentId);
        int CountSubLocation(int parentId, List<int> userAndGroupIds);
        int CountSubLocationByLocationIds(int parentId, List<Guid> locationIds);
        bool HasSubLocation(int parentId);

        bool HasSameName(string name, int parentId);
        RMLocation GetByName(string name, int parentId);
        void UpgradeBottomLocationAssociation();
        List<Guid> GetLocationSuiteAssociationIds(Guid uniqueId);
        RMLocation GetLocationInfo(Guid uniqueId);
        string GetLocationPath(string dirPath, bool isReplaceI18NKey = true);

        List<RMLocation> GetLocationInfos(IEnumerable<Guid> uniqueIds);

        Dictionary<int, string> GetLocationIdNameMapping();
        IEnumerable<RMLocation> GetLocationBottomByLocationIds(IEnumerable<Guid> locationIds);
        IEnumerable<RMLocation> GetLocationNormalByIds(List<string> ids);

        Task<List<RMLocation>> GetAllTopLocation();
        Task<List<Guid>> GetAllTopLocationIds();

        List<Guid> LoadAllLocationIdUnderTopLocation(List<Guid> topLocationIds);

        List<Guid> LoadAllLocationBottomIdUnderTopLocation(List<Guid> topLocationIds);

        List<string> LoadLocationPathByLocationIds(List<Guid> locationIds);
        Guid LoadTopLocationIdBySubLocation(Guid locationId);
        Guid GetLocationUniqueIdById(int id);
        List<Guid> GetLocationUniqueIds();
    }
}
