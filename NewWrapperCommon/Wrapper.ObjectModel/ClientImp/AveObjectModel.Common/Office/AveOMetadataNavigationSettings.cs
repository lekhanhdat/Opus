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
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.ObjectModel.Common.Office
{
    class AveOMetadataNavigationSettings : AveClientObject, IAveOMetadataNavigationSettings
    {
        private IAveRequest m_Request;

        public bool AutomaticallyManageListIndexing
        {
            get
            {
                return base.DataCache.GetProperty<bool>("AutomaticallyManageListIndexing");
            }
            set
            {
                base.DataCache.ChangedProperties["AutomaticallyManageListIndexing"] = value;
            }
        }

        public IAveOMetadataNavigationCollection<IAveOMetadataNavigationHierarchy> ConfiguredHierarchies
        {
            get { throw new NotImplementedException(); }
        }

        public IAveOMetadataNavigationCollection<IAveOMetadataNavigationKeyFilter> ConfiguredKeyFilters
        {
            get { throw new NotImplementedException(); }
        }

        public bool HideFoldersNode
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void AddViewToAllNodes(IAveOConfiguredView configuredView)
        {
            throw new NotImplementedException();
        }

        public void AddConfiguredHierarchy(IAveOMetadataNavigationHierarchy hierarchyToAdd)
        {
            throw new NotImplementedException();
        }

        public void AddConfiguredKeyFilter(IAveOMetadataNavigationKeyFilter keyFilterToAdd)
        {
            throw new NotImplementedException();
        }

        public void ClearConfiguredKeyFilters()
        {
            throw new NotImplementedException();
        }

        public void DetermineIndexingChanges(IAveOFieldIndexDictionary availableIndices, out IAveOFieldIndexDictionary indicesToDelete)
        {
            throw new NotImplementedException();
        }

        public IAveOMetadataNavigationSettings GetMetadataNavigationSettings(IAveList sourceList)
        {
            if (m_Request == null)
            {
                m_Request = (sourceList.ParentWeb.Site as AveSite).Request;
            }
            Dictionary<string, object> navigationSettingProperties = m_Request.GetMetadataNavigationSettings(sourceList.ParentWeb.ServerRelativeUrl, sourceList.ID, sourceList.Title);
            base.DataCache.AddPropertyies(navigationSettingProperties);
            return this;
        }

        public void SetMetadataNavigationSettings(IAveList sourceList, IAveOMetadataNavigationSettings listNavSettings)
        {
            m_Request.SetMetadataNavigationSettings(sourceList.ParentWeb.ServerRelativeUrl, sourceList.Title, sourceList.ID, base.DataCache.ChangedProperties);
        }

        public void SetMetadataNavigationSettings(IAveList sourceList, IAveOMetadataNavigationSettings listNavSettings, bool updateListIndexing)
        {
            throw new NotImplementedException();
        }

        public void SetMetadataNavigationSettings(IAveList sourceList, IAveOMetadataNavigationSettings listNavSettings, bool updateListIndexing, bool requiresListUpdate)
        {
            throw new NotImplementedException();
        }

        public void ExecuteIndexingChanges(IAveList currentList, IAveOFieldIndexDictionary availableIndices, IAveOFieldIndexDictionary indicesToDelete)
        {
            throw new NotImplementedException();
        }

        public IAveOViewSettingsCollection LookupNodeSettingsCollection(Guid fieldId)
        {
            throw new NotImplementedException();
        }

        public IAveONodeViewSettings LookupNodeSettings(Guid fieldId, string uniqueNodeId)
        {
            throw new NotImplementedException();
        }

        public IAveONodeViewSettings LookupNodeSettingsRecursiveSlow(IAveList list, Guid fieldId, string uniqueNodeId)
        {
            throw new NotImplementedException();
        }

        public bool IsBPOSS
        {
            get
            {
                return base.DataCache.GetProperty<bool>("BPOSS");
            }
        }

        public Dictionary<string, List<string[]>> Fields
        {
            get
            {
                return base.DataCache.GetProperty<Dictionary<string, List<string[]>>>("MetadataNavigationSettings");
            }
        }

        public void SetBPOSMetadataNavigationSettings(IAveList list, Dictionary<string, string> operations)
        {
            base.DataCache.ChangedProperties["HierarchyData"] = operations["AvailableHierarchyFields"];
            base.DataCache.ChangedProperties["HierarchyPicker"] = operations["SelectedHierarchyFields"];
            base.DataCache.ChangedProperties["KeyFilterData"] = operations["AvailableKeyFilterFields"];
            base.DataCache.ChangedProperties["KeyFilterPicker"] = operations["SelectedKeyFilterFields"];
            base.DataCache.ChangedProperties["HierarchyInitial"] = GetInitialSetting("Hierarchy");
            base.DataCache.ChangedProperties["KeyFilterInitial"] = GetInitialSetting("KeyFilter");
            this.SetMetadataNavigationSettings(list, null);
        }

        private string GetInitialSetting(string type)
        {
            StringBuilder result = new StringBuilder();
            List<string[]> initialFields;
            if (type.Equals("Hierarchy"))
            {
                initialFields = this.Fields["SelectedHierarchyFields"];
            }
            else
            {
                initialFields = this.Fields["SelectedKeyFilterFields"];
            }
            foreach (string[] field in initialFields)
            {
                result.Append(field[0] + "|t" + field[1] + "|t");
            }
            return result.ToString().TrimEnd('t').TrimEnd('|');
        }


        public Dictionary<string, object> GetPerLocationViewSettings(IAveList list)
        {
            if (m_Request == null)
            {
                m_Request = (list.ParentWeb.Site as AveSite).Request;
            }
            return m_Request.GetPerLocationViewSettings(list.ParentWebUrl, list.ID);
        }


        public void BPOSAddViewToAllNodes(IAveList list, Guid viewId)
        {
            if (m_Request == null)
            {
                m_Request = (list.ParentWeb.Site as AveSite).Request;
            }
            m_Request.AddViewToAllNodes(list.ParentWebUrl, list.ID, viewId);
        }


        public void SetPerLocalViewSetting(IAveList list, Dictionary<string, object> viewSettingProp)
        {
            if (m_Request == null)
            {
                m_Request = (list.ParentWeb.Site as AveSite).Request;
            }
            m_Request.SetPerLocalViewSetting(list.ParentWebUrl, list.ID, viewSettingProp);
        }
    }
}
