import { useHistory } from "react-router";
import RouterUrls from "../../../Constants/RouterUrls";
import { LicenseHelper } from '../../../Utilities/CommonUtil';

const Actions = () => {

    const history = useHistory();

    const onAddTemplate = async() => {
        history.push({ pathname: RouterUrls.CP_CreateEmailTemplate });
    };

    if(LicenseHelper.HasOpusILLicense()){
        return <div>
            <R.Button primary={true} classify="theme" text={RMResx.RM_JS_CP_EamilTemplate_CreateTemplate} onClick={onAddTemplate} />
        </div>;
    }
    return <div></div>;
};

export default Actions;