import _ from "lodash";

class ExceptionHandler {
    static handleAsync = async (asyncFunc, defaultValue) => {
        try{
            $$.loading(true);
            const res = await asyncFunc();
            if(!_.isNil(res)) {
                $$.loading(false);
                return res;
            }
            $$.loading(false);
        }
        catch(err) {
            console.error(err);
            if(!_.isNil(defaultValue)) {
                return defaultValue;
            }
            $$.loading(false);
        }
    };
};

export default ExceptionHandler;