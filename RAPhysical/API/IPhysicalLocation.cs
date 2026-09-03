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
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RAPhysical.API
{
    public interface IPhysicalLocation : IPhysicalFields, IDisposable
    {
        string Name { get; set; }
        string Description { get; set; }
        //容量
        double TotalCapacity { get; set; }
        int CurrentLocationType { get; set; }
        Guid UniqueId { get; }
        int IntId { get; }
        int LocationParentId { get; set; }
        //判断当前Location是否是根节点的Location, True means current location is Root
        bool IsRootLocation { get; }
        //判断当前Location是否是最底层节点， True 表示最底层，那么子节点就不会有Location
        bool IsBottomLocation { get; }
        bool Exist { get; }
        string DirPath { get; }
        IPhysicalLocation ParentLocation { get; set; }

        //提供当前Location 的所有下一层Locations
        List<IPhysicalLocation> AllSubLocations { get; }
        //提供当前Location下层所有的Box
        List<IPhysicalBox> AllBoxes { get; }
        //提供Location 下层所有的Files
        List<IPhysicalFile> AllFiles { get; }
        //提供Location 下层所有的Container
        List<IPhysicalCustom> AllContainers { get; }
        //提供当前Location下一层的Box
        List<IPhysicalBox> Boxes { get; }
        //提供Location 下一层的Files
        List<IPhysicalFile> Files { get; }
        //提供Location 下一层的Container
        List<IPhysicalCustom> Containers { get; }
        long GetBoxesCount(Expression<Func<Record, bool>> expression);
        List<Record> GetBoxesAndFoldOrderByDescending(Expression<Func<Record, bool>> expression);
        List<IPhysicalBox> GetBoxes(Expression<Func<Record, bool>> expression);
        long GetFilesCount(Expression<Func<Record, bool>> expression);
        List<IPhysicalFile> GetFiles(Expression<Func<Record, bool>> expression);
        Dictionary<RMNodeLevel, List<Object>> Query(Expression<Func<Record, bool>> expression);
        Task DeleteAsync();
        Task UpdateAsync();
        void SetFilterSubLocationByUniqueId(List<Guid> uniqueIds);
    }
}
