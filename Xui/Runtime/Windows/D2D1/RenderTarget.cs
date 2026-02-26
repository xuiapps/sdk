using System;
using System.Runtime.InteropServices;
using static Xui.Runtime.Windows.DWrite;

namespace Xui.Runtime.Windows;

public static partial class D2D1
{
    public unsafe class RenderTarget : Resource
    {
        public static new readonly Guid IID = new Guid("2CD90694-12E2-11DC-9FED-001143A055f9");

        private static readonly BrushProperties defaultBrushProperties = new BrushProperties() { Opacity = 1, Transform = new Matrix3X2F() { _11 = 1, _12 = 0, _21 = 1, _22 = 0, _31 = 0, _32 = 0} };

        public RenderTarget(void* ptr) : base(ptr)
        {
        }

        /// <summary>
        /// Creates a bitmap brush that paints with a tiled or clamped bitmap.
        /// Wraps <c>ID2D1RenderTarget::CreateBitmapBrush</c> (vtable [7]).
        /// </summary>
        public Brush.Ptr CreateBitmapBrushPtr(Bitmap1 bitmap, in BitmapBrushProperties bbProps, in BrushProperties brushProps)
        {
            void* brush;
            fixed (BitmapBrushProperties* bbPtr = &bbProps)
            fixed (BrushProperties* bpPtr = &brushProps)
            {
                Marshal.ThrowExceptionForHR(
                    ((delegate* unmanaged[MemberFunction]<void*, void*, BitmapBrushProperties*, BrushProperties*, void**, int>)this[7])
                    (this, bitmap, bbPtr, bpPtr, &brush));
            }
            return new Brush.Ptr(brush);
        }

        public SolidColorBrush CreateSolidColorBrush(in ColorF color)
        {
            void* brush;
            fixed (BrushProperties* bpPtr = &defaultBrushProperties)
            fixed (ColorF* colorPtr = &color)
            {
                Marshal.ThrowExceptionForHR(((delegate* unmanaged[MemberFunction]<void*, ColorF*, BrushProperties*, void**, int> )this[8])(this, colorPtr, bpPtr, &brush));
            }
            return new SolidColorBrush(brush);
        }

        public Brush.Ptr CreateSolidColorBrushPtr(in ColorF color)
        {
            void* brush;
            fixed (BrushProperties* bpPtr = &defaultBrushProperties)
            fixed (ColorF* colorPtr = &color)
            {
                Marshal.ThrowExceptionForHR(((delegate* unmanaged[MemberFunction]<void*, ColorF*, BrushProperties*, void**, int> )this[8])(this, colorPtr, bpPtr, &brush));
            }
            return new Brush.Ptr(brush);
        }

        public void* CreateGradientStopCollectionPtr(ReadOnlySpan<GradientStop> gradientStops, Gamma gamma = Gamma.Gamma_2_2, ExtendMode extendMode = ExtendMode.Clamp)
        {
            void* gradientStopCollectionPtr;
            uint gradientStopsCount = (uint)gradientStops.Length;
            fixed(GradientStop* gradientStopsPtr = gradientStops)
            {
                Marshal.ThrowExceptionForHR(((delegate* unmanaged[MemberFunction]<void*, GradientStop*, uint, Gamma, ExtendMode, void**, int>)this[9])(this, gradientStopsPtr, gradientStopsCount, gamma, extendMode, &gradientStopCollectionPtr));
            }
            return gradientStopCollectionPtr;
        }

        public GradientStopCollection CreateGradientStopCollection(ReadOnlySpan<GradientStop> gradientStops, Gamma gamma = Gamma.Gamma_2_2, ExtendMode extendMode = ExtendMode.Clamp) =>
            new GradientStopCollection(CreateGradientStopCollectionPtr(gradientStops, gamma, extendMode));

