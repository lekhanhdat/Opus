import { NormalCell } from "../../../../Common/TableTemplateCell";

export default class Template extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }

    render(Row, Cell) {
        let {
            MailboxAddress,
            TotalArchivedSize,
            TotalArchivedSizeWithoutRelatedSites,
        } = this.props.rowData;

        return (
            <Row>
                <NormalCell Cell={Cell} contentText={MailboxAddress} />
                <NormalCell Cell={Cell} contentText={TotalArchivedSize} />
                <NormalCell Cell={Cell} contentText={TotalArchivedSizeWithoutRelatedSites} />
            </Row>
        );
    }
}