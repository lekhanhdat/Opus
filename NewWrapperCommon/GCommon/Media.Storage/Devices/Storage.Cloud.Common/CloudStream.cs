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

namespace AvePoint.Media.Storage.Cloud.Common
{
    #region using directives
    using AvePoint.GCommon.Utility.I18N;
    using AvePoint.Media.Storage.Util;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Reflection;
    #endregion

    class CloudStream : XStream
    {
        public HttpWebRequest HttpWebRequest { set; get; }
        private ushort eventTaskCategory;

        protected ushort EventTaskCategory
        {
            get
            {
                return this.eventTaskCategory;
            }
            set
            {
                this.eventTaskCategory = value;
            }
        }

        private ContextValues.Storage.StorageType eventTaskMessage = ContextValues.Storage.StorageType.Cloud;

        protected ContextValues.Storage.StorageType EventTaskMessage
        {
            get
            {
                return this.eventTaskMessage;
            }
            set
            {
                this.eventTaskMessage = value;
            }
        }

        protected void SetEventTaskInfo(IXSystem currentSystem)
        {
            if (currentSystem == null)
            {
                return;
            }
            else
            {
                switch (System.GetType().Name)
                {
                    case "AmazonSystem":
                        eventTaskCategory = EventCategorys.DocAveStorageAPIService.Cloud_Amazon_S3;
                        eventTaskMessage = ContextValues.Storage.StorageType.Amazon;
                        break;
                    case "AtmosSystem":
                        if (currentSystem.XriObject.VIM.Equals("atmos_vim"))
                        {
                            eventTaskCategory = EventCategorys.DocAveStorageAPIService.Cloud_EMC_Atmos;
                            eventTaskMessage = ContextValues.Storage.StorageType.Atmos;
                        }
                        else
                        {
                            eventTaskCategory = EventCategorys.DocAveStorageAPIService.Cloud_ATT_Synaptic;
                            eventTaskMessage = ContextValues.Storage.StorageType.ATT;
                        }
                        break;
                    case "AzureSystem":
                        eventTaskCategory = EventCategorys.DocAveStorageAPIService.Cloud_Windows_Azure;
                        eventTaskMessage = ContextValues.Storage.StorageType.Azure;
                        break;
                    case "RackspaceSystem":
                        eventTaskCategory = EventCategorys.DocAveStorageAPIService.Cloud_Rackspace;
                        eventTaskMessage = ContextValues.Storage.StorageType.Rackspace;
                        break;
                    case "HCPSystem":
                        eventTaskCategory = EventCategorys.DocAveStorageAPIService.HDS_HCP;
                        eventTaskMessage = ContextValues.Storage.StorageType.HCP;
                        break;
                    default:
                        break;
                }
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}
