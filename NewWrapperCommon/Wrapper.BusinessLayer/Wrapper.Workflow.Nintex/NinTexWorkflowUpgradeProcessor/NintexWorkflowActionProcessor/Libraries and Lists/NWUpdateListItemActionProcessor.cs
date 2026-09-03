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
    class NWUpdateListItemActionProcessor : NWLibariesAndListsActionProcessor
    {

        public NWUpdateListItemActionProcessor(NintexWFActionProcessor workflowActionProcessor)
            : base(workflowActionProcessor)
        {
            CLASSNAME = "Microsoft.SharePoint.WorkflowServices.Activities.UpdateListItem";
        }

        protected override Image CreateImage()
        {
            return new Image
            {
                Src = "ext/workflowDesigner/Img/icons/NW2013_Action_IconMap.png?noCache=1440639687316",
                ClassName = CLASSNAME,
                x49x49 = 98,
                y49x49 = 237,
                x30x30 = 98,
                y30x30 = 286,
                x16x16 = 131,
                y16x16 = 286
            };
        }

        public override WorkflowAction UpgradeWorkflowAction(Native13NinTexWorkflowEntity.NWActionConfig nwActionConfig)
        {
            sourceConfig = nwActionConfig;

            var action = new WorkflowAction()
            {
                Id = actionId,
                ClassName = CLASSNAME,
                Configuration = CreateConfiguration()
            };

            return action;
        }

        protected override List<Property> CreateProperties()
        {
            ActivityParameter listId = null;
            ActivityParameter lookupField = null;
            ActivityParameter lookupFieldType = null;
            ActivityParameter lookupFieldValue = null;
            ActivityParameter thisItemValue = null;

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
            }

            CheckUnsupportedActionType(lookupFieldValue);

            var p0 = new Property
            {
                ID = "p0",
                DesignerType = "UpdateListItem",
                DisplayName = "Item",
                Parameters = new[]
                {
                    CreateListIdParameter(listId, lookupField, lookupFieldType,thisItemValue),
                    CreateItemGuidParameter(listId, lookupField, lookupFieldType, lookupFieldValue,thisItemValue),
                    CreateListItemProperties(listId)
                }
            };

            return new List<Property> { p0 };
        }

        private Parameters CreateListItemProperties(ActivityParameter listId)
        {
            var values = GetListItemsParameters(listId);

            return new Parameters()
            {
                Name = "ListItemProperties",
                DataType = "Dictionary",
                Description = "Select the list item properties used to update the item.",
                Required = true,
                Direction = "Input",
                DependentOn = "",
                OriginalSelectedValue = "",
                Value = new ParametersValue()
                {
                    Dictionary = values.ToArray()
                }
            };
        }

        protected override List<DictionaryValue> GetListItemsParameters(ActivityParameter listId)
        {
            List<DictionaryValue> values = new List<DictionaryValue>();

            foreach (var nwFieldReference in sourceConfig.FieldReferences)
            {
                if (string.Equals(nwFieldReference.Value, "Name", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(nwFieldReference.Value, "Title", StringComparison.OrdinalIgnoreCase))
                {
                    var title = values.Find(v => v.Key == "Title");
                    if (title != null)
                    {
                        continue;
                    }
                    values.Add(new DictionaryValue
                    {
                        Key = "Title",
                        Value = ConvertToValue(nwFieldReference),
                    });
                }
                else
                {
                    values.Add(new DictionaryValue
                    {
                        Key = nwFieldReference.Value,
                        Value = ConvertToValue(nwFieldReference),
                    });
                }
            }
            return values;
        }


    }
}
