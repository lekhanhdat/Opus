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
    class NWRunParallelActionsActionProcessor : NWActionProcessorBase
    {
        public NWRunParallelActionsActionProcessor(NintexWFActionProcessor workflowActionProcessor)
            : base(workflowActionProcessor)
        {
            CLASSNAME = "#ParallelBlock";
        }

        public override WorkflowAction UpgradeWorkflowAction(Native13NinTexWorkflowEntity.NWActionConfig nwActionConfig)
        {
            var workflowAction = base.UpgradeWorkflowAction(nwActionConfig);
            workflowAction.Children = GenerateChildrenWorkflowAction(nwActionConfig.ChildActivities);
            return workflowAction;
        }

        protected override Image CreateImage()
        {
            return new Image
            {
                Src = "ext/workflowDesigner/Img/icons/NW2013_Action_IconMap.png?noCache=1469003374427",
                ClassName = CLASSNAME,
                x49x49 = 49,
                y49x49 = 79,
                x30x30 = 49,
                y30x30 = 128,
                x16x16 = 82,
                y16x16 = 128
            };
        }

        private List<WorkflowAction> GenerateChildrenWorkflowAction(NWActionConfig[] childActivities)
        {
            List<WorkflowAction> workflowActions = new List<WorkflowAction>();
            foreach (var child in childActivities)
            {
                var childWorkflwAction = this.workflowActionProcessor.WorkflowActionAdapter.UpgradeWorkflowAction(child);
                //当前研究发现 child 节点都是SequenceActivity Action,Name为branch
                childWorkflwAction.Configuration.Name = "Branch";
                workflowActions.Add(childWorkflwAction);
            }
            return workflowActions;
        }

        protected override List<Property> CreateProperties()
        {
            Property property = new Property();
            property.ID = "completionCondition";
            property.DesignerType = "Variable";
            property.DisplayName = "Completion Condition";
            property.Parameters = new Parameters[]
            {
                new Parameters
                {
                    Name="completionCondition",
                    Description="When this property evaluates to true, the parallel block will complete when an individual branch completes. If the property evaluates to false, or is not set, the parallel block waits for all branches to complete before continuing to the next action.",
                    Required=false,
                    DataType="Boolean",
                    DesignerType="Variable",
                    Direction="Input",
                }
            };
            return new List<Property> { property };
        }
    }
}
