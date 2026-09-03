import React, { useState, useImperativeHandle, forwardRef, useRef, useEffect } from "react";
import _ from "lodash";
import {
    CollectionTimeFilter,
    CreatedByFilter,
    EscalatedFromFilter,
    ModifiedByFilter,
    RecordReviewerFilter,
    SourceFilter,
    WorkspaceFilter,
    DateModifiedFilter,
    DateCreatedFilter,
    ClassificationFilter,
} from "../Filters/index";

import { AvaiableOptions } from "../Constants/AvailableOptions";
import { Source } from "../Constants/Source";
import { LicenseHelper } from "../../../../Utilities/CommonUtil";

const UnderReviewFilterPanel = ({ onFilter, filterAvailableOptions }, ref) => {

    const filterDefinitionsRef = useRef(new Map());

    const [showPanel, setShowPanel] = useState(false);

    const [filterDefinitions, setFilterDefinitions] = useState(new Map());

    const [cachedFilterDefinitions, setcachedFilterDefinitions] = useState(new Map());

    const [classificationOptions, setClassificationOptions] = useState(new Map());

    useImperativeHandle(ref, () => ({
        showPanel: () => {
            setShowPanel(true);
            filterDefinitionsRef.current = _.cloneDeep(cachedFilterDefinitions);
            setFilterDefinitions(_.cloneDeep(cachedFilterDefinitions));
        },
    }));

    useEffect(() => {
        getClassificationOptions();
    }, [])

    const onResetFilters = () => {
        setFilterDefinitions(new Map());
        filterDefinitionsRef.current = new Map();
    };

    const onHidePanel = () => {
        setShowPanel(false);
    };

    const updateFilterDefinitionRef = (definitions) => {
        const result = [];
        for (const keyValue of _.cloneDeep(definitions)) {
            result.push(keyValue[1]);
        }
        return result;
    };

    const onFilterSave = () => {
        setcachedFilterDefinitions(_.cloneDeep(filterDefinitionsRef.current));
        onFilter(updateFilterDefinitionRef(filterDefinitionsRef.current));
        setShowPanel(false);
    };

    const onFilterChange = (filterValue) => {
        filterDefinitionsRef.current.set(filterValue.FilterOption, filterValue);
        setFilterDefinitions(_.cloneDeep(filterDefinitionsRef.current));
    };

    const onRemoveFilter = (filterValue) => {
        filterDefinitionsRef.current.delete(filterValue);
        setFilterDefinitions(_.cloneDeep(filterDefinitionsRef.current));
    };

    const getSourceAvailableOptions = () => {
        let availableSourceType = [ Source.SharePoint, Source.OneDrive ];
        if (LicenseHelper.HasUpgradeTeams()) {
            availableSourceType.push(Source.Teams);
        }
        if (LicenseHelper.HasOpusGoogleLicense()) {
            availableSourceType.push(Source.GoogleDrive);
        }
        let sourceAvailableOptions = filterAvailableOptions.get(AvaiableOptions.Source);
        if(sourceAvailableOptions){
            return sourceAvailableOptions.filter(item => availableSourceType.includes(item.Key));
        }else{
            return [];
        }
    };

    const getClassificationOptions = async () => {
        const requestOption = {   
            url: "/api/TrainingScopeApi/MLTermFilters",
        };
        $$.loading(true);
        const result = await fetchUtility(requestOption);
        $$.loading(false);
        
        if (result && result.length) {
            const resultMap = [];
            for (const item of result) {
                resultMap.push([item.Id, item.Name]);
            }
            setClassificationOptions(new Map(resultMap));
        }
    };

    return (
        <R.Panel
            id="reco-manual-review-filter-panel"
            header={RMResx.RM_Common_Filter}
            size={660}
            status={{ show: showPanel }}
            onHide={onHidePanel}
            destroy={true}
        >
            <div>
                <div className="ra-flex-justify-end">
                    <a
                        className="ra-main-filter-clear fia-funnel-clear"
                        onClick={() => onResetFilters()}
                        tabIndex="0"
                        role="button"
                    > {RMResx.RM_Common_ClearFilter}</a>
                </div>
                <CollectionTimeFilter
                    onFilterChange={onFilterChange}
                    onRemoveFilterChange={onRemoveFilter}
                    filterDefinitions={filterDefinitions}
                />
                <SourceFilter
                    onFilterChange={onFilterChange}
                    onRemoveFilterChange={onRemoveFilter}
                    filterDefinitions={filterDefinitions}
                    availableOptions={getSourceAvailableOptions()} 
                />
                <WorkspaceFilter
                    onFilterChange={onFilterChange}
                    onRemoveFilterChange={onRemoveFilter}
                    filterDefinitions={filterDefinitions}
                />
                <ClassificationFilter
                    onFilterChange={onFilterChange}
                    onRemoveFilterChange={onRemoveFilter}
                    filterDefinitions={filterDefinitions}
                    options={classificationOptions}
                />
                <ModifiedByFilter
                    onFilterChange={onFilterChange}
                    onRemoveFilterChange={onRemoveFilter}
                    filterDefinitions={filterDefinitions}
                />
                <CreatedByFilter
                    onFilterChange={onFilterChange}
                    onRemoveFilterChange={onRemoveFilter}
                    filterDefinitions={filterDefinitions}
                />
                <EscalatedFromFilter
                    onFilterChange={onFilterChange}
                    onRemoveFilterChange={onRemoveFilter}
                    filterDefinitions={filterDefinitions}
                />
                <RecordReviewerFilter
                    onFilterChange={onFilterChange}
                    onRemoveFilterChange={onRemoveFilter}
                    filterDefinitions={filterDefinitions}
                />
                <DateModifiedFilter
                    onFilterChange={onFilterChange}
                    onRemoveFilterChange={onRemoveFilter}
                    filterDefinitions={filterDefinitions}
                />
                <DateCreatedFilter
                    onFilterChange={onFilterChange}
                    onRemoveFilterChange={onRemoveFilter}
                    filterDefinitions={filterDefinitions}
                />
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={onHidePanel} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={onFilterSave} />
            </>
        </R.Panel>
    );

};

export default forwardRef(UnderReviewFilterPanel);