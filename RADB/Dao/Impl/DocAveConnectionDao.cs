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
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.DB.Model;
using System.Linq;

namespace AvePoint.RA.DB.Dao.Impl
{
    [RACodeReview("Allen Yin")]
    public class DocAveConnectionDao : BaseDao<RMCPDocAveConnection>, IDocAveConnectionDao
    {
        public RMCPDocAveConnection GetDocAveConnectionInfosFromRA()
        {
            return SharedDbContext.DocAveConnectionInfos.FirstOrDefault();
        }
        [RACodeReview("Allen Yin",comment:@"此处有重复写的风险，
            但考虑此记录并不会频繁更新，只加一个锁，保证db中不会出现两条记录即可")]
        public void SaveOrUpdate(RMCPDocAveConnection newConnData)
        {
            var context = SharedDbContext;
            RMCPDocAveConnection oldData = context.DocAveConnectionInfos.FirstOrDefault();
            if (oldData == null)
            {
                context.DocAveConnectionInfos.Add(newConnData);
                context.SaveChanges();
            }
            else
            {
                this.Update(newConnData);
            }
        }
    }
}
