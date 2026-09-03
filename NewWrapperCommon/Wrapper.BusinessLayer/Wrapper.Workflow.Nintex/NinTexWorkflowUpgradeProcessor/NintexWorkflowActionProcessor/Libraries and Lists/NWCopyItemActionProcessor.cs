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
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;

namespace LS.SPWorkflowProcessor
{
    class NWCopyItemActionProcessor : NWLibariesAndListsActionProcessor
    {
        AveLogger logger = AveLogger.GetInstance(typeof(NWCopyItemActionProcessor));

        public NWCopyItemActionProcessor(NintexWFActionProcessor workflowActionProcessor)
            : base(workflowActionProcessor)
        {
            CLASSNAME = "Microsoft.SharePoint.WorkflowServices.Activities.CopyItem";
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

        private bool IsDocumentLibrary(Guid listId)
        {
            try
            {
                return base.workflowActionProcessor.Web.GetList(listId) is IAveDocumentLibrary;
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while get list by id. List id: {0}, error: {1}", listId, e);
            }
            return false;
        }

        protected override Image CreateImage()
        {
            return new Image
            {
                Src = "ext/workflowDesigner/Img/icons/NW2013_Action_IconMap.png?noCache=1469003374396",
                ClassName = CLASSNAME,
                x49x49 = 147,
                y49x49 = 0,
                x30x30 = 147,
                y30x30 = 49,
                x16x16 = 180,
                y16x16 = 49
            };
        }

        protected override List<Property> CreateProperties()
        {
            ActivityParameter listId = null;
            ActivityParameter lookupField = null;
            ActivityParameter lookupFieldType = null;
            ActivityParameter lookupFieldValue = null;
            ActivityParameter destinationActivityParameter = null;
            ActivityParameter thisItemValue = null;
            ActivityParameter overwrite = null;

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
                else if (string.Equals(para.Name, "Destination", StringComparison.OrdinalIgnoreCase))
                {
                    destinationActivityParameter = para;
                }
                else if (string.Equals(para.Name, "Overwrite", StringComparison.OrdinalIgnoreCase))
                {
                    overwrite = para;
                }
            }

            base.CheckUnsupportedActionType(lookupFieldValue);

            var p0 = new Property
            {
                DesignerType = "ChooseDocumentLibraryItem",
                DisplayName = "Item",
                ID = "p0",
                Parameters = new[]
                {
                    CreateListIdParameter(listId,lookupField,lookupFieldType,thisItemValue),
                    CreateItemGuidParameter(listId,lookupField,lookupFieldType,lookupFieldValue,thisItemValue)
                }
            };

            var p1 = new Property
            {
                DesignerType = "ChooseDocumentLibrary",
                DisplayName = "Destination Library",
                ID = "p1",
                Parameters = new[] { CreateToListIdParameters(destinationActivityParameter) }
            };

            var p3 = new Property
            {
                ID = "p3",
                DesignerType = "Boolean",
                DisplayName = "Overwrite",

                Parameters = new[]
                {
                    CreateOverwriteParameter(overwrite)
                }
            };

            return new List<Property> { p0, p1, p3 };
        }

        private Parameters CreateOverwriteParameter(ActivityParameter overwriteActivityParameter)
        {
            return new Parameters
            {
                Name = "Overwrite",
                Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue { Type = "Boolean", Value = new Value(overwriteActivityParameter.PrimitiveValue.Value) },
                },

                Description = "",
                Required = true,
                DataType = "Boolean",
                DesignerType = "Boolean",
                Direction = "Input",
                DependentOn = "",
                OriginalSelectedValue = "",
            };
        }

        private Parameters CreateToListIdParameters(ActivityParameter destinationActivityParameter)
        {
            return new Parameters
            {
                Name = "ToListId",
                Description = "The destination library to copy the item to.",
                Required = true,
                DataType = "Guid",
                DesignerType = "ChooseDocumentLibrary",
                Direction = "Input",
                DependentOn = "",
                OriginalSelectedValue = "",
                Value = new ParametersValue
                {
                    ListLookup = new ListLookup
                    {
                        SelectList = destinationActivityParameter.PrimitiveValue.Value,
                        SelectField = "",
                        SelectFieldType = "",
                        WhereField = "",
                        WhereFieldType = "",
                        DisplayName = "",
                        DisplayValue = ""
                    }
                }

            };
        }


    }
}
