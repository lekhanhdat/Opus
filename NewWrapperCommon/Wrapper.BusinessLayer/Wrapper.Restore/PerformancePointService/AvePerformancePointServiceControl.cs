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
using System.IO;
using System.Reflection;
using System.Xml;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.Wrapper.Restore
{
    public class AvePerformancePointServiceControl : IDisposable
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public IAveListItem ListItem { private set; get; }

        public AveSPSite Site { private set; get; }

        public IAveWeb Web { get; set; }

        public Dictionary<string, XmlElement> KpiUrlInfoMapping = new Dictionary<string, XmlElement>();

        public Dictionary<string, XmlElement> IndicatorUrlInfoMapping = new Dictionary<string, XmlElement>();

        public Dictionary<string, XmlElement> ScoreCardUrlInfoMapping = new Dictionary<string, XmlElement>();

        public Dictionary<string, XmlElement> FilterUrlInfoMapping = new Dictionary<string, XmlElement>();

        public Dictionary<string, XmlElement> ReportUrlInfoMapping = new Dictionary<string, XmlElement>();

        public static void UpdateItemProperties(AveSPSite aveSpSite)
        {
            using (var performancePointService = new AvePerformancePointServiceControl(aveSpSite))
            {
                performancePointService.Process();
            }
        }

        private void Process()
        {
            //Performance Point Item 的还原按照引用关系进行

            ProcessItemByWeb(WrapperRuntime.WrapperCache.PerformancePointCache.IndicatorInfoMapping);

            ProcessItemByWeb(WrapperRuntime.WrapperCache.PerformancePointCache.KPIInfoMapping);

            ProcessItemByWeb(WrapperRuntime.WrapperCache.PerformancePointCache.FilterInfoMapping);

            ProcessItemByWeb(WrapperRuntime.WrapperCache.PerformancePointCache.ReportInfoMapping);

            ProcessItemByWeb(WrapperRuntime.WrapperCache.PerformancePointCache.ScoreCardInfoMapping);

            ProcessItemByWeb(WrapperRuntime.WrapperCache.PerformancePointCache.DashBoardInfoMapping);

            WrapperRuntime.WrapperCache.PerformancePointCache.ClearInfoMapping();
        }

        private void ProcessItemByWeb(Dictionary<Guid, Dictionary<Guid, List<int>>> mapping)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AvePerformancePointServiceControl.ProcessItemByWeb"))
            {

                try
                {
                    foreach (var mappingElement in mapping)
                    {
                        Web = Site.SPSite.OpenWeb(mappingElement.Key);
                        if (!Web.AllowUnsafeUpdates)
                        {
                            Web.AllowUnsafeUpdates = true;
                        }
                        ProcessByItemId(mappingElement);
                    }
                }
                catch (AveSecurityTrimingException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    log.Info("Exception was thrown while ProcessItemByWeb. {0}", e);
                }


            }

        }

        private void ProcessByItemId(KeyValuePair<Guid, Dictionary<Guid, List<int>>> mappingElement)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AvePerformancePointServiceControl.ProcessByItemId"))
            {

                foreach (var itemId in mappingElement.Value)
                {
                    this.ListItem = Web.GetFile(itemId.Key).Item;
                    if (ListItem.Fields.ContainsField("PPSMA_ObjectXML"))
                    {
                        foreach (int version in itemId.Value)
                        {
                            IAveListItemVersion listItemVersion = ListItem.Versions.GetVersionFromID(version);
                            if (!listItemVersion.IsCurrentVersion)
                            {
                                PorcessItemVersion();
                                continue;
                            }
                            ProcessCurrentVersion(ListItem);
                        }
                    }
                }

            }

        }

        private void ProcessCurrentVersion(IAveListItem listItem)
        {
            try
            {
                listItem["PPSMA_ObjectXML"] = RealRestore(listItem["PPSMA_ObjectXML"]);
                listItem.SystemUpdate(false);
            }
            catch (NullReferenceException nullReference)
            {
                log.Warn("Null PPSMA_ObjectXML field while restore {0}.Exception: {1}", listItem.DisplayName, nullReference);
            }
            catch (XmlException xmlException)
            {
                log.Warn("Invalid PPSMA_ObjectXML format. Exception was thrown while load field. Exception:", xmlException);
            }
            catch (Exception e)
            {
                log.Warn("Error while Restore list item {0},Exception :", listItem.DisplayName, e.ToString());
            }
        }

        private void PorcessItemVersion()
        {
            //如果需要支持VersionItem，在这里实现
        }

        public AvePerformancePointServiceControl(AveSPSite site)
        {
            this.Site = site;
        }


        public string RealRestore(object stringXml)
        {
            var xmlDocument = new XmlDocument();

            xmlDocument.LoadXml(stringXml.ToString());

            string itemType = xmlDocument.DocumentElement.Name;

            AvePPSBase replacer = AvePPSBase.CreateInstance(itemType, this);

            //mLog.Info("Begin to replace xml content");

            string newValue = replacer.Replace(xmlDocument);
            DebugOutPut(stringXml, newValue);
            return newValue;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Desti.xml is file name. ")]
        private void DebugOutPut(object stringXml, string newValue)
        {
#if DEBUG
            try
            {
                log.Error("Test Mode");
                if (!Directory.Exists("C:\\PPSFolder"))
                {
                    Directory.CreateDirectory("C:\\PPSFolder");
                }

                string curPath = @"C:\PPSFolder\" + ListItem.Name;
                if (!Directory.Exists(curPath))
                {
                    Directory.CreateDirectory(curPath);
                }
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(stringXml.ToString());
                xmlDocument.Save(curPath + "\\Source.xml");
                xmlDocument.LoadXml(newValue);
                xmlDocument.Save(curPath + "\\Desti.xml");
            }
            catch (Exception e)
            {
                File.WriteAllText("C:\\PPSFolder\\error.txt", e.ToString());
            }
#endif
        }

        public void Dispose()
        {
            if (this.Web != null)
            {
                this.Web.Dispose();
            }            
        }
    }
}
