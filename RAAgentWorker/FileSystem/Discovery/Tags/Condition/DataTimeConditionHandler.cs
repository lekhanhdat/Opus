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
    internal class DataTimeConditionHandler : ConditionHandler
    {

        private static readonly DateTime S_CURRENT_DATETIME = DateTime.UtcNow;

        public override ConditionCategory Category => ConditionCategory.DateTime;

        public override bool Handle(ConditionInfo info, object dataObject)
        {
            var logic = (DateTimeConditionLogicType)info.Logic;
            var conditionValue = info.Value;
            if (!(dataObject is DateTime dataValue))
            {
                throw new NotSupportedException($"The [{Category}] only supports value of DateTime type.");
            }

            switch (logic)
            {
                case DateTimeConditionLogicType.Before:
                    return IsBefore(conditionValue, dataValue);
                case DateTimeConditionLogicType.OlderThan:
                    return IsOlderThan(conditionValue, dataValue);
                default:
                    throw new NotSupportedException($"The [{Category}] does not support {logic}.");
            }
        }

        private static bool IsBefore(string criteriaValue, DateTime dataValue)
        {
            var criteriaDateTime = DateTime.Parse(criteriaValue);
            return dataValue < criteriaDateTime;
        }

        private bool IsOlderThan(string criteriaValue, DateTime dataValue)
        {
            var criteriaInfo = JsonConvert.DeserializeObject<DateConditionOlderThanInfo>(criteriaValue, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            var unit = criteriaInfo.Unit;
            DateTime olderDate;
            switch (criteriaInfo.UnitType)
            {
                case DateUnitType.Day:
                    olderDate = dataValue.AddDays(unit);
                    break;
                case DateUnitType.Week:
                    olderDate = dataValue.AddDays(unit * 7);
                    break;
                case DateUnitType.Month:
                    olderDate = dataValue.AddMonths(unit);
                    break;
                case DateUnitType.Year:
                    olderDate = dataValue.AddYears(unit);
                    break;
                default:
                    throw new NotSupportedException($"The older than criteria of [{Category}] not supports [{criteriaInfo.UnitType}] unit type.");
            }
            return olderDate <= S_CURRENT_DATETIME;
        }
    }
}
