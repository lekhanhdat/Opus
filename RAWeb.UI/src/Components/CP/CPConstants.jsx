import { StorageTypeIndex } from "../../Constants/Constants";

const StorageTypeCol = [
    {
        name: RMResx["MediaStorage_Amazon_Amazon_S3"],
        value: "Amazon",
        index: StorageTypeIndex.Amazon,
        vim: ["amazon_vim"],
        checked: false,
    },
    {
        name: RMResx["MediaStorage_S3Compatible_Compatible_Amazon_S3"],
        value: "S3Compatible",
        index: StorageTypeIndex.S3Compatible,
        vim: ["s3compatible_vim"],
        checked: false,
    },
    {
        name: RMResx["MediaStorage_Wasabi"],
        value: "Wasabi",
        index: StorageTypeIndex.WasabiS3Compatible,
        vim: ["wasabi_vim"],
        checked: false,
    },
    // {
    //     name: RMResx["MediaStorage_Box_Type"],
    //     value: "Box",
    //     index: StorageTypeIndex.Box,
    //     vim: ["box_vim"],
    //     checked: false,
    // },
    {
        name: RMResx["MediaStorage_Dropbox_Dropbox"],
        value: "Dropbox",
        index: StorageTypeIndex.Dropbox,
        vim: ["dropbox_vim"],
        checked: false,
    },
    {
        name: RMResx["MediaStorage_FTP_FTP"],
        value: "FTP",
        index: StorageTypeIndex.FTP,
        vim: ["ftp_vim"],
        checked: false,
    },
    {
        name: RMResx["MediaStorage_Google"],
        value: "Google",
        index: StorageTypeIndex.Google,
        vim: ["google_vim"],
        checked: false,
    },
    {
        name: RMResx["MediaStorage_Azure_Windows_Azure_Storage"],
        value: "Azure",
        index: StorageTypeIndex.AzureBlob,
        vim: ["azure_vim"],
        checked: true,
    },
    // {
    //     name: RMResx["MediaStorage_SFTP_NetApp_Alta_Vault"],
    //     value: "NetApp",
    //     index: StorageTypeIndex.NetApp_Alta_Vault,
    //     vim: ["netapp_alta_vault_vim"],
    //     checked: false,
    // },
    // {
    //     name: RMResx["MediaStorage_RackSpace_Cloud_Files"],
    //     value: "Rackspace",
    //     index: StorageTypeIndex.Rackspace,
    //     vim: ["rackspace_vim"],
    //     checked: false,
    // },
    {
        name: RMResx["MediaStorage_SFTP_SFTP"],
        value: "SFTP",
        index: StorageTypeIndex.SFTP,
        vim: ["sftp_vim"],
        checked: false,
    },
];

const StorageRegion = [
    {
        name: RMResx["MediaStorage_Amazon_US_Standard"],
        value: "usstandard",
        checked: true,
    },
    {
        name: RMResx["MediaStorage_Amazon_US_West_Northern_California"],
        value: "uswest",
        checked: false,
    },
    {
        name: RMResx["MediaStorage_Amazon_EU_Ireland"],
        value: "eu",
        checked: false,
    },
    {
        name: RMResx["MediaStorage_Amazon_EU_London"],
        value: "london",
        checked: false,
    },
    {
        name: RMResx["MediaStorage_Amazon_Asia_Pacific_Singapore"],
        value: "apac",
        checked: false,
    },
    {
        name: RMResx["MediaStorage_Amazon_Asia_Pacific_Tokyo"],
        value: "tokyo",
        checked: false,
    },
    {
        name: RMResx["MediaStorage_Amazon_Asia_Pacific_Sydney"],
        value: "sydney",
        checked: false,
    },
    {
        name: RMResx["MediaStorage_Amazon_US_West_Oregon"],
        value: "oregon",
        checked: false,
    },
    {
        name: RMResx["MediaStorage_Amazon_South_America_Saopaulo"],
        value: "saopaulo",
        checked: false,
    },
    {
        name: RMResx["MediaStorage_Amazon_US_Ohio"],
        value: "ohio",
        checked: false,
    },
    {
        name: RMResx["MediaStorage_Amazon_Canada_Central"],
        value: "canadacentral",
        checked: false,
    },
    {
        name: RMResx["MediaStorage_Amazon_EU_Frankfurt"],
        value: "frankfurt",
        checked: false,
    },
    {
        name: RMResx["MediaStorage_Amazon_Asia_Seoul"],
        value: "seoul",
        checked: false,
    },
    {
        name: RMResx["MediaStorage_Amazon_Asia_Mumbai"],
        value: "mumbai",
        checked: false,
    },
];

