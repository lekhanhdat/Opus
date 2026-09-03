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

namespace AvePoint.Wrapper.Common
{
    [Serializable]
    public class AveUserCustomActionInfo
    {
        public string CommandUIExtension { get; set; }
        public string Description { get; set; }
        public string Group { get; set; }
        public Guid Id { get; set; }
        public string ImageUrl { get; set; }
        public string Location { get; set; }
        public string Name { get; set; }
        public string RegistrationId { get; set; }
        public AveUserCustomActionRegistrationType RegistrationType { get; set; }
        public AveBasePermissions Rights { get; set; }
        public string ScriptBlock { get; set; }
        public string ScriptSrc { get; set; }
        public int Sequence { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
    }
}
