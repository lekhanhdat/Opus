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
    class NWPauseUntilProcessor : NWActionProcessorBase
    {
        public NWPauseUntilProcessor(NintexWFActionProcessor workflowActionProcessor)
            : base(workflowActionProcessor)
        {
            CLASSNAME = "Microsoft.SharePoint.WorkflowServices.Activities.DelayUntil";
        }

        public override WorkflowAction UpgradeWorkflowAction(NWActionConfig nwActionConfig)
        {
            this.sourceConfig = nwActionConfig;
            var workflowAction = new WorkflowAction();
            workflowAction.Id = actionId;
            workflowAction.ClassName = CLASSNAME;
            workflowAction.Configuration = CreateConfiguration();
            return workflowAction;
        }

        protected override Image CreateImage()
        {
            return new Image
            {
                Src = "ext/workflowDesigner/Img/icons/NW2013_Action_IconMap.png?noCache=1469003374404",
                ClassName = CLASSNAME,
                x49x49 = 441,
                y49x49 = 158,
                x30x30 = 441,
                y30x30 = 207,
                x16x16 = 474,
                y16x16 = 207,
            };
        }

        protected override List<Property> CreateProperties()
        {
            Property property = new Property();
            property.ID = "p0";
            property.DesignerType = "DateTime";
            property.DisplayName = "Date";
            property.Parameters = new Parameters[] { GetParameters() };
            return new List<Property> { property };
        }

        private Parameters GetParameters()
        {
            var sourceParmeter = this.sourceConfig.Parameters[0];

            var parameter = new Parameters
            {
                Name = "Date",
                Description = "Date to pause until.",
                Required = true,
                DataType = "DateTime",
                DesignerType = "DateTime",
                Direction = "Input",
                Value = base.ConvertParameterValue(sourceParmeter),
            };
            parameter.Value.Coercion = NWCoercionStringProcessor.GetCoercionString(parameter);
            return parameter;
        }
    }
}
