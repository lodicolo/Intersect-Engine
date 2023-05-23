using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Intersect.Compression;
using Intersect.Extensions;
using Intersect.Updater.Packing;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

#pragma warning disable CS1591

namespace Intersect.Update.Generator
{
    public enum FreeRectChoiceHeuristic
    {
        RectBestShortSideFit,

        ///< -BSSF: Positions the rectangle against the short side of a free rectangle into which it fits the best.
        RectBestLongSideFit,

        ///< -BLSF: Positions the rectangle against the long side of a free rectangle into which it fits the best.
        RectBestAreaFit,

        ///< -BAF: Positions the rectangle into the smallest free rect into which it fits.
        RectBottomLeftRule,

        ///< -BL: Does the Tetris placement.
        RectContactPointRule ///< -CP: Choosest the placement where the rectangle touches other rects as much as possible.
    }

    public class TextureAtlas
    {
        private readonly bool _allowRotations;

        private readonly List<Rectangle> _freeRectangles = new List<Rectangle>();

        private readonly uint _height;

        private readonly string _targetDirectory;

        private readonly Dictionary<string, PackedImage> _textures = new Dictionary<string, PackedImage>();

        private readonly List<Rectangle> _usedRectangles = new List<Rectangle>();

        private readonly uint _width;

        public TextureAtlas(string targetDirectory, uint width, uint height, bool allowRotations = true)
        {
            _targetDirectory = targetDirectory;
            _width = width;
            _height = height;
            _allowRotations = allowRotations;
            _freeRectangles.Add(
                new Rectangle
                {
                    X = 0,
                    Y = 0,
                    Width = (int)width,
                    Height = (int)height
                }
            );
        }

        public bool TryInsert(string path, string name, out Image<Rgba32> image)
        {
            image = Image.Load<Rgba32>(path);
            var bounds = Insert(image.Width, image.Height, FreeRectChoiceHeuristic.RectBestAreaFit);
            if (bounds.Height < 1 || bounds.Width < 1)
            {
                return false;
            }

            _textures.Add(name, new PackedImage(bounds, image));

            return true;
        }

        public bool TryInsert(string path, string name, Image<Rgba32> image)
        {
            var bounds = Insert(image.Width, image.Height, FreeRectChoiceHeuristic.RectBestAreaFit);
            if (bounds.Height < 1 || bounds.Width < 1)
            {
                return false;
            }

            _textures.Add(name, new PackedImage(bounds, image));

            return true;
        }

        public Task Export(
            string targetDirectory,
            string packAssetFileNameTemplate,
            string packMetadataFileNameTemplate,
            int index,
            bool debug
        )
        {
            return Task.Run(
                () =>
                {
                    // TODO: Trim Edges
                    var atlasTexture = new Image<Rgba32>((int)_width, (int)_height);
                    var maxUsedWidth = 0;
                    var maxUsedHeight = 0;
                    var frames = new List<PackedTextureInfo>();

                    foreach (var (name, (bounds, image)) in _textures)
                    {
                        var frame = new PackedTextureInfo
                        {
                            Filename = name,
                            SpriteSourceSize = new PackedTextureBounds
                            {
                                Width = image.Width,
                                Height = image.Width
                            },
                            SourceSize = new PackedTextureBounds
                            {
                                Width = image.Width,
                                Height = image.Width
                            },
                            Rotated = bounds.Width != bounds.Height &&
                                      bounds.Width == image.Height &&
                                      bounds.Height == image.Width,
                            Frame = new PackedTextureBounds
                            {
                                X = bounds.X,
                                Y = bounds.Y,
                                Width = bounds.Width,
                                Height = bounds.Height
                            }
                        };

                        if (frame.Rotated)
                        {
                            image.Mutate(context => context.RotateFlip(RotateMode.Rotate90, FlipMode.None));
                        }

                        atlasTexture.Mutate(
                            context => context.DrawImage(image, new SixLabors.ImageSharp.Point(bounds.X, bounds.Y), 1f)
                        );

                        maxUsedWidth = Math.Max(maxUsedWidth, bounds.Right);
                        maxUsedHeight = Math.Max(maxUsedHeight, bounds.Bottom);

                        frames.Add(frame);
                    }

                    if (maxUsedWidth <= 0 || maxUsedHeight <= 0)
                    {
                        atlasTexture.Dispose();
                    }

                    atlasTexture.Mutate(context => context.Crop(new Rectangle(0, 0, maxUsedWidth, maxUsedHeight)));

                    var indexString = index.ToString(CultureInfo.InvariantCulture);

                    var atlasInfo = new PackedAtlas
                    {
                        Frames = frames,
                        Metadata = new PackedAtlasMetadata
                        {
                            Name = packAssetFileNameTemplate.Replace("{0}", indexString),
                            Size = new PackedTextureBounds
                            {
                                Width = atlasTexture.Width,
                                Height = atlasTexture.Height
                            }
                        }
                    };

                    var atlasInfoJson = JsonConvert.SerializeObject(atlasInfo);

                    var assetPath = Path.Combine(targetDirectory, atlasInfo.Metadata.Name);
                    using (var stream = GzipCompression.CreateCompressedFileStream(assetPath))
                    {
                        atlasTexture.Save(stream, new PngEncoder());
                    }

                    if (debug)
                    {
                        atlasTexture.Save($"{assetPath}.png");
                    }

                    atlasTexture.Dispose();

                    var metaPath = Path.Combine(
                        _targetDirectory,
                        packMetadataFileNameTemplate.Replace("{0}", indexString)
                    );
                    GzipCompression.WriteCompressedString(metaPath, atlasInfoJson);
                    if (debug)
                    {
                        File.WriteAllText($"{metaPath}.json", atlasInfoJson);
                    }
                }
            );
        }

