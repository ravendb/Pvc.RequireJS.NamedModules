Pvc.RequireJS.NamedModules
==========================

PVC Build plugin that finds RequireJS modules and changes them to RequireJS **named** modules.

This is useful for concatenating all the RequireJS class files into a single file; the modules will still be identified correctly when concatenated into a single file by loaders like DurandalJS or binding systems like KnockoutJS.

**In**: <br/>
    MyFoo.js: <code>define(["require", "exports"], function(require, exports) {
        var MyFoo = (function () {
           ...
        })();
    }</code>
    
**Out**: <br />
    MyFoo.js: <code>define("MyFoo", ["require", "exports"], function(require, exports) {
        var MyFoo = (function () {
           ...
        })();
    })();</code>
