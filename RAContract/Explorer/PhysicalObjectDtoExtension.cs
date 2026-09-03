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
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Explorer
{
    public static class PhysicalObjectDtoExtension
    {
        public static ExplorerQueryV2Dto Convert2ExplorerQueryV2Dto(this PhysicalObjectDto dto, int pageSize = 100, string pageIndex = "")
        {
            var queryOptionV2 = PhysicalExplorerQueryDtoExtension.GetDefaultQueryOptionV2();
            var result = new ExplorerQueryV2Dto
            {
                QueryOption = queryOptionV2,
                PagingInfo = new ExplorerPagingInfo
                {
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                }
            };

            return result;
        }

        /// <summary>
        /// get the parent ids including bottom location id, location id is the first element
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public static List<Guid> GetParentsIdList(this PhysicalObjectDto dto)
        {
            if (dto.Ancestors != null) // new format data
            {
                var r = new Guid[dto.Ancestors.Count];
                dto.Ancestors.CopyTo(r);
                return r.ToList();
            }
            //old format data
            var result = new List<Guid>();
            result.Add(dto.LocationId);
            if (dto.BoxId != Guid.Empty)
            {
                result.Add(dto.BoxId);
            }

            if (dto.FileId != Guid.Empty)
            {
                result.Add(dto.FileId);
            }

            return result;
        }
    }

    public class PhysicalNodeTypeConverter
    {
        public static RMNodeLevel Convert2NodeLevel(RMNodeType nodeType)
        {
            switch(nodeType)
            {
                case RMNodeType.PhysicalBottomLocation:
                    return RMNodeLevel.PhysicalBottomLocation;
                case RMNodeType.PhyCustom:
                    return RMNodeLevel.PhysicalCustom;
                case RMNodeType.PhyBox:
                    return RMNodeLevel.PhysicalBox;
                case RMNodeType.PhyFile:
                    return RMNodeLevel.PhysicalFile;
                case RMNodeType.PhyRecord:
                    return RMNodeLevel.PhysicalRecord;
                default:
                    return RMNodeLevel.Undefined;
            }
        }
    }
}
