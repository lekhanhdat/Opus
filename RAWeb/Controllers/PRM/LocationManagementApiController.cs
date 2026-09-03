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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.LocationManagement;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Tree;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.PRM
{
    [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin, preferred: false)]
    public class LocationManagementApiController : BaseApiController
    {
   

        private ILocationManagementService _LocationManagementService;
        private ILocationManagementService LocationManagementService => PlatformWindsorManager.GetService(ref _LocationManagementService);


        private string replaceStr(string sourceStr)
        {
            string resultStr = "";
            if (!string.IsNullOrEmpty(sourceStr))
            {
                Regex reg = new Regex(@"[;<>|]+");
                sourceStr = reg.Replace(sourceStr, "");
                if (!string.IsNullOrEmpty(sourceStr) && (sourceStr.Contains("&") || sourceStr.Contains("\"")))
                {
                    //替换成全角的
                    resultStr = sourceStr.Replace('&', '＆').Replace('"', '＂');
                }
                else
                {
                    resultStr = sourceStr;
                }
            }
            return resultStr;
        }


        private const string LocationSheetName = "Locations";
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        public async Task<string> ImportData()
        {
            try
            {
                var file = Request.Form.Files["locationFileUp"];
                Logger.Info("Physical location import file,file name :{0}", file.FileName);
                string extension = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
                if (extension != "csv" && extension != "xlsx")
                {
                    throw new Exception("The file is not a 'CSV' or 'XLSX' file.");
                }
                List<string[]> datas = new List<string[]>();
                if ("xlsx".Equals(extension, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        datas = ExcelUtil.ReadExcel(file.OpenReadStream(), LocationSheetName, false);
                    }
                    catch (Exception e)
                    {
                        return e.Message;
                        //if (e.ToString().Contains("Invalid Hyperlink"))
                        //{ 
                        //    UriFixer.FixInvalidUri(file.InputStream, brokenUri => UriFixer.FixUri(brokenUri));  
                        //    datas = ExcelUtil.ReadExcel(file.InputStream, LocationSheetName, false);
                        //}
                    }
                    return await LocationManagementService.ImportXlsFileAsync(datas);
                }
                else if ("csv".Equals(extension, StringComparison.OrdinalIgnoreCase))
                {
                    using (StreamReader sr = new StreamReader(file.OpenReadStream(), Encoding.UTF8))
                    {
                        while (!sr.EndOfStream)
                        {
                            string csvLine = sr.ReadLine();
                            if (csvLine != null)
                            {
                                datas.Add(CSVHelper.AnalyseCSVRow2Array(csvLine));
                            }
                        }
                    }
                    return await LocationManagementService.ImportXlsFileAsync(datas);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("error occurred import data:{0}", ex.ToString());
                throw ex;
            }
            return "ok";
        }

        [HttpPost]
        //[Microsoft.AspNetCore.Mvc.TypeFilter(typeof(ValidateAntiForgeryTokenFilterAttribute))]
        //[FileDownloadFilter]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        public ActionResult DownloadTemplate()
        {
            try
            {
                var filepath = Path.Combine(WebUtil.GetInstallPath(), "Config", "Physical Locations Import Template.xlsx");
                return File(StreamUtl.ReadFile(filepath), "application/octet-stream", Path.GetFileName(filepath));
            }
            catch
            {
                return new StatusCodeResult((int)HttpStatusCode.NoContent);
            }
        }


        #region New Physical Logic
        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        public Task<string> GetChildrenByDB([FromQuery]TreePage tree)
        {
            int pIndex = 0;
            if (tree.PageIndex != null)
            {
                int.TryParse(tree.PageIndex.ToString(), out pIndex);
            }
            int pSize = 0;
            if (tree.PageSize != null)
            {
                int.TryParse(tree.PageSize.ToString(), out pSize);
            }
            pIndex = pIndex == 0 ? pIndex : pIndex - 1;

            string nodeId = string.Empty;
            if (tree.NodeId != null)
            {
                nodeId = tree.NodeId;
            }

            string nodeType = string.Empty;
            if (tree.NodeType != null)
            {
                nodeType = tree.NodeType;
            }
            return LocationManagementService.GetLocationTreeAsync(nodeId, pIndex, pSize, tree.IconStatus);
        }

        [HttpPost]
        public Task<string> RenameRootLocation([FromBody] LocationInfo locationInfo)
        {
            return LocationManagementService.RenameLocationAsync(locationInfo.LocationId, this.replaceStr(locationInfo.Name), RMNodeLevel.PhysicalRootLocation);
        }

        [HttpPost]
        public Task<string> RenameNormalLocation([FromBody] LocationInfo locationInfo)
        {
            return LocationManagementService.RenameLocationAsync(locationInfo.LocationId, this.replaceStr(locationInfo.Name), RMNodeLevel.PhysicalNormalLocation);
        }

        [HttpPost]
        public string CreateLocation([FromBody] LocationInfo locationInfo)
        {
            if (string.IsNullOrEmpty(locationInfo.Name))
            {
                return "";
            }
            return LocationManagementService.CreateLocation(this.replaceStr(locationInfo.Name), locationInfo.ParentId);
        }

        [HttpPost]
        public async Task<bool> DeleteLocation([FromBody]int locationId)
        {
            if (locationId > 1)
            {
                return await LocationManagementService.DeleteLocationAsync(locationId);
            }
            else
            {
                return false;
            }
        }

        [HttpPost]
        public Task<RAReturnMessage> SaveLocationSetting([FromBody]LocationInfo locationSetting)
        {
            return LocationManagementService.SaveLocationSettingAsync(locationSetting);
        }

        [HttpGet]
        public string Search(string locationStr)
        {
            return LocationManagementService.SearchLocation(this.replaceStr(locationStr));
        }

        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        [HttpGet]
        public RMLocationProfileNode SearchTree(string searchKey)
        {
            return LocationManagementService.SearchLocationTree(this.replaceStr(searchKey));
        }
        
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        [HttpPost]
        public async Task<RMLocationProfileNode> GetChildren([FromBody]RMLocationProfileNode node)
        {
            if (node.PagerSize == 0)
            {
                node.PagerSize = 10;
            }
            return await LocationManagementService.GetLocationChildren(node);
        }
        #endregion
    }
}