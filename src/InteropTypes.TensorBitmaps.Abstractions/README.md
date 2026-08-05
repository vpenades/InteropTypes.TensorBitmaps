### Bitmap abstractions

Contains interfaces to be implemented by derived libraries

### Bitmap interface chain

|read only|read and write|
|-|-|
|`IBitmapDimensions`||
|`IReadOnlyBitmap`|`IBitmap`|
|`IReadOnlyBitmap<TPixel>`|`IBitmap<TPixel>`|
|`IBitmapReader<TSelf,TPixel>`|`IBitmapWriter<TSelf,TPixel>`|
|`IReadOnlyBitmap<TSelf,TPixel>`|`IBitmap<TSelf,TPixel>`|