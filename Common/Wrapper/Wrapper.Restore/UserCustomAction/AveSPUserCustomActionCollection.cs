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

        public void Restore(IList<AveUserCustomActionInfo> infos,AveRestoreMode aveRestoreMode)
        {
            if (userCustomActionCollection == null)
            {
                log.Warn("Cannot get custom action collection.");
                return;
            }
            log.Info("Begin to restore user custom action.Current Restore Mode:{0},Current cache count:{1}", aveRestoreMode, infos.Count());
            foreach (AveUserCustomActionInfo info in infos)
            {
                string ucaName = info?.Name?.Value;
                string ucaTitle = info?.Title?.Value;
                try
                {
                    IAveUserCustomAction ca = GetUserCustomAction(info);
                    if (ca != null && aveRestoreMode == AveRestoreMode.Default)
                    {
                        log.Info("Skip restoring the custom action {0}({1}) due to current conflict resolution {2}", ucaName, ucaTitle, aveRestoreMode);
                        //todo report
                        continue;
                    }
                    ArgumentNullException.ThrowIfNull(info);
                    if (ca == null)
                    {
                        log.Info($"Not found user custom action {ucaName}({ucaTitle}), create new one.");
                        ca = userCustomActionCollection.Add(info?.Location.Value);
                    }
                    ca.ClientSideComponentId = info.ClientSideComponentId.Value;
                    //todo need to replace ClientSideComponentProperties in further
                    ca.ClientSideComponentProperties = info.ClientSideComponentProperties.Value;
                    ca.CommandUIExtension = ReplaceCommandUIExtensionPro(info.CommandUIExtension.Value);
                    ca.Description = info.Description.Value;
                    ca.Group = info.Group.Value;
                    ca.ImageUrl = AveReplaceProcessor.UrlReplace(info.ImageUrl.Value, mappingManager.SiteManagedMappings, new ReplaceOption(true, true), mappingManager.SourceSiteInfo, mappingManager.DestSiteInfo.ServerRelativeUrl);
                    ca.Location = info.Location.Value;
                    ca.Name = info.Name.Value;
                    ca.Rights = info.Rights.Value;
                    ca.ScriptBlock = info.ScriptBlock.Value;
                    ca.ScriptSrc = AveReplaceProcessor.UrlReplace(info.ScriptSrc.Value, mappingManager.SiteManagedMappings, new ReplaceOption(true, true), mappingManager.SourceSiteInfo, mappingManager.DestSiteInfo.ServerRelativeUrl);
                    ca.Sequence = info.Sequence.Value;
                    ca.Title = info.Title.Value;
                    ca.Url = ReplaceUrlProperty(info.Url.Value);
                    ca.Update();
                    log.Info("Finish to restore the custom action_Name:{0}, Title:{1}, Url:{2}, ScriptSrc:{3} ,Current conflict resolution {4}", ucaName, ucaTitle, ca?.Url, ca?.ScriptSrc, aveRestoreMode);
                }
                catch (Exception e)
                {
                    log.Error("An error occurred when restore one user custom action:{0},{1} due to {2}", ucaName, ucaTitle, e);
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
        private string ReplaceUrlProperty(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return url;
            }
            string replacedUrl = AveReplaceProcessor.UrlReplace(url, mappingManager.SiteManagedMappings, new ReplaceOption(true, true), mappingManager.SourceSiteInfo, mappingManager.DestSiteInfo.ServerRelativeUrl);
            Regex reg = new Regex("[A-F0-9]{8}(-[A-F0-9]{4}){3}-[A-F0-9]{12}", RegexOptions.IgnoreCase);
            Match result = reg.Match(replacedUrl);
            if (!string.IsNullOrEmpty(result.Value))
            {
                Guid newId = Guid.Empty;
                //todo add workflow template id mapping
                if (mappingManager.TryGetWorkflowIdFromMapping(new Guid(result.Value), out newId))
                {
                    replacedUrl = reg.Replace(replacedUrl, newId.ToString());
                }
            }
            return replacedUrl;
        }

        private IAveUserCustomAction GetUserCustomAction(AveUserCustomActionInfo info)
        {
            // Name默认是id的value，keep到目的端之后可以作为冲突判断的依据。
            if(userCustomActionCollection == null)
            {
                return null;
            }
            return userCustomActionCollection.FirstOrDefault(ca => ca.Name.Equals(info.Name.Value, StringComparison.OrdinalIgnoreCase));
        }
    }

    public class AveSPSiteUserCustomActionCollection : AveSPUserCustomActionCollection
    {
        private IAveSite site;
        public AveSPSiteUserCustomActionCollection(AveSPSite restoreSite)
            :this(restoreSite.SPSite,restoreSite.MappingManager.SiteMappingManager)
        {
        }
        public AveSPSiteUserCustomActionCollection(IAveSite parent,AveSiteMappingManager mapping)
        {
            site = parent;
            mappingManager = mapping;
            userCustomActionCollection = site.UserCustomActions;
        }
    }

    public class AveSPWebUserCustomActionCollection : AveSPUserCustomActionCollection
    {
        private IAveWeb web;
        public AveSPWebUserCustomActionCollection(AveSPWeb restoreWeb)
             : this(restoreWeb.SPWeb, restoreWeb.ParentSite.MappingManager.SiteMappingManager)
        {
        }
        public AveSPWebUserCustomActionCollection(IAveWeb parent, AveSiteMappingManager mapping)
        {
            web = parent;
            mappingManager = mapping;
            userCustomActionCollection = web.UserCustomActions;
        }
    }

    public class AveSPListUserCustomActionCollection : AveSPUserCustomActionCollection
    {
        private IAveList list;
        public AveSPListUserCustomActionCollection(AveSPList restoreList)
              : this(restoreList.SPList, restoreList.ParentSite.MappingManager.SiteMappingManager)
        {
        }
        public AveSPListUserCustomActionCollection(IAveList parent, AveSiteMappingManager mapping)
        {
            list = parent;
            mappingManager = mapping;
            userCustomActionCollection = list.UserCustomActions;
        }
    }
}
