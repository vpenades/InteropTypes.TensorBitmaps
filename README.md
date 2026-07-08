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
all tensor configurations. It also needs to be simple enough to avoid defining an
entire framework around pixel formats.

Declaring a pixel type is done using two types:

- `TensorPixelFormat`
- `TensorPixelComponent<T>`

Where `TensorPixelComponent` is just a collection of `TensorPixelComponent` plus a few predefined formats like:

- `TensorPixelFormat.Rgb24`
- `TensorPixelFormat.Rgb96f`
- `TensorPixelFormat.Rgba32`
- etc

With this architecture, defining a custom format is extremely simple:

```c#
var infrared = new TensorPixelComponent<float>("Infrared",0,1);
var depth = new TensorPixelComponent<float>("Depth",0,500);
var customFormat = new TensorPixelFormat(infrared, depth);
```

The data type is an INumber<T>, so it supports, Byte, UShort, Half, Float and so on.

Using this approach, it is possible to handle pixel conversions between a wide range
of pixel formats.

The component class also defines a minimum and maximum value,
which can be useful to automate the conversion between tensors
that require aplying a std-mean ramp for each pixel, for example:

```csharp
var red = new TensorPixelComponent<float>("Red", -0.823, +0.7432);
var green = new TensorPixelComponent<float>("Green", -0.923, +0.9432);
var blue = new TensorPixelComponent<float>("Blue", -0.623, +0.5432);
var tensorFormat = new TensorPixelFormat(red,green,blue);
```

The only drawback of this API is that it's not easy to declare
packed pixels formats like RGB565.

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

Where redPlane, greenPlane and bluePlane represent the thee componentized planes of the tensor.

### interop with third party libraries

TensorBitmaps is only a data type to contain pixel bitmaps, so it lacks lots of features expected
from full image libraries.

In fact, it is expected to be used as long as other imaging libraries for tasks like load and save
images from disk. As an example, the Unit Tests use ImageSharp as the backing library for load and
save images










