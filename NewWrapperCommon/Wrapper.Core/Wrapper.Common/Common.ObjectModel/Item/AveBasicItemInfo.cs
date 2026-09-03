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

namespace AvePoint.Wrapper.Common
{
    public class AveBasicItemInfo
    {
        private int mAuthor = 0;
        public int Author
        {
            get { return mAuthor; }
            set { mAuthor = value; }
        }
        private int mEditor = 0;
        public int Editor
        {
            get { return mEditor; }
            set { mEditor = value; }
        }
        private DateTime mTp_create = DateTime.MinValue;
        public DateTime Tp_create
        {
            get { return mTp_create; }
            set { mTp_create = value; }
        }
        private DateTime mTp_modify = DateTime.MinValue;
        public DateTime Tp_modify
        {
            get { return mTp_modify; }
            set { mTp_modify = value; }
        }
        private DateTime mCreate = DateTime.MinValue;
        public DateTime Create
        {
            get { return mCreate; }
            set { mCreate = value; }
        }
        private DateTime mModify = DateTime.MinValue;
        public DateTime Modify
        {
            get { return mModify; }
            set { mModify = value; }
        }
    }
}
