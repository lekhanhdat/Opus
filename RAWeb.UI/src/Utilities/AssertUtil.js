export default class AssertUtil {
    static Assert(condition, message) {
        if(!condition) {
            if(message) {
                throw new Error(`[Record] ${message}`);
            }
            throw new Error(`[Record] Unknow error.`);
        }
    }
}