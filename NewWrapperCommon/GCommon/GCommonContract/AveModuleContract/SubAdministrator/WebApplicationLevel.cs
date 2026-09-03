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
    /// Web Application Level
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class WebApplicationLevel : AveModule
    {
        private readonly WebApplicationLevelConfigurationEnforcer webapplicationlevelconfigurationenforcer = new WebApplicationLevelConfigurationEnforcer();

        public WebApplicationLevelConfigurationEnforcer WebApplicationLevelConfigurationEnforcer
        {
            get { return webapplicationlevelconfigurationenforcer; }
        }

        private readonly WebApplicationLevelManagement webapplicationlevelmanagement = new WebApplicationLevelManagement();

        public WebApplicationLevelManagement WebApplicationLevelManagement
        {
            get { return webapplicationlevelmanagement; }
        }

        private readonly WebApplicationLevelConfiguration webapplicationlevelconfiguration = new WebApplicationLevelConfiguration();

        public WebApplicationLevelConfiguration WebApplicationLevelConfiguration
        {
            get { return webapplicationlevelconfiguration; }
        }

        private readonly WebApplicationLevelSecurity webapplicationlevelsecurity = new WebApplicationLevelSecurity();

        public WebApplicationLevelSecurity WebApplicationLevelSecurity
        {
            get { return webapplicationlevelsecurity; }
        }
        public const string module_name = "Web Application Level";

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
            //result.Add(WebApplicationLevelConfigurationEnforcer);
            result.Add(WebApplicationLevelManagement);
            result.Add(WebApplicationLevelConfiguration);
            result.Add(WebApplicationLevelSecurity);
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
    /// Web Application Level
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class WebApplicationLevelConfigurationEnforcer : AveModule
    {
        public const string module_name = "Web Application Level Configuration Enforcer";

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
    /// Web Application Level
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class WebApplicationLevelManagement : AveModule
    {
        public const string module_name = "WebApplicationLevelManagement";

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
            features.Add(WebApplicationLevelFeatureDto.Management_CreateSiteCollection);
            features.Add(WebApplicationLevelFeatureDto.Management_NewHostNamedSiteCollection);
            features.Add(WebApplicationLevelFeatureDto.Management_CreateContentDatabase);
            features.Add(WebApplicationLevelFeatureDto.Management_Extend);
            features.Add(WebApplicationLevelFeatureDto.Management_DeleteWebApplication);
            features.Add(WebApplicationLevelFeatureDto.Management_RemoveSharePointfromIISWebSite);
            features.Add(WebApplicationLevelFeatureDto.Management_AdminSearch);
            features.Add(WebApplicationLevelFeatureDto.Management_GeneralSettings);
            features.Add(WebApplicationLevelFeatureDto.Management_ResourceThrottling);
            features.Add(WebApplicationLevelFeatureDto.Management_Workflow);
            features.Add(WebApplicationLevelFeatureDto.Management_OutgoingEmail);
            features.Add(WebApplicationLevelFeatureDto.Management_MobileAccount);
            features.Add(WebApplicationLevelFeatureDto.Management_SiteUseandDeletions);
            features.Add(WebApplicationLevelFeatureDto.Management_ManageFeatures);
            features.Add(WebApplicationLevelFeatureDto.Management_ManagePaths);
            features.Add(WebApplicationLevelFeatureDto.Management_ServiceConnections);
            features.Add(WebApplicationLevelFeatureDto.Management_ManageContentDatabases);
            features.Add(WebApplicationLevelFeatureDto.Management_SiteCollectionList);
            features.Add(WebApplicationLevelFeatureDto.Management_DeleteOrphanSites);
            features.Add(WebApplicationLevelFeatureDto.Management_SearchWebPart);
            features.Add(WebApplicationLevelFeatureDto.Management_SearchDuplicateFiles); 
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
    /// Web Application Level
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class WebApplicationLevelConfiguration : AveModule
    {
        public const string module_name = "Web Application Level Configuration";

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
            features.Add(WebApplicationLevelFeatureDto.Configuration_SendtoConnections);
            features.Add(WebApplicationLevelFeatureDto.Configuration_DocumentConversions);
            features.Add(WebApplicationLevelFeatureDto.Configuration_SharePointDesignerSettings);
            features.Add(WebApplicationLevelFeatureDto.Configuration_AlternateAccessMappings);
            features.Add(WebApplicationLevelFeatureDto.Configuration_CustomProperties);
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
    /// Web Application Level
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class WebApplicationLevelSecurity : AveModule
    {
        public const string module_name = "WebApplicationLevelSecurity";

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
            features.Add(WebApplicationLevelFeatureDto.Security_AuthenticationProviders);
            features.Add(WebApplicationLevelFeatureDto.Security_SelfServiceSiteCreation);
            features.Add(WebApplicationLevelFeatureDto.Security_SecuritySearch);
            features.Add(WebApplicationLevelFeatureDto.Security_BlockedFileTypes);
            features.Add(WebApplicationLevelFeatureDto.Security_UserPermissions);
            features.Add(WebApplicationLevelFeatureDto.Security_WebPartSecurity);
            features.Add(WebApplicationLevelFeatureDto.Security_CloneUserPermissions);
            features.Add(WebApplicationLevelFeatureDto.Security_UserPolicy);
            features.Add(WebApplicationLevelFeatureDto.Security_AnonymousPolicy);
            features.Add(WebApplicationLevelFeatureDto.Security_PermissionPolicy);
            features.Add(WebApplicationLevelFeatureDto.Security_DeadAccountCleaner);
            features.Add(WebApplicationLevelFeatureDto.Security_ViewTemporaryPermissions);
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

    public class WebApplicationLevelFeatureDto
    {
        public const string Management_CreateSiteCollection = "070101";
        public const string Management_CreateContentDatabase = "070102";
        public const string Management_Extend = "070103";
        public const string Management_DeleteWebApplication = "070104";
        public const string Management_RemoveSharePointfromIISWebSite = "070105";
        public const string Management_AdminSearch = "070106";
        public const string Management_GeneralSettings = "070107";
        public const string Management_ResourceThrottling = "070108";
        public const string Management_Workflow = "070109";
        public const string Management_OutgoingEmail = "070110";
        public const string Management_MobileAccount = "070111";
        public const string Management_SiteUseandDeletions = "070112";
        public const string Management_ManageFeatures = "070113";
        public const string Management_ManagePaths= "070114";
        public const string Management_ServiceConnections = "070115";
        public const string Management_ManageContentDatabases = "070116";
        public const string Management_SiteCollectionList = "070117";
        public const string Management_DeleteOrphanSites = "070118";
        public const string Management_SearchWebPart = "070119";
        public const string Management_SearchDuplicateFiles = "070120";
        public const string Management_NewHostNamedSiteCollection = "070121";

        public const string Configuration_SendtoConnections = "070201";
        public const string Configuration_DocumentConversions = "070202";
        public const string Configuration_SharePointDesignerSettings = "070203";
        public const string Configuration_AlternateAccessMappings = "070204";
        public const string Configuration_CustomProperties = "070205";

        public const string Security_AuthenticationProviders = "070301";
        public const string Security_SelfServiceSiteCreation = "070302";
        public const string Security_SecuritySearch = "070303";
        public const string Security_BlockedFileTypes = "070304";
        public const string Security_UserPermissions = "070305";
        public const string Security_WebPartSecurity = "070306";
        public const string Security_CloneUserPermissions = "070307";
        public const string Security_UserPolicy = "070308";
        public const string Security_AnonymousPolicy = "070309";
        public const string Security_PermissionPolicy = "070310";
        public const string Security_DeadAccountCleaner = "070311";
        public const string Security_ViewTemporaryPermissions = "070312";////新添加的
    }

}
