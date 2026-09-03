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
    class NWCalculateDateProcessor : NWActionProcessorBase
    {
        public NWCalculateDateProcessor(NintexWFActionProcessor workflowActionProcessor)
            : base(workflowActionProcessor)
        {
            CLASSNAME = "Microsoft.Activities.Expressions.AddToDate";
        }

        protected override Image CreateImage()
        {
            return new Image
            {
                Src = "ext/workflowDesigner/Img/icons/NW2013_Action_IconMap.png?noCache=1469003374398",
                ClassName = CLASSNAME,
                x49x49 = 0,
                y49x49 = 0,
                x30x30 = 0,
                y30x30 = 49,
                x16x16 = 33,
                y16x16 = 49
            };
        }

        protected override List<Property> CreateProperties()
        {
            var properties = new List<Property>();
            properties.Add(CreateMonthsProperty(NWCommonUtility.GetActivityParameterByName(this.sourceConfig.Parameters, "Months", true)));
            properties.Add(CreateDaysProperty(NWCommonUtility.GetActivityParameterByName(this.sourceConfig.Parameters, "Days", true)));
            properties.Add(CreateHoursProperty(NWCommonUtility.GetActivityParameterByName(this.sourceConfig.Parameters, "Hours", true)));
            properties.Add(CreateMinutesProperty(NWCommonUtility.GetActivityParameterByName(this.sourceConfig.Parameters, "Minutes", true)));
            properties.Add(CreateDateTimeProperty(NWCommonUtility.GetActivityParameterByName(this.sourceConfig.Parameters, "Date", true)));
            properties.Add(CreateOutputProperty(NWCommonUtility.GetActivityParameterByName(this.sourceConfig.Parameters, "Output", true)));
            properties.Add(CreateDateISOProperty(NWCommonUtility.GetActivityParameterByName(this.sourceConfig.Parameters, "OutputText", true)));

            return properties;
        }

        private Property CreateMonthsProperty(ActivityParameter monthsParameter)
        {
            var monthsProperty = new Property();
            monthsProperty.ID = "p1";
            monthsProperty.DesignerType = "Integer";
            monthsProperty.DisplayName = "Months";
            Parameters parameters = new Parameters
            {
                Name = "Months",
                Required = false,
                DataType = "Int32",
                DesignerType = "Integer",
                Direction = "Input",
                Description = "Months to add to date.",
            };
            //Months Type是Int32
            parameters.Value = ConvertParameterValue(monthsParameter, "Int32");
            parameters.Value.Coercion = NWCoercionStringProcessor.GetCoercionString(parameters);
            monthsProperty.Parameters = new Parameters[] { parameters };
            return monthsProperty;
        }

        private ParametersValue ConvertParameterValue(ActivityParameter activityParameter, string primitiveValueType)
        {
            var value = base.ConvertParameterValue(activityParameter);
            if (value.PrimitiveValue != null)
            {
                value.PrimitiveValue.Type = primitiveValueType;
            }
            if (value.Variable != null)
            {
                if (!string.Equals(value.Variable.DataType, "Double", StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(value.Variable.DataType, "Int32", StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(value.Variable.DataType, "String", StringComparison.OrdinalIgnoreCase))
                {
                    throw new UnSupportedDataException("Parameters Variable type is unsupported.");
                }
            }
            return value;
        }

        private Property CreateDaysProperty(ActivityParameter daysParameter)
        {
            var daysProperty = new Property();
            daysProperty.ID = "p2";
            daysProperty.DesignerType = "Integer";
            daysProperty.DisplayName = "Days";
            Parameters parameters = new Parameters
            {
                Name = "Days",
                Required = false,
                DataType = "Double",
                DesignerType = "Integer",
                Direction = "Input",
                Description = "Days to add to date.",
            };
            parameters.Value = ConvertParameterValue(daysParameter, "Double");
            parameters.Value.Coercion = NWCoercionStringProcessor.GetCoercionString(parameters);
            daysProperty.Parameters = new Parameters[] { parameters };
            return daysProperty;
        }

        private Property CreateHoursProperty(ActivityParameter hoursParameter)
        {
            var hoursProperty = new Property();
            hoursProperty.ID = "p3";
            hoursProperty.DesignerType = "Integer";
            hoursProperty.DisplayName = "Hours";
            Parameters parameters = new Parameters
            {
                Name = "Hours",
                Required = false,
                DataType = "Double",
                DesignerType = "Integer",
                Direction = "Input",
                Description = "Hours to add to date.",
            };
            parameters.Value = ConvertParameterValue(hoursParameter, "Double");
            parameters.Value.Coercion = NWCoercionStringProcessor.GetCoercionString(parameters);
            hoursProperty.Parameters = new Parameters[] { parameters };
            return hoursProperty;
        }


        private Property CreateMinutesProperty(ActivityParameter minutesParameter)
        {
            var minutesProperty = new Property();
            minutesProperty.ID = "p4";
            minutesProperty.DesignerType = "Integer";
            minutesProperty.DisplayName = "Minutes";
            Parameters parameters = new Parameters
            {
                Name = "Minutes",
                Required = false,
                DataType = "Double",
                DesignerType = "Integer",
                Direction = "Input",
                Description = "Minutes to add to date.",
            };
            parameters.Value = ConvertParameterValue(minutesParameter, "Double");
            parameters.Value.Coercion = NWCoercionStringProcessor.GetCoercionString(parameters);
            minutesProperty.Parameters = new Parameters[] { parameters };
            return minutesProperty;
        }

        private Property CreateDateTimeProperty(ActivityParameter dateTimeParameter)
        {
            var dateTimeProperty = new Property();
            dateTimeProperty.ID = "p5";
            dateTimeProperty.DesignerType = "DateTime";
            dateTimeProperty.DisplayName = "Date";
            Parameters parameters = new Parameters
            {
                Name = "Input",
                Required = true,
                DataType = "DateTime",
                DesignerType = "DateTime",
                Direction = "Input",
                Description = "Value of the date used by this action.",
            };

            parameters.Value = ConvertDateTimePropertyParametersValue(dateTimeParameter);
            dateTimeProperty.Parameters = new Parameters[] { parameters };
            return dateTimeProperty;
        }

        private ParametersValue ConvertDateTimePropertyParametersValue(ActivityParameter dateTimeParameter)
        {
            if (string.Equals("CurrentDate", dateTimeParameter.SpecialReference, StringComparison.OrdinalIgnoreCase)
                || string.Equals("CurrentDateTime", dateTimeParameter.SpecialReference, StringComparison.OrdinalIgnoreCase))
            {
                return new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue
                    {
                        Type = "DateTime",
                        Value = new Value { DateTimeInfo = new DateTimeInfo { UseCurrentDate = true } },
                    }
                };
            }
            var value = base.ConvertParameterValue(dateTimeParameter);
            value.Coercion = string.Equals("AsDNDateTime", NWCoercionStringProcessor.GetCoercionString("DateTime", value), StringComparison.OrdinalIgnoreCase) ? "AsDNDateTime" : "AsDNDateTimeFromString";
            return value;
        }

        private Property CreateOutputProperty(ActivityParameter outputParameter)
        {
            var outputProperty = new Property();
            outputProperty.ID = "p6";
            outputProperty.DesignerType = "Variable";
            outputProperty.DisplayName = "Output as date";
            Parameters parameters = new Parameters
            {
                Name = "Result",
                Required = true,
                DataType = "DateTime",
                DesignerType = "Variable",
                Direction = "Output",
                Description = "Date/Time variable to store the date.",
            };
            parameters.Value = base.ConvertParameterValue(outputParameter);
            outputProperty.Parameters = new Parameters[] { parameters };
            return outputProperty;
        }

        private Property CreateDateISOProperty(ActivityParameter dateISOParameter)
        {
            var dateISOProperty = new Property();
            dateISOProperty.ID = "DateISO";
            dateISOProperty.DesignerType = "Variable";
            dateISOProperty.DisplayName = "Output as ISO 8601 date string";
            Parameters parameters = new Parameters
            {
                Name = "DateISO",
                Required = false,
                DataType = "String",
                DesignerType = "Variable",
                Direction = "Output",
                Description = "Text variable to store the date as a string in ISO 8601 format.",
                Type = "Any",
                DefaultType = "Any",
            };
            parameters.Value = base.ConvertParameterValue(dateISOParameter);
            dateISOProperty.Parameters = new Parameters[] { parameters };
            return dateISOProperty;
        }

        protected override ServerInfo CreateServerInfo()
        {
            return new ServerInfo
            {
                ClassName = CLASSNAME,
                Assembly = "Microsoft.Activities, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
            };
        }
    }
}
