import { NodeLevel } from "../../../../Constants/DAEnums";

const GuidEmpty = "00000000-0000-0000-0000-000000000000";
export const RAMessageType = {
    Successful: 0,
    Failed: 1,
    Exception: 2
};

export const RAFailedType =
{
    None: 0,
    NameExisting: 1,
    RunningJobExist: 2,
    NoIndexDevice: 3,
    NoDBSetting: 4,
    NoIndexDeviceAndDBSetting: 5,
    NoLocation: 6,
    LicenseExpired: 7,
    ScheduleServiceFailed: 8,
    DefaultTermIsOrphaned: 9,
    DisableRecordsManagement: 10,
    BreakFolderNode: 11,
    PhysicalMoveHasHoldConflict: 12,
    DeleteUsingSuite: 13,
    DeleteUningTemplate: 14,
    SoftDeleted: 15,
    UpdateFailed: 16,
    HasRunningWorkflowInstance: 17,
    UniqueIdSettingIsEmpty: 18,
    NotAvailableAgent: 19,
    AccessDenied: 20,
    EarlierThanNow: 21,
    MissingRequiredSettings: 22
};

export const ValidateResultType =
{
    AllCorrect: 0,
    NoDocAveConnection: 1,
    NoGlobalStorageSetting: 2,
    Nothing: 3
};

export const SplitterSize = {
    minAsize: "30%",
    minBsize: "30%",
    defaultAsize: "60%"
};

export const OperationState =
{
    None: 0,
    Running: 1,
    Succeeded: 2,
    Failed: 3,
    Exception: 4
};

export const CleanupAndDelRestoredType =
{
    OnlyFileAndVersion: 1,
    RelatedFileOrVersion: 2,
};

export default {
    GuidEmpty: GuidEmpty,
    newGuid: function () {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    },
    guidIsEmpty: function (guid) {
        return guid == null || guid == "" || guid == GuidEmpty;
    },
    isSiteCollection(node) {
        if (node.Level == NodeLevel.SiteCollection) {
            return true;
        } else {
            return false;
        }
    },
    isSite(node) {
        if (node.Level == NodeLevel.Site) {
            return true;
        } else {
            return false;
        }
    }, 
    isFolder(node) {
        if (node.Level == NodeLevel.Folder || node.Level == NodeLevel.Folders || node.Level == NodeLevel.RootFolder) {
            return true;
        } else {
            return false;
        }
    },
    isGroup(node) {
        if (node.Level == NodeLevel.WebApplication) {
            return true;
        } else {
            return false;
        }
    },
    isTeams(node) {
        if (node.Level == NodeLevel.Office365GroupEntire) {
            return true;
        } else {
            return false;
        }
    },
    getGroupNode(currNode){
        var node = currNode;
        while (node.Level != NodeLevel.WebApplication)
        {
            node = node.Parent;
        }
        return node;
    },
    isEXOGroup(node){
        if (node.Level == NodeLevel.ExchangeOnlineMailboxGroup) {
            return true;
        } else {
            return false;
        }
    },
    isEXOMailBox(node) {
        if (node.Level == NodeLevel.ExchangeOnlineMailbox) {
            return true;
        } else {
            return false;
        }
    },
    isAllowSetByRulesOnedriveLevel(node){
        let allowCustomTermOnedriveLevel = [NodeLevel.WebApplication, NodeLevel.SiteCollection];
        return allowCustomTermOnedriveLevel.includes(node.Level);
    },
    getEXOGroupNode(currNode){
        var node = currNode;
        while (node.Level != NodeLevel.ExchangeOnlineMailboxGroup)
        {
            node = node.Parent;
        }
        return node;
    },
    isFSFolder(node) {
        if (node.Level == NodeLevel.FSFolder) {
            return true;
        } else {
            return false;
        }
    },
    isAzureFileGroup(node) {
        if (node.level == NodeLevel.AzureFileShareGroup) {
            return true;
        } else {
            return false;
        }
    },
    getAzureFileGroupNode(currNode){
        var node = currNode;
        while (node.level != NodeLevel.AzureFileShareGroup)
        {
            node = node.parent;
        }
        return node;
    },
    isBoxGroup(node) {
        if (node.level == NodeLevel.BoxConnectionGroup) {
            return true;
        } else {
            return false;
        }
    },
    getBoxGroupNode(currNode){
        var node = currNode;
        while (node.level != NodeLevel.BoxConnectionGroup)
        {
            node = node.parent;
        }
        return node;
    },
    isGoogleContainer(node) {
        return !!(node.Level == NodeLevel.GoogleSharedDriveContainer || node.Level == NodeLevel.GoogleUserDriveContainer);
    },
    isGoogleDriveItem(node) {
        return !!(node.Level == NodeLevel.GoogleUserDrive || node.Level == NodeLevel.GoogleSharedDrive);
    },
    getGoogleDriveContainerNode(currNode) {
        let node = currNode;
        while (node.Level != NodeLevel.GoogleUserDriveContainer && node.Level != NodeLevel.GoogleSharedDriveContainer)
        {
            node = node.Parent;
        }
        return node;
    },
    isAllowSetByRulesGoogleLevel(node){
        let allowCustomTermGoogleLevel = new Set([
            NodeLevel.GoogleUserDriveContainer,
            NodeLevel.GoogleSharedDriveContainer,
            NodeLevel.GoogleUserDrive,
            NodeLevel.GoogleSharedDrive
        ]);
        return allowCustomTermGoogleLevel.has(node.Level);
    },
    commonHandelErrorMessage: function (data) {
        // if (data.MessageType == RM.RAMessageType.Failed) {
        //     return data.ErrorMessage;
        // }
        if (data.FaildType == RAFailedType.NotAvailableAgent) {
            return RMResx.RM_JS_BCM_RunJobFailed_NoAvailableAgent;
        }
        else {
            return data.ErrorMessage;
        }
    },
    convertUsersToRichCombobox(users) {
        let newUsers = [];
        users.forEach(user => {
            newUsers.push({
                name: user.DisplayName,
                value: user.UserId,
                disabled: false,
                tooltip: user.UserPrincipalName,
                readonly: false,
                invalid: false,
                conflict: false,
                data: user,
            });
        });
        return newUsers;
    }
};