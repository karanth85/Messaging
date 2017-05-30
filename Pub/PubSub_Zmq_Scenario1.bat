echo off
REM ZeroMQ Pub-Sub pattern scenario 1
REM One Pub and two Sub
REM -d flag for publisher is for sending 1MB of message (remove -d if sending small messages)
REM Author: Karan Thapa

start "Subscriber 1" cmd /T:1E /k Sub.exe -s "MEDICATIONS";"PROBLEMS" -q "ZeroMq"
start "Subscriber 2" cmd /T:1E /k Sub.exe -s "ALLERGY" -q "ZeroMq"
start "Publisher" cmd /T:2E /k Pub.exe -n 1 -q "ZeroMq" -p "MEDICATIONS";"PROBLEMS";"ALLERGY" -d