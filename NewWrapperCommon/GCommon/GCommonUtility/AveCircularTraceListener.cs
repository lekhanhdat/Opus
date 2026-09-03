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
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace AvePoint.GCommon
{

    public class AveCircularTraceListener : XmlWriterTraceListener
    {

        static CircularStream m_stream = null;
        bool MaxQuotaInitialized = false;
        const string FileQuotaAttribute = "maxFileSizeKB";
        const long DefaultMaxQuota = 2;
        const string DefaultTraceFile = "logs/DocAve-Control-Exception.log";
        int maxSizeRollBackups = 5;
        string[] FPath;
        #region Member Functions

        private long MaxQuotaSize
        {


            get
            {
                long MaxFileQuota = 0;
                //if (!this.MaxQuotaInitialized)
                //{
                //    try
                //    {
                //        string MaxQuotaOption = this.Attributes[AveCircularTraceListener.FileQuotaAttribute];
                //        if (MaxQuotaOption == null)
                //        {
                //            MaxFileQuota = DefaultMaxQuota;
                //        }
                //        else
                //        {
                //            MaxFileQuota = int.Parse(MaxQuotaOption, CultureInfo.InvariantCulture);
                //        }
                //    }
                //    catch (Exception)
                //    {
                //        MaxFileQuota = DefaultMaxQuota;
                //    }
                //    finally
                //    {
                //        this.MaxQuotaInitialized = true;
                //    }
                //}

                //if (MaxFileQuota <= 0)
                //{
                //    MaxFileQuota = DefaultMaxQuota;
                //}

                //MaxFileQuota is in MB in the configuration file, convert to bytes

                MaxFileQuota = DefaultMaxQuota * 1024 * 1024;
                return MaxFileQuota;
            }
        }

        private int MaxSizeRollBackups
        {
            get
            {
                return 5;
            }

        }

        public void MoveOldData()
        {
            if (FPath != null)
            {
                int length = FPath.Length;
                for (int i = length - 2; i >= 0; i--)
                {
                    if (File.Exists(FPath[i]))
                    {
                        if (File.Exists(FPath[i + 1]))
                        {
                            File.Delete(FPath[i + 1]);
                        }
                        File.Move(FPath[i], FPath[i + 1]);
                    }
                }
            }
        }
        private void DetermineOverQuota()
        {
            if (!this.MaxQuotaInitialized)
            {
                m_stream.MaxQuotaSize = this.MaxQuotaSize;
            }

            if (m_stream.IsOverQuota)
            {
                base.Flush();
                m_stream.SwitchFilesBegin();
                MoveOldData();
                m_stream.SwitchFilesEnd();
            }
        }

        private void InitFPath(string file)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = basePath + Path.GetDirectoryName(file);
            string fileBase = Path.GetFileNameWithoutExtension(file);
            string fileExt = Path.GetExtension(file);
            FPath = new string[MaxSizeRollBackups];
            FPath[0] = Path.Combine(filePath, fileBase + fileExt);
            for (int i = 1; i < MaxSizeRollBackups; i++)
            {
                FPath[i] = Path.Combine(filePath, fileBase + fileExt + "." + i);
            }
        }
        #endregion

        #region XmlWriterTraceListener Functions

        public AveCircularTraceListener(string file)
            : base(m_stream = new CircularStream(file))
        {
            InitFPath(file);
        }

        public AveCircularTraceListener()
            : base(m_stream = new CircularStream(DefaultTraceFile))
        {
            InitFPath(DefaultTraceFile);
        }

        protected override string[] GetSupportedAttributes()
        {
            return new string[] { AveCircularTraceListener.FileQuotaAttribute };
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            DetermineOverQuota();
            base.TraceData(eventCache, source, eventType, id, data);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            DetermineOverQuota();
            base.TraceEvent(eventCache, source, eventType, id);
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
            DetermineOverQuota();
            base.TraceData(eventCache, source, eventType, id, data);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            DetermineOverQuota();
            base.TraceEvent(eventCache, source, eventType, id, format, args);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            DetermineOverQuota();
            base.TraceEvent(eventCache, source, eventType, id, message);
        }

        public override void TraceTransfer(TraceEventCache eventCache, string source, int id, string message, Guid relatedActivityId)
        {
            DetermineOverQuota();
            base.TraceTransfer(eventCache, source, id, message, relatedActivityId);

        }

        #endregion

    }

    public class CircularStream : System.IO.Stream
    {
        private FileStream FStream = null;
        private long DataWritten = 0;
        private long FileQuota = 0;
        private string CurrentFile;
        private string stringWritten = string.Empty;
        private int maxSizeRollBackups;
        public int MaxSizeRollBackups
        {
            get
            {
                return maxSizeRollBackups;
            }
            set
            {
                maxSizeRollBackups = value;
            }
        }

        public CircularStream(string FileName)
        {
            try
            {
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                string filePath = basePath + Path.GetDirectoryName(FileName);
                string fileBase = Path.GetFileNameWithoutExtension(FileName);
                string fileExt = Path.GetExtension(FileName);
                CurrentFile = Path.Combine(filePath, fileBase + fileExt);
                FStream = new FileStream(CurrentFile, FileMode.Create);
            }
            catch (Exception e) { Trace.TraceWarning(e.ToString()); }

        }

        public long MaxQuotaSize
        {
            get
            {
                return FileQuota;
            }
            set
            {
                FileQuota = value;
            }
        }


        public void SwitchFilesBegin()
        {
            try
            {
                DataWritten = 0;
                FStream.Close();
            }
            catch (Exception e) { Trace.TraceWarning(e.ToString()); }
        }

        public void SwitchFilesEnd()
        {
            FStream = new FileStream(CurrentFile, FileMode.Create);
        }

        public bool IsOverQuota
        {
            get
            {
                return (DataWritten >= FileQuota);
            }

        }

        public override bool CanRead
        {
            get
            {
                try
                {
                    return FStream.CanRead;
                }
                catch (Exception e)
                {
                    Trace.TraceWarning(e.ToString());
                    return true;
                }
            }
        }

        public override bool CanSeek
        {
            get
            {
                try
                {
                    return FStream.CanSeek;
                }
                catch (Exception e)
                {
                    Trace.TraceWarning(e.ToString());
                    return false;
                }
            }
        }

        public override long Length
        {
            get
            {
                try
                {
                    return FStream.Length;
                }
                catch (Exception e)
                {
                    Trace.TraceWarning(e.ToString());
                    return -1;
                }
            }
        }

        public override long Position
        {
            get
            {
                try
                {
                    return FStream.Position;
                }
                catch (Exception e)
                {
                    Trace.TraceWarning(e.ToString());
                    return -1;
                }
            }
            set
            {
                try
                {
                    FStream.Position = Position;
                }
                catch (Exception e) { Trace.TraceWarning(e.ToString()); }
            }
        }

        public override bool CanWrite
        {
            get
            {
                try
                {
                    return FStream.CanWrite;
                }
                catch (Exception e)
                {
                    Trace.TraceWarning(e.ToString());
                    return true;
                }
            }
        }

        public override void Flush()
        {
            try
            {
                FStream.Flush();
            }
            catch (Exception e) { Trace.TraceWarning(e.ToString()); }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            try
            {
                return FStream.Seek(offset, origin);
            }
            catch (Exception e)
            {
                Trace.TraceWarning(e.ToString());
                return -1;
            }
        }

        public override void SetLength(long value)
        {
            try
            {
                FStream.SetLength(value);
            }
            catch (Exception e) { Trace.TraceWarning(e.ToString()); }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            try
            {
                //Write to current file

                FStream.Write(buffer, offset, count);
                DataWritten += count;

            }
            catch (Exception e) { Trace.TraceWarning(e.ToString()); }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            try
            {
                return FStream.Read(buffer, offset, count);
            }
            catch (Exception e)
            {
                Trace.TraceWarning(e.ToString());
                return -1;
            }
        }

        public override void Close()
        {
            try
            {
                FStream.Close();
            }
            catch (Exception e) { Trace.TraceWarning(e.ToString()); }
        }


    }

}
