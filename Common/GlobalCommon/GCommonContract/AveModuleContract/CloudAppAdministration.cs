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
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AvePoint.GCommon.Contract.AveModuleContract
{
    public class CloudAppAdministration : AveModuleContainer
    {
        private const string MODULE_TYPE_DOCAVE_CLOUDAPPADMIN_NAME = "Cloud App Administration";

        private readonly CloudAppAdminModule cloudappadmin = new CloudAppAdminModule();

        public CloudAppAdminModule CloudAppAdmin
        {
            get { return cloudappadmin; }
        }

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_CLOUDAPPADMIN_ID; }
        }

        public override string Name
        {
            get { return MODULE_TYPE_DOCAVE_CLOUDAPPADMIN_NAME; }
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            result.Add(CloudAppAdmin);
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
    /// Cloud App Admin模块
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class CloudAppAdminModule : AveModule
    {
        private const string name = "CloudAppAdmin";

        #region planType

        private const int cloudappadmin_plan = 1;

        public int CLOUDAPPADMIN_PLAN
        {
            get { return cloudappadmin_plan; }
        }

        #endregion planType

        #region jobType

        public const int cloudappadmin_job = (int)JobTypes.CloudAppAdminJob;

        public int CLOUDAPPADMIN_JOB
        {
            get { return cloudappadmin_job; }
        }

        #endregion jobType

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_CLOUDAPPADMIN_ID;
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
            List<int> planTypes = new List<int>();
            planTypes.Add(CLOUDAPPADMIN_PLAN);
            return planTypes;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
            jobTypes.Add(CLOUDAPPADMIN_JOB);
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
}