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
using System.Text;
using System.Globalization;

namespace AvePoint.Wrapper.Common
{
    public sealed class AveBuiltInContentTypeId
    {
        // Fields
        public const string System = "0x";
        public const string Item = "0x01";
        public const string Document = "0x0101";
        public const string XMLDocument = "0x010101";
        public const string ODCDocument = "0x010100629D00608F814DD6AC8A86903AEE72AA";
        public const string UDCDocument = "0x010100B4CBD48E029A4AD8B62CB0E41868F2B0";
        public const string Picture = "0x010102";
        public const string UntypedDocument = "0x010104";
        public const string MasterPage = "0x010105";
        public const string WikiDocument = "0x010108";
        public const string DocumentWorkflowItem = "0x010107";
        public const string Event = "0x0102";
        public const string Issue = "0x0103";
        public const string Announcement = "0x0104";
        public const string Link = "0x0105";
        public const string Contact = "0x0106";
        public const string Message = "0x0107";
        public const string Discussion = "0x012002";
        public const string Task = "0x0108";
        public const string WorkflowTask = "0x010801";
        public const string AdminTask = "0x010802";
        public const string WorkflowHistory = "0x0109";
        public const string BlogPost = "0x0110";
        public const string BlogComment = "0x0111";
        public const string Folder = "0x0120";
        public const string RootOfList = "0x012001";
        public const string Person = "0x010A";
        public const string SharePointGroup = "0x010B";
        public const string DomainGroup = "0x010C";
        public const string BasicPage = "0x010109";
        public const string WebPartPage = "0x01010901";
        public const string LinkToDocument = "0x01010A";
        public const string FarEastContact = "0x0116";
        public const string DublinCoreName = "0x01010B";
        public const string HealthRuleDefinition = "0x01003A8AA7A4F53046158C5ABD98036A01D5";
        public const string HealthReport = "0x0100F95DB3A97E8046b58C6A54FB31F2BD46";
        public const string SummaryTask = "0x012004";
        public const string DocumentSet = "0x0120D5";
        public const string Schedule = "0x0102007DBDC1392EAF4EBBBF99E41D8922B264";
        public const string ResourceReservation = "0x0102004F51EFDEA49C49668EF9C6744C8CF87D";
        public const string ScheduleAndResourceReservation = "0x01020072BB2A38F0DB49C3A96CF4FA85529956";
        public const string GbwCirculationCTName = "0x01000F389E14C9CE4CE486270B9D4713A5D6";
        public const string GbwOfficialNoticeCTName = "0x01007CE30DD1206047728BAFD1C39A850120";
        public const string CallTracking = "0x0100807FBAC5EB8A4653B8D24775195B5463";
        public const string Resource = "0x01004C9F4486FBF54864A7B0A33D02AD19B1";
        public const string ResourceGroup = "0x0100CA13F2F8D61541B180952DFB25E3E8E4";
        public const string Holiday = "0x01009BE2AB5291BF4C1A986910BD278E4F18";
        public const string Timecard = "0x0100C30DDA8EDB2E434EA22D793D9EE42058";
        public const string WhatsNew = "0x0100A2CA87FF01B442AD93F37CD7DD0943EB";
        public const string Whereabouts = "0x0100FBEEE6F0C500489B99CDA6BB16C398F7";
        public const string IMEDictionaryItem = "0x010018F21907ED4E401CB4F14422ABC65304";
        public const string XSLStyle = "0x010100734778F2B7DF462491FC91844AE431CF";
        //Oliver:只在静态构造方法中初始化
        private static Dictionary<string, bool> s_dict;
        private static object s_lock = new object();
        // Methods
        static AveBuiltInContentTypeId()
        {
            s_dict = new Dictionary<string, bool>(0x34, StringComparer.OrdinalIgnoreCase);

            s_dict[DocumentWorkflowItem] = true;
            s_dict[Schedule] = true;
            s_dict[WikiDocument] = true;
            s_dict[Discussion] = true;
            s_dict[SummaryTask] = true;
            s_dict[Message] = true;
            s_dict[DocumentSet] = true;
            s_dict[Link] = true;
            s_dict[WebPartPage] = true;
            s_dict[Document] = true;
            s_dict[BlogComment] = true;
            s_dict[AdminTask] = true;
            s_dict[MasterPage] = true;
            s_dict[RootOfList] = true;
            s_dict[Issue] = true;
            s_dict[WorkflowHistory] = true;
            s_dict[GbwOfficialNoticeCTName] = true;
            s_dict[Item] = true;
            s_dict[HealthRuleDefinition] = true;
            s_dict[Timecard] = true;
            s_dict[ODCDocument] = true;
            s_dict[IMEDictionaryItem] = true;
            s_dict[CallTracking] = true;
            s_dict[DomainGroup] = true;
            s_dict[Person] = true;
            s_dict[HealthReport] = true;
            s_dict[Contact] = true;
            s_dict[FarEastContact] = true;
            s_dict[Resource] = true;
            s_dict[Announcement] = true;
            s_dict[UntypedDocument] = true;
            s_dict[Event] = true;
            s_dict[Folder] = true;
            s_dict[BasicPage] = true;
            s_dict[Holiday] = true;
            s_dict[BlogPost] = true;
            s_dict[UDCDocument] = true;
            s_dict[System] = true;
            s_dict[Task] = true;
            s_dict[XMLDocument] = true;
            s_dict[WorkflowTask] = true;
            s_dict[ResourceGroup] = true;
            s_dict[Picture] = true;
            s_dict[ScheduleAndResourceReservation] = true;
            s_dict[Whereabouts] = true;
            s_dict[DublinCoreName] = true;
            s_dict[XSLStyle] = true;
            s_dict[GbwCirculationCTName] = true;
            s_dict[LinkToDocument] = true;
            s_dict[SharePointGroup] = true;
            s_dict[WhatsNew] = true;
            s_dict[ResourceReservation] = true;
        }

