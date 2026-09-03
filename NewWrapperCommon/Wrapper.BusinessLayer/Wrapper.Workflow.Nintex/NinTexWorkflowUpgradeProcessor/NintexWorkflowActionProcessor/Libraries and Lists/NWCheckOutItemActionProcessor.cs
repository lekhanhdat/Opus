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
    class NWCheckOutItemActionProcessor : NWLibariesAndListsActionProcessor
    {
        public NWCheckOutItemActionProcessor(NintexWFActionProcessor workflowActionProcessor)
            : base(workflowActionProcessor)
        {
            CLASSNAME = "Microsoft.SharePoint.WorkflowServices.Activities.CheckOutItem";
        }

        public override WorkflowAction UpgradeWorkflowAction(NWActionConfig nwActionConfig)
        {
            sourceConfig = nwActionConfig;
            actionId = Guid.NewGuid().ToString();
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
                Src = "ext/workflowDesigner/Img/icons/NW2013_Action_IconMap.png?noCache=1469003374395",
                ClassName = CLASSNAME,
                x49x49 = 245,
                y49x49 = 316,
                x30x30 = 245,
                y30x30 = 365,
                x16x16 = 278,
                y16x16 = 365
            };
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

            var p1 = new Property
            {
                ID = "p0",
                DesignerType = "ChooseDocumentLibraryItem",
                DisplayName = "Item",
                Parameters = new[]
                {
                     CreateListIdParameter(listId, lookupField, lookupFieldType,thisItemValue),
                     CreateItemGuidParameter(listId, lookupField, lookupFieldType, lookupFieldValue,thisItemValue)
                }
            };

            return new List<Property> { p1 };
        }



    }
}
