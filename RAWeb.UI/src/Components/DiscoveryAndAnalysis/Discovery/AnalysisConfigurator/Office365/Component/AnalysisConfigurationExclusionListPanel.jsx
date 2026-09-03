import Whitelist from "./Whitelist";
import './AnalysisConfiguration.less';

const AnalysisConfigurationExclusionListPanel = ({ show, onClose }) => {
    return (
        <R.Panel
            id="raExclusionListPanel"
            header={RMResx.RM_FA_Discovery_ExclusionList}
            size={680}
            status={{ show }}
            destroy={true}
            onClose={onClose}
        >
            <R.Button
                slot="buttons"
                primary
                classify="theme"
                text={RMResx.RM_JS_Common_Close}
                onClick={onClose}
            />
            <Whitelist onClosePanel={onClose} />
        </R.Panel>
    );
};

export default AnalysisConfigurationExclusionListPanel;
