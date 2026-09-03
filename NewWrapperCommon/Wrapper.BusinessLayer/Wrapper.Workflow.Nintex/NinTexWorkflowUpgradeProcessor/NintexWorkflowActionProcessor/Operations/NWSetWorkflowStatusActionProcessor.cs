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
    class NWSetWorkflowStatusActionProcessor : NWActionProcessorBase
    {
        public NWSetWorkflowStatusActionProcessor(NintexWFActionProcessor workflowActionProcessor) : base(workflowActionProcessor)
        {
            CLASSNAME = "Microsoft.SharePoint.WorkflowServices.Activities.SetWorkflowStatus";
        }

        protected override Image CreateImage()
        {
            return new Image
            {
                Src = "ext/workflowDesigner/Img/icons/NW2013_Action_IconMap.png?noCache=1469003374406",
                ClassName = CLASSNAME,
                x49x49 = 245,
                y49x49 = 79,
                x30x30 = 245,
                y30x30 = 128,
                x16x16 = 278,
                y16x16 = 128
            };
        }

        protected override List<Property> CreateProperties()
        {
            ActivityParameter status = null;

            foreach (var para in sourceConfig.Parameters)
            {

                if (string.Equals(para.Name, "Status", StringComparison.OrdinalIgnoreCase))
                {
                    status = para;
                }
            }

            var statusPara = new Property
            {
                DesignerType = "Text",
                DisplayName = "Status",
                ID = "p0",
                Parameters = new[]
                {
                    CreateStatusParameters(status),
                }
            };

            return new List<Property> { statusPara };
        }

        private Parameters CreateStatusParameters(ActivityParameter status)
        {
            return new Parameters()
            {
                Name = "Status",
                Value = new ParametersValue()
                {
                    PrimitiveValue = NWPrimitiveValueConverter.ConvertPrimitiveValue(status.PrimitiveValue, base.workflowActionProcessor, true)
                },
                Description = "",
                Required = true,
                DataType = "String",
                DesignerType = "Text",
                Direction = "Input",
                DependentOn = "",
                OriginalSelectedValue = ""
            };
        }
    }
}
