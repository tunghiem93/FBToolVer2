﻿using CMS_DTO.CMSCrawler;
using CMS_DTO.CMSImage;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CMS_DTO.CMSProduct
{
    public class CMS_ProductsModels
    {
        public string Id { get; set; }
        public string ProductName { get; set; }
        public int repin_count { get; set; }
        public string Link { get; set; }
        public string Board { get; set; }
        public DateTime Created_At { get; set; }
        public DateTime DateCrawler { get; set; }
        public bool IsActive { get; set; }

        public int TypeTime { get; set; }
        public int Sort1 { get; set; }
        public int Sort2 { get; set; }
        public int TypePin { get; set; }
        public int Index { get; set; }
        public string GroupID { get; set; }
        public List<SelectListItem> ListTime { get; set; }
        public List<string> listKeywords { get; set; }
        public List<string> listGroups { get; set; }
        public int TypeQuantity { get; set; }
        public List<SelectListItem> ListQuantity { get; set; }
        public List<SelectListItem> ListRePin { get; set; }
        public List<SelectListItem> ListIndex { get; set; }
        public List<SelectListItem> ListSort2 { get; set; }
        public HttpPostedFileBase[] PictureUpload { get; set; }
        public byte[] PictureByte { get; set; }
        public string ImageURL { get; set; }

        public CMS_CrawlerModels Crawler { get; set; }
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FromDate { get; set; }
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ToDate { get; set; }

        public CMS_ProductsModels()
        {
            ListTime = new List<SelectListItem>();
            ListQuantity = new List<SelectListItem>();
            Crawler = new CMS_CrawlerModels();
            FromDate = new DateTime(1990, 01, 01);
            ToDate = DateTime.Now;
            listKeywords = new List<string>();
            listGroups = new List<string>();
        }
    }
}
