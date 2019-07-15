using System;
using System.IO;
using System.Threading.Tasks;

namespace Realms.LFS
{
    public class FileData : RealmObject
    {
        [PrimaryKey]
        public string Id { get; private set; } = Guid.NewGuid().ToString();

        public Task<Stream> GetStream() => FileManager.ReadFile(this);

        private int StatusInt { get; set; }

        public DataStatus Status
        {
            get => (DataStatus)StatusInt;
            internal set => StatusInt = (int)value;
        }

        public string LocalUrl => FileManager.GetFilePath(this);

        public string Url { get; internal set; }

        public string Name { get; private set; }

        public FileData(Stream data, string name = null)
        {
            if (IsManaged)
            {
                throw new Exception("???");
            }
            else
            {
                FileManager.WriteFile(FileLocation.Temporary, Id, data);
            }
            Name = name;
            Status = DataStatus.Local;
        }

        private FileData()
        {
        }

        protected internal override void OnManaged()
        {
            base.OnManaged();

            FileManager.UploadFile(FileLocation.Temporary, this);
        }
    }
}
