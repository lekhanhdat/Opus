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

using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace AvePoint.GCommon.Contract.AveModuleContract.SubAdministrator
{
    /// <summary>
    /// List Library Level
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class ListLibraryLevel : AveModule
    {

        private readonly ListLibraryLevelManagement listlibrarylevelmanagement = new ListLibraryLevelManagement();

        public ListLibraryLevelManagement ListLibraryLevelManagement
        {
            get { return listlibrarylevelmanagement; }
        }

        private readonly ListLibraryLevelConfiguration listlibrarylevelconfiguration = new ListLibraryLevelConfiguration();

        public ListLibraryLevelConfiguration ListLibraryLevelConfiguration
        {
            get { return listlibrarylevelconfiguration; }
        }

        private readonly ListLibraryLevelSecurity listlibrarylevelsecurity = new ListLibraryLevelSecurity();

        public ListLibraryLevelSecurity ListLibraryLevelSecurity
        {
            get { return listlibrarylevelsecurity; }
        }

        private readonly ListLibraryLevelPermissionTools listlibrarylevelpermissiontools = new ListLibraryLevelPermissionTools();

        public ListLibraryLevelPermissionTools ListLibraryLevelPermissionTools
        {
            get { return listlibrarylevelpermissiontools; }
        }
        public const string module_name = "List Library Level";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_CENTRALADMIN_ID;
            }

        }

        public override string Name
        {
            get
            {
                return module_name;
            }

        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            result.Add(ListLibraryLevelManagement);
            result.Add(ListLibraryLevelConfiguration);
            result.Add(ListLibraryLevelSecurity);
            result.Add(ListLibraryLevelPermissionTools);
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            List<int> planTypes = new List<int>();
            return planTypes;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            List<int> categories = new List<int>();
            return categories;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }

    /// <summary>
    /// List Library Level
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class ListLibraryLevelManagement : AveModule
    {
        public const string module_name = "List Library Level Management";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_CENTRALADMIN_ID;
            }

        }

        public override string Name
        {
            get
            {
                return module_name;
            }

        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            return result;
        }

        public override List<string> getAllFetures()
        {
            List<string> features = new List<string>();
            features.Add(ListLibraryLevelFeatureDto.Management_NewFolder);
            features.Add(ListLibraryLevelFeatureDto.Management_Delete);
            features.Add(ListLibraryLevelFeatureDto.Management_AdminSearch);
            features.Add(ListLibraryLevelFeatureDto.Management_WorkflowSettings);
            features.Add(ListLibraryLevelFeatureDto.Management_MetadataandKeywordsSettings);
            features.Add(ListLibraryLevelFeatureDto.Management_InformationManagementPolicySettings);
            features.Add(ListLibraryLevelFeatureDto.Management_IndexedColumns);
            features.Add(ListLibraryLevelFeatureDto.Management_NoCheckedInVersionFiles);
            features.Add(ListLibraryLevelFeatureDto.Management_ChangeMetadata);
            return features;
        }

        public override List<int> getAllPlanTypes()
        {
            List<int> planTypes = new List<int>();
            return planTypes;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            List<int> categories = new List<int>();
            return categories;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }

    /// <summary>
    /// List Library Level
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class ListLibraryLevelConfiguration : AveModule
    {
        public const string module_name = "List Library Level Configuration";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_CENTRALADMIN_ID;
            }

        }

        public override string Name
        {
            get
            {
                return module_name;
            }

        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            return result;
        }

        public override List<string> getAllFetures()
        {
            List<string> features = new List<string>();
            features.Add(ListLibraryLevelFeatureDto.Configuration_VersioningSettings);
            features.Add(ListLibraryLevelFeatureDto.Configuration_AdvancedSettings);
            features.Add(ListLibraryLevelFeatureDto.Configuration_ValidationSettings);
            features.Add(ListLibraryLevelFeatureDto.Configuration_GeneralSettingsColumnDefaultValueSettings);
            features.Add(ListLibraryLevelFeatureDto.Configuration_GeneralSettingsRatingSettings);
            features.Add(ListLibraryLevelFeatureDto.Configuration_GeneralSettingsAudienceTargetingSettings);
            features.Add(ListLibraryLevelFeatureDto.Configuration_GeneralSettingsMetadataNavigationSettings);
            features.Add(ListLibraryLevelFeatureDto.Configuration_GeneralSettingsPrelocationViewSettings);
            features.Add(ListLibraryLevelFeatureDto.Configuration_GeneralSettingsRSSSetting);
            features.Add(ListLibraryLevelFeatureDto.Configuration_TittleDescriptionandNavigation);
            return features;
        }

        public override List<int> getAllPlanTypes()
        {
            List<int> planTypes = new List<int>();
            return planTypes;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            List<int> categories = new List<int>();
            return categories;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }
    /// <summary>
    /// List Library Level
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class ListLibraryLevelSecurity : AveModule
    {
        public const string module_name = "List Library Level Security";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_CENTRALADMIN_ID;
            }

        }

        public override string Name
        {
            get
            {
                return module_name;
            }

        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            return result;
        }

        public override List<string> getAllFetures()
        {
            List<string> features = new List<string>();
            features.Add(ListLibraryLevelFeatureDto.Security_ListLibraryPermissions);
            features.Add(ListLibraryLevelFeatureDto.Security_SecuritySearch);
            features.Add(ListLibraryLevelFeatureDto.Security_CloneUserPermissions);
            features.Add(ListLibraryLevelFeatureDto.Security_CloneListLibraryPermissions);
            //features.Add(ListLibraryLevelFeatureDto.Security_GrantPermissions);
            features.Add(ListLibraryLevelFeatureDto.Security_StopinheritingPermissionsBreakeInheritanceforSelectedNodes);
            features.Add(ListLibraryLevelFeatureDto.Security_StopinheritingPermissionsBreakeInheritanceforSubnodes);
            features.Add(ListLibraryLevelFeatureDto.Security_inheritPermissionsApplyInheritancetoSelectedNodes);
            features.Add(ListLibraryLevelFeatureDto.Security_inheritPermissionsPushInheritancetoSubnodes);
            features.Add(ListLibraryLevelFeatureDto.Security_AnonymousAccess);
            features.Add(ListLibraryLevelFeatureDto.Security_AlertmeSetAlertonthisListLirbary);
            features.Add(ListLibraryLevelFeatureDto.Security_AlertmeManageMyAlert);
            features.Add(ListLibraryLevelFeatureDto.Security_GrantPermanentPermissions);
            features.Add(ListLibraryLevelFeatureDto.Security_GrantTemporaryPermissions);
            features.Add(ListLibraryLevelFeatureDto.Security_ViewTemporaryPermissions); 
            return features;
        }

        public override List<int> getAllPlanTypes()
        {
            List<int> planTypes = new List<int>();
            return planTypes;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            List<int> categories = new List<int>();
            return categories;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }

    /// <summary>
    /// List Library Level
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class ListLibraryLevelPermissionTools : AveModule
    {
        public const string module_name = "List Library Level Permission Tools";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_CENTRALADMIN_ID;
            }

        }

        public override string Name
        {
            get
            {
                return module_name;
            }

        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            return result;
        }

        public override List<string> getAllFetures()
        {
            List<string> features = new List<string>();
            features.Add(ListLibraryLevelFeatureDto.PermissionTools_StopinheritingPermissionsBreakeInheritanceforSelectedNodes);
            features.Add(ListLibraryLevelFeatureDto.PermissionTools_StopinheritingPermissionsBreakeInheritanceforSubnodes);
            features.Add(ListLibraryLevelFeatureDto.PermissionTools_inheritPermissionsApplyInheritancetoSelectedNodes);
            features.Add(ListLibraryLevelFeatureDto.PermissionTools_inheritPermissionsPushInheritancetoSubnodes);
            features.Add(ListLibraryLevelFeatureDto.PermissionTools_GrantPermissions);
            features.Add(ListLibraryLevelFeatureDto.PermissionTools_EditUserPermissions);
            features.Add(ListLibraryLevelFeatureDto.PermissionTools_RemoveUserPermissions);
            features.Add(ListLibraryLevelFeatureDto.PermissionTools_AnonymousAccess);
            return features;
        }

        public override List<int> getAllPlanTypes()
        {
            List<int> planTypes = new List<int>();
            return planTypes;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            List<int> categories = new List<int>();
            return categories;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }

    public class ListLibraryLevelFeatureDto
    {
        public const string Management_NewFolder = "040101";
        public const string Management_Delete = "040102";
        public const string Management_AdminSearch = "040103";
        public const string Management_WorkflowSettings = "040104";
        public const string Management_MetadataandKeywordsSettings = "040105";
        public const string Management_InformationManagementPolicySettings = "040106";
        public const string Management_IndexedColumns = "040107";
        public const string Management_NoCheckedInVersionFiles = "040108";
        public const string Management_ChangeMetadata = "040109";

        public const string Configuration_VersioningSettings = "040201";
        public const string Configuration_AdvancedSettings = "040202";
        public const string Configuration_ValidationSettings = "040203";
        public const string Configuration_GeneralSettingsColumnDefaultValueSettings = "040204";
        public const string Configuration_GeneralSettingsRatingSettings = "040205";
        public const string Configuration_GeneralSettingsAudienceTargetingSettings = "040206";
        public const string Configuration_GeneralSettingsMetadataNavigationSettings = "040207";
        public const string Configuration_GeneralSettingsPrelocationViewSettings = "040208";
        public const string Configuration_GeneralSettingsRSSSetting = "040209";
        public const string Configuration_TittleDescriptionandNavigation = "040210";

        public const string Security_ListLibraryPermissions = "040301";
        public const string Security_SecuritySearch = "040302"; 
        public const string Security_CloneUserPermissions ="040303";
        public const string Security_CloneListLibraryPermissions = "040304";
        //public const string Security_GrantPermissions = "040305";
        public const string Security_StopinheritingPermissionsBreakeInheritanceforSelectedNodes ="040306";
        public const string Security_StopinheritingPermissionsBreakeInheritanceforSubnodes = "040307";
        public const string Security_inheritPermissionsApplyInheritancetoSelectedNodes = "040308";
        public const string Security_inheritPermissionsPushInheritancetoSubnodes ="040309";
        public const string Security_AnonymousAccess ="040310";
        public const string Security_AlertmeSetAlertonthisListLirbary = "040311";
        public const string Security_AlertmeManageMyAlert = "040312";
        public const string Security_GrantPermanentPermissions ="040313"; //新添加
        public const string Security_GrantTemporaryPermissions = "040314"; //新添加
        public const string Security_ViewTemporaryPermissions = "040315"; //新添加

        public const string PermissionTools_StopinheritingPermissionsBreakeInheritanceforSelectedNodes ="040401";
        public const string PermissionTools_StopinheritingPermissionsBreakeInheritanceforSubnodes = "040402";
        public const string PermissionTools_inheritPermissionsApplyInheritancetoSelectedNodes = "040403";
        public const string PermissionTools_inheritPermissionsPushInheritancetoSubnodes = "040404";
        public const string PermissionTools_GrantPermissions = "040405";
        public const string PermissionTools_EditUserPermissions = "040406";
        public const string PermissionTools_RemoveUserPermissions = "040407";
        public const string PermissionTools_AnonymousAccess = "040408";

    }


}
