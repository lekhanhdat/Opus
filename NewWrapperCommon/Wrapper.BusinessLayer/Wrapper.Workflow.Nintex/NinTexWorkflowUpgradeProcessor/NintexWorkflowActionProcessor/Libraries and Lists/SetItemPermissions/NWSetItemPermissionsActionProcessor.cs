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
using AvePoint.Wrapper.Resource;
using AvePoint.Wrapper.Resource.Workflow.Nintex;
using Native13NinTexWorkflowEntity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LS.SPWorkflowProcessor
{
    class NWSetItemPermissionsActionProcessor : NWLibariesAndListsActionProcessor
    {
        private string serviceId = "927A6553-5574-4871-8437-CFDA3BF2F8DC";
        private string productId = "Office365UpdateItemPermissions";

        public NWSetItemPermissionsActionProcessor(NintexWFActionProcessor workflowActionProcessor)
            : base(workflowActionProcessor)
        {
            CLASSNAME = "#NintexLive";
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

        protected override Configuration CreateConfiguration()
        {
            var configuration = base.CreateConfiguration();
            configuration.Id = serviceId;
            configuration.HelpKey = string.Format("NL{0}", serviceId.Replace("-", ""));
            configuration.Live = CreateLive();
            configuration.SubscriptionInfo = CreateSubscriptionInfo();
            return configuration;
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
                PreLoadedKey = string.Format("nw_i_{0}", serviceId.Replace("-", ""))
            };
        }

        protected override List<Property> CreateProperties()
        {
            bool inheritedEnabled = sourceConfig.InheritedEnabled;
            ActivityParameter overwrite = null;
            ActivityParameter message = null;
            ActivityParameter addToExistingPermissions = null;
            ActivityParameter thisItem = null;
            ActivityParameter listId = null;
            ActivityParameter lookupField = null;
            ActivityParameter lookupFieldType = null;
            ActivityParameter lookupFieldValue = null;

            foreach (var para in sourceConfig.Parameters)
            {
                if (string.Equals(para.Name, "Overwrite", StringComparison.OrdinalIgnoreCase))
                {
                    overwrite = para;
                }
                else if (string.Equals(para.Name, "Message", StringComparison.OrdinalIgnoreCase))
                {
                    message = para;
                }
                else if (string.Equals(para.Name, "AddToExistingPermissions", StringComparison.OrdinalIgnoreCase))
                {
                    addToExistingPermissions = para;
                }
                else if (string.Equals(para.Name, "ThisItem", StringComparison.OrdinalIgnoreCase))
                {
                    thisItem = para;
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

            #region p0
            var p0 = base.CreateDestinationSiteURLProperty("p0", "InputDestinationSharePointUrl", "");
           
            #endregion

            #region p1
            var p1 = new Property
            {
                ID = "p1",
                DesignerType = "Text",
                DisplayName = "List name",
                Parameters = new[]
                {
                    GenerateP1Parameters(thisItem, listId)
                }
            };
            #endregion

            #region p2
            var p2 = new Property
            {
                ID = "p2",
                DesignerType = "DisplayExpression",
                DisplayName = "Items to update",
                Parameters = new[]
                {
                    GenerateP2Parameters(thisItem, listId, lookupField, lookupFieldType, lookupFieldValue)
                }
            };
            #endregion

            #region p3
            var p3 = new Property
            {
                ID = "p3",
                DesignerType = "Boolean",
                DisplayName = "Inherit permissions from parent",
                Parameters = new[]
                {
                    new Parameters
                    {
                        Name = "InputInheritPermissions",
                        Value = new ParametersValue
                        {
                            PrimitiveValue = new PrimitiveValue("Boolean", overwrite.PrimitiveValue.Value)
                        },
                        Description = string.Empty,
                        Required = false,
                        DataType = "Boolean",
                        DesignerType = "Boolean",
                        Direction = "Input"
                    }
                }
            };
            #endregion

            #region p4
            var p4 = new Property
            {
                ID = "p4",
                DesignerType = "Boolean",
                DisplayName = "Remove existing permissions",
                Parameters = new[]
                {
                    new Parameters
                    {
                        Name = "InputRemoveExistingPermissions",
                        Value = new ParametersValue
                        {
                            PrimitiveValue = new PrimitiveValue("Boolean", Convert.ToBoolean(addToExistingPermissions.PrimitiveValue.Value.ToString()) ? "False" : "True")
                        },
                        Description = string.Empty,
                        Required = false,
                        DataType = "Boolean",
                        DesignerType = "Boolean",
                        Direction = "Input"
                    }
                }
            };
            #endregion

            #region p5
            var p5 = new Property
            {
                ID = "p5",
                DesignerType = "ChoiceList",
                DisplayName = "Target",
                Parameters = new[]
                {
                    new Parameters
                    {
                        Name = "InputSourceType",
                        Description = string.Empty,
                        Required = false,
                        DataType = "String",
                        DesignerType = "ChoiceList",
                        Direction = "Input",
                        Options = new Options[]
                        {
                            new Options
                            {
                                Text = "User",
                                Value = "User"
                            },
                            new Options
                            {
                                Text = "Group",
                                Value = "Group"
                            }
                        }
                    }
                }
            };
            if (message.PrimitiveValue.Value != string.Empty)
            {
                string targetType = message.PrimitiveValue.Value.Split(new string[] { "$$##" }, StringSplitOptions.None)[0].Split(new string[] { ";#" }, StringSplitOptions.None)[2]; //get target type from string, for example:DLBRANCH\qlluo;#Full Control;#User;#1073741829
                p5.Parameters[0].Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue("String", ConvertToTargetType(targetType))
                };
            }
            #endregion

            #region p6
            var p6 = new Property
            {
                ID = "p6",
                DesignerType = "Text",
                DisplayName = "User or group name",
                Parameters = new[] { GenerateP6Parameters(message) }
            };
            #endregion

            #region p7
            var p7 = new Property
            {
                ID = "p7",
                DesignerType = "ChoiceList",
                DisplayName = "Permission",
                Parameters = new[]
                {
                    new Parameters
                    {
                        Name = "InputPermission",
                        Value = new ParametersValue
                        {
                            PrimitiveValue = new PrimitiveValue("String", message.PrimitiveValue.Value.Equals(string.Empty) ? "Full Control" : message.PrimitiveValue.Value.Split(new string[] { "$$##" }, StringSplitOptions.None)[0].Split(new string[] { ";#" }, StringSplitOptions.None)[1]),
                        },
                        Description = string.Empty,
                        Required = false,
                        DataType = "String",
                        DesignerType = "ChoiceList",
                        Direction = "Input",
                        Options = new[]
                        {
                            new Options
                            {
                                Text = "Full Control",
                                Value = "Full Control"
                            },
                            new Options
                            {
                                Text = "Design",
                                Value = "Design"
                            },
                            new Options
                            {
                                Text = "Edit",
                                Value = "Edit"
                            },
                            new Options
                            {
                                Text = "Contribute",
                                Value = "Contribute"
                            },
                            new Options
                            {
                                Text = "Read",
                                Value = "Read"
                            },
                            new Options
                            {
                                Text = "View Only",
                                Value = "View Only"
                            },
                            new Options
                            {
                                Text = "Remove",
                                Value = "Remove"
                            },
                        }
                    }
                }
            };
            #endregion

            #region p8
            var p8 = base.CreateDestnationSharePointUrlProperty("p8", "InputSharePointOnlineSiteUrl", "e.g. http://targetdomain.sharepoint.com");
            #endregion

            #region p9
            var p9 = base.CreateUserNameProperty("p9");
            #endregion

            #region p10
            var p10 = base.CreatePasswordProperty("p10", base.workflowActionProcessor.Web.Site.UserAccountInfo.Password,string.Empty);
            p10.ID = "p10";
            p10.Parameters[0].Description = string.Empty;
            #endregion

            #region p11
            var p11 = new Property
            {
                ID = "p11",
                DesignerType = "Variable",
                DisplayName = "All matched items updated",
                Parameters = new[]
                {
                    new Parameters
                    {
                        Name = "OutputAllMatchedItemsUpdated",
                        Value = new ParametersValue(),
                        Description = "Returns \"Yes\" if all matched items were successfully updated.",
                        Required = false,
                        DataType = "Boolean",
                        DesignerType = "Variable",
                        Direction = "Output"
                    }
                }
            };
            #endregion

            #region p12
            var p12 = new Property
            {
                ID = "p12",
                DesignerType = "Variable",
                DisplayName = "List item ID",
                Parameters = new[]
                {
                    new Parameters
                    {
                        Name = "OutputListItemId",
                        Value = new ParametersValue(),
                        Description = string.Empty,
                        Required = false,
                        DataType = "DynamicValue",
                        DefaultType = "Array",
                        DesignerType = "Variable",
                        Direction = "Output"
                    }
                }
            };
            #endregion

            #region p13
            var p13 = new Property
            {
                ID = "p13",
                DesignerType = "Variable",
                DisplayName = "List item URL",
                Parameters = new[]
                {
                    new Parameters
                    {
                        Name = "OutputListItemUrl",
                        Value = new ParametersValue(),
                        Description = string.Empty,
                        Required = false,
                        DataType = "DynamicValue",
                        DefaultType = "Array",
                        DesignerType = "Variable",
                        Direction = "Output"
                    }
                }
            };
            #endregion

            return new List<Property> { p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13 };
        }

        private Live CreateLive()
        {
            return new Live
            {
                ServiceId = serviceId,
                VersionId = "20150819071304",
                ServiceName = "Office 365 update item permissions",
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

        private Parameters GenerateP1Parameters(ActivityParameter thisItem, ActivityParameter listId)
        {
            Parameters para = new Parameters
            {
                Name = "InputDestinationListTitle",
                Description = string.Empty,
                Required = true,
                DataType = "String",
                DesignerType = "Text",
                Direction = "Input"
            };

            if (Convert.ToBoolean(thisItem.PrimitiveValue.Value))
            {
                para.Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue("String", "{0}")
                    {
                        FormatValues = new List<FormatValues>
                        {
                            new FormatValues
                            {
                                SelectedValue = new SelectedValue
                                {
                                    Coercion = "AsDNString",
                                    WorkflowContext = new WorkflowContext
                                    {
                                        Value = "ListName",
                                        Type = "String"
                                    }
                                }
                            }
                        }
                    }
                };
            }
            else
            {
                para.Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue("String", listId.PrimitiveValue.Value)
                };
            }

            return para;
        }

        private Parameters GenerateP2Parameters(ActivityParameter thisItem, ActivityParameter listId, ActivityParameter lookupField, ActivityParameter lookupFieldType, ActivityParameter lookupFieldValue)
        {
            CheckUnsupportedActionType(lookupFieldValue);
            Parameters para = new Parameters();
            para.Name = "InputSelector";
            #region structure para.Value
            para.Value = new ParametersValue
            {
                PrimitiveValue = new PrimitiveValue("String", "")
                {
                    FormatValues = new List<FormatValues>
                        {
                            new FormatValues
                            {
                                SelectedValue = new SelectedValue
                                {
                                    PrimitiveValue = new PrimitiveValue("String", "{0}")
                                    {
                                        FormatValues = new List<FormatValues>
                                        {
                                            new FormatValues
                                            {
                                                SelectedValue = new SelectedValue
                                                {

                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                }
            };
            #endregion
            if (IsCurrentItem(thisItem))
            {
                para.Value.PrimitiveValue.Value.StringValue = "0<View Scope=\"FilesOnly\"><Query><Where><Eq><FieldRef Name=\"ID\"/><Value Type=\"Text\">{0}</Value></Eq></Where></Query></View>";
                var tmpValueLookup = new ValueLookup
                {
                    LookupType = SLLookupType.ThisItemLookup,
                    ListId = listId.PrimitiveValue.Value,
                    Field = new Field
                    {
                        Name = "ID",
                        Type = "Integer",
                    },
                };
                para.Value.PrimitiveValue.FormatValues[0].SelectedValue.PrimitiveValue.FormatValues[0].SelectedValue.ListLookup = NWListLookupConverter.ConvertListLookup(tmpValueLookup, workflowActionProcessor);
                para.Value.PrimitiveValue.FormatValues[0].SelectedValue.PrimitiveValue.FormatValues[0].SelectedValue.Coercion = "AsDNString";
            }
            else
            {
                para.Value.PrimitiveValue.Value.StringValue = string.Format("0<View Scope=\"FilesOnly\"><Query><Where><Eq><FieldRef Name=\"{0}\"/><Value Type=\"Text\">", lookupField.PrimitiveValue.Value) + "{0}</Value></Eq></Where></Query></View>";
                if (lookupFieldValue.PrimitiveValue != null)
                {
                    para.Value.PrimitiveValue = new PrimitiveValue("String", string.Format(para.Value.PrimitiveValue.Value.StringValue, lookupFieldValue.PrimitiveValue.Value));
                }
                else
                {
                    para.Value.PrimitiveValue.FormatValues[0].SelectedValue.PrimitiveValue.FormatValues[0].SelectedValue = GenerateSelectValueForP2Parameters(listId, lookupField, lookupFieldType, lookupFieldValue);
                }
            }
            para.Description = string.Empty;
            para.Required = true;
            para.DataType = "String";
            para.DesignerType = "DisplayExpression";
            para.Direction = "Input";

            #region set para.Properties
            var stream = Assembly.GetCallingAssembly().GetManifestResourceStream("Wrapper.Workflow.Nintex.NinTexWorkflowUpgradeProcessor.NintexWorkflowActionProcessor.Libraries_and_Lists.SetItemPermissions.DisplayExpression.txt");
            if (stream == null)
            {
                throw new FileNotFoundException("DisplayExpression.txt");
            }
            using (var sr = new StreamReader(stream))
            {
                para.Properties = new ParametersProperties
                {
                    DisplayExpression = sr.ReadToEnd()
                };
            }
            #endregion

            return para;
        }

        private SelectedValue GenerateSelectValueForP2Parameters(ActivityParameter listId, ActivityParameter lookupField, ActivityParameter lookupFieldType, ActivityParameter lookupFieldValue)
        {
            SelectedValue selectedValue = new SelectedValue();
            if (lookupFieldValue.Variable != null)
            {
                selectedValue.Variable = workflowActionProcessor.VariablesCacheManager.GetSimpleVariable(lookupFieldValue.Variable.Name);
                selectedValue.Coercion = lookupFieldValue.Coercion.Value;
            }
            else if (lookupFieldValue.ListLookup != null)
            {
                selectedValue.ListLookup = NWListLookupConverter.ConvertListLookup(lookupFieldValue.ListLookup, workflowActionProcessor);
                selectedValue.Coercion = "AsDNString";
            }
            else if (lookupFieldValue.WorkflowContextData != null)
            {
                selectedValue.WorkflowContext = NWWorkflowContextDataConverter.ConvertWorkflowContextData(lookupFieldValue.WorkflowContextData);
            }

            return selectedValue;
        }

        private Parameters GenerateP6Parameters(ActivityParameter message)
        {
            var para = new Parameters();
            para.Name = "InputUser";
            if (message.PrimitiveValue.Value != string.Empty)
            {
                para.Value.PrimitiveValue = new PrimitiveValue("String", "");
                List<string> users = message.PrimitiveValue.Value.Split(new string[] { "$$##" }, StringSplitOptions.None).ToList();

                string firstTargetType = ConvertToTargetType(users[0].Split(new string[] { ";#" }, StringSplitOptions.None)[2]);
                string firstPermission = users[0].Split(new string[] { ";#" }, StringSplitOptions.None)[1];
                int count = 0;
                foreach (var user in users)
                {
                    List<string> userInfo = user.Split(new string[] { ";#" }, StringSplitOptions.None).ToList();
                    if (!firstTargetType.Equals(ConvertToTargetType(userInfo[2]), StringComparison.OrdinalIgnoreCase))
                    {
                        throw new UnSupportedSettingException(string.Format(WrapperNintexWorkflowResource.UnSupportedSetting, CLASSNAME, "Users"));
                    }
                    else if (!firstPermission.Equals(userInfo[1], StringComparison.OrdinalIgnoreCase))
                    {
                        throw new UnSupportedSettingException(string.Format(WrapperNintexWorkflowResource.UnSupportedSetting, CLASSNAME, "Permission"));
                    }
                    if (userInfo[2].Equals("None", StringComparison.OrdinalIgnoreCase))
                    {
                        if (userInfo[0].StartsWith("{Common:", StringComparison.OrdinalIgnoreCase) || userInfo[0].StartsWith("{ItemProperty:", StringComparison.OrdinalIgnoreCase) || userInfo[0].StartsWith("{WorkflowVariable:", StringComparison.OrdinalIgnoreCase))
                        {
                            para.Value.PrimitiveValue.Value.StringValue = para.Value.PrimitiveValue.Value.StringValue == string.Empty ? ("{" + count.ToString() + "}") : (para.Value.PrimitiveValue.Value.StringValue + ";" + "{" + count.ToString() + "}");
                            if (para.Value.PrimitiveValue.FormatValues == null)
                            {
                                para.Value.PrimitiveValue.FormatValues = new List<FormatValues>();
                            }

                            List<FormatValues> tmpFormatValuesList = NWPrimitiveValueConverter.ConvertPrimitiveValueToFormatValuesList(new List<KeyValuePair<string, bool>> { new KeyValuePair<string, bool>(userInfo[0], false) }, workflowActionProcessor, true);
                            if (tmpFormatValuesList[0].SelectedValue != null)
                            {
                                if (tmpFormatValuesList[0].SelectedValue.ListLookup != null &&
                                    tmpFormatValuesList[0].SelectedValue.ListLookup.SelectFieldType.Equals("User", StringComparison.OrdinalIgnoreCase))
                                {
                                    tmpFormatValuesList[0].SelectedValue.Coercion = "UserEmailAddressAsText"; //Office 365中该action中的user name和user's email address这两项只支持Email格式的
                                }
                                else if (tmpFormatValuesList[0].SelectedValue.WorkflowContext != null &&
                                    tmpFormatValuesList[0].SelectedValue.WorkflowContext.Type.Equals("User", StringComparison.OrdinalIgnoreCase))
                                {
                                    tmpFormatValuesList[0].SelectedValue.Coercion = "UserEmailAddressAsText"; //Office 365中该action中的user name和user's email address这两项只支持Email格式的
                                }
                                else if (tmpFormatValuesList[0].SelectedValue.Variable != null &&
                                    workflowActionProcessor.VariablesCacheManager.GetVariable(tmpFormatValuesList[0].SelectedValue.Variable.Name, true).DataType.Equals("User", StringComparison.OrdinalIgnoreCase))
                                {
                                    tmpFormatValuesList[0].SelectedValue.Coercion = "UserEmailAddressAsText"; //Office 365中该action中的user name和user's email address这两项只支持Email格式的
                                }
                            }

                            para.Value.PrimitiveValue.FormatValues.AddRange(tmpFormatValuesList);
                            count++;
                        }
                        else
                        {
                            para.Value.PrimitiveValue.Value.StringValue = para.Value.PrimitiveValue.Value.StringValue == string.Empty ? userInfo[0] : (para.Value.PrimitiveValue.Value.StringValue + ";" + userInfo[0]);
                        }
                    }
                    else
                    {
                        para.Value.PrimitiveValue.Value.StringValue = para.Value.PrimitiveValue.Value.StringValue == string.Empty ? userInfo[0] : (para.Value.PrimitiveValue.Value.StringValue + ";" + userInfo[0]);
                    }
                }
            }
            para.Description = "Specify the value for the corresponding target to assign access permissions for the item.";
            para.Required = false;
            para.DataType = "String";
            para.DesignerType = "Text";
            para.Direction = "Input";

            return para;
        }

        private string ConvertToTargetType(string srcType)
        {
            string targetType = srcType;
            if (targetType.Equals("User", StringComparison.OrdinalIgnoreCase) || targetType.Equals("None", StringComparison.OrdinalIgnoreCase) || targetType.Equals("undefined", StringComparison.OrdinalIgnoreCase))
            {
                targetType = "User";
            }
            else
            {
                targetType = "Group";
            }
            return targetType;
        }
    }
}
