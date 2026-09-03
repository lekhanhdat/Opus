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
using Microsoft.SharePoint.WorkflowServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;
using AvePoint.GCommon;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveWorkflowDeploymentService : SolutionProvider, IAveWorkflowDeploymentService
    {
        // Methods
        //protected IAveWorkflowDeploymentService();
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveWorkflowDeploymentService));
        private SPSite mParentSite = null;
        private WorkflowDeploymentService mWorkflowDeploymentService = null;

        public AveWorkflowDeploymentService(WorkflowDeploymentService deploymentService) 
        {
            mWorkflowDeploymentService = deploymentService;
        }

        public void DeleteCollateral(Guid workflowDefinitionId, string leafFileName) 
        {
            mWorkflowDeploymentService.DeleteCollateral(workflowDefinitionId, leafFileName);
        }

        public void DeleteDefinition(Guid definitionId)
        {
            mWorkflowDeploymentService.DeleteDefinition(definitionId);
        }

        public void DeprecateDefinition(Guid definitionId) 
        {
            mWorkflowDeploymentService.DeprecateDefinition(definitionId);
        }

        public IAveWorkflowDefinitionCollection EnumerateDefinitions(bool publishedOnly) 
        {
            return new AveWorkflowDefinitionCollection(mWorkflowDeploymentService.EnumerateDefinitions(publishedOnly));
        }

        public IDictionary<string, string> GetActivitySignatures(DateTime lastChanged) 
        {
            return mWorkflowDeploymentService.GetActivitySignatures(lastChanged);
        }

        public Uri GetCollateralUri(Guid workflowDefinitionId, string leafFileName) 
        {
            return mWorkflowDeploymentService.GetCollateralUri(workflowDefinitionId, leafFileName);
        }

        public IAveWorkflowDefinition GetDefinition(Guid definitionId)
        {
            WorkflowDefinition definition = mWorkflowDeploymentService.GetDefinition(definitionId);
            if (definition != null)
            {
                return new AveWorkflowDefinition(definition);
            }
            return null;
        }

        public IAveWorkflowDefinition GetDefinition(Guid workflowDefinitionId, IAveSite parentSite)
        {
            if (Guid.Empty == workflowDefinitionId)
            {
                throw new ArgumentNullException("definitionId");
            }
            mParentSite = ((AveSite)parentSite).Site;
            WorkflowServicesContext context = (WorkflowServicesContext)AveAssemblyUtility.GetFieldValue(mWorkflowDeploymentService, "context");
            Object workflowStore = AveAssemblyUtility.CreateInstance("Microsoft.SharePoint.WorkflowServices.WorkflowStore", new Type[] { typeof(SPWeb) }, new object[] { context.Web });
            Object file = AveAssemblyUtility.InvokeMethod(workflowStore, "GetFile", new Type[] { typeof(Guid) }, new object[] { workflowDefinitionId });
            if (file == null)
            {
                return null;
            }
            WorkflowDefinition definition = CreateDefinitionFromFile(file);
            return new AveWorkflowDefinition(definition);
        }

        private WorkflowDefinition CreateDefinitionFromFile(object file)
        {
            WorkflowDefinition definition = new WorkflowDefinition(delegate
            {
                string str;
                //using (Stream stream = file.GetBlob())
                using (Stream stream = (Stream)AveAssemblyUtility.InvokeMethod(file, "GetBlob"))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        str = reader.ReadToEnd();
                    }
                }
                return str;
            });
            definition.SetProperties((IDictionary<String, String>)AveAssemblyUtility.InvokeMethod(file, "GetDefinitionMetadataFields", new Type[] { typeof(bool) }, new object[] { false }));
            definition.Id = (Guid)AveAssemblyUtility.GetPropertyValue(file, "Id");
            definition.Published = (int)AveAssemblyUtility.GetPropertyValue(file, "PublishState") == 3;
            definition.DraftVersion = GetFileVersion(file);
            return definition;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "List name to use.")] 
        private string GetFileVersion(Object file)
        {
            SPSecurity.CodeToRunElevated secureCode = null;
            string version = null;
            SPListItem item = (SPListItem)AveAssemblyUtility.GetFieldValue(file, "item");
            if ((item != null) && (item.File != null))
            {
                item.Web.CheckPermissions(SPBasePermissions.EditListItems);
                if (secureCode == null)
                {
                    secureCode = delegate
                    {
                        using (SPWeb web = mParentSite.OpenWeb(item.Web.ServerRelativeUrl))
                        {
                            try
                            {
                                SPList list = web.Lists["wfsvc"];
                                if (list != null)
                                {
                                    SPListItem itemById = list.GetItemById(item.ID);
                                    version = itemById.File.ETag;
                                }
                            }
                            catch (ArgumentException)
                            {
                                logger.Info("An error occurred while getting the workflow file version");
                            }
                        }
                    };
                }
                SPSecurity.RunWithElevatedPrivileges(secureCode);
            }
            return version;
        }

        public string GetDesignerActions(IAveWeb web)
        {
            AveWeb aveWeb = (AveWeb)web;
            return this.mWorkflowDeploymentService.GetDesignerActions(aveWeb.Web);
        }

        public string PackageDefinition(Guid definitionId, string packageDefaultFilename, string packageTitle, string packageDescription) 
        {
            return this.mWorkflowDeploymentService.PackageDefinition(definitionId, packageDefaultFilename, packageTitle, packageDescription);
        }
        //protected string PackageDefinitionFolder(IAveFolder folder, string packageDefaultFilename, string packageTitle, string packageDescription, IAveList assetLibrary, Dictionary<string, string> configuration);

        public void PublishDefinition(Guid definitionId) 
        {
            this.mWorkflowDeploymentService.PublishDefinition(definitionId);
        }

        public void SaveCollateral(Guid workflowDefinitionId, string leafFileName, Stream fileContent) 
        {
            this.mWorkflowDeploymentService.SaveCollateral(workflowDefinitionId, leafFileName, fileContent);
        }

        public Guid SaveDefinition(IAveWorkflowDefinition definition) 
        {
            AveWorkflowDefinition aveWorkflowDefinition = definition as AveWorkflowDefinition;
            WorkflowDefinition wFDefinition = null;
            if (aveWorkflowDefinition != null)
            {
                wFDefinition = aveWorkflowDefinition.WFDefinition;
            }
            return this.mWorkflowDeploymentService.SaveDefinition(wFDefinition);
        }

        public string ValidateActivity(string activityXaml) 
        {
            return mWorkflowDeploymentService.ValidateActivity(activityXaml);
        }
        // Properties
        public string ScopePath 
        {
            get 
            {
                return mWorkflowDeploymentService.ScopePath;
            }
        }
    }
}
