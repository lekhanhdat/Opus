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
    class NWLogInHistoryListProcessor : NWActionProcessorBase
    {
        public NWLogInHistoryListProcessor(NintexWFActionProcessor workflowActionProcessor)
            : base(workflowActionProcessor)
        {
            CLASSNAME = "#WriteToHistory";
        }

        protected override Image CreateImage()
        {
            return new Image
            {
                Src = "ext/workflowDesigner/Img/icons/NW2013_Action_IconMap.png?noCache=1469003374405",
                ClassName = CLASSNAME,
                x49x49 = 392,
                y49x49 = 0,
                x30x30 = 392,
                y30x30 = 49,
                x16x16 = 425,
                y16x16 = 49,
            };
        }

        protected override List<Property> CreateProperties()
        {
            var sourceMessageParameter = NWCommonUtility.GetActivityParameterByName(this.sourceConfig.Parameters, "Message", true);
            var property = new Property
            {
                ID = "p0",
                DesignerType = "Text",
                DisplayName = "Message",
            };
            Parameters parameter = new Parameters
            {
                Name = "Message",
                Description = "Message to log to the history list.",
                Required = true,
                DataType = "String",
                DesignerType = "Text",
                Direction = "Input",
                Value=base.ConvertParameterValue(sourceMessageParameter),
            };
            property.Parameters = new Parameters[] { parameter };
            return new List<Property> { property };
        }
    }
}
