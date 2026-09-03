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
using AvePoint.Wrapper.Common.Office;
using Microsoft.Office.Server.Search.Administration.Query;

namespace AvePoint.ObjectModel.Server19.Office
{
    class AveOSource:IAveOSource
    {
        private Source mSource;
        public AveOSource(Source s)
        {
            this.mSource = s;
        }

        public bool Active
        {
            get { return this.mSource.Active; }
        }

        public bool BuiltIn
        {
            get { return this.mSource.BuiltIn; }
        }

        public int ConnectionTimeout
        {
            get { return this.mSource.ConnectionTimeout; }
            set { this.mSource.ConnectionTimeout = value; }
        }

        public string ConnectionUrlTemplate
        {
            get { return this.mSource.ConnectionUrlTemplate; }
            set { this.mSource.ConnectionUrlTemplate = value; }
        }

        public DateTime CreatedDate
        {
            get { return this.mSource.CreatedDate; }
        }

        public string Description
        {
            get { return this.mSource.Description; }
            set { this.mSource.Description = value; }
        }

        public bool HasPermissionToReadAuthInfo
        {
            get { return this.mSource.HasPermissionToReadAuthInfo; }
        }

        public Guid Id
        {
            get { return this.mSource.Id; }
        }

        public int IndexOffset
        {
            get { return this.mSource.IndexOffset; }
            set { this.mSource.IndexOffset = value; }
        }

        public DateTime LastModifiedDate
        {
            get { return this.mSource.LastModifiedDate; }
        }

        public int MaximumResponseLength
        {
            get { return this.mSource.MaximumResponseLength; }
            set { this.mSource.MaximumResponseLength = value; }
        }

        public string Name
        {
            get { return this.mSource.Name; }
            set { this.mSource.Name = value; }
        }

        public Guid ProviderId
        {
            get { return this.mSource.ProviderId; }
            set { this.mSource.ProviderId = value; }
        }

        public void Activate()
        {
            this.mSource.Activate();
        }

        public bool CanEdit()
        {
            return this.mSource.CanEdit();
        }

        public void Commit()
        {
            this.mSource.Commit();
        }

        public void Deactivate()
        {
            this.mSource.Deactivate();
        }

        public void ImportFromFederatedLocation(string filePath)
        {
            this.mSource.ImportFromFederatedLocation(filePath);
        }


        public Wrapper.Common.IAveQueryTransform QueryTransform
        {
            get { return new AveQueryTransform(mSource.QueryTransform); }
        }
    }
}
