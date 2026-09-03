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
using System.IO;
using System.Text;
using AvePoint.GCommon.Utility;
using log4net.Appender;

namespace AvePoint.GCommon
{
    public class MultiTenantFileLocker : FileAppender.MinimalLock
    {
        public const string CommonFolder = "common";
        private string m_filename;        
        private bool m_append;        
        private FileStream m_stream = null;        

        public override void OpenFile(string filename, bool append, Encoding encoding)
        {
            m_filename = filename;
            m_append = append;            
        }

        public override Stream AcquireLock()
        {       
            try
            {
                
                if (m_stream == null)            
                {
                    m_stream = this.CreateStream(FileName, m_append, FileShare.Read) as FileStream;
                    m_append = true;                                                    
                }
            }
            catch (Exception e1)
            {
                CurrentAppender.ErrorHandler.Error("Unable to acquire lock on file " + m_filename + ". " + e1.Message);
            }
            return m_stream;
        }      

        protected Stream CreateStream(string filename, bool append, FileShare fileShare)
        {
            using (CurrentAppender.SecurityContext.Impersonate(this))
            {
                string directoryFullName = Path.GetDirectoryName(filename);
             
                if (!Directory.Exists(directoryFullName))
                {
                    Directory.CreateDirectory(directoryFullName);
                }                
                FileMode fileOpenMode = append ? FileMode.Append : FileMode.Create;
                return new FileStream(filename, fileOpenMode, FileAccess.Write, fileShare);
            }
        }

        public override void ReleaseLock()
        {            
            this.Count = m_stream.Length;
            CloseStream(m_stream);                            
            m_stream = null;
        }

        protected void CloseStream(Stream stream)
        {
            using (CurrentAppender.SecurityContext.Impersonate(this))
            {
                stream.Close();
            }
        }

        public string FileName
        {
            get
            {                
                //string tenantIdentity = CallContext.LogicalGetData("TenantIdentity") as string;
                //string jobId = CallContext.LogicalGetData("ThreadJobId") as string;
                //if (string.IsNullOrEmpty(tenantIdentity))
                //{
                //    tenantIdentity = CommonFolder;
                //}
                //if (string.IsNullOrEmpty(jobId))
                //{
                //    jobId = string.Empty;
                //}
                //else
                //{
                //    jobId = "_" + jobId;
                //}
                return string.Format(m_filename, CommonFolder, string.Empty);
            }
        }

        public long Count
        {
            get;
            set;
        }

        public string BaseFileName
        {
            get { return m_filename; }
        }
    }
}
