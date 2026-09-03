import { useEffect, useState, forwardRef, useImperativeHandle } from "react";
import {
    TrainingScopeStatus,
    DocumnetFilterColumnType,
} from "../Config/Constains";
import MultipleChoiceColumn from "./Common/Component/MultipleChoiceColumn";

const TrainingScopeFilterForm = ({ filterColumnsParam }, ref) => {
    
    const [filterColumns, setFilterColumns] = useState({});

    const [selectedStatusValueList, setSelectedStatusValueList] = useState([]);

    const [termList, setTermList] = useState([]);

    const [selectedTermValueList, setSelectedTermValueList] = useState([]);

    useEffect(() => {
        setColumnOptionsValues(filterColumnsParam);
        setMLTerms();
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

    const setMLTerms = async() => {
        const requestOption = {   
            url: "/api/TrainingScopeApi/MLTermFilters",
        };
        $$.loading(true);
        let result = await fetchUtility(requestOption);
        $$.loading(false);
        setMLTermsOptions(result);
    };

    const setMLTermsOptions = (termList) => {
        if(termList?.length > 0){
            let termMapList = [];
            for(let item of termList){
                termMapList.push( [item.Id, item.Name]);
            }
            setTermList(new Map(termMapList));
        }
    };

    const setColumnOptionsValues = (filterColumnsParam) => {
        let selectedStatusValueList = [];
        let selectedTermValueList = [];
        let filterColumnsValues = [];
        if (filterColumnsParam.length > 0) {
            selectedStatusValueList = filterColumnsParam.find(
                (item) => item.Column == DocumnetFilterColumnType.Status
            )?.ColumnValues;
            selectedTermValueList = filterColumnsParam.find(
                (item) => item.Column == DocumnetFilterColumnType.Classification
            )?.ColumnValues;
            filterColumnsValues = getFilterColumns();
        }
        setSelectedStatusValueList(selectedStatusValueList);
        setSelectedTermValueList(selectedTermValueList);
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
        filterColumns[DocumnetFilterColumnType.Status] = valueList;
        setSelectedStatusValueList(valueList);
    };

    const onChangeTermOption = (valueList) => {
        filterColumns[DocumnetFilterColumnType.Classification] = valueList;
        setSelectedTermValueList(valueList);
    };

    return (
        <div>
            <MultipleChoiceColumn
                label={RMResx.RM_ML_TS_Column_Status}
                options={TrainingScopeStatus}
                selectedOptionValueList={selectedStatusValueList}
                onChange={onChangeStatusOption}
            />
            <MultipleChoiceColumn
                label={RMResx.RM_ML_TS_Column_Classification}
                options={termList}
                searchable={true}
                selectedOptionValueList={selectedTermValueList}
                onChange={onChangeTermOption}
            />
        </div>
    );
};

export default forwardRef(TrainingScopeFilterForm);
