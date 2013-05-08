ODEC
====

The ODEC command line tool supports the creation, extension, 
validation, and inspection of *Open Digital Evidence Containers*.

Syntax
------

    odec (-?|-i|(-m c|e|i|v|t)) [-c <file/dir>]
      [[-o <ownerfile> -cert <certfile>] -k <key file>] [-nb] [-ns]
      [-s (z, d) [-t <file/dir>]] [-vv] [-vc [-ca <file/dir>]] 
      [-p <profile file> [-vp]] 
      [-cr <copyright text file>] [-cm <comments text file>] [-nxc]
      [-e <entity description file>]*

The order of the arguments is not relevant. Some arguments depend on
other arguments. If an argument is used more than ones, the last
one counts. Except for `-e <entity description file>`
which can be used more than once. For detailed description see below.

* `--help`, `-?`
  Display this help text.
* `--info`, `-i`
  Display version information and copyright.
* `--mode`, `-m <mode>`
  Choose the working mode:
  - `create`, `c`
  - `extend`, `e`
  - `inspect`, `i`
  - `validate`, `v`
  - `transform`, `t`
  - `reinstate`, `r`
* `--container <file/dir>`, `-c <file/dir>`
  Specifies the container file or directory.
* `--owner <owner description file>`, `-o <owner description file>`
  Specifies a XML file with the owner description.
  (see section 'Owner Description File')
* `--copyright <copyright text file>`, `-cr <copyright text file>`
  Specifies a text file with copyright information.
* `--comments <comments text file>`, -`cm <comments text file>`
  Specifies a text file with comments.
* `--storage <storage type>`, `-s <storage type>`
  Choose the storage type:
  - `directory`, `dir`, `d`
  - `zipfile`, `zip`, `z`
* `--entity <entity file>`, `-e <entity file>`
  Specify an entity description file.
  (see section 'Entity Description File')
* `--profile <profile file>`, `-p <profile file>`
  Specify a profile description file.
* `--certauthority <certificate file/dir>`, `-ca <certificate file/dir>`
  Specifies a single trusted certification authority or a directory 
  with a number of trusted certification authorities.
* `--validatevalue`, `-vv`
  Validate the signatures of the entity values.
* `--validatecert`, `-vc`
  Validate the owner certificates.
* `--validateprofile`, `-vp`
  Validate the structure with a profile.
* `--target <file/dir>`, `-t <file/dir>`
  Specify the target name for a container transformation.
* `--targetedition <edition ID>`, `-te <edition ID>`
* `--noback`, `-nb`
  Removes the salt of the former current edition, if present,
  to prevent the possibility of reinstating the former edition 
  as current edition later on.
* `--nosalt`, `-ns`
  Omits the creation of random data as salt in the new edition.
* `--noxmlcanon`, `-nxc`
  Switches to compatibility mode with no C14N XML canonicalization
  of XML structure documents before signature creation.

Examples
--------

Create a new container as ZipFile with one base entity

`> odec -m c -o owner.xml -p myprofile.xml 
    -c newcontainer.zip -e baseentity.xml`

Inspect the container

`> odec -m i -c newcontainer.zip`

Validate the integrity and authenticity of a container including
the validity of the owner certificates

`> odec -m v -vv -vc -c newcontainer.zip`

Extend a container with three new entities

`> odec -m e -nb -o owner.xml -c container.zip 
    -e entity1.xml -e entity2.xml -e entity3.xml`

Take the ownership of a container without adding new entities

`> odec -m e -o owner.xml -c container.zip`

Transform a container from a ZipFile to a directory

`> odec -m t -s d -c container.zip -t ./newcontainerdir`

Reinstate a history edition

`> odec -m r -c container.zip 
    -te {c9fe6591-3487-42fb-ae34-c30375f4150d}`

Working Modes
-------------

### Create
The creation of a container begins with an owner and one or	more
entity descriptions. The owner, including its associated 
certificate and private key file, is described by a XML file.
The entities are also described by XML files. Optional a storage 
type for the container can be chosen, the default is zip.
Optional the container can be associated with a container profile.

**Required arguments**

* `-c <container file/dir>`
* `-o <owner description file>`
* `-e <entity description file>` (one or more times)

**Optional arguments**

* `-s <storage type>`
* `-p <profile file>`
* `-cr <copyright text file>`
* `-cm <comments text file>`
* `-ns`

### Extend
The extension of a container begins with the owner, which is 
described by a XML file.
Through the extension, new entities can be added to the 
container. If no entity is added, simply the owner changes. 
Before the extension, the first validation step is executed.
Further steps can be activated with the corresponding arguments.
If the validation fails, the container will be left unchanged.

**Required arguments**

* `-c <container file/dir>`
* `-o <owner description file>`

**Optional arguments**

* `-e <entity description file>` (one or more times)
* `-cr <copyright text file>`
* `-cm <comments text file>`
* `-nb`
* `-ns`
* `-vv`
* `-vc`
* `-vp`, `-p <profile file>`

### Inspect
The inspection of a container begins with the first validation
step (see 'Validate'). Only error messages are displayed while 
the validation. If the structure of the container is
valid, a tree, showing all elements of the container, is given.

### Validate
The validation of a container works in four steps. Info and error
messages are displayed.

