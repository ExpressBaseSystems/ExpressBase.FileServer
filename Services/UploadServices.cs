using ExpressBase.Common.Data;
using ExpressBase.Common.EbServiceStack.ReqNRes;
using ExpressBase.Common.Enums;
using ExpressBase.Common.Structures;
using ServiceStack;
using ServiceStack.Messaging;
using System;
using System.Data.Common;

namespace ExpressBase.StaticFileServer
{
    public class UploadServices : BaseService
    {
        public UploadServices(IEbConnectionFactory _dbf, IMessageProducer _msp, IMessageQueueClient _mqc) : base(_dbf, _msp, _mqc)
        {
        }

        private static readonly string IdFetchQuery =
@"INSERT INTO
    eb_files_ref (userid, filename, filetype, tags, filecategory) 
VALUES 
    (@userid, @filename, @filetype, @tags, @filecategory) 
RETURNING id";

        [Authenticate]
        public UploadAsyncResponse Post(UploadFileAsyncRequest request)
        {
            UploadAsyncResponse res = new UploadAsyncResponse();
            try
            {
                request.FileDetails.FileRefId = GetFileRefId(request.UserId, request.FileDetails.FileName, request.FileDetails.FileType, request.FileDetails.MetaDataDictionary.ToString(), request.FileDetails.FileCategory);

                this.MessageProducer3.Publish(new UploadFileRequest()
                {
                    FileRefId = request.FileDetails.FileRefId,
                    Byte = request.FileByte,
                    SolnId = request.SolnId,
                    UserId = request.UserId,
                    UserAuthId = request.UserAuthId,
                    BToken = (!String.IsNullOrEmpty(this.Request.Authorization)) ? this.Request.Authorization.Replace("Bearer", string.Empty).Trim() : String.Empty,
                    RToken = (!String.IsNullOrEmpty(this.Request.Headers["rToken"])) ? this.Request.Headers["rToken"] : String.Empty
                });
                res.FileRefId = request.FileDetails.FileRefId;
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
                    Byte = request.ImageByte,
                    FileCategory = request.ImageInfo.FileCategory,
                    SolnId = request.SolnId,
                    UserId = request.UserId,
                    UserAuthId = request.UserAuthId,
                    BToken = (!String.IsNullOrEmpty(this.Request.Authorization)) ? this.Request.Authorization.Replace("Bearer", string.Empty).Trim() : String.Empty,
                    RToken = (!String.IsNullOrEmpty(this.Request.Headers["rToken"])) ? this.Request.Headers["rToken"] : String.Empty
                };

                req.ImageRefId = GetFileRefId(request.UserId, request.ImageInfo.FileName, request.ImageInfo.FileType, request.ImageInfo.MetaDataDictionary.ToJson(), request.ImageInfo.FileCategory);

                this.MessageProducer3.Publish(req);
                res.FileRefId = req.ImageRefId;
            }
            catch (Exception e)
            {
                Log.Info("Exception:" + e.ToString());
                res.FileRefId = 0;
                res.ResponseStatus.Message = e.Message;
            }
            return res;
        }

        private int GetFileRefId(int userId, string filename, string filetype, string tags, EbFileCategory ebFileCategory)
        {
            int refId = 0;
            try
            {
                DbParameter[] parameters =
                {
                        this.EbConnectionFactory.DataDB.GetNewParameter("userid", EbDbTypes.Int32, userId),
                        this.EbConnectionFactory.DataDB.GetNewParameter("filename", EbDbTypes.String, filename),
                        this.EbConnectionFactory.DataDB.GetNewParameter("filetype", EbDbTypes.String, filetype),
                        this.EbConnectionFactory.DataDB.GetNewParameter("tags", EbDbTypes.String, tags),
                        this.EbConnectionFactory.DataDB.GetNewParameter("filecategory", EbDbTypes.Int16, ebFileCategory)
            };
                var table = this.EbConnectionFactory.DataDB.DoQuery(IdFetchQuery, parameters);
                refId = (int)table.Rows[0][0];
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: POSGRE: " + e.Message);
            }
            return refId;
        }
    }
}