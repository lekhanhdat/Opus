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
using AvePoint.GCommon;
using AvePoint.GCommon.Utility;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace AvePoint.Wrapper.Restore
{
    public abstract class AveNintexForm
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        protected IAveList mAveList;
        protected AveSPWeb mAveSPWeb;

        public AveNintexForm(IAveList aveList, AveSPWeb aveSPWeb)
        {
            mAveList = aveList;
            mAveSPWeb = aveSPWeb;
        }
        public static AveNintexForm CreateNintexForm(IAveList aveList, AveSPWeb aveSPWeb)
        {
            switch(aveSPWeb.ParentSite.SPContextKind)
            {
                case AveContextKind.Server10ObjectModel:
                case AveContextKind.Server13ObjectModel:
                case AveContextKind.Server16ObjectModel:
                case AveContextKind.ServerObjectModel:
                    return new AveNintexFormLocal(aveList, aveSPWeb);
                case AveContextKind.ClientObjectModel:
                    if(aveSPWeb.ParentSite.SPSite.IsOnlineSite)
                    {
                        return new AveNintexFormOnline(aveList, aveSPWeb);
                    }
                    else
                    {
                        return new AveNintexFormLocal(aveList, aveSPWeb);
                    }
                default:
                    return null;
            }
        }

        public void RestoreForm(string nintexFormXml, string contentTypeId)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("AveNintexForm.RestoreForm"))
            {
                GenerateNintexFormValueProcessors(nintexFormXml, contentTypeId);
                string newNintexFormXml = RemoverUnsupportedFormControl(nintexFormXml);
                newNintexFormXml = ReplaceNintexFormContent(newNintexFormXml, contentTypeId);
                PublishNintexForm(newNintexFormXml, contentTypeId);
            }
        }
        public abstract string RemoverUnsupportedFormControl(string nintexFormXml);
        public abstract void PublishNintexForm(string newNintexFormXml, string contentTypeId);
        private void GenerateNintexFormValueProcessors(string nintexFormXml, string contentTypeId)
        {
            Dictionary<Guid, AveNintexFormControlType> uniqueIdMapping = new Dictionary<Guid, AveNintexFormControlType>();
            Dictionary<string, AveNintexFormControlType> displayNameMapping = new Dictionary<string, AveNintexFormControlType>();
            XmlDocument xd = new XmlDocument();
            xd.LoadXml(nintexFormXml);
            XmlNamespaceManager nsManager = GenerateXmlNamespaceManager(xd);
            XmlNode formControls = xd.SelectSingleNode("/ns:Form/ns:FormControls", nsManager);
            foreach (XmlNode formControl in formControls.ChildNodes)
            {
                var controlTypeUniqueId = new Guid(formControl.SelectSingleNode("d2p1:FormControlTypeUniqueId", nsManager).InnerText);
                var controlUniqueId = new Guid(formControl.SelectSingleNode("d2p1:UniqueId", nsManager).InnerText);
                var displayName = formControl.SelectSingleNode("d2p1:DisplayName", nsManager).InnerText;
                var controlType = AveNintexFormUtility.GetNintexFormControlTypeByControlTypeId(controlTypeUniqueId);
                uniqueIdMapping.Add(controlUniqueId, controlType);
                displayNameMapping[displayName] = controlType;
            }

            if (!mAveSPWeb.NintexFormControlTypeCache.ContainsKey(mAveList.ID))
            {
                mAveSPWeb.NintexFormControlTypeCache[mAveList.ID] = new Dictionary<string, Tuple<Dictionary<Guid, AveNintexFormControlType>, Dictionary<string, AveNintexFormControlType>>>();
            }
            mAveSPWeb.NintexFormControlTypeCache[mAveList.ID][contentTypeId.ToLower()] = new Tuple<Dictionary<Guid, AveNintexFormControlType>, Dictionary<string, AveNintexFormControlType>>(uniqueIdMapping, displayNameMapping);
        }

        private XmlNamespaceManager GenerateXmlNamespaceManager(XmlDocument xmlDocment)
        {
            XmlNamespaceManager nsManager = new XmlNamespaceManager(xmlDocment.NameTable);
            nsManager.AddNamespace("ns", "http://schemas.datacontract.org/2004/07/Nintex.Forms"); //namespace is in export file
            nsManager.AddNamespace("d2p1", "http://schemas.datacontract.org/2004/07/Nintex.Forms.FormControls"); //namespace is in export file
            return nsManager;
        }

        public string ReplaceNintexFormContent(string nintexFormXml, string contentTypeId)
        {
            IAveFieldMapping currentListFieldMapping;
            mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.TryGetValueFromListFieldsMapping(mAveList.ID, out currentListFieldMapping);

            XmlDocument xd = new XmlDocument();
            xd.LoadXml(nintexFormXml);
            XmlNamespaceManager nsManager = GenerateXmlNamespaceManager(xd);
            XmlNode formControls = xd.SelectSingleNode("/ns:Form/ns:FormControls", nsManager);

            List<Guid> notFoundlookupListIdCol = new List<Guid>();
            foreach (XmlNode formControl in formControls.ChildNodes)
            {
                #region replace lookup list id
                if (formControl.Attributes["i:type"].Value.Equals("d3p1:SharePointLookupFormControlProperties", StringComparison.OrdinalIgnoreCase)
                    && formControl.SelectSingleNode("d2p1:LookupList", nsManager) != null
                    && formControl.SelectSingleNode("d2p1:LookupList", nsManager).InnerText != string.Empty)
                {
                    ReplaceLookupListId(formControl, contentTypeId, nsManager);
                    formControl.SelectSingleNode("d2p1:LookupWeb", nsManager).InnerText = mAveSPWeb.ServerRelativeUrl;
                }
                #endregion

                #region replace column info
                if (formControl.HasChildNodes
                    && formControl.SelectSingleNode("d2p1:DataField", nsManager) != null && !string.IsNullOrEmpty(formControl.SelectSingleNode("d2p1:DataField", nsManager).InnerText)
                    && currentListFieldMapping != null)
                {
                    #region replace field internal name and field display name
                    string oldDataField = formControl.SelectSingleNode("d2p1:DataField", nsManager).InnerText;
                    if (oldDataField.Split(':')[0].Equals("List", StringComparison.OrdinalIgnoreCase)) //If oldDataField starts with "List", the relevant field links current list
                    {
                        string newDataField = currentListFieldMapping.GetMappingRestoredFieldInternalName(oldDataField.Split(':')[1]);
                        if (newDataField == null)
                        {
                            log.Debug("Can not found field:{0} in handle the content of Nintex form.", oldDataField);
                        }
                        formControl.SelectSingleNode("d2p1:DataField", nsManager).InnerText = string.IsNullOrEmpty(newDataField) ? oldDataField : string.Format("List:{0}", newDataField);
                        if (newDataField != null
                            && !string.Equals(newDataField, oldDataField.Split(':')[1], StringComparison.OrdinalIgnoreCase))
                        {
                            string oldDataFieldDisplayName = formControl.SelectSingleNode("d2p1:DataFieldDisplayName", nsManager).InnerText;
                            string newDataFieldDisplayName = string.IsNullOrEmpty(currentListFieldMapping.GetMappingRestoredFieldDisplayName(oldDataFieldDisplayName)) ? oldDataFieldDisplayName : currentListFieldMapping.GetMappingRestoredFieldDisplayName(oldDataFieldDisplayName);
                            formControl.SelectSingleNode("d2p1:DataFieldDisplayName", nsManager).InnerText = newDataFieldDisplayName;
                        }
                    }
                    #endregion
                }
                #endregion

                #region handle attachment control
                if (formControl.SelectSingleNode("d2p1:FormControlTypeUniqueId", nsManager).InnerText.Equals("5f8b447a-4195-485b-9a04-477d7f24be73", StringComparison.OrdinalIgnoreCase))
                {
                    XmlNode blockedFileExtenstions = formControl.SelectSingleNode("d2p1:BlockedFileExtenstions", nsManager);
                    if (blockedFileExtenstions != null && blockedFileExtenstions.ChildNodes.Count == 0)
                    {
                        int needRemovedAttributeIndex = -1;
                        for (int i = 0; i < blockedFileExtenstions.Attributes.Count; i++)
                        {
                            if (blockedFileExtenstions.Attributes[i].Name.Equals("i:nil", StringComparison.OrdinalIgnoreCase))
                            {
                                needRemovedAttributeIndex = i;
                                break;
                            }
                        }
                        if (needRemovedAttributeIndex != -1)
                        {
                            blockedFileExtenstions.Attributes.RemoveAt(needRemovedAttributeIndex);
                        }
                        blockedFileExtenstions.InnerXml = "<d4p1:string>ashx</d4p1:string><d4p1:string>asmx</d4p1:string><d4p1:string>asp</d4p1:string><d4p1:string>aspq</d4p1:string><d4p1:string>axd</d4p1:string><d4p1:string>cshtm</d4p1:string><d4p1:string>dll</d4p1:string><d4p1:string>exe</d4p1:string><d4p1:string>json</d4p1:string><d4p1:string>rem</d4p1:string><d4p1:string>shtm</d4p1:string><d4p1:string>shtml</d4p1:string><d4p1:string>soap</d4p1:string><d4p1:string>stm</d4p1:string><d4p1:string>svc</d4p1:string><d4p1:string>vbhtm</d4p1:string><d4p1:string>vbhtml</d4p1:string><d4p1:string>xamlx</d4p1:string>"; //For POC-14749
                    }
                }
                #endregion
            }
            return AveNintexFormUtility.ReplaceContent(xd, mAveList, UrlReplace);
        }

        private string UrlReplace(string sourceUrl)
        {
            var siteMappingManager = mAveSPWeb.ParentSite.MappingManager.SiteMappingManager;
            return AveReplaceProcessor.UrlReplace(sourceUrl, siteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), siteMappingManager.SourceSiteInfo, siteMappingManager.DestSiteInfo.ServerRelativeUrl);
        }

        private void ReplaceLookupListId(XmlNode formControl, string contentTypeId, XmlNamespaceManager nsManager)
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
                    throw new AveNintexFormListNotFoundException(string.Format("Lookup List {0} did not have been restored, put nintex form of content type {1} in post action", lookupListId.ToString(), contentTypeId));
                }
            }
        }
    }

    public class AveNintexFormLocal : AveNintexForm
    {
        public AveNintexFormLocal(IAveList aveList, AveSPWeb aveSPWeb)
            : base(aveList, aveSPWeb)
        {

        }
        public override string RemoverUnsupportedFormControl(string nintexFormXml)
        {
            return nintexFormXml;
        }
        public override void PublishNintexForm(string newNintexFormXml, string contentTypeId)
        {
            using (var nfContext = new NfClientContext(mAveSPWeb.SPWeb.Url, null, mAveSPWeb.ParentSite.ObjectModelFactory.ContextKind))
            {
                nfContext.PublishForm(mAveList.ID.ToString("B"), contentTypeId, newNintexFormXml);
            }
        }
    }
    public class AveNintexFormOnline : AveNintexForm
    {
        public AveNintexFormOnline(IAveList aveList, AveSPWeb aveSPWeb)
            : base(aveList, aveSPWeb)
        {

        }
        public override string RemoverUnsupportedFormControl(string nintexFormXml)
        {
            return AveNintexFormUtility.RemoverUnsupportedFormControl(nintexFormXml, true);
        }
        public override void PublishNintexForm(string newNintexFormXml, string contentTypeId)
        {
            mAveList.SaveNintexForm(newNintexFormXml, contentTypeId);
            mAveList.PublishNintexForm(contentTypeId);
        }
    }
    public class AveNintexFormListNotFoundException : AveWrapperBaseException
    {
        public AveNintexFormListNotFoundException(string message)
            : base(message)
        { }

    }
}
