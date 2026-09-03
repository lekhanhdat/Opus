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
    /// Item Level
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class ItemLevel : AveModule
    {

        private readonly ItemLevelManagement itemlevelmanagement = new ItemLevelManagement();

        public ItemLevelManagement ItemLevelManagement
        {
            get { return itemlevelmanagement; }
        }

        private readonly ItemLevelSecurity itemlevelsecurity = new ItemLevelSecurity();

        public ItemLevelSecurity ItemLevelSecurity
        {
            get { return itemlevelsecurity; }
        }

        private readonly ItemLevelPermissionTools ItemLevelpermissiontools = new ItemLevelPermissionTools();

        public ItemLevelPermissionTools ItemLevelPermissionTools
        {
            get { return ItemLevelpermissiontools; }
        }
        public const string module_name = "Item Level";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_GRANULARBACKUP_ID;
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
            result.Add(ItemLevelManagement);
            result.Add(ItemLevelSecurity);
            result.Add(ItemLevelPermissionTools);
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
    /// Item Level
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class ItemLevelManagement : AveModule
    {
        public const string module_name = "Item Level Management";

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
            features.Add(ItemLevelFeatureDto.Management_AdminSearch);
            features.Add(ItemLevelFeatureDto.Management_Delete);
            features.Add(ItemLevelFeatureDto.Management_ChangeMetadata);
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
    /// Item Level
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class ItemLevelSecurity : AveModule
    {
        public const string module_name = "Item Level Security";

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
            features.Add(ItemLevelFeatureDto.Security_ItemPermissions);
            features.Add(ItemLevelFeatureDto.Security_SecuritySearch);
            features.Add(ItemLevelFeatureDto.Security_GrantPermissions);
            features.Add(ItemLevelFeatureDto.Security_StopInheritingPermissionsBreakInheritanceForSelectedNode);
            features.Add(ItemLevelFeatureDto.Security_InheritPermissionsApplyInheritanceToSelectedNode);
            features.Add(ItemLevelFeatureDto.Security_AlertMe);
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
    /// Item Level
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class ItemLevelPermissionTools : AveModule
    {
        public const string module_name = "Item Level Permission Tools";

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
            features.Add(ItemLevelFeatureDto.PermissionTools_StopinheritingPermissionsBreakInheritanceforSelectedNode);
            features.Add(ItemLevelFeatureDto.PermissionTools_inheritPermissionsApplyInheritancetoSelectedNode);
            features.Add(ItemLevelFeatureDto.PermissionTools_GrantPermissions);
            features.Add(ItemLevelFeatureDto.PermissionTools_EditUserPermissions);
            features.Add(ItemLevelFeatureDto.PermissionTools_RemoveUserPermissions);
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

    public class ItemLevelFeatureDto
    {
        public const string Management_AdminSearch = "030101";
        public const string Management_Delete = "030102";

        public const string Security_ItemPermissions = "030201";
        public const string Security_SecuritySearch = "030202";
        public const string Security_GrantPermissions = "030203";
        public const string Security_StopInheritingPermissionsBreakInheritanceForSelectedNode = "030204";
        public const string Security_InheritPermissionsApplyInheritanceToSelectedNode = "030205";
        public const string Security_AlertMe = "030206";

        public const string PermissionTools_StopinheritingPermissionsBreakInheritanceforSelectedNode = "030301";
        public const string PermissionTools_inheritPermissionsApplyInheritancetoSelectedNode = "030302";
        public const string PermissionTools_GrantPermissions = "030303";
        public const string PermissionTools_EditUserPermissions = "030304";
        public const string PermissionTools_RemoveUserPermissions = "030305";
        public const string Management_ChangeMetadata = "030306";//新添加
        
    }

}
