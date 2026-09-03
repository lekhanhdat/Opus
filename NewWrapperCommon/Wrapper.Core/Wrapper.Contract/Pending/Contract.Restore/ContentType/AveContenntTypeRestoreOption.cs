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
using AvePoint.GCommon.Utility;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Restore
{
    public class AveContentTypeRestoreOption
    {
        /// <summary>
        /// Content Type还原过程中使用的Find逻辑，有FindById，Schema，Name等一些条件。
        /// </summary>
        public ContentTypeFindOption[] FindOption = new ContentTypeFindOption[] { ContentTypeFindOption.FindBySchema, ContentTypeFindOption.FindById, ContentTypeFindOption.FindByName, ContentTypeFindOption.FindByParent };
        /// <summary>
        /// Content Type还原过程中需要在哪个级别上去找Content Type，有当前级别和parent，还是children级别。
        /// </summary>
        public ContentTypeFindScope[] FindScope = new ContentTypeFindScope[] { ContentTypeFindScope.Current, ContentTypeFindScope.Parent, ContentTypeFindScope.Children };
        /// <summary>
        /// Content Type冲突solution，因为无法比较content type是改变的还是新添加的，所以只要有不一样的地方就认为是冲突，
        /// 冲突的解决方案有Overwrite，Skip，还有Append
        /// </summary>
        public ContentTypeConflictHandleOption ConflictHandleOption = ContentTypeConflictHandleOption.Overwrite;
        /// <summary>
        /// 创建Content Type的选项。
        /// </summary>
        public ContentTypeCreateOption[] CreateOption = new ContentTypeCreateOption[] { ContentTypeCreateOption.UseId, ContentTypeCreateOption.ForceCreate, ContentTypeCreateOption.UseParent };
        public GetParentContentTypeOption GetParentOption = GetParentContentTypeOption.Default;
        public bool WEB_CONTENTTYPE = true;
        public bool WEB_CONTENTTYPE_SKIP = true;
        public bool WEB_CONTENTTYPE_RENAME = true;
        public bool WEB_CONTENTTYPE_UPDATECHILD = false;
        public ContentTypeFieldLinksOption FIELDLINKSOPTION = ContentTypeFieldLinksOption.OverWrite;
        public bool LIST_CONTENTTYPE = true;
        public bool COMPARE_MD5 = false;
        public bool WEB_CONTENTTYPE_CREATETEMP = false;
        //当还原fieldlink的时候，此field在list级别不存在的时候是否check web级别的field
        public bool List_ContentType_CheckWebFieldLink = true;
        /// <summary>
        /// 此选项主要控制 如配置了content type mapping，如果目的端list上这个content type 不存在，若该属性值为true，
        /// 则会以mapping name 在目的端web上根据这个mapping name找出这个content type，然后在list 级别上创建以这个为parent的content type
        /// 若为false，则会以源端备份过来的parent info找出parent 来创建content type
        /// </summary>
        public bool CreateContentTypeWithParentFindByMappingName = false;

        /// <summary>
        ///ADO-205916 CM遇到了数个客户 源端的content type不再用了，改用目的端的content type，而且不想修改目的端content type 的field link，因此添加了该属性，
        /// </summary>
        public bool SkipFieldLinkWhenFindInContentTypeMapping = false;
    }
    /// <summary>
    /// FindBySchema: Wrapper will add a mapping property to list or web to keep the mappings of source content type id and desination id. If need to search by mapping, add the option to FindOption arrary.
    /// FindById: Find content type using current content type id.
    /// FindByName: Find by content type name.
    /// </summary>
    public enum ContentTypeFindOption
    {
        /// <summary>
        /// Schema是Wrapper自己产生的Mapping关系，存储在Web的Properties中，如果还原过一次，就认为两端建立关系，
        /// 以后通过Schema来还原就可以。
        /// </summary>
        FindBySchema,
        /// <summary>
        /// 通过Content Type Id来还原，因为大部分Content Type是Keep Id的。
        /// </summary>
        FindById,
        /// <summary>
        /// 通过Name来寻找
        /// </summary>
        FindByName,
        /// <summary>
        /// 通过找Parent
        /// </summary>
        FindByParent
    }

    public enum ContentTypeFindScope
    {
        /// <summary>
        /// Find Scope为当前
        /// </summary>
        Current,
        /// <summary>
        /// Find Scope为parent
        /// </summary>
        Parent,
        /// <summary>
        /// Find Scope为Children，这个有效率问题，一般不建议使用
        /// </summary>
        Children
    }

    /// <summary>
    /// Source win will change the name of the destination content type, then create the source content type.
    /// </summary>
    public enum ContentTypeConflictHandleOption
    {
        None,
        /// <summary>
        /// 冲突时，不还原
        /// </summary>
        Skip,
        /// <summary>
        /// 冲突时，新添加一个。
        /// </summary>
        Append,
        /// <summary>
        /// 
        /// </summary>
        AppendSourceWin,
        AppendDestinationWin,
        /// <summary>
        /// 冲突时，直接覆盖目的端的setting
        /// </summary>
        Overwrite,
        CreateNew,
        /// <summary>
        /// 先删除目的端已有的ct，再创建新的，只在list level生效。
        /// </summary>
        Replace
    }

    public enum ContentTypeCreateOption
    {
        /// <summary>
        /// 使用Id来创建，Keep Id
        /// </summary>
        UseId,
        /// <summary>
        /// 使用Parent来创建
        /// </summary>
        UseParent,
        /// <summary>
        /// 强制创建,keep Id
        /// </summary>
        ForceCreate,
        /// <summary>
        /// 强制创建,不keep Id
        /// </summary>
        ForceCreateWithoutKeepId
    }

    public enum GetParentContentTypeOption
    {
        Default,
        BuildinParent,
        RestoreFamily
    }

    public enum ContentTypeFieldLinksOption
    {
        /// <summary>
        ///The FieldLinks would be overwritten on the destination, no matter the Content Type is newly created or not
        /// </summary>
        OverWrite,
        /// <summary>
        /// The FieldLinks would be merged with the one of source, no matter the Content Type is newly created or not
        /// </summary>
        Merge,
        /// <summary>
        /// Judge whether the Content Type is newly created on the destination, if so the FieldLinks would be overwritten, and if not, the FieldLinks would be merged
        /// </summary>
        OverWriteIfNewCreated
    }
    /// <summary>
    /// 记录ContentType的还原情况，当前主要是DM的Hub使用
    /// </summary>
    public class ContentTypeRestoreReport
    {
        /// <summary>
        /// 记录Restore的方式
        /// </summary>
        public ContentTypeConflictHandleOption RestoreOption { get; set; }

        /// <summary>
        /// 记录异常信息
        /// </summary>
        public string FailedException { get; set; }

        /// <summary>
        /// 记录目的端还原后名称
        /// </summary>
        public string RestoreName { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ContentTypeRestoreReport(ContentTypeConflictHandleOption option)
        {
            RestoreOption = option;
        }
    }

    public class AveSchemaDependencyNotFoundException : AveWrapperBaseException
    {
        public string SchemaDependencyName = string.Empty;
        public string SchemaDependencyType = string.Empty;
        public AveSchemaDependencyNotFoundException(string name, string type)
            : base(AveInternalResourceKey.Wrapper_Exception_Contract_ConnotFindSchemaDependency , name, type)
        {
            SchemaDependencyName = name;
        }
    }

    public class AveSchemaDependencyConflictException : AveWrapperBaseException
    {
        public string SchemaDependencyName = string.Empty;
        public string SchemaDependencyType = string.Empty;

        public AveSchemaDependencyConflictException(string name, string type)
            : base(AveInternalResourceKey.Wrapper_Exception_Contract_ContentTypeConflict, name, type)
        {
            SchemaDependencyName = name;
        }
    }
}
