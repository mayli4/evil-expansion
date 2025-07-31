@echo off

set OUT_ERR=err.log
for /r %%i in ("*.fx") do (
    set "filepath=%%~dpi"
    setlocal enabledelayedexpansion
    set "filepath=!filepath:~0,-1!"

    powershell -Command "$content = Get-Content -Encoding UTF8 '%%i'; $Utf8NoBomEncoding = New-Object System.Text.UTF8Encoding $False; [System.IO.File]::WriteAllLines('%%~dpi%%~ni.lol', $content, $Utf8NoBomEncoding)"

    echo Compiling %%~nxI...
    fxc.exe /Gec /T "fx_2_0" /Fo "%%~dpi%%~ni.fxc" "%%~dpi%%~ni.lol" >nul 2>%OUT_ERR%
    del "%%~dpi%%~ni.lol"

    type %OUT_ERR%
    endlocal
)

del %OUT_ERR%
pause
