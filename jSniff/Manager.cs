using System;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace jSniff
{
    public class Manager
    {
        #region jSniff.js
        private const string JSNIFF_SCRIPT = @"
(function (window) {

    'use strict';

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

        var funcParams = match[1].split(',');
        var paramsAuditingFunc = '';
        var evalParams = new Array();

        for (var i = 0; funcParams.length > i; i++) {
            if (funcParams[i] != '') {
                paramsAuditingFunc += 'window.jSniff.spies.' + sniffName + '.invocations[window.jSniff.spies.' + sniffName + '.invocations.length - 1].params.' + funcParams[i] + ' = JSON.parse(JSON.stringify(' + funcParams[i] + '));';
            }
        }

        for (var j = 0; funcParams.length > j; j++) {
            evalParams.push('\'' + funcParams[j].replace(/\s/gm, '') + '\'');
        }
		
		if(customFunc){
			var uniqueId = new Date().getTime();
			window.jSniff.spies[sniffName][uniqueId] = customFunc;
		}
        
        // used toString since it returned null when was a js Date object - jSniff.NET
        var text = 'var jsniffExecutionStart = Date.now();window.jSniff.spies.' + sniffName + '.invocations.push({ executionDate: jsniffExecutionStart, params: {} });';

        text += paramsAuditingFunc.replace(/\s/gm, '').replace(/'/gm, '\\\'');

        text += customFunc ? 'window.jSniff.spies[\\\'' + sniffName + '\\\'][' + uniqueId + ']();' : '';

        text += 'window.jSniff.spies.' + sniffName + '.toExecute(' + evalParams.join(',').replace(/'/g, '') + ');';
		
		var durationSnippet = 'window.jSniff.spies.' + sniffName + '.invocations[window.jSniff.spies.' + sniffName + '.invocations.length - 1].duration = Date.now() - jsniffExecutionStart;'
		
        var toEval = 'new Function(' + evalParams.join(',') + ',\'' + text + durationSnippet + '\')';

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

})(window);                      


";
        #endregion

        private static bool isScriptLoaded = false;
        private IJavaScriptExecutor driver;

        public Manager(IJavaScriptExecutor driver)
        {
            this.driver = driver;
        }

        
        private void LoadScript()
        {
            driver.ExecuteScript(JSNIFF_SCRIPT, null);
            isScriptLoaded = true;
        }

        public Sniff Sniffify(string obj, string functionName, string sniffName, string customFunc = "")
        {
            string toExecute = string.Empty;

            if (!isScriptLoaded)
            {
                LoadScript();
            }

            if (!string.IsNullOrEmpty(customFunc))
            {
                toExecute = string.Format("jSniff.sniffify({0},'{1}','{2}',{3});", obj, functionName, sniffName, customFunc);
            }
            else
            {
                toExecute = string.Format("jSniff.sniffify({0},'{1}','{2}');", obj, functionName, sniffName);
            }

            driver.ExecuteScript(toExecute, null);

            return new Sniff(driver, sniffName);
        }

        [Obsolete("use Sniff object instance returned from Sniffify instead.")]
        public Invocation[] GetInvocations(string sniffName)
        {
            if (!isScriptLoaded)
            {
                LoadScript();
            }

            var result = driver.ExecuteScript(string.Format("return jSniff.getInvocations('{0}')", sniffName), null);
            return Converter.ConvertToInvocation(result);
        }

        [Obsolete("use Sniff object instance returned from Sniffify instead.")]
        public Invocation GetLastInvocation(string sniffName)
        {
            var invocations = GetInvocations(sniffName);
            return invocations[invocations.Length - 1];
        }

        [Obsolete("use Sniff object instance returned from Sniffify instead.")]
        public Dictionary<string, object> GetLastInvocationParams(string sniffName)
        {
            return GetLastInvocation(sniffName).ExecutionParameters;
        }

        [Obsolete("use Sniff object instance returned from Sniffify instead.")]
        public long GetLastInvocationDuration(string sniffName)
        {
            return GetLastInvocation(sniffName).Duration;
        }
    }
}
