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
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Publishing;

namespace AvePoint.ObjectModel.Server19
{
    public class AveImageRendition : IAveImageRendition
    {
        internal ImageRendition mageRendition;

        public AveImageRendition(ImageRendition mageRendition)
        {
            this.mageRendition = mageRendition;
        }

        public int Id
        {
            get
            {
                return this.mageRendition.Id;
            }
        }

        public string Name
        {
            get
            {
                return this.mageRendition.Name;
            }
            set
            {
                this.mageRendition.Name = value;
            }
        }

        public int Height
        {
            get
            {
                return this.mageRendition.Height;
            }
            set
            {
                this.mageRendition.Height = value;
            }
        }

        public int Width
        {
            get
            {
                return this.mageRendition.Width;
            }
            set
            {
                this.mageRendition.Width = value;
            }
        }

        public bool IsValid
        {
            get { return this.mageRendition.IsValid; }
        }

        public int Version
        {
            get { return this.mageRendition.Version; }
        }
    }
}
