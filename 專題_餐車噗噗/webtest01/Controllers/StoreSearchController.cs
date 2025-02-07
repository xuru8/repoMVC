﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using webtest01.Models;

namespace testWebApi.Controllers
{
    public class StoreSearchController : ApiController
    {
        trytryEntities db = new trytryEntities();       

        IQueryable storesSearch;
        string CusID;

        public IQueryable Post(receiveObj obj)
        {
            //
            //要再補一個判斷有沒有登入過->改用Layout去做控制，登入成功的Navbar會用Session存值並顯示不同Navbar。
            //
            // 先把前端傳遞過來的JSON資料做字串內容整理，再去對應資料庫裡的資料。
            //
            if (obj.fdate != "")
            {
                // 將搜尋的營業日期做字串修改，以便與資料庫內的資料做搜尋比較。
                // Replace 改變原有的字串內容
                searchTimeClass.nowdate = obj.fdate.Replace("-", "");
            }
            
            if (obj.ftime != "")
            {
                //將搜尋的營業時間切割為陣列，以便與資料庫內的資料做搜尋比較。ex:06:00-12:00以"-"切割成，string[0]="06:00"，string[1]="12:00"。
                // Split 切割字串
                string[] times = obj.ftime.Split('-');
                for (int i = 0; i < times.Length; i++)
                {
                    if (i == 0)
                    {
                        // ex:string[0]="06:00"，Replace成string[0]="0600"
                        searchTimeClass.fortime0 = times[i].Replace(":", "");
                    }
                    else if (i == 1)
                    {
                        // ex:string[1]="12:00"，Replace成string[1]="1200"
                        searchTimeClass.fortime1 = times[i].Replace(":", "");
                    }
                }

                // 把搜尋的營業日期和營業時間[0]加起來(這邊相加是string)，然後再轉為int。
                searchTimeClass.dateTimeANDsearchStart = Convert.ToInt64(searchTimeClass.nowdate + searchTimeClass.fortime0);

                // 把搜尋的營業日期和營業時間[1]加起來(這邊相加是string)，然後再轉為int。
                searchTimeClass.dateTimeANDsearchEnd = Convert.ToInt64(searchTimeClass.nowdate + searchTimeClass.fortime1);
            }
            //----------------------------------------------------------------------------------------------------------------------------------------//
            //         
            // LINQ的contains搜尋資料庫時，資料庫的欄位資料不能為NULL值，與SQL語法的like%%會自動跳過null值不同。以下分為四個部分。
            //
            //----------------------------------------------------------------------------------------------------------------------------------------//
            //
            //狀態1: 日期、時間，都有搜尋
            //
            if (obj.ftime != "" && obj.fdate != "")
            {
                //
                //狀態1-1: 日期、時間，都有搜尋，且依據搜尋時段是否為12:00~18:00做判斷區分。
                //
                if (Convert.ToInt32(searchTimeClass.fortime0) == 1200)
                {
                    var tt =
                        from z in
                        (  // 4. 呈現出來的會是全部店家，把這些包在 z 裡面，再去做搜尋的where判斷。
                            from y in
                            (
                                        // 3.整合成 y 之後，再去用 group by 和 order by 去整理後再 select 成要呈現的樣子。
                                        from x in
                                            (
                                            // 1.先把Store和Store_Business整合成一個 x 。
                                            // 2.再用x去left join Message_Board into 到 g 並利用 DefaultIfEmpty() 去整合成 ssbm，在ssbm裡面去用select選值並計算後利用class去接值後，整合成 y 。
                                                    from s in db.Store
                                                    join sb in db.Store_Business
                                                    on s.StoreID equals sb.StoreID
                                                    select new
                                                    {
                                                        StoreID = s.StoreID,
                                                        Store_Name = s.Store_Name,
                                                        Store_Class = s.Store_Class,
                                                        Address_Area = s.Address_Area,
                                                        Address_City = sb.Address_City,
                                                        Address_Detail = sb.Address_City + sb.Address_Local + sb.Store_Address,
                                                        Picture = s.Picture,
                                                        Phone = s.Phone,
                                                        Introduce = s.Introduce,
                                                        Punch_Start = sb.Punch_Start,
                                                        Punch_End = sb.Punch_End,
                                                        Punch_Time = sb.Punch_Start + "~" + sb.Punch_End.Substring(sb.Punch_End.Length - 5),
                                                        CalendarID = sb.CalendarID,
                                                    }

                            ) // x in(x 的裡面)
                                        join m in db.Message_Board
                                        on x.StoreID equals m.StoreID into g
                                        from ssbm in g.DefaultIfEmpty()  
                                        //如果 Sequence 不包含任何元素，則 DefaultIfEmpty 用於返回預設元素。此元素可以是型別的預設值或該型別的使用者定義例項。
                                        select new SearchStoreCard()
                                        {
                                            //在這邊把left JOIN後的值都先select出來並且計算好後再出去外層用group分類into成一個新的然後顯示
                                            StoreID = x.StoreID,
                                            Store_Name = x.Store_Name,
                                            Store_Class = x.Store_Class,
                                            Address_Area = x.Address_Area,
                                            Address_City = x.Address_City,
                                            Address_Detail = x.Address_Detail,
                                            Picture = x.Picture,
                                            Phone = x.Phone,
                                            Introduce = x.Introduce,
                                            Punch_Start = x.Punch_Start,
                                            Punch_End = x.Punch_End,
                                            Punch_Time = x.Punch_Time,
                                            StarRating = Math.Round((double)g.Average(m => m.Star_Rating), 1),  //平均後取到小數點後第一位
                                            ContentCount = g.Count(),
                                            CalendarID = x.CalendarID
                                        }
                    ) // y in(y 的裡面)

                                        group new
                                        {
                                            // y 去 group
                                            y.StoreID,
                                            y.Store_Name,
                                            y.Store_Class,
                                            y.Address_Area,
                                            y.Address_City,
                                            y.Address_Detail,
                                            y.Picture,
                                            y.Phone,
                                            y.Introduce,
                                            y.Punch_Start,
                                            y.Punch_End,
                                            y.Punch_Time,
                                            //y.StarRating,
                                            //y.ContentCount
                                        }
                                        by new
                                        {
                                            y.StoreID,
                                            y.Store_Name,
                                            y.Store_Class,
                                            y.Address_Area,
                                            y.Address_City,
                                            y.Address_Detail,
                                            y.Picture,
                                            y.Phone,
                                            y.Introduce,
                                            y.Punch_Start,
                                            y.Punch_End,
                                            y.Punch_Time,
                                            y.StarRating,
                                            y.ContentCount,
                                            y.CalendarID
                                        }
                                        into gro
                                        orderby gro.Key.Punch_Start
                                        select new
                                        {
                                            CalendarID = gro.Key.CalendarID,
                                            StoreID = gro.Key.StoreID,
                                            Store_Name = gro.Key.Store_Name,
                                            Store_Class = gro.Key.Store_Class,
                                            Address_Area = gro.Key.Address_Area,
                                            Address_City = gro.Key.Address_City,
                                            Address_Detail = gro.Key.Address_Detail,
                                            Picture = gro.Key.Picture,
                                            Phone = gro.Key.Phone,
                                            Introduce = gro.Key.Introduce,
                                            Punch_Start = gro.Key.Punch_Start,
                                            Punch_End = gro.Key.Punch_End,
                                            Punch_Time = gro.Key.Punch_Time,
                                            StarRating = gro.Key.StarRating == null ? 0 : gro.Key.StarRating, //如果等於null值(尚未有評論)時變成0，如果不等於就顯示正常的
                                            ContentCount = gro.Key.ContentCount
                                        }
                        ).AsEnumerable() // z in (z 的裡面)         
                        where z.CalendarID == 1
                             & z.Store_Name.Contains(obj.fname)
                        & z.Store_Class.Contains(obj.fclass)
                        & z.Address_City.Contains(obj.fcity)
                        & (Convert.ToInt32((z.Punch_Start.Substring(0, 10)).Replace("-", "")) == Convert.ToInt32(searchTimeClass.nowdate))
                        &
                        (
                        ((Convert.ToInt64(z.Punch_Start.Replace("-", "").Replace(" ", "").Replace(":", "")) >= Convert.ToInt64(searchTimeClass.nowdate + "0600"))
                        &
                            (Convert.ToInt64(z.Punch_Start.Replace("-", "").Replace(" ", "").Replace(":", "")) <= searchTimeClass.dateTimeANDsearchEnd))
                        |
                        ((Convert.ToInt64(z.Punch_End.Replace("-", "").Replace(" ", "").Replace(":", "")) >= searchTimeClass.dateTimeANDsearchStart)
                        &
                            (Convert.ToInt64(z.Punch_End.Replace("-", "").Replace(" ", "").Replace(":", "")) <= searchTimeClass.dateTimeANDsearchEnd))
                            )
                        orderby Convert.ToInt64(z.Punch_Start.Replace("-", "").Replace(" ", "").Replace(":", ""))
                        select new SearchStoreCard()
                        {
                            StoreID = z.StoreID,
                            Store_Name = z.Store_Name,
                            Store_Class = z.Store_Class,
                            Address_Area = z.Address_Area,
                            Address_City = z.Address_City,
                            Address_Detail = z.Address_Detail,
                            Picture = z.Picture,
                            Phone = z.Phone,
                            Introduce = z.Introduce,
                            Punch_Start = z.Punch_Start,
                            Punch_End = z.Punch_End,
                            Punch_Time = z.Punch_Time,
                            StarRating = z.StarRating,
                            ContentCount = z.ContentCount
                        };
                    storesSearch = tt.AsQueryable();

                    //if (storesSearch == null)
                    //{
                    //    var noresult = "查無資料";
                    //    return noresult.AsQueryable();
                    //}

                    return storesSearch;
                }
                //----------------------------------------------------------------------------------------------------------------------------------------//                
                //
                // 狀態1-2: 日期、時間，都有搜尋後再依據搜尋時段是否為12:00~18:00做判斷區分，else為12:00~18:00以外的時段。
                //
                else
                {
                    var tt =
                    from z in
                    (
                             from y in
                            (
                                     from x in
                                    (
                                        from s in db.Store
                                        join sb in db.Store_Business
                                        on s.StoreID equals sb.StoreID
                                        select new
                                        {
                                            StoreID = s.StoreID,
                                            Store_Name = s.Store_Name,
                                            Store_Class = s.Store_Class,
                                            Address_Area = s.Address_Area,
                                            Address_City = sb.Address_City,
                                            Address_Detail = sb.Address_City + sb.Address_Local + sb.Store_Address,
                                            Picture = s.Picture,
                                            Phone = s.Phone,
                                            Introduce = s.Introduce,
                                            Punch_Start = sb.Punch_Start,
                                            Punch_End = sb.Punch_End,
                                            Punch_Time = sb.Punch_Start + "~" + sb.Punch_End.Substring(sb.Punch_End.Length - 5),
                                            CalendarID = sb.CalendarID,
                                        }

                                    ) // x in(x 的裡面)
                                     join m in db.Message_Board
                                     on x.StoreID equals m.StoreID into g
                                     from ssbm in g.DefaultIfEmpty()
                                     select new SearchStoreCard()
                                     {
                                         StoreID = x.StoreID,
                                         Store_Name = x.Store_Name,
                                         Store_Class = x.Store_Class,
                                         Address_Area = x.Address_Area,
                                         Address_City = x.Address_City,
                                         Address_Detail = x.Address_Detail,
                                         Picture = x.Picture,
                                         Phone = x.Phone,
                                         Introduce = x.Introduce,
                                         Punch_Start = x.Punch_Start,
                                         Punch_End = x.Punch_End,
                                         Punch_Time = x.Punch_Time,
                                         StarRating = Math.Round((double)g.Average(m => m.Star_Rating), 1),
                                         ContentCount = g.Count(),
                                         CalendarID = x.CalendarID
                                     }
                                    ) //y

                             group new
                             {
                                 y.StoreID,
                                 y.Store_Name,
                                 y.Store_Class,
                                 y.Address_Area,
                                 y.Address_City,
                                 y.Address_Detail,
                                 y.Picture,
                                 y.Phone,
                                 y.Introduce,
                                 y.Punch_Start,
                                 y.Punch_End,
                                 y.Punch_Time,
                                 //y.StarRating,
                                 //y.ContentCount
                             }
                             by new
                             {
                                 y.StoreID,
                                 y.Store_Name,
                                 y.Store_Class,
                                 y.Address_Area,
                                 y.Address_City,
                                 y.Address_Detail,
                                 y.Picture,
                                 y.Phone,
                                 y.Introduce,
                                 y.Punch_Start,
                                 y.Punch_End,
                                 y.Punch_Time,
                                 y.StarRating,
                                 y.ContentCount,
                                 y.CalendarID
                             }
                                        into gro
                             orderby gro.Key.Punch_Start
                             select new
                             {
                                 CalendarID = gro.Key.CalendarID,
                                 StoreID = gro.Key.StoreID,
                                 Store_Name = gro.Key.Store_Name,
                                 Store_Class = gro.Key.Store_Class,
                                 Address_Area = gro.Key.Address_Area,
                                 Address_City = gro.Key.Address_City,
                                 Address_Detail = gro.Key.Address_Detail,
                                 Picture = gro.Key.Picture,
                                 Phone = gro.Key.Phone,
                                 Introduce = gro.Key.Introduce,
                                 Punch_Start = gro.Key.Punch_Start,
                                 Punch_End = gro.Key.Punch_End,
                                 Punch_Time = gro.Key.Punch_Time,
                                 StarRating = gro.Key.StarRating == null ? 0 : gro.Key.StarRating,
                                 ContentCount = gro.Key.ContentCount
                             }
                                    ).AsEnumerable() //z              

                    where z.CalendarID == 1
                             & z.Store_Name.Contains(obj.fname)
                         & z.Store_Class.Contains(obj.fclass)
                         & z.Address_City.Contains(obj.fcity)
                         & (Convert.ToInt32((z.Punch_Start.Substring(0, 10)).Replace("-", "")) == Convert.ToInt32(searchTimeClass.nowdate))
                         &
                         (
                         ((Convert.ToInt64(z.Punch_Start.Replace("-", "").Replace(" ", "").Replace(":", "")) >= searchTimeClass.dateTimeANDsearchStart)
                         &
                             (Convert.ToInt64(z.Punch_Start.Replace("-", "").Replace(" ", "").Replace(":", "")) <= searchTimeClass.dateTimeANDsearchEnd))
                         |
                         ((Convert.ToInt64(z.Punch_End.Replace("-", "").Replace(" ", "").Replace(":", "")) >= searchTimeClass.dateTimeANDsearchStart)
                         &
                             (Convert.ToInt64(z.Punch_End.Replace("-", "").Replace(" ", "").Replace(":", "")) <= searchTimeClass.dateTimeANDsearchEnd))
                             )
                    orderby Convert.ToInt64(z.Punch_Start.Replace("-", "").Replace(" ", "").Replace(":", ""))
                    select new SearchStoreCard()
                    {
                        StoreID = z.StoreID,
                        Store_Name = z.Store_Name,
                        Store_Class = z.Store_Class,
                        Address_Area = z.Address_Area,
                        Address_City = z.Address_City,
                        Address_Detail = z.Address_Detail,
                        Picture = z.Picture,
                        Phone = z.Phone,
                        Introduce = z.Introduce,
                        Punch_Start = z.Punch_Start,
                        Punch_End = z.Punch_End,
                        Punch_Time = z.Punch_Time,
                        StarRating = z.StarRating,
                        ContentCount = z.ContentCount
                    };
                    storesSearch = tt.AsQueryable();
                    return storesSearch;
                }
            }
            //----------------------------------------------------------------------------------------------------------------------------------------//                
            //
            // 狀態2: 有搜尋日期，沒搜尋時間
            // 
            else if (obj.ftime == "" && obj.fdate != "")
            {
                var tt =
                from z in
                (
                         from y in
                        (
                                 from x in
                                (
                                    from s in db.Store
                                    join sb in db.Store_Business
                                    on s.StoreID equals sb.StoreID
                                    select new
                                    {
                                        StoreID = s.StoreID,
                                        Store_Name = s.Store_Name,
                                        Store_Class = s.Store_Class,
                                        Address_Area = s.Address_Area,
                                        Address_City = sb.Address_City,
                                        Address_Detail = sb.Address_City + sb.Address_Local + sb.Store_Address,
                                        Picture = s.Picture,
                                        Phone = s.Phone,
                                        Introduce = s.Introduce,
                                        Punch_Start = sb.Punch_Start,
                                        Punch_End = sb.Punch_End,
                                        Punch_Time = sb.Punch_Start + "~" + sb.Punch_End.Substring(sb.Punch_End.Length - 5),
                                        CalendarID = sb.CalendarID,
                                    }

                                ) // x in(x 的裡面)
                                 join m in db.Message_Board
                                 on x.StoreID equals m.StoreID into g
                                 from ssbm in g.DefaultIfEmpty()
                                 select new SearchStoreCard()
                                 {
                                     StoreID = x.StoreID,
                                     Store_Name = x.Store_Name,
                                     Store_Class = x.Store_Class,
                                     Address_Area = x.Address_Area,
                                     Address_City = x.Address_City,
                                     Address_Detail = x.Address_Detail,
                                     Picture = x.Picture,
                                     Phone = x.Phone,
                                     Introduce = x.Introduce,
                                     Punch_Start = x.Punch_Start,
                                     Punch_End = x.Punch_End,
                                     Punch_Time = x.Punch_Time,
                                     StarRating = Math.Round((double)g.Average(m => m.Star_Rating), 1),
                                     ContentCount = g.Count(),
                                     CalendarID = x.CalendarID
                                 }
                                ) //y

                         group new
                         {
                             y.StoreID,
                             y.Store_Name,
                             y.Store_Class,
                             y.Address_Area,
                             y.Address_City,
                             y.Address_Detail,
                             y.Picture,
                             y.Phone,
                             y.Introduce,
                             y.Punch_Start,
                             y.Punch_End,
                             y.Punch_Time,
                             //y.StarRating,
                             //y.ContentCount
                         }
                         by new
                         {
                             y.StoreID,
                             y.Store_Name,
                             y.Store_Class,
                             y.Address_Area,
                             y.Address_City,
                             y.Address_Detail,
                             y.Picture,
                             y.Phone,
                             y.Introduce,
                             y.Punch_Start,
                             y.Punch_End,
                             y.Punch_Time,
                             y.StarRating,
                             y.ContentCount,
                             y.CalendarID
                         }
                                    into gro
                         orderby gro.Key.Punch_Start
                         select new
                         {
                             CalendarID = gro.Key.CalendarID,
                             StoreID = gro.Key.StoreID,
                             Store_Name = gro.Key.Store_Name,
                             Store_Class = gro.Key.Store_Class,
                             Address_Area = gro.Key.Address_Area,
                             Address_City = gro.Key.Address_City,
                             Address_Detail = gro.Key.Address_Detail,
                             Picture = gro.Key.Picture,
                             Phone = gro.Key.Phone,
                             Introduce = gro.Key.Introduce,
                             Punch_Start = gro.Key.Punch_Start,
                             Punch_End = gro.Key.Punch_End,
                             Punch_Time = gro.Key.Punch_Time,
                             StarRating = gro.Key.StarRating == null ? 0 : gro.Key.StarRating,
                             ContentCount = gro.Key.ContentCount
                         }
                                ).AsEnumerable() //z  

                where z.CalendarID == 1
                         & z.Store_Name.Contains(obj.fname)
                     & z.Store_Class.Contains(obj.fclass)
                     & z.Address_City.Contains(obj.fcity)
                     & (Convert.ToInt32((z.Punch_Start.Substring(0, 10)).Replace("-", "")) == Convert.ToInt32(searchTimeClass.nowdate))
                orderby Convert.ToInt64(z.Punch_Start.Replace("-", "").Replace(" ", "").Replace(":", ""))
                //& sb.Punch_day.ToString().Replace("-", "").Remove(4,1) == searchTimeClass.nowdate
                select new SearchStoreCard()
                {
                    StoreID = z.StoreID,
                    Store_Name = z.Store_Name,
                    Store_Class = z.Store_Class,
                    Address_Area = z.Address_Area,
                    Address_City = z.Address_City,
                    Address_Detail = z.Address_Detail,
                    Picture = z.Picture,
                    Phone = z.Phone,
                    Introduce = z.Introduce,
                    Punch_Start = z.Punch_Start,
                    Punch_End = z.Punch_End,
                    Punch_Time = z.Punch_Time,
                    StarRating = z.StarRating,
                    ContentCount = z.ContentCount
                };
                storesSearch = tt.AsQueryable();
                return storesSearch;
            }
            //----------------------------------------------------------------------------------------------------------------------------------------//
            //
            // 狀態3: 有搜尋時間，沒有搜尋日期。
            //
            else if (obj.ftime != "" && obj.fdate == "")
            {
                //
                // 狀態3-1: 有搜尋時間，沒有搜尋日期，再依據搜尋時間是否為12:00~18:00做判斷區分
                //
                if (Convert.ToInt32(searchTimeClass.fortime0) == 1200)
                {
                    var tt =
                    from z in
                    (
                             from y in
                            (
                                     from x in
                                    (
                                        from s in db.Store
                                        join sb in db.Store_Business
                                        on s.StoreID equals sb.StoreID
                                        select new
                                        {
                                            StoreID = s.StoreID,
                                            Store_Name = s.Store_Name,
                                            Store_Class = s.Store_Class,
                                            Address_Area = s.Address_Area,
                                            Address_City = sb.Address_City,
                                            Address_Detail = sb.Address_City + sb.Address_Local + sb.Store_Address,
                                            Picture = s.Picture,
                                            Phone = s.Phone,
                                            Introduce = s.Introduce,
                                            Punch_Start = sb.Punch_Start,
                                            Punch_End = sb.Punch_End,
                                            Punch_Time = sb.Punch_Start + "~" + sb.Punch_End.Substring(sb.Punch_End.Length - 5),
                                            CalendarID = sb.CalendarID,
                                        }

                                    ) // x in(x 的裡面)
                                     join m in db.Message_Board
                                     on x.StoreID equals m.StoreID into g
                                     from ssbm in g.DefaultIfEmpty()
                                     select new SearchStoreCard()
                                     {
                                         StoreID = x.StoreID,
                                         Store_Name = x.Store_Name,
                                         Store_Class = x.Store_Class,
                                         Address_Area = x.Address_Area,
                                         Address_City = x.Address_City,
                                         Address_Detail = x.Address_Detail,
                                         Picture = x.Picture,
                                         Phone = x.Phone,
                                         Introduce = x.Introduce,
                                         Punch_Start = x.Punch_Start,
                                         Punch_End = x.Punch_End,
                                         Punch_Time = x.Punch_Time,
                                         StarRating = Math.Round((double)g.Average(m => m.Star_Rating), 1),
                                         ContentCount = g.Count(),
                                         CalendarID = x.CalendarID
                                     }
                                    ) //y

                             group new
                             {
                                 y.StoreID,
                                 y.Store_Name,
                                 y.Store_Class,
                                 y.Address_Area,
                                 y.Address_City,
                                 y.Address_Detail,
                                 y.Picture,
                                 y.Phone,
                                 y.Introduce,
                                 y.Punch_Start,
                                 y.Punch_End,
                                 y.Punch_Time,
                                 //y.StarRating,
                                 //y.ContentCount
                             }
                             by new
                             {
                                 y.StoreID,
                                 y.Store_Name,
                                 y.Store_Class,
                                 y.Address_Area,
                                 y.Address_City,
                                 y.Address_Detail,
                                 y.Picture,
                                 y.Phone,
                                 y.Introduce,
                                 y.Punch_Start,
                                 y.Punch_End,
                                 y.Punch_Time,
                                 y.StarRating,
                                 y.ContentCount,
                                 y.CalendarID
                             }
                                        into gro
                             orderby gro.Key.Punch_Start
                             select new
                             {
                                 CalendarID = gro.Key.CalendarID,
                                 StoreID = gro.Key.StoreID,
                                 Store_Name = gro.Key.Store_Name,
                                 Store_Class = gro.Key.Store_Class,
                                 Address_Area = gro.Key.Address_Area,
                                 Address_City = gro.Key.Address_City,
                                 Address_Detail = gro.Key.Address_Detail,
                                 Picture = gro.Key.Picture,
                                 Phone = gro.Key.Phone,
                                 Introduce = gro.Key.Introduce,
                                 Punch_Start = gro.Key.Punch_Start,
                                 Punch_End = gro.Key.Punch_End,
                                 Punch_Time = gro.Key.Punch_Time,
                                 StarRating = gro.Key.StarRating == null ? 0 : gro.Key.StarRating,
                                 ContentCount = gro.Key.ContentCount
                             }
                                    ).AsEnumerable() //z     

                    where z.CalendarID == 1
                             & z.Store_Name.Contains(obj.fname)
                                 & z.Store_Class.Contains(obj.fclass)
                                 & z.Address_City.Contains(obj.fcity)
                                 &
                                     (
                                     (Convert.ToInt64(z.Punch_Start.Replace("-", "").Replace(" ", "").Replace(":", ""))) >= (Convert.ToInt64((z.Punch_Start.Substring(0, 10).Replace("-", "")) + "0600"))
                                     &
                                        (Convert.ToInt64(z.Punch_Start.Replace("-", "").Replace(" ", "").Replace(":", ""))) <= (Convert.ToInt64((z.Punch_Start.Substring(0, 10).Replace("-", "")) + searchTimeClass.fortime1))
                                     |
                                     (Convert.ToInt64(z.Punch_End.Replace("-", "").Replace(" ", "").Replace(":", ""))) >= (Convert.ToInt64((z.Punch_Start.Substring(0, 10).Replace("-", "")) + searchTimeClass.fortime0))
                                     &
                                        (Convert.ToInt64(z.Punch_End.Replace("-", "").Replace(" ", "").Replace(":", ""))) <= (Convert.ToInt64((z.Punch_Start.Substring(0, 10).Replace("-", "")) + searchTimeClass.fortime1))
                                    )
                    orderby Convert.ToInt64(z.Punch_Start.Replace("-", "").Replace(" ", "").Replace(":", ""))
                    //& sb.Punch_day.ToString().Replace("-", "").Remove(4,1) == searchTimeClass.nowdate
                    select new SearchStoreCard()
                    {
                        StoreID = z.StoreID,
                        Store_Name = z.Store_Name,
                        Store_Class = z.Store_Class,
                        Address_Area = z.Address_Area,
                        Address_City = z.Address_City,
                        Address_Detail = z.Address_Detail,
                        Picture = z.Picture,
                        Phone = z.Phone,
                        Introduce = z.Introduce,
                        Punch_Start = z.Punch_Start,
                        Punch_End = z.Punch_End,
                        Punch_Time = z.Punch_Time,
                        StarRating = z.StarRating,
                        ContentCount = z.ContentCount
                    };
                    storesSearch = tt.AsQueryable();
                    return storesSearch;
                }
                //----------------------------------------------------------------------------------------------------------------------------------------//
                //
                // 狀態3-2: 有搜尋時間，沒有搜尋日期，再依據搜尋時間是否為12:00~18:00做判斷區分，else為12:00-18:00以外的時段。
                //
                else
                {
                    var tt =
                    from z in
                    (
                             from y in
                            (
                                     from x in
                                    (
                                        from s in db.Store
                                        join sb in db.Store_Business
                                        on s.StoreID equals sb.StoreID
                                        select new
                                        {
                                            StoreID = s.StoreID,
                                            Store_Name = s.Store_Name,
                                            Store_Class = s.Store_Class,
                                            Address_Area = s.Address_Area,
                                            Address_City = sb.Address_City,
                                            Address_Detail = sb.Address_City + sb.Address_Local + sb.Store_Address,
                                            Picture = s.Picture,
                                            Phone = s.Phone,
                                            Introduce = s.Introduce,
                                            Punch_Start = sb.Punch_Start,
                                            Punch_End = sb.Punch_End,
                                            Punch_Time = sb.Punch_Start + "~" + sb.Punch_End.Substring(sb.Punch_End.Length - 5),
                                            CalendarID = sb.CalendarID,
                                        }

                                    ) // x in(x 的裡面)
                                     join m in db.Message_Board
                                     on x.StoreID equals m.StoreID into g
                                     from ssbm in g.DefaultIfEmpty()
                                     select new SearchStoreCard()
                                     {
                                         StoreID = x.StoreID,
                                         Store_Name = x.Store_Name,
                                         Store_Class = x.Store_Class,
                                         Address_Area = x.Address_Area,
                                         Address_City = x.Address_City,
                                         Address_Detail = x.Address_Detail,
                                         Picture = x.Picture,
                                         Phone = x.Phone,
                                         Introduce = x.Introduce,
                                         Punch_Start = x.Punch_Start,
                                         Punch_End = x.Punch_End,
                                         Punch_Time = x.Punch_Time,
                                         StarRating = Math.Round((double)g.Average(m => m.Star_Rating), 1),
                                         ContentCount = g.Count(),
                                         CalendarID = x.CalendarID
                                     }
                                    ) //y

                             group new
                             {
                                 y.StoreID,
                                 y.Store_Name,
                                 y.Store_Class,
                                 y.Address_Area,
                                 y.Address_City,
                                 y.Address_Detail,
                                 y.Picture,
                                 y.Phone,
                                 y.Introduce,
                                 y.Punch_Start,
                                 y.Punch_End,
                                 y.Punch_Time,
                                 //y.StarRating,
                                 //y.ContentCount
                             }
                             by new
                             {
                                 y.StoreID,
                                 y.Store_Name,
                                 y.Store_Class,
                                 y.Address_Area,
                                 y.Address_City,
                                 y.Address_Detail,
                                 y.Picture,
                                 y.Phone,
                                 y.Introduce,
                                 y.Punch_Start,
                                 y.Punch_End,
                                 y.Punch_Time,
                                 y.StarRating,
                                 y.ContentCount,
                                 y.CalendarID
                             }
                                        into gro
                             orderby gro.Key.Punch_Start
                             select new
                             {
                                 CalendarID = gro.Key.CalendarID,
                                 StoreID = gro.Key.StoreID,
                                 Store_Name = gro.Key.Store_Name,
                                 Store_Class = gro.Key.Store_Class,
                                 Address_Area = gro.Key.Address_Area,
                                 Address_City = gro.Key.Address_City,
                                 Address_Detail = gro.Key.Address_Detail,
                                 Picture = gro.Key.Picture,
                                 Phone = gro.Key.Phone,
                                 Introduce = gro.Key.Introduce,
                                 Punch_Start = gro.Key.Punch_Start,
                                 Punch_End = gro.Key.Punch_End,
                                 Punch_Time = gro.Key.Punch_Time,
                                 StarRating = gro.Key.StarRating == null ? 0 : gro.Key.StarRating,
                                 ContentCount = gro.Key.ContentCount
                             }
                                    ).AsEnumerable() //z     

                    where z.CalendarID == 1
                             & z.Store_Name.Contains(obj.fname)
                             & z.Store_Class.Contains(obj.fclass)
                             & z.Address_City.Contains(obj.fcity)
                             &
                                 (
                                 (Convert.ToInt64(z.Punch_Start.Replace("-", "").Replace(" ", "").Replace(":", ""))) >= (Convert.ToInt64((z.Punch_Start.Substring(0, 10).Replace("-", "")) + searchTimeClass.fortime0))
                                 &
                                    (Convert.ToInt64(z.Punch_Start.Replace("-", "").Replace(" ", "").Replace(":", ""))) <= (Convert.ToInt64((z.Punch_Start.Substring(0, 10).Replace("-", "")) + searchTimeClass.fortime1))
                                 |
                                 (Convert.ToInt64(z.Punch_End.Replace("-", "").Replace(" ", "").Replace(":", ""))) >= (Convert.ToInt64((z.Punch_Start.Substring(0, 10).Replace("-", "")) + searchTimeClass.fortime0))
                                 &
                                    (Convert.ToInt64(z.Punch_End.Replace("-", "").Replace(" ", "").Replace(":", ""))) <= (Convert.ToInt64((z.Punch_Start.Substring(0, 10).Replace("-", "")) + searchTimeClass.fortime1))
                                )
                    orderby Convert.ToInt64(z.Punch_Start.Replace("-", "").Replace(" ", "").Replace(":", ""))
                    select new SearchStoreCard()
                    {
                        StoreID = z.StoreID,
                        Store_Name = z.Store_Name,
                        Store_Class = z.Store_Class,
                        Address_Area = z.Address_Area,
                        Address_City = z.Address_City,
                        Address_Detail = z.Address_Detail,
                        Picture = z.Picture,
                        Phone = z.Phone,
                        Introduce = z.Introduce,
                        Punch_Start = z.Punch_Start,
                        Punch_End = z.Punch_End,
                        Punch_Time = z.Punch_Time,
                        StarRating = z.StarRating,
                        ContentCount = z.ContentCount
                    };
                    storesSearch = tt.AsQueryable();
                    return storesSearch;
                }
            }
            //----------------------------------------------------------------------------------------------------------------------------------------//
            //
            // 狀態4: 沒有搜尋日期 也沒有搜尋時間
            //
            else
            {
                var tt =
                    from z in
                    (
                             from y in
                            (
                                     from x in
                                    (
                                        from s in db.Store
                                        join sb in db.Store_Business
                                        on s.StoreID equals sb.StoreID
                                        select new
                                        {
                                            StoreID = s.StoreID,
                                            Store_Name = s.Store_Name,
                                            Store_Class = s.Store_Class,
                                            Address_Area = s.Address_Area,
                                            Address_City = sb.Address_City,
                                            Address_Detail = sb.Address_City + sb.Address_Local + sb.Store_Address,
                                            Picture = s.Picture,
                                            Phone = s.Phone,
                                            Introduce = s.Introduce,
                                            Punch_Start = sb.Punch_Start,
                                            Punch_End = sb.Punch_End,
                                            Punch_Time = sb.Punch_Start + "~" + sb.Punch_End.Substring(sb.Punch_End.Length - 5),
                                            CalendarID = sb.CalendarID,
                                        }

                                    ) // x in(x 的裡面)
                                     join m in db.Message_Board
                                     on x.StoreID equals m.StoreID into g
                                     from ssbm in g.DefaultIfEmpty()
                                     select new SearchStoreCard()
                                     {
                                         StoreID = x.StoreID,
                                         Store_Name = x.Store_Name,
                                         Store_Class = x.Store_Class,
                                         Address_Area = x.Address_Area,
                                         Address_City = x.Address_City,
                                         Address_Detail = x.Address_Detail,
                                         Picture = x.Picture,
                                         Phone = x.Phone,
                                         Introduce = x.Introduce,
                                         Punch_Start = x.Punch_Start,
                                         Punch_End = x.Punch_End,
                                         Punch_Time = x.Punch_Time,
                                         StarRating = Math.Round((double)g.Average(m => m.Star_Rating), 1),
                                         ContentCount = g.Count(),
                                         CalendarID = x.CalendarID
                                     }
                                    ) //y

                             group new
                             {
                                 y.StoreID,
                                 y.Store_Name,
                                 y.Store_Class,
                                 y.Address_Area,
                                 y.Address_City,
                                 y.Address_Detail,
                                 y.Picture,
                                 y.Phone,
                                 y.Introduce,
                                 y.Punch_Start,
                                 y.Punch_End,
                                 y.Punch_Time,
                                 //y.StarRating,
                                 //y.ContentCount
                             }
                             by new
                             {
                                 y.StoreID,
                                 y.Store_Name,
                                 y.Store_Class,
                                 y.Address_Area,
                                 y.Address_City,
                                 y.Address_Detail,
                                 y.Picture,
                                 y.Phone,
                                 y.Introduce,
                                 y.Punch_Start,
                                 y.Punch_End,
                                 y.Punch_Time,
                                 y.StarRating,
                                 y.ContentCount,
                                 y.CalendarID
                             }
                                        into gro
                             orderby gro.Key.Punch_Start
                             select new
                             {
                                 CalendarID = gro.Key.CalendarID,
                                 StoreID = gro.Key.StoreID,
                                 Store_Name = gro.Key.Store_Name,
                                 Store_Class = gro.Key.Store_Class,
                                 Address_Area = gro.Key.Address_Area,
                                 Address_City = gro.Key.Address_City,
                                 Address_Detail = gro.Key.Address_Detail,
                                 Picture = gro.Key.Picture,
                                 Phone = gro.Key.Phone,
                                 Introduce = gro.Key.Introduce,
                                 Punch_Start = gro.Key.Punch_Start,
                                 Punch_End = gro.Key.Punch_End,
                                 Punch_Time = gro.Key.Punch_Time,
                                 StarRating = gro.Key.StarRating == null ? 0 : gro.Key.StarRating,
                                 ContentCount = gro.Key.ContentCount
                             }
                                    ).AsEnumerable() //z     

                    where z.CalendarID == 1
                         & z.Store_Name.Contains(obj.fname)
                         & z.Store_Class.Contains(obj.fclass)
                         & z.Address_City.Contains(obj.fcity)
                    //& (Convert.ToInt32((sb.Punch_Start.Substring(0, 9)).Replace("-", "")) == Convert.ToInt32(searchTimeClass.nowdate))
                    orderby Convert.ToInt64(z.Punch_Start.Replace("-", "").Replace(" ", "").Replace(":", ""))
                    //& sb.Punch_day.ToString().Replace("-", "").Remove(4,1) == searchTimeClass.nowdate
                    select new SearchStoreCard()
                    {
                        StoreID = z.StoreID,
                        Store_Name = z.Store_Name,
                        Store_Class = z.Store_Class,
                        Address_Area = z.Address_Area,
                        Address_City = z.Address_City,
                        Address_Detail = z.Address_Detail,
                        Picture = z.Picture,
                        Phone = z.Phone,
                        Introduce = z.Introduce,
                        Punch_Start = z.Punch_Start,
                        Punch_End = z.Punch_End,
                        Punch_Time = z.Punch_Time,
                        StarRating = z.StarRating,
                        ContentCount = z.ContentCount
                    };
                storesSearch = tt.AsQueryable();
                return storesSearch;
            }
            //
            //-----------------------------------------------有登入的跑法要帶CID，join我的最愛顯示(未實作)-----------------------------------------------
            //

        }
    }

}


