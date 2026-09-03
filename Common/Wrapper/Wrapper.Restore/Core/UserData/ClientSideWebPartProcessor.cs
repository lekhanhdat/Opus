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
    using System.Reflection;
    using AvePoint.GCommon;
    using AvePoint.Wrapper.Common;
    using AvePoint.Wrapper.Common.Core;
    using PnP.Core.Model.SharePoint;
    using PnP.Core.Services;
    using System.Web;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    class ClientSideWebPartProcessor : IUserDataProcessor
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(ClientSideWebPartProcessor));
        private static Dictionary<string, IClientSideWebPartWorker> workers = new Dictionary<string, IClientSideWebPartWorker>(StringComparer.OrdinalIgnoreCase)
        {
            { ClientSideWebPartHighlightedContentWorker.Id, new ClientSideWebPartHighlightedContentWorker()},
            { ClientSideWebPartFileViewerWorker.Id, new ClientSideWebPartFileViewerWorker()},
            { ClientSideWebPartEventsWorker.Id, new ClientSideWebPartEventsWorker()},
            { ClientSideWebPartHeroWorker.Id, new ClientSideWebPartHeroWorker()},
            { ClientSideWebPartImageWorker.Id, new ClientSideWebPartImageWorker()},
            { ClientSideWebPartImageGalleryWorker.Id, new ClientSideWebPartImageGalleryWorker()},
            { ClientSideWebPartLinkWorker.Id, new ClientSideWebPartLinkWorker()},
            //{ ClientSideWebPartListWorker.Id, new ClientSideWebPartListWorker()},
            //{ ClientSideWebPartNewsListWorker.Id, new ClientSideWebPartNewsListWorker()},
            { ClientSideWebPartNewsWorker.Id, new ClientSideWebPartNewsWorker()},
            { ClientSideWebPartPeopleWorker.Id, new ClientSideWebPartPeopleWorker()},
            { ClientSideWebPartQuickLinksWorker.Id, new ClientSideWebPartQuickLinksWorker()},
            { ClientSideWebPartOffice365VideoWorker.Id, new ClientSideWebPartOffice365VideoWorker()},
            { ClientSideWebPartDividerWorker.Id, new ClientSideWebPartDividerWorker()},
            { ClientSideWebPartSitesWorker.Id, new ClientSideWebPartSitesWorker()},
            { ClientSideWebPartTwitterWorker.Id, new ClientSideWebPartTwitterWorker()},
            { ClientSideWebPartBingMapsWorker.Id, new ClientSideWebPartBingMapsWorker()},
            { ClientSideWebPartSiteActivityWorker.Id, new ClientSideWebPartSiteActivityWorker()},
            { ClientSideWebPartDocumentLibraryWorker.Id, new ClientSideWebPartDocumentLibraryWorker()},
        };

        private readonly AveSPDoc document;
        private readonly bool enableFilter;

        private List<Guid> postActionsIds = new List<Guid>();

        public ClientSideWebPartProcessor(AveSPDoc document)
        {
            this.document = document;
        }

        internal ClientSideWebPartProcessor() { }

        public ClientSideWebPartProcessor(PostActionContract contract)
        {
            if (contract.PostSender != null && contract.PostSender is AveSPDoc)
            {
                this.document = contract.PostSender as AveSPDoc;
            }
            enableFilter = true;
            postActionsIds = (List<Guid>)contract.Metadata["postActionsIds"];
        }

        public Guid Id { get { return new Guid("F0D9EBDA-817D-4D53-AFB9-83B1C1A4C022"); } }

        public PostActionContract GeneratePostActionContract()
        {
            return new PostActionContract()
            {
                Id = Id,
                PostSender = this.document,
                Metadata = new Dictionary<string, object>(1) { { "postActionsIds", postActionsIds } }
            };
        }

        private readonly string nullImageSources = "&quot;imageSources&quot;&#58;null";
        private readonly string normalImageSources = "&quot;imageSources&quot;&#58;&#123;&#125;";

        public bool Process(Dictionary<string, object> userData)
        {
            return Process(userData, (k, v) => userData[k] = v).Item1;
        }

        public bool Process(IAveListItem listItem)
        {
            return Process(listItem.FieldValues, (k, v) => listItem[k] = v).Item2;
        }

        private (bool, bool) Process(Dictionary<string, object> userData, Action<string, object> updater)
        {
            bool requirePostAction = false;
            var changed = false;

            object clientSideApplicationId;
            if (userData != null &&
                userData.TryGetValue(PnP.Framework.Modernization.Constants.ClientSideApplicationIdField, out clientSideApplicationId) &&
                clientSideApplicationId != null &&
                PnP.Framework.Modernization.Constants.FeatureId_Web_ModernPage.ToString().Equals(clientSideApplicationId.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                Object columnValueAsObj;

                if (userData.TryGetValue(PnP.Framework.Modernization.Constants.CanvasContentField, out columnValueAsObj) && columnValueAsObj != null)
                {
                    var columnValue = columnValueAsObj.ToString();

                    if (!string.IsNullOrEmpty(columnValue))
                    {
                        logger.Info("Load ClientSidePage From Html:[{0}]", columnValue);

                        if (columnValue.Contains(nullImageSources))
                        {
                            try
                            {
                                columnValue = ProcessImageSources(columnValue);
                                logger.Info($"Load ClientSidePage From Html with ImageSources replace:[{columnValue}]");
                            }
                            catch (Exception ex)
                            {
                                logger.Warn($"Failed to replace the ImageSources in ClientSidePage Html. Reason:[{ex}]");
                            }
                        }

                        AveAssemblyUtility.AddTypeSearchAssembly(Assembly.Load(new AssemblyName("PnP.Core")));
                        var page = (IPage)AveAssemblyUtility.CreateInstance("PnP.Core.Model.SharePoint.Page", new Type[] { typeof(PnPContext), typeof(IList), typeof(IListItem), typeof(PageLayoutType) }, new object[] { null, null, null, null });

                        try
                        {
                            AveAssemblyUtility.InvokeGenericMethod(page, "LoadFromHtml", new object[] { columnValue, "" }, new Type[] { typeof(string), typeof(string) });
                        }
                        catch (Exception ex)
                        {
                            logger.Info("Load ClientSidePage From Html failed. The page may not have any control. Error:{0}", ex);
                            return (true, false);
                        }

                        foreach (var control in page.Controls)
                        {
                            if (control is IPageWebPart webPart && (enableFilter == false || postActionsIds.Contains(webPart.InstanceId)))
                            {
                                CompatibleForOneColumnFullWidthSection(webPart);

                                var aveWebPart = new AveClientSideWebPart(webPart);

                                var worker = GetClientSiteWebPartWorker(aveWebPart.WebPartId);

                                if (worker != null && document != null)
                                {
                                    if (worker.Process(aveWebPart, document, enableFilter))
                                    {
                                        changed = true;
                                    }
                                    else
                                    {
                                        if (!enableFilter)
                                        {
                                            postActionsIds.Add(aveWebPart.InstanceId);
                                        }
                                        requirePostAction = true;
                                    }
                                }
                            }
                        }

                        if (changed)
                        {
                            try
                            {
                                var changedValue = (string)AveAssemblyUtility.InvokeGenericMethod(page, "ToHtml", new object[] { }, new Type[] { });

                                updater(PnP.Framework.Modernization.Constants.CanvasContentField, changedValue);

                                logger.Info("Update the CanvasField Value:{0} to {1}\r\nRequired Post Action:{2}",
                                    columnValue,
                                    changedValue,
                                    requirePostAction);
                            }
                            catch (Exception ex)
                            {
                                if (WrapperConfiguration.WrapperConfigurationForBPOS.SkipWebPartError)
                                {
                                    logger.Warn($"Still restore file when web part analyze failed.messsage:{ex}.");
                                }
                                else
                                {
                                    logger.Warn($"Failed analyze web part.messsage:{ex}.");
                                    throw;
                                }
                            }
                            
                        }
                        else if (requirePostAction)
                        {
                            logger.Info("Required Post Action --> The CanvasField Value:{0}", columnValue);
                        }
                    }
                }
            }

            return (!requirePostAction, changed);
        }

        private IClientSideWebPartWorker GetClientSiteWebPartWorker(string webPartId)
        {
            IClientSideWebPartWorker worker;

            workers.TryGetValue(webPartId, out worker);

            return worker;
        }

        private void CompatibleForOneColumnFullWidthSection(IPageWebPart webPart)
        {
            if (webPart.Section.Type == CanvasSectionTemplate.OneColumnFullWidth && !webPart.SupportsFullBleed)
            {
                AveAssemblyUtility.SetPropertyValue(webPart, "SupportsFullBleed", true);
            }
        }

        private string ProcessImageSources(string columnValue)
        {
            try
            {
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(columnValue);

                htmlDocument.DocumentNode.SelectNodes("//div")?.ToList().ForEach(i =>
                {
                    if (i.Attributes.Contains("data-sp-webpartdata"))
                    {
                        var data = HttpUtility.HtmlDecode(i.Attributes["data-sp-webpartdata"].Value);
                        var dynamicObj = JsonConvert.DeserializeObject<dynamic>(data);
                        if (dynamicObj["serverProcessedContent"]?["imageSources"]?.ToString() == string.Empty)
                        {
                            dynamicObj["serverProcessedContent"]["imageSources"] = JObject.FromObject(new object());
                            data = JsonConvert.SerializeObject(dynamicObj);
                            i.Attributes["data-sp-webpartdata"].Value = HttpUtility.HtmlEncode(data);
                        }
                    }
                });
                return htmlDocument.DocumentNode.OuterHtml;
            }
            catch (Exception ex)
            {
                logger.Error($"Failed process the null ImageSources. Reason: {ex}");
                return columnValue.Replace(nullImageSources, normalImageSources);
            }
        }
    }
}
