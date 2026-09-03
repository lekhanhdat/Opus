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
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.Web.Common
{
    public class SPTreeUtil
    {
        private static RALogger mLogger = RALogger.GetInstance(typeof(SPTreeUtil));

        //public static ISPSettingTreeService mSPSettingTreeService { get; set; }

        //public static RMSPTreeNode GetFarmNode()
        //{
        //    RMSPTreeNode farmNode = null;
        //    try
        //    {
        //        farmNode = SPTreeCacheUtil.GetNodeById(SPTreeCacheUtil.FarmNodeKey, RAModule.Common);
        //        if (farmNode == null)
        //        {
        //            //加载SharePoint tree farm节点并缓存该节点
        //            farmNode = mSPSettingTreeService.LoadFarm()[0];
        //            SPTreeCacheUtil.CacheNode(farmNode, RAModule.Common);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        mLogger.Error("Failed to get farm node.ERROR:{0}",e.ToString());
        //    }
        //    return farmNode;
        //}
        public static string BuildSPTreeXMLStr(string arrayStr, string farmId = "")
        {
            try
            {
                List<RMSPTreeNode> treeNodes = SerializerHelper.DeserializeByJsonConvert<List<RMSPTreeNode>>(arrayStr);

                //build tree结构
                Dictionary<string, RMSPTreeNode> nodesDic = GetNodesDic(treeNodes);
                AddParentForSPTreeNode(nodesDic);
                var farmNode = GetFarmNode(treeNodes[0]);
                //if (farmNode.FarmId != farmId)
                //{
                //    mLogger.Error("Failed to build EXO tree xml string farm id is not incorrect,FarmId From DA:{0}, FarmId From Profile:{1}", farmId, farmNode.FarmId);
                //    throw new Exception("farm id NOT incorrect");
                //}
                BuildTree(farmNode, nodesDic);

                return SerializerHelper.SerializeByDataContractSerializer(farmNode);
            }
            catch (Exception e)
            {
                mLogger.Error("Failed to build SP tree xml string.ERROR:{0}", e.ToString());
                throw;
            }
        }

        public static string BuildSPTreeJsonStr(List<RMSPTreeNode> arrayStr, string farmId = "")
        {
            try
            {
                //build tree结构
                Dictionary<string, RMSPTreeNode> nodesDic = GetNodesDic(arrayStr);
                AddParentForSPTreeNode(nodesDic);
                var farmNode = GetFarmNode(arrayStr[0]);
                //if (farmNode.FarmId != farmId)
                //{
                //    mLogger.Error("Failed to build EXO tree xml string farm id is not incorrect,FarmId From DA:{0}, FarmId From Profile:{1}", farmId, farmNode.FarmId);
                //    throw new Exception("farm id NOT incorrect");
                //}
                BuildTree(farmNode, nodesDic);

                return SerializerHelper.SerializeByJsonSerializer(farmNode);
            }
            catch (Exception e)
            {
                mLogger.Error("Failed to build SP tree xml string.ERROR:{0}", e.ToString());
                throw;
            }
        }

        public static RMSPTreeNode BuildSPTree(List<RMSPTreeNode> arrayStr, string farmId = "")
        {
            try
            {
                //build tree结构
                Dictionary<string, RMSPTreeNode> nodesDic = GetNodesDic(arrayStr);
                AddParentForSPTreeNode(nodesDic);
                var farmNode = GetFarmNode(arrayStr[0]);
                BuildTree(farmNode, nodesDic);

                return farmNode;
            }
            catch (Exception e)
            {
                mLogger.Error("Failed to build SP tree xml string.ERROR:{0}", e.ToString());
                throw;
            }
        }

        public static string BuildEXOTreeXMLStr(string arrayStr, string rootId = "")
        {
            try
            {
                List<RMEXOTreeNode> treeNodes = SerializerHelper.DeserializeByJsonConvert<List<RMEXOTreeNode>>(arrayStr);

                //build tree结构
                Dictionary<string, RMEXOTreeNode> nodesDic = GetNodesDicForEXO(treeNodes);
                AddParentForEXOTreeNode(nodesDic);
                var farmNode = GetFarmNodeForEXO(treeNodes[0]);
                //if (farmNode.Id != rootId)
                //{
                //    mLogger.Error("Failed to build SP tree xml string farm id is not incorrect,FarmId From DA:{0}, FarmId From Profile:{1}", rootId, farmNode.Id);
                //    throw new Exception("root id NOT incorrect");
                //}
                BuildTreeForEXO(farmNode, nodesDic);

                return SerializerHelper.SerializeByDataContractSerializer(farmNode);
            }
            catch (Exception e)
            {
                mLogger.Error("Failed to build SP tree xml string.ERROR:{0}", e.ToString());
                throw;
            }
        }
        public static string BuildFSTreeXMLStr(string arrayStr, Guid farmId = default)
        {
            try
            {
                List<RMFSTreeNode> treeNodes = SerializerHelper.DeserializeByJsonSerializer<List<RMFSTreeNode>>(arrayStr);

                //build tree结构
                Dictionary<string, RMFSTreeNode> nodesDic = GetNodesDicForFS(treeNodes);
                AddParentForFSTreeNode(nodesDic);
                var farmNode = GetFarmNodeForFS(treeNodes[0]);

                //if (farmId != Guid.Empty && farmId != farmNode.Id)
                //{
                //    mLogger.Error("Failed to build FS tree xml string farm id is not incorrect,FarmId From DA:{0}, Root ID From Profile:{1}", farmId, farmNode.FarmID);
                //    throw new Exception("root id NOT incorrect");
                //}

                BuildTreeForFS(farmNode, nodesDic);

                return SerializerHelper.SerializeByDataContractSerializer(farmNode);
            }
            catch (Exception e)
            {
                mLogger.Error("Failed to build SP tree xml string.ERROR:{0}", e.ToString());
                throw;
            }
        }

        public static string ConvertXmlStrToSPTreeJsonStr(string xmlStr)
        {
            try
            {
                RMSPTreeNode farmNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(xmlStr);
                List<RMSPTreeNode> nodes = CacheNodeAndRemoveProperties(farmNode);
                return JsonConvert.SerializeObject(nodes);
            }
            catch (Exception e)
            {
                mLogger.Error("Failed to convert xml string to json string.ERROR:{0}", e.ToString());
                throw;
            }
        }

        public static List<RMSPTreeNode> ConvertTreeStrToSPTreeJsonStr(string treeStr)
        {
            try
            {
                RMSPTreeNode farmNode = SerializerHelper.DeserializeByJsonSerializer<RMSPTreeNode>(treeStr, true);
                List<RMSPTreeNode> nodes = CacheNodeAndRemoveProperties(farmNode);
                return nodes;
            }
            catch (Exception e)
            {
                mLogger.Error("Failed to convert xml string to json string.ERROR:{0}", e.ToString());
                throw;
            }
        }

        public static string ConvertXmlStrToEXOTreeJsonStr(string xmlStr)
        {
            try
            {
                RMEXOTreeNode farmNode = SerializerHelper.DeserializeByDataContractSerializer<RMEXOTreeNode>(xmlStr);
                List<RMEXOTreeNode> nodes = CacheNodeAndRemovePropertiesForEXO(farmNode);
                return JsonConvert.SerializeObject(nodes);
            }
            catch (Exception e)
            {
                mLogger.Error("Failed to convert xml string to json string.ERROR:{0}", e.ToString());
                throw;
            }
        }

        public static string ConvertXmlStrToFSTreeJsonStr(string xmlStr)
        {
            try
            {
                RMFSTreeNode farmNode = SerializerHelper.DeserializeByDataContractSerializer<RMFSTreeNode>(xmlStr);
                List<RMFSTreeNode> nodes = CacheNodeAndRemovePropertiesForFS(farmNode);
                return SerializerHelper.SerializeByJsonSerializer(nodes);
            }
            catch (Exception e)
            {
                mLogger.Error("Failed to convert xml string to json string.ERROR:{0}", e.ToString());
                throw;
            }
        }

        private static List<RMSPTreeNode> CacheNodeAndRemoveProperties(RMSPTreeNode farmNode)
        {
            List<RMSPTreeNode> nodes = new List<RMSPTreeNode>();
            GetNodesList(farmNode, nodes);

            List<RMSPTreeNode> resultNodes = new List<RMSPTreeNode>();
            foreach (var node in nodes)
            {
                if (node.Children != null)
                {
                    //删除Children属性，避免以后convert to SPTree时出现死循环
                    node.Children = null;
                }
                //缓存节点
                //SPTreeCacheUtil.CacheNode(node, RAModule.Common);
                //删除不必要属性，减少序列化以及通信的size
                //var newNode = node.Clone();
                //if (newNode.Parent != null)
                //{
                //    newNode.ParentId = newNode.Parent.Id;
                //    newNode.Parent = null;
                //}
                //newNode.Children = null;
                if (node.Parent != null)
                {
                    node.ParentId = node.Parent.Id;
                    node.Parent = null;
                }
                resultNodes.Add(node);
            }
            return resultNodes;
        }

        private static List<RMEXOTreeNode> CacheNodeAndRemovePropertiesForEXO(RMEXOTreeNode farmNode)
        {
            List<RMEXOTreeNode> nodes = new List<RMEXOTreeNode>();
            GetNodesListForEXO(farmNode, nodes);

            List<RMEXOTreeNode> resultNodes = new List<RMEXOTreeNode>();
            foreach (var node in nodes)
            {
                if(string.IsNullOrEmpty(node.DisplayName))
                {
                    node.DisplayName = node.Name;
                }
                if (node.Children != null)
                {
                    node.Children = null;
                }
                resultNodes.Add(node);
            }
            return resultNodes;
        }

        private static List<RMFSTreeNode> CacheNodeAndRemovePropertiesForFS(RMFSTreeNode farmNode)
        {
            List<RMFSTreeNode> nodes = new List<RMFSTreeNode>();
            GetNodesListForFS(farmNode, nodes);

            List<RMFSTreeNode> resultNodes = new List<RMFSTreeNode>();
            foreach (var node in nodes)
            {
                if (node.Children != null)
                {
                    node.Children = null;
                }
                resultNodes.Add(node);
            }
            return resultNodes;
        }
        private static void GetNodesList(RMSPTreeNode node, List<RMSPTreeNode> nodesList)
        {
            nodesList.Add(node);
            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    GetNodesList(child, nodesList);
                }
            }
        }

        private static void GetNodesListForEXO(RMEXOTreeNode node, List<RMEXOTreeNode> nodesList)
        {
            nodesList.Add(node);
            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    GetNodesListForEXO(child, nodesList);
                }
            }
        }

        private static void GetNodesListForFS(RMFSTreeNode node, List<RMFSTreeNode> nodesList)
        {
            nodesList.Add(node);
            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    GetNodesListForFS(child, nodesList);
                }
            }
        }

        private static Dictionary<string, RMSPTreeNode> GetNodesDic(List<RMSPTreeNode> nodesList)
        {
            Dictionary<string, RMSPTreeNode> nodesDic = new Dictionary<string, RMSPTreeNode>();
            foreach (var node in nodesList)
            {
                if (!nodesDic.Keys.Contains(node.Id))
                {
                    nodesDic.Add(node.Id, node);
                }
            }
            return nodesDic;
        }

        private static Dictionary<string, RMEXOTreeNode> GetNodesDicForEXO(List<RMEXOTreeNode> nodesList)
        {
            Dictionary<string, RMEXOTreeNode> nodesDic = new Dictionary<string, RMEXOTreeNode>();
            foreach (var node in nodesList)
            {
                if (!nodesDic.Keys.Contains(node.Id.ToString()))
                {
                    nodesDic.Add(node.Id.ToString(), node);
                }
            }
            return nodesDic;
        }

        private static Dictionary<string, RMFSTreeNode> GetNodesDicForFS(List<RMFSTreeNode> nodesList)
        {
            Dictionary<string, RMFSTreeNode> nodesDic = new Dictionary<string, RMFSTreeNode>();
            foreach (var node in nodesList)
            {
                if (!nodesDic.Keys.Contains(node.Id.ToString()))
                {
                    nodesDic.Add(node.Id.ToString(), node);
                }
            }
            return nodesDic;
        }

        private static void AddParentForSPTreeNode(Dictionary<string, RMSPTreeNode> nodesDic)
        {
            foreach (KeyValuePair<string, RMSPTreeNode> pair in nodesDic)
            {
                var tempNode = pair.Value;
                if (!string.IsNullOrEmpty(tempNode.ParentId))
                {
                    tempNode.Parent = nodesDic[tempNode.ParentId];
                }
            }
        }

        private static void AddParentForEXOTreeNode(Dictionary<string, RMEXOTreeNode> nodesDic)
        {
            foreach (KeyValuePair<string, RMEXOTreeNode> pair in nodesDic)
            {
                var tempNode = pair.Value;
                if (!string.IsNullOrEmpty(tempNode.ParentId))
                {
                    tempNode.Parent = nodesDic[tempNode.ParentId];
                }
            }
        }

        private static void AddParentForFSTreeNode(Dictionary<string, RMFSTreeNode> nodesDic)
        {
            foreach (KeyValuePair<string, RMFSTreeNode> pair in nodesDic)
            {
                var tempNode = pair.Value;
                if (!string.IsNullOrEmpty(tempNode.ParentId))
                {
                    tempNode.Parent = nodesDic[tempNode.ParentId];
                }
            }
        }

        private static RMSPTreeNode GetFarmNode(RMSPTreeNode node)
        {
            if (node != null)
            {
                if (node.Level != -1)
                {
                    return GetFarmNode(node.Parent);
                }
                else
                {
                    return node;
                }
            }
            else
            {
                return node;
            }
        }

        private static RMEXOTreeNode GetFarmNodeForEXO(RMEXOTreeNode node)
        {
            if (node != null)
            {
                if (node.Level != 5000)
                {
                    return GetFarmNodeForEXO(node.Parent);
                }
                else
                {
                    return node;
                }
            }
            else
            {
                return node;
            }
        }

        private static RMFSTreeNode GetFarmNodeForFS(RMFSTreeNode node)
        {
            if (node != null)
            {
                if (node.Level != -1)
                {
                    return GetFarmNodeForFS(node.Parent);
                }
                else
                {
                    return node;
                }
            }
            else
            {
                return node;
            }
        }

        private static void BuildTree(RMSPTreeNode node, Dictionary<string, RMSPTreeNode> nodesDic)
        {
            if (node.ChildrenIds != null)
            {
                foreach (var id in node.ChildrenIds)
                {
                    if (node.Children == null)
                    {
                        node.Children = new List<RMSPTreeNode>();
                    }
                    if (nodesDic.ContainsKey(id))
                    {
                        node.Children.Add(nodesDic[id]);
                    }
                    else
                    {
                        mLogger.Warn("Node does not exist, id:[{0}]", id);
                    }
                }
                if (node.Children != null)
                {
                    foreach (var child in node.Children)
                    {
                        BuildTree(child, nodesDic);
                    }
                }
            }
        }

        private static void BuildTreeForEXO(RMEXOTreeNode node, Dictionary<string, RMEXOTreeNode> nodesDic)
        {
            if (node.ChildrenIds != null)
            {
                foreach (var id in node.ChildrenIds)
                {
                    if (node.Children == null)
                    {
                        node.Children = new List<RMEXOTreeNode>();
                    }
                    if (nodesDic.ContainsKey(id))
                    {
                        node.Children.Add(nodesDic[id]);
                    }
                    else
                    {
                        mLogger.Warn("Node does not exist, id:[{0}]", id);
                    }
                }
                if (node.Children != null)
                {
                    foreach (var child in node.Children)
                    {
                        BuildTreeForEXO(child, nodesDic);
                    }
                }
            }
        }

        private static void BuildTreeForFS(RMFSTreeNode node, Dictionary<string, RMFSTreeNode> nodesDic)
        {
            if (node.ChildrenIds != null)
            {
                foreach (var id in node.ChildrenIds)
                {
                    if (node.Children == null)
                    {
                        node.Children = new List<RMFSTreeNode>();
                    }
                    if (nodesDic.ContainsKey(id))
                    {
                        node.Children.Add(nodesDic[id]);
                    }
                    else
                    {
                        mLogger.Warn("Node does not exist, id:[{0}]", id);
                    }
                }
                if (node.Children != null)
                {
                    foreach (var child in node.Children)
                    {
                        BuildTreeForFS(child, nodesDic);
                    }
                }
            }
        }
    }
}