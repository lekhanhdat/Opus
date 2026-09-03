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
using System.Web;
using System.Web.Script.Serialization;
using System.Xml;

namespace AvePoint.RA.SharePoint.RMExplorer
{
    //[Serializable]
    //public class RMRelatedItemInfo
    //{
    //    #region
    //    public int DocLibRowId { get; set; }
    //    public string name { get; set; }
    //    public bool NeedDelete { get; set; }
    //    public string url { get; set; }
    //    public Guid id { get; set; }//ItemUniqueId
    //    #endregion
    //    public string SiteUrl { get; set; }
    //    public Guid SiteId { get; set; }
    //    public string WebUrl { get; set; }
    //    public Guid WebId { get; set; }
    //    public string WebServerRelativeUrl { get; set; }
    //    public Guid ListId { get; set; }
    //    public string ListUrl { get; set; }
    //    public Guid FolderId { get; set; }
    //    public string FolderUrl { get; set; }

    //    //public Guid ItemId { get; set; }
    //    //public int DocLibRowId { get; set; }
    //    public string ItemUrl { get; set; }
    //    public bool ParentFolderIsRootFolder { get; set; }
    //    public SOEndUserArchiverNodeLevel level { get; set; }
    //}

    //public class RelatedItemUtil
    //{
    //    public List<RMRelatedItemInfo> GetRelatedProperties(string recordsRelatedValue)
    //    {
    //        if (!string.IsNullOrEmpty(recordsRelatedValue))
    //        {
    //            var sourceUrlValue = recordsRelatedValue;
    //            List<RMRelatedItemInfo> infos = new List<RMRelatedItemInfo>();
    //            XmlDocument xmlDoc = new XmlDocument();
    //            // sourceUrlValue = HttpUtility.UrlDecode(sourceUrlValue);//??
    //            sourceUrlValue = sourceUrlValue.Replace("&#58;", ":");
    //            xmlDoc.LoadXml(sourceUrlValue);
    //            foreach (var ele in xmlDoc.GetElementsByTagName("a"))
    //            {
    //                XmlElement element = ele as XmlElement;
    //                var relatedObjString = element.GetAttribute("rel");
    //                relatedObjString = HttpUtility.UrlDecode(relatedObjString);
    //                //var parmArray = relatedWebURL.Split(';');
    //                //Dictionary<string, string> parmDic = new Dictionary<string, string>();
    //                //foreach (var parm in parmArray)
    //                //{
    //                //    var a = parm.Split('=');
    //                //    parmDic.Add(a[0], a[1]);
    //                //}
    //                JavaScriptSerializer jss = new JavaScriptSerializer();
    //                RMRelatedItemInfo relatedObj = jss.Deserialize<RMRelatedItemInfo>(relatedObjString);
    //                var relatedItemUrl = HttpUtility.UrlDecode(element.GetAttribute("href"));
    //                //string url = string.Empty;
    //                string url = relatedItemUrl;
    //                relatedObj.url = relatedItemUrl;
    //                //if (!element.GetAttribute("href").StartsWith(relatedObj.SiteUrl))//parmDic["SiteUrl"]))
    //                //{
    //                //    //var webServerRelativeUrl = currentWeb.ServerRelativeUrl;
    //                //    //url = element.GetAttribute("href").Substring(webServerRelativeUrl.TrimEnd('/').Length + 1);
    //                //    //url = parmDic["SiteUrl"] + "/" + url;
    //                //    var webServerRelativeUrl = currentWeb.ServerRelativeUrl;
    //                //    url = element.GetAttribute("href").Substring(webServerRelativeUrl.TrimEnd('/').Length + 1);
    //                //    url = relatedObj.SiteUrl + "/" + url;
    //                //}
    //                relatedObj.url = url;
    //                infos.Add(relatedObj);
    //            }
    //            return infos;
    //        }
    //        return null;
    //    }

    //}

    //[Serializable]
    //public enum SOEndUserArchiverNodeLevel
    //{
    //    None = 0,
    //    Site = 1,
    //    Web = 2,
    //    List = 3,
    //    Folder = 4,
    //    Item = 5,
    //    Document = 6,
    //    Multifiles = 7,
    //    Attachment = 8
    //}
}
