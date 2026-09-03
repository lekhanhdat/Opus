import { NormalCell, TooptipByApiCell } from "../../Common/TableTemplateCell";
import { TooptipByApiCellType } from "../../../Constants/Constants";
import { StatusList } from "./Config";

export default class Template extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }

    render(Row, Cell) {
        let {
            RecordName, 
            UniqueId, 
            DestroyedDate,
            ApprovedBy,
            Status,
            Classification,
            HomeLocation
        } = this.props.rowData;
        let statusText = StatusList.filter((item) =>{return item.value == Status;})[0].name;
        
        return <Row>
            <NormalCell Cell={Cell} contentText={RecordName}/>
            <NormalCell Cell={Cell} contentText={UniqueId}/>
            <Cell>
                {<TooptipByApiCell 
                    cellType={TooptipByApiCellType.Term} 
                    rowData={this.props.rowData}
                    contentText={Classification}
                />}
            </Cell> 
            <NormalCell Cell={Cell} contentText={DestroyedDate}/>
            <NormalCell Cell={Cell} contentText={HomeLocation}/>
            <NormalCell Cell={Cell} contentText={ApprovedBy}/>
            <NormalCell Cell={Cell} contentText={statusText}/>
        </Row>;
    }
}