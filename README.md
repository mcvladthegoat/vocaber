# Vocaber

Vocaber helps you to build russian vocabulary from english source.
Feature list:

  - Automatic translating from english to russian via Yandex.Translator
  - Categorizing translated words in 3 categories (verified, "not sure", "bad list")
  - Saving your progress in one project file without any dependencies
  - Merging files

Other information will be here soon.
## How to use
### Intro
Click `File` - `Import plain vocabulary` to load source list in english for the fist time. Program will translate all lines. The result will be displayed on the left side `All words`
On the right side you can observe "bad words", which couldn't be translated via Yandex.

### How to work with left and right panes?
There are 3 states of each word with binded keyboard shortcuts: `verified [F2]`, `not sure [F3]`,  `bad word [F4]`.  Double-click on word in column `Translated` to change it. Also you can copy source word and "verify" it at once - just press `[F4]`. Or you can verify by regexp (`Edit` - `Auto verify by pattern`) .

### Saving project file and exporting
Choose `File` - `Save` (or `Save as`) to save project in xml serialized file. If you want to export your vocabulary  **with** spaces in the lines' beginnings, choose `File` - `Export Formatted vocabulary`. Otherwise, choose `Export vocabulary`. See the difference:

Formatted version:
```  
  "AFN" {
    "Афгани"
    "Афганская валюта"
    "Валюта Афганистана"
    "AFN"
  }
  "ALL" {
    "Валюта Албании"
    "Албанская валюта"
    "Албанский лек"
```

Standart version:
```
"AFN" {
"Афгани"
"Афганская валюта"
"Валюта Афганистана"
"AFN"
}
"ALL" {
"Валюта Албании"
"Албанская валюта"
"Албанский лек"
```
### Bonus
`Tools` - `Merge` can be used to merge text files in two clicks.

### What else?
 - [ ] Undo - Redo actions stack
 - [ ] Splitting files by user rules
 - [ ] Importing structured sources without user actions
 - [ ] Fixing words endings (morphology)
 - [ ] UI customizing, full screen mode
 - [ ] Support "compressed-like" vocabularies
 - [x] Language selection support
