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
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Retrying
{
    public class Demo
    {
        private static readonly RMRetryer Retryer = RMRetryerBuilder.CreateBuilder().Build();



        public void Test()
        {
            Retryer.Retry(() =>
            {

            });
        }

        public void Test1()
        {
            var i = Retryer.Retry(() =>
            {
                return 2;
            });
        }

        public async Task Test2()
        {
            await Retryer.RetryAsync(async () =>
            {
                await Task.Delay(1000);
            });
        }

        public async Task<int> Test3()
        {
            return await Retryer.RetryAsync(async () =>
            {
                await Task.Delay(1000);
                return 3;
            });
        }
    }
}
