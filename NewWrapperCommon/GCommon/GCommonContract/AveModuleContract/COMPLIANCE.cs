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
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class Compliance : AveModuleContainer
    {

        private const string MODULE_TYPE_DOCAVE_COMPLIANCE_NAME = "Compliance";

        #region agentType
        //public const string AGENT_TYPE_SP2007_COMPLIANCE_ARCHIVE = AgentTypes.AGENT_TYPE_SP2007_COMPLIANCE_ARCHIVE;



        //public const string AGENT_TYPE_AUDITOR2007 = AgentTypes.AGENT_TYPE_AUDITOR2007;


        public const string AGENT_TYPE_EDISCOVERY = AgentTypes.AGENT_TYPE_EDISCOVERY;
        public const string AGENT_TYPE_COMPLIANCE_VAULT = AgentTypes.AGENT_TYPE_COMPLIANCE_VAULT;

        //public const string AGENT_TYPE_AUDITOR = AgentTypes.AGENT_TYPE_AUDITOR;


        #endregion

        private readonly Vault vault = new Vault();

        public Vault Vault
        {
            get { return vault; }
        }

        private readonly EDiscovery ediscovery = new EDiscovery();

        public EDiscovery EDiscovery
        {
            get { return ediscovery; }
        }

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_COMPLIANCE_ID; }
        }

        public override string Name
        {
            get { return MODULE_TYPE_DOCAVE_COMPLIANCE_NAME; }
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            result.Add(EDiscovery);
            result.Add(Vault);
            return result;

        }

        public override List<int> getAllPlanTypes()
        {
            return new List<int>();
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
            jobTypes.AddRange(EDiscovery.getAllJobTypes());
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            return new List<int>();
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_EDISCOVERY);
            agentTypes.Add(AGENT_TYPE_COMPLIANCE_VAULT);
            return agentTypes;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.Available; }
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class EDiscovery : AveModule
    {
        private const string name = "eDiscovery";

        #region agentType

        public const string AGENT_TYPE_EDISCOVERY = AgentTypes.AGENT_TYPE_EDISCOVERY;
        #endregion

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_EDISCOVERY);
            return agentTypes;
        }


        #region jobType
        private static readonly int ed_contentsource_job_type = (int)JobTypes.EDContentSourceJob;
//        private static readonly int ed_hold_manager_sync_file_job_type = (int)JobTypes.EDSyncFileJob;
//        private static readonly int ed_hold_manager_sync_item_job_type = (int)JobTypes.EDSyncItemJob;
        private static readonly int ed_hold_job_type = (int)JobTypes.EDHoldJob;
        private static readonly int ed_release_job_type = (int)JobTypes.EDReleaseJob;
        private static readonly int ed_search_job_type = (int)JobTypes.EDSearchJob;
//        private static readonly int ed_search_archive_job_type = (int)JobTypes.EDSearchArchiveJob;
        private static readonly int ed_sync_job_type = (int)JobTypes.EDSyncJob;
        private const int ed_export_job_type = (int) JobTypes.EDExportJob;
        private const int ed_search_result_export_job_type = (int)JobTypes.EDDownloadSearchResult;

        

        private static readonly string ed_hold_job_display_str = "Hold";
        private static readonly string ed_release_job_display_str = "Release";
        private static readonly string ed_sync_job_display_str = "Sync";
        private static readonly string ed_search_job_display_str = "Search";
        private static readonly string ed_export_job_display_str = "Export";
        private static readonly string ed_download_search_result_job_display_str = "Download Search Result";

        public static readonly int None = 0;
        public static readonly int SPDataSearchPlan = 71;
        public static readonly int ARDataSearchPlan = 72;


        public static string GetJobTypeDisplayStr(int type)
        {
            string str = String.Empty;

            if (type == EDiscovery.ED_HOLD_JOB_TYPE)
            {
                return ed_hold_job_display_str;
            }

            if (type == EDiscovery.ED_RELEASE_JOB_TYPE)
            {
                return ed_release_job_display_str;
            }

            if (type == EDiscovery.ED_SYNC_JOB_TYPE)
            {
                return ed_sync_job_display_str;
            }

            if (type == EDiscovery.ED_SEARCH_JOB_TYPE)
            {
                return ed_search_job_display_str;
            }

            if (type == EDiscovery.ED_EXPORT_JOB_TYPE)
            {
                return ed_export_job_display_str;
            }

            if (type == EDiscovery.ED_SEARCH_RESULT_EXPORT_JOB_TYPE)
            {
                return ed_download_search_result_job_display_str;
            }

            return str;
        }


        public static int ED_HOLD_JOB_TYPE
        {
            get { return ed_hold_job_type; }
        }

        public static int ED_RELEASE_JOB_TYPE
        {
            get { return ed_release_job_type; }
        }


        public static int ED_SEARCH_JOB_TYPE
        {
            get { return ed_search_job_type; }
        }

