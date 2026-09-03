class NotImplementedError extends Error {
    constructor(className, methodName) {
        super(`The [${className}-${methodName}] doesn't provide an implementation.`);
        this.name = "NotImplementedError";
    }
}

export default NotImplementedError;