echo off
REM RabbitMq Client-Server scenario
REM One Client and Server
REM Author: Karan Thapa

start "Server (Rep)" cmd /T:0B /k Server.exe -q "RabbitMq" 
start "Client (Req)" cmd /T:0E /k Client.exe -n 1 -q "RabbitMq"