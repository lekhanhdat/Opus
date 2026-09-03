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
using AvePoint.Wrapper.Mapping;

namespace AvePoint.Wrapper.Mapping
{
    public class AveListTitleMappingCondition:AveMappingCondition
    {
        public List<AveMappingConditionInfo> siteCondition = new List<AveMappingConditionInfo>();
        public List<AveMappingConditionInfo> listCondition = new List<AveMappingConditionInfo>();

        public override bool CheckCondition(object listOrWeb, Guid fieldId)
        {
            bool result = false;
            if (listOrWeb == null)
            {
                listOrWeb = AveMappingSourceSPListOrWebInfo;
            }
            if (listOrWeb != null)
            {
                if (listOrWeb is AveMappingSourceSPListInfo)
                {
                    result = base.CheckConditionResult(listOrWeb, siteCondition, fieldId) && base.CheckConditionResult(listOrWeb, listCondition, fieldId);
                }
                else if (listOrWeb is AveMappingSourceSPWebInfo)
                {
                    result = listCondition.Count == 0  && base.CheckConditionResult(listOrWeb, siteCondition, fieldId);
                }
            }
            return result;
        }
    }
}
