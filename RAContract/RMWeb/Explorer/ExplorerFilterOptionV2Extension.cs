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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.Contract.RMWeb.Explorer
{
    public static class ExplorerFilterOptionV2Extension
    {     
        public static bool IsQueryElectronic(this ExplorerFilterOptionV2 filterOption)
        {
            var sourceFlags = SourceFlagHelper.GetDefaultElectricSourceFlags();
            return filterOption.SourceFlags.Exists(s => sourceFlags.Contains(s)) && GetElectronicNodeTypes(filterOption).Count() > 0;
        }

        public static bool IsQueryPhysical(this ExplorerFilterOptionV2 filterOption)
        {
            return filterOption.SourceFlags.Exists(s => s == SourceFlag.Physical) && GetPhysicalNodeTypes(filterOption).Count() > 0;
        }

        /// <summary>
        /// to be updated to add sp on-premise node types...
        /// </summary>
        /// <param name="filterOption"></param>
        /// <returns></returns>
        public static int[] GetElectronicNodeTypes(this ExplorerFilterOptionV2 filterOption)
        {
            var nodeTypes = new List<int>();
            if (filterOption.SourceFlags.Contains(SourceFlag.SharePoint) || filterOption.SourceFlags.Contains(SourceFlag.OneDrive) || filterOption.SourceFlags.Contains(SourceFlag.Teams)) nodeTypes.AddRange(new List<int> { (int)RMNodeLevel.Item , (int)RMNodeLevel.Folder});
            if (filterOption.SourceFlags.Contains(SourceFlag.SharePointOnPrem)) nodeTypes.AddRange(new List<int> { (int)RMNodeLevel.Item, (int)RMNodeLevel.Folder });
            if (filterOption.SourceFlags.Contains(SourceFlag.Exchange)) nodeTypes.Add((int)NodeLevel.ExchangeOnlineItem);
            if (filterOption.SourceFlags.Contains(SourceFlag.Google)) nodeTypes.AddRange(new List<int> { (int)RMNodeLevel.GoogleFolder, (int)RMNodeLevel.GoogleFile });
            if (filterOption.SourceFlags.Contains(SourceFlag.FileSystem))
            {
                if (filterOption.FSFolderLevelEnabled)
                {
                    nodeTypes.AddRange(new List<int> { (int)RMNodeLevel.FSFolder });
                }
                else
                {
                    nodeTypes.AddRange(new List<int> { (int)RMNodeLevel.FSFolder, (int)RMNodeLevel.FSFile });
                }
            }
            if (filterOption.SourceFlags.Contains(SourceFlag.AzureFileShare))
            {
                nodeTypes.AddRange(new List<int> { (int)RMNodeLevel.AzureFileShareDirectory, (int)RMNodeLevel.AzureFileShareFile });
            }
            if (filterOption.SourceFlags.Contains(SourceFlag.Box))
            {
                nodeTypes.AddRange(new List<int> { (int)RMNodeLevel.BoxFolder, (int)RMNodeLevel.BoxFile });
            }
            if (filterOption.SourceFlags.Any(item => (int)item >= 1000)) {
                nodeTypes.Add((int)RMNodeLevel.CustomizeConnectorItem);
            }

            var withoutNodeTypes = filterOption.WithoutNodeTypes;
            if (withoutNodeTypes != null && withoutNodeTypes.Count > 0)
            {
                foreach (var withoutType in withoutNodeTypes)
                {
                    if (nodeTypes.Contains((int)withoutType))
                    {
                        nodeTypes.Remove((int)withoutType);
                    }
                }
            }

            if (filterOption.NodeTypes != null)
            {
                var tempNodeTypes = filterOption.NodeTypes.Select(o => (int)o).ToList();
                return nodeTypes.Intersect(tempNodeTypes).ToArray();
            }

            return nodeTypes.ToArray();
        }       

        public static int[] GetPhysicalNodeTypes(this ExplorerFilterOptionV2 filterOption)
        {
            var result = filterOption.GetDefaultPhysicalNodeTypes();

            var withoutNodeTypes = filterOption.WithoutNodeTypes;
            if (withoutNodeTypes != null && withoutNodeTypes.Count > 0)
            {
                var resultList = result.ToList();
                foreach (var withoutType in withoutNodeTypes)
                {
                    if (resultList.Contains((int)withoutType))
                    {
                        resultList.Remove((int)withoutType);
                    }
                }
                result = resultList.ToArray();
            }

            if (filterOption.NodeTypes != null)
            {
                var tempNodeTypes = filterOption.NodeTypes.Select(o => (int)o).ToList();
                return result.Intersect(tempNodeTypes).ToArray();
            }

            return result;
        }
        public static int[] GetDefaultPhysicalNodeTypes(this ExplorerFilterOptionV2 filterOption)
        {
            return new int[] { (int)RMNodeLevel.PhysicalCustom, (int)RMNodeLevel.PhysicalBox, (int)RMNodeLevel.PhysicalFile, (int)RMNodeLevel.PhysicalRecord };
        }

        public static RMRecordStatus[] GetPhysicalStatus(this ExplorerFilterOptionV2 filterOption)
        {
            var result = RecordStatusHelper.GetDefaultPhysicalStatus();
            if (filterOption.Status != null)
            {
                return result.Intersect(filterOption.Status).ToArray();
            }

            return result;
        }

        //public static RMRecordStatus[] GetDefaultPhysicalStatus(this ExplorerFilterOptionV2 filterOption)
        //{
        //    return new RMRecordStatus[] { RMRecordStatus.Active, RMRecordStatus.Closed, RMRecordStatus.Destroyed, RMRecordStatus.Missing };
        //}

        //public static RMRecordStatus[] GetElectronicStatus(this ExplorerFilterOptionV2 filterOption)
        //{
        //    return filterOption.GetDefaultElectronicStatus();

        //}
        //public static RMRecordStatus[] GetDefaultElectronicStatus(this ExplorerFilterOptionV2 filterOption)
        //{
        //    return new RMRecordStatus[] { RMRecordStatus.Active };

        //}
    }
}
