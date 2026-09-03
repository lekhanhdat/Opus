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

namespace AvePoint.GCommon.Contract.AveModuleContract
{

    public static class ModuleContract
    {
        public static DocAvePlatform DocAvePlatform = new DocAvePlatform();

    }



    public class DocAvePlatform : AveModuleContainer
    {


        #region agentType
        public const string AGENT_TYPE_SP_VERSION_UNKNOW = AgentTypes.AGENT_TYPE_SP_VERSION_UNKNOW;



        public const string AGENT_TYPE_SP2003 = AgentTypes.AGENT_TYPE_SP2003;


        public const string AGENT_TYPE_SP2007 = AgentTypes.AGENT_TYPE_SP2007;



        public const string AGENT_TYPE_SP2010 = AgentTypes.AGENT_TYPE_SP2010;


        public const string AGENT_TYPE_BPOS = AgentTypes.AGENT_TYPE_BPOS;

        #endregion

        private const int MODULE_TYPE_DOCAVE_PLATFORM_ID = 0;
        private const string MODULE_TYPE_DOCAVE_PLATFORM_NAME = "DocAvePlatform";

        private readonly DataProtection dataprotection = new DataProtection();

        public DataProtection DataProtection
        {
            get { return dataprotection; }
        }

        private readonly Migration migration = new Migration();

        public Migration Migration
        {
            get { return migration; }
        }

        private readonly Compliance compliance = new Compliance();

        public Compliance Compliance
        {
            get { return compliance; }
        }

        private readonly ReportCenter reportcenter = new ReportCenter();

        public ReportCenter ReportCenter
        {
            get { return reportcenter; }
        }

        private readonly StorageOptimization storageoptimization = new StorageOptimization();

        public StorageOptimization StorageOptimization
        {
            get { return storageoptimization; }
        }

        private readonly Administration administration = new Administration();

        public Administration Administration
        {
            get { return administration; }
        }

        private readonly ControlPanel controlpannel = new ControlPanel();

        public ControlPanel ControlPanel
        {
            get { return controlpannel; }
        } 

        private readonly GovernanceAutomation governanceAutomation = new GovernanceAutomation();

        public GovernanceAutomation GovernanceAutomation
        {
            get { return governanceAutomation; }
        } 


        public override int ID
        {
            get { return MODULE_TYPE_DOCAVE_PLATFORM_ID; }
        }

        public override string Name
        {
            get { return MODULE_TYPE_DOCAVE_PLATFORM_NAME; }
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            result.Add(this.DataProtection);
            result.Add(this.Migration);
            result.Add(this.Compliance);
            result.Add(this.ReportCenter);
            result.Add(this.StorageOptimization);
            result.Add(this.Administration);
            result.Add(this.ControlPanel);
            result.Add(this.GovernanceAutomation);
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
}
