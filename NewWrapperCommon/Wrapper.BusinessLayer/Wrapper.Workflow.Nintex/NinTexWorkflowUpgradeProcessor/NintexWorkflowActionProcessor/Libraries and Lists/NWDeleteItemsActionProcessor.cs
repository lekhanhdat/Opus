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
using System.Xml;
using LS.SPWorkflowProcessor;
using Native13NinTexWorkflowEntity;

namespace LS.SPWorkflowProcessor
{
    class NWDeleteItemsActionProcessor : NWLibariesAndListsActionProcessor
    {
        private string ListName;
        public NWDeleteItemsActionProcessor(NintexWFActionProcessor workflowActionProcessor)
            : base(workflowActionProcessor)
        {
            CLASSNAME = "Microsoft.SharePoint.WorkflowServices.Activities.DeleteListItem";
        }

        protected override Image CreateImage()
        {
            return new Image
            {
                Src = "ext/workflowDesigner/Img/icons/NW2013_Action_IconMap.png?noCache=1469150084844",
                ClassName = CLASSNAME,
                x49x49 = 196,
                y49x49 = 0,
                x30x30 = 196,
                y30x30 = 49,
                x16x16 = 229,
                y16x16 = 49
            };
        }

        protected override List<Property> CreateProperties()
        {
            ActivityParameter siteId = null;
            ActivityParameter webId = null;
            ActivityParameter isSetToBuilderMode = null;
            ActivityParameter query = null;
            ActivityParameter url = null;
            ActivityParameter hiddenUrl = null;

            foreach (var para in sourceConfig.Parameters)
            {
                if (string.Equals(para.Name, "SiteId", StringComparison.OrdinalIgnoreCase))
                {
                    siteId = para;
                }
                else if (string.Equals(para.Name, "WebId", StringComparison.OrdinalIgnoreCase))
                {
                    webId = para;
                }
                else if (string.Equals(para.Name, "IsSetToBuilderMode", StringComparison.OrdinalIgnoreCase))
                {
                    isSetToBuilderMode = para;
                }
                else if (string.Equals(para.Name, "Query", StringComparison.OrdinalIgnoreCase))
                {
                    query = para;
                }
                else if (string.Equals(para.Name, "Url", StringComparison.OrdinalIgnoreCase))
                {
                    url = para;
                }
                else if (string.Equals(para.Name, "HiddenUrl", StringComparison.OrdinalIgnoreCase))
                {
                    hiddenUrl = para;
                }
            }


            var p0 = new Property
            {
                ID = "p0",
                DesignerType = "Text",
                DisplayName = "Destination site URL",

                Parameters = new[]
                {
                    CreateInputDestinationSharePointUrlParameter( url ),
                },


            };



            var p2 = new Property
            {
                ID = "p2",
                DesignerType = "DisplayExpression",
                DisplayName = "Items to delete",
                Parameters = new[]
                {
                    CreateItemToDeleteParameter( url )
                }
            };

            var p1 = new Property
            {
                ID = "p1",
                DesignerType = "Text",
                DisplayName = "List name",

                Parameters = new[]
                {
                    CreateInputDestinationListTitleParameter()
                },
            };

            var p3 = new Property
            {
                ID = "p3",
                DesignerType = "Text",
                DisplayName = "SharePoint Online URL",
                Parameters = new[]
                {
                    CreateInputSharePointOnlineSiteUrlParameter(url)
                }
            };


            var p4 = new Property
            {
                ID = "p4",
                DesignerType = "Text",
                DisplayName = "Username",
                Parameters = new[]
                {
                    CreateInputUserNameParameter()
                }
            };

            var p5 = new Property
            {
                ID = "p5",
                DesignerType = "Secure",
                DisplayName = "Password",
                Parameters = new[]
                {
                    CreateInputUserNameParameter()
                }
            };


            var p6 = new Property
            {
                ID = "p6",
                DesignerType = "Variable",
                DisplayName = "All matched items deleted",
                Parameters = new[]
                {
                    CreateInputUserNameParameter()
                }
            };


            return new List<Property>() { p0, p1, p2, p3, p4, p5, p6 };

        }

        private Parameters CreateInputUserNameParameter()
        {
            return new Parameters()
            {
                Name = "InputPassword",
                Value = new ParametersValue()
                {
                    PrimitiveValue = new PrimitiveValue { Type = "String", Value = new Value("") }
                },
                Required = true,
                DataType = "String",
                DesignerType = "Text",
                Direction = "Input"
            };
        }

        private Parameters CreatePasswordParameter()
        {
            return new Parameters()
            {
                Name = "InputSharePointOnlineSiteUrl",
                Value = new ParametersValue()
                {
                    PrimitiveValue = new PrimitiveValue { Type = "String", Value = new Value("") }
                },
                Required = true,
                DataType = "String",
                DesignerType = "Text",
                Direction = "Input"
            };
        }

        private Parameters CreateInputSharePointOnlineSiteUrlParameter(ActivityParameter url)
        {
            return new Parameters()
            {
                Name = "InputSharePointOnlineSiteUrl",
                Value = new ParametersValue()
                {
                    PrimitiveValue = NWPrimitiveValueConverter.ConvertPrimitiveValue(url.PrimitiveValue, base.workflowActionProcessor, true),
                },
                Required = true,
                DataType = "String",
                DesignerType = "Text",
                Direction = "Input"
            };
        }

        private Parameters CreateInputDestinationListTitleParameter()
        {
            return new Parameters()
            {
                Value = new ParametersValue { PrimitiveValue = new PrimitiveValue { Type = "string", Value = new Value(this.ListName) }, },
                Required = true,
                DataType = "String",
                DesignerType = "Text",
                Direction = "Input"
            };
        }


        private Parameters CreateInputDestinationSharePointUrlParameter(ActivityParameter url)
        {
            return new Parameters()
            {
                Value = new ParametersValue() { PrimitiveValue = NWPrimitiveValueConverter.ConvertPrimitiveValue(url.PrimitiveValue, base.workflowActionProcessor, true) },
                Description = "e.g. http://targetdomain.sharepoint.com",
                Required = true,
                DataType = "String",
                DesignerType = "Text",
                Direction = "Input"
            };
        }

        private Parameters CreateItemToDeleteParameter(ActivityParameter query)
        {
            var itemToDelete = query.PrimitiveValue.Value;

            XmlDocument doc = new XmlDataDocument();
            doc.LoadXml(itemToDelete);

            var listNode = doc.DocumentElement.SelectSingleNode("/Query/Lists/List") as XmlElement;

            ListName = listNode.GetAttribute("Title");


            var whereNode = doc.DocumentElement.SelectSingleNode("/Query/Where") as XmlElement;

            XmlDocument newDocument = new XmlDocument();
            var viewNode = newDocument.CreateElement("View");
            viewNode.SetAttribute("Scope", "FilesOnly");
            newDocument.AppendChild(viewNode);
            var importedWhereNode = newDocument.ImportNode(whereNode, true);
            viewNode.AppendChild(importedWhereNode);




            return new Parameters()
            {
                Name = "InputDestinationListTitle",
                Value = new ParametersValue()
                {
                    PrimitiveValue = new PrimitiveValue { Type = "String", Value = new Value(newDocument.OuterXml) }
                },
                Required = true,
                DataType = "String",
                DesignerType = "DisplayExpression",
                Direction = "Input",
                Properties = new ParametersProperties()
            };

        }



    }
}
