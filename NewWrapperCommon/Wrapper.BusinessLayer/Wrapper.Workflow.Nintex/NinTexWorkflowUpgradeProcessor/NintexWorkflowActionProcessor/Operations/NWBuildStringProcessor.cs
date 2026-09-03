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
    class NWBuildStringProcessor : NWActionProcessorBase
    {
        private const string DefaultInputText = "DocAveTextPalceHolder";

        public NWBuildStringProcessor(NintexWFActionProcessor workflowActionProcessor) : base(workflowActionProcessor)
        {
            CLASSNAME = "#StringBuilder";
        }

        protected override List<Property> CreateProperties()
        {
            return new List<Property> { CreateInputStringProperty(), CreateOutputStringProperty() };
        }

        protected override Image CreateImage()
        {
            return new Image
            {
                Src = "ext/workflowDesigner/Img/icons/NW2013_Action_IconMap.png?noCache=1469003374161",
                ClassName = CLASSNAME,
                x49x49 = 0,
                y49x49 = 395,
                x30x30 = 0,
                y30x30 = 444,
                x16x16 = 32,
                y16x16 = 444
            };
        }

        private Property CreateInputStringProperty()
        {
            var sourceInputString = NWCommonUtility.GetActivityParameterByName(this.sourceConfig.Parameters, "Input", true);
            Property inputString = new Property
            {
                ID = "InputString",
                DesignerType = "Multiline",
                DisplayName = "String",
            };
            Parameters parameter = new Parameters
            {
                Name = "InputString",
                Required = true,
                DataType = "String",
                DesignerType = "Multiline",
                Direction = "Input",
                Value = base.ConvertParameterValue(sourceInputString),
            };
            if(parameter.Value.PrimitiveValue == null)
            {
                parameter.Value.PrimitiveValue = new PrimitiveValue()
                {
                    Type = "String",
                    Value = new Value(DefaultInputText),
                };
            }
            inputString.Parameters = new Parameters[] { parameter };
            return inputString;
        }

        private Property CreateOutputStringProperty()
        {
            var sourceOutputString = NWCommonUtility.GetActivityParameterByName(this.sourceConfig.Parameters, "Output", true);
            Property outputString = new Property
            {
                ID = "OutputString",
                DesignerType = "Variable",
                DisplayName = "Output",
            };
            Parameters parameter = new Parameters
            {
                Name = "OutputString",
                Required = true,
                DataType = "String",
                DesignerType = "Variable",
                Direction = "Output",
                Value = base.ConvertParameterValue(sourceOutputString),
            };
            outputString.Parameters = new Parameters[] { parameter };
            return outputString;
        }
    }
}
