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
using System.IO;
using System.Text;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;

namespace AvePoint.Wrapper.Common
{
    public class AveDestTreePath
    {        
        private LinkedList<AveDestTreeNode> mPathNodes;
        public const string TREE_NODE_NO_SELECT = "0";
        public const string TREE_NODE_SELF_SELECT = "1";
        public const string TREE_NODE_CHILD_SELECT = "2";
        public const int SITE_COLLECTION_LENGTH = 0;
        public const int WEB_LENGTH = 1;
        public const int LIST_LENGTH = 2;
        public const char TREE_NODE_INFO_SEPERATOR = '\t';

        public void InitDestTreePath(string path)
        {                        
            LinkedList<string> nodesInPath = TrimUselessLines(File.ReadAllLines(path));
            mPathNodes = new LinkedList<AveDestTreeNode>();
            BuildTreeNodePath(nodesInPath);                       
        }

        public AveDestTreeNode RootNode
        {
            get
            {
                return mPathNodes.First.Value;
            }
        }

        public AveDestTreeNode EndNode
        {
            get
            {
                return mPathNodes.Last.Value;
            }
        }

        private void BuildTreeNodePath(LinkedList<string> nodesInPath)
        {            
            foreach (string nodeInPath in nodesInPath)
            {
                string[] nodeInfo = nodeInPath.Split(TREE_NODE_INFO_SEPERATOR);                                
                List<string> info = TrimNodeInfo(nodeInfo);
                AveDestTreeNode destTreeNode = new AveDestTreeNode();
                destTreeNode.Type = GetType(nodeInfo);
                destTreeNode.State = info[0] == TREE_NODE_SELF_SELECT ? AveDestTreeState.SelfSelect : AveDestTreeState.ChildSelect;
                destTreeNode.DisplayName = TrimLiveLinkProjectLevel(info[1]);                
                if (info.Count == 3)
                {
                    destTreeNode.AttachedInfo = info[2];
                }
                if (mPathNodes.Last != null)
                {
                    destTreeNode.ParentNode = mPathNodes.Last.Value;
                    mPathNodes.Last.Value.SingleChildNode = destTreeNode;
                }
                mPathNodes.AddLast(destTreeNode);
                destTreeNode.ServerRelativeUrl = GetRelativePath(mPathNodes.First.Value);
            }
        }

        private string TrimLiveLinkProjectLevel(string projectLevel)
        {
            int index = -1;
            if ((index = projectLevel.IndexOf("??",StringComparison.OrdinalIgnoreCase)) != -1)
            {
                return projectLevel.Substring(0, index);
            }
            return projectLevel;
        }

        private List<string> TrimNodeInfo(string[] nodeInfos)
        {
            List<string> nodeInfoList = new List<string>();
            if (nodeInfos.Length == 1)
            {
                nodeInfoList.Add(nodeInfos[0][0].ToString());
                nodeInfoList.Add(nodeInfos[0].Substring(1));
            }
            else
            {
                foreach (string nodeInfo in nodeInfos)
                {
                    if (!string.IsNullOrEmpty(nodeInfo))
                    {
                        nodeInfoList.Add(nodeInfo);
                    }
                }
            }
            return nodeInfoList;
        }

        private char GetType(string[] nodeInfos)
        {
            int tabCount = 1;

            if (nodeInfos.Length == 1 || nodeInfos.Length == 2)
            {
                tabCount = nodeInfos.Length - 1;
            }            
            else
            {
                for (int i = 1; i < nodeInfos.Length; i++)
                {
                    if (string.IsNullOrEmpty(nodeInfos[i]))
                    {
                        tabCount++;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            switch (tabCount)
            {
                case SITE_COLLECTION_LENGTH:
                    return AveConstants.TYPE_SITE;
                case WEB_LENGTH:
                    return AveConstants.TYPE_WEB;
                case LIST_LENGTH:
                    return AveConstants.TYPE_LIST;
                default:
                    return AveConstants.TYPE_FOLDER;
            }
        }

        private string GetRelativePath(AveDestTreeNode destTreeNode)
        {
            StringBuilder relativePath = new StringBuilder();
            while (destTreeNode != null)
            {
                if (!destTreeNode.DisplayName.Equals("."))
                {
                    relativePath.Append(destTreeNode.DisplayName)
                                .Append("/");
                }
                destTreeNode = destTreeNode.SingleChildNode;
            }
            relativePath.Remove(relativePath.Length - 1, 1);
            int index = relativePath.ToString().IndexOf("/", "https://".Length,StringComparison.OrdinalIgnoreCase);
            if (index != -1)
            {
                return relativePath.Remove(0, index).ToString();
            }
            else
            {
                return "/";
            }            
        }        

        private LinkedList<string> TrimUselessLines(string[] linesStr)
        {
            LinkedList<string> nodesInPath = new LinkedList<string>();
            foreach (string treeNodeStr in linesStr)
            {
                if (!IsShouldSkip(treeNodeStr))
                {
                    nodesInPath.AddLast(treeNodeStr);
                }
            }
            return nodesInPath;
        }

        private static bool IsShouldSkip(string lineContent)
        {
            return string.IsNullOrEmpty(lineContent) || lineContent.StartsWith(TREE_NODE_NO_SELECT,StringComparison.OrdinalIgnoreCase);
        }
    }

    public class AveDestTreeNode
    {
        public AveDestTreeState State { get; set; }
        public string DisplayName { get; set; }
        public string ServerRelativeUrl { get; set; }
        public char Type { get; set; }
        public string AttachedInfo { get; set; }
        public AveDestTreeNode ParentNode { get; set; }
        public AveDestTreeNode SingleChildNode { get; set; }
    }

    public enum AveDestTreeState
    {
        NoSelect = 0,        
        SelfSelect = 1,
        ChildSelect = 2
    }
}
