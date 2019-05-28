using ExpressBase.Common;
using ExpressBase.Common.Data;
using ExpressBase.Common.EbServiceStack.ReqNRes;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Data;

namespace ExpressBase.StaticFileServer.Services
{
    public class SearchServices : BaseService
    {
        public SearchServices(IEbConnectionFactory _dbf) : base(_dbf)
        {
        }

        [Authenticate]
        public List<FileMeta> Post(FindFilesByTagRequest request)
        {
            List<FileMeta> FileList = new List<FileMeta>();
            
            string sql = @"SELECT id, userid, filestore_id, length, tags, filecategory, filetype, uploadts, eb_del FROM eb_files_ref WHERE regexp_split_to_array(tags, ',') @> @tags AND COALESCE(eb_del, 'F')='F';";
            DataTable dt = new DataTable();

            if (EbConnectionFactory.DataDB.Vendor == DatabaseVendors.MYSQL)
            {
                throw new NotImplementedException();

            }
            else if (EbConnectionFactory.DataDB.Vendor == DatabaseVendors.PGSQL)
            {
                using (var con = this.EbConnectionFactory.DataDB.GetNewConnection() as Npgsql.NpgsqlConnection)
                {
                    try
                    {
                        con.Open();

                        var ada = new Npgsql.NpgsqlDataAdapter(sql, con);
                        ada.SelectCommand.Parameters.Add(new Npgsql.NpgsqlParameter("tags", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text) { Value = request.Tags });
                        ada.Fill(dt);

                        foreach (DataRow dr in dt.Rows)
                        {
                            FileList.Add(
                                new FileMeta()
                                {
                                    FileStoreId = dr["objid"].ToString(),
                                    FileType = dr["filetype"].ToString(),
                                    Length = (Int64)dr["length"],
                                    UploadDateTime = (DateTime)dr["uploaddatetime"]
                                });
                        }
                        return FileList;
                    }
                    catch (Exception e)
                    {
                        Log.Info("Exception:" + e.ToString());
                        return null;
                    }
                }
            }
            return null;
        }
    }
}