using Cognex.VisionPro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BeevisionSolution.Utils
{
    /// <summary>
    /// Creates a detached record tree for overlay rendering. Images and other immutable content are shared;
    /// graphic objects are cloned so changing their render properties cannot invalidate the live UI record.
    /// </summary>
    internal sealed class OverlayRecordSnapshot : IDisposable
    {
        private readonly List<IDisposable> _ownedContent = new List<IDisposable>();
        private bool _disposed;

        private OverlayRecordSnapshot(ICogRecord source, ICogImage rootImage)
        {
            try
            {
                Record = CloneRecord(source, rootImage, true);
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        public ICogRecord Record { get; private set; }

        public static OverlayRecordSnapshot Create(ICogRecord source, ICogImage rootImage)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new OverlayRecordSnapshot(source, rootImage);
        }

        private ICogRecord CloneRecord(ICogRecord source, ICogImage rootImage, bool isRoot)
        {
            object content = isRoot ? (object)rootImage : CloneGraphicContent(source.Content);
            Type contentType = content?.GetType() ?? source.ContentType;
            var clone = new Cognex.VisionPro.Implementation.CogRecord(
                source.RecordKey,
                contentType,
                source.RecordUsage,
                true,
                content,
                source.Annotation);

            if (source.SubRecords != null)
            {
                for (int index = 0; index < source.SubRecords.Count; index++)
                {
                    clone.SubRecords.Add(CloneRecord(source.SubRecords[index], null, false));
                }
            }

            return clone;
        }

        private object CloneGraphicContent(object content)
        {
            if (content is CogGraphicCollection graphicCollection)
                return TrackClone((ICloneable)graphicCollection);

            if (content is CogGraphicInteractiveCollection interactiveCollection)
                return TrackClone((ICloneable)interactiveCollection);

            if (content is ICogGraphic && content is ICloneable graphic)
                return TrackClone(graphic);

            return content;
        }

        private object TrackClone(ICloneable source)
        {
            object clone = source.Clone();
            if (clone is IDisposable disposable)
                _ownedContent.Add(disposable);
            return clone;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            Record = null;
            for (int index = _ownedContent.Count - 1; index >= 0; index--)
            {
                try
                {
                    _ownedContent[index].Dispose();
                }
                catch (Exception ex)
                {
                    Common.Bug($"Unable to dispose overlay record content: {ex.Message}");
                }
            }
            _ownedContent.Clear();
        }
    }

    /// <summary>
    /// Temporarily applies a screen-pixel line width to every graphic in a VisionPro record tree.
    /// Disposing the scope restores all original widths.
    /// </summary>
    internal sealed class OverlayGraphicLineWidthScope : IDisposable
    {
        private readonly List<GraphicWidthState> _originalWidths = new List<GraphicWidthState>();
        private readonly List<CustomPenWidthState> _originalCustomPenWidths = new List<CustomPenWidthState>();
        private readonly HashSet<object> _visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
        private bool _disposed;

        public int ModifiedCount => _originalWidths.Count + _originalCustomPenWidths.Count;

        private OverlayGraphicLineWidthScope(ICogRecord record, int width)
        {
            VisitRecord(record, Math.Max(1, Math.Min(20, width)));
        }

        public static OverlayGraphicLineWidthScope Apply(ICogRecord record, int width)
        {
            return new OverlayGraphicLineWidthScope(record, width);
        }

        private void VisitRecord(ICogRecord record, int width)
        {
            if (record == null || !_visited.Add(record))
                return;

            VisitContent(record.Content, width);

            if (record.SubRecords == null)
                return;

            foreach (object item in record.SubRecords)
            {
                VisitRecord(item as ICogRecord, width);
            }
        }

        private void VisitContent(object content, int width)
        {
            if (content == null || content is string || content is ICogImage || !_visited.Add(content))
                return;

            var graphic = content as ICogGraphic;
            if (graphic != null)
            {
                try
                {
                    int originalWidth = graphic.LineWidthInScreenPixels;
                    graphic.LineWidthInScreenPixels = width;
                    _originalWidths.Add(new GraphicWidthState(graphic, originalWidth));
                }
                catch (Exception ex)
                {
                    Common.Bug($"Unable to adjust overlay graphic line width: {ex.Message}");
                }

                var parentChild = graphic as ICogGraphicParentChild;
                if (parentChild != null)
                {
                    VisitContent(parentChild.Children, width);
                }

                var contour = graphic as CogGeneralContour;
                if (contour != null)
                {
                    AdjustCustomPenWidths(contour.OwnedCustomPens, width);
                }
            }

            var graphicChildren = content as CogGraphicChildren;
            if (graphicChildren != null)
            {
                for (int index = 0; index < graphicChildren.Count; index++)
                {
                    VisitContent(graphicChildren[index], width);
                }
                return;
            }

            var dictionary = content as IDictionary;
            if (dictionary != null)
            {
                foreach (DictionaryEntry entry in dictionary)
                {
                    VisitContent(entry.Value, width);
                }
                return;
            }

            var enumerable = content as IEnumerable;
            if (enumerable == null)
                return;

            foreach (object item in enumerable)
            {
                VisitContent(item, width);
            }
        }

        private void AdjustCustomPenWidths(ICogGraphicMultiPen customPens, int width)
        {
            if (customPens == null)
                return;

            for (int index = 0; index < customPens.PenCount; index++)
            {
                try
                {
                    int key = customPens.GetPenKey(index);
                    if (customPens.GetPenType(key) != CogGraphicMultiPenPenTypeConstants.Simple)
                        continue;

                    CogColorConstants color;
                    int originalWidth;
                    CogGraphicLineStyleConstants lineStyle;
                    customPens.GetSimplePenAttributes(
                        key, out color, out originalWidth, out lineStyle);

                    customPens.SetSimplePenAttributes(key, color, width, lineStyle);
                    _originalCustomPenWidths.Add(
                        new CustomPenWidthState(customPens, key, color, originalWidth, lineStyle));
                }
                catch (Exception ex)
                {
                    Common.Bug($"Unable to adjust overlay contour pen width: {ex.Message}");
                }
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            for (int index = _originalCustomPenWidths.Count - 1; index >= 0; index--)
            {
                CustomPenWidthState state = _originalCustomPenWidths[index];
                try
                {
                    state.CustomPens.SetSimplePenAttributes(
                        state.Key, state.Color, state.Width, state.LineStyle);
                }
                catch (Exception ex)
                {
                    Common.Bug($"Unable to restore overlay contour pen width: {ex.Message}");
                }
            }

            for (int index = _originalWidths.Count - 1; index >= 0; index--)
            {
                GraphicWidthState state = _originalWidths[index];
                try
                {
                    state.Graphic.LineWidthInScreenPixels = state.Width;
                }
                catch (Exception ex)
                {
                    Common.Bug($"Unable to restore overlay graphic line width: {ex.Message}");
                }
            }
        }

        private sealed class GraphicWidthState
        {
            public GraphicWidthState(ICogGraphic graphic, int width)
            {
                Graphic = graphic;
                Width = width;
            }

            public ICogGraphic Graphic { get; }
            public int Width { get; }
        }

        private sealed class CustomPenWidthState
        {
            public CustomPenWidthState(
                ICogGraphicMultiPen customPens,
                int key,
                CogColorConstants color,
                int width,
                CogGraphicLineStyleConstants lineStyle)
            {
                CustomPens = customPens;
                Key = key;
                Color = color;
                Width = width;
                LineStyle = lineStyle;
            }

            public ICogGraphicMultiPen CustomPens { get; }
            public int Key { get; }
            public CogColorConstants Color { get; }
            public int Width { get; }
            public CogGraphicLineStyleConstants LineStyle { get; }
        }

        private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();

            public new bool Equals(object x, object y)
            {
                return ReferenceEquals(x, y);
            }

            public int GetHashCode(object obj)
            {
                return RuntimeHelpers.GetHashCode(obj);
            }
        }
    }
}
