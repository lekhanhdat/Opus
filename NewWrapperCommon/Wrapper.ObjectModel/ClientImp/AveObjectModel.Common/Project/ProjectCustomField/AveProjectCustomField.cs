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
    class AveProjectCustomField : AveClientObject, IAveProjectCustomField
    {
        private IAveRequest mRequest;
        private IAveProjectEntityType mEntityType;
        private IAveProjectLookupEntryCollection mLookupEntries;

        public AveProjectCustomField(IAveRequest request, Dictionary<string, object> prop)
        {
            this.mRequest = request;
            base.DataCache.AddPropertyies(prop);
        }

        #region Properties
        public Guid AppAlternateId
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("AppAlternateId");
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

        public IAveProjectEntityType EntityType
        {
            get
            {
                if (this.mEntityType == null)
                {
                    var props = base.DataCache.GetProperty<Dictionary<string, object>>("EntityType");
                    this.mEntityType = new AveProjectEntityType(this.mRequest, props);
                    base.DataCache.RemoveProperty("EntityType");
                }
                return this.mEntityType;
            }
        }

        public int FieldType
        {
            get
            {
                return base.DataCache.GetProperty<int>("FieldType");
            }
        }

        public string Formula
        {
            get
            {
                return base.DataCache.GetProperty<string>("Formula");
            }

            set
            {
                base.DataCache.AddChangedProperty("Formula", value);
            }
        }

        public Guid Id
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("Id");
            }
        }

        public string InternalName
        {
            get
            {
                return base.DataCache.GetProperty<string>("InternalName");
            }
        }

        public bool IsEditableInVisibility
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsEditableInVisibility");
            }

            set
            {
                base.DataCache.AddChangedProperty("IsEditableInVisibility", value);
            }
        }

        public bool IsMultilineText
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsMultilineText");
            }

            set
            {
                base.DataCache.AddChangedProperty("IsMultilineText", value);
            }
        }

        public bool IsRequired
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsRequired");
            }

            set
            {
                base.DataCache.AddChangedProperty("IsRequired", value);
            }
        }

        public bool IsWorkflowControlled
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsWorkflowControlled");
            }

            set
            {
                base.DataCache.AddChangedProperty("IsWorkflowControlled", value);
            }
        }

        public bool LookupAllowMultiSelect
        {
            get
            {
                return base.DataCache.GetProperty<bool>("LookupAllowMultiSelect");
            }
        }

        public Guid LookupDefaultValue
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("LookupDefaultValue");
            }

            set
            {
                base.DataCache.AddChangedProperty("LookupDefaultValue", value);
            }
        }

        public IAveProjectLookupEntryCollection LookupEntries
        {
            get
            {
                if (this.mLookupEntries == null)
                {
                    var props = base.DataCache.GetProperty<List<Dictionary<string, object>>>("LookupEntries");
                    this.mLookupEntries = new AveProjectLookupEntryCollection(this.mRequest, props);
                    base.DataCache.RemoveProperty("LookupEntries");
                }
                return this.mLookupEntries;
            }
        }

        public string LookupTable
        {
            get
            {
                return base.DataCache.GetProperty<string>("LookupTable");
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

        public bool RollsDownToAssignments
        {
            get
            {
                return base.DataCache.GetProperty<bool>("RollsDownToAssignments");
            }

            set
            {
                base.DataCache.AddChangedProperty("RollsDownToAssignments", value);
            }
        }
        #endregion

        #region Method

        public void Update()
        {
            Dictionary<string, object> fieldProp = mRequest.UpdateCustomField(this.Id, base.DataCache.ChangedProperties);
            if (fieldProp.Count > 0)
            {
                base.DataCache.UpdateProperties(fieldProp);
            }
        }

        #endregion
    }
}
