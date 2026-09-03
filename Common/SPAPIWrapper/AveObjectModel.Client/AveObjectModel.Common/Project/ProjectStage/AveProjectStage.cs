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
    class AveProjectStage : AveClientObject, IAveProjectStage
    {
        private IAveRequest mRequest;
        private IAveProjectStageCustomFieldCollection mCustomFields;
        private IAveProjectStageDetailPageCollection mProjectDetailPages;
        private IAveProjectDetailPage mWorkflowStatusPage;

        public AveProjectStage(IAveRequest request, Dictionary<string, object> prop)
        {
            this.mRequest = request;
            base.DataCache.AddPropertyies(prop);
        }
        
        public int Behavior
        {
            get
            {
                return base.DataCache.GetProperty<int>("Behavior");
            }
        }

        public bool CheckInRequired
        {
            get
            {
                return base.DataCache.GetProperty<bool>("CheckInRequired");
            }

            set
            {
                base.DataCache.AddChangedProperty("CheckInRequired", value);
            }
        }

        public IAveProjectStageCustomFieldCollection CustomFields
        {
            get
            {
                if (this.mCustomFields == null)
                {
                    var props = base.DataCache.GetProperty<List<Dictionary<string, object>>>("CustomFields");
                    this.mCustomFields = new AveProjectStageCustomFieldCollection(this.mRequest, props);
                    base.DataCache.RemoveProperty("CustomFields");
                }
                return this.mCustomFields;
            }
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

        public string Phase
        {
            get
            {
                return base.DataCache.GetProperty<string>("Phase");
            }
        }

        public IAveProjectStageDetailPageCollection ProjectDetailPages
        {
            get
            {
                if (this.mProjectDetailPages == null)
                {
                    var props = base.DataCache.GetProperty<List<Dictionary<string, object>>>("ProjectDetailPages");
                    this.mProjectDetailPages = new AveProjectStageDetailPageCollection(this.mRequest, props);
                    base.DataCache.RemoveProperty("ProjectDetailPages");
                }
                return this.mProjectDetailPages;
            }

            set
            {
                //base.DataCache.AddChangedProperty("ProjectDetailPages", value);
            }
        }

        public string SubmitDescription
        {
            get
            {
                return base.DataCache.GetProperty<string>("SubmitDescription");
            }

            set
            {
                base.DataCache.AddChangedProperty("SubmitDescription", value);
            }
        }

        public IAveProjectDetailPage WorkflowStatusPage
        {
            get
            {
                if (this.mWorkflowStatusPage == null)
                {
                    var props = base.DataCache.GetProperty<Dictionary<string, object>>("WorkflowStatusPage");
                    this.mWorkflowStatusPage = new AveProjectDetailPage(this.mRequest, props);
                    base.DataCache.RemoveProperty("WorkflowStatusPage");
                }
                return this.mWorkflowStatusPage;
            }

            set
            {
                //base.DataCache.AddChangedProperty("WorkflowStatusPage", value);
            }
        }

        #region Method

        public void Update()
        {
            Dictionary<string, object> stageProp = mRequest.UpdateStage(this.Id, base.DataCache.ChangedProperties);
            if (stageProp.Count > 0)
            {
                base.DataCache.UpdateProperties(stageProp);
            }
        }

        #endregion
    }
}
