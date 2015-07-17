# jSniff.NET

jSniff.NET is [jSniff.js] for .NET developers to be used with Selenium WebDriver
[jSniff.js]:https://github.com/galziv/jSniff

## How to use
Check [jSniff.js] for more details on the functions.
<br/>
####Initialize the jSniff manager with Selenium web driver
```c#
jSniff.Manager manager = new Manager(new ChromeDriver());
```
#####sniff the function using:
#####Manager.Sniffify
----
```c#
public void Sniffify(string obj, string functionName, string sniffName, string customFunc = "")
```

 - obj: The object on which the function exists (if it is a function declaration then use window)
 - funcName: The name of the function
 - sniffName: The name for the sniff. It is later used when retrieving the invocations data
 - customFunc: A custom function which will be executed prior to the original function's code

lets assume we have a javascript client function declaration as follows:
```js
function multiply(a,b){
    return a*b;
}
```

so to sniff it we execute:
```c#
manager.Sniffify("window", "multiply", "window_multiply", "function() { console.log('log from custom function');}");
```

<br />
#####retrieve sniffed data using:
#####jSniff.getInvocations
----
```c#
public Invocation[] GetInvocations(string sniffName)
```
This method returns an array of Invocation object. Invocation object consists of the sniffed data and has two properties:
 - executionDate: Javascript Date object with the execution date.
  - params: Array of the execution parameters. Each element in array is Dictionary<string, object> (parameter name,paramter value)

<br />
#####jSniff.getLastInvocation
----
```c#
pubilc Invocation GetLastInvocation(string sniffName)
```
This method returns the last execution sniffed data. the Invocation object has two properties:
 - executionDate: Javascript Date object with the execution date.
 - params: Array of the execution parameters. Each element in array is Dictionary&lt;string, object> (parameter name,paramter value)

<br />
#####jSniff.getInvocations
----
```c#
Dictionary<string, object> GetLastInvocationParams(string sniffName)
```
This method returns an array of the last execution parameters. Each element in the array is Dictionary<string, object>

