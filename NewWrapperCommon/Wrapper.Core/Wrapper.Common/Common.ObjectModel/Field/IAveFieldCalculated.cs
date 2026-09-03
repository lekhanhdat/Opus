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

namespace AvePoint.Wrapper.Common
{
    public interface IAveFieldCalculated : IAveField
    {
        AveDateTimeFieldFormatType DateFormat { get; set; } 
        /// <summary>
        /// Formula属性与FieldRefsXml是存在关联关系的，并不单纯是一个String。在备份、还原Formula属性的时候需要对FieldRefsXml属性也做相应的处理。
        /// </summary>
        string Formula { get; set; }        
        AveFieldType OutputType { get; set; }
        AveNumberFormatTypes DisplayFormat { get; set; }
        bool ShowAsPercentage { get; set; }
        int CurrencyLocaleId { get; set; }
        /// <summary>
        /// 以Xml储存了Formula 属性中关联的Field name与Field Id的Mapping关系，不进行处理Formula属性将出现界面显示异常。
        /// </summary>
        String FieldRefsXml { get; set; }
    }    
}
