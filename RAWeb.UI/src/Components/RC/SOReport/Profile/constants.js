import { ReportType } from "../config";

export const TIME_FRAME_TYPES = {
    ALL: 0,
    CUSTOM: 5,
};

export const objectLevelItems = [
    {
        name: RMResx.RM_MA_DocumentAndItem,
        value: ReportType.AllItem,
        checked: true,
    },
    {
        name: RMResx.RM_MA_SubSite,
        value: ReportType.SubSite,
        checked: false,
    },
];

export const googleObjectLevelItems = [
    {
        name: RMResx.RM_MA_Document,
        value: ReportType.AllGoogleDriveItems,
        checked: true,
    },
];
