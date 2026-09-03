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

        private readonly AdministratorManagement administratorManagement = new AdministratorManagement();

        public AdministratorManagement AdministratorManagement
        {
            get { return administratorManagement; }
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

        private readonly int ca_profile_job_type = (int)JobTypes.CAProfileJob;

        public int CA_PROFILE_JOB_TYPE
        {
            get { return ca_profile_job_type; }
        }

        private readonly int ca_profile_auditor_only_type = (int)JobTypes.CAOnlyAuditorRulePEJob;

        public int CA_PROFILE_AUDITOR_ONLY_JOB_TYPE
        {
            get { return ca_profile_auditor_only_type; }
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
            result.Add(AdministratorManagement);
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

    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class AdministratorManagement : AveModule
    {

        public const string module_name = "Administrator Management";

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




    //[AveModuleAttribute(ModuleContract.DocAvePlatform.ControlPanel.Monitor.Name, DisplayMode.Available)]
    // [AveModuleAttribute(ModuleContract.DocAvePlatform.ControlPanel.AgentGroup.Name, DisplayMode.Disable)]
    // [AveModuleAttribute("Account Manager", DisplayMode.None)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class Replicator : AveModule
    {

        private const string name = "Replicator";

        public const string AGENT_TYPE_REPLICATOR = AgentTypes.AGENT_TYPE_REPLICATOR;

        public const int replicator_job_dto_type = (int)JobTypes.Replicator;

        public const int replicator_import_job_type = (int)JobTypes.ReplicatorImportPlan;

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
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
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

    //[AveModuleAttribute("Account Manager", DisplayMode.None)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DeploymentManager : AveModule
    {

        private const string name = "Deployment Manager";

        public const string AGENT_TYPE_DEPLOYMENT_SITE_LEVEL = AgentTypes.AGENT_TYPE_DEPLOYMENT_SITE_LEVEL;
        public const string AGENT_TYPE_FRONTEND_DEPLOMENT = AgentTypes.AGENT_TYPE_FRONTEND_DEPLOMENT;
        public const string AGENT_TYPE_SOLUTION_CENTER = AgentTypes.AGENT_TYPE_SOLUTION_CENTER;


        private readonly int design_manager_job_type = (int)JobTypes.DesignManagerJob;


        public const int PLAN_TYPE_DESIGNMANAGER = 0;
        public const int PLAN_TYPE_FRONTEND_DEPLOYMENT = 1;
        public const int PLAN_TYPE_SOLUTIONCENTER = 2;

        public const int JOB_TYPE_DEPLOYMENT_MANAGER = (int)JobTypes.DeploymentManagerJob;
        public const int JOB_TYPE_DESIGN_MANAGE = (int)JobTypes.DesignManagerJob;
        public const int JOB_TYPE_COMPARE_NOW = (int)JobTypes.DeploymentManagerCompare;
        public const int JOB_TYPE_FRONTEND_DEPLOYMENT = (int)JobTypes.FrontendDeployment;
        public const int JOB_TYPE_SOLUTIONCENTER = (int)JobTypes.SoluctionCenter;
        public const int JOB_TYPE_METADATASERVICE = (int)JobTypes.MetadataService;
        public const int JOB_TYPE_EXCEL_UPLOAD = (int)JobTypes.DeploymentManagerUpload;
        public const int JOB_TYPE_DEPLOYMENT_MANAGERBACKUP = (int)JobTypes.DPMBackupJob;

        public const int JOB_TYPE_UPGRADESOLUTIONDATA = (int)JobTypes.UpgradeSolutionData;

        public int DESIGN_MANAGER_JOB_TYPE
        {
            get { return design_manager_job_type; }
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
            return null;
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

}
