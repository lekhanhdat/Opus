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
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Backup
{
    public class AveSPNavigation : IDisposable
    {

        private AveSPWeb mAveSPWeb = null;

        public AveSPNavigation(AveSPWeb _AveWeb)
        {
            mAveSPWeb = _AveWeb;
        }

        public AveNavigationInfoList GetNavigations()
        {
            return mAveSPWeb.SPWeb.NavigationSerializer.GetObjectData() as AveNavigationInfoList;
        }

        public void Export(IAveBackupStream output)
        {
            Export(output, true);
        }

        public void Export(IAveBackupStream output, bool backupInheritedNavNodes)
        {
            Export(output, backupInheritedNavNodes, string.Empty);
        }

        public void Export(IAveBackupStream output, bool backupInheritedNavNodes, string srcWebAppUrl)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("Backup.AveSPWeb.Navigation"))
            {
                var serializer = mAveSPWeb.SPWeb.NavigationSerializer;
                serializer.BackupFromInheritedWeb = backupInheritedNavNodes;
                serializer.SourceWebApplicationUrl = srcWebAppUrl;//This argumment is used for PRItem
                var list = serializer.GetObjectData();
                output.WriteMetadata(AveMetadataType.Navigation, list);
            }
        }

        public void Dispose()
        {
            //TODO
        }
    }

    public class AveMOSSNavigation
    {
        public static int GetNodeType(string nodeType)
        {
            return (int)(Enum.Parse(typeof(AveNodeTypes), nodeType));
        }
    }

    public enum AveNodeTypes
    {
        // Summary:
        //     Specifies no node types.
        None = 0,

        //
        // Summary:
        //     Specifies any type of Microsoft.SharePoint.SPWeb site.
        Area = 1,

        //
        // Summary:
        //     Specifies a List item in the Pages list.
        Page = 2,

        //
        // Summary:
        //     Specifies a Microsoft SharePoint Foundation list (SPList).
        List = 4,

        //
        // Summary:
        //     Specifies a Microsoft SharePoint Foundation list item (SPListItem).
        ListItem = 8,

        //
        // Summary:
        //     Specifies a CMS Page Layout.
        PageLayout = 16,

        //
        // Summary:
        //     Specifies a navigation heading.
        Heading = 32,

        //
        // Summary:
        //     Specifies an authored link that references a page.
        AuthoredLinkToPage = 64,

        //
        // Summary:
        //     Specifies an authored link that references a Web site or area.
        AuthoredLinkToWeb = 128,

        //
        // Summary:
        //     Specifies a generic authored link.
        AuthoredLinkPlain = 256,

        //
        // Summary:
        //     Specifies any type of authored link.
        AuthoredLink = 448,

        //
        // Summary:
        //     Specifies a combination of Area, Page, Heading and AuthoredLink. Navigation
        //     uses this value to determine which node types to return by default.
        Default = 483,

        //
        // Summary:
        //     Specifies a custom node type that may be useful for extensibility purposes.
        Custom = 512,

        //
        // Summary:
        //     Specifies all node types, including Area, Page, List, ListItem, PageLayout,
        //     Heading, AuthoredLink, and Custom.
        All = 1023,

        //
        Error = 1024,
    }
}