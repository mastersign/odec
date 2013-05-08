@ECHO.
@ECHO Extend the container with owner2 and a result entity

odec -m e -c democontainer.zip ^
  -o ..\owner\owner2.xml -cr ..\owner\copyright.txt ^
  -e ..\entitysource\description2.xml
