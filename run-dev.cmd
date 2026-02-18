@echo off
setlocal
powershell -ExecutionPolicy Bypass -File ".\scripts\dev\dev-stack.ps1" up
endlocal
