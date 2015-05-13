//------------------------------------------------------------------------------
//
// Copyright (C) Microsoft. All rights reserved.
//
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas.Effects;
using Windows.UI;
using Windows.UI.Composition;

namespace SlideShow
{
    partial class LayoutManager : IDisposable
    {
        public async Task Create(ContainerVisual rootContainer)
        {
            // Create a panning Visual that will be the entire size of all photos.

            _rootContainer = rootContainer;
            _compositor = rootContainer.Compositor;

            _panningContainer = _compositor.CreateContainerVisual();
            _rootContainer.Children.InsertAtTop(_panningContainer);


            // Configure the photo database and create the initial layout.

            _random = new Random();
            _allTiles = new List<Tile>();
            _farTileHistory = new List<Tile>();
            _nearTileHistory = new List<Tile>();
            _photoDatabase = new PhotoDatabase();
            await _photoDatabase.Create(_compositor.DefaultGraphicsDevice);

            CreatePlaceholderLayout();


            // Start pictures loading:
            // - Don't load all of the pictures at once, as this will create threadpool threads for
            //   each and will hammer the IO system, delaying everything.
            // - As each image arrives, connect it with a tile and start loading the next photo
            //   until all tiles have photos.

            for (int i = 0; i < ConcurrentDecodeThreads; i++)
            {
                LoadNextPhoto();
            }
        }


        public void Dispose()
        {
            _panningContainer = null;
            _rootContainer = null;
            _compositor = null;
        }


        public IReadOnlyList<Tile> AllTiles
        {
            get
            {
                return _allTiles;
            }
        }


        public ContainerVisual GridVisual
        {
            get
            {
                return _panningContainer;
            }
        }


        public Tile GetCurrentPictureFrame()
        {
            return _allTiles[_currentTileIndex];
        }


        public Tile GetFarNeighbor()
        {
            Tile chosen = null;
            int maxIterations = 20;

            do
            {
                chosen = GetFarNeighbor_MatrixLayout();

                if (maxIterations == 0)
                {
                    break;
                }
                else
                {
                    maxIterations--;
                }

                foreach (Tile recent in _farTileHistory)
                {
                    if (chosen == recent)
                    {
                        // Try again if we chose a tile that has been selected recently.
                        chosen = null;
                        break;
                    }
                }
            } while (chosen == null);

            Debug.Assert(chosen != null);

            if (_farTileHistory.Count > 10)
            {
                // Limit history to 10 tiles.
                _farTileHistory.RemoveAt(0);
            }

            _farTileHistory.Add(chosen);

            return chosen;
        }


        public Tile GetFarNeighborPreserveSelection()
        {
            Tile farTile;
            int currentIndex = _currentTileIndex;

            farTile = GetFarNeighbor();

            // GetFarNeighbor() updates _currentTileIndex, so restore it now.
            _currentTileIndex = currentIndex;

            return farTile;
        }


        public Tile GetNearNeighbor()
        {
            Tile chosen = null;
            int maxIterations = 20;

            do
            {
                int currentIndex = _currentTileIndex;
                chosen = GetNearNeighbor_MatrixLayout();

                if (maxIterations == 0)
                {
                    break;
                }
                else
                {
                    maxIterations--;
                }

                foreach (Tile recent in _nearTileHistory)
                {
                    if (chosen == recent)
                    {
                        // Restore the current index if the selection was rejected.
                        _currentTileIndex = currentIndex;

                        // Try again if we chose a tile that has been selected recently.
                        chosen = null;
                        break;
                    }
                }
            } while (chosen == null);

            Debug.Assert(chosen != null);

            if (_nearTileHistory.Count > 2)
            {
                // Limit near history to 2 tiles due to corner cases (literally).
                _nearTileHistory.RemoveAt(0);
            }

            _nearTileHistory.Add(chosen);

            return chosen;
        }


        public void UpdateWindowSize(Vector2 value)
        {
            Debug.Assert((value.X > 0) && (value.Y > 0));

            _rootContainer.Size = value;


            // Configure a "fake" perspective matrix to make further z-distance look smaller.

#if ENABLE_TESTRANGE
            Debug.Assert((NearPlane >= _nearRange) &&
                (_nearRange >= _farRange) &&
                (_farRange >= FarPlane),
                "Ensure properly ordered distances");
#endif

            var matOffsetA = Matrix4x4.CreateTranslation(-value.X / 2, -value.Y / 2, 0);

            var matShrink = new Matrix4x4(
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, 1.0f / FarPlane,
                0, 0, 0, 1);

            var matOffsetB = Matrix4x4.CreateTranslation(value.X / 2, value.Y / 2, 0);

            Matrix4x4 matTotal = matOffsetA * matShrink * matOffsetB;
            _rootContainer.TransformMatrix = matTotal;
        }


