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
using System.Collections.Generic;

namespace LS.SPWorkflowProcessor
{
    class NWPlaceHolderActionProcesor : NWActionProcessorBase
    {
        public NWPlaceHolderActionProcesor(NintexWFActionProcessor workflowActionProcessor) : base(workflowActionProcessor)
        {
            CLASSNAME = "#UnavailableAction";
        }

        protected override Image CreateImage()
        {
            return null;
        }

        protected override List<Property> CreateProperties()
        {
            var property = new Property();
            property.ID = "p0";
            property.DesignerType = "UnavailableAction";
            property.DisplayName = "Description";
            property.Parameters = new Parameters[]
            {
                new Parameters
                {
                    Name="Description",
                    Value = new ParametersValue
                    {
                        PrimitiveValue = new PrimitiveValue
                        {
                            Type="String",
                            Value = new Value("The action was replaced by this placeholder"),
                        },
                    },
                    Description = "Placeholder action to replace an unsupported action.",
                    Required = true,
                    DataType="String",
                    DesignerType = "UnavailableAction",
                    Direction = "Input",
                }
            };
            return new List<Property> { property };
        }

    }
}
