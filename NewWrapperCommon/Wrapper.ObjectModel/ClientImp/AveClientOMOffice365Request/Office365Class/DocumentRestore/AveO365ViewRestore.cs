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
using System.IO;
using AveClientRequest.Common;
using AvePoint.ObjectModel.ClientOM;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Resource.Client;
using Microsoft.SharePoint.Client;
using ClientFile = Microsoft.SharePoint.Client.File;
using AvePoint.Office365.Api;

namespace AvePoint.ObjectModel.ClientOM
{
    public class AveO365ViewRestore : AveO365BaseDocumentRestore
    {
        public AveO365ViewRestore(AveClientContext context, AveClientOMOffice365Request request, FederationToken tokenProvider, AveDocumentInfo docInfo, Stream fileStream)
            : base(context, request, tokenProvider, docInfo, fileStream)
        { }

        public override Dictionary<string, object> Restore()
        {
            if (DocInfo.AveView.Vinfos == null)
            {
                return null;
            }
            var restoreResult = new Dictionary<string, object>();
            try
            {
                PrepareRestore();
                foreach (var viewInfo in DocInfo.AveView.Vinfos)
                {
                    //365 do not support personal view.
                    if (viewInfo.IsPersonal)
                    {
                        restoreResult["SkipViewItem"] = true;
                        restoreResult["SkipViewMessage"] = "Skip personal view restore.";
                        return restoreResult;
                    }
                    if (viewInfo.LeafName.Equals("mod-view.aspx", StringComparison.OrdinalIgnoreCase)
                        || viewInfo.LeafName.Equals("my-sub.aspx", StringComparison.OrdinalIgnoreCase))
                    {
                        //ADO-166849 过滤这两个view，因为关闭content approval后无法keep之前修改，而且开启content approval会产生view双份问题
                        restoreResult["SkipViewItem"] = true;
                        restoreResult["SkipViewMessage"] = "Skip 365 default mod-view.aspx or my-sub.aspx restore.";
                        return restoreResult;
                    }
                    View restoringView = TryGetView(viewInfo, DocInfo.FindViewByTitle);
                    if (restoringView != null)
                    {
                        restoreResult["ConflictWithDocument"] = true;
                    }
                    if (NeedSkipView(ref restoringView, viewInfo) && !DocInfo.IsNewCreated)
                    {
                        restoreResult["SkipViewItem"] = true;
                        restoreResult["SkipViewMessage"] = "Skip view restore, when conflict.";
                        return restoreResult;
                    }
                    if (AddNewView(ref restoringView, viewInfo))
                    {
                        var viewProperties = new Dictionary<string, object>();
                        Request.AssembleViewProperties(viewProperties, restoringView, DocInfo.ParentWebRelativeUrl);
                        restoreResult["View"] = viewProperties;
                    }
                    DocInfo.MappingManager.SiteMappingManager.AddViewGuidMapping(viewInfo.Id, restoringView.Id);
                    DocInfo.AveView.Views[viewInfo.Id] = restoringView.Id;
                    restoreResult["ViewUrl"] = restoringView.ServerRelativeUrl.Substring(DocInfo.ParentWebRelativeUrl.Length + 1);
                    restoreResult["RestoreSuccessfully"] = true;

                    ClientFile viewFile = HandleViewFile(restoringView);
                    if (viewFile != null)
                    {
                        var fileProperties = new Dictionary<string, object>();
                        fileProperties["ListName"] = DocInfo.ParentListTitle;
                        fileProperties["Exists"] = true;
                        Request.AssembleFileProperties(fileProperties, viewFile, DocInfo.ParentWebRelativeUrl, null);
                        restoreResult["File"] = fileProperties;
                    }
                }
            }
            catch (Exception ex)
            {
                restoreResult["RestoreSuccessfully"] = false;
                restoreResult["Exception"] = string.Format("Restore view under list:{0} failed:{1}.\r\n", DocInfo.ParentListTitle, ex.ToString());
            }
            return restoreResult;
        }

        protected override void PrepareRestore()
        {
            base.PrepareRestore();
            ParentWeb = Context.Site.OpenWeb(DocInfo.ParentWebRelativeUrl);
            PrepareLoadList();
        }

        private void PrepareLoadList()
        {
            if (DocInfo.ListId == Guid.Empty)
            {
                return;
            }
            ParentList = ParentWeb.Lists.GetById(this.DocInfo.ListId);
            Context.Load(ParentList);
            Context.Load(ParentList, l => l.BaseTemplate, l => l.Views);
            Context.ExecuteQuery();
        }

