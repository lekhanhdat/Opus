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
using AvePoint.GCommon.Contract.Server.Common.Attribute;
using System.Diagnostics.CodeAnalysis;

[module: SuppressMessage("Microsoft.Naming", "CA1708:IdentifiersShouldDifferByMoreThanCase", Scope = "type", Target = "AvePoint.GCommon.Contract.AveModuleContract.Monitor")]
namespace AvePoint.GCommon.Contract.AveModuleContract
{
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class ControlPanel : AveModuleContainer
    {

        private const string MODULE_TYPE_DOCAVE_CONTROLPANNEL_NAME = "Control Panel";

        private readonly SubControlPanel subControlPanel = new SubControlPanel();
        public SubControlPanel SubControlPanel
        {
            get { return subControlPanel; }
        }

        private readonly Monitor monitor = new Monitor();
        public Monitor Monitor
        {
            get { return monitor; }
        }

        private readonly SystemOptions systemOptions = new SystemOptions();
        public SystemOptions SystemOptions
        {
            get { return systemOptions; }
        }

        private readonly AccountManager accountManager = new AccountManager();
        public AccountManager AccountManager
        {
            get { return accountManager; }
        }

        private readonly AuthenticationManager authenticationManager = new AuthenticationManager();
        public AuthenticationManager AuthenticationManager
        {
            get { return authenticationManager; }
        }

        private readonly LicenseManager licenseManager = new LicenseManager();
        public LicenseManager LicenseManager
        {
            get { return licenseManager; }
        }

        private readonly UpdateManager updateManager = new UpdateManager();
        public UpdateManager UpdateManager
        {
            get { return updateManager; }
        }

        private readonly AgentGroup agentGroup = new AgentGroup();
        public AgentGroup AgentGroup
        {
            get { return agentGroup; }
        }

        private readonly UserNotificationSettings userNotificationSettings = new UserNotificationSettings();
        public UserNotificationSettings UserNotificationSettings
        {
            get { return userNotificationSettings; }
        }

        private readonly UserSendNotificationSettings userSendNotificationSettings = new UserSendNotificationSettings();
        public UserSendNotificationSettings UserSendNotificationSettings
        {
            get { return userSendNotificationSettings; }
        }
        private readonly JobPruning jobPruning = new JobPruning();
        public JobPruning JobPruning
        {
            get { return jobPruning; }
        }

        private readonly PerformanceAlert performanceAlert = new PerformanceAlert();
        public PerformanceAlert PerformanceAlert
        {
            get { return performanceAlert; }
        }

        private readonly HostProfile hostProfile = new HostProfile();
        public HostProfile HostProfile
        {
            get { return hostProfile; }
        }

        private readonly LogManager logManager = new LogManager();
        public LogManager LogManager
        {
            get { return logManager; }
        }

        private readonly Office365 office365 = new Office365();
        public Office365 Office365
        {
            get { return office365; }
        }

        private readonly ProfileManager profileManager = new ProfileManager();
        public ProfileManager ProfileManager
        {
            get { return profileManager; }
        }

        private readonly ManagedAccountsProfile managedAccountsProfile = new ManagedAccountsProfile();
        public ManagedAccountsProfile ManagedAccountsProfile
        {
            get { return managedAccountsProfile; }
        }

        private readonly SolutionManager solutionManager = new SolutionManager();
        public SolutionManager SolutionManager
        {
            get { return solutionManager; }
        }

        private readonly StorageManager storageManager = new StorageManager();
        public StorageManager StorageManager
        {
            get { return storageManager; }
        }

        private readonly PlanGroup planGroup = new PlanGroup();
        public PlanGroup PlanGroup
        {
            get { return planGroup; }
        }

        private readonly DataManager dataManager = new DataManager();
        public DataManager DataManager
        {
            get { return dataManager; }
        }

        private readonly IndexManager indexManager = new IndexManager();
        public IndexManager IndexManager
        {
            get { return indexManager; }
        }

        private readonly ExportLocation exportLocation = new ExportLocation();
        public ExportLocation ExportLocation
        {
            get { return exportLocation; }
        }

        private readonly MappingManager mappingManager = new MappingManager();
        public MappingManager MappingManager
        {
            get { return mappingManager; }
        }

        private readonly FilterPolicy filterPolicy = new FilterPolicy();
        public FilterPolicy FilterPolicy
        {
            get { return filterPolicy; }
        }

        private readonly HealthAnalyzer healthAnalyzer = new HealthAnalyzer();
        public HealthAnalyzer HealthAnalyzer
        {
            get { return healthAnalyzer; }
        }

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_CONTROLPANNEL_ID; }
        }

        public override string Name
        {
            get { return MODULE_TYPE_DOCAVE_CONTROLPANNEL_NAME; }
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            //result.Add(SubControlPanel);
            result.Add(Monitor);
            result.Add(SystemOptions);
            result.Add(AccountManager);
            result.Add(AuthenticationManager);
            result.Add(LicenseManager);
            result.Add(UpdateManager);
            result.Add(AgentGroup);
            result.Add(UserNotificationSettings);
            result.Add(UserSendNotificationSettings);
            result.Add(JobPruning);
            result.Add(LogManager);
            result.Add(profileManager);
            result.Add(ManagedAccountsProfile);
            result.Add(Office365);
            result.Add(SolutionManager);
            result.Add(StorageManager);
            result.Add(ExportLocation);
            result.Add(PlanGroup);
            result.Add(DataManager);
            result.Add(IndexManager);
            result.Add(MappingManager);
            result.Add(FilterPolicy);
            result.Add(PerformanceAlert);
            result.Add(HostProfile);
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return new List<int>();
        }

        public override List<int> getAllJobTypes()
        {
            return new List<int>();
        }

        public override List<int> getCategories()
        {
            return new List<int>();
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }

    #region Control Panel
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    public class SubControlPanel : AveModule
    {
        private const string MODULE_TYPE_DOCAVE_CONTROLPANNEL_NAME = "Control Panel";
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_CONTROLPANNEL_ID; }
        }

        public override string Name
        {
            get { return MODULE_TYPE_DOCAVE_CONTROLPANNEL_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string>();
        }

        public override List<AveModule> getSubModules()
        {
            return new List<AveModule>();
        }

        public override List<int> getAllPlanTypes()
        {
            return new List<int>();
        }

        public override List<int> getAllJobTypes()
        {
            return new List<int>();
        }

        public override List<int> getCategories()
        {
            return new List<int>();
        }
    }
    #endregion

    #region Monitor
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    public class Monitor : AveModule
    {
        private const string name = "Monitor";
        public const int type_job_collect = (int)JobTypes.ReportCollector;
        public const string name4Job = "Report Collector";

        public int JOB_TYPE_REPOER_COLLECT
        {
            get { return type_job_collect; }
        }

        public string Name4Job
        {
            get { return name4Job; }
        }

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_CONTROLPANEL_MONITOR_ID; }
        }

        public override string Name
        {
            get { return name; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string>();
        }

        public override List<AveModule> getSubModules()
        {
            return new List<AveModule>();
        }

        public override List<int> getAllPlanTypes()
        {
            return new List<int>();
        }

        public override List<int> getAllJobTypes()
        {
            return new List<int>();
        }

        public override List<int> getCategories()
        {
            return new List<int>();
        }
    }
    #endregion

    #region System Options
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    public class SystemOptions : AveModule
    {
        private const string name = "System Options";

        private const int type_language_translater = (int)JobTypes.LanguageTranslater;
        public int TYPE_LANGUAGE_TRANSLATER
        {
            get { return type_language_translater; }
        }

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_CONTROLPANEL_SYSTEMOPTIONS_ID; }
        }

        public override string Name
        {
            get { return name; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string>();
        }

        public override List<AveModule> getSubModules()
        {
            return new List<AveModule>();
        }

        public override List<int> getAllPlanTypes()
        {
            return new List<int>();
        }

        public override List<int> getAllJobTypes()
        {
            return new List<int>();
        }

        public override List<int> getCategories()
        {
            return new List<int>();
        }
    }
    #endregion

    #region Account Manager
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    public class AccountManager : AveModule
    {
        private const string name = "Account Manager";

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_CONTROLPANEL_ACCOUNTMANAGER_ID; }
        }

        public override string Name
        {
            get { return name; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string>();
        }

        public override List<AveModule> getSubModules()
        {
            return new List<AveModule>();
        }

        public override List<int> getAllPlanTypes()
        {
            return new List<int>();
        }

        public override List<int> getAllJobTypes()
        {
            return new List<int>();
        }

        public override List<int> getCategories()
        {
            return new List<int>();
        }
    }
    #endregion

    #region Authentication Manager
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    public class AuthenticationManager : AveModule
    {
        private const string name = "Authentication Manager";

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_CONTROLPANEL_AUTHENTICATIONMANAGER_ID; }
        }

        public override string Name
        {
            get { return name; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string>();
        }

        public override List<AveModule> getSubModules()
        {
            return new List<AveModule>();
        }

        public override List<int> getAllPlanTypes()
        {
            return new List<int>();
        }

        public override List<int> getAllJobTypes()
        {
            return new List<int>();
        }

        public override List<int> getCategories()
        {
            return new List<int>();
        }
    }
    #endregion

    #region License Manager
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    public class LicenseManager : AveModule
    {
        private const string name = "License Manager";

        public const int type_license_manager = (int)JobTypes.LicenseManager;

        public int JOB_TYPE_LICENSE_MANAGER
        {
            get { return type_license_manager; }
        }

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_CONTROLPANEL_LICENSEMANAGER_ID; }
        }

        public override string Name
        {
            get { return name; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string>();
        }

        public override List<AveModule> getSubModules()
        {
            return new List<AveModule>();
        }

        public override List<int> getAllPlanTypes()
        {
            return new List<int>();
        }

        public override List<int> getAllJobTypes()
        {
            return new List<int>();
        }

        public override List<int> getCategories()
        {
            return new List<int>();
        }
    }
    #endregion

    #region Update Manager
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    public class UpdateManager : AveModule
    {
        private const string name = "Update Manager";

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_CONTROLPANEL_UPDATEMANAGER_ID; }
        }

        public override string Name
        {
            get { return name; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string>();
        }

        public override List<AveModule> getSubModules()
        {
            return new List<AveModule>();
        }

        public override List<int> getAllPlanTypes()
        {
            return new List<int>();
        }

        public override List<int> getAllJobTypes()
        {
            return new List<int>();
        }

        public override List<int> getCategories()
        {
            return new List<int>();
        }
    }
    #endregion

    #region Agent Group
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    public class AgentGroup : AveModule
    {
        private const string name = "Agent Group";

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_CONTROLPANEL_AGENTGROUP_ID; }
        }

        public override string Name
        {
            get { return name; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string>();
        }

        public override List<AveModule> getSubModules()
        {
            return new List<AveModule>();
        }

        public override List<int> getAllPlanTypes()
        {
            return new List<int>();
        }

        public override List<int> getAllJobTypes()
        {
            return new List<int>();
        }

        public override List<int> getCategories()
        {
            return new List<int>();
        }
    }
    #endregion

    #region User Notification Settings
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class UserNotificationSettings : AveModule
    {
        private const string name = "User Notification Settings";

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_CONTROLPANEL_USERNOTIFICATIONSETTINGS_ID; }
        }

        public override string Name
        {
            get { return name; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string>();
        }

        public override List<AveModule> getSubModules()
        {
            return new List<AveModule>();
        }

        public override List<int> getAllPlanTypes()
        {
            return new List<int>();
        }

        public override List<int> getAllJobTypes()
        {
            return new List<int>();
        }

        public override List<int> getCategories()
        {
            return new List<int>();
        }
    }
    #endregion

    #region User Send Notification Settings
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    public class UserSendNotificationSettings : AveModule
    {
        private const string name = "User Send Notification Settings";

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_CONTROLPANEL_USERNOTIFICATIONSETTINGS_ID; }
        }

        public override string Name
        {
            get { return name; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string>();
        }

        public override List<AveModule> getSubModules()
        {
            return new List<AveModule>();
        }

        public override List<int> getAllPlanTypes()
        {
            return new List<int>();
        }

        public override List<int> getAllJobTypes()
        {
            return new List<int>();
        }

        public override List<int> getCategories()
        {
            return new List<int>();
        }
    }
    #endregion

    #region Job Pruning
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    public class JobPruning : AveModule
    {
        private const string name = "Job Pruning";

        public const int type_job_pruning = (int)JobTypes.JobPruning;

        public int JOB_TYPE_JOB_PRUNING
        {
            get { return type_job_pruning; }
        }

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_CONTROLPANEL_JOBPRUNING_ID; }
        }

        public override string Name
        {
            get { return name; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string>();
        }

        public override List<AveModule> getSubModules()
        {
            return new List<AveModule>();
        }

        public override List<int> getAllPlanTypes()
        {
            return new List<int>();
        }

        public override List<int> getAllJobTypes()
        {
            return new List<int>();
        }

        public override List<int> getCategories()
        {
            return new List<int>();
        }
    }
    #endregion

    #region Performance Alert
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    public class PerformanceAlert : AveModule
    {
        private const string name = "Performance Alert";

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_CONTROLPANEL_PERFORMANCEALERT_ID; }
        }

        public override string Name
        {
            get { return name; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.Available; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string>();
        }

        public override List<AveModule> getSubModules()
        {
            return new List<AveModule>();
        }

        public override List<int> getAllPlanTypes()
        {
            return new List<int>();
        }

        public override List<int> getAllJobTypes()
        {
            return new List<int>();
        }

        public override List<int> getCategories()
        {
            return new List<int>();
        }
    }
    #endregion
    #region Host Profile
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class HostProfile : AveModule
    {
        private const string name = "Host Profile";

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_CONTROLPANEL_HOSTPROFILE_ID; }
        }

        public override string Name
        {
            get { return name; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string>();
        }

        public override List<AveModule> getSubModules()
        {
            return new List<AveModule>();
        }

        public override List<int> getAllPlanTypes()
        {
            return new List<int>();
        }

        public override List<int> getAllJobTypes()
        {
            return new List<int>();
        }

        public override List<int> getCategories()
        {
            return new List<int>();
        }
    }
    #endregion
    
    #region Log Manager
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    public class LogManager : AveModule
    {
        private const string name = "Log Manager";

        public const int type_log_manager = (int)JobTypes.LogManager;

        public int JOB_TYPE_LOG_MANAGER
        {
            get { return type_log_manager; }
        }
        public const int type_log_manager_by_job = (int)JobTypes.LogManagerByJob;

        public int JOB_TYPE_LOG_MANAGER_BY_JOB
        {
            get { return type_log_manager_by_job; }
        }
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_CONTROLPANEL_LOGMANAGER_ID; }
        }

        public override string Name
        {
            get { return name; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string>();
        }

        public override List<AveModule> getSubModules()
        {
            return new List<AveModule>();
        }

        public override List<int> getAllPlanTypes()
        {
            return new List<int>();
        }

        public override List<int> getAllJobTypes()
        {
            return new List<int>();
        }

        public override List<int> getCategories()
        {
            return new List<int>();
        }
    }
    #endregion

    #region Office365
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class Office365 : AveModule
    {
        private const string name = "SharePoint Sites";

        public const int type_office365_autoscan = (int)JobTypes.Office365AutoScan;

        public int JOB_TYPE_OFFICE365_AUTOSCAN
        {
            get { return type_office365_autoscan; }
        }

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_CONTROLPANEL_OFFICE365_ID; }
        }

        public override string Name
        {
            get { return name; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string>();
        }

        public override List<AveModule> getSubModules()
        {
            return new List<AveModule>();
        }

        public override List<int> getAllPlanTypes()
        {
            return new List<int>();
        }

        public override List<int> getAllJobTypes()
        {
            return new List<int>();
        }

        public override List<int> getCategories()
        {
            return new List<int>();
        }
    }
    #endregion

    #region ProfileManager

    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class ProfileManager : AveModule
    {
        private const string name = "Security Profile";
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_CONTROLPANEL_SCURITYPROFILE_ID; }
        }
        public override string Name
        {
            get { return name; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string>();
        }

        public override List<AveModule> getSubModules()
        {
            return new List<AveModule>();
        }

        public override List<int> getAllPlanTypes()
        {
            return new List<int>();
        }

        public override List<int> getAllJobTypes()
        {
            return new List<int>();
        }

        public override List<int> getCategories()
        {
            return new List<int>();
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class ManagedAccountsProfile : AveModule
    {
        private const string name = "Managed Accounts Profile";
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_CONTROLPANEL_MANAGEDACCOUNTSPROFILE_ID; }
        }
        public override string Name
        {
            get { return name; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string>();
        }

        public override List<AveModule> getSubModules()
        {
            return new List<AveModule>();
        }

        public override List<int> getAllPlanTypes()
        {
            return new List<int>();
        }

        public override List<int> getAllJobTypes()
        {
            return new List<int>();
        }

        public override List<int> getCategories()
        {
            return new List<int>();
        }
    }
    #endregion

    #region Solution Manager
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    public class SolutionManager : AveModule
    {
        private const string name = "Solution Manager";

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_CONTROLPANEL_SOLUTIONMANGER_ID; }
        }

        public override string Name
        {
            get { return name; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string>();
        }

        public override List<AveModule> getSubModules()
        {
            return new List<AveModule>();
        }

        public override List<int> getAllPlanTypes()
        {
            return new List<int>();
        }

        public override List<int> getAllJobTypes()
        {
            return new List<int>();
        }

        public override List<int> getCategories()
        {
            return new List<int>();
        }
    }
    #endregion

    #region Storage Manager
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class StorageManager : AveModule
    {
        private const string name = "Storage Configuration";

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_CONTROLPANEL_STORAGEMANAGER_ID; }
        }

        public override string Name
        {
            get { return name; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string>();
        }

        public override List<AveModule> getSubModules()
        {
            return new List<AveModule>();
        }

        public override List<int> getAllPlanTypes()
        {
            return new List<int>();
        }

        public override List<int> getAllJobTypes()
        {
            return new List<int>();
        }

        public override List<int> getCategories()
        {
            return new List<int>();
        }
    }
    #endregion

    #region Export Location
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class ExportLocation : AveModule
    {
        private const string name = "Export Location";

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_CONTROLPANEL_EXPORTLOCATION_ID; }
        }

        public override string Name
        {
            get { return name; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string>();
        }

        public override List<AveModule> getSubModules()
        {
            return new List<AveModule>();
        }

        public override List<int> getAllPlanTypes()
        {
            return new List<int>();
        }

        public override List<int> getAllJobTypes()
        {
            return new List<int>();
        }

        public override List<int> getCategories()
        {
            return new List<int>();
        }
    }
    #endregion

    #region Plan Group
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    public class PlanGroup : AveModule
    {
        private const string name = "Plan Group";

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_CONTROLPANEL_PLANGROUP_ID; }
        }

        public override string Name
        {
            get { return name; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string>();
        }

        public override List<AveModule> getSubModules()
        {
            return new List<AveModule>();
        }

        public override List<int> getAllPlanTypes()
        {
            return new List<int>();
        }

        public override List<int> getAllJobTypes()
        {
            return new List<int>();
        }

        public override List<int> getCategories()
        {
            return new List<int>();
        }
    }
    #endregion

    #region Data Manager
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class DataManager : AveModule
    {
        private const string name = "Data Manager";

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_CONTROLPANEL_DATAMANAGER_ID; }
        }

        public override string Name
        {
            get { return name; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string>();
        }

        public override List<AveModule> getSubModules()
        {
            return new List<AveModule>();
        }

        public override List<int> getAllPlanTypes()
        {
            return new List<int>();
        }

        public override List<int> getAllJobTypes()
        {
            return new List<int>();
        }

        public override List<int> getCategories()
        {
            return new List<int>();
        }
    }
    #endregion

    #region Index Manager
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class IndexManager : AveModule
    {
        private const string name = "Index Manager";

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_CONTROLPANEL_INDEXMANAGER_ID; }
        }

        public override string Name
        {
            get { return name; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string>();
        }

        public override List<AveModule> getSubModules()
        {
            return new List<AveModule>();
        }

        public override List<int> getAllPlanTypes()
        {
            return new List<int>();
        }

        public override List<int> getAllJobTypes()
        {
            return new List<int>();
        }

        public override List<int> getCategories()
        {
            return new List<int>();
        }
    }
    #endregion

    #region Mapping Manager
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class MappingManager : AveModule
    {
        private const string name = "Mapping Manager";

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_CONTROLPANEL_MAPPINGMANAGER_ID; }
        }

        public override string Name
        {
            get { return name; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string>();
        }

        public override List<AveModule> getSubModules()
        {
            return new List<AveModule>();
        }

        public override List<int> getAllPlanTypes()
        {
            return new List<int>();
        }

        public override List<int> getAllJobTypes()
        {
            return new List<int>();
        }

        public override List<int> getCategories()
        {
            return new List<int>();
        }
    }
    #endregion

    #region Filter Policy
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class FilterPolicy : AveModule
    {
        private const string name = "Filter Policy";

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_CONTROLPANEL_FILTERPOLICY_ID; }
        }

        public override string Name
        {
            get { return name; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string>();
        }

        public override List<AveModule> getSubModules()
        {
            return new List<AveModule>();
        }

        public override List<int> getAllPlanTypes()
        {
            return new List<int>();
        }

        public override List<int> getAllJobTypes()
        {
            return new List<int>();
        }

        public override List<int> getCategories()
        {
            return new List<int>();
        }
    }
    #endregion

    #region Health Analyzer
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    public class HealthAnalyzer : AveModule
    {
        private const string name = "Health Analyzer";

        public const int type_health_analyzer = (int)JobTypes.HealthAnalyzer;

        public int JOB_TYPE_HEALTH_ANALYZER
        {
            get { return type_health_analyzer; }
        }

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_CONTROLPANEL_HEALTHANALYZER_ID; }
        }

        public override string Name
        {
            get { return name; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string>();
        }

        public override List<AveModule> getSubModules()
        {
            return new List<AveModule>();
        }

        public override List<int> getAllPlanTypes()
        {
            return new List<int>();
        }

        public override List<int> getAllJobTypes()
        {
            return new List<int>();
        }

        public override List<int> getCategories()
        {
            return new List<int>();
        }
    }
    #endregion
}