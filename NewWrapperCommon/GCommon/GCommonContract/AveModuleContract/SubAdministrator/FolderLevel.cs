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
    /// Folder Level  
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class FolderLevel : AveModule
    {

        private readonly FolderLevelManagement folderlevelmanagement = new FolderLevelManagement();

        public FolderLevelManagement FolderLevelManagement
        {
            get { return folderlevelmanagement; }
        }

        private readonly FolderLevelSecurity folderlevelsecurity = new FolderLevelSecurity();

        public FolderLevelSecurity FolderLevelSecurity
        {
            get { return folderlevelsecurity; }
        }

        private readonly FolderLevelPermissionTools folderlevelpermissiontools = new FolderLevelPermissionTools();

        public FolderLevelPermissionTools FolderLevelPermissionTools
        {
            get { return folderlevelpermissiontools; }
        }
        public const string module_name = "Folder Level";

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
            result.Add(FolderLevelManagement);
            result.Add(FolderLevelSecurity);
            result.Add(FolderLevelPermissionTools);
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
    /// Folder Level  
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class FolderLevelManagement : AveModule
    {
        public const string module_name = "Folder Level Management";

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
            features.Add(FolderLevelFeatureDto.Management_NewFolder);
            features.Add(FolderLevelFeatureDto.Management_AdminSearch);
            features.Add(FolderLevelFeatureDto.Management_Delete);
            features.Add(FolderLevelFeatureDto.Management_EditProperties);
            features.Add(FolderLevelFeatureDto.Management_ViewProperties);
            features.Add(FolderLevelFeatureDto.Management_ChangeMetadata);
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
    /// Folder Level  
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class FolderLevelSecurity : AveModule
    {
        public const string module_name = "Folder Level Security";

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
            features.Add(FolderLevelFeatureDto.Security_FolderPermissions);
            features.Add(FolderLevelFeatureDto.Security_SecuritySearch);
            features.Add(FolderLevelFeatureDto.Security_CloneUserPermissions);
            features.Add(FolderLevelFeatureDto.Security_CloneFolderPermissions);
            features.Add(FolderLevelFeatureDto.Security_GrantPermissions);
            features.Add(FolderLevelFeatureDto.Security_StopinheritingPermissionsBreakeInheritanceforSelectedNodes);
            features.Add(FolderLevelFeatureDto.Security_StopinheritingPermissionsBreakeInheritanceforSubnodes);
            features.Add(FolderLevelFeatureDto.Security_inheritPermissionsApplyInheritancetoSelectedNodes);
            features.Add(FolderLevelFeatureDto.Security_inheritPermissionsPushInheritancetoSubnodes);
            features.Add(FolderLevelFeatureDto.Security_AlertMe);
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
    /// Folder Level  
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class FolderLevelPermissionTools : AveModule
    {
        public const string module_name = "Folder Level PermissionTools";

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
            features.Add(FolderLevelFeatureDto.PermissionTools_GrantPermissions);
            features.Add(FolderLevelFeatureDto.PermissionTools_EditUserPermissions);
            features.Add(FolderLevelFeatureDto.PermissionTools_RemoveUserPermissions);                                    
            features.Add(FolderLevelFeatureDto.PermissionTools_StopinheritingPermissionsBreakeInheritanceforSelectedNodes);
            features.Add(FolderLevelFeatureDto.PermissionTools_StopinheritingPermissionsBreakeInheritanceforSubnodes);
            features.Add(FolderLevelFeatureDto.PermissionTools_inheritPermissionsApplyInheritancetoSelectedNodes);
            features.Add(FolderLevelFeatureDto.PermissionTools_inheritPermissionsPushInheritancetoSubnodes);              
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

    public class FolderLevelFeatureDto
    {
        public const string Management_NewFolder = "020101";
        public const string Management_AdminSearch = "020102";
        public const string Management_Delete = "020103";
        public const string Management_EditProperties = "020104";
        public const string Management_ViewProperties = "020105";
        public const string Management_ChangeMetadata = "020106";

        public const string Security_FolderPermissions = "020201";
        public const string Security_SecuritySearch = "020202";
        public const string Security_CloneUserPermissions = "020203";
        public const string Security_CloneFolderPermissions = "020204";
        public const string Security_GrantPermissions = "020205";
        public const string Security_StopinheritingPermissionsBreakeInheritanceforSelectedNodes = "020206";
        public const string Security_StopinheritingPermissionsBreakeInheritanceforSubnodes = "020207";
        public const string Security_inheritPermissionsApplyInheritancetoSelectedNodes ="020208";
        public const string Security_inheritPermissionsPushInheritancetoSubnodes = "020209";
        public const string Security_AlertMe = "020210";


        public const string PermissionTools_GrantPermissions = "020301";
        public const string PermissionTools_EditUserPermissions = "020302";
        public const string PermissionTools_RemoveUserPermissions = "020303";
        public const string PermissionTools_StopinheritingPermissionsBreakeInheritanceforSelectedNodes = "020304";
        public const string PermissionTools_StopinheritingPermissionsBreakeInheritanceforSubnodes = "020305";
        public const string PermissionTools_inheritPermissionsApplyInheritancetoSelectedNodes = "020306";
        public const string PermissionTools_inheritPermissionsPushInheritancetoSubnodes = "020307";


    }

}
