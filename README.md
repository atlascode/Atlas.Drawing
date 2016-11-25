# Atlas.Drawing
Pure .Net Core Implementation of the System.Drawing namespace

### Currently Supported Reading Formats

| Format        | BitsPerPixel  | BitMasks | ColorPalettes | Transparency  | Compression |
| ------------- |:-------------:|:--------:|:-------------:|:-------------:|:-----------:|
| ~~BMP~~       | 1             | No       | Yes           | Yes           | No          |
| ~~BMP~~       | 2             | No       | Yes           | Yes           | No          |
| BMP           | 4             | No       | Yes           | Yes           | RLE4        |
| BMP           | 8             | No       | Yes           | Yes           | RLE8        |
| ~~BMP~~       | 16            | ~~Yes~~  | Yes           | Yes           | No          |
| BMP           | 24            | No       | Yes           | No            | No          |
| BMP           | 32            | ~~Yes~~  | Yes           | No            | No          |

### Currently Supported Writing Formats

| Format        | BitsPerPixel  | BitMasks | ColorPalettes | Transparency  | Compression |
| ------------- |:-------------:|:--------:|:-------------:|:-------------:|:-----------:|
| PNG           | 32            | No       | Yes           | Yes           | zlib        |