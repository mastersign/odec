CLR State
=========

This document describes the state of the CLR implementation of ODEC.

The state of a feature is indicated by three aspects:

* **I**mplemented - Source code is written for (**L**ibrary, **C**omand line, **G**raphical UI)
* **D**ocumented - The usage is documented by means of a manual or tutorial
* **T**ested - Unit tests exist and pass

| Feature | I | D | T | Description | Code References | Comment
| :------ | - | - | - | :---------- | :-------------- | :------
| Input Formats |||| The definition and processing of input data for container creation
| Input Formats - Owner |||| Schema and input processing for an owner
| Input Formats - Owner - Providing Certificate |||| Mechanism to provide the certificate of an owner
| Input Formats - Owner - Providing Private Key |||| Mechanism to provide the private of an owner
| Input Formats - Edition |||| Schema and input processing for a new edition
| Input Formats - Entity |||| Schema and input processing for a new entity
| Input Formats - Provenance |||| Schema and input processing for a provenance description
| Input Formats - Profile |||| The definition and processing of container profile catalogs
| Input Formats - Profile - Data Types |||| Schema and input processing for the data type catalog
| Input Formats - Profile - Entity Types |||| Schema and input processing for the entity type catalog
| Input Formats - Profile - Provenance Interfaces |||| Schema and input processing for the provenance interface catalog
| Create |||| Creation of a new container
| Validate |||| Validation of certain aspects of an existing container
| Validate - Structure |||| Checking the container structure (current edition, index, history, entities)
| Validate - Structure - Format |||| Checking existence and schema conformity of XML files
| Validate - Structure - Integrity |||| Checking integrity of signatures for all elements except entity values and provenance parameter sets
| Validate - Entity Values |||| Checking entity values and provenance parameter sets
| Validate - Entity Values - Integrity |||| Checking integrity of signatures for entity values and provenance parameter sets
| Validate - Certificates |||| Checking the validity of the owner certificates
| Validate - Certificates - Date Validation |||| Checking the the validity by the current date
| Validate - Certificates - CA Validation |||| Checking the validity inside of a PKI
| Validate - Profile |||| Validate the container content by means of a specified container profile
| Validate - Profile - Entity Content |||| Check the occurrence of entity values against the entity types
| Validate - Profile - Entity Provenance |||| Check the entity provenance against the provenance interface description
| Validate - Profile - Entity Predecessors |||| Check the entity predecessors against the provenance interface descriptions
| Validate - Profile - Value Type |||| Check the content of entity values against the entity value data type
| Inspect |||| Show the content of a container
| Inspect - Container Info |||| Show general information about the container
| Inspect - Editions |||| Show information about the current edition and all history editions
| Inspect - Entities |||| Show the list of entities and entity values
| Extend |||| Add a new edition with a number of entities to an existing container
| Reinstate |||| Remove a number of editions from the container and reinstate a former edition as current edition

