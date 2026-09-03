import React, { useEffect, useRef, useState } from "react";
import _ from "lodash";
import {
    ApprovalStatusFilter,
    ApprovedByFilter,
    CollectionTimeFilter,
    RuleDisposalClassFilter,
    RuleNameFilter,
    SourceFilter,
    ModifiedTimeFilter,
} from "../Filters/index";

import { AvaiableOptions } from "../Constants/index";
import ActionTimeFilter from "../Filters/ActionTimeFilter";

const HistoryFilterPanel = ({ show, onFilter, onHide, defaultFilterDefinitions, filterAvailableOptions }) => {

    const filterDefinitionsRef = useRef(defaultFilterDefinitions);

    const [filterDefinitions, setFilterDefinitions] = useState(new Map());

    useEffect(() => {
        const definitions = new Map();
        for (const definition of defaultFilterDefinitions) {
            definitions.set(definition.FilterOption, definition);
        }
        setFilterDefinitions(definitions);
        filterDefinitionsRef.current = defaultFilterDefinitions;
    }, [defaultFilterDefinitions]);

    const onResetFilters = () => {
        setFilterDefinitions(new Map());
        filterDefinitionsRef.current = [];
    };

    const onFilterSave = () => {
        const result = _.cloneDeep(filterDefinitionsRef.current);
        onFilter(result);
    };

    const updateFilterDefinitionRef = (definitions) => {
        const result = [];
        for (const keyValue of definitions) {
            result.push(keyValue[1]);
        }
        filterDefinitionsRef.current = result;
    };

    const onFilterChange = (filterValue) => {
        const definitions = filterDefinitions;
        definitions.set(filterValue.FilterOption, filterValue);
        setFilterDefinitions(definitions);
        updateFilterDefinitionRef(definitions);
    };

    const onRemoveFilter = (filterValue) => {
        const definitions = filterDefinitions;
        definitions.delete(filterValue);
        setFilterDefinitions(definitions);
        updateFilterDefinitionRef(definitions);
    };

    return (
        <R.Panel
            id="reco-manual-review-filter-panel"
            header={RMResx.RM_Common_Filter}
            size={660}
            status={{ show: show }}
            onHide={onHide}
            destroy={false}
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
                <ActionTimeFilter
                    onFilterChange={onFilterChange}
                    onFilterSave={onFilterSave}
                    onRemoveFilterChange={onRemoveFilter}
                    filterDefinitions={filterDefinitions}
                />
                <SourceFilter
                    onFilterChange={onFilterChange}
                    onFilterSave={onFilterSave}
                    onRemoveFilterChange={onRemoveFilter}
                    filterDefinitions={filterDefinitions}
                    availableOptions={filterAvailableOptions.get(AvaiableOptions.Source)}
                />
                <ApprovalStatusFilter
                    onFilterChange={onFilterChange}
                    onFilterSave={onFilterSave}
                    onRemoveFilterChange={onRemoveFilter}
                    filterDefinitions={filterDefinitions}
                    availableOptions={filterAvailableOptions.get(AvaiableOptions.ApprovalStatus)}
                />
                <RuleNameFilter
                    onFilterChange={onFilterChange}
                    onFilterSave={onFilterSave}
                    onRemoveFilterChange={onRemoveFilter}
                    filterDefinitions={filterDefinitions}
                    availableOptions={filterAvailableOptions.get(AvaiableOptions.RuleName)}
                />
                <RuleDisposalClassFilter
                    onFilterChange={onFilterChange}
                    onFilterSave={onFilterSave}
                    onRemoveFilterChange={onRemoveFilter}
                    filterDefinitions={filterDefinitions}
                    availableOptions={filterAvailableOptions.get(AvaiableOptions.RuleDisposalClass)}
                />
                <ApprovedByFilter
                    onFilterChange={onFilterChange}
                    onFilterSave={onFilterSave}
                    onRemoveFilterChange={onRemoveFilter}
                    filterDefinitions={filterDefinitions}
                />
                <ModifiedTimeFilter
                    onFilterChange={onFilterChange}
                    onFilterSave={onFilterSave}
                    onRemoveFilterChange={onRemoveFilter}
                    filterDefinitions={filterDefinitions}
                />
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={onHide} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={onFilterSave} />
            </>
        </R.Panel>
    );

};

export default HistoryFilterPanel;