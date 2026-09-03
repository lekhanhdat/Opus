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


using System.Net;
using System.IO;

namespace AveClientRequest.Common
{
    public class AveHttpWebResponse : WebResponse
    {
        private WebResponse m_HttpWebResponse;
        DataMonitor m_DataMonitor = null;
        public DataMonitor DataMonitor
        {
            get
            {
                if (m_DataMonitor == null)
                {
                    this.m_DataMonitor = new DataMonitor();
                }
                return this.m_DataMonitor;
            }
        }
        
        public AveHttpWebResponse(WebResponse webResponse, DataMonitor dataMonitor)
        {            
            m_HttpWebResponse = webResponse;
            m_DataMonitor = dataMonitor;
        }
        public override void Close()
        {
            m_HttpWebResponse.Close();
        }
        public override Stream GetResponseStream()
        {
            //this.DataMonitor.ByteReceive += this.Headers.ToString().Length;
            Stream stream = m_HttpWebResponse.GetResponseStream();
            return new AveWebStream(stream, m_DataMonitor);
        }
        public override long ContentLength
        {
            get
            {
                return this.m_HttpWebResponse.ContentLength;
            }
            set
            {
                this.m_HttpWebResponse.ContentLength = value;
            }
        }
        public override string ContentType
        {
            get
            {
                return this.m_HttpWebResponse.ContentType;
            }
            set
            {
                this.m_HttpWebResponse.ContentType = value;
            }
        }
        public override WebHeaderCollection Headers
        {
            get
            {
                return this.m_HttpWebResponse.Headers;
            }
        }
        public override bool IsFromCache
        {
            get
            {
                return this.m_HttpWebResponse.IsFromCache;
            }
        }
        public override bool IsMutuallyAuthenticated
        {
            get
            {
                return this.m_HttpWebResponse.IsMutuallyAuthenticated;
            }
        }
        public override System.Uri ResponseUri
        {
            get
            {
                return this.m_HttpWebResponse.ResponseUri;
            }
        }
    }
}
