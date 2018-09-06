using ExpressBase.Common;
using ExpressBase.Common.Constants;
using ExpressBase.Common.Data;
using ExpressBase.Common.EbServiceStack.ReqNRes;
using ExpressBase.Common.Enums;
using ExpressBase.Common.Structures;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;

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
                        FileType = request.FileName.SplitOnLast(CharConstants.DOT).Last()
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

            string sFilePath = string.Format("../StaticFiles/{0}/{1}.{2}", request.SolnId, request.FileDetails.FileStoreId, request.FileDetails.FileType);

            MemoryStream ms = null;

            DownloadFileResponse dfs = new DownloadFileResponse();

            try
            {
                if (!System.IO.File.Exists(sFilePath))
                {
                    EbFileCategory category = request.FileDetails.FileCategory;

                    fb = this.EbConnectionFactory.FilesDB.DownloadFileById(request.FileDetails.FileStoreId, category);

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
                        FileStoreId = request.FileDetails.FileStoreId,
                        UploadDateTime = request.FileDetails.UploadDateTime,
                        MetaDataDictionary = (request.FileDetails.MetaDataDictionary != null) ? request.FileDetails.MetaDataDictionary : new Dictionary<String, List<string>>() { },
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
        public DownloadFileResponse Get(DownloadFileByRefIdRequest request)
        {
            byte[] fb = new byte[0];
            DownloadFileResponse dfs = new DownloadFileResponse();

            string sql = "SELECT filestore_id, filecategory FROM eb_files_ref WHERE id = @refid AND eb_del = 'F';";

            EbDataTable dt = this.EbConnectionFactory.DataDB.DoQuery(sql, new DbParameter[] { this.EbConnectionFactory.DataDB.GetNewParameter("@refid", EbDbTypes.Int32, request.FileDetails.FileRefId) });

            if (dt.Rows.Count != 0)
            {
                request.FileDetails.FileStoreId = (dt.Rows[0][0].ToString());
                request.FileDetails.FileCategory = (EbFileCategory)Convert.ToInt32(dt.Rows[0][1].ToString());

                string sFilePath = string.Format("../StaticFiles/{0}/{1}.{2}", request.SolnId, request.FileDetails.FileStoreId, request.FileDetails.FileType);

                MemoryStream ms = null;

                try
                {
                    if (!System.IO.File.Exists(sFilePath))
                    {
                        fb = this.EbConnectionFactory.FilesDB.DownloadFileById(request.FileDetails.FileStoreId, request.FileDetails.FileCategory);

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
                            FileStoreId = request.FileDetails.FileStoreId,
                            UploadDateTime = request.FileDetails.UploadDateTime,
                            MetaDataDictionary = (request.FileDetails.MetaDataDictionary != null) ? request.FileDetails.MetaDataDictionary : new Dictionary<String, List<string>>() { },
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
            }
            return dfs;
        }

        [Authenticate]
        public DownloadFileResponse Get(DownloadImageByIdRequest request)
        {
            byte[] fb = new byte[0];

            string sFilePath = string.Format("../StaticFiles/{0}/{1}.{2}", request.SolnId, request.ImageInfo.FileStoreId, request.ImageInfo.FileType);

            MemoryStream ms = null;

            DownloadFileResponse dfs = new DownloadFileResponse();

            try
            {
                if (!System.IO.File.Exists(sFilePath))
                {
                    EbFileCategory category = request.ImageInfo.FileCategory;

                    fb = this.EbConnectionFactory.FilesDB.DownloadFileById(request.ImageInfo.FileStoreId, category);

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
                        FileStoreId = request.ImageInfo.FileStoreId,
                        UploadDateTime = request.ImageInfo.UploadDateTime,
                        MetaDataDictionary = (request.ImageInfo.MetaDataDictionary != null) ? request.ImageInfo.MetaDataDictionary : new Dictionary<String, List<string>>() { },
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

            string sFilePath = string.Format("../StaticFiles/{0}/{1}", request.SolnId, request.ImageInfo.FileName);

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
                        FileStoreId = request.ImageInfo.FileStoreId,
                        UploadDateTime = request.ImageInfo.UploadDateTime,
                        MetaDataDictionary = (request.ImageInfo.MetaDataDictionary != null) ? request.ImageInfo.MetaDataDictionary : new Dictionary<String, List<string>>() { },
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