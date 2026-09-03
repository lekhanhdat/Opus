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
    class NWChangeStateActionProcessor : NWActionProcessorBase
    {
        private const string EXITSTATEMACHINE = "ExitStateMachine";
        public NWChangeStateActionProcessor(NintexWFActionProcessor workflowActionProcessor)
            : base(workflowActionProcessor)
        {
            CLASSNAME = "#SetNextState";
        }

        protected override Configuration CreateConfiguration()
        {
            var configuration = base.CreateConfiguration();
            configuration.StateConfiguration = new StateConfiguration();
            return configuration;
        }

        protected override Image CreateImage()
        {
            return new Image
            {
                Src = "ext/workflowDesigner/Img/icons/NW2013_Action_IconMap.png?noCache=1469003374429",
                ClassName = CLASSNAME,
                x49x49 = 49,
                y49x49 = 0,
                x30x30 = 49,
                y30x30 = 49,
                x16x16 = 79,
                y16x16 = 49
            };
        }


        protected override List<Property> CreateProperties()
        {
            var property = new Property();
            property.ID = "nextStateId";
            property.DisplayName = "Next State";
            property.DesignerType = "NextState";
            property.Parameters = new Parameters[]
            {
                new Parameters
                {
                    Name ="nextStateId",
                    Required =true,
                    DataType ="String",
                    Direction ="Input",
                    Value = CreateParametersValue(),
                },
            };
            return new List<Property>() { property };
        }

        private ParametersValue CreateExitStateParametersValue()
        {
            return new ParametersValue
            {
                PrimitiveValue = new PrimitiveValue
                {
                    Type = "String",
                    Value = new Value("__EXIT__"),
                }
            };
        }

        private ParametersValue CreateParametersValue()
        {
            var state = NWCommonUtility.GetActivityParameterByName(this.sourceConfig.Parameters, "State", true);
            if (string.Equals(state.PrimitiveValue.Value, EXITSTATEMACHINE, StringComparison.CurrentCulture))
            {
                return CreateExitStateParametersValue();
            }

            return base.ConvertParameterValue(state);
        }
        
    }
}
