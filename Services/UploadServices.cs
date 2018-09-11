using ExpressBase.Common.Data;
using ExpressBase.Common.EbServiceStack.ReqNRes;
using ServiceStack;
using ServiceStack.Messaging;
using System;

namespace ExpressBase.StaticFileServer
{
    public class UploadServices : BaseService
    {
        public UploadServices(IEbConnectionFactory _dbf, IMessageProducer _msp, IMessageQueueClient _mqc) : base(_dbf, _msp, _mqc)
        {
        }

        [Authenticate]
        public UploadAsyncResponse Post(UploadFileAsyncRequest request)
        {
            UploadAsyncResponse res = new UploadAsyncResponse();
            try
            {
                this.MessageProducer3.Publish(new UploadFileRequest()
                {
                    FileDetails = request.FileDetails,
                    Byte = request.FileByte,
                    SolnId = request.SolnId,
                    UserId = request.UserId,
                    UserAuthId = request.UserAuthId,
                    BToken = (!String.IsNullOrEmpty(this.Request.Authorization)) ? this.Request.Authorization.Replace("Bearer", string.Empty).Trim() : String.Empty,
                    RToken = (!String.IsNullOrEmpty(this.Request.Headers["rToken"])) ? this.Request.Headers["rToken"] : String.Empty
                });
            }
            catch (Exception e)
            {
                Log.Info("Exception:" + e.ToString());
                res.ResponseStatus.Message = e.Message;
            }
            return res;
        }

        [Authenticate]
        public UploadAsyncResponse Post(UploadImageAsyncRequest request)
        {
            UploadAsyncResponse res = new UploadAsyncResponse();

            Log.Info("Inside ImageAsyncUpload");

            try
            {
                UploadImageRequest req = new UploadImageRequest()
                {
                    ImageInfo = request.ImageInfo,
                    Byte = request.ImageByte,
                    SolnId = request.SolnId,
                    UserId = request.UserId,
                    UserAuthId = request.UserAuthId,
                    BToken = (!String.IsNullOrEmpty(this.Request.Authorization)) ? this.Request.Authorization.Replace("Bearer", string.Empty).Trim() : String.Empty,
                    RToken = (!String.IsNullOrEmpty(this.Request.Headers["rToken"])) ? this.Request.Headers["rToken"] : String.Empty
                };

                req.ImageInfo.FileRefId = UploadImageRequest.GetFileRefId(EbConnectionFactory.DataDB, request.UserId, req.ImageInfo.FileName, req.ImageInfo.FileType, req.ImageInfo.MetaDataDictionary.ToJson(), req.ImageInfo.FileCategory);

                this.MessageProducer3.Publish(req);
                res.ImgRefId = req.ImageInfo.FileRefId;
                
            }
            catch (Exception e)
            {
                Log.Info("Exception:" + e.ToString());
                res.ImgRefId = 0;
                res.ResponseStatus.Message = e.Message;
            }
            return res;
        }
    }
}