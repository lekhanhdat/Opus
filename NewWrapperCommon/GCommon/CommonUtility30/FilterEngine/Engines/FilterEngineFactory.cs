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

namespace AvePoint.Common.FilterEngine
{
    internal class FilterEngineFactory
    {
        public static IFilterEngine GetFilterEngine(FilterOption option, FilterLevel level)
        {
            switch (level)
            {
                case FilterLevel.WebApp:
                    return new WebApplicationFilterEngine(option);
                case FilterLevel.SiteCollection:
                    return new SiteCollectionFilterEngine(option);
                case FilterLevel.Site:
                    return new SiteFilterEngine(option);
                case FilterLevel.List:
                    return new ListFilterEngine(option);
                case FilterLevel.Folder:
                    return new FolderFilterEngine(option);
                case FilterLevel.Document:
                    return new DocumentFilterEngine(option);
                case FilterLevel.DocumentVersion:
                    return new DocumentVersionFilterEngine(option);
                case FilterLevel.Item:
                    return new ItemFilterEngine(option);
                case FilterLevel.MicroFeedItem:
                    return new MicroFeedItemFilterEngine(option);
                case FilterLevel.ItemVersion:
                    return new ItemVersionFilterEngine(option);
                case FilterLevel.Attachment:
                    return new AttachmentFilterEngine(option);
                case FilterLevel.TreeNode:
                    return new TreeNodeFilterEngine(option);
                case FilterLevel.FSFolder:
                    return new FSFolderFilterEngine(option);
                case FilterLevel.FSFile:
                    return new FSFileFilterEngine(option);
                case FilterLevel.PhysicalBox:
                    return new PhysicalBoxFilterEngine(option);
                case FilterLevel.PhysicalFolder:
                    return new PhysicalFolderFilterEngine (option);
                default:
                    throw new LevelNotSupportedException(level.ToString());
            }
        }
    }
}
