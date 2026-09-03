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
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Hybrid.Browser.SharePointBrowser.Query
{
    public abstract class BaseBrowserQuery : IBrowserQuery
    {
        #region 各个level 的比较器
        internal class AveSiteBrowserInfoComparer : IComparer<AveSiteBrowserInfo>
        {
            public int Compare(AveSiteBrowserInfo x, AveSiteBrowserInfo y)
            {
                return string.Compare(x.DisplayName, y.DisplayName, StringComparison.CurrentCultureIgnoreCase);
            }
        }
        internal class AveWebBrowserInfoComparer : IComparer<AveWebBrowserInfo>
        {
            public int Compare(AveWebBrowserInfo x, AveWebBrowserInfo y)
            {
                return string.Compare(x.Url, y.Url, StringComparison.CurrentCulture);
            }
        }
        internal class AveListBrowserInfoComparer : IComparer<AveListBrowserInfo>
        {
            public int Compare(AveListBrowserInfo x, AveListBrowserInfo y)
            {
                return string.Compare(x.Name, y.Name, StringComparison.CurrentCulture);
            }
        }
        internal class AveFolderBrowserInfoComparer : IComparer<AveFolderBrowserInfo>
        {
            public int Compare(AveFolderBrowserInfo x, AveFolderBrowserInfo y)
            {
                return string.Compare(x.Name, y.Name, StringComparison.CurrentCulture);
            }
        }
        internal class AveListItemBrowserInfoComparer : IComparer<AveItemBrowserInfo>
        {
            public int Compare(AveItemBrowserInfo x, AveItemBrowserInfo y)
            {
                return x.ID - y.ID;
            }
        }
        internal class AveDocumentBrowserInfoComparer : IComparer<AveItemBrowserInfo>
        {
            public int Compare(AveItemBrowserInfo x, AveItemBrowserInfo y)
            {
                return string.Compare(x.Name, y.Name, StringComparison.CurrentCulture);
            }
        }
        internal class AveItemVersionBrowserInfoComparer : IComparer<AveItemVersionBrowserInfo>
        {
            public int Compare(AveItemVersionBrowserInfo x, AveItemVersionBrowserInfo y)
            {
                return string.Compare(x.VersionLabel, y.VersionLabel, StringComparison.CurrentCulture);
            }
        }

        internal class SiteInfoComparer : IEqualityComparer<AveSiteBrowserInfo>
        {

            public bool Equals(AveSiteBrowserInfo x, AveSiteBrowserInfo y)
            {
                return x.ID == y.ID;
            }

            public int GetHashCode(AveSiteBrowserInfo obj)
            {
                if (obj == null)
                    return 0;
                return obj.ID.GetHashCode();
            }
        }
        #endregion

        public abstract List<AveSiteBrowserInfo> GetBrowserSites(IAveWebApplication webApp, List<string> usernames, int startIndex, uint perPage, ref int childrenCount, ref bool hasError, bool needFilterInfo = false);

        public abstract string GetBrowserQueryConnectionString(string siteUrl, ref Guid siteId);

        public abstract void Dispose();

        public abstract List<AveWebBrowserInfo> GetBrowserWebs(AveBrowserOption option);

        public abstract List<AveFolderBrowserInfo> GetBrowserSubFolders(AveBrowserOption option);

        public abstract List<AveItemBrowserInfo> GetBrowserItems(AveBrowserOption option);

        public abstract List<AveItemVersionBrowserInfo> GetBrowserItemVersions(AveBrowserOption option);

        public abstract AveWebBrowserInfo GetBrowserRootWeb(AveBrowserOption option);

        public abstract AveFolderBrowserInfo GetBrowserRootFolder(AveBrowserOption option);

        public abstract List<AveListBrowserInfo> GetBrowserLists(AveBrowserOption option);
    }
}
