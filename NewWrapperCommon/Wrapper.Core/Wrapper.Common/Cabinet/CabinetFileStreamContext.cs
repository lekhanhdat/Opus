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

namespace AvePoint.Wrapper.Common
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;

    internal class CabinetFileStreamContext : ICabinetCreateStreamContext, ICabinetExtractStreamContext
    {
        private IList<string> cabinetFiles;
        private string directory;
        private bool enableOffsetOpen;
        private bool extractOnlyNewerFiles;
        private IDictionary<string, string> files;

        public CabinetFileStreamContext(string cabinetFile) : this(cabinetFile, null, null)
        {
        }

        public CabinetFileStreamContext(IList<string> cabinetFiles, string directory, IDictionary<string, string> files)
        {
            if ((cabinetFiles == null) || (cabinetFiles.Count == 0))
            {
                throw new ArgumentNullException("cabinetFiles");
            }
            this.cabinetFiles = cabinetFiles;
            this.directory = directory;
            this.files = files;
        }

        public CabinetFileStreamContext(string cabinetFile, string directory, IDictionary<string, string> files) : this(new string[] { cabinetFile }, directory, files)
        {
            if (cabinetFile == null)
            {
                throw new ArgumentNullException("cabinetFile");
            }
        }

        public virtual void CloseCabinetReadStream(int cabinetNumber, string cabinetName, Stream stream)
        {
            if (stream != null)
            {
                stream.Close();
            }
        }

        public virtual void CloseCabinetWriteStream(int cabinetNumber, string cabinetName, Stream stream)
        {
            if (stream != null)
            {
                stream.Close();
            }
        }

        public virtual void CloseFileReadStream(string path, Stream stream)
        {
            if (stream != null)
            {
                stream.Close();
            }
        }

        public virtual void CloseFileWriteStream(string path, Stream stream, FileAttributes attributes, DateTime lastWriteTime)
        {
            if (stream != null)
            {
                stream.Close();
            }
            string fileName = this.TranslateFilePath(path);
            if (fileName != null)
            {
                FileInfo info = new FileInfo(fileName);
                if (lastWriteTime != DateTime.MinValue)
                {
                    try
                    {
                        info.LastWriteTime = lastWriteTime;
                    }
                    catch (ArgumentException)
                    {
                    }
                    catch (IOException)
                    {
                    }
                }
                try
                {
                    info.Attributes = attributes;
                }
                catch (IOException)
                {
                }
            }
        }

        public virtual string GetCabinetName(int cabinetNumber)
        {
            if (cabinetNumber < this.cabinetFiles.Count)
            {
                return Path.GetFileName(this.cabinetFiles[cabinetNumber]);
            }
            return string.Empty;
        }

        public virtual Stream OpenCabinetReadStream(int cabinetNumber, string cabinetName)
        {
            if (cabinetNumber >= this.cabinetFiles.Count)
            {
                return null;
            }
            string path = this.cabinetFiles[cabinetNumber];
            Stream source = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (this.enableOffsetOpen)
            {
                using (CabinetExtractor extractor = new CabinetExtractor())
                {
                    long offset = extractor.FindCabinetOffset(new DuplicateStream(source));
                    if (offset > 0L)
                    {
                        source = new OffsetStream(source, offset);
                    }
                }
            }
            return source;
        }

        public virtual Stream OpenCabinetWriteStream(int cabinetNumber, string cabinetName)
        {
            if (cabinetNumber >= this.cabinetFiles.Count)
            {
                return null;
            }
            string path = this.cabinetFiles[cabinetNumber];
            Stream source = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            if (this.enableOffsetOpen)
            {
                using (CabinetExtractor extractor = new CabinetExtractor())
                {
                    long offset = extractor.FindCabinetOffset(new DuplicateStream(source));
                    if (offset < 0L)
                    {
                        offset = source.Length;
                    }
                    if (offset > 0L)
                    {
                        source = new OffsetStream(source, offset);
                    }
                }
            }
            source.SetLength(0L);
            return source;
        }

        public virtual Stream OpenFileReadStream(string path, out FileAttributes attributes, out DateTime lastWriteTime)
        {
            string str = this.TranslateFilePath(path);
            if (str == null)
            {
                attributes = FileAttributes.Normal;
                lastWriteTime = DateTime.Now;
                return null;
            }
            attributes = File.GetAttributes(str);
            lastWriteTime = File.GetLastWriteTime(str);
            return File.Open(str, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public virtual Stream OpenFileWriteStream(string path, long fileSize, DateTime lastWriteTime)
        {
            string fileName = this.TranslateFilePath(path);
            if (fileName == null)
            {
                return null;
            }
            FileInfo info = new FileInfo(fileName);
            if (info.Exists)
            {
                if ((this.extractOnlyNewerFiles && (lastWriteTime != DateTime.MinValue)) && (info.LastWriteTime >= lastWriteTime))
                {
                    return null;
                }
                if ((info.Attributes & FileAttributes.ReadOnly) != 0)
                {
                    info.Attributes &= ~FileAttributes.ReadOnly;
                }
            }
            if (!info.Directory.Exists)
            {
                info.Directory.Create();
            }
            return File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
        }

        private string TranslateFilePath(string path)
        {
            string str;
            if (this.files != null)
            {
                str = this.files[path];
            }
            else
            {
                str = path;
            }
            if ((str != null) && (this.directory != null))
            {
                str = Path.Combine(this.directory, str);
            }
            return str;
        }

        public IList<string> CabinetFiles
        {
            get
            {
                return this.cabinetFiles;
            }
            set
            {
                if ((value == null) || (value.Count == 0))
                {
                    throw new ArgumentNullException("value");
                }
                this.cabinetFiles = value;
            }
        }

        public string Directory
        {
            get
            {
                return this.directory;
            }
            set
            {
                this.directory = value;
            }
        }

        public bool EnableOffsetOpen
        {
            get
            {
                return this.enableOffsetOpen;
            }
            set
            {
                this.enableOffsetOpen = value;
            }
        }

        public bool ExtractOnlyNewerFiles
        {
            get
            {
                return this.extractOnlyNewerFiles;
            }
            set
            {
                this.extractOnlyNewerFiles = value;
            }
        }

        public IDictionary<string, string> Files
        {
            get
            {
                return this.files;
            }
            set
            {
                this.files = value;
            }
        }
    }
}

