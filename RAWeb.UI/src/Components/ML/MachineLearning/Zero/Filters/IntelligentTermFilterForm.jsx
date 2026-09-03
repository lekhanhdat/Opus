import { useEffect, useState, forwardRef, useImperativeHandle } from "react";

import { AutoApplyStatus, TermFilterColumnType } from "../Config/Constants";
import MultipleChoiceColumn from "../../Filter/Common/Component/MultipleChoiceColumn";

const IntelligentTermFilterForm = ({ filterColumnsParam }, ref) => {
    const [filterColumns, setFilterColumns] = useState({});

    const [selectedAutoApplyValueList, setSelectedAutoApplyValueList] =
        useState([]);

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
        let selectedAutoApplyValueList = [];
        let filterColumnsValues = [];
        if (filterColumnsParam.length > 0) {
            selectedAutoApplyValueList = filterColumnsParam.find(
                (item) => item.Column == TermFilterColumnType.AutoApply
            )?.ColumnValues;
            filterColumnsValues = getFilterColumns();
        }
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

    const onChangeAutoApplyOption = (valueList) => {
        filterColumns[TermFilterColumnType.AutoApply] = valueList;
        setSelectedAutoApplyValueList(valueList);
    };

    return (
        <div>
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
