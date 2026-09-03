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
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Restore.NintexForm
{
    abstract class FormControlBase
    {
        protected static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private const string OnlineFormControlAssemblyPrefix = "AvePoint.Wrapper.Restore.NintexForm.Online";


        protected AveSPWeb mWeb;
        protected IAveList mList;
        protected XmlNode mControlNode;
        protected XmlNamespaceManager nsManager;
        protected string contentTypeId;

        public static FormControlBase CreateProcessor(AveNintexFormControlType type, AveSPWeb web, IAveList list, string contentTypeId, XmlNode controlNode, XmlNamespaceManager nsManager)
        {
            FormControlBase processor = null;
            string typeNamePrefix = OnlineFormControlAssemblyPrefix;
            string typename = string.Format("{0}.{1}Control", typeNamePrefix, type);
            processor = GetFormControlProcessorByTypeName(web, list, contentTypeId, controlNode, nsManager, typename);
            if (processor == null)
            {
                string defaultTypeName = string.Format("{0}.BaseControl", typeNamePrefix);
                processor = GetFormControlProcessorByTypeName(web, list, contentTypeId, controlNode, nsManager, defaultTypeName);
            }
            return processor;
        }

        //如果是目的端是online站点的话，如果不替换成绝对url，会指向app web 里
        protected virtual bool InternalUrlReplaced(string sourceUrl, out string newUrl, bool isPostAction)
        {
            var tempSourceUrl = System.Web.HttpUtility.UrlDecode(sourceUrl);
            newUrl = sourceUrl;
            //{data:data}这种不作为url 进行替换
            if (string.IsNullOrEmpty(tempSourceUrl) || (tempSourceUrl.StartsWith("{") && tempSourceUrl.EndsWith("}")))
            {
                return true;
            }
            var siteMappingManager = mWeb.ParentSite.MappingManager.SiteMappingManager;
            newUrl = AveReplaceProcessor.UrlReplace(tempSourceUrl, siteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), siteMappingManager.SourceSiteInfo, siteMappingManager.DestSiteInfo.ServerRelativeUrl);
            if (!isPostAction && string.Equals(newUrl, tempSourceUrl, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return true;
        }



        private static FormControlBase GetFormControlProcessorByTypeName(AveSPWeb web, IAveList list, string contentTypeId,
            XmlNode controlNode, XmlNamespaceManager nsManager, string typename)
        {
            FormControlBase processor = null;
            var type = Type.GetType(typename, false, true);
            if (type != null)
            {
                var constructorInfo =
                    type.GetConstructor(new Type[]
                    {typeof (AveSPWeb), typeof (IAveList), typeof (string), typeof (XmlNode), typeof (XmlNamespaceManager)});
                if (constructorInfo != null)
                {
                    processor =
                        constructorInfo.Invoke(new object[] { web, list, contentTypeId, controlNode, nsManager }) as
                            FormControlBase;
                }
            }
            return processor;
        }


        protected FormControlBase(AveSPWeb web, IAveList list, string contentTypeId, XmlNode controlNode, XmlNamespaceManager nsManager)
        {
            mWeb = web;
            mList = list;
            mControlNode = controlNode;
            this.nsManager = nsManager;
            this.contentTypeId = contentTypeId;
            AddControlNameSpace();
        }
        public virtual void ProcessControl(bool isPost)
        {

            #region replace lookup list id
            if (mControlNode.Attributes["i:type"].Value.Equals("d3p1:SharePointLookupFormControlProperties", StringComparison.OrdinalIgnoreCase)
                && mControlNode.SelectSingleNode("d2p1:LookupList", nsManager) != null
                && mControlNode.SelectSingleNode("d2p1:LookupList", nsManager).InnerText != string.Empty)
            {
                ReplaceLookupListId(mControlNode, contentTypeId, nsManager);
                var oldUrl = mControlNode.SelectSingleNode("d2p1:LookupWeb", nsManager).InnerText;
                var newUrl = AveReplaceProcessor.UrlReplace(oldUrl, mWeb.ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), mWeb.ParentSite.MappingManager.SiteMappingManager.SourceSiteInfo, mWeb.ParentSite.MappingManager.SiteMappingManager.DestSiteInfo.ServerRelativeUrl);
                mControlNode.SelectSingleNode("d2p1:LookupWeb", nsManager).InnerText = newUrl;
            }
            #endregion

            #region replace column info
            IAveFieldMapping currentListFieldMapping = null;
            if (mList != null)
            {
                mWeb.ParentSite.MappingManager.SiteMappingManager.TryGetValueFromListFieldsMapping(mList.ID, out currentListFieldMapping);
            }
            if (mControlNode.HasChildNodes
                && mControlNode.SelectSingleNode("d2p1:DataField", nsManager) != null && !string.IsNullOrEmpty(mControlNode.SelectSingleNode("d2p1:DataField", nsManager).InnerText)
                && currentListFieldMapping != null)
            {
                #region replace field internal name and field display name
                string oldDataField = mControlNode.SelectSingleNode("d2p1:DataField", nsManager).InnerText;
                if (oldDataField.Split(':')[0].Equals("List", StringComparison.OrdinalIgnoreCase)) //If oldDataField starts with "List", the relevant field links current list
                {
                    string newDataField = currentListFieldMapping.GetMappingRestoredFieldInternalName(oldDataField.Split(':')[1]);
                    if (newDataField == null)
                    {
                        log.Debug("Can not found field:{0} in handle the content of Nintex form.", oldDataField);
                    }
                    mControlNode.SelectSingleNode("d2p1:DataField", nsManager).InnerText = string.IsNullOrEmpty(newDataField) ? oldDataField : string.Format("List:{0}", newDataField);
                    if (newDataField != null
                        && !string.Equals(newDataField, oldDataField.Split(':')[1], StringComparison.OrdinalIgnoreCase))
                    {
                        string oldDataFieldDisplayName = mControlNode.SelectSingleNode("d2p1:DataFieldDisplayName", nsManager).InnerText;
                        string newDataFieldDisplayName = string.IsNullOrEmpty(currentListFieldMapping.GetMappingRestoredFieldDisplayName(oldDataFieldDisplayName)) ? oldDataFieldDisplayName : currentListFieldMapping.GetMappingRestoredFieldDisplayName(oldDataFieldDisplayName);
                        mControlNode.SelectSingleNode("d2p1:DataFieldDisplayName", nsManager).InnerText = newDataFieldDisplayName;
                    }
                }
                #endregion
            }
            #endregion

            #region handle attachment control
            if (mControlNode.SelectSingleNode("d2p1:FormControlTypeUniqueId", nsManager).InnerText.Equals("5f8b447a-4195-485b-9a04-477d7f24be73", StringComparison.OrdinalIgnoreCase))
            {
                XmlNode blockedFileExtenstions = mControlNode.SelectSingleNode("d2p1:BlockedFileExtenstions", nsManager);
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

        public abstract void AddControlNameSpace();

        private void ReplaceLookupListId(XmlNode formControl, string contentTypeId, XmlNamespaceManager nsManager)
        {
            var lookupListIdString = formControl.SelectSingleNode("d2p1:LookupList", nsManager).InnerText;
            if (Validator.IsGuid(lookupListIdString)) //If source list is expression, the value of setting may be list id.
            {
                Guid lookupListId = new Guid(lookupListIdString);
                Guid value;
                if (mWeb.ParentSite.MappingManager.SiteMappingManager.GetValueFromListIdMapping(lookupListId, out value))
                {
                    formControl.SelectSingleNode("d2p1:LookupList", nsManager).InnerText = value.ToString();
                }
                else
                {
                    throw new AveNintexFormListNotFoundException(lookupListId.ToString(), contentTypeId);
                }
            }
            else
            {
                string listTitle;
                if (mWeb.ParentSite.MappingManager.SiteMappingManager.GetValueFromListTitleMappnig(this.mWeb.SPWeb.ID, lookupListIdString, out listTitle))
                {
                    formControl.SelectSingleNode("d2p1:LookupList", nsManager).InnerText = listTitle;
                }

            }
        }
    }


}
