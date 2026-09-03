import { useEffect, useState, forwardRef, useImperativeHandle } from "react";
import {
    IntelligentTermStatus,
    AutoApplyStatus,
    TermFilterColumnType,
} from "../Config/Constains";
import MultipleChoiceColumn from "./Common/Component/MultipleChoiceColumn";
import DatetimeColumn from "./Common/Component/DateTimeColumn";

const IntelligentTermFilterForm = ({ filterColumnsParam }, ref) => {

    const [filterColumns, setFilterColumns] = useState({});

    const [intelligentTermList, setIntelligentTermList] = useState([]);
    const [reclassifyTermList, setReclassifyTermList] = useState([]);
    const [statusList, setStatusList] = useState([]);
    const [timeRangeList, setTimeRangeList] = useState([]);

    const [selectedIntelligentTermsValueList, setSelectedIntelligentValueList] = useState([]);
    const [selectedReclassifyTermValueList, setSelectedReclassifyTermValueList] = useState([]);
    const [selectedStatusList, setSelectedStatusList] = useState([]);

    useEffect(() => {
        setColumnOptionsValues(filterColumnsParam);
        setReclassificationTerms();
        setIntelligentTerms();
        setStatus();
    }, []);

    useImperativeHandle(ref, () => ({
        getColumns: () => {
            let filterParam = [];
            for (let key in filterColumns) {
                filterParam.push({
                    Column: key,
                    ColumnValues: filterColumns[key],
                });
            }
            return filterParam;
        },
        clearColumns: () => {
            setColumnOptionsValues([]);
        },
    }));

    const setStatus = async () => {
        var temps = [
            [0, RMResx.RM_ML_Report_ApprovalStatus_AutoApply],
            [1, RMResx.RM_ML_Report_ApprovalStatus_Waiting],
            [2, RMResx.RM_ML_Report_ApprovalStatus_Approved],
            [3, RMResx.RM_ML_Report_ApprovalStatus_Reclassify]
        ];
        setStatusList(new Map(temps));
    };

    const setIntelligentTerms = async () => {
        const requestOption = {
            url: "/api/TrainingReportApi/GetIntelligentClassificationFilter",
        };
        $$.loading(true);
        let result = await fetchUtility(requestOption);
        
        if (result?.length > 0) {
            setIntelligentTermList(new Map(result));
        }

        $$.loading(false);
    };

    const setReclassificationTerms = async () => {
        const requestOption = {
            url: "/api/TrainingReportApi/GetReclassificationFilter",
        };
        $$.loading(true);
        let result = await fetchUtility(requestOption);
        
        if (result?.length > 0) {
            setReclassifyTermList(new Map(result));
        }

        $$.loading(false);
    };

    const setColumnOptionsValues = (filterColumnsParam) => {
        let tempSelectedIntelligentTermValueList = [];
        let tempSelectedReclassifyTermValueList = [];
        let tempSelectedStatusList = [];
        let tempSelectedTimeList = [];
        let filterColumnsValues = [];
        if (filterColumnsParam.length > 0) {
            tempSelectedIntelligentTermValueList = filterColumnsParam.find(
                (item) => item.Column == TermFilterColumnType.IntelligentTerms
            )?.ColumnValues;
            tempSelectedReclassifyTermValueList = filterColumnsParam.find(
                (item) => item.Column == TermFilterColumnType.Reclassify
            )?.ColumnValues;
            tempSelectedStatusList = filterColumnsParam.find(
                (item) => item.Column == TermFilterColumnType.ApprovalStatus
            )?.ColumnValues;
            tempSelectedTimeList = filterColumnsParam.find(
                (item) => item.Column == TermFilterColumnType.PredictTime
            )?.ColumnValues;
            filterColumnsValues = getFilterColumns();
        }
        setSelectedIntelligentValueList(tempSelectedIntelligentTermValueList);
        setSelectedReclassifyTermValueList(tempSelectedReclassifyTermValueList);
        setSelectedStatusList(tempSelectedStatusList);
        setTimeRangeList(tempSelectedTimeList);
        setFilterColumns(filterColumnsValues);
    };

    const getFilterColumns = () => {
        let filterColumns = {};
        for (let item of filterColumnsParam) {
            filterColumns[item.Column] = item.ColumnValues;
        }
        return filterColumns;
    };

    const onChangeIntelligentTermsOption = (valueList) => {
        filterColumns[TermFilterColumnType.IntelligentTerms] = valueList;
        setSelectedIntelligentValueList(valueList);
    };

    const onChangeReclassifyTermOption = (valueList) => {
        filterColumns[TermFilterColumnType.Reclassify] = valueList;
        setSelectedReclassifyTermValueList(valueList);
    };

    const onChangeStatusOption = (valueList) => {
        filterColumns[TermFilterColumnType.ApprovalStatus] = valueList;
        setSelectedStatusList(valueList);
    };

    const onChangeTimeRange = (valueList) => {
        filterColumns[TermFilterColumnType.PredictTime] = valueList;
        setTimeRangeList(valueList);
    };

    return (
        <div>
            <DatetimeColumn
                label={RMResx.RM_MachineLearning_ReprotPredictTime}
                options={timeRangeList}
                onChange={onChangeTimeRange}
            />
            <MultipleChoiceColumn
                label={RMResx.RM_MachineLearning_ReprotIntelligentClassification}
                options={intelligentTermList}
                selectedOptionValueList={selectedIntelligentTermsValueList}
                onChange={onChangeIntelligentTermsOption}
            />
            <MultipleChoiceColumn
                label={RMResx.RM_MachineLearning_ReprotCurrentClassification}
                options={reclassifyTermList}
                selectedOptionValueList={selectedReclassifyTermValueList}
                onChange={onChangeReclassifyTermOption}
            />
            <MultipleChoiceColumn
                label={RMResx.RM_JS_JMD_Grid_ApprovalStatus}
                options={statusList}
                selectedOptionValueList={selectedStatusList}
                onChange={onChangeStatusOption}
            />
        </div>
    );
};

export default forwardRef(IntelligentTermFilterForm);
