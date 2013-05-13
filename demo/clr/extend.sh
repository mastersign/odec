#!/bin/sh
echo "Extend the container with owner2 and a result entity"

./odec.sh -m e -c democontainer.zip -o ../owner/owner2.xml -cr ../owner/copyright.txt -e ../entitysource/description2.xml

