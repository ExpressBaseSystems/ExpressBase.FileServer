using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExpressBase.Common;
using ExpressBase.Common.Constants;
using ExpressBase.Common.Data;
using ExpressBase.Common.EbServiceStack.ReqNRes;
using MongoDB.Bson;
using ServiceStack;

namespace ExpressBase.StaticFileServer.Services
{
    public class DownloadServices : BaseService
    {
        public DownloadServices(IEbConnectionFactory _dbf) : base(_dbf) { }

        public byte[] Post(DownloadFileExtRequest request)
        {
            string bucketName = StaticFileConstants.EXTERNAL;

            if (request.FileName.StartsWith(StaticFileConstants.LOGO))
            {
                bucketName = StaticFileConstants.SOL_LOGOS;
            }

            try
            {
                return InfraConnectionFactory.FilesDB.DownloadFile(request.FileName, bucketName);
            }
            catch (Exception e)
            {
                Log.Info("Exception:" + e.ToString());
                return null;
            }
        }

        [Authenticate]
        public byte[] Post(DownloadFileRequest request)
        {
            string bucketName = string.Empty;
            ObjectId objectId;
            var FileNameParts = request.FileDetails.FileName.Substring(0, request.FileDetails.FileName.IndexOf(CharConstants.DOT))?.Split(CharConstants.UNDERSCORE);
            // 3 cases = > 1. ObjectId.(fileextension), 2. ObjectId_(size).(imageextionsion), 3. dp_(userid)_(size).(imageextension)
            if (request.FileDetails.FileName.StartsWith(StaticFileConstants.DP))
            {
                if (Enum.IsDefined(typeof(ImageTypes), request.FileDetails.FileType.ToString()))
                    bucketName = StaticFileConstants.DP_IMAGES;
            }
            else if (FileNameParts.Length == 1)
            {
                if (Enum.IsDefined(typeof(ImageTypes), request.FileDetails.FileType.ToString()))
                    bucketName = StaticFileConstants.IMAGES_ORIGINAL;

                if (bucketName == string.Empty)
                    bucketName = StaticFileConstants.FILES;

                objectId = new ObjectId(FileNameParts[0]);

                return this.EbConnectionFactory.FilesDB.DownloadFile(new ObjectId(FileNameParts[0]), bucketName);
            }
            else if (FileNameParts.Length == 2)
            {
                if (Enum.IsDefined(typeof(ImageTypes), request.FileDetails.FileType.ToString()))
                {
                    if (FileNameParts[1] == StaticFileConstants.SMALL)
                        bucketName = StaticFileConstants.IMAGES_SMALL;
                    else if (FileNameParts[1] == StaticFileConstants.MEDIUM)
                        bucketName = StaticFileConstants.IMAGES_MEDIUM;
                    else if (FileNameParts[1] == StaticFileConstants.LARGE)
                        bucketName = StaticFileConstants.IMAGES_LARGE;
                }
                if (bucketName == string.Empty)
                {
                }
            }


            if (bucketName != string.Empty)
            {
                return this.EbConnectionFactory.FilesDB.DownloadFile(request.FileDetails.FileName, bucketName);
            }
            else { return (new byte[0]); }
        }
    }
}
