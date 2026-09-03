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




namespace AvePoint.Wrapper.Mapping
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using AvePoint.Common;
    using AvePoint.Wrapper.Common;

    public class AveFieldMapping : AveRestoredFieldMapping, IAveFieldMapping
    {
        IAveCustomFieldMapping customMapping;
        public IAveCustomFieldMapping CustomMapping
        {
            get { return customMapping; }
        }

        public Dictionary<string, object> NullToDefaultValueMapping
        {
            get 
            {
                if (customMapping != null)
                {
                    return customMapping.NullToDefaultValueMapping;
                }
                else
                {
                    return new Dictionary<string, object>();
                }
            }
        }

        public AveFieldMapping()
        {
        }


        #region SetCustomMapping
        public void SetCustomMapping(IAveCustomFieldMapping customFieldMapping)
        {
            customMapping = customFieldMapping;
        }
        #endregion

        public AveCustomFieldInfo GetMappingFieldBeforeAdd(AveSourceFieldInfo sourceFieldInfo)
        {
            if (customMapping != null)
            {
                return customMapping.GetMappingFieldBeforeAdd(sourceFieldInfo);
            }
            else
            {
                return null;
            }
        }

        public string GetMappingValue(AveSourceFieldValueInfo sourceFieldValueInfo)
        {
            if (customMapping != null)
            {
                return customMapping.GetMappingValue(sourceFieldValueInfo);
            }
            else
            {
                return null;
            }
        }

        public override void Dispose()
        {
            if (customMapping != null)
            {
                customMapping.Dispose();
                customMapping = null;
            }
            base.Dispose();
        }
    }
}
