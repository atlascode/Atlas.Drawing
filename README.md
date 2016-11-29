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
| PNG           | Greyscale | 1             | N/A      | No            | No            | zlib        | No         |
| PNG           | Greyscale | 2             | N/A      | No            | No            | zlib        | No         |
| PNG           | Greyscale | 4             | N/A      | No            | No            | zlib        | No         |
| PNG           | Greyscale | 8             | N/A      | No            | No            | zlib        | No         |
| PNG           | Greyscale | 16            | N/A      | No            | No            | zlib        | No         |
| PNG           | Color     | 24            | N/A      | No            | No            | zlib        | No         |

### Currently Supported Writing Formats

| Format        | BitsPerPixel  | BitMasks | ColorPalettes | Transparency  | Compression |
| ------------- |:-------------:|:--------:|:-------------:|:-------------:|:-----------:|
| PNG           | 32            | No       | Yes           | Yes           | zlib        |