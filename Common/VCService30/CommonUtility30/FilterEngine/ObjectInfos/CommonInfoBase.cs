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




namespace AvePoint.Common.FilterEngine
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class CommonInfoBase : ObjectInfoBase
    {

        public string Title { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
        public string Name { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
        public DateTime Modified { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
        public DateTime Created { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
        public string ModifiedByLogonName { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
        public string ModifiedByTitle { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
        public string CreatedByLogonName { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
        public string CreatedByTitle { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
        public string ListType { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
        public bool IsStub { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
        public DateTime StubCreated { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
        public DateTime StubLastAccessTime { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
        public DateTime LastAccessCompatibleModifiedTime { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
        public DateTime AccessTime { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
        public string ModifiedByEmail { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); } //SAAS-10859 添加对email格式的支持
        public string CreateByEmail { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); } //SAAS-10859 添加对email格式的支持
    }
}
