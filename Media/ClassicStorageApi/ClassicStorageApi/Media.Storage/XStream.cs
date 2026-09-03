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







namespace AvePoint.Media.ClassicStorage
{
    #region using directives
    using System;
    using System.IO;
    #endregion

    public abstract class XStream : Stream
    {
        public AbstractXSystem System { get; set; }
        public int ReadLength { get; set; }
        public StorageInfo Info { get; set; }
        public XURIResult URI { get; set; }

        public int MaxRetryCount { get; set; }

        public BufferedStream InnerStream { get; set; }

        private bool isCommitStream = false;

        public virtual bool IsCommitStream
        {
            get { return isCommitStream; }
            set { isCommitStream = value; }
        }

        protected XStream() 
        {
            this.MaxRetryCount = 6;
        
        }

        protected XStream(AbstractXSystem sys)
        {
            System = sys;
            URI = new XURIResult();
            this.MaxRetryCount = 6;
           
        }

        public bool IsCommited { get; set; }


        public virtual void BeginRead(StorageInfo info)
        {
            //do nothing, if need, plz override in sub class
        }

        public virtual void EndRead()
        {
            //do nothing, if need, plz override in sub class
        }

         
        public virtual StorageResult Commit()
        {
            return Commit(false);
        }

        public virtual StorageResult Commit(bool closeParent)
        {
            StorageResult rs = new StorageResult();
            rs.PdId = System.SystemID;
            rs.NeedCommit = true;
            return rs;
        }

        /// <summary>
        /// 获取资源定位器
        /// </summary>
        public virtual XURIResult GetURI()
        {
            throw new InvalidOperationException("Not Implement In This Layer.");
        }

        public override bool CanRead
        {
            get { throw new NotImplementedException(); }
        }

        public override bool CanSeek
        {
            get { throw new NotImplementedException(); }
        }

        public override bool CanWrite
        {
            get { throw new NotImplementedException(); }
        }



        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public virtual void ClosedUnmoral()
        {
        }

        public virtual void Abort()
        { }
    }
}
