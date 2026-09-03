import _ from "lodash";

const Input = ({ name, type, value, isEmail, isTooLongValidation, onChange }) => {

    const customVerify = (value) => {
        value = value.trim();
        if (_.isNil(value) || _.isEmpty(value)) {
            return RMResx.RM_FS_Register_NameInputValidateMessage;
        } else if (isTooLongValidation && value.length > 255) {
            return RMResx.RM_JS_Common_Msg_CannotExceed255;
        }
        return true;
    };

    return (
        <div className="reco-box-input">
            <div className="reco-box-input-label require">
                {name}
            </div>
            <R.Validation
                element="Input"
                rules={{
                    isEmail: isEmail,
                    customVerify: customVerify,
                }}
            >
                <R.Input
                    id="raBoxConfigIpt"
                    type={type}
                    width={"100%"}
                    value={value}
                    onChange={onChange}
                    aria={{ ariaLabel: name }}
                />
            </R.Validation>
        </div>
    );
};
export default Input;