const CustomizedRegion = {
    name: RMResx["MediaStorage_Amazon_CustomizedRegion"],
    value: "customized",
    checked: false,
};

const RetentionDataTimeRadioValue = {
    ArchivedTime: 1,
    ModifiedTime: 2,
};

const DataRadioValue = {
    DelData: 1,
    MoveData: 2,
    MarkDataTier: 3,
};

const TierValue = {
    ArchivedTier: 3,
    ColdTier: 4,
};

const DateUnit = {
    Day: 0,
    Week: 1,
    Month: 2,
    Year: 3,
};

const MessageType = {
    Successful: 0,
    Failed: 1,
    Exception: 2
};

const StubFileType = {
    Aspx: 0,
    Txt: 1,
    Html: 2,
    Url: 3,
};

const ASPXStubFileType = {
    name: RMResx.RM_AR_CP_Stub_Type_Aspx,
    value: StubFileType.Aspx,
    checked: false,
}

const StubFileTypeColNoASPX = [
    {
        name: RMResx.RM_AR_CP_Stub_Type_Txt,
        value: StubFileType.Txt,
        checked: true,
    },
    {
        name: RMResx.RM_AR_CP_Stub_Type_Html,
        value: StubFileType.Html,
        checked: false,
    },
    {
        name: RMResx.RM_AR_CP_Stub_Type_RestoreLink,
        value: StubFileType.Url,
        checked: false,
    },
]

const StubFileTypeCol = [
    ASPXStubFileType,
    ...StubFileTypeColNoASPX,
];

const GroupTeamSitePermissionType = {
    Owner: 0,
    OwnerOrMember: 1,
};

const GroupTeamSitePermission = [
    {
        name: RMResx["StorageOptimization.Gui_363ea7bd-6f86-404e-a9ed-250cfb1248b6"],
        value: GroupTeamSitePermissionType.Owner,
        checked: true,
    },
    {
        name: RMResx["StorageOptimization.Gui_b2f2702c-5e4b-443e-b1e2-175db48169f7"],
        value: GroupTeamSitePermissionType.OwnerOrMember,
        checked: false,
    },
];

const SiteCollectionPermissionType = {
    SiteOwner: 0,
    SiteOwnerOrSiteMemberGroup: 1,
    SiteOwnerOrSiteMemberOrSiteVisitor: 3,
    SiteOwnerOrSpecialGroup: 2,
};

const SiteCollectionPermission = [
    {
        name: RMResx.RM_AR_CP_EURS_Permission_SiteOwner,
        value: SiteCollectionPermissionType.SiteOwner,
        checked: true,
    },
    {
        name: RMResx.RM_AR_CP_EURS_Permission_SiteMember,
        value: SiteCollectionPermissionType.SiteOwnerOrSiteMemberGroup,
        checked: false,
    },
    {
        name: RMResx.RM_AR_CP_EURS_Permission_SiteVisitor,
        value: SiteCollectionPermissionType.SiteOwnerOrSiteMemberOrSiteVisitor,
        checked: false,
    },
    {
        name: RMResx.RM_AR_CP_EURS_Permission_SPGroup,
        value: SiteCollectionPermissionType.SiteOwnerOrSpecialGroup,
        checked: false,
    },
];

export {
    StorageTypeCol,
    StorageRegion,
    CustomizedRegion,
    RetentionDataTimeRadioValue,
    DataRadioValue,
    TierValue,
    DateUnit,
    MessageType,
    StubFileType,
    StubFileTypeColNoASPX,
    StubFileTypeCol,
    GroupTeamSitePermissionType,
    GroupTeamSitePermission,
    SiteCollectionPermissionType,
    SiteCollectionPermission,
};