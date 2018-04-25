using ExpressBase.Common;
using ExpressBase.Common.Constants;
using ExpressBase.Common.Data;
using ExpressBase.Common.EbServiceStack.ReqNRes;
using MongoDB.Bson;
using ServiceStack;
using System;
using System.IO;

namespace ExpressBase.StaticFileServer.Services
{
    public class DownloadServices : BaseService
    {
        public DownloadServices(IEbConnectionFactory _dbf) : base(_dbf)
        {
        }

        public DownloadFileResponse Get(DownloadFileExtRequest request)
        {
            string sFilePath = string.Format("../StaticFiles/{0}/{1}", CoreConstants.EXPRESSBASE, request.FileName);

            byte[] fb = null;

            DownloadFileResponse dfs = new DownloadFileResponse();

            MemoryStream ms = null;
            try
            {
                if (!System.IO.File.Exists(sFilePath))
                {
                    string bucketName = StaticFileConstants.EXTERNAL;

                    if (request.FileName.StartsWith(StaticFileConstants.LOGO))
                    {
                        bucketName = StaticFileConstants.SOL_LOGOS;
                    }

                    fb = InfraConnectionFactory.FilesDB.DownloadFile(request.FileName, bucketName);
                    if (fb != null)
                        EbFile.Bytea_ToFile(fb, sFilePath);
                }
                if (File.Exists(sFilePath))
                {
                    ms = new MemoryStream(File.ReadAllBytes(sFilePath)); // Use a dummy for every use

                    dfs.StreamWrapper = new MemorystreamWrapper(ms);
                    dfs.FileDetails = new FileMeta
                    {
                        FileName = request.FileName,
                        FileType = request.FileName.Split(CharConstants.DOT)[1]
                    };
                }
            }
            catch (Exception e)
            {
                Log.Info("Exception:" + e.ToString());
            }
            return dfs;
        }

        [Authenticate]
        public DownloadFileResponse Get(DownloadFileRequest request)
        {
            byte[] fb = new byte[0];

            string sFilePath = string.Format("../StaticFiles/{0}/{1}", request.TenantAccountId, request.FileDetails.FileName);

            var FileNameParts = request.FileDetails.FileName.Substring(0, request.FileDetails.FileName.IndexOf(CharConstants.DOT))?.Split(CharConstants.UNDERSCORE);

            MemoryStream ms = null;

            DownloadFileResponse dfs = new DownloadFileResponse();

            try
            {
                if (!System.IO.File.Exists(sFilePath))
                {
                    string bucketName = string.Empty;
                    ObjectId objectId = new ObjectId();

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
                        if (objectId.Pid != 0)
                            fb = this.EbConnectionFactory.FilesDB.DownloadFile(objectId, bucketName);
                        else
                            fb = this.EbConnectionFactory.FilesDB.DownloadFile(request.FileDetails.FileName, bucketName);

                        //return this.EbConnectionFactory.FilesDB.DownloadFile(request.FileDetails.FileName, bucketName);
                    }
                    if (fb != null)
                        EbFile.Bytea_ToFile(fb, sFilePath);
                }

                if (File.Exists(sFilePath))
                {
                    ms = new MemoryStream(File.ReadAllBytes(sFilePath));

                    dfs.StreamWrapper = new MemorystreamWrapper(ms);
                    dfs.FileDetails = new FileMeta
                    {
                        FileName = request.FileDetails.FileName,
                        FileType = request.FileDetails.FileType,
                        Length = request.FileDetails.Length,
                        ObjectId = request.FileDetails.ObjectId,
                        UploadDateTime = request.FileDetails.UploadDateTime,
                        MetaDataDictionary = request.FileDetails.MetaDataDictionary
                    };
                }
                else
                    throw new Exception("File Not Found");
            }
            catch (FormatException e)
            {
                Console.WriteLine("ObjectId not in Correct Format: " + FileNameParts[0].ToString());
            }
            catch (Exception e)
            {
                Log.Info("Exception:" + e.ToString());
            }

            return dfs;

        }
    }
}