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
using AvePoint.GCommon;
using AvePoint.RA.CommonUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RMExplorer
{

    public class DiscoverCache : IEnumerable<SourceBase>, IEnumerator<SourceBase>, IDisposable
    {

        //将PCContaiern 对象封装在DiscoverCache 里面，并且是ReadOnly 的，保证一个DiscoverCache只会有一个PCContainer， 此处public 为了方便外界调用，往PCContainer 中生成数据。 如果以后需要外界传递来PCContainer，可以新加构造函数
        public readonly PCContainer<SourceBase> PCContainer = null;

        public DiscoverCache()
        {
            PCContainer = new PCContainer<SourceBase>(1000);
        }

        private SourceBase mCurrent;
        public SourceBase Current
        {
            get
            {
                return mCurrent;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return null;// throw new NotImplementedException();
            }
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public IEnumerator<SourceBase> GetEnumerator()
        {
            return this;
        }

        public bool MoveNext()
        {
            return (mCurrent = PCContainer.Consume()) != null;
        }

        public void Reset()
        {
            //throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }
    }
}
