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





namespace AvePoint.Wrapper.Mapping
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AvePoint.Wrapper.Common;

    internal class AveCustomContentTypeMapping : IAveCustomContentTypeMapping
    {
        private Dictionary<string, string> contentTypeNameMappingFromGui = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public AveCustomContentTypeInfo GetMappingContentTypeBeforeAdd(string srcName)
        {
            return null;
        }
        #region contentTypeNameMappingFromGui

        public void SetContentTypeNameMappingFromGui(Dictionary<string, string> customMapping)
        {
            contentTypeNameMappingFromGui = customMapping;
        }

        public string GetContentTypeNameMappingFromGui(string srcCTName)
        {
            string temp = contentTypeNameMappingFromGui.GetValueWithLock(srcCTName);
            if (String.IsNullOrEmpty(temp))
            {
                temp = srcCTName;
            }
            return temp;


            //return contentTypeNameMappingFromGui.GetValueWithLock(srcCTName);
        }
        #endregion
        public void Dispose()
        {
            if (contentTypeNameMappingFromGui != null)
            {
                contentTypeNameMappingFromGui = null;
            }
        }
    }
}
