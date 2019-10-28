using System;
using System.Collections.Generic;

namespace CS.VerticalSlices.Data.Entities
{
    public class SalesOrder : IEntity
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<Line> Lines { get; set; }
        public class Line
        {
            public int OrderId { get; set; }
            public int Number { get; set; }
            public int ProductId { get; set; }
            public Product Product { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
        }
    }
    public class PurchaseOrder : IEntity
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ApprovedDate { get; set; }
        public List<Line> Lines { get; set; }
        public class Line
        {
            public int OrderId { get; set; }
            public int Number { get; set; }
            public int ProductId { get; set; }
            public Product Product { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
        }
    }
}
