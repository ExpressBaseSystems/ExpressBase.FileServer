﻿using ExpressBase.Common;
using ExpressBase.Common.Constants;
using ExpressBase.Common.Data;
using ExpressBase.Common.EbServiceStack.ReqNRes;
using ServiceStack;
using ServiceStack.Messaging;
using System;
using System.Collections.Generic;

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
                this.MessageProducer3.Publish(new UploadFileRequest()
                {
                    FileDetails = new FileMeta
                    {
                        FileName = request.FileDetails.FileName,
                        MetaDataDictionary = (request.FileDetails.MetaDataDictionary != null) ?
                        request.FileDetails.MetaDataDictionary :
                        new Dictionary<String, List<string>>() { },
                        FileType = request.FileDetails.FileType,
                        Length = request.FileDetails.Length
                    },
                    FileByte = request.FileByte,
                    BucketName = bucketName,
                    TenantAccountId = request.TenantAccountId,
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
            string bucketName = StaticFileConstants.IMAGES_ORIGINAL;

            try
            {
                if (request.ImageInfo.FileName.StartsWith(StaticFileConstants.DP))
                    bucketName = StaticFileConstants.DP_IMAGES;
                else if (request.ImageInfo.FileName.StartsWith(StaticFileConstants.LOGO))
                {
                    bucketName = StaticFileConstants.SOL_LOGOS;

                    //Temporary only for testing
                    request.TenantAccountId = CoreConstants.EXPRESSBASE;
                }
                else if (request.ImageInfo.FileName.StartsWith(StaticFileConstants.LOCATION_DP))
                    bucketName = StaticFileConstants.LOCATION_IMAGES;


                this.MessageProducer3.Publish(new UploadFileRequest()
                {
                    FileDetails = new FileMeta
                    {
                        FileName = request.ImageInfo.FileName,
                        MetaDataDictionary = (request.ImageInfo.MetaDataDictionary != null) ?
                        request.ImageInfo.MetaDataDictionary :
                        new Dictionary<String, List<string>>() { },
                        FileType = request.ImageInfo.FileType,
                        Length = request.ImageInfo.Length
                    },
                    FileByte = request.ImageByte,
                    BucketName = bucketName,
                    TenantAccountId = request.TenantAccountId,
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
    }
}