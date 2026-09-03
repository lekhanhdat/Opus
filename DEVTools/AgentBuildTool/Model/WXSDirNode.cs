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
using AgentBuildTool.Common;

namespace AgentBuildTool.Model
{
    public class WXSDirNode
    {
        private string relativeFullPath = null;

        public WXSDirNode(WXSDirNode parentNode, string dirName, string dirId = null)
        {
            Name = dirName;
            Id = dirId ?? CommonConfig.GetWXSNodeId(WXSNodeIdType.Directory);
            ParentNode = parentNode;
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(relativeFullPath))
                {
                    relativeFullPath = $"{(ParentNode == null ? "" : (ParentNode.FullName + "\\"))}{Name}";
                }
                return relativeFullPath;
            }
        }

        public WXSDirNode ParentNode { get; set; }

        public WXSDirNode CreateChildDirNode(string childDirName)
        {
            return new WXSDirNode(this, childDirName);
        }

        public WXSFileNode CreateChildFileNode(string filePath)
        {
            return new WXSFileNode(this, filePath);
        }

        public string ToWXSString()
        {
            return string.Format(
                WXSFragmentTemplates.WXS_DirectoryFragment,
                ParentNode.Id,
                this.Id,
                this.Name
            );
        }
    }
}
