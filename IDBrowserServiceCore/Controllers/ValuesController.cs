using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using IDBrowserServiceCore.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;

namespace IDBrowserServiceCore.Controllers
{
    [Route("api/[controller]/[action]")]
    public class ValuesController : Controller
    {
        private TransactionOptions readUncommittedTransactionOptions;
        private IDImagerDB db;

        public ValuesController()
        {
            readUncommittedTransactionOptions = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadUncommitted
            };

            if (db == null)
                db = new IDImagerDB();
        }

        [HttpGet("{guid?}")]
        [ActionName("GetImageProperties")]
        public List<ImageProperty> GetImageProperties(string guid)
        {
            try
            {
                List<ImageProperty> listImageProperty;

                if (guid == null)
                {
                    var query = from tbl in db.v_PropCategory
                                where !tbl.CategoryName.Equals("Internal")
                                orderby tbl.CategoryName
                                select new ImageProperty
                                {
                                    GUID = tbl.GUID,
                                    Name = tbl.CategoryName,
                                    ImageCount = tbl.idPhotoCount,
                                    SubPropertyCount = db.idProp.Where(x => x.ParentGUID == tbl.GUID).Count()
                                };

                    listImageProperty = query.ToList();
                }
                else
                {
                    var query = from tbl in db.v_prop
                                where tbl.ParentGUID == guid
                                orderby tbl.PropName
                                select new ImageProperty
                                {
                                    GUID = tbl.GUID,
                                    Name = tbl.PropName,
                                    ImageCount = tbl.idPhotoCount,
                                    SubPropertyCount = db.idProp.Where(x => x.ParentGUID == tbl.GUID).Count()
                                };

                    listImageProperty = query.ToList();
                }

                return listImageProperty;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
