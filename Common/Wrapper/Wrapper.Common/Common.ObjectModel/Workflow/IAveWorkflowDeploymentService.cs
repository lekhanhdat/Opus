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
using System.IO;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Common
{
    public interface IAveWorkflowDeploymentService
    {
        // Methods
        //protected IAveWorkflowDeploymentService();
        void DeleteCollateral(Guid workflowDefinitionId, string leafFileName);

        void DeleteDefinition(Guid definitionId);

        void DeprecateDefinition(Guid definitionId);

        IAveWorkflowDefinitionCollection EnumerateDefinitions(bool publishedOnly);

        IDictionary<string, string> GetActivitySignatures(DateTime lastChanged);

        Uri GetCollateralUri(Guid workflowDefinitionId, string leafFileName);

        IAveWorkflowDefinition GetDefinition(Guid definitionId);

        string GetDesignerActions(IAveWeb web);

        string PackageDefinition(Guid definitionId, string packageDefaultFilename, string packageTitle, string packageDescription);
        //protected string PackageDefinitionFolder(IAveFolder folder, string packageDefaultFilename, string packageTitle, string packageDescription, IAveList assetLibrary, Dictionary<string, string> configuration);

        void PublishDefinition(Guid definitionId);

        void SaveCollateral(Guid workflowDefinitionId, string leafFileName, Stream fileContent);

        Guid SaveDefinition(IAveWorkflowDefinition definition);

        string ValidateActivity(string activityXaml);
        // Properties
        string ScopePath { get; }
    }
}
