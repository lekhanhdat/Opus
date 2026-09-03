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
using AvePoint.Wrapper.Common;

namespace LS.SPWorkflowProcessor
{
    class NWCreateItemActionProcessor : NWLibariesAndListsActionProcessor
    {

        public NWCreateItemActionProcessor(NintexWFActionProcessor workflowActionProcessor)
            : base(workflowActionProcessor)
        {
            CLASSNAME = "Microsoft.SharePoint.WorkflowServices.Activities.CreateListItem";
        }

        protected override List<Property> CreateProperties()
        {
            ActivityParameter listId = null;
            ActivityParameter output = null;
            ActivityParameter contentType = null;
            ActivityParameter overwrite = null;

            foreach (var para in sourceConfig.Parameters)
            {

                if (string.Equals(para.Name, "ListId", StringComparison.OrdinalIgnoreCase))
                {
                    listId = para;
                }
                else if (string.Equals(para.Name, "Output", StringComparison.OrdinalIgnoreCase))
                {
                    output = para;
                }
                else if (string.Equals(para.Name, "ContentType", StringComparison.OrdinalIgnoreCase))
                {
                    contentType = para;
                }
                else if (string.Equals(para.Name, "Overwrite", StringComparison.OrdinalIgnoreCase))
                {
                    overwrite = para;
                }
            }

            var returnList = new List<Property>();

            returnList.Add(new Property
            {
                DesignerType = "CreateListItem",
                DisplayName = "Item",
                ID = "p0",
                Parameters = new[]
                {
                    CreateListIdParameter(listId,output,contentType),
                    CreateListItemPropertiesParameter(listId,contentType)
                }
            });


            returnList.Add(new Property
            {
                DesignerType = "Variable",
                DisplayName = "Output as GUID",
                ID = "p1",
                Parameters = new[] { CreateGUIDOutputPara() }
            });

            returnList.Add(new Property
            {
                DesignerType = "Variable",
                DisplayName = "Output as ID",
                ID = "ItemId",
                Parameters = new[] { CreateItemIdOutputPara(output) }
            });

            return returnList;
        }

        protected override Image CreateImage()
        {
            return new Image
            {
                Src = "ext/workflowDesigner/Img/icons/NW2013_Action_IconMap.png?noCache=1469003374394",
                ClassName = CLASSNAME,
                x49x49 = 441,
                y49x49 = 79,
                x30x30 = 441,
                y30x30 = 128,
                x16x16 = 474,
                y16x16 = 128
            };
        }

        private Parameters CreateGUIDOutputPara()
        {
            var para = new Parameters
            {
                Name = "Result",
                Description = "GUID variable to store the list item identifier as GUID (globally unique identifier).",
                Required = false,
                DataType = "Guid",
                Direction = "Output",
                DesignerType = "Variable",
                DependentOn = "",
                OriginalSelectedValue = "",
                Properties = new ParametersProperties(),
                Value = new ParametersValue
                {
                    Variable = new Variable
                    {
                        Name = "",
                        DataType = ""
                    }
                },
            };

            return para;
        }

        private Parameters CreateItemIdOutputPara(ActivityParameter output)
        {
            var para = new Parameters
            {
                Name = "ItemId",
                Description = "Integer variable to store the list item identifier as ID (the value of the ID property of the list item).",
                Required = false,
                DataType = "Int32",
                Direction = "Output",
                Value = new ParametersValue
                {
                    Variable = new Variable
                    {
                        Name = output.Variable.Name,
                        DataType = "Int32"
                    }
                },
                DesignerType = "Variable",
                DependentOn = "",
                OriginalSelectedValue = "",
                Properties = new ParametersProperties(),
            };

            return para;
        }

        private DictionaryValue GetListItemsContentTypeParameter(ActivityParameter contentType, ActivityParameter listIdParameter)
        {
            if (contentType.PrimitiveValue != null)
            {
                var contentTypeId = contentType.PrimitiveValue.Value;
                return new DictionaryValue
                {
                    Key = "ContentTypeId",
                    Value = new Value
                    {
                        PrimitiveValue = new PrimitiveValue
                        {
                            Type = "String",
                            Value = new Value(contentTypeId),
                        }
                    }
                };

            }
            return null;
        }

        private List<DictionaryValue> GetListItemsParameters(ActivityParameter listId, ActivityParameter contentType)
        {
            var values = GetListItemsParameters(listId);
            var contentTypeParameter = GetListItemsContentTypeParameter(contentType, listId);
            if (contentTypeParameter != null)
            {
                values.Add(contentTypeParameter);
            }
            return values;
        }

        private Parameters CreateListItemPropertiesParameter(ActivityParameter listId, ActivityParameter contentType)
        {
            var values = GetListItemsParameters(listId, contentType);

            var para = new Parameters
            {
                Name = "ListItemProperties",
                Description = "Select the list item properties used to create the item.",
                Required = true,
                DataType = "Dictionary",
                Direction = "Input",
                Value = new ParametersValue
                {
                    Dictionary = values.ToArray()
                },
                DependentOn = "",
                OriginalSelectedValue = "",
                Properties = new ParametersProperties(),
            };

            return para;
        }

    }
}
