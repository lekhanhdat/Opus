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
using AvePoint.Common;
using AvePoint.Wrapper.Common;
using Native13NinTexWorkflowEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LS.SPWorkflowProcessor
{
    class NWStartWorkflowProcessor : NintexOffice365ActionBase
    {
        private bool? isWebLevelWorkflow;

        public NWStartWorkflowProcessor(NintexWFActionProcessor workflowActionProcessor)
            : base(workflowActionProcessor)
        {
            CLASSNAME = "#NintexLive";
        }

        protected override Image CreateImage()
        {
            return new Image
            {
                Src = "https://ec.nintex.com/EXT/V1/Icons?type=primary&amp;serviceId=0a4ff141-09b9-4d0c-a1ab-29e94b805e81",
                ClassName = CLASSNAME,
                x49x49 = 0,
                y49x49 = 0,
                x30x30 = 0,
                y30x30 = 0,
                x16x16 = 0,
                y16x16 = 0
            };
        }

        protected override Configuration CreateConfiguration()
        {
            var configuration = base.CreateConfiguration();
            configuration.Live = new Live
            {
                ServiceId = "0a4ff141-09b9-4d0c-a1ab-29e94b805e81",
                VersionId = "20141021084521",
                ProductId = "StartWorkflow",
            };
            configuration.SubscriptionInfo = new SubscriptionInfo
            {
                EndDate = DateTime.UtcNow,
                Type = "Free",
                ProductId = "StartWorkflow",
            };
            return configuration;
        }
        protected override List<Property> CreateProperties()
        {
            var properties = new List<Property>();
            properties.Add(CreateDestinationSiteURLProperty());
            properties.Add(CreateWorkflowTypeProperty());
            properties.Add(CreateWorkflowNameProperty());
            properties.Add(CreateItemIDOrGUIDProperty());
            properties.Add(CreateSharePointURLProperty());
            properties.Add(CreateUserNameProperty());
            properties.Add(base.CreatePasswordProperty("p6", base.workflowActionProcessor.Web.Site.UserAccountInfo.Password, string.Empty));
            properties.Add(CreateWorkflowStartSuccessfulProperty());
            properties.Add(CreateWorkflowInstanceIDProperty());
            return properties;
        }

        private Property CreateDestinationSiteURLProperty()
        {
            return new Property
            {
                ID = "p0",
                DesignerType = "Text",
                DisplayName = "Destination site URL",
                Parameters = new Parameters[]
                {
                    new Parameters
                     {
                        Name ="InputDestinationSharePointUrl",
                        Required = true,
                        DataType ="String",
                        DesignerType = "Text",
                        Direction = "Input",
                        Value = new ParametersValue
                        {
                            PrimitiveValue= new PrimitiveValue
                            {
                               Type="String",
                               Value=new Value("{0}"),
                               FormatValues= new List<FormatValues>
                               {
                                   new FormatValues
                                   {
                                       SelectedValue=new SelectedValue
                                       {
                                           Coercion="AsDNString",
                                           WorkflowContext= new WorkflowContext {Value="CurrentWebUrl",Type="String" }
                                       }
                                   }
                               }
                            }
                        }
                     },
                },
            };
        }

        private Property CreateWorkflowTypeProperty()
        {
            var parameter = NWCommonUtility.GetActivityParameterByName(this.sourceConfig.Parameters, "AssociationId", true);
            isWebLevelWorkflow = IsWebLevelWorkflow(parameter.PrimitiveValue.Value);
            return new Property
            {
                ID = "p1",
                DesignerType = "ChoiceList",
                DisplayName = "Workflow type",
                Parameters = new Parameters[]
                {
                    new Parameters
                     {
                        Name="InputWorkflowType",
                        Required = true,
                        DataType ="String",
                        DesignerType = "ChoiceList",
                        Direction = "Input",
                        Options=new Options[]
                        {
                            new Options { Text = "Site workflow", Value = "Site" },
                            new Options { Text="List workflow",Value="List" }
                        },
                        Value = new ParametersValue
                        {
                            PrimitiveValue= new PrimitiveValue
                            {
                               Type="String",
                               Value=new Value(isWebLevelWorkflow.Value ? "Site" : "List"),
                            }
                        }
                     },
                },
            };
        }

        private bool IsWorkflowExistIn(List<IAveWorkflowSubscription> workflowSubscriptions, string workflowName)
        {
            return workflowSubscriptions.Exists(workflow => string.Equals(workflow.Name, workflowName, StringComparison.CurrentCulture));
        }

        private bool IsWebLevelWorkflow(string workflowName)
        {
            if (base.workflowActionProcessor.List == null)
            {
                //如果list==null说明该workflow为site level workflow,
                //只能选择site level的workflow,因此直接返回true
                return true;
            }

            var webWorkflow = base.workflowActionProcessor.Web.WorkflowAssociations.GetAssociationByName(workflowName, System.Globalization.CultureInfo.CurrentCulture);
            if (webWorkflow != null)
            {
                return true;
            }

            var listWorkflow = base.workflowActionProcessor.List.WorkflowAssociations.GetAssociationByName(workflowName, System.Globalization.CultureInfo.CurrentCulture);
            if (listWorkflow != null)
            {
                return false;
            }

            IAveWorkflowServicesManager manager = WrapperRuntime.CurrentContext.ModelFactory.CreateWorkflowServicesManager(base.workflowActionProcessor.Web);
            var subscriptionService = manager.GetWorkflowSubscriptionService();
            var webSubscriptions = subscriptionService.EnumerateSubscriptionsByEventSource(base.workflowActionProcessor.Web.ID).ToList();
            if (IsWorkflowExistIn(webSubscriptions, workflowName))
            {
                return true;
            }

            var listSubscriptions = subscriptionService.EnumerateSubscriptionsByList(base.workflowActionProcessor.List.ID).ToList();
            if (IsWorkflowExistIn(listSubscriptions, workflowName))
            {
                return false;
            }

            throw new NWNeedPostActionException(string.Format("Cannot find workflow by name: {0}", workflowName));
        }

        private Property CreateWorkflowNameProperty()
        {
            var parameter = NWCommonUtility.GetActivityParameterByName(this.sourceConfig.Parameters, "AssociationId", true);
            return new Property
            {
                ID = "p2",
                DesignerType = "Text",
                DisplayName = "Workflow name",
                Parameters = new Parameters[]
                {
                    new Parameters
                     {
                        Name="InputWorkflowName",
                        Required = true,
                        DataType ="String",
                        DesignerType = "Text",
                        Direction = "Input",
                        Value = new ParametersValue
                        {
                            PrimitiveValue= new PrimitiveValue
                            {
                               Type="String",
                               Value=new Value(parameter.PrimitiveValue.Value),
                            }
                        }
                     },
                },
            };
        }

        private Property CreateItemIDOrGUIDProperty()
        {
            var result = new Property
            {
                ID = "p3",
                DesignerType = "Text",
                DisplayName = "Item ID or GUID",
                Parameters = new Parameters[]
                {
                    new Parameters
                     {
                        Name="InputItemID",
                        Required = false,
                        DataType ="String",
                        DesignerType = "Text",
                        Direction = "Input",
                        Description="Note: Not required for Site workflow.",
                        Value = new ParametersValue
                        {
                            PrimitiveValue= new PrimitiveValue
                            {
                               Type="String",
                               Value=new Value(""),
                            }
                        }
                     },
                },
            };
            if (isWebLevelWorkflow.HasValue && !isWebLevelWorkflow.Value)
            {
                result.Parameters[0].Value.PrimitiveValue = new PrimitiveValue("String", "{0}")
                {
                    FormatValues = new List<FormatValues>() {
                        new FormatValues() {
                            SelectedValue = new SelectedValue() {
                                ListLookup = new ListLookup() {
                                    SelectList = "[Current Item]",
                                    SelectField = "ID",
                                    SelectFieldType = "Int32",
                                    DisplayName = "Current Item",
                                    DisplayValue = "ID"
                                }
                            }
                        }
                    }
                };
            }

            return result;
        }

        private Property CreateSharePointURLProperty()
        {
            return new Property
            {
                ID = "p4",
                DesignerType = "Text",
                DisplayName = "SharePoint URL",
                Parameters = new Parameters[]
                {
                    new Parameters
                     {
                        Name="InputSharePointOnlineSiteUrl",
                        Required = true,
                        DataType ="String",
                        DesignerType = "Text",
                        Direction = "Input",
                        Description="e.g. http://targetdomain.sharepoint.com",
                        Value = new ParametersValue
                        {
                            PrimitiveValue= new PrimitiveValue
                            {
                               Type="String",
                               Value=new Value(AveUrlUtility.GetServerUrl(base.workflowActionProcessor.Web.Site.Url)),
                            }
                        }
                     },
                },
            };
        }

        private Property CreateUserNameProperty()
        {
            return new Property
            {
                ID = "p5",
                DesignerType = "Text",
                DisplayName = "Username",
                Parameters = new Parameters[]
                {
                    new Parameters
                     {
                        Name="InputUserName",
                        Required = true,
                        DataType ="String",
                        DesignerType = "Text",
                        Direction = "Input",
                        Value = new ParametersValue
                        {
                            PrimitiveValue= new PrimitiveValue
                            {
                               Type="String",
                               Value=new Value(base.workflowActionProcessor.Web.Site.UserAccountInfo.UserName),
                            }
                        }
                     },
                },
            };
        }

        private Property CreateWorkflowStartSuccessfulProperty()
        {
            return new Property
            {
                ID = "p7",
                DesignerType = "Variable",
                DisplayName = "Workflow start successful",
                Parameters = new Parameters[]
                {
                    new Parameters
                     {
                        Name="OutputWorkflowStartSuccessful",
                        Required = false,
                        DataType ="Boolean",
                        DesignerType = "Variable",
                        Direction = "Output",
                        Description="Returns \"Yes\" if the workflow was successfully started.",
                        Value = new ParametersValue
                        {
                            Variable = new Variable {Name=string.Empty, DataType=string.Empty,},
                        }
                     },
                },
            };
        }

        private Property CreateWorkflowInstanceIDProperty()
        {
            var parameter = NWCommonUtility.GetActivityParameterByName(this.sourceConfig.Parameters, "InstanceId", true);
            var variable = new Variable { Name = string.Empty, DataType = string.Empty, };
            if (parameter.Variable != null && !string.IsNullOrEmpty(parameter.Variable.Name))
            {
                variable = base.workflowActionProcessor.VariablesCacheManager.GetSimpleVariable(parameter.Variable.Name);
            }
            return new Property
            {
                ID = "p8",
                DesignerType = "Variable",
                DisplayName = "Workflow instance ID",
                Parameters = new Parameters[]
                {
                    new Parameters
                     {
                        Name="OutputWorkflowInstanceID",
                        Required = false,
                        DataType ="String",
                        DesignerType = "Variable",
                        Direction = "Output",
                        Value = new ParametersValue
                        {
                            Variable =variable,
                        }
                     },
                },
            };
        }
    }
}
