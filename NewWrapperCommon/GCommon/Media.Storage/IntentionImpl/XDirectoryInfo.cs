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


namespace AvePoint.Media.Storage
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.AccessControl;
    using AvePoint.Media.Storage.Util;

    #endregion using directives

    public class StorageDirectoryInfo : XDirectoryInfo
    {
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public StorageDirectoryInfo()
        {
        }
        private string name;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">文件夹名</param>
        public StorageDirectoryInfo(string name)
        {
            this.name = name;
        }
        /// <summary>
        /// 文件夹名
        /// </summary>
        public override string Name
        {
            get { return name; }
        }
        /// <summary>
        /// 判断文件夹是否为空
        /// </summary>
        public override bool IsEmpty
        {
            get { throw new NotSupportedException(); }
        }
    }

    public abstract class XDirectoryInfo : StorageInfo
    {
        /// <summary>
        /// 访问文件夹所需要的用户名,目前仅适用于netshare
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 访问文件夹所需要的密码,目前仅适用于netshare
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// 访问文件夹所需要的用户所在域的域名,目前仅适用于netshare
        /// </summary>
        public string Domain { get; set; }
        /// <summary>
        /// 文件夹的属性,目前仅适用于netshare
        /// </summary>
        public virtual FileAttributes Attribute { get; set; }
        /// <summary>
        /// 文件夹的DirectoryInfo对象,目前仅适用于netshare
        /// </summary>
        public virtual DirectoryInfo DirInfo { get; set; }
        /// <summary>
        /// 文件夹的所有者,目前仅适用于netshare
        /// </summary>
        public virtual string Owner { get { return null; } }

        private bool writable;
        /// <summary>
        /// 判断文件夹是否可写.暂时不可用
        /// </summary>
        public bool Writable
        {
            get
            {
                //FileAttributes.r
                return writable;
            }
        }

        private List<XFileInfo> subFiles = new List<XFileInfo>();
        /// <summary>
        /// 文件夹下的子文件.暂时不可用
        /// </summary>
        public virtual List<XFileInfo> SubFiles { get { return subFiles; } }

        private List<XDirectoryInfo> subDirs = new List<XDirectoryInfo>();
        /// <summary>
        /// 文件夹下的子文件夹.暂时不可用
        /// </summary>
        public virtual List<XDirectoryInfo> SubDirectories { get { return subDirs; } }
        /// <summary>
        /// 构造函数
        /// </summary>
        public XDirectoryInfo()
        {
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="highName">文件夹相对路径</param>
        public XDirectoryInfo(string highName)
        {
            this.HighName = highName;
        }
        /// <summary>
        /// 文件夹相对路径
        /// </summary>
        public override string FullName
        {
            get
            {
                return PathUtil.CombinePath(HighName, LowName);
            }
        }
        /// <summary>
        /// 删除方法,目前只支持FTP
        /// </summary>
        public override void Delete()
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// 判断文件夹是否存在
        /// </summary>
        public override bool Exists
        {
            get { throw new NotSupportedException(); }
        }
        /// <summary>
        /// 文件夹名称
        /// </summary>
        public override string Name
        {
            get { throw new NotSupportedException(); }
        }
        /// <summary>
        /// 文件夹创建时间,目前只支持netshare
        /// </summary>
        public new virtual DateTime CreationTime { get; set; }
        /// <summary>
        /// 文件夹UTC创建时间
        /// </summary>
        public new virtual DateTime CreationTimeUtc { get; set; }
        /// <summary>
        /// 文件夹最近访问时间,目前只支持netshare
        /// </summary>
        public new virtual DateTime LastAccessTime { get; set; }
        /// <summary>
        /// 文件夹最近访问UTC时间,目前只支持netshare
        /// </summary>
        public new virtual DateTime LastAccessTimeUtc { get; set; }
        /// <summary>
        /// 文件夹最近修改时间,目前只支持netshare
        /// </summary>
        public new virtual DateTime LastWriteTime { get; set; }
        /// <summary>
        /// 文件夹最近修改UTC时间
        /// </summary>
        public new virtual DateTime LastWriteTimeUtc { get; set; }
        /// <summary>
        /// 父文件夹全路径,目前只支持netshare
        /// </summary>
        public virtual string ParentFullName { get; set; }
        /// <summary>
        /// 获取文件夹权限,目前仅适用于netshare
        /// </summary>
        public virtual DirectorySecurity AccessControl { get; set; }
        /// <summary>
        /// 文件夹全路径,目前只支持netshare
        /// </summary>
        public virtual string DirFullPath { get; set; }
        /// <summary>
        /// 文件夹全UNC路径,目前只支持netshare
        /// </summary>
        public virtual string UNCFullPath { get; set; }

        /// <summary>
        /// 获取原始文件全路径
        /// </summary>
        public virtual String OriginalDirFullPath { get; set; }
        /// <summary>
        /// 文件夹下的文件数量
        /// </summary>
        public virtual Int32 FileCount { get; private set; }
        //*****************for box********************
        /// <summary>
        /// 文件夹改名,目前只支持Box
        /// </summary>
        public virtual string SetNewName { get; set; }
        /// <summary>
        /// 文件夹创建者,目前只支持Box
        /// </summary>
        public virtual string CreatedBy { get; set; }
        /// <summary>
        /// 文件夹修改者,目前只支持Box
        /// </summary>
        public virtual string ModifiedBy { get; set; }
        /// <summary>
        /// 文件夹拥有者,目前只支持Box
        /// </summary>
        public virtual string OwnedBy { get; set; }
        /// <summary>
        /// 文件夹标签,目前只支持Box
        /// </summary>
        public virtual List<string> Tags { get; set; }
        /// <summary>
        /// 文件夹描述,目前只支持Box
        /// </summary>
        public virtual string Description { get; set; }
        /// <summary>
        /// 文件夹访问地址,目前只支持Box
        /// </summary>
        public virtual String Url { get; set; }
        /// <summary>
        /// 文件夹下载地址,目前只支持Box
        /// </summary>
        public virtual String DownloadUrl { get; set; }
        //*****************for box********************
        /// <summary>
        /// 文件夹是否为空
        /// </summary>
        public abstract bool IsEmpty
        {
            get;
        }
    }
}