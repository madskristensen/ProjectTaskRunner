## DNX Scripts Task Runner extension

Adds support for DNX scripts in Visual Studio 2015's
Task Runner Explorer.

[![Build status](https://ci.appveyor.com/api/projects/status/v2l19gi8n12nvh86?svg=true)](https://ci.appveyor.com/project/madskristensen/projecttaskrunner)

Download the extension at the
[VS Gallery](https://visualstudiogallery.msdn.microsoft.com/8f2f2cbc-4da5-43ba-9de2-c9d08ade4941)
or get the
[nightly build](http://vsixgallery.com/extension/ec768980-f2de-4db0-a6e2-5e57fa612ad5/)

### DNX scripts

Inside project.json it is possible to add custom scripts inside
the "scripts" element.

```js
{
	"scripts": {
		"postbundle": [ "npm install", "bower install" ],
		"postpack": "npm install"
	}
}
```

### Execute scripts

When scripts are specified, the Task Runner Explorer
will show those scripts.

![Task list](art/task-list.png)

Each script can be executed by double-clicking the task.

### Bindings

Script bindings make it possible to associate individual scripts
with Visual Studio events such as "After build" etc.

![Visual Studio bindings](art/bindings.png)

### Intellisense

If you manually edit bindings in project.json, then full
Intellisense is provided.

![Visual Studio Intellisense](art/intellisense.png)