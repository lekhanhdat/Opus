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
using AvePoint.RA.Contract.RMWeb.Tree;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeTest
{
    class Program
    {
        static void Main(string[] args)
        {
            RMPhysicalExplorerNode node = NewNode("1", 1, 3);
            var nodeText = JsonConvert.SerializeObject(node);
            

        }

        private static RMPhysicalExplorerNode NewNode(string id, int type, int maxType)
        {
            RMPhysicalExplorerNode node = new RMPhysicalExplorerNode();
            node.Id = id;
            node.Name = $"node{node.Id}";
            node.NodeType = type;
            node.Children = new List<RMPhysicalExplorerNode>();
            node.ChildrenCount = node.Children.Count;
            node.OtherProp = Guid.NewGuid().ToString();
            if(type < maxType)
            {
                AppendChildren(node, type, maxType);
            }
            
            return node;
        }

        private static void AppendChildren(RMPhysicalExplorerNode node, int parentType, int maxType)
        {
            int type = parentType + 1;
            for (int i = 1; i <= 5; i++)
            {
                var child = NewNode($"{node.Id}_{i}", type, maxType);
                child.ParentId = node.Id;
                node.Children.Add(child);
            }
        }
    }
}
