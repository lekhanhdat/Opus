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
    /// Site Collection Level
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class SiteCollectionLevel : AveModule
    {

        private readonly SiteCollectionLevelConfigurationEnforcer sitecollectionlevelconfigurationenforcer = new SiteCollectionLevelConfigurationEnforcer();

        public SiteCollectionLevelConfigurationEnforcer SiteCollectionLevelConfigurationEnforcer
        {
            get { return sitecollectionlevelconfigurationenforcer; }
        }

        private readonly SiteCollectionLevelManagement sitecollectionlevelmanagement = new SiteCollectionLevelManagement();

        public SiteCollectionLevelManagement SiteCollectionLevelManagement
        {
            get { return sitecollectionlevelmanagement; }
        }

        private readonly SiteCollectionLevelConfiguration sitecollectionlevelconfiguration = new SiteCollectionLevelConfiguration();

        public SiteCollectionLevelConfiguration SiteCollectionLevelConfiguration
        {
            get { return sitecollectionlevelconfiguration; }
        }

        private readonly SiteCollectionLevelSecurity sitecollectionlevelsecurity = new SiteCollectionLevelSecurity();

        public SiteCollectionLevelSecurity SiteCollectionLevelSecurity
        {
            get { return sitecollectionlevelsecurity; }
        }

        private readonly SiteCollectionLevelPermissionTools sitecollectionlevelpermissiontools = new SiteCollectionLevelPermissionTools();

        public SiteCollectionLevelPermissionTools SiteCollectionLevelPermissionTools
        {
            get { return sitecollectionlevelpermissiontools; }
        }

         private readonly SiteCollectionLevelUsersandPermissions sitecollectionlevelusersandpermissions = new SiteCollectionLevelUsersandPermissions();

        public SiteCollectionLevelUsersandPermissions SiteCollectionLevelUsersandPermissions
        {
            get { return sitecollectionlevelusersandpermissions; }
        }
        public const string module_name = "Site Collection Level";

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
            //result.Add(SiteCollectionLevelConfigurationEnforcer);
            result.Add(SiteCollectionLevelManagement);
            result.Add(SiteCollectionLevelConfiguration);
            result.Add(SiteCollectionLevelSecurity);
            result.Add(SiteCollectionLevelPermissionTools);
            result.Add(SiteCollectionLevelUsersandPermissions);
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
    /// Site Collection Level
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class SiteCollectionLevelConfigurationEnforcer : AveModule
    {
        public const string module_name = "Site Collection Level Configuration Enforcer";

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
    /// Site Collection Level
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class SiteCollectionLevelManagement : AveModule
    {
        public const string module_name = "Site Collection Level Management";

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
            features.Add(SiteCollectionLevelFeatureDto.Management_New);
            features.Add(SiteCollectionLevelFeatureDto.Management_Move);
            features.Add(SiteCollectionLevelFeatureDto.Management_Delete);
            features.Add(SiteCollectionLevelFeatureDto.Management_AdminSearch);
            features.Add(SiteCollectionLevelFeatureDto.Management_SiteCollectionFeatures);
            features.Add(SiteCollectionLevelFeatureDto.Management_PortalSiteConnection);
            features.Add(SiteCollectionLevelFeatureDto.Management_ContentTypePublishing);
            features.Add(SiteCollectionLevelFeatureDto.Management_WebPart);
            features.Add(SiteCollectionLevelFeatureDto.Management_Themes);
            features.Add(SiteCollectionLevelFeatureDto.Management_Solutions);
            features.Add(SiteCollectionLevelFeatureDto.Management_SiteColumns);
            features.Add(SiteCollectionLevelFeatureDto.Management_CheckBrokenLink);
            features.Add(SiteCollectionLevelFeatureDto.Management_SearchWebPart);
            features.Add(SiteCollectionLevelFeatureDto.Management_SearchDuplicateFiles);
            features.Add(SiteCollectionLevelFeatureDto.Management_ControlWebPartSecurity);

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
    /// Site Collection Level
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class SiteCollectionLevelConfiguration : AveModule
    {
        public const string module_name = "SiteCollectionLevelConfiguration";

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
            features.Add(SiteCollectionLevelFeatureDto.Configuration_SearchSettings);
            features.Add(SiteCollectionLevelFeatureDto.Configuration_QuotasandLocks);
            features.Add(SiteCollectionLevelFeatureDto.Configuration_DeploySiteMaximumDepth);
            features.Add(SiteCollectionLevelFeatureDto.Configuration_VisualUpgrade);
            features.Add(SiteCollectionLevelFeatureDto.Configuration_RSS);
            features.Add(SiteCollectionLevelFeatureDto.Configuration_HelpSettings);
            features.Add(SiteCollectionLevelFeatureDto.Configuration_SharePointDesignerSettings);
            features.Add(SiteCollectionLevelFeatureDto.Configuration_CustomProperties);
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
    /// Site Collection Level
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class SiteCollectionLevelSecurity : AveModule
    {
        public const string module_name = "Site Collection Level Security";

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
            features.Add(SiteCollectionLevelFeatureDto.Security_PeopleandGroups);
            features.Add(SiteCollectionLevelFeatureDto.Security_SitePermissions);
            features.Add(SiteCollectionLevelFeatureDto.Security_SecuritySearch);
            features.Add(SiteCollectionLevelFeatureDto.Security_CloneUserPermissions);
            features.Add(SiteCollectionLevelFeatureDto.Security_CloneSitePermission);
            //features.Add(SiteCollectionLevelFeatureDto.Security_GrantPermissions);
            features.Add(SiteCollectionLevelFeatureDto.Security_GrantPermanentPermissions);
            features.Add(SiteCollectionLevelFeatureDto.Security_GrantTemporaryPermissions);
            features.Add(SiteCollectionLevelFeatureDto.Security_ViewTemporaryPermissions);
            features.Add(SiteCollectionLevelFeatureDto.Security_CreateGroup);
            features.Add(SiteCollectionLevelFeatureDto.Security_EditUserPermissions);
            features.Add(SiteCollectionLevelFeatureDto.Security_DeleteUsersandGroups);
            features.Add(SiteCollectionLevelFeatureDto.Security_StopinheritingPermissionsBreakInheritanceforSubnodes);
            features.Add(SiteCollectionLevelFeatureDto.Security_PermissionLevel);
            features.Add(SiteCollectionLevelFeatureDto.Security_AnoymousAccess);
            features.Add(SiteCollectionLevelFeatureDto.Security_ExportGroupForEditing);
            features.Add(SiteCollectionLevelFeatureDto.Security_ImportConfigurationFile);
            features.Add(SiteCollectionLevelFeatureDto.Security_SiteCollectionAdministrators);
            features.Add(SiteCollectionLevelFeatureDto.Security_DeadAccountCleaner);
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
    /// Site Collection Level
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class SiteCollectionLevelPermissionTools : AveModule
    {
        public const string module_name = "Site Collection Level Permission Tools";

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
            features.Add(SiteCollectionLevelFeatureDto.PermissionTools_GrantPermissions);
            features.Add(SiteCollectionLevelFeatureDto.PermissionTools_CreateGroup);
            features.Add(SiteCollectionLevelFeatureDto.PermissionTools_EditUserPermissions);
            features.Add(SiteCollectionLevelFeatureDto.PermissionTools_RemoveUserPermissions);
            features.Add(SiteCollectionLevelFeatureDto.PermissionTools_DeleteUserGroupsfromSiteCollection);
            features.Add(SiteCollectionLevelFeatureDto.PermissionTools_PermissionLevels);
            features.Add(SiteCollectionLevelFeatureDto.PermissionTools_AnoymousAccess);
            features.Add(SiteCollectionLevelFeatureDto.PermissionTools_SiteCollectionAdministrator);
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
    /// Site Collection Level
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class SiteCollectionLevelUsersandPermissions : AveModule
    {
        public const string module_name = "Site Collection Level Users and Permissions";

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
            features.Add(SiteCollectionLevelFeatureDto.UsersandPermissions_DeleteUserfromSiteCollection);
            features.Add(SiteCollectionLevelFeatureDto.UsersandPermissions_CreateGroup);
            //features.Add(SiteCollectionLevelFeatureDto.UsersandPermissions_EditGroupSetting);
            features.Add(SiteCollectionLevelFeatureDto.UsersandPermissions_Addusers);
            features.Add(SiteCollectionLevelFeatureDto.UsersandPermissions_RemoveUserfromGroup);
            features.Add(SiteCollectionLevelFeatureDto.UsersandPermissions_Groupsettings);
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

    public class SiteCollectionLevelFeatureDto
    {
        public const string Management_New = "050101";
        public const string Management_Move = "050102";
        public const string Management_Delete = "050103";
        public const string Management_AdminSearch = "050104";
        public const string Management_SiteCollectionFeatures = "050105";
        public const string Management_PortalSiteConnection = "050106";
        public const string Management_ContentTypePublishing = "050107";
        public const string Management_WebPart = "050108";
        public const string Management_Themes = "050109";
        public const string Management_Solutions = "050110";
        public const string Management_SiteColumns = "050111";
        public const string Management_CheckBrokenLink = "050112";
        public const string Management_SearchWebPart = "050113";
        public const string Management_SearchDuplicateFiles = "050114";
        public const string Management_ControlWebPartSecurity = "050115";//ADO-172804

        public const string Configuration_SearchSettings = "050201";
        public const string Configuration_QuotasandLocks = "050202";
        public const string Configuration_DeploySiteMaximumDepth = "050203";
        public const string Configuration_VisualUpgrade = "050204";
        public const string Configuration_RSS = "050205";
        public const string Configuration_HelpSettings = "050206";
        public const string Configuration_SharePointDesignerSettings = "050207";
        public const string Configuration_CustomProperties = "050208";

        public const string Security_PeopleandGroups = "050301";
        public const string Security_SitePermissions = "050302";
        public const string Security_SecuritySearch = "050303";
        public const string Security_CloneUserPermissions = "050304";
        public const string Security_CloneSitePermission = "050305";
        //public const string Security_GrantPermissions = "050306";
        public const string Security_CreateGroup = "050307";
        public const string Security_EditUserPermissions = "050308";
        public const string Security_DeleteUsersandGroups = "050309";
        public const string Security_StopinheritingPermissionsBreakInheritanceforSubnodes = "050310";
        public const string Security_PermissionLevel = "050311";
        public const string Security_AnoymousAccess = "050312";
        public const string Security_ExportGroupForEditing = "050313";
        public const string Security_SiteCollectionAdministrators = "050314";
        public const string Security_DeadAccountCleaner = "050315";
        public const string Security_GrantPermanentPermissions = "050316";//新添加的
        public const string Security_GrantTemporaryPermissions = "050317"; //新添加的
        public const string Security_ViewTemporaryPermissions = "050318";//新添加的
        public const string Security_ImportConfigurationFile = "050319";//新添加的

        public const string PermissionTools_GrantPermissions = "050401";
        public const string PermissionTools_CreateGroup = "050402";
        public const string PermissionTools_EditUserPermissions = "050403";
        public const string PermissionTools_RemoveUserPermissions = "050404";
        public const string PermissionTools_DeleteUserGroupsfromSiteCollection = "050405";
        public const string PermissionTools_PermissionLevels = "050406";
        public const string PermissionTools_AnoymousAccess = "050407";
        public const string PermissionTools_SiteCollectionAdministrator = "050408";

        public const string UsersandPermissions_DeleteUserfromSiteCollection = "050501";
        public const string UsersandPermissions_CreateGroup = "050502";
        //public const string UsersandPermissions_EditGroupSetting = "050503";
        public const string UsersandPermissions_Addusers = "050504";
        public const string UsersandPermissions_RemoveUserfromGroup = "050505";
        public const string UsersandPermissions_Groupsettings = "050506";
    }


}
