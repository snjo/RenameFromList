# RenameFromList

Renames files in a directory based on a CSV list. Entries should be listed without extensions.
All matching files will be renamed / overwritten, of any file type.

RENAMEFROMLIST [csv filename] [csv separator character] [filename split symbol]

| Argument               | Description                   |
|------------------------|-------------------------------|
|csv file                |The csv file containing names to rename from and to|
|csv separator           |Optional if not using split symbol. Defaults to comma.|
|filename split symbol   |Optional. Files in the directory will be matched using the part of the name before the sybmol. Ex: If the symbol is _, F01_01.pdf will be treated as F01.pdf|


## Example input csv
OLDNAME,NEWNAME
workfile-AA,delivery-AA
workfile-BB,delivery-BB

## Rename result, using split symbol argument _:

RENAMEFROMLIST.EXE example.csv , _
workfile-AA.docx        > delivery-AA.docx
workfile-AA.pdf         > delivery-AA.pdf
workfile-BB_1234567.txt > delivery-BB.txt