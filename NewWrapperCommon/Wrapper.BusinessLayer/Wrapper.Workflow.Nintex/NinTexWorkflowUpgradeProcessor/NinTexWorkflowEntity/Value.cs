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
using System.Xml.Serialization;

namespace LS.SPWorkflowProcessor
{

    /// <remarks />
    [Serializable]
    [XmlType(AnonymousType = true)]
    public class Value
    {
        public Value()
        {
        }
        public Value(string value)
        {
            StringValue = value;
        }

        public Value(PrimitiveValue value)
        {
            PrimitiveValue = value;
        }

        public Value(ListLookup value)
        {
            ListLookup = value;
        }

        public Value(WorkflowContext value)
        {
            WorkflowContext = value;
        }

        public Value(DateTimeInfo dateTimeInfo)
        {
            DateTimeInfo = dateTimeInfo;
        }

        public Value(Variable variable)
        {
            Variable = variable;
        }
        /// <remarks />
        public WorkflowContext WorkflowContext { get; set; }

        /// <remarks />
        public ListLookup ListLookup { get; set; }

        /// <remarks />
        public PrimitiveValue PrimitiveValue { get; set; }

        [XmlElement("string")]
        public string StringValue { set; get; }


        public Variable Variable { set; get; }

        public DateTimeInfo DateTimeInfo
        {
            set;
            get;
        }

        public string Coercion { get; set; }
    }
}

