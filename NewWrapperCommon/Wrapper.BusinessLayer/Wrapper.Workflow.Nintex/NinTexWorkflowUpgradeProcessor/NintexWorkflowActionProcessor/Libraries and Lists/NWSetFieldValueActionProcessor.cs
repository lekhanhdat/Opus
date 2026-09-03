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
    class NWSetFieldValueActionProcessor : NWLibariesAndListsActionProcessor
    {
        public NWSetFieldValueActionProcessor(NintexWFActionProcessor workflowActionProcessor)
            : base(workflowActionProcessor)
        {
            CLASSNAME = "Microsoft.SharePoint.WorkflowServices.Activities.SetField";
        }

        public override WorkflowAction UpgradeWorkflowAction(Native13NinTexWorkflowEntity.NWActionConfig nwActionConfig)
        {
            sourceConfig = nwActionConfig;
            return new WorkflowAction()
            {
                Id = actionId,
                ClassName = CLASSNAME,
                Configuration = new Configuration()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = sourceConfig.TLabel,
                    Image = CreateImage(),
                    ServerInfo = new ServerInfo { ClassName = CLASSNAME },
                    Properties = CreateProperties(),
                    HelpKey = CLASSNAME
                },

            };
        }

        protected override Image CreateImage()
        {
            return new Image
            {
                Src = "ext/workflowDesigner/Img/icons/NW2013_Action_IconMap.png?noCache=1469502327705",
                ClassName = CLASSNAME,
                x49x49 = 147,
                y49x49 = 79,
                x30x30 = 147,
                y30x30 = 128,
                x16x16 = 180,
                y16x16 = 128
            };
        }

        protected override List<Property> CreateProperties()
        {
            ActivityParameter lookupField = null;
            ActivityParameter lookupFieldType = null;
            ActivityParameter lookupFieldValue = null;

            foreach (var para in sourceConfig.Parameters)
            {
                if (string.Equals(para.Name, "LookupField", StringComparison.OrdinalIgnoreCase))
                {
                    lookupField = para;
                }
                else if (string.Equals(para.Name, "LookupFieldType", StringComparison.OrdinalIgnoreCase))
                {
                    lookupFieldType = para;
                }
                else if (string.Equals(para.Name, "LookupFieldValue", StringComparison.OrdinalIgnoreCase))
                {
                    lookupFieldValue = para;
                }
            }

            CheckUnsupportedActionType(lookupFieldValue);

            var p0 = new Property
            {
                DesignerType = "WritableFields",
                DisplayName = "Field",
                ID = "p0",
                Parameters = new[]
                {
                    new Parameters() {
                        Name = "FieldName",
                        Value = new ParametersValue() {
                            ListLookup = new ListLookup() {
                                SelectList = "[Current Item]",
                                SelectField = lookupField.PrimitiveValue.Value,
                            }
                        },
                        Description = "Field to set the value of.",
                        Required = true,
                        DataType = "String",
                        DesignerType = "WritableFields",
                        Direction = "Input"
                    }
                }
            };

            var p1 = new Property
            {
                DesignerType = "Dependent",
                DisplayName = "Value",
                ID = "p1",
                Parameters = new[]
                {
                    new Parameters()
                    {
                        Name = "FieldValue",
                        Value = new ParametersValue(),
                        Description = "Value to set the field to.",
                        Required = true,
                        DataType = "String",
                        DesignerType = lookupFieldType.PrimitiveValue.ValueType,
                        Direction = "Input",
                        DependentOn = "FieldName"
                    }
                }
            };
            p1.Parameters[0].Value.PrimitiveValue = ConvertToPrimitiveValue(lookupFieldValue);

            return new List<Property> { p0, p1 };
        }
    }
}
