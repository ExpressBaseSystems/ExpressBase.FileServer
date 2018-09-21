//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace ExpressBase.FileServer.Services
//{
//    public class ImageResize
//    {
//        //public string Post(ImageResizeRequest request)
//        //{
//        //    UploadImageRequest uploadImageRequest = new UploadImageRequest();
//        //    uploadImageRequest.TenantAccountId = request.TenantAccountId;
//        //    uploadImageRequest.UserId = request.UserId;

//        //    MemoryStream ms = new MemoryStream(request.ImageByte);
//        //    ms.Position = 0;

//        //    try
//        //    {
//        //        using (Image img = Image.FromStream(ms))
//        //        {
//        //            if (request.ImageInfo.FileCategory == EbFileCategory.Dp)
//        //            {
//        //                foreach (string size in Enum.GetNames(typeof(DPSizes)))
//        //                {
//        //                    int sz = (int)((DPSizes)Enum.Parse(typeof(DPSizes), size));

//        //                    Stream ImgStream = Resize(img, sz, sz);
//        //                    request.ImageByte = new byte[ImgStream.Length];
//        //                    ImgStream.Read(request.ImageByte, 0, request.ImageByte.Length);

//        //                    uploadImageRequest.Byte = request.ImageByte;
//        //                    uploadImageRequest.ImageInfo = new ImageMeta()
//        //                    {
//        //                        FileName = String.Format("{0}_{1}.{2}", request.ImageInfo.FileStoreId, size, request.ImageInfo.FileType),
//        //                        MetaDataDictionary = (request.ImageInfo.MetaDataDictionary != null) ? request.ImageInfo.MetaDataDictionary : new Dictionary<String, List<string>>() { },
//        //                        FileType = request.ImageInfo.FileType,
//        //                        FileCategory = EbFileCategory.Dp,
//        //                        ImageQuality = ImageQuality.other
//        //                    };
//        //                    uploadImageRequest.AddAuth(request.BToken, request.RToken);
//        //                    this.MessageProducer3.Publish(uploadImageRequest);
//        //                }
//        //            }
//        //            else if (request.ImageInfo.FileCategory == EbFileCategory.SolLogo)
//        //            {
//        //                foreach (string size in Enum.GetNames(typeof(LogoSizes)))
//        //                {
//        //                    int sz = (int)Enum.Parse<LogoSizes>(size);

//        //                    Stream ImgStream = Resize(img, sz, sz);
//        //                    request.ImageByte = new byte[ImgStream.Length];
//        //                    ImgStream.Read(request.ImageByte, 0, request.ImageByte.Length);

//        //                    uploadImageRequest.Byte = request.ImageByte;
//        //                    uploadImageRequest.ImageInfo = new ImageMeta()
//        //                    {
//        //                        FileName = String.Format("{0}_{1}.{2}", request.ImageInfo.FileStoreId, size, request.ImageInfo.FileType),
//        //                        MetaDataDictionary = (request.ImageInfo.MetaDataDictionary != null) ? request.ImageInfo.MetaDataDictionary : new Dictionary<String, List<string>>() { },
//        //                        FileType = request.ImageInfo.FileType,
//        //                        FileCategory = EbFileCategory.SolLogo,
//        //                        ImageQuality = ImageQuality.other
//        //                    };
//        //                    uploadImageRequest.AddAuth(request.BToken, request.RToken);
//        //                    this.MessageProducer3.Publish(uploadImageRequest);
//        //                }
//        //            }
//        //            else if (request.ImageInfo.FileCategory == EbFileCategory.LocationFile)
//        //            {
//        //                foreach (string size in Enum.GetNames(typeof(LogoSizes)))
//        //                {
//        //                    int sz = (int)Enum.Parse<LogoSizes>(size);

//        //                    Stream ImgStream = Resize(img, sz, sz);
//        //                    request.ImageByte = new byte[ImgStream.Length];
//        //                    ImgStream.Read(request.ImageByte, 0, request.ImageByte.Length);

//        //                    uploadImageRequest.Byte = request.ImageByte;
//        //                    uploadImageRequest.ImageInfo = new ImageMeta()
//        //                    {
//        //                        FileName = String.Format("{0}_{1}.{2}", request.ImageInfo.FileStoreId, size, request.ImageInfo.FileType),
//        //                        MetaDataDictionary = (request.ImageInfo.MetaDataDictionary != null) ? request.ImageInfo.MetaDataDictionary : new Dictionary<String, List<string>>() { },
//        //                        FileType = request.ImageInfo.FileType,
//        //                        FileCategory = EbFileCategory.LocationFile
//        //                    };

//        //                    uploadImageRequest.AddAuth(request.BToken, request.RToken);
//        //                    this.MessageProducer3.Publish(uploadImageRequest);
//        //                }
//        //            }
//        //            else
//        //            {
//        //                foreach (string size in Enum.GetNames(typeof(ImageQuality)))
//        //                {

//        //                    int sz = (int)Enum.Parse<ImageQuality>(size);

//        //                    if (sz > 1 && sz < 500)
//        //                    {
//        //                        Stream ImgStream = Resize(img, sz, sz);

//        //                        request.ImageByte = new byte[ImgStream.Length];
//        //                        ImgStream.Read(request.ImageByte, 0, request.ImageByte.Length);

//        //                        uploadImageRequest.ImageInfo = new ImageMeta()
//        //                        {
//        //                            FileName = String.Format("{0}_{1}.{2}", request.ImageInfo.FileStoreId, size, request.ImageInfo.FileType),
//        //                            MetaDataDictionary = (request.ImageInfo.MetaDataDictionary != null) ? request.ImageInfo.MetaDataDictionary : new Dictionary<String, List<string>>() { },
//        //                            FileType = request.ImageInfo.FileType,
//        //                            FileCategory = EbFileCategory.Images,
//        //                            ImageQuality = Enum.Parse<ImageQuality>(size),
//        //                            FileRefId = request.ImageInfo.FileRefId // Not needed resized images are not updated in eb_files_ref
//        //                        };
//        //                        uploadImageRequest.Byte = request.ImageByte;

//        //                        uploadImageRequest.AddAuth(request.BToken, request.RToken);
//        //                        this.MessageProducer3.Publish(uploadImageRequest);
//        //                    }
//        //                }
//        //            }
//        //        }
//        //    }
//        //    catch (Exception e)
//        //    {
//        //        Log.Info("Exception:" + e.ToString());
//        //    }
//        //    return null;
//        //}



//        //public static Stream Resize(Image img, int newWidth, int newHeight)
//        //{
//        //    if (newWidth != img.Width || newHeight != img.Height)
//        //    {
//        //        var ratioX = (double)newWidth / img.Width;
//        //        var ratioY = (double)newHeight / img.Height;
//        //        var ratio = Math.Max(ratioX, ratioY);
//        //        var width = (int)(img.Width * ratio);
//        //        var height = (int)(img.Height * ratio);

//        //        var newImage = new Bitmap(width, height);
//        //        Graphics.FromImage(newImage).DrawImage(img, 0, 0, width, height);
//        //        img = newImage;
//        //    }

//        //    var ms = new MemoryStream();
//        //    img.Save(ms, ImageFormat.Png);
//        //    ms.Position = 0;
//        //    return ms;
//        //}
//    }
//}
