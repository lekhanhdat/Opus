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
using System.Globalization;
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
            base.DataCache.PropertiesCache["LookupId"] = id;
        }

        public AveFieldLookupValue(int lookupId, string lookupValue)
        {
            this.LookupId = lookupId;
            this.LookupValue = lookupValue;
        }

        public AveFieldLookupValue(string fieldValue)
        {
            if (!string.IsNullOrEmpty(fieldValue))
            {
                if (fieldValue == "***")
                {
                    IsSecretFieldValue = true;
                }
                else
                {
                    int index = fieldValue.IndexOf(";#", StringComparison.Ordinal);
                    if (index < 0)
                    {
                        LookupId  = this.ParseLookupId(fieldValue);
                        LookupValue = string.Empty;
                    }
                    else
                    {
                        LookupId = this.ParseLookupId(fieldValue.Substring(0, index));
                        index += 2;
                        if (index < fieldValue.Length)
                        {
                            LookupValue = fieldValue.Substring(index, fieldValue.Length - index);
                        }
                    }
                }
            }
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
            set
            {
                base.DataCache.AddChangedProperty("IsSecretFieldValue", value);
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
                return null;
            }
            return (this.LookupId.ToString() + ";#" + this.LookupValue);
        }

        private int ParseLookupId(string fieldValue)
        {
            int num;
            if (!int.TryParse(fieldValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out num)) throw new ArgumentException();
            return num;
        }



    }
}
