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

using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AvePoint.ObjectModel.Common
{
    class AveWorkflowDeploymentService : AveClientObject, IAveWorkflowDeploymentService 
    {
        private IAveWeb mWeb;
        private IAveRequest mRequest;

        public AveWorkflowDeploymentService(IAveWeb web)
        {
            mWeb = web as AveWeb;
            mRequest = ((AveSite)mWeb.Site).Request;
            base.DataCache.AddPropertyies(mRequest.GetWorkflowServicesManager(mWeb.ServerRelativeUrl));
        }

        public void DeleteCollateral(Guid workflowDefinitionId, string leafFileName)
        {
        }

        public void DeleteDefinition(Guid definitionId) 
        {
        }

        public void DeprecateDefinition(Guid definitionId) 
        {
        }

        public IAveWorkflowDefinitionCollection EnumerateDefinitions(bool publishedOnly)
        {
            return new AveWorkflowDefinitionCollection(mWeb, null, "ReusableWorkflowDefinition", mRequest.EnumWorkflowDefinition(mWeb.ServerRelativeUrl, publishedOnly));
        }

        public IDictionary<string, string> GetActivitySignatures(DateTime lastChanged) 
        {
            return null;
        }

        public Uri GetCollateralUri(Guid workflowDefinitionId, string leafFileName) 
        {
            return null;
        }

        public IAveWorkflowDefinition GetDefinition(Guid definitionId)
        {
            var properties = mRequest.GetWorkflowDefinitionById(mWeb.ServerRelativeUrl, definitionId);
            if (properties != null && properties.Count > 0)
            {
                return new AveWorkflowDefinition(mWeb, string.Empty, properties);
            }
            return null;
        }

        public IAveWorkflowDefinition GetDefinition(Guid definitionId, IAveSite parentSite)
        {
            return null;
        }

        public string GetDesignerActions(IAveWeb web)
        {
            return null;
        }

        public string PackageDefinition(Guid definitionId, string packageDefaultFilename, string packageTitle, string packageDescription) 
        {
            return null;
        }

        public void PublishDefinition(Guid definitionId) 
        {
            mRequest.PublishDefinition(mWeb.ServerRelativeUrl, definitionId);
        }

        public void SaveCollateral(Guid workflowDefinitionId, string leafFileName, Stream fileContent) 
        {

        }

        public Guid SaveDefinition(IAveWorkflowDefinition definition)
        {
            return mRequest.SaveDefinition(mWeb.ServerRelativeUrl, definition);
        }

        public string ValidateActivity(string activityXaml) 
        {
            return null;
        }

        public string ScopePath 
        {
            get
            {
                return string.Empty;
            }
        }
    }
}
