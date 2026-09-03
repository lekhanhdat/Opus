import React, { useEffect, useState, useRef } from "react";
import PropTypes from "prop-types";
import "./index.less";

import { SourceFlag, DateRange } from "../Common/Constants";
import { DateRangeI18N } from "../Common/I18N";
import EmptyContent from "../Components/EmptyContent/index";
import CacheUtility from "../Common/CacheUtility";

const DateRangeSelectorItems = [
    {
        checked: true,
        name: DateRangeI18N.get(DateRange.Last12Month),
        value: DateRange.Last12Month,
        disabled: false,
    },
    {
        checked: false,
        name: DateRangeI18N.get(DateRange.Last10Week),
        value: DateRange.Last10Week,
        disabled: false
    },
    {
        checked: false,
        name: DateRangeI18N.get(DateRange.Last10Day),
        value: DateRange.Last10Day,
        disabled: false
    },
];

const DatasRequestOption = (sourceFlag, dateRange) => ({
    url: "/api/Dashboard/GetLineChartItems",
    method: "Post",
    data: { Flag: sourceFlag, DateRange: dateRange }
});

const Colors = [
    "#24BCA4",
    "#F7A100",
    "#41AEE8"
];

const CachePrevKey = "RCBS";

const RecordCountByStatus = ({ sourceFlags }) => {

    const cardRef = useRef();

    const [selectedSourceFlag, setSelectedSourceFlag] = useState(SourceFlag.None);

    const [selectedDateRange, setSelectedDateRange] = useState(DateRange.Last12Month);

    const [datas, setDatas] = useState([]);

    const [lineChartRightPadding, setLineChartRightPadding] = useState(32);

    useEffect(() => {
        if(window.navigator.userAgent.indexOf("Firefox") !== -1) {
            const widthListen = () => {
                const canvasDom = cardRef.current.children[0].children[0].children[0].children[0];
                canvasDom.style.width = 0;
            };
    
            window.addEventListener("resize", widthListen);
    
            return () => {
                window.removeEventListener("resize", widthListen);
            };
        }
    }, []);

    useEffect(() => {
        const setInitialSourceFlag = async () => {
            if (sourceFlags.length === 0) {
                return;
            }
            setSelectedSourceFlag(sourceFlags[0].value);
        };
        setInitialSourceFlag();
    }, [sourceFlags]);

    useEffect(() => {
        const fetchData = async () => {
            if (selectedSourceFlag === SourceFlag.None) {
                return;
            }
            setDatas([]);
            let responseData = [];
            const cacheKey = generatorCacheKey();
            if (CacheUtility.Instance.has(cacheKey)) {
                responseData = CacheUtility.Instance.get(cacheKey);
            } else {
                responseData = await fetchUtility(DatasRequestOption(selectedSourceFlag, selectedDateRange));
                CacheUtility.Instance.set(cacheKey, responseData);
            }
            const maxNumber = responseData.reduce((prev, cur) => Math.max(prev, cur.value), 0);
            const maxNumberStr = maxNumber.toString();
            const paddingDist = (maxNumberStr.length - 2) * 8;
            setLineChartRightPadding(32 + (paddingDist > 0 ? paddingDist : 0));
            setDatas(responseData);
        };

        fetchData();
    }, [selectedSourceFlag, selectedDateRange]);

    const generatorCacheKey = () => {
        return `${CachePrevKey}-"SourceFlag"-${selectedSourceFlag}-"Date"-${selectedDateRange}`;
    };

    const checkIsEmpty = () => {
        return datas.reduce((prev, cur) => prev + cur.value, 0) === 0;
    };

    const formattedChartItems = datas.map(d => ({
        name: d.date,
        value: d.value,
        group: d.name
    }));

    return (
        <div className="reco-dashboard-rcbs-wrapper reco-dashboard-card">
            <div className="reco-dashboard-rcbs-top-section">
                <div className="reco-dashboard-card-title" style={{ marginBottom: 0 }} tabIndex="0">
                    {RMResx.RM_DSB_Timeline_Title}
                </div>
                <div className="reco-dashboard-rcbs-selectors">
                    <div className="reco-dashboard-rcbs-source-selector">
                        <R.Combobox
                            width="100%"
                            height="100%"
                            items={sourceFlags}
                            disabled={false}
                            textField="name"
                            valueField="value"
                            searchable={false}
                            linkMode={false}
                            excludeChecked={true}
                            mini={true}
                            onChange={(args) => setSelectedSourceFlag(args.newValue.value)}
                        />
                    </div>
                    <div className="reco-dashboard-rcbs-datarange-selector">
                        <R.Combobox
                            width="100%"
                            height="100%"
                            items={DateRangeSelectorItems}
                            disabled={false}
                            textField="name"
                            valueField="value"
                            searchable={false}
                            linkMode={false}
                            excludeChecked={true}
                            mini={true}
                            onChange={(args) => setSelectedDateRange(args.newValue.value)}
                        />
                    </div>
                </div>
            </div>
            <EmptyContent isEmpty={checkIsEmpty()}>
                <div className="reco-dashboard-rcbs-status-section">
                    <div className="reco-dashboard-rcbs-status" tabIndex="0">
                        <span className="reco-dashboard-rcbs-status-color" style={{ backgroundColor: "#24BCA4" }}></span>
                        {RMResx.RM_DSB_Created}
                    </div>
                    <div className="reco-dashboard-rcbs-status" tabIndex="0">
                        <span className="reco-dashboard-rcbs-status-color" style={{ backgroundColor: "#F7A100" }}></span>
                        {RMResx.RM_DSB_Destroyed}
                    </div>
                    <div className="reco-dashboard-rcbs-status" tabIndex="0">
                        <span className="reco-dashboard-rcbs-status-color" style={{ backgroundColor: "#41AEE8" }}></span>
                        {RMResx.RM_DSB_Approval}
                    </div>
                </div>
                <div className="reco-dashboard-rcbs-line-chart-section" ref={cardRef}>
                    <R.Charts>
                        <R.Charts.Grid 
                            type="line" 
                            items={formattedChartItems} 
                            color={Colors} 
                            showPoint={true}
                        />
                    </R.Charts>
                </div>
            </EmptyContent>
        </div>
    );
};

RecordCountByStatus.propTypes = {
    sourceFlags: PropTypes.array
};


export default RecordCountByStatus;