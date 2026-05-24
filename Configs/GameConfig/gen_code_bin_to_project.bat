Cd /d %~dp0
echo %CD%

set WORKSPACE=../..
set LUBAN_DLL=%WORKSPACE%\Tools\Luban\Luban.dll
if not exist "%LUBAN_DLL%" set LUBAN_DLL=D:\Work\Project\3rd\luban_examples\Tools\Luban\Luban.dll
if not exist "%LUBAN_DLL%" set LUBAN_DLL=D:\Work\Project\Learn\ServerBuild\GameServers\Tools\luban\Tools\Luban\Luban.dll
if not exist "%LUBAN_DLL%" (
    echo [Error] Luban.dll not found. Please place it at %WORKSPACE%\Tools\Luban\Luban.dll
    exit /b 1
)
set CONF_ROOT=.
set DATA_OUTPATH=%WORKSPACE%/UnityProject/Assets/AssetRaw/Configs/bytes/
set CODE_OUTPATH=%WORKSPACE%/UnityProject/Assets/GameScripts/HotFix/GameProto/GameConfig/

copy /y "%CONF_ROOT%\CustomTemplate\ConfigSystem.cs" "%WORKSPACE%\UnityProject\Assets\GameScripts\HotFix\GameProto\ConfigSystem.cs"
copy /y "%CONF_ROOT%\CustomTemplate\ExternalTypeUtil.cs" "%WORKSPACE%\UnityProject\Assets\GameScripts\HotFix\GameProto\ExternalTypeUtil.cs"

dotnet %LUBAN_DLL% ^
    -t client ^
    -c cs-bin ^
    -d bin^
    --conf %CONF_ROOT%\luban.conf ^
    -x code.lineEnding=crlf ^
    -x outputCodeDir=%CODE_OUTPATH% ^
    -x outputDataDir=%DATA_OUTPATH% 
if not defined AI_MODE pause
