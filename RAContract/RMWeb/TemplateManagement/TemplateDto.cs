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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.TemplateManagement;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.RMWeb.TemplateManagement
{
    [DataContract]
    public class TemplateDto
    {
        [DataMember]
        public int id { set; get; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string description { get; set; }

        /// <summary>
        /// 1:Box Template; 2:Folder Template
        /// 
        /// </summary>
        [DataMember]
        public TemplateType type { set; get; }
        [DataMember]
        public string prefix { set; get; }
        [DataMember]
        public int numberOfDigits { set; get; }
        [DataMember]
        public int parentId { set; get; }
        [DataMember]
        public Guid parentUniqueId { set; get; }
        [DataMember]
        public Guid suiteUniqueId { set; get; }
        [DataMember]
        public Guid uniqueId { get; set; }
        [DataMember]
        public Guid boxTemplateId { get; set; }
        [DataMember]
        public Guid folderTemplateId { get; set; }

        #region Base Info
        [DataMember]
        public double size { get; set; }
        [DataMember]
        public ToUserInfo creater { set; get; }
        [DataMember]
        public DateTime createdOn { get; set; }
        [DataMember]
        public string createdOnStr { get; set; }
        [DataMember]
        public ToUserInfo modifier { set; get; }
        [DataMember]
        public DateTime lastModifiedOn { get; set; }
        [DataMember]
        public string lastModifiedOnStr { get; set; }

        #endregion
        [DataMember]
        public List<TemplateCategoryDto> categories { set; get; }

        //目前一个tempalte level 只会有一个模板，所以代码这么写，以后支持多个的时候，需要返回集合对象，不使用两个对象表示
        [DataMember]
        public List<TemplatesContentCategoriesDto> childTemplateCategories { set; get; }

        //public List<TemplateCategoryDto> childFolderCategories { get; set; }

        /// <summary>
        /// template id list starting from suite. first one is suite unique id, others are template id(not unique id)
        /// </summary>
        [DataMember]
        public List<string> ParentTemplateIdList { get; set; }
    }
    [DataContract]
    public class BarcodeTemplateDto
    {
        [DataMember]
        public string Id { set; get; }
        [DataMember]
        public BarcodeTemplateType Type { set; get; }
        [DataMember]
        public string ImgBase64Str { get; set; }
        [DataMember]
        public string ColumnB { set; get; }
        [DataMember]
        public string ColumnC { set; get; }
        [DataMember]
        public List<string> ColumnD { set; get; }
        [DataMember]
        public string ColumnE { set; get; }
        [DataMember]
        public string ColumnF { set; get; }
        [DataMember]
        public string ImageName { set; get; }
        [DataMember]
        public string ImageType { set; get; }
        [DataMember]
        public string lastModifiedOn { get; set; }
    }

    [DataContract]
    public class TemplatesContentCategoriesDto
    {
        [DataMember]
        public Guid uniqueId { get; set; }
        [DataMember]
        public string templateName { get; set; }
        [DataMember]
        public TemplateType type { set; get; }
        [DataMember]
        public List<TemplateCategoryDto> currentCategories { get; set; }
        [DataMember]
        public List<TemplatesContentCategoriesDto> childrenCategories { get; set; }
    }
    [DataContract]
    public class TemplateCategoryDto
    {
        [DataMember]
        public Guid id { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public bool allowEdit { get; set; }
        [DataMember]
        public List<TemplateColumnDto> columns { get; set; }
    }
    [DataContract]
    public class TemplateColumnDto
    {
        [DataMember]
        public Guid categoryId { get; set; }
        [DataMember]
        public Guid uniqueId { get; set; }

        //public Guid pushCategoryId { get; set; }
        //public Guid pushFolderCategoryId { get; set; }
        [DataMember]
        public List<TemplateIdAndCategoryId> pushFoldTemplateCategoriesId { get; set; }
        [DataMember]
        public List<TemplateIdAndCategoryId> pushRecordTemplateCategoriesId { get; set; }
        [DataMember]
        public string columnName { get; set; }
        [DataMember]
        public bool required { get; set; }
        [DataMember]
        public int typeId { get; set; }
        [DataMember]
        public bool pushToChild { get; set; }
        [DataMember]
        public bool inheritFromParent { get; set; }
        [DataMember]
        public bool inheritFromParentFolder { get; set; }
        [DataMember]
        public bool childInheritsValue { get; set; }
        [DataMember]
        public bool allowModifyValue { get; set; }
        [DataMember]
        public bool showInEditForm { get; set; }
        [DataMember]
        public bool allowEdit { get; set; }
        [DataMember]
        public bool? allowSort { get; set; }

        /// <summary>
        /// 是否允许在GUI上设置sort选项
        /// </summary>
        [DataMember]
        public bool allowEditSort { get; set; }
        [DataMember]
        public string optionsJSON { get; set; }
        [DataMember]
        public int optionsMaxIdReachedValue { get; set; }
        
    }
    [DataContract]
    public class TemplateIdAndCategoryId
    {
        [DataMember]
        public string tempalteId { get; set; }
        [DataMember]
        public string categoryId { get; set; }
    }
    //public enum CategoryAction
    //{
    //    Load = 1,
    //    Create = 2,
    //    Edit = 3,
    //    Remove = 4
    //};

    public class TemplateColumn4Display
    {
        public Guid UniqueId { get; set; }
        public string ColumnName { get; set; }

        public ColumnType ColumnType { get; set; }
        public Guid NameHash { get; set; }

		public bool? AllowSort { get; set; }
        public string OptionsJSON { get; set; }

        public List<Guid> IdsWithDuplicateName { set; get; } = new List<Guid>();

        public List<NameAndIdDto> Templates { get; set; } = new List<NameAndIdDto>();
    }
    [DataContract]
    public class LoadTemplateColumn4DisplayParam
    {
        [DataMember]
        public bool LoadAll { get; set; }
        [DataMember]
        public List<ColumnType> ColumnTypes { get; set; }  //valid when LoadAll is true
    }
    [DataContract]
    public class TemplateColumn4Query
    {
        [DataMember]
        public Guid UniqueId { get; set; } // column id
        [DataMember]
        public List<int> TemplateIds { get; set; }
    }

    public class TemplateConstants
    {
        public const int MaxCustomTemplateDepth = 10;

        /// <summary>
        /// minimum box size
        /// </summary>
        public const double MinBoxSize = 0.01;
    }
}