        private Rectangle Insert(int width, int height, FreeRectChoiceHeuristic method)
        {
            Rectangle newNode;
            var score1 = 0; // Unused in this function. We don't need to know the score after finding the position.
            var score2 = 0;
            switch (method)
            {
                case FreeRectChoiceHeuristic.RectBestShortSideFit:
                    newNode = FindPositionForNewNodeBestShortSideFit(width, height, out score1, ref score2);

                    break;
                case FreeRectChoiceHeuristic.RectBottomLeftRule:
                    newNode = FindPositionForNewNodeBottomLeft(width, height, ref score1, ref score2);

                    break;
                case FreeRectChoiceHeuristic.RectContactPointRule:
                    newNode = FindPositionForNewNodeContactPoint(width, height, out score1);

                    break;
                case FreeRectChoiceHeuristic.RectBestLongSideFit:
                    newNode = FindPositionForNewNodeBestLongSideFit(width, height, ref score2, out score1);

                    break;
                case FreeRectChoiceHeuristic.RectBestAreaFit:
                    newNode = FindPositionForNewNodeBestAreaFit(width, height, out score1, ref score2);

                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(method), method, null);
            }

            if (newNode.Height == 0)
            {
                return newNode;
            }

            var numRectanglesToProcess = _freeRectangles.Count;
            for (var rectangleIndex = 0; rectangleIndex < numRectanglesToProcess; ++rectangleIndex)
            {
                if (!SplitFreeNode(_freeRectangles[rectangleIndex], ref newNode))
                {
                    continue;
                }

                _freeRectangles.RemoveAt(rectangleIndex);
                --rectangleIndex;
                --numRectanglesToProcess;
            }

            PruneFreeList();

            _usedRectangles.Add(newNode);

            return newNode;
        }

        // public void Insert(List<Rectangle> rects, List<Rectangle> dst, FreeRectChoiceHeuristic method)
        // {
        //     dst.Clear();
        //
        //     while (rects.Count > 0)
        //     {
        //         var bestScore1 = int.MaxValue;
        //         var bestScore2 = int.MaxValue;
        //         var bestRectIndex = -1;
        //         var bestNode = new Rectangle();
        //
        //         for (var i = 0; i < rects.Count; ++i)
        //         {
        //             var score1 = 0;
        //             var score2 = 0;
        //             var newNode = ScoreRect(rects[i].Width, rects[i].Height, method, ref score1, ref score2);
        //
        //             if (score1 < bestScore1 || (score1 == bestScore1 && score2 < bestScore2))
        //             {
        //                 bestScore1 = score1;
        //                 bestScore2 = score2;
        //                 bestNode = newNode;
        //                 bestRectIndex = i;
        //             }
        //         }
        //
        //         if (bestRectIndex == -1)
        //         {
        //             return;
        //         }
        //
        //         PlaceRect(bestNode);
        //         rects.RemoveAt(bestRectIndex);
        //     }
        // }

