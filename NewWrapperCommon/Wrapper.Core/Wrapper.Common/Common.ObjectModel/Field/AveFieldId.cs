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

namespace AvePoint.Wrapper.Common
{
    public class AveFieldId : IAveFieldId
    {
        public static Guid AnonymousCacheProfile
        {
            get
            {
                return new Guid("BD51BBE5-9A06-4195-B385-E04FE47A33C8");
            }
        }

        public static Guid ArticleDate
        {
            get
            {
                return new Guid("71316CEA-40A0-49f3-8659-F0CEFDBDBD4F");
            }
        }

        public static Guid AssociatedContentType
        {
            get
            {
                return new Guid("b510aac1-bba3-4652-ab70-2d756c29540f");
            }
        }

        public static Guid AssociatedVariations
        {
            get
            {
                return new Guid("d211d750-4fe6-4d92-90e8-eb16dff196c8");
            }
        }

        public static Guid AudienceTargeting
        {
            get
            {
                return new Guid("61cbb965-1e04-4273-b658-eedaa662f48d");
            }
        }

        public static Guid AuthenticatedCacheProfile
        {
            get
            {
                return new Guid("9A36D6C6-F7D4-4cce-8923-AD99A44E2F5B");
            }
        }

        public static Guid AutomaticUpdate
        {
            get
            {
                return new Guid("e977ed93-da24-4fcc-b77d-ac34eea7288f");
            }
        }

        public static Guid AverageRatings
        {
            get
            {
                return new Guid("5a14d1ab-1513-48c7-97b3-657a5ba6c742");
            }
        }

        public static Guid ByLine
        {
            get
            {
                return new Guid("D3429CC9-ADC4-439b-84A8-5679070F84CB");
            }
        }

        public static Guid CacheAllowWriters
        {
            get
            {
                return new Guid("773ED051-58DB-4ff2-879B-08B21AB001E0");
            }
        }

        public static Guid CacheAuthenticatedUse
        {
            get
            {
                return new Guid("0A90B5E8-185A-4dec-BF3C-E60AAE08373F");
            }
        }

        public static Guid CacheCacheability
        {
            get
            {
                return new Guid("18f165be-6285-4a57-b3ab-4e9f913d299f");
            }
        }

        public static Guid CacheCheckForChanges
        {
            get
            {
                return new Guid("5b4d927c-d383-496b-bc79-1e61bd383019");
            }
        }

        public static Guid CacheDisplayDescription
        {
            get
            {
                return new Guid("9550e77a-4d10-464f-bc0c-102d5b1aec42");
            }
        }

        public static Guid CacheDisplayName
        {
            get
            {
                return new Guid("983f490b-fc53-4820-9354-e8de646b4b82");
            }
        }

        public static Guid CacheDuration
        {
            get
            {
                return new Guid("bdd1b3c3-18db-4acf-a963-e70ef4227fbc");
            }
        }

        public static Guid CacheEnabled
        {
            get
            {
                return new Guid("d8f18167-7cff-4c4e-bdbe-e7b0f01678f3");
            }
        }

        public static Guid CachePerformAclCheck
        {
            get
            {
                return new Guid("db03cb99-cf1e-40b8-adc7-913f7181dac3");
            }
        }

        public static Guid CacheVaryByCustom
        {
            get
            {
                return new Guid("4689a812-320e-4623-aab9-10ad68941126");
            }
        }

        public static Guid CacheVaryByHeader
        {
            get
            {
                return new Guid("89587dfd-b9ca-4fae-8eb9-ba779e917d48");
            }
        }

        public static Guid CacheVaryByParam
        {
            get
            {
                return new Guid("b8abfc64-c2bd-4c88-8cef-b040c1b9d8c0");
            }
        }

        public static Guid CacheVaryByRights
        {
            get
            {
                return new Guid("d4a6af1d-c6d7-4045-8def-cefa25b9ec30");
            }
        }

        public static Guid Contact
        {
            get
            {
                return new Guid("aea1a4dd-0f19-417d-8721-95a1d28762ab");
            }
        }

        public static Guid ContentType
        {
            get
            {
                return AveBuiltInFieldId.ContentType;
            }
        }

        public static Guid ContentTypeId
        {
            get
            {
                return AveBuiltInFieldId.ContentTypeId;
            }
        }

        public static Guid CreatedBy
        {
            get
            {
                return AveBuiltInFieldId.Author;
            }
        }

        public static Guid CreatedDate
        {
            get
            {
                return AveBuiltInFieldId.Created;
            }
        }

