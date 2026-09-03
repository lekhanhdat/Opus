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
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Native13NinTexWorkflowEntity
{
    [Serializable, GeneratedCode("xsd", "2.0.50727.42"), DesignerCategory("code"), DebuggerStepThrough]
    public class NWFieldReference : ActivityParameter
    {
        // Properties
        [XmlElement("Choice", Form = XmlSchemaForm.Unqualified)]
        public string[] Choice
        {
            get;
            set;
        }

        [XmlAttribute]
        public ChangeType Dirty
        {
            get;
            set;
        }

        [XmlAttribute]
        public string InternalFieldName
        {
            get;
            set;
        }

        [XmlAttribute]
        public string LookupListDisplayName
        {
            get;
            set;
        }

        [XmlAttribute]
        public string LookupListFieldDisplayName
        {
            get;
            set;
        }

        [XmlAttribute]
        public bool Required
        {
            get;
            set;
        }

        [XmlIgnore]
        public bool RequiredSpecified
        {
            get;
            set;
        }

        [XmlAttribute]
        public string Type
        {
            get;
            set;
        }

        [XmlAttribute]
        public string Value
        {
            get;
            set;
        }

        // Nested Types
        public enum ChangeType
        {
            None,
            Edited,
            Added,
            Deleted
        }
    }


}
