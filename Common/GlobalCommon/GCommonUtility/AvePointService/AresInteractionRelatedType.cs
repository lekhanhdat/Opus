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
namespace AvePoint.GCommon.Utility.AvePointService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Base type for objects that are delivered when interacting with AvePoint service
    /// </summary>
    public class AresBaseDto
    {
        /// <summary>
        /// Default construct method
        /// </summary>
        protected AresBaseDto() { MsgVersion = 0; }

        /// <summary>
        /// Message version
        /// </summary>
        public Int32 MsgVersion { get; set; }

        /// <summary>
        /// Whether has error
        /// </summary>
        public Boolean HasError { get; set; }

        /// <summary>
        /// Error message
        /// </summary>
        public String ErrorMsg { get; set; }

        /// <summary>
        /// the errors related id to troubleshoot issue  
        /// </summary>
        public String CorrelationId { get; set; }
    }

    public class Endpoint : AresBaseDto
    {
        /// <summary>
        /// Guid used to identify the client group
        /// </summary>
        public string ClientGroupId { get; set; }
        /// <summary>
        /// String used to identify the datacenter
        /// </summary>
        public String InstanceId { get; set; }
        /// <summary>
        /// Module
        /// </summary>
        public Module Module { get; set; }
        /// <summary>
        /// Queue or relay in service bus
        /// </summary>
        public RelayType RelayType { get; set; }
        /// <summary>
        /// Service bus connection string
        /// </summary>
        public String ConnectionInfo { get; set; }
    }

    /// <summary>
    /// Client information like region, module and so on...
    /// </summary>
    public class ClientInfoDto : AresBaseDto
    {
        /// <summary>
        /// Guid used to identify the client group.
        /// </summary>
        public String ClientGroupKey { get; set; }

        /// <summary>
        /// Specify a service bus region
        /// AvePoint service region is managed by the traffic manager in Azure, there is only one URL public.
        /// </summary>
        public String Region { get; set; }

        /// <summary>
        /// Module
        /// </summary>
        public String Module { get; set; }

        /// <summary>
        /// Queue or Relay
        /// </summary>
        public String RelayType { get; set; }

        /// <summary>
        /// Get client information
        /// </summary>
        /// <param name="region"></param>
        /// <param name="module"></param>
        /// <param name="relayType"></param>
        /// <returns></returns>
        public static ClientInfoDto GetClientInfo(Region region, Module module, RelayType relayType)
        {
            ClientInfoDto info = new ClientInfoDto();
            info.Region = region.ToString();
            info.Module = module.ToString();
            info.RelayType = relayType.ToString();
            return info;
        }
    }

    /// <summary>
    /// Service bus connection information returned by AvePoint service
    /// </summary>
    public class SbConnectionDto : AresBaseDto
    {
        /// <summary>
        /// String used to identify the datacenter
        /// </summary>
        public String InstanceId { get; set; }
        /// <summary>
        /// Module: RP, CA...
        /// </summary>
        public Module Module { get; set; }
        /// <summary>
        /// Queue or relay
        /// </summary>
        public RelayType RelayType { get; set; }
        /// <summary>
        /// Service bus connection string
        /// </summary>
        public String ConnectionInfo { get; set; }
        /// <summary>
        /// Service bus path
        /// </summary>
        public String ConnectionPath { get; set; }
        /// <summary>
        /// Client version
        /// </summary>
        public String ClientVersion { get; set; }
        /// <summary>
        /// Is staging environment
        /// </summary>
        public Boolean IsStaging { get; set; }
    }

    /// <summary>
    /// Registration information that sent to AvePoint Service to regist the SharePoint event receiver
    /// </summary>
    public class RegistrationDto : AresBaseDto
    {
        /// <summary>
        /// Guid used to identify the client group
        /// </summary>
        public String ClientGroupId { get; set; }
        /// <summary>
        /// Site URL that is being registed
        /// </summary>
        public String SiteUrl { get; set; }
        /// <summary>
        /// Credential used for connecting to SharePoint
        /// </summary>
        public RegistrationCredentialDto Credential { get; set; }
        /// <summary>
        /// WebRelativeUrl
        /// Root web this property is Null, other web like subsite1/subsite2/subsite3
        /// If event receiver is registed to higher level, this property is Null
        /// </summary>
        public String RelativeWeb { get; set; }
        /// <summary>
        /// List title
        /// If event receiver is registed to higher level, ths property is Null
        /// </summary>
        public String ListTitle { get; set; }
        /// <summary>
        /// For replicator, this property is mapping ID.
        /// For the other module, if there is no mapping concept, please assign a const Guid value
        /// </summary>
        public String ScopeId { get; set; }
        /// <summary>
        /// Module, RP, CG, CA and so on
        /// </summary>
        public Module Module { get; set; }
        /// <summary>
        /// Level which the event receiver is registed to
        /// </summary>
        public ObjectType ObjectType { get; set; }
        /// <summary>
        /// Kinds of event receiver type that need to registed 
        /// </summary>
        public EventType[] EventTypes { get; set; }
        /// <summary>
        /// Extention property, can store some extra information in it
        /// </summary>
        public String Extension { get; set; }
    }

    /// <summary>
    /// The registration result, should check the HasError property to make sure the registration process successfully
    /// </summary>
    public class RegisterResultDto : AresBaseDto
    {
    }

    /// <summary>
    /// Credential used to connect to SharePoint
    /// </summary>
    public class RegistrationCredentialDto : AresBaseDto
    {
        /// <summary>
        /// User login name
        /// </summary>
        public String UserName { get; set; }
        /// <summary>
        /// Domain name, no use temporarily
        /// </summary>
        public String UserDomain { get; set; }
        /// <summary>
        /// Need encrypted
        /// </summary>
        public String UserPass { get; set; }
    }

    /// <summary>
    /// Used to send a response when AvePoint service calls a synchronized method that need a response value
    /// </summary>
    public class AresRemoteEventResultDto
    {
        /// <summary>
        /// Error message
        /// </summary>
        public string ErrorMessage { get; set; }
        /// <summary>
        /// The continuation status of this event, equals to SPRemoteEventServiceStatus status
        /// </summary>
        public AresRemoteEventServiceStatus Status { get; set; }
    }

    /// <summary>
    /// Service region, maybe add more later.
    /// </summary>
    public enum Region
    {
        /// <summary>
        /// Default value
        /// </summary>
        Unknown,
        /// <summary>
        /// Specify the service bus region to United States
        /// </summary>
        US,
        /// <summary>
        /// Specify the service bus region to Europe
        /// </summary>
        EU,
        /// <summary>
        /// Specify the service bus region to Asia
        /// </summary>
        AS
    }

    /// <summary>
    /// Modules that use AvePoint service
    /// </summary>
    public enum Module
    {
        /// <summary>
        /// Replicator
        /// </summary>
        RP = 0,
        /// <summary>
        /// RC
        /// </summary>
        RC = 1,
        /// <summary>
        /// GA
        /// </summary>
        GA = 2,
    }

    /// <summary>
    /// Service bus type
    /// </summary>
    public enum RelayType
    {
        /// <summary>
        /// Default value
        /// </summary>
        Unknown,
        /// <summary>
        /// Use relay to receive message
        /// </summary>
        Relay = 0,
        /// <summary>
        /// Use queue to recevie message
        /// </summary>
        Queue = 1,
    }

    /// <summary>
    /// Event continuation status
    /// </summary>
    public enum AresRemoteEventServiceStatus : byte
    {
        /// <summary>
        /// Notify SharePoint to continue handle the action
        /// </summary>
        Continue = 0,
        /// <summary>
        /// Notify SharePoint interrupt the action without any reason
        /// </summary>
        CancelNoError = 1,
        /// <summary>
        /// Notify SharePoint interrupt the action without some error
        /// </summary>
        CancelWithError = 2,
        [Obsolete("Default list forms are committed through asynchronous XmlHttpRequests, so redirect urls specified in this way aren't followed by default.  In order to force a list form to follow a cancelation redirect url, set the list form web part's CSRRenderMode property to CSRRenderMode.ServerRender")]
        CancelWithRedirectUrl = 3,
    }

    public enum ObjectType
    {
        Site = 0,
        Web = 1,
        List = 2,
    }

    public enum EventType
    {
        ItemAdding = 1,
        ItemUpdating = 2,
        ItemDeleting = 3,
        ItemCheckingIn = 4,
        ItemCheckingOut = 5,
        ItemUncheckingOut = 6,
        ItemAttachmentAdding = 7,
        ItemAttachmentDeleting = 8,
        ItemFileMoving = 9,
        ItemVersionDeleting = 11,
        FieldAdding = 101,
        FieldUpdating = 102,
        FieldDeleting = 103,
        ListAdding = 104,
        ListDeleting = 105,
        SiteDeleting = 201,
        WebDeleting = 202,
        WebMoving = 203,
        WebAdding = 204,
        GroupAdding = 301,
        GroupUpdating = 302,
        GroupDeleting = 303,
        GroupUserAdding = 304,
        GroupUserDeleting = 305,
        RoleDefinitionAdding = 306,
        RoleDefinitionUpdating = 307,
        RoleDefinitionDeleting = 308,
        RoleAssignmentAdding = 309,
        RoleAssignmentDeleting = 310,
        InheritanceBreaking = 311,
        InheritanceResetting = 312,
        ItemAdded = 10001,
        ItemUpdated = 10002,
        ItemDeleted = 10003,
        ItemCheckedIn = 10004,
        ItemCheckedOut = 10005,
        ItemUncheckedOut = 10006,
        ItemAttachmentAdded = 10007,
        ItemAttachmentDeleted = 10008,
        ItemFileMoved = 10009,
        ItemFileConverted = 10010,
        ItemVersionDeleted = 10011,
        FieldAdded = 10101,
        FieldUpdated = 10102,
        FieldDeleted = 10103,
        ListAdded = 10104,
        ListDeleted = 10105,
        SiteDeleted = 10201,
        WebDeleted = 10202,
        WebMoved = 10203,
        WebProvisioned = 10204,
        WebRestored = 10205,
        GroupAdded = 10301,
        GroupUpdated = 10302,
        GroupDeleted = 10303,
        GroupUserAdded = 10304,
        GroupUserDeleted = 10305,
        RoleDefinitionAdded = 10306,
        RoleDefinitionUpdated = 10307,
        RoleDefinitionDeleted = 10308,
        RoleAssignmentAdded = 10309,
        RoleAssignmentDeleted = 10310,
        InheritanceBroken = 10311,
        InheritanceReset = 10312,
    }
}
