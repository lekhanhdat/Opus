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

namespace AvePoint.GCommon.Contract.AveModuleContract
{
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    public class GovernanceAutomation : AveModuleContainer
    {
        private const string MODULE_TYPE_DOCAVE_GOVERNANCE_AUTOMATION_NAME = "Governance Automation";

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_GOVERNANCE_AUTOMATION_ID; }
        }

        public override string Name
        {
            get { return MODULE_TYPE_DOCAVE_GOVERNANCE_AUTOMATION_NAME; }
        }

        private readonly SubGovernanceAutomation subGovernanceAutomation = new SubGovernanceAutomation();

        public SubGovernanceAutomation SubGovernanceAutomation
        {
            get { return subGovernanceAutomation; }
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            result.Add(SubGovernanceAutomation);
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
            get { return DisplayMode.Available; }
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    public class SubGovernanceAutomation : AveModule
    {
        private const string MODULE_TYPE_DOCAVE_GOVERNANCE_AUTOMATION_NAME = "Governance Automation";
        public const string AGENT_TYPE_GOVERNANCE_AUTOMATION = AgentTypes.AGENT_TYPE_GOVERNANCE_AUTOMATION;


        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_GOVERNANCE_AUTOMATION_ID; }
        }

        public override string Name
        {
            get { return MODULE_TYPE_DOCAVE_GOVERNANCE_AUTOMATION_NAME; }
        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_GOVERNANCE_AUTOMATION);
            return agentTypes;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
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
