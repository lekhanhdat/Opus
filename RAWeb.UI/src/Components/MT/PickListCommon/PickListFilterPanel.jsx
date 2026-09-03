import React, { useState, useImperativeHandle, forwardRef } from "react";
const PickListFilterPanel = ({ statusList, onFilter }, ref) => {
    const [isShow, setPanelIsShow] = useState(false);

    const [statusOptions, setStatusOptions] = useState(statusList);

    const [filterOptions, setFilterOptions] = useState({});

    useImperativeHandle(ref, () => ({
        openPanel: (filterParam) => {
            setPanelIsShow(true);
            setFilterOptions(filterParam);
            setStatusColumnOptions(filterParam);
        },
        getFilterOptions: () => {
            return filterOptions;
        }
    }));

    const setStatusColumnOptions = (filterOptions) => {
        for (let option of statusOptions) {
            if (filterOptions.Status) {
                option.checked = filterOptions.Status.includes(option.value);
            } else {
                option.checked = true;
            }
        }
        setStatusOptions(RM.deepcopy(statusOptions));
    };

    const onChangeStatus = (args) => {
        setFilterOptions({Status: args.newValue.map( item => item.value )});
    };

    const onClosePanel = () => {
        setPanelIsShow(false);
    };

    const onClickFiltePickList = () => {
        onFilter();
        onClosePanel();
    };

    const onClearFilter = () => {
        setFilterOptions({});
        setStatusColumnOptions({});
    };

    const renderFilterForm = () => {
        return (
            <div>
                <div className="ra-flex-justify-end">
                    <a
                        className="ra-main-filter-clear fia-funnel-clear"
                        tabIndex="0"
                        role="button"
                        onClick={onClearFilter}
                    > {RMResx.RM_Common_ClearFilter}</a>
                </div>
                <$g.FormRow label={RMResx.RM_MT_PickList_Column_Status}>
                    <R.Multicombobox
                        id="raMtPickListFilterStatus"
                        width={"100%"}
                        textField="name"
                        items={statusOptions}
                        onChange={onChangeStatus}
                        searchable={false}
                    />
                </$g.FormRow>
            </div>
        );
    };

    return (
        <R.Panel
            header={RMResx.RM_Common_Filter}
            size={664}
            status={{ show: isShow }}
            destroy={true}
            onHide={onClosePanel}
        >
            {renderFilterForm()}
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={onClosePanel} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={onClickFiltePickList} />
            </>
        </R.Panel>
    );
};

export default forwardRef(PickListFilterPanel);
