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
using System.Reflection;
using System.Text;
using System.Xml;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Restore.NintexForm
{
    internal class NintexFormContentProcessor
    {

        private IAveList mAveList;
        private AveSPWeb mAveSPWeb;


        public NintexFormContentProcessor(AveSPWeb web, IAveList list)
        {
            this.mAveSPWeb = web;
            this.mAveList = list;
        }


        /// <summary>
        /// Common namespace.
        /// </summary>
        /// <param name="xmlDocment"></param>
        /// <returns></returns>
        private XmlNamespaceManager GenerateXmlNamespaceManager(XmlDocument xmlDocment)
        {
            XmlNamespaceManager nsManager = new XmlNamespaceManager(xmlDocment.NameTable);
            nsManager.AddNamespace("ns", "http://schemas.datacontract.org/2004/07/Nintex.Forms"); //namespace is in export file
            nsManager.AddNamespace("d2p1", "http://schemas.datacontract.org/2004/07/Nintex.Forms.FormControls"); //namespace is in export file
            return nsManager;
        }


        private XmlDocument GenerateXmlDocument(string nintexFormXml, out XmlNamespaceManager nsManager)
        {
            XmlDocument xd = new XmlDocument();
            xd.LoadXml(nintexFormXml);
            nsManager = GenerateXmlNamespaceManager(xd);
            return xd;
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
        private string UrlReplace(string sourceUrl)
        {
            var siteMappingManager = mAveSPWeb.ParentSite.MappingManager.SiteMappingManager;
            return AveReplaceProcessor.UrlReplace(sourceUrl, siteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), siteMappingManager.SourceSiteInfo, siteMappingManager.DestSiteInfo.ServerRelativeUrl);
        }

        private void ReplaceNodeUrl(XmlNode rootNode, XmlNamespaceManager nsManager, string xPath)
        {
            var node = rootNode.SelectSingleNode(xPath, nsManager);
            if (node != null && !string.IsNullOrEmpty(node.InnerText))
            {
                node.InnerText = UrlReplace(node.InnerText);
            }
        }
        private void ReplaceIconNodeUrl(XmlNode rootNode, XmlNamespaceManager nsManager, bool isPost)
        {
            string xPath = "/ns:Form/ns:Icon";
            ReplaceNodeUrl(rootNode, nsManager, xPath);
        }

        public string ReplaceFormContent(string formXml, string contentTypeId, bool isPost)
        {
            XmlNamespaceManager nsManager;
            var formXd = GenerateXmlDocument(formXml, out nsManager);
            ReplaceControlContent(formXd, nsManager, contentTypeId, isPost);
            ReplaceFormLayoutContent(formXd, nsManager);
            ReplaceIconNodeUrl(formXd.DocumentElement, nsManager, isPost);
            return formXd.InnerXml;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="nintexFormXml"></param>
        /// <param name="contentTypeId"></param>
        private XmlDocument ReplaceControlContent(XmlDocument formXd, XmlNamespaceManager nsManager, string contentTypeId, bool isPost)
        {
            IAveFieldMapping currentListFieldMapping;
            mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.TryGetValueFromListFieldsMapping(mAveList.ID, out currentListFieldMapping);


            XmlNode formControls = formXd.SelectSingleNode("/ns:Form/ns:FormControls", nsManager);
            CacheControlIdAndName(contentTypeId, formControls, nsManager);
            ProcessControls(contentTypeId, formControls, nsManager, isPost);

            return formXd;
        }

        private void ProcessControls(string contentTypeId, XmlNode formControls, XmlNamespaceManager nsManager, bool isPost)
        {
            foreach (XmlNode formControl in formControls.ChildNodes)
            {
                Guid controlTypeUniqueId = new Guid(formControl.SelectSingleNode("d2p1:FormControlTypeUniqueId", nsManager).InnerText);
                AveNintexFormControlType controlType;
                if (!NintexFormControlTypeMapping.NintexFormControlType.TryGetValue(controlTypeUniqueId, out controlType))
                {
                    controlType = AveNintexFormControlType.None;
                }

                var processor = FormControlBase.CreateProcessor(controlType, mAveSPWeb, mAveList, contentTypeId, formControl, nsManager);
                processor.ProcessControl(isPost);
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
                if (!NintexFormControlTypeMapping.NintexFormControlType.TryGetValue(controlTypeUniqueId, out controlType))
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
            mAveSPWeb.AddNintexFormControlTypeMapping((Guid?)(mAveList?.ID) ?? Guid.Empty, contentTypeId, uniqueIdMapping, displayNameMapping);
        }

    }
}
