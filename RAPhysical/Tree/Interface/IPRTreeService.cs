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
using AvePoint.RA.RAPhysical.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.Contract.RMWeb.Tree;
using AvePoint.RA.RAPhysical.API;

namespace AvePoint.RA.RAPhysical.Tree.Interface
{
    public interface IPRTreeService
    {
        IPRTreeService ConfigRootLocationAction(Func<IPhysicalLocation,Task> action);
        IPRTreeService ConfigNormalLocationAction(Func<IPhysicalLocation, Task> action);
        IPRTreeService ConfigBottomLocationAction(Func<IPhysicalLocation, Task> action);
        IPRTreeService ConfigContainerAction(Func<IPhysicalCustom, Task> action);
        IPRTreeService ConfigBoxAction(Func<IPhysicalBox, Task> action);
        IPRTreeService ConfigFileAction(Func<IPhysicalFile, Task> action);
        IPRTreeService ConfigRecordGroupAction(Func<IEnumerable<IPhysicalRecord>, Task> action);

        Task ProcessAsync(IEnumerable<RMLocationProfileNode> nodes, BrowseOptions options);
    }
}
