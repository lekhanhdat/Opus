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
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics.CodeAnalysis;
using AvePoint.GCommon;
using System.Reflection;
using System.Text.RegularExpressions;
using AvePoint.GCommon.Contract.Storage.Entity;
using Storage;

namespace RAExportCommon
{
    public class GeneratorManifest
    {
        private const string ERROR = @"[!ERROR!]";
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        Manifest_Schema.SetManifest veo_Manifest = null;
        Manifest_Schema.ElectronicTransfer veo_ElectronicTransfer = null;
        List<Manifest_Schema.ManifestObjectItem> items = new List<Manifest_Schema.ManifestObjectItem>();
        private ManifestVEOXML manifestXML = null;
        private PhysicalDeviceDto deviceInfo = null;

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        private void Initialization()
        {
            DateTime dateNow = DateTime.Now;
            veo_Manifest = new Manifest_Schema.SetManifest();

            #region 依照Manifest文档规定，不符合VEO文档规定显示ERROR.ADO-173280

            //Consignment Type：The value must consist of one or two alphabetic characters.
            string tempConsignmentType = manifestXML.ConsignmentType.DefaultValue;
            tempConsignmentType = new Regex("^[a-zA-Z]{1,2}$", RegexOptions.IgnoreCase).IsMatch(tempConsignmentType) == true ? tempConsignmentType : ERROR;

            //Consignment Number:This must be four digits long, and padded with leading zeros if necessary.
            string tempConsignmentNumber = manifestXML.ConsignmentNumber.DefaultValue;
            tempConsignmentNumber = new Regex(@"^\d{4}$").IsMatch(tempConsignmentNumber) == true ? tempConsignmentNumber : ERROR;

            //Job Identification number must start with two alpha characters, followedby a space, then 4 numeric characters, a forward slash, and finally four further numeric characters.
            string tempJobID = manifestXML.JobID.DefaultValue;
            tempJobID = new Regex(@"^[a-zA-Z]{2}\s\d{4}/\d{4}$", RegexOptions.IgnoreCase).IsMatch(tempJobID) == true ? tempJobID : ERROR;

            #endregion

            veo_ElectronicTransfer = new Manifest_Schema.ElectronicTransfer();
            veo_ElectronicTransfer.created_timestamp = manifestXML.CreatedTimeStamp == null?null: dateNow.ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
            veo_ElectronicTransfer.agency_id = CustomizeProperty.GetPropertyValue(manifestXML.AgencyIdentifier.DefaultValue);
            veo_ElectronicTransfer.consignment_number = CustomizeProperty.GetPropertyValue(tempConsignmentNumber);
            veo_ElectronicTransfer.consignment_type = CustomizeProperty.GetPropertyValue(tempConsignmentType);
            veo_ElectronicTransfer.series_number = CustomizeProperty.GetPropertyValue(manifestXML.SeriesNumber.DefaultValue);
            veo_ElectronicTransfer.series_type = manifestXML.SeriesType == null ? null :Manifest_Schema.SeriesType.VPRS.ToString();
            veo_ElectronicTransfer.job_id = CustomizeProperty.GetPropertyValue(tempJobID);//"TR 2012/0015";
            veo_Manifest.Item = veo_ElectronicTransfer;
        }


