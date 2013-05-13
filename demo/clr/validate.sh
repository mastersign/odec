#!/bin/sh
echo "Run a complete validation of the container"

./odec.sh -m v -vv -vc -ca ../owner/ca.crt -vp -p ../profile.xml -c democontainer.zip

