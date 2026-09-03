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
using Microsoft.Office.Server.Search.Administration.Topology;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.ServerSE.Office
{
    class AveOSearchComponent: IAveOSearchComponent
    {
        private AveSearchComponentType mComponentType;

        private uint mIndexPartitionOrdinal;

        private string mRootDirectory;

        public ISearchComponent SearchComponent { get; private set; }

        public AveSearchComponentType ComponentType
        {
            get { return mComponentType; }
        }

        public AveOSearchComponent(ISearchComponent searchComponet)
        {
            this.SearchComponent = searchComponet;

            if (searchComponet is AdminComponent)
            {
                this.mComponentType = AveSearchComponentType.AdminComponent;
            }
            else if(searchComponet is IndexComponent)
            {
                this.mComponentType = AveSearchComponentType.IndexComponent;
                this.mIndexPartitionOrdinal = (searchComponet as IndexComponent).IndexPartitionOrdinal;
                this.mRootDirectory = (searchComponet as IndexComponent).RootDirectory;
            }
            else if (searchComponet is ContentProcessingComponent)
            {
                this.mComponentType = AveSearchComponentType.ContentProcessingComponent;
            }
            else if (searchComponet is AnalyticsProcessingComponent)
            {
                this.mComponentType = AveSearchComponentType.AnalyticsProcessingComponent;
            }
            else if (searchComponet is QueryProcessingComponent)
            {
                this.mComponentType = AveSearchComponentType.QueryProcessingComponent;
            }
            else if (searchComponet is CrawlComponent)
            {
                this.mComponentType = AveSearchComponentType.CrawlComponent;
            }
        }

        public Guid ComponentId
        {
            get { return this.SearchComponent.ComponentId; }
        }

        public IEnumerable<KeyValuePair<string, string>> GetProperties()
        {
            throw new NotImplementedException();
        }

        public string Name
        {
            get { return this.SearchComponent.Name; }
        }

        public Guid ServerId
        {
            get { return this.SearchComponent.ServerId; }
        }

        public string ServerName
        {
            get { return this.SearchComponent.ServerName; }
        }

        public Guid TopologyId
        {
            get { return this.SearchComponent.TopologyId; }
        }


        public uint IndexPartitionOrdinal
        {
            get { return this.mIndexPartitionOrdinal; }
        }

        public string RootDirectory
        {
            get
            {
                return this.mRootDirectory;
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
