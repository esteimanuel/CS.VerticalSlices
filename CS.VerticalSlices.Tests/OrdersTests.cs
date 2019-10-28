using System;
using System.Collections.Generic;
using System.Linq;
using CS.VerticalSlices.Data;
using CS.VerticalSlices.Features.SalesOrders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CS.VerticalSlices.Tests
{
    [TestClass]
    public class OrdersTests
    {
        [TestMethod]
        public void GivenNoOrdersViewOrderOverview()
        {
            using var factory = new DbContextFactory<ShopContext>();
            var options = factory.CreateOptions();
            using var context = factory.CreateContext(options);
            
            var query = new Overview.Query();
            var handler = new Overview.Handler(context);
            var overviewPage = handler.Handle(query, default).Result;

            Assert.AreEqual(0, overviewPage.OrdersTotal);
        }

        [TestMethod]
        public void GivenAnOrderViewOrderOverview()
        {
            using var factory = new DbContextFactory<ShopContext>();
            var options = factory.CreateOptions();
            using var context = factory.CreateContext(options);

            var createProductHandler = new Features.Products.Create.Handler(context);
            var foo = createProductHandler.Handle(new Features.Products.Create.Command { Code = "Foo", Description = "Foo description" }, default).Result;
            var bar = createProductHandler.Handle(new Features.Products.Create.Command { Code = "Bar", Description = "Bar description" }, default).Result;

            // Create an order
            var createCommand = new Create.Command
            {
                DateCreated = new DateTime(2019, 1, 1),
                Lines = new List<Create.Command.OrderLine>
                {
                    new Create.Command.OrderLine { ProductCode = foo, Quantity = 1 },
                    new Create.Command.OrderLine { ProductCode = bar, Quantity = 3 },
                }
            };
            var createHandler = new Create.Handler(context);
            var orderNumber = createHandler.Handle(createCommand, default).Result;

            // View order overview
            var query = new Overview.Query();
            var overviewHandler = new Overview.Handler(context);
            var overviewPage = overviewHandler.Handle(query, default).Result;

            Assert.AreEqual(1, overviewPage.OrdersTotal);
            var order = overviewPage.Orders.Single();
            Assert.AreEqual(orderNumber, order.Number);
        }

        [TestMethod]
        public void GivenAnOrderViewOrderDetails()
        {
            using var factory = new DbContextFactory<ShopContext>();
            var options = factory.CreateOptions();
            using var context = factory.CreateContext(options);

            var createProductHandler = new Features.Products.Create.Handler(context);
            var foo = createProductHandler.Handle(new Features.Products.Create.Command { Code = "Foo", Description = "Foo description" }, default).Result;
            var bar = createProductHandler.Handle(new Features.Products.Create.Command { Code = "Bar", Description = "Bar description" }, default).Result;
            
            // Create an order
            var createCommand = new Create.Command
            {
                DateCreated = new DateTime(2019, 1, 1),
                Lines = new List<Create.Command.OrderLine>
                {
                    new Create.Command.OrderLine { ProductCode = foo, Quantity = 1 },
                    new Create.Command.OrderLine { ProductCode = bar, Quantity = 3 },
                }
            };
            var createHandler = new Create.Handler(context);
            var orderNumber = createHandler.Handle(createCommand, default).Result;

            // View order details
            var query = new Details.Query { Number = orderNumber };
            var detailsHandler = new Details.Handler(context);
            var orderDetails = detailsHandler.Handle(query, default).Result;

            Assert.AreEqual(createCommand.DateCreated, orderDetails.CreatedDate);
            Assert.AreEqual(createCommand.Lines.Count(), orderDetails.Lines.Count());
            foreach (var (line, index) in orderDetails.Lines.Select((l, i) => (l, i)))
            {
                var orderLine = createCommand.Lines.Single(x => x.ProductCode == line.ProductCode);
                var orderLineNumber = index + 1;
                Assert.AreEqual(orderLineNumber, line.Number);
                Assert.AreEqual(orderLine.Quantity, line.Quantity);
            }
        }
    }
}
