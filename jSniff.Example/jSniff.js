(function (window, undefined) {

    "use strict";

    var jSniff = {};

    jSniff.sniffify = function (obj, funcName, sniffName, customFunc) {

        obj = obj || window;

        if (!jSniff.spies) {
            jSniff.spies = {};
        }

        if (!jSniff.spies[sniffName]) {
            jSniff.spies[sniffName] = {
                toExecute: null,
                invocations: []
            };
        }

        jSniff.spies[sniffName].toExecute = obj[funcName];

        var match = obj[funcName].toString().match(/\((.*?)\)/);

        if (match == null) {
            return null;
        }

        var funcParams = match[1].split(",");
        var paramsAuditingFunc = "";
        var evalParams = new Array();

        for (var i = 0; funcParams.length > i; i++) {
            if (funcParams[i] != "") {
                paramsAuditingFunc += "window.jSniff.spies." + sniffName + ".invocations[window.jSniff.spies." + sniffName + ".invocations.length - 1].params." + funcParams[i] + " = JSON.parse(JSON.stringify(" + funcParams[i] + "));";
            }
        }

        for (var j = 0; funcParams.length > j; j++) {
            evalParams.push("'" + funcParams[j].replace(/\s/gm, '') + "'");
        }
		
		if(customFunc){
			var uniqueId = new Date().getTime();
			window.jSniff.spies[sniffName][uniqueId] = customFunc;
		}

        var text = "window.jSniff.spies." + sniffName + ".invocations.push({ executionDate: new Date(), params: {} });";

        text += paramsAuditingFunc.replace(/\s/gm, '').replace(/'/gm, "\\'");

        text += customFunc ? 'window.jSniff.spies["' + sniffName + '"][' + uniqueId + ']();' : "";

        text += "window.jSniff.spies." + sniffName + ".toExecute(" + evalParams.join(",").replace(/'/g, "") + ")";

        var toEval = "new Function(" + evalParams.join(",") + ",'" + text + "')";

        obj[funcName] = eval(toEval);
    };

    jSniff.getInvocations = function (sniffName) {

        if (!sniffName) {
            console.error('jSniff.js: no jSniff sniffName specified. operation cancelled.');
            return;
        }

        return jSniff.spies[sniffName].invocations;
    };

    jSniff.getLastInvocation = function (sniffName) {

        if (!sniffName) {
            console.error('jSniff.js: no jSniff sniffName specified. operation cancelled.');
            return;
        }

        var invocations = jSniff.spies[sniffName].invocations;

        return invocations[invocations.length - 1];
    };

    jSniff.getLastInvocationParams = function (sniffName) {

        if (!sniffName) {
            console.error('jSniff.js: no jSniff sniffName specified. operation cancelled.');
            return;
        }

        var invocations = jSniff.spies[sniffName].invocations;

        return invocations[invocations.length - 1].params;
    };

    window.jSniff = jSniff;

})(window, undefined);