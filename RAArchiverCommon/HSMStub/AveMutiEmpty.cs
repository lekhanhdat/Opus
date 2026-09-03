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
using AvePoint.RA.SharePoint.ArchiverCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HSMCommon
{

    public class AveMutiEmpty : AveMultiReceiveTask
    {
        public AveMutiEmpty(int level, bool multiple) : base(level, multiple) { }

        public override void PreAction()
        {
            //throw new NotImplementedException();
        }

        public override void Process()
        {
            //throw new NotImplementedException();
        }

        public override void Complete()
        {
            //throw new NotImplementedException();
        }

        public override void Exception(Exception e)
        {
            //throw new NotImplementedException();
        }

        public override void PostAction()
        {
            //throw new NotImplementedException();

        }
    }
}
