import React, { useEffect, useState, useRef, use } from "react";
import _ from "lodash";
import SearchBox from "../Search/SearchBox";
import Filter from '../Search/Filter';
import Table from "../Table/Index";
import ContinuationTokenPager from "../../../Common/ContinuationTokenPager";
import RowTemplate from "../RowTemplate/TrainingScopeTemplate";
import TrainingScopeFilterForm  from '../Filter/TrainingScopeFilterForm';
import { TrainingScopeTableColumns } from "../Config/TableColumnsConfig";
import { DocumnetFilterColumnType, MTSSourceFlag, TrainingMode } from "../Config/Constains";
import TrainingScopeActions from "../Actions/TrainingScopeActions";
import ManageTrainingScope from "../Actions/TrainingScopes/ManageScope";
import AddTrainingScope from "../Actions/TrainingScopes/AddScope";

const TrainingScope = ({termId}) =>{

    const defaultFilterOptions = [{
        Column: DocumnetFilterColumnType.Classification,  
        ColumnValues: [termId]
    }];

    const tokenPagerRef = useRef();

    const [trainingScopeInfo, setTrainingScopeInfo] = useState({
        locationId: "",
        location: "",
        sourceFlag: MTSSourceFlag.SPO,
        trainingScopeOption: TrainingMode.Auto,
    });

    const [ trainingScope, setTrainingScope ] = useState([]);

    const [selectedItems, setSelectedItems] = useState([]);

    const [ totalCount, setTotalCount ] = useState(0);

    const [ searchValue, setSearchValue] = useState("");

    const [ filterOptions, setFilterOptions] = useState(termId ? defaultFilterOptions  : []);

    const [ sortInfo, setSortInfo ] =  useState({ SortBy: "", IsAscending: false});

    const [continuationToken, setContinuationToken] = useState("");

    const [pagerSize, setPagerSize] = useState(10);

    const manageScopeRef = useRef(null);

    const addScopeRef = useRef(null);

    useEffect(()=>{
        loadTrainingScope();
    },[searchValue, filterOptions, sortInfo]);

    useEffect(() => {
        loadTrainingScopeOption();
    }, []);

    const loadTrainingScope = async(continuationToken, currentPagerSize) => {
        const requestOption = {   
            url: "/api/TrainingScopeApi/Query",
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
        let trainingScopes = result.TrainingScopes;
        $$.loading(false);
        if(_.isUndefined(continuationToken)){
            tokenPagerRef.current.reset();
        }
        setContinuationToken(result.PageIndex);
        setTotalCount(result.TotalCount); 
        setTrainingScope(trainingScopes || []);
    };  

    const loadTrainingScopeOption = async() => {
        const requestOption = {   
            url: "/api/TrainingScopeApi/GetTrainingScopeOption",
            method: "GET",
        };
        $$.loading(true);
        const res = await fetchUtility(requestOption);
        $$.loading(false);
        if (res) {
            setTrainingScopeInfo({
                locationId: res.LocationId,
                location: res.Location,
                sourceFlag: res.SourceFlag,
                trainingScopeOption: res.TrainingScopeOption,
            });
        }
    };

    const onPagerChange = (continuationToken, pagerSize)=> {
        setPagerSize(pagerSize);
        loadTrainingScope(continuationToken, pagerSize);
    };
 
    const onSearch = (value) =>{
        setSearchValue(value);
    };

    const onFilter = (filterOptions) => {
        setFilterOptions(filterOptions);
    };

    const onSort = (isAsc, sortColumn) => {
        setSortInfo({
            SortBy: sortColumn, 
            IsAscending: isAsc
        });
    };

    const doAction = (actionType) => {
        switch(actionType) {
            case "OPEN_MANAGE_SCOPE_PANEL":
                loadTrainingScopeOption().then(() => {
                    manageScopeRef.current.openPanel();
                });
                break;
            case "OPEN_ADD_SCOPE_PANEL":
                addScopeRef.current.openPanel();
                break;
            case "REFRESH":
                loadTrainingScopeOption();
                break;
            default:
                loadTrainingScope();
                break;
        }
    }

    const renderActions = () =>{
        return (
            <TrainingScopeActions
                doAction={doAction}
                trainingScopeInfo={trainingScopeInfo}
                selectedItems={selectedItems}
            />
        );
    };

    return <div className="ra-page-container">
        <div className="ra-main-header">
            <SearchBox onSearch={onSearch} placeholder={RMResx.RM_ML_TS_Search_Placeholder}/>
            <Filter 
                onFilter={onFilter} 
                FilterForm={TrainingScopeFilterForm} 
                filterParam={filterOptions}
            />
        </div>
        <div className="padding-inline-l">
            <Table
                checkable={trainingScopeInfo.trainingScopeOption === TrainingMode.Manual}
                columns={TrainingScopeTableColumns} 
                items={trainingScope}        
                template={RowTemplate}
                actionComponent={renderActions()}
                onSort={onSort}
                onSelect={setSelectedItems}
            />
        </div>
        <div className="ra-main-footer">
            <ContinuationTokenPager
                ref={tokenPagerRef}
                totalCount={totalCount}
                showPagerCounter={true}
                shownCount={trainingScope.length}
                showPagerSize={true}
                continuationToken={continuationToken}
                pagerSizeOptions={[5, 10, 15, 50]}
                onChange={onPagerChange}
            />
        </div>
        <ManageTrainingScope
            ref={manageScopeRef}
            trainingScopeInfo={trainingScopeInfo}
            doAction={doAction}
        />
        <AddTrainingScope
            ref={addScopeRef}
            doAction={doAction}
        />
    </div>;
};

export default TrainingScope;
