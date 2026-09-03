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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.UI;
using System.Xml;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.ServerAPI2010;

namespace AvePoint.Wrapper.Common
{
    //Moved from Server module
    public class AveWebPartUtility
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        //resultCode:1,不能解析allUsers；2，不能解析perUser
        public static Dictionary<string, object> GetProperties(byte[] allUsers, byte[] perUser, out int resultCode)
        {
            resultCode = 0;
            Dictionary<string, object> properties = new Dictionary<string, object>();
            try
            {
                bool flag1 = false;
                bool flag2 = false;
                if (allUsers != null && allUsers[0] == 0x01 && allUsers[1] == 0x05)
                {
                    flag1 = true;
                }
                if (perUser != null && perUser[0] == 0x01 && perUser[1] == 0x05)
                {
                    flag2 = true;
                }

                if (flag1 && flag2)
                {
                    Parse0X0105(allUsers, perUser, properties);
                }
                else if (flag1 && !flag2)
                {
                    Parse0X0105(allUsers, null, properties);
                    try
                    {
                        object[] objects = Parse0xFF01(perUser);
                        if (objects != null)
                        {
                            GetApplyProperties(objects, properties);
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, ServerAPIResource.GetPropertyError, e.ToString());
                        resultCode = resultCode | 2;
                    }
                }
                else if (!flag1 && flag2)
                {
                    try
                    {
                        object[] objects = Parse0xFF01(allUsers);
                        if (objects != null)
                        {
                            GetApplyProperties(objects, properties);
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, ServerAPIResource.GetPropertyError, e.ToString());
                        resultCode = resultCode | 1;
                    }
                    Parse0X0105(null, perUser, properties);
                }
                else if (!flag1 && !flag2)
                {
                    object[] objects = null;
                    try
                    {
                        objects = Parse0xFF01(allUsers);
                        if (objects != null)
                        {
                            GetApplyProperties(objects, properties);
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, ServerAPIResource.GetPropertyError, e.ToString());
                        resultCode = resultCode | 1;
                    }
                    try
                    {
                        objects = Parse0xFF01(perUser);
                        if (objects != null)
                        {
                            GetApplyProperties(objects, properties);
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, ServerAPIResource.GetPropertyError, e.ToString());
                        resultCode = resultCode | 2;
                    }
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, ServerAPIResource.GetPropertyError, e.ToString());
                resultCode = 3;
            }
            return properties;
        }

        private static Dictionary<string, object> GetApplyProperties(object[] objects, Dictionary<string, object> properties)
        {
            if (properties == null)
            {
                properties = new Dictionary<string, object>();
            }
            ApplyProperty apply = new ApplyProperty(objects);
            Dictionary<string, object> temProperties = apply.ApplyPropertyState();
            if (temProperties != null)
            {
                foreach (string key in temProperties.Keys)
                {
                    if ((temProperties[key] as Pair) != null)
                    {
                        properties[key] = (temProperties[key] as Pair).Second;
                    }
                    else
                    {
                        properties[key] = temProperties[key];
                    }
                }
            }
            return properties;
        }

        private static object[] DeserializeByteArrayToObject(byte[] bytes)
        {
            ObjectStateFormatter formatter = new ObjectStateFormatter();
            object[] values = null;
            if ((bytes != null) && (bytes.Length != 0))
            {
                values = (object[])formatter.Deserialize(new MemoryStream(bytes));
            }
            return values;
        }

        private static object[] Parse0xFF01(byte[] bts)
        {
            byte[] temp1 = null;
            byte[] temp2 = null;
            if (bts != null)
            {
                for (int i = 0; i < bts.Length; i++)
                {
                    if (bts[i] == 0xff && bts[i + 1] == 0x01)
                    {
                        temp1 = new byte[i];
                        temp2 = new byte[bts.Length - i];
                        for (int j = 0; j < bts.Length; j++)
                        {
                            if (j < temp1.Length)
                            {
                                temp1[j] = bts[j];
                            }
                            else
                            {
                                temp2[j - temp1.Length] = bts[j];
                            }
                        }
                        break;
                    }
                }
                if (temp2 != null)
                {
                    return DeserializeByteArrayToObject(temp2);
                }
            }
            return null;
        }

