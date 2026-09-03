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
using RAFileSystem.FileSystem.Discovery.Tags.Contract;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAFileSystem.FileSystem.Discovery.Tags.Condition
{
    internal class FileSizeConditionHandler : ConditionHandler
    {
        public override ConditionCategory Category => ConditionCategory.FileSize;

        public override bool Handle(ConditionInfo info, object dataObject)
        {
            var logic = (FileSizeConditionLogicType)info.Logic;
            var conditionValue = JsonConvert.DeserializeObject<FileSizeConditionInfo>(info.Value, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            if (!(dataObject is long dataValue))
            {
                throw new NotSupportedException($"The [{Category}] only supports value of int type.");
            }

            var criteriaFileSize = GetCriteriaFileSize(conditionValue);
            switch (logic)
            {
                case FileSizeConditionLogicType.LessThanEquals:
                    return dataValue <= criteriaFileSize;
                case FileSizeConditionLogicType.GreaterThanEquals:
                    return dataValue >= criteriaFileSize;
                default:
                    throw new NotSupportedException($"The [{Category}] does not support {logic}.");
            }
        }

        private long GetCriteriaFileSize(FileSizeConditionInfo criteriaInfo)
        {
            switch (criteriaInfo.UnitType)
            {
                case FileSizeUnitType.KB:
                    return (long)criteriaInfo.Unit * 1024;
                case FileSizeUnitType.MB:
                    return (long)criteriaInfo.Unit * 1024 * 1024;
                case FileSizeUnitType.GB:
                    return (long)criteriaInfo.Unit * 1024 * 1024 * 1024;
                default:
                    throw new NotSupportedException($"The older than criteria of [{Category}] not supports [{criteriaInfo.UnitType}] unit type.");
            }
        }
    }
}
