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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RMExplorer
{
    public abstract class SourceBase
    {
        public string ExportFilePath { get; private set; } = string.Empty;
        public string FileName { get; private set; } = string.Empty;
        //表示文件的从根路径，到当前文件的Path， 目前只用来拼接目的端Url 的，例如Explorer中存在文件\\ip\c$\a\b\c.txt. GUI 上选中了：a folder 进行move，等到Move到c.txt 的时候，MoveParentPath 即为：a\b
        public string MoveParentPath { get; set; } = string.Empty;
        public string SourceUrl { get; private set; } = string.Empty;
        public int NodeType { get; private set; }

        //Path 生成的MD5 ， 主要用来查询DB 中记录
        public Guid Id { get; private set; }
        public Guid ScopeId { get; private set; }
        public Guid NodeId { get; set; }

        //目前通过一个Int值来记录父子关系，每个GUI 上选择的节点 值为0 ，下层节点为1 ，依次叠加，此值不负责处理节点顺序和结构，只表示节点级别
        public int NodeLevel { get; private set; }
        /// <summary>
        /// 原端对象基类，存储必要的导出文件路径，导出文件名字等基本属性
        /// </summary>
        /// <param name="nodeId">RMManagedRecords 表的Id 字段，用来更新DB 的主键，如果不需要更新的节点，赋值-1。</param>
        /// <param name="sourceUrl">导出Source file Url</param>
        /// <param name="exportFilePath">导出文件路径</param>
        /// <param name="fileName">备份文件的名字，还原的时候需要提供想要还原的文件名</param>
        public SourceBase(Guid id, Guid scopeId, Guid nodeId, int nodeType, int nodeLevel, string sourceUrl, string exportFilePath, string MoveParentPath, string fileName)
        {
            Init(id,scopeId, nodeId, nodeType, nodeLevel, sourceUrl, exportFilePath, MoveParentPath, fileName);
        }

        public abstract void MoveBackup();

        public abstract void Delete();

        public abstract Guid GetSourceTermId(string columnName);

        private void Init(Guid id, Guid scopeId, Guid nodeId, int nodeType, int nodeLevel, string sourceUrl, string exportFilePath, string moveParentPath, string fileName)
        {
            Id = id;
            ScopeId = scopeId;
            NodeId = nodeId;
            NodeType = nodeType;
            NodeLevel = nodeLevel;
            SourceUrl = sourceUrl;
            ExportFilePath = exportFilePath;
            MoveParentPath = moveParentPath;
            FileName = fileName;
        }
    }
}
