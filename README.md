![51Degrees](https://51degrees.com/DesktopModules/FiftyOne/Distributor/Logo.ashx?utm_source=github&utm_medium=repository&utm_content=readme_main&utm_campaign=data-file-tool "Data rewards the curious") **Data file meta-data utility**

# Introduction

This tool reads the header information from 51Degrees data files and displays it to the user.
Simply drag and drop the file onto the window of the utility.

# Compatibility

This utility will work with the following 51Degrees device detection data files, either gziped or uncompressed:

* 3.1 Pattern
* 3.2 Pattern
* 3.4 Hash Trie
* 4.1 Hash

# Outputs

| Field name  | Description |
|------------------|-------------|
| Header structure | The structure used to store the header. This is generally the same with minor differences but the v34 Hash files use a very different structure |
| Dataset format version | The version number for this data file |
| Dataset format name | The name of the format, usually very similar to dataset format version |
| Dataset name | This will be one of: Lite (Free), Premium, Enterprise or TAC |
| Dataset guid | A unique identifier for the dataset, generated when it was created by 51Degrees' internal systems |
| Datafile guid | A unique identifier for the combination of the dataset and the mechanism used to obtain it |
| Publish date | The date that this dataset was created |
| Date of next expected update | The date when an updated dataset of the same format and name would usually be available |
| Longest string | The length of the longest string stored in this dataset |
| Total number of string values | The total number of variable length strings stored in this dataset |
| Copyright notice | The copyright notice for this data file |
