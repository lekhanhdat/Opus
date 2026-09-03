import React, { useState, useEffect,useRef } from "react";
import { FilterOptions, FilterI18Ns } from "../Constants/index";
import { Source } from "../Constants/Source";
import { useStableCallback } from "../../../Common/Hooks/index";
import _ from "lodash";

const FilterOption = FilterOptions.Workspace;
const AvailableContentSource = new Set([Source.SharePoint, Source.OneDrive, Source.Teams]);

const WorkspaceFilter = ({ onFilterChange, onRemoveFilterChange, filterDefinitions,enableFolderPath }) => {

    const [isDisable, setIsDisable] = useState(true);

    const [contentSource, setContentSource] = useState(Source.None);

    const [pageCount, setPageCount] = useState(0);

    const [selectedWorkspaces, setSelectedWorkspaces] = useState([]);

    useEffect(() => {
        if (filterDefinitions.has(FilterOptions.Source)) {
            const sourceValue = JSON.parse(filterDefinitions.get(FilterOptions.Source).Value);
            if (sourceValue.length === 1 && AvailableContentSource.has(sourceValue[0])) {
                const source = sourceValue[0];
                setIsDisable(!AvailableContentSource.has(source));
                setContentSource(source);
                setPageCount(0);

                if (filterDefinitions.has(FilterOption)) {
                    const value = filterDefinitions.get(FilterOption);
                    const beforeSource = JSON.parse(value.Value).ContentSource;
                    setSelectedWorkspaces(source === beforeSource ? value.AttacheValue : []);
                }else{
                    setSelectedWorkspaces([]);
                }
                return;
            }
        }
        else  //是不是特殊的review
        {
            if(enableFolderPath ) 
            {
                setIsDisable(false);
                setContentSource(Source.OneDrive);
                setPageCount(0);
                if (filterDefinitions.has(FilterOption)) {
                    const value = filterDefinitions.get(FilterOption);
                    setSelectedWorkspaces(value.AttacheValue);
                }
                else
                {
                    setSelectedWorkspaces([]);
                }
                return
            }
        }
        setPageCount(0);
        setIsDisable(true);
        setContentSource(Source.None);
        setSelectedWorkspaces([]);
        if (filterDefinitions.has(FilterOption)) {
            onRemoveFilterChange(FilterOption);
        }

    }, [filterDefinitions]);

    const onChange = (args) => {
        const workspaces = _.cloneDeep(args.newValue);
        setSelectedWorkspaces(workspaces);
        if (workspaces.length === 0) {
            onRemoveFilterChange(FilterOption);
            return;
        }

        if (filterDefinitions.has(FilterOptions.FolderPath)){
            onRemoveFilterChange(FilterOptions.FolderPath);
        }

        var value = {
            FilterOption: FilterOption,
            Value: JSON.stringify({
                WorkspaceIds: workspaces.map(item => item.workspaceId),
                WorkspacePaths: workspaces.map(item => item.workspacePath),
                ContentSource: contentSource,
            }),
            AttacheValue: _.cloneDeep(workspaces)
        };
        onFilterChange(value);
    };

    const doLoad = useStableCallback(async (args) => {
        args.count = 15;
        const pageIndex = (args.start / args.count) >>> 0;
        if (pageIndex > 0 && pageIndex >= pageCount) {
            return [];
        }
        var requestDefinition = {
            url: "/api/ManualApproval/QueryWorkspaces",
            data: {
                SearchValue: args.key,
                ContentSource: contentSource,
                PageIndex: pageIndex,
                PageSize: args.count,
            }
        };
        const res = await fetchUtility(requestDefinition);
        
        res.workspaceItems = res.workspaceItems.filter(item => !selectedWorkspaces.some(selectedItem => selectedItem.workspacePath === item.workspacePath));

        if (pageIndex === 0) {
            setPageCount((res.workspaceCount + args.count - 1) / args.count >>> 0);
        }
        return res.workspaceItems;
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
                    <$g.Popover>{RMResx.RM_JS_MA_Location_Filter_Desc_RD}</$g.Popover>
                </div>
            </div>
            <R.Multicombobox
                height={34}
                width={"100%"}
                checkedField="checked"
                textField="workspacePath"
                valueField="workspaceId"
                hasFilter={true}
                required={true}
                value={selectedWorkspaces}
                filter={true}
                onChange={onChange}
                doLoad={doLoad}
                disabled={isDisable}
                lazyStep={15}
            />
        </div>
    );
};

export default WorkspaceFilter;