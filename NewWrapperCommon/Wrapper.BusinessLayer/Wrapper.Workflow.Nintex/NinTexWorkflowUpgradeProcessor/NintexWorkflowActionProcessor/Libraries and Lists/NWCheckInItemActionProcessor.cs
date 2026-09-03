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
using Native13NinTexWorkflowEntity;

namespace LS.SPWorkflowProcessor
{

    class NWCheckInItemActionProcessor : NWLibariesAndListsActionProcessor
    {
        public NWCheckInItemActionProcessor(NintexWFActionProcessor workflowActionProcessor)
            : base(workflowActionProcessor)
        {
            CLASSNAME = "#CheckInItemActivity";
        }


        public override WorkflowAction UpgradeWorkflowAction(NWActionConfig nwActionConfig)
        {
            sourceConfig = nwActionConfig;

            var action = new WorkflowAction
            {
                Id = actionId,
                ClassName = CLASSNAME,
                Configuration = CreateConfiguration()
            };

            return action;
        }


        protected override Image CreateImage()
        {
            return new Image
            {
                Src = "ext/workflowDesigner/Img/icons/NW2013_Action_IconMap.png?noCache=1440639687316",
                ClassName = CLASSNAME,
                x49x49 = 98,
                y49x49 = 0,
                x30x30 = 98,
                y30x30 = 49,
                x16x16 = 131,
                y16x16 = 49
            };
        }

        protected override List<Property> CreateProperties()
        {
            ActivityParameter listId = null;
            ActivityParameter lookupField = null;
            ActivityParameter lookupFieldType = null;
            ActivityParameter lookupFieldValue = null;
            ActivityParameter messageActivityParameter = null;
            ActivityParameter thisItemValue = null;
            ActivityParameter checkInType = null;

            foreach (var para in sourceConfig.Parameters)
            {
                if (string.Equals(para.Name, "ThisItem", StringComparison.OrdinalIgnoreCase))
                {
                    thisItemValue = para;
                }
                else if (string.Equals(para.Name, "ListId", StringComparison.OrdinalIgnoreCase))
                {
                    listId = para;
                }
                else if (string.Equals(para.Name, "LookupField", StringComparison.OrdinalIgnoreCase))
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
                else if (string.Equals(para.Name, "Message", StringComparison.OrdinalIgnoreCase))
                {
                    messageActivityParameter = para;
                }
                else if (string.Equals(para.Name, "CheckInType", StringComparison.OrdinalIgnoreCase))
                {
                    checkInType = para;
                }
            }

            CheckUnsupportedActionType(lookupFieldValue);

            var p1 = new Property
            {
                DesignerType = "ChooseDocumentLibraryItem",
                DisplayName = "Item",
                ID = "p0",
                Parameters = new[]
                {
                    CreateListIdParameter(listId,lookupField,lookupFieldType,thisItemValue),
                    CreateItemGuidParameter(listId,lookupField,lookupFieldType ,lookupFieldValue,thisItemValue)
                }
            };

            var pComment = new Property
            {
                DesignerType = "Text",
                DisplayName = "Comment",
                ID = "Comment",

                Parameters = new[]
                {
                    CreateCommentParameters(messageActivityParameter)
                }
            };

            var pCheckInType = new Property
            {
                DesignerType = "ChoiceList",
                DisplayName = "Check in type",
                ID = "CheckInType",

                Parameters = new[]
                {
                    CreateCheckInTypeParameters(checkInType)
                }
            };

            return new List<Property> { p1, pComment, pCheckInType };
        }

        private Parameters CreateCheckInTypeParameters(ActivityParameter checkInType)
        {

            var primitiveValue = NWPrimitiveValueConverter.ConvertPrimitiveValue(checkInType.PrimitiveValue, base.workflowActionProcessor, true);

            var srcCheckInType = checkInType.PrimitiveValue.Value;
            switch (srcCheckInType)
            {
                case "Minor":
                    primitiveValue.Value.StringValue = "0";
                    break;
                case "Major":
                    primitiveValue.Value.StringValue = "1";
                    break;
                case "Overwrite":
                    primitiveValue.Value.StringValue = "2";
                    break;
            }


            return new Parameters
            {
                Name = "CheckInType",
                Required = false,
                DataType = "String",
                DesignerType = "ChoiceList",
                Direction = "Input",
                Value = new ParametersValue
                {
                    PrimitiveValue = primitiveValue
                },
                Options = new[]
                {
                    new Options {Text="Use Library Settings",Value="-1" },
                    new Options {Text="Minor Version",Value="0" },
                    new Options {Text="Major Version",Value="1"},
                    new Options {Text="No Version Change",Value="2"}
                }
            };
        }

        private Parameters CreateCommentParameters(ActivityParameter messageActivityParameter)
        {
            return new Parameters
            {
                Name = "Comment",
                Required = false,
                DataType = "String",//必须是大写的S
                DesignerType = "Text",
                Direction = "Input",
                Description = "Comment to accompany file check in.",
                Value = new ParametersValue { PrimitiveValue = NWPrimitiveValueConverter.ConvertPrimitiveValue(messageActivityParameter.PrimitiveValue, base.workflowActionProcessor, true) },
            };
        }

    }
}