        public LinearGradientBrush CreateLinearGradientBrush(in LinearGradientBrush.Properties linearGradientBrushProperties, in BrushProperties brushProperties, ReadOnlySpan<GradientStop> gradientStops, Gamma gamma = Gamma.Gamma_2_2, ExtendMode extendMode = ExtendMode.Clamp)
        {
            fixed(LinearGradientBrush.Properties* linearGradientBrushPropertiesPtr = &linearGradientBrushProperties)
            fixed(BrushProperties* brushPropertiesPtr = &brushProperties)
            {
                void* gradientStopCollectionPtr = CreateGradientStopCollectionPtr(gradientStops, gamma, extendMode);
                void* linearGradientBrushPtr;
                Marshal.ThrowExceptionForHR(((delegate* unmanaged[MemberFunction]<void*, LinearGradientBrush.Properties*, BrushProperties*, void*, void**, int>)this[10])(this, linearGradientBrushPropertiesPtr, brushPropertiesPtr, gradientStopCollectionPtr, &linearGradientBrushPtr));
                Release(gradientStopCollectionPtr);
                return new LinearGradientBrush(linearGradientBrushPtr, gradientStopCollectionPtr);
            }
        }

        public RadialGradientBrush CreateRadialGradientBrush(in RadialGradientBrush.Properties radialGradientBrushProperties, in BrushProperties brushProperties, ReadOnlySpan<GradientStop> gradientStops, Gamma gamma = Gamma.Gamma_2_2, ExtendMode extendMode = ExtendMode.Clamp)
        {
            fixed(RadialGradientBrush.Properties* radialGradientBrushPropertiesPtr = &radialGradientBrushProperties)
            fixed(BrushProperties* brushPropertiesPtr = &brushProperties)
            {
                void* gradientStopCollectionPtr = CreateGradientStopCollectionPtr(gradientStops, gamma, extendMode);
                void* radialGradientBrushPtr;
                Marshal.ThrowExceptionForHR(((delegate* unmanaged[MemberFunction]<void*, RadialGradientBrush.Properties*, BrushProperties*, void*, void**, int>)this[11])(this, radialGradientBrushPropertiesPtr, brushPropertiesPtr, gradientStopCollectionPtr, &radialGradientBrushPtr));
                Release(gradientStopCollectionPtr);
                return new RadialGradientBrush(radialGradientBrushPtr, gradientStopCollectionPtr);
            }
        }

        public Brush.Ptr CreateLinearGradientBrushPtr(in LinearGradientBrush.Properties linearGradientBrushProperties, in BrushProperties brushProperties, ReadOnlySpan<GradientStop> gradientStops, Gamma gamma = Gamma.Gamma_2_2, ExtendMode extendMode = ExtendMode.Clamp)
        {
            fixed(LinearGradientBrush.Properties* linearGradientBrushPropertiesPtr = &linearGradientBrushProperties)
            fixed(BrushProperties* brushPropertiesPtr = &brushProperties)
            {
                void* gradientStopCollectionPtr = CreateGradientStopCollectionPtr(gradientStops, gamma, extendMode);
                void* linearGradientBrushPtr;
                Marshal.ThrowExceptionForHR(((delegate* unmanaged[MemberFunction]<void*, LinearGradientBrush.Properties*, BrushProperties*, void*, void**, int>)this[10])(this, linearGradientBrushPropertiesPtr, brushPropertiesPtr, gradientStopCollectionPtr, &linearGradientBrushPtr));
                Release(gradientStopCollectionPtr);
                return new Brush.Ptr(linearGradientBrushPtr);
            }
        }

        public Brush.Ptr CreateRadialGradientBrushPtr(in RadialGradientBrush.Properties radialGradientBrushProperties, in BrushProperties brushProperties, ReadOnlySpan<GradientStop> gradientStops, Gamma gamma = Gamma.Gamma_2_2, ExtendMode extendMode = ExtendMode.Clamp)
        {
            fixed(RadialGradientBrush.Properties* radialGradientBrushPropertiesPtr = &radialGradientBrushProperties)
            fixed(BrushProperties* brushPropertiesPtr = &brushProperties)
            {
                void* gradientStopCollectionPtr = CreateGradientStopCollectionPtr(gradientStops, gamma, extendMode);
                void* radialGradientBrushPtr;
                Marshal.ThrowExceptionForHR(((delegate* unmanaged[MemberFunction]<void*, RadialGradientBrush.Properties*, BrushProperties*, void*, void**, int>)this[11])(this, radialGradientBrushPropertiesPtr, brushPropertiesPtr, gradientStopCollectionPtr, &radialGradientBrushPtr));
                Release(gradientStopCollectionPtr);
                return new Brush.Ptr(radialGradientBrushPtr);
            }
        }

        
        public void DrawRectangle(in RectF rect, Brush brush, float strokeWidth = 1.0f, StrokeStyle? strokeStyle = null)
        {
            fixed (RectF* rectPtr = &rect)
            {
                ((delegate* unmanaged[MemberFunction]<void*, RectF*, void*, float, void*, void>)this[16])(this, rectPtr, brush, strokeWidth, strokeStyle);
            }
        }

