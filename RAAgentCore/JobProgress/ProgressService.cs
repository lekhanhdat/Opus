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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.FileSystem.Core
{
    public class ProgressService : IProgressService
    {
        private long total;
        private long finished;
        public long Finished { get { return finished; } }
        public long Total { get { return total; } }

        public ProgressService()
        {
        }

        public void IncreaseBase(long value)
        {
            Interlocked.Add(ref total, value);
        }

        public void Increase()
        {
            Interlocked.Add(ref finished, 1);
        }

        public void Increase(int x)
        {
            Interlocked.Add(ref finished, x);
        }

        public void IncreaseToComplete()
        {
            Interlocked.Exchange(ref finished, total);
        }

        public void SetTotal(long total)
        {
            this.total = total;
        }
    }
}
