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
using AvePoint.Common;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Restore;
using AvePoint.Wrapper.Restore.NintexForm;
using Native13NinTexWorkflowEntity;
using Newtonsoft.Json;
//using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;

namespace LS.SPWorkflowProcessor
{
    class NWFormFileProcessor
    {
        private Dictionary<string, string> mActionIdAndFormUrlMapping;
        private IAveSPWeb mParentWeb;
        private ExportedWorkflow mExportedWorkflow;
        private INintexDataMappingManager mMappingManager;
        private bool mIsPostAction;

        public NWFormFileProcessor(IAveSPWeb parentWeb, ExportedWorkflow exportedWorkflow, Dictionary<string, string> actionIdAndFormUrlMapping, INintexDataMappingManager mappingManager,bool isPostAction)
        {
            mParentWeb = parentWeb;
            mActionIdAndFormUrlMapping = actionIdAndFormUrlMapping;
            mExportedWorkflow = exportedWorkflow;
            mMappingManager = mappingManager;
            mIsPostAction = isPostAction;
        }

        private string GetFormXmlString(IAveList parentList, string customId, string formJson)
        {
            string jsonStr = string.Format("{{\"fileName\":\"\",\"form\":{0}}}", formJson);
            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonStr)))
            {
                string formXmlStr = mParentWeb.SPWeb.ConvertNintexFormJsonObjectToXml(formJson, string.Empty);
                var contentProcessor = new NintexFormContentProcessorOnline(mParentWeb, parentList);
                return contentProcessor.ReplaceFormContent(formXmlStr, string.Empty, mIsPostAction);
            }
        }

        private string GenerateFormXml(string innerText, string formXmlStr)
        {
            XmlDocument rootXD = new XmlDocument();
            rootXD.AppendChild(rootXD.CreateXmlDeclaration("1.0", "", null));
            XmlElement xe = rootXD.CreateElement("Form");
            XmlAttribute xa1 = rootXD.CreateAttribute("xmlns:xsd");
            xa1.Value = "http://www.w3.org/2001/XMLSchema";
            XmlAttribute xa2 = rootXD.CreateAttribute("xmlns:xsi");
            xa2.Value = "http://www.w3.org/2001/XMLSchema-instance";
            xe.Attributes.Append(xa1);
            xe.Attributes.Append(xa2);
            XmlNode root = rootXD.AppendChild(xe);
            XmlNode secondActionId = root.AppendChild(rootXD.CreateElement("ActionId"));
            secondActionId.InnerText = innerText;
            XmlNode secondFormXml = root.AppendChild(rootXD.CreateElement("FormXml"));
            secondFormXml.InnerText = formXmlStr;
            return rootXD.InnerXml;
        }

        public List<byte[]> GenerateFormFiles(IAveList parentList, string customId)
        {
            List<byte[]> forms = new List<byte[]>();
            List<ExtensionProperty> extensionPros = mExportedWorkflow.Configurations.ActionConfigs[0].ExtensionProperties;
            foreach (KeyValuePair<string, string> extensionPro in mActionIdAndFormUrlMapping)
            {
                foreach (ExtensionProperty ep in extensionPros)
                {
                    if (ep.Key.Equals(extensionPro.Value))
                    {
                        var formXmlStr = GetFormXmlString(parentList, customId, ep.Value);

                        var formXml = GenerateFormXml(extensionPro.Key, formXmlStr);

                        forms.Add(Encoding.UTF8.GetBytes(formXml));
                        break;
                    }
                }
            }

            return forms;
        }

        private XmlDocument ReplaceLookupListFormControlProperties(string nintexFormXml)
        {
            XmlDocument xd = new XmlDocument();
            xd.LoadXml(nintexFormXml);
            XmlNamespaceManager nsManager = new XmlNamespaceManager(xd.NameTable);
            nsManager.AddNamespace("ns", "http://schemas.datacontract.org/2004/07/Nintex.Forms"); //namespace is in export file
            nsManager.AddNamespace("d2p1", "http://schemas.datacontract.org/2004/07/Nintex.Forms.FormControls"); //namespace is in export file
            XmlNode formControls = xd.SelectSingleNode("/ns:Form/ns:FormControls", nsManager);

            string lookupListIdsString = string.Empty;
            foreach (XmlNode formControl in formControls.ChildNodes)
            {
                if (formControl.Attributes["i:type"].Value.Equals("d3p1:SharePointLookupFormControlProperties", StringComparison.OrdinalIgnoreCase))
                {
                    #region replace lookup list id
                    if (Validator.IsGuid(formControl.SelectSingleNode("d2p1:LookupList", nsManager).InnerText)) //If source list is expression, the value of setting may be list id.
                    {
                        Guid lookupListId = new Guid(formControl.SelectSingleNode("d2p1:LookupList", nsManager).InnerText);

                        lookupListIdsString = string.IsNullOrEmpty(lookupListIdsString) ? lookupListId.ToString() : string.Format("{0};{1}", lookupListIdsString, lookupListId.ToString());
                        Guid value = mMappingManager.GetListIdFromMapping(lookupListId);
                        formControl.SelectSingleNode("d2p1:LookupList", nsManager).InnerText = value.ToString();
                    }
                    #endregion

                    #region replace lookup web url
                    formControl.SelectSingleNode("d2p1:LookupWeb", nsManager).InnerText = mParentWeb.SPWeb.ServerRelativeUrl;
                    #endregion
                }
            }

            return xd;
        }
    }
}
