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
    class AveFieldUserValue : AveFieldLookupValue, IAveFieldUserValue
    {
        private SPFieldUserValue mFieldUserValue;
        private IAveUser mUser;
        private AveWeb mWeb;

        public AveFieldUserValue(SPFieldUserValue fieldUserValue)
            : base(fieldUserValue)
        {
            mFieldUserValue = fieldUserValue;
        }

        public AveFieldUserValue(IAveWeb web, int lookupId, string lookupValue)
            : base(lookupId, lookupValue)
        {
            mWeb = web as AveWeb;
            mFieldUserValue = new SPFieldUserValue(mWeb.Web, lookupId, lookupValue);
        }

        public AveFieldUserValue(IAveWeb web, string fieldValue)
        {
            mWeb = web as AveWeb;
            mFieldUserValue = new SPFieldUserValue(mWeb.Web, fieldValue);
            base.FieldLookupValue = mFieldUserValue;
        }

        internal SPFieldUserValue FieldUserValue
        {
            get
            {
                return mFieldUserValue;
            }
        }

        public IAveUser User
        {
            get
            {
                if (mUser == null)
                {
                    SPUser user = mFieldUserValue.User;
                    if (user != null)
                    {
                        mUser = new AveUser(mWeb, user);
                    }
                }
                return mUser;
            }
        }
    }
}
