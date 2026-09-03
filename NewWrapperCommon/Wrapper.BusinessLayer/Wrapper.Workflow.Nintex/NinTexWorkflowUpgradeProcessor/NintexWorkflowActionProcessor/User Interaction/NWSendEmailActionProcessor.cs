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
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using Native13NinTexWorkflowEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LS.SPWorkflowProcessor
{
    class NWSendEmailActionProcessor : NWActionProcessorBase
    {
        AveLogger logger = AveLogger.GetInstance(typeof(NWSendEmailActionProcessor));
        private const string BODY_DEFAULT_VALUE = "<span id=\"ms-rterangepaste-start\">Fill in the message body is necessary, modify here to fill out the message body.</span>";//HtmlEncode 对应 space

        public NWSendEmailActionProcessor(NintexWFActionProcessor workflowActionProcessor)
            : base(workflowActionProcessor)
        {
            CLASSNAME = "#SendNotificationActivity";
        }

        protected override List<Property> CreateProperties()
        {
            var property = new Property();
            property.ID = "p0";
            property.DesignerType = "Email";
            property.DisplayName = "Email";
            property.Parameters = CreateParameters();
            return new List<Property> { property };
        }

        protected override Image CreateImage()
        {
            return new Image
            {
                Src = "ext/workflowDesigner/Img/icons/NW2013_Action_IconMap.png?noCache=1469003374397",
                ClassName = CLASSNAME,
                x49x49 = 0,
                y49x49 = 158,
                x30x30 = 0,
                y30x30 = 207,
                x16x16 = 33,
                y16x16 = 207
            };

        }
        private Parameters[] CreateParameters()
        {
            var message = this.sourceConfig.Message;

            List<Parameters> parameters = new List<Parameters>();
            //Online body can not be empty
            parameters.Add(CreateBodyParameters(string.IsNullOrEmpty(message.Body) ? BODY_DEFAULT_VALUE : message.Body));
            parameters.Add(CreateToUserParameters(this.sourceConfig.Approvers));
            parameters.Add(CreateUserParameters(message.CcList, "CC", "Carbon copy recipients of the email.", false));
            parameters.Add(CreateSubjectParameters(message.Subject));
            parameters.Add(CreateUserParameters(message.BccList, "BCC", "Blind carbon copy recipients of the email.", false));
            parameters.Add(CreateAttachmentsParameters(message.Attachments));
            parameters.Add(CreateIncludeCurrentItemParameters(message.AttachFile));
            parameters.Add(CreateListBaseTypeParameters());
            parameters.Add(CreateEmailTypesParameters(message.Attachments.Count > 0 ? "externalEmail" : "internalEmail"));
            return parameters.ToArray();
        }

        private Parameters CreateListBaseTypeParameters()
        {

            Parameters parameter = new Parameters();
            parameter.Name = "ListBaseType";
            parameter.Required = false;
            parameter.DataType = "Int32";
            parameter.Direction = "Input";
            parameter.Value = new ParametersValue
            {
                PrimitiveValue = new PrimitiveValue { Type = "Int32", Value = new Value("-1") },
            };
            return parameter;
        }

        private Parameters CreateIncludeCurrentItemParameters(bool value)
        {
            Parameters parameter = new Parameters();
            parameter.Name = "IncludeCurrentItem";
            parameter.Required = false;
            parameter.DataType = "Boolean";
            parameter.Direction = "Input";
            parameter.Value = new ParametersValue
            {
                PrimitiveValue = new PrimitiveValue { Type = "Boolean", Value = new Value(value.ToString()) },
            };
            return parameter;
        }

        private Parameters CreateEmailTypesParameters(string emailType)
        {
            Parameters parameter = new Parameters();
            parameter.Name = "EmailTypes";
            parameter.Required = false;
            parameter.DataType = "String";
            parameter.Direction = "Input";
            parameter.Value = new ParametersValue
            {
                PrimitiveValue = new PrimitiveValue { Type = "String", Value = new Value(emailType) },
            };
            return parameter;

        }

        private Parameters CreateAttachmentsParameters(MessageAttachmentCollection attachments)
        {
            Parameters parameter = new Parameters();
            parameter.Name = "AttachmentsUrl";
            parameter.Required = false;
            parameter.DataType = "Collection";
            parameter.DesignerType = "Text";
            parameter.Direction = "Input";
            parameter.Value = new ParametersValue
            {
                Collection = new Collection
                {
                    SelectedValue = new List<SelectedValue>(),
                },
            };
            foreach (var attachment in attachments)
            {
                var url = attachment.Source;
                var selectValue = new SelectedValue { PrimitiveValue = new PrimitiveValue { Type = "String", Value = new Value(url) } };
                parameter.Value.Collection.SelectedValue.Add(selectValue);
            }
            return parameter;
        }

        private Parameters CreateToUserParameters(NWApprover[] approvers)
        {
            var users = new UserCollection();
            approvers.ToList().ForEach(item => users.Add(new Native13NinTexWorkflowEntity.User { UserID = item.User }));
            return CreateUserParameters(users, "To", "Recipients of the email.", true);
        }

        private Parameters CreateSubjectParameters(string subject)
        {
            Parameters parameter = new Parameters();
            parameter.Name = "Subject";
            parameter.Required = true;
            parameter.DataType = "String";
            parameter.DesignerType = "Text";
            parameter.Direction = "Input";
            parameter.Description = "Subject line of the email.";
            parameter.Value = new ParametersValue()
            {
                PrimitiveValue = NWPrimitiveValueConverter.ConvertPrimitiveValue(AveHtmlUtility.HtmlDecode(subject), "Text", base.workflowActionProcessor, false),
            };
            return parameter;
        }



        private Parameters CreateBodyParameters(string body)
        {
            Parameters parameter = new Parameters();
            parameter.Name = "Body";
            parameter.Required = true;
            parameter.DataType = "String";
            parameter.DesignerType = "Text";
            parameter.Direction = "Input";
            parameter.Description = "Body text of the email.";
            try
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(body);
                var nodes = doc.DocumentNode.Elements("style").ToList();
                for (var i = 0; i < nodes.Count(); i++)
                {
                    nodes[i].Remove();
                }
                body = doc.DocumentNode.OuterHtml;
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while parsing the email body.Error:{0}", e);
                body = body.Replace("<style>", "").Replace("</style>", "");
            }
            parameter.Value = new ParametersValue()
            {
                PrimitiveValue = NWPrimitiveValueConverter.ConvertPrimitiveValue(body, "Text", base.workflowActionProcessor, false),
            };
            return parameter;
        }

        private Parameters CreateUserParameters(UserCollection users, string parameterName, string description, bool required)
        {
            Parameters parameter = new Parameters();
            parameter.Name = parameterName;
            parameter.Description = description;
            parameter.Required = required;
            parameter.DataType = "Collection";
            parameter.Direction = "Input";
            parameter.Value = new ParametersValue
            {
                Collection = new Collection
                {
                    SelectedValue = new List<SelectedValue>(),
                },
            };
            foreach (var user in users)
            {
                var selectedValue = NWUserConverter.ConvertUserToSelectedValue(base.workflowActionProcessor, user.UserID);
                parameter.Value.Collection.SelectedValue.Add(selectedValue);
            }
            return parameter;
        }
    }
}
