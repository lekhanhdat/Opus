import { NormalCell } from "../../Common/TableTemplateCell";
import { StatusList } from "./Config";

export default class Template extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }

    render(Row, Cell) {
        let {
            ItemName, 
            UniqueId, 
            ApproveBy,
            HomeLocation,
            DestinationLocation,
            Status,
            Comment
        } = this.props.rowData;
        
        let statusObj = StatusList.find(item => item.value === Status);
        let statusText = statusObj ? statusObj.name : Status;
        
        return <Row>
            <NormalCell Cell={Cell} contentText={ItemName}/>
            <NormalCell Cell={Cell} contentText={UniqueId}/>
            <NormalCell Cell={Cell} contentText={ApproveBy}/>
            <NormalCell Cell={Cell} contentText={HomeLocation}/>
            <NormalCell Cell={Cell} contentText={DestinationLocation}/>
            <NormalCell Cell={Cell} contentText={statusText}/>
            <NormalCell Cell={Cell} contentText={Comment}/>
        </Row>;
    }
}