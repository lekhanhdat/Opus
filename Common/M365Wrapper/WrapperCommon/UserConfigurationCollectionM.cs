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
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeCommonWrapper
{
    [DataContract]
    public class UserConfigurationCollectionM
    {
        [DataMember]
       //public List<UserConfigurationM> UserConfigurations { get; set; }

        public Dictionary<string, UserConfigurationM> UserConfigurations { get; set; }
    }

    [DataContract]
    public class UserConfigurationM
    {
        [DataMember]
        public FolderIdM FolderId { get; set; }
        [DataMember]
        public int View { get; set; }
        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int SortOrder { get; set; }
        [DataMember]
        public int SortColumn { get; set; }
        [DataMember]
        public int ReadingPanePosition { get; set; }
        [DataMember]
        public bool IsExpanded { get; set; }
        [DataMember]
        public int SearchScope { get; set; }
    }
    [DataContract]
    public sealed class FolderIdM
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string ChangeKey { get; set; }
    }
}