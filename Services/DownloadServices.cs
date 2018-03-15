using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExpressBase.Common;
using ExpressBase.Common.Data;
using ExpressBase.Common.EbServiceStack.ReqNRes;
using MongoDB.Bson;
using ServiceStack;

namespace ExpressBase.StaticFileServer.Services
{
    public class DownloadServices : BaseService
    {
        public DownloadServices(IEbConnectionFactory _dbf) : base(_dbf)
        {
        }

        [Authenticate]
        public byte[] Post(DownloadFileRequest request)
        {
            string bucketName = string.Empty;
            ObjectId objectId;
            var FileNameParts = request.FileDetails.FileName.Substring(0, request.FileDetails.FileName.IndexOf('.'))?.Split('_');
            // 3 cases = > 1. ObjectId.(fileextension), 2. ObjectId_(size).(imageextionsion), 3. dp_(userid)_(size).(imageextension)
            if (request.FileDetails.FileName.StartsWith("dp"))
            {
                if (Enum.IsDefined(typeof(ImageTypes), request.FileDetails.FileType.ToString()))
                    bucketName = "dp_images";
            }
            else if (FileNameParts.Length == 1)
            {
                if (Enum.IsDefined(typeof(ImageTypes), request.FileDetails.FileType.ToString()))
                    bucketName = "images_original";

                if (bucketName == string.Empty)
                    bucketName = "files";

                objectId = new ObjectId(FileNameParts[0]);

                return (new EbConnectionFactory(request.TenantAccountId, this.Redis)).FilesDB.DownloadFile(new ObjectId(FileNameParts[0]), bucketName);
            }
            else if (FileNameParts.Length == 2)
            {
                if (Enum.IsDefined(typeof(ImageTypes), request.FileDetails.FileType.ToString()))
                {
                    if (FileNameParts[1] == "small")
                        bucketName = "images_small";
                    else if (FileNameParts[1] == "medium")
                        bucketName = "images_medium";
                    else if (FileNameParts[1] == "large")
                        bucketName = "images_large";
                }
                if (bucketName == string.Empty)
                {
                }
            }


            if (bucketName != string.Empty)
            {
                return (this.EbConnectionFactory.FilesDB.DownloadFile(request.FileDetails.FileName, bucketName));
            }
            else { return (new byte[0]); }
        }
    }
}
