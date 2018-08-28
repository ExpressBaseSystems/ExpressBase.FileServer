using ExpressBase.Common;
using ExpressBase.Common.Constants;
using ExpressBase.Common.Data;
using ExpressBase.Common.EbServiceStack.ReqNRes;
using ExpressBase.Common.Enums;
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
                    EbFileCategory cat = EbFileCategory.External;

                    if (request.FileName.StartsWith(StaticFileConstants.LOGO))
                    {
                        cat = EbFileCategory.SolLogo;
                    }

                    fb = InfraConnectionFactory.FilesDB.DownloadFileByName(request.FileName, cat);
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
        public DownloadFileResponse Get(DownloadFileByIdRequest request)
        {
            byte[] fb = new byte[0];

            string sFilePath = string.Format("../StaticFiles/{0}/{1}", request.TenantAccountId, request.FileDetails.ObjectId.ObjectId);

            MemoryStream ms = null;

            DownloadFileResponse dfs = new DownloadFileResponse();

            try
            {
                if (!System.IO.File.Exists(sFilePath))
                {
                    EbFileCategory category = request.FileDetails.FileCategory;

                    fb = this.EbConnectionFactory.FilesDB.DownloadFileById(request.FileDetails.ObjectId, category);

                    if (fb != null)
                    {
                        EbFile.Bytea_ToFile(fb, sFilePath);
                    }
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
                        MetaDataDictionary = request.FileDetails.MetaDataDictionary,
                    };
                }
                else
                    throw new Exception("File Not Found");
            }
            catch (FormatException e)
            {
                Console.WriteLine("ObjectId not in Correct Format: " + request.FileDetails.FileName);
                Console.WriteLine("Exception: " + e.ToString());
            }
            catch (Exception e)
            {
                Log.Info("Exception:" + e.ToString());
            }

            return dfs;
        }

        [Authenticate]
        public DownloadFileResponse Get(DownloadImageByIdRequest request)
        {
            byte[] fb = new byte[0];

            string sFilePath = string.Format("../StaticFiles/{0}/{1}", request.TenantAccountId, request.ImageInfo.ObjectId.ObjectId);

            MemoryStream ms = null;

            DownloadFileResponse dfs = new DownloadFileResponse();

            try
            {
                if (!System.IO.File.Exists(sFilePath))
                {
                    EbFileCategory category = request.ImageInfo.FileCategory;

                    fb = this.EbConnectionFactory.FilesDB.DownloadFileById(request.ImageInfo.ObjectId, category);

                    if (fb != null)
                        EbFile.Bytea_ToFile(fb, sFilePath);
                }

                if (File.Exists(sFilePath))
                {
                    ms = new MemoryStream(File.ReadAllBytes(sFilePath));

                    dfs.StreamWrapper = new MemorystreamWrapper(ms);
                    dfs.FileDetails = new FileMeta
                    {
                        FileName = request.ImageInfo.FileName,
                        FileType = request.ImageInfo.FileType,
                        Length = request.ImageInfo.Length,
                        ObjectId = request.ImageInfo.ObjectId,
                        UploadDateTime = request.ImageInfo.UploadDateTime,
                        MetaDataDictionary = request.ImageInfo.MetaDataDictionary,
                    };
                }
                else
                    throw new Exception("File Not Found");
            }
            catch (FormatException e)
            {
                Console.WriteLine("ObjectId not in Correct Format: " + request.ImageInfo.FileName);
                Console.WriteLine("Exception: " + e.ToString());
            }
            catch (Exception e)
            {
                Log.Info("Exception:" + e.ToString());
            }

            return dfs;
        }

        [Authenticate]
        public DownloadFileResponse Get(DownloadImageByNameRequest request)
        {
            byte[] fb = new byte[0];

            string sFilePath = string.Format("../StaticFiles/{0}/{1}", request.TenantAccountId, request.ImageInfo.FileName);

            MemoryStream ms = null;

            DownloadFileResponse dfs = new DownloadFileResponse();

            try
            {
                if (!System.IO.File.Exists(sFilePath))
                {
                    EbFileCategory category = request.ImageInfo.FileCategory;

                    fb = this.EbConnectionFactory.FilesDB.DownloadFileByName(request.ImageInfo.FileName, category);

                    if (fb != null)
                        EbFile.Bytea_ToFile(fb, sFilePath);
                }

                if (File.Exists(sFilePath))
                {
                    ms = new MemoryStream(File.ReadAllBytes(sFilePath));

                    dfs.StreamWrapper = new MemorystreamWrapper(ms);
                    dfs.FileDetails = new FileMeta
                    {
                        FileName = request.ImageInfo.FileName,
                        FileType = request.ImageInfo.FileType,
                        Length = request.ImageInfo.Length,
                        ObjectId = request.ImageInfo.ObjectId,
                        UploadDateTime = request.ImageInfo.UploadDateTime,
                        MetaDataDictionary = request.ImageInfo.MetaDataDictionary,
                    };
                }
                else
                    throw new Exception("File Not Found");
            }
            catch (FormatException e)
            {
                Console.WriteLine("ObjectId not in Correct Format: " + request.ImageInfo.FileName);
                Console.WriteLine("Exception: " + e.ToString());
            }
            catch (Exception e)
            {
                Log.Info("Exception:" + e.ToString());
            }

            return dfs;
        }
    }
}