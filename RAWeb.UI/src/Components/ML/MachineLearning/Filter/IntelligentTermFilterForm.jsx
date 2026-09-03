import { useEffect, useState, forwardRef, useImperativeHandle } from "react";
import {
    IntelligentTermStatus,
    AutoApplyStatus,
    TermFilterColumnType,
} from "../Config/Constains";
import MultipleChoiceColumn from "./Common/Component/MultipleChoiceColumn";

const IntelligentTermFilterForm = ({ filterColumnsParam }, ref) => {
    
    const [filterColumns, setFilterColumns] = useState({});

    const [selectedStatusValueList, setSelectedStatusValueList] = useState([]);

    const [selectedAutoApplyValueList, setSelectedAutoApplyValueList] = useState([]);

    useEffect(() => {
        setColumnOptionsValues(filterColumnsParam);
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

    const setColumnOptionsValues = (filterColumnsParam) => {
        let selectedStatusValueList = [];
        let selectedAutoApplyValueList = [];
        let filterColumnsValues = [];
        if (filterColumnsParam.length > 0) {
            selectedStatusValueList = filterColumnsParam.find(
                (item) => item.Column == TermFilterColumnType.Status
            )?.ColumnValues;
            selectedAutoApplyValueList = filterColumnsParam.find(
                (item) => item.Column == TermFilterColumnType.AutoApply
            )?.ColumnValues;
            filterColumnsValues = getFilterColumns();
        }
        setSelectedStatusValueList(selectedStatusValueList);
        setSelectedAutoApplyValueList(selectedAutoApplyValueList);
        setFilterColumns(filterColumnsValues);
    };

    const getFilterColumns = () => {
        let filterColumns = {};
        for (let item of filterColumnsParam) {
            filterColumns[item.Column] = item.ColumnValues;
        }
        return filterColumns;
    };

    const onChangeStatusOption = (valueList) => {
        filterColumns[TermFilterColumnType.Status] = valueList;
        setSelectedStatusValueList(valueList);
    };

    const onChangeAutoApplyOption = (valueList) => {
        filterColumns[TermFilterColumnType.AutoApply] = valueList;
        setSelectedAutoApplyValueList(valueList);
    };

    return (
        <div>
            <MultipleChoiceColumn
                label={RMResx.RM_ML_IT_Column_Status}
                options={IntelligentTermStatus}
                selectedOptionValueList={selectedStatusValueList}
                onChange={onChangeStatusOption}
            />
            <MultipleChoiceColumn
                label={RMResx.RM_ML_IT_Column_AutoApply}
                options={AutoApplyStatus}
                selectedOptionValueList={selectedAutoApplyValueList}
                onChange={onChangeAutoApplyOption}
            />
        </div>
    );
};

export default forwardRef(IntelligentTermFilterForm);
