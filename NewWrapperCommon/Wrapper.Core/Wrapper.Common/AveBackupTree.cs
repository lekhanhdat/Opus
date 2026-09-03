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
using System.Text;
using System.IO;

namespace AvePoint.Wrapper.Common
{
    public class AveBackupTree
    {
        public const char TREE_NODE_INFO_SEPERATOR = '\t';
        public const int TREE_NODE_CANNOT_SELECT = -1;
        public const int TREE_NODE_NO_SELECT = 0;
        public const int TREE_NODE_ALL_SELECT = 1;
        public const int TREE_NODE_SELF_SELECT = 2;
        public const int EROOM_SITE_INFO_LENGTH = 2;
        public const int COMMUNITY_INFO_LENGTH = 4;
        public const int FACILITY_INFO_LENGTH = 5;
        public const int EROOM_INFO_LENGTH = 6;
        public const int LIST_INFO_LENGTH = 7;

        private AveBackupTreeNode mRootNode;

        public AveBackupTreeNode RootNode
        {
            get
            {
                return mRootNode;
            }            
        }

        public void InitBackupTree(string location)
        {
            string[] rawTreeContent = File.ReadAllLines(location);            
            AveBackupTreeNode communityNode = null;
            AveBackupTreeNode facilityNode = null;
            AveBackupTreeNode eroomNode = null;
            List<string> treeContent = new List<string>();
            int lineCount = 0;

            foreach (string lineContent in rawTreeContent)
            {
                if (!IsShouldSkip(lineContent))
                {
                    treeContent.Add(lineContent);
                }
            }

            while (lineCount < treeContent.Count)
            {
                string[] nodeInfo = treeContent[lineCount++].Split(TREE_NODE_INFO_SEPERATOR);
                switch (nodeInfo.Length)
                {
                    case EROOM_SITE_INFO_LENGTH:
                        mRootNode = ConstructRootNode(Trim(nodeInfo));
                        break;
                    case COMMUNITY_INFO_LENGTH:
                        communityNode = ConstructTreeNode(Trim(nodeInfo));
                        mRootNode.AddChild(communityNode);
                        break;
                    case FACILITY_INFO_LENGTH:
                        facilityNode = ConstructTreeNode(Trim(nodeInfo));
                        communityNode.AddChild(facilityNode);
                        break;
                    case EROOM_INFO_LENGTH:
                        eroomNode = ConstructTreeNode(Trim(nodeInfo));
                        facilityNode.AddChild(eroomNode);
                        break;
                    case LIST_INFO_LENGTH:
                        eroomNode.AddChild(ConstructListNodeRecursively(treeContent, ref lineCount, nodeInfo));
                        break;
                    default:
                        break;
                }
            }
        }

        private static AveBackupTreeNode ConstructListNodeRecursively(List<string> treeContent, ref int lineCount, string[] nodeInfo)
        {
            AveBackupTreeNode listNode = ConstructTreeNode(Trim(nodeInfo));
            while (lineCount < treeContent.Count)
            {
                string[] nextNodeInfo = treeContent[lineCount++].Split(TREE_NODE_INFO_SEPERATOR);
                if (nextNodeInfo.Length <= nodeInfo.Length)
                {
                    lineCount--;
                    break;
                }
                else if (nextNodeInfo.Length > nodeInfo.Length)
                {
                    listNode.AddChild(ConstructListNodeRecursively(treeContent, ref lineCount, nextNodeInfo));
                }
            }
            return listNode;
        }

        private static List<string> Trim(string[] nodeInfos)
        {
            List<string> results = new List<string>();
            foreach (string nodeInfo in nodeInfos)
            {
                if (!string.IsNullOrEmpty(nodeInfo))
                {
                    results.Add(nodeInfo);
                }
            }
            return results;
        }

        private static AveBackupTreeNode ConstructRootNode(List<string> nodeInfo)
        {
            return new AveBackupTreeNode(nodeInfo[0], null, AveTreeSelectFlag.CanNotSelect, AveTreeSelectSuffixFlag.NoSuffix, true);
        }