        private static Dictionary<string, object> Parse0X0105(byte[] allUsers, byte[] perUser, Dictionary<string, object> properties)
        {
            //Assembly assem = Assembly.GetAssembly(typeof(Microsoft.SharePoint.WebPartPages.WebPart));
            //Type WebPartNameTable = assem.GetType("Microsoft.SharePoint.WebPartPages.WebPartNameTable");
            //Type CompressedXmlReader = assem.GetType("Microsoft.SharePoint.WebPartPages.CompressedXmlReader");
            //ConstructorInfo constructorInfo1 = WebPartNameTable.GetConstructors()[0];
            //object obj = constructorInfo1.Invoke(null);
            //ConstructorInfo constructorInfo2 = CompressedXmlReader.GetConstructors()[0];
            //XmlReader reader = (XmlReader)constructorInfo2.Invoke(new object[] { new XmlNamespaceManager((XmlNameTable)obj), allUsers, perUser });
            CompressedXmlReader reader = new CompressedXmlReader(new XmlNamespaceManager(new WebPartNameTable()), perUser, allUsers);
            if (properties == null)
            {
                properties = new Dictionary<string, object>();
            }
            string propertyName = string.Empty;
            object value = null;
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        propertyName = reader.LocalName;
                        break;
                    case XmlNodeType.CDATA:
                    case XmlNodeType.Text:
                        value = reader.Value;
                        break;
                    case XmlNodeType.EndElement:
                        if (!String.IsNullOrEmpty(propertyName) && propertyName != "WebPart")
                        {
                            properties[propertyName] = value;
                        }
                        propertyName = String.Empty;
                        value = String.Empty;
                        break;
                    default:
                        break;
                }
            }
            return properties;
        }

        public static string SerializeWebPartConnection(object obj)
        {
            string result = string.Empty;
            ObjectStateFormatter formatter = new ObjectStateFormatter();
            MemoryStream stream = new MemoryStream();
            try
            {
                formatter.Serialize(stream, obj);
                byte[] buffer = stream.GetBuffer();
                result = Convert.ToBase64String(buffer);
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.DEBUG, "Failed to serialize web part connection. Error: {0}", ex.ToString());
            }
            finally
            {
                stream.Close();
            }
            return result;
        }

        public static object DeserializeWebPartConnection(string text)
        {
            object result = null;
            try
            {
                ObjectStateFormatter formatter = new ObjectStateFormatter();
                byte[] buffer = Convert.FromBase64String(text);
                result = formatter.Deserialize(new MemoryStream(buffer));
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.DEBUG, "Failed to deserialize web part connection. Error: {0}", ex.ToString());
            }
            return result;
        }

        public static System.Web.UI.WebControls.Unit ConvertStringToUnit(string value)
        {
            System.Web.UI.WebControls.UnitConverter converter = new System.Web.UI.WebControls.UnitConverter();
            return (System.Web.UI.WebControls.Unit)converter.ConvertFromString(null, System.Globalization.CultureInfo.InvariantCulture, value);
        }
    }

    internal class ApplyProperty
    {
        private int m_index = 3;
        private object[] m_activeObjects;
        private short m_segmentType;
        private int SerializationMajorVersion;
        private int SerializationMinorVersion;
        private int m_count;
        private string[] predefinedStrings;

        public ApplyProperty(object[] array)
        {
            m_activeObjects = array;
            predefinedStrings = GetPredefinedStrings();
            if (m_activeObjects.Length > 2)
            {
                if (m_activeObjects[0] is int)
                {
                    SerializationMajorVersion = (int)m_activeObjects[0];
                }
                if (m_activeObjects[1] is int)
                {
                    SerializationMinorVersion = (int)m_activeObjects[1];
                }
            }
        }

        protected bool GetNextSegment()
        {
            if ((this.m_activeObjects != null) && (this.m_index >= this.m_activeObjects.Length))
            {
                return false;
            }
            this.m_segmentType = (this.m_activeObjects[this.m_index] is SegmentType) ? ((short)((SegmentType)this.m_activeObjects[this.m_index++])) : ((short)this.m_activeObjects[this.m_index++]);
            while (this.m_segmentType >= 5)
            {
                this.m_index += 1 + ((int)this.m_activeObjects[this.m_index]);
                if (this.m_index >= this.m_activeObjects.Length)
                {
                    return false;
                }
                this.m_segmentType = (this.m_activeObjects[this.m_index] is SegmentType) ? ((short)((SegmentType)this.m_activeObjects[this.m_index++])) : ((short)this.m_activeObjects[this.m_index++]);
            }
            return true;

        }

        protected short GetSegmentType()
        {
            return this.m_segmentType;
        }

        protected int ObjectCount()
        {
            this.m_count = (int)this.m_activeObjects[this.m_index++];
            return this.m_count;
        }

        protected string[] GetPredefinedStrings()
        {
            //Assembly assem = Assembly.GetAssembly(typeof(Microsoft.SharePoint.WebPartPages.WebPart));
            //Type xmlSchema = assem.GetType("Microsoft.SharePoint.WebPartPages.XmlSchema");
            //FieldInfo[] fields = xmlSchema.GetFields();
            //string[] predefinedStrings = new string[fields.Length];

            //for (ushort i = 0; i < fields.Length; i++)
            //{
            //    FieldInfo info = fields[i];
            //    string s = (string)info.GetValue(null);
            //    if (s != null)
            //    {
            //        predefinedStrings[i] = s;
            //    }
            //}
            //return predefinedStrings;
            return PredefinedStrings.PREDEFINEDSTRINGS;
        }

        protected string ResolveTokenizedString(int key)
        {
            if (key < predefinedStrings.Length)
            {
                return predefinedStrings[key];
            }
            return null;
        }

        protected object GetNextObject()
        {
            return this.m_activeObjects[this.m_index++];
        }

        protected void SkipSegment()
        {
            this.m_index += 1 + ((int)this.m_activeObjects[this.m_index]);
        }

        public Dictionary<string, object> ApplyPropertyState()
        {
            if (m_activeObjects.Length < 3)
            {
                return null;
            }
            Dictionary<string, object> properties = new Dictionary<string, object>();
            if ((2 == this.SerializationMajorVersion) && ((3 == this.SerializationMinorVersion) || (2 == this.SerializationMinorVersion)))
            {
                while (this.GetNextSegment())
                {
                    int num;
                    string key = string.Empty;
                    object nextObject;
                    switch (this.GetSegmentType())
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                            num = this.ObjectCount();
                            while (num > 0)
                            {
                                nextObject = this.GetNextObject();
                                if (nextObject is int)
                                {
                                    key = ResolveTokenizedString((int)nextObject);
                                }
                                else
                                {
                                    key = nextObject.ToString();
                                }
                                nextObject = this.GetNextObject();
                                properties.Add(key, nextObject);
                                num -= 2;
                            }
                            continue;

                        case 4:
                            this.SkipSegment();
                            continue;
                        default:
                            break;
                    }
                    num = this.ObjectCount();
                    while (num > 0)
                    {
                        int index = (int)this.GetNextObject();
                        num--;
                    }
                    continue;
                }
            }
            return properties;
        }

        internal enum SegmentType : byte
        {
            AttachedProperties = 3,
            IPersonalizableProperties = 2,
            LinkMap = 4,
            NonPersonalizableProperties = 1,
            PersonalizableProperties = 0,
            Unknown = 5
        }
    }
    public class WebPartUpdaterMappings
    {
        private static Dictionary<Guid, AveWebPartType> WebPartUpdaterMapping = new Dictionary<Guid, AveWebPartType>();

        static WebPartUpdaterMappings()
        {
            //2007
            WebPartUpdaterMapping[new Guid("74bd016c-baa0-14a8-d5d8-b75dc7e6f429")] = AveWebPartType.ContactDetailWebPart;
            WebPartUpdaterMapping[new Guid("f62babb5-a14d-11a7-ae1a-537c36fc53ae")] = AveWebPartType.CategoryResultsWebPart;
            WebPartUpdaterMapping[new Guid("9f030319-fa14-b625-4892-89f6f9f9d58b")] = AveWebPartType.TableOfContentsWebPart;
            WebPartUpdaterMapping[new Guid("2f1510c7-75d5-921f-b120-2ce98fe3afe3")] = AveWebPartType.ContentByQueryWebPart;
            WebPartUpdaterMapping[new Guid("a2e08067-888b-2ca1-4b3d-2bb33bdc3b37")] = AveWebPartType.ThisWeekInPicturesWebPart;
            WebPartUpdaterMapping[new Guid("b9a7f972-708a-cd77-4ffd-a235dfed5c38")] = AveWebPartType.DataFormWebPart;
            WebPartUpdaterMapping[new Guid("e60f6c95-e86c-4717-2c0d-6d8563c9caf7")] = AveWebPartType.ContentEditorWebPart;
            //
            WebPartUpdaterMapping[new Guid("7494019e-cc3c-dc3d-88ee-f9782d55ba37")] = AveWebPartType.TableOfContentsWebPart;
            WebPartUpdaterMapping[new Guid("b2b35bdf-5e78-ab22-5351-6639ca63203f")] = AveWebPartType.ContentEditorWebPart;
            WebPartUpdaterMapping[new Guid("b4bd2bdf-cf0c-ffce-ecb1-ae7c4882e17a")] = AveWebPartType.ExcelWebRendererWebPart;
            WebPartUpdaterMapping[new Guid("d9731c15-6aeb-ae5f-0994-e8f6bd13ff10")] = AveWebPartType.VisioWebAccessWebPart;
            WebPartUpdaterMapping[new Guid("107AB2DC-58A6-809C-9B41-F2E17E6E064F")] = AveWebPartType.ContentByQueryWebPart;
            WebPartUpdaterMapping[new Guid("874f5460-71f9-fecc-e894-e7e858d9713e")] = AveWebPartType.XsltListViewWebPart;
            WebPartUpdaterMapping[new Guid("2242cce6-491a-657a-c8ee-b10a2a993eda")] = AveWebPartType.ListViewWebPart;
            WebPartUpdaterMapping[new Guid("7fbf9a80-8ae1-fa7e-9c51-30a786d33155")] = AveWebPartType.ListViewWebPart;
            WebPartUpdaterMapping[new Guid("baf5274e-a800-8dc3-96d0-0003d9405663")] = AveWebPartType.ListViewWebPart;
            WebPartUpdaterMapping[new Guid("9f56656f-6aa3-0d55-a812-711bf65864ea")] = AveWebPartType.ListFormWebPart;
            WebPartUpdaterMapping[new Guid("1a8eda1f-6a8c-d5b9-0a7a-062455488c90")] = AveWebPartType.ListFormWebPart;
            WebPartUpdaterMapping[new Guid("293e8d0e-486f-e21e-40e3-75bfb77202de")] = AveWebPartType.ListFormWebPart;
            WebPartUpdaterMapping[new Guid("bdf3c494-4f90-8428-15f5-49220aa08d98")] = AveWebPartType.SummaryLinkWebPart;
            WebPartUpdaterMapping[new Guid("db128878-9a93-4768-2256-cc2c390ffb57")] = AveWebPartType.SummaryLinkWebPart;
            WebPartUpdaterMapping[new Guid("9afe11f2-9603-ac36-62a9-debeb61bcac0")] = AveWebPartType.TagCloudWebPart;
            WebPartUpdaterMapping[new Guid("e25ec220-41d8-6e8e-2d58-d685e621a47e")] = AveWebPartType.SocialCommentWebPart;
            WebPartUpdaterMapping[new Guid("11525f26-1b2e-a3c2-ced4-6259ce71c159")] = AveWebPartType.MediaWebPart;
            WebPartUpdaterMapping[new Guid("53151e66-1f43-e802-2dde-f459d09d97be")] = AveWebPartType.SiteDocuments;
            WebPartUpdaterMapping[new Guid("2fc2e287-55c9-b5d1-0d5c-7458bc3c9841")] = AveWebPartType.ContactDetailWebPart;
            WebPartUpdaterMapping[new Guid("2e1a7e3e-8464-a4ce-aedb-47b04678f859")] = AveWebPartType.DataFormWebPart;
            WebPartUpdaterMapping[new Guid("B1DC92E2-8558-F555-AE81-35ED9DDF1644")] = AveWebPartType.BrowserFormWebPart;
            WebPartUpdaterMapping[new Guid("B5D9F5EA-9147-6D6A-2BF1-C434E144A2CD")] = AveWebPartType.MembersWebPart;
            //2013 15
            WebPartUpdaterMapping[new Guid("bd5d3ea4-8040-1691-574c-5bdad906238d")] = AveWebPartType.TableOfContentsWebPart;
            WebPartUpdaterMapping[new Guid("4c06cea2-364f-47e3-e1d7-08d53f441157")] = AveWebPartType.ContentEditorWebPart;
            WebPartUpdaterMapping[new Guid("066cabc4-48cb-ae18-e7c6-953875ac7ed6")] = AveWebPartType.ExcelWebRendererWebPart;
            WebPartUpdaterMapping[new Guid("bfff2915-72aa-45d2-5929-54d47ab82a4e")] = AveWebPartType.VisioWebAccessWebPart;
            WebPartUpdaterMapping[new Guid("c13236c3-5cc0-ad43-e5cc-8790ba11a7bb")] = AveWebPartType.ContentByQueryWebPart;
            WebPartUpdaterMapping[new Guid("a6524906-3fd2-ee4e-23ee-252d3c6e0dc9")] = AveWebPartType.XsltListViewWebPart;
            WebPartUpdaterMapping[new Guid("05d0fd94-372a-5ee7-b480-ccb8f9cd2c23")] = AveWebPartType.ListViewWebPart;
            WebPartUpdaterMapping[new Guid("42fddde2-e0cf-c8ab-48b7-db1fcac0a917")] = AveWebPartType.ListFormWebPart;
            WebPartUpdaterMapping[new Guid("62961f97-6029-0309-2def-fa1531f5f226")] = AveWebPartType.SummaryLinkWebPart;
            WebPartUpdaterMapping[new Guid("eb962a66-5ba1-76c6-4a2f-eaaea9486f91")] = AveWebPartType.TagCloudWebPart;
            WebPartUpdaterMapping[new Guid("e97ff0f2-57f9-7cad-bb0a-5bfe3ea30cd1")] = AveWebPartType.SocialCommentWebPart;
            WebPartUpdaterMapping[new Guid("3c5da7f7-4804-bd53-b38e-a411e20d6aeb")] = AveWebPartType.BusinessDataWebPart;
            WebPartUpdaterMapping[new Guid("a817b3e7-8db0-090a-2a28-23d054a36013")] = AveWebPartType.BusinessDataWebPart;
            WebPartUpdaterMapping[new Guid("4aaa156a-db8b-5d45-2b5f-4d941b70f309")] = AveWebPartType.BusinessDataWebPart;
            WebPartUpdaterMapping[new Guid("8bd7632b-46fb-13f4-d081-4095becac22b")] = AveWebPartType.XMLWebPart;
            WebPartUpdaterMapping[new Guid("aa995cba-0d36-1807-8224-9ad08ca39e36")] = AveWebPartType.CategoryResultsWebPart;
            WebPartUpdaterMapping[new Guid("d0e1a21b-f5e2-9c81-961c-14e8f484fac0")] = AveWebPartType.CategoryResultsWebPart;
            WebPartUpdaterMapping[new Guid("E6218CA5-B379-8D58-1EAD-99AED88F5246")] = AveWebPartType.ScriptEditorWebPart;
            WebPartUpdaterMapping[new Guid("cd493281-d4ab-f70c-45c0-3cb09338cdd1")] = AveWebPartType.MediaWebPart;
            WebPartUpdaterMapping[new Guid("5347bb5f-73da-0e02-d46b-b3916af34e00")] = AveWebPartType.RSSAggregatorWebPart;
            WebPartUpdaterMapping[new Guid("8b1b1472-8a82-3432-b088-1ffbff2789c9")] = AveWebPartType.BlogLinksWebPart;
            WebPartUpdaterMapping[new Guid("b2b082ad-524c-6eff-41ee-113843e3a649")] = AveWebPartType.SiteDocuments;
            WebPartUpdaterMapping[new Guid("f71460f3-a358-c374-7f02-fd32a7294728")] = AveWebPartType.TimeLineWebPart;
            WebPartUpdaterMapping[new Guid("90CC1C93-0192-0ECC-79F0-921FEFE4A115")] = AveWebPartType.ContactDetailWebPart;
            WebPartUpdaterMapping[new Guid("ba009853-eac3-16c8-9094-a8834485ad33")] = AveWebPartType.DataFormWebPart;
            WebPartUpdaterMapping[new Guid("883476E7-EA82-921A-B83D-0F1A07D9093C")] = AveWebPartType.BrowserFormWebPart;
            WebPartUpdaterMapping[new Guid("6C231A03-AA37-3E1C-BA04-6C5F94C63B93")] = AveWebPartType.MembersWebPart;
            //2013 16
            WebPartUpdaterMapping[new Guid("0f676169-0639-1e85-ade6-0fd81be9f2aa")] = AveWebPartType.TableOfContentsWebPart;
            WebPartUpdaterMapping[new Guid("9b7bf700-588d-6333-22ec-14b3bbfce104")] = AveWebPartType.ContentEditorWebPart;
            WebPartUpdaterMapping[new Guid("09d893f7-7913-7a29-b787-fbee3d5d3e2d")] = AveWebPartType.ExcelWebRendererWebPart;
            WebPartUpdaterMapping[new Guid("c20b33bb-6b4a-243b-cbc1-4e71ea88963f")] = AveWebPartType.VisioWebAccessWebPart;
            WebPartUpdaterMapping[new Guid("d0ef5974-f2fb-8482-65ee-b0afcb1d37e9")] = AveWebPartType.ContentByQueryWebPart;
            WebPartUpdaterMapping[new Guid("35aee725-be5b-7f5c-30f1-fb758cbc1310")] = AveWebPartType.XsltListViewWebPart;
            WebPartUpdaterMapping[new Guid("48841bfc-db88-9cf8-8566-567ba93d1197")] = AveWebPartType.ListViewWebPart;
            WebPartUpdaterMapping[new Guid("3417184e-0312-0308-7a02-a4b2ce7fa8aa")] = AveWebPartType.ListFormWebPart;
            WebPartUpdaterMapping[new Guid("1d4b9f5c-f1ce-da60-7c78-a8de74a12007")] = AveWebPartType.SummaryLinkWebPart;
            WebPartUpdaterMapping[new Guid("064c38b1-1d81-9a14-9f48-975550a969c3")] = AveWebPartType.TagCloudWebPart;
            WebPartUpdaterMapping[new Guid("79bd35e7-9add-ed7f-9098-08ce54fcf2a7")] = AveWebPartType.SocialCommentWebPart;
            WebPartUpdaterMapping[new Guid("082ad9d5-0f8a-c2a2-22be-58f525052677")] = AveWebPartType.MediaWebPart;
            WebPartUpdaterMapping[new Guid("2880fb1d-efa8-68dd-6bba-3cb0a605a7e3")] = AveWebPartType.BusinessDataWebPart;
            WebPartUpdaterMapping[new Guid("1ae90ce0-39c5-fbfe-ae5b-ceaee0860fdf")] = AveWebPartType.BusinessDataWebPart;
            WebPartUpdaterMapping[new Guid("0023cbe9-78e9-ffe2-acee-55ff45a36ee7")] = AveWebPartType.BusinessDataWebPart;
            WebPartUpdaterMapping[new Guid("ce57a450-50f1-305d-567a-e68ed13de758")] = AveWebPartType.XMLWebPart;
            WebPartUpdaterMapping[new Guid("0ff062a7-682c-13c1-23ea-78ba39385942")] = AveWebPartType.CategoryResultsWebPart;
            WebPartUpdaterMapping[new Guid("ce37770f-b871-2315-f32b-c1a7f40557a3")] = AveWebPartType.ScriptEditorWebPart;
            WebPartUpdaterMapping[new Guid("b2b70b48-b38f-af4d-3f43-9834a047b4f9")] = AveWebPartType.RSSAggregatorWebPart;
            WebPartUpdaterMapping[new Guid("ea9c1dd0-2027-3ab1-b0ee-48212f8d9179")] = AveWebPartType.BlogLinksWebPart;
            WebPartUpdaterMapping[new Guid("7d38305c-72db-5f93-84c0-aa52134537df")] = AveWebPartType.SiteDocuments;
            WebPartUpdaterMapping[new Guid("e674be25-25bb-b87a-5564-ba8515efba1c")] = AveWebPartType.TimeLineWebPart;
            WebPartUpdaterMapping[new Guid("ec0644c7-606c-feef-f929-7a0528fd6ddc")] = AveWebPartType.ContactDetailWebPart;
            WebPartUpdaterMapping[new Guid("F29349AD-FC31-E5C2-C7C2-29B99AB21D21")] = AveWebPartType.BrowserFormWebPart;
            WebPartUpdaterMapping[new Guid("7d32616f-5da2-37be-eea8-696a0912d0c5")] = AveWebPartType.CategoryWebPart;
            WebPartUpdaterMapping[new Guid("f30bdc98-bed5-2479-e293-4f7d98461f0b")] = AveWebPartType.ClientWebPart;
            WebPartUpdaterMapping[new Guid("780F7294-29FF-B743-602E-1EE3F814E0DB")] = AveWebPartType.TermPropertyWebPart;
            //SharePoint Online 
            WebPartUpdaterMapping[new Guid("c51fcd7d-57d0-4147-c79a-92cc184e24cb")] = AveWebPartType.DataFormWebPart;
            WebPartUpdaterMapping[new Guid("b2922567-b718-19c2-0c2b-5b45ba6f4fb6")] = AveWebPartType.BlogViewWebPat;

        }
        public static bool TryGetValue(Guid id ,out AveWebPartType value)
        {
            return WebPartUpdaterMapping.TryGetValue(id, out value);
        }
    }
    public enum AveWebPartType
    {
        DefaultWebpartType,
        ContentByQueryWebPart,
        ContentEditorWebPart,
        ExcelWebRendererWebPart,
        TableOfContentsWebPart,
        VisioWebAccessWebPart,
        ListViewWebPart,
        ListFormWebPart,
        XsltListViewWebPart,
        SummaryLinkWebPart,
        TagCloudWebPart,
        SocialCommentWebPart,
        BusinessDataWebPart,
        XMLWebPart,
        CategoryResultsWebPart,
        ScriptEditorWebPart,
        MediaWebPart,
        RSSAggregatorWebPart,
        BlogLinksWebPart,
        ThisWeekInPicturesWebPart,
        SiteDocuments,
        TimeLineWebPart,
        ContactDetailWebPart,
        DataFormWebPart,
        BrowserFormWebPart,
        CategoryWebPart,
        ClientWebPart,
        MembersWebPart,
        TermPropertyWebPart,
        BlogViewWebPat,
    }

}

    internal class CompressedXmlReader : XmlReader
    {
        // Fields
        private bool _needToPopScope;
        private const int ATTRIBUTE_NIL = -1;
        private ArrayList attributes = new ArrayList();
        private BinaryReader br;
        private int depth;
        private bool eof;
        private byte[] global;
        private int iAttribute = -1;
        private string localName;
        private WebPartNameTable nameTable;
        private string ns;
        private XmlNamespaceManager nsManager;
        private byte[] personal;
        private string text;
        private XmlNodeType type;
        private bool usePersonal;

        // Methods
        public CompressedXmlReader(XmlNamespaceManager nsManager, byte[] personal, byte[] global)
        {
            //ULS.ShipAssertTag(0x3839316d, ULSCat.msoulscat_WSS_WebParts, (personal != null) || (global != null));
            this.personal = personal;
            this.global = global;
            this.nameTable = WebPartNameTable.GlobalNameTable();
            this.nsManager = nsManager;
            this.SetBinaryReader(personal != null);
            //ULS.ShipAssertTag(0x3839316e, ULSCat.msoulscat_WSS_WebParts, this.br != null);
        }

        public override void Close()
        {
            this.br.Close();
        }

        public override string GetAttribute(int i)
        {
            throw new NotImplementedException();
        }

        public override string GetAttribute(string name)
        {
            throw new NotImplementedException();
        }

        public override string GetAttribute(string name, string ns)
        {
            foreach (WebPartXmlAttribute attribute in this.attributes)
            {
                if ((attribute.localName == name) && (attribute.ns == ns))
                {
                    return attribute.val;
                }
            }
            return null;
        }

        public override string LookupNamespace(string prefix)
        {
            return this.nsManager.LookupNamespace(this.nameTable.Get(prefix));
        }

        public override void MoveToAttribute(int i)
        {
            throw new NotImplementedException();
        }

        public override bool MoveToAttribute(string name)
        {
            throw new NotImplementedException();
        }

        public override bool MoveToAttribute(string name, string ns)
        {
            throw new NotImplementedException();
        }

        public override bool MoveToElement()
        {
            bool flag = false;
            if (this.iAttribute >= 0)
            {
                this.PopToElement();
                flag = true;
            }
            return flag;
        }

        public override bool MoveToFirstAttribute()
        {
            bool flag = false;
            this.iAttribute = -1;
            if ((this.type == XmlNodeType.Element) && (this.attributes.Count > 0))
            {
                this.depth++;
                this.type = XmlNodeType.Attribute;
                this.iAttribute = 0;
                flag = true;
            }
            return flag;
        }

        public override bool MoveToNextAttribute()
        {
            switch (this.type)
            {
                case XmlNodeType.Element:
                    return this.MoveToFirstAttribute();

                case XmlNodeType.Attribute:
                    break;

                case XmlNodeType.Text:
                    this.depth--;
                    this.type = XmlNodeType.Attribute;
                    break;

                default:
                    return false;
            }
            if ((this.iAttribute + 1) < this.attributes.Count)
            {
                this.iAttribute++;
                return true;
            }
            return false;
        }

        private XmlNodeType PeekNodeType()
        {
            //ULS.ShipAssertTag(0x3839316f, ULSCat.msoulscat_WSS_WebParts, !this.eof);
            return (XmlNodeType)this.br.PeekChar();
        }

        private void PopToElement()
        {
            switch (this.type)
            {
                case XmlNodeType.Attribute:
                    this.depth--;
                    break;

                case XmlNodeType.Text:
                    this.depth -= 2;
                    break;
            }
            this.type = XmlNodeType.Element;
        }

        public override bool Read()
        {
            if (this.eof)
            {
                return false;
            }
            if (this._needToPopScope)
            {
                this._needToPopScope = false;
                this.nsManager.PopScope();
            }
            else if (this.iAttribute >= 0)
            {
                this.PopToElement();
                this.iAttribute = -1;
                this.attributes.Clear();
            }
            XmlNodeType type = (XmlNodeType)this.br.ReadByte();
            switch (type)
            {
                case XmlNodeType.None:
                    break;

                case XmlNodeType.Element:
                    this.nsManager.PushScope();
                    this.localName = this.ReadPredefinedString();
                    this.ns = this.ReadPredefinedString();
                    if (this.ns.Length > 0)
                    {
                        this.nsManager.AddNamespace(string.Empty, this.ns);
                    }
                    this.text = null;
                    this.depth++;
                    this.ReadAttributes();
                    break;

                case XmlNodeType.Text:
                    this.text = this.ReadPredefinedString(false);
                    break;

                case XmlNodeType.CDATA:
                    this.text = this.ReadPredefinedString(false);
                    break;

                case XmlNodeType.EndElement:
                    this.depth--;
                    this._needToPopScope = true;
                    if (this.depth == 0)
                    {
                        this.br = null;
                        if (this.usePersonal && (this.global != null))
                        {
                            this.type = XmlNodeType.None;
                            this.SetBinaryReader(false);
                            this.MoveToContent();
                            this.Read();
                            type = this.type;
                        }
                        else
                        {
                            this.eof = true;
                        }
                    }
                    break;

                default:
                    //ULS.ShipAssertTag(0x3839317a, ULSCat.msoulscat_WSS_WebParts, false);
                    break;
            }
            this.type = type;
            return true;
        }
       
        private void ReadAttributes()
        {
            this.attributes.Clear();
            while (this.PeekNodeType() == XmlNodeType.Attribute)
            {
                this.br.ReadByte();
                WebPartXmlAttribute attribute = new WebPartXmlAttribute
                {
                    prefix = this.ReadPredefinedString(),
                    localName = this.ReadPredefinedString(),
                    ns = this.ReadPredefinedString()
                };
                this.text = null;
                while (this.Read() && (this.type != XmlNodeType.None))
                {
                }
                attribute.val = this.text;
                if (attribute.prefix == "xmlns")
                {
                    this.nsManager.AddNamespace(attribute.localName, attribute.val);
                }
                this.attributes.Add(attribute);
            }
            this.iAttribute = -1;
        }

        public override bool ReadAttributeValue()
        {
            bool flag = false;
            if (this.type == XmlNodeType.Attribute)
            {
                this.depth++;
                this.type = XmlNodeType.Text;
                flag = true;
            }
            return flag;
        }

        public override string ReadInnerXml()
        {
            throw new NotImplementedException();
        }

        public override string ReadOuterXml()
        {
            throw new NotImplementedException();
        }

        private string ReadPredefinedString()
        {
            return this.ReadPredefinedString(true);
        }

        private string ReadPredefinedString(bool addToNameTable)
        {
            string predefinedString = null;
            ushort us = this.br.ReadUInt16();
            if (us == 0xffff)
            {
                if (addToNameTable)
                {
                    return this.nameTable.Add(this.br.ReadString());
                }
                return this.br.ReadString();
            }
            predefinedString = this.nameTable.LookupPredefinedString(us);
            if (predefinedString == null)
            {
                switch (us)
                {
                    case 0x61:
                        return "http://schemas.microsoft.com/WebPart/v2/PivotView";

                    case 0x31:
                        return "CaptureMethod";
                }
            }
            return predefinedString;
        }

        public override string ReadString()
        {
            string str = "";
            while (this.type != XmlNodeType.EndElement)
            {
                if (this.type == XmlNodeType.Text)
                {
                    str = str + this.text;
                }
                if (!this.Read())
                {
                    return str;
                }
            }
            return str;
        }

        public override void ResolveEntity()
        {
            throw new NotImplementedException();
        }

        private void SetBinaryReader(bool usePersonal)
        {
            byte[] personal = this.personal;
            this.usePersonal = usePersonal;
            if (!usePersonal)
            {
                personal = this.global;
            }
            this.br = new BinaryReader(new MemoryStream(personal));
        }

        // Properties
        public override int AttributeCount
        {
            get
            {
                return this.attributes.Count;
            }
        }

        public override string BaseURI
        {
            get
            {
                return string.Empty;
            }
        }

        public override int Depth
        {
            get
            {
                return this.depth;
            }
        }

        public override bool EOF
        {
            get
            {
                return this.eof;
            }
        }

        public override bool HasValue
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool IsDefault
        {
            get
            {
                return false;
            }
        }

        public override bool IsEmptyElement
        {
            get
            {
                return false;
            }
        }

        public override string this[string name]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override string this[int i]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override string this[string name, string ns]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override string LocalName
        {
            get
            {
                switch (this.type)
                {
                    case XmlNodeType.Element:
                        return this.localName;

                    case XmlNodeType.Attribute:
                        return ((WebPartXmlAttribute)this.attributes[this.iAttribute]).localName;
                }
                return null;
            }
        }

        public override string Name
        {
            get
            {
                if (this.Prefix.Length == 0)
                {
                    return this.LocalName;
                }
                return (this.Prefix + ":" + this.LocalName);
            }
        }

        public override string NamespaceURI
        {
            get
            {
                string ns = string.Empty;
                switch (this.type)
                {
                    case XmlNodeType.Element:
                        ns = this.ns;
                        break;

                    case XmlNodeType.Attribute:
                        ns = ((WebPartXmlAttribute)this.attributes[this.iAttribute]).ns;
                        break;
                }
                if (ns.Length != 0)
                {
                    return ns;
                }
                if (this.Prefix.Length > 0)
                {
                    return this.LookupNamespace(this.Prefix);
                }
                return this.nsManager.DefaultNamespace;
            }
        }

        public override XmlNameTable NameTable
        {
            get
            {
                return this.nameTable;
            }
        }

        public override XmlNodeType NodeType
        {
            get
            {
                return this.type;
            }
        }

        public override string Prefix
        {
            get
            {
                if (this.type == XmlNodeType.Attribute)
                {
                    return ((WebPartXmlAttribute)this.attributes[this.iAttribute]).prefix;
                }
                return string.Empty;
            }
        }

        public override char QuoteChar
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override ReadState ReadState
        {
            get
            {
                if (this.eof)
                {
                    return ReadState.EndOfFile;
                }
                return ReadState.Interactive;
            }
        }

        public override string Value
        {
            get
            {
                switch (this.type)
                {
                    case XmlNodeType.Element:
                        return this.text;

                    case XmlNodeType.Attribute:
                        return ((WebPartXmlAttribute)this.attributes[this.iAttribute]).val;

                    case XmlNodeType.Text:
                        if (this.iAttribute < 0)
                        {
                            return this.text;
                        }
                        return ((WebPartXmlAttribute)this.attributes[this.iAttribute]).val;

                    case XmlNodeType.CDATA:
                        if (this.text.StartsWith("<![CDATA[", StringComparison.OrdinalIgnoreCase) && this.text.EndsWith("]]>", StringComparison.OrdinalIgnoreCase))
                        {
                            this.text = this.text.Substring(9, this.text.Length - 12);
                        }
                        return this.text;
                }
                return null;
            }
        }

        public override string XmlLang
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override XmlSpace XmlSpace
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        // Nested Types
        private class WebPartXmlAttribute
        {
            // Fields
            public string localName;
            public string ns;
            public string prefix;
            public string val;
        }
    }

    internal class WebPartNameTable : XmlNameTable
    {
        // Fields
        private static WebPartNameTable _nameTable;
        private static Hashtable _table;
        private static string[] predefinedStrings;

        // Methods
        static WebPartNameTable()
        {
            //FieldInfo[] fields = typeof(XmlSchema).GetFields();
            //ULS.ShipAssertTag(0x39676839, ULSCat.msoulscat_WSS_WebParts, fields.Length < 0xffff);
            predefinedStrings = new string[PredefinedStrings.PREDEFINEDSTRINGS.Length];
            _nameTable = new WebPartNameTable();
            _table = new Hashtable();
            for (ushort i = 0; i < PredefinedStrings.PREDEFINEDSTRINGS.Length; i = (ushort)(i + 1))
            {
                //FieldInfo info = fields[i];
                string s = PredefinedStrings.PREDEFINEDSTRINGS[i];
                if (string.Empty == s)
                {
                    s = string.Empty;
                }
                AddPredefinedString(i, s);
            }
        }

        public override string Add(string array)
        {
            string str = this.Get(array);
            if (str == null)
            {
                lock (_table)
                {
                    str = this.Get(array);
                    if (str == null)
                    {
                        _table[array] = new StringEntry(array);
                        str = array;
                    }
                }
            }
            return str;
        }

        public override string Add(char[] array, int offset, int length)
        {
            return this.Add(new string(array, offset, length));
        }

        private static void AddPredefinedString(ushort us, string s)
        {
            if (s != null)
            {
                predefinedStrings[us] = s;
                _table[s] = new StringEntry(s, us);
            }
        }

        public override string Get(string array)
        {
            StringEntry entry = (StringEntry)_table[array];
            if (entry != null)
            {
                return entry._s;
            }
            return null;
        }

        public override string Get(char[] array, int offset, int length)
        {
            return this.Get(new string(array, offset, length));
        }

        public static WebPartNameTable GlobalNameTable()
        {
            return _nameTable;
        }

        public string LookupPredefinedString(ushort us)
        {
            return predefinedStrings[us];
        }

        public static ushort LookupPredefinedStringConstant(string s)
        {
            ushort num = 0xffff;
            StringEntry entry = (StringEntry)_table[s];
            if (entry != null)
            {
                num = entry._predefinedConstant;
            }
            return num;
        }

        // Nested Types
        public class StringEntry
        {
            // Fields
            public readonly ushort _predefinedConstant;
            public readonly string _s;

            // Methods
            public StringEntry(string s)
            {
                this._s = s;
                this._predefinedConstant = 0xffff;
            }

            public StringEntry(string s, ushort predefinedConstant)
                : this(s)
            {
                this._predefinedConstant = predefinedConstant;
            }
        }
    }