        public void DrawRectangle(in RectF rect, Brush.Ptr brush, float strokeWidth = 1.0f, StrokeStyle? strokeStyle = null)
        {
            fixed (RectF* rectPtr = &rect)
            {
                ((delegate* unmanaged[MemberFunction]<void*, RectF*, void*, float, void*, void>)this[16])(this, rectPtr, brush, strokeWidth, strokeStyle);
            }
        }

        public void DrawRectangle(in RectF rect, Brush.Ptr brush, float strokeWidth, StrokeStyle.Ptr strokeStyle)
        {
            fixed (RectF* rectPtr = &rect)
            {
                ((delegate* unmanaged[MemberFunction]<void*, RectF*, void*, float, void*, void>)this[16])(this, rectPtr, brush, strokeWidth, strokeStyle);
            }
        }

        public void FillRectangle(in RectF rect, Brush brush)
        {
            fixed (RectF* rectPtr = &rect)
            {
                ((delegate* unmanaged[MemberFunction]<void*, RectF*, void*, void> )this[17])(this, rectPtr, brush);
            }
        }

        public void FillRectangle(in RectF rect, Brush.Ptr brush)
        {
            fixed (RectF* rectPtr = &rect)
            {
                ((delegate* unmanaged[MemberFunction]<void*, RectF*, void*, void> )this[17])(this, rectPtr, brush);
            }
        }

        public void DrawRoundedRectangle(in RoundRect roundRect, Brush brush, float strokeWidth = 1f, StrokeStyle? strokeStyle = null)
        {
            fixed(RoundRect* roundedRectPtr = &roundRect)
            {
                ((delegate* unmanaged[MemberFunction]<void*, RoundRect*, void*, float, void*, void> )this[18])(this, roundedRectPtr, brush, strokeWidth, strokeStyle);
            }
        }

        public void DrawRoundedRectangle(in RoundRect roundRect, Brush.Ptr brush, float strokeWidth = 1f, StrokeStyle? strokeStyle = null)
        {
            fixed(RoundRect* roundedRectPtr = &roundRect)
            {
                ((delegate* unmanaged[MemberFunction]<void*, RoundRect*, void*, float, void*, void> )this[18])(this, roundedRectPtr, brush, strokeWidth, strokeStyle);
            }
        }

        public void FillRoundedRectangle(in RoundRect roundRect, Brush brush)
        {
            fixed(RoundRect* roundedRectPtr = &roundRect)
            {
                ((delegate* unmanaged[MemberFunction]<void*, RoundRect*, void*, void> )this[19])(this, roundedRectPtr, brush);
            }
        }

        public void FillRoundedRectangle(in RoundRect roundRect, Brush.Ptr brush)
        {
            fixed(RoundRect* roundedRectPtr = &roundRect)
            {
                ((delegate* unmanaged[MemberFunction]<void*, RoundRect*, void*, void> )this[19])(this, roundedRectPtr, brush);
            }
        }

        public void DrawGeometry(Geometry geometry, Brush brush, float strokeWidth = 1f, StrokeStyle? strokeStyle = null) =>
            ((delegate* unmanaged[MemberFunction]<void*, void*, void*, float, void*, void> )this[22])(this, geometry, brush, strokeWidth, strokeStyle);

        public void DrawGeometry(Geometry geometry, Brush.Ptr brush, float strokeWidth = 1f, StrokeStyle? strokeStyle = null) =>
            ((delegate* unmanaged[MemberFunction]<void*, void*, void*, float, void*, void> )this[22])(this, geometry, brush, strokeWidth, strokeStyle);