        private static AveBackupTreeNode ConstructTreeNode(List<string> nodeInfo)
        {
            return new AveBackupTreeNode(nodeInfo[2], nodeInfo[1], (AveTreeSelectFlag)Convert.ToInt32(nodeInfo[0]), (AveTreeSelectSuffixFlag)Convert.ToInt32(nodeInfo[nodeInfo.Count - 1]), false);
        }

        private static bool IsShouldSkip(string lineContent)
        {
            return string.IsNullOrEmpty(lineContent) || lineContent.StartsWith(TREE_NODE_NO_SELECT.ToString(),StringComparison.OrdinalIgnoreCase);
        }
    }
    
    public class AveBackupTreeNode
    {
        private string mId;
        private string mName;
        private string mRelativeUrl;
        private AveTreeSelectFlag mTreeSelectedFlag;
        private AveTreeSelectSuffixFlag mSuffixFlag;        
        private int mDepth;
        private bool mIsRoot;
        private AveBackupTreeNode mParent;
        private List<AveBackupTreeNode> mChildren = new List<AveBackupTreeNode>();

        public AveBackupTreeNode()
        {
        }

        public AveBackupTreeNode(string id, string name, AveTreeSelectFlag selectedFlag, AveTreeSelectSuffixFlag suffixFlag, bool isRoot)
        {
            mId = id;
            mName = name;
            mTreeSelectedFlag = selectedFlag;
            mIsRoot = isRoot;
            mSuffixFlag = suffixFlag;
        }

        public string Id
        {
            get
            {
                return mId;
            }
            set
            {
                mId = value;
            }
        }

        public string Name
        {
            get
            {
                return mName;
            }
            set
            {
                mName = value;
            }
        }

        public string RelativeUrl
        {
            get
            {
                return mRelativeUrl;
            }
            set
            {
                mRelativeUrl = value;
            }
        }

        public AveTreeSelectFlag SelectedFlag
        {
            get
            {
                return mTreeSelectedFlag;
            }
            set
            {
                mTreeSelectedFlag = value;
            }
        }

        public AveTreeSelectSuffixFlag SuffixFlag
        {
            get
            {
                return mSuffixFlag;
            }
            set
            {
                mSuffixFlag = value;
            }
        }

        public int Depth
        {
            get
            {
                return mDepth;
            }
            set
            {
                mDepth = value;
            }
        }

        public bool IsRoot
        {
            get
            {
                return mIsRoot;
            }
            set
            {
                mIsRoot = value;
            }
        }

        public bool IsSelectable
        {
            get
            {
                return mTreeSelectedFlag == AveTreeSelectFlag.CanNotSelect;
            }
        }

        public AveBackupTreeNode ParentNode
        {
            get
            {
                return mParent;
            }
            set
            {
                mParent = value;
            }
        }

        public List<AveBackupTreeNode> Children
        {
            get
            {
                return mChildren;
            }
        }

        public void AddChild(AveBackupTreeNode childNode)
        {
            mChildren.Add(childNode);
            childNode.ParentNode = this;
        }
    }
    
    //3,4 only exist in list level; 5,6 only exist in item level
    //5,6 only used to indentify it is a item
    public enum AveTreeSelectFlag
    {
        CanNotSelect = -1,
        NotSelect = 0,
        AllSelect = 1,
        ChildSelect = 2,
        HasItemSelect = 3,
        SelfAndHasItemSelect = 4,
        ParentNoSelect = 5,
        ParentSelect = 6,
        SelfSelect = 7
    }

    public enum AveTreeSelectSuffixFlag
    {
        NoSuffix= -1,
        NoSelectAllAndNoSelectSecurity = 0,
        NoSelectAllHasSelectSecurity = 1,
        HasSelectAllNoSelectSecurity = 2,
        HasSelectAllAndSelectSecurity = 3
    }
}
