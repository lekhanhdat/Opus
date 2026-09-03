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
using System.Text;

namespace AvePoint.Wrapper.Common
{

    #region Just Test
    public struct DocumentRestoreInfo
    {
        public RestoreWebInfo ParentWebInfo { get; set; }
        public RestoreListInfo ParentListInfo { get; set; }
        public RestoreFolderInfo ParentFolderInfo { get; set; }
    }

    public class RestoreWebInfo
    {
        public Guid Id { get; private set; }
        public string Url { get; private set; }
        public string WebTemplate { get; private set; }
        public string ServerRelativeUrl { get; private set; }
        public string RootFolderWelcomePage { get; private set; }

        public RestoreWebInfo(IAveWeb web)
        {
            Id = web.ID;
            Url = web.Url;
            WebTemplate = web.Template;
            ServerRelativeUrl = web.ServerRelativeUrl;
            RootFolderWelcomePage = web.RootFolder.WelcomePage;
        }
    }

    public class RestoreListInfo
    {
        public Guid Id { get; private set; }
        public string Title { get; private set; }
        public int BaseTemplate { get; private set; }
        public int BaseType { get; private set; }
        public int DraftVersionVisibility { get; private set; }
        public bool EnableVersioning { get; set; }
        public bool EnableMinorVersions { get; set; }
        public bool ForceCheckout { get; private set; }
        public bool EnableModeration { get; private set; }
        public bool ServerTemplateCanCreateFolders { get; private set; }
        public bool EnableFolderCreation { get; private set; }
        public bool HasExternalDataSource { get; private set; }
        public List<string> ListFieldInternalNames { get; set; }
        public bool isListVersionSettingChanged { get; set; }

        public RestoreListInfo()
        {
        }

        public RestoreListInfo(IAveList list)
        {
            Id = list.ID;
            Title = list.Title;
            BaseTemplate = (int)list.BaseTemplate;
            BaseType = (int)list.BaseType;
            DraftVersionVisibility = (int)list.DraftVersionVisibility;
            EnableVersioning = list.EnableVersioning;
            EnableMinorVersions = list.EnableMinorVersions;
            ForceCheckout = list.ForceCheckout;
            EnableModeration = list.EnableModeration;
            ServerTemplateCanCreateFolders = list.ServerTemplateCanCreateFolders;
            EnableFolderCreation = list.EnableFolderCreation;
            HasExternalDataSource = list.HasExternalDataSource;
            isListVersionSettingChanged = false;
        }
    }

    public class RestoreFolderInfo
    {
        public string ServerRelativeUrl { get; private set; }

        public RestoreFolderInfo(IAveFolder folder)
        {
            ServerRelativeUrl = folder.ServerRelativeUrl;
        }
    }
    #endregion
}
