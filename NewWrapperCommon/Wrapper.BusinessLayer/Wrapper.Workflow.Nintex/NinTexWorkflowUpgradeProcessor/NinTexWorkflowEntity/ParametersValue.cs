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
    [XmlType(AnonymousType = true, TypeName = "Value")]
    public class ParametersValue
    {
        private DictionaryValue[] dictionaryField;

        /// <remarks />
        [XmlArrayItem("DictionaryValue", IsNullable = false)]
        public DictionaryValue[] Dictionary
        {
            get { return dictionaryField; }
            set { dictionaryField = value; }
        }

        public Value Value { set; get; }

        public ListLookup ListLookup { set; get; }

        /// <remarks />
        public PrimitiveValue PrimitiveValue
        {
            set;
            get;
        }

        public Collection Collection
        {
            set;
            get;
        }

        [System.Xml.Serialization.XmlElementAttribute("user")]
        public User User
        {
            get;
            set;
        }

        public string Type
        {
            get;
            set;
        }

        public Variable Variable
        {
            get;
            set;
        }

        public string Coercion
        {
            get;
            set;
        }

        public WorkflowContext WorkflowContext
        {
            get;
            set;
        }

        [XmlElement("contentType")]
        public ContentType ContentType
        {
            get;
            set;
        }
    }
}