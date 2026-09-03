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
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Restore
{
    [Serializable]
    public class AveNintexFormListNotFoundException : AveNintexFormPostException
    {
        //TODO,需要加到Resource文件。
        const string format = "Lookup List {0} did not have been restored, nintex form content type id : {1}";
        public AveNintexFormListNotFoundException(string listTitleOrId, string contentTypeId)
            : base(string.Format(format, listTitleOrId, contentTypeId))
        {

        }
    }

    [Serializable]
    public class AveNintexFormListItemNotFoundException : AveNintexFormPostException
    {
        //TODO,需要加到Resource文件。
        const string format = "The list item {0} in lookup List {1} did not have been restored, nintex form content type id : {2}";
        public AveNintexFormListItemNotFoundException(string listTitle, int itemId, string contentTypeId)
            : base(string.Format(format, itemId, listTitle, contentTypeId))
        {

        }
    }

    [Serializable]
    public class AveNintexFormPostException :Exception
    {
        //TODO,需要加到Resource文件。
        const string format = "Lookup {0} {1} did not have been restored, nintex form content type id : {2}";
        public AveNintexFormPostException(string objectType, string objectUrl, string contentTypeId)
            : base(string.Format(format, objectType, objectUrl, contentTypeId))
        { }
        public AveNintexFormPostException(string message) : base(message)
        { }
    }
}
