import { BuildColumnIds } from "../../Constants";
import { PhysicalDefaultColumnIDs, PhysicalObjectColumnType } from "../../../../../Constants/Constants";
export default class HSMultipleChoice extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            options: RM.deepcopy(this.props.options),
            isSingleChoice: this.props.isSingleChoice
        };
    }

    componentReceive(data) {
        let options = this.state.options;
        if (data.Value || data.Value === false) {
            if (data.ColumnType === PhysicalObjectColumnType.YesOrNo) {
                options[0].isChecked = data.Value;
                options[1].isChecked = !data.Value;
            } else {
                switch (data.UniqueId) {
                    case BuildColumnIds.HoldStatus:
                    case BuildColumnIds.DeclaredRecord:
                    case BuildColumnIds.LockedByRecordLabel:
                    case BuildColumnIds.ContentArchived:
                        options[0].isChecked = data.Value;
                        options[1].isChecked = !data.Value;
                        break;
                    case PhysicalDefaultColumnIDs.Status:
                        var selectedOptionValues = data.Value.map((item) => { return item.Value; });
                        for (let item of options) {
                            item.isChecked = selectedOptionValues.includes(item.id);
                        }
                        break;
                    default:
                        var values = data.Value.map((item) => { return item.Value; });
                        for (let item of options) {
                            item.isChecked = values.includes(item.name);
                        }
                }
            }
        }
        this.setState({
            options: RM.deepcopy(options),
        });
    }

    onFilterColChange = (args) => {
        this.props.onChange(args);
    }

    renderSeacrchChoice() {
        if (this.state.isSingleChoice) {
            return <R.Validation element="Combobox" require={RMResx.RM_HS_NoSearchColValValidMsg}>
                <R.Combobox
                    items={this.state.options}
                    height={40}
                    width={"100%"}
                    disabled={false}
                    searchable={false}
                    checkedField="isChecked"
                    textField="name"
                    valueField="id"
                    linkMode={false}
                    onChange={this.onFilterColChange}
                />
            </R.Validation>;
        } else {
            return <R.Validation element="Multicombobox" require={RMResx.RM_HS_NoSearchColValValidMsg}>
                <R.Multicombobox
                    items={this.state.options}
                    height={40}
                    width={"100%"}
                    disabled={false}
                    searchable={false}
                    checkedField="isChecked"
                    textField="name"
                    valueField="id"
                    clearable={true}
                    linkMode={false}
                    onChange={this.onFilterColChange}
                />
            </R.Validation>;
        }
    }

    render() {
        let condition = this.state.isSingleChoice ? RMResx.RM_HS_Equals : RMResx.RM_HS_Contains;
        return <div className="flex">
            <div className="flex-1">
                <R.Input
                    type="text"
                    value={condition}
                    width={"100%"}
                    height={40}
                    readonly={true}
                />
            </div>
            <div className="flex-1 margin-left-m width-0">
                {this.renderSeacrchChoice()}
            </div>
        </div>;
    }
}