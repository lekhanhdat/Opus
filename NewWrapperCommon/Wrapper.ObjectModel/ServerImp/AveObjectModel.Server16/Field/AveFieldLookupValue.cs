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



using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server16
{
    class AveFieldLookupValue : IAveFieldLookupValue
    {
        private SPFieldLookupValue mFieldLookupValue;

        public AveFieldLookupValue(SPFieldLookupValue fieldLookupValue)
        {
            mFieldLookupValue = fieldLookupValue;
        }

        public AveFieldLookupValue()
        { }

        public AveFieldLookupValue(int lookupid, string lookupValue)
        {
            mFieldLookupValue = new SPFieldLookupValue(lookupid, lookupValue);
        }

        internal SPFieldLookupValue FieldLookupValue
        {
            get
            {
                return mFieldLookupValue;
            }
            set
            {
                mFieldLookupValue = value;
            }
        }

        #region IAveFieldLookupValue Members

        public int LookupId
        {
            get
            {
                return mFieldLookupValue.LookupId;
            }
            set
            {
                mFieldLookupValue.LookupId = value;
            }
        }

        public string LookupValue
        {
            get { return mFieldLookupValue.LookupValue; }
        }

        public override string ToString()
        {
            return mFieldLookupValue.ToString();
        }

        #endregion
    }
}
