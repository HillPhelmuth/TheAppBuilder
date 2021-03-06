﻿
export function reloadIFrame(id, newSrc) {
    console.log("reloadIframe called for:");
    console.log(id);
    console.log("with src:");
    console.log(newSrc);
    const iFrame = document.getElementById(id);
    if (iFrame) {
        if (newSrc) {
            iFrame.src = newSrc;
        } else {
            iFrame.contentWindow.location.reload();
        }
    }
}

var _dotNetInstance = null;
export function init(dotNetInstance) {
    console.log("init run from js");
    _dotNetInstance = dotNetInstance;
    throttleLastTimeFuncNameMappings['compile'] = new Date();
}
export function updateUserAssemblyInCacheStorage(file) {
    console.log("updateUserAssemblyInCacheStorage run from js");
    const response = new Response(new Blob([base64ToArrayBuffer(file)], { type: 'application/octet-stream' }));

    caches.open('blazor-resources-/').then(function (cache) {
        if (!cache) {
            _dotNetInstance.invokeMethodAsync('AppBuilderAlone', 'ShowCacheError');
            return;
        }

        cache.keys().then(function (keys) {
            const keysForDelete = keys.filter(x => x.url.indexOf('Output') > -1);

            const dll = keysForDelete.find(x => x.url.indexOf('dll') > -1).url.substr(window.location.origin.length);
            cache.delete(dll).then(function () {
                cache.put(dll, response).then(function () { });
            });
        });
    });
}
export function copyToClipboard(text) {
    if (!text) {
        return;
    }

    const input = document.createElement('textarea');
    input.style.top = '0';
    input.style.left = '0';
    input.style.position = 'fixed';
    input.innerHTML = text;
    document.body.appendChild(input);
    input.select();
    document.execCommand('copy');
    document.body.removeChild(input);
}
export function initOnOffLine(dotNetInstance) {
    window.addEventListener('offline', function(e) {
        console.log('offline');
        dotNetInstance.invokeMethodAsync('HandleOnOffLine', 'offline');

    });

    window.addEventListener('online', function(e) {
        console.log('online');
        dotNetInstance.invokeMethodAsync('HandleOnOffLine', 'online');
    });
}
export function checkOnline() {
    return window.navigator.onLine;
}
export function saveAsFile(filename, bytesBase64) {
    var link = document.createElement('a');
    link.download = filename;
    link.href = "data:application/octet-stream;base64," + bytesBase64;
    document.body.appendChild(link); // Needed for Firefox
    link.click();
    document.body.removeChild(link);
}
export function dispose() {
    _dotNetInstance = null;
    //window.removeEventListener('keydown', onKeyDown);
}

const throttleLastTimeFuncNameMappings = {};

function base64ToArrayBuffer(base64) {
    const binaryString = window.atob(base64);
    const binaryLen = binaryString.length;
    const bytes = new Uint8Array(binaryLen);
    for (let i = 0; i < binaryLen; i++) {
        const ascii = binaryString.charCodeAt(i);
        bytes[i] = ascii;
    }

    return bytes;
}