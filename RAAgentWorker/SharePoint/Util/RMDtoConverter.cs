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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.Contract.Global.Object;
using System;
using System.Collections.Generic;

namespace RAFileSystem.SharePoint.Util
{
    public class RMDtoConverter
    {
        private static readonly IAveLogger logger = AveLogger.GetInstance(typeof(RMDtoConverter));
        public static RMSPTreeNode ConvertSPTree2RMTree(SPTreeNodeDto spTree, RMSPTreeNode rm = null)
        {
            if (rm == null)
            {
                rm = new RMSPTreeNode();
            }
            rm.Id = spTree.ID;
            rm.FarmId = spTree.FarmID;
            rm.FarmName = spTree.FarmName;
            rm.Name = spTree.Name;
            rm.Title = spTree.Title;
            rm.FullPath = spTree.FullPath;
            rm.Level = (int)spTree.Level;
            rm.NodeType = (int)spTree.Type;
            rm.SPObjectId = spTree.SPObjectId;
            rm.SPVersion = spTree.SPVersion;
            rm.SPType = (int)spTree.SPType;
            rm.Expanded = spTree.Expanded;
            rm.Hidden = spTree.Hidden;
            if (rm.Name == "{System Folder}")
            {
                rm.Hidden = true;
            }
            rm.ChildrenCount = spTree.ChildrenCount;
            rm.CheckNumber = spTree.CheckNumber;
            rm.TemplateId = spTree.Template;
            //rm.TeamName = spTree.TeamName;
            if (spTree.NodeExtension != null && spTree.NodeExtension.BposInfo != null)
            {
                rm.BposInfo = null;//spTree.NodeExtension.BposInfo;
            }
            if (spTree.Parent != null && rm.Parent == null)
            {
                RMSPTreeNode tempParent = new RMSPTreeNode();
                tempParent.Children = new List<RMSPTreeNode>() { rm };
                rm.Parent = ConvertSPTree2RMTree(spTree.Parent, tempParent);
            }
            if (spTree.Children != null && spTree.Children.Count > 0 &&
                (rm.Children == null || rm.Children.Count == 0))
            {
                rm.Children = new List<RMSPTreeNode>();
                foreach (SPTreeNodeDto child in spTree.Children)
                {
                    RMSPTreeNode temp = new RMSPTreeNode();
                    temp.Parent = rm;
                    RMSPTreeNode rmChild = ConvertSPTree2RMTree(child, temp);
                    rm.Children.Add(rmChild);
                }
            }
            return rm;
        }

        public static AvePoint.RA.Contract.Global.Object.RMSPTreeNode ConvertRMSPTreeNode2GlobalDto(RMSPTreeNode spTree, AvePoint.RA.Contract.Global.Object.RMSPTreeNode rm = null)
        {
            if (rm == null)
            {
                rm = new AvePoint.RA.Contract.Global.Object.RMSPTreeNode();
            }
            rm.Id = spTree.Id;
            rm.FarmId = spTree.FarmId;
            rm.FarmName = spTree.FarmName;
            rm.Name = spTree.Name;
            rm.Title = spTree.Title;
            rm.FullPath = spTree.FullPath;
            rm.Level = (int)spTree.Level;
            rm.NodeType = (int)spTree.NodeType;
            rm.SPObjectId = spTree.SPObjectId;
            rm.SPVersion = spTree.SPVersion;
            rm.SPType = (int)spTree.SPType;
            rm.Expanded = spTree.Expanded;
            rm.Hidden = spTree.Hidden;
            if (rm.Name == "{System Folder}")
            {
                rm.Hidden = true;
            }
            rm.ChildrenCount = spTree.ChildrenCount;
            rm.CheckNumber = spTree.CheckNumber;
            rm.TemplateId = spTree.TemplateId;
            rm.TeamName = spTree.TeamName;
            //if (spTree.BposInfo != null)
            //{
            //    rm.BposInfo = ConvertBpos2GlobalDto(spTree.BposInfo);
            //}
            if (spTree.Parent != null && rm.Parent == null)
            {
                AvePoint.RA.Contract.Global.Object.RMSPTreeNode tempParent = new AvePoint.RA.Contract.Global.Object.RMSPTreeNode();
                tempParent.Children = new List<AvePoint.RA.Contract.Global.Object.RMSPTreeNode>() { rm };
                rm.Parent = ConvertRMSPTreeNode2GlobalDto(spTree.Parent, tempParent);
            }
            if (spTree.Children != null && spTree.Children.Count > 0 &&
                (rm.Children == null || rm.Children.Count == 0))
            {
                rm.Children = new List<AvePoint.RA.Contract.Global.Object.RMSPTreeNode>();
                foreach (RMSPTreeNode child in spTree.Children)
                {
                    AvePoint.RA.Contract.Global.Object.RMSPTreeNode temp = new AvePoint.RA.Contract.Global.Object.RMSPTreeNode();
                    temp.Parent = rm;
                    AvePoint.RA.Contract.Global.Object.RMSPTreeNode rmChild = ConvertRMSPTreeNode2GlobalDto(child, temp);
                    rm.Children.Add(rmChild);
                }
            }
            return rm;
        }

