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
using System.IO;
using System.Security.AccessControl;

namespace AvePoint.Media.Storage
{
    public class XFileInfo : StorageInfo
    {
        /// <summary>
        /// 文件的大小
        /// </summary>
        public virtual long FileSize { get; set; }
        /// <summary>
        /// 访问文件所需要的用户名,目前仅适用于netshare
        /// </summary>
        public virtual string UserName { get; set; }
        /// <summary>
        /// 访问文件所需要的密码,目前仅适用于netshare
        /// </summary>
        public virtual string Password { get; set; }
        /// <summary>
        /// 访问文件所需要的用户所在域的域名,目前仅适用于netshare
        /// </summary>
        public virtual string Domain { get; set; }
        /// <summary>
        /// 文件的属性,目前仅适用于netshare & Netapp
        /// </summary>
        public virtual FileAttributes Attribute { get; set; }
        /// <summary>
        /// 文件的FileInfo对象,目前仅适用于netshare
        /// </summary>
        public virtual FileInfo FileInfo { get; set; }
        /// <summary>
        /// 文件的所有者,目前仅适用于netshare
        /// </summary>
        public virtual string Owner { get { return null; } }
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public XFileInfo()
        {
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="highName">文件所在文件夹路径</param>
        /// <param name="lowName">文件名</param>
        public XFileInfo(string highName, string lowName)
        {
            HighName = highName;
            LowName = lowName;
        }
        /// <summary>
        /// 文件的相对路径
        /// </summary>
        public override string FullName
        {
            get
            {
                return Path.Combine(HighName, LowName);
            }
        }
        /// <summary>
        /// 文件名
        /// </summary>
        public override string Name
        {
            get 
            {
                return LowName; 
            }
        }
        /// <summary>
        /// 删除文件方法,目前仅适用于FTP
        /// </summary>
        public override void Delete()
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// 判断文件是否存在
        /// </summary>
        public override bool Exists
        {
            get { throw new NotSupportedException(); }
        }

        #region virtual time
        /// <summary>
        /// 文件的创建时间
        /// </summary>
        public new virtual DateTime CreationTime { get; set; }    
        /// <summary>
        /// 文件的UTC创建时间
        /// </summary>
        public new virtual DateTime CreationTimeUtc { get; set; }       
        /// <summary>
        /// 最近访问时间
        /// </summary>
        public new virtual DateTime LastAccessTime { get; set; }         
        /// <summary>
        /// 最近UTC访问时间
        /// </summary>
        public new virtual DateTime LastAccessTimeUtc { get; set; }
        /// <summary>
        /// 最近修改时间
        /// </summary>
        public new virtual DateTime LastWriteTime { get; set; }         
        /// <summary>
        /// 最近UTC修改时间
        /// </summary>
        public new virtual DateTime LastWriteTimeUtc { get; set; }         
        /// <summary>
        /// 父文件夹名称,目前仅适用于netshare
        /// </summary>
        public virtual string ParentFullName { get; set; }
        /// <summary>
        /// 获取文件权限,目前仅适用于netshare
        /// </summary>
        public virtual FileSecurity AccessControl { get; set; }
        /// <summary>
        /// 获取文件全路径
        /// </summary>
        public virtual String FileFullPath { get; set; }

        /// <summary>
        /// 获取原始文件全路径
        /// </summary>
        public virtual String OriginalFileFullPath { get; set; }

        //*****************for box********************
        /// <summary>
        /// 改文件名,目前只支持Box
        /// </summary>
        public virtual string SetNewName { get; set; }
        /// <summary>
        /// 文件的version,目前只支持Box
        /// </summary>
        public virtual List<XFileInfo> Versions { get; set; }
        /// <summary>
        /// 文件的查看的URL,目前只支持Box
        /// </summary>
        public virtual String Url { get; set; }
        /// <summary>
        /// 文件下载的URL,目前只支持Box
        /// </summary>
        public virtual String DownloadUrl { get; set; }
        /// <summary>
        /// 数据创建时间,目前只支持Box
        /// </summary>
        public virtual DateTime ContentCreatedTime { get; set; }
        /// <summary>
        /// 数据修改时间,目前只支持Box
        /// </summary>
        public virtual DateTime ContentModifiedTime { get; set; }
        /// <summary>
        /// 文件创建者,目前只支持Box
        /// </summary>
        public virtual string CreatedBy { get; set; }
        /// <summary>
        /// 文件修改者,目前只支持Box
        /// </summary>
        public virtual string ModifiedBy { get; set; }
        /// <summary>
        /// 文件拥有者,目前只支持Box
        /// </summary>
        public virtual string OwnedBy { get; set; }
        /// <summary>
        /// 文件标签,目前只支持Box
        /// </summary>
        public virtual List<string> Tags { get; set; }
        /// <summary>
        /// 文件描述,目前只支持Box
        /// </summary>
        public virtual string Description { get; set; }
        /// <summary>
        /// 文件是否被锁,目前只支持Box
        /// </summary>
        public virtual bool IsLocked { get; set; }
        //*****************for box********************

        #endregion

    }
}
