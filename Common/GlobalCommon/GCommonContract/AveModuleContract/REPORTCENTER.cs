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



using System.Collections.Generic;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Attribute;

namespace AvePoint.GCommon.Contract.AveModuleContract
{
    /// <summary>
    /// Report Center负责人：包洪沨(Nick Bao)
    /// </summary>
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
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

        private readonly RCComplianceReports rcRCComplianceReports = new RCComplianceReports();
        public RCComplianceReports RCComplianceReports
        {
            get { return rcRCComplianceReports; }
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

        private readonly RCDocAveReport rcdocavereport = new RCDocAveReport();
        public RCDocAveReport RCDocAveReport
        {
            get { return rcdocavereport; }
        }

        private readonly RCUsageReport rcUsageReport = new RCUsageReport();
        public RCUsageReport RCUsageReport
        {
            get { return rcUsageReport; }
        }
        #endregion

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            //result.Add(RCUsage);
            //result.Add(RCInfrastructure);
            //result.Add(RCStorageOptimization);
            //result.Add(RCCustomize);
            //result.Add(RCAuditorReports);
            //result.Add(RCRealtimeMonidtoring);
            //result.Add(RCActivityHistory);

            result.Add(SubReportCenter);
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

    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class RCDocAveReport : AveModule
    {
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
            //List<AveModule> result = new List<AveModule>();
            //result.Add(RCDocAveAuditor);
            //return result;
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

    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
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

    public class RCComplianceReports : AveModule
    {
        private const string MODULE_TYPE_DOCAVE_REPORTCENTER_NAME = "Compliance Reports";
        public const string AGENT_TYPE_COMPLIANCE_REPORTS = AgentTypes.AGENT_TYPE_COMPLIANCE_REPORTS;

        private readonly int rc_collector_job_dto_type = (int)JobTypes.RCCollectorJob;
        public int RC_CELLECTOR_JOB_DTO_TYPE
        {
            get { return rc_collector_job_dto_type; }
        }

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

    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
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
            //List<AveModule> result = new List<AveModule>();
            //result.Add(RCUserLifecycle);
            //result.Add(RCSiteAccess);
            //result.Add(RCListAccess);
            //result.Add(RCListDeletion);
            //result.Add(RCItemLifecycle);
            //result.Add(RCPermissionChanges);
            //result.Add(RCContentTypeChanges);
            //result.Add(RCCustomizedReport);
            //return result;
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

    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
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

    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
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

    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
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

    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
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

    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
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

    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
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

    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
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

    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
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

    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
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

        private readonly RCComplianceReports rcRCComplianceReports = new RCComplianceReports();
        public RCComplianceReports RCComplianceReports
        {
            get { return rcRCComplianceReports; }
        }

        private readonly RCUsageReport rcUsageReports = new RCUsageReport();
        public RCUsageReport RCUsageReport
        {
            get { return rcUsageReports; }
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
            result.Add(RCAdministration);
            result.Add(RCComplianceReports);
            result.Add(RCUsageReport);
            result.Add(RCSettings);
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
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCUsage : AveModule
    {
        private const string MOUDEL_NAME = "Usage";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_COMPLIANCE_REPORTS;

        private readonly SiteVisitorsAndActivity sitevisitorsandactivity = new SiteVisitorsAndActivity();

        public SiteVisitorsAndActivity SiteVisitorsAndActivity
        {
            get { return sitevisitorsandactivity; }
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
            result.Add(SiteVisitorsAndActivity);
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

    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SiteActivityRanking : AveModule
    {
        private const string MOUDEL_NAME = "Site Activity Ranking";
        

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

    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ActiveUser : AveModule
    {
        private const string MOUDEL_NAME = "Active User";


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

    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
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

    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCInfrastructure : AveModule
    {
        private const string MODULE_NAME = "Infrastructure";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;

        private readonly IRStorageOptimization irstorageoptimization = new IRStorageOptimization();

        public IRStorageOptimization IRStorageOptimization
        {
            get { return irstorageoptimization; }
        }

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

    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class IRStorageOptimization : AveModule
    {
        private readonly RCStorageTrends rcstoragetrends = new RCStorageTrends();

        public RCStorageTrends RCStorageTrends
        {
            get { return rcstoragetrends; }
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

    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
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

    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
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

    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
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

    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCSettings : AveModule
    {
        private readonly RCAuditPruning rcauditpruning = new RCAuditPruning();

        public RCAuditPruning RCAuditPruning
        {
            get { return rcauditpruning; }
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

        private readonly RCSTExportLocation rcstexportlocation = new RCSTExportLocation();

        public RCSTExportLocation RCSTExportLocation
        {
            get { return rcstexportlocation; }
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
            //List<AveModule> result = new List<AveModule>();
            //result.Add(RCAuditPruning);
            //result.Add(RCAuditController);
            //result.Add(RCManageFeature);
            //result.Add(RCDataCollection);
            //result.Add(RCSTExportLocation);
            //return result;
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

    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCSTExportLocation : AveModule
    {
        private const string MOUDEL_NAME = "RC Export Location";
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

    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
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

    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
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

    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
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
            //List<AveModule> result = new List<AveModule>();
            //result.Add(RCForAdministration);
            //return result;
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

    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
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

    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
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

    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
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

    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCUsageReport : AveModule
    {
        private const string MODULE_NAME = "Usage Reports";
        private const string AGENT_TYPE_REPORT_CENTER = AgentTypes.AGENT_TYPE_REPORT_CENTER;

        //private readonly RCUsageReport rcforusagereport = new RCUsageReport();
        //public RCUsageReport RCForAdministration
        //{
        //    get { return rcforusagereport; }
        //}

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPORTCENTER_USAGE_ID; }
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

    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
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