        public static Guid DeclaredRecord
        {
            get
            {
                return new Guid("F9A44731-84EB-43a4-9973-CD2953AD8646");
            }
        }

        public static Guid Description
        {
            get
            {
                return AveBuiltInFieldId.Comments;
            }
        }

        public static Guid DocumentId
        {
            get
            {
                return new Guid("AE3E2A36-125D-45d3-9051-744B513536A6");
            }
        }

        public static Guid DocumentIdPersistId
        {
            get
            {
                return new Guid("C010D384-479C-494f-968C-C413DBE3DE29");
            }
        }

        public static Guid DocumentIdUrl
        {
            get
            {
                return new Guid("3B63724F-3418-461f-868B-7706F69B029C");
            }
        }

        public static Guid ExemptFromPolicy
        {
            get
            {
                return new Guid("B0227F1A-B179-4D45-855B-A18F03706BCB");
            }
        }

        public static Guid ExpirationDate
        {
            get
            {
                return new Guid("ACD16FDF-052F-40F7-BB7E-564C269C9FBC");
            }
        }

        public static Guid ExpirationDateSaved
        {
            get
            {
                return new Guid("74E6AE8A-0E3E-4DCB-BBFF-B5A016D74D64");
            }
        }

        public static Guid ExpiryDate
        {
            get
            {
                return new Guid("a990e64f-faa3-49c1-aafa-885fda79de62");
            }
        }

        public static Guid HeaderStylesDefinitions
        {
            get
            {
                return new Guid("a932ec3f-94c1-48b1-b6dc-41aaa6eb7e54");
            }
        }

        public static Guid Hidden
        {
            get
            {
                return new Guid("7581e709-5d87-42e7-9fe6-698ef5e86dd3");
            }
        }

        public static Guid HoldRecordStatus
        {
            get
            {
                return new Guid("3AFCC5C7-C6EF-44f8-9479-3561D72F9E8E");
            }
        }

        public static Guid IconOverlay
        {
            get
            {
                return new Guid("B77CDBCF-5DCE-4937-85A7-9FC202705C91");
            }
        }

        public static Guid IsLocked
        {
            get
            {
                return new Guid("740931E6-D79E-44a6-A752-A06EB23C11B0");
            }
        }

        public static Guid LastModifiedBy
        {
            get
            {
                return AveBuiltInFieldId.Editor;
            }
        }

        public static Guid LastModifiedDate
        {
            get
            {
                return AveBuiltInFieldId.Modified;
            }
        }

        public static Guid LocalContactEmail
        {
            get
            {
                return new Guid("c79dba91-e60b-400e-973d-c6d06f192720");
            }
        }

        public static Guid LocalContactImage
        {
            get
            {
                return new Guid("dc47d55f-9bf9-494a-8d5b-e619214dd19a");
            }
        }

        public static Guid LocalContactName
        {
            get
            {
                return new Guid("7546ad0d-6c33-4501-b470-fb3003ca14ba");
            }
        }

        public static Guid MigratedGuid
        {
            get
            {
                return new Guid("75bed596-0661-4edd-9724-1d607ab8d3b5");
            }
        }

        public static Guid NotificationListDeliveryDate
        {
            get
            {
                return new Guid("E5BD81F1-C408-4abe-84E6-2119D568361E");
            }
        }

        public static Guid NotificationListUrl
        {
            get
            {
                return new Guid("789A7435-9C89-4d65-A472-D386ABA0EDF4");
            }
        }

        public static Guid PageLayout
        {
            get
            {
                return new Guid("0f800910-b30d-4c8f-b011-8189b2297094");
            }
        }

        public static Guid PreviewImage
        {
            get
            {
                return new Guid("188ce56c-61e0-4d2a-9d3e-7561390668f7");
            }
        }

        public static Guid PublishedLinksDescription
        {
            get
            {
                return new Guid("92BBA27E-EEF6-41aa-B728-6DD9CAF2BDE2");
            }
        }

        public static Guid PublishedLinksDisplayName
        {
            get
            {
                return new Guid("C80F535B-A430-4273-8F4F-F3E95507B62A");
            }
        }

        public static Guid PublishedLinksUrl
        {
            get
            {
                return new Guid("70B38565-A310-4546-84A7-709CFDC140CF");
            }
        }

        public static Guid PublishingImageCaption
        {
            get
            {
                return new Guid("66F500E9-7955-49ab-ABB1-663621727D10");
            }
        }

