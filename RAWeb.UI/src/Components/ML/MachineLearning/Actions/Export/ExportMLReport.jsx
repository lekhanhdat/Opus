import React, { useEffect, useState } from "react";
import _ from "lodash";
import { ExportType,ExportTypeI18Ns } from "../../../../RDM/ManualApproval/Constants";
import { showToast } from "../../../../../Utilities/CommonUtil";



const ExportMLReport = ({show, onHide}) => {

    const todayDate = RM.TimeUtil.getTodayStartEndTime();

    const [showDataDialog, setShowDataDialog] = useState(false);
    const [selectSetting, setSelectSetting] = useState({ LatestExportType: 1, CustomDate: { StartDateTime: "", EndDateTime: "" } });

    const [showDatapicker,setShowDatapicker] = useState(false);

    const [startDate, setStartDate] = useState(todayDate.start);

    const [endDate, setEndDate] = useState(todayDate.end);

    useEffect(() => {
        setShowDataDialog(show);
    }, [show]);

    const onChangeLatestExportType = (args) => {
        const clonedSetting = _.cloneDeep(selectSetting);
        clonedSetting.LatestExportType = Number(args);
        if(Number(args) === ExportType.Custom){
            setShowDatapicker(true);
            clonedSetting.CustomDate = {
                StartDateTime:RM.TimeUtil.getCommonDateStr(startDate),
                EndDateTime:RM.TimeUtil.getCommonDateStr(endDate)
            };
            setSelectSetting(clonedSetting);
            return;
        }
        setShowDatapicker(false);
        clonedSetting.CustomDate = {
            StartDateTime:RM.TimeUtil.getCommonDateStr(new Date(0)) + ":0",
            EndDateTime:RM.TimeUtil.getCommonDateStr(new Date(0)) + ":59"
        };
        setSelectSetting(clonedSetting);
    };

    // const onHide = () => {
    //     setShowDataDialog(false);
    // };

    const onChangeTimeRange = (args) =>{
        if(_.isNil(args.newValue)) {
            setStartDate(todayDate.start);
            setEndDate(todayDate.end);
            return;
        }
        const startDate = args.newValue.start;
        const endDate = args.newValue.end;
        setStartDate(startDate);
        setEndDate(endDate);
        const clonedSetting = _.cloneDeep(selectSetting);
        clonedSetting.LatestExportType = ExportType.Custom;
        clonedSetting.CustomDate = {
            StartDateTime:RM.TimeUtil.getCommonDateStr(startDate) + ":0",
            EndDateTime:RM.TimeUtil.getCommonDateStr(endDate) + ":59",
        };
        setSelectSetting(clonedSetting);
    };

    const onExport = () => {
        processExport();
    };

    const processExport = async () => {
        let requestOption = {
            url: "/api/TrainingReportApi/ExportTrainingReport",
            method:"POST",
            data: {
                TimeRange: selectSetting.LatestExportType,
                StartTime: selectSetting.CustomDate.StartDateTime,
                EndTime: selectSetting.CustomDate.EndDateTime
            },
        };

        $$.loading(true);
        const result = await fetchUtility(requestOption);
        $$.loading(false);

        if (result.MessageType === 0) {
            showToast.success(<$g.I18NProvider msg={RMResx.RM_MA_HistoryExport_JobStart}>
                <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                <a className="ra-link-a" href="/Root/DC/Download">{RMResx.RM_JS_DC_Title}</a>
            </$g.I18NProvider>);
        } else {
            showToast.error(result.ErrorMessage);
        }
        onHide();
    };

    return (
        <R.Dialog
            id="exportMLReportContainer"
            header={RMResx.RM_MA_Export}
            width={464}
            status={{ show: showDataDialog }}
            struct={{ foot: true }}
            onHide={onHide}
            destroy={true}
        >
            <div id="export-history-dialog">
                <h4 className="radio-title">
                    {RMResx.RM_MA_HistoryExport_Title}
                </h4>
                <div className="radio-content">
                    <div>
                        <div className="browser-radio-content">
                            <div className="radio-item">
                                <R.Radio
                                    name="location-radio"
                                    text={ExportTypeI18Ns.get(
                                        ExportType.After3Month
                                    )}
                                    value="1"
                                    checked={selectSetting.LatestExportType === 1}
                                    onChange={onChangeLatestExportType}
                                />
                            </div>
                            <div className="radio-item">
                                <R.Radio
                                    name="location-radio"
                                    value="2"
                                    text={ExportTypeI18Ns.get(
                                        ExportType.After6Month
                                    )}
                                    checked={selectSetting.LatestExportType === 2}
                                    onChange={onChangeLatestExportType}
                                />
                            </div>
                            <div className="radio-item">
                                <R.Radio
                                    name="location-radio"
                                    value="3"
                                    text={ExportTypeI18Ns.get(
                                        ExportType.After1Year
                                    )}
                                    checked={selectSetting.LatestExportType === 3}
                                    onChange={onChangeLatestExportType}
                                />
                            </div>
                            <div className="radio-item">
                                <R.Radio
                                    name="location-radio"
                                    value="4"
                                    text={ExportTypeI18Ns.get(
                                        ExportType.Custom
                                    )}
                                    checked={selectSetting.LatestExportType === 4}
                                    onChange={onChangeLatestExportType}
                                />
                                {showDatapicker && 
                                <div className="radio-item rangePicker">
                                    <R.Rangepicker
                                        selectedDate={_.isNil(startDate) || _.isNil(endDate) ? null : {
                                            start: startDate,
                                            end: endDate,
                                        }}
                                        data-part="vtWidget"
                                        width={374}
                                        dateTimeFormat={RM.TimeSettingModel.DateFormat}
                                        onChange={onChangeTimeRange}
                                        enableDates={{end : new Date()}}
                                    />
                                </div>}                                                         
                            </div>
                            <div className="radio-item">
                                <R.Radio
                                    name="location-radio"
                                    value="5"
                                    text={ExportTypeI18Ns.get(
                                        ExportType.All
                                    )}
                                    checked={selectSetting.LatestExportType === 5}
                                    onChange={onChangeLatestExportType}
                                />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={onHide} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_RDM_Explorer_ExportBarcode_DialogExportBtn} onClick={onExport} />
            </>
        </R.Dialog>
    );
};

export default ExportMLReport;