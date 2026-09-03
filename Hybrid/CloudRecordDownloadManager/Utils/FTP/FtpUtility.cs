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
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace CloudRecordDownloadManager.Utils.FTP {

    public class FtpItem {

        public string Name { get; set; }
        public Uri Path { get; set; }
        public bool IsFile { get; set; }
        public List<FtpItem> SubItems { get; set; }
        public Exception Exception { get; set; }

    }

    public static class FtpUtility {

        private const int StreamBufferSize = 1024 * 1024 * 10; // 10 MB
        private const int MaxExecutingActionCount = 5;

        private static FtpWebRequest CreateRequest(Uri ftp, string operation, string user = null, string password = null) {
            var request = (FtpWebRequest) WebRequest.Create(ftp);
            request.Method = operation;
            request.KeepAlive = false;
            request.UseBinary = true;
            if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(password)) request.Credentials = new NetworkCredential(user, password);

            return request;
        }

        private static FtpWebResponse Request(Uri ftp, string operation, string user = null, string password = null) {
            var request = CreateRequest(ftp, operation, user, password);
            return (FtpWebResponse) request.GetResponse();
        }

        public static bool FtpExisted(Uri ftp, out bool isFile, string user = null, string password = null) {
            isFile = false;
            var myName = ftp.Me();
            var myBody = _FtpListDirectory(ftp, user, password);
            if (myBody == null) // not existed
                return false;

            switch (myBody.Count) {
                case 0 when !ftp.Segments.Last().EndsWith("/"): {
                    var folderMe = ftp.Append("/");
                    var myFolderBody = _FtpListDirectory(folderMe, user, password);
                    if (myFolderBody == null) return false;

                    break;
                }
                // one item and with same name, may a file or a same folder, check it out.
                case 1 when myBody.Contains(myName): {
                    var child = ftp.Append(myName);
                    var childBody = _FtpListDirectory(child, user, password);
                    if (childBody == null) isFile = true;

                    break;
                }
            }

            return true;
        }

        public static bool FtpMakeDirectory(Uri ftp, string user = null, string password = null) {
            // self check
            if (FtpExisted(ftp, out var isFile, user, password)) return !isFile;

            // parent check
            var parent = ftp.Parent();
            if (FtpExisted(parent, out var isParentAFile, user, password)) {
                if (isParentAFile) return false;

                using (var response = Request(ftp, WebRequestMethods.Ftp.MakeDirectory, user, password)) {
                    return true;
                }
            }

            if (FtpMakeDirectory(parent, user, password)) return FtpMakeDirectory(ftp, user, password);

            return false;
        }

        private static IList<string> _FtpListDirectory(Uri ftp, string user = null, string password = null) {
            try {
                var list = new List<string>();
                using (var response = Request(ftp, WebRequestMethods.Ftp.ListDirectory, user, password))
                using (var reader = new StreamReader(response.GetResponseStream() ?? throw new InvalidOperationException())) {
                    var line = reader.ReadLine();
                    // string.IsNullOrEmpty(line) // its A empty folder
                    while (!string.IsNullOrEmpty(line)) {
                        list.Add(line.Split('/').Last());
                        line = reader.ReadLine();
                    }

                    return list;
                }
            } catch (WebException e) {
                if (e.Response is FtpWebResponse response && response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable) // not found
                    return null;

                throw;
            }
        }

        public static FtpItem FtpListDirectory(Uri ftp, string user = null, string password = null) {
            var last = ftp.Segments.LastOrDefault();

            var currentItem = new FtpItem {
                Name = last,
                Path = ftp
            };
            try {
                var names = _FtpListDirectory(ftp, user, password);
                switch (names.Count) {
                    case 0: {
                        // its A empty folder
                        currentItem.IsFile = false;
                        currentItem.SubItems = new List<FtpItem>();
                        goto EscapeScope;
                    }
                    case 1 when names.First() == last: {
                        FtpExisted(ftp, out var isFile, user, password);
                        currentItem.IsFile = isFile;
                        goto EscapeScope;
                    }
                }

                currentItem.IsFile = false;
                currentItem.SubItems = new List<FtpItem>(names.Count);
                var tasks = new List<Task>(names.Count);
                names.ToList().ForEach(name => {
                    tasks.Add(Task.Factory.StartNew(() => {
                        var item = new FtpItem {
                            Name = name,
                            Path = ftp.Append(name)
                        };
                        var subItem = FtpListDirectory(item.Path, user, password);
                        if (subItem.IsFile) {
                            item.IsFile = true;
                        } else {
                            item.IsFile = false;
                            item.SubItems = subItem.SubItems;
                        }

                        currentItem.SubItems.Add(item);
                    }));
                });
                Task.WaitAll(tasks.ToArray());

                // foreach (var name in names) {
                //     var item = new FtpItem {
                //         Name = name,
                //         Path = ftp.Append(name)
                //     };
                //     var subItem = FtpListDirectory(item.Path, user, password);
                //     if (subItem.IsFile) {
                //         item.IsFile = true;
                //     } else {
                //         item.IsFile = false;
                //         item.SubItems = subItem.SubItems;
                //     }
                //
                //     currentItem.SubItems.Add(item);
                // }
            } catch (Exception ex) {
                currentItem.Exception = ex;
            }

            EscapeScope:
            return currentItem;
        }

        private static void _FtpUploadFile(FileInfo file, Uri ftp, bool rename = false, string user = null, string password = null) {
            Uri parent, me;
            if (rename || file.Name == ftp.Me()) {
                parent = ftp.Parent();
                me = ftp;
            } else {
                parent = ftp;
                me = ftp.Append(file.Name);
            }

            if (!FtpMakeDirectory(parent, user, password)) throw new TargetException($"Error: Cannot Make Directory at {ftp}");

            var request = CreateRequest(me, WebRequestMethods.Ftp.UploadFile, user, password);
            using (var stream = request.GetRequestStream())
            using (var fileStream = file.OpenRead()) {
                var buffer = new byte[StreamBufferSize];
                int read;
                do {
                    read = fileStream.Read(buffer, 0, buffer.Length);
                    stream.Write(buffer, 0, read);
                    stream.Flush();
                } while (read > 0);
            }
        }

        private static void _FtpUploadFolder(DirectoryInfo dir, Uri ftp, bool rename = false, string user = null, string password = null) {
            var me = rename ? ftp : dir.Name == ftp.Me() ? ftp : ftp.Append(dir.Name);
            if (!FtpMakeDirectory(me, user, password)) throw new TargetException($"Error: Cannot Make Directory at {ftp}");

            foreach (var subDir in dir.GetDirectories()) _FtpUploadFolder(subDir, me.Append(subDir.Name), false, user, password);

            // foreach (var subFile in dir.GetFiles()) {
            //     _FtpUploadFile(subFile, me.Append(subFile.Name), false, user, password);
            // }

            // 文件并行上传 默认数量 MaxExecutingActionCount = 5
            var actions = dir.GetFiles().Select(subFile => (Action) (() => { _FtpUploadFile(subFile, me.Append(subFile.Name), false, user, password); }));
            Parallel.Invoke(new ParallelOptions {MaxDegreeOfParallelism = MaxExecutingActionCount}, actions.ToArray());
        }

        public static void FtpUpload(string local, Uri ftp, bool rename = false, string user = null, string password = null) {
            if (File.Exists(local)) {
                var file = new FileInfo(local);
                _FtpUploadFile(file, ftp, rename, user, password);
            } else if (Directory.Exists(local)) {
                var dir = new DirectoryInfo(local);
                _FtpUploadFolder(dir, ftp, rename, user, password);
            } else {
                throw new NullReferenceException($"what is {local}? where is it?");
            }
        }

        public static long FtpFileSize(Uri ftp, string user = null, string password = null) {
            using (var response = Request(ftp, WebRequestMethods.Ftp.GetFileSize, user, password))
                return response.ContentLength;
        }

        private static void _FtpDownloadFile(string local, Uri ftp, string user = null, string password = null) {
            using (var response = Request(ftp, WebRequestMethods.Ftp.DownloadFile, user, password))
            using (var stream = response.GetResponseStream() ?? throw new InvalidOperationException())
            using (var file = new FileStream(local, FileMode.Create)) {
                var buffer = new byte[StreamBufferSize];

                int read;
                do {
                    read = stream.Read(buffer, 0, buffer.Length);
                    file.Write(buffer, 0, read);
                    file.Flush();
                } while (read > 0);
            }
        }

        private static void _FtpDownloadFolder(string dir, FtpItem item, string user = null, string password = null) {
            Directory.CreateDirectory(dir);
            foreach (var subItem in item.SubItems) {
                var path = Path.Combine(dir, subItem.Name);
                switch (subItem.IsFile) {
                    case true: {
                        _FtpDownloadFile(path, subItem.Path, user, password);
                        break;
                    }
                    case false: {
                        _FtpDownloadFolder(path, subItem, user, password);
                        break;
                    }
                }
            }
        }

        public static void FtpDownload(string local, Uri ftp, bool rename = false, string user = null, string password = null) {
            if (!FtpExisted(ftp, out var isFile, user, password)) throw new TargetException($"Error: target not existed at {ftp}");

            var isLocalDir = local.EndsWith(Path.PathSeparator.ToString());

            Directory.CreateDirectory(isFile ? isLocalDir ? local : Directory.GetParent(local).FullName : local);
            var item = FtpListDirectory(ftp, user, password);
            if (isFile) {
                var path = isLocalDir ? $"{local}{Path.PathSeparator}{item.Name}" : local;
                _FtpDownloadFile(path, ftp, user, password);
            } else {
                _FtpDownloadFolder(local, item, user, password);
            }

            // var item = FtpListDirectory(ftp, user, password);
            // var last = local.Split(Path.DirectorySeparatorChar).Last();
            // var hasExtension = last.Split('.').Length > 1;
            // Directory.CreateDirectory(hasExtension ? Directory.GetParent(local).FullName : local);
            // switch (item.IsFile) {
            //     case true: {
            //         var path = hasExtension ? Path.Combine(local, item.Name) : local;
            //         _FtpDownloadFile(path, item.Path, user, password);
            //         break;
            //     }
            //     case false: {
            //         _FtpDownloadFolder(local, item, user, password);
            //         break;
            //     }
            // }
        }

        private static bool _FtpDeleteFile(Uri ftp, string user = null, string password = null) {
            using (var response = Request(ftp, WebRequestMethods.Ftp.DeleteFile, user, password))
                return true;
        }

        private static bool _FtpRemoveDirectory(Uri ftp, string user = null, string password = null) {
            var self = FtpListDirectory(ftp, user, password);
            return _FtpRemoveDirectory(ftp, self, user, password);
        }

        private static bool _FtpRemoveDirectory(Uri ftp, FtpItem self = null, string user = null, string password = null) {
            self = self ?? FtpListDirectory(ftp, user, password);

            var tasks = new List<Task>(self.SubItems.Count);
            self.SubItems.ForEach(item => {
                tasks.Add(Task.Factory.StartNew(() => {
                    if (item.IsFile)
                        _FtpDeleteFile(item.Path, user, password);
                    else
                        _FtpRemoveDirectory(item.Path, item, user, password);
                }));
            });

            Task.WaitAll(tasks.ToArray());

            // foreach (var item in self.SubItems) {
            //     if (item.IsFile) {
            //         _FtpDeleteFile(item.Path, user, password);
            //     } else {
            //         _FtpRemoveDirectory(item.Path, item, user, password);
            //     }
            // }

            using (var response = Request(ftp, WebRequestMethods.Ftp.RemoveDirectory, user, password)) {
                return true;
            }
        }

        public static bool FtpDelete(Uri ftp, string user = null, string password = null) {
            if (!FtpExisted(ftp, out var isFile, user, password)) return true;

            return isFile ? _FtpDeleteFile(ftp, user, password) : _FtpRemoveDirectory(ftp, user, password);
        }

    }

}