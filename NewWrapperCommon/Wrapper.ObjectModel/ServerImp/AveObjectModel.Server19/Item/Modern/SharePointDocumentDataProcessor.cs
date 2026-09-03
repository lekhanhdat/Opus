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
namespace AvePoint.ObjectModel.Server19
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AvePoint.Wrapper.Common;
    using Microsoft.SharePoint;

    /// <summary>
    /// WebPart也需要遵从这个Solution，慢慢得把post action合理化。
    /// </summary>
    class SharePointDocumentDataProcessor : SharePointDataProcessor
    {
        private static Dictionary<Guid, Type> processorMapping = new Dictionary<Guid, Type>()
        {
            { new ClientSideWebPartProcessor().Id, typeof(ClientSideWebPartProcessor) },
        };

        private readonly IAveSite site;
        private readonly IAveFile document;
        private readonly AveSiteMappingManager mapping;
        private readonly AveSiteInfo sourceSitInfo;

        public SharePointDocumentDataProcessor(IAveFile document, AveSiteMappingManager mapping, AveSiteInfo SourceSiteInfo)
        {
            this.document = document;
            this.mapping = mapping;
            this.sourceSitInfo = SourceSiteInfo;
        }

        public SharePointDocumentDataProcessor(IAveSite site, AveSiteMappingManager mapping, AveSiteInfo SourceSiteInfo)
        {
            this.site = site;
            this.mapping = mapping;
            this.sourceSitInfo = SourceSiteInfo;
        }

        /// <summary>
        /// true  => completed
        /// false => require post action
        /// </summary>
        /// <param name="userData"></param>
        /// <returns></returns>
        public override bool ProcessUserData(Dictionary<string, object> userData)
        {
            bool needPostAction = false;

            var processorList = new List<IUserDataProcessor>() {
                new ClientSideWebPartProcessor(document,this.mapping,this.sourceSitInfo)
            };

            foreach (var processor in processorList)
            {
                if (!processor.Process(userData))
                {
                    postActions.Add(processor);

                    needPostAction = true;
                }
            }

            return !needPostAction;
        }

        public override void RecordPostActions()
        {
            if (postActions.Count > 0)
            {
                this.mapping.AddDocumentPostActions(
                    this.document.Web.Site.ID,
                    document.Web.ID,
                    document.ParentFolder.ParentListId,
                    document.UniqueId,
                    postActions.Select(a => a.GeneratePostActionContract())
                    );
            }
        }

        public override void PostActionImpl()
        {
            var mapping = this.mapping.DocumentPostActions;

            if (mapping != null && mapping.Count > 0)
            {
                try
                {
                    foreach (var siteInfo in mapping)
                    {
                        if (siteInfo.Key != this.site.ID)
                        {
                            logger.Warn("Cannot post action the data in site id:{0}, the current site id:{1} and url:{2}.", siteInfo.Key, site.ID, site.Url);
                        }
                        else
                        {
                            foreach (var webInfo in siteInfo.Value)
                            {
                                try
                                {
                                    using (var web = this.site.OpenWeb(webInfo.Key))
                                    {
                                        foreach (var listInfo in webInfo.Value)
                                        {
                                            try
                                            {
                                                IAveList list = null;
                                                if (listInfo.Key != Guid.Empty)
                                                {
                                                    list = web.GetList(listInfo.Key);
                                                }

                                                var listChanged = false;
                                                //var isEnableMinorVersionsChanged = false;

                                                foreach (var documentInfo in listInfo.Value)
                                                {
                                                    try
                                                    {
                                                        var document = web.GetFile(documentInfo.Key);

                                                        var changed = false;

                                                        foreach (var postActionContract in documentInfo.Value)
                                                        {
                                                            try
                                                            {
                                                                postActionContract.PostSender = document;
                                                                IUserDataProcessor processor = GenerateProcessor(postActionContract);

                                                                if (processor == null)
                                                                {
                                                                    logger.Error("Cannot find processor for document post actions with site id:{0}, web id:{1}, list id:{2} and document:{3}. processor id:{4}", siteInfo.Key, webInfo.Key, listInfo.Key, document.ServerRelativeUrl, postActionContract.Id);
                                                                }
                                                                else if (processor.Process(document.Item))
                                                                {
                                                                    changed = true;
                                                                }
                                                                else
                                                                {
                                                                    logger.Error("Cannot post document post actions with site id:{0}, web id:{1}, list id:{2} and document:{3}. processor id:{4} -- {5}", siteInfo.Key, webInfo.Key, listInfo.Key, document.ServerRelativeUrl, postActionContract.Id, processor.GetType().Name);
                                                                }
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                logger.Error("Document post actions for site id:{0}, web id:{1}, list id:{2} and document id:{3} failed:{4}", siteInfo.Key, webInfo.Key, listInfo.Key, documentInfo.Key, ex);
                                                            }
                                                        }

                                                        if (changed)
                                                        {
                                                            if (!listChanged && list != null)
                                                            {
                                                                if (document.UIVersion % 512 == 0 && list.EnableMinorVersions)
                                                                {
                                                                    logger.Info("start to disable minor version of list:{0} to post action the document.", list.RootFolder.ServerRelativeUrl);
                                                                    list.EnableMinorVersions = false;
                                                                    list.Update();
                                                                    //isEnableMinorVersionsChanged = true;
                                                                    listChanged = true;
                                                                }
                                                            }

                                                            logger.Info("start to post action document:{0}", document.ServerRelativeUrl);
                                                            document.Item.SystemUpdate();
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        logger.Error("Document post actions for site id:{0}, web id:{1}, list id:{2} and document id:{3} failed:{4}", siteInfo.Key, webInfo.Key, listInfo.Key, documentInfo.Key, ex);
                                                    }
                                                }

                                                if (listChanged)
                                                {
                                                    logger.Info("start to rollback minor version of list:{0} after post action the document.", list.RootFolder.ServerRelativeUrl);
                                                    list.EnableMinorVersions = true;
                                                    list.Update();
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                logger.Error("Document post actions for site id:{0}, web id:{1} and list id:{2} failed:{3}", siteInfo.Key, webInfo.Key, listInfo.Key, ex);
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Error("Document post actions for site id:{0} and web id:{1} failed:{2}", siteInfo.Key, webInfo.Key, ex);
                                }
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    logger.Error("Document post actions failed:{0}", ex);
                }
            }
        }

        private IUserDataProcessor GenerateProcessor(PostActionContract postActionContract)
        {
            Type implType;
            if (processorMapping.TryGetValue(postActionContract.Id, out implType))
            {
                return (IUserDataProcessor)Activator.CreateInstance(implType, new object[3] { postActionContract, this.mapping, this.sourceSitInfo });
            }

            return null;
        }
        
    }
}
