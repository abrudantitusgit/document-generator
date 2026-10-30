# Fonts for generated drawings

`DejaVuSans.ttf` and `DejaVuSans-Bold.ttf` are subsets of DejaVu Sans 2.37 (license: `LICENSE-DejaVu.txt`),
reduced to Latin, Romanian (ă â î ș ț and cedilla forms) and a few electrical symbols so that every PDF
drawing embeds a small font. They are used by `Drawings/Exporters/SkiaRenderer.cs` for the PDF, PNG and DOCX exports.

To rebuild them with fontTools (`pip install fonttools`):

```sh
for n in DejaVuSans DejaVuSans-Bold; do
  pyftsubset /usr/share/fonts/truetype/dejavu/$n.ttf --output-file=$n.ttf \
    --unicodes="U+0020-007E,U+00A0-017F,U+0218-021B,U+0394,U+03A3,U+03A9,U+03BC,U+2013-2014,U+2018-201E,U+2022,U+2026,U+2190-2195,U+21C4,U+2212,U+2248,U+2264-2265" \
    --layout-features='*' --name-IDs='*' --name-legacy --notdef-outline --glyph-names
done
```

Characters outside this set render as an empty box; add their code points above if new labels need them.
