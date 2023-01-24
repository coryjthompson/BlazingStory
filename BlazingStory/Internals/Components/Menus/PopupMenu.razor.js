﻿export const getBoundingClientRect = (element) => {
    return element.getBoundingClientRect();
};

export const subscribeDocumentEvent = (eventType, dotnetObj, methodName, excludeSelector) => {
    const evendListener = (ev) => {
        if (excludeSelector && ev.srcElement.matches(excludeSelector)) return;
        dotnetObj.invokeMethodAsync(methodName);
    }
    document.addEventListener(eventType, evendListener);
    return ({ dispose: () => document.removeEventListener(eventType, evendListener) });
}