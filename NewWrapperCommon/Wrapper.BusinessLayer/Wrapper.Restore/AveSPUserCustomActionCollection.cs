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

using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace AvePoint.Wrapper.Restore
{
    public abstract class AveSPUserCustomActionCollection
    {
        protected static AveLogger log = AveLogger.GetInstance(typeof(AveSPUserCustomActionCollection));
        protected AveSiteMappingManager mappingManager;
        protected IAveUserCustomActionCollection userCustomActionCollection;
        public void Restore(List<AveUserCustomActionInfo> infos)
        {
            foreach(AveUserCustomActionInfo info in infos)
            {
                try
                {
                    if (userCustomActionCollection == null)
                    {
                        log.Warn("Cannot get custom action collection.");
                        break;
                    }
                    IAveUserCustomAction ca = GetUserCustomAction(info);
                    if (ca == null)
                    {
                        ca = userCustomActionCollection.Add();
                    }
                    ca.CommandUIExtension = ReplaceCommandUIExtensionPro(info.CommandUIExtension);
                    ca.Description = info.Description;
                    ca.Group = info.Group;
                    if (!string.IsNullOrEmpty(info.ImageUrl))
                    {
                        ca.ImageUrl = AveReplaceProcessor.UrlReplace(info.ImageUrl, mappingManager.SiteManagedMappings, new ReplaceOption(true, true), mappingManager.SourceSiteInfo, mappingManager.DestSiteInfo.ServerRelativeUrl);
                    }
                    else
                    {
                        log.Debug("ImageUrl is null.");
                    }
                    ca.Location = info.Location;
                    ca.Name = info.Name;
                    ca.Rights = info.Rights;
                    ca.ScriptBlock = info.ScriptBlock;
                    if (!string.IsNullOrEmpty(info.ScriptSrc))
                    {
                        ca.ScriptSrc = AveReplaceProcessor.UrlReplace(info.ScriptSrc, mappingManager.SiteManagedMappings, new ReplaceOption(true, true), mappingManager.SourceSiteInfo, mappingManager.DestSiteInfo.ServerRelativeUrl);
                    }
                    else
                    {
                        log.Debug("ScriptSrc is null.");
                    }
                    ca.Sequence = info.Sequence;
                    ca.Title = info.Title;
                    var needReplaceId = false;
                    if (!string.IsNullOrEmpty(info.Url))
                    {
                        ca.Url = ReplaceUrlProperty(info.Url, ca.Id, info.Id, ref needReplaceId);
                        if (needReplaceId)
                        {
                            ReplaceCustomActionIDNative(ca, info.Id);
                        }
                    }
                    else
                    {
                        log.Debug("Url is null.");
                    }
                    ca.Update();
                }
                catch(Exception e)
                {
                    log.Error("Failed to resotre this custom action: {0}, Error: {1}", info.Name, e);
                }
            }
        }

        private string ReplaceCommandUIExtensionPro(string commandUIExtension)
        {
            if(string.IsNullOrEmpty(commandUIExtension))
            {
                return commandUIExtension;
            }
            List<string> attributeNames = new List<string>() { "CommandAction", "Image16by16", "Image32by32" };
            var doc = new XmlDocument();
            doc.LoadXml(commandUIExtension);
            return ReplaceCommandUIExtensionProByAttribute(doc, attributeNames);
        }

        private string ReplaceCommandUIExtensionProByAttribute(XmlDocument doc, List<string> attributeNames)
        {
            string result = doc.OuterXml;
            foreach (string attName in attributeNames)
            {
                string xPath = string.Format("//@{0}", attName);
                XmlNode att = doc.SelectSingleNode(xPath);
                if(att != null)
                {
                    string oldValue = att.Value;
                    string newValue = AveReplaceProcessor.UrlReplace(oldValue, mappingManager.SiteManagedMappings, new ReplaceOption(true, true), mappingManager.SourceSiteInfo, mappingManager.DestSiteInfo.ServerRelativeUrl);
                    if(!string.Equals(oldValue, newValue, StringComparison.OrdinalIgnoreCase))
                    {
                        result = result.Replace(oldValue, newValue);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// url example: ~site/_layouts/wfstart.aspx?List={ListId}&ID={ItemId}&TemplateID=3a612145-1e16-4c35-810e-5e5f66f68168&AssociationName=wf
        /// 1. need replace url
        /// 2. need replace workflow id
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string ReplaceUrlProperty(string url, Guid destACID, Guid souceACID ,ref bool needReplaceId)
        {
            log.Debug("customaction  url:{0}   destACID:{1}     souceACID{2}", url, destACID, souceACID);
            string replacedUrl = AveReplaceProcessor.UrlReplace(url, mappingManager.SiteManagedMappings, new ReplaceOption(true, true), mappingManager.SourceSiteInfo, mappingManager.DestSiteInfo.ServerRelativeUrl);
            Regex reg = new Regex("[A-F0-9]{8}(-[A-F0-9]{4}){3}-[A-F0-9]{12}", RegexOptions.IgnoreCase);
            Match result = reg.Match(replacedUrl);
            if (!string.IsNullOrEmpty(result.Value))
            {
                var newId = Guid.Empty;
                var oldId = new Guid(result.Value);
                if (mappingManager.TryGetValueFromWorkflowIdMapping(oldId, out newId))
                {
                    if (oldId != newId)
                    {
                        replacedUrl = reg.Replace(replacedUrl, newId.ToString());
                        needReplaceId = true;
                        log.Debug("restore custom action replaceurl and use the source templateid :{0} find mapping destination templateid is:{1} and after replace the url is:{2}", result.Value, newId, replacedUrl);
                    }
                    else
                    {
                        log.Debug("Add mapping");
                        AddMapping(destACID, souceACID);
                    }
                }
                else
                {
                    log.Debug("connot find in mapping");
                }
            }
            return replacedUrl;
        }
        
        protected virtual void ReplaceCustomActionIDNative(IAveUserCustomAction action, Guid newId)
        {
            
        }

        protected virtual void AddMapping(Guid destACID, Guid souceACID)
        {
            
        }

        private IAveUserCustomAction GetUserCustomAction(AveUserCustomActionInfo info)
        {
            // Name默认是id的value，keep到目的端之后可以作为冲突判断的依据。
            if(userCustomActionCollection == null)
            {
                return null;
            }
            return userCustomActionCollection.FirstOrDefault(ca => ca.Name.Equals(info.Name, StringComparison.OrdinalIgnoreCase));
        }
    }

    public class AveSPSiteUserCustomActionCollection : AveSPUserCustomActionCollection
    {
        private IAveSite site;
        public AveSPSiteUserCustomActionCollection(IAveSPSite restoreSite)
        {
            site = restoreSite.SPSite;
            mappingManager = restoreSite.MappingManager.SiteMappingManager;
        }
    }

    public class AveSPWebUserCustomActionCollection : AveSPUserCustomActionCollection
    {
        private IAveWeb web;
        public AveSPWebUserCustomActionCollection(IAveSPWeb restoreWeb)
        {
            web = restoreWeb.SPWeb;
            mappingManager = restoreWeb.ParentSite.MappingManager.SiteMappingManager;
        }
    }

    public class AveSPListUserCustomActionCollection : AveSPUserCustomActionCollection
    {
        private IAveList list;
        private IAveBackupRestoreQueryService query;
        public AveSPListUserCustomActionCollection(IAveSPList restoreList)
        {
            list = restoreList.SPList;
            query = restoreList.ParentSite.QueryService;
            mappingManager = restoreList.ParentSite.MappingManager.SiteMappingManager;
            userCustomActionCollection = list.UserCustomActions;
        }

        protected override void AddMapping(Guid destACID,Guid sourceACID)
        {
            mappingManager.AddCustomActionIdMapping(this.list.ID, destACID, sourceACID);
        }

        protected override void ReplaceCustomActionIDNative(IAveUserCustomAction action, Guid sourceId)
        {
            if (query != null && sourceId != Guid.Empty)
            {
                query.ReplaceCustomActionId(this.list.ParentWeb.Site.ID, this.list.ParentWeb.ID, action.RegistrationId, action.Id, sourceId);
            }
        }
    }
}
