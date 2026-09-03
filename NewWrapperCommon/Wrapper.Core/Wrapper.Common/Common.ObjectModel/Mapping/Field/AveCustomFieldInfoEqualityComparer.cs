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
using System.Globalization;

namespace AvePoint.Wrapper.Common
{
    [Serializable]
    public class AveCustomFieldInfoEqualityComparer : IEqualityComparer<AveSourceFieldInfo>
    {
        public bool Equals(AveSourceFieldInfo x, AveSourceFieldInfo y)
        {
            if (string.Equals(x.SourceInternalName, y.SourceInternalName, StringComparison.CurrentCultureIgnoreCase) && string.Equals(x.SourceDisplayName, y.SourceDisplayName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int GetHashCode(AveSourceFieldInfo obj)
        {
            int hashCode = 0;
            if (!string.IsNullOrEmpty(obj.SourceDisplayName))
            {
                hashCode += obj.SourceDisplayName.ToLower(CultureInfo.CurrentCulture).GetHashCode();
            }
            if (!string.IsNullOrEmpty(obj.SourceInternalName))
            {
                hashCode += obj.SourceInternalName.ToLower(CultureInfo.InvariantCulture).GetHashCode();
            }
            return hashCode;
        }
    }

    public class AveCustomFieldInfoDisplayNameEqualityComparer : IEqualityComparer<AveSourceFieldInfo>
    {
        public bool Equals(AveSourceFieldInfo x, AveSourceFieldInfo y)
        {
            return string.Equals(x.SourceDisplayName, y.SourceDisplayName, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(AveSourceFieldInfo obj)
        {
            if (obj.SourceDisplayName == null)
            {
                return 0;
            }
            return obj.SourceDisplayName.GetHashCode();
        }
    }
}