        public static Guid PublishingPageContent
        {
            get
            {
                return new Guid("F55C4D88-1F2E-4ad9-AAA8-819AF4EE7EE8");
            }
        }

        public static Guid PublishingPageIcon
        {
            get
            {
                return new Guid("3894ec3f-4674-4924-a440-8872bec40cf9");
            }
        }

        public static Guid PublishingPageImage
        {
            get
            {
                return new Guid("3de94b06-4120-41a5-b907-88773e493458");
            }
        }

        public static Guid RatingsCount
        {
            get
            {
                return new Guid("b1996002-9167-45e5-a4df-b2c41c6723c7");
            }
        }

        public static Guid RedirectURL
        {
            get
            {
                return new Guid("AC57186E-E90B-4711-A038-B6C6A62A57DC");
            }
        }

        public static Guid ReusableHtml
        {
            get
            {
                return new Guid("82dd22bf-433e-4260-b26e-5b8360dd9105");
            }
        }

        public static Guid ReusableText
        {
            get
            {
                return new Guid("890e9d41-5a0e-4988-87bf-0fb9d80f60df");
            }
        }

        public static Guid ReusableTextType
        {
            get
            {
                return new Guid("3a4b7f98-8d14-4800-8bf5-9ad1dd6a82ee");
            }
        }

        public static Guid RollupImage
        {
            get
            {
                return new Guid("543BC2CF-1F30-488e-8F25-6FE3B689D9AC");
            }
        }

        public static Guid ShowInRibbon
        {
            get
            {
                return new Guid("32E03F99-6949-466a-A4A6-057C21D4B516");
            }
        }

        public static Guid SpsDescription
        {
            get
            {
                return new Guid("631ab9cc-6687-488d-ab1d-a65b364a0d44");
            }
        }

        public static Guid StartDate
        {
            get
            {
                return new Guid("51d39414-03dc-4bd0-b777-d3e20cb350f7");
            }
        }

        public static Guid SummaryAudience
        {
            get
            {
                return new Guid("5f667e57-afc0-44e9-a33f-292b69060e8a");
            }
        }

        public static Guid SummaryGroup
        {
            get
            {
                return new Guid("d42659b5-ded6-41da-a941-c66e06f0e574");
            }
        }

        public static Guid SummaryIcon
        {
            get
            {
                return new Guid("897e4d29-edcf-42a4-951c-b6dbd6d1701f");
            }
        }

        public static Guid SummaryImage
        {
            get
            {
                return new Guid("366ec12a-7e06-48fb-97d3-6d3094b3dcc2");
            }
        }

        public static Guid SummaryLinks
        {
            get
            {
                return new Guid("B3525EFE-59B5-4f0f-B1E4-6E26CB6EF6AA");
            }
        }

        public static Guid SummaryLinks2
        {
            get
            {
                return new Guid("27761311-936A-40ba-80CD-CA5E7A540A36");
            }
        }

        public static Guid TargetItemId
        {
            get
            {
                return new Guid("c546204c-9a88-41d9-913c-ad32cff7dc73");
            }
        }

        public static Guid Title
        {
            get
            {
                return AveBuiltInFieldId.Title;
            }
        }

        public static Guid VariationGroupId
        {
            get
            {
                return new Guid("914fdb80-7d4f-4500-bf4c-ce46ad7484a4");
            }
        }

        public static Guid VariationRelationshipLink
        {
            get
            {
                return new Guid("766da693-38e5-4b1b-997f-e830b6dfcc7b");
            }
        }

        // Nested Types
        internal static class Legacy
        {
            // Properties
            public static Guid ImageDimension
            {
                get
                {
                    return new Guid("664151A8-54FF-434c-A59A-401D3A00DD79");
                }
            }

            public static Guid ImageHeight
            {
                get
                {
                    return new Guid("ABB77A79-8021-405c-8BD1-45403FD6CF98");
                }
            }

            public static Guid ImageWidth
            {
                get
                {
                    return new Guid("FE5429DC-CD77-4dc4-B24C-C82105AE3B36");
                }
            }

            public static Guid Thumbnail
            {
                get
                {
                    return new Guid("BCDE52EA-BD4F-4a2a-9F50-3224D3BD2778");
                }
            }
        }

        Guid IAveFieldId.AverageRatings
        {
            get { return AveFieldId.AverageRatings; }
        }

        Guid IAveFieldId.RatingsCount
        {
            get { return AveFieldId.RatingsCount; }
        }
    }
}
