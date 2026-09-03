import { forwardRef, useImperativeHandle, useRef, useState } from "react";
import _ from "lodash";

const ReturnHistoryFilterPanel = ({ onFilter }, ref) => {
    const [isShow, setPanelIsShow] = useState(false);

    const [startDate, setStartDate] = useState(null);

    const [endDate, setEndDate] = useState(null);

    const [filterOptions, setFilterOptions] = useState({});

    const dateRef = useRef(null);

    useImperativeHandle(ref, () => ({
        openPanel: (filterParam) => {
            setPanelIsShow(true);
            setFilterOptions(filterParam);
            if (Object.values(filterParam)) {
                dateRef.current = filterParam;
                setStartDate(new Date(filterParam.StartTime));
                setEndDate(new Date(filterParam.EndTime));
            }
        },
        getFilterOptions: () => {
            return filterOptions;
        },
    }));

    const onClosePanel = () => {
        setPanelIsShow(false);
        const dateObj = dateRef.current;
        if (dateObj && Object.values(dateObj)) {
            const { startDate, endDate } = dateObj;
            setStartDate(startDate);
            setEndDate(endDate);
        }
    };

    const onClickFiltePickList = () => {
        if (_.isNil(startDate) && _.isNil(endDate)) {
            dateRef.current = null;
        }
        onFilter();
        onClosePanel();
    };

    const onClearFilter = () => {
        setFilterOptions({});
        setStartDate(null);
        setEndDate(null);
    };

    const onChange = (args) => {
        if (_.isNil(args.newValue)) {
            onClearFilter();
            return;
        }
        const startDate = args.newValue.start;
        const endDate = args.newValue.end;
        setStartDate(startDate);
        setEndDate(endDate);
        const dateValue = {
            StartTime: RM.TimeUtil.getCommonDateStr(startDate),
            EndTime: RM.TimeUtil.getCommonDateStr(endDate),
        };
        dateRef.current = dateValue;
        setFilterOptions(dateValue);
    };

    const renderFilterForm = () => {
        return (
            <div>
                <div className="ra-flex-justify-end">
                    <a
                        className="ra-main-filter-clear fia-funnel-clear"
                        tabIndex="0"
                        role="button"
                        onClick={onClearFilter}
                        onKeyDown={(e)=>{
                            if (e.key === "Enter") {
                                onClearFilter();
                            }
                        }}
                    >
                        {" "}
                        {RMResx.RM_Common_ClearFilter}
                    </a>
                </div>
                <$g.FormRow
                    label={RMResx.RM_MT_ReturnHistory_Filter_ReturnTime}
                >
                    <R.Rangepicker
                        selectedDate={
                            _.isNil(startDate) || _.isNil(endDate)
                                ? null
                                : {
                                      start: startDate,
                                      end: endDate,
                                  }
                        }
                        data-part="vtWidget"
                        width={"100%"}
                        dateTimeFormat={RM.TimeSettingModel.DateFormat}
                        onChange={onChange}
                        clearable={true}
                    />
                </$g.FormRow>
            </div>
        );
    };

    return (
        <R.Panel
            header={RMResx.RM_Common_Filter}
            size={664}
            status={{ show: isShow }}
            destroy={true}
            onHide={onClosePanel}
        >
            {renderFilterForm()}
            <>
                <R.Button
                    slot="buttons"
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={onClosePanel}
                />
                <R.Button
                    slot="buttons"
                    primary
                    classify="theme"
                    text={RMResx.RM_JS_Common_Save}
                    onClick={onClickFiltePickList}
                />
            </>
        </R.Panel>
    );
};

export default forwardRef(ReturnHistoryFilterPanel);
