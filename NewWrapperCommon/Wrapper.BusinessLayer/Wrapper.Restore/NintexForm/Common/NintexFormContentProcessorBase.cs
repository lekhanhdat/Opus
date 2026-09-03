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
using System.Xml;
using AvePoint.Common;
using AvePoint.GCommon.Utility;
using AvePoint.Wrapper.Common;
using System.Xml.Linq;

namespace AvePoint.Wrapper.Restore.NintexForm
{
    abstract class NintexFormContentProcessorBase : INintexFormContentProcessor
    {
        protected IAveList mAveList;
        protected IAveSPWeb mAveSPWeb;

        protected abstract Dictionary<Guid, AveNintexFormControlType> NintexFormControlTypeMapping { get; }
        protected abstract string RemoveUnsupportedFormControl(string formXml);
        protected abstract string ReplaceNintexFormContent(XmlDocument xd, string contentTypeId);
        //添加option 对于找不到的数据 是否继续执行，主要为了on-premise to on-premise workflow和form 结合的case使用
        private bool needContinue = false;
        protected NintexFormContentProcessorBase(IAveSPWeb web, IAveList list)
        {
            mAveSPWeb = web;
            mAveList = list;
        }
        protected NintexFormContentProcessorBase(IAveSPWeb web, IAveList list, bool needContinue) : this(web, list)
        {
            this.needContinue = needContinue;
        }


        private bool IsResponsive(XmlDocument formXml, XmlNamespaceManager nsManager)
        {
            var IsResponsive = formXml.SelectSingleNode("/ns:Form/ns:IsResponsive", nsManager);
            return IsResponsive == null ? false : string.Equals(IsResponsive.InnerText, bool.TrueString, StringComparison.OrdinalIgnoreCase);
        }

        public string ReplaceFormContent(string formXml, string contentTypeId, bool isPost)
        {
            XmlNamespaceManager nsManager;
            var newFormXml = RemoveUnsupportedFormControl(formXml);
            var formXd = GenerateXmlDocument(newFormXml, out nsManager);
            ReplaceControlContent(formXd, nsManager, contentTypeId, isPost);
            ReplaceResponsiveFormControlContent(formXd, nsManager, contentTypeId, isPost);
            ReplaceFormLayoutContent(formXd, nsManager);
            ReplaceIconNodeUrl(formXd.DocumentElement, nsManager, isPost);
            var finalFormXml = ReplaceNintexFormContent(formXd, contentTypeId);
            return finalFormXml;
        }



        private XmlDocument GenerateXmlDocument(string nintexFormXml, out XmlNamespaceManager nsManager)
        {
            XmlDocument xd = new XmlDocument();
            xd.LoadXml(nintexFormXml);
            nsManager = GenerateXmlNamespaceManager(xd);
            return xd;
        }
        private void ReplaceIconNodeUrl(XmlNode rootNode, XmlNamespaceManager nsManager, bool isPost)
        {
            //ADO-199143 icon 比较特殊，如果文件不存在，或者空格不替换为%20，publish时会找不到文件 导致publish 失败
            string xPath = "/ns:Form/ns:Icon";
            var node = rootNode.SelectSingleNode(xPath, nsManager);
            if (node != null && !string.IsNullOrEmpty(node.InnerText))
            {
                node.InnerText = UrlReplace(node.InnerText).Replace(" ", "%20");
                if (!isPost && !CheckFileExists(node.InnerText))
                {
                    throw new AveNintexFormPostException("Icon file not exists.");
                }
            }
        }