        /// <summary>
        /// Try to get view from destination.
        /// </summary>
        /// <param name="viewInfo"></param>
        /// <returns>null or finded view</returns>
        private View TryGetView(AveViewInfo viewInfo, bool findByTitle)
        {
            if (ParentList == null)
            {
                return null;
            }
            foreach (var view in ParentList.Views)
            {
                if (view.ServerRelativeUrl.EndsWith("/" + viewInfo.LeafName.TrimStart('/'), StringComparison.OrdinalIgnoreCase))
                {
                    return view;
                }
            }
            if (findByTitle)
            {
                foreach (var view in ParentList.Views)
                {
                    if (string.Equals(viewInfo.Title, view.Title, StringComparison.OrdinalIgnoreCase))
                    {
                        return view;
                    }
                }
            }
            return null;
        }

        private bool NeedSkipView(ref View view, AveViewInfo viewInfo)
        {
            //1.view exists. 2.overwrite is true. 3.View type equal. do not skip it or create new, just update it.
            if (view == null)
            {
                return false;
            }
            if (!view.ViewType.Equals(viewInfo.ViewType.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                view.DeleteObject();
                view = null;
                return false;
            }
            if (!DocInfo.SettingInfo.DELETE_ITEM)
            {
                DocInfo.MappingManager.SiteMappingManager.AddViewGuidMapping(viewInfo.Id, view.Id);
                DocInfo.AveView.Views[viewInfo.Id] = view.Id;
                return true;
            }
            return false;
        }

        private bool AddNewView(ref View view, AveViewInfo viewInfo)
        {
            if (ParentList == null)
            {
                return false;
            }
            bool newView = false;
            if (view == null)
            {
                string leafName = viewInfo.LeafName.Substring(0, viewInfo.LeafName.LastIndexOf('.'));
                var creationInformation = new ViewCreationInformation()
                {
                    Title = leafName,
                    Paged = true,
                    Query = string.Empty,
                    RowLimit = 100,
                    SetAsDefaultView = false,
                    ViewTypeKind = (ViewType)Enum.Parse(typeof(ViewType), viewInfo.ViewType.ToString()),
                    PersonalView = false
                };
                //ADO-130300 对于DataSheet类型的View(ViewType为Grid)的特殊处理，否则在切换view处会不显示
                if (creationInformation.ViewTypeKind == ViewType.Grid || creationInformation.ViewTypeKind == ViewType.Calendar)
                {
                    creationInformation.ViewTypeKind |= ViewType.Html;
                }
                view = ParentList.Views.Add(creationInformation);
                Context.Load(view);
                Context.ExecuteQuery();
                newView = true;
            }
            if (!view.ServerRelativeUrl.EndsWith(viewInfo.LeafName, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    ClientFile viewFile = ParentWeb.GetFileByServerRelativeUrl(view.ServerRelativeUrl);
                    viewFile.MoveTo(DocInfo.ServerRelativeUrl, MoveOperations.None);
                    Context.Load(ParentList.Views);
                    Context.Load(viewFile);
                    Context.ExecuteQuery();
                    view = TryGetView(viewInfo, DocInfo.FindViewByTitle);
                }
                catch (Exception e)
                {
                    Log.Warn("An error occurred while changing view file name. Error: " + e.ToString());
                }
            }
            UpdateView(view, viewInfo);
            if (newView)
            {
                Context.Load(view);
                Context.Load(view, v => v.ViewFields);
                Context.ExecuteQuery();
                DocInfo.RestoringItem.IsNewItem = DocInfo.IsNewCreated = true;
            }
            return newView;
        }
        private void UpdateView(View view, AveViewInfo viewInfo)
        {
            bool needUpdate = false;
            if (!string.Equals(view.Title, viewInfo.Title))
            {
                view.Title = viewInfo.Title;
                needUpdate = true;
            }
            if (view.Hidden != viewInfo.Hidden)
            {
                view.Hidden = viewInfo.Hidden;
                needUpdate = true;
            }
            if (needUpdate)
            {
                view.Update();
                Context.ExecuteQuery();
            }
        }

        private ClientFile HandleViewFile(View view)
        {
            try
            {
                var path = ResourcePath.FromDecodedUrl(view.ServerRelativeUrl);
                ClientFile file = ParentWeb.GetFileByServerRelativePath(path);
                LoadFileInfo(file);
                if (DocInfo.IsNewCreated || DocInfo.SettingInfo.DELETE_ITEM)
                {
                    RestoreWebParts(file);
                }
                if (DocInfo.HasStream && file != null)
                {
                    ClientFile.SaveBinaryDirect(Context, view.ServerRelativeUrl, FileStream, true);
                }
                return file;
            }
            catch (Exception ex)
            {
                Log.Debug(AveClientOMRequestResource.RestoreViewError, view.ServerRelativeUrl, ex.ToString());
                return null;
            }
        }
    }
}
