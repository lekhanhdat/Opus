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
using Microsoft.Office.DocumentManagement.MetadataNavigation;
using AvePoint.Wrapper.Common;
using AvePoint.Common;
using AvePoint.Wrapper.Common.Office;
using AvePoint.ObjectModel.Server16.Office;
using Microsoft.SharePoint;
using System.Collections.Generic;

namespace AvePoint.ObjectModel.Server16.Office
{
    class AveOMetadataNavigationSettings : IAveOMetadataNavigationSettings
    {
        private readonly string mMetadataNavigationSettings_AutomaticallyManageListIndexing_Member = "AutomaticallyManageListIndexing";
        private readonly string mMetadataNavigationSettings_HideFoldersNode_Member = "HideFoldersNode";
        private readonly string mMetadataNavigationSettings_ConfiguredHierarchies_Member = "ConfiguredHierarchies";
        private readonly string mMetadataNavigationSettings_ConfiguredKeyFilters_Member = "ConfiguredKeyFilters";
        private readonly string mMetadataNavigationSettings_DetermineIndexingChanges_Mothed = "DetermineIndexingChanges";
        private readonly string mMetadataNavigationSettings_ExecuteIndexingChanges_Mothed = "ExecuteIndexingChanges";
        private readonly string mMetadataNavigationSettings_SetMetadataNavigationSettings_Mothed = "SetMetadataNavigationSettings";
        private readonly string mMetadataNavigationSettings_LookupNodeSettings_Mothed = "LookupNodeSettings";
        private MetadataNavigationSettings mMetadataNavigationSettings;
        private AveOMetadataNavigationCollection<IAveOMetadataNavigationHierarchy> mConfiguredHierarcheis;
        private AveOMetadataNavigationCollection<IAveOMetadataNavigationKeyFilter> mConfiguredKeyFilters;

        public AveOMetadataNavigationSettings()
            : this(new MetadataNavigationSettings())
        { }

        public AveOMetadataNavigationSettings(MetadataNavigationSettings metadataNavSettings)
        {
            mMetadataNavigationSettings = metadataNavSettings;
        }

        public AveOMetadataNavigationSettings(string xmlMetadataNavigationSettings)
        {
            mMetadataNavigationSettings = new MetadataNavigationSettings(xmlMetadataNavigationSettings);
        }

        internal MetadataNavigationSettings MetadataNavigationSettings
        {
            get
            {
                return mMetadataNavigationSettings;
            }
        }

        #region IAveMetadataNavigationSettings Members

        public void AddViewToAllNodes(IAveOConfiguredView configuredView)
        {
            object pramaConfiguredView = (configuredView as AveOConfiguredView).ConfigureView;
            AveAssemblyUtility.InvokeMethod(mMetadataNavigationSettings, "AddViewToAllNodes", new Type[] { pramaConfiguredView.GetType() }, new object[] { pramaConfiguredView });
        }

        public void ClearConfiguredKeyFilters()
        {
            mMetadataNavigationSettings.ClearConfiguredKeyFilters();
        }

        public IAveOMetadataNavigationSettings GetMetadataNavigationSettings(IAveList sourceList)
        {
            return new AveOMetadataNavigationSettings(MetadataNavigationSettings.GetMetadataNavigationSettings((sourceList as AveList).List));
        }

        public void SetMetadataNavigationSettings(IAveList sourceList, IAveOMetadataNavigationSettings listNavSettings)
        {
            MetadataNavigationSettings.SetMetadataNavigationSettings((sourceList as AveList).List, (listNavSettings as AveOMetadataNavigationSettings).MetadataNavigationSettings,true);
        }

        public void SetMetadataNavigationSettings(IAveList sourceList, object listNavSettings)
        {
            MetadataNavigationSettings.SetMetadataNavigationSettings((sourceList as AveList).List, (MetadataNavigationSettings)listNavSettings);
        }

        public void SetMetadataNavigationSettings(IAveList sourceList, IAveOMetadataNavigationSettings listNavSettings, bool updateListIndexing)
        {
            MetadataNavigationSettings.SetMetadataNavigationSettings((sourceList as AveList).List, (listNavSettings as AveOMetadataNavigationSettings).MetadataNavigationSettings, updateListIndexing);
        }

        public void SetMetadataNavigationSettings(IAveList sourceList, IAveOMetadataNavigationSettings listNavSettings, bool updateListIndexing, bool requiresListUpdate)
        {
            object[] paramObjs = new object[] { (sourceList as AveList).List, (listNavSettings as AveOMetadataNavigationSettings).MetadataNavigationSettings, updateListIndexing, requiresListUpdate };
            Type[] types = new Type[] { typeof(SPList), typeof(MetadataNavigationSettings), typeof(bool), typeof(bool) };
            AveAssemblyUtility.InvokeStaticMethod(mMetadataNavigationSettings.GetType(), mMetadataNavigationSettings_SetMetadataNavigationSettings_Mothed, types, paramObjs);
        }

        public void ExecuteIndexingChanges(IAveList currentList, IAveOFieldIndexDictionary availableIndices, IAveOFieldIndexDictionary indicesToDelete)
        {
            object[] parObjs = new object[] { (currentList as AveList).List, (availableIndices as AveOFieldIndexDictionary).FieldIndexDictionary, (indicesToDelete as AveOFieldIndexDictionary).FieldIndexDictionary };
            AveAssemblyUtility.InvokeMethod(mMetadataNavigationSettings, mMetadataNavigationSettings_ExecuteIndexingChanges_Mothed, parObjs);
        }

