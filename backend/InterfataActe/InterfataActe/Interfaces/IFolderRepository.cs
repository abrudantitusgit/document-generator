using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InterfataActe.Models;

namespace InterfataActe.Interfaces
{
    public interface IFolderRepository
    {
        Folder GetFolderByID(Guid id);
        Folder CreateFolder(Folder folder);
        List<Folder> GetAllFoldere();
    }
}
