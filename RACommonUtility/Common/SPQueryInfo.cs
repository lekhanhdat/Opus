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
using AvePoint.RA.RACommonUtility.CAMLHelper.CAML;
using AvePoint.RA.RACommonUtility.Extension;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Discovery;

namespace AvePoint.RA.RACommonUtility.Common
{
    public class SPQueryInfo
    {
        public IAveList List { get; set; }

        public AveDiscoverList DiscoverList { get; set; }
        private AveDiscoverFolder _folder = null;
        public AveDiscoverFolder Folder
        {
            get
            {
                return _folder;
            }
            set
            {
                _folder = value;

            }

        }
        //如果按照Folder查询, 需要给ServerRelativeUrl赋值
        private string _serverRelativeUrl;
        public string ServerRelativeUrl
        {
            get
            {
                return _serverRelativeUrl;
            }
            set { _serverRelativeUrl = value; }
        }
        private IAveFolder _currentFolder;
        public IAveFolder CurrentFolder
        {
            get
            {
                if (_currentFolder == null)
                {
                    if (IsRootFolder)
                    {
                        _currentFolder = List.RootFolder;
                    }
                    else
                    {
                        _currentFolder = Folder.AveFolder;
                    }
                }
                return _currentFolder;
            }
            set { _currentFolder = value; }
        }
        public bool IsRootFolder { get; set; }
        public Types.ScopeTypes ScopeType { get; set; } = Types.ScopeTypes.Default;
        private int _rowlimit = -1;
        public int RowLimit
        {
            get
            {
                if (_rowlimit == -1)
                {
                    _rowlimit = List.ParentWeb.Site.GetMaxItemsPerThrottledOperation();

                }
                return _rowlimit;
            }
            set { _rowlimit = value; }
        }
        private int _startIndex = 0;
        public int StartIndex
        {
            get
            {
                return _startIndex;
            }
            set { _startIndex = value; }
        }
       
        public int MaxItemId { get; set; }
        public CAMLManager CAML { get; set; } = new CAMLManager();

        public bool Valid()
        {
            if (List == null || CurrentFolder == null || RowLimit == 0)
            {
                throw new ArgumentException($"spquery info param is invalid.");
            }
            return true;
        }
    }
}
