import { useRef, useLayoutEffect, useCallback } from "react";

function useEvent(handler) {
    const handlerRef = useRef(null);
    useLayoutEffect(() => {
        handlerRef.current = handler;
    });

    return useCallback((...args) => {
        const fn = handlerRef.current;
        return fn(...args);
    }, []);
}

export default useEvent;