        [SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison")]
        private Manifest_Schema.ManifestObjectItem GeneratorManifestObjectItem(XmlDocument doc, string filename, string size)
        {
            Manifest_Schema.ManifestObjectItem item = new Manifest_Schema.ManifestObjectItem();
            item.computer_filename = manifestXML.ComputerFileName == null? null: filename;
            item.file_identifier = new string[1];
            string fileid = CustomizeProperty.GetPropertyValue(manifestXML.FileIdentifier.DefaultValue, manifestXML.FileIdentifier.ElementName, doc);
            mLog.Info("Generator Manifest Object Item File ID: {0}.", fileid);
            //ADO-167986 为防止客户随便添加column mapping，对的column value截取进行异常处理，防止Job异常
            //try
            //{
            //    if (fileid.Length > 15)
            //    {
            //        string temp1 = string.Empty;
            //        string temp2 = string.Empty;
            //        temp1 = fileid.Substring(0, 36).Replace("-", "");
            //        temp2 = fileid.Substring(fileid.LastIndexOf("_") + 1);
            //        if (temp2.Length >= 12)
            //        {
            //            fileid = temp1.Substring(0, 7) + temp2.Substring(0, 6) + temp2.Substring(10, 2);
            //        }
            //        else
            //        {
            //            fileid = temp1.Substring(0, 7) + temp2;
            //            if (fileid.Length > 15)
            //            {
            //                fileid = fileid.Substring(0, 15);
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    mLog.Info("An error occur while get field ID,Message: {0}.", ex.ToString());
            //    if (fileid.Length > 15)
            //    {
            //        fileid = fileid.Substring(0, 15);
            //    }
            //}

            item.file_identifier[0] = fileid;

            string recordid = CustomizeProperty.GetPropertyValue(manifestXML.VersRecordIdentifier.DefaultValue, manifestXML.VersRecordIdentifier.ElementName, doc);
            mLog.Info("Generator Manifest Object Item Record ID: {0}.", recordid);
            if (recordid.Length > 15)
            {
                recordid = recordid.Substring(0, 15);
            }
            //file veo 获取不到record ID，需要赋值为空.
            item.vers_record_identifier = recordid == @"[!ERROR!]" ? string.Empty : recordid;
            item.size_kb = manifestXML.SizeKB == null?null: size;
            item.veo_access_category = CustomizeProperty.GetPropertyValue(manifestXML.VEOAccessCategory.DefaultValue, manifestXML.VEOAccessCategory.ElementName, doc);
            item.veo_classification = CustomizeProperty.GetPropertyValue(manifestXML.VEOClassification.DefaultValue, manifestXML.VEOClassification.ElementName, doc);
            item.veo_date_range = new Manifest_Schema.VeoDateRange();
            item.veo_date_range.veo_start_date = CustomizeProperty.GetPropertyValue(manifestXML.VEODateRange.StartDate.DefaultValue, manifestXML.VEODateRange.StartDate.ElementName, doc);
            // VEO 文档中规定，End TimeIf the VEO is a RecordVEO, the end date is empty，If the VEO is a File VEO, the end date isvers: DateTimeClosed(M144), if present, otherwise it is empty
            string endDate = CustomizeProperty.GetPropertyValue(manifestXML.VEODateRange.EndDate.DefaultValue, manifestXML.VEODateRange.EndDate.ElementName, doc);
            item.veo_date_range.veo_end_date = endDate == @"[!ERROR!]" ? string.Empty : endDate;

            item.veo_disposal_authority = CustomizeProperty.GetPropertyValue(manifestXML.VEODisposalAutority.DefaultValue, manifestXML.VEODisposalAutority.ElementName, doc);
            item.veo_title = CustomizeProperty.GetPropertyValue(manifestXML.VEOTitle.DefaultValue, manifestXML.VEOTitle.ElementName, doc);
            return item;
        }

        private void Finally(string JobID)
        {
            if (items.Count > 0)
            {
                try
                {
                    using (DeviceUtil deviceUtil = new DeviceUtil())
                    {
                        deviceUtil.Open(this.deviceInfo);
                        List<XDirectoryInfo> directories = deviceUtil.GetDirectories(JobID).Where(d => d.Name == JobID).ToList();
                        List<XFileInfo> manifests = deviceUtil.GetFiles(directories[0]).Where(f => f.Name == "manifest.xml").ToList();

                        if (manifests.Count != 0)
                        {
                            using (Stream sm = deviceUtil.OpenStream(manifests[0]))
                            {
                                Manifest_Schema.SetManifest manifestSet = (Manifest_Schema.SetManifest)new XmlSerializer(typeof(Manifest_Schema.SetManifest)).Deserialize(sm);
                                Manifest_Schema.ElectronicTransfer source_VEO_ElectronicTransfer = (Manifest_Schema.ElectronicTransfer)manifestSet.Item;
                                List<Manifest_Schema.ManifestObjectItem> sourceManifestObjectItem = source_VEO_ElectronicTransfer.manifest_object_list.ToList();
                                items.AddRange(sourceManifestObjectItem);
                                veo_ElectronicTransfer.manifest_object_list = items.ToArray();
                                veo_Manifest.Item = veo_ElectronicTransfer;
                                mLog.Info(string.Format("Generator Manifest, destination have manifest xml, Manifest Object Item Count:{0}, new Manifest Object Item Count:{1}.", sourceManifestObjectItem.Count, items.Count));
                            }
                        }
                        else
                        {
                            veo_ElectronicTransfer.manifest_object_list = items.ToArray();
                            veo_Manifest.Item = veo_ElectronicTransfer;
                        }
                    }
                }
                catch (Exception ex)
                {
                    mLog.Info(string.Format("An error occur while finally generator manifest. Message:{0}.", ex.ToString()));
                    veo_ElectronicTransfer.manifest_object_list = items.ToArray();
                    veo_Manifest.Item = veo_ElectronicTransfer;
                }
            }
            else
            {
                veo_Manifest = null;
            }
        }

        //need to do some change for multi phy
        public GeneratorManifest(ManifestVEOXML manifestXML, PhysicalDeviceDto physicalDevice = null)
        {
            this.manifestXML = manifestXML;
            this.deviceInfo = physicalDevice;
            Initialization();
        }

        public void AddItem(XmlDocument veoObj, string filename, string size)
        {
            Manifest_Schema.ManifestObjectItem item = GeneratorManifestObjectItem(veoObj, filename, size);

            items.Add(item);
        }

        //public void AddItem(FileVEOClass.VERSEncapsulatedObject veoObj, string filename, string size)
        //{
        //    Manifest_Schema.ManifestObjectItem item = GeneratorManifestObjectItem(veoObj, filename, size);

        //    items.Add(item);
        //}

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public Stream GenerateManifestStream(string JobID)
        {
            this.Finally(JobID);
            if (veo_Manifest != null)
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("dam", "http://www.prov.vic.gov.au/digitalarchive/");
                ns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");

                XmlSerializer xs = new XmlSerializer(typeof(Manifest_Schema.SetManifest));
                using (Stream memStream = new MemoryStream())
                {
                    xs.Serialize(memStream, veo_Manifest, ns);
                    memStream.Position = 0;
                    Stream tempStream = new MemoryStream();
                    XmlDocument doc = new XmlDocument();
                    doc.Load(memStream);
                    doc.XmlResolver = null;
                    XmlElement root = doc.DocumentElement;
                    XmlAttribute att = doc.CreateAttribute("xsi", "schemaLocation", "http://www.w3.org/2001/XMLSchema-instance");
                    att.Value = "http://www.prov.vic.gov.au/digitalarchive/" + " " + "http://www.prov.vic.gov.au/digitalarchive/setManifest_1_0_0.xsd";
                    root.Attributes.Append(att);
                    doc.Save(tempStream);
                    tempStream.Seek(0, SeekOrigin.Begin);

                    return tempStream;
                }
            }
            else
            {
                return null;
            }
        }
    }
}
