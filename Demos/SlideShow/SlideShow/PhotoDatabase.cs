//------------------------------------------------------------------------------
//
// Copyright (C) Microsoft. All rights reserved.
//
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Composition.Toolkit;
using Windows.Storage;
using Windows.UI.Composition;

namespace SlideShow
{
    //------------------------------------------------------------------------------
    //
    // class PhotoDatabase
    //
    //  This class represents the collection of all Photos, and tracks their loading
    //  state.
    //
    //------------------------------------------------------------------------------

    class PhotoDatabase
    {
        public async Task Create(CompositionImageFactory imageFactory)
        {
            _random = new Random();
            _imageFactory = imageFactory;
            _allFiles = new List<StorageFile>();


            // Search pictures to build File collection of images.

            StorageFolder picturesFolder = KnownFolders.PicturesLibrary;
            StorageFolder useFolder = picturesFolder;

            try
            {
                var item = await picturesFolder.TryGetItemAsync("Demo");
                if ((item != null) && item.IsOfType(StorageItemTypes.Folder))
                {
                    StorageFolder demosFolder = (StorageFolder)item;
                    useFolder = demosFolder;
                }
            }
            catch (Exception)
            {

            }

            await AddFiles(useFolder);


            // Ensure that each file is only listed once

            for (int a = 0; a < _allFiles.Count - 1; a++)
            {
                for (int b = a + 1; b < _allFiles.Count; b++)
                {
                    if (String.Equals(
                        _allFiles[a].Path,
                        _allFiles[b].Path,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.Assert(false, "Same filename appears twice");
                    }
                }
            }


            // Build a Photo for each of the given uris.

            _allPhotos = new Photo[_allFiles.Count];
            int index = 0;
            foreach (var file in _allFiles)
            {
                _allPhotos[index++] = new Photo(file, _imageFactory);
            }


            // Rather than continously picking random photos, we want to shuffle all photos to
            // ensure that we don't unnecessarily duplicate photos.  Otherwise, statistically, we
            // will have more duplicated photos than necessary, and not even load some photos.

            ShufflePhotos();
        }


        private async Task AddFiles(StorageFolder parentFolder)
        {
            var files = await parentFolder.GetFilesAsync().AsTask();
            foreach (var file in files)
            {
                string fileType = file.FileType.ToLowerInvariant();
                switch (fileType)
                {
                    case ".jpg":
                    case ".png":
                        lock (_allFiles)
                        {
                            if (_allFiles.Count >= MaxPhotos)
                            {
                                return;
                            }

                            _allFiles.Add(file);
                        }
                        break;
                }
            }

            if (_allFiles.Count < MaxPhotos)
            {
                var childFolders = await parentFolder.GetFoldersAsync();
                foreach (var folder in childFolders)
                {
                    await AddFiles(folder);
                }
            }
        }


        public Photo GetNextPhoto()
        {
            if (_allPhotos.Length <= 0)
            {
                return null;
            }

            var photo = _allPhotos[_nextPhotoIndex];
            Debug.Assert(photo != null);

            _nextPhotoIndex++;
            if (_nextPhotoIndex >= _allPhotos.Length)
            {
                _nextPhotoIndex = 0;
            }

            return photo;
        }


        private void ShufflePhotos()
        {
            if (_allPhotos.Length > 0)
            {
                for (int idx = 0; idx < 1000; idx++)
                {
                    int a = _random.Next(_allPhotos.Length);
                    int b = _random.Next(_allPhotos.Length);

                    var temp = _allPhotos[a];
                    _allPhotos[a] = _allPhotos[b];
                    _allPhotos[b] = temp;
                }
            }
        }

        private const int MaxPhotos = 250;

        private CompositionImageFactory _imageFactory;
        private Random _random;
        private List<StorageFile> _allFiles;
        private Photo[] _allPhotos;
        private int _nextPhotoIndex;
    }
}
