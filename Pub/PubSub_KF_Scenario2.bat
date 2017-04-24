echo off
REM Kafka Pub-Sub pattern scenario 2
REM One Pub and three Sub
REM Author: Karan Thapa

start "Publisher" cmd /T:2E /k Pub.exe -n 10 -q "Kafka"
start "Subscriber 1" cmd /T:1E /k Sub.exe -s "MEDICATIONS" -q "Kafka"
start "Subscriber 2" cmd /T:1E /k Sub.exe -s "PROBLEMS" -q "Kafka"
start "Subscriber 3" cmd /T:1E /k Sub.exe -s "ALLERGY" -q "Kafka"