        private bool CheckFileExists(string iconFileUrl)
        {
            try
            {
                var file = this.mAveSPWeb.SPWeb.GetFile(iconFileUrl);
                return file.Exists;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private void ReplaceNodeUrl(XmlNode rootNode, XmlNamespaceManager nsManager, string xPath)
        {
            var node = rootNode.SelectSingleNode(xPath, nsManager);
            if (node != null)
            {
                node.InnerText = UrlReplace(node.InnerText);
            }
        }
        private void ReplaceFormLayoutContent(XmlDocument formXd, XmlNamespaceManager nsManager)
        {
            XmlNode formLayouts = formXd.SelectSingleNode("/ns:Form/ns:FormLayouts", nsManager);
            foreach (XmlNode formLayout in formLayouts.ChildNodes)
            {
                ReplaceNodeUrl(formLayout, nsManager, "ns:BackgroundImageUrl");
                ReplaceNodeUrl(formLayout, nsManager, "ns:RedirectUrl");
            }
        }

        private void ReplaceResponsiveFormControlContent(XmlDocument document, XmlNamespaceManager nsManager, string contentTypeId, bool isPost)
        {
            if (!IsResponsive(document, nsManager))
            {
                return;
            }

            var responsiveFormNode = document.SelectSingleNode("/ns:Form/ns:ResponsiveForm", nsManager);
            var rootPrefix = responsiveFormNode.GetPrefixOfNamespace("http://schemas.datacontract.org/2004/07/Nintex.Forms.Responsive");

            if (string.IsNullOrEmpty(rootPrefix))
            {
                rootPrefix = "ns";
            }
            else
            {
                nsManager.AddNamespace(rootPrefix, "http://schemas.datacontract.org/2004/07/Nintex.Forms.Responsive"); //namespace is in export file
            }


            var rowContainers = responsiveFormNode.SelectNodes(string.Format("{0}:Rows/{0}:RowContainer", rootPrefix), nsManager);
            foreach (XmlNode rowContainer in rowContainers)
            {
                var columns = rowContainer.SelectSingleNode(string.Format("{0}:Columns", rootPrefix), nsManager);

                var prefix = columns.GetPrefixOfNamespace("http://schemas.datacontract.org/2004/07/Nintex.Forms.FormControls");
                nsManager.AddNamespace(prefix, "http://schemas.datacontract.org/2004/07/Nintex.Forms.FormControls"); //namespace is in export file
                ProcessControls(contentTypeId, columns, nsManager, prefix, isPost);

                ReplaceRowContainers(rowContainer, nsManager, contentTypeId, rootPrefix, isPost);
            }
            ReplaceContentForMigrateOnline(document);

        }

        private void ReplaceContentForMigrateOnline(XmlDocument xmlDocument)
        {
            if (!this.mAveList.ParentWeb.Site.IsOnlineSite)
            {
                return;
            }
            XDocument document = XDocument.Parse(xmlDocument.OuterXml);

            var element = document.Descendants(XName.Get("ResponsiveForm", "http://schemas.datacontract.org/2004/07/Nintex.Forms")).FirstOrDefault();
            if (element.Attributes(XName.Get("d2p1", "http://www.w3.org/2000/xmlns/")).FirstOrDefault() == null)//== null说明是on-premise nintex form 需要做replace
            {
                foreach (var rowType in element.Descendants(XName.Get("RowType", "http://schemas.datacontract.org/2004/07/Nintex.Forms")))
                {
                    if (string.Equals(rowType.Value, "Control", StringComparison.OrdinalIgnoreCase))
                    {
                        rowType.Value = "Row";
                    }
                }
                element.Add(new XAttribute(XNamespace.Xmlns + "d2p1", "http://schemas.datacontract.org/2004/07/Nintex.Forms.Responsive"));
                foreach (var sub in element.Elements())
                {
                    ResetNodeName(sub);
                }
                xmlDocument.LoadXml(document.Declaration.ToString()+document.ToString());
            }
        }

        private void ResetNodeName(XElement element)
        {
            if (!string.Equals("http://schemas.datacontract.org/2004/07/Nintex.Forms", element.Name.NamespaceName))
            {
                return;
            }
            element.Name = XName.Get(element.Name.LocalName, "http://schemas.datacontract.org/2004/07/Nintex.Forms.Responsive");
            foreach (var sub in element.Elements())
            {
                ResetNodeName(sub);
            }
        }
        private void ReplaceRowContainers(XmlNode rowContainer, XmlNamespaceManager nsManager, string contentTypeId, string rootPrefix, bool isPost)
        {
            var subRowContainers = rowContainer.SelectNodes(string.Format("{0}:Rows/{0}:RowContainer", rootPrefix), nsManager);
            foreach (XmlNode subRowContainer in subRowContainers)
            {
                var columns = subRowContainer.SelectSingleNode(string.Format("{0}:Columns", rootPrefix), nsManager);
                var prefix = columns.GetPrefixOfNamespace("http://schemas.datacontract.org/2004/07/Nintex.Forms.FormControls");
                nsManager.AddNamespace(prefix, "http://schemas.datacontract.org/2004/07/Nintex.Forms.FormControls"); //namespace is in export file
                ProcessControls(contentTypeId, columns, nsManager, prefix, isPost);

                ReplaceRowContainers(subRowContainer, nsManager, contentTypeId, rootPrefix, isPost);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nintexFormXml"></param>
        /// <param name="contentTypeId"></param>
        protected virtual XmlDocument ReplaceControlContent(XmlDocument formXd, XmlNamespaceManager nsManager, string contentTypeId, bool isPost)
        {
            IAveFieldMapping currentListFieldMapping;
            mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.TryGetValueFromListFieldsMapping(mAveList.ID, out currentListFieldMapping);


            XmlNode formControls = formXd.SelectSingleNode("/ns:Form/ns:FormControls", nsManager);
            CacheControlIdAndName(contentTypeId, formControls, nsManager);
            ProcessControls(contentTypeId, formControls, nsManager, "d2p1", isPost);

            return formXd;
        }

        private void ProcessControls(string contentTypeId, XmlNode formControls, XmlNamespaceManager nsManager, string prefix, bool isPost)
        {
            foreach (XmlNode formControl in formControls.ChildNodes)
            {
                try
                {
                    Guid controlTypeUniqueId = new Guid(formControl.SelectSingleNode(string.Format("{0}:FormControlTypeUniqueId", prefix), nsManager).InnerText);
                    AveNintexFormControlType controlType;
                    if (!NintexFormControlTypeMapping.TryGetValue(controlTypeUniqueId, out controlType))
                    {
                        controlType = AveNintexFormControlType.None;
                    }

                    var processor = FormControlBase.CreateProcessor(controlType, mAveSPWeb, mAveList, contentTypeId, formControl, nsManager, prefix);
                    processor.ProcessControl(isPost);
                }
                catch (Exception)
                {
                    if (!needContinue)
                    {
                        throw;
                    }
                }
            }
        }


        private void CacheControlIdAndName(string contentTypeId, XmlNode formControls, XmlNamespaceManager nsManager)
        {
            if (mAveList == null && string.IsNullOrEmpty(contentTypeId))
            {
                return;
            }

            Dictionary<Guid, AveNintexFormControlType> uniqueIdMapping = new Dictionary<Guid, AveNintexFormControlType>();
            Dictionary<string, AveNintexFormControlType> displayNameMapping = new Dictionary<string, AveNintexFormControlType>();
            foreach (XmlNode formControl in formControls.ChildNodes)
            {
                Guid controlTypeUniqueId = new Guid(formControl.SelectSingleNode("d2p1:FormControlTypeUniqueId", nsManager).InnerText);

                AveNintexFormControlType controlType;
                if (!NintexFormControlTypeMapping.TryGetValue(controlTypeUniqueId, out controlType))
                {
                    controlType = AveNintexFormControlType.None;
                }

                Guid controlUniqueId = new Guid(formControl.SelectSingleNode("d2p1:UniqueId", nsManager).InnerText);
                uniqueIdMapping.Add(controlUniqueId, controlType);

                var displayNameNode = formControl.SelectSingleNode("d2p1:Name", nsManager);
                if (displayNameNode != null)
                {
                    var displayName = displayNameNode.InnerText;
                    displayNameMapping[displayName] = controlType;
                }
            }
            mAveSPWeb.AddNintexFormControlTypeMapping(mAveList.ID, contentTypeId, uniqueIdMapping, displayNameMapping);
        }


        /// <summary>
        /// Common namespace.
        /// </summary>
        /// <param name="xmlDocment"></param>
        /// <returns></returns>
        protected virtual XmlNamespaceManager GenerateXmlNamespaceManager(XmlDocument xmlDocment)
        {
            XmlNamespaceManager nsManager = new XmlNamespaceManager(xmlDocment.NameTable);
            nsManager.AddNamespace("ns", "http://schemas.datacontract.org/2004/07/Nintex.Forms"); //namespace is in export file
            nsManager.AddNamespace("d2p1", "http://schemas.datacontract.org/2004/07/Nintex.Forms.FormControls"); //namespace is in export file
            return nsManager;
        }

        protected virtual string UrlReplace(string sourceUrl)
        {
            var siteMappingManager = mAveSPWeb.ParentSite.MappingManager.SiteMappingManager;
            return AveReplaceProcessor.UrlReplace(sourceUrl, siteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), siteMappingManager.SourceSiteInfo, siteMappingManager.DestSiteInfo.ServerRelativeUrl);
        }

        protected void ReplaceLookupListId(XmlNode formControl, string contentTypeId, XmlNamespaceManager nsManager)
        {
            var lookupListIdString = formControl.SelectSingleNode("d2p1:LookupList", nsManager).InnerText;
            if (Validator.IsGuid(lookupListIdString)) //If source list is expression, the value of setting may be list id.
            {
                Guid lookupListId = new Guid(lookupListIdString);
                Guid value;
                if (mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.GetValueFromListIdMapping(lookupListId, out value))
                {
                    formControl.SelectSingleNode("d2p1:LookupList", nsManager).InnerText = value.ToString();
                }
                else
                {
                    throw new AveNintexFormListNotFoundException(lookupListId.ToString(), contentTypeId);
                }
            }
        }

    }
}
