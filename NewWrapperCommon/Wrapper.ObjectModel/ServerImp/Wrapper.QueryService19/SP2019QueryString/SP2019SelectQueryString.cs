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
using System.Threading.Tasks;

namespace AvePoint.Wrapper.QueryService
{
    internal class SP2019SelectQueryString
    {
        public const string GetItemInRecyclebinById_SELECT_AllUserData = @"select tp_ID,tp_DeleteTransactionId FROM AllUserData With(nolock) where tp_SiteId=@SiteId AND tp_ListId =@ListId and tp_DeleteTransactionId<>0x and tp_IsCurrentVersion =1 and tp_ID=@TP_ID and tp_CalculatedVersion=0 and tp_Level>0 and tp_RowOrdinal=0";
        
        public const string GetItemInRecyclebinByDeleteId_SELECT_RecycleBin = @"select EffectiveDeleteTransactionId FROM RecycleBin With(nolock) where SiteId=@SiteId and DeleteTransactionId =@DeleteTransactionId and BinId<=2";

        public const string GetItemInRecyclebinByTitle_SELECT_AllUserData = @"select tp_ID,tp_DeleteTransactionId FROM AllUserData With(nolock) where tp_SiteId=@SiteId and tp_ListId =@ListId and tp_DeleteTransactionId<>0x and tp_IsCurrentVersion =1 and nvarchar1=@title and tp_CalculatedVersion=0 and tp_RowOrdinal=0";
    }
}
