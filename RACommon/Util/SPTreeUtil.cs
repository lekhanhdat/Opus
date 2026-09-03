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
using AvePoint.GCommon.Contract.StorageOptimization.Connector;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.Box;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using Google.Cloud.AIPlatform.V1;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Graph.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Util
{
    public class RuleSPTreeUtil
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(RuleSPTreeUtil));

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
        public static string BuildSPTreeXMLStr(string arrayStr)
        {
            try
            {
                List<RMSPTreeNode> treeNodes = SerializerHelper.DeserializeByJsonConvert<List<RMSPTreeNode>>(arrayStr);

                //build tree结构
                Dictionary<string, RMSPTreeNode> nodesDic = GetNodesDic(treeNodes);
                AddParentForSPTreeNode(nodesDic);
                var farmNode = GetFarmNode(treeNodes[0]);
                BuildTree(farmNode, nodesDic);

                return SerializerHelper.SerializeByDataContractSerializer(farmNode);
            }
            catch (Exception e)
            {
                mLogger.Error("Failed to build SP tree xml string.ERROR:{0}", e.ToString());
                throw;
            }
        }

        public static string BuildFSTreeXMLStr(string arrayStr)
        {
            try
            {
                List<RMFSTreeNode> treeNodes = SerializerHelper.DeserializeByJsonConvert<List<RMFSTreeNode>>(arrayStr);

                //build tree结构
                Dictionary<string, RMFSTreeNode> nodesDic = GetNodesDicForFS(treeNodes);
                AddParentForFSTreeNode(nodesDic);
                var farmNode = GetFarmNodeForFS(treeNodes[0]);
                BuildTreeForFS(farmNode, nodesDic);

                return SerializerHelper.SerializeByDataContractSerializer(farmNode);
            }
            catch (Exception e)
            {
                mLogger.Error("Failed to build SP tree xml string.ERROR:{0}", e.ToString());
                throw;
            }
        }

        public static string BuildFSTreeJsonStr(string arrayStr)
        {
            try
            {
                List<RMFSTreeNode> treeNodes = SerializerHelper.DeserializeByJsonConvert<List<RMFSTreeNode>>(arrayStr);

                //build tree结构
                Dictionary<string, RMFSTreeNode> nodesDic = GetNodesDicForFS(treeNodes);
                AddParentForFSTreeNode(nodesDic);
                var farmNode = GetFarmNodeForFS(treeNodes[0]);
                BuildTreeForFS(farmNode, nodesDic);

                return SerializerHelper.SerializeByJsonSerializer(farmNode);
            }
            catch (Exception e)
            {
                mLogger.Error("Failed to build FS tree Json string.ERROR:{0}", e.ToString());
                throw;
            }
        }

        public static string BuildBoxTreeJsonStr(string arrayStr)
        {
            try
            {
                List<BoxTreeNode> treeNodes = SerializerHelper.DeserializeByJsonConvert<List<BoxTreeNode>>(arrayStr);

                //build tree结构
                Dictionary<string, BoxTreeNode> nodesDic = GetNodesDicForBox(treeNodes);
                AddParentForBoxTreeNode(nodesDic);
                var farmNode = GetFarmNodeForBox(treeNodes[0]);
                BuildTreeForBox(farmNode, nodesDic);

                return SerializerHelper.SerializeByJsonSerializer(farmNode);
            }
            catch (Exception e)
            {
                mLogger.Error("Failed to build FS tree Json string.ERROR:{0}", e.ToString());
                throw;
            }
        }

        public static string BuildGoogleTreeJsonStr(string arrayStr)
        {
            try
            {
                List<RMGoogleTreeNode> treeNodes = SerializerHelper.DeserializeByJsonConvert<List<RMGoogleTreeNode>>(arrayStr);

                //build tree结构
                Dictionary<string, RMGoogleTreeNode> nodesDic = GetNodesDicForGoogle(treeNodes);
                AddParentForGoogleTreeNode(nodesDic);
                var farmNode = GetFarmNodeForGoogle(treeNodes[0]);
                BuildTreeForGoogle(farmNode, nodesDic);

                return SerializerHelper.SerializeByJsonSerializer(farmNode);
            }
            catch (Exception e)
            {
                mLogger.Error("Failed to build FS tree Json string.ERROR:{0}", e.ToString());
                throw;
            }
        }

        public static string BuildGoogleTreeXmlStr(string arrayStr)
        {
            try
            {
                List<RMGoogleTreeNode> treeNodes = SerializerHelper.DeserializeByJsonConvert<List<RMGoogleTreeNode>>(arrayStr);

                //build tree结构
                Dictionary<string, RMGoogleTreeNode> nodesDic = GetNodesDicForGoogle(treeNodes);
                AddParentForGoogleTreeNode(nodesDic);
                var farmNode = GetFarmNodeForGoogle(treeNodes[0]);
                BuildTreeForGoogle(farmNode, nodesDic);

                return SerializerHelper.SerializeByDataContractSerializer(farmNode);
            }
            catch (Exception e)
            {
                mLogger.Error("Failed to build FS tree Json string.ERROR:{0}", e.ToString());
                throw;
            }
        }

        public static string BuildFSTreeXMLStrFromFarmNode(string arrayStr)
        {
            try
            {
                var farmNode = SerializerHelper.DeserializeByJsonSerializer<RMFSTreeNode>(arrayStr);
                List<RMFSTreeNode> treeNodes = CacheNodeAndRemovePropertiesForFS(farmNode);

                //build tree结构
                Dictionary<string, RMFSTreeNode> nodesDic = GetNodesDicForFS(treeNodes);
                AddParentForFSTreeNode(nodesDic);
               
                BuildTreeForFS(farmNode, nodesDic);

                return SerializerHelper.SerializeByDataContractSerializer(farmNode);
            }
            catch (Exception e)
            {
                mLogger.Error("Failed to Build FS Tree XML string From Farm Node.ERROR:{0}", e.ToString());
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

        public static string ConvertXmlStrToFSTreeJsonStr(string xmlStr)
        {
            try
            {
                RMFSTreeNode farmNode = SerializerHelper.DeserializeByDataContractSerializer<RMFSTreeNode>(xmlStr);
                List<RMFSTreeNode> nodes = CacheNodeAndRemovePropertiesForFS(farmNode);
                return JsonConvert.SerializeObject(nodes);
            }
            catch (Exception e)
            {
                mLogger.Error("Failed to convert xml string to FS Tree json string.ERROR:{0}", e.ToString());
                throw;
            }
        }
        public static string ConvertFSTreeJsonStrToListStr(string treeJson)
        {
            try
            {
                RMFSTreeNode farmNode = SerializerHelper.DeserializeByJsonSerializer<RMFSTreeNode>(treeJson);
                List<RMFSTreeNode> nodes = CacheNodeAndRemovePropertiesForFS(farmNode);
                return SerializerHelper.SerializeByJsonConvert(nodes);
            }
            catch (Exception e)
            {
                mLogger.Error("Failed to Convert FS Tree Json string To List string.ERROR:{0}", e.ToString());
                throw;
            }
        }

        public static string ConvertXmlStrToFSTreeStr(string xmlStr)
        {
            try
            {
                RMFSTreeNode farmNode = SerializerHelper.DeserializeByDataContractSerializer<RMFSTreeNode>(xmlStr);
                return JsonConvert.SerializeObject(farmNode);
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

        private static List<BoxTreeNode> CacheNodeAndRemovePropertiesForBox(BoxTreeNode farmNode)
        {
            List<BoxTreeNode> nodes = new List<BoxTreeNode>();
            GetNodesListForBox(farmNode, nodes);

            List<BoxTreeNode> resultNodes = new List<BoxTreeNode>();
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

        private static List<RMGoogleTreeNode> CacheNodeAndRemovePropertiesForGoogle(RMGoogleTreeNode farmNode)
        {
            List<RMGoogleTreeNode> nodes = new List<RMGoogleTreeNode>();
            GetNodesListForGoogle(farmNode, nodes);

            List<RMGoogleTreeNode> resultNodes = new List<RMGoogleTreeNode>();
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

        private static void GetNodesListForBox(BoxTreeNode node, List<BoxTreeNode> nodesList)
        {
            nodesList.Add(node);
            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    GetNodesListForBox(child, nodesList);
                }
            }
        }

        private static void GetNodesListForGoogle(RMGoogleTreeNode node, List<RMGoogleTreeNode> nodesList)
        {
            nodesList.Add(node);
            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    GetNodesListForGoogle(child, nodesList);
                }
            }
        }

        public static string ConvertXmlStrToBoxTreeStr(string xmlStr)
        {
            try
            {
                BoxTreeNode farmNode = SerializerHelper.DeserializeByDataContractSerializer<BoxTreeNode>(xmlStr);
                return JsonConvert.SerializeObject(farmNode);
            }
            catch (Exception e)
            {
                mLogger.Error("Failed to convert xml string to json string.ERROR:{0}", e.ToString());
                throw;
            }
        }

        public static string ConvertXmlStrToGoogleTreeStr(string xmlStr)
        {
            try
            {
                RMGoogleTreeNode farmNode = SerializerHelper.DeserializeByDataContractSerializer<RMGoogleTreeNode>(xmlStr);
                return JsonConvert.SerializeObject(farmNode);
            }
            catch (Exception e)
            {
                mLogger.Error("Failed to convert xml string to json string.ERROR:{0}", e.ToString());
                throw;
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

        private static Dictionary<string, BoxTreeNode> GetNodesDicForBox(List<BoxTreeNode> nodesList)
        {
            Dictionary<string, BoxTreeNode> nodesDic = new Dictionary<string, BoxTreeNode>();
            foreach (var node in nodesList)
            {
                if (node.Id != null && !nodesDic.Keys.Contains(node.Id.ToString()))
                {
                    nodesDic.Add(node.Id.ToString(), node);
                }
            }
            return nodesDic;
        }

        private static void AddParentForBoxTreeNode(Dictionary<string, BoxTreeNode> nodesDic)
        {
            foreach (KeyValuePair<string, BoxTreeNode> pair in nodesDic)
            {
                var tempNode = pair.Value;
                if (tempNode.Parent != null && !string.IsNullOrEmpty(tempNode.Parent.Id))
                {
                    tempNode.Parent = nodesDic[tempNode.Parent.Id];
                }
            }
        }

        private static Dictionary<string, RMGoogleTreeNode> GetNodesDicForGoogle(List<RMGoogleTreeNode> nodesList)
        {
            Dictionary<string, RMGoogleTreeNode> nodesDic = new Dictionary<string, RMGoogleTreeNode>();
            foreach (var node in nodesList)
            {
                if (!nodesDic.Keys.Contains(node.Id))
                {
                    nodesDic.Add(node.Id.ToString(), node);
                }
            }
            return nodesDic;
        }

        private static void AddParentForGoogleTreeNode(Dictionary<string, RMGoogleTreeNode> nodesDic)
        {
            foreach (KeyValuePair<string, RMGoogleTreeNode> pair in nodesDic)
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

        /// <summary>
        /// Get Container(web application/group)
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static RMSPTreeNode GetContainerNode(RMSPTreeNode node)
        {
            if (node != null)
            {
                if (node.Level != (int)GCommon.Contract.Tree.Object.NodeLevel.WebApplication)
                {
                    return GetContainerNode(node.Parent);
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

        public static string ConvertBoxTreeJsonStrToListStr(string treeJson)
        {
            try
            {
                BoxTreeNode farmNode = SerializerHelper.DeserializeByJsonSerializer<BoxTreeNode>(treeJson);
                List<BoxTreeNode> nodes = CacheNodeAndRemovePropertiesForBox(farmNode);
                return SerializerHelper.SerializeByJsonConvert(nodes);
            }
            catch (Exception e)
            {
                mLogger.Error("Failed to Convert FS Tree Json string To List string.ERROR:{0}", e.ToString());
                throw;
            }
        }

        private static BoxTreeNode GetFarmNodeForBox(BoxTreeNode node)
        {
            if (node != null)
            {
                if ((int)node.Level != -2)
                {
                    return GetFarmNodeForBox(node.Parent);
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

        public static string ConvertGoogleTreeJsonStrToListStr(string treeJson)
        {
            try
            {
                RMGoogleTreeNode farmNode = SerializerHelper.DeserializeByJsonSerializer<RMGoogleTreeNode>(treeJson);
                List<RMGoogleTreeNode> nodes = CacheNodeAndRemovePropertiesForGoogle(farmNode);
                return JsonConvert.SerializeObject(nodes);
            }
            catch (Exception e)
            {
                mLogger.Error("Failed to Convert FS Tree Json string To List string.ERROR:{0}", e.ToString());
                throw;
            }
        }

        public static string ConvertXmlStrToGoogleTreeJsonStr(string treeXml)
        {
            try
            {
                RMGoogleTreeNode farmNode =
                    SerializerHelper.DeserializeByDataContractSerializer<RMGoogleTreeNode>(treeXml);
                List<RMGoogleTreeNode> nodes = CacheNodeAndRemovePropertiesForGoogle(farmNode);
                return JsonConvert.SerializeObject(nodes);
            }
            catch (Exception e)
            {
                mLogger.Error("Failed to Convert Google Tree Json string To List string.ERROR:{0}", e.ToString());
                throw;
            }
        }

        private static RMGoogleTreeNode GetFarmNodeForGoogle(RMGoogleTreeNode node)
        {
            if (node != null)
            {
                if ((int)node.Level != -2)
                {
                    return GetFarmNodeForGoogle(node.Parent);
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

        private static void BuildTreeForBox(BoxTreeNode node, Dictionary<string, BoxTreeNode> nodesDic)
        {
            if (node.ChildrenIds != null)
            {
                foreach (var id in node.ChildrenIds)
                {
                    if (node.Children == null)
                    {
                        node.Children = new List<BoxTreeNode>();
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
                        BuildTreeForBox(child, nodesDic);
                    }
                }
            }
        }

        public static List<RMSPTreeNode> FilterSCAvailableNodeByRunningUrl(List<RMSPTreeNode> availableNode, List<string> runningUrl, RMSPTreeNode selectNode,string folderFullPath = "")
        {
            List<RMSPTreeNode> result = availableNode.ToList();
            string fullPathForConflictCheck = string.IsNullOrEmpty(folderFullPath) ? selectNode.FullPath : folderFullPath;
            if (selectNode.Level > (int)NodeLevel.SiteCollection && availableNode.Count() == 1)
            {
                if (runningUrl.Any(url => IsPrefixWithSlash(url, fullPathForConflictCheck) || IsPrefixWithSlash(fullPathForConflictCheck, url)))
                {
                    result.Clear();
                }
                return result;
            }

            foreach (var url in runningUrl)
            {
                foreach (var node in availableNode.OrderByDescending(node => node.FullPath.Length))
                {
                    string fullPath = node.FullPath;
                    if (IsPrefixWithSlash(url, fullPath) || IsPrefixWithSlash(fullPath, url))
                    {
                        result.Remove(node);
                    }
                }
            }
            
            return result;
        }

        public static List<RMSPTreeNode> FilterTeamsAvailableNodeByRunningUrl(List<RMSPTreeNode> availableNode, Dictionary<string, List<string>> runningUrl, RMSPTreeNode selectedNode)
        {
            List<RMSPTreeNode> result = new List<RMSPTreeNode>();

            foreach (var node in availableNode)
            {
                var teamNode = node.GetTeamsNode();
                var teamName = teamNode?.Name;
                var teamRunningScopes = !string.IsNullOrWhiteSpace(teamName)
                    && runningUrl.TryGetValue(teamName, out var scopes)
                    ? scopes ?? new List<string>()
                    : null;

                if (node.Level == (int)NodeLevel.Office365GroupEntire && teamRunningScopes != null)
                {
                    mLogger.Info($"Teams/Group {node.FullPath} is running in another job. Skip it in this job of group {selectedNode.FullPath}");
                    continue;
                }

                string fullPath = node.Level == (int)NodeLevel.Folder && !node.FullPath.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                    ? node.FullUrl
                    : node.FullPath;
                if (teamRunningScopes != null && teamRunningScopes.Count == 0)
                {
                    continue;
                }

                var runningSiteScopes = runningUrl.Values
                    .SelectMany(urls => urls ?? new List<string>())
                    .Where(scope => !string.IsNullOrWhiteSpace(scope));
                if (runningSiteScopes.Any(scope => IsPrefixWithSlash(scope, fullPath) || IsPrefixWithSlash(fullPath, scope)))
                {
                    continue;
                }
                result.Add(node);
            }
            return result;
        }

        public static List<RMSPSampleTreeNode> FilterTeamsAvailableNodeByRunningUrl(List<RMSPSampleTreeNode> availableNode, Dictionary<string, List<string>> runningUrl)
        {
            List<RMSPSampleTreeNode> result = new List<RMSPSampleTreeNode>();
            List<string> runningNames = runningUrl.Keys.ToList();
            runningNames.AddRange(runningUrl.Values.SelectMany(urls => urls));
            runningNames = runningNames.Where(name => !string.IsNullOrWhiteSpace(name)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            foreach (var node in availableNode)
            {
                string fullPath = node.FullPath;
                if (runningNames.Any(name => IsPrefixWithSlash(name, fullPath) || IsPrefixWithSlash(fullPath, name)))
                {
                    continue;
                }
                result.Add(node);
            }
            return result;
        }


        public static bool IsPrefixWithSlash(string prefix, string path)
        {
            prefix = prefix.TrimEnd('/') + '/';
            path = path.TrimEnd('/') + '/';
            return path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }
        public static string GenerateArchiveJobMonitorExtension(RMSPTreeNode selectNode, TreeMode treeMode, List<string> siteUrls = null,bool useImportSite = false, List<string> processNodeUrls = null)
        {
            ArchiveJobMonitorExtension extension = new ArchiveJobMonitorExtension();
            extension.treeMode = treeMode;
            if (useImportSite && siteUrls!=null && siteUrls.Count>0)
            {
                mLogger.Info("this archive job is use import site to run,just add siteurl as confilct");
                extension.SiteUrls = siteUrls;
                extension.ConflictNodeLevel = ConflictNodeLevel.SiteCollection;
            }
            else if (selectNode.Level == (int)NodeLevel.WebApplication)
            {
                extension.IsGroupLevelArchive = true;
                extension.GroupNode = selectNode;
                extension.ConflictNodeLevel = ConflictNodeLevel.Group;
            }
            else
            {
                extension.IsGroupLevelArchive = false;
                if (siteUrls != null)
                {
                    extension.SiteUrls = siteUrls;
                }
                else
                {
                    extension.SiteUrls = new List<string>() { selectNode.GetSiteCollectionNode()?.FullPath };
                }
                extension.ConflictNodeLevel = ConflictNodeLevel.SiteCollection;
            }
            extension.ProcessNodeUrls = processNodeUrls;
            
            return SerializerHelper.SerializeByDataContractSerializer(extension);
        }
        
        public static ConflictNodeLevel CheckNodeConflictLevel(RMSPTreeNode selectNode, bool useImportSite = false)
        {
            if (useImportSite)
            {
                mLogger.Info("this archive job is use import site to run,just add siteurl as confilct");
                return ConflictNodeLevel.ArchiverImportTeams;
            }
            else
            {
                switch (selectNode.Level)
                {
                    case (int)NodeLevel.WebApplication:
                        {
                            return ConflictNodeLevel.Group;
                        }
                    case (int)NodeLevel.Office365GroupEntire:
                        {
                            return ConflictNodeLevel.Teams;
                        }
                    default:
                        {
                            return ConflictNodeLevel.SiteCollection;
                        }
                }
            }
        }

        public static Dictionary<string, List<string>> BuildSearchFilter(RMSPTreeNode selectNode, List<RMSPTreeNode> avaliableNodes)
        {
            Dictionary<string, List<string>> res = new Dictionary<string, List<string>>();
            try
            {
                foreach (RMSPTreeNode node in avaliableNodes)
                {
                    RMSPTreeNode teamNode = node.GetTeamsNode();
                    if (!res.ContainsKey(teamNode.Name))
                    {
                        res.Add(teamNode.Name, new List<string>());
                    }
                    RMSPTreeNode siteNode = node.GetSiteCollectionNode();
                    if(siteNode != null)
                    {
                        res[teamNode.Name].Add(siteNode.FullPath);
                    }
                }
            }
            catch (Exception ex) 
            {
                mLogger.Error($"Fail build search filter");
                return new Dictionary<string, List<string>>();
            }
            return res;
        }

        public static bool CheckNeedLoadRuningSCUrlBySelectNode(RMSPTreeNode selectNode, bool useImportTeam = false)
        {
            if (useImportTeam)
            {
                return false;
            }
            if (selectNode.Level == (int)NodeLevel.WebApplication || selectNode.Level == (int)NodeLevel.Office365GroupEntire)
            {
                return false;
            }
            return true;
        }

        public static string GenerateTeamsArchiveJobMonitorExtension(RMSPTreeNode selectNode, TreeMode treeMode, List<string> teamUrls = null, bool useImportSite = false, string teamsUrl = "")
        {
            ArchiveJobMonitorExtension extension = new ArchiveJobMonitorExtension();
            extension.treeMode = treeMode;
            if (useImportSite && teamUrls != null && teamUrls.Count > 0)
            {
                mLogger.Info("this archive job is use import site to run,just add siteurl as confilct");
                extension.ConflictNodeLevel = ConflictNodeLevel.ArchiverImportTeams;
                extension.teamsUrls = teamUrls;
            }
            else
            {
                switch (selectNode.Level) {
                    case (int)NodeLevel.WebApplication:
                        {
                            extension.ConflictNodeLevel = ConflictNodeLevel.Group;
                            extension.GroupNode = selectNode;
                        }
                        break;
                    case (int)NodeLevel.Office365GroupEntire:
                        {
                            extension.ConflictNodeLevel = ConflictNodeLevel.Teams;
                            extension.GroupNode = selectNode;
                        }
                        break;
                    default:
                        {
                            extension.ConflictNodeLevel = ConflictNodeLevel.SiteCollection;
                            if (teamUrls != null)
                            {
                                extension.SiteUrls = teamUrls;
                            }
                            else
                            {
                                extension.SiteUrls = new List<string>() { selectNode.GetSiteCollectionNode()?.FullPath };
                            }
                        }
                        break;
                }
            }
            return SerializerHelper.SerializeByDataContractSerializer(extension);
        }

        private static void BuildTreeForGoogle(RMGoogleTreeNode node, Dictionary<string, RMGoogleTreeNode> nodesDic)
        {
            if (node.ChildrenIds != null && node.ChildrenIds.Count > 0)
            {
                foreach (var id in node.ChildrenIds)
                {
                    if (node.Children == null)
                    {
                        node.Children = new List<RMGoogleTreeNode>();
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

                node.Children = node.Children.OrderBy(item => item.DisplayName, StringComparer.OrdinalIgnoreCase).ToList();

                if (node.Children != null)
                {
                    foreach (var child in node.Children)
                    {
                        BuildTreeForGoogle(child, nodesDic);
                    }
                }
            }
        }
    }
}
