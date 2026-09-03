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
    /// Farm Level
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class FarmLevel : AveModule
    {
        private readonly FarmLevelConfigurationEnforcer farmlevelconfigurationenforcer = new FarmLevelConfigurationEnforcer();

        public FarmLevelConfigurationEnforcer FarmLevelConfigurationEnforcer
        {
            get { return farmlevelconfigurationenforcer; }
        }
        private readonly FarmLevelManagement farmlevelmanagement = new FarmLevelManagement();

        public FarmLevelManagement FarmLevelManagement
        {
            get { return farmlevelmanagement; }
        }

        private readonly FarmLevelConfiguration farmlevelconfiguration = new FarmLevelConfiguration();

        public FarmLevelConfiguration FarmLevelConfiguration
        {
            get { return farmlevelconfiguration; }
        }

        private readonly FarmLevelSecurity farmlevelsecurity = new FarmLevelSecurity();

        public FarmLevelSecurity FarmLevelSecurity
        {
            get { return farmlevelsecurity; }
        }
        public const string module_name = "Farm Level";

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
            //result.Add(FarmLevelConfigurationEnforcer);
            result.Add(FarmLevelManagement);
            result.Add(FarmLevelConfiguration);
            result.Add(FarmLevelSecurity);
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
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class FarmLevelConfigurationEnforcer : AveModule
    {
        public const string module_name = "Farm Level Configuration Enforcer";

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
    /// Farm Level
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class FarmLevelManagement : AveModule
    {
        public const string module_name = "Farm Level Management";

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
            features.Add(FarmLevelFeatureDto.FarmLevelManagement_New);
            features.Add(FarmLevelFeatureDto.FarmLevelManagement_AdminSearch);
            features.Add(FarmLevelFeatureDto.FarmLevelManagement_ManageFarmFeatures);
            features.Add(FarmLevelFeatureDto.FarmLevelManagement_ManageFarmSolutions);
            features.Add(FarmLevelFeatureDto.FarmLevelManagement_ManageUserSolutions);
            features.Add(FarmLevelFeatureDto.FarmLevelManagement_ServersinFarm);
            features.Add(FarmLevelFeatureDto.FarmLevelManagement_ServicesonServer);
            features.Add(FarmLevelFeatureDto.FarmLevelManagement_QuiesceFarm);
            features.Add(FarmLevelFeatureDto.FarmLevelManagement_ServiceApplications);
            features.Add(FarmLevelFeatureDto.FarmLevelManagement_SearchService);
            features.Add(FarmLevelFeatureDto.FarmLevelManagement_DefaultDatabaseServer);
            features.Add(FarmLevelFeatureDto.FarmLevelManagement_DataRetrievalService);
            features.Add(FarmLevelFeatureDto.FarmLevelManagement_SpecifyQuotaTemplates);
            features.Add(FarmLevelFeatureDto.FarmLevelManagement_UpgradeandPatchManagement);
            features.Add(FarmLevelFeatureDto.FarmLevelManagement_SearchDuplicateFiles);
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
    /// Farm Level
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class FarmLevelConfiguration : AveModule
    {
        public const string module_name = "Farm Level Configuration";

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
            features.Add(FarmLevelFeatureDto.FarmLevelConfiguration_EmailandTextMessageSMS);
            features.Add(FarmLevelFeatureDto.FarmLevelConfiguration_AlternateAccessMappings);
            features.Add(FarmLevelFeatureDto.FarmLevelConfiguration_PrivacyOptions);
            features.Add(FarmLevelFeatureDto.FarmLevelConfiguration_CrosFirewallAccessZone);
            features.Add(FarmLevelFeatureDto.FarmLevelConfiguration_RecordsCenter);
            features.Add(FarmLevelFeatureDto.FarmLevelConfiguration_InfoPathFormsServices);
            features.Add(FarmLevelFeatureDto.FarmLevelConfiguration_CrawlerImpactRules);
            features.Add(FarmLevelFeatureDto.FarmLevelConfiguration_TheSiteDirectory);
            features.Add(FarmLevelFeatureDto.FarmLevelConfiguration_ScanSiteDirectoryLinks);
            features.Add(FarmLevelFeatureDto.FarmLevelConfiguration_FarmSearchAdministration);
            features.Add(FarmLevelFeatureDto.FarmLevelConfiguration_CustomProperties);
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
    /// Farm Level
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class FarmLevelSecurity : AveModule
    {
        public const string module_name = "Farm Level Security";

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
            features.Add(FarmLevelFeatureDto.FarmLevelSecurity_SecuritySearch);
            features.Add(FarmLevelFeatureDto.FarmLevelSecurity_ConfigureManagedAccounts);
            features.Add(FarmLevelFeatureDto.FarmLevelSecurity_ConfigureServiceAccounts);
            features.Add(FarmLevelFeatureDto.FarmLevelSecurity_ConfigurePasswordChangesSettings);
            features.Add(FarmLevelFeatureDto.FarmLevelSecurity_ManageTrust);
            features.Add(FarmLevelFeatureDto.FarmLevelSecurity_ManageAntivirusSettings);
            features.Add(FarmLevelFeatureDto.FarmLevelSecurity_DefineBlockedFileTypes);
            features.Add(FarmLevelFeatureDto.FarmLevelSecurity_ManageWebPartSecurity);
            features.Add(FarmLevelFeatureDto.FarmLevelSecurity_CloneUserPermissions);
            features.Add(FarmLevelFeatureDto.FarmLevelSecurity_ImportConfigurationFile);
            features.Add(FarmLevelFeatureDto.FarmLevelSecurity_ViewTemporaryPermissions);
            features.Add(FarmLevelFeatureDto.FarmLevelSecurity_ConfigureInformationRightsManagement);
            features.Add(FarmLevelFeatureDto.FarmLevelSecurity_ConfigureInformationManagementPolicy);
            features.Add(FarmLevelFeatureDto.FarmLevelSecurity_DeadAccountCleaner);
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

    public class FarmLevelFeatureDto
    {
        public const string FarmLevelManagement_New = "010101";
        public const string FarmLevelManagement_AdminSearch = "010102";
        public const string FarmLevelManagement_ManageFarmFeatures = "010103";
        public const string FarmLevelManagement_ManageFarmSolutions = "010104";
        public const string FarmLevelManagement_ManageUserSolutions = "010105";
        public const string FarmLevelManagement_ServersinFarm = "010106";
        public const string FarmLevelManagement_ServicesonServer = "010107";
        public const string FarmLevelManagement_QuiesceFarm = "010108";
        public const string FarmLevelManagement_ServiceApplications = "010109";
        public const string FarmLevelManagement_SearchService = "010110";
        public const string FarmLevelManagement_DefaultDatabaseServer = "010111";
        public const string FarmLevelManagement_DataRetrievalService = "010112";
        public const string FarmLevelManagement_SpecifyQuotaTemplates = "010113";
        public const string FarmLevelManagement_UpgradeandPatchManagement = "010114";
        public const string FarmLevelManagement_SearchDuplicateFiles = "010115";

        public const string FarmLevelConfiguration_EmailandTextMessageSMS = "010201";
        public const string FarmLevelConfiguration_AlternateAccessMappings = "010202";
        public const string FarmLevelConfiguration_PrivacyOptions = "010203";
        public const string FarmLevelConfiguration_CrosFirewallAccessZone = "010204";
        public const string FarmLevelConfiguration_RecordsCenter = "010205";
        public const string FarmLevelConfiguration_InfoPathFormsServices = "010206";
        public const string FarmLevelConfiguration_CrawlerImpactRules = "010207";
        public const string FarmLevelConfiguration_TheSiteDirectory = "010208";
        public const string FarmLevelConfiguration_ScanSiteDirectoryLinks = "010209";
        public const string FarmLevelConfiguration_FarmSearchAdministration = "010210";
        public const string FarmLevelConfiguration_CustomProperties = "010211";

        public const string FarmLevelSecurity_SecuritySearch = "010301";
        public const string FarmLevelSecurity_ConfigureManagedAccounts = "010302";
        public const string FarmLevelSecurity_ConfigureServiceAccounts = "010303";
        public const string FarmLevelSecurity_ConfigurePasswordChangesSettings = "010304";
        public const string FarmLevelSecurity_ManageTrust = "010305";
        public const string FarmLevelSecurity_ManageAntivirusSettings = "010306";
        public const string FarmLevelSecurity_DefineBlockedFileTypes = "010307";
        public const string FarmLevelSecurity_ManageWebPartSecurity = "010308";
        public const string FarmLevelSecurity_CloneUserPermissions = "010309";
        public const string FarmLevelSecurity_ImportConfigurationFile = "010310";
        public const string FarmLevelSecurity_ConfigureInformationRightsManagement = "010311";
        public const string FarmLevelSecurity_ConfigureInformationManagementPolicy = "010312";
        public const string FarmLevelSecurity_DeadAccountCleaner = "010313";
        public const string FarmLevelSecurity_ViewTemporaryPermissions = "010314"; //新添加的

    }

  
}
