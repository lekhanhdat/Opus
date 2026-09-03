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
    class AveFieldUser : AveFieldLookup, IAveFieldUser
    {
        private IAveRequest mRequest;
        private AveList mParentList;
        private AveWeb mWeb;
        private AveFieldCollection mFieldCollection;
        private string mFieldSource;
        private IDictionary<string, object> mContentTypeProp;

        public AveFieldUser(IAveRequest request, AveList list, AveWeb web, string fieldSource, AveFieldCollection fieldCollection, IDictionary<string, object> contentTypeProp, IDictionary<string, object> prop)
            : base(request, list, web, fieldSource, fieldCollection, contentTypeProp, prop)
        {
            mRequest = request;
            mParentList = list;
            mWeb = web;
            mFieldCollection = fieldCollection;
            mFieldSource = fieldSource;
            mContentTypeProp = contentTypeProp;
            base.DataCache.AddPropertyies(prop);
        }
        public bool AllowDisplay
        {
            get
            {
                return base.DataCache.GetProperty<bool>("AllowDisplay");
            }
            set
            {
                base.DataCache.AddChangedProperty("AllowDisplay", value);
            }
        }
        public bool Presence
        {
            get
            {
                return base.DataCache.GetProperty<bool>("Presence");
            }
            set
            {
                base.DataCache.AddChangedProperty("Presence", value);
            }
        }
        public int SelectionGroup
        {
            get
            {
                return base.DataCache.GetProperty<int>("SelectionGroup");
            }
            set
            {
                base.DataCache.AddChangedProperty("SelectionGroup", value);
            }
        }
        public AveFieldUserSelectionMode SelectionMode
        {
            get
            {
                return base.DataCache.GetProperty<AveFieldUserSelectionMode>("SelectionMode");
            }
            set
            {
                base.DataCache.AddChangedProperty("SelectionMode", (int)value);
            }
        }

        public override object GetFieldValue(string value)
        {
            if (value != null)
            {
                if (!AllowMultipleValues)
                {
                    List<string> fieldValue = new List<string>();
                    int lookupId;
                    string lookupValue;
                    if (AveSPUtility.TryParseMultiColumnValue(value, out fieldValue))
                    {
                        if (fieldValue.Count == 2)
                        {
                            lookupValue = fieldValue[1];
                            if (int.TryParse(fieldValue[0], out lookupId))
                            {
                                return new AveFieldUserValue(this.mWeb, lookupId, lookupValue);
                            }
                        }
                    }
                }
                else
                {
                    return new AveFieldUserValueCollection(this.mWeb, value);
                }
            }
            return null;
        }
    }
}