1. Check the structure and integrity of the current edition, 
   the container history, the entity index and the entity 
   headers. This step is executed mandatory.
2. Check the integrity of the entity values.
   To activate this step, use `-vv`.
3. Check the validity of the owner certificates.
   To activate this step, use `-vc`.
   If `-ca` is provided than only owner certificates derived from
   the given trusted CA's are valid; otherwise only
   self signed owner certificates are valid.
4. Check the structural conformance to the profile.
   To activate this step, use `-vp` and `-p <profile file>`.

**Required arguments**

* `-c <container file/dir>`

**Optional arguments**

* `-vv`
* `-vc`, `-ca <certificate file/dir>`
* `-vp`, `-p <profile file>`

### Transform
The transformation of a container allows to copy a container
from one storage type to another. It can, e.g. be used to extract
a ZipFile container to a directory based container or vice versa.
No validation is done.

**Required arguments**

* `-c <existing container file/dir>`
* `-t <new container file/dir>`
* `-s <new storage type>`
    
## Reinstate
Builds a container back to the given history edition.

**Required arguments**

* `-c <existing container file/dir>`
* `-te <GUID of history edition>`

Owner Description File
----------------------
The owner description file is a simple XML document in the default 
XML namespace. The `CertificateFile` tag contains a path to a PEM
encoded X.509 compatible certificate for use with the RSA schema. 
The `PrivateKeyFile` tag contains a path to a PEM encoded private key
for use with the RSA schema. If the paths are relative, they are
interpreted in relation to the location of the owner description 
file. The following XML text is a complete example:

    <?xml version="1.0" ?>
    <Owner>
      <Institute>University of Demoland</Institute>
      <Operator>John Smith</Operator>
      <!-- The role element is optional -->
      <Role>forensic expert</Role>
      <Email>john.smith@server.com</Email>
      <CertificateFile>owner.crt</CertificateFile>
      <PrivateKeyFile>owner.key</PrivateKeyFile>
    </Owner>

Entity Description File
-----------------------
The entity description file is a simple XML file in the default XML 
namespace. Every entity can have predecessors, which are identified 
by their IDs. The entity IDs are running numbers, starting with 1.
Furthermore, every entity has a type specified by a GUID, 
a provenance specified by a GUID, optionally a parameter set and at
least one value. The parameter set and the values are specified by a
name, a type (GUID) and a source file. The path of the source file
can be relative to the entity description file.
The following XML text is a simple example:

    <?xml version="1.0" ?>
    <Entity>
      <Type>9e719384-86fe-4a46-a068-6b8216ac473d</Type>
      <Provenance>
        <Guid>9e719384-86fe-4a46-a068-6b8216ac473d</Guid>
      </Provenance>
      <Value>
        <Name>metadata.xml</Name>
        <Type>410d596b-89a8-4a14-99e1-80dc6021fc11</Type>
        <SourceFile>entitydata\scan001_meta.xml
      </Value>
      <Value>
        <Name>rawdata.frt</Name>
        <Type>4db9614c-c005-4efb-9955-1e791730ff32</Type>
        <SourceFile>entitydata\scan001.frt
      </Value>
    </Entity>

The following example uses predecessors, a parameter set and a 
suppressed value:

    <?xml version="1.0" ?>
    <Entity>
      <Predecessors>1 3</Predecessors>
      <Type>4fd8b288-13b9-45fe-ba29-a646137e05ef</Type>
      <Provenance>
        <Guid>4fd8b288-13b9-45fe-ba29-a646137e05ef</Guid>
      </Provenance>
      <ParameterSet>
        <Name>preproc_config.ini</Name>
        <Type>23d40921-d601-4e62-9bd9-d81b00029b1a</Type>
        <SourceFile>config/preprocessing.ini</SourceFile>
      </ParameterSet>
      <Value>
        <Name>preproc1.bin</Name>
        <Type>4fd8b288-13b9-45fe-ba29-a646137e05ef</Type>
        <!-- 
          if no source file is given, the value is marked as suppressed. 
        -->
      </Value>
      <Value>
        <Name>preproc2.bin</Name>
        <Type>4fd8b288-13b9-45fe-ba29-a646137e05ef</Type>
        <SourceFile>entitydata\proc2.bin
      </Value>
    </Entity>

Error Codes
-----------
The error codes returned by ODEC:

*   0 - No error. The requested operation was successful.
*   2 - Argument error. One or more given arguments are invalid.
*   3 - Missing argument. A required argument is missing.
*   4 - Container already exists.
*   5 - XML error. One of the specified XML documents is invalid.
*   6 - Initialize container failed.
*   7 - Seal container failed.
*   8 - Create new entity failed.
*   9 - Store entity failed.
*  10 - Description is invalid.
*  11 - Value source file not found.
*  12 - Container not found.
*  13 - Profile mismatch.
*  14 - Open container storage failed.
*  15 - Copy storage file failed.
*  16 - Certificate missing.
*  17 - Private key missing.
*  18 - Reinstating history edition is impossible.
*  19 - Reinstating history edition failed.
* 101 - Validation error step 1. Basis container structure invalid.
* 102 - Validation error step 2. Value integrity damaged.
* 103 - Validation error step 3. Owner certificate invalid.
* 104 - Validation error step 4. Container is not conform to profile.