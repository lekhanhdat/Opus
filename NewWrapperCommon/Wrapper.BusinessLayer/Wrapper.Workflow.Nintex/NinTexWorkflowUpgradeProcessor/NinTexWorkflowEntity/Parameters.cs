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
using System.Xml.Schema;
using System.Xml.Serialization;

namespace LS.SPWorkflowProcessor
{

    /// <remarks />
    [Serializable]
    [XmlType(AnonymousType = true)]
    public class Parameters
    {
        public Parameters()
        {
            Value = new ParametersValue();
        }

        /// <remarks />
        public string Name { get; set; }

        /// <remarks />
        public ParametersValue Value { get; set; }

        /// <remarks />
        public string Description { get; set; }

        /// <remarks />
        public bool Required { get; set; }

        /// <remarks />
        /// 如果是字符串类型，必须是String，不能是string
        public string DataType { get; set; }

        /// <remarks />
        public string DesignerType { get; set; }

        /// <remarks />
        public string Direction { get; set; }

        /// <remarks />
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string DependentOn { get; set; }

        /// <remarks />
        public ParametersProperties Properties { get; set; }

        /// <remarks />
        public string OriginalSelectedValue { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("Options")]
        public Options[] Options { set; get; }

        public string Type { get; set; }

        public string DefaultType { get; set; }

        public string SecureValue { get; set; }
    }
}