        // private void PlaceRect(Rectangle node)
        // {
        //     var numRectanglesToProcess = _freeRectangles.Count;
        //     for (var i = 0; i < numRectanglesToProcess; ++i)
        //     {
        //         if (SplitFreeNode(_freeRectangles[i], ref node))
        //         {
        //             _freeRectangles.RemoveAt(i);
        //             --i;
        //             --numRectanglesToProcess;
        //         }
        //     }
        //
        //     PruneFreeList();
        //
        //     _usedRectangles.Add(node);
        // }
        //
        // private Rectangle ScoreRect(
        //     int width,
        //     int height,
        //     FreeRectChoiceHeuristic method,
        //     ref int score1,
        //     ref int score2
        // )
        // {
        //     var newNode = new Rectangle();
        //     score1 = int.MaxValue;
        //     score2 = int.MaxValue;
        //     switch (method)
        //     {
        //         case FreeRectChoiceHeuristic.RectBestShortSideFit:
        //             newNode = FindPositionForNewNodeBestShortSideFit(width, height, ref score1, ref score2);
        //
        //             break;
        //         case FreeRectChoiceHeuristic.RectBottomLeftRule:
        //             newNode = FindPositionForNewNodeBottomLeft(width, height, ref score1, ref score2);
        //
        //             break;
        //         case FreeRectChoiceHeuristic.RectContactPointRule:
        //             newNode = FindPositionForNewNodeContactPoint(width, height, ref score1);
        //             score1 = -score1; // Reverse since we are minimizing, but for contact point score bigger is better.
        //
        //             break;
        //         case FreeRectChoiceHeuristic.RectBestLongSideFit:
        //             newNode = FindPositionForNewNodeBestLongSideFit(width, height, ref score2, ref score1);
        //
        //             break;
        //         case FreeRectChoiceHeuristic.RectBestAreaFit:
        //             newNode = FindPositionForNewNodeBestAreaFit(width, height, ref score1, ref score2);
        //
        //             break;
        //     }
        //
        //     // Cannot fit the current rectangle.
        //     if (newNode.Height == 0)
        //     {
        //         score1 = int.MaxValue;
        //         score2 = int.MaxValue;
        //     }
        //
        //     return newNode;
        // }
        //
        // /// Computes the ratio of used surface area.
        // public float Occupancy()
        // {
        //     ulong usedSurfaceArea = 0;
        //     for (var i = 0; i < _usedRectangles.Count; ++i)
        //     {
        //         usedSurfaceArea += (uint)_usedRectangles[i].Width * (uint)_usedRectangles[i].Height;
        //     }
        //
        //     return (float)usedSurfaceArea / (_width * _height);
        // }

        private Rectangle FindPositionForNewNodeBottomLeft(int width, int height, ref int bestY, ref int bestX)
        {
            var bestNode = new Rectangle();

            //memset(bestNode, 0, sizeof(Rect));

            bestY = int.MaxValue;

            for (var i = 0; i < _freeRectangles.Count; ++i)
            {
                // Try to place the rectangle in upright (non-flipped) orientation.
                if (_freeRectangles[i].Width >= width && _freeRectangles[i].Height >= height)
                {
                    var topSideY = _freeRectangles[i].Y + height;
                    if (topSideY < bestY || (topSideY == bestY && _freeRectangles[i].X < bestX))
                    {
                        bestNode.X = _freeRectangles[i].X;
                        bestNode.Y = _freeRectangles[i].Y;
                        bestNode.Width = width;
                        bestNode.Height = height;
                        bestY = topSideY;
                        bestX = _freeRectangles[i].X;
                    }
                }

                if (_allowRotations && _freeRectangles[i].Width >= height && _freeRectangles[i].Height >= width)
                {
                    var topSideY = _freeRectangles[i].Y + width;
                    if (topSideY < bestY || (topSideY == bestY && _freeRectangles[i].X < bestX))
                    {
                        bestNode.X = _freeRectangles[i].X;
                        bestNode.Y = _freeRectangles[i].Y;
                        bestNode.Width = height;
                        bestNode.Height = width;
                        bestY = topSideY;
                        bestX = _freeRectangles[i].X;
                    }
                }
            }

            return bestNode;
        }

