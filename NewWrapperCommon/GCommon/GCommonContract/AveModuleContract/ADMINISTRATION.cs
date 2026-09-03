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
using AvePoint.GCommon.Contract.AveModuleContract.SubAdministrator;
using System.Diagnostics.CodeAnalysis;

[module: SuppressMessage("Microsoft.Naming", "CA1708:IdentifiersShouldDifferByMoreThanCase", Scope = "type", Target = "AvePoint.GCommon.Contract.AveModuleContract.Replicator")]
namespace AvePoint.GCommon.Contract.AveModuleContract
{
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class Administration : AveModuleContainer
    {
        private const string MODULE_TYPE_DOCAVE_ADMINISTRATION_NAME = "Administration";

        private readonly CentralAdmin centraladmin = new CentralAdmin();
        public CentralAdmin CentralAdmin
        {
            get { return centraladmin; }
        }
        private readonly ContentManager contentmanager = new ContentManager();

        public ContentManager ContentManager
        {
            get { return contentmanager; }
        }
        private readonly Replicator replicator = new Replicator();

        public Replicator Replicator
        {
            get { return replicator; }
        }
        private readonly DeploymentManager deploymentmanager = new DeploymentManager();

        public DeploymentManager DeploymentManager
        {
            get { return deploymentmanager; }
        }

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_ADMINISTRATION_ID; }
        }

        public override string Name
        {
            get { return MODULE_TYPE_DOCAVE_ADMINISTRATION_NAME; }
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            result.Add(CentralAdmin);
            result.Add(ContentManager);
            result.Add(Replicator);
            result.Add(DeploymentManager);
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
    }

    /// <summary>
    /// Central Admin模块，由秦汉负责
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class CentralAdmin : AveModule
    {

        private readonly AdministratorPolicyEnforcer administratorpolicyenforcer = new AdministratorPolicyEnforcer();

        public AdministratorPolicyEnforcer AdministratorPolicyEnforcer
        {
            get { return administratorpolicyenforcer; }
        }
        private readonly FarmLevel farmlevel = new FarmLevel();

        public FarmLevel FarmLevel
        {
            get { return farmlevel; }
        }
        private readonly FolderLevel folderlevel = new FolderLevel();

        public FolderLevel FolderLevel
        {
            get { return folderlevel; }
        }
        private readonly ItemLevel itemlevel = new ItemLevel();

        public ItemLevel ItemLevel
        {
            get { return itemlevel; }
        }
        private readonly ListLibraryLevel listlibrarylevel = new ListLibraryLevel();

        public ListLibraryLevel ListLibraryLevel
        {
            get { return listlibrarylevel; }
        }
        private readonly SiteCollectionLevel sitecollectionlevel = new SiteCollectionLevel();

        public SiteCollectionLevel SiteCollectionLevel
        {
            get { return sitecollectionlevel; }
        }
        private readonly SiteLevel sitelevel = new SiteLevel();

        public SiteLevel SiteLevel
        {
            get { return sitelevel; }
        }
        private readonly WebApplicationLevel webapplicationlevel = new WebApplicationLevel();

        public WebApplicationLevel WebApplicationLevel
        {
            get { return webapplicationlevel; }
        }
        private readonly ItemDocumentVersionLevel itemdocumentversionlevel = new ItemDocumentVersionLevel();

        public ItemDocumentVersionLevel ItemDocumentVersionLevel
        {
            get { return itemdocumentversionlevel; }
        }

        private const string name = "Administrator";
        #region agentType
        public const string AGENT_TYPE_SMS = AgentTypes.AGENT_TYPE_SMS;

        #endregion
        #region category
        private readonly int centralAdmin = 2;

        //public int CentralAdmin
        //{
        //    get { return centralAdmin; }
        //} 

        #endregion
        #region planType
        private readonly int ca_plan_deleted = -1;

        public int CA_PLAN_DELETED
        {
            get { return ca_plan_deleted; }
        }

        private readonly int ca_admin_search = 1;

        public int CA_ADMIN_SEARCH
        {
            get { return ca_admin_search; }
        }

        private readonly int ca_admin_search_anonymous = 10;

        public int CA_ADMIN_SEARCH_ANONYMOUS
        {
            get { return ca_admin_search_anonymous; }
        }

        private readonly int ca_security_search = 2;

        public int CA_SECURITY_SEARCH
        {
            get { return ca_security_search; }
        }

        private readonly int ca_security_search_anonymous = 20;

        public int CA_SECURITY_SEARCH_ANONYMOUS
        {
            get { return ca_security_search_anonymous; }
        }

        private readonly int create_site_collection = 3;

        public int CREATE_SITE_COLLECTION
        {
            get { return create_site_collection; }
        }

        private readonly int create_site_collection_anonymous = 30;

        public int CREATE_SITE_COLLECTION_ANONYMOUS
        {
            get { return create_site_collection_anonymous; }
        }

        private readonly int create_web_application = 4;

        public int CREATE_WEB_APPLICATION
        {
            get { return create_web_application; }
        }

        private readonly int create_web_application_anonymous = 40;

        public int CREATE_WEB_APPLICATION_ANONYMOUS
        {
            get { return create_web_application_anonymous; }
        }

        #endregion
        #region jobType
        private readonly int ca_search_job_dto_type = (int)JobTypes.CASearchJob;

        public int CA_SEARCH_JOB_DTO_TYPE
        {
            get { return ca_search_job_dto_type; }
        }

        private readonly int ca_job_dto_type = (int)JobTypes.CAJob;

        public int CA_JOB_DTO_TYPE
        {
            get { return ca_job_dto_type; }
        }

        #endregion


        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_SMS);
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
                return name;
            }

        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            result.Add(AdministratorPolicyEnforcer);
            result.Add(FarmLevel);
            result.Add(WebApplicationLevel);
            result.Add(SiteCollectionLevel);
            result.Add(SiteLevel);
            result.Add(ListLibraryLevel);
            result.Add(FolderLevel);
            result.Add(ItemLevel);
            result.Add(ItemDocumentVersionLevel);
            return result;
        }


        public override List<int> getAllPlanTypes()
        {
            List<int> planTypes = new List<int>();
            planTypes.Add(CA_PLAN_DELETED);
            planTypes.Add(CA_ADMIN_SEARCH);
            planTypes.Add(CA_ADMIN_SEARCH_ANONYMOUS);
            planTypes.Add(CA_SECURITY_SEARCH);
            planTypes.Add(CA_SECURITY_SEARCH_ANONYMOUS);
            planTypes.Add(CREATE_SITE_COLLECTION);
            planTypes.Add(CREATE_SITE_COLLECTION_ANONYMOUS);
            planTypes.Add(CREATE_WEB_APPLICATION);
            planTypes.Add(CREATE_WEB_APPLICATION_ANONYMOUS);
            return planTypes;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
            jobTypes.Add(CA_SEARCH_JOB_DTO_TYPE);
            jobTypes.Add(CA_JOB_DTO_TYPE);
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            List<int> categories = new List<int>();
            categories.Add(centralAdmin);
            return categories;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.Available; }
        }
    }

    /// <summary>
    /// Administrator Policy Enforcer
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class AdministratorPolicyEnforcer : AveModule
    {

        public const string module_name = "Administrator Policy Enforcer";

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
    /// Content Manager模块，由李光伟负责
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class ContentManager : AveModule
    {
        private const string name = "Content Manager";
        #region agentType
        public const string AGENT_TYPE_CONTENT_MANAGER2010 = AgentTypes.AGENT_TYPE_CONTENT_MANAGER2010;          //17179869184L



        public const string AGENT_TYPE_TOPOLOGY = AgentTypes.AGENT_TYPE_TOPOLOGY;


        public const string AGENT_TYPE_SP2007_TOPOLOGY = AgentTypes.AGENT_TYPE_SP2007_TOPOLOGY;


        public const string AGENT_TYPE_CONTENT_MANAGER_OFFICE365 = AgentTypes.AGENT_TYPE_CONTENT_MANAGER_OFFICE365;



        #endregion
        #region
        private readonly int contentManager = 3;

        //public int ContentManager
        //{
        //    get { return contentManager; }
        //} 

        #endregion
        #region planType
        private readonly int cm_plan_deleted = -1;

        public int CM_PLAN_DELETED
        {
            get { return cm_plan_deleted; }
        }

        private readonly int cm_plan_export = 0;

        public int CM_PLAN_EXPORT
        {
            get { return cm_plan_export; }
        }

        private readonly int cm_plan_import = 1;

        public int CM_PLAN_IMPORT
        {
            get { return cm_plan_import; }
        }

        private readonly int cm_plan_copy = 2;

        public int CM_PLAN_COPY
        {
            get { return cm_plan_copy; }
        }

        private readonly int cm_plan_move = 3;

        public int CM_PLAN_MOVE
        {
            get { return cm_plan_move; }
        }

        private readonly int cm_plan_copy_anonymous = 20;

        public int CM_PLAN_COPY_ANONYMOUS
        {
            get { return cm_plan_copy_anonymous; }
        }

        private readonly int cm_plan_move_anonymous = 30;

        public int CM_PLAN_MOVE_ANONYMOUS
        {
            get { return cm_plan_move_anonymous; }
        }

        private readonly int cm_plan_delete_content = 4;

        public int CM_PLAN_DELETE_CONTENT
        {
            get { return cm_plan_delete_content; }
        }
        #endregion

        #region jobType
        private readonly int contentmanager_job_dto_type = (int)JobTypes.ContentManagerJob;
        public int CONTENTMANAGER_JOB_DTO_TYPE
        {
            get { return contentmanager_job_dto_type; }
        }

        private readonly int contentmanager_export_job_dto_type = (int)JobTypes.ContentManagerExportJob;
        public int CONTENTMANAGER_EXPORT_JOB_DTO_TYPE
        {
            get { return contentmanager_export_job_dto_type; }
        }

        private readonly int contentmanager_import_job_dto_type = (int)JobTypes.ContentManagerImportJob;
        public int CONTENTMANAGER_IMPORT_JOB_DTO_TYPE
        {
            get { return contentmanager_import_job_dto_type; }
        }

        private readonly int contentmanager_backup_job_dto_type = (int)JobTypes.CMBackupJob;
        public int CONTENTMANAGER_BACKUP_JOB_DTO_TYPE
        {
            get { return contentmanager_backup_job_dto_type; }
        }

        private readonly int contentmanager_restore_job_dto_type = (int)JobTypes.CMRestoreJob;
        public int CONTENTMANAGER_RESTORE_JOB_DTO_TYPE
        {
            get { return contentmanager_restore_job_dto_type; }
        }

        #endregion


        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_CONTENT_MANAGER2010);
            agentTypes.Add(AGENT_TYPE_TOPOLOGY);
            agentTypes.Add(AGENT_TYPE_SP2007_TOPOLOGY);
            agentTypes.Add(AGENT_TYPE_CONTENT_MANAGER_OFFICE365);
            return agentTypes;
        }



        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_CONTENTMANAGER_ID;
            }

        }

        public override string Name
        {
            get
            {
                return name;
            }

        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }



        public override List<int> getAllPlanTypes()
        {
            List<int> planTypes = new List<int>();
            planTypes.Add(CM_PLAN_DELETED);
            planTypes.Add(CM_PLAN_IMPORT);
            planTypes.Add(CM_PLAN_EXPORT);
            planTypes.Add(CM_PLAN_COPY);
            planTypes.Add(CM_PLAN_MOVE);
            planTypes.Add(CM_PLAN_DELETE_CONTENT);
            planTypes.Add(CM_PLAN_COPY_ANONYMOUS);
            planTypes.Add(CM_PLAN_MOVE_ANONYMOUS);
            return planTypes;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
            jobTypes.Add(CONTENTMANAGER_JOB_DTO_TYPE);
            jobTypes.Add(CONTENTMANAGER_EXPORT_JOB_DTO_TYPE);
            jobTypes.Add(CONTENTMANAGER_IMPORT_JOB_DTO_TYPE);
            jobTypes.Add(CONTENTMANAGER_BACKUP_JOB_DTO_TYPE);
            jobTypes.Add(CONTENTMANAGER_RESTORE_JOB_DTO_TYPE);
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            List<int> categories = new List<int>();
            categories.Add(contentManager);
            return categories;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.Available; }
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class Replicator : AveModule
    {

        private const string name = "Replicator";

        public const string AGENT_TYPE_REPLICATOR = AgentTypes.AGENT_TYPE_REPLICATOR;

        public const int replicator_job_dto_type = (int)JobTypes.Replicator;

        public const int replicator_import_job_type = (int)JobTypes.ReplicatorImportPlan;

        public const int replicator_healthcheck_job_type = (int)JobTypes.RPHealthCheckJob;

        public const int replicator_deployment_job_type = (int)JobTypes.RPDeploymentJob;

        private readonly ReplicatorCacheDatabase replicatorCacheDatabase = new ReplicatorCacheDatabase();
        public ReplicatorCacheDatabase ReplicatorCacheDatabase
        {
            get { return replicatorCacheDatabase; }
        }

        public int REPLICATOR_JOB_DTO_TYPE
        {
            get { return replicator_job_dto_type; }
        }

        public int REPLICATOR_IMPORT_JOB_TYPE
        {
            get { return replicator_import_job_type; }
        }

        public int REPLICATOR_HEALTHChECK_JOB_TYPE
        {
            get { return replicator_healthcheck_job_type; }
        }

        public int REPLICATOR_DEPLOYMENT_JOB_TYPE
        {
            get { return replicator_deployment_job_type; }
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_REPLICATOR);
            return agentTypes;
        }



        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_REPLICATOR_ID;
            }

        }

        public override string Name
        {
            get
            {
                return name;
            }

        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            result.Add(ReplicatorCacheDatabase);
            return result;

        }


        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
            jobTypes.Add(REPLICATOR_JOB_DTO_TYPE);
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

    /// <summary>
    /// Replicator 子模块。
    /// </summary>
    #region Replicator Cache Database
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ReplicatorCacheDatabase : AveModule
    {
        private const string name = "Replicator Cache Database";

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_REPLICATORCACHEDATABASE_ID; }
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
    #region Deployment Manager
    /// <summary>
    ///Deployment Manager 
    /// </summary>
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DeploymentManager : AveModule
    {

        private const string name = "Deployment Manager";
        private readonly SelectionOptions selectionOptions = new SelectionOptions();

        public SelectionOptions SelectionOptions
        {
            get { return selectionOptions; }
        }
        private readonly PlanSettings planSettings = new PlanSettings();

        public PlanSettings PlanSettings
        {
            get { return planSettings; }
        }
        private readonly PlanOptions planOptions = new PlanOptions();

        public PlanOptions PlanOptions
        {
            get { return planOptions; }
        }

        private readonly PatternMode patternMode = new PatternMode();
        public PatternMode PatternMode
        {
            get { return patternMode; }
        }

        public const string AGENT_TYPE_DEPLOYMENT_SITE_LEVEL = AgentTypes.AGENT_TYPE_DEPLOYMENT_SITE_LEVEL;
        public const string AGENT_TYPE_FRONTEND_DEPLOMENT = AgentTypes.AGENT_TYPE_FRONTEND_DEPLOMENT;
        public const string AGENT_TYPE_SOLUTION_CENTER = AgentTypes.AGENT_TYPE_SOLUTION_CENTER;


        private readonly int design_manager_job_type = (int)JobTypes.DesignManagerJob;
        private readonly int dm_comparereport_job_type = (int)JobTypes.DMCompareReport;
        private readonly int dm_spappupdate_type = (int)JobTypes.DMSPAppUpdate;
        private readonly int dm_spapppushupdate_type = (int)JobTypes.DMSPAPPPushUpdate;


        public const int PLAN_TYPE_DESIGNMANAGER = 0;
        public const int PLAN_TYPE_FRONTEND_DEPLOYMENT = 1;
        public const int PLAN_TYPE_SOLUTIONCENTER = 2;

        public const int JOB_TYPE_DEPLOYMENT_MANAGER = (int)JobTypes.DeploymentManagerJob;
        public const int JOB_TYPE_DESIGN_MANAGE = (int)JobTypes.DesignManagerJob;
        public const int JOB_TYPE_FRONTEND_DEPLOYMENT = (int)JobTypes.FrontendDeployment;
        public const int JOB_TYPE_SOLUTIONCENTER = (int)JobTypes.SoluctionCenter;
        public const int JOB_TYPE_METADATASERVICE = (int)JobTypes.MetadataService;
        public const int JOB_TYPE_COMPAREREPORT = (int)JobTypes.DMCompareReport;
        public const int JOB_TYPE_EXCEL_UPLOAD = (int)JobTypes.DeploymentManagerUpload;
        public const int JOB_TYPE_DEPLOYMENT_MANAGERBACKUP = (int)JobTypes.DPMBackupJob;
        public const int JOB_TYPE_SPAPP_UPDATE = (int)JobTypes.DMSPAppUpdate;
        public const int JOB_TYPE_SPAPP_PUSHUPDATE = (int)JobTypes.DMSPAPPPushUpdate;

        public const int JOB_TYPE_UPGRADESOLUTIONDATA = (int)JobTypes.UpgradeSolutionData;

        public int DESIGN_MANAGER_JOB_TYPE
        {
            get { return design_manager_job_type; }
        }

        public int DM_COMPAREREPORT_TYPE
        {
            get { return dm_comparereport_job_type; }
        }

        public int DM_SPAPPUPDATE_TYPE
        {
            get { return dm_spappupdate_type; }
        }

        public int DM_SPAPPPUSHUPDATE_TYPE
        {
            get { return dm_spapppushupdate_type; }
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_DEPLOYMENT_SITE_LEVEL);
            agentTypes.Add(AGENT_TYPE_FRONTEND_DEPLOMENT);
            agentTypes.Add(AGENT_TYPE_SOLUTION_CENTER);
            return agentTypes;
        }



        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_DEPLOYMENTMANAGER_ID;
            }

        }

        public override string Name
        {
            get
            {
                return name;
            }

        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            result.Add(SelectionOptions);
            result.Add(PlanSettings);
            result.Add(PlanOptions);
            result.Add(PatternMode);
            return result;
        }


        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobList = new List<int>();
            jobList.Add(DESIGN_MANAGER_JOB_TYPE);
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
    /// <summary>
    /// Selection Options
    /// </summary>
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SelectionOptions : AveModule
    {

        private const string name = "Selection Options";

        private readonly DesignElement designElement = new DesignElement();
        public DesignElement DesignElement
        {
            get { return designElement; }
        }

        private WebFrontEnd webFrontEnd = new WebFrontEnd();
        public WebFrontEnd WebFrontEnd
        {
            get { return webFrontEnd; }
        }

        private FarmSolution farmSolution = new FarmSolution();
        public FarmSolution FarmSolution
        {
            get { return farmSolution; }
        }

        private SharedServices sharedServices = new SharedServices();
        public SharedServices SharedServices
        {
            get { return sharedServices; }
        }
        private SharepointOnline sharepointOnline = new SharepointOnline();
        public SharepointOnline SharepointOnline
        {
            get { return sharepointOnline; }
        }

        
        public const string AGENT_TYPE_DEPLOYMENT_SITE_LEVEL = AgentTypes.AGENT_TYPE_DEPLOYMENT_SITE_LEVEL;
        public const string AGENT_TYPE_FRONTEND_DEPLOMENT = AgentTypes.AGENT_TYPE_FRONTEND_DEPLOMENT;
        public const string AGENT_TYPE_SOLUTION_CENTER = AgentTypes.AGENT_TYPE_SOLUTION_CENTER;


        private readonly int design_manager_job_type = (int)JobTypes.DesignManagerJob;
        private readonly int dm_comparereport_job_type = (int)JobTypes.DMCompareReport;


        public const int PLAN_TYPE_DESIGNMANAGER = 0;
        public const int PLAN_TYPE_FRONTEND_DEPLOYMENT = 1;
        public const int PLAN_TYPE_SOLUTIONCENTER = 2;

        public const int JOB_TYPE_DEPLOYMENT_MANAGER = (int)JobTypes.DeploymentManagerJob;
        public const int JOB_TYPE_DESIGN_MANAGE = (int)JobTypes.DesignManagerJob;
        public const int JOB_TYPE_FRONTEND_DEPLOYMENT = (int)JobTypes.FrontendDeployment;
        public const int JOB_TYPE_SOLUTIONCENTER = (int)JobTypes.SoluctionCenter;
        public const int JOB_TYPE_METADATASERVICE = (int)JobTypes.MetadataService;
        public const int JOB_TYPE_COMPAREREPORT = (int)JobTypes.DMCompareReport;
        public const int JOB_TYPE_EXCEL_UPLOAD = (int)JobTypes.DeploymentManagerUpload;
        public const int JOB_TYPE_DEPLOYMENT_MANAGERBACKUP = (int)JobTypes.DPMBackupJob;

        public const int JOB_TYPE_UPGRADESOLUTIONDATA = (int)JobTypes.UpgradeSolutionData;

        public int DESIGN_MANAGER_JOB_TYPE
        {
            get { return design_manager_job_type; }
        }

        public int DM_COMPAREREPORT_TYPE
        {
            get { return dm_comparereport_job_type; }
        }


        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_DEPLOYMENT_SITE_LEVEL);
            agentTypes.Add(AGENT_TYPE_FRONTEND_DEPLOMENT);
            agentTypes.Add(AGENT_TYPE_SOLUTION_CENTER);
            return agentTypes;
        }



        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_DEPLOYMENTMANAGER_ID;
            }

        }

        public override string Name
        {
            get
            {
                return name;
            }

        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            result.Add(DesignElement);
            result.Add(WebFrontEnd);
            result.Add(FarmSolution);
            result.Add(SharedServices);
            result.Add(SharepointOnline);
            return result;
        }


        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobList = new List<int>();
            jobList.Add(DESIGN_MANAGER_JOB_TYPE);
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
    /// <summary>
    /// Design Element
    /// </summary>
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DesignElement : AveModule
    {

        private const string name = "Design Element";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_DEPLOYMENTMANAGER_ID;
            }

        }

        public override string Name
        {
            get
            {
                return name;
            }

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
            List<int> jobList = new List<int>();
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
    /// <summary>
    /// Web Front End
    /// </summary>
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class WebFrontEnd : AveModule
    {

        private const string name = "Web front End";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }
        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_DEPLOYMENTMANAGER_ID;
            }

        }

        public override string Name
        {
            get
            {
                return name;
            }

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
            List<int> jobList = new List<int>();
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
    /// <summary>
    /// Farm Solution
    /// </summary>
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FarmSolution : AveModule
    {

        private const string name = "Farm Solution";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_DEPLOYMENTMANAGER_ID;
            }

        }

        public override string Name
        {
            get
            {
                return name;
            }

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
            List<int> jobList = new List<int>();
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
    /// <summary>
    /// Sharepoint Online
    /// </summary>
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SharepointOnline : AveModule
    {

        private const string name = "Sharepoint Online";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_DEPLOYMENTMANAGER_ID;
            }

        }

        public override string Name
        {
            get
            {
                return name;
            }

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

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }


    /// <summary>
    /// Shared Services
    /// </summary>
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SharedServices : AveModule
    {

        private const string name = "Shared Services";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_DEPLOYMENTMANAGER_ID;
            }

        }

        public override string Name
        {
            get
            {
                return name;
            }

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
            List<int> jobList = new List<int>();
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
    /// <summary>
    /// Plan Settings
    /// </summary>
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PlanSettings : AveModule
    {

        private const string name = "Plan Settings";

        //private readonly NewPlan newPlan = new NewPlan();
        //public NewPlan NewPlan
        //{
        //    get { return newPlan; }
        //}

        private DPMAddToQueue dpmAddToQueue = new DPMAddToQueue();
        public DPMAddToQueue DPMAddToQueue
        {
            get { return dpmAddToQueue; }
        }

        private SaveAsaPlan saveAsaPlan = new SaveAsaPlan();
        public SaveAsaPlan SaveAsaPlan
        {
            get { return saveAsaPlan; }
        }

        private UpLoadQueues upLoadQueues = new UpLoadQueues();
        public UpLoadQueues UpLoadQueues
        {
            get { return upLoadQueues; }
        }
        private readonly DownLoadQueues downLoadQueues = new DownLoadQueues();
        public DownLoadQueues DownLoadQueues
        {
            get { return downLoadQueues; }
        }

        private EditPlan editPlan = new EditPlan();
        public EditPlan EditPlan
        {
            get { return editPlan; }
        }

        private DeletePlan deletePlan = new DeletePlan();
        public DeletePlan DeletePlan
        {
            get { return deletePlan; }
        }

        private DataExport dataExport = new DataExport();
        public DataExport DataExport
        {
            get { return dataExport; }
        }
        private readonly DataImport dataImport = new DataImport();
        public DataImport DataImport
        {
            get { return dataImport; }
        }

        private SharePointManagementShell sharePointManagementShell = new SharePointManagementShell();
        public SharePointManagementShell SharePointManagementShell
        {
            get { return sharePointManagementShell; }
        }
        private FileSystemConfiguration fileSystemConfiguration = new FileSystemConfiguration();
        public FileSystemConfiguration FileSystemConfiguration
        {
            get { return fileSystemConfiguration; }
        }

        //private QueueTools queueTools = new QueueTools();
        //public QueueTools QueueTools
        //{
        //    get { return queueTools; }
        //}

        public const string AGENT_TYPE_DEPLOYMENT_SITE_LEVEL = AgentTypes.AGENT_TYPE_DEPLOYMENT_SITE_LEVEL;
        public const string AGENT_TYPE_FRONTEND_DEPLOMENT = AgentTypes.AGENT_TYPE_FRONTEND_DEPLOMENT;
        public const string AGENT_TYPE_SOLUTION_CENTER = AgentTypes.AGENT_TYPE_SOLUTION_CENTER;


        private readonly int design_manager_job_type = (int)JobTypes.DesignManagerJob;
        private readonly int dm_comparereport_job_type = (int)JobTypes.DMCompareReport;


        public const int PLAN_TYPE_DESIGNMANAGER = 0;
        public const int PLAN_TYPE_FRONTEND_DEPLOYMENT = 1;
        public const int PLAN_TYPE_SOLUTIONCENTER = 2;

        public const int JOB_TYPE_DEPLOYMENT_MANAGER = (int)JobTypes.DeploymentManagerJob;
        public const int JOB_TYPE_DESIGN_MANAGE = (int)JobTypes.DesignManagerJob;
        public const int JOB_TYPE_FRONTEND_DEPLOYMENT = (int)JobTypes.FrontendDeployment;
        public const int JOB_TYPE_SOLUTIONCENTER = (int)JobTypes.SoluctionCenter;
        public const int JOB_TYPE_METADATASERVICE = (int)JobTypes.MetadataService;
        public const int JOB_TYPE_COMPAREREPORT = (int)JobTypes.DMCompareReport;
        public const int JOB_TYPE_EXCEL_UPLOAD = (int)JobTypes.DeploymentManagerUpload;
        public const int JOB_TYPE_DEPLOYMENT_MANAGERBACKUP = (int)JobTypes.DPMBackupJob;

        public const int JOB_TYPE_UPGRADESOLUTIONDATA = (int)JobTypes.UpgradeSolutionData;

        public int DESIGN_MANAGER_JOB_TYPE
        {
            get { return design_manager_job_type; }
        }

        public int DM_COMPAREREPORT_TYPE
        {
            get { return dm_comparereport_job_type; }
        }


        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_DEPLOYMENT_SITE_LEVEL);
            agentTypes.Add(AGENT_TYPE_FRONTEND_DEPLOMENT);
            agentTypes.Add(AGENT_TYPE_SOLUTION_CENTER);
            return agentTypes;
        }



        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_DEPLOYMENTMANAGER_ID;
            }

        }

        public override string Name
        {
            get
            {
                return name;
            }

        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            //result.Add(NewPlan);
            //result.Add(DPMAddToQueue);
            //result.Add(SaveAsaPlan);
            result.Add(UpLoadQueues);
            result.Add(DownLoadQueues);
            result.Add(EditPlan);
            result.Add(DeletePlan);
            result.Add(DataExport);
            result.Add(DataImport);
            result.Add(SharePointManagementShell);
            result.Add(FileSystemConfiguration);
            //result.Add(QueueTools);
            return result;
        }


        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobList = new List<int>();
            jobList.Add(DESIGN_MANAGER_JOB_TYPE);
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
    /// <summary>
    /// New Plan
    /// </summary>
    //[AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    //[AveModuleAttribute("System Permission", DisplayMode.Available)]
    //[DataContract(Namespace = ContractConstants.Namespace)]
    //public class NewPlan : AveModule
    //{

    //    private const string name = "New Plan";

    //    public override List<string> getAllAgentTypes()
    //    {
    //        List<string> agentTypes = new List<string>();
    //        return agentTypes;
    //    }

    //    public override int ID
    //    {
    //        get
    //        {
    //            return AveModuleID.MODULE_TYPE_DOCAVE_DEPLOYMENTMANAGER_ID;
    //        }

    //    }

    //    public override string Name
    //    {
    //        get
    //        {
    //            return name;
    //        }

    //    }

    //    public override List<AveModule> getSubModules()
    //    {
    //        return null;
    //    }


    //    public override List<int> getAllPlanTypes()
    //    {
    //        return null;
    //    }

    //    public override List<int> getAllJobTypes()
    //    {
    //        List<int> jobList = new List<int>();
    //        return null;
    //    }

    //    public override List<int> getCategories()
    //    {
    //        return null;
    //    }

    //    public override DisplayMode ModuleDisplayMode
    //    {
    //        get { return DisplayMode.None; }
    //    }
    //}
    /// <summary>
    /// Add To Queue
    /// </summary>
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DPMAddToQueue : AveModule
    {

        private const string name = "Add to Queue";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_DEPLOYMENTMANAGER_ID;
            }

        }

        public override string Name
        {
            get
            {
                return name;
            }

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
            List<int> jobList = new List<int>();
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
    /// <summary>
    /// Save As a Plan
    /// </summary>
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SaveAsaPlan : AveModule
    {

        private const string name = "Save as a Plan";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_DEPLOYMENTMANAGER_ID;
            }

        }

        public override string Name
        {
            get
            {
                return name;
            }

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
            List<int> jobList = new List<int>();
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
    /// <summary>
    /// Upload Queues
    /// </summary>
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class UpLoadQueues : AveModule
    {

        private const string name = "Upload Queues";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_DEPLOYMENTMANAGER_ID;
            }

        }

        public override string Name
        {
            get
            {
                return name;
            }

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
            List<int> jobList = new List<int>();
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
    /// <summary>
    /// Download Queues
    /// </summary>
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DownLoadQueues : AveModule
    {

        private const string name = "Download Queues";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_DEPLOYMENTMANAGER_ID;
            }

        }

        public override string Name
        {
            get
            {
                return name;
            }

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
            List<int> jobList = new List<int>();
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
    /// <summary>
    /// Edit Plan
    /// </summary>
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EditPlan : AveModule
    {

        private const string name = "Edit Plan";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_DEPLOYMENTMANAGER_ID;
            }

        }

        public override string Name
        {
            get
            {
                return name;
            }

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
            List<int> jobList = new List<int>();
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
    /// <summary>
    /// Delete Plan
    /// </summary>
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DeletePlan : AveModule
    {

        private const string name = "Delete Plan";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_DEPLOYMENTMANAGER_ID;
            }

        }

        public override string Name
        {
            get
            {
                return name;
            }

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
            List<int> jobList = new List<int>();
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
    /// <summary>
    /// Data Export
    /// </summary>
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DataExport : AveModule
    {

        private const string name = "Data Export";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_DEPLOYMENTMANAGER_ID;
            }

        }

        public override string Name
        {
            get
            {
                return name;
            }

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
            List<int> jobList = new List<int>();
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
    /// <summary>
    /// Data Import
    /// </summary>
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DataImport : AveModule
    {

        private const string name = "Data Import";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_DEPLOYMENTMANAGER_ID;
            }

        }

        public override string Name
        {
            get
            {
                return name;
            }

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
            List<int> jobList = new List<int>();
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
    /// <summary>
    /// Share Point ManagementShell
    /// </summary>
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SharePointManagementShell : AveModule
    {

        private const string name = "SharePoint Management Shell";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_DEPLOYMENTMANAGER_ID;
            }

        }

        public override string Name
        {
            get
            {
                return name;
            }

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
            List<int> jobList = new List<int>();
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
    /// <summary>
    /// Share Point ManagementShell
    /// </summary>
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FileSystemConfiguration : AveModule
    {

        private const string name = "File System Configuration";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_DEPLOYMENTMANAGER_ID;
            }

        }

        public override string Name
        {
            get
            {
                return name;
            }

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

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }

    /// <summary>
    /// Queue Tools
    /// </summary>
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class QueueTools : AveModule
    {

        private const string name = "Queue Tools";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_DEPLOYMENTMANAGER_ID;
            }

        }

        public override string Name
        {
            get
            {
                return name;
            }

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
            List<int> jobList = new List<int>();
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
    /// <summary>
    /// Plan Options
    /// </summary>
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PlanOptions : AveModule
    {

        private const string name = "Plan Options";

        private readonly TestRun testRun = new TestRun();
        public TestRun TestRun
        {
            get { return testRun; }
        }

        private Run run = new Run();
        public Run Run
        {
            get { return run; }
        }

        private Compare compare = new Compare();
        public Compare Compare
        {
            get { return compare; }
        }

        //private DPMJobMonitor dpmJobMonitor = new DPMJobMonitor();
        //public DPMJobMonitor DPMJobMonitor
        //{
        //    get { return dpmJobMonitor; }
        //}
        //private readonly QuickRun quickRun = new QuickRun();
        //public QuickRun QuickRun
        //{
        //    get { return quickRun; }
        //}
        //private PushUpdate pushUpdate = new PushUpdate();
        //public PushUpdate PushUpdate
        //{
        //    get { return pushUpdate; }
        //}

        private SolutionTools solutionTools = new SolutionTools();
        public SolutionTools SolutionTools
        {
            get { return solutionTools; }
        }
        private readonly AppTools appTools = new AppTools();
        public AppTools AppTools
        {
            get { return appTools; }
        }

        public const string AGENT_TYPE_DEPLOYMENT_SITE_LEVEL = AgentTypes.AGENT_TYPE_DEPLOYMENT_SITE_LEVEL;
        public const string AGENT_TYPE_FRONTEND_DEPLOMENT = AgentTypes.AGENT_TYPE_FRONTEND_DEPLOMENT;
        public const string AGENT_TYPE_SOLUTION_CENTER = AgentTypes.AGENT_TYPE_SOLUTION_CENTER;


        private readonly int design_manager_job_type = (int)JobTypes.DesignManagerJob;
        private readonly int dm_comparereport_job_type = (int)JobTypes.DMCompareReport;


        public const int PLAN_TYPE_DESIGNMANAGER = 0;
        public const int PLAN_TYPE_FRONTEND_DEPLOYMENT = 1;
        public const int PLAN_TYPE_SOLUTIONCENTER = 2;

        public const int JOB_TYPE_DEPLOYMENT_MANAGER = (int)JobTypes.DeploymentManagerJob;
        public const int JOB_TYPE_DESIGN_MANAGE = (int)JobTypes.DesignManagerJob;
        public const int JOB_TYPE_FRONTEND_DEPLOYMENT = (int)JobTypes.FrontendDeployment;
        public const int JOB_TYPE_SOLUTIONCENTER = (int)JobTypes.SoluctionCenter;
        public const int JOB_TYPE_METADATASERVICE = (int)JobTypes.MetadataService;
        public const int JOB_TYPE_COMPAREREPORT = (int)JobTypes.DMCompareReport;
        public const int JOB_TYPE_EXCEL_UPLOAD = (int)JobTypes.DeploymentManagerUpload;
        public const int JOB_TYPE_DEPLOYMENT_MANAGERBACKUP = (int)JobTypes.DPMBackupJob;

        public const int JOB_TYPE_UPGRADESOLUTIONDATA = (int)JobTypes.UpgradeSolutionData;

        public int DESIGN_MANAGER_JOB_TYPE
        {
            get { return design_manager_job_type; }
        }

        public int DM_COMPAREREPORT_TYPE
        {
            get { return dm_comparereport_job_type; }
        }


        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_DEPLOYMENT_SITE_LEVEL);
            agentTypes.Add(AGENT_TYPE_FRONTEND_DEPLOMENT);
            agentTypes.Add(AGENT_TYPE_SOLUTION_CENTER);
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_DEPLOYMENTMANAGER_ID;
            }

        }

        public override string Name
        {
            get
            {
                return name;
            }

        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            result.Add(TestRun);
            result.Add(Run);
            result.Add(Compare);
            //result.Add(DPMJobMonitor);
            //result.Add(QuickRun);
            //result.Add(PushUpdate);
            result.Add(SolutionTools);
            result.Add(AppTools);
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobList = new List<int>();
            jobList.Add(DESIGN_MANAGER_JOB_TYPE);
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
    /// <summary>
    /// Test Run
    /// </summary>
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class TestRun : AveModule
    {
        private const string name = "Test Run";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_DEPLOYMENTMANAGER_ID;
            }

        }

        public override string Name
        {
            get
            {
                return name;
            }

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
            List<int> jobList = new List<int>();
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
    /// <summary>
    /// Run
    /// </summary>
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Run : AveModule
    {

        private const string name = "Run";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_DEPLOYMENTMANAGER_ID;
            }

        }

        public override string Name
        {
            get
            {
                return name;
            }

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
            List<int> jobList = new List<int>();
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
    /// <summary>
    /// Compare
    /// </summary>
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Compare : AveModule
    {

        private const string name = "Compare";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_DEPLOYMENTMANAGER_ID;
            }

        }

        public override string Name
        {
            get
            {
                return name;
            }

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
            List<int> jobList = new List<int>();
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
    ///// <summary>
    ///// Job Monitor
    ///// </summary>
    //[AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    //[AveModuleAttribute("System Permission", DisplayMode.Available)]
    //[DataContract(Namespace = ContractConstants.Namespace)]
    //public class DPMJobMonitor : AveModule
    //{

    //    private const string name = "Job Monitor";

    //    public override List<string> getAllAgentTypes()
    //    {
    //        List<string> agentTypes = new List<string>();
    //        return agentTypes;
    //    }

    //    public override int ID
    //    {
    //        get
    //        {
    //            return AveModuleID.MODULE_TYPE_DOCAVE_DEPLOYMENTMANAGER_ID;
    //        }

    //    }

    //    public override string Name
    //    {
    //        get
    //        {
    //            return name;
    //        }

    //    }

    //    public override List<AveModule> getSubModules()
    //    {
    //        return null;
    //    }


    //    public override List<int> getAllPlanTypes()
    //    {
    //        return null;
    //    }

    //    public override List<int> getAllJobTypes()
    //    {
    //        List<int> jobList = new List<int>();
    //        return null;
    //    }

    //    public override List<int> getCategories()
    //    {
    //        return null;
    //    }

    //    public override DisplayMode ModuleDisplayMode
    //    {
    //        get { return DisplayMode.None; }
    //    }
    //}
    /// <summary>
    /// Quick Run
    /// </summary>
    //[AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    //[AveModuleAttribute("System Permission", DisplayMode.Available)]
    //[DataContract(Namespace = ContractConstants.Namespace)]
    //public class QuickRun : AveModule
    //{

    //    private const string name = "Quick Run";

    //    public override List<string> getAllAgentTypes()
    //    {
    //        List<string> agentTypes = new List<string>();
    //        return agentTypes;
    //    }

    //    public override int ID
    //    {
    //        get
    //        {
    //            return AveModuleID.MODULE_TYPE_DOCAVE_DEPLOYMENTMANAGER_ID;
    //        }

    //    }

    //    public override string Name
    //    {
    //        get
    //        {
    //            return name;
    //        }

    //    }

    //    public override List<AveModule> getSubModules()
    //    {
    //        List<AveModule> result = new List<AveModule>();
    //        return result;
    //    }


    //    public override List<int> getAllPlanTypes()
    //    {
    //        return null;
    //    }

    //    public override List<int> getAllJobTypes()
    //    {
    //        List<int> jobList = new List<int>();
    //        return null;
    //    }

    //    public override List<int> getCategories()
    //    {
    //        return null;
    //    }

    //    public override DisplayMode ModuleDisplayMode
    //    {
    //        get { return DisplayMode.None; }
    //    }
    //}
    /// <summary>
    /// Push Update
    /// </summary>
    //[AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    //[AveModuleAttribute("System Permission", DisplayMode.Available)]
    //[DataContract(Namespace = ContractConstants.Namespace)]
    //public class PushUpdate : AveModule
    //{

    //    private const string name = "Push Update";

    //    public override List<string> getAllAgentTypes()
    //    {
    //        List<string> agentTypes = new List<string>();
    //        return agentTypes;
    //    }

    //    public override int ID
    //    {
    //        get
    //        {
    //            return AveModuleID.MODULE_TYPE_DOCAVE_DEPLOYMENTMANAGER_ID;
    //        }

    //    }

    //    public override string Name
    //    {
    //        get
    //        {
    //            return name;
    //        }

    //    }

    //    public override List<AveModule> getSubModules()
    //    {
    //        List<AveModule> result = new List<AveModule>();
    //        return result;
    //    }


    //    public override List<int> getAllPlanTypes()
    //    {
    //        return null;
    //    }

    //    public override List<int> getAllJobTypes()
    //    {
    //        List<int> jobList = new List<int>();
    //        return null;
    //    }

    //    public override List<int> getCategories()
    //    {
    //        return null;
    //    }

    //    public override DisplayMode ModuleDisplayMode
    //    {
    //        get { return DisplayMode.None; }
    //    }
    //}
    /// <summary>
    /// Solution Tools
    /// </summary>
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SolutionTools : AveModule
    {

        private const string name = "Solution Tools";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_DEPLOYMENTMANAGER_ID;
            }

        }

        public override string Name
        {
            get
            {
                return name;
            }

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
            List<int> jobList = new List<int>();
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
    /// <summary>
    /// App Tools
    /// </summary>
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AppTools : AveModule
    {

        private const string name = "App Tools";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_DEPLOYMENTMANAGER_ID;
            }

        }

        public override string Name
        {
            get
            {
                return name;
            }

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
            List<int> jobList = new List<int>();
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
    /// <summary>
    /// Pattern Options
    /// </summary>
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PatternMode : AveModule
    {
        private const string name = "Pattern Mode";

        public const string AGENT_TYPE_DEPLOYMENT_SITE_LEVEL = AgentTypes.AGENT_TYPE_DEPLOYMENT_SITE_LEVEL;

        private readonly int design_manager_job_type = (int)JobTypes.DesignManagerJob;
        public int DESIGN_MANAGER_JOB_TYPE
        {
            get { return design_manager_job_type; }
        }

        private CreatePattern createPattern = new CreatePattern();
        public CreatePattern CreatePattern
        {
            get { return createPattern; }
        }

        private UpdatePattern editPattern = new UpdatePattern();
        public UpdatePattern EditPattern
        {
            get { return editPattern; }
        }

        private DeletePattern deletePattern = new DeletePattern();
        public DeletePattern DeletePattern
        {
            get { return deletePattern; }
        }

        private DeployPattern deployPattern = new DeployPattern();
        public DeployPattern DeployPattern
        {
            get { return deployPattern; }
        }

        private UpdateScope updateScope = new UpdateScope();
        public UpdateScope UpdateScope
        {
            get { return updateScope; }
        }
        
        
        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_DEPLOYMENT_SITE_LEVEL);
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_DEPLOYMENTMANAGER_ID;
            }

        }

        public override string Name
        {
            get
            {
                return name;
            }

        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            result.Add(CreatePattern);
            result.Add(EditPattern);
            result.Add(DeletePattern);
            result.Add(DeployPattern);
            result.Add(UpdateScope);
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobList = new List<int>();
            jobList.Add(DESIGN_MANAGER_JOB_TYPE);
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
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CreatePattern : AveModule
    {
        private const string name = "Create";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_DEPLOYMENTMANAGER_ID;
            }

        }

        public override string Name
        {
            get
            {
                return name;
            }

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
            List<int> jobList = new List<int>();
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
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class UpdatePattern : AveModule
    {
        private const string name = "Edit";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_DEPLOYMENTMANAGER_ID;
            }

        }

        public override string Name
        {
            get
            {
                return name;
            }

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
            List<int> jobList = new List<int>();
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
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DeletePattern : AveModule
    {
        private const string name = "Delete";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_DEPLOYMENTMANAGER_ID;
            }

        }

        public override string Name
        {
            get
            {
                return name;
            }

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
            List<int> jobList = new List<int>();
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
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DeployPattern : AveModule
    {
        private const string name = "Deploy";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_DEPLOYMENTMANAGER_ID;
            }

        }

        public override string Name
        {
            get
            {
                return name;
            }

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
            List<int> jobList = new List<int>();
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
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class UpdateScope : AveModule
    {
        private const string name = "Update Scope";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_DEPLOYMENTMANAGER_ID;
            }

        }

        public override string Name
        {
            get
            {
                return name;
            }

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
            List<int> jobList = new List<int>();
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
    #endregion
}
