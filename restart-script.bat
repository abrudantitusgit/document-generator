@echo off
echo Wake detected...

REM asteapta retea + SMB
timeout /t 10

REM mergi in folderul unde e scriptul
cd /d %~dp0

echo Restarting Docker Compose...

docker compose down
timeout /t 5
docker compose up -d

echo Done!