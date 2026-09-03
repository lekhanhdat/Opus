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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Attribute;

namespace AvePoint.GCommon.Contract.AveModuleContract
{
    /// <summary>
    /// Report Center负责人：包洪沨(Nick Bao)
    /// </summary>
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class ReportCenter : AveModuleContainer
    {
        private const string MODULE_TYPE_DOCAVE_REPORTCENTER_NAME = "Report Center";

        private readonly SubReportCenter subreportcenter = new SubReportCenter();

        public SubReportCenter SubReportCenter
        {
            get { return subreportcenter; }
        }

        public const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;

        public const string AGENT_TYPE_COMPLIANCE_REPORTS = AgentTypes.AGENT_TYPE_COMPLIANCE_REPORTS;

        private readonly int rc_collector_job_dto_type = (int)JobTypes.RCCollectorJob;

        public int RC_CELLECTOR_JOB_DTO_TYPE
        {
            get { return rc_collector_job_dto_type; }
        }

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ID; }
        }

        public override string Name
        {
            get { return MODULE_TYPE_DOCAVE_REPORTCENTER_NAME; }
        }
        #region ==RC License Module==
        private readonly RCUsage rcUsage = new RCUsage();
        public RCUsage RCUsage
        {
            get { return rcUsage; }
        }

        private readonly RCInfrastructure rcInfrastructure = new RCInfrastructure();
        public RCInfrastructure RCInfrastructure
        {
            get { return rcInfrastructure; }
        }

        private readonly RCStorageOptimization rcStorageOptimization = new RCStorageOptimization();
        public RCStorageOptimization RCStorageOptimization
        {
            get { return rcStorageOptimization; }
        }

        private readonly RCAdministration rcAdministration = new RCAdministration();
        public RCAdministration RCAdministration
        {
            get { return rcAdministration; }
        }
        private readonly RCSettings rcSettings = new RCSettings();
        public RCSettings RCSettings
        {
            get { return rcSettings; }
        }
        private readonly RCCustomize rcCustomize = new RCCustomize();
        public RCCustomize RCCustomize
        {
            get { return rcCustomize; }
        }

        private readonly RCAuditorReports rcAuditorReports = new RCAuditorReports();
        public RCAuditorReports RCAuditorReports
        {
            get { return rcAuditorReports; }
        }

        private readonly RCRealtimeMonidtoring rcRealtimeMonidtoring = new RCRealtimeMonidtoring();
        public RCRealtimeMonidtoring RCRealtimeMonidtoring
        {
            get { return rcRealtimeMonidtoring; }
        }

        private readonly RCActivityHistory rcActivityHistory = new RCActivityHistory();
        public RCActivityHistory RCActivityHistory
        {
            get { return rcActivityHistory; }
        }

        private readonly RCComplianceReports rcRCComplianceReports = new RCComplianceReports();
        public RCComplianceReports RCComplianceReports
        {
            get { return rcRCComplianceReports; }
        }

        private readonly RCDocAveReport rcdocavereport = new RCDocAveReport();
        public RCDocAveReport RCDocAveReport
        {
            get { return rcdocavereport; }
        }

        private readonly RCUsagePatternAlerting rcusagePatternAlerting = new RCUsagePatternAlerting();
        public RCUsagePatternAlerting RCUsagePatternAlerting
        {
            get { return rcusagePatternAlerting; }
        }
        
        #endregion

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            result.Add(RCUsage);
            result.Add(RCInfrastructure);
            result.Add(RCStorageOptimization);
            result.Add(RCCustomize);
            result.Add(RCAuditorReports);
            result.Add(RCRealtimeMonidtoring);
            result.Add(RCActivityHistory);
            result.Add(RCAdministration);
            result.Add(RCSettings);
            result.Add(RCComplianceReports);
            result.Add(RCDocAveReport);
            result.Add(RCUsagePatternAlerting);
            return result;
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_REPORT_CENTER);
            agentTypes.Add(AGENT_TYPE_COMPLIANCE_REPORTS);
            return agentTypes;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
            jobTypes.Add(RC_CELLECTOR_JOB_DTO_TYPE);
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class RCComplianceReports : AveModule
    {
        private readonly RCComplianceAuditorReports rccomplianceauditorreports = new RCComplianceAuditorReports();

        public RCComplianceAuditorReports RCComplianceAuditorReports
        {
            get { return rccomplianceauditorreports; }
        }

        private readonly RCO365ActivityReports rco365activityreports = new RCO365ActivityReports();
        
        public RCO365ActivityReports RCO365ActivityReports
        {
            get { return rco365activityreports; }
        }

        private readonly RCFileShareServerReports rcfileShareServerReports = new RCFileShareServerReports();

        public RCFileShareServerReports RCFileShareServerReports
        {
            get { return rcfileShareServerReports; }
        }

        private readonly RCTermStoreChanges rctermstorechanges = new RCTermStoreChanges();

        public RCTermStoreChanges RCTermStoreChanges
        {
            get { return rctermstorechanges; }
        }

        private readonly RCContentTypeUsage rccontenttypeusage = new RCContentTypeUsage();

        public RCContentTypeUsage RCContentTypeUsage
        {
            get { return rccontenttypeusage; }
        }
        private readonly RCInformationManagementPolicies rcInformationManagementPolicies = new RCInformationManagementPolicies();

        public RCInformationManagementPolicies RCInformationManagementPolicies
        {
            get { return rcInformationManagementPolicies; }
        }
        private readonly RCUpcomingContentExpiration rcUpcomingContentExpiration = new RCUpcomingContentExpiration();

        public RCUpcomingContentExpiration RCUpcomingContentExpiration
        {
            get { return rcUpcomingContentExpiration; }
        }
        private readonly RCBlogActivity rcblogactivity = new RCBlogActivity();

        public RCBlogActivity RCBlogActivity
        {
            get { return rcblogactivity; }
        }
       
        private const string MODULE_TYPE_DOCAVE_REPORTCENTER_NAME = "Compliance Reports";
        public const string AGENT_TYPE_COMPLIANCE_REPORTS = AgentTypes.AGENT_TYPE_COMPLIANCE_REPORTS;

        private readonly int rc_collector_job_dto_type = (int)JobTypes.RCCollectorJob;
        public int RC_CELLECTOR_JOB_DTO_TYPE
        {
            get { return rc_collector_job_dto_type; }
        }

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ID; }
        }

        public override string Name
        {
            get { return MODULE_TYPE_DOCAVE_REPORTCENTER_NAME; }
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            result.Add(RCComplianceAuditorReports);
            result.Add(RCO365ActivityReports);
            result.Add(RCTermStoreChanges);
            result.Add(RCContentTypeUsage);
            result.Add(RCInformationManagementPolicies);
            result.Add(RCUpcomingContentExpiration);
            result.Add(RCFileShareServerReports);
            //result.Add(RCBlogActivity);
            return result;
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_COMPLIANCE_REPORTS);
            return agentTypes;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.Available; }
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class RCDocAveReport : AveModule
    {
        private readonly RCDocAveTopology rcdocavetopology = new RCDocAveTopology();

        public RCDocAveTopology RCDocAveTopology
        {
            get { return rcdocavetopology; }
        }

        private readonly RCPerformanceMonitoring rcperformancemonitoring = new RCPerformanceMonitoring();

        public RCPerformanceMonitoring RCPerformanceMonitoring
        {
            get { return rcperformancemonitoring; }
        }

        private readonly RCDiskSpaceMonitoring rcdiskspacemonitoring = new RCDiskSpaceMonitoring();

        public RCDiskSpaceMonitoring RCDiskSpaceMonitoring
        {
            get { return rcdiskspacemonitoring; }
        }

        private readonly RCJobPerformanceMonitoring rcjobperformancemonitoring = new RCJobPerformanceMonitoring();

        public RCJobPerformanceMonitoring RCJobPerformanceMonitoring
        {
            get { return rcjobperformancemonitoring; }
        }

        private readonly RCDocAveAuditor rcdocaveauditor = new RCDocAveAuditor();

        public RCDocAveAuditor RCDocAveAuditor
        {
            get { return rcdocaveauditor; }
        }

        private const string MODULE_TYPE_DOCAVE_REPORTCENTER_NAME = "DocAve Report";

        public override string Name
        {
            get { return MODULE_TYPE_DOCAVE_REPORTCENTER_NAME; }
        }

        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ID; }
        }
        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            result.Add(RCDocAveTopology);
            result.Add(RCPerformanceMonitoring);
            result.Add(RCDiskSpaceMonitoring);
            result.Add(RCJobPerformanceMonitoring);
            result.Add(RCDocAveAuditor);
            return result;
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_REPORT_CENTER);
            return agentTypes;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }  

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class RCUsagePatternAlerting : AveModule
    {
        private readonly RCDocAveTopology rcdocavetopology = new RCDocAveTopology();

        public RCDocAveTopology RCDocAveTopology
        {
            get { return rcdocavetopology; }
        }

        private readonly RCPerformanceMonitoring rcperformancemonitoring = new RCPerformanceMonitoring();

        public RCPerformanceMonitoring RCPerformanceMonitoring
        {
            get { return rcperformancemonitoring; }
        }

        private readonly RCDiskSpaceMonitoring rcdiskspacemonitoring = new RCDiskSpaceMonitoring();

        public RCDiskSpaceMonitoring RCDiskSpaceMonitoring
        {
            get { return rcdiskspacemonitoring; }
        }

        private readonly RCJobPerformanceMonitoring rcjobperformancemonitoring = new RCJobPerformanceMonitoring();

        public RCJobPerformanceMonitoring RCJobPerformanceMonitoring
        {
            get { return rcjobperformancemonitoring; }
        }

        private readonly RCDocAveAuditor rcdocaveauditor = new RCDocAveAuditor();

        public RCDocAveAuditor RCDocAveAuditor
        {
            get { return rcdocaveauditor; }
        }

        private const string MODULE_TYPE_DOCAVE_REPORTCENTER_NAME = "Usage Pattern Alerting";

        public override string Name
        {
            get { return MODULE_TYPE_DOCAVE_REPORTCENTER_NAME; }
        }

        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ID; }
        }
        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            return result;
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_REPORT_CENTER);
            return agentTypes;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }
     
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class RCDocAveTopology : AveModule
    {
        private const string MODULE_TYPE_DOCAVE_REPORTCENTER_NAME = "DocAve Topology";
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ID; }
        }

        public override string Name
        {
            get { return MODULE_TYPE_DOCAVE_REPORTCENTER_NAME; }
        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class RCPerformanceMonitoring : AveModule
    {
        private const string MODULE_TYPE_DOCAVE_REPORTCENTER_NAME = "Performance Monitoring";
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ID; }
        }

        public override string Name
        {
            get { return MODULE_TYPE_DOCAVE_REPORTCENTER_NAME; }
        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class RCDiskSpaceMonitoring : AveModule
    {
        private const string MODULE_TYPE_DOCAVE_REPORTCENTER_NAME = "Disk Space Monitoring";
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ID; }
        }

        public override string Name
        {
            get { return MODULE_TYPE_DOCAVE_REPORTCENTER_NAME; }
        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class RCJobPerformanceMonitoring : AveModule
    {
        private const string MODULE_TYPE_DOCAVE_REPORTCENTER_NAME = "Job Performance Monitoring";
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ID; }
        }

        public override string Name
        {
            get { return MODULE_TYPE_DOCAVE_REPORTCENTER_NAME; }
        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class RCDocAveAuditor : AveModule
    {
        private const string MODULE_TYPE_DOCAVE_REPORTCENTER_NAME = "DocAve Auditor";
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ID; }
        }

        public override string Name
        {
            get { return MODULE_TYPE_DOCAVE_REPORTCENTER_NAME; }
        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class RCComplianceAuditorReports : AveModule
    {
        private readonly RCUserLifecycle rcuserlifecycle = new RCUserLifecycle();

        public RCUserLifecycle RCUserLifecycle
        {
            get { return rcuserlifecycle; }
        }
        private readonly RCSiteAccess rcsiteaccess = new RCSiteAccess();

        public RCSiteAccess RCSiteAccess
        {
            get { return rcsiteaccess; }
        }
        private readonly RCListAccess rclistaccess = new RCListAccess();

        public RCListAccess RCListAccess
        {
            get { return rclistaccess; }
        }
        private readonly RCListDeletion rclistdeletion = new RCListDeletion();

        public RCListDeletion RCListDeletion
        {
            get { return rclistdeletion; }
        }
        private readonly RCItemLifecycle rcitemlifecycle = new RCItemLifecycle();

        public RCItemLifecycle RCItemLifecycle
        {
            get { return rcitemlifecycle; }
        }
        private readonly RCPermissionChanges rcpermissionchanges = new RCPermissionChanges();

        public RCPermissionChanges RCPermissionChanges
        {
            get { return rcpermissionchanges; }
        }
        private readonly RCContentTypeChanges rccontenttypechanges = new RCContentTypeChanges();

        public RCContentTypeChanges RCContentTypeChanges
        {
            get { return rccontenttypechanges; }
        }

        private readonly RCCustomizedReport rccustomizedreport = new RCCustomizedReport();

        public RCCustomizedReport RCCustomizedReport
        {
            get { return rccustomizedreport; }
        }
        private const string MODULE_TYPE_DOCAVE_REPORTCENTER_NAME = "Auditor Reports";
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ID; }
        }

        public override string Name
        {
            get { return MODULE_TYPE_DOCAVE_REPORTCENTER_NAME; }
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            result.Add(RCUserLifecycle);
            result.Add(RCSiteAccess);
            result.Add(RCListAccess);
            result.Add(RCListDeletion);
            result.Add(RCItemLifecycle);
            result.Add(RCPermissionChanges);
            result.Add(RCContentTypeChanges);
            result.Add(RCCustomizedReport);
            return result;
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class RCUserLifecycle : AveModule
    {
        private const string MODULE_TYPE_DOCAVE_REPORTCENTER_NAME = "User Lifecycle";
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ID; }
        }

        public override string Name
        {
            get { return MODULE_TYPE_DOCAVE_REPORTCENTER_NAME; }
        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class RCSiteAccess : AveModule
    {
        private const string MODULE_TYPE_DOCAVE_REPORTCENTER_NAME = "Site Access";
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ID; }
        }

        public override string Name
        {
            get { return MODULE_TYPE_DOCAVE_REPORTCENTER_NAME; }
        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class RCListAccess : AveModule
    {
        private const string MODULE_TYPE_DOCAVE_REPORTCENTER_NAME = "List Access";
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ID; }
        }

        public override string Name
        {
            get { return MODULE_TYPE_DOCAVE_REPORTCENTER_NAME; }
        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class RCListDeletion : AveModule
    {
        private const string MODULE_TYPE_DOCAVE_REPORTCENTER_NAME = "List Deletion";
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ID; }
        }

        public override string Name
        {
            get { return MODULE_TYPE_DOCAVE_REPORTCENTER_NAME; }
        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class RCItemLifecycle : AveModule
    {
        private const string MODULE_TYPE_DOCAVE_REPORTCENTER_NAME = "Item Lifecycle";
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ID; }
        }

        public override string Name
        {
            get { return MODULE_TYPE_DOCAVE_REPORTCENTER_NAME; }
        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class RCPermissionChanges : AveModule
    {
        private const string MODULE_TYPE_DOCAVE_REPORTCENTER_NAME = "Permission Changes";
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ID; }
        }

        public override string Name
        {
            get { return MODULE_TYPE_DOCAVE_REPORTCENTER_NAME; }
        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class RCContentTypeChanges : AveModule
    {
        private const string MODULE_TYPE_DOCAVE_REPORTCENTER_NAME = "Content Type Changes";
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ID; }
        }

        public override string Name
        {
            get { return MODULE_TYPE_DOCAVE_REPORTCENTER_NAME; }
        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class RCCustomizedReport : AveModule
    {
        private const string MODULE_TYPE_DOCAVE_REPORTCENTER_NAME = "Customized Report";
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ID; }
        }

        public override string Name
        {
            get { return MODULE_TYPE_DOCAVE_REPORTCENTER_NAME; }
        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class RCTermStoreChanges : AveModule
    {
        private const string MODULE_TYPE_DOCAVE_REPORTCENTER_NAME = "Term Store Changes";
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ID; }
        }

        public override string Name
        {
            get { return MODULE_TYPE_DOCAVE_REPORTCENTER_NAME; }
        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }


    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class RCContentTypeUsage : AveModule
    {
        private const string MODULE_TYPE_DOCAVE_REPORTCENTER_NAME = "Content Type Usage";
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ID; }
        }

        public override string Name
        {
            get { return MODULE_TYPE_DOCAVE_REPORTCENTER_NAME; }
        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class RCO365ActivityReports : AveModule
    {
        private const string MODULE_TYPE_DOCAVE_REPORTCENTER_NAME = "Office 365 Activity Reports";
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ID; }
        }

        public override string Name
        {
            get { return MODULE_TYPE_DOCAVE_REPORTCENTER_NAME; }
        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class RCFileShareServerReports : AveModule
    {
        private const string MODULE_TYPE_DOCAVE_REPORTCENTER_NAME = "File Share Server Reports";
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ID; }
        }

        public override string Name
        {
            get { return MODULE_TYPE_DOCAVE_REPORTCENTER_NAME; }
        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class RCInformationManagementPolicies : AveModule
    {
        private const string MODULE_TYPE_DOCAVE_REPORTCENTER_NAME = "Information Management Policies";
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ID; }
        }

        public override string Name
        {
            get { return MODULE_TYPE_DOCAVE_REPORTCENTER_NAME; }
        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class RCUpcomingContentExpiration : AveModule
    {
        private const string MODULE_TYPE_DOCAVE_REPORTCENTER_NAME = "Upcoming Content Expiration";
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ID; }
        }

        public override string Name
        {
            get { return MODULE_TYPE_DOCAVE_REPORTCENTER_NAME; }
        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class RCBlogActivity : AveModule
    {
         private const string MODULE_TYPE_DOCAVE_REPORTCENTER_NAME = "Blog Activity";
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ID; }
        }

        public override string Name
        {
            get { return MODULE_TYPE_DOCAVE_REPORTCENTER_NAME; }
        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }
     
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SubReportCenter : AveModule
    {
        private const string MODULE_TYPE_DOCAVE_REPORTCENTER_NAME = "Report Center";
        public const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;

        private readonly int rc_collector_job_dto_type = (int)JobTypes.RCCollectorJob;
        public int RC_CELLECTOR_JOB_DTO_TYPE
        {
            get { return rc_collector_job_dto_type; }
        }

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ID; }
        }

        public override string Name
        {
            get { return MODULE_TYPE_DOCAVE_REPORTCENTER_NAME; }
        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_REPORT_CENTER);
            return agentTypes;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
            jobTypes.Add(RC_CELLECTOR_JOB_DTO_TYPE);
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            return null;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.Available; }
        }
    }
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCUsage : AveModule
    {
        private readonly RCSearchUsage rcsearchusage = new RCSearchUsage();

        public RCSearchUsage RCSearchUsage
        {
            get { return rcsearchusage; }
        }

        private readonly SiteVisitorsAndActivity sitevisitorsandactivity = new SiteVisitorsAndActivity();

        public SiteVisitorsAndActivity SiteVisitorsAndActivity
        {
            get { return sitevisitorsandactivity; }
        }

        private readonly CheckedOutDocuments checkedoutdocuments = new CheckedOutDocuments();

        public CheckedOutDocuments CheckedOutDocuments
        {
            get { return checkedoutdocuments; }
        }

        private readonly RCPageTraffic rcpagetraffic = new RCPageTraffic();

        public RCPageTraffic RCPageTraffic
        {
            get { return rcpagetraffic; }
        }

        private readonly Referrers referrers = new Referrers();

        public Referrers Referrers
        {
            get { return referrers; }
        }

        private readonly RCLastAccessedTime rclastaccessedtime = new RCLastAccessedTime();

        public RCLastAccessedTime RCLastAccessedTime
        {
            get { return rclastaccessedtime; }
        }

        private readonly RCFailedLoginAttempts rcfailedloginattempts = new RCFailedLoginAttempts();

        public RCFailedLoginAttempts RCFailedLoginAttempts
        {
            get { return rcfailedloginattempts; }
        }

        private readonly RCWorkflowStatus rcworkflowstatus = new RCWorkflowStatus();

        public RCWorkflowStatus RCWorkflowStatus
        {
            get { return rcworkflowstatus; }
        }

        private readonly RCSharePointAlerts rcsharepointalerts = new RCSharePointAlerts();

        public RCSharePointAlerts RCSharePointAlerts
        {
            get { return rcsharepointalerts; }
        }

        private readonly RCDownloadRanking rcdownloadranking = new RCDownloadRanking();

        public RCDownloadRanking RCDownloadRanking
        {
            get { return rcdownloadranking; }
        }

        private readonly SiteActivityRanking siteactivityranking = new SiteActivityRanking();

        public SiteActivityRanking SiteActivityRanking
        {
            get { return siteactivityranking; }
        }

        private readonly ActiveUser activeuser = new ActiveUser();

        public ActiveUser ActiveUser
        {
            get { return activeuser; }
        }
        private const string MOUDEL_NAME = "Usage";

        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_COMPLIANCE_REPORTS;

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_USAGE_ID; }
        }

        public override string Name
        {
            get { return MOUDEL_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string> { AGENT_TYPE_REPORT_CENTER };
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            result.Add(RCSearchUsage);
            result.Add(SiteVisitorsAndActivity);
            result.Add(CheckedOutDocuments);
            result.Add(RCPageTraffic);
            result.Add(Referrers);
            result.Add(RCLastAccessedTime);
            result.Add(RCFailedLoginAttempts);
            result.Add(RCWorkflowStatus);
            result.Add(RCSharePointAlerts);
            result.Add(RCDownloadRanking);
            result.Add(SiteActivityRanking);
            result.Add(ActiveUser);
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCSearchUsage : AveModule
    {
        private const string MOUDEL_NAME = "Search Usage";
       
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_USAGE_ID; }
        }

        public override string Name
        {
            get { return MOUDEL_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SiteVisitorsAndActivity : AveModule
    {
        private const string MOUDEL_NAME = "Site Visitors and Activity";
       
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_USAGE_ID; }
        }

        public override string Name
        {
            get { return MOUDEL_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CheckedOutDocuments: AveModule
    {
        private const string MOUDEL_NAME = "Checked-Out Documents";
       
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_USAGE_ID; }
        }

        public override string Name
        {
            get { return MOUDEL_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCPageTraffic : AveModule
    {
        private const string MOUDEL_NAME = "Page Traffic";
      
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_USAGE_ID; }
        }

        public override string Name
        {
            get { return MOUDEL_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Referrers : AveModule
    {
        private const string MOUDEL_NAME = "Referrers";
       
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_USAGE_ID; }
        }

        public override string Name
        {
            get { return MOUDEL_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCLastAccessedTime : AveModule
    {
        private const string MOUDEL_NAME = "Last Accessed Time";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_COMPLIANCE_REPORTS;

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_USAGE_ID; }
        }

        public override string Name
        {
            get { return MOUDEL_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCFailedLoginAttempts : AveModule
    {
        private const string MOUDEL_NAME = "Failed Login Attempts";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_COMPLIANCE_REPORTS;

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_USAGE_ID; }
        }

        public override string Name
        {
            get { return MOUDEL_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCWorkflowStatus : AveModule
    {
        private const string MOUDEL_NAME = "Workflow Status";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_COMPLIANCE_REPORTS;

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_USAGE_ID; }
        }

        public override string Name
        {
            get { return MOUDEL_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCSharePointAlerts : AveModule
    {
        private const string MOUDEL_NAME = "SharePoint Alerts";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_COMPLIANCE_REPORTS;

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_USAGE_ID; }
        }

        public override string Name
        {
            get { return MOUDEL_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCDownloadRanking : AveModule
    {
        private const string MOUDEL_NAME = "Download Ranking";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_COMPLIANCE_REPORTS;

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_USAGE_ID; }
        }

        public override string Name
        {
            get { return MOUDEL_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SiteActivityRanking : AveModule
    {
        private const string MOUDEL_NAME = "Site Activity Ranking";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_COMPLIANCE_REPORTS;

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_USAGE_ID; }
        }

        public override string Name
        {
            get { return MOUDEL_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ActiveUser : AveModule
    {
        private const string MOUDEL_NAME = "Active User";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_COMPLIANCE_REPORTS;

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_USAGE_ID; }
        }

        public override string Name
        {
            get { return MOUDEL_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCInfrastructure : AveModule
    {
        private readonly Infrastructure infrastructure = new Infrastructure();

        public Infrastructure Infrastructure
        {
            get { return infrastructure; }
        }

        private readonly IRStorageOptimization irstorageoptimization = new IRStorageOptimization();

        public IRStorageOptimization IRStorageOptimization
        {
            get { return irstorageoptimization; }
        }

        private const string MODULE_NAME = "Infrastructure";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_INFRASTRUCTURE_ID; }
        }

        public override string Name
        {
            get { return MODULE_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string> { AGENT_TYPE_REPORT_CENTER };
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            result.Add(Infrastructure);
            result.Add(IRStorageOptimization);
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }


    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Infrastructure : AveModule
    {
        private readonly RCSharePointServices rcsharepointservices = new RCSharePointServices();

        public RCSharePointServices RCSharePointServices
        {
            get { return rcsharepointservices; }
        }

        private readonly CPUORMemoryUsage cpuormemoryusage = new CPUORMemoryUsage();

        public CPUORMemoryUsage CPUORMemoryUsage
        {
            get { return cpuormemoryusage; }
        }

        private readonly RCNetworking rcnetworking = new RCNetworking();

        public RCNetworking RCNetworking
        {
            get { return rcnetworking; }
        }

        private readonly RCSharePointTopology rcsharepointtopology = new RCSharePointTopology();

        public RCSharePointTopology RCSharePointTopology
        {
            get { return rcsharepointtopology; }
        }

        private readonly SharePointSearchService sharepointsearchservice = new SharePointSearchService();

        public SharePointSearchService SharePointSearchService
        {
            get { return sharepointsearchservice; }
        }

        private readonly RCEnvironmentSearch rcenvironmentsearch = new RCEnvironmentSearch();

        public RCEnvironmentSearch RCEnvironmentSearch
        {
            get { return rcenvironmentsearch; }
        }

        private readonly SiteCollectionComparison sitecollectioncomparison = new SiteCollectionComparison();

        public SiteCollectionComparison SiteCollectionComparison
        {
            get { return sitecollectioncomparison; }
        }

        private readonly SiteCollectionLoadTime sitecollectionloadtime = new SiteCollectionLoadTime();

        public SiteCollectionLoadTime SiteCollectionLoadTime
        {
            get { return sitecollectionloadtime; }
        }

        private const string MOUDEL_NAME = "RCInfrastructure";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_INFRASTRUCTURE_ID; }
        }

        public override string Name
        {
            get { return MOUDEL_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string> { AGENT_TYPE_REPORT_CENTER };
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            result.Add(RCSharePointServices);
            result.Add(CPUORMemoryUsage);
            result.Add(RCNetworking);
            result.Add(RCSharePointTopology);
            result.Add(SharePointSearchService);
            result.Add(RCEnvironmentSearch);
            result.Add(SiteCollectionComparison);
            result.Add(SiteCollectionLoadTime);
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCSharePointServices : AveModule
    {
        private const string MOUDEL_NAME = "SharePoint Services";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_INFRASTRUCTURE_ID; }
        }

        public override string Name
        {
            get { return MOUDEL_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string> { AGENT_TYPE_REPORT_CENTER };
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CPUORMemoryUsage : AveModule
    {
        private const string MOUDEL_NAME = "CPU/Memory Usage";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_INFRASTRUCTURE_ID; }
        }

        public override string Name
        {
            get { return MOUDEL_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string> { AGENT_TYPE_REPORT_CENTER };
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCNetworking : AveModule
    {
        private const string MOUDEL_NAME = "Networking";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_INFRASTRUCTURE_ID; }
        }

        public override string Name
        {
            get { return MOUDEL_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string> { AGENT_TYPE_REPORT_CENTER };
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCSharePointTopology : AveModule
    {
        private const string MOUDEL_NAME = "SharePoint Topology";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_INFRASTRUCTURE_ID; }
        }

        public override string Name
        {
            get { return MOUDEL_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string> { AGENT_TYPE_REPORT_CENTER };
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SharePointSearchService : AveModule
    {
        private const string MOUDEL_NAME = "SharePoint Search Service";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_INFRASTRUCTURE_ID; }
        }

        public override string Name
        {
            get { return MOUDEL_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string> { AGENT_TYPE_REPORT_CENTER };
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCEnvironmentSearch : AveModule
    {
        private const string MOUDEL_NAME = "Environment Search";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_INFRASTRUCTURE_ID; }
        }

        public override string Name
        {
            get { return MOUDEL_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string> { AGENT_TYPE_REPORT_CENTER };
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SiteCollectionComparison : AveModule
    {
        private const string MOUDEL_NAME = "Site Collection Comparison";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_INFRASTRUCTURE_ID; }
        }

        public override string Name
        {
            get { return MOUDEL_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string> { AGENT_TYPE_REPORT_CENTER };
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SiteCollectionLoadTime : AveModule
    {
        private const string MOUDEL_NAME = "Site Collection Load Time";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_INFRASTRUCTURE_ID; }
        }

        public override string Name
        {
            get { return MOUDEL_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string> { AGENT_TYPE_REPORT_CENTER };
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class IRStorageOptimization : AveModule
    {
        private readonly RCStorageTrends rcstoragetrends = new RCStorageTrends();

        public RCStorageTrends RCStorageTrends
        {
            get { return rcstoragetrends; }
        }
        private readonly StorageAnalyzer storageanalyzer = new StorageAnalyzer();

        public StorageAnalyzer StorageAnalyzer
        {
            get { return storageanalyzer; }
        }
        private readonly RCUserStorageSize rcuserstoragesize = new RCUserStorageSize();

        public RCUserStorageSize RCUserStorageSize
        {
            get { return rcuserstoragesize; }
        }
        private const string MOUDEL_NAME = "IRStorage Optimization";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_INFRASTRUCTURE_ID; }
        }

        public override string Name
        {
            get { return MOUDEL_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string> { AGENT_TYPE_REPORT_CENTER };
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            result.Add(RCStorageTrends);
            result.Add(StorageAnalyzer);
            result.Add(RCUserStorageSize);
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCStorageTrends : AveModule
    {
        private const string MOUDEL_NAME = "Storage Trends";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_INFRASTRUCTURE_ID; }
        }

        public override string Name
        {
            get { return MOUDEL_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string> { AGENT_TYPE_REPORT_CENTER };
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class StorageAnalyzer : AveModule
    {
        private const string MOUDEL_NAME = "Storage Analyzer";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_INFRASTRUCTURE_ID; }
        }

        public override string Name
        {
            get { return MOUDEL_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string> { AGENT_TYPE_REPORT_CENTER };
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCUserStorageSize : AveModule
    {
        private const string MOUDEL_NAME = "User Storage Size";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_INFRASTRUCTURE_ID; }
        }

        public override string Name
        {
            get { return MOUDEL_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string> { AGENT_TYPE_REPORT_CENTER };
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCStorageOptimization : AveModule
    {
        private const string MODULE_NAME = "Storage Optimization";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_STORAGEOPTIMIZATION_ID; }
        }

        public override string Name
        {
            get { return MODULE_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string> { AGENT_TYPE_REPORT_CENTER };
        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCSettings : AveModule
    {
        private readonly RCAuditPruning rcauditpruning = new RCAuditPruning();

        public RCAuditPruning RCAuditPruning
        {
            get { return rcauditpruning; }
        }

        private readonly RCManageFeature rcmanagefeature = new RCManageFeature();

        public RCManageFeature RCManageFeature
        {
            get { return rcmanagefeature; }
        }

        private readonly RCAuditController rcauditcontroller = new RCAuditController();

        public RCAuditController RCAuditController
        {
            get { return rcauditcontroller; }
        }

        private readonly RCDataCollection rcdatacollection = new RCDataCollection();

        public RCDataCollection RCDataCollection
        {
            get { return rcdatacollection; }
        }

        private readonly RCIISLogging rciislogging = new RCIISLogging();

        public RCIISLogging RCIISLogging
        {
            get { return rciislogging; }
        }

        private readonly RCReportingService rcreportingservice = new RCReportingService();

        public RCReportingService RCReportingService
        {
            get { return rcreportingservice; }
        }
        private readonly RCCrossFarmServiceConfiguration rccrossfarmserviceconfiguration = new RCCrossFarmServiceConfiguration();

        public RCCrossFarmServiceConfiguration RCCrossFarmServiceConfiguration
        {
            get { return rccrossfarmserviceconfiguration; }
        }

        private readonly RCSTExportLocation rcstexportlocation = new RCSTExportLocation();

        public RCSTExportLocation RCSTExportLocation
        {
            get { return rcstexportlocation; }
        }

        private readonly ActivityHistoryPruning activityHistoryPruning = new ActivityHistoryPruning();

        public ActivityHistoryPruning ActivityHistoryPruning
        {
            get { return activityHistoryPruning; }
        }

        private readonly RCScopeFilter rcscopeFilter = new RCScopeFilter();

        public RCScopeFilter RCScopeFilter
        {
            get { return rcscopeFilter; }
        }

        private readonly RCSharedLocation rcSharedLocation = new RCSharedLocation();

        public RCSharedLocation RCSharedLocation
        {
            get { return rcSharedLocation; }
        }
        private readonly RCItemCacheService rcItemCacheService = new RCItemCacheService();

        public RCItemCacheService RCItemCacheService
        {
            get { return rcItemCacheService; }
        }

        private readonly RCUsageActivityWebpartSettings rcUsageActivityWebpartSettings = new RCUsageActivityWebpartSettings();

        public RCUsageActivityWebpartSettings RCUsageActivityWebpartSettings
        {
            get { return rcUsageActivityWebpartSettings; }
        } 
        private const string MODULE_NAME = "Settings";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ADMINISTRATION_ID; }
        }

        public override string Name
        {
            get { return MODULE_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string> { AGENT_TYPE_REPORT_CENTER };
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            result.Add(RCAuditPruning);
            result.Add(RCAuditController);
            result.Add(RCManageFeature);
            result.Add(RCDataCollection);
            result.Add(RCIISLogging);
            result.Add(RCReportingService);
            result.Add(RCCrossFarmServiceConfiguration);
            result.Add(RCSTExportLocation);
            result.Add(ActivityHistoryPruning);
            result.Add(RCScopeFilter);
            result.Add(RCSharedLocation);
            result.Add(RCItemCacheService);
            result.Add(RCUsageActivityWebpartSettings);
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCAuditPruning : AveModule
    {
        private const string MODULE_NAME = "Audit Pruning";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ADMINISTRATION_ID; }
        }

        public override string Name
        {
            get { return MODULE_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string> { AGENT_TYPE_REPORT_CENTER };
        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;                                                                                   
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCManageFeature : AveModule
    {
        private const string MODULE_NAME = "Manage Feature";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ADMINISTRATION_ID; }
        }

        public override string Name
        {
            get { return MODULE_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string> { AGENT_TYPE_REPORT_CENTER };
        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCAuditController : AveModule
    {
        private const string MODULE_NAME = "Audit Controller";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ADMINISTRATION_ID; }
        }

        public override string Name
        {
            get { return MODULE_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string> { AGENT_TYPE_REPORT_CENTER };
        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class RCAdministration : AveModule
    {
        private const string MODULE_NAME = "Administrator Reports";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;
        private readonly RCForAdministration rcforadministration = new RCForAdministration();
        public RCForAdministration RCForAdministration
        {
            get { return rcforadministration; }
        }
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ADMINISTRATION_ID; }
        }

        public override string Name
        {
            get { return MODULE_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.Available; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string> { AGENT_TYPE_REPORT_CENTER };
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            result.Add(RCForAdministration);
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCForAdministration : AveModule
    {
        private const string MOUDEL_NAME = "RC_Administration";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;
        private readonly RCConfigurationReports rcconfigurationreports = new RCConfigurationReports();
        public RCConfigurationReports RCConfigurationReports
        {
            get { return rcconfigurationreports; }
        }
        private readonly RCBestPracticeReports rcbestpracticereports = new RCBestPracticeReports();
        public RCBestPracticeReports RCBestPracticeReports
        {
            get { return rcbestpracticereports; }
        }
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ADMINISTRATION_ID; }
        }

        public override string Name
        {
            get { return MOUDEL_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string> { AGENT_TYPE_REPORT_CENTER };
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            result.Add(RCConfigurationReports);
            result.Add(RCBestPracticeReports);
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCDataCollection : AveModule
    {
        private const string MODULE_NAME = "Data Collection";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ADMINISTRATION_ID; }
        }

        public override string Name
        {
            get { return MODULE_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string> { AGENT_TYPE_REPORT_CENTER };
        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCIISLogging : AveModule
    {
        private const string MODULE_NAME = "IIS Logging";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ADMINISTRATION_ID; }
        }

        public override string Name
        {
            get { return MODULE_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string> { AGENT_TYPE_REPORT_CENTER };
        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCReportingService : AveModule
    {
        private const string MODULE_NAME = "Reporting Service";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ADMINISTRATION_ID; }
        }

        public override string Name
        {
            get { return MODULE_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string> { AGENT_TYPE_REPORT_CENTER };
        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class RCCrossFarmServiceConfiguration : AveModule
    {
        private const string MODULE_NAME = "Cross Farm Service Configuration";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;
        private readonly RCForAdministration rcforadministration = new RCForAdministration();
        public RCForAdministration RCForAdministration
        {
            get { return rcforadministration; }
        }
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ADMINISTRATION_ID; }
        }

        public override string Name
        {
            get { return MODULE_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.Available; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string> { AGENT_TYPE_REPORT_CENTER };
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCSTExportLocation : AveModule
    {
        private const string MOUDEL_NAME = "RC Export Location";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;
        private readonly RCConfigurationReports rcconfigurationreports = new RCConfigurationReports();
        public RCConfigurationReports RCConfigurationReports
        {
            get { return rcconfigurationreports; }
        }
        private readonly RCBestPracticeReports rcbestpracticereports = new RCBestPracticeReports();
        public RCBestPracticeReports RCBestPracticeReports
        {
            get { return rcbestpracticereports; }
        }
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ADMINISTRATION_ID; }
        }

        public override string Name
        {
            get { return MOUDEL_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string> { AGENT_TYPE_REPORT_CENTER };
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ActivityHistoryPruning : AveModule
    {
        private const string MOUDEL_NAME = "Activity History Pruning";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ADMINISTRATION_ID; }
        }

        public override string Name
        {
            get { return MOUDEL_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string> { AGENT_TYPE_REPORT_CENTER };
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCScopeFilter : AveModule
    {
        private const string MOUDEL_NAME = "Scope Filter";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ADMINISTRATION_ID; }
        }

        public override string Name
        {
            get { return MOUDEL_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string> { AGENT_TYPE_REPORT_CENTER };
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCSharedLocation : AveModule
    {
        private const string MOUDEL_NAME = "Shared Location";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ADMINISTRATION_ID; }
        }

        public override string Name
        {
            get { return MOUDEL_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string> { AGENT_TYPE_REPORT_CENTER };
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCItemCacheService : AveModule
    {
         private const string MOUDEL_NAME = "Item Cache Service";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ADMINISTRATION_ID; }
        }

        public override string Name
        {
            get { return MOUDEL_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string> { AGENT_TYPE_REPORT_CENTER };
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCUsageActivityWebpartSettings : AveModule
    {
        private const string MOUDEL_NAME = "Usage Activity Webpart Settings";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;
        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ADMINISTRATION_ID; }
        }

        public override string Name
        {
            get { return MOUDEL_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string> { AGENT_TYPE_REPORT_CENTER };
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCConfigurationReports : AveModule
    {
        private const string MOUDEL_NAME = "Configuration Reports";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ADMINISTRATION_ID; }
        }

        public override string Name
        {
            get { return MOUDEL_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string> { AGENT_TYPE_REPORT_CENTER };
        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCBestPracticeReports : AveModule
    {
        private const string MOUDEL_NAME = "Best Practice Reports";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_ADMINISTRATION_ID; }
        }

        public override string Name
        {
            get { return MOUDEL_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string> { AGENT_TYPE_REPORT_CENTER };
        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCCustomize : AveModule
    {
        private const string MODULE_NAME = "Customize";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_CUSTOMIZE_ID; }
        }

        public override string Name
        {
            get { return MODULE_NAME; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string> { AGENT_TYPE_REPORT_CENTER };
        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCAuditorReports : AveModule
    {
        private const string MODULE_NAME = "Auditor Reports";

        public const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_AUDITORREPORTS_ID; }
        }

        public override string Name
        {
            get { return MODULE_NAME; }
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_REPORT_CENTER);
            return agentTypes;
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();

            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCRealtimeMonidtoring : AveModule
    {
        private const string MODULE_NAME = "Real-time Monitoring";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_REALTIMEMONITORING_ID; }
        }

        public override string Name
        {
            get { return MODULE_NAME; }
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_REPORT_CENTER);
            return agentTypes;
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();

            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCActivityHistory : AveModule
    {
        private const string MODULE_NAME = "Activity History";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_REALTIMEMONITORING_ID; }
        }

        public override string Name
        {
            get { return MODULE_NAME; }
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_REPORT_CENTER);
            return agentTypes;
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();

            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }
}
