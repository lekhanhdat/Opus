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
using AvePoint.RA.Contract.Services;
using AvePoint.GCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace RAFileSystem.SharePoint.EnforceRuleAction.LeaveStub
{
    public class OnPremSPLeaveStubUtility
    {
        private AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public string ConvertToXML(List<FieldDataInfo> itemFieldsInfo, string srcUrl)
        {
            try
            {
                XmlDocument xml = new XmlDocument();
                xml.LoadXml("<Document></Document>");
                XmlAttribute srcPath = xml.CreateAttribute("sourcePath");
                srcPath.Value = srcUrl;
                xml.DocumentElement.Attributes.Append(srcPath);
                foreach (FieldDataInfo fieldInfo in itemFieldsInfo)
                {
                    try
                    {
                        XmlElement e = xml.CreateElement("Metadata");

                        XmlAttribute internalNameWithTypeAttr = xml.CreateAttribute("internalNameWithType");
                        internalNameWithTypeAttr.Value = string.Format("{0}_{1}", fieldInfo.InternalName, fieldInfo.FieldType);
                        e.Attributes.Append(internalNameWithTypeAttr);
                        XmlAttribute internalNameAttr = xml.CreateAttribute("internalName");
                        internalNameAttr.Value = fieldInfo.InternalName;
                        e.Attributes.Append(internalNameAttr);
                        XmlAttribute typeAttr = xml.CreateAttribute("type");
                        typeAttr.Value = fieldInfo.FieldType;
                        e.Attributes.Append(typeAttr);
                        XmlAttribute valueAttr = xml.CreateAttribute("value");
                        valueAttr.Value = fieldInfo.Value;
                        e.Attributes.Append(valueAttr);
                        XmlAttribute nameAttr = xml.CreateAttribute("displayName");
                        nameAttr.Value = fieldInfo.DisplayName;
                        e.Attributes.Append(nameAttr);
                        //XmlAttribute schemaXmlAttr = xml.CreateAttribute("schemaXml");
                        //schemaXmlAttr.Value = fieldInfo.SchemaXml;
                        //e.Attributes.Append(schemaXmlAttr);
                        xml.DocumentElement.AppendChild(e);
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn(string.Format("Get Field failed ,field name : {0} , field value : {1}, Reason : {2}", fieldInfo.DisplayName.LogBase64(), fieldInfo.Value.LogBase64(), ex.ToString()));
                    }
                }
                return xml.InnerXml.ToString();
            }
            catch (Exception ex)
            {
                mLog.Warn("Error in Get Field XML" + ex.ToString());
                return null;
            }
        }
    }

    public class FieldDataInfo
    {
        public string DisplayName;
        public string InternalName;
        public string Value;
        public string FieldType;
    }
}

