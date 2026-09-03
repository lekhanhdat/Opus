import { Select } from './Components/Select';
import { GridCellType } from "../../../Constants/Constants";

export default class GridRow extends R.DatagridRow {
    constructor(props) {
        super(props);
        this.initBinding();
    }

    componentDidMount() {
        document.addEventListener("keydown", this.keydown);
    }

    //初始化bind this
    initBinding() {
        const eventsArr = ['handleCheckboxChange'];
        eventsArr.forEach((ev) => {
            this[ev] = this[ev].bind(this);
        });
    }

    //checkbox  change
    handleCheckboxChange(checked) {
        let e = window.e;
        this.rowData.isChecked = checked;
        this.trigger('rowDataChanged', e, {
            actionType: "checked"
        });
        this.setState({});
    }

    //获取cell组件
    getCellComponent(row, item) {
        var CellComponent = item.cellComponent;
        switch (item.cellComponent.type) {
            case GridCellType.SelectAll:
                return <Select onChange={this.handleCheckboxChange} isChecked={this.rowData.isChecked} />;
            default:
                var editing = false;
                if (item.isEditing) {
                    editing = item.isEditing(row);
                }
                return <CellComponent {...item.props} editing={editing} rowData={row} />;
        }
    }
    keydown(event) {
        //alert(event.keyCode);
    }
    render() {
        var row = this.props.rowData;
        return (
            <div data-part="row">
                {
                    this.rootData.map((item, key) => {
                        return <div key={key} data-part="cell">
                            {this.getCellComponent(row, item)}
                        </div>;
                    })
                }
            </div>
        );
    }
}