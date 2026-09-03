import React, { useEffect, useState } from "react";
import _ from "lodash";
import { ExportType, ExportTypeI18Ns } from "../Constants/index.js";


const HistorActionPanel = ({show, onHide, onChange, onSave, Setting}) => {

    const todayDate = RM.TimeUtil.getTodayStartEndTime();

    const [selectSetting, setSelectSetting] = useState(Setting);

    const [showDatapicker,setShowDatapicker] = useState(false);

    const [startDate, setStartDate] = useState(todayDate.start);

    const [endDate, setEndDate] = useState(todayDate.end);

    useEffect(() => {
        setSelectSetting(Setting);
    }, [Setting]);

    const onChangeLatestExportType = (args) => {
        const clonedSetting = _.cloneDeep(Setting);
        clonedSetting.LatestExportType = Number(args);
        if(Number(args) === ExportType.Custom){
            setShowDatapicker(true);
            clonedSetting.CustomDate = {
                StartDateTime:RM.TimeUtil.getCommonDateStr(startDate),
                EndDateTime:RM.TimeUtil.getCommonDateStr(endDate)
            };
            onChange(clonedSetting);
            setSelectSetting(clonedSetting);
            return;
        }
        setShowDatapicker(false);
        clonedSetting.CustomDate = {
            StartDateTime:RM.TimeUtil.getCommonDateStr(new Date(0)) + ":0",
            EndDateTime:RM.TimeUtil.getCommonDateStr(new Date(0)) + ":59"
        };
        onChange(clonedSetting);
        setSelectSetting(clonedSetting);
    };

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
        const clonedSetting = _.cloneDeep(Setting);
        clonedSetting.LatestExportType = ExportType.Custom;
        clonedSetting.CustomDate = {
            StartDateTime:RM.TimeUtil.getCommonDateStr(startDate) + ":0",
            EndDateTime:RM.TimeUtil.getCommonDateStr(endDate) + ":59",
        };
        onChange(clonedSetting);
        setSelectSetting(clonedSetting);
    };

    const onSaveSetting = () =>{
        onSave(selectSetting);
    };

    return (
        <R.Dialog
            id="exportBarcodesContainer"
            header={RMResx.RM_MA_ExportToHistory}
            width={464}
            status={{ show: show }}
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
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_RDM_Explorer_ExportBarcode_DialogExportBtn} onClick={onSaveSetting} />
            </>
        </R.Dialog>
    );
};

export default HistorActionPanel;