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
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.DB.Model;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.SharePoint.Client;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using PnP.Core.QueryModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using DocumentFormat.OpenXml.ExtendedProperties;
using AvePoint.RA.Contract.Common;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.Explorer;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMRetentionSimulateInfosDao : BaseDao<RMRetentionSimulateInfos>, IRMRetentionSimulateInfosDao
    {
        public void AccumulateUpdateRetentionInfo(int sourceFalg, long fileNumber, long size = 0)
        {
            if (fileNumber == 0 && size == 0)
            {
                return;
            }
            using (var context = GetNewContext())
            {
                var r = context.RMRetentionInfos.AsQueryable().FirstOrDefault(u => u.SourceFlag == sourceFalg);

                if (r != null)
                {
                    r.DataSize += size;
                    r.FileNumber += fileNumber;
                    context.RMRetentionInfos.AddOrUpdate(r);
                    context.SaveChanges();
                }
            }
        }

        public void AddOrUpdateRetentionInfo(RMRetentionSimulateInfos info)
        {
            using (var context = GetNewContext())
            {
                context.RMRetentionInfos.AddOrUpdate(info);
                context.SaveChanges();
            }
        }

        public List<RMRetentionSimulateInfos> GetAll()
        {
            using (var context = GetNewContext())
            {
                return context.RMRetentionInfos.ToList();
            }
        }
    }
}