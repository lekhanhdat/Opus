import React, { useState, useEffect, useRef  } from "react";
import { FilterOptions, FilterI18Ns } from "../Constants/index";
import { useStableCallback } from "../../../Common/Hooks/index";
import { Source } from "../Constants/Source";

import _ from "lodash";


const FilterOption = FilterOptions.FolderPath;
const AvailableContentSource = new Set([Source.OneDrive]);

const FolderPathFilter = ({ onFilterChange, onRemoveFilterChange, filterDefinitions ,enableFolderPath,  OnlyOneLocationDefinitions ,manualApprovalTab}) => {
    //设置不可用
    const [isDisable, setIsDisable] = useState(true);
    // 设置页数
    const [pageCount, setPageCount] = useState(0);
    //数据源
    const [contentSource, setContentSource] = useState(Source.None);
    // 设置下面的WorkSpace
    const [workSpaceSource, setWorkSpaceSource] = useState([]);
    // 设置选中的FolderPath
    const [selectedFolderPath, setSelectedFolderPath] = useState([]);
  
    const [totalpageCount, settotalpageCount] = useState(-1);

    let [continuation, setContinuation] = useState(null);
    useEffect(() => {   
        const initializeFilter = async () => {
            let source = Source.OneDrive;
            let sourceValue = [Source.OneDrive]; 
            if(filterDefinitions.has(FilterOptions.Source)) {
                sourceValue = JSON.parse(filterDefinitions.get(FilterOptions.Source).Value);
                source = sourceValue[0];
            }
            if (filterDefinitions.has(FilterOptions.Workspace)) {
                const workSpacePathValue = JSON.parse(filterDefinitions.get(FilterOptions.Workspace).Value).WorkspacePaths;    
                if(workSpacePathValue.length === 1 && AvailableContentSource.has(source)) {
                    setIsDisable(false);     
                    setWorkSpaceSource(workSpacePathValue);
                    setContentSource(source);
                    setPageCount(0);
                    if (filterDefinitions.has(FilterOption)) {
                        const value = filterDefinitions.get(FilterOption);
                        const workspacebeforevalue = JSON.parse(filterDefinitions.get(FilterOption).Value).WorkSpace;
                        const sourcebeforevalue = JSON.parse(filterDefinitions.get(FilterOption).Value).ContentSource;
                        setSelectedFolderPath(JSON.stringify(workspacebeforevalue) === JSON.stringify(workSpacePathValue) && sourcebeforevalue === source ? value.AttacheValue : []);
                        setWorkSpaceSource(sourcebeforevalue === source ? workSpacePathValue : []);
                    } else {
                        setSelectedFolderPath([]);
                    }
                    return;
                }
            }
            if(enableFolderPath) {  
                if(!OnlyOneLocationDefinitions && !filterDefinitions.has(FilterOptions.Workspace) && sourceValue.length === 1) {
                    setIsDisable(false);  
                    setContentSource(source);
                    setWorkSpaceSource([]);
                    if (filterDefinitions.has(FilterOption)) {
                        const value = filterDefinitions.get(FilterOption);
                        setSelectedFolderPath(value.AttacheValue); 
                    } else {
                        setSelectedFolderPath([]);
                    }
                    return;
                } else if(OnlyOneLocationDefinitions) {
                    setIsDisable(false);  
                    setContentSource(source);
                    setWorkSpaceSource([]);
                    if (filterDefinitions.has(FilterOption)) {
                        const value = filterDefinitions.get(FilterOption);
                        setSelectedFolderPath(value.AttacheValue); 
                    } else {
                        setSelectedFolderPath([]);
                    }
                    return;
                } else {
                    setIsDisable(true); 
                    setContentSource(source);
                    setSelectedFolderPath([]);
                    return; 
                }
            }
            setPageCount(0);
            setIsDisable(true);
            setContentSource(Source.None);
            setWorkSpaceSource([]);
            setSelectedFolderPath([]);  
            if (filterDefinitions.has(FilterOptions.Source)) {
                const sourceValue = JSON.parse(filterDefinitions.get(FilterOptions.Source).Value);
                setContentSource(sourceValue[0]);
            }
            if (filterDefinitions.has(FilterOption)) {
                onRemoveFilterChange(FilterOption);
            }
        };

        initializeFilter();
    }, [filterDefinitions]);


    const onChange = (args) => {
        const folderPathChange = _.cloneDeep(args.newValue);
      
       setSelectedFolderPath(folderPathChange);
        if (folderPathChange.length === 0) {
            onRemoveFilterChange(FilterOption);
            return;
        }
        var value = {
            FilterOption: FilterOption,
            Value: JSON.stringify({
                folderPathResults: folderPathChange.map(item => item.value),
                ContentSource: contentSource,
                WorkSpace : workSpaceSource
            }),

            AttacheValue: _.cloneDeep(folderPathChange)
        };

        onFilterChange(value);
    };
    
  
    const doLoad = useStableCallback(async (args) => {
        args.count = 15;
        const pageIndex =Math.floor(args.start / args.count);
        if(args.start == 0){
            continuation = null;
            setContinuation(null);
        }
        if ((pageIndex > 0 && pageIndex >= pageCount) || args.start ==  totalpageCount) {
            settotalpageCount(null);
            setContinuation(null);
            return [];
        } 
        var requestDefinition = {
            url: "/api/ManualApproval/QueryFolderPath",
            data: {
                SearchValue: args.key,
                WorkSpaceSource : workSpaceSource,
                ContentSource: contentSource,
                PageIndex: pageIndex,
                PageSize:  args.count,
                Continuation : continuation,
                ManualApprovalTab : manualApprovalTab
            }
        }; 
        const res = await fetchUtility(requestDefinition);
        const folderPathOptions = res.folderPathResults.map(item => ({
            name: item,
            value: item,
            checked: false,
        })).filter(item => !selectedFolderPath.some(selectedItem => selectedItem.value === item.value));

        setContinuation(res.continuation);
        if(folderPathOptions.length ==0 && pageIndex<pageCount && pageIndex!= 0){
            return await doLoad(args); 
         }
        if (pageIndex === 0) {
            setPageCount(Math.ceil(res.folderPathResultsCount/ args.count));
            settotalpageCount(res.folderPathResultsCount);
        }
        return folderPathOptions;
    });
    
    return (
        <div className="reco-manual-review-filter" style={{ marginTop: "-6px" }}>
            <div className="reco-manual-review-filter-title-haspopover">
                <div className="reco-manual-review-filter-flex">
                    <span tabIndex="0">
                        {
                            FilterI18Ns.get(FilterOption)
                        }
                    </span>
                    <$g.Popover>{RMResx.RM_JS_MA_FolderPath_Filter_Desc}</$g.Popover>
                </div>
            </div>
            <R.Multicombobox
                height={34}
                width={"100%"}
                checkedField="checked"
                textField="name"
                valueField="value"
                hasFilter={true}
                required={true}
                value={selectedFolderPath}
                items={selectedFolderPath}
                filter={true}
                onChange={onChange}
                doLoad={doLoad}
                disabled={isDisable}
                lazyStep={15}
            />
        </div>
    );
};



export default FolderPathFilter;