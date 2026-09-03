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
    class AveProjectLookupTable : AveClientObject, IAveProjectLookupTable
    {
        private IAveRequest mRequest;
        private IAveProjectLookupEntryCollection mEntries;
        private List<AveProjectLookupMask> mMasks;

        public AveProjectLookupTable(IAveRequest request, Dictionary<string, object> prop)
        {
            this.mRequest = request;
            base.DataCache.AddPropertyies(prop);
        }

        public Guid AppAlternateId
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("AppAlternateId");
            }
        }

        public IAveProjectLookupEntryCollection Entries
        {
            get
            {
                if (this.mEntries == null)
                {
                    var props = base.DataCache.GetProperty<List<Dictionary<string, object>>>("Entries");
                    this.mEntries = new AveProjectLookupEntryCollection(this.mRequest, props);
                    base.DataCache.RemoveProperty("Entries");
                }
                return this.mEntries;
            }
        }

        public AveProjectCustomFieldType FieldType
        {
            get
            {
                return (AveProjectCustomFieldType)base.DataCache.GetProperty<int>("FieldType");
            }
        }

        public Guid Id
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("Id");
            }
        }

        public IEnumerable<IAveProjectLookupMask> Masks
        {
            get
            {
                if (this.mMasks == null)
                {
                    var props = base.DataCache.GetProperty<List<Dictionary<string, object>>>("Masks");
                    this.mMasks = new List<AveProjectLookupMask>(props.Count);
                    foreach (var prop in props)
                    {
                        var mask = new AveProjectLookupMask(this.mRequest, prop);
                        this.mMasks.Add(mask);
                    }
                    base.DataCache.RemoveProperty("Masks");
                }
                return this.mMasks;
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

        public int SortOrder
        {
            get
            {
                return base.DataCache.GetProperty<int>("SortOrder");
            }

            set
            {
                base.DataCache.AddChangedProperty("SortOrder", value);
            }
        }

        #region Method

        public void Update()
        {
            Dictionary<string, object> tableProp = mRequest.UpdateLookupTable(this.Id, base.DataCache.ChangedProperties);
            if (tableProp.Count > 0)
            {
                base.DataCache.UpdateProperties(tableProp);
            }
        }

        #endregion
    }
}
