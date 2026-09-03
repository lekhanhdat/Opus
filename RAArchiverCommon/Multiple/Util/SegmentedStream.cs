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
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//Fortify fix: Path Manipulation

//namespace AvePoint.RA.SharePoint.ArchiverCommon
//{
//    internal sealed class SegmentedStream : Stream
//    {
//        private enum Mode : byte { Write, Read }

//        private Mode mode;
//        private string folderPath;
//        private string fileName;
//        private long length;
//        private int readPos;
//        private int writePos;
//        private bool finishWrite;
//        private FileStream file;

//        public SegmentedStream(string folderPath)
//        {
//            this.folderPath = folderPath;

//            this.mode = Mode.Write;
//            this.length = 0;
//            this.readPos = 0;
//            this.writePos = 0;
//            this.finishWrite = false;

//            InitStream();
//        }

//        private void InitStream()
//        {
//            if (this.file != null) return;

//            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

//            if (this.mode == Mode.Write)
//            {
//                string filePath;

//                do
//                {
//                    this.fileName = string.Format("{0}.avetemp", Guid.NewGuid());
//                    filePath = Path.Combine(this.folderPath, this.fileName);

//                } while (File.Exists(filePath));

//                this.file = new FileStream(filePath, FileMode.CreateNew,
//                    FileAccess.Write, FileShare.Write,
//                    65536, FileOptions.None);
//            }
//            else
//            {
//                var filePath = Path.Combine(this.folderPath, this.fileName);
//                if (File.Exists(filePath) == false)
//                {
//                    throw new InvalidOperationException("Invalid logic. Cannot find the file:" + filePath);
//                }

//                this.file = new FileStream(filePath, FileMode.Open,
//                    FileAccess.Read, FileShare.Read,
//                    65536, FileOptions.DeleteOnClose);
//            }
//        }

//        public void CloseFileWriteHandle()
//        {
//            if (this.mode == Mode.Write)
//            {
//                this.file.Flush();
//                this.file.Dispose();
//                this.file = null;

//                this.mode = Mode.Read;
//            }
//        }

//        public override bool CanRead
//        {
//            get { return this.file.CanRead; }
//        }

//        public override bool CanSeek
//        {
//            get { return this.file.CanSeek; }
//        }

//        public override bool CanWrite
//        {
//            get { return this.file.CanWrite; }
//        }

//        public override void Flush()
//        {
//            this.file.Flush();
//        }

//        public override long Length
//        {
//            get { length = this.file.Length; return length; }
//        }

//        public override long Position
//        {
//            get
//            {
//                return this.file.Position;
//            }
//            set
//            {
//                throw new NotImplementedException();
//            }
//        }

//        public override int Read(byte[] buffer, int offset, int count)
//        {
//            InitStream();
//            return this.file.Read(buffer, offset, count);
//        }

//        public override long Seek(long offset, SeekOrigin origin)
//        {
//            throw new NotImplementedException();
//        }

//        public override void SetLength(long value)
//        {
//            throw new NotImplementedException();
//        }

//        public override void Write(byte[] buffer, int offset, int count)
//        {
//            if (count <= 0) return;

//            InitStream();
//            this.file.Write(buffer, offset, count);
//        }

//        public override void Close()
//        {
//            base.Close();
//            if (this.file != null)
//            {
//                this.file.Dispose();
//                this.file = null;
//            }
//            var filePath = Path.Combine(this.folderPath, this.fileName);
//            try
//            {
//                while (File.Exists(filePath))
//                {
//                    File.Delete(filePath);
//                }
//            }
//            catch (Exception ex)
//            {
//                var message = ex.ToString();
//            }
//        }
//    }
//}
