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
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using AvePoint.GCommon.Contract.CodeReview;

namespace AvePoint.Media.Storage.FTP
{
    #region file Struct
    struct FileStruct
    {
        private string flags;
        public string Flags
        {
            get { return flags; }
            set { flags = value; }
        }

        private string owner;
        public string Owner
        {
            get { return owner; }
            set { owner = value; }
        }

        private string group;
        public string Group
        {
            get { return group; }
            set { group = value; }
        }

        private bool isDirectory;
        public bool IsDirectory
        {
            get { return isDirectory; }
            set { isDirectory = value; }
        }

        private DateTime createTime;
        public DateTime CreateTime
        {
            get { return createTime; }
            set { createTime = value; }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public Int64 Size { get; set; }
    }  

    enum FileListStyle
    {
        UnixStyle,
        WindowsStyle,
        Unknown
    }
    #endregion

    #region CodeReview
    [AveCodeReview(
    "2012/2/22",
    "rongbiao.sun@avepoint.com",
    "dapeng.zhang@avepoint.com",
    new string[] { CodeReviewConstants.CHECK_LIST_ID_FA_1 },
    "ADO-26069",
    true)]
    #endregion

    class FTPDirectoryListParser
    {
        public FileStruct[] GetList(string DirectorysAndFilesStr)
        {
            List<FileStruct> myListArray = new List<FileStruct>();  
            string[] listDirAndFiles = DirectorysAndFilesStr.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            FileListStyle directoryListStyle = GuessFileListStyle(listDirAndFiles);

            foreach (string s in listDirAndFiles)  
            {  
                if (directoryListStyle != FileListStyle.Unknown && s != "")  
                {  
                    FileStruct f = new FileStruct();  
                    f.Name = "..";  
                    switch (directoryListStyle)  
                    {  
                        case FileListStyle.UnixStyle:  
                            f = ParseFileStructFromUnixStyleRecord(s);  
                            break;  

                        case FileListStyle.WindowsStyle:  
                            f = ParseFileStructFromWindowsStyleRecord(s);  
                            break;  
                    }  
                    if (!(f.Name == "." || f.Name == ".."))  
                    {  
                        myListArray.Add(f);  
                    }  
                }  
            }  
            return myListArray.ToArray();  
        }

        private FileListStyle GuessFileListStyle(string[] recordList)
        {
            foreach (string s in recordList)
            {
                if (s.Length > 10 && Regex.IsMatch(s.Substring(0, 10), "(-|d)(-|r)(-|w)(-|x)(-|r)(-|w)(-|x)(-|r)(-|w)(-|x)"))
                {
                    return FileListStyle.UnixStyle;
                }
                else if (s.Length > 8 && Regex.IsMatch(s.Substring(0, 8), "[0-9][0-9]-[0-9][0-9]-[0-9][0-9]"))
                {
                    return FileListStyle.WindowsStyle;
                }
            }
            return FileListStyle.Unknown;
        }

        private FileStruct ParseFileStructFromWindowsStyleRecord(string Record)
        {
            FileStruct fileStruct = new FileStruct();
            string processstr = Record.Trim();
            string dateStr = processstr.Substring(0, 8);
            processstr = (processstr.Substring(8, processstr.Length - 8)).Trim();
            string timeStr = processstr.Substring(0, 7);
            processstr = (processstr.Substring(7, processstr.Length - 7)).Trim();
            DateTimeFormatInfo myDTFI = new CultureInfo("en-US", false).DateTimeFormat;
            myDTFI.ShortTimePattern = "t";
            fileStruct.CreateTime = DateTime.Parse(dateStr + " " + timeStr, myDTFI);
            if (processstr.Substring(0, 5) == "<DIR>")
            {
                fileStruct.IsDirectory = true;
                processstr = (processstr.Substring(5, processstr.Length - 5)).Trim();
            }
            else
            {
                //string[] strs = processstr.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                //processstr = strs[1];
                fileStruct.Size = Int64.Parse(processstr.Substring(0, processstr.IndexOf(' ')));
                processstr = processstr.Substring(processstr.IndexOf(' ') + 1);
                fileStruct.IsDirectory = false;
            }
            fileStruct.Name = processstr;
            return fileStruct;
        }  

        private FileStruct ParseFileStructFromUnixStyleRecord(string Record)
        {
            FileStruct fileStruct = new FileStruct();
            string processstr = Record.Trim();
            fileStruct.Flags = processstr.Substring(0, 10);
            fileStruct.IsDirectory = (fileStruct.Flags[0] == 'd');
            processstr = (processstr.Substring(11)).Trim();
            CutSubstringFromStringWithTrim(ref processstr, ' ', 0);   //跳过一部分  
            fileStruct.Owner = CutSubstringFromStringWithTrim(ref processstr, ' ', 0);
            fileStruct.Group = CutSubstringFromStringWithTrim(ref processstr, ' ', 0);
            CutSubstringFromStringWithTrim(ref processstr, ' ', 0);   //跳过一部分  
            string yearOrTime = processstr.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[2];
            if (yearOrTime.IndexOf(":",StringComparison.CurrentCulture) >= 0)  //time  
            {
                processstr = processstr.Replace(yearOrTime, DateTime.Now.Year.ToString());
            }
            fileStruct.CreateTime = DateTime.Parse(CutSubstringFromStringWithTrim(ref processstr, ' ', 8));
            fileStruct.Name = processstr;   //最后就是名称  
            return fileStruct;
        }

        private string CutSubstringFromStringWithTrim(ref string s, char c, int startIndex)
        {
            int pos1 = s.IndexOf(c, startIndex);
            string retString = s.Substring(0, pos1);
            s = (s.Substring(pos1)).Trim();
            return retString;
        }  
    }
}
