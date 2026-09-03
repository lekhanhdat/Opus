import PeoplePicker from "../../../Common/PeoplePicker";
import {bindEvents} from "../../../../Utilities/CommonUtil";

export default class PhyObjectFilter extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        //console.log(props);
        this.nodeTypeFiltersBasicList = [
            { id: 0, name: RMResx.RM_PRM_PRE_Filter_All, isChecked: true },
            // {id: 9000, name: "Physical Root Location"},
            // {id: 9100, name: "Physical Normal Location"},
            // {id: 9200, name: "Physical Bottom Location"},
            { id: 9250, name: RMResx.RM_PRM_PRE_TableItemType_Container },
            { id: 9300, name: RMResx.RM_PRM_PRE_Filter_PhysicalBox },
            { id: 9400, name: RMResx.RM_PRM_PRE_Filter_PhysicalFile },
            { id: 9500, name: RMResx.RM_PRM_PRE_Filter_PhysicalRecord }
        ];

        this.statusFiltersBasicList = [
            { id: 0, name: RMResx.RM_JS_BCM_Explorer_Filter_All, isChecked: true },
            { id: 1, name: RMResx.RM_Template_Column_Value_Status_Open },
            { id: 6, name: RMResx.RM_Template_Column_Value_Status_Closed },
            { id: 2, name: RMResx.RM_Template_Column_Value_Status_Destroyed },
            { id: 7, name: RMResx.RM_Template_Column_Value_Status_Missing },
        ];
        this.filterData = JSON.parse(JSON.stringify(props.data));
        this.state = {
            nodeTypeFilters: this.filterData.NodeType != -4 ? this.getSelectedOptions(this.filterData.NodeType, this.nodeTypeFiltersBasicList) : this.nodeTypeFiltersBasicList,
            statusFilters: this.filterData.Status != -1 ? this.getSelectedOptions(this.filterData.Status, this.statusFiltersBasicList) : this.statusFiltersBasicList,
            recordsOwners: props.data.RecordsOwner && props.data.RecordsOwner.length > 0 ? props.data.RecordsOwner : [],
            creators: props.data.CreatedBy && props.data.CreatedBy.length > 0 ? props.data.CreatedBy : [],
            Modifiers: props.data.ModifiedBy && props.data.ModifiedBy.length > 0 ? props.data.ModifiedBy : [],
        };
        this.bind([]);
    }

    getSelectedOptions(id, Options) {
        for (let Option of Options) {
            if (Option.id == id) {
                Option.isChecked = true;
            } else {
                Option.isChecked = false;
            }
        }
        return Options;
    }

    componentReceive(action, data) {
        this.handleFilterButton(data);
    }

    handleNodeTypeSelectedChange = (args) => {
        this.filterData.NodeType = args.newValue.id;
    };

    handleStatusSelectedChange = (args) => {
        this.filterData.Status = args.newValue.id;
    };

    onPeopleSelectionChanged = (type, args) => {
        this.filterData[type] = args;
    };

    handleFilterButton = (isClear) => {
        if(!isClear) {
            this.props.onSave(this.filterData);
        } else {
            this.resetFilterData();
        }
    };

    resetFilterData() {
        this.filterData = {};
        let typeList = RM.deepcopy(this.nodeTypeFiltersBasicList);
        typeList.forEach(o => { o.isChecked = o.id == 0; });
        let statusList = RM.deepcopy(this.statusFiltersBasicList)
        statusList.forEach(o => { o.isChecked = o.id == 0; });
        this.setState({
            nodeTypeFilters: typeList,
            statusFilters: statusList,
            recordsOwners:  [],
            creators: [],
            Modifiers: []
        });
    }

    render() {
        return <div className='ra-phyExp-filterForm' id={this.props.id}>
            <$g.FormRow label={RMResx.RM_PRM_PRE_ItemType.replace(":", "")}>
                <R.Combobox
                    id="raPhyFilterColumnNodeType"
                    valueField="id"
                    textField="name"
                    checkedField="isChecked"
                    searchable={false}
                    width={"100%"}
                    items={this.state.nodeTypeFilters}
                    onChange={this.handleNodeTypeSelectedChange}
                />
            </$g.FormRow>
            <$g.FormRow label={RMResx.RM_PRM_PRE_Column_Status.replace(":", "")}>
                <R.Combobox
                    id="raPhyFilterColumnStatus"
                    valueField="id"
                    textField="name"
                    checkedField="isChecked"
                    searchable={false}
                    width={"100%"}
                    items={this.state.statusFilters}
                    onChange={this.handleStatusSelectedChange}
                />
            </$g.FormRow>
            <$g.FormRow label={RMResx.RM_PRM_PRE_Filter_RecordsOwner.replace(":", "")}>
                <PeoplePicker
                    id="raPhyFilterColumnRecordsOwner"
                    width={"100%"}
                    items={this.state.recordsOwners}
                    selectionChanged={this.onPeopleSelectionChanged.bind(this, 'RecordsOwner')}
                />
            </$g.FormRow>
            <$g.FormRow label={RMResx.RM_PRM_PRE_Filter_CreatedBy.replace(":", "")}>
                <PeoplePicker
                    id="raPhyFilterColumnCreatedBy"
                    width={"100%"}
                    items={this.state.creators}
                    selectionChanged={this.onPeopleSelectionChanged.bind(this, 'CreatedBy')}
                />
            </$g.FormRow>
            <$g.FormRow label={RMResx.RM_PRM_PRE_Filter_ModifiedBy.replace(":", "")}>
                <PeoplePicker
                    id="raPhyFilterColumnModifiedBy"
                    width={"100%"}
                    items={this.state.Modifiers}
                    selectionChanged={this.onPeopleSelectionChanged.bind(this, 'ModifiedBy')}
                />
            </$g.FormRow>
        </div>;
    }
}
