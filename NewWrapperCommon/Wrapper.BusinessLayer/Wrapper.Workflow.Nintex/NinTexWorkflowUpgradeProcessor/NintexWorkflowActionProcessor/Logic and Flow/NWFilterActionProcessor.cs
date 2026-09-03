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
    class NWFilterActionProcessor : NWConditionActionBase
    {
        public NWFilterActionProcessor(NintexWFActionProcessor workflowActionProcessor)
            : base(workflowActionProcessor)
        {
            CLASSNAME = "#Filter";
        }

        protected override Image CreateImage()
        {
            return new Image
            {
                Src = "ext/workflowDesigner/Img/icons/NW2013_Action_IconMap.png?noCache=1469003374431",
                ClassName = CLASSNAME,
                x49x49 = 245,
                y49x49 = 395,
                x30x30 = 245,
                y30x30 = 444,
                x16x16 = 278,
                y16x16 = 444,
            };
        }

        protected override List<Property> CreateProperties()
        {
            var property = new Property();
            property.DesignerType = "ConditionBuilder";
            property.DisplayName = "Filter";
            property.ID = string.Format("Filter_{0}", Guid.NewGuid().ToString());
            property.Parameters = base.BuildParameters(sourceConfig.Condition);
            return new List<Property>() { property };
        }
    }
}