        public static bool Contains(IAveContentTypeId contentTypeId)
        {
            return Contains(contentTypeId.ToString()); 
        }

        public static bool Contains(string contentTypeId)
        {
            bool flag = false;
            s_dict.TryGetValue(contentTypeId.ToUpper(CultureInfo.InvariantCulture), out flag);
            return flag;
        }

        public static bool Contains(byte[] contentTypeId)
        {
            string id = ConvertBytesToHex(contentTypeId);
            return Contains(id);
        }

        public static string ConvertBytesToHex(byte[] bts)
        {
            StringBuilder sb = new StringBuilder("0x");
            foreach (byte b in bts)
            {
                sb.AppendFormat("{0:x2}", b);
            }
            return sb.ToString();
        }
    }
    
    public sealed class AveSystemContentTypeId
    {            
        private static IAveContentTypeId sharePointListbasedStatusIndicator;
        private static IAveContentTypeId documentSet;
        private static object splsiLock = new object();
        private static object dsLock = new object();

        public static IAveContentTypeId SharePointListbasedStatusIndicator
        {
            get
            {
                if (sharePointListbasedStatusIndicator == null)
                {
                    lock (splsiLock)
                    {
                        if (sharePointListbasedStatusIndicator == null)
                        {
                            sharePointListbasedStatusIndicator = WrapperRuntime.CurrentContext.ModelFactory.CreateContentTypeId("0x00A7470EADF4194E2E9ED1031B61DA088402");
                        }
                    }
                }
                return sharePointListbasedStatusIndicator;
            }
        }

        public static IAveContentTypeId DocumentSet
        {
            get
            {
                if (documentSet == null)
                {
                    lock (dsLock)
                    {
                        if (documentSet == null)
                        {
                            documentSet = WrapperRuntime.CurrentContext.ModelFactory.CreateContentTypeId("0x0120D520");
                        }
                    }
                }
                return documentSet;
            }
        }
    }
}
