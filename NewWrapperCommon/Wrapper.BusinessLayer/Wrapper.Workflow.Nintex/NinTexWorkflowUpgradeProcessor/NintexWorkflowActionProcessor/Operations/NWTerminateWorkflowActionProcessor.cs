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

namespace LS.SPWorkflowProcessor
{
    class NWTerminateWorkflowActionProcessor : NintexOffice365ActionBase
    {
        //on-primise 对于AllExceptCurrentWorkflow在xml中存为以下值
        private const string AllExceptCurrentWorkflow = "494587C1-6210-4a5d-B8CB-B4DE70E89A27";

        public NWTerminateWorkflowActionProcessor(NintexWFActionProcessor workflowActionProcessor)
            : base(workflowActionProcessor)
        {
            CLASSNAME = "#NintexLive";
        }

        protected override Image CreateImage()
        {
            return new Image
            {
                Src = "https://ec.nintex.com/EXT/V1/Icons?type=primary&serviceId=7683c678-73b4-4ab7-a237-16261e45437f",
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
            configuration.Live = CreateLive();
            configuration.SubscriptionInfo = CreateSubscriptionInfo();
            return configuration;
        }

        private SubscriptionInfo CreateSubscriptionInfo()
        {
            return new SubscriptionInfo
            {
                EndDate = DateTime.Now,
                Type = "Free",
                ProductId = "TerminateWorkflow",
            };
        }
        private Live CreateLive()
        {
            return new Live
            {
                ServiceId = "7683c678-73b4-4ab7-a237-16261e45437f",
                VersionId = "20150115075609",
                ProductId = "TerminateWorkflow"
            };
        }

        protected override List<Property> CreateProperties()
        {
            var properties = new List<Property>();
            properties.Add(base.CreateDestinationSiteURLProperty("p0", "InputDestinationSiteUrl", "URL of the site or site collection containing the workflow instances to be terminated.<br /> Note: You can terminate workflow instances on other tenants."));
            properties.Add(base.CreateUserNameProperty("p1"));
            properties.Add(base.CreatePasswordProperty("p2", base.workflowActionProcessor.Web.Site.UserAccountInfo.Password, string.Empty));
            properties.Add(CreateWorkflowNameProperty());
            properties.Add(CreateListItemIDProperty());
            properties.Add(CreateTerminateActiveInstancesProperty());
            return properties;
        }

        private void CheckSupportCase(string workflowName)
        {
            if (string.Equals(workflowName, AllExceptCurrentWorkflow, StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException("Can not support terminate workflow action with All except current workflow parameter.");
            }
        }

        private Property CreateWorkflowNameProperty()
        {
            var workflowNameParameter = NWCommonUtility.GetActivityParameterByName(this.sourceConfig.Parameters, "WorkflowName", true);
            CheckSupportCase(workflowNameParameter.PrimitiveValue.Value);
            return new Property
            {
                ID = "p3",
                DesignerType = "Text",
                DisplayName = "Workflow name",
                Parameters = new Parameters[]
                {
                    new Parameters
                    {
                        Name="InputWorkflowName",
                        Required=true,
                        DataType ="String",
                        DesignerType="Text",
                        Direction = "Input",
                        Value = new ParametersValue
                        {
                            PrimitiveValue = new PrimitiveValue
                            {
                                Type="String",
                                Value = new Value(workflowNameParameter.PrimitiveValue.Value)
                            }
                        }
                    }
                }
            };
        }

        private Property CreateListItemIDProperty()
        {
            return new Property
            {
                ID = "p4",
                DesignerType = "Text",
                DisplayName = "List item ID",
                Parameters = new Parameters[]
                {
                    new Parameters
                    {
                        Name="InputItemID",
                        Description="Required for list - and library - specific workflows.< br /> Indicate the item with which workflow instances are associated.",
                        Required =false,
                        DataType ="String",
                        DesignerType="Text",
                        Direction = "Input",
                        //Value = new ParametersValue
                        //{
                        //    PrimitiveValue = new PrimitiveValue
                        //    {
                        //        Type="String",
                        //        Value = new Value(workflowNameParameter.PrimitiveValue.Value)
                        //    }
                        //}
                    }
                }
            };
        }

        private Property CreateTerminateActiveInstancesProperty()
        {
            return new Property
            {
                ID = "p5",
                DesignerType = "Boolean",
                DisplayName = "Terminate active instances",
                Parameters = new Parameters[]
                {
                    new Parameters
                    {
                        Name="InputTerminateRunningWorkflow",
                        Required =false,
                        DataType ="Boolean",
                        DesignerType="Boolean",
                        Direction = "Input",
                        Value = new ParametersValue
                        {
                            PrimitiveValue = new PrimitiveValue
                            {
                                Type="Boolean",
                                Value = new Value(bool.TrueString)
                            }
                        }
                    }
                }
            };
        }




    }
}
