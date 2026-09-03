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
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Common
{
    class AveTaxonomyFieldValue : AveClientObject, IAveTaxonomyFieldValue
    {
        #region IAveTaxonomyFieldValue Members
        private IAveTaxonomyField taxonomyField;
        public AveTaxonomyFieldValue()
        { }
        public AveTaxonomyFieldValue(IAveTaxonomyField field)
        {
            this.taxonomyField = field;
        }
        public string Label
        {
            get
            {
                return base.DataCache.GetProperty<string>("Label");
            }
            set
            {
                base.DataCache.AddChangedProperty("Label", value);
            }
        }

        public string TermGuid
        {
            get
            {
                return base.DataCache.GetProperty<string>("TermGuid");
            }
            set
            {
                base.DataCache.AddChangedProperty("TermGuid", value);
            }
        }

        public string ValidatedString
        {
            get
            {
                return base.DataCache.GetProperty<string>("ValidatedString");
            }
        }

        public int WssId
        {
            get
            {
                if (base.DataCache.GetProperty<int>("WssId") == 0)
                {
                    string[] array = this.taxonomyField.DefaultValue.Split(';');
                    if (array.Length > 1)
                    {
                        base.DataCache.AddProperty("WssId",Convert.ToInt32(array[0]));
                    }
                }
                return base.DataCache.GetProperty<int>("WssId");
            }
            set
            {
                base.DataCache.AddChangedProperty("WssId", value);
            }
        }

        public void PopulateFromLabelGuidPair(string text)
        {
            if (text == null)
            {
                throw new ArgumentException(AveSPResource.GetString("ErrorValueNotFormatted"));
            }
            string[] strArray = text.Split(new char[] { '|' });
            if (strArray.Length >= 2)
            {
                this.Label = strArray[0];
                this.TermGuid = strArray[strArray.Length - 1];
            }
            else
            {
                this.Label = string.Empty;
                this.TermGuid = text;
            }
        }
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            if (!string.IsNullOrEmpty(this.Label) || !string.IsNullOrEmpty(this.TermGuid))
            {
                builder.Append(this.Label);
                builder.Append('|');
                builder.Append(this.TermGuid);
            }
            return builder.ToString();
        }
        #endregion
    }
}
