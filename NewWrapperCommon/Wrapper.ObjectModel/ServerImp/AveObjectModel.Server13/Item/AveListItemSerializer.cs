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
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server13
{
    internal class AveListItemSerializer : IAveListItemSerializer,IDisposable
    {
        private IReport mReport;
        public IReport Report
        {
            get
            {
                if (mReport == null)
                {
                    mReport = new AveWrapperReport();
                }
                return mReport;
            }
        }
        private AveSite mSite;
        private AveWeb mParentWeb;
        private AveList mParentList;
        private AveListItemImport mListItemImport;

        public AveListItemImport ListItemImport
        {
            get
            {
                if (mListItemImport == null)
                {
                    mListItemImport = new AveListItemImport(mSite, mParentWeb, mParentList);
                }
                return mListItemImport;
            }
        }

        public AveListItemSerializer(AveSite site, AveWeb web, AveList list)
        {
            mSite = site;
            mParentWeb = web;
            mParentList = list;
        }

        public void SetReport(IReport report)
        {
            mReport = report;
        }

        public object GetObjectData()
        {
            throw new NotImplementedException();
        }

        public AveRestoreResult SetObjectData(AveListItemInfo itemInfo)
        {
            this.ListItemImport.SetReport(Report);
            return this.ListItemImport.Import(itemInfo);            
        }

        public void BeforeSetObjectData()
        {
            
        }

        public void AfterSetObjectData()
        {
            
        }

        public void Dispose()
        {
            if (mReport != null)
            mReport.Dispose();
        }
    }
}
