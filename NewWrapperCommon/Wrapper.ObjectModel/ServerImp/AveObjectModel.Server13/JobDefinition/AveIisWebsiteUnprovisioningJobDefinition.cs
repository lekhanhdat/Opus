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
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.ObjectModel.Server13
{
    class AveIisWebsiteUnprovisioningJobDefinition : AveAdministrationServiceJobDefinition, IAveIisWebsiteUnprovisioningJobDefinition
    {
        private SPAdministrationServiceJobDefinition mIisWebsiteUnprovisioningJobDefinition;
        private const string mIisWebsiteUnprovisioningJobDefinition_Type = "Microsoft.SharePoint.Administration.SPIisWebsiteUnprovisioningJobDefinition";

        public AveIisWebsiteUnprovisioningJobDefinition(SPAdministrationServiceJobDefinition iisWebsiteUnprovisioningJobDefinition)
            : base(iisWebsiteUnprovisioningJobDefinition)
        {
            mIisWebsiteUnprovisioningJobDefinition = iisWebsiteUnprovisioningJobDefinition;
        }

        public AveIisWebsiteUnprovisioningJobDefinition(bool deleteWebSites, string[] serverComments, string applicationPoolId, string[] vdirs, Guid webAppId, bool webAppUnprovisioning)
            : base(GetAdministrationServiceJobDefinition(deleteWebSites, serverComments, applicationPoolId, vdirs, webAppId, webAppUnprovisioning))
        {
            mIisWebsiteUnprovisioningJobDefinition = base.JobDefinition as SPAdministrationServiceJobDefinition;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint type name.")]
        private static SPAdministrationServiceJobDefinition GetAdministrationServiceJobDefinition(bool deleteWebSites, string[] serverComments, string applicationPoolId, string[] vdirs, Guid webAppId, bool webAppUnprovisioning)
        {
            return AveAssemblyUtility.CreateInstance(mIisWebsiteUnprovisioningJobDefinition_Type, new Type[] { typeof(bool), typeof(string[]), typeof(string), typeof(string[]), typeof(Guid), typeof(bool) }, new object[] { deleteWebSites, serverComments, applicationPoolId, vdirs, webAppId, webAppUnprovisioning }) as SPAdministrationServiceJobDefinition;
        }

        internal SPAdministrationServiceJobDefinition IisWebsiteUnprovisioningJobDefinition
        {
            get
            {
                return mIisWebsiteUnprovisioningJobDefinition;
            }
        }
    }
}
