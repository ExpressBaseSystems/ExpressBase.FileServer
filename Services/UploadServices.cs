using ExpressBase.Common;
using ExpressBase.Common.Constants;
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

        //        private static readonly string IdFetchQuery =
        //@"INSERT INTO
        //    eb_files_ref (userid, filename, filetype, tags, filecategory, uploadts) 
        //VALUES 
        //    (@userid, @filename, @filetype, @tags, @filecategory, NOW()) 
        //RETURNING id";

        [Authenticate]
        public UploadAsyncResponse Post(UploadFileAsyncRequest request)
        {
            Log.Info("Inside FileUpload");

            UploadAsyncResponse res = new UploadAsyncResponse();
            try
            {
                string context = string.IsNullOrEmpty(request.FileDetails.Context) ? StaticFileConstants.CONTEXT_DEFAULT : request.FileDetails.Context;
                string meta = (request.FileDetails.MetaDataDictionary == null) ? "" : request.FileDetails.MetaDataDictionary.ToJson();
                request.FileDetails.FileRefId = GetFileRefId(request.UserId, request.FileDetails.FileName, request.FileDetails.FileType, meta, request.FileDetails.FileCategory, context);

                Log.Info("FileRefId : " + request.FileDetails.FileRefId);

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

                Log.Info("File Pushed to MQ");
            }
            catch (Exception e)
            {
                Log.Info("Exception:" + e.StackTrace);
                res.ResponseStatus.Message = e.Message;
            }
            return res;
        }

        [Authenticate]
        public UploadAsyncResponse Post(UploadImageAsyncRequest request)
        {
            UploadAsyncResponse res = new UploadAsyncResponse();
            Log.Info("Inside ImageAsyncUpload");
            IUploadImageRequest req = null;

            if (request.ImageInfo.FileCategory == EbFileCategory.Dp)
                req = new UploadDpRequest();
            else if (request.ImageInfo.FileCategory == EbFileCategory.Images)
                req = new UploadImageRequest();
            else if (request.ImageInfo.FileCategory == EbFileCategory.SolLogo)
                req = new UploadLogoRequest();

            try
            {
                string context = string.IsNullOrEmpty(request.ImageInfo.Context) ? StaticFileConstants.CONTEXT_DEFAULT : request.ImageInfo.Context;
                req.Byte = request.ImageByte;
                req.FileCategory = request.ImageInfo.FileCategory;
                req.SolutionId = request.SolutionId;
                req.SolnId = request.SolnId;
                req.UserId = (req is UploadDpRequest) ? request.UserIntId : request.UserId;
                req.UserAuthId = request.UserAuthId;
                req.BToken = (!String.IsNullOrEmpty(this.Request.Authorization)) ? this.Request.Authorization.Replace("Bearer", string.Empty).Trim() : String.Empty;
                req.RToken = (!String.IsNullOrEmpty(this.Request.Headers["rToken"])) ? this.Request.Headers["rToken"] : String.Empty;

                req.ImageRefId = GetFileRefId(request.UserId, request.ImageInfo.FileName, request.ImageInfo.FileType, request.ImageInfo.MetaDataDictionary.ToJson(), request.ImageInfo.FileCategory, request.ImageInfo.Context);

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

        [Authenticate]
        public UploadAsyncResponse Post(UploadImageInfraRequest request)
        {
            UploadAsyncResponse res = new UploadAsyncResponse();
            Log.Info("Inside ImageInfraUpload");
            IUploadImageRequest req = new UploadImageInfraMqRequest();

            try
            {
                string context = string.IsNullOrEmpty(request.ImageInfo.Context) ? StaticFileConstants.CONTEXT_DEFAULT : request.ImageInfo.Context;
                req.Byte = request.ImageByte;
                req.FileCategory = request.ImageInfo.FileCategory;
                req.SolutionId = request.SolutionId;
                req.SolnId = request.SolnId;
                req.UserId = request.UserId;
                req.UserAuthId = request.UserAuthId;
                req.BToken = (!String.IsNullOrEmpty(this.Request.Authorization)) ? this.Request.Authorization.Replace("Bearer", string.Empty).Trim() : String.Empty;
                req.RToken = (!String.IsNullOrEmpty(this.Request.Headers["rToken"])) ? this.Request.Headers["rToken"] : String.Empty;
                
                req.ImageRefId = this.GetFileRefIdInfra(request.UserId, request.ImageInfo.FileName, request.ImageInfo.FileType, request.ImageInfo.MetaDataDictionary.ToJson(), request.ImageInfo.FileCategory, request.ImageInfo.Context);

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

        private int GetFileRefId(int userId, string filename, string filetype, string tags, EbFileCategory ebFileCategory, string context)
        {
            int refId = 0;
            EbDataTable table = null;
            //logging connection
            Console.WriteLine("FileClient Connection at GetFileRefId");
            Console.WriteLine(this.EbConnectionFactory.DataDB.DBName);

            try
            {
                DbParameter[] parameters =
                {
                        this.EbConnectionFactory.DataDB.GetNewParameter("userid", EbDbTypes.Int32, userId),
                        this.EbConnectionFactory.DataDB.GetNewParameter("filename", EbDbTypes.String, filename),
                        this.EbConnectionFactory.DataDB.GetNewParameter("filetype", EbDbTypes.String, filetype),
                        this.EbConnectionFactory.DataDB.GetNewParameter("tags", EbDbTypes.String, string.IsNullOrEmpty(tags)? string.Empty: tags),
                        this.EbConnectionFactory.DataDB.GetNewParameter("filecategory", EbDbTypes.Int16, (int)ebFileCategory),
                        this.EbConnectionFactory.DataDB.GetNewParameter("context",EbDbTypes.String,context)
            };
                if (ebFileCategory == EbFileCategory.SolLogo)
                {
                    table = this.InfraConnectionFactory.DataDB.DoQuery(EbConnectionFactory.DataDB.EB_UPLOAD_IDFETCHQUERY, parameters);
                }
                else
                    table = this.EbConnectionFactory.DataDB.DoQuery(EbConnectionFactory.DataDB.EB_UPLOAD_IDFETCHQUERY, parameters);

                string s = table.Rows[0][0].ToString();
                refId = int.Parse(s);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: POSGRE: " + e.Message);
            }
            return refId;
        }

        private int GetFileRefIdInfra(int userId, string filename, string filetype, string tags, EbFileCategory ebFileCategory, string context)
        {
            int refId = 0;
            EbDataTable table = null;
            //logging connection
            Console.WriteLine("FileClient Connection at GetFileRefIdInfra");
            Console.WriteLine(this.InfraConnectionFactory.DataDB.DBName);

            try
            {
                DbParameter[] parameters =
                {
                        this.InfraConnectionFactory.DataDB.GetNewParameter("userid", EbDbTypes.Int32, userId),
                        this.InfraConnectionFactory.DataDB.GetNewParameter("filename", EbDbTypes.String, filename),
                        this.InfraConnectionFactory.DataDB.GetNewParameter("filetype", EbDbTypes.String, filetype),
                        this.InfraConnectionFactory.DataDB.GetNewParameter("tags", EbDbTypes.String, string.IsNullOrEmpty(tags)? string.Empty: tags),
                        this.InfraConnectionFactory.DataDB.GetNewParameter("filecategory", EbDbTypes.Int16, (int)ebFileCategory),
                        this.InfraConnectionFactory.DataDB.GetNewParameter("context",EbDbTypes.String,context)
            };
                table = this.InfraConnectionFactory.DataDB.DoQuery(this.InfraConnectionFactory.DataDB.EB_UPLOAD_IDFETCHQUERY, parameters);

                string s = table.Rows[0][0].ToString();
                refId = int.Parse(s);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: POSGRE: " + e.Message);
            }
            return refId;
        }

        [Authenticate]
        public FileCategoryChangeResponse Post(FileCategoryChangeRequest request)
        {
            int result;
            string sql = EbConnectionFactory.DataDB.EB_FILECATEGORYCHANGE;
            try
            {
                DbParameter[] parameters =
              {
                this.EbConnectionFactory.DataDB.GetNewParameter("categry", EbDbTypes.String,request.Category),
                this.EbConnectionFactory.DataDB.GetNewParameter("ids", EbDbTypes.String, request.FileRefId.Join(",")),
                };

                result = this.EbConnectionFactory.DataDB.DoNonQuery(sql, parameters);
            }
            catch (Exception ex)
            {
                result = 0;
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("Exception while updating Category:", ex.Message);
            }

            return new FileCategoryChangeResponse { Status = (result > 0) ? true : false };
        }
    }
}