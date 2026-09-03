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
using AvePoint.Media.ClassicStorage.Cloud.Common.ListWrapper;
using AvePoint.Media.ClassicStorage.Cloud.Common.Client;
using AvePoint.Media.ClassicStorage;
using AvePoint.GCommon.Contract.CodeReview;

namespace ArrayListTest
{
    #region CodeReview
    [AveCodeReview(
    "2012/5/23",
    "rongbiao.sun@avepoint.com",
    "liang.wang@avepoint.com",
     new string[] { CodeReviewConstants.CHECK_LIST_ID_BL_1 },
    null,
     true)]
    #endregion
    public class ArrayListSubDirsWrapper : AbstractArrayListWrapper
    {
        private int num = 0;
        private ResponseInfo responseInfo ;
        private Dictionary<string, string> queryParams;
        private string urlWithoutQueryParms ;
        private Dictionary<string, string> headers ;
        private StorageInfo dirInfo;
        private StorageInfo storageInfo;
        private IChangeArrayListResultsListener arrayListResultsListener;
        private int count = 0;



        public ArrayListSubDirsWrapper(IChangeArrayListResultsListener arrayListResultsListener)
        {
            this.arrayListResultsListener = arrayListResultsListener;
            this.count = arrayListResultsListener.GetDirsResultsCount();
        }

      
        public override int Count
        {
            get
            {
                return count;
            }
        }

        private void AddNextConnection()
        {
            ar.Clear();
            ar = arrayListResultsListener.GetNextDirsResults(responseInfo, queryParams, urlWithoutQueryParms
                , headers, dirInfo , storageInfo);
        }

        public override object this[int index]
        {
            get
            {
                if (index - num >= ar.Count)
                {
                    num = num + ar.Count;
                    AddNextConnection();
                }
                return ar[index - num];
            }
            set
            {
                ar.Add(value);
            }
        }

        public override int Add(object value)
        {
            return ar.Add(value);
        }


        public override void SetState(ResponseInfo responseInfo, Dictionary<string, string> queryParams,
          string urlWithoutQueryParms, Dictionary<string, string> headers, StorageInfo dirInfo, StorageInfo storageInfo)
        {
            this.responseInfo = responseInfo;
            this.queryParams = queryParams;
            this.urlWithoutQueryParms = urlWithoutQueryParms;
            this.headers = headers;
            this.dirInfo = dirInfo;
            this.storageInfo = storageInfo;
        }
    }
}
