import React, { useState, useEffect } from "react";
import { FilterOptions, FilterI18Ns } from "../Constants/index";
import { Source } from "../Constants/index";
import { useStableCallback } from "../../../Common/Hooks/index";
import _ from "lodash";
import { LicenseHelper } from "../../../../Utilities/CommonUtil";
import { EmptyGUID } from "../../../../Constants/Constants";

const FilterOption = FilterOptions.MLWorkspace;
const AvailableContentSource = new Set([
    Source.SharePoint,
    Source.OneDrive,
    Source.Teams,
    Source.GoogleDrive,
]);

const WorkspaceFilter = ({ onFilterChange, onRemoveFilterChange, filterDefinitions }) => {

    const [isDisable, setIsDisable] = useState(true);

    const [contentSource, setContentSource] = useState(Source.None);

    const [pageCount, setPageCount] = useState(0);

    const [selectedWorkspaces, setSelectedWorkspaces] = useState([]);

    const hasOnlyGoogleSource = LicenseHelper.HasOpusGoogleLicenseOnly();

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
                }
                return;
            }
        } else if (hasOnlyGoogleSource) {
            setIsDisable(false);
            setContentSource(Source.GoogleDrive);
            setPageCount(0);

            if (filterDefinitions.has(FilterOption)) {
                const value = filterDefinitions.get(FilterOption);
                const beforeSource = JSON.parse(value.Value).ContentSource;
                setSelectedWorkspaces(beforeSource === Source.GoogleDrive ? value.AttacheValue : []);
            }
            return;
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
        var value = {
            FilterOption: FilterOption,
            Value: JSON.stringify({
                WorkspaceIds: workspaces.map(item => item.workspaceId),
                WorkspacePaths: workspaces.map(item => item.workspacePath),
                ContentSource: contentSource,
            }),
            AttacheValue: _.cloneDeep(workspaces)
        };
        if (contentSource === Source.GoogleDrive) {
            value = {
                ...value,
                Value: JSON.stringify({
                    WorkspacePaths: workspaces.map(
                        (item) => item.workspacePath
                    ),
                    WorkspaceIds: workspaces.map(() => EmptyGUID),
                    Extentions: workspaces.map((item) => item.extention),
                    ContentSource: contentSource,
                }),
                AttacheValue: _.cloneDeep(workspaces),
            };
        }
        onFilterChange(value);
    };

    const doLoad = useStableCallback(async (args) => {
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
        if (pageIndex === 0) {
            setPageCount((res.workspaceCount + args.count - 1) / args.count >>> 0);
        }
        if (contentSource === Source.GoogleDrive && res.workspaceItems.length) {
            res.workspaceItems = res.workspaceItems.map((item) => ({
                workspaceId: item.extention,
                workspacePath: item.workspacePath,
                extention: item.extention,
            }));
        }
        return res.workspaceItems;
    });

    return (
        <div className="reco-manual-review-filter">
            <div className="reco-manual-review-filter-title">
                <div className="reco-manual-review-filter-flex">
                    {
                        FilterI18Ns.get(FilterOption)
                    }
                    <$g.Popover>{RMResx.RM_JS_MA_Location_Filter_Desc}</$g.Popover>
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