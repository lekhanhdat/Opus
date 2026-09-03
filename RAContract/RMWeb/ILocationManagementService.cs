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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.LocationManagement;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Tree;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb
{
    public interface ILocationManagementService
    {
        //string GetTaxonomyTreeData(string typeName, string treeNodeId, int pageIndex, int pageCount);
        //bool DeleteTerm(Guid termId);
        //string Search(int termSetId, string termLabel);
        string RunImportPhysicalFilesAndRecords(JobRunBy jobRunBy, string upFilePath, int settingId, int customId = 0);

        /// <summary>
        /// 导入Zip包
        /// </summary>
        /// <param name="jobRunBy"></param>
        /// <param name="upFilePath"></param>
        /// <param name="settingId"></param>
        /// <returns></returns>
        string RunImportPhysicalZipFilesAndRecords(JobRunBy jobRunBy, string upFilePath, int settingId);

        /// <summary>
        /// 导出Zip包
        /// </summary>
        /// <param name="jobRunBy"></param>
        /// <param name="templateIds"></param>
        /// <returns></returns>
        string RunExportPhysicalZipFilesAndRecords(JobRunBy jobRunBy, string templateIds);
        //FileInfo GetPhysicalTemplateFileInfo(string filePath);
        //Dictionary<string, int> GetPhysicalSettingsInfo();
        //string CreateTerm(string termName, int parentTermId, int termSetId);
        //string RenameTerm(int termId, string termName, int termSetId);
        //string UpdateTermSet(int termSetId, string termSetName, string des);
        Task<string> RealRunImportPhysicalFilesAndRecordsAsync(JobRunBy jobRunBy, string jobRunByUser, string upFilePath, int settingId, int customId = 0);
        Task<string> RealRunImportPhysicalZipFilesAndRecordsAsync(JobRunBy jobRunBy, string jobRunByUser, string upFilePath, int settingId);
        Task<string> RealRunExportPhysicalZipFilesAndRecordsAsync(JobRunBy jobRunBy, string jobRunByUser, string templateIds);
        Task<string> ImportXlsFileAsync(List<string[]> xlsFileContent);
        #region New Physical Logic
        Task<PhysicalObjectDto> GetPhysicalObjectByIdAsync(int id);
        Task<PhysicalResultInfo> QueryPhysicalNodesAsync(PhysicalExplorerQueryDto dto);
        Task<string> GetLocationTreeAsync(string treeNodeId, int pageIndex, int pageCount, bool iconStatus);
        Task<string> RenameLocationAsync(int locationId, string name, RMNodeLevel nodeType);
        string CreateLocation(string name, int parentId);
        Task<bool> DeleteLocationAsync(int locationId);
        string SearchLocation(string locationStr);
        RMLocationProfileNode SearchLocationTree(string searchKey);
        Task<RMLocationProfileNode> GetLocationChildren(RMLocationProfileNode node);
        RMLocationProfileNode Convert2ProfileNode(int locationId, bool widthChildIDs = false, bool isChecked = false);
        Task<RAReturnMessage> SaveLocationSettingAsync(LocationInfo locationSetting);
        string GetLocationPathById(Guid id, bool isReplaceI18NKey = true);
        bool CheckPhysicalRootLocation(string treeNode);
        #endregion

    }
}
