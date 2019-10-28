using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CS.VerticalSlices.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace CS.VerticalSlices.Features.SalesOrders
{
    public class Overview
    {
        public class Query : IRequest<Page>
        {
            public string Search { get; set; } = "";
            public int PageIndex { get; set; } = 0;
            public int PageSize { get; set; } = 10;
            public Sorting Sort { get; set; } = Sorting.CreatedDate;
            public bool Desc { get; set; } = true;

            public enum Sorting
            {
                Number,
                CreatedDate,
            }
        }

        public class Page
        {
            public int Index { get; set; }

            public int Size { get; set; }

            public List<Order> Orders { get; set; }

            public int OrdersTotal { get; set; }

            public class Order
            {
                public string Number { get; set; }
                public DateTime CreatedDate { get; set; }
            }
        }

        public class Handler : IRequestHandler<Query, Page>
        {
            private readonly ShopContext context;

            public Handler(ShopContext context) => this.context = context;

            public async Task<Page> Handle(Query request, CancellationToken cancellationToken)
            {
                var filteredOrders = 
                    this.context.SalesOrders
                    .Where(order => order.Number.Contains(request.Search));

                var sortedOrders = 
                    request.Desc 
                    ? request.Sort switch
                    {
                        Query.Sorting.CreatedDate => filteredOrders.OrderByDescending(x => x.CreatedDate),
                        Query.Sorting.Number => filteredOrders.OrderByDescending(x => x.Number),
                        _ => throw new ArgumentException(message: "invalid enum value", paramName: nameof(request.Sort))
                    }
                    : request.Sort switch 
                    { 
                        Query.Sorting.CreatedDate => filteredOrders.OrderBy(x => x.CreatedDate),
                        Query.Sorting.Number => filteredOrders.OrderBy(x => x.Number),
                        _ => throw new ArgumentException(message: "invalid enum value", paramName: nameof(request.Sort))
                    };

                var ordersPage =
                    sortedOrders
                    .Skip(request.PageIndex * request.PageSize)
                    .Take(request.PageSize);

                var orders =
                    await ordersPage
                    .Select(order => new Page.Order 
                    {
                        Number = order.Number,
                        CreatedDate = order.CreatedDate
                    })
                    .ToListAsync();

                var ordersTotal = filteredOrders.Count();

                return new Page
                {
                    Index = request.PageIndex,
                    Size = request.PageSize,
                    Orders = orders,
                    OrdersTotal = ordersTotal
                };
            }
        }
    }
}
