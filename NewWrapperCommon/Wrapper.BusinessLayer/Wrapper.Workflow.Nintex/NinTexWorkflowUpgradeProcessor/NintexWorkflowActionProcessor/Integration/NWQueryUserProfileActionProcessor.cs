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
    class NWQueryUserProfileActionProcessor : NintexOffice365ActionBase
    {
        private string serviceId = "3db45847-75e8-4e40-8195-6eaa605f8728";
        private string productId = "Office365QueryUserProfile";

        public NWQueryUserProfileActionProcessor(NintexWFActionProcessor workflowActionProcessor)
            : base(workflowActionProcessor)
        {
            CLASSNAME = "#NintexLive";
        }

        public override WorkflowAction UpgradeWorkflowAction(NWActionConfig nwActionConfig)
        {
            sourceConfig = nwActionConfig;
            actionId = Guid.NewGuid().ToString();
            var action = new WorkflowAction
            {
                Id = actionId,
                ClassName = CLASSNAME,
                Configuration = new Configuration()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = GetWorkflowActionName(),
                    Image = CreateImage(),
                    ServerInfo = new ServerInfo() { ClassName = CLASSNAME },
                    Properties = CreateProperties(),
                    HelpKey = "NL3DB4584775E84E4081956EAA605F8728",
                    Live = CreateLive(),
                    SubscriptionInfo = CreateSubscriptionInfo()
                }
            };

            return action;
        }

        protected override Image CreateImage()
        {
            return new Image
            {
                Src = string.Format("https://ec.nintex.com/EXT/V1/Icons?type=primary&serviceId={0}", serviceId),
                x49x49 = 0,
                y49x49 = 0,
                x30x30 = 0,
                y30x30 = 0,
                x16x16 = 0,
                y16x16 = 0,
                PreLoadedKey = string.Format("nw_i_{0}", serviceId)
            };
        }

        protected override List<Property> CreateProperties()
        {
            ActivityParameter queryUserName = null;
            ActivityParameter queryProfileProperties = null;
            ActivityParameter queryResultsVariable = null;
            ActivityParameter username = null;
            ActivityParameter password = null;

            foreach (var para in sourceConfig.Parameters)
            {
                if (string.Equals(para.Name, "QueryUserName", StringComparison.OrdinalIgnoreCase))
                {
                    queryUserName = para;
                }
                else if (string.Equals(para.Name, "QueryProfileProperties", StringComparison.OrdinalIgnoreCase))
                {
                    queryProfileProperties = para;
                }
                else if (string.Equals(para.Name, "QueryResultsVariable", StringComparison.OrdinalIgnoreCase))
                {
                    queryResultsVariable = para;
                }
                else if (string.Equals(para.Name, "Username", StringComparison.OrdinalIgnoreCase))
                {
                    username = para;
                }
                else if (string.Equals(para.Name, "Password", StringComparison.OrdinalIgnoreCase))
                {
                    password = para;
                }
            }

            ValueStorageCollection valueStorageItems = sourceConfig.ValueStorageCollection.ValueStorageItems;

            var p0 = base.CreateDestnationSharePointUrlProperty("p0", "InputDestinationSharePointUrl", "e.g. https://targetdomain.sharepoint.com");

            var p1 = GenerateCommonUsernameProperty();

            var p2 = GenerateQuerUserPasswordProperty();

            var p3 = new Property
            {
                ID = "p3",
                DesignerType = "Text",
                DisplayName = "User's email address",
                Parameters = new[]
                {
                    new Parameters
                    {
                        Name="InputUsers",
                        Value= GenerateUserNameRelatedParameter(),
                        Description="Specify an email address corresponding to a SharePoint user profile.",
                        Required=true,
                        DataType="String",
                        DesignerType="Text",
                        Direction="Input"
                    }
                }

            };

            string properties = string.Empty;
            if (valueStorageItems != null && valueStorageItems.Count > 0)
            {
                foreach (var valueStorage in valueStorageItems)
                {
                    if (string.IsNullOrEmpty(properties))
                    {
                        properties = valueStorage.ValueIdentifier;
                    }
                    else
                    {
                        properties += string.Format(",{0}", valueStorage.ValueIdentifier);  //Office 365该action中只支持query一个property，但是即使填写多个property，使用“，”分隔，run workflow时也不会出错，只不过只会query第一个property
                    }
                }
            }
            else if (queryProfileProperties != null) //原端query一个property的时候，property信息会存储在QueryProfileProperties中
            {
                properties = queryProfileProperties.PrimitiveValue.Value;
            }
            var p4 = new Property
            {
                ID = "p4",
                DesignerType = "Text",
                DisplayName = "Property",
                Parameters = new[]
                {
                    new Parameters
                    {
                        Name="InputProperties",
                        Value= new ParametersValue() {
                            PrimitiveValue = new PrimitiveValue("String", properties)
                        },
                        Description="Specify the property to query in the profile of the specified user.",
                        Required=true,
                        DataType="String",
                        DesignerType="Text",
                        Direction="Input",
                    }
                }
            };

            var p5 = new Property
            {
                ID = "p5",
                DesignerType = "Variable",
                DisplayName = "Store property in",
                Parameters = new[]
                {
                    new Parameters
                    {
                        Name="OutputUserProfilesFirstProperty",
                        Value = new ParametersValue
                        {
                             Variable=new Variable
                             {
                                 Name= valueStorageItems.Count > 0 ? valueStorageItems[0].VariableName : queryResultsVariable.Variable.Name,
                                 DataType="String"
                             }
                        },
                        Description="Specify a text workflow variable to store the value of the property.",
                        Required = false,
                        DataType="String",
                        DesignerType="Variable",
                        Direction="Output",
                    }
                }
            };

            return new List<Property> { p0, p1, p2, p3, p4, p5 };
        }

        private Property GenerateQuerUserPasswordProperty()
        {
            var passwordParameter = sourceConfig.Parameters.First(parameter => string.Equals("Password", parameter.Name, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrEmpty(passwordParameter.PrimitiveValue.Value))
            {
                return base.CreatePasswordProperty("p2", base.workflowActionProcessor.Web.Site.UserAccountInfo.Password, "Specify the password for the above username.");
            }

            return base.CreatePasswordProperty("p2", passwordParameter.PrimitiveValue.Value, "Specify the password for the above username.");
        }

        private Property GenerateCommonUsernameProperty()
        {
            var userNameProperty = base.CreateUserNameProperty("p1");
            userNameProperty.Parameters[0].Description = "Specify a username with permissions to query the user profiles.";

            var userNameParameter = sourceConfig.Parameters.First(parameter => string.Equals("Username", parameter.Name, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(userNameParameter.PrimitiveValue.Value))
            {
                userNameProperty.Parameters[0].Value = new ParametersValue { PrimitiveValue = NWPrimitiveValueConverter.ConvertPrimitiveValue(userNameParameter.PrimitiveValue, base.workflowActionProcessor, true) };
            }

            return userNameProperty;
        }

        protected ParametersValue GenerateUserNameRelatedParameter()
        {
            var parameterValue = new ParametersValue { PrimitiveValue = new PrimitiveValue() };
            ActivityParameter queryUserName = sourceConfig.Parameters.First(parameter => string.Equals("QueryUserName", parameter.Name, StringComparison.OrdinalIgnoreCase));
            var userString = queryUserName.PrimitiveValue.Value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[0];//由于Online只支持获取单个user profile，因此此处只对第一个user 进行处理

            string tmpDestUser = string.Empty;
            if (userString.StartsWith("{ItemProperty:", StringComparison.OrdinalIgnoreCase) || userString.StartsWith("{Common:", StringComparison.OrdinalIgnoreCase) || userString.StartsWith("{WorkflowVariable:", StringComparison.OrdinalIgnoreCase))
            {
                parameterValue.PrimitiveValue = new PrimitiveValue()
                {
                    Type = "String",
                    Value = new Value("{0}"),
                    FormatValues = NWPrimitiveValueConverter.ConvertPrimitiveValueToFormatValuesList(new List<KeyValuePair<string, bool>>() { new KeyValuePair<string, bool>(userString, false) }, workflowActionProcessor, true)
                };
                if (parameterValue.PrimitiveValue.FormatValues[0].SelectedValue != null)
                {
                    if (userString.StartsWith("{ItemProperty:", StringComparison.OrdinalIgnoreCase) &&
                        parameterValue.PrimitiveValue.FormatValues[0].SelectedValue.ListLookup.SelectFieldType.Equals("User", StringComparison.OrdinalIgnoreCase))
                    {
                        parameterValue.PrimitiveValue.FormatValues[0].SelectedValue.Coercion = "UserEmailAddressAsText"; //Office 365中该action中的user name和user's email address这两项只支持Email格式的
                    }
                    else if (userString.StartsWith("{Common:", StringComparison.OrdinalIgnoreCase) &&
                        parameterValue.PrimitiveValue.FormatValues[0].SelectedValue.WorkflowContext.Type.Equals("User", StringComparison.OrdinalIgnoreCase))
                    {
                        parameterValue.PrimitiveValue.FormatValues[0].SelectedValue.Coercion = "UserEmailAddressAsText"; //Office 365中该action中的user name和user's email address这两项只支持Email格式的
                    }
                    else if (userString.StartsWith("{WorkflowVariable:", StringComparison.OrdinalIgnoreCase) &&
                        workflowActionProcessor.VariablesCacheManager.GetVariable(userString.Substring("{WorkflowVariable:".Length).Trim('}'), true).DataType.Equals("User", StringComparison.OrdinalIgnoreCase))
                    {
                        parameterValue.PrimitiveValue.FormatValues[0].SelectedValue.Coercion = "UserEmailAddressAsText"; //Office 365中该action中的user name和user's email address这两项只支持Email格式的
                    }
                }
            }
            else
            {
                parameterValue.PrimitiveValue = new PrimitiveValue("String", userString); //Office 365中该setting是必填项，但是on-premises中该setting不是必填的，如果原端没有配置该setting，为了保证publish成功，暂时使用字符串“UserName@XXX.onmicrosoft.com”填充该setting
            }


            return parameterValue;
        }

        private Live CreateLive()
        {
            return new Live
            {
                ServiceId = serviceId,
                VersionId = "20150115072936",
                ServiceName = "Office 365 Query User Profile",
                ProductId = productId
            };
        }

        private SubscriptionInfo CreateSubscriptionInfo()
        {
            //当前365站点的subscription信息
            return new SubscriptionInfo()
            {
                Type = "Free",
                ProductId = productId,
                EndDate = DateTime.Now.AddYears(1)
            };
        }
    }
}
