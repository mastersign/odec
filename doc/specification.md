ODEC - Specification
====================

* Author: Tobias Kiertscher <dev@mastersign.de>
* Revision: 0.1
* State: draft
* Last change: 2013-05-06

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
it reaches the form used for presentation, digital evidence it not simply 
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

<a id="fig_example-process-graph"></a>
![Example Process][fig:example-process-graph]

*Figure 1: Example process graph with a work-flow for images from a digital camera*

## The Meta-Model

In this section the meta-model of a container is described in detail.
The meta-model describes the semantic elements of a container on an abstract 
level. An overview to the meta-model is shown in [Figure 2](#fig_structure). 
In the following section, the meta-model is described bottom-up, beginning with 
the entity values up to the container.

<a id="fig_structure"></a>
![ODEC Meta-Model][fig:structure]

*Figure 2: The meta-model of a container*

### Value and Provenance Parameter

### Value Reference

### Entity Header

### Index and Index Item

### Owner

### Edition

### History and History Item

### Container

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
