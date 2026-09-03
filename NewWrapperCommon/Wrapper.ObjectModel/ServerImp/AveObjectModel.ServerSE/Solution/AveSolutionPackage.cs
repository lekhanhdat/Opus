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



namespace AvePoint.ObjectModel.ServerSE
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AvePoint.Wrapper.Common;
    #endregion

    abstract class AveSolutionPackage : IAveSolutionPackage
    {
        private object mSolutionPackage;

        public AveSolutionPackage(object solutionPackage)
        {
            mSolutionPackage = solutionPackage;
        }

        public string Name
        {
            get
            {
                return (string)AveAssemblyUtility.GetPropertyValue(mSolutionPackage, "Name");
            }
        }

        public bool ContainsCasPolicy
        {
            get
            {
                return (bool)AveAssemblyUtility.GetPropertyValue(mSolutionPackage, "ContainsCasPolicy");
            }
        }

        public bool ContainsGlobalAssembly
        {
            get
            {
                return (bool)AveAssemblyUtility.GetPropertyValue(mSolutionPackage, "ContainsGlobalAssembly");
            }
        }

        public bool ContainsWebApplicationResource
        {
            get
            {
                return (bool)AveAssemblyUtility.GetPropertyValue(mSolutionPackage, "ContainsWebApplicationResource");
            }
        }

        public AveServerRole DeploymentServerType
        {
            get
            {
                return (AveServerRole)AveAssemblyUtility.GetPropertyValue(mSolutionPackage, "DeploymentServerType");
            }
        }

        public Guid SolutionId
        {
            get
            {
                return (Guid)AveAssemblyUtility.GetFieldValue(mSolutionPackage, "SolutionId");
            }
        }


        public List<AveSolutionFeature> Features
        {
            get { throw new NotImplementedException(); }
        }

        public List<AveSolutionDependency> SolutionDependencies
        {
            get { throw new NotImplementedException(); }
        }

        public List<Dictionary<string, string>> WebTemplatesInfo
        {
            get { throw new NotImplementedException(); }
        }
    }
}
