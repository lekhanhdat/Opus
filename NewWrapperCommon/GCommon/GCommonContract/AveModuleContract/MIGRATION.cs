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
using AvePoint.GCommon.Contract.Server.Common.Attribute;

namespace AvePoint.GCommon.Contract.AveModuleContract
{
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class Migration : AveModuleContainer
    {
        private const string MODULE_TYPE_DOCAVE_MIGRATION_NAME = "Migration";

        #region agentType
        public const string AGENT_TYPE_MIGRATION_EROOM_SRC = AgentTypes.AGENT_TYPE_MIGRATION_EROOM_SRC;

        public const string AGENT_TYPE_MIGRATION_EROOM_DEST = AgentTypes.AGENT_TYPE_MIGRATION_EROOM_DEST;

        public const string AGENT_TYPE_MIGRATION_FILE = AgentTypes.AGENT_TYPE_MIGRATION_FILE;

        public const string AGENT_TYPE_MIGRATION_DIRECT_MOSS2007 = AgentTypes.AGENT_TYPE_MIGRATION_DIRECT_MOSS2007;

        public const string AGENT_TYPE_MIGRATION_LIVELINK_SRC = AgentTypes.AGENT_TYPE_MIGRATION_LIVELINK_SRC;

        public const string AGENT_TYPE_MIGRATION_LIVELINK_DEST = AgentTypes.AGENT_TYPE_MIGRATION_LIVELINK_DEST;

        public const string AGENT_TYPE_MIGRATION_NOTES_SRC = AgentTypes.AGENT_TYPE_MIGRATION_NOTES_SRC;

        public const string AGENT_TYPE_MIGRATION_NOTES_DEST = AgentTypes.AGENT_TYPE_MIGRATION_NOTES_DEST;

        public const string AGENT_TYPE_MIGRATION_PFTO2003 = AgentTypes.AGENT_TYPE_MIGRATION_PFTO2003;

        public const string AGENT_TYPE_MIGRATION_PFTO2007 = AgentTypes.AGENT_TYPE_MIGRATION_PFTO2007;

        public const string AGENT_TYPE_MIGRATION_EPF_SRC = AgentTypes.AGENT_TYPE_MIGRATION_EPF_SRC;

        public const string AGENT_TYPE_MIGRATION_EMC_SRC = AgentTypes.AGENT_TYPE_MIGRATION_EMC_SRC;

        public const string AGENT_TYPE_MIGRATION_EMC_DEST = AgentTypes.AGENT_TYPE_MIGRATION_EMC_DEST;

        public const string AGENT_TYPE_MIGRATION_07_10 = AgentTypes.AGENT_TYPE_MIGRATION_07_10;

        public const string AGENT_TYPE_MIGRATION_SPTOMOSS = AgentTypes.AGENT_TYPE_MIGRATION_SPTOMOSS;

        public const string AGENT_TYPE_MIGRATION_DIRECT_SP2003 = AgentTypes.AGENT_TYPE_MIGRATION_DIRECT_SP2003;
        #endregion

        private readonly eRoomMigration eroommigration = new eRoomMigration();
        private readonly eRoomMigrationSource eroommigrationsource = new eRoomMigrationSource();
        private readonly eRoomMigrationDestination eroommigrationdestination = new eRoomMigrationDestination();
        private readonly FileMigration filemigration = new FileMigration();
        private readonly LivelinkMigration livelinkmigration = new LivelinkMigration();
        private readonly LivelinkMigrationSource livelinkmigrationsource = new LivelinkMigrationSource();
        private readonly LivelinkMigrationDestination livelinkmigrationdestination = new LivelinkMigrationDestination();
        private readonly NotesMigration notesmigration = new NotesMigration();
        private readonly NotesMigrationSource notesmigrationsource = new NotesMigrationSource();
        private readonly NotesMigrationDestination notesmigrationdestination = new NotesMigrationDestination();
        private readonly PublicFolderMigration publicfoldermigration = new PublicFolderMigration();
        private readonly PublicFolderMigrationSource publicfoldermigrationsource = new PublicFolderMigrationSource();
        private readonly PublicFolderMigrationDestination publicfoldermigrationdestination = new PublicFolderMigrationDestination();
        private readonly SPMigration spmigration = new SPMigration();
        private readonly SPHSMigration sphsmigration = new SPHSMigration();
        private readonly QuickPlaceMigration quickPlaceMigration = new QuickPlaceMigration();
        private readonly QuickPlaceMigrationSource quickPlaceMigrationSource = new QuickPlaceMigrationSource();
        private readonly QuickPlaceMigrationDestination quickPlaceMigrationDestination = new QuickPlaceMigrationDestination();
        private readonly DocumentumMigration documentumMigration = new DocumentumMigration();
        private readonly DocumentumMigrationSource documentumMigrationSource = new DocumentumMigrationSource();
        private readonly DocumentumMigrationDestination documentumMigrationDestination = new DocumentumMigrationDestination();

        #region
        private readonly int migration_plan_standard = 0;
        public int MIGRATION_PLAN_STANDARD
        {
            get { return migration_plan_standard; }
        }

        private readonly int migration_plan_export = 1;
        public int MIGRATION_PLAN_EXPORT
        {
            get { return migration_plan_export; }
        }

        private readonly int migration_plan_import = 2;
        public int MIGRATION_PLAN_IMPORT
        {
            get { return migration_plan_import; }
        }
        #endregion

        public eRoomMigration eRoomMigration
        {
            get { return eroommigration; }
        }

        public eRoomMigrationSource eRoomMigrationSource
        {
            get { return eroommigrationsource; }
        }

        public eRoomMigrationDestination eRoomMigrationDestination
        {
            get { return eroommigrationdestination; }
        }

        public FileMigration FileMigration
        {
            get { return filemigration; }
        }

        public LivelinkMigration LivelinkMigration
        {
            get { return livelinkmigration; }
        }

        public LivelinkMigrationSource LivelinkMigrationSource
        {
            get { return livelinkmigrationsource; }
        }

        public LivelinkMigrationDestination LivelinkMigrationDestination
        {
            get { return livelinkmigrationdestination; }
        }

        public NotesMigration NotesMigration
        {
            get { return notesmigration; }
        }

        public NotesMigrationSource NotesMigrationSource
        {
            get { return notesmigrationsource; }
        }

        public NotesMigrationDestination NotesMigrationDestination
        {
            get { return notesmigrationdestination; }
        }

        public PublicFolderMigration PublicFolderMigration
        {
            get { return publicfoldermigration; }
        }

        public PublicFolderMigrationSource PublicFolderMigrationSource
        {
            get { return publicfoldermigrationsource; }
        }

        public PublicFolderMigrationDestination PublicFolderMigrationDestination
        {
            get { return publicfoldermigrationdestination; }
        }

        public SPMigration SPMigration
        {
            get { return spmigration; }
        }

        public SPHSMigration SPHSMigration
        {
            get { return sphsmigration; }
        }

        public QuickPlaceMigration QuickPlaceMigration
        {
            get { return this.quickPlaceMigration; }
        }

        public QuickPlaceMigrationSource QuickPlaceMigrationSource
        {
            get { return this.quickPlaceMigrationSource; }
        }

        public QuickPlaceMigrationDestination QuickPlaceMigrationDestination
        {
            get { return this.quickPlaceMigrationDestination; }
        }

        public DocumentumMigration DocumentumMigration
        {
            get { return this.documentumMigration; }
        }

        public DocumentumMigrationSource DocumentumMigrationSource
        {
            get { return this.documentumMigrationSource; }
        }

        public DocumentumMigrationDestination DocumentumMigrationDestination
        {
            get { return this.documentumMigrationDestination; }
        }

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_MIGRATION_ID; }
        }

        public override string Name
        {
            get { return MODULE_TYPE_DOCAVE_MIGRATION_NAME; }
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>()
            {
                eRoomMigration,
                eRoomMigrationSource,
                eRoomMigrationDestination,
                FileMigration,
                LivelinkMigration,
                LivelinkMigrationSource,
                LivelinkMigrationDestination,
                NotesMigration,
                NotesMigrationSource,
                NotesMigrationDestination,
                PublicFolderMigration,
                PublicFolderMigrationSource,
                PublicFolderMigrationDestination,
                QuickPlaceMigration,
                QuickPlaceMigrationSource,
                QuickPlaceMigrationDestination,
                DocumentumMigration,
                DocumentumMigrationSource,
                DocumentumMigrationDestination,
                SPMigration,
            };
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            List<int> planTypes = new List<int>();
            planTypes.Add(MIGRATION_PLAN_STANDARD);
            planTypes.Add(MIGRATION_PLAN_EXPORT);
            planTypes.Add(MIGRATION_PLAN_IMPORT);
            return planTypes;
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
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    public class PublicFolderMigrationSource : AveModule
    {
        private const string name = "Exchange Public Folder Migration for Exchange Public Folder Agent";

        public const string AGENT_TYPE_MIGRATION_PublicFolder = AgentTypes.AGENT_TYPE_MIGRATION_EPF_SRC;

        private readonly int pfmigration_job_dto_type = (int)JobTypes.PublicFolderMigration;

        public int PFMIGRATION_JOB_DTO_TYPE
        {
            get { return pfmigration_job_dto_type; }
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_MIGRATION_PublicFolder);
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_MIGRATION_ID;
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
            List<int> jobTypes = new List<int>();
            jobTypes.Add(PFMIGRATION_JOB_DTO_TYPE);
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
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    public class PublicFolderMigrationDestination : AveModule
    {
        private const string name = "Exchange Public Folder Migration for SharePoint 2010 Agent";

        public const string AGENT_TYPE_MIGRATION_PublicFolder = AgentTypes.AGENT_TYPE_MIGRATION_PFTO2007;

        private readonly int pfmigration_job_dto_type = (int)JobTypes.PublicFolderMigration;

        public int PFMIGRATION_JOB_DTO_TYPE
        {
            get { return pfmigration_job_dto_type; }
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_MIGRATION_PublicFolder);
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_MIGRATION_ID;
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
            List<int> jobTypes = new List<int>();
            jobTypes.Add(PFMIGRATION_JOB_DTO_TYPE);
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
    public class PublicFolderMigration : AveModule
    {
        private const string name = "Exchange Public Folder Migration";

        public const string AGENT_TYPE_MIGRATION_PublicFolder_Src = AgentTypes.AGENT_TYPE_MIGRATION_EPF_SRC;

        public const string AGENT_TYPE_MIGRATION_PublicFolder_Dest = AgentTypes.AGENT_TYPE_MIGRATION_PFTO2007;

        private readonly int pfmigration_job_dto_type = (int)JobTypes.PublicFolderMigration;

        public int PFMIGRATION_JOB_DTO_TYPE
        {
            get { return pfmigration_job_dto_type; }
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_MIGRATION_PublicFolder_Src);
            agentTypes.Add(AGENT_TYPE_MIGRATION_PublicFolder_Dest);
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_MIGRATION_ID;
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
            List<int> jobTypes = new List<int>();
            jobTypes.Add(PFMIGRATION_JOB_DTO_TYPE);
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            return null;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public List<string> getSrcAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_MIGRATION_PublicFolder_Src);
            return agentTypes;
        }

        public List<string> getDestAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_MIGRATION_PublicFolder_Dest);
            return agentTypes;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class SPMigration : AveModule
    {
        private const string name = "SharePoint 2007 Migration";

        public const string AGENT_TYPE_MIGRATION_07_10 = AgentTypes.AGENT_TYPE_MIGRATION_07_10;

        public const string AGENT_TYPE_MIGRATION_07_13 = AgentTypes.AGENT_TYPE_MIGRATION_07_13;

        public const string AGENT_TYPE_MIGRATION_10_13 = AgentTypes.AGENT_TYPE_MIGRATION_10_13;

        private readonly int spmigration_07_10_job_dto_type = (int)JobTypes.SPMigration07_10;
        private readonly int spmigration_07_13_job_dto_type = (int)JobTypes.SPMigration07_13;
        private readonly int spmigration_10_13_job_dto_type = (int)JobTypes.SPMigration10_13;

        private readonly int spmigration_07_Export_job_dto_type = (int)JobTypes.SPMigration07Export;
        private readonly int spmigration_10_Export_job_dto_type = (int)JobTypes.SPMigration10Export;

        private readonly int spmigration_07_10_Import_job_dto_type = (int)JobTypes.SPMigration07_10_Import;
        private readonly int spmigration_07_13_Import_job_dto_type = (int)JobTypes.SPMigration07_13_Import;
        private readonly int spmigration_10_13_Import_job_dto_type = (int)JobTypes.SPMigration10_13_Import;

        private readonly int spmigration_07_13Remote_job_dto_type = (int)JobTypes.SPMigration07_13Remote;
        private readonly int spmigration_10_13Remote_job_dto_type = (int)JobTypes.SPMigration10_13Remote;

        private readonly int spmigration_13_Export_job_dto_type = (int)JobTypes.SPMigration13Export;
        private readonly int spmigration_07_16_Import_job_dto_type = (int)JobTypes.SPMigration07_16_Import;
        private readonly int spmigration_10_16_Import_job_dto_type = (int)JobTypes.SPMigration10_16_Import;
        private readonly int spmigration_13_16_Import_job_dto_type = (int)JobTypes.SPMigration13_16_Import;
        private readonly int spmigration_07_16_job_dto_type = (int)JobTypes.SPMigration07_16;
        private readonly int spmigration_10_16_job_dto_type = (int)JobTypes.SPMigration10_16;
        private readonly int spmigration_13_16_job_dto_type = (int)JobTypes.SPMigration13_16;
        private readonly int spmigration_13_13Remote_job_dto_type = (int)JobTypes.SPMigration13_13Remote;

        private readonly int spmigration_16_Export_job_dto_type = (int)JobTypes.SPMigration16Export;
        private readonly int spmigration_07_19_Import_job_dto_type = (int)JobTypes.SPMigration07_19_Import;
        private readonly int spmigration_10_19_Import_job_dto_type = (int)JobTypes.SPMigration10_19_Import;
        private readonly int spmigration_13_19_Import_job_dto_type = (int)JobTypes.SPMigration13_19_Import;
        private readonly int spmigration_16_19_Import_job_dto_type = (int)JobTypes.SPMigration16_19_Import;
        private readonly int spmigration_07_19_job_dto_type = (int)JobTypes.SPMigration07_19;
        private readonly int spmigration_10_19_job_dto_type = (int)JobTypes.SPMigration10_19;
        private readonly int spmigration_13_19_job_dto_type = (int)JobTypes.SPMigration13_19;
        private readonly int spmigration_16_19_job_dto_type = (int)JobTypes.SPMigration16_19;
        private readonly int spmigration_16_19Remote_job_dto_type = (int)JobTypes.SPMigration16_13Remote;

        private readonly int spmigration_07_remote_Import_job_dto_type = (int)JobTypes.SPMigration07_Remote_Import;
        private readonly int spmigration_10_remote_Import_job_dto_type = (int)JobTypes.SPMigration10_Remote_Import;
        private readonly int spmigration_13_remote_Import_job_dto_type = (int)JobTypes.SPMigration13_Remote_Import;
        private readonly int spmigration_16_remote_Import_job_dto_type = (int)JobTypes.SPMigration16_Remote_Import;

        public int SPMIGRATION_13_EXPORT_JOB_DTO_TYPE
        {
            get { return spmigration_13_Export_job_dto_type; }
        }
        public int SPMIGRATION_07_16_IMPORT_JOB_DTO_TYPE
        {
            get { return spmigration_07_16_Import_job_dto_type; }
        }
        public int SPMIGRATION_10_16_IMPORT_JOB_DTO_TYPE
        {
            get { return spmigration_10_16_Import_job_dto_type; }
        }
        public int SPMIGRATION_13_16_IMPORT_JOB_DTO_TYPE
        {
            get { return spmigration_13_16_Import_job_dto_type; }
        }
        public int SPMIGRATION_07_16_JOB_DTO_TYPE
        {
            get { return spmigration_07_16_job_dto_type; }
        }
        public int SPMIGRATION_10_16_JOB_DTO_TYPE
        {
            get { return spmigration_10_16_job_dto_type; }
        }
        public int SPMIGRATION_13_16_JOB_DTO_TYPE
        {
            get { return spmigration_13_16_job_dto_type; }
        }
        public int SPMIGRATION_13_13REMOTE_JOB_DTO_TYPE
        {
            get { return spmigration_13_13Remote_job_dto_type; }
        }

        public int SPMIGRATION_07_10_JOB_DTO_TYPE
        {
            get { return spmigration_07_10_job_dto_type; }
        }
        public int SPMIGRATION_07_13_JOB_DTO_TYPE
        {
            get { return spmigration_07_13_job_dto_type; }
        }
        public int SPMIGRATION_10_13_JOB_DTO_TYPE
        {
            get { return spmigration_10_13_job_dto_type; }
        }

        public int SPMIGRATION_07_EXPORT_JOB_DTO_TYPE
        {
            get { return spmigration_07_Export_job_dto_type; }
        }

        public int SPMIGRATION_10_EXPORT_JOB_DTO_TYPE
        {
            get { return spmigration_10_Export_job_dto_type; }
        }

        public int SPMIGRATION_07_10_Import_JOB_DTO_TYPE
        {
            get { return spmigration_07_10_Import_job_dto_type; }
        }

        public int SPMIGRATION_07_13_Import_JOB_DTO_TYPE
        {
            get { return spmigration_07_13_Import_job_dto_type; }
        }

        public int SPMIGRATION_10_13_Import_JOB_DTO_TYPE
        {
            get { return spmigration_10_13_Import_job_dto_type; }
        }

        public int SPMIGRATION_07_13Remote_JOB_DTO_TYPE
        {
            get { return spmigration_07_13Remote_job_dto_type; }
        }

        public int SPMIGRATION_10_13Remote_JOB_DTO_TYPE
        {
            get { return spmigration_10_13Remote_job_dto_type; }
        }

        public int SPMIGRATION_16_EXPORT_JOB_DTO_TYPE
        {
            get { return spmigration_16_Export_job_dto_type; }
        }

        public int SPMIGRATION_07_19_IMPORT_JOB_DTO_TYPE
        {
            get { return spmigration_07_19_Import_job_dto_type; }
        }

        public int SPMIGRATION_10_19_IMPORT_JOB_DTO_TYPE
        {
            get { return spmigration_10_19_Import_job_dto_type; }
        }

        public int SPMIGRATION_13_19_IMPORT_JOB_DTO_TYPE
        {
            get { return spmigration_13_19_Import_job_dto_type; }
        }

        public int SPMIGRATION_16_19_IMPORT_JOB_DTO_TYPE
        {
            get { return spmigration_16_19_Import_job_dto_type; }
        }

        public int SPMIGRATION_07_19_JOB_DTO_TYPE
        {
            get { return spmigration_07_19_job_dto_type; }
        }

        public int SPMIGRATION_10_19_JOB_DTO_TYPE
        {
            get { return spmigration_10_19_job_dto_type; }
        }

        public int SPMIGRATION_13_19_JOB_DTO_TYPE
        {
            get { return spmigration_13_19_job_dto_type; }
        }

        public int SPMIGRATION_16_19_JOB_DTO_TYPE
        {
            get { return spmigration_16_19_job_dto_type; }
        }

        public int SPMIGRATION_16_19REMOTE_JOB_DTO_TYPE
        {
            get { return spmigration_16_19Remote_job_dto_type; }
        }

        public int SPMIGRATION_07_REMOTE_IMPORT_JOB_DTO_TYPE
        {
            get { return spmigration_07_remote_Import_job_dto_type; }
        }

        public int SPMIGRATION_10_REMOTE_IMPORT_JOB_DTO_TYPE
        {
            get { return spmigration_10_remote_Import_job_dto_type; }
        }

        public int SPMIGRATION_13_REMOTE_IMPORT_JOB_DTO_TYPE
        {
            get { return spmigration_13_remote_Import_job_dto_type; }
        }

        public int SPMIGRATION_16_REMOTE_IMPORT_JOB_DTO_TYPE
        {
            get { return spmigration_16_remote_Import_job_dto_type; }
        }
        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_MIGRATION_07_10);
            agentTypes.Add(AGENT_TYPE_MIGRATION_07_13);
            agentTypes.Add(AGENT_TYPE_MIGRATION_10_13);
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_MIGRATION_ID;
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
            List<int> jobTypes = new List<int>();
            jobTypes.Add(SPMIGRATION_07_10_JOB_DTO_TYPE);
            jobTypes.Add(SPMIGRATION_07_13_JOB_DTO_TYPE);
            jobTypes.Add(SPMIGRATION_10_13_JOB_DTO_TYPE);
            jobTypes.Add(SPMIGRATION_07_EXPORT_JOB_DTO_TYPE);
            jobTypes.Add(SPMIGRATION_10_EXPORT_JOB_DTO_TYPE);
            jobTypes.Add(SPMIGRATION_07_10_Import_JOB_DTO_TYPE);
            jobTypes.Add(SPMIGRATION_07_13_Import_JOB_DTO_TYPE);
            jobTypes.Add(SPMIGRATION_10_13_Import_JOB_DTO_TYPE);
            jobTypes.Add(SPMIGRATION_07_13Remote_JOB_DTO_TYPE);
            jobTypes.Add(SPMIGRATION_10_13Remote_JOB_DTO_TYPE);
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
    public class SPHSMigration : AveModule
    {
        private const string name = "SharePoint High Speed Migration";

        public const string AGENT_TYPE_MIGRATION_07_10 = AgentTypes.AGENT_TYPE_MIGRATION_07_10;

        public const string AGENT_TYPE_MIGRATION_07_13 = AgentTypes.AGENT_TYPE_MIGRATION_07_13;

        public const string AGENT_TYPE_MIGRATION_10_13 = AgentTypes.AGENT_TYPE_MIGRATION_10_13;

        private readonly int sp07HighSpeedMigrationImport_job_dto_type = (int)JobTypes.SharePoint07HighSpeedMigrationImportJob;
        private readonly int sp10HighSpeedMigrationImport_job_dto_type = (int)JobTypes.SharePoint10HighSpeedMigrationImportJob;
        private readonly int sp13HighSpeedMigrationImport_job_dto_type = (int)JobTypes.SharePoint13HighSpeedMigrationImportJob;
        private readonly int sp07HighSpeedMigrationExport_job_dto_type = (int)JobTypes.SharePoint07HighSpeedMigrationExportJob;
        private readonly int sp10HighSpeedMigrationExport_job_dto_type = (int)JobTypes.SharePoint10HighSpeedMigrationExportJob;
        private readonly int sp13HighSpeedMigrationExport_job_dto_type = (int)JobTypes.SharePoint13HighSpeedMigrationExportJob;
        private readonly int sp07HighSpeedMigrationOnline_job_dto_type = (int)JobTypes.SharePoint07HighSpeedMigrationOnlineJob;
        private readonly int sp10HighSpeedMigrationOnline_job_dto_type = (int)JobTypes.SharePoint10HighSpeedMigrationOnlineJob;
        private readonly int sp13HighSpeedMigrationOnline_job_dto_type = (int)JobTypes.SharePoint13HighSpeedMigrationOnlineJob;

        private readonly int sp16HighSpeedMigrationExport_job_dto_type = (int)JobTypes.SharePoint16HighSpeedMigrationExportJob;
        private readonly int sp16HighSpeedMigrationImport_job_dto_type = (int)JobTypes.SharePoint16HighSpeedMigrationImportJob;
        private readonly int sp16HighSpeedMigrationOnline_job_dto_type = (int)JobTypes.SharePoint16HighSpeedMigrationOnlineJob;

        public int SPHSMIGRATION_07ONLINE_JOB_DTO_TYPE
        {
            get { return sp07HighSpeedMigrationOnline_job_dto_type; }
        }

        public int SPHSMIGRATION_10ONLINE_JOB_DTO_TYPE
        {
            get { return sp10HighSpeedMigrationOnline_job_dto_type; }
        }

        public int SPHSMIGRATION_13ONLINE_JOB_DTO_TYPE
        {
            get { return sp13HighSpeedMigrationOnline_job_dto_type; }
        }

        public int SPHSMIGRATION_07EXPORT_JOB_DTO_TYPE
        {
            get { return sp07HighSpeedMigrationExport_job_dto_type; }
        }

        public int SPHSMIGRATION_10EXPORT_JOB_DTO_TYPE
        {
            get { return sp10HighSpeedMigrationExport_job_dto_type; }
        }

        public int SPHSMIGRATION_13EXPORT_JOB_DTO_TYPE
        {
            get { return sp13HighSpeedMigrationExport_job_dto_type; }
        }

        public int SPHSMIGRATION_07IMPORT_JOB_DTO_TYPE
        {
            get { return sp07HighSpeedMigrationImport_job_dto_type; }
        }

        public int SPHSMIGRATION_10IMPORT_JOB_DTO_TYPE
        {
            get { return sp10HighSpeedMigrationImport_job_dto_type; }
        }

        public int SPHSMIGRATION_13IMPORT_JOB_DTO_TYPE
        {
            get { return sp13HighSpeedMigrationImport_job_dto_type; }
        }

        public int SPHSMIGRATION_16EXPORT_JOB_DTO_TYPE
        {
            get { return sp16HighSpeedMigrationExport_job_dto_type; }
        }

        public int SPHSMIGRATION_16IMPORT_JOB_DTO_TYPE
        {
            get { return sp16HighSpeedMigrationImport_job_dto_type; }
        }

        public int SPHSMIGRATION_16ONLINE_JOB_DTO_TYPE
        {
            get { return sp16HighSpeedMigrationOnline_job_dto_type; }
        }
        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_MIGRATION_07_10);
            agentTypes.Add(AGENT_TYPE_MIGRATION_07_13);
            agentTypes.Add(AGENT_TYPE_MIGRATION_10_13);
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_MIGRATION_ID;
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
            List<int> jobTypes = new List<int>();
            jobTypes.Add(SPHSMIGRATION_07ONLINE_JOB_DTO_TYPE);
            jobTypes.Add(SPHSMIGRATION_10ONLINE_JOB_DTO_TYPE);
            jobTypes.Add(SPHSMIGRATION_13ONLINE_JOB_DTO_TYPE);
            jobTypes.Add(SPHSMIGRATION_07EXPORT_JOB_DTO_TYPE);
            jobTypes.Add(SPHSMIGRATION_10EXPORT_JOB_DTO_TYPE);
            jobTypes.Add(SPHSMIGRATION_13EXPORT_JOB_DTO_TYPE);
            jobTypes.Add(SPHSMIGRATION_07IMPORT_JOB_DTO_TYPE);
            jobTypes.Add(SPHSMIGRATION_10IMPORT_JOB_DTO_TYPE);
            jobTypes.Add(SPHSMIGRATION_13IMPORT_JOB_DTO_TYPE);
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
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    public class eRoomMigrationSource : AveModule
    {
        private const string name = "eRoom Migration for eRoom agent";

        public const string AGENT_TYPE_MIGRATION_EROOM_SRC = AgentTypes.AGENT_TYPE_MIGRATION_EROOM_SRC;

        private readonly int eroommigration_job_dto_type = (int)JobTypes.eRoomMigrationJob;

        public int EROOMMIGRATION_JOB_DTO_TYPE
        {
            get { return eroommigration_job_dto_type; }
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_MIGRATION_ID;
            }
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
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_MIGRATION_EROOM_SRC);
            return agentTypes;
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
            jobTypes.Add(EROOMMIGRATION_JOB_DTO_TYPE);
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            return new List<int>();
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    public class eRoomMigrationDestination : AveModule
    {
        private const string name = "eRoom Migration for SharePoint 2010 agent";

        public const string AGENT_TYPE_MIGRATION_EROOM_DEST = AgentTypes.AGENT_TYPE_MIGRATION_EROOM_DEST;

        private readonly int eroommigration_job_dto_type = (int)JobTypes.eRoomMigrationJob;

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_MIGRATION_ID;
            }
        }

        public int EROOMMIGRATION_JOB_DTO_TYPE
        {
            get { return eroommigration_job_dto_type; }
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
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_MIGRATION_EROOM_DEST);
            return agentTypes;
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
            jobTypes.Add(EROOMMIGRATION_JOB_DTO_TYPE);
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            return new List<int>();
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class eRoomMigration : AveModule
    {
        private const string name = "eRoom Migration";

        public const string AGENT_TYPE_MIGRATION_EROOM_SRC = AgentTypes.AGENT_TYPE_MIGRATION_EROOM_SRC;

        public const string AGENT_TYPE_MIGRATION_EROOM_DEST = AgentTypes.AGENT_TYPE_MIGRATION_EROOM_DEST;

        private readonly int eroommigration_job_dto_type = (int)JobTypes.eRoomMigrationJob;
        private readonly int eroom_highspeed_migration_job_dto_type = (int)JobTypes.eRoomHighSpeedMigrationJob;

        public int EROOMMIGRATION_JOB_DTO_TYPE
        {
            get { return eroommigration_job_dto_type; }
        }

        public int EROOM_HIGHSPEED_MIGRATION_JOB_DTO_TYPE
        {
            get { return eroom_highspeed_migration_job_dto_type; }
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_MIGRATION_EROOM_SRC);
            agentTypes.Add(AGENT_TYPE_MIGRATION_EROOM_DEST);
            return agentTypes;
        }

        public List<string> getSrcAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_MIGRATION_EROOM_SRC);
            return agentTypes;
        }

        public List<string> getDestAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_MIGRATION_EROOM_DEST);
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_MIGRATION_ID;
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
            List<int> jobTypes = new List<int>();
            jobTypes.Add(EROOMMIGRATION_JOB_DTO_TYPE);
            return jobTypes;
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
    public class FileMigration : AveModule
    {
        private const string name = "File System Migration";

        public const string AGENT_TYPE_MIGRATION_FILE = AgentTypes.AGENT_TYPE_MIGRATION_FILE;

        private readonly int filemigration_job_dto_type = (int)JobTypes.FileMigrationJob;
        private readonly int file_high_speed_migration_job_dto_type = (int)JobTypes.FileHighSpeedMigrationJob;

        public int FILEMIGRATION_JOB_DTO_TYPE
        {
            get { return filemigration_job_dto_type; }
        }

        public int FILEHIGHSPEEDMIGRATION_JOB_DTO_TYPE
        {
            get { return file_high_speed_migration_job_dto_type; }
        }

        private readonly int filemigration_generate_excel_job_type = (int)JobTypes.FileMigrationGenerateExcelFile;
        private readonly int file_high_speed_migration_generate_excel_job_type = (int)JobTypes.FileHighSpeedMigrationGenerateExcelJob;

        public int FILEMIGRATION_GENERATE_EXCEL_JOB_TYPE
        {
            get { return filemigration_generate_excel_job_type; }
        }

        public int FILEHIGHSPEEDMIGRATION_GENERATE_EXCEL_JOB_TYPE
        {
            get { return file_high_speed_migration_generate_excel_job_type; }
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_MIGRATION_FILE);
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_MIGRATION_ID;
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
            List<int> jobTypes = new List<int>();
            jobTypes.Add(FILEMIGRATION_JOB_DTO_TYPE);
            jobTypes.Add(FILEMIGRATION_GENERATE_EXCEL_JOB_TYPE);
            jobTypes.Add(FILEHIGHSPEEDMIGRATION_JOB_DTO_TYPE);
            jobTypes.Add(FILEHIGHSPEEDMIGRATION_GENERATE_EXCEL_JOB_TYPE);
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
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    public class LivelinkMigrationSource : AveModule
    {
        private const string name = "Livelink Migration for Livelink agent";

        public const string AGENT_TYPE_MIGRATION_LIVELINK_SRC = AgentTypes.AGENT_TYPE_MIGRATION_LIVELINK_SRC;

        private readonly int livelinkmigration_job_dto_type = (int)JobTypes.LivelinkMigrationJob;

        public int LIVELINKMIGRATION_JOB_DTO_TYPE
        {
            get { return livelinkmigration_job_dto_type; }
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_MIGRATION_LIVELINK_SRC);
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_MIGRATION_ID;
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
            List<int> jobTypes = new List<int>();
            jobTypes.Add(LIVELINKMIGRATION_JOB_DTO_TYPE);
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
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    public class LivelinkMigrationDestination : AveModule
    {
        private const string name = "Livelink Migration for SharePoint 2010 agent";

        public const string AGENT_TYPE_MIGRATION_LIVELINK_DEST = AgentTypes.AGENT_TYPE_MIGRATION_LIVELINK_DEST;

        private readonly int livelinkmigration_job_dto_type = (int)JobTypes.LivelinkMigrationJob;

        public int LIVELINKMIGRATION_JOB_DTO_TYPE
        {
            get { return livelinkmigration_job_dto_type; }
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_MIGRATION_LIVELINK_DEST);
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_MIGRATION_ID;
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
            List<int> jobTypes = new List<int>();
            jobTypes.Add(LIVELINKMIGRATION_JOB_DTO_TYPE);
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
    public class LivelinkMigration : AveModule
    {
        private const string name = "LivelinkMigration";

        public const string AGENT_TYPE_MIGRATION_LIVELINK_SRC = AgentTypes.AGENT_TYPE_MIGRATION_LIVELINK_SRC;

        public const string AGENT_TYPE_MIGRATION_LIVELINK_DEST = AgentTypes.AGENT_TYPE_MIGRATION_LIVELINK_DEST;

        private readonly int livelinkmigration_job_dto_type = (int)JobTypes.LivelinkMigrationJob;
        private readonly int livelink_high_speed_migration_job_dto_type = (int)JobTypes.LivelinkHighSpeedMigrationJob;

        public int LIVELINKMIGRATION_JOB_DTO_TYPE
        {
            get { return livelinkmigration_job_dto_type; }
        }

        public int LIVELINK_HIGH_SPEED_MIGRATION_JOB_DTO_TYPE
        {
            get { return livelink_high_speed_migration_job_dto_type; }
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_MIGRATION_LIVELINK_SRC);
            agentTypes.Add(AGENT_TYPE_MIGRATION_LIVELINK_DEST);
            return agentTypes;
        }

        public List<string> getSrcAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_MIGRATION_LIVELINK_SRC);
            return agentTypes;
        }

        public List<string> getDestAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_MIGRATION_LIVELINK_DEST);
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_MIGRATION_ID;
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
            List<int> jobTypes = new List<int>();
            jobTypes.Add(LIVELINKMIGRATION_JOB_DTO_TYPE);
            jobTypes.Add(LIVELINK_HIGH_SPEED_MIGRATION_JOB_DTO_TYPE);
            return jobTypes;
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
    public class NotesMigrationSource : AveModule
    {
        private const string name = "Lotus Notes Migration for Lotus agent";

        public const string AGENT_TYPE_MIGRATION_NOTES_SRC = AgentTypes.AGENT_TYPE_MIGRATION_NOTES_SRC;

        private readonly int notesmigration_job_dto_type = (int)JobTypes.NotesMigrationJob;

        private readonly int notesmigration_high_speed_job_dto_type = (int)JobTypes.NotesHighSpeedMigrationJob;
        public int NOTESMIGRATION_JOB_DTO_TYPE
        {
            get { return notesmigration_job_dto_type; }
        }

        public int NOTESMIGRATION_HIGH_SPEED_JOB_DTO_TYPE
        {
            get { return notesmigration_high_speed_job_dto_type; }
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_MIGRATION_ID;
            }
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
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_MIGRATION_NOTES_SRC);
            return agentTypes;
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
            jobTypes.Add(NOTESMIGRATION_JOB_DTO_TYPE);
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            return new List<int>();
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    public class NotesMigrationDestination : AveModule
    {
        private const string name = "Lotus Notes Migration for SharePoint 2010 agent";

        public const string AGENT_TYPE_MIGRATION_NOTES_DEST = AgentTypes.AGENT_TYPE_MIGRATION_NOTES_DEST;

        private readonly int notesmigration_job_dto_type = (int)JobTypes.NotesMigrationJob;

        public int NOTESMIGRATION_JOB_DTO_TYPE
        {
            get { return notesmigration_job_dto_type; }
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_MIGRATION_ID;
            }
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
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_MIGRATION_NOTES_DEST);
            return agentTypes;
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
            jobTypes.Add(NOTESMIGRATION_JOB_DTO_TYPE);
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            return new List<int>();
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class NotesMigration : AveModule
    {
        private const string name = "Lotus Notes Migration";

        public const string AGENT_TYPE_MIGRATION_NOTES_SRC = AgentTypes.AGENT_TYPE_MIGRATION_NOTES_SRC;

        public const string AGENT_TYPE_MIGRATION_NOTES_DEST = AgentTypes.AGENT_TYPE_MIGRATION_NOTES_DEST;

        private readonly int notesmigration_job_dto_type = (int)JobTypes.NotesMigrationJob;
        private readonly int notes_high_speed_migration_job_dto_type = (int)JobTypes.NotesHighSpeedMigrationJob;

        public int NOTESMIGRATION_JOB_DTO_TYPE
        {
            get { return notesmigration_job_dto_type; }
        }

        public int Notes_HIGH_SPEED_MIGRATION_JOB_DTO_TYPE
        {
            get { return notes_high_speed_migration_job_dto_type; }
        }
        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_MIGRATION_NOTES_SRC);
            agentTypes.Add(AGENT_TYPE_MIGRATION_NOTES_DEST);
            return agentTypes;
        }

        public List<string> getSrcAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_MIGRATION_NOTES_SRC);
            return agentTypes;
        }

        public List<string> getDestAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_MIGRATION_NOTES_DEST);
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_MIGRATION_ID;
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
            List<int> jobTypes = new List<int>();
            jobTypes.Add(NOTESMIGRATION_JOB_DTO_TYPE);
            return jobTypes;
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
    public class QuickPlaceMigrationSource : AveModule
    {
        private const string name = "Quick Place Migration for Quick Place agent";

        public const string AGENT_TYPE_MIGRATION_QUICKPLACE_SRC = AgentTypes.AGENT_TYPE_MIGRATION_QUICK_PLACE_SRC;

        private readonly int quick_place_migration_job_dto_type = (int)JobTypes.QuickPlaceMigrationJob;

        public int QUICKPLACEMIGRATION_JOB_DTO_TYPE
        {
            get { return quick_place_migration_job_dto_type; }
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_MIGRATION_ID;
            }
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
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_MIGRATION_QUICKPLACE_SRC);
            return agentTypes;
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
            jobTypes.Add(quick_place_migration_job_dto_type);
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            return new List<int>();
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    public class QuickPlaceMigrationDestination : AveModule
    {
        private const string name = "Quick Place Migration for SharePoint 2010 agent";

        public const string AGENT_TYPE_MIGRATION_QUICKPLACE_DEST = AgentTypes.AGENT_TYPE_MIGRATION_QUICK_PLACE_DEST;

        private readonly int quick_place_migration_job_dto_type = (int)JobTypes.QuickPlaceMigrationJob;

        public int QUICKPLACEMIGRATION_JOB_DTO_TYPE
        {
            get { return quick_place_migration_job_dto_type; }
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_MIGRATION_ID;
            }
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
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_MIGRATION_QUICKPLACE_DEST);
            return agentTypes;
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
            jobTypes.Add(quick_place_migration_job_dto_type);
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            return new List<int>();
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class QuickPlaceMigration : AveModule
    {
        private const string name = "Quick Place Migration";

        public const string AGENT_TYPE_MIGRATION_QUICKPLACE_SRC = AgentTypes.AGENT_TYPE_MIGRATION_QUICK_PLACE_SRC;

        public const string AGENT_TYPE_MIGRATION_QUICKPLACE_DEST = AgentTypes.AGENT_TYPE_MIGRATION_QUICK_PLACE_DEST;

        private readonly int quick_place_migration_job_dto_type = (int)JobTypes.QuickPlaceMigrationJob;

        public int QUICKPLACEMIGRATION_JOB_DTO_TYPE
        {
            get { return quick_place_migration_job_dto_type; }
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_MIGRATION_QUICKPLACE_SRC);
            agentTypes.Add(AGENT_TYPE_MIGRATION_QUICKPLACE_DEST);
            return agentTypes;
        }

        public List<string> getSrcAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_MIGRATION_QUICKPLACE_SRC);
            return agentTypes;
        }

        public List<string> getDestAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_MIGRATION_QUICKPLACE_DEST);
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_MIGRATION_ID;
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
            List<int> jobTypes = new List<int>();
            jobTypes.Add(quick_place_migration_job_dto_type);
            return jobTypes;
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
    public class DocumentumMigrationSource : AveModule
    {
        private const string name = "Documentum Migration for Documentum agent";

        public const string AGENT_TYPE_MIGRATION_DOCUMENTUM_SRC = AgentTypes.AGENT_TYPE_MIGRATION_DOCUMENTUM_SRC;

        private readonly int documentum_migration_job_dto_type = (int)JobTypes.DocumentumMigrationJob;

        public int DOCUMENTUMMIGRATION_JOB_DTO_TYPE
        {
            get { return documentum_migration_job_dto_type; }
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_MIGRATION_ID;
            }
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
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_MIGRATION_DOCUMENTUM_SRC);
            return agentTypes;
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
            jobTypes.Add(documentum_migration_job_dto_type);
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            return new List<int>();
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    public class DocumentumMigrationDestination : AveModule
    {
        private const string name = "Documentum Migration for SharePoint 2010 agent";

        public const string AGENT_TYPE_MIGRATION_DOCUMENTUM_DEST = AgentTypes.AGENT_TYPE_MIGRATION_DOCUMENTUM_DEST;

        private readonly int documentum_migration_job_dto_type = (int)JobTypes.DocumentumMigrationJob;

        public int DOCUMENTUMMIGRATION_JOB_DTO_TYPE
        {
            get { return documentum_migration_job_dto_type; }
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_MIGRATION_ID;
            }
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
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_MIGRATION_DOCUMENTUM_DEST);
            return agentTypes;
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
            jobTypes.Add(documentum_migration_job_dto_type);
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            return new List<int>();
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class DocumentumMigration : AveModule
    {
        private const string name = "EMC Documentum Migration";

        public const string AGENT_TYPE_MIGRATION_DOCUMENTUM_SRC = AgentTypes.AGENT_TYPE_MIGRATION_DOCUMENTUM_SRC;

        public const string AGENT_TYPE_MIGRATION_DOCUMENTUM_DEST = AgentTypes.AGENT_TYPE_MIGRATION_DOCUMENTUM_DEST;

        private readonly int documentum_migration_job_dto_type = (int)JobTypes.DocumentumMigrationJob;

        private readonly int documentum_high_speed_migration_job_dto_type = (int)JobTypes.DocumentumHighSpeedMigrationJob;

        public int DOCUMENTUMMIGRATION_JOB_DTO_TYPE
        {
            get { return documentum_migration_job_dto_type; }
        }

        public int DOCUMENTUM_HIGHSPEED_MIGRATION_JOB_DTO_TYPE
        {
            get { return documentum_high_speed_migration_job_dto_type; }
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_MIGRATION_DOCUMENTUM_SRC);
            agentTypes.Add(AGENT_TYPE_MIGRATION_DOCUMENTUM_DEST);
            return agentTypes;
        }

        public List<string> getSrcAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_MIGRATION_DOCUMENTUM_SRC);
            return agentTypes;
        }

        public List<string> getDestAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_MIGRATION_DOCUMENTUM_DEST);
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_MIGRATION_ID;
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
            List<int> jobTypes = new List<int>();
            jobTypes.Add(documentum_migration_job_dto_type);
            return jobTypes;
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
