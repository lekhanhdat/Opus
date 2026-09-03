import _ from "lodash";
import { ArchiveDataType, MS365DataType, ArchiveOrRemoveVersionType } from "../../../../Constants/DataOptimizeType";

const MS365DataTypeScope = ({ dataOptimizeParameter, onChange }) => {

    const handleOptionChange = (ms365DataType) => {
        let clonedParameter = _.cloneDeep(dataOptimizeParameter);
        initOptimizeParameter(clonedParameter, ms365DataType);
        onChange(clonedParameter);
    };

    const initOptimizeParameter = (parameter, ms365DataType) => {
        parameter.ms365DataType = ms365DataType;
        if (ms365DataType === MS365DataType.Phl) {
            parameter.archiveDataType = ArchiveDataType.All
        }

        parameter.fileExtensionQueryParameter = {};

        parameter.inactiveRuleQueryParameter.enable = false;
        if (ms365DataType === MS365DataType.Phl) {
            parameter.rotRuleQueryParameter.enable = false;
        }
        else {
            parameter.rotRuleQueryParameter.enable = true;
        } 
        
        parameter.processActionParameter = {
            ...parameter.processActionParameter,
            archiveOrRemoveVersion : ArchiveOrRemoveVersionType.ArchiveAndRemove,
            isEnableLeaveStub: false,
            deleteRecords: false,
            isArchiveVersionOption: false,
            archiveVersionValue: "0",
            selectedLevelStub: {},
        };
    };

    return (
        <div>
            <div className="reco-optimize-title">{RMResx.RM_FA_DataOptimize_DSOMS365DataFilterTypeTitle}</div>
            <div>
                <R.Radio
                    name="dataSourceOption"
                    value= { MS365DataType.Default }
                    checked= { dataOptimizeParameter.ms365DataType === MS365DataType.Default }
                    text= { RMResx.RM_FA_DataOptimize_SharepointOrOneDriveTitle }
                    onChange={ () => handleOptionChange(MS365DataType.Default) }
                />
            </div>

            <div>
                <R.Radio
                    name="dataSourceOption"
                    value= { MS365DataType.Phl }
                    checked={ dataOptimizeParameter.ms365DataType === MS365DataType.Phl }
                    text= { RMResx.RM_FA_DataOptimize_PreservationHoldLibraryTitle }
                    onChange= { () => handleOptionChange(MS365DataType.Phl) }
                />
                <$g.Popover> { RMResx.RM_FA_DataOptimize_PreservationHoldLibraryPopover } </$g.Popover>
            </div>
        </div>
    );
};

export default MS365DataTypeScope;