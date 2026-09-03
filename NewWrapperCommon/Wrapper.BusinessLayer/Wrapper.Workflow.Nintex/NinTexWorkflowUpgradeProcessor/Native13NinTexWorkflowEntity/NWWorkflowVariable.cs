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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Native13NinTexWorkflowEntity
{
    [Serializable, DebuggerStepThrough, DesignerCategory("code")]
    public class NWWorkflowVariable
    {
        // Properties
        [XmlElement("Choice", Form = XmlSchemaForm.Unqualified)]
        public string[] Choice { get; set; }

        [XmlAttribute]
        public WorkflowInitiationControlType ControlType { get; set; }

        [XmlAttribute]
        public string DefaultValue { get; set; }

        [XmlAttribute]
        public string Description { get; set; }

        [XmlAttribute]
        public ParameterDirection Direction { get; set; }

        [XmlAttribute]
        public bool Initiate { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public bool Required { get; set; }

        [XmlAttribute]
        public bool StartupOptionsConfigured { get; set; }

        [XmlAttribute]
        public string Type { get; set; }
    }


}
