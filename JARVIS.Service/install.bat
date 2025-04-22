@echo off
sc create JARVIS binPath= "%~dp0\JARVIS.Service.exe" start= auto
sc start JARVIS
pause