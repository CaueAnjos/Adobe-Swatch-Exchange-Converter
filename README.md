# ase-convert

Convert Adobe Swatch Exchange (`.ase`) palettes to Adobe Color (`.aco`), plain
text (`.txt`) or hex lists (`.hex`).

## Install

**With the .NET SDK (10+):**

```sh
dotnet tool install -g AseConvert
```

Update with `dotnet tool update -g AseConvert`. To try it without installing:
`dnx AseConvert -- palettes/`.

**Standalone binary (no .NET required):** download the archive for your platform
from the [Releases](https://github.com/YOUR_USER/ase-convert/releases) page,
extract it and put `ase-convert` on your `PATH`.

On macOS, a downloaded binary is unsigned; clear the quarantine flag once with
`xattr -d com.apple.quarantine ./ase-convert`.

## Usage

```sh
ase-convert <paths...> [-f aco|txt|hex]... [-o <output folder>]
```

```sh
ase-convert palettes/                         # every .ase in the folder -> .aco next to each file
ase-convert a.ase b.ase -f txt -f hex         # several files, several formats
ase-convert palettes/ -f aco -o out/          # write everything into out/
```

Output goes next to each source file unless `-o` is given. Colors in RGB, Gray
and CMYK are converted; LAB entries are skipped with a warning.
