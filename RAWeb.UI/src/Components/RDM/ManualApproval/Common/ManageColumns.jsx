import _ from "lodash";

const ManageColumns = ({columns, checkedField = "checked", textField, valueField, onChange}) => {

    const managedColumnsChanged = (args) => {
        let tableColumns = _.cloneDeep(columns);
        let selectedManageColumnIds = args.newValue.map(item => item.id);
        for(let column of tableColumns){
            column.visible = selectedManageColumnIds.includes(column.id);
        }
        onChange(tableColumns);
    };

    return <R.Multicombobox
        checkedField={checkedField}
        textField={textField}
        valueField={valueField}
        hasFilter={true}
        required={true}
        hasSelectAll={true}
        clearable={true}
        customTrigger={true}
        items={_.cloneDeep(columns)}
        noneText={RMResx.RM_JS_JM_CustomColumns}
        allText={RMResx.RM_JS_JM_CustomColumns}
        selectedItemsTemplate={RMResx.RM_JS_JM_CustomColumns}
        selectedItemTemplate={RMResx.RM_JS_JM_CustomColumns}
        onChange={managedColumnsChanged}
        triggerBySource={true}
    >
        <R.Button icon="fia-manage-column" text={RMResx.RM_JS_JM_CustomColumns} tooltip={RMResx.RM_JS_JM_CustomColumns} />
    </R.Multicombobox>;
};
export default ManageColumns;

