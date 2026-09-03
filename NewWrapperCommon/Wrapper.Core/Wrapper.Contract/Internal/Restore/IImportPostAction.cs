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

namespace AvePoint.Wrapper.Core.Internal.Restore
{
    public interface IImportPostAction : IDisposable
    {

    }

    public interface ISiteImportPostAction : IImportPostAction
    {
        bool Resolve(ISiteImport import);
    }

    public interface IWebImportPostAction : ISiteImportPostAction
    {
        bool Resolve(IWebImport import);
    }

    class PortalUrlPostAction : ISiteImportPostAction
    {
        private string url;

        public PortalUrlPostAction(string url)
        {
            this.url = url;
        }

        public bool Resolve(ISiteImport import)
        {
            import.RestorePortalUrl(url);

            return true;
        }

        public void Dispose()
        {
        }
    }

    class GroupOwnerPostAction : ISiteImportPostAction
    {
        private int groupId;
        private string groupName;
        private int groupOwnerSourceId;

        public GroupOwnerPostAction(int groupId, int groupOwnerSourceId, string groupName)
        {
            this.groupId = groupId;
            this.groupOwnerSourceId = groupOwnerSourceId;
            this.groupName = groupName;
        }
        public bool Resolve(ISiteImport import)
        {
            return import.RestoreGroupOwner(groupId, groupOwnerSourceId, groupName);
        }

        public void Dispose()
        {
        }
    }

    class MasterPagePostAction : IWebImportPostAction
    {
        private Guid webId;
        private AvePoint.Wrapper.Common.AveWebMasterPageInfo masterWebPageInfo;
        public MasterPagePostAction(Guid webId, AvePoint.Wrapper.Common.AveWebMasterPageInfo masterWebPageInfo)
        {
            this.webId = webId;
            this.masterWebPageInfo = masterWebPageInfo;
        }
        public bool Resolve(ISiteImport import) 
        {
            //return import.RestoreWebMasterPageInfoPostAction(webId, masterWebPageInfo);
            return false; 
        }
        public bool Resolve(IWebImport import)
        {
            return false;
        }
        public void Dispose()
        { 
        }
    }

    class WebLastModifiedTimePostAction : IWebImportPostAction
    {
        private Guid webId;
        private DateTime lastModifiedTime;
        public WebLastModifiedTimePostAction(Guid webId, DateTime lastModifiedTime)
        {
            this.webId = webId;
            this.lastModifiedTime = lastModifiedTime;
        }
        public bool Resolve(ISiteImport import) { return false; }
        public bool Resolve(IWebImport import)
        {
            return import.RestoreWebLastModifiedTimePostAction(webId, lastModifiedTime);
        }
        public void Dispose()
        {
        }
    }

    class UrlPostAction : IWebImportPostAction
    {
        private string strKey;
        private string metaValue;
        public UrlPostAction(string strKey, string metaValue)
        {
            this.strKey = strKey;
            this.metaValue=metaValue;
        }
        public bool Resolve(ISiteImport import) { return false; }
        public bool Resolve(IWebImport import)
        {
            return import.RestoreUrlPostAction(strKey, metaValue);
        }
        public void Dispose()
        {
        }
    }

    class WebAllPropertiesPostAction : IWebImportPostAction
    {
        private Guid webId;
        private Dictionary<string, string> metaInfoDictionary;
        public WebAllPropertiesPostAction(Guid webId, Dictionary<string, string> metaInfoDictionary)
        {
            this.webId = webId;
            this.metaInfoDictionary = metaInfoDictionary;
        }
        public bool Resolve(ISiteImport import) { return false; }
        public bool Resolve(IWebImport import)
        {
            return import.RetoreWebAllPropertiesPostAction(webId, metaInfoDictionary);
        }
        public void Dispose()
        {
        }
    }
}