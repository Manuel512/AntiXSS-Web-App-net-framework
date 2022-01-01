using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace XSSWebApp.Models
{
    public class Payload
    {
        public Payload()
        {
            Children = new List<Child>();
            CarsQuantity = new List<int>();
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public bool IsMarried { get; set; }

        [AllowHtml]
        public string Description { get; set; }
        public List<Child> Children { get; set; }
        public Child ChildNames { get; set; }
        public List<int> CarsQuantity { get; set; }
    }

    public class Child
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}