echo off
REM Kafka Pub-Sub pattern scenario 2
REM One Pub and three Sub
REM Author: Karan Thapa

start "Publisher 1" cmd /T:2E /k Pub.exe -n 1000 -q "Kafka" -p "MEDICATIONS"
start "Publisher 2" cmd /T:2E /k Pub.exe -n 1000 -q "Kafka" -p "PROBLEMS"
start "Publisher 3" cmd /T:2E /k Pub.exe -n 1000 -q "Kafka" -p "ALLERGY"
start "Subscriber 1" cmd /T:1E /k Sub.exe -s "MEDICATIONS" -q "Kafka"
start "Subscriber 2" cmd /T:1E /k Sub.exe -s "PROBLEMS" -q "Kafka"
start "Subscriber 3" cmd /T:1E /k Sub.exe -s "ALLERGY" -q "Kafka"
