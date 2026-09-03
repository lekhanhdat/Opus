import React, {
    forwardRef,
    useImperativeHandle,
    useRef,
    useState,
} from "react";
import _ from "lodash";

import { RAMessageType, ReportType } from "../config";
import TopButtonsComponent from "../../../Common/Util/TopButtonsComponent";
import ReportTableForTeams from "./ReportTableForTeams";
import { LicenseHelper, showToast } from "../../../../Utilities/CommonUtil";

const TableColumnsForTeams = [
    {
        header: RMResx.RM_DSB_Column_TeamsAndGroups,
        width: [180],
        resizeable: true,
    },
    {
        header: RMResx.RM_DSB_Column_Teams_TotalArchivedSize,
        width: [150],
        resizeable: true,
    },
    {
        header: (
            <div className="flex align-center">
                {RMResx.RM_DSB_Column_Teams_TotalSize}
                <$g.Popover>
                    {RMResx.RM_DSB_Column_Teams_TotalSizeNote}
                </$g.Popover>
            </div>
        ),
        width: [220],
        resizeable: true,
    },
];

function ArchivedSitesForTeams(props, ref) {
    const {
        filterData,
        selectSetting,
        exportReportInfoForTeams,
        setExportReportInfoForTeams,
        renderSearchBox,
        renderFooter,
        setShowExportDialog,
        setTotalCount,
    } = props;

    const refTopButtonsForTeams = useRef(null);
    const refReportTableForTeams = useRef(null);
    const refTeamsGroupCheckedCache = useRef([]);

    const [teamsGroupChecked, setTeamsGroupChecked] = useState([]);

    useImperativeHandle(ref, () => ({
        loadAllTeamsGroups,
        onExportDoActionForTeams,
    }));

    // const selectCount = RMResx.RM_Common_SelectTableItemsCounter.format(
    //     teamsGroupChecked.length,
    //     totalCount
    // );

    const loadAllTeamsGroups = async (isResetPagerIndex, paramFilterData) => {
        $$.loading(true);
        const requestOption = {
            url: "/api/Dashboard/GetArchiverTeamsGroupInfoByPager",
            data: paramFilterData || filterData,
        };
        const res = await fetchUtility(requestOption);
        $$.loading(false);
        if (res) {
            setTotalCount(res.Count);
            refReportTableForTeams.current.setTableInfo({
                items: res.ArchiverTeamsGroupSizeInfoes,
                isReset: isResetPagerIndex,
            });
        }
    };

    const onExportBtnForTeams = (reportType) => {
        setShowExportDialog(true);
        const clonedExportReportInfo = _.cloneDeep(exportReportInfoForTeams);
        clonedExportReportInfo.ReportType = reportType;
        if (reportType === ReportType.SpecifyTeamsGroup) {
            clonedExportReportInfo.SiteInfos = refTeamsGroupCheckedCache.current;
        } else {
            clonedExportReportInfo.SiteInfos = null;
        }
        setExportReportInfoForTeams(clonedExportReportInfo);
    };

    const onExportDoActionForTeams = async () => {
        exportReportInfoForTeams.TimeRange = selectSetting.TimeRange;
        exportReportInfoForTeams.StartTime = selectSetting.StartTime;
        exportReportInfoForTeams.EndTime = selectSetting.EndTime;
        $$.loading(true);
        const requestOption = {
            url: "/api/Dashboard/RunExportArchiverSiteInfoJob",
            data: exportReportInfoForTeams,
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

    const getShowActionsForTeams = (showExportTeamsGroupBtn) => {
        const buttonsInfo = [
            {
                isStatic: true,
                name: RMResx.RM_AR_Report_ExportAllTeamsGroup,
                onClick: () => onExportBtnForTeams(ReportType.AllTeamsGroup),
                isShow: showExportTeamsGroupBtn,
            },
            // {
            //     name: RMResx.RM_AR_Report_ExportSpecifyTeamsGroup,
            //     icon: "fia-export-settings",
            //     onClick: () =>
            //         onExportBtnForTeams(ReportType.SpecifyTeamsGroup),
            //     isShow: showExportSpecifyTeamsGroupBtn,
            // },
        ];
        const showButtons = buttonsInfo.filter((item) => item.isShow);
        return showButtons;
    };

    // Maybe can have checked in the future, so keep it
    const onSelectChangeForTeams = (items) => {
        const itemsSelected = items.length > 0;
        refTeamsGroupCheckedCache.current = items;
        setTeamsGroupChecked(items);
        const showButtons = getShowActionsForTeams(!itemsSelected);
        refTopButtonsForTeams.current.updateButtons(showButtons);
    };

    return (
        <>
            {LicenseHelper.EnableRecordsArchiver() && renderSearchBox()}
            <div className="ra-main-navbar">
                <div className="flex">
                    <TopButtonsComponent
                        ref={refTopButtonsForTeams}
                        data={{ menuBtnItems: getShowActionsForTeams(true) }}
                        showCount={4}
                    ></TopButtonsComponent>
                </div>
                {/* <div className="ra-main-selected-counter">{selectCount}</div> */}
            </div>
            <div className="ra-main-table">
                <ReportTableForTeams
                    id="raReportTableForTeams"
                    ref={refReportTableForTeams}
                    columns={TableColumnsForTeams}
                    uniqueKey={"TeamsGroupId"}
                    // checkable={true}
                    onChange={onSelectChangeForTeams}
                />
            </div>
            {renderFooter()}
        </>
    );
}

export default forwardRef(ArchivedSitesForTeams);
