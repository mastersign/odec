@ECHO.
@ECHO Create a container with owner1 and a base entity

odec -m c -s z -o ..\owner\owner1.xml ^
  -c democontainer.zip -p ..\profile.xml ^
  -e ..\entitysource\description1.xml