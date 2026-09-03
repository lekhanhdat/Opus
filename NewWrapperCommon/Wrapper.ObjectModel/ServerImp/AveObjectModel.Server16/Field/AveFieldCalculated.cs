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


using System.Xml;
using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server16
{
    class AveFieldCalculated : AveField, IAveFieldCalculated
    {
        private SPFieldCalculated mFieldCalculated;
        private string mFieldRefsXml = string.Empty;

        public AveFieldCalculated(AveFieldCollection fieldColl, SPFieldCalculated field)
            : base(fieldColl, field)
        {
            mFieldCalculated = field;
        }

        public AveFieldCalculated(SPField fieldCalculated)
            : base(fieldCalculated)
        {
            mFieldCalculated = (SPFieldCalculated)fieldCalculated;
        }

        #region IAveFieldCalculated Members

        public AveDateTimeFieldFormatType DateFormat
        {
            get
            {
                return (AveDateTimeFieldFormatType)mFieldCalculated.DateFormat;
            }
            set
            {
                mFieldCalculated.DateFormat = (SPDateTimeFieldFormatType)value;
            }
        }

        public string Formula
        {
            get
            {
                return mFieldCalculated.Formula;
            }
            set
            {
                mFieldCalculated.Formula = value;
            }
        }

        public AveFieldType OutputType
        {
            get
            {
                return (AveFieldType)mFieldCalculated.OutputType;
            }
            set
            {
                mFieldCalculated.OutputType = (SPFieldType)value;
            }
        }

        public override string GetFieldValueAsText(object value)
        {
            return mFieldCalculated.GetFieldValueAsText(value);
        }

        public AveNumberFormatTypes DisplayFormat
        {
            get
            {
                return (AveNumberFormatTypes)mFieldCalculated.DisplayFormat;
            }
            set
            {
                mFieldCalculated.DisplayFormat = (SPNumberFormatTypes)value;
            }
        }

        public bool ShowAsPercentage
        {
            get
            {
                return mFieldCalculated.ShowAsPercentage;
            }
            set
            {
                mFieldCalculated.ShowAsPercentage = value;
            }
        }

        public int CurrencyLocaleId
        {
            get
            {
                return mFieldCalculated.CurrencyLocaleId;
            }
            set
            {
                mFieldCalculated.CurrencyLocaleId = value;
            }
        }

        public string FieldRefsXml
        {
            get
            {

                if (string.IsNullOrEmpty(mFieldRefsXml))
                {
                    if (!string.IsNullOrEmpty(SchemaXml))
                    {
                        var tempXmlDoc = new XmlDocument();
                        tempXmlDoc.LoadXml(SchemaXml);
                        var tempCalculatedFieldRootXmlElement = tempXmlDoc.DocumentElement;
                        var tempXmlFieldRefNode = tempCalculatedFieldRootXmlElement.SelectSingleNode("FieldRefs");
                        if (tempXmlFieldRefNode != null)
                        { return mFieldRefsXml = tempXmlFieldRefNode.InnerXml; }
                    }
                    return mFieldRefsXml;
                }
                else
                {
                    return mFieldRefsXml;
                }
            }
            set
            {
                var tempXmlDoc = new XmlDocument();
                tempXmlDoc.LoadXml(SchemaXml);
                var tempCalculatedFieldRootXmlElement = tempXmlDoc.DocumentElement;
                var tempFieldRefNode = tempCalculatedFieldRootXmlElement.SelectSingleNode("FieldRefs");
                if (tempFieldRefNode != null)
                {
                    tempFieldRefNode.InnerXml = mFieldRefsXml = value;
                }
                else
                {
                    var fieldRefsXml = tempXmlDoc.CreateElement("FieldRefs");
                    fieldRefsXml.InnerXml = mFieldRefsXml = value;
                    tempCalculatedFieldRootXmlElement.AppendChild(fieldRefsXml as XmlNode);
                }
            }
        }

        #endregion
    }
}
