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
using AvePoint.Wrapper.Common;
namespace AvePoint.ObjectModel.Common
{
    class AveFieldRatingScale:AveFieldMultiChoice,IAveFieldRatingScale
    {
        private IAveRequest mRequest;
        private AveList mParentList;
        private AveWeb mWeb;
        private AveFieldCollection mFieldCollection;
        private string mFieldSource;
        private Dictionary<string, object> mContentTypeProp;

        public AveFieldRatingScale(IAveRequest request, AveList list, AveWeb web, string fieldSource, AveFieldCollection fieldCollection, Dictionary<string, object> contentTypeProp, Dictionary<string, object> prop)
            :base(request,list,web,fieldSource,fieldCollection,contentTypeProp,prop)
        {
            mRequest = request;
            mParentList = list;
            mWeb = web;
            mFieldCollection = fieldCollection;
            mFieldSource = fieldSource;
            mContentTypeProp = contentTypeProp;
            base.DataCache.AddPropertyies(prop);
        }
        public int GridEndNumber 
        {
            get
            {
                return base.DataCache.GetProperty<int>("GridEndNumber");
            }
            set
            {
                base.DataCache.AddChangedProperty("GridEndNumber", value);
            }
        }
        public string GridNAOptionText 
        {
            get
            {
                return base.DataCache.GetProperty<string>("GridNAOptionText");
            }
            set
            {
                base.DataCache.AddChangedProperty("GridNAOptionText", value);
            }
        }
        public int GridStartNumber
        {
            get
            {
                return base.DataCache.GetProperty<int>("GridStartNumber");
            }
            set
            {
                base.DataCache.AddChangedProperty("GridStartNumber", value);
            }
        }
        public string GridTextRangeAverage 
        {
            get
            {
                return base.DataCache.GetProperty<string>("GridTextRangeAverage");
            }
            set
            {
                base.DataCache.AddChangedProperty("GridTextRangeAverage", value);
            }
        }
        public string GridTextRangeHigh 
        {
            get
            {
                return base.DataCache.GetProperty<string>("GridTextRangeHigh");
            }
            set
            {
                base.DataCache.AddChangedProperty("GridTextRangeHigh", value);
            }
        }
        public string GridTextRangeLow 
        {
            get
            {
                return base.DataCache.GetProperty<string>("GridTextRangeLow");
            }
            set
            {
                base.DataCache.AddChangedProperty("GridTextRangeLow", value);
            }
        }
        public int RangeCount 
        {
            get
            {
                return base.DataCache.GetProperty<int>("RangeCount");
            }
        }
    }
}
