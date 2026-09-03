import { LicenseHelper } from "../../../Utilities/CommonUtil";

export const getPermissionReportList = () => {
    if (LicenseHelper.HasOpusSOLicenseOnly()) {
        return [
            {
                text: RMResx.RM_CP_AM_Report_Permission_SpecificReport_Option05,
                tooltip: RMResx.RM_CP_AM_Report_Permission_SpecificReport_Option05,
                value: 16,
                checked: true,
            },
            {
                text: RMResx.RM_CP_AM_Report_Permission_SpecificReport_Option06,
                tooltip: RMResx.RM_CP_AM_Report_Permission_SpecificReport_Option06,
                value: 32,
                checked: true,
            },
        ];
    }

    return [
        {
            text: RMResx.RM_CP_AM_Report_Permission_SpecificReport_Option01,
            tooltip: RMResx.RM_CP_AM_Report_Permission_SpecificReport_Option01,
            value: 1,
            checked: true,
        },
        {
            text: RMResx.RM_CP_AM_Report_Permission_SpecificReport_Option02,
            tooltip: RMResx.RM_CP_AM_Report_Permission_SpecificReport_Option02,
            value: 2,
            checked: true,
        },
        {
            text: RMResx.RM_CP_AM_Report_Permission_SpecificReport_Option03,
            tooltip: RMResx.RM_CP_AM_Report_Permission_SpecificReport_Option03,
            value: 4,
            checked: true,
        },
        {
            text: RMResx.RM_CP_AM_Report_Permission_SpecificReport_Option04,
            tooltip: RMResx.RM_CP_AM_Report_Permission_SpecificReport_Option04,
            value: 8,
            checked: true,
        },
        {
            text: RMResx.RM_CP_AM_Report_Permission_SpecificReport_Option05,
            tooltip: RMResx.RM_CP_AM_Report_Permission_SpecificReport_Option05,
            value: 16,
            checked: true,
        },
        {
            text: RMResx.RM_CP_AM_Report_Permission_SpecificReport_Option06,
            tooltip: RMResx.RM_CP_AM_Report_Permission_SpecificReport_Option06,
            value: 32,
            checked: true,
        },
        {
            text: RMResx.RM_CP_AM_Report_Permission_SpecificReport_Option07,
            tooltip: RMResx.RM_CP_AM_Report_Permission_SpecificReport_Option07,
            value: 64,
            checked: true,
        },
    ];
};
