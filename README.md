# EPDoc2Json

This utility will convert the EnergyPlus LaTeX files into a json.
It reads the `Doc\*.tex` files and outputs them to `DocJson\*.json` (which is gitignored).

## Setup

The `Doc` and `DocJson` folder contain a single example of, respectively, input (LaTeX) and output (Json) to illustrate the principle.

You are responsible for populating the `Doc` folder with the LaTeX files, and the `DocJson` will be populated by running the compiled executable.

You can either symlink or copy the `EnergyPlus/doc/input-output-reference/src/overview/group*.tex` files to `EPDoc2Json\Doc`.
Build the project using Visual Studio (tested up to 2019), and run the executable to produce the JSON files (in `DocJson`) that will be used by [Ironbug](https://github.com/MingboPeng/Ironbug).
