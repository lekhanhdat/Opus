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
namespace AvePoint.Wrapper.Restore.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AvePoint.Wrapper.Common;
    using AvePoint.Wrapper.Common.Core;

    /// <summary>
    /// WebPart也需要遵从这个Solution，慢慢得把post action合理化。
    /// </summary>
    class SharePointDocumentDataProcessor : SharePointDataProcessor
    {
        private static Dictionary<Guid, Type> processorMapping = new Dictionary<Guid, Type>()
        {
            { new ClientSideWebPartProcessor().Id, typeof(ClientSideWebPartProcessor) },
        };

        private readonly AveSPSite site;
        private readonly AveSPDoc document;

        public SharePointDocumentDataProcessor(AveSPDoc document)
        {
            this.document = document;
        }

        private SharePointDocumentDataProcessor(AveSPSite site)
        {
            this.site = site;
        }

        /// <summary>
        /// true  => completed
        /// false => require post action
        /// </summary>
        /// <param name="userData"></param>
        /// <returns></returns>
        public bool ProcessUserData(Dictionary<string, object> userData)
        {
            bool needPostAction = false;

            var processorList = new List<IUserDataProcessor>() {
                new ClientSideWebPartProcessor(document)
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
                document.ParentSite.MappingManager.SiteMappingManager.AddDocumentPostActions(
                    document.ParentSite.SPSite.ID,
                    document.Web.ID,
                    document.ParentFolder.ParentList.Id,
                    document.AveSPItem.Id,
                    postActions.Select(a => a.GeneratePostActionContract())
                    );
            }
        }

        protected override void PostActionImpl()
        {
            var mapping = site.MappingManager.SiteMappingManager.DocumentPostActions;
            if (mapping != null && mapping.Count > 0)
            {
                try
                {
                    foreach (var siteInfo in mapping)
                    {
                        if (siteInfo.Key != site.SPSite.ID)
                        {
                            logger.Warn("Cannot post action the data in site id:{0}, the current site id:{1} and url:{2}.", siteInfo.Key, site.SPSite.ID, site.SPSite.Url);
                        }
                        else
                        {
                            foreach (var webInfo in siteInfo.Value)
                            {
                                try
                                {
                                    using (var web = site.SPSite.OpenWeb(webInfo.Key))
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
                                                AveDraftVisibilityType listDraftVersionVisibility = AveDraftVisibilityType.Reader;
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
                                                                IUserDataProcessor processor = GenerateProcessor(postActionContract);
                                                                if (processor == null)
                                                                {
                                                                    logger.Error("Cannot find processor for document post actions with site id:{0}, web id:{1}, list id:{2} and document:{3}. processor id:{4}", siteInfo.Key, webInfo.Key, listInfo.Key, documentInfo.Key, postActionContract.Id);
                                                                }
                                                                else if (processor.Process(document.Item))
                                                                {
                                                                    changed = true;
                                                                }
                                                                else
                                                                {
                                                                    logger.Error("Cannot post document post actions with site id:{0}, web id:{1}, list id:{2} and document:{3}. processor id:{4} -- {5}", siteInfo.Key, webInfo.Key, listInfo.Key, documentInfo.Key, postActionContract.Id, processor.GetType().Name);
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
                                                                    listDraftVersionVisibility = list.DraftVersionVisibility;
                                                                    list.Update();
                                                                    //isEnableMinorVersionsChanged = true;
                                                                    listChanged = true;
                                                                }
                                                            }

                                                            logger.Info("start to post action document:{0}", documentInfo.Key);
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
                                                    if (listDraftVersionVisibility != AveDraftVisibilityType.Reader)
                                                    {
                                                        list.DraftVersionVisibility = listDraftVersionVisibility;
                                                    }
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
                return (IUserDataProcessor)Activator.CreateInstance(implType, postActionContract);
            }
            return null;
        }

        public static void PostAction(AveSPSite site)
        {
            new SharePointDocumentDataProcessor(site).PostActionImpl();
        }
    }
}
