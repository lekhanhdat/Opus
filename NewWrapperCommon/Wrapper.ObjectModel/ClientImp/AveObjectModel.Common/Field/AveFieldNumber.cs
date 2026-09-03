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
using System.Xml;
using AvePoint.GCommon;
namespace AvePoint.ObjectModel.Common
{
    class AveFieldNumber : AveField, IAveFieldNumber
    {
        private IAveRequest mRequest;
        private AveList mParentList;
        private AveWeb mWeb;
        private AveFieldCollection mFieldCollection;
        private string mFieldSource;
        private Dictionary<string, object> mContentTypeProp;
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveFieldNumber));

        public AveFieldNumber(IAveRequest request, AveList list, AveWeb web, string fieldSource, AveFieldCollection fieldCollection, Dictionary<string, object> contentTypeProp, Dictionary<string, object> prop)
            : base(request, list, web, fieldSource, fieldCollection, contentTypeProp, prop)
        {
            mRequest = request;
            mParentList = list;
            mWeb = web;
            mFieldCollection = fieldCollection;
            mFieldSource = fieldSource;
            mContentTypeProp = contentTypeProp;
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(this.SchemaXml);
            if (doc.DocumentElement.HasAttribute("Percentage"))
            {
                prop["ShowAsPercentage"] = Convert.ToBoolean(doc.DocumentElement.GetAttribute("Percentage"));
            }
            base.DataCache.AddPropertyies(prop);
        }
        public double MaximumValue
        {
            get
            {
                return base.DataCache.GetProperty<double>("MaximumValue");
            }
            set
            {
                base.DataCache.AddChangedProperty("MaximumValue", value);
            }
        }
        public double MinimumValue
        {
            get
            {
                return base.DataCache.GetProperty<double>("MinimumValue");
            }
            set
            {
                base.DataCache.AddChangedProperty("MinimumValue", value);
            }
        }

        public bool ShowAsPercentage
        {
            get
            {
                return base.DataCache.GetProperty<bool>("ShowAsPercentage");
            }
            set
            {
                base.DataCache.AddChangedProperty("ShowAsPercentage", value);
            }
        }

        /// <summary>
        /// get this value from schemaxml because the API Enum cause default value effect the true value
        /// </summary>
        public AveNumberFormatTypes DisplayFormat
        {
            get
            {
                AveNumberFormatTypes formatType = AveNumberFormatTypes.Automatic;

                XmlDocument schemaXml = new XmlDocument();
                schemaXml.LoadXml(this.SchemaXml);

                if (schemaXml.DocumentElement.HasAttribute("Decimals"))
                {
                    int decimals = 0;
                    try
                    {
                        decimals = Convert.ToInt32(schemaXml.DocumentElement.GetAttribute("Decimals"));
                    }
                    catch(Exception ex)
                    {
                        mLogger.Warn("Get display format failed.Error Message:{0}.",ex.ToString());
                    }

                    if (decimals > 5 || decimals < -1)
                    {
                        formatType = AveNumberFormatTypes.Automatic;
                    }
                    else
                    {
                        formatType = (AveNumberFormatTypes)decimals;
                    }
                }

                return formatType;

                //return base.DataCache.GetProperty<AveNumberFormatTypes>("DisplayFormat");
                //throw new NotImplementedException();
            }
            set
            {
                base.DataCache.AddChangedProperty("DisplayFormat", value);
                //throw new NotImplementedException();
            }
        }
    }
}
