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

namespace AvePoint.ObjectModel.Common
{
    class AvePageLayout : IAvePageLayout
    {
        #region IAvePageLayout Members

        public string Name
        {
            //在server上实现，Client没有实现,返回默认值
            get { return default(string); }
        }

        public object PageLayout
        {
            //在server上实现，Client没有实现,返回默认值
            get { return default(object); }
        }

        public string Title
        {
            //在server上实现，Client没有实现,返回默认值
            get
            {
                return default(string);
            }
            set
            {

            }
        }

        #endregion
    }
}
