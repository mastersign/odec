ODEC - Specification
====================

* Author: Tobias Kiertscher <tobias@kiertscher.net>
* Revision: 0.1
* State: draft
* Last change: 2013-05-07

## Overview

+ Introduction
+ Change Log
+ The Generic Process
+ The Meta-Model
+ The Framework
+ The Technical Implementation
+ References

## Introduction

This document contains the specification for an open digital evidence container 
based on [KVL2011][].

It splits up into the following sections: In "The Generic Process", the concept
of a processing chain for digital evidence is explained. While laying out the
generic process, a number of important terms are introduced. 
In "The Meta-Model", the structure of a container is described. This structure 
is basically independent from a technical implementation of the container, 
instead it describes the semantic items of a container and their relations.
In "The Framework", the tailoring of the generic process into an actual evidence
work-flow is described. Thereby the profiling of a generic container into a
specific container format is explained. In "The Technical Implementation" 
finally the technical aspects of the specification are described.

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

In this section the meta-model of a container is described in detail.
The meta-model describes the semantic elements of a container on an abstract 
level. An overview to the meta-model is shown in [Figure 2](#fig_structure). 
In the following section, the meta-model is described bottom-up, beginning with 
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

As described in the section "The Generic Process", the entity is a group of one 
or more entity values and optional a provenance parameter set. The entity header
contains all data necessary to describe the entity. The entity is identified 
with an ID, unique inside the container. It can optionally be marked with a 
label for easier recognition. 

Potentially, an entity type is associated with the entity, to tell which values 
are allowed or required. More on entity types can be found in section 
"The Framework". Potentially a provenance is associated with the entity and in 
case the provenance used some input entities to produce the described entity, 
these input entities are referenced by their IDs as predecessors. 
If the associated edition does not reference a profile the entity type and the
provenance can be omitted. 

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

As described in section "The Owner", the owner is an institute and/or person 
responsible for the initialization or extension of a container. The owner is 
identified by an institute name, an operator name, and an email address. 
The email address can also be used to contact the owner. Additionally, contact 
information as postal address can be given. Optional, the owner can be 
associated with a role, e.g. to support role-based access control. Every owner 
needs a certificate, which can be part of a PKI.

### Edition

<a name="fig_detailed-edition"></a>
![Edition][fig:detailed-edition]

*Figure 8: Structure of a container edition*

As described in section "The Container", every time a container is initialized 
or modified an edition is formed. A container has a *current edition* and, if it 
was modified, one or more past editions. An edition is identified by a global 
unique identifier. The edition is protected by a signature, created by the owner
of the signature. The signature of the current edition is called 
*master signature*.

The edition contains some meta-data:
* A reference to the software, used to create or extend the container
* Optional a reference to an associated container profile including a version 
  number, see section "The Framework"
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
will be discussed in section "History and History Item").

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


## Technical Implementation


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
