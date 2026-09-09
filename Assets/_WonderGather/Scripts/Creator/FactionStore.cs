using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
namespace WonderGather
{
    public sealed class FactionEntry
    {
        public string Id,Name,Problem,Token;
        public FactionRecord Record;
        public bool CanOpen=>Record!=null && string.IsNullOrEmpty(Problem);
    }
    public sealed class FactionStore
    {
        public string DirectoryPath {get;}
        public FactionStore(string directory)=>DirectoryPath=Path.GetFullPath(directory);
        public string FilePath(string id)
        {
            if(!Guid.TryParseExact(id,"N",out _)) throw new InvalidDataException("Invalid faction file ID.");
            return Path.Combine(DirectoryPath,id+".faction.json");
        }
        private static string Hash(byte[] bytes)
        {using(var sha=SHA256.Create()) return Convert.ToBase64String(sha.ComputeHash(bytes));}
        public FactionEntry Read(string id)
        {
            var path=FilePath(id);
            if(new FileInfo(path).Length>65536) throw new InvalidDataException("This faction file is too large for the supported format.");
            var bytes=File.ReadAllBytes(path);var record=FactionRecord.Decode(new UTF8Encoding(false,true).GetString(bytes));
            if(record.Id!=id) throw new InvalidDataException("The faction's ID does not match its filename.");
            return new FactionEntry{Id=id,Name=record.Name,Record=record,Token=Hash(bytes)};
        }
        public List<FactionEntry> List()
        {
            var result=new List<FactionEntry>();if(!Directory.Exists(DirectoryPath)) return result;
            foreach(var path in Directory.EnumerateFiles(DirectoryPath,"*.faction.json",SearchOption.TopDirectoryOnly))
            {
                string id=Path.GetFileName(path).Replace(".faction.json","");
                try{result.Add(Read(id));}
                catch(Exception error) when(IsStorageError(error))
                {result.Add(new FactionEntry{Id=Guid.TryParseExact(id,"N",out _)?id:null,Name=Path.GetFileName(path),Problem=error.Message});}
            }
            return result.OrderBy(x=>x.Name,StringComparer.CurrentCultureIgnoreCase).ThenBy(x=>x.Id,StringComparer.Ordinal).ToList();
        }
        private static FileStream Lock(string path)=>new FileStream(path+".lock",FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None,1,FileOptions.DeleteOnClose);
        public FactionEntry Save(FactionRecord record,string expectedToken=null)
        {
            string text=record.Encode();string destination=FilePath(record.Id);
            Directory.CreateDirectory(DirectoryPath);
            using(var gate=Lock(destination))
            {
            bool exists=File.Exists(destination);
            if(exists)
            {
                if(expectedToken==null || Read(record.Id).Token!=expectedToken) throw new IOException("This saved faction changed outside the creator. Open it again or save your draft as a copy.");
            }
            else if(expectedToken!=null) throw new IOException("The saved faction is no longer present. Save your draft as a copy.");
            string temporary=destination+"."+Guid.NewGuid().ToString("N")+".tmp";
            try
            {
                var bytes=new UTF8Encoding(false).GetBytes(text);
                using(var stream=new FileStream(temporary,FileMode.CreateNew,FileAccess.Write,FileShare.None))
                {stream.Write(bytes,0,bytes.Length);stream.Flush(true);}
                if(exists) File.Replace(temporary,destination,destination+".bak");
                else File.Move(temporary,destination);
                return new FactionEntry{Id=record.Id,Name=record.Name,Record=record,Token=Hash(bytes)};
            }
            finally{if(File.Exists(temporary)) File.Delete(temporary);}
            }
        }
        public void Delete(string id,string expectedToken)
        {
            var path=FilePath(id);using(var gate=Lock(path))
            {
            var bytes=File.ReadAllBytes(path);
            if(expectedToken!=null && Hash(bytes)!=expectedToken) throw new IOException("This file changed after the library was opened. Refresh the library before deleting it.");
            var deleted=Path.Combine(DirectoryPath,"Deleted");Directory.CreateDirectory(deleted);
            File.Move(path,Path.Combine(deleted,id+"-"+Guid.NewGuid().ToString("N")+".faction.json"));
            }
        }
        public static bool IsStorageError(Exception error)=>error is InvalidDataException || error is IOException || error is UnauthorizedAccessException || error is ArgumentException || error is NotSupportedException || error is System.Security.SecurityException;
    }
}
