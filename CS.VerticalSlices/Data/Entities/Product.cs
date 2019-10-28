using System;
using System.Collections.Generic;
using System.Text;

namespace CS.VerticalSlices.Data.Entities
{
    public class Product : IEntity
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
    }
}
