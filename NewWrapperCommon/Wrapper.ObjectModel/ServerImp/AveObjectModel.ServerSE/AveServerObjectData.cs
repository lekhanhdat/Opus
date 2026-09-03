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



using System.Collections.Generic;

namespace AvePoint.ObjectModel.ServerSE
{
    public sealed class AveServerObjectData
    {
        private Dictionary<string, object> m_PropertiesCache;
        private Dictionary<string, object> m_MethodReturnCache;

        public AveServerObjectData()
        {
            m_PropertiesCache = new Dictionary<string, object>();
            m_MethodReturnCache = new Dictionary<string, object>();
        }

        public Dictionary<string, object> PropertiesCache
        {
            get { return m_PropertiesCache; }
            set { m_PropertiesCache = value; }
        }

        public Dictionary<string, object> MethodReturnCache
        {
            get { return m_MethodReturnCache; }
            set { m_MethodReturnCache = value; }
        }

        public bool IsPropertyNotLoaded(string propertyName)
        {
            return !m_PropertiesCache.ContainsKey(propertyName);
        }

    }
}
