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
    public class WXSFileNode
    {
        public WXSFileNode(WXSDirNode parentNode, string fileName, string id = null)
        {
            FileName = fileName;
            Id = id ?? CommonConfig.GetWXSNodeId(WXSNodeIdType.File);
            ComponentId = CommonConfig.GetWXSNodeId(WXSNodeIdType.Component);
            Guid = System.Guid.NewGuid().ToString("B").ToUpper();
            ParentNode = parentNode;
        }

        public string Id { get; set; }

        public string ComponentId { get; set; }

        public string Guid { get; set; }

        public string FileName { get; set; }

        public string FullName
        {
            get
            {
                return $"{ParentNode.FullName}\\{FileName}";
            }
        }

        public WXSDirNode ParentNode { get; set; }

        public string ToWXSString()
        {
            return string.Format(
                WXSFragmentTemplates.WXS_FileComponent,
                this.ComponentId,
                ParentNode.Id,
                Guid,
                this.Id,
                this.FullName
            );
        }
    }
}