        //public static AvePoint.RA.Contract.Global.Object.BposInfo ConvertBpos2GlobalDto(BposInfo bposInfo)
        //{
        //    AvePoint.RA.Contract.Global.Object.BposInfo info = new Contract.Global.Object.BposInfo()
        //    {
        //        UserAccountInfo = new Contract.Global.Object.BposUserAccountInfo()
        //        {
        //            Domain = bposInfo.UserAccountInfo.Domain,
        //            Username = bposInfo.UserAccountInfo.Username,
        //            Password = bposInfo.UserAccountInfo.Password
        //        },
        //        SiteUrl = bposInfo.SiteUrl
        //    };
        //    return info;
        //}

        public static SPTreeNodeDto ConvertRMTree2SPTree(RMSPTreeNode rmTree, SPTreeNodeDto sp = null)
        {
            if (sp == null)
            {
                sp = new SPTreeNodeDto();
            }
            sp.ID = rmTree.Id;
            sp.FarmID = rmTree.FarmId;
            sp.FarmName = rmTree.FarmName;
            sp.Name = rmTree.Name;
            sp.Title = rmTree.Title;
            sp.FullPath = rmTree.FullPath;
            sp.Url = rmTree.FullPath;
            sp.Level = (NodeLevel)rmTree.Level;
            sp.Type = (NodeType)rmTree.NodeType;
            sp.SPType = (SPType)rmTree.SPType;
            sp.SPObjectId = rmTree.SPObjectId;
            sp.SPVersion = rmTree.SPVersion;
            sp.Expanded = rmTree.Expanded;
            sp.ChildrenCount = rmTree.ChildrenCount;
            sp.CheckNumber = rmTree.CheckNumber;
            sp.Hidden = rmTree.Hidden;
            sp.Template = rmTree.TemplateId;
            if (sp.NodeExtension == null)
            {
                sp.NodeExtension = new NodeExtensionDto();
            }
            //sp.NodeExtension.BposInfo = rmTree.BposInfo;
            if (rmTree.Parent != null && sp.Parent == null)
            {
                SPTreeNodeDto tempParent = new SPTreeNodeDto();
                tempParent.Children = new List<SPTreeNodeDto> { sp };
                sp.Parent = ConvertRMTree2SPTree(rmTree.Parent, tempParent);
            }
            if (rmTree.Children != null && rmTree.Children.Count > 0 &&
                (sp.Children == null || sp.Children.Count == 0))
            {
                sp.Children = new List<SPTreeNodeDto>();
                foreach (RMSPTreeNode child in rmTree.Children)
                {
                    SPTreeNodeDto tempChild = new SPTreeNodeDto();
                    tempChild.Parent = sp;
                    sp.Children.Add(ConvertRMTree2SPTree(child, tempChild));
                }
            }
            return sp;
        }

        public static void ConvertSPTreeBeforeToJSON(SPTreeNodeDto currentNode)
        {
            RemoveParentChildrenNodes(currentNode);
            RemoveChildrenParentNodes(currentNode);
        }
        private static SPTreeNodeDto RemoveParentChildrenNodes(SPTreeNodeDto currentNode)
        {
            if (currentNode.Parent != null)
            {
                currentNode.Parent.ChildrenCount = 0;
                currentNode.Parent.Children = new List<SPTreeNodeDto>();
                return RemoveParentChildrenNodes(currentNode.Parent);
            }
            else
            {
                return null;
            }
        }

        private static SPTreeNodeDto RemoveChildrenParentNodes(SPTreeNodeDto currentNode)
        {
            if (currentNode.Children != null && currentNode.Children.Count > 0)
            {
                foreach (var c in currentNode.Children)
                {
                    c.Parent = null;
                    RemoveChildrenParentNodes(c);
                }
            }
            return null;
        }
    }

}
