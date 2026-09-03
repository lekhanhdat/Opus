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
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public class VariableConfiguration
    {

        private string descriptionField;

        private object defaultValueField;

        private string defaultValueTypeField;

        private bool allowBlankField;

        private bool allowBlankFieldSpecified;

        private string displayFormatField;



        /// <remarks/>
        public string Description
        {
            get
            {
                return this.descriptionField;
            }
            set
            {
                this.descriptionField = value;
            }
        }

        /// <remarks/>
        public object DefaultValue
        {
            get
            {
                return this.defaultValueField;
            }
            set
            {
                this.defaultValueField = value;
            }
        }





        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AllowBlankSpecified
        {
            get
            {
                return this.allowBlankFieldSpecified;
            }
            set
            {
                this.allowBlankFieldSpecified = value;
            }
        }



      


        /// <remarks/>
        public bool AllowBlank
        {
            get
            {
                return this.allowBlankField;
            }
            set
            {
                this.allowBlankField = value;
            }
        }

        /// <remarks/>
        //public string DisplayFormat
        //{
        //    get
        //    {
        //        return this.displayFormatField;
        //    }
        //    set
        //    {
        //        this.displayFormatField = value;
        //    }
        //}



        /// <remarks/>
        //public bool Multiline
        //{
        //    get
        //    {
        //        return this.multilineField;
        //    }
        //    set
        //    {
        //        this.multilineField = value;
        //    }
        //}

        /// <remarks/>
        //public string DefaultValueType
        //{
        //    get
        //    {
        //        return this.defaultValueTypeField;
        //    }
        //    set
        //    {
        //        this.defaultValueTypeField = value;
        //    }
        //}
    }
}
