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
using AvePoint.Wrapper.Common;
using Microsoft.Office.Server.SocialData;
using AvePoint.Wrapper.Common.Office;
using Microsoft.Office.Server.Social;
using System.Collections.Generic;

namespace AvePoint.ObjectModel.Server16.Office
{
    class AveOSocialFeed : IAveOSocialFeed
    {
        private SPSocialFeed mSocialFeed;

        public AveOSocialFeed(SPSocialFeed socialFeed)
        {
            mSocialFeed = socialFeed;
        }

        public IAveOSocialThread[] Threads
        {
            get
            {
                List<IAveOSocialThread> thread = new List<IAveOSocialThread>();
                SPSocialThread[] tmpThread = mSocialFeed.Threads;
                foreach (SPSocialThread st in tmpThread)
                {
                    if (tmpThread != null)
                    {
                        thread.Add(new AveOSocialThread(st));
                    }
                    else
                    {
                        thread.Add(null);
                    }
                }
                return thread.ToArray();
            }
        }

    }
}
