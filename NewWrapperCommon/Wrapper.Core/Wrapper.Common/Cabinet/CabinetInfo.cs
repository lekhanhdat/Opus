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
    using System.Globalization;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Text.RegularExpressions;

    [Serializable]
    public sealed class CabinetInfo : FileSystemInfo
    {
        public CabinetInfo(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            base.OriginalPath = path;
            base.FullPath = Path.GetFullPath(path);
        }

        private CabinetInfo(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public void CompressDirectory(string sourceDirectory)
        {
            this.CompressDirectory(sourceDirectory, false, CabinetCompressionLevel.Max, null);
        }

        public void CompressDirectory(string sourceDirectory, bool includeSubdirectories, CabinetCompressionLevel compLevel, EventHandler<CabinetProgressEventArgs> progressHandler)
        {
            IList<string> relativeFilePathsInDirectoryTree = this.GetRelativeFilePathsInDirectoryTree(sourceDirectory, includeSubdirectories);
            this.CompressFiles(sourceDirectory, relativeFilePathsInDirectoryTree, relativeFilePathsInDirectoryTree, compLevel, progressHandler);
        }

        public void CompressFiles(string sourceDirectory, IList<string> sourceFileNames, IList<string> fileNames)
        {
            this.CompressFiles(sourceDirectory, sourceFileNames, fileNames, CabinetCompressionLevel.Max, null);
        }

        public void CompressFiles(string sourceDirectory, IList<string> sourceFileNames, IList<string> fileNames, CabinetCompressionLevel compLevel, EventHandler<CabinetProgressEventArgs> progressHandler)
        {
            if (sourceFileNames == null)
            {
                throw new ArgumentNullException("sourceFileNames");
            }
            if (fileNames == null)
            {
                string[] strArray = new string[sourceFileNames.Count];
                for (int i = 0; i < sourceFileNames.Count; i++)
                {
                    strArray[i] = Path.GetFileName(sourceFileNames[i]);
                }
                fileNames = strArray;
            }
            else if (fileNames.Count != sourceFileNames.Count)
            {
                throw new ArgumentOutOfRangeException("fileNames");
            }
            using (CabinetCreator creator = new CabinetCreator())
            {
                creator.Progress += progressHandler;
                IDictionary<string, string> files = CreateStringDictionary(fileNames, sourceFileNames);
                CabinetFileStreamContext streamContext = new CabinetFileStreamContext(this.FullName, sourceDirectory, files);
                streamContext.EnableOffsetOpen = true;
                creator.CompressionLevel = compLevel;
                creator.Create(streamContext, fileNames);
            }
        }

        public void CompressFileSet(string sourceDirectory, IDictionary<string, string> fileNames)
        {
            this.CompressFileSet(sourceDirectory, fileNames, CabinetCompressionLevel.Max, null);
        }

        public void CompressFileSet(string sourceDirectory, IDictionary<string, string> fileNames, CabinetCompressionLevel compLevel, EventHandler<CabinetProgressEventArgs> progressHandler)
        {
            if ((fileNames == null) || (fileNames.Keys == null))
            {
                throw new ArgumentNullException("fileNames");
            }
            string[] array = new string[fileNames.Count];
            fileNames.Keys.CopyTo(array, 0);
            using (CabinetCreator creator = new CabinetCreator())
            {
                creator.Progress += progressHandler;
                CabinetFileStreamContext streamContext = new CabinetFileStreamContext(this.FullName, sourceDirectory, fileNames);
                streamContext.EnableOffsetOpen = true;
                creator.CompressionLevel = compLevel;
                creator.Create(streamContext, array);
            }
        }

        public void CopyTo(string destFileName)
        {
            File.Copy(this.FullName, destFileName);
        }

        public void CopyTo(string destFileName, bool overwrite)
        {
            File.Copy(this.FullName, destFileName, overwrite);
        }

        private static IDictionary<string, string> CreateStringDictionary(IList<string> keys, IList<string> values)
        {
            IDictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < keys.Count; i++)
            {
                dictionary.Add(keys[i], values[i]);
            }
            return dictionary;
        }

        public override void Delete()
        {
            File.Delete(this.FullName);
        }

        public void ExtractAll(string destDirectory)
        {
            this.ExtractAll(destDirectory, null);
        }

        public void ExtractAll(string destDirectory, EventHandler<CabinetProgressEventArgs> progressHandler)
        {
            using (CabinetExtractor extractor = new CabinetExtractor())
            {
                extractor.Progress += progressHandler;
                CabinetFileStreamContext streamContext = new CabinetFileStreamContext(this.FullName, destDirectory, null);
                streamContext.EnableOffsetOpen = true;
                extractor.Extract(streamContext, false, null);
            }
        }

        public void ExtractFile(string fileName, string destFileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            if (destFileName == null)
            {
                throw new ArgumentNullException("destFileName");
            }
            this.ExtractFiles(new string[] { fileName }, null, new string[] { destFileName });
        }

        public void ExtractFiles(IList<string> fileNames, string destDirectory, IList<string> destFileNames)
        {
            this.ExtractFiles(fileNames, destDirectory, destFileNames, null);
        }

        public void ExtractFiles(IList<string> fileNames, string destDirectory, IList<string> destFileNames, EventHandler<CabinetProgressEventArgs> progressHandler)
        {
            if (fileNames == null)
            {
                throw new ArgumentNullException("fileNames");
            }
            if (destFileNames == null)
            {
                if (destDirectory == null)
                {
                    throw new ArgumentNullException("destFileNames");
                }
                destFileNames = fileNames;
            }
            if (destFileNames.Count != fileNames.Count)
            {
                throw new ArgumentOutOfRangeException("destFileNames");
            }
            IDictionary<string, string> dictionary = CreateStringDictionary(fileNames, destFileNames);
            this.ExtractFileSet(dictionary, destDirectory, progressHandler);
        }

        public void ExtractFileSet(IDictionary<string, string> fileNames, string destDirectory)
        {
            this.ExtractFileSet(fileNames, destDirectory, null);
        }

        public void ExtractFileSet(IDictionary<string, string> fileNames, string destDirectory, EventHandler<CabinetProgressEventArgs> progressHandler)
        {
            Predicate<string> fileFilter = null;
            if (fileNames == null)
            {
                throw new ArgumentNullException("fileNames");
            }
            using (CabinetExtractor extractor = new CabinetExtractor())
            {
                extractor.Progress += progressHandler;
                CabinetFileStreamContext streamContext = new CabinetFileStreamContext(this.FullName, destDirectory, fileNames);
                streamContext.EnableOffsetOpen = true;
                if (fileFilter == null)
                {
                    fileFilter = delegate (string match) {
                        return fileNames.ContainsKey(match);
                    };
                }
                extractor.Extract(streamContext, false, fileFilter);
            }
        }

        internal CabinetFileInfo GetFile(string path)
        {
            IList<CabinetFileInfo> files = this.InternalGetFiles(delegate (string match) {
                return string.Compare(match, path,StringComparison.OrdinalIgnoreCase) == 0;
            });
            if ((files != null) && (files.Count > 0))
            {
                return files[0];
            }
            return null;
        }

        public IList<CabinetFileInfo> GetFiles()
        {
            return this.InternalGetFiles(null);
        }

        public IList<CabinetFileInfo> GetFiles(string searchPattern)
        {
            if (searchPattern == null)
            {
                throw new ArgumentNullException();
            }
            string pattern = string.Format(CultureInfo.InvariantCulture, "^{0}$", new object[] { Regex.Escape(searchPattern).Replace(@"\*", ".*").Replace(@"\?", ".") });
            Regex regex = new Regex(pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            return this.InternalGetFiles(delegate (string match) {
                return regex.IsMatch(match);
            });
        }

        internal IList<string> GetRelativeFilePathsInDirectoryTree(string dir, bool includeSubdirectories)
        {
            IList<string> fileList = new List<string>();
            this.RecursiveGetRelativeFilePathsInDirectoryTree(dir, string.Empty, includeSubdirectories, fileList);
            return fileList;
        }

        private IList<CabinetFileInfo> InternalGetFiles(Predicate<string> fileFilter)
        {
            using (CabinetExtractor extractor = new CabinetExtractor())
            {
                CabinetFileStreamContext streamContext = new CabinetFileStreamContext(this.FullName, null, null);
                IList<CabinetFileInfo> list = extractor.GetFileInfo(streamContext, false, fileFilter);
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].Cabinet = this;
                }
                return list;
            }
        }

        public bool IsValid()
        {
            bool flag;
            using (Stream stream = File.OpenRead(this.FullName))
            {
                using (CabinetExtractor extractor = new CabinetExtractor())
                {
                    flag = extractor.FindCabinetOffset(stream) >= 0L;
                }
            }
            return flag;
        }

        public void MoveTo(string destFileName)
        {
            File.Move(this.FullName, destFileName);
            base.FullPath = Path.GetFullPath(destFileName);
        }

        private void RecursiveGetRelativeFilePathsInDirectoryTree(string dir, string relativeDir, bool includeSubdirectories, IList<string> fileList)
        {
            foreach (string str in System.IO.Directory.GetFiles(dir))
            {
                string fileName = Path.GetFileName(str);
                fileList.Add(Path.Combine(relativeDir, fileName));
            }
            if (includeSubdirectories)
            {
                foreach (string str3 in System.IO.Directory.GetDirectories(dir))
                {
                    string str4 = Path.GetFileName(str3);
                    this.RecursiveGetRelativeFilePathsInDirectoryTree(Path.Combine(dir, str4), Path.Combine(relativeDir, str4), includeSubdirectories, fileList);
                }
            }
        }

        public override string ToString()
        {
            return this.FullName;
        }

        public DirectoryInfo Directory
        {
            get
            {
                return new DirectoryInfo(Path.GetDirectoryName(this.FullName));
            }
        }

        public string DirectoryName
        {
            get
            {
                return Path.GetDirectoryName(this.FullName);
            }
        }

        public override bool Exists
        {
            get
            {
                return File.Exists(this.FullName);
            }
        }

        public long Length
        {
            get
            {
                return new FileInfo(this.FullName).Length;
            }
        }

        public override string Name
        {
            get
            {
                return Path.GetFileName(this.FullName);
            }
        }
    }
}