        private Rectangle FindPositionForNewNodeBestShortSideFit(
            int width,
            int height,
            out int bestShortSideFit,
            ref int bestLongSideFit
        )
        {
            var bestNode = new Rectangle();

            bestShortSideFit = int.MaxValue;

            foreach (var freeRectangle in _freeRectangles)
            {
                // Try to place the rectangle in upright (non-flipped) orientation.
                if (freeRectangle.Width >= width && freeRectangle.Height >= height)
                {
                    var leftoverHoriz = Math.Abs(freeRectangle.Width - width);
                    var leftoverVert = Math.Abs(freeRectangle.Height - height);
                    var shortSideFit = Math.Min(leftoverHoriz, leftoverVert);
                    var longSideFit = Math.Max(leftoverHoriz, leftoverVert);

                    if (shortSideFit < bestShortSideFit ||
                        (shortSideFit == bestShortSideFit && longSideFit < bestLongSideFit))
                    {
                        bestNode.X = freeRectangle.X;
                        bestNode.Y = freeRectangle.Y;
                        bestNode.Width = width;
                        bestNode.Height = height;
                        bestShortSideFit = shortSideFit;
                        bestLongSideFit = longSideFit;
                    }
                }

                if (!_allowRotations || freeRectangle.Width < height || freeRectangle.Height < width)
                {
                    continue;
                }

                var flippedLeftoverHoriz = Math.Abs(freeRectangle.Width - height);
                var flippedLeftoverVert = Math.Abs(freeRectangle.Height - width);
                var flippedShortSideFit = Math.Min(flippedLeftoverHoriz, flippedLeftoverVert);
                var flippedLongSideFit = Math.Max(flippedLeftoverHoriz, flippedLeftoverVert);

                if (flippedShortSideFit >= bestShortSideFit &&
                    (flippedShortSideFit != bestShortSideFit || flippedLongSideFit >= bestLongSideFit))
                {
                    continue;
                }

                bestNode.X = freeRectangle.X;
                bestNode.Y = freeRectangle.Y;
                bestNode.Width = height;
                bestNode.Height = width;
                bestShortSideFit = flippedShortSideFit;
                bestLongSideFit = flippedLongSideFit;
            }

            return bestNode;
        }

        private Rectangle FindPositionForNewNodeBestLongSideFit(
            int width,
            int height,
            ref int bestShortSideFit,
            out int bestLongSideFit
        )
        {
            var bestNode = new Rectangle();

            bestLongSideFit = int.MaxValue;

            foreach (var freeRectangle in _freeRectangles)
            {
                // Try to place the rectangle in upright (non-flipped) orientation.
                if (freeRectangle.Width >= width && freeRectangle.Height >= height)
                {
                    var leftoverHoriz = Math.Abs(freeRectangle.Width - width);
                    var leftoverVert = Math.Abs(freeRectangle.Height - height);
                    var shortSideFit = Math.Min(leftoverHoriz, leftoverVert);
                    var longSideFit = Math.Max(leftoverHoriz, leftoverVert);

                    if (longSideFit < bestLongSideFit ||
                        (longSideFit == bestLongSideFit && shortSideFit < bestShortSideFit))
                    {
                        bestNode.X = freeRectangle.X;
                        bestNode.Y = freeRectangle.Y;
                        bestNode.Width = width;
                        bestNode.Height = height;
                        bestShortSideFit = shortSideFit;
                        bestLongSideFit = longSideFit;
                    }
                }

                if (!_allowRotations || freeRectangle.Width < height || freeRectangle.Height < width)
                {
                    continue;
                }

                var flippedLeftoverHoriz = Math.Abs(freeRectangle.Width - height);
                var flippedLeftoverVert = Math.Abs(freeRectangle.Height - width);
                var flippedShortSideFit = Math.Min(flippedLeftoverHoriz, flippedLeftoverVert);
                var flippedLongSideFit = Math.Max(flippedLeftoverHoriz, flippedLeftoverVert);

                if (flippedLongSideFit >= bestLongSideFit &&
                    (flippedLongSideFit != bestLongSideFit || flippedShortSideFit >= bestShortSideFit))
                {
                    continue;
                }

                bestNode.X = freeRectangle.X;
                bestNode.Y = freeRectangle.Y;
                bestNode.Width = height;
                bestNode.Height = width;
                bestShortSideFit = flippedShortSideFit;
                bestLongSideFit = flippedLongSideFit;
            }

            return bestNode;
        }

