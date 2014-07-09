using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DataLayer.TransferModels
{
    /// <summary>
    /// Gets returned when editing current Article
    /// </summary>
    public class ArticleEditTM
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}