        public void DrawGeometry(PathGeometry.Ptr geometry, Brush.Ptr brush, float strokeWidth = 1f, StrokeStyle.Ptr strokeStyle = default) =>
            ((delegate* unmanaged[MemberFunction]<void*, void*, void*, float, void*, void> )this[22])(this, geometry, brush, strokeWidth, strokeStyle);

        public void FillGeometry(Geometry geometry, Brush brush, Brush? opacityBrush = null) =>
            ((delegate* unmanaged[MemberFunction]<void*, void*, void*, void*, void> )this[23])(this, geometry, brush, opacityBrush);

        public void FillGeometry(Geometry geometry, Brush.Ptr brush) =>
            ((delegate* unmanaged[MemberFunction]<void*, void*, void*, void*, void> )this[23])(this, geometry, brush, null);

        public void FillGeometry(PathGeometry.Ptr geometry, Brush.Ptr brush) =>
            ((delegate* unmanaged[MemberFunction]<void*, void*, void*, void*, void> )this[23])(this, geometry, brush, null);

        /// <summary>
        /// Draws a bitmap with the given opacity onto the render target.
        /// Wraps <c>ID2D1RenderTarget::DrawBitmap</c> (vtable [26]).
        /// Interpolation mode is always Linear (1).
        /// </summary>
        public void DrawBitmap(Bitmap1 bitmap, in RectF dest, float opacity)
        {
            fixed (RectF* destPtr = &dest)
            {
                ((delegate* unmanaged[MemberFunction]<void*, void*, RectF*, float, uint, RectF*, void>)this[26])
                    (this, bitmap, destPtr, opacity, 1u /* Linear */, null);
            }
        }

        /// <summary>
        /// Draws a sub-region of a bitmap with the given opacity onto the render target.
        /// Wraps <c>ID2D1RenderTarget::DrawBitmap</c> (vtable [26]).
        /// Interpolation mode is always Linear (1).
        /// </summary>
        public void DrawBitmap(Bitmap1 bitmap, in RectF dest, float opacity, in RectF source)
        {
            fixed (RectF* destPtr = &dest)
            fixed (RectF* srcPtr = &source)
            {
                ((delegate* unmanaged[MemberFunction]<void*, void*, RectF*, float, uint, RectF*, void>)this[26])
                    (this, bitmap, destPtr, opacity, 1u /* Linear */, srcPtr);
            }
        }

        public void DrawText(string text, TextFormat textFormat, in RectF layoutRect, Brush defaultFillBrush, DrawTextOptions options = DrawTextOptions.None, MeasuringMode measuringMode = MeasuringMode.Natural)
        {
            fixed (void* textPtr = &global::System.Runtime.InteropServices.Marshalling.Utf16StringMarshaller.GetPinnableReference(text))
            fixed (RectF* layoutRectPtr = &layoutRect)
            {
                ((delegate* unmanaged[MemberFunction]<void*, void*, uint, void*, RectF*, void*, DrawTextOptions, MeasuringMode, void>)this[27])(this, textPtr, (uint)text.Length, textFormat, layoutRectPtr, defaultFillBrush, options, measuringMode);
            }
        }

        public void DrawTextLayout(Point2F origin, DWrite.TextLayout textLayout, Brush defaultFillBrush, DrawTextOptions options = DrawTextOptions.None) =>
            ((delegate* unmanaged[MemberFunction]<void*, Point2F, void*, void*, DrawTextOptions, void>)this[28])(this, origin, textLayout, defaultFillBrush, options);

        public void DrawTextLayout(Point2F origin, DWrite.TextLayout.Ref textLayout, Brush defaultFillBrush, DrawTextOptions options = DrawTextOptions.None) =>
            ((delegate* unmanaged[MemberFunction]<void*, Point2F, void*, void*, DrawTextOptions, void>)this[28])(this, origin, textLayout, defaultFillBrush, options);

        public void DrawTextLayout(Point2F origin, DWrite.TextLayout textLayout, Brush.Ptr defaultFillBrush, DrawTextOptions options = DrawTextOptions.None) =>
            ((delegate* unmanaged[MemberFunction]<void*, Point2F, void*, void*, DrawTextOptions, void>)this[28])(this, origin, textLayout, defaultFillBrush, options);

