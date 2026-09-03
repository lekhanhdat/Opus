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
using System.Xml;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource;

namespace AvePoint.Wrapper.Restore
{
    public class AveSPPPSReportViewer : AvePPSBase
    {
        public AveSPPPSReportViewer(AvePerformancePointServiceControl avePerformancePointService) : base(avePerformancePointService)
        {
        }

        public override string Replace(XmlDocument document)
        {
             foreach (XmlElement element in document.DocumentElement)
             {
                 if(string.Equals(element.Name,"Location",StringComparison.OrdinalIgnoreCase))
                 {
                     ReplaceLocation(element);
                 }
                 if(string.Equals(element.Name,"ScorecardLocation",StringComparison.OrdinalIgnoreCase))
                 {
                     ReplaceLocation(element);
                 }
                 if(string.Equals(element.Name,"CustomData",StringComparison.OrdinalIgnoreCase))
                 {
                     ReplaceCustomData(element);
                 }
             }
            return document.OuterXml;
        }

        public override void SetInfoMapping(string url, XmlElement location)
        {
            PerformancePointService.ReportUrlInfoMapping.Add(url, location);
        }

        private void ReplaceCustomData(XmlElement customData)
        {
            XmlDocument customDataDocument = new XmlDocument();
            try
            {
                customDataDocument.LoadXml(customData.InnerText.Trim());
                //处理相关的数据,现在只处理了Report Servcie，Excel Services 和Analysis service 以后在实现

                string reportType = customDataDocument.DocumentElement.Name;

                switch (reportType)
                {
                    case "ReportView":
                        //Support Report Service Report
                        RepplaceReportServiceData(customDataDocument.DocumentElement);
                        break;
                    case "OLAPReportView":
                        //Support Analyse Service Report
                        RestoreAnalysisServiceReportData(customDataDocument.DocumentElement);
                        break;
                    default:
                        //For Excel Services 和Analysis service 实现
                        break;
                }
            }
            catch (XmlException)
            {
                customData.InnerText = ReplaceDefault(customData.InnerText);
                return;
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, WrapperRestoreResource.ReplaceCustomValueError, e);
            }
            customData.InnerText = customDataDocument.OuterXml;
        }

        private void RestoreAnalysisServiceReportData(XmlElement customDataRootElement)
        {
            foreach (XmlElement dataSource in customDataRootElement.GetElementsByTagName("DataSourceLocation"))
            {
                ReplaceDataSourceLocation(dataSource);
            }
        }

        private void RepplaceReportServiceData(XmlElement xmlElement)
        {
            if(xmlElement.HasAttribute("ReportUrl"))
            {
                string oldUrl = xmlElement.GetAttribute("ReportUrl");
                xmlElement.SetAttribute("ReportUrl", ReplaceDefault(oldUrl));
            }

            //Report Server Url 为Farm唯一的，是否需要替换？
        }             
    }
}