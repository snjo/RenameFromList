# RenameFromList
*By Andreas Aakvik Gogstad 2026*

https://github.com/snjo/RenameFromList

Renames files in a directory based on a CSV list. Entries should be listed without extensions.
All matching files will be renamed / overwritten, of any file type.

RENAMEFROMLIST [file.csv] [/f file.csv] [/d directory] [/cs ,] [/sp _] [/strict or /loose] [/keep] [/del or /nodel]

	/f       /file       /csv
	   CSV file containing a list of name pairs to rename from and to (OLD,NEW)

	/d       /directory  /folder
	   The folder containing the renamable files. Defaults to the working directory

	/cs      /separator
	   Symbol used in the CSV to separate values, Default is comma

	/sp      /split
	   Split Symbol, used in file names to indicate a suffix, will match files with the first part of the name,
	   ignoring the suffix   Ex: If the symbol is _, F01_01.pdf will be treated as F01.pdf

	/loose
	   Will match and replace any file containing the OLD name with the NEW name, keeping any prefix and suffix
	   (split symbol is ignored)

	/strict
	   Will only match files using the exact name (split symbols can be used)

	/keep    /keepsuffix
	   Keep any suffixes from the Split Symbol while using Strict match mode

	/del
	   Deletes the old files (Renames files)

	/nodel
	   Don't delete the old files (Copies files)

	/noow    /nooverwrite
	   Don't overwrite file if it already exists. By default files are overwritten.
	   If the file exists and can't be overwritten, the original file is not deleted.


## Example input csv

	OLDNAME,NEWNAME
	workfile-AA,delivery-AA
	workfile-BB,delivery-BB

## Rename result, using split symbol argument _:

	RENAMEFROMLIST.EXE example.csv , _
	workfile-AA.docx        > delivery-AA.docx
	workfile-AA.pdf         > delivery-AA.pdf
	workfile-BB_1234567.txt > delivery-BB.txt