        private Rectangle FindPositionForNewNodeBestAreaFit(
            int width,
            int height,
            out int bestAreaFit,
            ref int bestShortSideFit
        )
        {
            var bestNode = new Rectangle();

            bestAreaFit = int.MaxValue;

            foreach (var freeRectangle in _freeRectangles)
            {
                var areaFit = (freeRectangle.Width * freeRectangle.Height) - (width * height);

                // Try to place the rectangle in upright (non-flipped) orientation.
                if (freeRectangle.Width >= width && freeRectangle.Height >= height)
                {
                    var leftoverHoriz = Math.Abs(freeRectangle.Width - width);
                    var leftoverVert = Math.Abs(freeRectangle.Height - height);
                    var shortSideFit = Math.Min(leftoverHoriz, leftoverVert);

                    if (areaFit < bestAreaFit || (areaFit == bestAreaFit && shortSideFit < bestShortSideFit))
                    {
                        bestNode.X = freeRectangle.X;
                        bestNode.Y = freeRectangle.Y;
                        bestNode.Width = width;
                        bestNode.Height = height;
                        bestShortSideFit = shortSideFit;
                        bestAreaFit = areaFit;
                    }
                }

                if (!_allowRotations || freeRectangle.Width < height || freeRectangle.Height < width)
                {
                    continue;
                }

                var flippedLeftoverHoriz = Math.Abs(freeRectangle.Width - height);
                var flippedLeftoverVert = Math.Abs(freeRectangle.Height - width);
                var flippedShortSideFit = Math.Min(flippedLeftoverHoriz, flippedLeftoverVert);

                if (areaFit >= bestAreaFit && (areaFit != bestAreaFit || flippedShortSideFit >= bestShortSideFit))
                {
                    continue;
                }

                bestNode.X = freeRectangle.X;
                bestNode.Y = freeRectangle.Y;
                bestNode.Width = height;
                bestNode.Height = width;
                bestShortSideFit = flippedShortSideFit;
                bestAreaFit = areaFit;
            }

            return bestNode;
        }

        /// Returns 0 if the two intervals i1 and i2 are disjoint, or the length of their overlap otherwise.
        private static int CommonIntervalLength(int i1start, int i1end, int i2start, int i2end)
        {
            if (i1end < i2start || i2end < i1start)
            {
                return 0;
            }

            return Math.Min(i1end, i2end) - Math.Max(i1start, i2start);
        }

        private int ContactPointScoreNode(int x, int y, int width, int height)
        {
            var score = 0;

            if (x == 0 || x + width == _width)
            {
                score += height;
            }

            if (y == 0 || y + height == _height)
            {
                score += width;
            }

            foreach (var usedRectangle in _usedRectangles)
            {
                if (usedRectangle.X == x + width || usedRectangle.X + usedRectangle.Width == x)
                {
                    score += CommonIntervalLength(
                        usedRectangle.Y,
                        usedRectangle.Y + usedRectangle.Height,
                        y,
                        y + height
                    );
                }

                if (usedRectangle.Y == y + height || usedRectangle.Y + usedRectangle.Height == y)
                {
                    score += CommonIntervalLength(usedRectangle.X, usedRectangle.X + usedRectangle.Width, x, x + width);
                }
            }

            return score;
        }

