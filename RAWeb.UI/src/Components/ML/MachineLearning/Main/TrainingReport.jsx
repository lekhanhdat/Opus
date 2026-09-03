import React, { useEffect, useState, useRef } from "react";
import _ from "lodash";
import SearchBox from "../Search/SearchBox";
import Filter from '../Search/Filter';
import Table from "../Table/Index";
import Actions from "../Actions/TrainingReportAction";
import TrainingReportFilterForm  from '../Filter/TrainingReportFilterForm';
import RowTemplate from "../RowTemplate/TrainingReportTemplate";
import { TrainingReportTableColumns } from "../Config/TableColumnsConfig";
import ContinuationTokenPager from "../../../Common/ContinuationTokenPager";


const TrainingReport = () =>{

    const tokenPagerRef = useRef();

    const [ trainingReport, setTrainingReport ] = useState([]);
    const [ totalCount, setTotalCount ] = useState(0);

    const [ searchValue, setSearchValue] = useState("");

    const [ filterOptions, setFilterOptions] = useState([]);

    const [ sortInfo, setSortInfo ] =  useState({ SortBy: "", IsAscending: false});

    const tableRef = useRef();

    const [continuationToken, setContinuationToken] = useState("");

    const [pagerSize, setPagerSize] = useState(10);

    useEffect(() => {
        loadTrainingReports();
    }, [searchValue, filterOptions, sortInfo]);

    const loadTrainingReports = async(continuationToken, currentPagerSize) => {
        const requestOption = {   
            url: "/api/TrainingReportApi/Query",
            data: {     
                PageSize: currentPagerSize || pagerSize,
                PageIndex: continuationToken || "",      
                SearchValue: searchValue,
                SortBy: sortInfo.SortBy,
                IsAscending: sortInfo.IsAscending,
                Filters: filterOptions
            }
        };
        $$.loading(true);
        let result = await fetchUtility(requestOption);
        let trainingReports = result.TrainingReports;
        $$.loading(false);
        if (_.isUndefined(continuationToken)) {
            tokenPagerRef.current.reset();
        }
        setContinuationToken(result.PageIndex);
        setTotalCount(result.TotalCount); 
        setTrainingReport(trainingReports || []);
    };

    
    const onPagerChange = (continuationToken, pagerSize)=> {
        setPagerSize(pagerSize);
        loadTrainingReports(continuationToken, pagerSize);
    };
    const onSearch = (value) => {
        setSearchValue(value);
    };

    const onFilter = (filterOptions) =>{
        setFilterOptions(filterOptions);
    };

    const onSort = (isAsc, sortColumn) => {
        setSortInfo({
            SortBy: sortColumn, 
            IsAscending: isAsc
        });
    };

    const renderActions = () =>{
        return <Actions/>;
    };

    return <div className="ra-page-container">
        <div className="ra-main-header">
            <SearchBox
                onSearch={onSearch}
                placeholder={RMResx.RM_ML_TS_Search_Placeholder}
            />
            <Filter 
                onFilter={onFilter} 
                FilterForm={TrainingReportFilterForm} 
            />
        </div>
        <div className="padding-inline-l">
            <Table
                ref={tableRef}
                checkable={false}
                columns={TrainingReportTableColumns} 
                actionComponent={renderActions()}
                items={trainingReport}        
                template={RowTemplate}
                onSort={onSort}
            />
        </div>
        <div className="ra-main-footer">
            <ContinuationTokenPager
                ref={tokenPagerRef}
                totalCount={totalCount}
                showPagerCounter={true}
                shownCount={trainingReport.length}
                showPagerSize={true}
                continuationToken={continuationToken}
                pagerSizeOptions={[5, 10, 15, 50]}
                onChange={onPagerChange}
            />
        </div>
    </div>;
};

export default TrainingReport;