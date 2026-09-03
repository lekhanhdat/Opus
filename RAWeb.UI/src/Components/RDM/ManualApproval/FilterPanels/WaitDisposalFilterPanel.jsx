import React, { useEffect, useRef, useState } from "react";
import _ from "lodash";
import {
    ApprovalStatusFilter,
    ApprovedByFilter,
    CollectionTimeFilter,
    CreatedByFilter,
    EscalatedFromFilter,
    ModifiedByFilter,
    RecordReviewerFilter,
    RuleDisposalClassFilter,
    RuleNameFilter,
    SourceFilter,
    WorkspaceFilter,
    FolderPathFilter,
    QuickReasonFilter,
    ModifiedTimeFilter,
} from "../Filters/index";

import { AvaiableOptions, FilterI18Ns, FilterOptions, ManualTab } from "../Constants/index";
import CustomColumnTextFilter from "../Filters/CustomColumnTextFilter";
import { CustomColumnType } from "../../../BCM/ContentRepositoryManagement/CustomMetadataSetting/Constants";
import CustomColumnYesOrNoFilter from "../Filters/CustomColumnYesOrNoFilter";
import CustomColumnDateTimeFilter from "../Filters/CustomColumnDateTimeFilter";

const WaitDisposalFilterPanel = ({ show, onFilter, onHide, defaultFilterDefinitions, filterAvailableOptions,SpecialEnableReviewDefinitions,approvalCommentQuickReasons,SpeciallEnableReviewOnlyOneLocationDefinitions, customColumns}) => {
    
    const filterDefinitionsRef = useRef(defaultFilterDefinitions);

    const [filterDefinitions, setFilterDefinitions] = useState(new Map());

    useEffect(() => {
        const fetchData = async () => {
            const definitions = new Map();
            for (const definition of defaultFilterDefinitions) {
                if (definition.CustomColumnId) {
                    definitions.set(definition.CustomColumnId, definition);
                } else {
                    
                    definitions.set(definition.FilterOption, definition);
                }
            }
            setFilterDefinitions(definitions);
            filterDefinitionsRef.current = defaultFilterDefinitions;
        };

        fetchData();
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
        setFilterDefinitions(_.cloneDeep(definitions));
        updateFilterDefinitionRef(definitions);
    };

    const onCustomFilterChange = (filterValue) => {
        const definitions = filterDefinitions;
        definitions.set(filterValue.CustomColumnId, filterValue);
        setFilterDefinitions(_.cloneDeep(definitions));
        updateFilterDefinitionRef(definitions);
    };

    const onRemoveFilter = (filterValue) => {
        const definitions = filterDefinitions;
        definitions.delete(filterValue);
        setFilterDefinitions(_.cloneDeep(definitions));
        updateFilterDefinitionRef(definitions);
    };

    const renderCustomFilter = () => {
        if (customColumns && customColumns.length) {
            const renderComponent = (item) => {
                switch (item.columnType) {
                    case CustomColumnType.SingleText:
                        return (
                            <CustomColumnTextFilter
                                key={item.id}
                                title={item.header}
                                filterOption={FilterOptions.CustomColumnText}
                                customColumnId={item.id}
                                filterDefinitions={filterDefinitions}
                                onFilterChange={onCustomFilterChange}
                                onRemoveFilterChange={onRemoveFilter}
                            />
                        );
                    case CustomColumnType.YesOrNo:
                        return (
                            <CustomColumnYesOrNoFilter
                                key={item.id}
                                title={item.header}
                                customColumnId={item.id}
                                filterDefinitions={filterDefinitions}
                                onFilterChange={onCustomFilterChange}
                                onRemoveFilterChange={onRemoveFilter}
                            />
                        );
                    case CustomColumnType.DateTime:
                        return (
                            <CustomColumnDateTimeFilter
                                key={item.id}
                                title={item.header}
                                customColumnId={item.id}
                                filterDefinitions={filterDefinitions}
                                onFilterChange={onCustomFilterChange}
                                onRemoveFilterChange={onRemoveFilter}
                            />
                        );
                    case CustomColumnType.Number:
                        return (
                            <CustomColumnTextFilter
                                key={item.id}
                                title={item.header}
                                filterOption={FilterOptions.CustomColumnNumber}
                                customColumnId={item.id}
                                filterDefinitions={filterDefinitions}
                                onFilterChange={onCustomFilterChange}
                                onRemoveFilterChange={onRemoveFilter}
                            />
                        );
                    default:
                        return null;
                };
            };
            return customColumns.map(renderComponent);
        }
        return null;
    }

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
                <CollectionTimeFilter
                    onFilterChange={onFilterChange}
                    onFilterSave={onFilterSave}
                    onRemoveFilterChange={onRemoveFilter}
                    filterDefinitions={filterDefinitions}
                />
            {!SpecialEnableReviewDefinitions && (
                <SourceFilter
                    onFilterChange={onFilterChange}
                    onFilterSave={onFilterSave}
                    onRemoveFilterChange={onRemoveFilter}
                    filterDefinitions={filterDefinitions}
                    availableOptions={filterAvailableOptions.get(AvaiableOptions.Source)}
                />
            )}
            {!SpecialEnableReviewDefinitions  &&(
                <WorkspaceFilter
                    onFilterChange={onFilterChange}
                    onFilterSave={onFilterSave}
                    onRemoveFilterChange={onRemoveFilter}
                    filterDefinitions={filterDefinitions}
                    enableFolderPath={SpecialEnableReviewDefinitions}
                />
            )}
            {SpecialEnableReviewDefinitions && !SpeciallEnableReviewOnlyOneLocationDefinitions && (
            <WorkspaceFilter
                onFilterChange={onFilterChange}
                onFilterSave={onFilterSave}
                onRemoveFilterChange={onRemoveFilter}
                filterDefinitions={filterDefinitions}
                enableFolderPath={SpecialEnableReviewDefinitions}
            />
            )}
                <FolderPathFilter
                    onFilterChange={onFilterChange}
                    onFilterSave={onFilterSave}
                    onRemoveFilterChange={onRemoveFilter}
                    filterDefinitions={filterDefinitions}
                    enableFolderPath={SpecialEnableReviewDefinitions}
                    OnlyOneLocationDefinitions = {SpeciallEnableReviewOnlyOneLocationDefinitions}
                    manualApprovalTab = {ManualTab.WaitDisposal}
                />
                <QuickReasonFilter
                    onFilterChange={onFilterChange}
                    onFilterSave={onFilterSave}
                    onRemoveFilterChange={onRemoveFilter}
                    filterDefinitions={filterDefinitions}
                    filterAvailableOptions = {filterAvailableOptions}
                    approvalCommentQuickReasons = {approvalCommentQuickReasons}
                />
                <ApprovalStatusFilter
                    onFilterChange={onFilterChange}
                    onFilterSave={onFilterSave}
                    onRemoveFilterChange={onRemoveFilter}
                    filterDefinitions={filterDefinitions}
                    availableOptions={filterAvailableOptions.get(AvaiableOptions.ApprovalStatus)}
                />
                <ModifiedByFilter
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
                <CreatedByFilter
                    onFilterChange={onFilterChange}
                    onFilterSave={onFilterSave}
                    onRemoveFilterChange={onRemoveFilter}
                    filterDefinitions={filterDefinitions}
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
                <EscalatedFromFilter
                    onFilterChange={onFilterChange}
                    onFilterSave={onFilterSave}
                    onRemoveFilterChange={onRemoveFilter}
                    filterDefinitions={filterDefinitions}
                />
                <RecordReviewerFilter
                    onFilterChange={onFilterChange}
                    onFilterSave={onFilterSave}
                    onRemoveFilterChange={onRemoveFilter}
                    filterDefinitions={filterDefinitions}
                />
                <ApprovedByFilter
                    onFilterChange={onFilterChange}
                    onFilterSave={onFilterSave}
                    onRemoveFilterChange={onRemoveFilter}
                    filterDefinitions={filterDefinitions}
                />
                {renderCustomFilter()}
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={onHide} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={onFilterSave} />
            </>
        </R.Panel>
    );

};

export default WaitDisposalFilterPanel;