        private Rectangle FindPositionForNewNodeContactPoint(int width, int height, out int bestContactScore)
        {
            var bestNode = new Rectangle();

            bestContactScore = -1;

            foreach (var freeRectangle in _freeRectangles)
            {
                // Try to place the rectangle in upright (non-flipped) orientation.
                int score;
                if (freeRectangle.Width >= width && freeRectangle.Height >= height)
                {
                    score = ContactPointScoreNode(freeRectangle.X, freeRectangle.Y, width, height);

                    if (score > bestContactScore)
                    {
                        bestNode.X = freeRectangle.X;
                        bestNode.Y = freeRectangle.Y;
                        bestNode.Width = width;
                        bestNode.Height = height;
                        bestContactScore = score;
                    }
                }

                if (!_allowRotations || freeRectangle.Width < height || freeRectangle.Height < width)
                {
                    continue;
                }

                score = ContactPointScoreNode(freeRectangle.X, freeRectangle.Y, height, width);

                if (score <= bestContactScore)
                {
                    continue;
                }

                bestNode.X = freeRectangle.X;
                bestNode.Y = freeRectangle.Y;
                bestNode.Width = height;
                bestNode.Height = width;
                bestContactScore = score;
            }

            return bestNode;
        }

        private bool SplitFreeNode(Rectangle freeNode, ref Rectangle usedNode)
        {
            // Test with SAT if the rectangles even intersect.
            if (usedNode.X >= freeNode.X + freeNode.Width ||
                usedNode.X + usedNode.Width <= freeNode.X ||
                usedNode.Y >= freeNode.Y + freeNode.Height ||
                usedNode.Y + usedNode.Height <= freeNode.Y)
            {
                return false;
            }

            if (usedNode.X < freeNode.X + freeNode.Width && usedNode.X + usedNode.Width > freeNode.X)
            {
                // New node at the top side of the used node.
                if (usedNode.Y > freeNode.Y && usedNode.Y < freeNode.Y + freeNode.Height)
                {
                    var newNode = freeNode;
                    newNode.Height = usedNode.Y - newNode.Y;
                    _freeRectangles.Add(newNode);
                }

                // New node at the bottom side of the used node.
                if (usedNode.Y + usedNode.Height < freeNode.Y + freeNode.Height)
                {
                    var newNode = freeNode;
                    newNode.Y = usedNode.Y + usedNode.Height;
                    newNode.Height = freeNode.Y + freeNode.Height - (usedNode.Y + usedNode.Height);
                    _freeRectangles.Add(newNode);
                }
            }

            if (usedNode.Y < freeNode.Y + freeNode.Height && usedNode.Y + usedNode.Height > freeNode.Y)
            {
                // New node at the left side of the used node.
                if (usedNode.X > freeNode.X && usedNode.X < freeNode.X + freeNode.Width)
                {
                    var newNode = freeNode;
                    newNode.Width = usedNode.X - newNode.X;
                    _freeRectangles.Add(newNode);
                }

                // New node at the right side of the used node.
                if (usedNode.X + usedNode.Width < freeNode.X + freeNode.Width)
                {
                    var newNode = freeNode;
                    newNode.X = usedNode.X + usedNode.Width;
                    newNode.Width = freeNode.X + freeNode.Width - (usedNode.X + usedNode.Width);
                    _freeRectangles.Add(newNode);
                }
            }

            return true;
        }

        private void PruneFreeList()
        {
            for (var i = 0; i < _freeRectangles.Count; ++i)
            for (var j = i + 1; j < _freeRectangles.Count; ++j)
            {
                if (IsContainedIn(_freeRectangles[i], _freeRectangles[j]))
                {
                    _freeRectangles.RemoveAt(i);
                    --i;

                    break;
                }

                if (!IsContainedIn(_freeRectangles[j], _freeRectangles[i]))
                {
                    continue;
                }

                _freeRectangles.RemoveAt(j);
                --j;
            }
        }

        private bool IsContainedIn(Rectangle a, Rectangle b)
        {
            return a.X >= b.X && a.Y >= b.Y && a.X + a.Width <= b.X + b.Width && a.Y + a.Height <= b.Y + b.Height;
        }
    }
}