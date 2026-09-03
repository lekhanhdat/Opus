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
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TestModel
{
    class CreateMetadata
    {
        public static string path = @"D:\Storage\Data";

        public void WriterAllMetadata(int userCount,int groupCount, int group, int termset, int term, int label)
        {
            DateTime start = DateTime.Now;
            string name = Path.Combine(path, "AllMetadata2.xml");
            using (FileStream fs = File.OpenWrite(name))
            using (XmlWriter writer = XmlWriter.Create(fs))
            {
                BeginWriteMetadata(writer);
                var groups = CreateGroups(userCount, groupCount);
                WriteMetadata(AveMetadataType.Groups, groups, writer);
                var user = CreateUsers(userCount);
                WriteMetadata(AveMetadataType.Users, user, writer);
               
                var store = GenerateTermStoreInfo(group, termset, term, label);
                WriteMetadata(AveMetadataType.MetadataService, store, writer);
                EndWriteMetadata(writer);
                writer.Flush();
            }
            Console.WriteLine("Time cost:"+(DateTime.Now-start));
        }

        public void WriteGroupData(int groupCount,int userCount)
        {
            string name =Path.Combine(path,groupCount.ToString() + "_" + userCount.ToString() + "_AveGroups.xml");
            using (FileStream fs = File.OpenWrite(name))
            using (XmlWriter writer=XmlWriter.Create(fs))
            {
                BeginWriteMetadata(writer);
                var groups = CreateGroups(userCount, groupCount);
                WriteMetadata(AveMetadataType.Groups, groups, writer);
                EndWriteMetadata(writer);
                writer.Flush();
            }
        }

        public void WriteUserData( int userCount)
        {
            string name = Path.Combine(path, userCount.ToString() + "_AveUser.xml");
            using (FileStream fs = File.OpenWrite(name))
            using (XmlWriter writer = XmlWriter.Create(fs))
            {
                BeginWriteMetadata(writer);
                var users = CreateUsers(userCount);
                WriteMetadata(AveMetadataType.Users, users, writer);
                EndWriteMetadata(writer);
                writer.Flush();
            }
        }

       //totally count termCount*groupCount*termsetCount
        public void WriteTermStore(int group,int termset,int term,int label)
        {
            var store = GenerateTermStoreInfo(group, termset, term, label);

            string name = Path.Combine(path, "MetadataService" + "_" + group.ToString() + "_" + termset.ToString() + "_" + term.ToString() + "_" + label.ToString() + "_AveGroups.xml");
            using (FileStream fs = File.OpenWrite(name))
            using (XmlWriter writer = XmlWriter.Create(fs))
            {
                BeginWriteMetadata(writer);
                WriteMetadata(AveMetadataType.Groups, store, writer);
                EndWriteMetadata(writer);
                writer.Flush();
            }


        }

        private static List<AveTermStoreInfo> GenerateTermStoreInfo(int group, int termset, int term, int label)
        {
            var ace = new AveAceInfo { DisplayName = "Demo user", PrincipalName = "PrincipalName", DenyRightsMask = 111111, GrantRightsMask = 111111 };
            var TermStoreAdministrators = new List<AveAceInfo> { };
            for (int k = 0; k < 200; k++)
            {
                TermStoreAdministrators.Add(ace);
            }
            List<AveTermStoreInfo> info = new List<AveTermStoreInfo>();
            var store = new AveTermStoreInfo { WorkingLanguage = 1033, TermStoreAdministrators = TermStoreAdministrators, DefaultLanguage = 1033, Groups = new List<AveMetadataGroupInfo> { }, Id = Guid.NewGuid(), LastAccessTime = DateTime.UtcNow, Name = "termstore" + Guid.NewGuid().ToString(), UniqueId = Guid.NewGuid() };

            var groupInfo = GenerateTermGroup(TermStoreAdministrators, termset, term, label);
            for (int k = 0; k < group; k++)
            {
                store.Groups.Add(groupInfo);
            }

            return new List<AveTermStoreInfo> { store};
        }

        private static AveLableInfo GenerateLabel()
        {
            var labelInfo = new AveLableInfo
            {
                Language = 1033,
                Description = "This is label description",
                IsDefaultForLanguage = false,
                Value = "Label descroptionwadaiufSAD"
            };
            return labelInfo;
        }

        private static AveMetadataGroupInfo GenerateTermGroup(List<AveAceInfo> TermStoreAdministrators,int termset,int term,int label)
        {
            var termGroup = new AveMetadataGroupInfo();
            termGroup.GroupManagers = TermStoreAdministrators;
            termGroup.Description = "Description";
            termGroup.Contributors = TermStoreAdministrators;
            termGroup.Id = Guid.NewGuid();
            termGroup.IsSiteCollectionGroup = true;
            termGroup.IsSystemGroup = true;
            termGroup.Sites = new List<Guid> { };
            termGroup.TermSets = new List<AveTermSetInfo>();
            var set = GenerateTermSet(term,label);
            for (int k = 0; k < termset; k++)
            {
                termGroup.TermSets.Add(set);
            }
            return termGroup;
        }

        private static AveTermSetInfo GenerateTermSet(int termCount,int labelCount)
        {
            var temsetInfo = new AveTermSetInfo
            {
                IsAvailableForTagging = true,
                Contact = "",
                CustomProperties = new Dictionary<string, string> { },
                CustomSortOrder = "",
                Description = "",
                Id = Guid.NewGuid(),
                IsOpenForTermCreation = false,
                Name = Guid.NewGuid().ToString(),
                OperationType = AveTermChangeItem.ChangedOperationType.Copy,
                Owner = "",
                ParentId = Guid.Empty,
                Stakeholders = new List<string> { },
                Terms = new List<AveTermInfo>(),
                Type = 0
            };
            var term = GenerateTermInfo(labelCount);
            for (int k = 0; k < termCount; k++)
            {
                temsetInfo.Terms.Add(term);
            }
            return temsetInfo;
        }

        private static AveTermInfo GenerateTermInfo(int labelCount)
        {
            var termInfo = new AveTermInfo
            {
                CustomProperties = StringDicDemo,
                Id = Guid.NewGuid(),
                CustomSortOrder = "DESC",
                Description = "This is term desctiption",
                IsAvailableForTagging = true,
                IsDeprecated = false,
                IsKeyword = false,
                IsPinned = false,
                IsReused = false,
                IsRoot = false,
                IsSourceTerm = false,
                Labels = new List<AveLableInfo> { },
                LocalCustomProperties = StringDicDemo,
                MergedTermIds = new List<Guid> { },
                Name = "TermName" + Guid.NewGuid(),
                OperationType = AveTermChangeItem.ChangedOperationType.Copy,
                Owner = "1033",
                ParentTermId = Guid.NewGuid(),
                ParentTermSetId = Guid.NewGuid(),
                PinSourceTermSetId = Guid.NewGuid(),
                SourceTermId = new Guid(),
                SourceTermName = "SourceTermName",
                TermName = "TermName" + Guid.NewGuid(),
                Terms = new List<AveTermInfo> { },
            };
            var labelInfo = GenerateLabel();
            for (int k = 0; k < labelCount; k++)
            {
                termInfo.Labels.Add(labelInfo);
            }
            return termInfo;
        }

        private static Dictionary<string, string> StringDicDemo= new Dictionary<string, string> { { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() }, { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() }, { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() }, { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() }, { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() } };

        private List<AveUserInfo> CreateUsers(int count)
        {
            List<AveUserInfo> users = new List<AveUserInfo>();
            for (int m = 1; m < count + 1; m++)
            {
                AveUserInfo userInfo = new AveUserInfo();
                userInfo.Email = m.ToString() + "_demo@m365x113665.onmicrosoft.com";
                userInfo.ID = m;
                userInfo.Login =m.ToString() + "_demo@m365x113665.onmicrosoft.com";
                userInfo.Title =  m.ToString() + "_Title";
                userInfo.DomainGroup = false;
                users.Add(userInfo);
            }
            return users;
        }

        private void BeginWriteMetadata(XmlWriter writer)
        {
            //<Data version="1.0">
            writer.WriteStartElement("Data");
            writer.WriteAttributeString("version", "1.0");

        }

        private void EndWriteMetadata(XmlWriter writer)
        {
            //<Data version="1.0">
            writer.WriteEndElement();
            
        }

        private List<AveGroupInfo> CreateGroups(int userCountInEachGroup,int GroupCount)
        {
            List<AveGroupInfo> info = new List<AveGroupInfo>();
            for (int k = 1; k < GroupCount+1; k++)
            {

                AveGroupInfo groupInfo = new AveGroupInfo();
                groupInfo.Description = "Group Description" + k;
                groupInfo.ID = k;
                groupInfo.Title = "Group Title" + k;
                //添加诊断log,判断空引用的地方SAAS-28834

                groupInfo.Owner = k;
                groupInfo.OwnerIsUser = true;
                //SAAS-8191 增加Group Settings中的四个属性
                groupInfo.AllowMembersEditMembership = true;
                groupInfo.AllowRequestToJoinLeave = true;
                groupInfo.AutoAcceptRequestToJoinLeave = true;
                groupInfo.OnlyAllowMembersViewMembership = true;
                for (int m = 1; m < userCountInEachGroup+1; m++)
                {
                    AveUserInfo userInfo = new AveUserInfo();
                    userInfo.Email = k.ToString() + "_" + m.ToString() + "_demo@m365x113665.onmicrosoft.com";
                    userInfo.ID = m;
                    userInfo.Login = k.ToString() + "_" + m.ToString() + "_demo@m365x113665.onmicrosoft.com";
                    userInfo.Title = k.ToString() + "_" + m.ToString() + "_Title";
                    userInfo.DomainGroup = false;
                    groupInfo.Members.Add(userInfo);
                    groupInfo.Memberships.Add(userInfo.ID);
                }

                groupInfo.OwnerInfo = groupInfo.Members[0];
                info.Add(groupInfo);
            }
            return info;
        }

        //AveMetadataType.Group  List<AveGroupInfo> 
        //AveMetadataType.Users List<AveUserInfo>
        //AveMetadataType.MetadataService  List<AveTermStoreInfo>
        private void WriteMetadata<T>(AveMetadataType type,T data,XmlWriter writer)
        {
            AveXmlSerializer.Serialize(writer, type.ToString(), data);

        }
    }
}
