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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Common
{
    class AveProjectEnterpriseProjectType : AveClientObject, IAveProjectEnterpriseProjectType
    {
        private IAveRequest mRequest;

        public AveProjectEnterpriseProjectType(IAveRequest request, Dictionary<string, object> prop)
        {
            this.mRequest = request;
            base.DataCache.AddPropertyies(prop);
        }

        public string Description
        {
            get
            {
                return base.DataCache.GetProperty<string>("Description");
            }

            set
            {
                base.DataCache.AddChangedProperty("Description", value);
            }
        }

        public Guid Id
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("Id");
            }
        }

        public string ImageUrl
        {
            get
            {
                return base.DataCache.GetProperty<string>("ImageUrl");
            }

            set
            {
                base.DataCache.AddChangedProperty("ImageUrl", value);
            }
        }

        public bool IsDefault
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsDefault");
            }

            set
            {
                base.DataCache.AddChangedProperty("IsDefault", value);
            }
        }

        public bool IsManaged
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsManaged");
            }

            set
            {
                base.DataCache.AddChangedProperty("IsManaged", value);
            }
        }

        public string Name
        {
            get
            {
                return base.DataCache.GetProperty<string>("Name");
            }

            set
            {
                base.DataCache.AddChangedProperty("Name", value);
            }
        }

        public int Order
        {
            get
            {
                return base.DataCache.GetProperty<int>("Order");
            }

            set
            {
                base.DataCache.AddChangedProperty("Order", value);
            }
        }

        public bool PermissionSyncEnable
        {
            get
            {
                return base.DataCache.GetProperty<bool>("PermissionSyncEnable");
            }
            set
            {
                base.DataCache.AddChangedProperty("PermissionSyncEnable", value);
            }
        }
        public bool TaskListSyncEnable
        {
            get
            {
                return base.DataCache.GetProperty<bool>("TaskListSyncEnable");
            }
            set
            {
                base.DataCache.AddChangedProperty("TaskListSyncEnable", value);
            }
        }

        public AveEnterpriseProjectTypeSiteCreationOptions SiteCreationOption
        {
            get
            {
                //client api有问题，获取出来的值有偏差
                AveEnterpriseProjectTypeSiteCreationOptions createOption = base.DataCache.GetProperty<AveEnterpriseProjectTypeSiteCreationOptions>("SiteCreationOption");
                if (createOption == AveEnterpriseProjectTypeSiteCreationOptions.AskOnPublish)
                {
                    base.DataCache.AddProperty("SiteCreationOption",AveEnterpriseProjectTypeSiteCreationOptions.CreateOnFirstPublish);
                }
                else if (createOption == AveEnterpriseProjectTypeSiteCreationOptions.CreateOnFirstPublish)
                {
                    base.DataCache.AddProperty("SiteCreationOption",AveEnterpriseProjectTypeSiteCreationOptions.NotSpecified);
                }
                else if (createOption == AveEnterpriseProjectTypeSiteCreationOptions.NotSpecified)
                {
                    base.DataCache.AddProperty("SiteCreationOption",AveEnterpriseProjectTypeSiteCreationOptions.AskOnPublish);
                }
                return base.DataCache.GetProperty<AveEnterpriseProjectTypeSiteCreationOptions>("SiteCreationOption");
            }
            set
            {
                if (value == AveEnterpriseProjectTypeSiteCreationOptions.AskOnPublish)
                {
                    base.DataCache.AddChangedProperty("SiteCreationOption", AveEnterpriseProjectTypeSiteCreationOptions.NotSpecified);
                }
                else if (value == AveEnterpriseProjectTypeSiteCreationOptions.CreateOnFirstPublish)
                {
                    base.DataCache.AddChangedProperty("SiteCreationOption", AveEnterpriseProjectTypeSiteCreationOptions.AskOnPublish);
                }
                else if (value == AveEnterpriseProjectTypeSiteCreationOptions.NotSpecified)
                {
                    base.DataCache.AddChangedProperty("SiteCreationOption", AveEnterpriseProjectTypeSiteCreationOptions.CreateOnFirstPublish);
                }
            }
        }

        public IAveProjectDetailPageCollection ProjectDetailPages
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("ProjectDetailPages"))
                {
                    var props = base.DataCache.GetProperty<List<Dictionary<string, object>>>("ProjectDetailPages" + AveObjectModelConstant.ObjectPropertySuffix);
                    AveProjectDetailPageCollection pages = new AveProjectDetailPageCollection(this.mRequest, props);
                    base.DataCache.AddProperty("ProjectDetailPages",pages);
                }
                return base.DataCache.GetProperty<IAveProjectDetailPageCollection>("ProjectDetailPages");
            }
        }

        public string SiteCreationURL
        {
            get
            {
                return base.DataCache.GetProperty<string>("SiteCreationURL");
            }
            set
            {
                base.DataCache.AddChangedProperty("SiteCreationURL", value);
            }
        }

        public Guid ProjectPlanTemplateId
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("ProjectPlanTemplateId");
            }

            set
            {
                base.DataCache.AddChangedProperty("ProjectPlanTemplateId", value);
            }
        }

        public Guid WorkflowAssociationId
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("WorkflowAssociationId");
            }

            set
            {
                base.DataCache.AddChangedProperty("WorkflowAssociationId", value);
            }
        }

        public string WorkflowAssociationName
        {
            get
            {
                return base.DataCache.GetProperty<string>("WorkflowAssociationName");
            }

            set
            {
                base.DataCache.AddChangedProperty("WorkflowAssociationName", value);
            }
        }

        public int WorkspaceTemplateLCID
        {
            get
            {
                return base.DataCache.GetProperty<int>("WorkspaceTemplateLCID");
            }
            set
            {
                base.DataCache.AddChangedProperty("WorkspaceTemplateLCID", value);
            }
        }

        public string WorkspaceTemplateName
        {
            get
            {
                return base.DataCache.GetProperty<string>("WorkspaceTemplateName");
            }

            set
            {
                base.DataCache.AddChangedProperty("WorkspaceTemplateName", value);
            }
        }

        #region Method
        public void Update()
        {
            Dictionary<string, object> eptProp = mRequest.UpdateEnterpriseProjectType(this.Id, base.DataCache.ChangedProperties);
            if (eptProp.Count > 0)
            {
                base.DataCache.UpdateProperties(eptProp);
            }
        }

        public List<AveProjectDetailPageInfo> GetDetailPages()
        {
            return mRequest.GetDetailPages(this.Id);
        }

        public void UpdateEnterpriseTypeByPSI(AveProjectEnterpriseProjectTypeInfo eptInfo)
        {
            mRequest.UpdateEnterpriseTypeByPSI(this.Id, eptInfo);
        }

        #endregion
    }
}
