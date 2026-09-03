import { NormalCell, TooptipByApiCell} from "../../Common/TableTemplateCell";
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
            Requestor, 
            HomeLocation, 
            Status
        } = this.props.rowData;
        let statusText = StatusList.filter((item) =>{return item.value == Status;})[0].name;
        
        return <Row>
            <NormalCell Cell={Cell} contentText={RecordName}/>
            <NormalCell Cell={Cell} contentText={UniqueId}/>
            <NormalCell Cell={Cell} contentText={Requestor}/>
            <NormalCell Cell={Cell} contentText={HomeLocation}/>
            <NormalCell Cell={Cell} contentText={statusText}/>
        </Row>;
    }
}

export class ReturnHistoryTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }

    render(Row, Cell) {
        let {
            ItemName, 
            UniqueId, 
            RequestBy, 
            HomeLocation, 
            ReturnTime
        } = this.props.rowData;
        
        return <Row>
            <NormalCell Cell={Cell} contentText={ItemName}/>
            <NormalCell Cell={Cell} contentText={UniqueId}/>
            <NormalCell Cell={Cell} contentText={RequestBy}/>
            <NormalCell Cell={Cell} contentText={HomeLocation}/>
            <NormalCell Cell={Cell} contentText={ReturnTime}/>
        </Row>;
    }
}
