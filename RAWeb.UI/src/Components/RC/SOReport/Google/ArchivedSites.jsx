import {
    forwardRef,
    useImperativeHandle,
    useRef,
    useState,
} from "react";
import _ from "lodash";

import { RAMessageType, ReportType } from "../config";
import TopButtonsComponent from "../../../Common/Util/TopButtonsComponent";
import GoogleReportTable from "./ReportTable";
import { LicenseHelper, showToast } from "../../../../Utilities/CommonUtil";

const TableColumns = [
    {
        header: RMResx.RM_DSB_Column_GoogleDrive,
        width: [180],
        resizeable: true,
    },
    {
        header: RMResx.RM_DSB_Column_Size,
        width: [150],
        resizeable: true,
    },
    {
        header: <div className="flex align-center">
            {RMResx.RM_DSB_Column_Deleted_Size}
            {/* <$g.Popover>{RMResx.RM_DSB_Column_Deleted_Size_Note}</$g.Popover> */}
        </div>,
        width: [150],
        resizeable: true,
    },
];

function ArchivedSitesForGoogle(props, ref) {
    const {
        filterData,
        selectSetting,
        exportReportInfoForGoogle,
        setExportReportInfoForGoogle,
        renderSearchBox,
        renderFooter,
        setShowExportDialog,
        setTotalCount,
        totalCount
    } = props;

    const refTopButtonsForGoogle = useRef(null);
    const refReportTableForGoogle = useRef(null);
    const refGoogleCheckedCache = useRef([]);

    const [googleChecked, setGoogleChecked] = useState([]);

    useImperativeHandle(ref, () => ({
        loadAllGoogleData,
        onExportDoActionForGoogle,
    }));

    const selectCount = RMResx.RM_Common_SelectTableItemsCounter.format(
        googleChecked.length,
        totalCount
    );

    const loadAllGoogleData = async (isResetPagerIndex, paramFilterData) => {
        $$.loading(true);
        const requestOption = {
            url: "/api/Dashboard/GetGoogleArchiverInfoByPager",
            data: paramFilterData || filterData,
        };
        const res = await fetchUtility(requestOption);
        $$.loading(false);
        if (res) {
            setTotalCount(res.Count);
            refReportTableForGoogle.current.setTableInfo({
                items: res.ArchiverSiteSizeInfos,
                isReset: isResetPagerIndex,
            });
        }
    };

    const onExportBtnForGoogle = (reportType) => {
        setShowExportDialog(true);
        const clonedExportReportInfo = _.cloneDeep(exportReportInfoForGoogle);
        clonedExportReportInfo.ReportType = reportType;
        if (reportType === ReportType.AllGoogleDriveItems) {
            clonedExportReportInfo.SiteInfos = refGoogleCheckedCache.current;
        } else {
            clonedExportReportInfo.SiteInfos = null;
        }
        setExportReportInfoForGoogle(clonedExportReportInfo);
    };

    const onExportDoActionForGoogle = async () => {
        exportReportInfoForGoogle.TimeRange = selectSetting.TimeRange;
        exportReportInfoForGoogle.StartTime = selectSetting.StartTime;
        exportReportInfoForGoogle.EndTime = selectSetting.EndTime;
        $$.loading(true);
        const requestOption = {
            url: "/api/Dashboard/RunExportArchiveGoogleDriveInfoJob",
            data: exportReportInfoForGoogle,
        };
        const res = await fetchUtility(requestOption);
        if (res.MessageType === RAMessageType.Successful) {
            showToast.success(
                <$g.I18NProvider msg={RMResx.RM_MA_HistoryExport_JobStart}>
                    <a className="ra-link-a" href="/Root/JM/Index">
                        {RMResx.RM_JS_JM_Title}
                    </a>
                    <a className="ra-link-a" href="/Root/DC/Download">
                        {RMResx.RM_JS_DC_Title}
                    </a>
                </$g.I18NProvider>
            );
        } else {
            showToast.error(res.ErrorMessage);
        }
        $$.loading(false);
        setShowExportDialog(false);
    };

    const getShowActionsForGoogle = (showExportGoogleBtn, showExportItemBtn) => {
        const buttonsInfo = [
            {
                isStatic: true,
                name: RMResx.RM_AR_Report_ExportAllTeamsGroup,
                onClick: () => onExportBtnForGoogle(ReportType.AllGoogleDrive),
                isShow: showExportGoogleBtn,
            },
            { 
                name: RMResx.RM_AR_Report_ExportItem,
                icon: "fia-export-settings",
                onClick: () => onExportBtnForGoogle(ReportType.AllGoogleDriveItems),
                isShow: showExportItemBtn
            }
        ];
        const showButtons = buttonsInfo.filter((item) => item.isShow);
        return showButtons;
    };

    // Maybe can have checked in the future, so keep it
    const onSelectChangeForGoogle = (items) => {
        const hasItemsSelected = items.length > 0;
        refGoogleCheckedCache.current = items;
        setGoogleChecked(items);
        const showButtons = getShowActionsForGoogle(!hasItemsSelected, hasItemsSelected);
        refTopButtonsForGoogle.current.updateButtons(showButtons);
    };

    return (
        <>
            {LicenseHelper.EnableRecordsArchiver() && renderSearchBox()}
            <div className="ra-main-navbar">
                <div className="flex">
                    <TopButtonsComponent
                        ref={refTopButtonsForGoogle}
                        data={{ menuBtnItems: getShowActionsForGoogle(true, false) }}
                        showCount={4}
                    ></TopButtonsComponent>
                </div>
                <div className="ra-main-selected-counter">{selectCount}</div>
            </div>
            <div className="ra-main-table">
                <GoogleReportTable
                    id="raReportTableForGoogle"
                    ref={refReportTableForGoogle}
                    columns={TableColumns}
                    uniqueKey={"SiteId"}
                    checkable={true}
                    onChange={onSelectChangeForGoogle}
                />
            </div>
            {renderFooter()}
        </>
    );
}

export default forwardRef(ArchivedSitesForGoogle);
