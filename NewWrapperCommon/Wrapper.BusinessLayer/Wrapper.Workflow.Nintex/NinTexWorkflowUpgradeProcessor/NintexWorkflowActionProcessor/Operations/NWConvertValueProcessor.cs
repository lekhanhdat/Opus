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

namespace LS.SPWorkflowProcessor
{
    class NWConvertValueProcessor : NWActionProcessorBase
    {
        public NWConvertValueProcessor(NintexWFActionProcessor workflowActionProcessor) : base(workflowActionProcessor)
        {
            CLASSNAME = "#ConvertValue";
        }

        protected override Image CreateImage()
        {
            return new Image
            {
                Src = "ext/workflowDesigner/Img/icons/NW2013_Action_IconMap.png?noCache=1469003374211",
                ClassName = CLASSNAME,
                x49x49 = 196,
                y49x49 = 395,
                x30x30 = 196,
                y30x30 = 444,
                x16x16 = 229,
                y16x16 = 444
            };
        }

        protected override List<Property> CreateProperties()
        {
            var property = new Property
            {
                ID = "ConvertValue",
                DesignerType = "ConvertValue",
                Parameters = new Parameters[] { CreateInputStringParameters(), CreateInputCultureParameters(), CreateInputDateTimeFormatParameters(), CreateOutputStringParameters() }
            };
            return new List<Property> { property };
        }

        private Parameters CreateInputStringParameters()
        {
            var sourceInputString = NWCommonUtility.GetActivityParameterByName(this.sourceConfig.Parameters, "Input", true);
            return new Parameters
            {
                Name = "InputString",
                Required = true,
                DataType = "String",
                DesignerType = "Text",
                Direction = "Input",
                Value = base.ConvertParameterValue(sourceInputString),
            };
        }

        private Parameters CreateInputCultureParameters()
        {
            var sourceInputCulture = NWCommonUtility.GetActivityParameterByName(this.sourceConfig.Parameters, "Culture", true);
            return new Parameters
            {
                Name = "InputCulture",
                Description = "When default is selected, the current language settings of the SharePoint site are used.",
                Required = false,
                DataType = "String",
                DesignerType = "Text",
                Direction = "Input",
                Value = base.ConvertParameterValue(sourceInputCulture),
            };
        }

        private Parameters CreateInputDateTimeFormatParameters()
        {
            var sourceInputCulture = NWCommonUtility.GetActivityParameterByName(this.sourceConfig.Parameters, "DateTimeFormat", true);
            return new Parameters
            {
                Name = "InputDateTimeFormat",
                Required = false,
                DataType = "String",
                DesignerType = "Text",
                Direction = "Input",
                Value = base.ConvertParameterValue(sourceInputCulture),
            };
        }

        private Parameters CreateOutputStringParameters()
        {
            var sourceOutputString = NWCommonUtility.GetActivityParameterByName(this.sourceConfig.Parameters, "Output", true); 
            return new Parameters
            {
                Name = "OutputString",
                Description = "The workflow variable for storing the converted value of the input string.",
                Required = true,
                DataType = "Primitive",
                DesignerType = "Variable",
                Direction = "Output",
                Value = base.ConvertParameterValue(sourceOutputString),
            };
        }
    }
}
