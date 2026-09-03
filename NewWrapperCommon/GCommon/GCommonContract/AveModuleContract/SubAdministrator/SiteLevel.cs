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
using System.Diagnostics.CodeAnalysis;                                                                        

namespace AvePoint.GCommon.Contract.AveModuleContract.SubAdministrator
{
    /// <summary>
    /// Site Level
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class SiteLevel : AveModule
    {

        private readonly SiteLevelManagement sitelevelmanagement = new SiteLevelManagement();

        public SiteLevelManagement SiteLevelManagement
        {
            get { return sitelevelmanagement; }
        }

        private readonly SiteLevelConfiguration sitelevelconfiguration = new SiteLevelConfiguration();

        public SiteLevelConfiguration SiteLevelConfiguration
        {
            get { return sitelevelconfiguration; }
        }

        private readonly SiteLevelSecurity sitelevelsecurity = new SiteLevelSecurity();

        public SiteLevelSecurity SiteLevelSecurity
        {
            get { return sitelevelsecurity; }
        }

        private readonly SiteLevelPermissionTools sitelevelpermissiontools = new SiteLevelPermissionTools();

        public SiteLevelPermissionTools SiteLevelPermissionTools
        {
            get { return sitelevelpermissiontools; }
        }

        private readonly SiteLevelUsersandPermissions sitelevelusersandpermissions = new SiteLevelUsersandPermissions();

        public SiteLevelUsersandPermissions SiteLevelUsersandPermissions
        {
            get { return sitelevelusersandpermissions; }
        }
        public const string module_name = "Site Level";

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
            result.Add(SiteLevelManagement);
            result.Add(SiteLevelConfiguration);
            result.Add(SiteLevelSecurity);
            result.Add(SiteLevelPermissionTools);
            result.Add(SiteLevelUsersandPermissions);
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
    /// Site Level
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class SiteLevelManagement : AveModule
    {
        public const string module_name = "Site Level Management";

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
            features.Add(SiteLevelFeatureDto.Management_CreateSubsite);
            features.Add(SiteLevelFeatureDto.Management_CreateListLibrary);
            features.Add(SiteLevelFeatureDto.Management_AdminSearch);
            features.Add(SiteLevelFeatureDto.Management_Delete);
            features.Add(SiteLevelFeatureDto.Management_SiteFeatures);
            features.Add(SiteLevelFeatureDto.Management_ResetToSiteDefinition);
            features.Add(SiteLevelFeatureDto.Management_RegionalSettings);
            features.Add(SiteLevelFeatureDto.Management_SiteColumns);
            features.Add(SiteLevelFeatureDto.Management_SiteContentTypes);
            features.Add(SiteLevelFeatureDto.Management_MasterPage);
            features.Add(SiteLevelFeatureDto.Management_CheckBrokenLink);
            features.Add(SiteLevelFeatureDto.Management_SearchWebPart);
            features.Add(SiteLevelFeatureDto.Management_SearchDuplicateFiles);





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
    /// Site Level
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class SiteLevelConfiguration : AveModule
    {
        public const string module_name = "Site Level Configuration";

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
            features.Add(SiteLevelFeatureDto.Configuration_RSSSetting);
            features.Add(SiteLevelFeatureDto.Configuration_SearchAndOfflineAvailability);
            features.Add(SiteLevelFeatureDto.Configuration_RelatedLinksScopeSettings);
            features.Add(SiteLevelFeatureDto.Configuration_TitleDescriptionandicon);
            features.Add(SiteLevelFeatureDto.Configuration_QuickLaunch);
            features.Add(SiteLevelFeatureDto.Configuration_TopLinkBar);
            features.Add(SiteLevelFeatureDto.Configuration_TreeView);
            features.Add(SiteLevelFeatureDto.Configuration_ChangeTheLook);
            features.Add(SiteLevelFeatureDto.Configuration_SiteTheme);
            features.Add(SiteLevelFeatureDto.Configuration_CustomProperties);




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
    /// Site Level
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class SiteLevelSecurity : AveModule
    {
        public const string module_name = "Site Level Security";

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
            features.Add(SiteLevelFeatureDto.Security_PeopleandGroups);
            features.Add(SiteLevelFeatureDto.Security_SitePermissions);
            features.Add(SiteLevelFeatureDto.Security_SecuritySearch);
            features.Add(SiteLevelFeatureDto.Security_CloneUserPermissions);
            features.Add(SiteLevelFeatureDto.Security_CloneSitePermission);
            //features.Add(SiteLevelFeatureDto.Security_GrantPermissions);
            features.Add(SiteLevelFeatureDto.Security_GrantPermanentPermissions);
            features.Add(SiteLevelFeatureDto.Security_GrantTemporaryPermissions);
            features.Add(SiteLevelFeatureDto.Security_ViewTemporaryPermissions); 
            features.Add(SiteLevelFeatureDto.Security_CreateGroup);
            features.Add(SiteLevelFeatureDto.Security_EditUserPermissions);
            features.Add(SiteLevelFeatureDto.Security_AnonymousAccess);
            features.Add(SiteLevelFeatureDto.Security_Permissionlevels);
            features.Add(SiteLevelFeatureDto.Security_StopinheritingPermissionsBreakeInheritanceforSelectedNodes);
            features.Add(SiteLevelFeatureDto.Security_StopinheritingPermissionsBreakeInheritanceforSubnodes);
            features.Add(SiteLevelFeatureDto.Security_inheritPermissions);
            features.Add(SiteLevelFeatureDto.Security_AlertMeUserAlert);
            features.Add(SiteLevelFeatureDto.Security_AlertMeSearchAlert);
            features.Add(SiteLevelFeatureDto.Security_DeadAccountCleaner);
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
    /// Site Level
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class SiteLevelPermissionTools : AveModule
    {
        public const string module_name = "Site Level Permission Tools";

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
            features.Add(SiteLevelFeatureDto.PermissionTools_GrantPermissions);
            features.Add(SiteLevelFeatureDto.PermissionTools_CreateGroup);
            features.Add(SiteLevelFeatureDto.PermissionTools_EditUserPermissions);
            features.Add(SiteLevelFeatureDto.PermissionTools_RemoveUserPermissions);
            features.Add(SiteLevelFeatureDto.PermissionTools_PermissionLevels);
            features.Add(SiteLevelFeatureDto.PermissionTools_AnonymousAccess);
            features.Add(SiteLevelFeatureDto.PermissionTools_StopinheritingPermissionsBreakeInheritanceforSelectedNodes);
            features.Add(SiteLevelFeatureDto.PermissionTools_StopinheritingPermissionsBreakeInheritanceforSubnodes);
            features.Add(SiteLevelFeatureDto.PermissionTools_InheritPermissionsApplyInheritancetoSelectedNode);
            features.Add(SiteLevelFeatureDto.PermissionTools_InheritPermissionsPushInheritancetoSubnodes);



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
    /// Site Level
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class SiteLevelUsersandPermissions : AveModule
    {
        public const string module_name = "Site Level Users and Permissions";

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
            features.Add(SiteLevelFeatureDto.UsersandPermissions_DeleteUserfromSiteCollection);
            features.Add(SiteLevelFeatureDto.UsersandPermissions_CreateGroup);
            //features.Add(SiteLevelFeatureDto.UsersandPermissions_EditGroupSetting);
            features.Add(SiteLevelFeatureDto.UsersandPermissions_Addusers);
            features.Add(SiteLevelFeatureDto.UsersandPermissions_RemoveUserfromGroup);
            features.Add(SiteLevelFeatureDto.UsersandPermissions_Groupsettings);
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
    [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "Subsite")]
    public class SiteLevelFeatureDto
    {
        public const string Management_CreateSubsite= "060101";
        public const string Management_CreateListLibrary= "060102";
        public const string Management_AdminSearch= "060103";
        public const string Management_Delete= "060104";
        public const string Management_SiteFeatures= "060105";
        public const string Management_ResetToSiteDefinition= "060106";
        public const string Management_RegionalSettings= "060107";
        public const string Management_SiteColumns= "060108";
        public const string Management_SiteContentTypes= "060109";
        public const string Management_MasterPage= "060110";
        public const string Management_CheckBrokenLink= "060111";
        public const string Management_SearchWebPart= "060112";
        public const string Management_SearchDuplicateFiles= "060113";

        public const string Configuration_RSSSetting = "060201 ";
        public const string Configuration_SearchAndOfflineAvailability = "060202";
        public const string Configuration_RelatedLinksScopeSettings = "060203";
        public const string Configuration_TitleDescriptionandicon = "060204";
        public const string Configuration_QuickLaunch = "060205";
        public const string Configuration_TopLinkBar = "060206";
        public const string Configuration_TreeView = "060207";
        public const string Configuration_SiteTheme = "060208";
        public const string Configuration_CustomProperties = "060209";
        public const string Configuration_ChangeTheLook = "060210"; //新添加

        public const string Security_PeopleandGroups = "060301";
        public const string Security_SitePermissions = "060302";
        public const string Security_SecuritySearch = "060303";
        public const string Security_CloneUserPermissions = "060304";
        public const string Security_CloneSitePermission = "060305";
        //public const string Security_GrantPermissions = "060306";
        public const string Security_CreateGroup = "060307";
        public const string Security_EditUserPermissions = "060308";
        public const string Security_AnonymousAccess = "060309";
        public const string Security_Permissionlevels = "060310";
        public const string Security_StopinheritingPermissionsBreakeInheritanceforSelectedNodes= "060311";
        public const string Security_StopinheritingPermissionsBreakeInheritanceforSubnodes= "060312";
        public const string Security_inheritPermissions = "060313";
        public const string Security_AlertMeUserAlert = "060314";
        public const string Security_AlertMeSearchAlert = "060315";
        public const string Security_DeadAccountCleaner = "060316";
        public const string Security_GrantPermanentPermissions ="060317"; //新添加
        public const string Security_GrantTemporaryPermissions ="060318"; //新添加
        public const string Security_ViewTemporaryPermissions = "060319"; //新添加
        
        public const string PermissionTools_GrantPermissions = "060401";
        public const string PermissionTools_CreateGroup = "060402";
        public const string PermissionTools_EditUserPermissions = "060403";
        public const string PermissionTools_RemoveUserPermissions = "060404";
        public const string PermissionTools_PermissionLevels = "060405";
        public const string PermissionTools_AnonymousAccess = "060406";
        public const string PermissionTools_StopinheritingPermissionsBreakeInheritanceforSelectedNodes = "060407";
        public const string PermissionTools_StopinheritingPermissionsBreakeInheritanceforSubnodes ="060408";
        public const string PermissionTools_InheritPermissionsApplyInheritancetoSelectedNode ="060409";
        public const string PermissionTools_InheritPermissionsPushInheritancetoSubnodes ="060410";

        public const string UsersandPermissions_DeleteUserfromSiteCollection = "060501";
        public const string UsersandPermissions_CreateGroup = "060502";
        //public const string UsersandPermissions_EditGroupSetting = "060503";
        public const string UsersandPermissions_Addusers = "060504";
        public const string UsersandPermissions_RemoveUserfromGroup = "060505";
        public const string UsersandPermissions_Groupsettings = "060506";
    }


}
