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
using Native13NinTexWorkflowEntity;
using AvePoint.GCommon;

namespace LS.SPWorkflowProcessor
{
    class DatetimeVariablesProcessor : VariablesProcessorBase
    {
        AveLogger logger = AveLogger.GetInstance(typeof(DatetimeVariablesProcessor));

        public DatetimeVariablesProcessor(NWWorkflowVariable currentVar)
            : base(currentVar)
        {
        }
        protected override VariableConfiguration GetVariableConfiguration()
        {
            var dateConfiguration = new DateTimeConfiguration
            {
                Description = currentVar.Description,
                AllowBlank = !currentVar.Required,
                AllowBlankSpecified = !currentVar.Required,
                DefaultValue = string.Equals(currentVar.DefaultValue, "[today]", StringComparison.OrdinalIgnoreCase) ? currentVar.DefaultValue : string.Empty,
                DefaultValueType = string.Equals(currentVar.DefaultValue, "[today]", StringComparison.OrdinalIgnoreCase) ? "today" : "None",
                DisplayFormat = GetDisplayFormat(currentVar.ControlType),
            };

            return dateConfiguration;
        }

        private string GetDisplayFormat(WorkflowInitiationControlType controlType)
        {
            if (controlType == WorkflowInitiationControlType.DateOnly)
            {
                return "Date";
            }
            if (controlType != WorkflowInitiationControlType.DateTime)
            {
                logger.Warn("Control type is not date time type. Control type is {0}", controlType);
            }
            return "DateTime";
        }
    }
}
