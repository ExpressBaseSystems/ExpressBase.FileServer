using ExpressBase.Common;
using ExpressBase.Common.Data;
using ExpressBase.Common.EbServiceStack.ReqNRes;
using ExpressBase.Common.ServerEvents_Artifacts;
using ExpressBase.Common.ServiceClients;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpressBase.StaticFileServer
{
    public class UploadServices : BaseService
    {
        public UploadServices(IEbConnectionFactory _dbf, IEbServerEventClient _sec, IEbMqClient _mqc) : base(_dbf, _sec, _mqc) { }

        [Authenticate]
        public bool Post(UploadFileAsyncRequest request)
        {
            string bucketName = "files";
            try
            {
                if (Enum.IsDefined(typeof(ImageTypes), request.FileDetails.FileType.ToString()))
                {
                    bucketName = "images_original";
                    if (request.FileDetails.FileName.StartsWith("dp"))
                    {
                        bucketName = "dp_images";
                    }
                }
                this.MqClient.AddAuthentication(this.Request);
                this.MqClient.Post<bool>(new UploadFileMqRequest()
                {
                    BucketName = bucketName,
                    FileDetails = request.FileDetails,
                    FileByte = request.FileByte
                });
            }
            catch (Exception e)
            {
                Log.Info("Exception:" + e.ToString());
                return false;
            }
            return true;
        }

        [Authenticate]
        public bool Post(UploadImageAsyncRequest request)
        {
            Log.Info("Inside ImageAsyncUpload");
            string bucketName = "images_original";
            if (request.ImageInfo.FileName.StartsWith("dp"))
                bucketName = "dp_images";
            this.MqClient.AddAuthentication(this.Request);
            try
            {
                this.MqClient.Post<bool>(new UploadFileMqRequest()
                {
                   BucketName = bucketName,
                   FileDetails = request.ImageInfo,
                   FileByte = request.ImageByte
                });
            }
            catch (Exception e)
            {
                Log.Info("Exception:" + e.ToString());
                return false;
            }
            return true;
        }
    }
}
