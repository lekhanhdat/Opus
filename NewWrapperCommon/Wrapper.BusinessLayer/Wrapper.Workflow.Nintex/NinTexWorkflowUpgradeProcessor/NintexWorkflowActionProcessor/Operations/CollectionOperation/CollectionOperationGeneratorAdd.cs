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
using Native13NinTexWorkflowEntity;

namespace LS.SPWorkflowProcessor
{
    internal class CollectionOperationGeneratorAdd : CollectionOperationGenerator
    {

        public override Image CreateImage()
        {
            return new Image
            {
                Src = "ext/workflowDesigner/Img/icons/NW2013_Action_IconMap.png?noCache=1469150084710",
                ClassName = "#CollectionOperationItemAdd",
                x49x49 = 294,
                y49x49 = 395,
                x30x30 = 294,
                y30x30 = 444,
                x16x16 = 327,
                y16x16 = 444
            };
        }

        public CollectionOperationGeneratorAdd(CollectionAvailableOperation availableOperation) : base(availableOperation)
        {
            ClassName = "#CollectionOperationItemAdd";
            InputCollectionDescription = "The collection variable to add the item to.";
            InputIndexDescription = "Index of the item to add.";
            OutputCollectionDescription = "Collection variable to store the collection with the added item.";
            ActionName = "Add Item to Collection";
        }

        public override List<Property> GenerateProperties(NintexWFActionProcessor nintexWorkflowActionProcessor,NWActionConfig config)
        {
            var property = new Property
            {
                ID = "CollectionOperationItemAdd",
                DesignerType = "CollectionOperation",
                DisplayName = "Output",
            };
            List<ActivityParameter> sourceParameters = config.Parameters.ToList();
            var targetParameter = GetActivityParameterByName(sourceParameters, "Target");
            //parameters need to be added in specific order
            List<Parameters> parameters = new List<Parameters>
            {
                CreateInputOperationParameter(),
                CreateInputCollectionParameter(nintexWorkflowActionProcessor, targetParameter),
                CreateIndexParameter(nintexWorkflowActionProcessor, GetActivityParameterByName(sourceParameters, "Index")),
                CreateInputValueParameter(nintexWorkflowActionProcessor, GetActivityParameterByName(sourceParameters, "LookupFieldValue")),
                CreateOutPutTypeParameter(),
                CreateOutputCollectionParameter(nintexWorkflowActionProcessor, targetParameter)
            };
            property.Parameters = parameters.ToArray();
            return new List<Property>() { property };
        }

        //private static Parameters[] SortParameters(List<Parameters> source)
        //{
        //    Parameters[] ps = new Parameters[6];
        //    var p1 = source.Find(p => p.Name == "InputOperation");
        //    var p2 = source.Find(p => p.Name == "InputCollection");
        //    var p3 = source.Find(p => p.Name == "InputIndex");
        //    var p4 = source.Find(p => p.Name == "InputValue");
        //    var p5 = source.Find(p => p.Name == "OutputType");
        //    var p6 = source.Find(p => p.Name == "OutputCollection");
        //    ps[0] = p1;
        //    ps[1] = p2;
        //    ps[2] = p3;
        //    ps[3] = p4;
        //    ps[4] = p5;
        //    ps[5] = p6;
        //    return ps;
        //}

    }
}