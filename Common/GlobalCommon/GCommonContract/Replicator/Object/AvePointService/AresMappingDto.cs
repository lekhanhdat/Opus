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
namespace AvePoint.GCommon.Contract.Replicator.Object.AvePointService
{
    using AvePoint.GCommon.Contract.Replicator.Object.Message;
    using AvePoint.GCommon.Contract.Server.ControlPanel.ColumnMapping.Object;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
    using AvePoint.GCommon.Contract.Server.ControlPanel.LanguageMapping.Object;
    using AvePoint.GCommon.Contract.Server.ControlPanel.SuperUserConfiguration.Object;
    using AvePoint.GCommon.Contract.Server.ControlPanel.UserAndDomainMapping.DomainMapping;
    using AvePoint.GCommon.Contract.Server.ControlPanel.UserAndDomainMapping.UserMapping;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class AresMappingDto
    {
        public ColumnMappingDataContract ColumnMapping = null;

        public FilterPolicyWrapper FilterPolicy = null;

        public MappingSetting Setting = null;

        public LanguageMappingDto LanguageMapping = null;

        public UserMappingDataContract UserMapping = null;

        public DomainMappingDataContract DomainMapping = null;

        public DataEncryptionProfile DataEncryptionProfile = null;
        public Dictionary<string, SuperUserConfigurationDto> SuperUserConfigurationSiteUrlMappings = null;

        public string PlanId { get; set; }
        public string PlanName { get; set; }
        public string MappingId { get; set; }
        public bool Enable { get; set; }
        public Guid SrcFarmId { get; set; }
        public Guid DestFarmId { get; set; }
        public ReplicationEvent EventHandlerTypes { get; set; }
        public bool IsEventHandlerEnable { get; set; }
        public int Type { get; set; }
        public string SourcePath { get; set; }
        public string DestPath { get; set; }
        public AresTreeNodeDto SrcItems { get; set; }
        public AresTreeNodeDto DestItems { get; set; }
        public List<AresMappingDto> ImplicitMappings { get; set; }
    }
}
