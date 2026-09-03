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
using System.Security.Policy;
using System.Text;
using AvePoint.Wrapper.Common;
using Native13NinTexWorkflowEntity;

namespace LS.SPWorkflowProcessor
{
    class NWCollectionOperationProcessor:NWActionProcessorBase
    {

        protected CollectionOperationGenerator CollectionOperationGenerator { get; set; }

        public NWCollectionOperationProcessor(NintexWFActionProcessor workflowActionProcessor) : base(workflowActionProcessor)
        {
            CLASSNAME = "#CollectionOperation";
        }

        protected override Image CreateImage()
        {
            return CollectionOperationGenerator.CreateImage();
        }

        public override WorkflowAction UpgradeWorkflowAction(NWActionConfig nwActionConfig)
        {
            sourceConfig = nwActionConfig;
            InitCollectionOperationGenerator();

            var action = new WorkflowAction
            {
                Id = actionId,
                ClassName = CLASSNAME,
                Configuration = CreateConfiguration()
            };
            CollectionOperationGenerator.PostUpdateWorkflowAction(action, nwActionConfig);
            return action;
        }

        private void InitCollectionOperationGenerator()
        {
            var operationType = GetOperation(sourceConfig);
            CollectionOperationGenerator = CollectionOperationGenerator.CreateInstance(operationType);
        }

        private CollectionAvailableOperation GetOperation(NWActionConfig nwActionConfig)
        {
            var parameter=nwActionConfig.Parameters.FirstOrDefault(para => string.Equals(para.Name, "Operation"));
            if (parameter == null)
            {
                throw new NotSupportedException("Operation Type not found, cannot migrate the Collection Operation Action.");
            }
            string operation= parameter.PrimitiveValue.Value;
            if (string.IsNullOrEmpty(operation)||!Enum.IsDefined(typeof(CollectionAvailableOperation), operation))
            {
                throw new NotSupportedException("Invalid Operation Type, cannot migrate the Collection Operation Action."+operation);
            }
            return (CollectionAvailableOperation) Enum.Parse(typeof (CollectionAvailableOperation), operation);
        }

        protected override List<Property> CreateProperties()
        {
            return CollectionOperationGenerator.GenerateProperties(workflowActionProcessor, sourceConfig);
        }

      

        

     
    }
}
