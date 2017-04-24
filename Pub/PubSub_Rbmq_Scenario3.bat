echo off
REM RabbitMq Pub-Sub pattern scenario 3
REM One Pub and two Sub
REM Author: Karan Thapa

start "Publisher" cmd /T:2E /k Pub.exe -n 20 -q "RabbitMq"
start "Subscriber 1" cmd /T:1E /k Sub.exe -s "MEDICATIONS";"PROBLEMS" -q "RabbitMq"
start "Subscriber 2" cmd /T:1E /k Sub.exe -s "ALLERGY" -q "RabbitMq"