        private void LoadNextPhoto()
        {
            // Start a new image loading:
            // - At the point of starting the image loading, decrement the number of "empty" tiles,
            //   which we will use to determine if we still need to load more when each image
            //   finishes loading.
            // - If we decrement after the image has actually loaded, we may kick off too many image
            //   loads.

            var startPhoto = _photoDatabase.GetNextPhoto();
            if (startPhoto == null)
            {
                return;
            }

            var image = startPhoto.ThumbnailImage;


            // When the image has loaded, attach the owning Photo to the most recently shown Tile
            // (at _currentTileIndex):
            // - This needs to be safe for multiple threadpool threads to notify simultaneously
            //   when each image has completed.
            //
            // NOTE: To handle synchronization properly, since we are being called on the UI thread,
            // TaskScheduler.FromCurrentSynchronizationContext() to call the Task back on the UI
            // thread instead of the thread-pool thread that completed the request.

            try
            {
                image.CompleteLoadAsync().AsTask().ContinueWith(_ =>
                {
                    ProcessImageLoaded(startPhoto);
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch (Exception)
            {
                // Something went wrong when loading this image.
            }
        }


        private void ProcessImageLoaded(Photo startPhoto)
        {
            var selectedTile = _allTiles[_currentTileIndex];
            if (selectedTile.Photo == null)
            {
                selectedTile.Photo = startPhoto;
            }
            else
            {
                var nearestEmpty = FindNearestEmptyTile(selectedTile);
                if (nearestEmpty != null)
                {
                    Debug.Assert(nearestEmpty.Photo == null);
                    nearestEmpty.Photo = startPhoto;
                }
            }


            // Now that we've loaded a new Photo, if we still have empty tiles, begin loading
            // the next photo:
            // - We cannot reduce "_emptyTileCount" until after the CompleteLoadAsync()
            //   completes successfully, in case there is a problem loading any images.  This
            //   means that we may load more images than get immediately used.

            --_emptyTileCount;
            if (_emptyTileCount > 0)
            {
                LoadNextPhoto();
            }
            else
            {
                foreach (Tile checkTile in _allTiles)
                {
                    if (checkTile.Photo == null)
                    {
                        Debug.Assert(false);
                    }

                    var checkImage = checkTile.Photo.ThumbnailImage;
                    if ((checkImage.Size.Width <= 0) || (checkImage.Size.Height <= 0))
                    {
                        Debug.Assert(false);
                    }
                }
            }
        }


        private Tile FindNearestEmptyTile(Tile center)
        {
            // Iterate through all of the tiles, looking for the tile that is the closest to the
            // currently selected tile, measured by distance to the center of each tile:
            // - If none are found, return 'null'.

            Tile found = null;
            float distance = 100000000;
            var centerOffset = center.Offset;
            var centerSize = center.Size;
            centerOffset.X += centerSize.X / 2;
            centerOffset.Y += centerSize.Y / 2;

            foreach (Tile candidate in _allTiles)
            {
                if ((candidate == center) || (candidate.Photo != null))
                {
                    continue;
                }

                var candidateOffset = candidate.Offset;
                var candiateSize = candidate.Size;
                candidateOffset.X += candiateSize.X / 2;
                candidateOffset.Y += candiateSize.Y / 2;

                var delta = candidateOffset - centerOffset;
                float test = delta.Length();
                if (distance > test)
                {
                    found = candidate;
                    distance = test;
                }
            }

            return found;
        }


        private Tile GetNearNeighbor_MatrixLayout()
        {
            int curRow = _currentTileIndex / TotalColumns;
            int curCol = _currentTileIndex % TotalColumns;

            int nearRow;
            int nearCol;
            int iterationsLeft = 20;

            do
            {
                iterationsLeft--;

                nearRow = curRow + _random.Next(3) - 1;
                nearCol = curCol + _random.Next(3) - 1;
                Debug.Assert((nearRow >= curRow - 1) && (nearRow <= curRow + 1));
                Debug.Assert((nearCol >= curCol - 1) && (nearCol <= curCol + 1));

                if ((nearRow != curRow) && (nearCol != curCol))
                {
                    // Force horizontal / vertical selection only.
                    if (_random.Next(2) == 0)
                    {
                        nearRow = curRow;
                    }
                    else
                    {
                        nearCol = curCol;
                    }

                    Debug.Assert((nearRow == curRow) || (nearCol == curCol));
                }

                if ((nearRow >= 0) && (nearRow < TotalRows) &&
                    (nearCol >= 0) && (nearCol < TotalColumns) &&
                    ((nearRow != curRow) || (nearCol != curCol)))
                {
                    // Done if we've chosen a valid row/col that is different than the current.
                    break;
                }
            } while (iterationsLeft > 0);

            Debug.Assert(iterationsLeft > 0);
            if (iterationsLeft == 0)
            {
                nearRow = curRow;
                nearCol = curCol;
            }

            _currentTileIndex = (nearRow * TotalColumns) + nearCol;

            return _allTiles[_currentTileIndex];
        }


        private Tile GetFarNeighbor_MatrixLayout()
        {
            int curRow = _currentTileIndex / TotalColumns;
            int curCol = _currentTileIndex % TotalColumns;


            // Search for a far tile that (ideally) has a photo already loaded.
            //
            // NOTE: When we conduct the first search, no photo has loaded, so we will perform the
            // most expensive possible search (all iterations, all passes).  We always need to have
            // a reasonable exit.

            int newTileIndex = -1;
            Tile newTile = null;

            for (int pass = 0; pass < 2; pass++)
            {
                for (int iteration = 0; iteration < 100; iteration++)
                {
                    int farCol = _random.Next(TotalColumns);
                    int farRow = _random.Next(TotalRows);


                    // For pass 0, if the grid is sufficiently large, try to avoid tiles on the
                    // outer edge.  For later passes, we drop this restriction, as we are getting
                    // desperate.

                    if (pass == 0)
                    {
                        if ((farCol < EdgeColumns) || (farCol >= TotalColumns - EdgeColumns))
                        {
                            continue;
                        }

                        if ((farRow < EdgeRows) || (farRow >= TotalRows - EdgeRows))
                        {
                            continue;
                        }
                    }


                    // For each search, find a candidate that is a minimum distance away from the
                    // current tile.

                    int distanceSquared = ((farRow - curRow) * (farRow - curRow)) +
                        ((farCol - curCol) * (farCol - curCol));

                    if (distanceSquared >= 9)
                    {
                        // At this point, only use the new tile if it has a picture loaded.
                        // Otherwise, we want to keep searching.

                        newTileIndex = (farRow * TotalColumns) + farCol;

                        newTile = _allTiles[newTileIndex];
                        if (newTile.Photo != null)
                        {
                            break;
                        }
                    }
                }
            }


            // If we found a new tile, update current index and return.  Otherwise, fallback to
            // using our current tile.

            if (newTileIndex >= 0)
            {
                Debug.Assert(newTile != null);
                _currentTileIndex = newTileIndex;
            }

            return newTile;
        }


        private void CreatePlaceholderLayout()
        {
            CreateTilesWithImagesLayout();
        }


        private void CreateTilesWithImagesLayout()
        {
            // Create a grid of tiles.

            for (int row = 0; row < TotalRows; row++)
            {
                for (int col = 0; col < TotalColumns; col++)
                {
                    Tile tile = new Tile(_panningContainer, Border);

                    tile.GridRow = row;
                    tile.GridColumn = col;

                    tile.Size = new Vector2(PictureFrameWidth, PictureFrameHeight);

                    tile.Offset = new Vector3(
                        col * (PictureFrameWidth + Margin),
                        row * (PictureFrameHeight + Margin),
                        0);

                    tile.IsVisible = true;
                    _allTiles.Add(tile);
                    _emptyTileCount++;
                }
            }

            _panningContainer.Size = new Vector2(
                TotalColumns * (PictureFrameWidth + Margin + 2 * Border) - Margin,
                TotalRows * (PictureFrameHeight + Margin + 2 * Border) - Margin);


            // Pick the middle Tile to be the "current" one.

            _currentTileIndex = TotalRows / 2 * TotalColumns + TotalColumns / 2;
        }


        public const float NearPlane = 0.0f;
        public const float FarPlane = -400.0f;

        private const int ConcurrentDecodeThreads = 4;

        private const float Border = 10.0f;
        private const float Margin = 20.0f;
        private const float OffsetVariation = 3.0f;
        private const float PictureFrameWidth = 200.0f;
        private const float PictureFrameHeight = 150.0f;

        private const int TotalRows = 10 * 2;
        private const int TotalColumns = 10 * 2;
        private const int EdgeRows = 2;
        private const int EdgeColumns = 2;

        private Random _random;
        private PhotoDatabase _photoDatabase;
        private int _currentTileIndex;

        private Compositor _compositor;
        private ContainerVisual _rootContainer;
        private ContainerVisual _panningContainer;
        private List<Tile> _allTiles;
        private int _emptyTileCount;
        private List<Tile> _farTileHistory;
        private List<Tile> _nearTileHistory;
    }
}
