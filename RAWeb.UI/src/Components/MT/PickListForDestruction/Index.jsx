import SiteMapLinks from '../../../Constants/SiteMapLinks';
import PickListCommon from "../PickListCommon/Index";
import Template from "./Template";
import Actions from "./Actions";
import { TableColumns, StatusList } from "./Config";

const PickListForDestruction = () => {
    return <>
        <$g.SiteMap data={[SiteMapLinks.MT_PickListForDestruction]} />
        <div id="raMtPickListForDestruction">
            <PickListCommon
                recordListApiUrl="/api/PickListApi/QueryDestruction"
                tableColumns={TableColumns}
                tableTemplate={Template}
                statusList={StatusList}
                exportUrl={"/api/PickListApi/StartExportDestructionJob"}
                Actions={Actions}
            />
        </div>
    </>;
};

export default PickListForDestruction;