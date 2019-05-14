using ExpressBase.Common;
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
    eb_files_ref (userid, filename, filetype, tags, filecategory, uploadts) 
VALUES 
    (@userid, @filename, @filetype, @tags, @filecategory, NOW()) 
RETURNING id";

        [Authenticate]
        public UploadAsyncResponse Post(UploadFileAsyncRequest request)
        {
            Log.Info("Inside FileUpload");

            UploadAsyncResponse res = new UploadAsyncResponse();
            try
            {
                request.FileDetails.FileRefId = GetFileRefId(request.UserId, request.FileDetails.FileName, request.FileDetails.FileType, request.FileDetails.MetaDataDictionary.ToString(), request.FileDetails.FileCategory);

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
                req.Byte = request.ImageByte;
                req.FileCategory = request.ImageInfo.FileCategory;
                req.SolutionId = request.SolutionId;
                req.SolnId = request.SolnId;
                req.UserId = request.UserId;
                req.UserAuthId = request.UserAuthId;
                req.BToken = (!String.IsNullOrEmpty(this.Request.Authorization)) ? this.Request.Authorization.Replace("Bearer", string.Empty).Trim() : String.Empty;
                req.RToken = (!String.IsNullOrEmpty(this.Request.Headers["rToken"])) ? this.Request.Headers["rToken"] : String.Empty;

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
            EbDataTable table = null;
            try
            {
                DbParameter[] parameters =
                {
                        this.EbConnectionFactory.DataDB.GetNewParameter("userid", EbDbTypes.Int32, userId),
                        this.EbConnectionFactory.DataDB.GetNewParameter("filename", EbDbTypes.String, filename),
                        this.EbConnectionFactory.DataDB.GetNewParameter("filetype", EbDbTypes.String, filetype),
                        this.EbConnectionFactory.DataDB.GetNewParameter("tags", EbDbTypes.String, string.IsNullOrEmpty(tags)? string.Empty: tags),
                        this.EbConnectionFactory.DataDB.GetNewParameter("filecategory", EbDbTypes.Int16, (int)ebFileCategory)
            };
                if (ebFileCategory == EbFileCategory.SolLogo)
                {
                    table = this.InfraConnectionFactory.DataDB.DoQuery(IdFetchQuery, parameters);
                }
                else
                    table = this.EbConnectionFactory.DataDB.DoQuery(IdFetchQuery, parameters);

                refId = (int)table.Rows[0][0];
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
            var sql = @"UPDATE 
	                        eb_files_ref FR
                        SET
	                        tags = jsonb_set(cast(tags as jsonb),
							'{Category}',
							(SELECT (cast(tags as jsonb)->'Category')-0 || to_jsonb(:categry::text)),
                            true)
                        WHERE 
                            FR.id = ANY(string_to_array(:ids,',')::int[]);";
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
           
            return new FileCategoryChangeResponse { Status = (result > 0)?true:false };
        }
    }
}