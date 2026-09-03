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
namespace RAFileSystem.FileSystem.Collector
{
    // simple default filter
    public class FileSystemDefaultFilter : IFileSystemFilter
    {
        private readonly bool includeFiles;
        private readonly bool includeDirectories;
        private readonly bool shouldDiscoverFolders;

        public FileSystemDefaultFilter() : this(true, true, true) { }

        public FileSystemDefaultFilter(bool includeDirectories, bool includeFiles, bool shouldDiscoverFolders)
        {
            this.includeDirectories = includeDirectories;
            this.includeFiles = includeFiles;
            this.shouldDiscoverFolders = shouldDiscoverFolders;
        }
        public bool ShouldIncludeFile(object filterObj = null)
        {
            return includeFiles;
        }
        public bool ShouldIncludeDirectory(object filterObj = null)
        {
            return includeDirectories;
        }
        public bool ShouldDiscoverDirectory(object filterObj = null)
        {
            return shouldDiscoverFolders;
        }
    }
}
