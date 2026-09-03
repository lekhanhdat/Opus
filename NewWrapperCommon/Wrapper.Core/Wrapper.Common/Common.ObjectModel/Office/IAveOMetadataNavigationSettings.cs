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



using System;
using System.Collections.Generic;
using System.Text;

namespace AvePoint.Wrapper.Common.Office
{
    public interface IAveOMetadataNavigationSettings
    {
        bool AutomaticallyManageListIndexing { get; set; }
        IAveOMetadataNavigationCollection<IAveOMetadataNavigationHierarchy> ConfiguredHierarchies { get; }
        IAveOMetadataNavigationCollection<IAveOMetadataNavigationKeyFilter> ConfiguredKeyFilters { get; }
        bool HideFoldersNode { get; set; }

        void AddViewToAllNodes(IAveOConfiguredView configuredView);
        void BPOSAddViewToAllNodes(IAveList list, Guid viewId);
        void AddConfiguredHierarchy(IAveOMetadataNavigationHierarchy hierarchyToAdd);
        void AddConfiguredKeyFilter(IAveOMetadataNavigationKeyFilter keyFilterToAdd);
        void ClearConfiguredKeyFilters();
        void DetermineIndexingChanges(IAveOFieldIndexDictionary availableIndices, out IAveOFieldIndexDictionary indicesToDelete);
        IAveOMetadataNavigationSettings GetMetadataNavigationSettings(IAveList sourceList);
        void SetMetadataNavigationSettings(IAveList sourceList, IAveOMetadataNavigationSettings listNavSettings);
        void SetMetadataNavigationSettings(IAveList sourceList, IAveOMetadataNavigationSettings listNavSettings, bool updateListIndexing);
        void SetMetadataNavigationSettings(IAveList sourceList, IAveOMetadataNavigationSettings listNavSettings, bool updateListIndexing, bool requiresListUpdate);
        void ExecuteIndexingChanges(IAveList currentList, IAveOFieldIndexDictionary availableIndices, IAveOFieldIndexDictionary indicesToDelete);
        IAveOViewSettingsCollection LookupNodeSettingsCollection(Guid fieldId);
        IAveONodeViewSettings LookupNodeSettings(Guid fieldId, string uniqueNodeId);
        IAveONodeViewSettings LookupNodeSettingsRecursiveSlow(IAveList list, Guid fieldId, string uniqueNodeId);
        Dictionary<string, List<string[]>> Fields { get; }
        void SetBPOSMetadataNavigationSettings(IAveList list, Dictionary<string, string> operations);
        Dictionary<string, object> GetPerLocationViewSettings(IAveList list);
        void SetPerLocalViewSetting(IAveList list, Dictionary<string, object> viewSettingProp);
    }
}
