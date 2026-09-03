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
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Administration;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace AvePoint.ObjectModel.Server13
{
    class AveSolutionDeploymentJobDefinition : AveAdministrationServiceJobDefinition, IAveSolutionDeploymentJobDefinition
    {
        private const string mSolutionDeploymentJobDefinition_Type = "Microsoft.SharePoint.Administration.SPSolutionDeploymentJobDefinition";
        private object mSolutionDeploymentJobDefinition;

        public AveSolutionDeploymentJobDefinition(object solutionDeploymentJobDefinition)
            : base(solutionDeploymentJobDefinition)
        {
            mSolutionDeploymentJobDefinition = solutionDeploymentJobDefinition;
        }

        public AveSolutionDeploymentJobDefinition()
            : this(GetAdministrationServiceJobDefinition())
        { }

        private static SPAdministrationServiceJobDefinition GetAdministrationServiceJobDefinition()
        {
            return AveAssemblyUtility.CreateInstance(mSolutionDeploymentJobDefinition_Type, new Type[] { }, new object[] { }) as SPAdministrationServiceJobDefinition;
        }

        public bool Exists(string solutionName, Guid solutionId, uint lcid, IAveFarm farm)
        {
            return Convert.ToBoolean(AveAssemblyUtility.InvokeStaticMethod(mSolutionDeploymentJobDefinition_Type, "Exists", new Type[] { typeof(string), typeof(Guid), typeof(uint), typeof(SPFarm) }, new object[] { solutionName, solutionId, lcid, (farm as AveFarm).Farm }));
        }

        public IAveSolutionDeploymentJobDefinition RetrieveJob(string solutionName, Guid solutionId, uint lcid, IAveFarm farm)
        {
            object solutionDeploymentJobDefinition = AveAssemblyUtility.InvokeStaticMethod(mSolutionDeploymentJobDefinition_Type, "RetrieveJob", new Type[] { typeof(string), typeof(Guid), typeof(uint), typeof(SPFarm) }, new object[] { solutionName, solutionId, lcid, (farm as AveFarm).Farm });
            if (solutionDeploymentJobDefinition == null)
            {
                return null;
            }
            return new AveSolutionDeploymentJobDefinition(solutionDeploymentJobDefinition);
        }

        public AveSolutionDeploymentJobType DeploymentType
        {
            get
            {
                return (AveSolutionDeploymentJobType)AveAssemblyUtility.GetPropertyValue(mSolutionDeploymentJobDefinition, "DeploymentType");
            }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mSolutionDeploymentJobDefinition, "DeploymentType", (SPSolutionDeploymentJobType)value);
            }
        }

        public AveRunningJobStatus JobStatus
        {
            get
            {
                return (AveRunningJobStatus)AveAssemblyUtility.GetPropertyValue(mSolutionDeploymentJobDefinition, "JobStatus");
            }
        }

        public StringCollection StopWebApplications(Collection<IAveWebApplication> webApplications)
        {
            Collection<SPWebApplication> spWebApplications = null;
            if (webApplications != null)
            {
                spWebApplications = new Collection<SPWebApplication>();
                foreach (IAveWebApplication webApp in webApplications)
                {
                    if (webApp != null)
                    {
                        spWebApplications.Add((webApp as AveWebApplication).WebApplication);
                    }
                    else
                    {
                        spWebApplications.Add(null);
                    }
                }
            }
           return (StringCollection)AveAssemblyUtility.InvokeStaticMethod(mSolutionDeploymentJobDefinition_Type, "StopWebApplications", new Type[] { typeof(Collection<SPWebApplication>) }, new object[] { spWebApplications });
        }

        public void StartApplicationPools(StringCollection appPoolIds)
        {
            AveAssemblyUtility.InvokeStaticMethod(mSolutionDeploymentJobDefinition_Type, "StartApplicationPools", new Type[] { typeof(StringCollection) }, new object[] { appPoolIds });
        }
    }
}
