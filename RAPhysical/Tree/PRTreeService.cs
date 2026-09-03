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
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Tree;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.RAPhysical.API;
using AvePoint.RA.RAPhysical.Common;
using AvePoint.RA.RAPhysical.Report;
using AvePoint.RA.RAPhysical.Tree.Interface;
using AvePoint.RA.RAPhysical.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.RAPhysical.Tree
{
    public class PRTreeService : IPRTreeService
    {
        private BrowseOptions _options = new BrowseOptions();
        public IPRTreeNodeService TreeNodeService { get; set; }
        public IRMSecurityGroupDao SecurityGroupDao { get; set; }

        public UserService UserService = new UserService();

        #region register actions
        private List<Func<IPhysicalLocation, Task>> _rootLocationActions = new List<Func<IPhysicalLocation, Task>>();
        private List<Func<IPhysicalLocation,Task>> _normalLocationActions = new List<Func<IPhysicalLocation, Task>>();
        private List<Func<IPhysicalLocation, Task>> _bottomLocationActions = new List<Func<IPhysicalLocation, Task>>();
        private List<Func<IPhysicalCustom, Task>> _containerActions = new List<Func<IPhysicalCustom, Task>>();
        private List<Func<IPhysicalBox, Task>> _boxActions = new List<Func<IPhysicalBox, Task>>();
        private List<Func<IPhysicalFile, Task>> _fileActions = new List<Func<IPhysicalFile, Task>>();
        private List<Func<IEnumerable<IPhysicalRecord>,Task>> _recordGroupActions = new List<Func<IEnumerable<IPhysicalRecord>,Task>>();

        public IPRTreeService ConfigRootLocationAction(Func<IPhysicalLocation, Task> action)
        {
            _rootLocationActions.Add(action);
            return this;
        }

        public IPRTreeService ConfigNormalLocationAction(Func<IPhysicalLocation,Task> action)
        {
            _normalLocationActions.Add(action);
            return this;
        }

        public IPRTreeService ConfigBottomLocationAction(Func<IPhysicalLocation, Task> action)
        {
            _bottomLocationActions.Add(action);
            return this;
        }

        public IPRTreeService ConfigContainerAction(Func<IPhysicalCustom, Task> action)
        {
            _containerActions.Add(action);
            return this;
        }

        public IPRTreeService ConfigBoxAction(Func<IPhysicalBox, Task> action)
        {
            _boxActions.Add(action);
            return this;
        }

        public IPRTreeService ConfigFileAction(Func<IPhysicalFile, Task> action)
        {
            _fileActions.Add(action);
            return this;
        }

        public IPRTreeService ConfigRecordGroupAction(Func<IEnumerable<IPhysicalRecord>,Task> action)
        {
            _recordGroupActions.Add(action);
            return this;
        }

        #endregion

        public async Task ProcessAsync(IEnumerable<RMLocationProfileNode> rootNodes, BrowseOptions options)
        {
            ThrowIfNull(rootNodes, "rootNodes");
            ThrowIfNull(options, "options");

            _options = options;

            foreach (var node in rootNodes)
            {
                await ProcessTreeNodeAsync(node);
            }
        }

        #region private
        private void ThrowIfNull(object obj, string argName)
        {
            if (obj == null) throw new ArgumentNullException(argName);
        }


        private RMNodeType GetNodeType(RMLocationProfileNode node)
        {
            RMNodeType nodeType;
            if (!Enum.TryParse(node.NodeType.ToString(), out nodeType))
            {
                throw new ArgumentException($"Wrong node type : {node.NodeType}");
            }

            return nodeType;
        }
        private async Task ProcessTreeNodeAsync(RMLocationProfileNode node)
        {
            if (node == null) return;
            RMNodeType nodeType = GetNodeType(node);
            switch (nodeType)
            {
                case RMNodeType.PhysicalRootLocation:
                    await ProcessRootLocationNodeAsync(node);
                    break;
                case RMNodeType.PhysicalNormalLocation:
                    await ProcessNormalLocationNodeAsync(node);
                    break;
                case RMNodeType.PhysicalBottomLocation:
                    await ProcessBottomLocationNodeAsync(node);
                    break;
                case RMNodeType.PhyCustom:
                    await ProcessContainerNodeAsync(node);
                    break;
                case RMNodeType.PhyBox:
                    await ProcessBoxNodeAsync(node);
                    break;
                case RMNodeType.PhyFile:
                    await ProcessFileNodeAsync(node);
                    break;
                default:
                    throw new ArgumentException($"Unsupported node type : {nodeType}");
            }
        }

        private async Task ProcessRootLocationNodeAsync(RMLocationProfileNode node)
        {
            var location = TreeNodeService.GetRootLocationInfo(node);

            var userIds = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);

            var userPermission = SecurityGroupDao.GetUserScopePermissions(userIds);
            if (!userPermission.IsAdmin)
            {
                var topLocationIds = userPermission.ScopePermissionInfo.FirstOrDefault(_ => _.DataSourceType == Contract.Explorer.SourceFlag.Physical)?.ScopeIds ?? new List<Guid>();
                location.SetFilterSubLocationByUniqueId(topLocationIds);
            }

            await ProcessLocationAsync(location, node);
        }

        private Task ProcessNormalLocationNodeAsync(RMLocationProfileNode node)
        {
            var location =  TreeNodeService.GetNormalLocationInfo(node);
            return ProcessLocationAsync(location);
        }


        private Task ProcessBottomLocationNodeAsync(RMLocationProfileNode node)
        {
            var location = TreeNodeService.GetBottomLocationInfo(node);

            return ProcessLocationAsync(location);
        }

        private async Task ProcessLocationAsync(IPhysicalLocation location, RMLocationProfileNode profileNode = null)
        {
            if (location == null) return;

            //节点为null和节点是checked状态，代表当前节点需要处理
            bool needProcess = profileNode == null || profileNode.Checked.GetValueOrDefault();
            if (needProcess)
            {
                if (location.IsRootLocation)
                {
                    if (!_options.NeedProcessRootLocation) return;
                    await ActionHelper.ExecuteAsync(_rootLocationActions, location);
                }
                else if (location.IsBottomLocation)
                {
                    if (!_options.NeedProcessBottomLocation) return;
                    await ActionHelper.ExecuteAsync(_bottomLocationActions, location);
                }
                else
                {
                    if (!_options.NeedProcessNormalLocation) return;
                    await ActionHelper.ExecuteAsync(_normalLocationActions, location);
                }

                if (_options.NeedProcessContainer)
                {
                    //deal with container, get all containers under location
                    var subContainers = location.AllContainers;
                    if (subContainers != null)
                    { 
                        await subContainers?.ForEachAsync(async subContainer => await ProcessContainerAsync(subContainer));
                    }
                }

                if (_options.NeedProcessBox)
                {
                    //deal with box, get all boxes under location, including boxes under custom container
                    var boxes = location.AllBoxes;
                    if (boxes != null)
                    { 
                        await boxes?.ForEachAsync(async box => await ProcessBoxAsync(box));
                    }
                }

                if (_options.NeedProcessFile)
                {
                    //deal with physical file, get physical files under location or custom container
                    var files = location.AllFiles;
                    if (files != null)
                    { 
                        await files?.ForEachAsync(async file => await ProcessFileAsync(file));
                    }
                }
            }

            //sub location
            var subLocations = location.AllSubLocations;
            if (subLocations != null)
            {
                await subLocations?.ForEachAsync(async subLocation =>
                {
                    if (profileNode == null //父节点为null，子节点直接处理，不需要检查状态
                        || profileNode.Checked.GetValueOrDefault() //父节点是checked状态，子节点需要处理
                        || (profileNode.IncludeNew.GetValueOrDefault() && !profileNode.ChildStates.ContainsKey(subLocation.IntId.ToString())) //父节点Include New，子节点需要处理
                        )
                    {
                        await ProcessLocationAsync(subLocation);
                    }
                    else
                    {
                        if (IsChildChecked(profileNode, subLocation.IntId.ToString()))//子节点是勾选状态
                        {
                            await ProcessLocationAsync(subLocation);
                        }
                        else
                        {
                            var childNodeInProfile = profileNode.Children.Where(c => c.Id == subLocation.IntId.ToString()).FirstOrDefault();
                            childNodeInProfile = childNodeInProfile != null ? childNodeInProfile : profileNode.OtherChildren?.Values.Where(oc => oc.Id == subLocation.IntId.ToString()).FirstOrDefault();
                            if (childNodeInProfile != null)
                            {
                                await ProcessLocationAsync(subLocation, childNodeInProfile);
                            }
                            else
                            {
                                //skip current child
                                return;
                            }
                        }
                    }
                });
            }
        }

        private bool IsChildChecked(RMLocationProfileNode node, string childId)
        {
            if (node != null && node.ChildStates != null && node.ChildStates.ContainsKey(childId) && node.ChildStates[childId].Count > 1)
            {
                var childState = node.ChildStates[childId][1];
                return Convert.ToBoolean(childState);
            }
            else
            {
                return false;
            }
        }

        private Task ProcessContainerNodeAsync(RMLocationProfileNode node)
        {
            var container = TreeNodeService.GetContainerInfo(node);
            return ProcessContainerAsync(container);
        }

        private Task ProcessBoxNodeAsync(RMLocationProfileNode node)
        {
            var box = TreeNodeService.GetBoxInfo(node);
            return ProcessBoxAsync(box);
        }

        private async Task ProcessContainerAsync(IPhysicalCustom container)
        {
            if (container == null) return;

            await ActionHelper.ExecuteAsync(_containerActions, container);

            //if (_options.NeedProcessFile)
            //{
            //    //process file
            //    var files = container.Files;
            //    files?.ForEach(file => ProcessFile(file));
            //}

            //if (_options.NeedProcessBox)
            //{
            //    //process file
            //    var boxes = container.Boxes;
            //    boxes?.ForEach(box => ProcessBox(box));
            //}

            //if (_options.NeedProcessContainer)
            //{
            //    var containers = container.CustomContainers;
            //    containers?.ForEach(subContainer => ProcessContainer(subContainer));
            //}
        }

        private async Task ProcessBoxAsync(IPhysicalBox box)
        {
            if (box == null) return;

            await ActionHelper.ExecuteAsync(_boxActions, box);
            
            if (_options.NeedProcessFile)
            {
                //process file
                var files = box.Files;
                if(files!= null)
                {
                    await files.ForEachAsync(async file => await ProcessFileAsync(file));
                }
            }
        }

        private Task ProcessFileNodeAsync(RMLocationProfileNode node)
        {
            var file = TreeNodeService.GetFileInfo(node);

            return ProcessFileAsync(file);

        }

        private async Task ProcessFileAsync(IPhysicalFile file)
        {
            if (file == null) return;

            await ActionHelper.ExecuteAsync(_fileActions, file);

            if (_options.NeedProcessRecord)
            {
                await ProcessRecordsAsync(file.Records);
            }
        }

        private async Task ProcessRecordsAsync(IEnumerable<IPhysicalRecord> records)
        {
            if (records == null || records.Count() == 0 || _recordGroupActions.Count == 0) return;

            await ActionHelper.ExecuteAsync(_recordGroupActions, records);

            //var itemsGroups = TreeNodeService.GetPhysicalRecordInfo(node, _options.GroupSize);
            //AveTenantTasks.RunAndWaitTasks(itemsGroups,
            //    new System.Threading.CancellationTokenSource(),
            //    group =>
            //    {
            //        ActionHelper.Execute(_recordGroupActions, group.Items);
            //    });
        }

        #endregion
    }
}
