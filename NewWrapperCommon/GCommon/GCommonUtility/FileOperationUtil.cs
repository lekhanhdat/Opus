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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Linq;
using AvePoint.GCommon.Contract.Server.ControlPanel.LogManager.Object;
using Microsoft.VisualBasic;

namespace AvePoint.GCommon.Utility
{
    public class FileOperationUtil
    {
        private static AveLogger mLogger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// 文件夹复制
        /// </summary>
        /// <param name="varFromDirectory"></param>
        /// <param name="varToDirectory"></param>
        [Obsolete("该方法即将废弃，敏感信息替换不需要文件复制操作")]
        public static void CopyDirectory(string varFromDirectory, string varToDirectory)
        {
            try
            {
                //实现从一个目录下完整拷贝到另一个目录下。
                Directory.CreateDirectory(varToDirectory);
                if (!Directory.Exists(varFromDirectory))
                {
                    //m_eorrStr = "对不起，您要拷贝的目录不存在。 ";
                    mLogger.Error("you want to copy the directory does not exist:{0}", varFromDirectory);
                    return;
                }

                string[] directories = Directory.GetDirectories(varFromDirectory);//取文件夹下所有文件夹名，放入数组；
                if (directories.Length > 0)
                {
                    for (int i = 0; i < directories.Length; i++)
                    {
                        string d = directories[i];
                        //varToDirectory + d.Substring(d.LastIndexOf("\\", StringComparison.Ordinal))
                        CopyDirectory(d, Path.Combine(varToDirectory, Path.GetFileName(d))); //递归拷贝文件和文件夹
                    }
                }
                string[] files = Directory.GetFiles(varFromDirectory);//取文件夹下所有文件名，放入数组；
                if (files.Length > 0)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        string s = files[i];
                        if (File.Exists(s))
                        {
                            File.Copy(s, Path.Combine(varToDirectory, Path.GetFileName(s)));
                        }
                    }
                }
                mLogger.Debug("Copy Folder {0} Successful", varFromDirectory);
            }
            catch (Exception e)
            {
                mLogger.Error("Copy Folder Error", e);
            }

        }
        /// <summary>
        ///  log manager支持敏感信息替换并以流的方式将文件夹压缩成一个包。
        ///  支持文件夹嵌套，仅支持文本文件，如果含有非文本文件可能会被损坏。
        /// </summary>
        /// <param name="progress">zip包时用于更新进度的回调</param>
        /// <param name="logsFolder">需要压缩的文件夹路径</param>
        /// <param name="listsensitivityDto">敏感信息</param>
        /// <param name="outputZipFile">压缩后的zip文件路径</param>
        /// <exception cref="Exception">本方法出现异常将会抛出，请使用者根据需要自行处理异常</exception>
        //public static void ZipFolderByLogManager(Action<long> progress, string logsFolder, List<LogRetrieveDto> listsensitivityDto, string outputZipFile, Func<string, bool> fileNeedScrub = null)
        //{
        //    using (var zip = new ZipFile())
        //    {
        //        try
        //        {
        //            zip.ParallelDeflateThreshold = -1;
        //            zip.SaveProgress += new EventHandler<SaveProgressEventArgs>(new ZipProgressManager(progress).ProcessedUpdate);
        //            List<LogRetrieveDto> _listDto = new List<LogRetrieveDto>();
        //            foreach (LogRetrieveDto logRetrieveDto in listsensitivityDto)
        //            {
        //                if (logRetrieveDto.OldString.Contains("\\"))
        //                {
        //                    string tmpstr = logRetrieveDto.OldString.Replace('\\', '#');
        //                    _listDto.Add(new LogRetrieveDto() { OldString = tmpstr.ToLower(CultureInfo.CurrentCulture), NewString = logRetrieveDto.NewString });
        //                }
        //                _listDto.Add(new LogRetrieveDto() { OldString = logRetrieveDto.OldString.ToLower(CultureInfo.CurrentCulture), NewString = logRetrieveDto.NewString });
        //            }
        //            AddFileOrFolderToEntry(logsFolder, logsFolder, zip, _listDto, fileNeedScrub);
        //            if (zip.Entries.Count > 65000)
        //            {
        //                mLogger.Info("The count of entries is larger than 65000, so we use UseZip64WhenSaving option.");
        //                zip.UseZip64WhenSaving = Zip64Option.AsNecessary;
        //            }
        //            mLogger.Info("There are {0} documents need to be compressed and the total size is :{1}", Directory.GetFiles(logsFolder).Count(), Directory.GetDirectories(logsFolder).Length);
        //            Stopwatch watch = new Stopwatch();
        //            watch.Start();
        //            zip.Save(outputZipFile);
        //            watch.Stop();
        //            mLogger.Debug("Save zip used :{0}", new TimeSpan(watch.ElapsedTicks));
        //        }
        //        catch (Exception e)
        //        {
        //            mLogger.Error("Zip file Error.", e);
        //            throw;
        //        }
        //    }
        //}

        //private static void AddFileOrFolderToEntry(string baseFolder, string logsFolder, ZipFile zip, List<LogRetrieveDto> listDto, Func<string, bool> fileNeedScrub)
        //{
        //    foreach (string file in Directory.GetFiles(logsFolder))
        //    {
        //        if (fileNeedScrub == null || fileNeedScrub(file))
        //        {
        //            zip.AddEntry(GetEntryName(baseFolder, file), new LogFileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, listDto));
        //        }
        //        else
        //        {
        //            zip.AddEntry(GetEntryName(baseFolder, file), new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
        //        }
        //    }
        //    foreach (string folder in Directory.GetDirectories(logsFolder))
        //    {
        //        if (Directory.GetFiles(folder).Length + Directory.GetDirectories(folder).Length == 0)
        //        {
        //            zip.AddDirectory(folder, GetEntryName(baseFolder, folder));
        //        }
        //        else
        //        {
        //            AddFileOrFolderToEntry(baseFolder, folder, zip, listDto, fileNeedScrub);
        //        }
        //    }

        //}
        private static string GetEntryName(string baseFolder, string fileorfolder)
        {
            return fileorfolder.Substring(baseFolder.Length + 1); ;
        }
        /// <summary>
        /// 文件内字符串替换
        /// </summary>
        /// <param name="listDto"></param>
        /// <param name="patch"></param>
        [Obsolete("该方法即将废弃，敏感信息将在创建压缩包的时候直接替换")]
        public static void EditFile(List<LogRetrieveDto> listDto, string patch)
        {
            //读出所有文本
            //执行替换逻辑foreach
            //写回替换文本
            string allText = File.ReadAllText(patch);
            foreach (var dto in listDto)
            {
                if (dto.OldString.Contains("\\"))
                {
                    string specialString = dto.OldString.Replace('\\', '#');
                    allText = Strings.Replace(allText, specialString.Trim(), dto.NewString.Trim(), 1, -1, CompareMethod.Text);
                    //allText =  ReplaceStr(allText, specialString, dto.NewString, true);
                    //allText = Regex.Replace(allText, specialString, dto.NewString, RegexOptions.IgnoreCase); //正则替换，VisualBasic 替换 含有乱码时替换不准确。
                }
                allText = Strings.Replace(allText, dto.OldString.Trim(), dto.NewString.Trim(), 1, -1, CompareMethod.Text);
                //allText = Regex.Replace(allText,dto.OldString,dto.NewString,RegexOptions.IgnoreCase);
            }
            File.WriteAllText(patch, allText);
        }

        /// <summary>
        /// 文件内字符串替换
        /// 返回Folder的新路
        /// </summary>
        /// <param name="listDto"></param>
        /// <param name="patch"></param>
        public static string EditFolder(List<LogRetrieveDto> listDto, string patch)
        {
            if (Directory.Exists(patch))
            {
                DirectoryInfo dir = new DirectoryInfo(patch);
                string newName = dir.Name;
                foreach (var dto in listDto)
                {
                    string tmpNewString = Format(dto.NewString.Trim());
                    newName = Strings.Replace(newName, dto.OldString.Trim(), tmpNewString, 1, -1, CompareMethod.Text);
                }
                if (!newName.Equals(dir.Name, StringComparison.OrdinalIgnoreCase))
                {
                    string tempPath = GetRenameFolderNewPath(patch, newName);
                    dir.MoveTo(tempPath);
                    return tempPath;
                }
            }
            return patch;
        }
        /// <summary>
        /// 去除非法字符 为 文件名一部分
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string Format(string name)
        {
            if (name == null) return null;
            //office中如果相邻空格超过4个，在zip包中打开的时候就会出现出现潜在危险的提示，为避免此问题将空格置换为_。

            List<char> invalidChar = new List<char> { ' ' };

            invalidChar.AddRange(Path.GetInvalidFileNameChars());

            foreach (var c in invalidChar.Where(c => name.Contains(c)))
            {
                name = name.Replace(c, '_');
            }
            //return string.Join("", name.Split(Path.GetInvalidFileNameChars())).Replace(' ', '_');
            return name;
        }
        private static string GetRenameFolderNewPath(string patch, string newName)
        {
            string newPath = string.Empty;
            string[] pathCores = patch.Split('\\');
            if (pathCores.Length > 0)
            {
                for (int index = 0; index < pathCores.Length - 1; index++)
                {
                    newPath = string.Format(@"{0}\{1}", newPath, pathCores[index]);
                }
                newPath = newPath = string.Format(@"{0}\{1}", newPath, newName);
                newPath = newPath.TrimStart('\\').TrimEnd('\\');
            }
            return newPath;
        }

        /// <summary>
        /// 敏感信息排序，保证先替换长字符串后替换短字符串，否则有问题
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<LogRetrieveDto> SortListLogRetrieve(List<LogRetrieveDto> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = 1; j < list.Count; j++)
                {
                    if (list[j - 1].OldString.Length < list[j].OldString.Length)
                    {
                        LogRetrieveDto temp = list[j - 1];
                        list[j - 1] = list[j];
                        list[j] = temp;
                    }
                }
            }
            return list;
        }
    }
}