        public IAveOViewSettingsCollection LookupNodeSettingsCollection(Guid fieldId)
        {
            object viewSettingsCollection = AveAssemblyUtility.InvokeMethod(mMetadataNavigationSettings, "LookupNodeSettingsCollection", new Type[] { typeof(Guid) }, new object[] { fieldId });
            if (viewSettingsCollection == null)
            {
                return null;
            }
            return new AveOViewSettingsCollection(viewSettingsCollection);
        }

        public IAveONodeViewSettings LookupNodeSettings(Guid fieldId, string uniqueNodeId)
        {
            object[] parObjs = new object[] { fieldId, uniqueNodeId };
            object nodeViewSettings = AveAssemblyUtility.InvokeMethod(mMetadataNavigationSettings, mMetadataNavigationSettings_LookupNodeSettings_Mothed, new Type[] { typeof(Guid), typeof(string) }, parObjs);
            if (nodeViewSettings == null)
            {
                return null;
            }
            return new AveONodeViewSettings(nodeViewSettings);
        }

        public IAveONodeViewSettings LookupNodeSettingsRecursiveSlow(IAveList list, Guid fieldId, string uniqueNodeId)
        {
            object nodeViewSettings = AveAssemblyUtility.InvokeMethod(mMetadataNavigationSettings, "LookupNodeSettingsRecursiveSlow", new Type[] { (list as AveList).List.GetType(), typeof(Guid), typeof(string) }, new object[] { (list as AveList).List, fieldId, uniqueNodeId });
            if (nodeViewSettings == null)
            {
                return null;
            }
            return new AveONodeViewSettings(nodeViewSettings);
        }

        public bool AutomaticallyManageListIndexing
        {
            get
            {
                return (bool)AveAssemblyUtility.GetPropertyValue(mMetadataNavigationSettings, mMetadataNavigationSettings_AutomaticallyManageListIndexing_Member);
            }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mMetadataNavigationSettings, mMetadataNavigationSettings_AutomaticallyManageListIndexing_Member, value);
            }
        }

        public IAveOMetadataNavigationCollection<IAveOMetadataNavigationHierarchy> ConfiguredHierarchies
        {
            get
            {
                if (mConfiguredHierarcheis == null)
                {
                    object configuredHierarcheis = AveAssemblyUtility.GetPropertyValue(mMetadataNavigationSettings, mMetadataNavigationSettings_ConfiguredHierarchies_Member);
                    if (configuredHierarcheis != null)
                    {
                        mConfiguredHierarcheis = new AveOMetadataNavigationCollection<IAveOMetadataNavigationHierarchy>(configuredHierarcheis);
                    }
                }
                return mConfiguredHierarcheis;
            }
        }

        public IAveOMetadataNavigationCollection<IAveOMetadataNavigationKeyFilter> ConfiguredKeyFilters
        {
            get
            {
                if (mConfiguredKeyFilters == null)
                {
                    object configuredKeyFilters = AveAssemblyUtility.GetPropertyValue(mMetadataNavigationSettings, mMetadataNavigationSettings_ConfiguredKeyFilters_Member);
                    if (configuredKeyFilters != null)
                    {
                        mConfiguredKeyFilters = new AveOMetadataNavigationCollection<IAveOMetadataNavigationKeyFilter>(configuredKeyFilters);
                    }
                }
                return mConfiguredKeyFilters;
            }
        }

        public bool HideFoldersNode
        {
            get
            {
                return (bool)AveAssemblyUtility.GetPropertyValue(mMetadataNavigationSettings, mMetadataNavigationSettings_HideFoldersNode_Member);
            }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mMetadataNavigationSettings, mMetadataNavigationSettings_HideFoldersNode_Member, value);
            }
        }

        public void AddConfiguredHierarchy(IAveOMetadataNavigationHierarchy hierarchyToAdd)
        {
            mMetadataNavigationSettings.AddConfiguredHierarchy((hierarchyToAdd as AveOMetadataNavigationHierarchy).MetadataNavigationHierarchy);
        }

        public void AddConfiguredKeyFilter(IAveOMetadataNavigationKeyFilter keyFilterToAdd)
        {
            mMetadataNavigationSettings.AddConfiguredKeyFilter((keyFilterToAdd as AveOMetadataNavigationKeyFilter).MetadataNavigationKeyFilter);
        }

        public void DetermineIndexingChanges(IAveOFieldIndexDictionary availableIndices, out IAveOFieldIndexDictionary indicesToDelete)
        {
            AveOFieldIndexDictionary revIndicesToDelete = new AveOFieldIndexDictionary();
            object[] parObjs = new object[] { (availableIndices as AveOFieldIndexDictionary).FieldIndexDictionary, (revIndicesToDelete as AveOFieldIndexDictionary).FieldIndexDictionary };
            AveAssemblyUtility.InvokeMethod(mMetadataNavigationSettings, mMetadataNavigationSettings_DetermineIndexingChanges_Mothed, parObjs);
            indicesToDelete = revIndicesToDelete;
        }

        #endregion

        public void BPOSAddViewToAllNodes(IAveList list, Guid viewId)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, System.Collections.Generic.List<string[]>> Fields
        {
            get { throw new NotImplementedException(); }
        }

        public void SetBPOSMetadataNavigationSettings(IAveList list, System.Collections.Generic.Dictionary<string, string> operations)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, object> GetPerLocationViewSettings(IAveList list)
        {
            throw new NotImplementedException();
        }

        public void SetPerLocalViewSetting(IAveList list, System.Collections.Generic.Dictionary<string, object> viewSettingProp)
        {
            throw new NotImplementedException();
        }
    }
}
