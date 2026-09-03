import { EnvironmentHelper, LicenseHelper } from "../../../../../Utilities/CommonUtil";
import { NormalCell } from "../../../../Common/TableTemplateCell";

const isNewOpusAccount = LicenseHelper.EnableRecordsArchiver();
const is21VEnv = LicenseHelper.Is21VEnv();
const isGccEnv = EnvironmentHelper.IsGovAzureEnv;

export default class Template extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
        this.isSupportedNewTable = isNewOpusAccount && !is21VEnv && !isGccEnv;
    }

    render(Row, Cell) {
        let {
            SiteUrl,
            TotalSize,
            TotalDeleteSize,
            TotalSizeArchivedByM365
        } = this.props.rowData;

        return <Row>
            <NormalCell Cell={Cell} contentText={SiteUrl} />
            <NormalCell Cell={Cell} contentText={TotalSize} />
            <NormalCell Cell={Cell} contentText={TotalDeleteSize} />
            {this.isSupportedNewTable && (
                <NormalCell Cell={Cell} contentText={TotalSizeArchivedByM365} />
            )}
        </Row>;
    }
}