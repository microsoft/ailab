using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SnipInsight.Forms.Features.Library
{
    public interface ILibraryService
    {
        void Delete(ImageModel image);

        DateTime GetLastWriteTime(string path);

        Task<IEnumerable<ImageModel>> LoadAllAsync();
    }
}
