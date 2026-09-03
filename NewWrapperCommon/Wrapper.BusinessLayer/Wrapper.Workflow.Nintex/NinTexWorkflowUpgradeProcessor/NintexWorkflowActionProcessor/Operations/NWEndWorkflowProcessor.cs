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
using Native13NinTexWorkflowEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LS.SPWorkflowProcessor
{
    class NWEndWorkflowProcessor : NWActionProcessorBase
    {
        private const string DEAFAULT_MESSAGE = "EndWorkflow";
        public NWEndWorkflowProcessor(NintexWFActionProcessor workflowActionProcessor)
            : base(workflowActionProcessor)
        {
            CLASSNAME = "#EndWorkflow";
        }

        protected override Image CreateImage()
        {
            return new Image
            {
                Src = "ext/workflowDesigner/Img/icons/NW2013_Action_IconMap.png?noCache=1469003374432",
                ClassName = CLASSNAME,
                x49x49 = 294,
                y49x49 = 0,
                x30x30 = 294,
                y30x30 = 49,
                x16x16 = 327,
                y16x16 = 49
            };
        }


        protected override List<Property> CreateProperties()
        {
            var property = new Property
            {
                ID = "reason",
                DesignerType = "Text",
                DisplayName = "Reason terminated",
                Parameters = new Parameters[] { CreateReasonParameter() },
            };

            return new List<Property> { property };
        }

        private Parameters CreateReasonParameter()
        {
            var sourceParameter = base.sourceConfig.Parameters[0];
            var parameterValue = new ParametersValue();
            if (string.IsNullOrEmpty(sourceParameter.PrimitiveValue.Value))
            {
                parameterValue.PrimitiveValue = new PrimitiveValue { Type = "String", Value = new Value(DEAFAULT_MESSAGE) };
            }
            else
            {
                parameterValue.PrimitiveValue = NWPrimitiveValueConverter.ConvertPrimitiveValue(sourceParameter.PrimitiveValue, base.workflowActionProcessor, false);
            }

            return new Parameters
            {
                Name = "reason",
                Description = "Text representing the reason for termination. This text is logged to the workflow history.",
                Required = true,
                DataType = "String",
                DesignerType = "Text",
                Direction = "Input",
                Value = parameterValue,
            };
        }

    }
}
