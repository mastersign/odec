Info
====

This folder contains scripts for the demonstration of the .NET/mono based command line interface of ODEC.

Preconditions
-------------

* The solution `\src\clr\ODEC.sln` need to be build in `Debug` mode.
  As a result the folder `\bin\clr\Debug` should contain the executable `odex.exe`

How-To
------

To run the scripts under Windows, you just can double click the `.bat` files.
Or you can open the command line change the directory to the `...\demo\clr`
and call the batch scripts simply by entering their names.

Typical Work-Flow
-----------------

1. `create` Create an initial container with one entity.
2. `validate` Check, if the container was created properly.
3. `inspect` Show the content of the created container.
4. `extend` Add additional entities to the container.
5. `validate` Check, if the extension worked properly.
6. `inspect` Show the new content.
7. `transform` Convert the ZIP-based container to an open directory.
8. `clean` Delete the created container.