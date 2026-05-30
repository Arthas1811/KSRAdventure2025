Add-Type -Path 'Library/ScriptAssemblies/Newtonsoft.Json.dll'
$foo = '{ \ currentImage\: \3\ }'
$jo = [Newtonsoft.Json.Linq.JObject]::Parse($foo)
[Console]::WriteLine($jo['currentImage'].ToString())
