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
using System.Collections;

namespace AvePoint.Media.ClassicStorage.Cloud.Common.ListWrapper
{
    public abstract class AbstractArrayListWrapper : ArrayList , IArrayListStateListener
    {
        private ArrayList arrrayList = new ArrayList();
        public int size { set; get; }


        public ArrayList ar
        {
            set
            {
                arrrayList = value;
            }
            get
            {
                return arrrayList;
            }
        }

        public override int Add(object value)
        {
            return ar.Add(value);
        }

        public virtual void setInitSize(int size)
        {
            this.size = size;
        }

        public virtual void SetState(Client.ResponseInfo responseInfo, Dictionary<string, string> queryParams, string urlWithoutQueryParms, Dictionary<string, string> headers, StorageInfo dirInfo, StorageInfo storageInfo)
        {
            throw new NotImplementedException();
        }
    }
}
