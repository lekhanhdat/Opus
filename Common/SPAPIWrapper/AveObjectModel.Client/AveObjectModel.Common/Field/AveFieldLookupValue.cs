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
    class AveFieldLookupValue : AveClientObject, IAveFieldLookupValue
    {
        public AveFieldLookupValue()
        {
        }

        public AveFieldLookupValue(int id)
        {
            base.DataCache.AddProperty("LookupId",id);
        }

        public AveFieldLookupValue(int lookupId, string lookupValue)
        {
            this.LookupId = lookupId;
            this.LookupValue = lookupValue;
        }

        public int LookupId
        {
            get
            {
                return base.DataCache.GetProperty<int>("LookupId");
            }
            set
            {
                base.DataCache.AddChangedProperty("LookupId", value);
            }
        }

        public virtual string LookupValue
        {
            get
            {
                return base.DataCache.GetProperty<string>("LookupValue");
            }
            set
            {
                base.DataCache.AddChangedProperty("LookupValue", value);
            }
        }

        internal bool IsSecretFieldValue
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsSecretFieldValue");
            }
        }

        public override string ToString()
        {
            if (this.IsSecretFieldValue)
            {
                return "***";
            }
            if (this.LookupId == 0)
            {
                return "";
            }
            return (this.LookupId.ToString() + ";#" + this.LookupValue);
        }

    }
}
