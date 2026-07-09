# InteropTypes.TensorBitmaps

### Overview

System.Numerics.Tensors is a great addition to .Net, and it is becoming the standard to interact with AI models.

But when dealing with AI Vision, the complexity of tensors is multiplied by the complexity of dealing with images.

.Net definitely needs a new Bitmap type that can be used as general purpose pixel container for a number of scenarios;
the official .Net Bitmap at System.Drawing.Common is outdated and in the process of being deprecated, and third party
libraries present their own set of problems.

In the mean time, I present my own solution: Tensor Bitmaps

This project contains some bare bones classes to wrap System.Numerics.Tensors with a Bitmap API facade.

Mapping between tensor and bitmaps:

|Tensor type|Bitmap type|
|-|-|
|`ITensor`|`ITensorBitmap`|
|`IReadOnlyTensor`|`IReadOnlyTensorBitmap`|
|`Tensor<TElement>`|`TensorBitmap<TElement,TPixel>`|
|`TensorSpan<TElement>`|`TensorSpanBitmap<TElement,TPixel>`|
|`ReadOnlyTensorSpan<TElement>`|`ReadOnlyTensorSpanBitmap<TElement,TPixel>`|

`TElement` is the type of the underlaying tensor and also a part of the `TPixel`.

For example:

```csharp
struct Rgb24 { byte R; byte G; byte B; }

// for a pixel format that uses 3 bytes
var bitmap1 = TensorBitmap<byte,Rgb24>.Create(256, 256, TensorPixelFormat.Rgb24); 

// for a bitmap using floating points and a pixel format defined by System.Numerics.Vector3
var bitmap2 = TensorBitmap<float,Vector3>.Create(256, 256, TensorPixelFormat.Rgb96f); 
```

but it is also possible to do

`TensorBitmap<byte,Vector3>` for a byte based tensor that uses 12 bytes casted to 3 floats

All the bitmap types have the same exact base API:

```csharp
class *Bitmap<TElement,TPixel>
{
    public Tensor Tensor { get; }
    public int Width { get; }
    public int Height { get; }
    public Span<TPixel> GetRowPixelsSpan(y);
    *Bitmap<TElement,TPixel> GetCropped(System.Drawing.Rectangle cropRect);
}
```

`GetCropped` returns a clipped area of the original surface without allocating new memory

### Pixel formats

Bitmaps usually require declaring some kind of pixel format.

The pixel format needs to be extremely flexible to support custom formats that fit
all tensor configurations, also it needs to handle conversions between formats.

So, pixel formats is a topic complex enough to deserve its own library, that's why
pixel formats are located at `InteropTypes.Numerics.PixelFormats`

Declaring a pixel type is done using two types:

- `PixelFormat`
- `PixelComponent<T>`

Where `PixelFormat` is just a collection of `PixelComponent` , for example:

- `PixelFormat`
  - `PixelComponent<Byte>("Red", 0, 255)`
  - `PixelComponent<Byte>("Green", 0, 255)`
  - `PixelComponent<Byte>("Blue", 0, 255)`

Predefined formats are already defined at `KnownPixelFormats` 

- `KnownPixelFormats.Rgb8`
- `KnownPixelFormats.Rgba8`
- `KnownPixelFormats.RgbF32`
- etc

Using this approach, it is possible to handle pixel conversions
between a wide range of pixel formats.

The component class also defines a minimum and maximum value,
which can be useful to automate the conversion between tensors
that require aplying a std-mean ramp for each pixel, for example:

```csharp
var infrared = new PixelComponent<double>("Infrared",0,1);
var depth = new PixelComponent<double>("Depth", 0, 6000);
var customFormat = new PixelFormat(infrared, depth);```

```csharp
var red = new PixelComponent<float>("Red", -0.823, +0.7432);
var green = new PixelComponent<float>("Green", -0.923, +0.9432);
var blue = new PixelComponent<float>("Blue", -0.623, +0.5432);
var tensorFormat = new PixelFormat(red,green,blue);
```

This architecture has a few limitations I expect to address in
the future:

- Packed formats like Rgb565 are not supported yet
- Conversions handling premultiplied values

### Planar bitmaps

This pixel format design also has the advantage to seamlessly translate
to CHW tensors that store each component per plane, where we would have
a bitmap per plane, and each bitmap defining a single pixel component.

For example:

```csharp
var tensor = Tensor.Create<float>(3,256,256); // CHW tensor

var planes = TensorSpanPlanes3<float>.Create(tensor,TensorPixelFormat.Rgb96f);

var planeRed = planes.PlaneX;
var planeGreen = planes.PlaneY;
var planeBlue = planes.PlaneZ;

```

Where redPlane, greenPlane and bluePlane represent the thee componentized
planes of the tensor.

### interop with third party libraries

TensorBitmaps is only a data type designed to store pixel bitmaps,
so it lacks lots of features expected from full image libraries.

In fact, it is expected to be used as long as other imaging libraries
for tasks like load and save images from disk. As an example, the Unit
Tests use ImageSharp as the backing library for load and save images










