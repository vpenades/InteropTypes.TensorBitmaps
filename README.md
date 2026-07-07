# InteropTypes.TensorBitmaps

### Overview

This project contains some bare bones classes to wrap System.Numerics.Tensors with a Bitmap API facade.

### Bitmap core types

|Tensor type|Bitmap type|
|-|-|
|`Tensor<TElement>`|`TensorBitmap<TElement,TPixel>`|
|`TensorSpan<TElement>`|`TensorSpanBitmap<TElement,TPixel>`|
|`ReadOnlyTensorSpan<TElement>`|`ReadOnlyTensorSpanBitmap<TElement,TPixel>`|

`TElement` is the type of the underlaying tensor as also a component of the `TPixel`, so for example we can do:

```csharp
struct Rgb24 { byte R; byte G; byte B; }

var bitmap1 = TensorBitmap<byte,Rgb24>.Create(256, 256, TensorPixelFormat.Rgb24); // for a pixel format that uses 3 bytes

var bitmap2 = TensorBitmap<float,Vector3>.Create(256, 256, TensorPixelFormat.Rgb96f); // for a bitmap using floating points and a pixel format defined by System.Numerics.Vector3
```

but it is also possible to do

`TensorBitmap<byte,Vector3>` for a byte based vector that uses 12 bytes casted to 3 floats

The three types have the same exact base API:

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

### pixel formats

Bitmaps usually require declaring some kind of pixel format.

The pixel format needs to be extremely flexible to support custom formats that fit
all tensor configurations. It also needs to be simple enough to avoid defining an
entire framework around pixel formats.

Declaring a pixel type is done using two types:

- `record TensorPixelFormat`
- `record TensorPixelComponent<T>`

Where `TensorPixelComponent` is just a collection of `TensorPixelComponent` plus a few predefined formats like:

- `TensorPixelFormat.Rgb24`
- `TensorPixelFormat.Rgb96f`
- `TensorPixelFormat.Rgba32`
- etc

But defining a custom format is extremely simple:

```c#
var infrared = new TensorPixelComponent<float>("Infrared",0,1);
var depth = new TensorPixelComponent<float>("Depth",0,500);
var customFormat = new TensorPixelFormat(infrared, depth);
```

The component class also defines a minimum and maximum value,
which can be useful to automate the conversion between tensors
that require aplying a standard-deviation pattern for each pixels, for example:

```csharp
var red = new TensorPixelComponent("Red", -0.823, +0.7432);
var green = new TensorPixelComponent("Green", -0.923, +0.9432);
var blue = new TensorPixelComponent("Blue", -0.623, +0.5432);
var tensorFormat = new TensorPixelFormat(red,green,blue);
```

This pixel format design also has the advantage to seamlessly translate
to CHW tensors that store each component per plane, where we would have
a bitmap per plane, and each bitmap defining a single pixel component.

For example:

```csharp
var tensor = Tensor.Create<float>(3,256,256); // CHW tensor

TensorBitmap<float,Vector3>.CreatePlanes(
    tensor,
    TensorPixelFormat.Rgb96f,
    out TensorBitmap<float,float> redPlane,
    out TensorBitmap<float,float> greenPlane,
    out TensorBitmap<float,float> bluePlane);
```

where redPlane, greenPlane and bluePlane represents each plane, and has captured each component of Rgb96f separately.






