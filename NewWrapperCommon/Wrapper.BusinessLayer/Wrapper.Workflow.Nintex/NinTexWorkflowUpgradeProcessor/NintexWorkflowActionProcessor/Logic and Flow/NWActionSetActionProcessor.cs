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
    class NWActionSetActionProcessor : NWContainerActionBase
    {
        public NWActionSetActionProcessor(NintexWFActionProcessor workflowActionProcessor)
            : base(workflowActionProcessor)
        {
            CLASSNAME = "#ActionSet";
        }
        
        protected override Image CreateImage()
        {
            return new Image
            {
                x49x49 = 245,
                y49x49 = 553,
                x30x30 = 245,
                y30x30 = 602,
                x16x16 = 278,
                y16x16 = 602
            };
        }

        protected override List<Property> CreateProperties()
        {
            var p1 = new Property
            {
                ID = "actionSet",
                DesignerType = "ActionSet",
                DisplayName = "Action Set",
                Parameters = new[]
               {
                     CreateactionSetParameter(),
                     CreateElevatedPermissionsParameter(),
                     CreatePreviousElevatedPermissionsParameter(),
                }
            };
            return new List<Property>() { p1 };
        }

        private Parameters CreatePreviousElevatedPermissionsParameter()
        {
            return new Parameters()
            {
                Name = "previousElevatedPermissions",
                Required = false,
                DataType = "Boolean",
                Direction = "Input",
                Value = new ParametersValue()
                {
                    PrimitiveValue = new PrimitiveValue()
                    {
                        Type = "Boolean",
                        Value = new Value("False"),
                    },
                }
            };
        }

        private Parameters CreateElevatedPermissionsParameter()
        {
            return new Parameters()
            {
                Name = "elevatedPermissions",
                Required = false,
                DataType = "Boolean",
                Direction = "Input",
                Value = new ParametersValue()
                {
                    PrimitiveValue = new PrimitiveValue()
                    {
                        Type = "Boolean",
                        Value = new Value("False"),
                    },
                }
            };
        }

        private Parameters CreateactionSetParameter()
        {
            return new Parameters()
            {
                Name = "actionSet",
                Required = false,
                DataType = "String",
                Direction = "Input",
            };
        }
    }
}
