import Permissions from './Permission';
import RouterUrls from '../Constants/RouterUrls';

const RECResources = 
[
    {
        "Name": "Home",
        "Url": RouterUrls.Home,
        "Permission": [
            Permissions.SuperAdmin,
            Permissions.PhysicalEndUser,
            Permissions.PhysicalAdmin,
            Permissions.JobMonitor
        ],
    },
    {
        "Name": "ControlPanel",
        "Url": RouterUrls.CP,
        "Permission": [
            Permissions.ControlPanel,
            Permissions.SuperAdmin
        ],
    },
    {
        "Name": "CP_Index",
        "Url": RouterUrls.CP_Index,
        "Permission": [
            Permissions.ControlPanel,
            Permissions.SuperAdmin
        ],
    },
    {
        "Name": "CP_StorageSettings",
        "Url": RouterUrls.CP_StorageSettings,
        "Permission": [
            Permissions.ControlPanel,
            Permissions.SuperAdmin
        ],
    },
    {
        "Name": "CP_AccountManagement",
        "Url": RouterUrls.CP_AccountManagement,
        "Permission": [
            Permissions.ControlPanel,
            Permissions.SuperAdmin
        ],
    },
    {
        "Name": "CP_AgentManagement",
        "Url": RouterUrls.CP_AgentManagement,
        "Permission": [
            Permissions.ControlPanel,
            Permissions.SuperAdmin
        ],
    },
    {
        "Name": "CP_GeneralSetting",
        "Url": RouterUrls.CP_GeneralSetting,
        "Permission": [
            Permissions.ControlPanel,
            Permissions.SuperAdmin
        ],
    },
    {
        "Name": "home",
        "Url": RouterUrls.CP_ExportSettings,
        "Permission": [
            Permissions.ControlPanel,
            Permissions.SuperAdmin
        ],
    },
    {
        "Name": "home",
        "Url": RouterUrls.CP_DashboardSettings,
        "Permission": [
            Permissions.ControlPanel,
            Permissions.SuperAdmin
        ],
    },
    {
        "Name": "home",
        "Url": RouterUrls.CP_EmailTemplate,
        "Permission": [
            Permissions.ControlPanel,
            Permissions.SuperAdmin
        ],
    },
    {
        "Name": "home",
        "Url": RouterUrls.CP_EditEmailTemplate,
        "Permission": [
            Permissions.ControlPanel,
            Permissions.SuperAdmin
        ],
    },
    {
        "Name": "home",
        "Url": RouterUrls.JM,
        "Permission": [
            Permissions.JobMonitor,
            Permissions.SuperAdmin,
            Permissions.DelegateAdmin
        ],
    },
    {
        "Name": "home",
        "Url": RouterUrls.JM_Index,
        "Permission": [
            Permissions.JobMonitor,
            Permissions.SuperAdmin
        ],
    },
    {
        "Name": "home",
        "Url": RouterUrls.JM_Detail,
        "Permission": [
            Permissions.JobMonitor,
            Permissions.SuperAdmin
        ],
    },
    {
        "Name": "home",
        "Url": RouterUrls.JM_PlanDetail,
        "Permission": [
            Permissions.JobMonitor,
            Permissions.SuperAdmin
        ],
    },
    {
        "Name": "BCM_TermManagement",
        "Url": RouterUrls.BCM_TermManagement,
        "Permission": [
            Permissions.TermManagementAdmin,
            Permissions.PhysicalEndUser,
            Permissions.PhysicalAdmin,
            Permissions.JobMonitor
        ],
    },
]

export default RECResources;