//        public static int ED_SEARCH_ARCHIVE_JOB_TYPE
//        {
//            get { return ed_search_archive_job_type; }
//        }


        public static int ED_CONTENTSOURCE_JOB_TYPE
        {
            get { return ed_contentsource_job_type; }
        }

        public static int ED_EXPORT_JOB_TYPE
        {
            get { return ed_export_job_type; }
        }


        public static int ED_SEARCH_RESULT_EXPORT_JOB_TYPE
        {
            get { return ed_search_result_export_job_type; }
        }

        #region 准备去掉的部分
//        public static int ED_HOLDMANAGER_SYNC_FILE_JOB_TYPE
//        {
//            get { return ed_sync_job_type; }
//        }
//
//        public static int ED_HOLD_MANAGER_SYNC_ITEM_JOB_TYPE
//        {
//            get { return ed_sync_job_type; }
//        }
        #endregion

        public static int ED_SYNC_JOB_TYPE
        {
            get { return ed_sync_job_type; }
        }


        #endregion



        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_COMPLIANCE_EDISCOVERY_ID;
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
            return new List<AveModule>();
        }


        public override List<int> getAllPlanTypes()
        {
            return new List<int>();
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
//            jobTypes.Add(ED_CONTENTSOURCE_JOB_TYPE);
//            jobTypes.Add(ED_HOLDMANAGER_SYNC_FILE_JOB_TYPE);
//            jobTypes.Add(ED_HOLD_MANAGER_SYNC_ITEM_JOB_TYPE);
            jobTypes.Add(ED_HOLD_JOB_TYPE);
            jobTypes.Add(ED_RELEASE_JOB_TYPE);
            jobTypes.Add(ED_SEARCH_JOB_TYPE);
            jobTypes.Add(ED_SEARCH_RESULT_EXPORT_JOB_TYPE);
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            return new List<int>();
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.Available; }
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class Vault : AveModule
    {
        private const string name = "Vault";

        #region agentType

        public const string AGENT_TYPE_COMPLIANCE_VAULT = AgentTypes.AGENT_TYPE_COMPLIANCE_VAULT;
        #endregion

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_COMPLIANCE_VAULT);
            return agentTypes;
        }

        #region jobType
        private readonly int vault_export_job_type = (int)JobTypes.VaultExportJob;
        private readonly int vault_scan_job_type = (int)JobTypes.VaultScanJob;
        public int VAULT_EXPORT_JOB_TYPE
        {
            get { return vault_export_job_type; }
        }

        public int VAULT_SCAN_JOB_TYPE
        {
            get { return vault_scan_job_type; }
        }
        //public readonly int VAULT_SCAN_JOB_TYPE = (int)JobTypes.VaultScanJob;
        //public readonly int VAULT_EXPORT_JOB_TYPE = (int)JobTypes.VaultExportJob;
        #endregion

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_COMPLIANCE_VAULT_ID;
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
            return new List<AveModule>();
        }


        public override List<int> getAllPlanTypes()
        {
            return new List<int>();
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
            //jobTypes.Add(ED_CONTENTSOURCE_JOB_DTO_TYPE);
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            return new List<int>();
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.Available; }
        }
    }
}
