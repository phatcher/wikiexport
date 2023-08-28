# Introduction

The [WikiExport project](https://github.com/phatcher/wikiexport) it a dotnet tool that allows you to export a part of, or an entire Azure DevOps wiki into a single Markdown file.

This can then be passed into a tool such as `pandoc` to convert to another format such as Word (docx) or PDF.

[![NuGet](https://img.shields.io/nuget/v/dotnet-wikiexport.svg)](https://www.nuget.org/packages/dotnet-wikiexport/)
[![Build Status](https://dev.azure.com/paulhatcher/wikiexport/_apis/build/status/phatcher.wikiexport?repoName=phatcher%2Fwikiexport&branchName=main)](https://dev.azure.com/paulhatcher/wikiexport/_build/latest?definitionId=1&repoName=phatcher%2Fwikiexport&branchName=main)

# Installation

This will install the latest official version from nuget

`dotnet tool install dotnet-wikiexport -g`

# Getting started

Once you have installed the tool, clone your wiki repository and then enter the following command

`wikiexport -s <WikiPath> -f <WikiFile> -t <OutputPath>`

This will produce a single markdown file containing the file and any of its associated directories into the output folder. We also copy any attachments into a local attachments folder and fix up the references in the wiki text.

We adjust the raw markdown in a number of ways - most of which can be changed by setting the appropriate option values.

1. We infer the wiki root from the topmost folder containing a **.order** file, this means we can work on code wikis as a well as the default DevOps one
1. The **project title** is inferred from the wiki folder name e.g. My Project.wiki -> My Project
1. The **document title** is inferred from the project title and the file passed to the tool e.g. given **My Wiki** and a file **Security** we make the title **My Wiki Security**
1. We adjust titles with content files according to their nesting level e.g. if a file contains a H2 heading and it is two levels down it is changed to a H4
1. We automatically detect sections starting with "Appendix" as appendices and adjust their heading levels, see below for more details.

## Options

The options default settings have been initialized to values that "just work", if need something different you should be able to adjust the option settings to suit; if not raise a ticket and we can see how to help.

* *-p* *--project*: Project name defaults to the wiki root (less .wiki if present)
* *--title*: Document title to use, defaults to **Project SourceFile**
* *--titleFormat*: Defaults to "{project} {title}" allows specification of different order + special characters.
* *-a* *--author*: Author name, used as document metadata
* *-s* *--source*: Source path
* *-f* *--file*: Source file, defaults to entire Source Path if not present
* *-t* *--target*: Target path
* *-u* *--targetFile*: Target file, defaults to Document Title
* *-h* *--autoheading*: Whether to add a heading for each content file, excluding the top-most. Defaults to true
* *-l* *--autolevel*: Whether to adjust the markdown headings according to the nesting level. Defaults to true
* *-c* *--retainCaption*: Whether to retain caption headings. Defaults to false
* *--appendix*: Whether we automatically detect/process appendices. Defaults to true
* *--appendixLevel*: What heading level do appendices start from. Defaults to 6
* *--log*: Set the logging level. Defaults to Warning
* *--toc*: Whether to generate a table of contents. Defaults to true
* *-e* *--error*: Logging level considered a fatal error. Defaults to Error

## Error Handling

By default if we find an error we raise an exception and quit. This is so we when running in a DevOps pipeline, we exit and don't trigger the next stage, typically converting the markdown to docx/pdf. Otherwise as far as your concerned the generation all worked nicely and your documents will be correct.

You can choose to ignore errors such as missing `.order`, markdown files or images by setting the error level to `Critical`. The application will then make a best efforts to produce a markdown file and not raise an error.

## Wiki Formatting

To get the best results and the easiest formatting we use the following settings.

1. All headings within a wiki section should start from H2, we consider each document to be its own H1 and then automatically infer the actual level based on the folder structure 
1. Do not replicate the document title as a heading, we do that for you (see `-autoheading`) 

## Code Wikis

If you are publishing a [code wiki](https://docs.microsoft.com/en-us/azure/devops/project/wiki/publish-repo-to-wiki?view=azure-devops&tabs=browser) your image folder may not be in the repository root, e.g. the repository may look a bit like this...

```
$
  src
    ...
  wiki
    .attachments
      cat.png
    .order
    doc.md
```
For this to work as a wiki, the image references need to be like this `/wiki/.attachments/cat.png` 

It is possible to have the `.attachements` folder in

* Root of repository
* Root of code wiki
* Any sub-folder of the code wiki folder

and the tool will still convert the image references correctly on export. 

### Appendices

The markdown we output is typically an intermediate format on the way to a Word or PDF document which can provide formatting etc, but one issue is what to do with appendices such as the bibliography or glossary.

There are a couple of obvious options:

1. Just treat them as H1 section headings
1. Denote them as appendices in some way

For option 1, it is simple as there is no special processing required and the resulting markdown can be converted easily, but for option 2 it is a bit more tricky.

If your appendix is named say "Appendix - Bibliography" and we take that directly into the unified markdown, we will generate "# Appendix - Bibliography" and end up with something like "10 Appendix - Bibliography" in the Word document. However, we can take advantage of a technique to treat higher heading formats in Word as appendix numbers.

With this option we treat H6 by default as the first appendix level and our "Appendix - Bibliography" will generate "###### Bibliography" which will result in "Appendix A Bibliography" in Word, dependent on your style formatting.

## Build pipelines

Although you can run the tool locally, the most likely usage is as part of an Azure DevOps pipeline to produce your documentation on a regular or ad-hoc basis.

To assist with this, a set of sample [Azure DevOps pipelines](https://dev.azure.com/paulhatcher/wikiexport/_git/wikiexport.samples) is maintained and these cover all the use cases we have needed to date e.g.

* Extract a section or entire wiki...
* Appendix handling...
* Convert to docx, PDF or HTML...
* Using either a Windows or Linux build agent

There are examples using different LaTex distributions as these have an impact on the speed of generation due to download size.

## Local builds

After building from the root directory of the project 

`dotnet tool install dotnet-wikiexport -g --add-source build\Debug\WikiExport\net6.0`

or 

`dotnet tool update dotnet-wikiexport -g --add-source build\Debug\WikiExport\net6.0`


# License

The code is available under the [MIT License](http://en.wikipedia.org/wiki/MIT_License), for more information see the [License file][1] in the GitHub repository.

 [1]: https://github.com/phatcher/wikiexport/blob/main/License.md


# Bibliography

[Word Appendix numbers](https://shaunakelly.com/word/numbering/numberingappendixes.html)
