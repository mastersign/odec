ODEC - Specification
====================

* Author: Tobias Kiertscher <tobias@kiertscher.net>
* Revision: 0.1
* State: draft
* Last change: 2013-05-07

## Overview

+ [Introduction](#introduction)
+ [Change Log](#change-log)
+ [The Generic Process](#the-generic-process)
  + [The Owner](#the-owner)
  + [The Container](#the-container)
  + [Example Process](#example-process)
+ [The Meta-Model](#the-meta-model)
  + [Value and Provenance Parameter](#value-and-provenance-parameter)
  + [Value Reference](#value-reference)
  + [Entity Header](#entity-header)
  + [Index and Index Item](#index-and-index-item)
  + [Owner](#owner)
  + [Edition](#edition)
  + [History and History Item](#history-and-history-item)
  + [Container](#container)
+ [The Framework](#the-framework)
+ [Technical Implementation](#technical-implementation)
  + [Implementation of the Meta-Model](#implementation-of-the-meta-model)
  + [Implementation of Profiles](#implementation-of-profiles)
+ [References](#references)

## Introduction

This document contains the specification for an open digital evidence container 
based on [KVL2011][].

It splits up into the following chapters: 
In [The Generic Process](#the-generic-process), the concept
of a processing chain for digital evidence is explained. While laying out the
generic process, a number of important terms are introduced. 
In [The Meta-Model](#the-meta-model), the structure of a container is described.
This structure is basically independent from a technical implementation of the 
container, instead it describes the semantic items of a container and their 
relations. In [The Framework](#the-framework), the tailoring of the generic 
process into an actual evidence work-flow is described. Thereby the profiling of
a generic container into a specific container format is explained. In 
[Technical Implementation](#technical-implementation) the technical 
implementation of the meta-model is described. Finally in [Profiles](#profiles)
the the way of defining a container profile is explained.

## Change Log

| Revision |   State   | Changes
| -------: | :-------: | :---------------------------------------------------
|      0.1 | draft     | Initial deduction from paper.

## The Generic Process

The ODEC is based on a process oriented concept. Because the most kind of 
digital evidence is processed somehow, manually or automatically, before 
it reaches the form used for presentation, digital evidence is not simply 
seen as binary data, but as a chain of transformed data. Therefore, the data 
of the digital evidence is stored as intermediate and final results in a 
data-flow with *data sources* and *transformation steps*. In this context,
usually the term *chain-of-custody* is used, to describe the authenticity and
integrity of every data esteemed part of the evidence.

Data which is produced by data sources and transformed by transformation steps 
is wrapped in an *entity*. A data source takes no input data and produces one 
output entity. A transformation step can take a number of input entities and 
produces one output entity. Data sources and transformation steps are gathered 
under the common term *entity provenance*. An example process graph is shown 
in [Figure 1](#fig_example-process-graph).

An entity is a group of *values* and optional a *parameter set* from the 
provenance. The parameter set contains the *configuration* of the entity 
provenance, which resulted in the entity. A transformation step with given 
input entities and a given parameter set needs to be deterministic. 
This is necessary to allow the reproducibility of a transformation step as 
part of the chain of custody.

### The Owner

Every provenance is governed by an *owner*. The owner is responsible for the 
integrity of data production and data transformation. Every entity produced 
by an owner is protected by copyright. The owner holds a certificate and an 
associated private key to sign every produced entity. The certificate can be 
part of a public key infrastructure (PKI). More than one owner can take part 
in one process.

### The Container

All entities of a process can be stored together in a *container*. The container 
can be stored and transmitted as a unit. It can be initialized with one or more 
entities, produced by one owner, and can be extended later by the same or by 
different owners. Every time, the container is extended by an owner, a new 
*edition* of the container is formed. An edition is protected by a master 
signature created by the owner, covering the whole content of the container. 
While extending a container and forming a new edition, the master signature of 
the former edition is preserved in the *history stack* of the container.

### Example Process

The following figure illustrates an example process, where a digital camera, 
configured with a set of camera settings, produces a RAW image file.
The RAW image file is stored inside an entity, which is taken to initialize 
a container. Later, a RAW development tool is used to derive a JPEG image and 
a downsized thumbnail image from the RAW image, while applying a set of 
development settings. The JPEG image and the thumbnail are used as entity values 
in a new entity extending the container. Finally some EXIF information is 
extracted from the JPEG file and stored as a XMP file in a third entity.

<a name="fig_example-process-graph"></a>
![Example Process][fig:example-process-graph]

*Figure 1: Example process graph with a work-flow for images from a digital camera*

## The Meta-Model

In this chapter the meta-model of a container is described in detail.
The meta-model describes the semantic elements of a container on an abstract 
level. An overview to the meta-model is shown in [Figure 2](#fig_structure). 
In the following, the meta-model is described bottom-up, beginning with 
the entity values up to the container.

<a name="fig_structure"></a>
![ODEC Meta-Model][fig:structure]

*Figure 2: The meta-model of a container*

### Value and Provenance Parameter

<a name="fig_detailed-value"></a>
![Value][fig:detailed-value]

*Figure 3: Structure of a value or provenance parameter set*

A value is the element which contains the actual data in the process. 
It is identified by a name. The integrity and authenticity is verified by a 
signature, created by the owner which governed the production of the value.
If the value does not contain the processed data itself, but a set of 
parameters, used to configure a transformation step or entity source, it is 
called provenance parameter set.

### Value Reference

<a name="fig_detailed-value-reference"></a>
![Value reference][fig:detailed-value-reference]

*Figure 4: Structure of a value reference*

A value reference is the way to reference a value inside of the container 
structure. It contains a couple of meta-data for the referenced value. 
At first it contains an identifier to address a description of the value 
data type. This data type description should be sufficient for correct 
interpretation of the value data. Optional the data type description can 
provide a way to validate the value data. If the associated edition does not 
reference a profile, the value type can be omitted. Further the size of the
value data is included in the value reference. The size describes the number 
of octets contained in the data value. This information can be useful to 
optimize the way to process the value data, e.g. if large values should be 
handled differently than small ones. 

A value can appear in different ways inside of the container. It can be stored 
plain, without any manipulations, or it can be stored in an encrypted way. 
A third alternative is to not include it in the container at all, 
which is useful if some kind of raw or intermediate data should be excluded 
because of the value size, privacy or security reasons.
The way a value does appear in the container is stored as *appearance* in the 
value reference. To secure the link to the value, a copy of the value signature 
is included in the value reference.

### Entity Header

<a name="fig_detailed-entity-header"></a>
![Entity header][fig:detailed-entity-header]

*Figure 5: Structure of an entity header*

As described in the chapter [The Generic Process](#the-generic-process), 
the entity is a group of one or more entity values and optional a provenance 
parameter set. The entity header contains all data necessary to describe the 
entity. The entity is identified with an ID, unique inside the container. 
It can optionally be marked with a label for easier recognition. 

Potentially, an entity type is associated with the entity, to tell which values 
are allowed or required. More on entity types can be found in chapter 
[The Framework](#the-framework). Potentially a provenance description is 
included in the entity header and in case the provenance used some input 
entities to produce the described entity, these input entities are referenced by
their IDs as predecessors. If the associated edition does not reference a 
profile, the entity type and the provenance description can be omitted.

All values of the entity are referenced by a value reference and if the 
provenance was parameterized in some way, the provenance parameter set is 
referenced by a value reference too. The entity header is protected by a 
signature, created by the owner which has governed the production of the entity.
The signature of the entity header covers all value references and with them 
copies of the value signatures. Therefore, the signature of the entity header 
is simply called entity signature.

### Index and Index Item

<a name="fig_index"></a>
![Index and index item][fig:index]

*Figure 6: Structure of a container index and index items*

All entities in a container are listed by the index. The index is a collection 
of index items. Because every container is initialized with at least one entity,
the index contains at least one index item too. An index item contains a 
reference to an entity, identified by ID and optionally by label, and a list 
with the IDs of its successors. A successor to the current entity is an entity,
produced by a provenance under use of the current entity as input. 

To preserve the trust between the index and the entities, a copy of the entity 
signature is included in an index item. The index itself is protected by the 
index signature.

### Owner

<a name="fig_detailed-owner"></a>
![Owner][fig:detailed-owner]

*Figure 7: Structure of an owner*

As described in section [The Owner](#the-owner), the owner is an institute 
and/or person responsible for the initialization or extension of a container. 
The owner is identified by an institute name, an operator name, and an email 
address. The email address can also be used to contact the owner. Additionally, 
contact information as postal address can be given. Optional, the owner can be 
associated with a role, e.g. to support role-based access control. Every owner 
needs a certificate, which can be part of a PKI.

### Edition

<a name="fig_detailed-edition"></a>
![Edition][fig:detailed-edition]

*Figure 8: Structure of a container edition*

As described in section [The Container](#the-container), every time a container 
is initialized or modified an edition is formed. A container has a 
*current edition* and, if it was modified, one or more past editions. An edition
is identified by a global unique identifier. The edition is protected by a 
signature, created by the owner of the signature. The signature of the current 
edition is called *master signature*.

The edition contains some meta-data:
* A reference to the software, used to create or extend the container
* Optional a reference to an associated container profile including a version 
  number, see chapter "The Framework"
* The owner, responsible for the edition
* A time stamp of the moment, the edition was formed
* A list with IDs of all entities, added to the container while forming the 
  edition
* A list with IDs of all entities, removed from the container while forming the 
  edition
* Optional some copyright information can be attached to the edition, to control
  the legal usage of the produced data.
* Optional some comments can be attached to the edition.

To create a trusted link to the content of the container, an edition contains a 
copy of the index signature and a copy of the history signature (the history 
will be discussed in section 
[History and History Item](#history-and-history-item)).

An edition is optionally equipped with a salt which, if present, is included 
in the computation of the master signature. If the container gets extended, 
and the former edition is pushed on to the history stack, the salt can be 
removed from the former edition. Thereby the possibility of removing the new
edition and restoring the former edition from the history stack can be 
prevented, because the old master signature cannot be reinstated without the 
lost salt of the former edition. This entails the following implications:

1. Restoring a history edition can only be prevented if it was equipped with a 
   salt in the first place.
2. Even if some of the editions are safe from restoring, if an earlier edition 
   on the history stack is not safe (because it never had a salt, or its salt 
   was not removed), it can be restored by removing all later editions.
3. The salt of an edition can only be removed in the moment it gets pushed on 
   to the history stack. Removing the salt of a history edition later on, can 
   lead to inconsistent signatures of following editions.

The edition salt is also called *reinstatement prevention salt* or *RPS*.

### History and History Item

<a name="fig_history"></a>
![History and history item][fig:history]

*Figure 9: Structure of a history and history items*

The history of a container is a stack with history items. A history item 
contains a past edition. If the container is modified, a new history item, 
containing the former current edition, is pushed on to the history stack. 
The history is protected by a signature, created by the owner of the current 
edition.

### Container

<a name="fig_detailed-container"></a>
![Container][fig:detailed-container]

*Figure 10: Structure of a container*

A container is formed by a current edition, a container history and an entity 
index, including all referenced entities. The master signature of the whole 
container is the signature of the current edition.

## The Framework

In this chapter, a short description is given, how to map an evidence processing
work-flow to the generic process, described in chapter 
[The Generic Process](#the-generic-process).

To map an evidence processing work-flow to the generic process, the process 
steps need to be classified in two classes: 

* Process steps, which take no input data, but produce output data 
  (e.g. a measuring device or forensic software), 
* and process steps, which take input data and produce output data 
  (e.g. some kind of pre-processing or feature extraction).

The first class is mapped to entity sources and the second class is mapped to 
the transformations. Both classes of process steps can be parameterized and 
their parameter set is encoded in an octet stream. Data in the process is 
defined as a group of values and every value is encoded in an octet stream. 
A group of semantic associated values is mapped to an entity.

To define the mapping of an evidence processing work-flow to the generic 
process, three catalogs need to be defined. One for data types, one for entity 
types and one for provenance interfaces (see the left part in 
[Figure 11](#fig_framework)). All items in the three catalogs are identified 
by global unique identifiers.

<a name="fig_framework"></a>
![Framework][fig:framework]

*Figure 11: Framework for profiling the container format with a process model*

The data types are used to specify the encoding of entity values and provenance 
parameter sets into octet streams. The entity types define the format for all 
entities in the process by specifying the required and optional values including
their data types. The description of a provenance interface must list the entity
types of input entities and the entity type of the output entity.

The description of the actual provenance, implementing the provenance interface
from the provenance interface catalog, is not given in the container profile, 
but in the container itself.

E.g., if multiple cameras are used in the process to produce color images, 
then a data type definition for the image file format is needed in the data type
catalog (e.g. `JPEG_File`), an entity type definition in the entity type catalog
is needed, referencing the data type for one of its values 
(e.g. `Capture_Result` with `image.jpg` as `JPEG_File`) and one provenance 
interface description in the provenance interface catalog is needed 
(e.g. `Capture_Device`) taking no input entities and producing an output entity 
of the entity type `Capture_Result`. If a container is formed with an image from
one of the cameras, the used camera represents the actual provenance and need to
be described thoroughly with the provenance description in the entity header,
e.g. with vendor, model name and serial number.

## Technical Implementation

In this chapter the implementation of the container format and container 
profiles is described.

### Implementation of the Meta-Model

In this section the technical implementation of the meta-model is specified. 
The result of the technical implementation is a generic evidence container, 
which can be used in various evidence work-flows.

In the following sub-sections, the physical structure of the container is 
specified. The physical structure forms the shape of the container model in 
concrete data structures and consists of three layers.

* The first layer is the storage mechanism, used to persist to data.
* The second layer is the implementation of all elements from the meta-model 
  with concrete data structures. 
* And the third layer describes the characteristics of the security mechanisms
  like cryptographic hashes and signatures.

#### Storage in Directory and ZipFile

This technical implementation provides two ways to persist the content of a 
container. The first way is to use a directory in a common file system like
Ext3, FAT32 or NTFS. The second way is to use a ZipFile. In both ways, the 
elements of the container are stored in a two level tree (see 
[Figure 12](#fig_file-structure)). In the first level, all elements beside the 
entities and their children are stored. In the second level, the entities are 
stored. Each entity is stored in its individual sub-directory.

In case of using a ZipFile as a container storage, the ZIP64 extension is used
to store large entity values.

<a name="fig_file-structure"></a>
![File Structure][fig:file-structure]

*Figure 12: 2-Level storage structure*

#### Implementation of the Meta-Model

In this sub-section all elements of the meta-model are mapped to a concrete data 
structure. Most of the elements in the model are mapped to XML. Some elements 
are mapped to a XML file and others are mapped to XML elements inside of a XML 
file. The XML elements are defined by a XML Schema
`odec-container.xsd`, which can be used for automated validation. 
The XML Schema is targeting the XML namespace 
`http://www.mastersign.de/odec/container/`.

In the following code snippets the prefix `xs` is used for the XML namespace of
the XML schema `http://www.w3.org/2001/XMLSchema`.

The following elements are mapped to storage files (see 
[Figure 12](#fig_file-structure)):

* Current edition: `/edition.xml` (with an `Edition` element as document root)
* Current edition signature (master signature): `/edition.xml.sig` \*
* Container history: `/history.xml` (with a `History` element as document root)
* Container history signature: `/history.xml.sig` \*
* Entity index: `/index.xml` (with an `Index` element as document root)
* Entity index signature: `/index.xml.sig` \*
* Entity header: `/<entity-ID>/entity.xml` (with an `Entity` element as document
  root)
* Entity signature: `/<entity-ID>/entity.xml.sig` \*
* Provenance parameter set and entity value: `/<entity-ID>/<name>`
* Provenance parameter set and entity value signature: `/<entity-ID>/<name>.sig`
  \*

\* For description of signature files see sub-section 
[Security Mechanisms](#security-mechanisms).

##### Values and Provenance Parameter Sets

Values and provenance parameter sets are stored inside the sub-directory of an 
entity in individual storage files. Every value or provenance parameter must be
serialized to an octet stream to be stored in a storage file. The file name of 
the storage file corresponds to the name of the value and provenance parameter 
set respectively.

The integrity and authenticity of values and provenance parameter sets is 
ensured by signature files (see sub-section 
[Security Mechanisms](#security-mechanisms)).

##### Entity and Entity Header

Every entity is stored in a sub-directory (level 2) in the container. 
The name of the sub-directory corresponds to the ID of the entity. The ID of 
an entity must be unique inside the container and is build by a running integer
number starting at zero. For the naming of the sub-directories, the entity ID 
is encoded in five decimal places with leading zeroes. As a result, the 
maximum number of entities in a container is 100.000.

The entity header is encoded in a XML document `entity.xml`, inside the 
sub-directory of the entity, with an `Entity` element as document root. 
The schema of the `Entity` element is specified by the following XML schema 
snippet:

    <xs:element name="Entity">
      <xs:complexType>
        <xs:sequence>
          <xs:element name="Id" type="ENTITYID" minOccurs="1" maxOccurs="1" />
          <xs:element name="Label" type="xs:string" minOccurs="0" maxOccurs="1" />
          <xs:element name="Predecessors" type="IDLIST" minOccurs="1" maxOccurs="1" />
          <xs:element name="Type" type="GUID" minOccurs="0" maxOccurs="1" />
          <xs:element name="Provenance" minOccurs="1" maxOccurs="1">
            <xs:complexType>
              <xs:sequence>
                <xs:element name="Guid" type="GUID" minOccurs="0" maxOccurs="1" />
                <xs:element name="ProvenanceDescription" type="PROVENANCEDESCRIPTION" minOccurs="0" maxOccurs="1" />
              </xs:sequence>
            </xs:complexType>
          </xs:element>
          <xs:element name="ParameterSet" type="VALUEREFERENCE" minOccurs="0" maxOccurs="1" />
          <xs:element name="Values" minOccurs="1" maxOccurs="1">
            <xs:complexType>
              <xs:sequence>
                <xs:element name="Value" type="VALUEREFERENCE" minOccurs="0" maxOccurs="unbounded" />
              </xs:sequence>
            </xs:complexType>
          </xs:element>
        </xs:sequence>
      </xs:complexType>
    </xs:element>

The primitive type `ENTITYID` is just an alias for the predefined XML type int.
The type `IDLIST` defines a white-space-separated list of `ENTITID`s. For the 
specification of the primitive type `GUID` see the complete XML schema 
`odec-container.xsd`. 

The complex type `PROVENANCEDESCRIPTION` defines a description for a provenance.
It contains three different types of provenance components: a manual procedure, 
a software component and a hardware component. A provenance can be a combination
of these three.

    <xs:complexType name="PROVENANCEDESCRIPTION">
      <xs:choice minOccurs="1" maxOccurs="unbounded">
        <xs:element name="ManualProcedure">
          <xs:complexType>
            <xs:sequence>
              <xs:element name="Institute" type="xs:string" minOccurs="1" maxOccurs="1" />
              <xs:element name="Operator" type="xs:string" minOccurs="1" maxOccurs="1" />
              <xs:element name="Role" type="xs:string" minOccurs="0" maxOccurs="1" />
              <xs:element name="Email" type="xs:string" minOccurs="1" maxOccurs="1" />
              <xs:element name="Procedure" type="xs:string" minOccurs="1" maxOccurs="1" />
            </xs:sequence>
          </xs:complexType>
        </xs:element>
        <xs:element name="SoftwareComponent">
          <xs:complexType>
            <xs:sequence>
              <xs:element name="MainComponent" type="SOFTWARECOMPONENT" minOccurs="1" maxOccurs="1" />
              <xs:element name="SubComponent" type="SOFTWARECOMPONENT" minOccurs="0" maxOccurs="unbounded" />
            </xs:sequence>
          </xs:complexType>
        </xs:element>
        <xs:element name="HardwareComponent">
          <xs:complexType>
            <xs:sequence>
              <xs:element name="Producer" type="xs:string" minOccurs="1" maxOccurs="1" />
              <xs:element name="ProductName" type="xs:string" minOccurs="1" maxOccurs="1" />
              <xs:element name="ProductVersion" type="xs:string" minOccurs="1" maxOccurs="1" />
              <xs:element name="SerialNumber" type="xs:string" minOccurs="1" maxOccurs="1" />
              <xs:element name="Configuration" type="xs:string" minOccurs="0" maxOccurs="1" />
            </xs:sequence>
          </xs:complexType>
        </xs:element>
      </xs:choice>
    </xs:complexType>

A software component can be a combination of one main component and a various 
number of sub-components. They are defined by the complex type 
`SOFTWARECOMPONENT`.

    <xs:complexType name="SOFTWARECOMPONENT">
      <xs:sequence>
        <xs:element name="Producer" type="xs:string" minOccurs="1" maxOccurs="1" />
        <xs:element name="ProductName" type="xs:string" minOccurs="1" maxOccurs="1" />
        <xs:element name="ProductVersion" type="xs:string" minOccurs="1" maxOccurs="1" />
      </xs:sequence>
    </xs:complexType>

If a software component is used to process evidence in a forensic process the 
producer of the software should name a number of supported platforms and must 
verify deterministic and equivalent behavior of the software for all supported 
platforms including operating systems and hardware. Further, the producer should
keep a product history associating a product name and version number with all 
deployed files including a copy of every deployed file.

The provenance parameter set and the entity values are referenced in the entity
by a XML structure, specified by the complex type `VALUEREFERENCE`.

The type `VALUEREFERENCE` is defined by the following XML schema snippet:

    <xs:complexType name="VALUEREFERENCE">
      <xs:sequence>
        <xs:element name="Name" type="xs:string" minOccurs="1" maxOccurs="1" />
        <xs:element name="Type" type="GUID" minOccurs="0" maxOccurs="1" />
        <xs:element name="Size" type="xs:long" minOccurs="1" maxOccurs="1" />
        <xs:element name="Appearance" type="VALUEAPPEARANCE" minOccurs="1" maxOccurs="1" />
        <xs:element name="ValueSignature" type="SIGNATUREWRAPPER" minOccurs="1" maxOccurs="1" />
      </xs:sequence>
    </xs:complexType>

The appearance of a value is defined by a primitive type called 
`VALUEAPPEARANCE`, which is an enumeration of the following strings: 
`plain`, `encrypted`, `suppressed`. The value reference contains a copy of the 
value signature, which is wrapped in an element called `ValueSignature`. 
This element is specified by the type `SIGNATUREWRAPPER`.

The integrity and authenticity of an entity header including all value and 
provenance parameter set signatures is ensured by a signature file 
`entity.xml.sig` inside the sub-directory of the entity. For further information
about signature files see sub-section 
[Security Mechanisms](#security-mechanisms).

##### Entity Index

The entity index is encoded in a XML document with an `Index` element as 
document root and stored in the storage file `/index.xml`.

    <xs:element name="Index">
      <xs:complexType>
        <xs:sequence>
          <xs:element name="LastId" minOccurs="1" maxOccurs="1" type="ENTITYID" />
          <xs:element name="IndexItem" minOccurs="1" maxOccurs="unbounded">
            <xs:complexType>
              <xs:sequence>
                <xs:element name="Id" type="ENTITYID" minOccurs="1" maxOccurs="1" />
                <xs:element name="Label" type="xs:string" minOccurs="0" maxOccurs="1" />
                <xs:element name="Successors" minOccurs="1" maxOccurs="1" type="IDLIST" />
                <xs:element name="EntitySignature" type="SIGNATUREWRAPPER" minOccurs="1" maxOccurs="1" />
              </xs:sequence>
            </xs:complexType>
          </xs:element>
        </xs:sequence>
      </xs:complexType>
    </xs:element>

The `Index` element has at first one `LastId` element containing the last used 
entity ID to support the running integer number for the entity ID. 
At second it has any number but at least one of `IndexItem` elements as 
children. An `IndexItem` element contains the ID of the referenced entity, 
a list with successor IDs and a copy of the entity signature in the 
`EntitySignature` element.

The integrity and authenticity of the index including the entity signature 
copies is ensured by the signature file `/index.xml.sig`. For further 
information about signature files see sub-section 
[Security Mechanisms](#security-mechanisms).

##### Current Edition

The current edition is encoded in a XML document with an `Edition` element as 
document root and stored in the storage file `/edition.xml`. 
The Edition element is specified by the following XML schema snippet:

    <xs:element name="Edition">
      <xs:complexType>
        <xs:sequence>
          <xs:element name="Guid" type="GUID" minOccurs="1" maxOccurs="1" />
          <xs:element name="Salt" type="xs:string" minOccurs="0" maxOccurs="1" />
          <xs:element name="Software" type="xs:string" minOccurs="1" maxOccurs="1" />
          <xs:element name="Profile" type="xs:string" minOccurs="0" maxOccurs="1" />
          <xs:element name="Version" type="xs:string" minOccurs="0" maxOccurs="1" />
          <xs:element name="Timestamp" type="xs:dateTime" minOccurs="1" maxOccurs="1" />
          <xs:element name="Owner" type="OWNER" minOccurs="1" maxOccurs="1"  />
          <xs:element name="Copyright" type="xs:string" minOccurs="0" maxOccurs="1" />
          <xs:element name="Comments" type="xs:string" minOccurs="0" maxOccurs="1" />
          <xs:element name="AddedEntities" type="IDLIST" minOccurs="1" maxOccurs="1" />
          <xs:element name="RemovedEntities" type="IDLIST" minOccurs="1" maxOccurs="1" />
          <xs:element name="HistorySignature" type="SIGNATUREWRAPPER" minOccurs="1" maxOccurs="1" />
          <xs:element name="IndexSignature" type="SIGNATUREWRAPPER" minOccurs="1" maxOccurs="1" />
        </xs:sequence>
      </xs:complexType>
    </xs:element>

Every edition is identified by a global unique identifier stored in the element
`Guid`. For the specification of the primitive type `GUID` see the complete 
XML schema `odec-container.xsd`. The `Timestamp` element contains the moment 
of creation of the edition. It is encoded following the predefined XML type 
`dateTime` (ISO 8601).

The salt of the edition is stored in the element `Salt`. The salt can be any 
kind of string but to avoid problems with non ASCII characters and white spaces 
it is strongly recommended to use a random octet stream encoded with the BASE64 
scheme.

The element `Software` contains a character string identifying the exact version
of the software component responsible for forming the container. The optional 
elements `Profile` and `Version` can be used to link the container to a 
container profile with data type, entity type and provenance definitions. 
Therefore, the `Profile` element contains the name of the profile and the 
`Version` element contains a character string allowing the distinction of 
different versions of the profile. The usage of a profile is strictly 
recommended to allow automated validation of the container content.

The elements `Copyright` and `Comments` are optional. The elements 
`AddedEntities` and `RemovedEntites` are referencing all entities, which are 
added to or removed from the container. According to the primitive type 
`IDLIST`, the IDs of the referenced entities are concatenated as a 
white-space-separated list.

To chain the trust for the integrity and authenticity of the container history 
and the entity index to the current edition, a copy of the history signature 
and a copy of the index signature are embedded into the current edition while 
using the complex type `SIGNATUREWRAPPER`. For detailed information about 
signatures see sub-section [Security Mechanisms](#security-mechanisms).

The `Owner` element is specified by the complex type `OWNER`, which is defined 
by the following XML schema snippet:

    <xs:complexType name="OWNER">
      <xs:sequence>
        <xs:element name="Institute" type="xs:string" minOccurs="1" maxOccurs="1" />
        <xs:element name="Operator" type="xs:string" minOccurs="1" maxOccurs="1" />
        <xs:element name="Role" type="xs:string" minOccurs="0" maxOccurs="1" />
        <xs:element name="Email" type="xs:string" minOccurs="1" maxOccurs="1" />
        <xs:element name="PostalAddress" type="POSTALADDRESS" minOccurs="0" maxOccurs="1" />
        <xs:element name="X509Certificate" type="xs:string" minOccurs="1" maxOccurs="1" />
      </xs:sequence>
    </xs:complexType>

The owner is described by an institute name, an operator, and an email for 
contact. Optional a role and a postal address can be given. For the 
specification of the complex type `POSTALADDRESS` see the complete XML schema 
`odec-container.xsd`. Further the X.509 certificate of the owner is embedded 
PEM encoded.

To ensure the integrity and authenticity of the current edition including the 
signatures of history and index, a signature file `/edition.xml.sig` is stored 
in the container. This signature file represents the *master signature* of the 
container. For further information about signature files see section 
[Security Mechanisms](#security-mechanisms).

##### Container History

The history of the container is encoded in a XML document with a `History` 
element as document root and stored in the storage file `/history.xml`. 
The `History` element is specified by the following XML schema snippet:

    <xs:element name="History">
      <xs:complexType>
        <xs:sequence>
          <xs:element name="HistoryItem" minOccurs="0" maxOccurs="unbounded">
            <xs:complexType>
              <xs:sequence>
                <xs:element ref="Edition" minOccurs="1" maxOccurs="1" />
                <xs:element name="PastMasterSignature" type="SIGNATUREWRAPPER" minOccurs="1" maxOccurs="1" />
              </xs:sequence>
            </xs:complexType>
          </xs:element>
        </xs:sequence>
      </xs:complexType>
    </xs:element>

The `History` element has any number of `HistoryItem` elements as children. 
A `HistoryItem` element contains the complete `Edition` element describing a 
past edition and the past editions signature embedded in the 
`PastMasterSignature` element. 

The integrity and authenticity of the container history is ensured by the 
signature file `/history.xml.sig`. For further information about signature 
files see sub-section [Security Mechanisms](#security-mechanisms).

#### Security Mechanisms

The security of storage files in the container and in fact the security of the 
whole container is ensured in two steps: At first with a cryptographic hash 
(or digest) to ensure the integrity, and at second with a signature to ensure 
the authenticity. To simplify the validation of the container, one generic 
approach is reused over the whole container. The granularity of the security 
protection corresponds to the granularity of the storage files in the container.
Or in other words, every storage file is secured with a signed cryptographic
hash.

For that purpose, signature files are used. A signature file is a file placed 
beside the signed file and named after the signed file with a postfix of `.sig`
attached. The signature file of an entity header e.g. `/00001/entity.xml`, 
is placed inside the sub-directory of the entity and is named accordingly to 
the rule, described above: `/00001/entity.xml.sig`.

The content of the signature file is a core XML signature according to the W3C 
standard `http://www.w3.org/TR/xmldsig-core/`. The root of the XML signature 
file is the `Signature` element from the XML namespace 
`http://www.w3.org/2000/09/xmldsig#`. Some restrictions are applied to the XML
signature:

* Only one `Reference` element is allowed in the `SignedInfo` element
* The `Reference` element must use the child element `URI`
* The content of the `URI` element in the `Reference` element must be a relative
  path to the signed storage file (only the name of the signed file)
* The X.509 certificate of the signing owner is not included in a `KeyInfo`
  element

The following algorithms are currently supported as digest methods:

* MD5 `http://www.w3.org/2001/04/xmldsig-more#md5`
* SHA-1 `http://www.w3.org/2000/09/xmldsig#sha1`
* SHA-256 `http://www.w3.org/2001/04/xmlenc#sha256`
* SHA-384 `http://www.w3.org/2001/04/xmldsig-more#sha384`
* SHA-512 `http://www.w3.org/2001/04/xmlenc#sha512`
* RIPEMD-160 `X-RipeMD-160`
* RIPEMD-256 `X-RipeMD-256`
* RIPEMD-320 `X-RipeMD-320`
* WHIRLPOOL `http://www.w3.org/2007/05/xmldsig-more#whirlpool`

The following canonicalization algorithms for XML are currently supported:

* C14N without comments `http://www.w3.org/TR/2001/REC-xml-c14n-20010315`

The following signature algorithms are currently supported:

* RSA with SHA-1 `http://www.w3.org/2000/09/xmldsig#rsa-sha1`

If a signature file is signing one of the XML files, representing the container 
structure (`edition.xml`, `history.xml`, `index.xml` and `entity.xml`), 
the signed XML document is canonicalized with the C14N algorithm (without 
regarding comments in the document) before calculating the digest. This way, 
irrelevant changes in these documents (e.g. inserting comments or changing 
XML attribute order) do not destroy the integrity and authenticity.

If a signature file is signing a provenance parameter set or an entity file, 
no canonicalization takes place, despite the fact, that some of these values 
are XML documents as well.

To embed a copy of a signature inside other XML documents, the 
`SIGNATUREWRAPPER` type is defined in the XML schema for the container 
`odec-container.xsd`. Essentially a XML element of the type `SIGNATUREWRAPPER` 
contains a `Signature` child element from the XML namespace 
`http://www.w3.org/2000/09/xmldsig#`.

To create the salt of an edition, a cryptographic random generator must be used.
Otherwise, reinstating a history edition with removed salt is potentially 
possible. Especially the use of a pseudo random generator initialized with a 
time stamp is considered a serious security thread. This is because the edition 
contains a time stamp which can be used as a starting point for a brute force 
attack to rebuild the salt.

### Implementation of Profiles

In this section, a way for describing a container profile in XML is specified. 
Even if it is not strictly necessary, it is very useful to create a profile 
for the generic container (see chapter "The Framework"). This way, the correct 
usage of the container in a process can be verified.

The XML elements of profiles are defined by a XML Schema `profile.xsd`, which 
can be used for automated validation. The XML Schema is targeting the XML 
namespace `http://www.mastersign.de/odec/profiles/`. In the following code 
snippets the prefix `xs` is used for the XML namespace of the XML schema 
`http://www.w3.org/2001/XMLSchema`.

A container profile splits in three catalogs (see chapter "The Framework"): 
*data type catalog*, *entity type catalog* and *provenance interface catalog*. 
The whole profile can be defined in one XML file. The following XML schema 
snippet specifies the root element `Profile`:

    <xs:element name="Profile">
      <xs:complexType>
        <xs:sequence>
          <xs:element ref="Name" minOccurs="1" maxOccurs="1" />
          <xs:element ref="Version" minOccurs="1" maxOccurs="1" />
          <xs:element ref="Description" minOccurs="0" maxOccurs="1" />
          <xs:element ref="DataTypeCatalog" minOccurs="1" maxOccurs="1" />
          <xs:element ref="EntityTypeCatalog" minOccurs="1" maxOccurs="1" />
          <xs:element ref="ProvenanceInterfaceCatalog" minOccurs="1" maxOccurs="1" />
        </xs:sequence>
      </xs:complexType>
    </xs:element>

Despite the three catalog elements, a profile contains three elements to 
identify and comment the profile: 

* The element `Name` contains a string to identify a profile. 
* With the `Version` element a version management can be realized. 
  A container can only be validated against a profile, if name and version are 
  matching the details in the current edition of the container. 
* With the `Description` element further human readable information can be 
  attached to the profile.

#### Data Type Definitions

In this subsection the XML elements to define the data type catalog are 
described. The `DataTypeCatalogue` element is a list of data file format 
definitions as `DataType` elements. It is specified by the following XML schema
snippet:

    <xs:element name="DataTypeCatalog">
      <xs:complexType>
        <xs:sequence>
          <xs:element ref="DataType" minOccurs="0" maxOccurs="unbounded" />
        </xs:sequence>
      </xs:complexType>
    </xs:element>

A file format is defined by a `DataType` element, which is specified by the 
following XML schema snippet:

    <xs:element name="DataType">
      <xs:complexType>
        <xs:complexContent>
          <xs:extension base="CATALOG_ITEM">
            <xs:sequence>
              <xs:element name="MimeType" type="xs:string" minOccurs="0" maxOccurs="1" />
              <xs:element name="Format" minOccurs="1" maxOccurs="1">
                <xs:complexType>
                  <xs:choice minOccurs="1" maxOccurs="1">
                    <xs:element ref="CustomDefinition" />
                    <xs:element ref="TextDefinition" />
                    <xs:element ref="WinIniDefinition" />
                    <xs:element ref="CsvDefinition" />
                    <xs:element ref="XmlDefinition" />
                    <xs:element ref="ArrayDefinition" />
                    <xs:element ref="PictureDefinition" />
                  </xs:choice>
                </xs:complexType>
              </xs:element>
            </xs:sequence>
          </xs:extension>
        </xs:complexContent>
      </xs:complexType>
    </xs:element>

The `DataType` element is derived from the XML type `CATALOGUE_ITEM`, which 
defines a required attribute `guid` for storing the global unique identifier of 
the data type, a required child element `Name` to store a human readable name 
and an optional child element `Description` to attach additional information.

The `DataType` element itself defines the child elements `MimeType` and 
`Format`. The optional element `MimeType` can be used to specify a MIME-Type 
for the format. The `Format` element is a container for one out of a list of 
format definition alternatives. The format definitions are building a derivation
tree. Currently, the following format definition elements are supported:

* **CustomDefinition**
  On the upside, this element can be used to describe 
  every possible file format, because it can contain any XML element and 
  any content, even simple text. On the downside no file format validation is 
  possible with a custom definition.
* **TextDefinition**
  This definition is used to specify a plain text file. 
  Optional the encoding (code page) of the text format can be specified.
  * **WinIniDefinition**
    With this definition, a Windows-INI format can be 
    specified. The content of the file cannot be validated.
  * **CsvDefinition**
    With this definition, a comma separated value format can 
    be specified. Therefore, it contains a list (Columns) of `Column` elements 
    with a `Name` and a `Type`. The following values are valid column types: 
    `UByte`, `SByte`, `UInt16`, `SInt16`, `UInt32`, `SInt32`, `UInt64`, 
    `SInt64`, `Single`, `Double`, `Boolean`, and `String`.
  * **XmlDefinition** With this definition, a XML format can be specified. 
    Therefore, an URL to a XML schema file can be specified with the child 
    element `XmlSchema`.
* **PictureDefinition** This definition is used to specify the format of a 
  picture file. Therefore, one of the following picture types can be specified
  in the child element `PictureType`: `JPEG`, `PNG`, `BMP`, `GIF`, `TIFF`, 
  `SVG`, `EPS`, `WMF`.
* **ArrayDefinition** The array definition can be used to specify the file 
  format for binary encoded numeric arrays of one or more dimensions. A binary 
  encoded numeric array consists of two parts: the header and the body. The 
  header contains the size of the array, for each dimension one numeric value 
  in the format, specified by the XML element `HeaderType`. The body contains 
  the values of the array, every elementary value in the format, specified by 
  the XML element `ElementType`. For `HeaderType` and `ElementType`, one of the 
  following values is allowed: `UByte`, `SByte`, `UInt16`, `SInt16`, `UInt32`, 
  `SInt32`, `UInt64`, `SInt64`, `Single`, `Double`, and `Boolean`.

Example for an `ArrayDefinition`: 

* `Dimensions` = 2
* `HeaderType` = `UInt16`
* `ElementType` = `Single`. 

The header is 4 bytes long. The first two bytes contain the number of columns, 
the second two bytes contain the number of rows: 
e.g. `0x00`, `0x02`, `0x00`, `0x03` → 2 columns and 3 rows. 
The body is 2 * 3 * 4 bytes (`Single` → 4 bytes per value) → 24 bytes long and 
contains 6 32 bit floating point values.

For further information about the `DataType` element see the XML schema 
`odec-profile.xsd`.

#### Entity Type Definitions

In this sub-section the XML elements to define the entity type catalog are 
described. The `EntityTypeCatalog` element is a list of entity type 
definitions as `EntityType` elements. It is specified by the following XML 
schema snippet:

    <xs:element name="EntityTypeCatalog">
      <xs:complexType>
        <xs:sequence>
          <xs:element ref="EntityType" minOccurs="0" maxOccurs="unbounded" />
        </xs:sequence>
      </xs:complexType>
    </xs:element>

An entity type is defined by a `EntityType` element, which is specified by the 
following XML schema snippet:

    <xs:element name="EntityType">
      <xs:complexType>
        <xs:complexContent>
          <xs:extension base="CATALOG_ITEM">
            <xs:sequence>
              <xs:element name="Values" minOccurs="1" maxOccurs="1">
                <xs:complexType>
                  <xs:sequence>
                    <xs:element name="Value" minOccurs="1" maxOccurs="unbounded">
                      <xs:complexType>
                        <xs:sequence>
                          <xs:element ref="Name" minOccurs="1" maxOccurs="1" />
                          <xs:element ref="Description" minOccurs="0" maxOccurs="1" />
                          <xs:element name="DataTypeRef" type="GUID" minOccurs="1" maxOccurs="1" />
                          <xs:element name="Usage" type="USAGE" minOccurs="1" maxOccurs="1" />
                        </xs:sequence>
                      </xs:complexType>
                    </xs:element>
                  </xs:sequence>
                  <xs:attribute name="severity" type="SEVERITY" use="optional" default="strict" />
                </xs:complexType>
              </xs:element>
            </xs:sequence>
          </xs:extension>
        </xs:complexContent>
      </xs:complexType>
    </xs:element>

The `EntityType` element is derived from the XML type `CATALOG_ITEM`, which 
defines a required attribute `guid` for storing the global unique identifier of 
the entity type, a required child element `Name` to store a human readable name 
and an optional child element `Description` to attach additional information.

The `EntityType` element itself defines the child element `Values`, which is a 
list of `Value` elements. A `Value` element describes an entity value for the 
entity type. An entity value is defined by a name, which is stored in the `Name`
element. Optional a description can be attached in the `Description` element. 
Further, the data type of the value is specified by its `GUID` in the 
`DataTypeRef` element. The usage of the value is stored in the element `Usage` 
and can either be required or optional.

The element `Values` has the optional attribute `severity` which controls the 
validation mode for the entity type. A value of `strict` means that only 
specified entity values are allowed. A value of `open` means, that more than the 
specified entity values are allowed. If the attribute `severity` is omitted, 
the default value `strict` is assumed.

For further information about the `DataType` element see the XML schema 
`odec-profile.xsd`.

#### Provenance Definitions

In this subsection the XML elements to define the provenance interface catalog
are described. The `ProvenanceInterfaceCatalog` element is a list of provenance
interface definitions as `ProvenanceInterface` elements. It is specified by the 
following XML schema snippet:

    <xs:element name="ProvenanceInterfaceCatalog">
      <xs:complexType>
        <xs:sequence>
          <xs:element ref="ProvenanceInterface" minOccurs="0" maxOccurs="unbounded" />
        </xs:sequence>
      </xs:complexType>
    </xs:element>

A provenance interface is defined by a `ProvenanceInterface` element, which is 
specified by the following XML schema snippet:

    <xs:element name="ProvenanceInterface">
      <xs:complexType>
        <xs:complexContent>
          <xs:extension base="CATALOG_ITEM">
            <xs:sequence>
              <xs:element name="Input" minOccurs="1" maxOccurs="1">
                <xs:complexType>
                  <xs:sequence>
                    <xs:element name="EntityTypeRef" type="GUID" minOccurs="0" maxOccurs="unbounded" />
                  </xs:sequence>
                </xs:complexType>
              </xs:element>
              <xs:element name="Output" minOccurs="1" maxOccurs="1">
                <xs:complexType>
                  <xs:sequence>
                    <xs:element name="EntityTypeRef" type="GUID" minOccurs="1" maxOccurs="1" />
                  </xs:sequence>
                </xs:complexType>
              </xs:element>
              <xs:element name="ParameterSet" minOccurs="0" maxOccurs="1">
                <xs:complexType>
                  <xs:sequence>
                    <xs:element ref="Name" minOccurs="1" maxOccurs="1" />
                    <xs:element ref="Description" minOccurs="0" maxOccurs="1" />
                    <xs:element name="DataTypeRef" type="GUID" minOccurs="1" maxOccurs="1" />
                  </xs:sequence>
                </xs:complexType>
              </xs:element>
            </xs:sequence>
          </xs:extension>
        </xs:complexContent>
      </xs:complexType>
    </xs:element>

The `ProvenanceInterface` element is derived from the XML type `CATALOG_ITEM`,
which defines a required attribute `guid` for storing the global unique 
identifier of the provenance interface, a required child element `Name` to store
a human readable name and an optional child element `Description` to attach 
additional information.

The `ProvenanceInterface` element itself defines the child elements `Input`, 
`Output` and `ParameterSet`. The `Input` and `Output` elements are lists of 
`EntityTypeRef` elements, which contain the `GUID`, referencing an entity type.
The `Input` list can contain none by many `EntityTypeRef` elements. The `Output`
list contains precisely one `EntityTypeRef` element. If the provenance is 
configurable and the configuration is provided as a file, the element 
`ParameterSet` describes its name of the parameter set file with the `Name` 
element, the file format with the `DataTypeRef` element and optional additional 
information with the `Description` element.

For further information about the `DataType` element see the XML schema 
`odec-profile.xsd`.

## References

* **KVL2011** Kiertscher, Tobias; Vielhauer, Claus; Leich, Marcus: *Automated 
              Forensic Fingerprint Analysis: A Novel Generic Process Model and 
              Container Format*. In: Lecture Notes in Computer Science. 2011. 
              Volume 6583/2011. pp 262-273. Springer, Heidelberg. 
              DOI 10.1007/978-3-642-19530-3_24. ISBN 978-3-642-19529-7. 
              The original publication is available at 
              <http://www.springerlink.com>.

[KVL2011]: http://informatik.fh-brandenburg.de/~kiertsch/publication/KVL_GenericProcessAndContainerFormat_2011-01-14.pdf
    "Automated Forensic Fingerprint Analysis: A Novel Generic Process Model and Container Format"

[fig:example-process-graph]: figures/example-process-graph.png
[fig:structure]: figures/structure.png
[fig:structure-simplified]: figures/structure-simplified.png
[fig:framework]: figures/framework.png
[fig:trust-hierarchy]: figures/trust-hierarchy.png
[fig:history]: figures/history.png
[fig:index]: figures/index.png
[fig:file-structure]: figures/file-structure.png
[fig:detailed-container]: figures/detailed-container.png
[fig:detailed-edition]: figures/detailed-edition.png
[fig:detailed-entity-header]: figures/detailed-entity-header.png
[fig:detailed-history]: figures/detailed-history.png
[fig:detailed-history-item]: figures/detailed-history-item.png
[fig:detailed-index]: figures/detailed-index.png
[fig:detailed-index-item]: figures/detailed-index-item.png
[fig:detailed-owner]: figures/detailed-owner.png
[fig:detailed-value]: figures/detailed-value.png
[fig:detailed-value-reference]: figures/detailed-value-reference.png