internal class PredefinedStrings
{
    //it is thread safe, no write operations
    public static readonly string[] PREDEFINEDSTRINGS = new string[150];

    [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint Property")]
    static PredefinedStrings()
    {
        PREDEFINEDSTRINGS[0] = "http://schemas.microsoft.com/WebPart/v2";
        PREDEFINEDSTRINGS[1] = "Dir";
        PREDEFINEDSTRINGS[2] = "Description";
        PREDEFINEDSTRINGS[3] = "Encoding";
        PREDEFINEDSTRINGS[4] = "Title";
        PREDEFINEDSTRINGS[5] = "WebPart";
        PREDEFINEDSTRINGS[6] = "IsIncluded";
        PREDEFINEDSTRINGS[7] = "Zone";
        PREDEFINEDSTRINGS[8] = "ZoneID";
        PREDEFINEDSTRINGS[9] = "PartOrder";
        PREDEFINEDSTRINGS[10] = "NumberLimit";
        PREDEFINEDSTRINGS[11] = "FrameState";
        PREDEFINEDSTRINGS[12] = "Height";
        PREDEFINEDSTRINGS[13] = "Width";
        PREDEFINEDSTRINGS[14] = "Toolbar";
        PREDEFINEDSTRINGS[15] = "ContentLink";
        PREDEFINEDSTRINGS[16] = "DisplayName";
        PREDEFINEDSTRINGS[17] = "DataFields";
        PREDEFINEDSTRINGS[18] = "DataQuery";
        PREDEFINEDSTRINGS[19] = "XSLLink";
        PREDEFINEDSTRINGS[20] = "XSL";
        PREDEFINEDSTRINGS[21] = "AllowRemove";
        PREDEFINEDSTRINGS[22] = "AllowMinimize";
        PREDEFINEDSTRINGS[23] = "IsVisible";
        PREDEFINEDSTRINGS[24] = "Namespace";
        PREDEFINEDSTRINGS[25] = "ViewFlag";
        PREDEFINEDSTRINGS[26] = "DetailLink";
        PREDEFINEDSTRINGS[27] = "HelpLink";
        PREDEFINEDSTRINGS[28] = "PartStorage";
        PREDEFINEDSTRINGS[29] = "";
        PREDEFINEDSTRINGS[30] = "";
        PREDEFINEDSTRINGS[31] = "PartImageSmall";
        PREDEFINEDSTRINGS[32] = "PartImageLarge";
        PREDEFINEDSTRINGS[33] = "Assembly";
        PREDEFINEDSTRINGS[34] = "TypeName";
        PREDEFINEDSTRINGS[35] = "";
        PREDEFINEDSTRINGS[36] = "";
        PREDEFINEDSTRINGS[37] = "FrameType";
        PREDEFINEDSTRINGS[38] = "Connections";
        PREDEFINEDSTRINGS[39] = "MissingAssembly";
        PREDEFINEDSTRINGS[40] = "Name";
        PREDEFINEDSTRINGS[41] = "";
        PREDEFINEDSTRINGS[42] = "xmlns";
        PREDEFINEDSTRINGS[43] = "AllowZoneChange";
        PREDEFINEDSTRINGS[44] = "ParamBindings";
        PREDEFINEDSTRINGS[45] = "FireInitialRow";
        PREDEFINEDSTRINGS[46] = "";
        PREDEFINEDSTRINGS[47] = "ImageLink";
        PREDEFINEDSTRINGS[48] = "";
        PREDEFINEDSTRINGS[49] = "";
        PREDEFINEDSTRINGS[50] = "PostData";
        PREDEFINEDSTRINGS[51] = "Tags";
        PREDEFINEDSTRINGS[52] = "TagIndexes";
        PREDEFINEDSTRINGS[53] = "RenderTags";
        PREDEFINEDSTRINGS[54] = "RenderTagIndexes";
        PREDEFINEDSTRINGS[55] = "LastUpdated";
        PREDEFINEDSTRINGS[56] = "RefreshInterval";
        PREDEFINEDSTRINGS[57] = "LastCached";
        PREDEFINEDSTRINGS[58] = "";
        PREDEFINEDSTRINGS[59] = "Content";
        PREDEFINEDSTRINGS[60] = "ConnectionID";
        PREDEFINEDSTRINGS[61] = "http://www.w3.org/2001/XMLSchema";
        PREDEFINEDSTRINGS[62] = "http://www.w3.org/2001/XMLSchema-instance";
        PREDEFINEDSTRINGS[63] = "Normal";
        PREDEFINEDSTRINGS[64] = "Minimized";
        PREDEFINEDSTRINGS[65] = "Default";
        PREDEFINEDSTRINGS[66] = "LeftToRight";
        PREDEFINEDSTRINGS[67] = "RightToLeft";
        PREDEFINEDSTRINGS[68] = "None";
        PREDEFINEDSTRINGS[69] = "Standard";
        PREDEFINEDSTRINGS[70] = "TitleBarOnly";
        PREDEFINEDSTRINGS[71] = "true";
        PREDEFINEDSTRINGS[72] = "false";
        PREDEFINEDSTRINGS[73] = "xsi";
        PREDEFINEDSTRINGS[74] = "xsd";
        PREDEFINEDSTRINGS[75] = "NoDefaultStyle";
        PREDEFINEDSTRINGS[76] = "VerticalAlignment";
        PREDEFINEDSTRINGS[77] = "HorizontalAlignment";
        PREDEFINEDSTRINGS[78] = "BackgroundColor";
        PREDEFINEDSTRINGS[79] = "IsIncludedFilter";
        PREDEFINEDSTRINGS[80] = "XML";
        PREDEFINEDSTRINGS[81] = "XMLLink";
        PREDEFINEDSTRINGS[82] = "HeaderCaption";
        PREDEFINEDSTRINGS[83] = "HeaderTitle";
        PREDEFINEDSTRINGS[84] = "HeaderDescription";
        PREDEFINEDSTRINGS[85] = "Image";
        PREDEFINEDSTRINGS[86] = "ContentHasToken";
        PREDEFINEDSTRINGS[87] = "ExportControlledProperties";
        PREDEFINEDSTRINGS[88] = "SourceType";
        PREDEFINEDSTRINGS[89] = "Fields";
        PREDEFINEDSTRINGS[90] = "http://schemas.microsoft.com/WebPart/v2/ContentEditor";
        PREDEFINEDSTRINGS[91] = "http://schemas.microsoft.com/WebPart/v2/PageViewer";
        PREDEFINEDSTRINGS[92] = "http://schemas.microsoft.com/WebPart/v2/Image";
        PREDEFINEDSTRINGS[93] = "http://schemas.microsoft.com/WebPart/v2/Xml";
        PREDEFINEDSTRINGS[94] = "http://schemas.microsoft.com/WebPart/v2/DataView";
        PREDEFINEDSTRINGS[95] = "http://schemas.microsoft.com/WebPart/v2/ListForm";
        PREDEFINEDSTRINGS[96] = "http://schemas.microsoft.com/WebPart/v2/ListView";
        PREDEFINEDSTRINGS[97] = "";
        PREDEFINEDSTRINGS[98] = "http://schemas.microsoft.com/WebPart/v2/TitleBar";
        PREDEFINEDSTRINGS[99] = "http://schemas.microsoft.com/WebPart/v2/SimpleForm";
        PREDEFINEDSTRINGS[100] = "http://schemas.microsoft.com/WebPart/v2/Members";
        PREDEFINEDSTRINGS[101] = "CacheDataStorage";
        PREDEFINEDSTRINGS[102] = "CacheDataTimeout";
        PREDEFINEDSTRINGS[103] = "CacheXslStorage";
        PREDEFINEDSTRINGS[104] = "AlternativeText";
        PREDEFINEDSTRINGS[105] = "DataSourceBindings";
        PREDEFINEDSTRINGS[106] = "Template";
        PREDEFINEDSTRINGS[107] = "http://schemas.microsoft.com/WebPart/v3";
        PREDEFINEDSTRINGS[108] = "ID";
        PREDEFINEDSTRINGS[109] = "AttachedPropertiesShared";
        PREDEFINEDSTRINGS[110] = "AttachedPropertiesUser";
        PREDEFINEDSTRINGS[111] = "AllowConnect";
        PREDEFINEDSTRINGS[112] = "AllowEdit";
        PREDEFINEDSTRINGS[113] = "AllowHide";
        PREDEFINEDSTRINGS[114] = "HelpMode";
        PREDEFINEDSTRINGS[115] = "http://schemas.microsoft.com/WebPart/v2/UserTasks";
        PREDEFINEDSTRINGS[116] = "http://schemas.microsoft.com/WebPart/v2/UserDocs";
        PREDEFINEDSTRINGS[117] = "http://schemas.microsoft.com/WebPart/v2/Aggregation";
        PREDEFINEDSTRINGS[118] = "QuerySiteCollection";
        PREDEFINEDSTRINGS[119] = "MaxItemsShown";
        PREDEFINEDSTRINGS[120] = "QueryLastModifiedBy";
        PREDEFINEDSTRINGS[121] = "QueryCreatedBy";
        PREDEFINEDSTRINGS[122] = "QueryCheckedOutBy";
        PREDEFINEDSTRINGS[123] = "DisplayFolderColumn";
        PREDEFINEDSTRINGS[124] = "DisplayItemLinkColumn";
        PREDEFINEDSTRINGS[125] = "TitleUrl";
        PREDEFINEDSTRINGS[126] = "DisplayType";
        PREDEFINEDSTRINGS[127] = "MembershipGroupId";
        PREDEFINEDSTRINGS[128] = "AllowClose";
        PREDEFINEDSTRINGS[129] = "AuthorizationFilter";
        PREDEFINEDSTRINGS[130] = "CatalogIconImageUrl";
        PREDEFINEDSTRINGS[131] = "ChromeState";
        PREDEFINEDSTRINGS[132] = "ChromeType";
        PREDEFINEDSTRINGS[133] = "Direction";
        PREDEFINEDSTRINGS[134] = "ExportMode";
        PREDEFINEDSTRINGS[135] = "HelpUrl";
        PREDEFINEDSTRINGS[136] = "Hidden";
        PREDEFINEDSTRINGS[137] = "ImportErrorMessage";
        PREDEFINEDSTRINGS[138] = "IsClosed";
        PREDEFINEDSTRINGS[139] = "TitleIconImageUrl";
        PREDEFINEDSTRINGS[140] = "ZoneIndex";
        PREDEFINEDSTRINGS[141] = "PersonalizableProperties";
        PREDEFINEDSTRINGS[142] = "NonPersonalizableProperties";
        PREDEFINEDSTRINGS[143] = "IPersonalizableProperties";
        PREDEFINEDSTRINGS[144] = "AttachedProperties";
        PREDEFINEDSTRINGS[145] = "LinkMap";
        PREDEFINEDSTRINGS[146] = "Unknown";
        PREDEFINEDSTRINGS[147] = "ViewContentTypeId";
        PREDEFINEDSTRINGS[148] = "CssStyleSheet";
        PREDEFINEDSTRINGS[149] = "ListName";
    }
}

