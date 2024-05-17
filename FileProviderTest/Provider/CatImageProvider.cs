using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Internal;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Primitives;
using System.Collections;
using System.Collections.Generic;
using static Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary;

namespace FileProviderTest.Provider
{
    public class CatImageProvider : IFileProvider
    {
        private readonly string _root;

        public CatImageProvider(string root)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            var fullPath = Path.Combine(_root, subpath);
            return new PhysicalDirectoryContents(fullPath);
        }


        public IFileInfo GetFileInfo(string subpath)
        {
            var fullPath = Path.Combine(_root, subpath);
            if (!File.Exists(fullPath))
            {
                return new NotFoundFileInfo(subpath);
            }

            return new PhysicalFileInfo(new FileInfo(fullPath));
        }

        public IChangeToken Watch(string filter)
        {
            // For simplicity, we are not implementing file change notifications.
            return NullChangeToken.Singleton;
        }
    }
}
