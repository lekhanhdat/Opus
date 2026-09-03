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
namespace AvePoint.ObjectModel.ClientOM
{
    using System;
    using System.Collections.Generic;
    using AvePoint.GCommon;
    using AvePoint.Wrapper.Common;
    using OfficeDevPnP.Core.Pages;
    using Microsoft.SharePoint;
    using AveClientRequest.Common;
    using Microsoft.SharePoint.Client;
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
            { ClientSideWebPartListWorker.Id, new ClientSideWebPartListWorker()},
            //{ ClientSideWebPartNewsListWorker.Id, new ClientSideWebPartNewsListWorker()},
            { ClientSideWebPartNewsWorker.Id, new ClientSideWebPartNewsWorker()},
            { ClientSideWebPartPeopleWorker.Id, new ClientSideWebPartPeopleWorker()},
            { ClientSideWebPartQuickLinksWorker.Id, new ClientSideWebPartQuickLinksWorker()},
            { ClientSideWebPartOffice365VideoWorker.Id, new ClientSideWebPartOffice365VideoWorker()},
            { ClientSideWebPartDividerWorker.Id, new ClientSideWebPartDividerWorker()},
            { ClientSideWebPartSitesWorker.Id, new ClientSideWebPartSitesWorker()},
        };

        private IAveFile document;
        private readonly bool enableFilter;
        private readonly AveSiteMappingManager mapping;
        private readonly AveSiteInfo sourceSitInfo;
        private readonly IAveWeb web;
        private readonly Guid fileId;

        private List<Guid> postActionsIds = new List<Guid>();
        private AveClientContext context;
        private readonly Func<string, string> getUseMethod;


        internal ClientSideWebPartProcessor() { }

        public ClientSideWebPartProcessor(PostActionContract contract, AveSiteMappingManager mapping, AveSiteInfo SourceSiteInfo, Func<string, string> GetUserFromMapping)
        {
            if (contract.PostSender != null && contract.PostSender is IAveFile)
            {
                this.document = contract.PostSender as IAveFile;
            }
            this.mapping = mapping;
            this.sourceSitInfo = SourceSiteInfo;
            enableFilter = true;
            this.getUseMethod = GetUserFromMapping;
            postActionsIds = (List<Guid>)contract.Metadata["postActionsIds"];
        }

        public ClientSideWebPartProcessor(AveClientContext context, IAveWeb web, Guid fileid, AveSiteMappingManager mapping, AveSiteInfo sourceSitInfo, Func<string, string> GetUserFromMapping)
        {
            this.context = context;
            this.fileId = fileid;
            this.web = web;
            this.mapping = mapping;
            this.sourceSitInfo = sourceSitInfo;
            this.getUseMethod = GetUserFromMapping;
        }

        public Guid Id { get { return new Guid("F0D9EBDA-817D-4D53-AFB9-83B1C1A4C022"); } }

        public PostActionContract GeneratePostActionContract()
        {
            return new PostActionContract()
            {
                Id = Id,
                Metadata = new Dictionary<string, object>(1) { { "postActionsIds", postActionsIds } }
            };
        }

        public bool Process(Dictionary<string, object> userData)
        {
            return Process(userData, (k, v) => {
                var value = userData[k] as AveFieldValueInfo;
                if (value != null)
                {
                    value.ColValue = v;
                    userData[k] = value;
                }
                else
                {
                    userData[k] = v;
                }
            });
        }

        public bool Process(IAveListItem listItem)
        {
            return Process(listItem.FieldValues, (k, v) => {
                var value = listItem[k] as AveFieldValueInfo;
                if (value != null)
                {
                    value.ColValue = v;
                    listItem[k] = value;
                }
                else
                {
                    listItem[k] = v;
                }
            });
        }

        private bool Process(Dictionary<string, object> userData, Action<string, object> updater)
        {
            bool requirePostAction = false;

            object clientSideApplicationId;
            if (userData != null &&
                userData.TryGetValue(ClientSidePage.ClientSideApplicationId, out clientSideApplicationId) &&
                clientSideApplicationId != null &&
                new Guid(ClientSidePage.SitePagesFeatureId)== new Guid(clientSideApplicationId.ToString()))
            {
                ReplacePageHeader(userData, updater);

                Object columnValueAsObj;

                if (userData.TryGetValue(ClientSidePage.CanvasField, out columnValueAsObj) && columnValueAsObj != null)
                {
                    var columnValue = columnValueAsObj.ToString();

                    if (!string.IsNullOrEmpty(columnValue))
                    {
                        var page = ClientSidePage.FromHtml(columnValue);

                        var changed = false;

                        foreach (var control in page.Controls)
                        {
                            var webPart = control as ClientSideWebPart;

                            if (webPart != null && (enableFilter == false || postActionsIds.Contains(webPart.InstanceId)))
                            {
                                var worker = GetClientSiteWebPartWorker(webPart.WebPartId);

                                if (worker != null)
                                {
                                    if (document == null && this.web !=null)
                                    {
                                        document = web.GetFile(fileId);
                                    }
                                    if (document != null)
                                    {
                                        worker.SetMappingAndSourceInfo(this.mapping, this.sourceSitInfo, context, this.getUseMethod);
                                        if (worker.Process(webPart, document, this.mapping, enableFilter))
                                        {
                                            changed = true;
                                        }
                                        else
                                        {
                                            if (!enableFilter)
                                            {
                                                postActionsIds.Add(webPart.InstanceId);
                                            }
                                            requirePostAction = true;
                                        }
                                    }
                                }
                            }
                        }

                        if (changed)
                        {
                            var changedValue = page.ToHtml();

                            updater(ClientSidePage.CanvasField, changedValue);

                            logger.Info("Update the CanvasField Value:{0} to {1}\r\nRequired Post Action:{2}",
                                columnValue,
                                changedValue,
                                requirePostAction);
                        }
                        else if (requirePostAction)
                        {
                            logger.Info("Required Post Action --> The CanvasField Value:{0}", columnValue);
                        }
                    }
                }
            }

            return !requirePostAction;
        }

        private void ReplacePageHeader(Dictionary<string, object> userData, Action<string, object> updater)
        {
            object headerValue;
            if (userData.TryGetValue(ClientSidePage.PageLayoutContentField, out headerValue) && headerValue != null)
            {
                var headerValueString = headerValue.ToString();
                var pageHeader = new ClientSidePageHeader(null, ClientSidePageHeaderType.Default, null);
                pageHeader.FromHtml(headerValueString);
                var needupdate = ChangeAuthors(pageHeader);
                needupdate = needupdate | ChangeAuthorByline(pageHeader);
                if (needupdate)
                {
                    updater(ClientSidePage.PageLayoutContentField, pageHeader.ToHtml(null));
                }
            }
        }

        private bool ChangeAuthorByline(ClientSidePageHeader pageHeader)
        {
            bool needupdate = false;
            if (!string.IsNullOrEmpty(pageHeader.AuthorByLine))
            {
                var mappedUser = this.getUseMethod(pageHeader.AuthorByLine);
                if (!string.IsNullOrEmpty(mappedUser))
                {
                    needupdate = true;
                    pageHeader.AuthorByLine = mappedUser;
                }
            }
            return needupdate;
        }

        private bool ChangeAuthors(ClientSidePageHeader pageHeader)
        {
            bool needupdate = false;
            if (!string.IsNullOrEmpty(pageHeader.Authors))
            {
                var authors = JArray.Parse(pageHeader.Authors);
                for (var i = 0; i < authors.Count; i++)
                {
                    var au = authors[i] as JObject;
                    var needchangeProperties = new List<string> { "id", "upn" };
                    foreach (var p in needchangeProperties)
                    {
                        if (au.ContainsKey(p))
                        {
                            var property = au.Property(p);
                            var mappedUser = this.getUseMethod(property.Value.ToString().Trim(true, false, "i:0#.f|membership|"));
                            if (!string.IsNullOrEmpty(mappedUser))
                            {
                                needupdate = true;
                                property.Value = mappedUser;
                                authors[i] = au;
                            }
                        }
                    }
                }
                pageHeader.Authors = authors.ToString();
            }

            return needupdate;
        }

        private IClientSideWebPartWorker GetClientSiteWebPartWorker(string webPartId)
        {
            IClientSideWebPartWorker worker;

            workers.TryGetValue(webPartId, out worker);

            return worker;
        }
    }
}
