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

namespace LS.SPWorkflowProcessor
{
    class NWPauseForAtionProcessor : NWActionProcessorBase
    {
        public NWPauseForAtionProcessor(NintexWFActionProcessor workflowActionProcessor) : base(workflowActionProcessor)
        {
            CLASSNAME = "#PauseForActivity";
        }


        protected override Image CreateImage()
        {
            return new Image
            {
                Src = "ext/workflowDesigner/Img/icons/NW2013_Action_IconMap.png?noCache=1469003374191",
                ClassName = CLASSNAME,
                x49x49 = 392,
                y49x49 = 158,
                x30x30 = 392,
                y30x30 = 207,
                x16x16 = 425,
                y16x16 = 207,
            };
        }

        protected override List<Property> CreateProperties()
        {
            ActivityParameter years = null;
            ActivityParameter months = null;
            ActivityParameter days = null;
            ActivityParameter hours = null;
            ActivityParameter minutes = null;
            ActivityParameter businessHoursOnly = null;

            foreach (var para in sourceConfig.Parameters)
            {

                if (string.Equals(para.Name, "Years", StringComparison.OrdinalIgnoreCase))
                {
                    years = para;
                }
                else if (string.Equals(para.Name, "Months", StringComparison.OrdinalIgnoreCase))
                {
                    months = para;
                }
                else if (string.Equals(para.Name, "Days", StringComparison.OrdinalIgnoreCase))
                {
                    days = para;
                }
                else if (string.Equals(para.Name, "Hours", StringComparison.OrdinalIgnoreCase))
                {
                    hours = para;
                }
                else if (string.Equals(para.Name, "Minutes", StringComparison.OrdinalIgnoreCase))
                {
                    minutes = para;
                }
                else if (string.Equals(para.Name, "BusinessHoursOnly", StringComparison.OrdinalIgnoreCase))
                {
                    businessHoursOnly = para;
                }
            }

            var daysPara = new Property
            {
                DesignerType = "Number",
                DisplayName = "Days",
                ID = "Days",
                Parameters = new[]
                {
                    CreateDaysParameters(days),
                }
            };

            var hoursPara = new Property
            {
                DesignerType = "Number",
                DisplayName = "Hours",
                ID = "Hours",
                Parameters = new[]
                {
                    CreateHoursParameters(hours),
                }
            };

            var minutesPara = new Property
            {
                DesignerType = "Number",
                DisplayName = "Minutes",
                ID = "Minutes",
                Parameters = new[]
                {
                    CreateMinutesParameters(minutes),
                }
            };

            var businessHoursOnlyPara = new Property
            {
                DesignerType = "CheckBox",
                DisplayName = "Business hours only",
                ID = "BusinessHoursOnly",
                Parameters = new[]
                {
                    CreateBusinessHoursOnlyParameters(businessHoursOnly),
                }
            };

            return new List<Property> { daysPara, hoursPara, minutesPara, businessHoursOnlyPara };
        }

        private Parameters CreateDaysParameters(ActivityParameter days)
        {
            return new Parameters()
            {
                Name = "Days",
                Value = NWPrimitiveValueConverter.ConvertPrimitiveValueToParametersValue(days.PrimitiveValue, "Double", workflowActionProcessor, true),
                Description = "",
                Required = true,
                DataType = "Double",
                DesignerType = "Number",
                Direction = "Input",
                DependentOn = "",
                OriginalSelectedValue = ""
            };
        }

        private Parameters CreateHoursParameters(ActivityParameter hours)
        {
            return new Parameters()
            {
                Name = "Hours",
                Value = NWPrimitiveValueConverter.ConvertPrimitiveValueToParametersValue(hours.PrimitiveValue, "Double", workflowActionProcessor, true),
                Description = "",
                Required = true,
                DataType = "Double",
                DesignerType = "Number",
                Direction = "Input",
                DependentOn = "",
                OriginalSelectedValue = ""
            };
        }

        private Parameters CreateMinutesParameters(ActivityParameter minutes)
        {
            return new Parameters()
            {
                Name = "Minutes",
                Value = NWPrimitiveValueConverter.ConvertPrimitiveValueToParametersValue(minutes.PrimitiveValue, "Double", workflowActionProcessor, true),
                Description = "",
                Required = true,
                DataType = "Double",
                DesignerType = "Number",
                Direction = "Input",
                DependentOn = "",
                OriginalSelectedValue = ""
            };
        }

        private Parameters CreateBusinessHoursOnlyParameters(ActivityParameter businessHourOnly)
        {
            return new Parameters()
            {
                Name = "BusinessHoursOnly",
                Value = new ParametersValue() { PrimitiveValue = new PrimitiveValue { Type = "Boolean", Value = new Value(businessHourOnly.PrimitiveValue.Value) } },
                Description = "",
                Required = false,
                DataType = "Boolean",
                DesignerType = "CheckBox",
                Direction = "Input",
                DependentOn = "",
                OriginalSelectedValue = ""
            };
        }
    }
}