        public void DrawTextLayout(Point2F origin, DWrite.TextLayout.Ref textLayout, Brush.Ptr defaultFillBrush, DrawTextOptions options = DrawTextOptions.None) =>
            ((delegate* unmanaged[MemberFunction]<void*, Point2F, void*, void*, DrawTextOptions, void>)this[28])(this, origin, textLayout, defaultFillBrush, options);

        public void SetTransform(in Matrix3X2F transform)
        {
            fixed(Matrix3X2F* transformPtr = &transform)
            {
                ((delegate* unmanaged[MemberFunction]<void*, Matrix3X2F*, void> )this[30])(this, transformPtr);
            }
        }

        public void GetTransform(out Matrix3X2F transform)
        {
            fixed(Matrix3X2F* transformPtr = &transform)
            {
                ((delegate* unmanaged[MemberFunction]<void*, Matrix3X2F*, void> )this[31])(this, transformPtr);
            }
        }

        public void PushLayer(in LayerParameters layoutParameters)
        {
            fixed(LayerParameters* layerParametersPtr = &layoutParameters)
            {
                ((delegate* unmanaged[MemberFunction]<void*, LayerParameters*, void*, void>)this[40])(this, layerParametersPtr, null);
            }
        }

        public void PopLayer() => ((delegate* unmanaged[MemberFunction]<void*, void>)this[41])(this);


        public void SaveDrawingState(DrawingStateBlock drawingStateBlock) =>
            ((delegate* unmanaged[MemberFunction]<void*, void*, void>)this[43])(this, drawingStateBlock);

        public void SaveDrawingState(DrawingStateBlock.Ptr drawingStateBlock) =>
            ((delegate* unmanaged[MemberFunction]<void*, void*, void>)this[43])(this, drawingStateBlock);

        public void RestoreDrawingState(DrawingStateBlock drawingStateBlock) =>
            ((delegate* unmanaged[MemberFunction]<void*, void*, void>)this[44])(this, drawingStateBlock);

        public void RestoreDrawingState(DrawingStateBlock.Ptr drawingStateBlock) =>
            ((delegate* unmanaged[MemberFunction]<void*, void*, void>)this[44])(this, drawingStateBlock);

        public void Clear(ColorF color) =>
            ((delegate* unmanaged[MemberFunction]<void*, ColorF*, void> )this[47])(this, &color);

        public void BeginDraw() =>
            ((delegate* unmanaged[MemberFunction]<void*, void> )this[48])(this);

        public void EndDraw()
        {
            ulong tag1;
            ulong tag2;
            Marshal.ThrowExceptionForHR(((delegate* unmanaged[MemberFunction]<void*, ulong*, ulong*, int>)this[49])(this, &tag1, &tag2));
        }

        /// <summary>
        /// Draws a bitmap into the given destination rectangle (vtable slot 26).
        /// Sits between FillOpacityMask (25) and DrawText (27) in the ID2D1RenderTarget vtable.
        /// interpolationMode: 0 = NearestNeighbor, 1 = Linear.
        /// </summary>
        public void DrawBitmap(Bitmap1 bitmap, in RectF destRect, float opacity = 1.0f, uint interpolationMode = 1)
        {
            fixed (RectF* destPtr = &destRect)
                ((delegate* unmanaged[MemberFunction]<void*, void*, RectF*, float, uint, RectF*, void>)this[26])
                    (this, bitmap, destPtr, opacity, interpolationMode, null);
        }

        /// <summary>
        /// Draws a cropped region of a bitmap (vtable slot 26 with source rect).
        /// </summary>
        public void DrawBitmap(Bitmap1 bitmap, in RectF destRect, in RectF srcRect, float opacity = 1.0f, uint interpolationMode = 1)
        {
            fixed (RectF* destPtr = &destRect, srcPtr = &srcRect)
                ((delegate* unmanaged[MemberFunction]<void*, void*, RectF*, float, uint, RectF*, void>)this[26])
                    (this, bitmap, destPtr, opacity, interpolationMode, srcPtr);
        }
    }
}