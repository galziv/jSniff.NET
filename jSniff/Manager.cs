using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OpenQA.Selenium;

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
        var text = 'window.jSniff.spies.' + sniffName + '.invocations.push({ executionDate: new Date().toString(), params: {} });';

        text += paramsAuditingFunc.replace(/\s/gm, '').replace(/'/gm, '\\\'');

        text += customFunc ? 'window.jSniff.spies[\\\'' + sniffName + '\\\'][' + uniqueId + ']();' : '';

        text += 'window.jSniff.spies.' + sniffName + '.toExecute(' + evalParams.join(',').replace(/'/g, '') + ')';

        var toEval = 'new Function(' + evalParams.join(',') + ',\'' + text + '\')';

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
        private const string EXECUTION_DATE = "executionDate";
        private const string PARAMS = "params";
        private IJavaScriptExecutor driver;

        public Manager(IJavaScriptExecutor driver)
        {
            this.driver = driver;
        }


        private static DateTime ConvertJsDateToDateTime(string date)
        {
            return DateTime.ParseExact(Regex.Match(date, "(.*) GMT").Groups[1].Value, "ddd MMM d yyyy HH:mm:ss", CultureInfo.InvariantCulture);
        }

        private static Invocation[] ConvertToInvocation(object result)
        {
            return ((IReadOnlyCollection<object>)result).Select(x =>
            {
                var dictionary = (Dictionary<string, object>)x;
                var parameters = ((Dictionary<string, object>)dictionary[PARAMS]);
                var paramsDictionary = new Dictionary<string, object>(parameters.Count);

                foreach (string key in parameters.Keys)
                {
                    paramsDictionary.Add(key, parameters[key]);
                }

                return new Invocation
                {
                    ExecutionDate = ConvertJsDateToDateTime(dictionary[EXECUTION_DATE].ToString()),
                    ExecutionParameters = paramsDictionary
                };

            }).ToArray();
        }

        private void LoadScript()
        {
            driver.ExecuteScript(JSNIFF_SCRIPT, null);
            isScriptLoaded = true;
        }

        public void Sniffify(string obj, string functionName, string sniffName, string customFunc = "")
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
        }

        public Invocation[] GetInvocations(string sniffName)
        {
            if (!isScriptLoaded)
            {
                LoadScript();
            }

            var result = driver.ExecuteScript(string.Format("return jSniff.getInvocations('{0}')", sniffName), null);
            return ConvertToInvocation(result);
        }

        public Invocation GetLastInvocation(string sniffName)
        {
            var invocations = GetInvocations(sniffName);
            return invocations[invocations.Length - 1];
        }

        public Dictionary<string, object> GetLastInvocationParams(string sniffName)
        {
            return GetLastInvocation(sniffName).ExecutionParameters;
        }
    }
}
