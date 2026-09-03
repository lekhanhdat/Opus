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
using AvePoint.GCommon.Contract.Server.ControlPanel.ListTitleMapping.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.ColumnMapping.Object;

namespace AvePoint.Wrapper.Mapping
{
    public class AveListTitleMappingConverter
    {

        public static List<AveListTitleMappingInfo> Convert(ListTitleMappingDataContract contract)
        {
            List<AveListTitleMappingInfo> tempListTitleMapping = new List<AveListTitleMappingInfo>();          
            if (contract.listTitleMappings != null)
            {
                foreach (ListTitleMappingDto mappingDto in contract.listTitleMappings)
                {
                    AveListTitleMappingInfo tempTitleMapping = new AveListTitleMappingInfo();
                    if (mappingDto.SiteConditions != null)
                    {
                        foreach (ColumnFilter filter in mappingDto.SiteConditions)
                        {
                            tempTitleMapping.ListTitleMappingCondition.siteCondition = InitConditionInfo(filter);
                        }
                    }
                    if (mappingDto.ListConditions != null)
                    {
                        foreach (ColumnFilter filter in mappingDto.ListConditions)
                        {
                            tempTitleMapping.ListTitleMappingCondition.listCondition = InitConditionInfo(filter);
                        }
                    }
                    if (mappingDto.MappingValues != null)
                    {
                        if (mappingDto.MappingValues != null)
                        {
                            foreach (ListTitleMappingValue value in mappingDto.MappingValues)
                            {
                                AveListTitleMappingValueInfo tempListTitleMappingInfo = new AveListTitleMappingValueInfo(value.SrcGroupName,value.DesGroupName);
                                tempTitleMapping.ListTitleMappingValueInfo.Add(tempListTitleMappingInfo);
                            }
                        }
                    }
                    tempListTitleMapping.Add(tempTitleMapping);
                }
            }
            return tempListTitleMapping;       
        }

        private static List<AveMappingConditionInfo> InitConditionInfo(ColumnFilter condition)
        {
            List<AveMappingConditionInfo> tempConditionInfos = new List<AveMappingConditionInfo>();
            if (condition.Conditions != null)
            {
                //Current the count of condition.Conditions is 1, we use list in case of extensive in future
                foreach (ConditionItem tempCondition in condition.Conditions)
                {
                    AveMappingConditionInfo tempConditionInfo = new AveMappingConditionInfo();
                    tempConditionInfo.ConditionType = (AveConditionType)Enum.Parse(typeof(AveConditionType), tempCondition.MetaDataType.ToString(), true);
                    tempConditionInfo.Operation = (MappingFilterCondition)Enum.Parse(typeof(MappingFilterCondition), tempCondition.ConditionType.ToString(), true);
                    tempConditionInfo.ConditionValue = tempCondition.Value;
                    tempConditionInfo.Relation = (AveConditionRelation)Enum.Parse(typeof(AveConditionRelation), tempCondition.AndOr.ToString(), true);
                    tempConditionInfos.Add(tempConditionInfo);
                }
            }
            return tempConditionInfos;
        }
    }
}
