# Atlas.Drawing
Pure .Net Core Implementation of the System.Drawing namespace

### Currently Supported Reading Formats

| Format        | Type      | BitsPerPixel  | BitMasks | ColorPalettes | Transparency  | Compression | Interlaced |
| ------------- |:---------:|:-------------:|:--------:|:-------------:|:-------------:|:-----------:|:----------:|
| BMP           | Color     | 1             | No       | Yes           | Yes           | No          | N/A        |
| BMP           | Color     | 2             | No       | Yes           | Yes           | No          | N/A        |
| BMP           | Color     | 4             | No       | Yes           | Yes           | RLE4        | N/A        |
| BMP           | Color     | 8             | No       | Yes           | Yes           | RLE8        | N/A        |
| BMP           | Color     | 16            | Yes      | Yes           | Yes           | No          | N/A        |
| BMP           | Color     | 24            | No       | Yes           | No            | No          | N/A        |
| BMP           | Color     | 32            | Yes      | Yes           | No            | No          | N/A        |
| PNG           | Greyscale | 1             | N/A      | Yes           | Alpha Palette | zlib        | Supported  |
| PNG           | Greyscale | 2             | N/A      | Yes           | Alpha Palette | zlib        | Supported  |
| PNG           | Greyscale | 4             | N/A      | Yes           | Alpha Palette | zlib        | Supported  |
| PNG           | Greyscale | 8             | N/A      | Yes           | Alpha Palette | zlib        | Supported  |
| PNG           | Color     | 1             | N/A      | Yes           | Alpha Palette | zlib        | Supported  |
| PNG           | Color     | 2             | N/A      | Yes           | Alpha Palette | zlib        | Supported  |
| PNG           | Color     | 4             | N/A      | Yes           | Alpha Palette | zlib        | Supported  |
| PNG           | Color     | 8             | N/A      | Yes           | Alpha Palette | zlib        | Supported  |
| PNG           | Greyscale | 1             | N/A      | No            | No            | zlib        | Supported  |
| PNG           | Greyscale | 2             | N/A      | No            | No            | zlib        | Supported  |
| PNG           | Greyscale | 4             | N/A      | No            | Single Color  | zlib        | Supported  |
| PNG           | Greyscale | 8             | N/A      | No            | No            | zlib        | Supported  |
| PNG           | Greyscale | 16            | N/A      | No            | Channel       | zlib        | Supported  |
| PNG           | Greyscale | 16            | N/A      | No            | No            | zlib        | Supported  |
| PNG           | Greyscale | 32            | N/A      | No            | Channel       | zlib        | Supported  |
| PNG           | Color     | 24            | N/A      | No            | Single Color  | zlib        | Supported  |
| PNG           | Color     | 32            | N/A      | No            | Channel       | zlib        | Supported  |
| PNG           | Color     | 48            | N/A      | No            | Single Color  | zlib        | Supported  |
| PNG           | Color     | 64            | N/A      | No            | Channel       | zlib        | Supported  |

### Currently Supported Writing Formats

| Format        | BitsPerPixel  | BitMasks | ColorPalettes | Transparency  | Compression |
| ------------- |:-------------:|:--------:|:-------------:|:-------------:|:-----------:|
| PNG           | 32            | No       | Yes           | Yes           | zlib        |