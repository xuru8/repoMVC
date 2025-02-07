﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using webtest01.Models;

namespace webtest01.Controllers
{
    public class DeleteLoveController : ApiController
    {
        trytryEntities db = new trytryEntities();

        // POST: api/DeleteLove
        // 刪除我的最愛
        public int Post(receiveObj obj)
        {
            int num = 0;
            try
            {
                var favorite有無存在 =
                   (from c in db.Customer_Favorite
                    where c.CustomerID == obj.cid & c.StoreID == obj.sid
                    select c).FirstOrDefault();
                if (favorite有無存在 != null) // 存在
                {
                    //Customer_Favorite Favorite = new Customer_Favorite();
                    //Favorite.CustomerID = Convert.ToInt32(obj.cid);
                    //Favorite.StoreID = Convert.ToInt32(obj.sid);
                    db.Customer_Favorite.Remove(favorite有無存在);
                    num = db.SaveChanges();
                    return num;
                }
                else  // 不存在
                {
                    num = 0;
                    return num;
                }
            }
            catch (Exception ex)
            {
                num = 0;
            }
            return num;
        }
        
    }
}
