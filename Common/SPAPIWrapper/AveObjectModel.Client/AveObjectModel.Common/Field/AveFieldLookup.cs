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
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Common
{
    class AveFieldLookup : AveField, IAveFieldLookup
    {
        private IAveRequest mRequest;
        private AveList mParentList;
        private AveWeb mWeb;
        private AveFieldCollection mFieldCollection;
        private string mFieldSource;
        private IDictionary<string, object> mContentTypeProp;

        public AveFieldLookup(IAveRequest request, AveList list, AveWeb web, string fieldSource, AveFieldCollection fieldCollection, IDictionary<string, object> contentTypeProp, IDictionary<string, object> prop)
            : base(request,list,web,fieldSource,fieldCollection,contentTypeProp,prop)
        {
            mRequest = request;
            mParentList = list;
            mWeb = web;
            mFieldCollection = fieldCollection;
            mFieldSource = fieldSource;
            mContentTypeProp = contentTypeProp;
            base.DataCache.AddPropertyies(prop);
        }
        public bool AllowMultipleValues 
        {
            get
            {
                return base.DataCache.GetProperty<bool>("AllowMultipleValues");
            }
            set
            {
                base.DataCache.AddChangedProperty("AllowMultipleValues", value);
            }
        }
        public bool IsRelationship
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsRelationship");
            }
            set
            {
                base.DataCache.AddChangedProperty("IsRelationship", value);
            }
        }
        public string LookupField 
        {
            get
            {
                return base.DataCache.GetProperty<string>("LookupField");
            }
            set
            {
                base.DataCache.AddChangedProperty("LookupField", value);
            }
        }
        public string LookupList
        {
            get
            {
                return base.DataCache.GetProperty<string>("LookupList");
            }
            set
            {
                string lookupList = base.DataCache.GetProperty<string>("LookupList");
                if (string.IsNullOrEmpty(lookupList))
                {
                    lookupList = Guid.Empty.ToString();
                }
                if (!string.IsNullOrEmpty(value) &&
                    !string.IsNullOrEmpty(lookupList) &&
                    !value.Trim(new char[] { '{', '}' }).Equals(lookupList.Trim(new char[] { '{', '}' }), StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("LookupList", value);
                }
            }
        }
        public Guid LookupWebId 
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("LookupWebId");
            }
            set
            {
                base.DataCache.AddChangedProperty("LookupWebId", value);
            }
        }
        public string PrimaryFieldId 
        {
            get
            {
                return base.DataCache.GetProperty<string>("PrimaryFieldId");
            }
            set
            {
                base.DataCache.AddChangedProperty("PrimaryFieldId", value);
            }
        }
        public AveRelationshipDeleteBehavior RelationshipDeleteBehavior
        {
            get
            {
                return base.DataCache.GetProperty<AveRelationshipDeleteBehavior>("RelationshipDeleteBehavior");
            }
            set
            {
                base.DataCache.AddChangedProperty("RelationshipDeleteBehavior", (int)value);
            }
        }
        
        public int Version
        {
            get
            {
                return base.DataCache.GetProperty<int>("Version");
            }
        }

        public bool PrependId
        {
            get
            {
                return base.DataCache.GetProperty<bool>("PrependId");
            }
            set
            {
                base.DataCache.AddChangedProperty("PrependId", value);
            }
        }

        public bool UnlimitedLengthInDocumentLibrary
        {
            get
            {
                return base.DataCache.GetProperty<bool>("UnlimitedLengthInDocumentLibrary");
            }
            set
            {
                base.DataCache.AddChangedProperty("UnlimitedLengthInDocumentLibrary", value);
            }
        }

        public bool CountRelated
        {
            get
            {
                return base.DataCache.GetProperty<bool>("CountRelated");
            }
            set
            {
                base.DataCache.AddChangedProperty("CountRelated", value);
            }
        }


        public bool IsDependentLookup
        {
            get
            {
                return (!string.IsNullOrEmpty(this.PrimaryFieldId) && AveSPCommonUtility.IsGuid(this.PrimaryFieldId));
            }
        }



        public string LookupListTitle
        {
            get { throw new NotImplementedException(); }
        }
    }
}
