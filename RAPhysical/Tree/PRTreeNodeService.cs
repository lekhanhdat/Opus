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
using AvePoint.RA.RAPhysical.API;
using AvePoint.RA.RAPhysical.Common;
using AvePoint.RA.RAPhysical.Discover;
using AvePoint.RA.RAPhysical.Tree.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RAPhysical.Tree
{
    public class PRTreeNodeService : IPRTreeNodeService
    {
        public IPRDiscover PRDiscover { get; set; }

        public IPhysicalLocation GetBottomLocationInfo(RMLocationProfileNode node)
        {
            IPhysicalLocation bottomLocation = new PhysicalLocation(node.UniqueId);
            return bottomLocation;
        }

        public IPhysicalBox GetBoxInfo(RMLocationProfileNode node)
        {
            throw new NotImplementedException();
        }

        public IPhysicalCustom GetContainerInfo(RMLocationProfileNode node)
        {
            throw new NotImplementedException();
        }

        public IPhysicalFile GetFileInfo(RMLocationProfileNode node)
        {
            throw new NotImplementedException();
        }

        public IPhysicalLocation GetNormalLocationInfo(RMLocationProfileNode node)
        {
            IPhysicalLocation normalLocation = new PhysicalLocation(node.UniqueId);
            return normalLocation;
        }

        public IEnumerable<ItemsGroup<IPhysicalRecord>> GetPhysicalRecordInfo(RMLocationProfileNode node, int groupSize)
        {
            //var itemsGroups = PRDiscover.GetItemsGroup(
            //    record => true, //to be add condition here
            //    groupSize);

            throw new NotImplementedException();
        }

        public IPhysicalLocation GetRootLocationInfo(RMLocationProfileNode node)
        {
            IPhysicalLocation rootLocation = new PhysicalLocation(node.UniqueId);
            return rootLocation;
        }
    }
}
