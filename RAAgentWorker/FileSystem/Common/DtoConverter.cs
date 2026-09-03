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
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.FileSystem.Core;
using Microsoft.IdentityModel.Tokens;
using RAFileSystem.FileSystem.FileSystem.Backup.CoreIndex.CoreIndexCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAFileSystem.FileSystem.Common
{
    public class DtoConverter
    {
        public static AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto ConvertGlobalDto2FSTreeNodeDto(FSTreeNodeDto dto, AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto treeDto = null)
        {
            if (treeDto == null)
            {
                treeDto = new AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto();
            }
            treeDto.ID = dto.Id.ToString();
            treeDto.FarmID = dto.FarmID;
            treeDto.Name = dto.Name;
            treeDto.FullPath = dto.FullPath;
            treeDto.Level = (AvePoint.GCommon.Contract.Tree.Object.NodeLevel)dto.Level;
            treeDto.Type = (AvePoint.GCommon.Contract.Tree.Object.NodeType)dto.NodeType;
            treeDto.Expanded = dto.Expanded;
            treeDto.ChildrenCount = dto.ChildrenCount;
            treeDto.CheckNumber = dto.CheckNumber;

            treeDto.Domain = dto.Domain;
            treeDto.Username = dto.Username;
            treeDto.EncryptedPassword = dto.EncryptedPassword;

            //fs.IncludeNew = Convert.ToBoolean(dto.IncludeNew) ? IncludeNewState.Checked : IncludeNewState.Unchecked;
            //if (fs.NodeExtension == null)
            //{
            //    fs.NodeExtension = new NodeExtensionDto();
            //}
            //sp.NodeExtension.BposInfo = dto.BposInfo;
            if (dto.Parent != null && treeDto.Parent == null)
            {
                AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto tempParent = new AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto();
                tempParent.Children = new List<AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto> { treeDto };
                treeDto.Parent = ConvertGlobalDto2FSTreeNodeDto(dto.Parent, tempParent);
                //fs.ParentId = dto.Parent.Id.ToString();
            }
            if (dto.CheckNumber == 1)
            {
                return treeDto;
            }
            if (dto.Children != null && dto.Children.Count > 0 &&
                (treeDto.Children == null || treeDto.Children.Count == 0))
            {
                treeDto.Children = new List<AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto>();
                foreach (AvePoint.RA.Contract.Global.Object.FSTreeNodeDto child in dto.Children)
                {
                    if (HasSelectNodeForFS(child))
                    {
                        AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto tempChild = new AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto();
                        tempChild.Parent = treeDto;
                        tempChild.ParentId = treeDto.ID.ToString();
                        treeDto.Children.Add(ConvertGlobalDto2FSTreeNodeDto(child, tempChild));
                    }
                    else
                    {
                       //logger.Debug("No select node in {0}", child.Name);
                    }
                }
            }
            return treeDto;
        }
        private static bool HasSelectNodeForFS(AvePoint.RA.Contract.Global.Object.FSTreeNodeDto current)
        {
            if (current.CheckNumber != 0)
            {
                return true;
            }
            if (current.Children == null || current.Children.Count==0)
            {
                return false;
            }
            else
            {
                foreach (AvePoint.RA.Contract.Global.Object.FSTreeNodeDto child in current.Children)
                {
                    if (HasSelectNodeForFS(child))
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        public static void SetMasterIndexValue(ArchiveIndexInfo masterInfo, ArchiverBasicIndex result)
        {
            result.Attributes = masterInfo.UNCPath;
            result.SitePath = masterInfo.ConnectionId;
            result.Url = masterInfo.ConnectionPath + "\\" + result.ExtraInfo + "\\" + result.Name;
        }

    }
}
